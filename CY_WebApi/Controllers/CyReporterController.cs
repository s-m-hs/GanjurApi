using CY_BM;
using CY_WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CY_WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CyReporterController :ControllerBase
    {
        private readonly CyContext _db;
        
        public CyReporterController(CyContext db)
        {
            _db = db;
        }



        [HttpGet("proCategoryReport")]
        async public Task<ActionResult> proCategoryReport()
        {
            var allOrderItems = await _db.CyOrderItem
                .Where(x => x.IsVisible)
                .ToListAsync();

            var groupedResult = allOrderItems
                .GroupBy(x => x.ProductCategory)
                .Select(g => new
                {
                    Category = g.Key,
                    SumOfQuantities = g.Sum(x => x.Quantity),  // Total sum of quantities
                    TotalItems = g.Count(),
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalPrice = g.Sum(x => x.TotalPrice),
                    Items = g.Select(item => new
                    {
                        item.PartNumber,
                        item.Manufacturer,
                        item.Quantity,
                        item.UnitPrice,
                        item.TotalPrice,
                    })
                    .ToList()
                })
                .OrderByDescending(x => x.SumOfQuantities)  // Sort by sum of quantities
                .Take(12)
                .ToList();

            var labels = groupedResult.Select(x => x.Category).ToArray();
            var data = groupedResult.Select(x => x.TotalQuantity).ToArray();
            return Ok(new { labels, data, groupedResult });
        }


        [HttpGet("proCategoryReportChart")]
        async public Task<ActionResult> proCategoryReportChart()
        {
            var allOrderItems = await _db.CyOrderItem
                .Where(x => x.IsVisible)
                .ToListAsync();

            // Group by ProductCategory and get top 10 by total quantity
            var topCategories = allOrderItems
                .GroupBy(x => x.ProductCategory)
                .Select(g => new
                {
                    Category = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10)
                .ToList();

            // Extract labels and data arrays
            var labels = topCategories.Select(x => x.Category).ToArray();
            var data = topCategories.Select(x => x.TotalQuantity).ToArray();

            return Ok(new { labels, data });
        }

        [HttpGet("proCategoryReport2")]
        async public Task<ActionResult> proCategoryReport2(DateTime? fromDate, DateTime? toDate)
        {
            var fromD=fromDate==null ? DateTime.Now.AddDays(-30) : fromDate;
            var toD=toDate==null ? DateTime.Now : toDate;

            var allOrderItems = await _db.CyOrderItem
                .Where(x => x.IsVisible && x.CreateDate >= fromD  && x.CreateDate <= toD && x.CyOrder.OrderMode==Ordermode.SaleToCustomer)
                .ToListAsync();

            var groupedResult = allOrderItems
                .GroupBy(x => x.ProductCategory)
                .Select(g => new
                {
                    Category = g.Key,
                    SumOfQuantities = g.Sum(x => x.Quantity),
                    TotalItems = g.Count(),
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalPrice = g.Sum(x => x.TotalPrice),
                    Items = g.GroupBy(item => item.PartNumber)
                             .Select(partGroup => new
                             {
                                 partNumber = partGroup.Key,
                                 manufacturer = partGroup.First().Manufacturer,
                                 quantity = partGroup.Sum(x => x.Quantity),
                                 unitPrice = partGroup.First().UnitPrice,
                                 totalPrice = partGroup.Sum(x => x.TotalPrice),
                                 orderCount = partGroup.Count()
                             })
                             .OrderByDescending(x => x.quantity)
                             .ToList()
                })
                .OrderByDescending(x => x.SumOfQuantities)
                .Take(18)
                .ToList();

            var labels = groupedResult.Select(x => x.Category).ToArray();
            var data = groupedResult.Select(x => x.TotalQuantity).ToArray();
            return Ok(new { labels, data, groupedResult });
        }
    }
}
