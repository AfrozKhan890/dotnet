namespace SIOMS.ViewModels  // ← YEH LINE ADD KARO
{
public class ReportViewModel
{
    public List<MonthlySalesData> MonthlySales { get; set; } = new();
    public int TotalProducts { get; set; }
    public int LowStockItems { get; set; }
    public int OutOfStockItems { get; set; }
    public decimal TotalSales { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class MonthlySalesData
{
    public string MonthName { get; set; } = string.Empty;
    public decimal Sales { get; set; }
}
}