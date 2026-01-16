using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using SIOMS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIOMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : AdminBaseController
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
                // Log error
                Console.WriteLine($"Dashboard Error: {ex.Message}");
                TempData["Error"] = "Error loading dashboard data.";
                return View(new DashboardViewModel());
            }
        }

        private async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var model = new DashboardViewModel();

            try
            {
                // Get summary statistics
                model.TotalProducts = await _context.Products.CountAsync();
                model.TotalCategories = await _context.Categories.CountAsync();
                model.TotalSuppliers = await _context.Suppliers.CountAsync();
                model.TotalCustomers = await _context.Customers.CountAsync();

                // Stock information
                model.LowStockItems = await _context.Products
                    .Where(p => p.StockQuantity <= p.MinStockLimit && p.StockQuantity > 0)
                    .CountAsync();
                model.OutOfStockItems = await _context.Products
                    .Where(p => p.StockQuantity == 0)
                    .CountAsync();
                model.TotalStockValue = await _context.Products
                    .SumAsync(p => p.StockQuantity * p.Price);

                // Order information
                model.PendingPurchaseOrders = await _context.PurchaseOrders
                    .Where(po => po.Status == "Pending")
                    .CountAsync();
                model.PendingSalesOrders = await _context.SalesOrders
                    .Where(so => so.Status == "Pending")
                    .CountAsync();
                model.TodaySales = await _context.SalesOrders
                    .Where(so => so.OrderDate.Date == DateTime.Today && so.Status == "Completed")
                    .SumAsync(so => so.GrandTotal);

                // Monthly data for charts
                model.MonthlySales = await GetMonthlySalesData();
                model.MonthlyPurchases = await GetMonthlyPurchasesData();

                // Recent alerts
                model.RecentAlerts = await GetRecentAlertsAsync();

                // Recent orders
                model.RecentOrders = await GetRecentOrdersAsync();

                // Top selling products
                model.TopSellingProducts = await GetTopSellingProductsAsync();

                // Stock by category
                model.StockByCategory = await GetStockByCategoryAsync();

                // Financial data
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                
                model.MonthlyRevenue = await _context.SalesOrders
                    .Where(so => so.OrderDate.Month == currentMonth && 
                           so.OrderDate.Year == currentYear && 
                           so.Status == "Completed")
                    .SumAsync(so => so.GrandTotal);
                
                model.MonthlyExpenses = await _context.PurchaseOrders
                    .Where(po => po.OrderDate.Month == currentMonth && 
                           po.OrderDate.Year == currentYear && 
                           po.Status == "Delivered")
                    .SumAsync(po => po.TotalAmount);
                
                model.ProfitMargin = model.MonthlyRevenue > 0 ? 
                    ((model.MonthlyRevenue - model.MonthlyExpenses) / model.MonthlyRevenue) * 100 : 0;

                return model;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetDashboardDataAsync: {ex.Message}");
                return model;
            }
        }

        private async Task<List<MonthlyData>> GetMonthlySalesData()
        {
            var currentYear = DateTime.Now.Year;
            var monthlySales = new List<MonthlyData>();

            for (int month = 1; month <= 12; month++)
            {
                var sales = await _context.SalesOrders
                    .Where(so => so.OrderDate.Month == month && 
                           so.OrderDate.Year == currentYear && 
                           so.Status == "Completed")
                    .SumAsync(so => (decimal?)so.GrandTotal) ?? 0;

                monthlySales.Add(new MonthlyData
                {
                    Month = month,
                    MonthName = new DateTime(currentYear, month, 1).ToString("MMM"),
                    Sales = sales
                });
            }

            return monthlySales;
        }

        private async Task<List<MonthlyData>> GetMonthlyPurchasesData()
        {
            var currentYear = DateTime.Now.Year;
            var monthlyPurchases = new List<MonthlyData>();

            for (int month = 1; month <= 12; month++)
            {
                var purchases = await _context.PurchaseOrders
                    .Where(po => po.OrderDate.Month == month && 
                           po.OrderDate.Year == currentYear && 
                           po.Status == "Delivered")
                    .SumAsync(po => (decimal?)po.TotalAmount) ?? 0;

                monthlyPurchases.Add(new MonthlyData
                {
                    Month = month,
                    MonthName = new DateTime(currentYear, month, 1).ToString("MMM"),
                    Purchases = purchases
                });
            }

            return monthlyPurchases;
        }

        private async Task<List<AlertViewModel>> GetRecentAlertsAsync()
        {
            return await _context.AlertLogs
                .Include(a => a.Product)
                .Where(a => !a.IsResolved)
                .OrderByDescending(a => a.AlertDate)
                .Take(10)
                .Select(a => new AlertViewModel
                {
                    Id = a.Id,
                    ProductName = a.Product.Name,
                    Message = a.Message,
                    AlertType = a.AlertType,
                    AlertDate = a.AlertDate,
                    IsResolved = a.IsResolved
                })
                .ToListAsync();
        }

        private async Task<List<RecentOrder>> GetRecentOrdersAsync()
        {
            return await _context.SalesOrders
                .Include(so => so.Customer)
                .OrderByDescending(so => so.OrderDate)
                .Take(5)
                .Select(so => new RecentOrder
                {
                    OrderNumber = so.OrderNumber,
                    CustomerName = so.Customer.Name,
                    Amount = so.GrandTotal,
                    OrderDate = so.OrderDate,
                    Status = so.Status
                })
                .ToListAsync();
        }

        private async Task<List<RecentProduct>> GetTopSellingProductsAsync()
        {
            return await _context.SalesOrderItems
                .Include(i => i.Product)
                .Include(i => i.SalesOrder)
                .Where(i => i.SalesOrder.Status == "Completed")
                .GroupBy(i => i.ProductId)
                .Select(g => new RecentProduct
                {
                    ProductName = g.First().Product.Name,
                    QuantitySold = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.TotalPrice)
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(5)
                .ToListAsync();
        }

        private async Task<List<CategoryData>> GetStockByCategoryAsync()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .Select(c => new CategoryData
                {
                    CategoryName = c.Name,
                    ProductCount = c.Products.Count,
                    TotalStock = c.Products.Sum(p => p.StockQuantity)
                })
                .ToListAsync();
        }
    }
}