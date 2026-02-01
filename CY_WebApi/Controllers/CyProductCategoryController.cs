using AutoMapper;
using CY_BM;
using CY_DM;
using CY_WebApi.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Validations;
using NuGet.Protocol.Core.Types;
using CY_WebApi.Services;
using System.Security.Claims;
using CY_WebApi.Models;
using Microsoft.Data.SqlClient;

namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CyProductCategoryController : ControllerBase
    {
        private readonly Repository<CyProductCategory> _repo;
        private readonly Repository<CyProduct> _productRepo;
        private readonly IMapper _mapper;
        private readonly CyContext _db;
        public CyProductCategoryController(CyContext context, IMapper mapper)
        {
            _repo = new Repository<CyProductCategory>(context);
            _productRepo = new Repository<CyProduct>(context);
            _db = context;
            _mapper = mapper;
        }

        //Seyyedi Edited

        [HttpGet]
        public async Task<IActionResult> getAllProductCategories()
        {
            var productCategories = await _repo.TableNoTracking.Where(pc => pc.IsVisible).OrderBy(pc => pc.OrderValue).ToListAsync();
            if (productCategories.Count == 0)
                return NotFound("دسته بندی تخصصی پیدا نشد");

            var result = _mapper.Map<List<ProductCategoryDTO>>(productCategories);
            return Ok(result);
        }

        [HttpPost("getPagedProductCategories")]
        public async Task<IActionResult> getPagedProductCategories([FromBody] PagedProdCatDTO input)
        {
            var skipCount = input.PageNumber * input.PageSize;

            var allcount = await _repo.Table.Where(m => m.IsVisible).CountAsync();


            var resu = await _repo.TableNoTracking.OrderByDescending(c => c.ID).Skip(skipCount).Take(input.PageSize).ToListAsync();
            var pagedres = new PageResultDTO<ProductCategoryDTO>
            {
                ItemList = resu.Select(c => _mapper.Map<ProductCategoryDTO>(c)).ToList(),
                AllCount = allcount,
            };
            return Ok(pagedres);
        }

        //[TypeAuthorize([UserType.SysAdmin])]
        [HttpPost("postNewProductCategory")]
        public async Task<IActionResult> postProductCategory([FromBody] ProductCategoryDTO newProdCat)
        {
            //var identity = User.Identities.FirstOrDefault();
            //if (identity == null)
            //    return Unauthorized("کاربر پیدا نشد");
            //var userid = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            newProdCat.ProductCount = 0;
            var toInsert = _mapper.Map<CyProductCategory>(newProdCat);
            var i = await _repo.Insert(toInsert);
            return Ok(i);
        }

        //[TypeAuthorize([UserType.SysAdmin])]
        [HttpDelete("deleteProductCategory/{id}")]
        public async Task<IActionResult> deleteProductCategory(int id)
        {
            //var identity = User.Identities.FirstOrDefault();
            //if (identity == null)
            //    return Unauthorized("کاربر پیدا نشد");
            //var userid = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

            await _repo.Delete(id);
            return Ok("دسته بندی محصول با موفقیت حذف شد");
        }

        //[TypeAuthorize([UserType.SysAdmin])]
        [HttpPut("updateProdCat/{id}")]
        public async Task<IActionResult> updateProdCat(ProductCategoryDTO toUpdate, int id)
        {
            //var identity = User.Identities.FirstOrDefault();
            //if (identity == null)
            //    return Unauthorized("کاربر پیدا نشد");
            //var userid = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
            if (toUpdate.ID != id)
            {
                return BadRequest();
            }
            var newProdCat = _mapper.Map<CyProductCategory>(toUpdate);
            var oldProdCat = await _repo.TableNoTracking.Where(pc => pc.IsVisible && pc.ID == toUpdate.ID).FirstOrDefaultAsync();
            if (oldProdCat != null)
            {
                newProdCat.ProductCount = oldProdCat.ProductCount;
                await _repo.Update(newProdCat);
                return Ok();
            }
            return NotFound();
        }

        [HttpGet("GetProductCategory")]
        public async Task<ActionResult<ProductCategoryDTO>> GetProductCategoryWithCode(string url)
        {
            var productCategory = await _repo.TableNoTracking.Where(pc => pc.Url == url && pc.IsVisible).FirstOrDefaultAsync();
            var result = _mapper.Map<ProductCategoryDTO>(productCategory);
            return Ok(result);
        }

        [HttpGet("GetSubCategories")]
        public async Task<ActionResult<List<ProductCategoryDTO>>> GetSubCategories(string? url)
        {
            ICollection<CyProductCategory>? subCategories;
            Dictionary<int, ProductCategoryDTO>? allCategories;
            var topCategories = new List<ProductCategoryDTO>();
            if (string.IsNullOrEmpty(url))
            {
                allCategories = await _repo.TableNoTracking.Where(pc => pc.IsVisible).Select(
                    pc => new ProductCategoryDTO()
                    {
                    ID = pc.ID,
                    Code = pc.Code,
                    Name = pc.Name,
                    Url = pc.Url,
                    OrderValue = pc.OrderValue,
                    RootId = pc.RootId,
                    ImageUrl = pc.ImageUrl,
                    Description = "",
                    ProductCount = pc.ProductCount,
                    Childs = new List<ProductCategoryDTO>()
                    }
                    ).ToDictionaryAsync(pc=>pc.ID);

                foreach (var node in allCategories)
                {
                    if (node.Value.RootId == null)
                    {
                        // This is a root node
                        topCategories.Add(node.Value);
                    }
                    else
                    {
                        // This node has a parent, so find the parent and add it to the parent's children collection
                        if (allCategories.TryGetValue((int)node.Value.RootId, out var parent))
                        {
                            parent.Childs?.Add(node.Value);
                        }
                    }
                }

                return Ok(topCategories);
            }
            else
            {
                subCategories = await _repo.Table.Where(pc => pc.RootCategory != null &&
                                                        pc.RootCategory.Url == url &&
                                                        pc.IsVisible)
                    .Include(pc => pc.Childs)
                                                 .ToListAsync();
            }
            var result = _mapper.Map<List<ProductCategoryDTO>>(subCategories);
            return Ok(result);
        }

        [HttpPost()]
        [Route("GetItemWChildAndRoot")]
        public async Task<ActionResult<CategoryDTO>> GetItemWChildAndRoot(GIDTO gdto)
        {
            var cyProductCategoryItem = await _repo.Table.Where(c => c.ID == gdto.ID && c.IsVisible).Include(c => c.RootCategory).Include(c => c.Childs).FirstOrDefaultAsync();

            if (cyProductCategoryItem == null)
            {
                return NotFound();
            }
            var MIWRAC = new ProductCategoryWithRootAndChild()
            {
                Item = _mapper.Map<ProductCategoryDTO>(cyProductCategoryItem),
                Root = cyProductCategoryItem.RootCategory is null ? null : _mapper.Map<ProductCategoryDTO>(cyProductCategoryItem.RootCategory),
                Childs = cyProductCategoryItem.Childs is null ? null : cyProductCategoryItem.Childs.Where(c => c.IsVisible).Select(c => _mapper.Map<ProductCategoryDTO>(c)).ToList()
            };
            return Ok(MIWRAC);
        } 

        [HttpGet()]
        [Route("GetFilterTitles")]
        public async Task<IActionResult> GetFilterTitles(string url)
        {
            //var FilterTitles = await _db.CyProductSpec.Where(ps => ps.IsVisible &&
            //                                                 ps.CyProductCategory.Code == prodCatCode)
            //                                          .Select(c => new { c.Name })
            //                                          .Distinct()
            //                                          .ToListAsync();
            var FilterTitles = await _db.CyProductSpec
                           .Where(ps => ps.IsVisible && ps.CyProductCategory.Url == url)
                           .GroupBy(c => c.Name)
                           .Select(g => new
                           FilterValue()
                           {
                               Name = g.Key,
                               Values = g.Select(v => v.Value).Distinct().ToList()
                           })
                           .ToListAsync();
            return Ok(FilterTitles);
        }

        [HttpPost()]
        [Route("searchWithFilter")]
        public async Task<ActionResult<List<ProductDTO>>> searchWithFilter([FromBody] ProdcutSearchFliterDTO input)
        {
            string Names = "";
            string Values = "";
            if (input.FilterValues != null)
            {
                foreach (KeyValuePair<string, string> item in input.FilterValues)
                {
                    Names += @$"{item.Key},";
                    Values += @$"{item.Value},";
                }
                Names = Names.TrimEnd(',');
                Values = Values.TrimEnd(',');
                string query = @$"exec get_product_data @ProductCategoryId = @ProductCategoryIdParam,
                                            @Names = @NamesParam,
                                            @Values = @ValuesParam,
                                            @PageSize = @PageSizeParam,
                                            @PageNumber = @PageNumberParam;";
                var parameters = new[]
                {
                    new SqlParameter("@ProductCategoryIdParam", input.ProductCategoryId),
                    new SqlParameter("@NamesParam", Names),
                    new SqlParameter("@ValuesParam", Values),
                    new SqlParameter("@PageSizeParam", input.PageSize),
                    new SqlParameter("@PageNumberParam", input.PageNumber)
                };
                var products = await _db.CyProduct.FromSqlRaw(query, parameters).ToListAsync();
                var result = _mapper.Map<List<ProductDTO>>(products);
                return Ok(result);
                
            }
            return BadRequest("فیلتر ها را وارد کنید");
        }

        [HttpGet("searchProductCategory/{searched}")]
        public async Task<ActionResult<List<ProductCategoryDTO>>> SearchProductCategories(string searched)
        {
            if (string.IsNullOrEmpty(searched.Trim()))
                return BadRequest("عبارت جست و جو را وارد کنید");
            var ProductCategoryList = await _repo.TableNoTracking.Where(m => m.Name.Contains(searched)).ToListAsync();
            var res = _mapper.Map<List<ProductCategoryDTO>>(ProductCategoryList);
            return Ok(res);
        }
    }
}