using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SIOMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SupplierController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public SupplierController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Suppliers/Index
        public async Task<IActionResult> Index(string search)
        {
            var suppliers = _context.Suppliers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                suppliers = suppliers.Where(s =>
                    s.Name.Contains(search) ||
                    s.ContactPerson.Contains(search) ||
                    s.Phone.Contains(search) ||
                    s.Email.Contains(search));
            }

            ViewBag.SearchTerm = search;
            return View(await suppliers.OrderBy(s => s.Name).ToListAsync());
        }

        // GET: Suppliers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var supplier = await _context.Suppliers
                .Include(s => s.Products)
                .Include(s => s.PurchaseOrders)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
                return NotFound();

            // Calculate statistics
            ViewBag.TotalProducts = supplier.Products.Count;
            ViewBag.TotalOrders = supplier.PurchaseOrders.Count;
            ViewBag.TotalSpent = supplier.PurchaseOrders
                .Where(po => po.Status == "Delivered")
                .Sum(po => po.TotalAmount);

            return View(supplier);
        }

        // GET: Suppliers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                supplier.CreatedAt = DateTime.Now;
                supplier.IsActive = true;
                _context.Add(supplier);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Supplier created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        // GET: Suppliers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound();
            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Supplier supplier)
        {
            if (id != supplier.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingSupplier = await _context.Suppliers.FindAsync(id);
                    existingSupplier.Name = supplier.Name;
                    existingSupplier.ContactPerson = supplier.ContactPerson;
                    existingSupplier.Phone = supplier.Phone;
                    existingSupplier.Email = supplier.Email;
                    existingSupplier.Address = supplier.Address;
                    existingSupplier.City = supplier.City;
                    existingSupplier.PostalCode = supplier.PostalCode;
                    existingSupplier.IsActive = supplier.IsActive;

                    _context.Update(existingSupplier);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Supplier updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupplierExists(supplier.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(supplier);
        }

        // GET: Suppliers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var supplier = await _context.Suppliers
                .Include(s => s.Products)
                .Include(s => s.PurchaseOrders)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (supplier == null)
                return NotFound();

            if (supplier.Products.Any() || supplier.PurchaseOrders.Any())
            {
                TempData["Error"] = "Cannot delete supplier because it has products or purchase orders.";
                return RedirectToAction(nameof(Index));
            }

            return View(supplier);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            
            // Check for related records
            var hasProducts = await _context.Products.AnyAsync(p => p.SupplierId == id);
            var hasOrders = await _context.PurchaseOrders.AnyAsync(po => po.SupplierId == id);
            
            if (hasProducts || hasOrders)
            {
                TempData["Error"] = "Cannot delete supplier because it has related records.";
                return RedirectToAction(nameof(Index));
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Supplier deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Suppliers/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return Json(new { success = false, message = "Supplier not found" });

            supplier.IsActive = !supplier.IsActive;
            _context.Update(supplier);
            await _context.SaveChangesAsync();

            string message = supplier.IsActive ? 
                "Supplier activated successfully!" : 
                "Supplier deactivated successfully!";

            return Json(new { 
                success = true, 
                message = message,
                isActive = supplier.IsActive 
            });
        }

        private bool SupplierExists(int id)
        {
            return _context.Suppliers.Any(e => e.Id == id);
        }
    }
}