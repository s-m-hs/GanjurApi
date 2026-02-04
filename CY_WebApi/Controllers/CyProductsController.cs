using AutoMapper;
using CY_BM;
using CY_DM;
using CY_WebApi.DataAccess;
using CY_WebApi.Models;
using CY_WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using ClosedXML.Excel;

namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CyProductsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly Repository<CyProduct> _repo;
        private readonly CyContext _db;
        private readonly ExcelReader _excelReader;

        public static string NormalizePartNumber(string? input)
        {
            return input?.Trim().Replace(" ", "").ToLowerInvariant() ?? string.Empty;
        }
        public CyProductsController(CyContext context, IMapper mapper, ExcelReader excelReader)
        {
            _mapper = mapper;
            _repo = new Repository<CyProduct>(context);
            _excelReader = excelReader;
            _db = context;
        }

        // GET: api/CyProducts
        [HttpPost("getAllProducts")]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetCyProduct(PageDTO PD)
        {
            var skipCount = PD.PageNumber * PD.PageSize;

            var allcount = await _repo.Table.Where(c => c.IsVisible).CountAsync();

            //For CHIPYAB
            //var resu = await _repo.TableNoTracking.OrderByDescending(c => c.ID).Skip(skipCount).Take(PD.PageSize).ToListAsync();

            //For Sane
            var resu = await _repo.TableNoTracking.OrderByDescending(c => c.ID).Skip(skipCount).Take(PD.PageSize).ToListAsync();// ; _db.CyProduct.ToListAsync();
            var pagedres = new PageResultDTO<ProductDTO>
            {

                ItemList = resu.Select(c => _mapper.Map<ProductDTO>(c)).ToList(),
                AllCount = allcount,
            };
            return Ok(pagedres);
        }

        // GET: api/CyProducts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetCyProduct(int id)
        {
            var CyProduct = await _repo.TableNoTracking.Where(c => c.ID == id && c.IsVisible).Include(d => d.Specifications).Include(m => m.CyManufacturer).FirstOrDefaultAsync();
            if (CyProduct == null)
            {
                return NotFound();
            }
            var prdDto = _mapper.Map<ProductDTO>(CyProduct);
            if (CyProduct.Specifications != null && CyProduct.Specifications.Count > 0)
            {
                prdDto.Spec = CyProduct.Specifications.Select(c => _mapper.Map<ProductSpecDTO>(c)).ToList();
            }
            prdDto.Manufacturer = CyProduct.CyManufacturer?.Name;
            return Ok(prdDto);
        }

        [HttpGet("prod/{PartNumber}")]
        public async Task<ActionResult<ProductDTO>> GetCyProductByPartNumber(string PartNumber)
        {
            var CyProduct = await _repo.TableNoTracking.Where(c => (c.ProductCode == PartNumber || c.ID.ToString() == PartNumber) &&
                                                            c.IsVisible)
                                                       .Include(m => m.CyManufacturer)
                                                       .FirstOrDefaultAsync();
            if (CyProduct == null)
            {
                return NotFound();
            }
            var prdDto = _mapper.Map<ProductDTO>(CyProduct);
            prdDto.Manufacturer = CyProduct.CyManufacturer?.Name;
            return Ok(prdDto);
        }

        [HttpGet("prod/spec/{id}")]
        public async Task<ActionResult<List<ProductSpecDTO>>> GetProductSpec(int id)
        {
            var prodSpec = await _db.CyProductSpec.Where(spec => spec.IsVisible && spec.CyProductId == id)
                                                  .Select(spec => new ProductSpecDTO()
                                                  {
                                                      Name = spec.Name,
                                                      Value = spec.Value,
                                                      CyProductId = id
                                                  })
                                                  .ToListAsync();
            return Ok(prodSpec);
        }

        [HttpPost("SearchProducts")]
        public async Task<IActionResult> SearchProductByAdmin(ProdcutSearchDTO input)
        {

            if (input.PageSize < 1)
            {
                return BadRequest("تعداد نتایج در هر صفحه را وارد کنید");
            }
            var skipCount = input.PageNumber * input.PageSize;

            var Query = _repo.Table.Where(c => c.IsVisible);

            if (input.Name != null)
            {   //For Chipyab
                //Query = Query.Where(c => c.Name.StartsWith(input.Name));

                //For Sane
                Query = Query.Where(c => c.Name.Contains(input.Name));
            }

            if (input.CategoryCode != null)
            {
                input.CategoryCode = input.CategoryCode.Trim();
                Query = Query.Where(c => c.CyCategory.Code == input.CategoryCode);
            }

            if (input.ProductCategoryId != null)
            {
                Query = Query.Where(c => c.CyProductCategoryId == input.ProductCategoryId);
            }
            else if (input.ProductCategoryCode != null)
            {
                input.ProductCategoryCode = input.ProductCategoryCode.Trim();
                Query = Query.Where(c => c.CyProductCategory.Code == input.ProductCategoryCode || c.CyProductCategory.Url == input.ProductCategoryCode);
            }

            if (input.ManufacturerName != null)
            {
                input.ManufacturerName = input.ManufacturerName.Trim();
                Query = Query.Where(c => c.CyManufacturer.Code == input.ManufacturerName);
            }

            var products = await Query.Skip(skipCount).Take(input.PageSize).ToListAsync();
            var resu = _mapper.Map<List<ProductDTO>>(products);
            var pagedres = new PageResultDTO<ProductDTO>
            {

                ItemList = resu.Select(c => _mapper.Map<ProductDTO>(c)).ToList(),
                AllCount = Query.Count(),
            };
            return Ok(pagedres);
        }

        // PUT: api/CyProducts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCyProduct(int id, ProductDTO product)
        {
            var CyProduct = _mapper.Map<CyProduct>(product);

            if (id != CyProduct.ID)
            {
                return BadRequest("Not Valid ID");
            }
            try
            {
                int cg1 = await _repo.Update(CyProduct);//  _db.SaveChangesAsync();
                if (product.Spec != null && product.Spec.Count > 0)
                {
                    var spec = await _db.CyProductSpec.Where(c => c.CyProductId == id).ToListAsync();
                    var newSpec = product.Spec;
                    var removeItem = new List<CyProductSpec>();
                    foreach (var item in spec)
                    {
                        var fItem = newSpec.Find(c => c.ID == item.ID);
                        if (fItem != null)
                        {
                            item.Value = fItem.Value;
                            item.Name = fItem.Name;
                        }
                        else
                        {
                            removeItem.Add(item);
                        }
                    }
                    var newItem = newSpec.Where(c => c.ID < 1).ToList();
                    if (newItem != null && newItem.Count > 0)
                    {
                        _db.CyProductSpec.AddRange(newItem.Select(c => _mapper.Map<CyProductSpec>(c)).ToList());
                    }
                    if (removeItem.Count > 0)
                    {
                        _db.CyProductSpec.RemoveRange(removeItem);
                    }
                    Ok(new { resu = await _db.SaveChangesAsync() + cg1 });
                }


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return NoContent();
        }

        // POST: api/CyProducts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> PostCyProduct(ProductDTO ProductDTO)
        {
            var CyProduct = _mapper.Map<CyProduct>(ProductDTO);

            if (ProductDTO.Spec != null && ProductDTO.Spec.Count > 0)
            {
                var cySpec = ProductDTO.Spec.Select(c => _mapper.Map<CyProductSpec>(c)).ToList();
                CyProduct.Specifications = cySpec;
            }

            CyProduct = await _repo.Insert(CyProduct);

            return CreatedAtAction("GetCyProduct", new { id = CyProduct.ID }, _mapper.Map<ProductDTO>(CyProduct));
        }

        //[HttpPost("UpdateByExcel")]
        //public async Task<IActionResult> UpdateByExcel(Guid input)
        //{
        //    //var identity = User.Identities.FirstOrDefault();
        //    //if (identity == null)
        //    //    return Unauthorized("کاربر پیدا نشد");
        //    //var userid = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

        //    var selFile = await _db.CyFile.Where(f => f.FileIQ == input && f.IsVisible).FirstOrDefaultAsync();
        //    if (selFile != null)
        //    {
        //        var BaseFolderPath = Path.Combine("FileContents", "private", selFile.FolderName);
        //        var RelatedAddress = Path.Combine(BaseFolderPath, selFile.FileIQ + selFile.FileType);
        //        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), RelatedAddress);

        //        if (!System.IO.File.Exists(fullPath))
        //        {
        //            return BadRequest("File not Exist!!");
        //        }

        //        var fileItems = _excelReader.ReadExcelForUpdating(fullPath);
        //        foreach (var partInfo in fileItems)
        //        {
        //            partInfo.PartNumber = partInfo.PartNumber?.Replace(" ","");
        //            var product = await _db.CyProduct.FirstOrDefaultAsync(p => p.IsVisible && (p.PartNo.Replace(" ","") == partInfo.PartNumber
        //            //||       p.Name.Replace(" ","") == partInfo.PartNumber

        //                                                                          ));
        //            if (product != null)
        //            {
        //                product.Supply = partInfo.Quantity;
        //                product.PartnerPrice = partInfo.PartnerPrice;
        //                if (product.Price == product.NoOffPrice)
        //                {
        //                    product.NoOffPrice = partInfo.Price;
        //                    product.Price = partInfo.Price;
        //                }
        //            }
        //            else
        //            {
        //                if (partInfo.PartNumber != null)
        //                {
        //                    var newProduct = new CyProduct()
        //                    {
        //                        PartnerPrice = partInfo.PartnerPrice,
        //                        Name = partInfo.PartNumber,
        //                        PartNo = partInfo.PartNumber,
        //                        ProductCode = partInfo.PartNumber,
        //                        NoOffPrice = partInfo.Price,
        //                        //For Sane only:
        //                        Price = partInfo.Price,

        //                        Supply = partInfo.Quantity
        //                    };
        //                    await _repo.Insert(newProduct);
        //                }
        //            }
        //        }
        //        return Ok(await _repo.SaveChangesAsync());
        //    }
        //    else
        //    {
        //        return BadRequest("no such file exists");
        //    }

        //}

        [HttpPost("UpdateByExcel")]
        public async Task<IActionResult> UpdateByExcel(Guid input)
        {
            var selFile = await _db.CyFile.Where(f => f.FileIQ == input && f.IsVisible).FirstOrDefaultAsync();
            if (selFile == null)
                return BadRequest("no such file exists");

            var baseFolderPath = Path.Combine("FileContents", "private", selFile.FolderName);
            var relatedAddress = Path.Combine(baseFolderPath, selFile.FileIQ + selFile.FileType);
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relatedAddress);

            if (!System.IO.File.Exists(fullPath))
                return BadRequest("File not Exist!!");

            var fileItems = _excelReader.ReadExcelForUpdating(fullPath);

            foreach (var partInfo in fileItems)
            {
                var normalizedInputPart = NormalizePartNumber(partInfo.PartNumber);

                if (string.IsNullOrWhiteSpace(normalizedInputPart))
                    continue;

                var product = _db.CyProduct
                    .Where(p => p.IsVisible)
                    .AsEnumerable()
                    .FirstOrDefault(p => NormalizePartNumber(p.PartNo) == normalizedInputPart);

                if (product != null)
                {
                    product.Supply = partInfo.Quantity;
                    product.PartnerPrice = partInfo.PartnerPrice;

                    if (product.Price == product.NoOffPrice)
                    {
                        product.Price = partInfo.Price;
                        product.NoOffPrice = partInfo.Price;
                    }
                }
                else
                {
                    var newProduct = new CyProduct
                    {
                        PartnerPrice = partInfo.PartnerPrice,
                        Name = partInfo.PartNumber,
                        PartNo = partInfo.PartNumber,
                        ProductCode = partInfo.PartNumber,
                        NoOffPrice = partInfo.Price,
                        Price = partInfo.Price,
                        Supply = partInfo.Quantity
                    };
                    await _repo.Insert(newProduct);
                }
            }
            return Ok(await _repo.SaveChangesAsync());
        }



        [HttpPost("UpdateByExcelV2")]
        public async Task<IActionResult> UpdateByExcelV2(Guid input)
        {
            //var identity = User.Identities.FirstOrDefault();
            //if (identity == null)
            //    return Unauthorized("کاربر پیدا نشد");
            //var userid = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            var selFile = await _db.CyFile.Where(f => f.FileIQ == input && f.IsVisible).FirstOrDefaultAsync();
            if (selFile != null)
            {
                var BaseFolderPath = Path.Combine("FileContents", "private", selFile.FolderName);
                var RelatedAddress = Path.Combine(BaseFolderPath, selFile.FileIQ + selFile.FileType);
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), RelatedAddress);

                if (!System.IO.File.Exists(fullPath))
                {
                    return BadRequest("File not Exist!!");
                }

                var fileItems = _excelReader.ReadExcelForUpdating(fullPath);
                var products = await _repo.TableNoTracking.Include(p => p.CyManufacturer).ToListAsync();
                var manufactures = await _db.CyManufacturer.Where(m => m.IsVisible).ToListAsync();
                foreach (var partInfo in fileItems)
                {
                    partInfo.PartNumber = partInfo.PartNumber?.Trim();
                    var product = products.FirstOrDefault(p => p.IsVisible && (p.PartNo == partInfo.PartNumber ||
                                                                                  p.Name == partInfo.PartNumber
                                                                                  ));

                    if (product != null)
                    {
                        product.Supply = partInfo.Quantity;
                        product.NoOffPrice = partInfo.Price;
                        product.Price = partInfo.Price;
                    }
                    else
                    {
                        var newProduct = new CyProduct()
                        {
                            Name = partInfo.PartNumber,
                            PartNo = partInfo.PartNumber,
                            ProductCode = partInfo.PartNumber,
                            NoOffPrice = partInfo.Price,
                            //For Sane only:
                            Price = partInfo.Price,


                            Supply = partInfo.Quantity
                        };
                        if (!string.IsNullOrEmpty(partInfo.Manufacturer))
                        {
                            var manufacturer = manufactures.FirstOrDefault(m => m.Name == partInfo.Manufacturer);
                            if (manufacturer == null)
                            {
                                CyManufacturer entity = new CyManufacturer()
                                {
                                    Name = partInfo.Manufacturer
                                };
                                var insertedManufacturer = _db.CyManufacturer.Add(entity);
                                newProduct.CyManufacturer = insertedManufacturer.Entity;
                            }
                            else
                            {
                                newProduct.CyManufacturer = manufacturer; 
                            }
                        }
                        _repo.InsertWithoutSave(newProduct);
                    }
                }
                        return Ok(await _db.SaveChangesAsync());
            }
            else
            {
                return BadRequest("no such file exists");
            }
        }

        [HttpPut("UpdateExcelForSane")]
        public async Task<ActionResult> updateSane(Guid guid, int id)
        {
            var selFile = await _db.CyFile.FirstOrDefaultAsync(f => f.FileIQ == guid && f.IsVisible);
            if (selFile == null)
                return BadRequest("File not found");

            var baseFolderPath = Path.Combine("FileContents", "private", selFile.FolderName);
            var relatedAddress = Path.Combine(baseFolderPath, selFile.FileIQ + selFile.FileType);
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), relatedAddress);

            if (!System.IO.File.Exists(fullPath))
                return BadRequest("File not Exist!!");

            var fileItems = _excelReader.ReadExcelForUpdating(fullPath);

            // ✅ لیست پارت نامبرهای فایل اکسل (نرمال شده)
            var fileItemNames = fileItems
                .Select(c => NormalizePartNumber(c.PartNumber))
                .Where(x => !string.IsNullOrEmpty(x))
                .ToHashSet();

            // ✅ لیست پارت نامبر محصولات دسته موردنظر (نرمال شده)
            var specCategoryProducts = await _repo.Table
                .Where(p => p.IsVisible && p.CyCategoryId == id)
                .Select(p => NormalizePartNumber(p.PartNo))
                .ToListAsync();

            var specCategorySet = new HashSet<string>(specCategoryProducts);

            var allProducts = await _repo.Table.Where(p => p.IsVisible).ToListAsync();

            foreach (var product in allProducts)
            {
                var normalizedPartNo = NormalizePartNumber(product.PartNo);

                if (!fileItemNames.Contains(normalizedPartNo) && !specCategorySet.Contains(normalizedPartNo))
                {
                    product.Supply = 0;
                }
            }

            await _repo.SaveChangesAsync();
            return Ok();
        }




        //[HttpPut("UpdateExcelForSane")]
        //public async Task<ActionResult> updateSane(Guid guid, int id)
        //{
        //    var selFile = await _db.CyFile.Where(f => f.FileIQ == guid && f.IsVisible).FirstOrDefaultAsync();
        //    if (selFile != null)
        //    {
        //        var BaseFolderPath = Path.Combine("FileContents", "private", selFile.FolderName);
        //        var RelatedAddress = Path.Combine(BaseFolderPath, selFile.FileIQ + selFile.FileType);
        //        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), RelatedAddress);

        //        if (!System.IO.File.Exists(fullPath))
        //        {
        //            return BadRequest("File not Exist!!");
        //        }

        //        var fileItems = _excelReader.ReadExcelForUpdating(fullPath);
        //        var fileItemNames = fileItems.Select(c => c.PartNumber?.Replace(" ", "")).ToList();

        //        var SpecCategoryProducts = await _repo.Table.Where(p => p.IsVisible && p.CyCategoryId == id).Select(p => p.PartNo.Replace(" ", "")).ToListAsync();

        //        var allProducts = await _repo.Table.Where(p => p.IsVisible).ToListAsync();

        //        foreach (var product in allProducts)
        //        {
        //            if (!fileItemNames.Contains(product.PartNo?.Replace(" ", "")) && !SpecCategoryProducts.Contains(product.PartNo.Replace(" ", "")))
        //            {
        //                product.Supply = 0;
        //            }
        //        }
        //        Ok(await _repo.SaveChangesAsync());
        //    }
        //    return BadRequest();
        //}

        [Route("GetProductByCat")]
        [HttpPost]
        public async Task<ActionResult<PageResultDTO<ProductDTO>>> GetProductByCat(PageDTO PD)
        {
            var skipCount = PD.PageNumber * PD.PageSize;

            var allcount = await _repo.Table.Where(c => c.CyCategoryId.HasValue && c.CyCategory.Code == PD.Cat
             && c.IsVisible).CountAsync();

            var ProdList = await _repo.Table.Where(c => c.CyCategoryId.HasValue && c.CyCategory.Code == PD.Cat
            && c.IsVisible)
                .OrderBy(c => c.CreateDate).Skip(skipCount).Take(PD.PageSize).ToListAsync();
            if (ProdList != null)
            {
                var menuWitems = new PageResultDTO<ProductDTO>
                {
                    PageTitle = ProdList[0]?.CyCategory?.Text,
                    ItemList = ProdList.Select(c => _mapper.Map<ProductDTO>(c)).ToList(),
                    AllCount = allcount,
                };
                return Ok(menuWitems);
            }
            return NotFound();
        }

        [Route("GetProductByProductCat")]
        [HttpPost]
        public async Task<ActionResult<PageResultDTO<ProductDTO>>> GetProductByProdCat(PageDTO PD)
        {
            var skipCount = PD.PageNumber * PD.PageSize;

            var allcount = await _repo.Table.Where(c => c.CyProductCategoryId.HasValue && c.CyProductCategory.Code == PD.Cat
             && c.IsVisible).CountAsync();

            var ProdList = await _repo.Table.Where(c => c.CyProductCategoryId.HasValue && c.CyProductCategory.Code == PD.Cat
            && c.IsVisible)
                .OrderBy(c => c.CreateDate).Skip(skipCount).Take(PD.PageSize).ToListAsync();
            if (ProdList.Count > 0)
            {
                var menuWitems = new PageResultDTO<ProductDTO>
                {
                    //PageTitle = ProdList[0]?.CyCategory?.Text,
                    ItemList = ProdList.Select(c => _mapper.Map<ProductDTO>(c)).ToList(),
                    AllCount = allcount,
                };
                return Ok(menuWitems);
            }
            return NotFound();
        }

        [HttpPost("productsWithNull")]
        public async Task<ActionResult<PageResultDTO<ProductDTO>>> getProductWithNull(PageDTOprodWithNull PD)
        {
            var skipCount = PD.PageNumber * PD.PageSize;
            int AllCount = 0;
            List<ProductDTO> result;
            if (PD.Property == ProductProperty.manufacturer)
            {
                AllCount = await _repo.TableNoTracking.Where(p => p.IsVisible && p.CyManufacturerId == null).CountAsync();
                result = await _repo.TableNoTracking.Where(p => p.IsVisible && p.CyManufacturerId == null).Skip(skipCount).Take(PD.PageSize).Select(p => _mapper.Map<ProductDTO>(p)).ToListAsync();
            }
            else if (PD.Property == ProductProperty.category)
            {
                AllCount = await _repo.TableNoTracking.Where(p => p.IsVisible && (p.CyProductCategoryId == null || p.CyCategoryId == null)).CountAsync();
                result = await _repo.TableNoTracking.Where(p => p.IsVisible && (p.CyProductCategoryId == null || p.CyCategoryId == null)).Skip(skipCount).Take(PD.PageSize).Select(p => _mapper.Map<ProductDTO>(p)).ToListAsync();
            }
            else
            {
                AllCount = await _repo.TableNoTracking.Where(p => p.IsVisible && p.SmallImage == null).Select(p => _mapper.Map<ProductDTO>(p)).CountAsync();
                result = await _repo.TableNoTracking.Where(p => p.IsVisible && p.SmallImage == null).Skip(skipCount).Take(PD.PageSize).Select(p => _mapper.Map<ProductDTO>(p)).ToListAsync();
            }
            var pagedRes = new PageResultDTO<ProductDTO>()
            {
                ItemList = result,
                AllCount = AllCount,
            };
            return Ok(pagedRes);
        }

        // DELETE: api/CyProducts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCyProduct(int id)
        {
            await _repo.Delete(id);
            return NoContent();
        }


        [HttpGet("TorobProduct")]
        public async Task<ActionResult<ProductForTorobDTO>> getProductForTorob()
        {
            var offer= await _db.CyKeyData.Where(x=>x.ID==13).FirstOrDefaultAsync();
            double doubleOffer = double.Parse(offer.Value);

            var products = await _db.CyProduct.Where(x => x.IsVisible && x.Supply != 0 && x.CyProductCategory!=null).Select(u =>
                          new ProductForTorobDTO()
                          {
                              old_price = u.Price/10,
                              price = u.Price == u.NoOffPrice ? (u.Price * doubleOffer)/10 : u.NoOffPrice/10,
                              availability = "instock",
                              page_url = $"https://sanecomputer.com/product/{u.ID}" ,
                              product_id = u.ID

                          }).OrderBy(y=>y.product_id).Reverse().ToListAsync();
        
            return Ok(products);
        }
        
        
        [HttpGet("breadcrumbs/{id}")] 
        public async Task<ActionResult> breadcrumb(int id)
        {
            
            List<string>  breadCrumList = new List<string>() ;

            var product = await _db.CyProduct.Where(x=>x.ID == id).FirstOrDefaultAsync();
            breadCrumList.Add(product.Name);
            var productcategory= await _db.CyProductCategory.Where(x=>x.ID == product.CyProductCategoryId).FirstOrDefaultAsync();
             breadCrumList.Add(productcategory.Name);

            if (productcategory.RootId == null || productcategory.RootId == Convert.ToInt32(MainProductCategory.hardWare) || productcategory.RootId == Convert.ToInt32(MainProductCategory.Accessories)) return Ok(breadCrumList);
            var productcategoryB=await _db.CyProductCategory.Where(x=> x.ID == productcategory.RootId).FirstOrDefaultAsync();
            breadCrumList.Add(productcategoryB.Name);
            return Ok(breadCrumList);
        }
      
        
        [HttpGet("getExellFromProduct")]    
        public async Task<IActionResult> getExellFromProduct()
    {
        var products = await _db.CyProduct
            .Where(x => x.IsVisible
                && x.CyProductCategoryId != null
                && x.CyCategoryId != null
                && x.Supply>0
                )
            .Select(s => new
            {
                s.ID,
                s.Name,
                s.Supply,
                s.Price,
                s.CyManufacturerId,
                s.CyProductCategoryId,
                s.CyCategoryId,
            })
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Products");

        // Header
        worksheet.Cell(1, 1).Value = "ID";
        worksheet.Cell(1, 2).Value = "name";
        worksheet.Cell(1, 3).Value = "supply ";
        worksheet.Cell(1, 4).Value = " price";
        worksheet.Cell(1, 5).Value = "manufacturId ";
        worksheet.Cell(1, 6).Value = " proCategoryId";
        worksheet.Cell(1, 7).Value = "categoryId";

        // Data
        int row = 2;
        foreach (var item in products)
        {
            worksheet.Cell(row, 1).Value = item.ID;
            worksheet.Cell(row, 2).Value = item.Name;
            worksheet.Cell(row, 3).Value = item.Supply;
            worksheet.Cell(row, 4).Value = item.Price;
            worksheet.Cell(row, 5).Value = item.CyManufacturerId;
            worksheet.Cell(row, 6).Value = item.CyProductCategoryId;
            worksheet.Cell(row, 7).Value = item.CyCategoryId;
            row++;
        }

        // Auto fit columns
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Products.xlsx"
        );
    }




}
}
