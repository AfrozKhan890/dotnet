using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIOMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ========== PURCHASE ORDERS ==========

        // GET: Admin/Order/PurchaseOrders
        public async Task<IActionResult> PurchaseOrders(string status, DateTime? fromDate, DateTime? toDate)
        {
            var orders = _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Items)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(po => po.Status == status);
            }

            if (fromDate.HasValue)
            {
                orders = orders.Where(po => po.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                orders = orders.Where(po => po.OrderDate <= toDate.Value);
            }

            ViewBag.StatusFilter = status;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(await orders.OrderByDescending(po => po.OrderDate).ToListAsync());
        }

        // GET: Admin/Order/PurchaseOrderDetails/5
        public async Task<IActionResult> PurchaseOrderDetails(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // GET: Admin/Order/CreatePurchaseOrder
        public IActionResult CreatePurchaseOrder()
        {
            ViewBag.Suppliers = _context.Suppliers
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToList();
            ViewBag.Products = _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToList();

            var order = new PurchaseOrder
            {
                OrderDate = DateTime.Now,
                ExpectedDeliveryDate = DateTime.Now.AddDays(7),
                OrderNumber = "PO-" + DateTime.Now.ToString("yyyyMMddHHmmss")
            };

            return View(order);
        }

        // POST: Admin/Order/CreatePurchaseOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePurchaseOrder(PurchaseOrder order, [FromForm] List<PurchaseOrderItem> items)
        {
            try
            {
                // ✅ FIX: Remove ALL navigation properties from ModelState
                ModelState.Remove("Supplier");
                ModelState.Remove("Items");
                ModelState.Remove("StockMovements");
                ModelState.Remove("PurchaseOrder");  // ✅ FROM PurchaseOrderItem
                ModelState.Remove("Product");        // ✅ FROM PurchaseOrderItem

                // ✅ Also remove from individual items if they exist in ModelState
                if (items != null)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        ModelState.Remove($"items[{i}].PurchaseOrder");
                        ModelState.Remove($"items[{i}].Product");
                    }
                }

                if (order.SupplierId <= 0)
                {
                    ModelState.AddModelError("SupplierId", "Please select a supplier");
                }

                if (items == null || !items.Any())
                {
                    ModelState.AddModelError("", "Please add at least one item to the order");
                }

                // Validate items
                if (items != null && items.Any())
                {
                    foreach (var item in items)
                    {
                        if (item.ProductId <= 0)
                        {
                            ModelState.AddModelError("", "Please select a product for all items");
                            break;
                        }
                        if (item.Quantity <= 0)
                        {
                            ModelState.AddModelError("", "Quantity must be greater than 0");
                            break;
                        }
                        if (item.UnitPrice <= 0)
                        {
                            ModelState.AddModelError("", "Unit price must be greater than 0");
                            break;
                        }
                    }
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Suppliers = _context.Suppliers
                        .Where(s => s.IsActive)
                        .OrderBy(s => s.Name)
                        .ToList();
                    ViewBag.Products = _context.Products
                        .Include(p => p.Category)
                        .OrderBy(p => p.Name)
                        .ToList();
                    return View(order);
                }

                // Calculate total
                order.TotalAmount = items.Sum(i => i.Quantity * i.UnitPrice);
                order.CreatedAt = DateTime.Now;
                order.Status = "Pending";

                _context.PurchaseOrders.Add(order);
                await _context.SaveChangesAsync();

                // Add items
                foreach (var item in items)
                {
                    item.PurchaseOrderId = order.Id;
                    item.TotalPrice = item.Quantity * item.UnitPrice;
                    _context.PurchaseOrderItems.Add(item);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Purchase order #{order.OrderNumber} created successfully!";
                return RedirectToAction(nameof(PurchaseOrders));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating purchase order: {ex.Message}";
                ViewBag.Suppliers = _context.Suppliers
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Name)
                    .ToList();
                ViewBag.Products = _context.Products
                    .Include(p => p.Category)
                    .OrderBy(p => p.Name)
                    .ToList();
                return View(order);
            }
        }

        // POST: Admin/Order/UpdatePurchaseOrderStatus/5
        [HttpPost]
        public async Task<IActionResult> UpdatePurchaseOrderStatus(int id, [FromBody] StatusUpdateModel model)
        {
            try
            {
                var order = await _context.PurchaseOrders
                    .Include(po => po.Items)
                    .FirstOrDefaultAsync(po => po.Id == id);

                if (order == null)
                    return Json(new { success = false, message = "Order not found" });

                order.Status = model.Status;
                order.UpdatedAt = DateTime.Now;

                if (model.Status == "Delivered")
                {
                    foreach (var item in order.Items)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;
                            product.UpdatedAt = DateTime.Now;

                            var movement = new StockMovement
                            {
                                ProductId = product.Id,
                                Quantity = item.Quantity,
                                MovementType = "Purchase",
                                ReferenceNumber = order.OrderNumber,
                                Notes = $"Purchase order #{order.OrderNumber}",
                                MovementDate = DateTime.Now
                            };
                            _context.StockMovements.Add(movement);
                            _context.Products.Update(product);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Order status updated to {model.Status}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: Admin/Order/EditPurchaseOrder/5
        public async Task<IActionResult> EditPurchaseOrder(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null)
                return NotFound();

            if (order.Status == "Delivered" || order.Status == "Cancelled")
            {
                TempData["Error"] = $"Cannot edit {order.Status} order.";
                return RedirectToAction(nameof(PurchaseOrders));
            }

            ViewBag.Suppliers = _context.Suppliers
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToList();
            ViewBag.Products = _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToList();

            return View(order);
        }

        // POST: Admin/Order/EditPurchaseOrder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPurchaseOrder(int id, PurchaseOrder order, [FromForm] List<PurchaseOrderItem> items)
        {
            if (id != order.Id)
                return NotFound();

            try
            {
                // ✅ FIX: Remove ALL navigation properties from ModelState
                ModelState.Remove("Supplier");
                ModelState.Remove("Items");
                ModelState.Remove("StockMovements");
                ModelState.Remove("PurchaseOrder");
                ModelState.Remove("Product");

                if (items != null)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        ModelState.Remove($"items[{i}].PurchaseOrder");
                        ModelState.Remove($"items[{i}].Product");
                    }
                }

                if (order.SupplierId <= 0)
                {
                    ModelState.AddModelError("SupplierId", "Please select a supplier");
                }

                if (items == null || !items.Any())
                {
                    ModelState.AddModelError("", "Please add at least one item to the order");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Suppliers = _context.Suppliers
                        .Where(s => s.IsActive)
                        .OrderBy(s => s.Name)
                        .ToList();
                    ViewBag.Products = _context.Products
                        .Include(p => p.Category)
                        .OrderBy(p => p.Name)
                        .ToList();
                    return View(order);
                }

                var existingOrder = await _context.PurchaseOrders
                    .Include(po => po.Items)
                    .FirstOrDefaultAsync(po => po.Id == id);

                if (existingOrder == null)
                    return NotFound();

                // Remove existing items
                if (existingOrder.Items != null && existingOrder.Items.Any())
                {
                    _context.PurchaseOrderItems.RemoveRange(existingOrder.Items);
                }

                // Update order
                existingOrder.SupplierId = order.SupplierId;
                existingOrder.OrderDate = order.OrderDate;
                existingOrder.ExpectedDeliveryDate = order.ExpectedDeliveryDate;
                existingOrder.Status = order.Status;
                existingOrder.Notes = order.Notes;
                existingOrder.UpdatedAt = DateTime.Now;
                existingOrder.TotalAmount = items.Sum(i => i.Quantity * i.UnitPrice);

                _context.Update(existingOrder);

                // Add new items
                foreach (var item in items)
                {
                    item.PurchaseOrderId = existingOrder.Id;
                    item.TotalPrice = item.Quantity * item.UnitPrice;
                    _context.PurchaseOrderItems.Add(item);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Purchase order #{existingOrder.OrderNumber} updated successfully!";
                return RedirectToAction(nameof(PurchaseOrders));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating purchase order: {ex.Message}";
                ViewBag.Suppliers = _context.Suppliers
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Name)
                    .ToList();
                ViewBag.Products = _context.Products
                    .Include(p => p.Category)
                    .OrderBy(p => p.Name)
                    .ToList();
                return View(order);
            }
        }

        // GET: Admin/Order/DeletePurchaseOrder/5
        public async Task<IActionResult> DeletePurchaseOrder(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.PurchaseOrders
                .Include(po => po.Supplier)
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null)
                return NotFound();

            if (order.Status == "Delivered")
            {
                TempData["Error"] = "Cannot delete delivered purchase order.";
                return RedirectToAction(nameof(PurchaseOrders));
            }

            return View(order);
        }

        // POST: Admin/Order/DeletePurchaseOrder/5
        [HttpPost, ActionName("DeletePurchaseOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePurchaseOrderConfirmed(int id)
        {
            try
            {
                var order = await _context.PurchaseOrders
                    .Include(po => po.Items)
                    .FirstOrDefaultAsync(po => po.Id == id);

                if (order == null)
                    return NotFound();

                if (order.Status == "Delivered")
                {
                    TempData["Error"] = "Cannot delete delivered purchase order.";
                    return RedirectToAction(nameof(PurchaseOrders));
                }

                if (order.Items != null && order.Items.Any())
                {
                    _context.PurchaseOrderItems.RemoveRange(order.Items);
                }

                _context.PurchaseOrders.Remove(order);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Purchase order #{order.OrderNumber} deleted successfully!";
                return RedirectToAction(nameof(PurchaseOrders));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting purchase order: {ex.Message}";
                return RedirectToAction(nameof(PurchaseOrders));
            }
        }

        // ========== SALES ORDERS ==========

        // GET: Admin/Order/SalesOrders
        public async Task<IActionResult> SalesOrders(string status, DateTime? fromDate, DateTime? toDate)
        {
            var orders = _context.SalesOrders
                .Include(so => so.Customer)
                .Include(so => so.Items)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(so => so.Status == status);
            }

            if (fromDate.HasValue)
            {
                orders = orders.Where(so => so.OrderDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                orders = orders.Where(so => so.OrderDate <= toDate.Value);
            }

            ViewBag.StatusFilter = status;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            return View(await orders.OrderByDescending(so => so.OrderDate).ToListAsync());
        }

        // GET: Admin/Order/SalesOrderDetails/5
        public async Task<IActionResult> SalesOrderDetails(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.SalesOrders
                .Include(so => so.Customer)
                .Include(so => so.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // GET: Admin/Order/CreateSalesOrder
        public IActionResult CreateSalesOrder()
        {
            ViewBag.Customers = _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();
            ViewBag.Products = _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToList();

            var order = new SalesOrder
            {
                OrderDate = DateTime.Now,
                DiscountPercentage = 0,
                TaxAmount = 0,
                OrderNumber = "SO-" + DateTime.Now.ToString("yyyyMMddHHmmss")
            };

            return View(order);
        }

      // POST: Admin/Order/CreateSalesOrder
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateSalesOrder(SalesOrder order, [FromForm] List<SalesOrderItem> items)
{
    try
    {
        // Remove navigation properties from ModelState
        ModelState.Remove("Customer");
        ModelState.Remove("Items");
        ModelState.Remove("StockMovements");
        ModelState.Remove("SalesOrder");
        ModelState.Remove("Product");

        if (items != null)
        {
            for (int i = 0; i < items.Count; i++)
            {
                ModelState.Remove($"items[{i}].SalesOrder");
                ModelState.Remove($"items[{i}].Product");
            }
        }

        if (order.CustomerId <= 0)
        {
            ModelState.AddModelError("CustomerId", "Please select a customer");
        }

        if (items == null || !items.Any())
        {
            ModelState.AddModelError("", "Please add at least one item to the order");
        }

        // Validate items and stock
        if (items != null && items.Any())
        {
            foreach (var item in items)
            {
                if (item.ProductId <= 0)
                {
                    ModelState.AddModelError("", "Please select a product for all items");
                    break;
                }
                if (item.Quantity <= 0)
                {
                    ModelState.AddModelError("", "Quantity must be greater than 0");
                    break;
                }
                if (item.UnitPrice <= 0)
                {
                    ModelState.AddModelError("", "Unit price must be greater than 0");
                    break;
                }

                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null && product.StockQuantity < item.Quantity)
                {
                    ModelState.AddModelError("", $"Insufficient stock for {product.Name}. Available: {product.StockQuantity}");
                    break;
                }
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Customers = _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();
            ViewBag.Products = _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToList();
            return View(order);
        }

        // Calculate totals
        decimal subtotal = items.Sum(i => i.Quantity * i.UnitPrice);
        decimal discount = subtotal * (order.DiscountPercentage / 100);
        order.TotalAmount = subtotal - discount;
        order.GrandTotal = order.TotalAmount + order.TaxAmount;
        order.CreatedAt = DateTime.Now;
        order.Status = "Pending";  // ✅ Default status is Pending

        _context.SalesOrders.Add(order);
        await _context.SaveChangesAsync();

        // ✅ FIX: ONLY deduct stock when status is Processing or Completed
        // Stock should NOT be deducted for Pending orders
        // For Pending, we only save the order without affecting stock
        
        // Add items (without stock deduction for Pending)
        foreach (var item in items)
        {
            item.SalesOrderId = order.Id;
            item.TotalPrice = item.Quantity * item.UnitPrice;
            _context.SalesOrderItems.Add(item);
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Sales order #{order.OrderNumber} created successfully!";
        return RedirectToAction(nameof(SalesOrders));
    }
    catch (Exception ex)
    {
        TempData["Error"] = $"Error creating sales order: {ex.Message}";
        ViewBag.Customers = _context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToList();
        ViewBag.Products = _context.Products
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToList();
        return View(order);
    }
}
        // POST: Admin/Order/UpdateSalesOrderStatus/5
[HttpPost]
public async Task<IActionResult> UpdateSalesOrderStatus(int id, [FromBody] StatusUpdateModel model)
{
    try
    {
        var order = await _context.SalesOrders
            .Include(so => so.Items)
            .FirstOrDefaultAsync(so => so.Id == id);

        if (order == null)
            return Json(new { success = false, message = "Order not found" });

        // ✅ FIX: Only check stock when moving from Pending to Processing/Completed
        // Stock is already deducted at creation time, so we don't need to check again
        
        // If cancelling completed order, restore stock
        if (order.Status == "Completed" && model.Status == "Cancelled")
        {
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    product.UpdatedAt = DateTime.Now;

                    var movement = new StockMovement
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        MovementType = "Return",
                        ReferenceNumber = order.OrderNumber,
                        Notes = $"Order cancellation #{order.OrderNumber}",
                        MovementDate = DateTime.Now
                    };
                    _context.StockMovements.Add(movement);
                    _context.Products.Update(product);
                }
            }
        }

        // ✅ FIX: If moving from Pending to Cancelled, restore stock
        if (order.Status == "Pending" && model.Status == "Cancelled")
        {
            foreach (var item in order.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    product.UpdatedAt = DateTime.Now;

                    var movement = new StockMovement
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        MovementType = "Return",
                        ReferenceNumber = order.OrderNumber,
                        Notes = $"Order cancellation #{order.OrderNumber}",
                        MovementDate = DateTime.Now
                    };
                    _context.StockMovements.Add(movement);
                    _context.Products.Update(product);
                }
            }
        }

        order.Status = model.Status;
        order.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = $"Order status updated to {model.Status}" });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = $"Error: {ex.Message}" });
    }
}
        // GET: Admin/Order/EditSalesOrder/5
        public async Task<IActionResult> EditSalesOrder(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.SalesOrders
                .Include(so => so.Items)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (order == null)
                return NotFound();

            if (order.Status == "Completed" || order.Status == "Cancelled")
            {
                TempData["Error"] = $"Cannot edit {order.Status} order.";
                return RedirectToAction(nameof(SalesOrders));
            }

            ViewBag.Customers = _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();
            ViewBag.Products = _context.Products
                .Include(p => p.Category)
                .OrderBy(p => p.Name)
                .ToList();

            return View(order);
        }

        // POST: Admin/Order/EditSalesOrder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSalesOrder(int id, SalesOrder order, [FromForm] List<SalesOrderItem> items)
        {
            if (id != order.Id)
                return NotFound();

            try
            {
                var existingOrder = await _context.SalesOrders
                    .Include(so => so.Items)
                    .FirstOrDefaultAsync(so => so.Id == id);

                if (existingOrder == null)
                    return NotFound();

                if (existingOrder.Status == "Completed" || existingOrder.Status == "Cancelled")
                {
                    TempData["Error"] = $"Cannot edit {existingOrder.Status} order.";
                    return RedirectToAction(nameof(SalesOrders));
                }

                // ✅ FIX: Remove ALL navigation properties from ModelState
                ModelState.Remove("Customer");
                ModelState.Remove("Items");
                ModelState.Remove("StockMovements");
                ModelState.Remove("SalesOrder");
                ModelState.Remove("Product");

                if (items != null)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        ModelState.Remove($"items[{i}].SalesOrder");
                        ModelState.Remove($"items[{i}].Product");
                    }
                }

                if (order.CustomerId <= 0)
                {
                    ModelState.AddModelError("CustomerId", "Please select a customer");
                }

                if (items == null || !items.Any())
                {
                    ModelState.AddModelError("", "Please add at least one item to the order");
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Customers = _context.Customers
                        .Where(c => c.IsActive)
                        .OrderBy(c => c.Name)
                        .ToList();
                    ViewBag.Products = _context.Products
                        .Include(p => p.Category)
                        .OrderBy(p => p.Name)
                        .ToList();
                    return View(order);
                }

                // Restore old stock quantities
                if (existingOrder.Items != null && existingOrder.Items.Any())
                {
                    foreach (var oldItem in existingOrder.Items)
                    {
                        var product = await _context.Products.FindAsync(oldItem.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += oldItem.Quantity;
                            _context.Products.Update(product);
                        }
                    }

                    _context.SalesOrderItems.RemoveRange(existingOrder.Items);
                }

                // Check new stock availability
                foreach (var newItem in items)
                {
                    var product = await _context.Products.FindAsync(newItem.ProductId);
                    if (product != null && product.StockQuantity < newItem.Quantity)
                    {
                        ModelState.AddModelError("", $"Insufficient stock for {product.Name}. Available: {product.StockQuantity}");
                        ViewBag.Customers = _context.Customers
                            .Where(c => c.IsActive)
                            .OrderBy(c => c.Name)
                            .ToList();
                        ViewBag.Products = _context.Products
                            .Include(p => p.Category)
                            .OrderBy(p => p.Name)
                            .ToList();
                        return View(order);
                    }
                }

                // Update order
                existingOrder.CustomerId = order.CustomerId;
                existingOrder.OrderDate = order.OrderDate;
                existingOrder.Status = order.Status;
                existingOrder.DiscountPercentage = order.DiscountPercentage;
                existingOrder.TaxAmount = order.TaxAmount;
                existingOrder.Notes = order.Notes;
                existingOrder.UpdatedAt = DateTime.Now;

                decimal subtotal = items.Sum(i => i.Quantity * i.UnitPrice);
                decimal discount = subtotal * (order.DiscountPercentage / 100);
                existingOrder.TotalAmount = subtotal - discount;
                existingOrder.GrandTotal = existingOrder.TotalAmount + order.TaxAmount;

                _context.Update(existingOrder);

                // Add new items and update stock
                foreach (var newItem in items)
                {
                    newItem.SalesOrderId = existingOrder.Id;
                    newItem.TotalPrice = newItem.Quantity * newItem.UnitPrice;
                    _context.SalesOrderItems.Add(newItem);

                    var product = await _context.Products.FindAsync(newItem.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= newItem.Quantity;
                        _context.Products.Update(product);

                        var movement = new StockMovement
                        {
                            ProductId = product.Id,
                            Quantity = -newItem.Quantity,
                            MovementType = "Sale",
                            ReferenceNumber = existingOrder.OrderNumber,
                            Notes = $"Sales order update #{existingOrder.OrderNumber}",
                            MovementDate = DateTime.Now
                        };
                        _context.StockMovements.Add(movement);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Sales order #{existingOrder.OrderNumber} updated successfully!";
                return RedirectToAction(nameof(SalesOrders));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating sales order: {ex.Message}";
                ViewBag.Customers = _context.Customers
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToList();
                ViewBag.Products = _context.Products
                    .Include(p => p.Category)
                    .OrderBy(p => p.Name)
                    .ToList();
                return View(order);
            }
        }

        // GET: Admin/Order/DeleteSalesOrder/5
        public async Task<IActionResult> DeleteSalesOrder(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.SalesOrders
                .Include(so => so.Customer)
                .Include(so => so.Items)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (order == null)
                return NotFound();

            if (order.Status == "Completed")
            {
                TempData["Error"] = "Cannot delete completed sales order.";
                return RedirectToAction(nameof(SalesOrders));
            }

            return View(order);
        }

        // POST: Admin/Order/DeleteSalesOrder/5
        [HttpPost, ActionName("DeleteSalesOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSalesOrderConfirmed(int id)
        {
            try
            {
                var order = await _context.SalesOrders
                    .Include(so => so.Items)
                    .FirstOrDefaultAsync(so => so.Id == id);

                if (order == null)
                    return NotFound();

                if (order.Status == "Completed")
                {
                    TempData["Error"] = "Cannot delete completed sales order.";
                    return RedirectToAction(nameof(SalesOrders));
                }

                // Restore stock for pending orders
                if (order.Status == "Pending" && order.Items != null)
                {
                    foreach (var item in order.Items)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;
                            _context.Products.Update(product);
                        }
                    }
                }

                if (order.Items != null && order.Items.Any())
                {
                    _context.SalesOrderItems.RemoveRange(order.Items);
                }

                _context.SalesOrders.Remove(order);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Sales order #{order.OrderNumber} deleted successfully!";
                return RedirectToAction(nameof(SalesOrders));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting sales order: {ex.Message}";
                return RedirectToAction(nameof(SalesOrders));
            }
        }

        private bool PurchaseOrderExists(int id)
        {
            return _context.PurchaseOrders.Any(e => e.Id == id);
        }

        private bool SalesOrderExists(int id)
        {
            return _context.SalesOrders.Any(e => e.Id == id);
        }

        public class StatusUpdateModel
        {
            public string? Status { get; set; } = string.Empty;
        }
    }
}