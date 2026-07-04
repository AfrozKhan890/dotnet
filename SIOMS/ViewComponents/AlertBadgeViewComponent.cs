using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIOMS.Data;
using SIOMS.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SIOMS.ViewComponents
{
    // ✅ ADDED: powers the top navbar alert bell with a live count of products
    // that need attention (out of stock or at/below minimum stock), per the
    // Alert Module requirement. Computed directly from Product data so it can
    // never go stale, regardless of which action last changed stock.
    public class AlertBadgeViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public AlertBadgeViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var atRiskProducts = await _context.Products
                .Where(p => p.StockQuantity <= p.MinStockLimit)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            return View(atRiskProducts);
        }
    }
}
