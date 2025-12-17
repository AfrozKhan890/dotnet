using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using SIOMS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SIOMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : AdminBaseController  // ✅ AdminBaseController inherit karein
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var dashboardData = await GetDashboardDataAsync();
                return View(dashboardData);
            }
            catch (Exception ex)
            {
                // For debugging
                ViewBag.Error = ex.Message;
                TempData["Error"] = "Error loading dashboard data. Please check database connection.";
                return View(new DashboardViewModel());
            }
        }


        private async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            return new DashboardViewModel
            {
                // Summary Cards
                TotalProducts = await _context.Products.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalSuppliers = await _context.Suppliers.CountAsync(),
                TotalCustomers = await _context.Customers.CountAsync(),
                
                // Stock Information
                LowStockItems = await _context.Products
                    .Where(p => p.StockQuantity <= p.MinStockLimit && p.StockQuantity > 0)
                    .CountAsync(),
                OutOfStockItems = await _context.Products
                    .Where(p => p.StockQuantity == 0)
                    .CountAsync(),
                TotalStockValue = await _context.Products
                    .SumAsync(p => p.StockQuantity * p.Price),
                
                // Order Information
                PendingPurchaseOrders = await _context.PurchaseOrders
                    .Where(po => po.Status == "Pending")
                    .CountAsync(),
                PendingSalesOrders = await _context.SalesOrders
                    .Where(so => so.Status == "Pending")
                    .CountAsync(),
                TodaySales = await _context.SalesOrders
                    .Where(so => so.OrderDate.Date == DateTime.Today)
                    .SumAsync(so => (decimal?)so.GrandTotal) ?? 0,
                
                // Monthly Data
                MonthlySales = await GetMonthlySales(),
                MonthlyPurchases = await GetMonthlyPurchases(),
                
                // Recent Data
                RecentAlerts = await GetRecentAlertsAsync(),
                RecentStockMovements = await GetRecentStockMovementsAsync(),
                TopSellingProducts = await GetTopSellingProductsAsync()
            };
        }

        private async Task<List<AlertViewModel>> GetRecentAlertsAsync()
        {
            return await _context.AlertLogs
                .Include(a => a.Product)
                .Where(a => !a.IsResolved)
                .OrderByDescending(a => a.AlertDate)
                .Take(5)
                .Select(a => new AlertViewModel
                {
                    Id = a.Id,
                    ProductName = a.Product != null ? a.Product.Name : "N/A",
                    Message = a.Message,
                    AlertDate = a.AlertDate,
                    AlertType = a.AlertType
                })
                .ToListAsync();
        }

        private async Task<List<StockMovement>> GetRecentStockMovementsAsync()
        {
            return await _context.StockMovements
                .Include(sm => sm.Product)
                .OrderByDescending(sm => sm.MovementDate)
                .Take(5)
                .ToListAsync();
        }

        private async Task<List<TopProductViewModel>> GetTopSellingProductsAsync()
        {
            return await _context.SalesOrderItems
                .Include(i => i.Product)
                .Include(i => i.SalesOrder)
                .Where(i => i.SalesOrder.Status == "Completed")
                .GroupBy(i => i.ProductId)
                .Select(g => new TopProductViewModel
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product != null ? g.First().Product.Name : "Unknown",
                    TotalSold = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.TotalPrice)
                })
                .OrderByDescending(p => p.TotalSold)
                .Take(5)
                .ToListAsync();
        }

        private async Task<List<MonthlyData>> GetMonthlySales()
        {
            var currentYear = DateTime.Now.Year;
            
            // Get monthly sales data
            var monthlyData = await _context.SalesOrders
                .Where(so => so.OrderDate.Year == currentYear && so.Status == "Completed")
                .GroupBy(so => so.OrderDate.Month)
                .Select(g => new 
                {
                    Month = g.Key,
                    Total = g.Sum(so => so.GrandTotal)
                })
                .ToListAsync();

            // Create result with all months
            var result = new List<MonthlyData>();
            for (int month = 1; month <= 12; month++)
            {
                var data = monthlyData.FirstOrDefault(m => m.Month == month);
                result.Add(new MonthlyData
                {
                    Month = month,
                    MonthName = new DateTime(currentYear, month, 1).ToString("MMM"),
                    Value = data?.Total ?? 0
                });
            }

            return result;
        }

        private async Task<List<MonthlyData>> GetMonthlyPurchases()
        {
            var currentYear = DateTime.Now.Year;
            
            // Get monthly purchase data
            var monthlyData = await _context.PurchaseOrders
                .Where(po => po.OrderDate.Year == currentYear && po.Status == "Delivered")
                .GroupBy(po => po.OrderDate.Month)
                .Select(g => new 
                {
                    Month = g.Key,
                    Total = g.Sum(po => po.TotalAmount)
                })
                .ToListAsync();

            // Create result with all months
            var result = new List<MonthlyData>();
            for (int month = 1; month <= 12; month++)
            {
                var data = monthlyData.FirstOrDefault(m => m.Month == month);
                result.Add(new MonthlyData
                {
                    Month = month,
                    MonthName = new DateTime(currentYear, month, 1).ToString("MMM"),
                    Value = data?.Total ?? 0
                });
            }

            return result;
        }

        // Quick stats for dashboard cards
        [HttpGet]
        public async Task<IActionResult> GetQuickStats()
        {
            var stats = new
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalOrders = await _context.SalesOrders.CountAsync(),
                LowStock = await _context.Products
                    .Where(p => p.StockQuantity <= p.MinStockLimit)
                    .CountAsync(),
                TodayRevenue = await _context.SalesOrders
                    .Where(so => so.OrderDate.Date == DateTime.Today && so.Status == "Completed")
                    .SumAsync(so => (decimal?)so.GrandTotal) ?? 0
            };

            return Json(stats);
        }

        // Recent activities
        [HttpGet]
        public async Task<IActionResult> GetRecentActivities()
        {
            var activities = new
            {
                RecentOrders = await _context.SalesOrders
                    .Include(so => so.Customer)
                    .OrderByDescending(so => so.OrderDate)
                    .Take(5)
                    .Select(so => new
                    {
                        OrderNumber = so.OrderNumber,
                        CustomerName = so.Customer != null ? so.Customer.Name : "Unknown",
                        Amount = so.GrandTotal,
                        Date = so.OrderDate.ToString("MMM dd"),
                        Status = so.Status
                    })
                    .ToListAsync(),
                
                RecentPurchases = await _context.PurchaseOrders
                    .Include(po => po.Supplier)
                    .OrderByDescending(po => po.OrderDate)
                    .Take(5)
                    .Select(po => new
                    {
                        OrderNumber = po.OrderNumber,
                        SupplierName = po.Supplier != null ? po.Supplier.Name : "Unknown",
                        Amount = po.TotalAmount,
                        Date = po.OrderDate.ToString("MMM dd"),
                        Status = po.Status
                    })
                    .ToListAsync()
            };

            return Json(activities);
        }
    }
}
