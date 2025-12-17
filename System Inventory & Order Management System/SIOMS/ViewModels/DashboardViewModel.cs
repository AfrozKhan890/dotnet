using System;
using System.Collections.Generic;
using SIOMS.Models;  

namespace SIOMS.ViewModels
{
    public class DashboardViewModel
    {
        // Summary Cards
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalCustomers { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public decimal TotalStockValue { get; set; }
        public int PendingPurchaseOrders { get; set; }
        public int PendingSalesOrders { get; set; }
        public decimal TodaySales { get; set; }
        
        // Monthly Data
        public List<MonthlyData> MonthlySales { get; set; } = new List<MonthlyData>();
        public List<MonthlyData> MonthlyPurchases { get; set; } = new List<MonthlyData>();
        
        // Recent Data
        public List<AlertViewModel> RecentAlerts { get; set; } = new List<AlertViewModel>();
        public List<StockMovement> RecentStockMovements { get; set; } = new List<StockMovement>();
        public List<TopProductViewModel> TopSellingProducts { get; set; } = new List<TopProductViewModel>();
        
        // Additional properties for charts
        public List<string> CourseNames { get; set; } = new List<string>();
        public List<int> StudentsPerCourse { get; set; } = new List<int>();
        public List<int> MonthlyAdmissions { get; set; } = new List<int>();
        public int UpcomingExams { get; set; } = 0;
    }

    public class MonthlyData
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class AlertViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public DateTime AlertDate { get; set; }
    }

    public class TopProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }
    
    // Additional ViewModel classes for dashboard
    public class Message
    {
        public string Name { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
    
    public class AdmissionVM
    {
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public DateTime AdmissionDate { get; set; }
        public string Message { get; set; } = string.Empty;
    }
    
    public class CourseViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int TopicsCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}