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
    public class OrderController : Controller
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
                .Where(p => p.StockQuantity > 0)
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
        public async Task<IActionResult> CreatePurchaseOrder(PurchaseOrder order, List<PurchaseOrderItem> items)
        {
            if (ModelState.IsValid && items != null && items.Any())
            {
                // Calculate total
                order.TotalAmount = items.Sum(i => i.Quantity * i.UnitPrice);
                order.CreatedAt = DateTime.Now;

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
                TempData["Success"] = "Purchase order created successfully!";
                return RedirectToAction(nameof(PurchaseOrders));
            }

            ViewBag.Suppliers = _context.Suppliers
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToList();
            ViewBag.Products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity > 0)
                .OrderBy(p => p.Name)
                .ToList();

            return View(order);
        }

        // POST: Admin/Order/UpdatePurchaseOrderStatus/5
        [HttpPost]
        public async Task<IActionResult> UpdatePurchaseOrderStatus(int id, [FromBody] StatusUpdateModel model)
        {
            var order = await _context.PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null)
                return Json(new { success = false, message = "Order not found" });

            order.Status = model.Status;
            order.UpdatedAt = DateTime.Now;

            // If delivered, update stock
            if (model.Status == "Delivered")
            {
                foreach (var item in order.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        int oldStock = product.StockQuantity;
                        product.StockQuantity += item.Quantity;
                        product.UpdatedAt = DateTime.Now;

                        // Record stock movement
                        var movement = new StockMovement
                        {
                            ProductId = product.Id,
                            Quantity = item.Quantity,
                            MovementType = "Purchase",
                            ReferenceNumber = order.OrderNumber,
                            Notes = $"Purchase order #{order.OrderNumber}"
                        };
                        _context.StockMovements.Add(movement);
                        _context.Products.Update(product);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Order status updated to {model.Status}" });
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
        public async Task<IActionResult> EditPurchaseOrder(int id, PurchaseOrder order, List<PurchaseOrderItem> items)
        {
            if (id != order.Id)
                return NotFound();

            if (ModelState.IsValid && items != null && items.Any())
            {
                try
                {
                    // Remove existing items
                    var existingItems = _context.PurchaseOrderItems.Where(i => i.PurchaseOrderId == id);
                    _context.PurchaseOrderItems.RemoveRange(existingItems);

                    // Update order
                    order.UpdatedAt = DateTime.Now;
                    order.TotalAmount = items.Sum(i => i.Quantity * i.UnitPrice);
                    _context.Update(order);

                    // Add new items
                    foreach (var item in items)
                    {
                        item.PurchaseOrderId = order.Id;
                        item.TotalPrice = item.Quantity * item.UnitPrice;
                        _context.PurchaseOrderItems.Add(item);
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Purchase order updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseOrderExists(order.Id))
                        return NotFound();
                    else
                        throw;
                }
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

            return View(order);
        }

        // POST: Admin/Order/DeletePurchaseOrder/5
        [HttpPost, ActionName("DeletePurchaseOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePurchaseOrderConfirmed(int id)
        {
            var order = await _context.PurchaseOrders.FindAsync(id);
            if (order == null)
                return NotFound();

            if (order.Status == "Delivered")
            {
                TempData["Error"] = "Cannot delete delivered purchase order.";
                return RedirectToAction(nameof(PurchaseOrders));
            }

            // Remove related items
            var items = _context.PurchaseOrderItems.Where(i => i.PurchaseOrderId == id);
            _context.PurchaseOrderItems.RemoveRange(items);

            // Remove order
            _context.PurchaseOrders.Remove(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Purchase order deleted successfully!";
            return RedirectToAction(nameof(PurchaseOrders));
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
                .Where(p => p.StockQuantity > 0)
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
        public async Task<IActionResult> CreateSalesOrder(SalesOrder order, List<SalesOrderItem> items)
        {
            if (ModelState.IsValid && items != null && items.Any())
            {
                // Check stock availability
                foreach (var item in items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null || product.StockQuantity < item.Quantity)
                    {
                        ModelState.AddModelError("", $"Insufficient stock for {product?.Name}");
                        ViewBag.Customers = _context.Customers
                            .Where(c => c.IsActive)
                            .OrderBy(c => c.Name)
                            .ToList();
                        ViewBag.Products = _context.Products
                            .Include(p => p.Category)
                            .Where(p => p.StockQuantity > 0)
                            .OrderBy(p => p.Name)
                            .ToList();
                        return View(order);
                    }
                }

                // Calculate totals
                decimal subtotal = items.Sum(i => i.Quantity * i.UnitPrice);
                decimal discount = subtotal * (order.DiscountPercentage / 100);
                order.TotalAmount = subtotal - discount;
                order.GrandTotal = order.TotalAmount + order.TaxAmount;
                order.CreatedAt = DateTime.Now;

                _context.SalesOrders.Add(order);
                await _context.SaveChangesAsync();

                // Add items and update stock
                foreach (var item in items)
                {
                    item.SalesOrderId = order.Id;
                    item.TotalPrice = item.Quantity * item.UnitPrice;
                    _context.SalesOrderItems.Add(item);

                    // Update product stock
                    var product = await _context.Products.FindAsync(item.ProductId);
                    int oldStock = product.StockQuantity;
                    product.StockQuantity -= item.Quantity;
                    product.UpdatedAt = DateTime.Now;

                    // Record stock movement
                    var movement = new StockMovement
                    {
                        ProductId = product.Id,
                        Quantity = -item.Quantity, // Negative for sales
                        MovementType = "Sale",
                        ReferenceNumber = order.OrderNumber,
                        Notes = $"Sales order #{order.OrderNumber}"
                    };
                    _context.StockMovements.Add(movement);
                    _context.Products.Update(product);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Sales order created successfully!";
                return RedirectToAction(nameof(SalesOrders));
            }

            ViewBag.Customers = _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();
            ViewBag.Products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity > 0)
                .OrderBy(p => p.Name)
                .ToList();

            return View(order);
        }

        // POST: Admin/Order/UpdateSalesOrderStatus/5
        [HttpPost]
        public async Task<IActionResult> UpdateSalesOrderStatus(int id, [FromBody] StatusUpdateModel model)
        {
            var order = await _context.SalesOrders
                .Include(so => so.Items)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (order == null)
                return Json(new { success = false, message = "Order not found" });

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

                        // Record stock movement
                        var movement = new StockMovement
                        {
                            ProductId = product.Id,
                            Quantity = item.Quantity, // Positive for cancellation
                            MovementType = "Return",
                            ReferenceNumber = order.OrderNumber,
                            Notes = $"Order cancellation #{order.OrderNumber}"
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

            if (order.Status == "Completed")
            {
                TempData["Error"] = "Cannot edit completed order.";
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
        public async Task<IActionResult> EditSalesOrder(int id, SalesOrder order, List<SalesOrderItem> items)
        {
            if (id != order.Id)
                return NotFound();

            var existingOrder = await _context.SalesOrders
                .Include(so => so.Items)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (existingOrder == null)
                return NotFound();

            if (existingOrder.Status == "Completed")
            {
                TempData["Error"] = "Cannot edit completed order.";
                return RedirectToAction(nameof(SalesOrders));
            }

            if (ModelState.IsValid && items != null && items.Any())
            {
                try
                {
                    // Restore old stock quantities
                    foreach (var oldItem in existingOrder.Items)
                    {
                        var product = await _context.Products.FindAsync(oldItem.ProductId);
                        if (product != null)
                        {
                            product.StockQuantity += oldItem.Quantity;
                            _context.Products.Update(product);
                        }
                    }

                    // Remove existing items
                    _context.SalesOrderItems.RemoveRange(existingOrder.Items);

                    // Check new stock availability
                    foreach (var newItem in items)
                    {
                        var product = await _context.Products.FindAsync(newItem.ProductId);
                        if (product == null || product.StockQuantity < newItem.Quantity)
                        {
                            // Restore original items and stock
                            await RestoreOriginalOrder(existingOrder);
                            
                            ModelState.AddModelError("", $"Insufficient stock for {product?.Name}");
                            ViewBag.Customers = _context.Customers
                                .Where(c => c.IsActive)
                                .OrderBy(c => c.Name)
                                .ToList();
                            ViewBag.Products = _context.Products
                                .Include(p => p.Category)
                                .Where(p => p.StockQuantity > 0)
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

                    // Calculate totals
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

                        // Update product stock
                        var product = await _context.Products.FindAsync(newItem.ProductId);
                        product.StockQuantity -= newItem.Quantity;
                        _context.Products.Update(product);

                        // Record stock movement
                        var movement = new StockMovement
                        {
                            ProductId = product.Id,
                            Quantity = -newItem.Quantity,
                            MovementType = "Sale",
                            ReferenceNumber = existingOrder.OrderNumber,
                            Notes = $"Sales order update #{existingOrder.OrderNumber}"
                        };
                        _context.StockMovements.Add(movement);
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Sales order updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SalesOrderExists(order.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(SalesOrders));
            }

            ViewBag.Customers = _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToList();
            ViewBag.Products = _context.Products
                .Include(p => p.Category)
                .Where(p => p.StockQuantity > 0)
                .OrderBy(p => p.Name)
                .ToList();

            return View(order);
        }

        private async Task RestoreOriginalOrder(SalesOrder order)
        {
            var originalItems = await _context.SalesOrderItems
                .Where(i => i.SalesOrderId == order.Id)
                .ToListAsync();

            foreach (var item in originalItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    _context.Products.Update(product);
                }
            }
            await _context.SaveChangesAsync();
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

            return View(order);
        }

        // POST: Admin/Order/DeleteSalesOrder/5
        [HttpPost, ActionName("DeleteSalesOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSalesOrderConfirmed(int id)
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
            if (order.Status == "Pending")
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

            // Remove related items
            _context.SalesOrderItems.RemoveRange(order.Items);

            // Remove order
            _context.SalesOrders.Remove(order);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Sales order deleted successfully!";
            return RedirectToAction(nameof(SalesOrders));
        }

        private bool PurchaseOrderExists(int id)
        {
            return _context.PurchaseOrders.Any(e => e.Id == id);
        }

        private bool SalesOrderExists(int id)
        {
            return _context.SalesOrders.Any(e => e.Id == id);
        }

        // Helper Model for Status Update
        public class StatusUpdateModel
        {
            public string Status { get; set; }
        }
    }
}