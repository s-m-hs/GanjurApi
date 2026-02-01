using CY_DM;
using CY_WebApi.Migrations;
using CY_WebApi.Models;
using CY_WebApi.Services;
using ExcelDataReader;
using Google.Rpc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CY_WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class CyProductsBController : ControllerBase
    {
        private readonly CyContext _db;

        public CyProductsBController(CyContext db)
        {

            _db = db;
        }

        [HttpGet("getAllProduct")]
        public async Task<ActionResult> getAllProduct(int statae = 0)
        {
            if (statae == 0)
            {
                var allProduct = await _db.CyProduct.AsNoTracking().Where(x => x.IsVisible && x.CyProductCategoryId != null).OrderBy(o => o.Name).
                    Select(s => new {
                        Id = s.ID,
                        Name = s.Name,
                        PartNo = s.PartNo,
                        ProductCode = s.ProductCode,
                        Supply = s.Supply,
                        ProductCategory = s.CyProductCategory.Name,
                        Manufacturer = s.CyManufacturer.Name,
                        Description = s.Description,
                        ShopPrice = s.ShopPrice,
                        Price = s.Price,
                        Price2 = s.Price2,
                        Price3 = s.Price3,
                        Price4 = s.Price4,
                        Price5 = s.Price5,
                        PartnerPrice = s.PartnerPrice,

                    }).ToListAsync();


                return Ok(allProduct);
            }
            var avalabeProduct = await _db.CyProduct.AsNoTracking().Where(x => x.IsVisible && x.Supply > 0 && x.CyProductCategoryId != null).OrderBy(o => o.Name)
             .Select(s => new {
                 Id = s.ID,
                 Name = s.Name,
                 PartNo = s.PartNo,
                 ProductCode = s.ProductCode,
                 Supply = s.Supply,
                 ProductCategory = s.CyProductCategory.Name,
                 Manufacturer = s.CyManufacturer.Name,
                 Description = s.Description,
                 ShopPrice = s.ShopPrice,
                 Price = s.Price,
                 Price2 = s.Price2,
                 Price3 = s.Price3,
                 Price4 = s.Price4,
                 Price5 = s.Price5,
                 PartnerPrice = s.PartnerPrice,

             }).ToListAsync();

            return Ok(avalabeProduct);

        }


        [HttpGet("getProductCategory")]
        public async Task<ActionResult> getProductCategory(int catRoot)
        {
            if (catRoot != 0)
            {
                var allProCat = _db.CyProductCategory.Where(x => x.IsVisible && x.RootId == catRoot).Select(s => new
                {
                    Id = s.ID,
                    Name = s.Name,
                    Code = s.Code,
                    RootId = s.RootId,
                    Child = s.Childs,
                    ParentId = s.ParentId
                }).ToList();

                return Ok(allProCat);
            }
            var ParntProCat = _db.CyProductCategory.Where(x => x.IsVisible && x.RootId == null).Select(s => new
            {
                Id = s.ID,
                Name = s.Name,
                Code = s.Code,
                RootId = s.RootId,
                //Child = s.Childs,
                ParentId = s.ParentId

            }).ToList();

            return Ok(ParntProCat);
        }






        [HttpGet("getProductByProCategory")]
        public async Task<ActionResult> getProductByProCategory(int proCatId, bool isSupply = true)
        {
            var query = _db.CyProduct.AsNoTracking().Where(x => x.IsVisible && x.CyProductCategoryId == proCatId).OrderBy(o => o.Name).Select(s => new
            {
                Id = s.ID,
                Name = s.Name,
                PartNo = s.PartNo,
                ProductCode = s.ProductCode,
                Supply = s.Supply,
                ProductCategory = s.CyProductCategory.Name,
                Manufacturer = s.CyManufacturer.Name,
                Description = s.Description,
                ShopPrice = s.ShopPrice,
                Price = s.Price,
                Price2 = s.Price2,
                Price3 = s.Price3,
                Price4 = s.Price4,
                Price5 = s.Price5,
                PartnerPrice = s.PartnerPrice,

            }).AsQueryable();

            if (isSupply == false)
            {
                var avalbleProducts = await query.Where(x => x.Supply > 0).ToListAsync();

                return Ok(avalbleProducts);
            }

            var allProducts = await query.ToListAsync();

            return Ok(allProducts);
        }

        //[HttpGet("getAllProToExell")]
        //public async Task<ActionResult> getAllProToExell()
        //{
        //    var products = await _db.CyProduct
        //        .Where(x => x.IsVisible
        //            && x.CyProductCategoryId != null
        //            && x.CyCategoryId != null).OrderBy(o => o.CyProductCategoryId).OrderByDescending(o => o.Supply)
        //        .Include(i => i.CyProductCategory).Select(s => new
        //        {
        //            Id = s.ID,
        //            Name = s.Name,
        //            Category = s.CyProductCategory.Name,
        //            Supply = s.Supply
        //        }).ToListAsync();

        //    using var workbook = new XLWorkbook();
        //    var worksheet = workbook.Worksheets.Add("Products");

        //    // Header
        //    worksheet.Cell(1, 1).Value = "ID";
        //    worksheet.Cell(1, 2).Value = " دسته بندی";
        //    worksheet.Cell(1, 3).Value = "نام محصول";
        //    worksheet.Cell(1, 4).Value = "موجودی";

        //    // Data
        //    int row = 2;
        //    foreach (var item in products)
        //    {
        //        worksheet.Cell(row, 1).Value = item.Id;
        //        worksheet.Cell(row, 2).Value = item.Category;
        //        worksheet.Cell(row, 3).Value = item.Name;
        //        worksheet.Cell(row, 4).Value = item.Supply;
        //        row++;
        //    }

        //    // Auto fit columns
        //    worksheet.Columns().AdjustToContents();

        //    using var stream = new MemoryStream();
        //    workbook.SaveAs(stream);
        //    stream.Position = 0;

        //    return File(
        //        stream.ToArray(),
        //        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //        "Products.xlsx"
        //    );
        //}




        [HttpGet("updateSupplyPriceByExell")]
        public async Task<ActionResult> updateSupplyPriceByExell(Guid input)
        {
            var selFile = await _db.CyFile.Where(f => f.FileIQ == input && f.IsVisible).FirstOrDefaultAsync();
            if (selFile == null)
                return BadRequest("no such file exists");

            var baseFolderPath = Path.Combine("FileContents", "private", selFile.FolderName);
            var relatedAddress = Path.Combine(baseFolderPath, selFile.FileIQ + selFile.FileType);
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relatedAddress);


            if (!System.IO.File.Exists(fullPath))
                return BadRequest("File not Exist!!");


            var partList = new List<listPro>();

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();

                    // Assuming the data is in the first sheet
                    var table = result.Tables[0];

                    for (int i = 1; i < table.Rows.Count; i++)
                    {
                        var Id = table.Rows[i][0].ToString();
                        var Supply = table.Rows[i][1].ToString();
                        var ShopPrice = table.Rows[i][2].ToString();


                        if (!string.IsNullOrEmpty(Id) && int.TryParse(Id, out int idd) && int.TryParse(Supply, out int quantity) && double.TryParse(ShopPrice, out double shopP))
                        {
                            partList.Add(new listPro
                            {
                                Id = idd,
                                Supply = quantity,
                                ShopPrice = shopP
                            });
                        }
                    }
                }
            }

            foreach (var item in partList)
            {
                var product = _db.CyProduct.Where(x => x.IsVisible && x.CyProductCategoryId != null && x.ID == item.Id).FirstOrDefault();
                if (product == null) return BadRequest(new { msg = "this product notFaound", product = product.Name });
                var baesPrice = item.ShopPrice;
                product.Supply = item.Supply;
                product.ShopPrice = item.ShopPrice;
                product.Price = baesPrice + (baesPrice * 40 / 100);   ///15-2   20-3 30-4   40-1    
                product.Price2 = baesPrice + (baesPrice * 15 / 100);   ///15-2   20-3 30-4   40-1    
                product.Price3 = baesPrice + (baesPrice * 20 / 100);   ///15-2   20-3 30-4   40-1    
                product.Price4 = baesPrice + (baesPrice * 30 / 100);   ///15-2   20-3 30-4   40-1    

            }

            await _db.SaveChangesAsync();


            return Ok(new { partList = partList, ListCount = partList.Count() });
        }



        [HttpGet("noneSupplyProduct")]
        async public Task<ActionResult> noneSupplyProduct()
        {
            var products = await _db.CyProduct.Where(x => x.IsVisible && x.CyProductCategoryId != null && x.ShopPrice == null).ToListAsync();

            foreach (var item in products)
            {
                item.Supply = 0;

            }

            await _db.SaveChangesAsync();

            return Ok();

        }

        public class listPro
        {
            public int Id { get; set; }
            public int Supply { get; set; }

            public double ShopPrice { get; set; }
        }


        [HttpGet("updateSupply")]
        public async Task<ActionResult> updateSupply(int id, int quntity, bool isAdd = true)
        {
            var products = await _db.CyProduct.Where(x => x.IsVisible && x.ID == id).FirstOrDefaultAsync();

            if (products == null) return BadRequest();

            if (isAdd == false)
            {

                products.Supply = products.Supply - quntity;
            }
            else
            {

                products.Supply = products.Supply + quntity;
            }



            await _db.SaveChangesAsync();

            return Ok(products);
        }


    }
}
