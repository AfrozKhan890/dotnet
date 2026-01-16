using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SIOMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CustomerController : AdminBaseController
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Customers/Index
        public async Task<IActionResult> Index(string search, string customerType)
        {
            var customers = _context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                customers = customers.Where(c =>
                    c.Name.Contains(search) ||
                    c.Phone.Contains(search) ||
                    c.Email.Contains(search));
            }

            if (!string.IsNullOrEmpty(customerType))
            {
                customers = customers.Where(c => c.CustomerType == customerType);
            }

            ViewBag.SearchTerm = search;
            ViewBag.CustomerType = customerType;
            return View(await customers.OrderBy(c => c.Name).ToListAsync());
        }

        // GET: Customers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var customer = await _context.Customers
                .Include(c => c.SalesOrders)
                .ThenInclude(so => so.Items)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound();

            // Calculate statistics
            ViewBag.TotalOrders = customer.SalesOrders.Count;
            ViewBag.TotalSpent = customer.SalesOrders
                .Where(so => so.Status == "Completed")
                .Sum(so => so.GrandTotal);
            ViewBag.AvgOrderValue = customer.SalesOrders
                .Where(so => so.Status == "Completed")
                .DefaultIfEmpty()
                .Average(so => so?.GrandTotal) ?? 0;

            return View(customer);
        }

        // GET: Customers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.Now;
                customer.IsActive = true;
                _context.Add(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound();
            return View(customer);
        }

        // POST: Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.Customers.FindAsync(id);
                    existingCustomer.Name = customer.Name;
                    existingCustomer.Phone = customer.Phone;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.Address = customer.Address;
                    existingCustomer.City = customer.City;
                    existingCustomer.PostalCode = customer.PostalCode;
                    existingCustomer.CustomerType = customer.CustomerType;
                    existingCustomer.IsActive = customer.IsActive;

                    _context.Update(existingCustomer);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Customer updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Customers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var customer = await _context.Customers
                .Include(c => c.SalesOrders)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound();

            if (customer.SalesOrders.Any())
            {
                TempData["Error"] = "Cannot delete customer because it has sales orders.";
                return RedirectToAction(nameof(Index));
            }

            return View(customer);
        }

        // POST: Customers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            
            // Check for related records
            var hasOrders = await _context.SalesOrders.AnyAsync(so => so.CustomerId == id);
            if (hasOrders)
            {
                TempData["Error"] = "Cannot delete customer because it has sales orders.";
                return RedirectToAction(nameof(Index));
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Customer deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Customers/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return Json(new { success = false, message = "Customer not found" });

            customer.IsActive = !customer.IsActive;
            _context.Update(customer);
            await _context.SaveChangesAsync();

            string message = customer.IsActive ? 
                "Customer activated successfully!" : 
                "Customer deactivated successfully!";

            return Json(new { 
                success = true, 
                message = message,
                isActive = customer.IsActive 
            });
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}