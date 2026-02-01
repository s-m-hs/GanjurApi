using AutoMapper;
using CY_DM;
using CY_BM;
using CY_WebApi.Models;
using Google.Type;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CY_WebApi.Services;
using DateTime = System.DateTime;
using Google.Rpc;
using Google.Cloud.RecaptchaEnterprise.V1;
using System.Security.Claims;
namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CyCouponController : ControllerBase
    {
        private readonly CyContext _db;
        private readonly IMapper _mapper;
        private readonly SmsService _smsService;

        public CyCouponController(CyContext db, IMapper mapper, SmsService smsService)
        {
            _db = db;
            _mapper = mapper;
            _smsService = smsService;
        }


        [HttpPost("addCoupon")]

        public async Task<ActionResult> addNewCoupon(CouponDTO newCoupon)
        {

            if (newCoupon == null) BadRequest();
            var coupon = _mapper.Map<CyCoupon>(newCoupon);
            coupon.IsVisible = true;
            coupon.CreateDate = System.DateTime.Now;
            await _db.CyCoupon.AddAsync(coupon);
            await _db.SaveChangesAsync();
            //newCoupon.IsVisible = true;
            //newCoupon.CreateDate = System.DateTime.Now;
            return Ok(newCoupon);
        }

        [HttpDelete("deletCoupon")]
        public async Task<ActionResult> deletCoupon(int couponId)
        {
            var coupon = await _db.CyCoupon.Where(x => x.IsVisible && x.ID == couponId).FirstOrDefaultAsync();
            if (coupon == null) return NotFound();
            coupon.IsVisible = false;

            await _db.SaveChangesAsync();
            return Ok(new { response = "کد تخفیف با موفقیت حذف شد" });
        }

        [HttpGet("getAllCoupon")]

        public async Task<ActionResult> getAllCoupon()
        {
            var allCoupon = await _db.CyCoupon.Where(x => x.IsVisible).ToArrayAsync();
            List<CouponDTO> allCouponDTO = new List<CouponDTO>();
            foreach (var item in allCoupon)
            {
                var coupondto = _mapper.Map<CouponDTO>(item);
                allCouponDTO.Add(coupondto);
            }
            return Ok(allCouponDTO);
        }


        [HttpPost("AddCouponToUser")]

        public async Task<IActionResult> AddCouponToUser(int userID, int couponId)

        {
            var userCoupon = await _db.CyCouponUsages.Where(x => x.UserId == userID && x.CouponId == couponId && x.IsVisible).FirstOrDefaultAsync();
            if (userCoupon != null) return BadRequest(new { response = "این کد قبلا اضافه شده است" });

            var ActiveCoupon = await _db.CyCouponUsages.Where(x => x.IsVisible && x.UserId == userID && !x.UsedAt.HasValue && x.Coupon.ExpireDate > DateTime.UtcNow).FirstOrDefaultAsync();

            if (ActiveCoupon != null) return BadRequest(new { msg = "این کاربر کد تخفیف فعال دارد" });




            CyCouponUsage CouponItem = new();

            var user = _db.CyUser.Where(x => x.ID == userID && x.IsVisible).FirstOrDefault();
            var coupon = _db.CyCoupon.Where(x => x.ID == couponId && x.IsVisible).FirstOrDefault();
            if (user == null || coupon == null) return BadRequest();
            CouponItem.UserId = userID;
            CouponItem.CouponId = couponId;
            CouponItem.IsVisible = true;
            CouponItem.CreateDate = System.DateTime.Now;

            _db.CyCouponUsages.Add(CouponItem);
            _db.SaveChanges();



            //await _smsService.SendStringSmsAsync(user.CyUsNm, "rn3r7ehu6cppjoy", user.CyUsNm, coupon.Code);
            return Ok(_mapper.Map<CouponUsageDTO>(CouponItem));
        }

        [HttpPost("AddCouponToArrayOfUsers")]
        async public Task<ActionResult> AddCouponToArrayOfUsers([FromBody] int[] UserIds, [FromQuery] int coupomId)
        {
            if (UserIds == null || !UserIds.Any()) return BadRequest();

            //var allUser = await _db.CyUser.Where(x => x.IsVisible && UserIds.Contains(x.ID)).ToArrayAsync();

            ///کپن آیتمهای فعال 
            var activeCouponItems = await _db.CyCouponUsages.Where(x => x.IsVisible && !x.UsedAt.HasValue).Where(s => s.Coupon.ExpireDate > DateTime.UtcNow).ToArrayAsync();

            List<int> usersCouponOk = new List<int>();///لیست جهت نمایش کاربرانی که کدتخفیف جدید برای انها ست شده است 


            ///حلقه روی ارایه ای دی ها 
            foreach (var item in UserIds)
            {
                var isCouponActiveForUser = activeCouponItems.Where(x => x.UserId == item).FirstOrDefault();
                if (isCouponActiveForUser == null)
                {
                    CyCouponUsage CouponItem = new();

                    CouponItem.UserId = item;
                    CouponItem.CouponId = coupomId;
                    CouponItem.IsVisible = true;
                    CouponItem.CreateDate = DateTime.Now;
                    _db.CyCouponUsages.Add(CouponItem);
                    usersCouponOk.Add(item);


                };
            }
            await _db.SaveChangesAsync();
            return Ok(usersCouponOk);
        }


        [HttpGet("getALlCouponItem")]
        public async Task<ActionResult> getALlCouponItem()
        {
            var AllCouponItems = await _db.CyCouponUsages.Where(x => x.IsVisible).ToArrayAsync();
            List<CouponUsageDTO> AllCouponItemsdto = new List<CouponUsageDTO>();

            foreach (var item in AllCouponItems)
            {
                var couponusagedto = _mapper.Map<CouponUsageDTO>(item);
                AllCouponItemsdto.Add(couponusagedto);
            }

            return Ok(AllCouponItemsdto);
        }


        [HttpGet("getCouponByUserId")]

        public async Task<ActionResult> getCouponByUserId(int userId)
        {
            var coupons = await _db.CyCouponUsages.Where(x => x.UserId == userId && x.IsVisible).ToArrayAsync();
            List<CouponUsageDTO> couponList = new();
            if (coupons.Any() == false) return NotFound(new { response = "این کاربر کد تخفیف ندارد" });

            foreach (var item in coupons)
            {
                var coupon = _mapper.Map<CouponUsageDTO>(item);
                couponList.Add(coupon);
            }

            return Ok(couponList);
        }


        [HttpGet("getActiveCouponByUserId")]

        public async Task<ActionResult> getActiveCouponByUserId()
        {

            var userClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userClaim, out int userId)) return Unauthorized();

            /// first query on coupon table 
            //var couponAvailable = await _db.CyCoupon.Where(x => x.IsVisible && x.ExpireDate > DateTime.UtcNow && x.Coupons.Where(x => x.UserId == userId && x.IsVisible && x.UsedAt == null).Any())
            //    .ToArrayAsync();

            ///// second query on couponItem table  and this is better
            var couponAvailable = await _db.CyCouponUsages.Where(c => c.IsVisible && c.UserId == userId && !c.UsedAt.HasValue && c.Coupon.ExpireDate > DateTime.UtcNow && c.Coupon.IsVisible).Include(s => s.Coupon)
                //.Select(c => c.Coupon)   ///get only coupon
                .ToListAsync();
            if (couponAvailable.Any() == false) return NotFound(new { msg = "این کاربر کدتخفیف فعال ندارد " });

            return Ok(new { couponAvailable = couponAvailable, DiscountAmount = couponAvailable[0].Coupon.DiscountAmount, code = couponAvailable[0].Coupon.Code });
        }


        [HttpGet("getMobilSByCouponID")]
        async public Task<ActionResult> getMobilSByCouponID(int CouponId)
        {
            var couponItemS = await _db.CyCouponUsages.Where(x => x.IsVisible && x.CouponId == CouponId).Select(s => s.UserId).ToArrayAsync();
            var userMobileS = await _db.CyUser.Where(x => x.IsVisible && couponItemS.Contains(x.ID)).Select(s => s.CyUsNm).ToArrayAsync();

            return Ok(userMobileS);
        }


        [HttpPost("setToUSed")]

        public async Task<ActionResult> setToUSed(int couponItemId)
        {
            var couponsItem = await _db.CyCouponUsages.Where(x => x.ID == couponItemId && x.IsVisible).FirstOrDefaultAsync();


            if (couponsItem == null) return NotFound(new { msg = "anyCoupon not found" });

            couponsItem.UsedAt = DateTime.Now;
            _db.SaveChanges();

            return Ok(new { msg = "coupon is in useing" });

        }

        [HttpPost("requestCoupon")]
        async public Task<ActionResult> RequestCoupon(int CoupItemId, int state)
        {

            var couponItem = await _db.CyCouponUsages.Where(x => x.IsVisible && x.ID == CoupItemId && !x.UsedAt.HasValue).FirstOrDefaultAsync();
            if (couponItem == null) return NotFound(new { msg = "couponItem NotFound" });

            if (state == 1)
            {
                couponItem.IsRequested = true;

            }
            else if (state == 2)
            {
                couponItem.IsRequested = false;

            }

            await _db.SaveChangesAsync();

            return Ok(new { msg = "couponItem isRequested" });
        }

        [HttpGet("requsetCuoponSetToFalse")]
        async public Task<ActionResult> requsetCuoponSetToFalse()
        {
            var userclaims = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userclaims, out int userId)) return Unauthorized();

            var coupon = await _db.CyCouponUsages.
                Where(x => x.IsVisible && x.UserId == userId && x.Coupon.ExpireDate > DateTime.UtcNow && !x.UsedAt.HasValue && x.IsRequested == true).FirstOrDefaultAsync();
            if (coupon != null)
            {
                coupon.IsRequested = false;
                await _db.SaveChangesAsync();

            }
            return Ok();
        }

        [HttpGet("useCouponByCustomer")]
        async public Task<ActionResult> useCouponByCustomer(string CouponCode)
        {
            if (CouponCode == null) return BadRequest(new { msg = "کد تخفیف وارد نشده است" });
            CouponCode.Trim();
            var isTrueCoupon = _db.CyCoupon.Where(x => x.IsVisible && x.Code == CouponCode).FirstOrDefault();
            if (isTrueCoupon == null) return BadRequest(new { msg = "کد تخفیف اشتباه است " });

            var userClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userClaim, out int userId)) return Unauthorized(new { msg = "لطفا ابتدا با شماره همراه خود وارد شوید" });

            var couponUsage = _db.CyCouponUsages.
                Where(x => x.IsVisible && x.UserId == userId && x.Coupon.Code == CouponCode).Include(s => s.Coupon).FirstOrDefault();

            if (couponUsage == null) return BadRequest(new { msg = "این کد تخفیف برای این شماره همراه معتبر نمی باشد " });


            if (couponUsage.UsedAt.HasValue) return BadRequest(new { msg = "این کد تخفیف قبلا استفاده شده است" });


            if (couponUsage.Coupon.ExpireDate <= DateTime.UtcNow) return BadRequest(new { msg = "این کد تخفیف منقضی شده است" });


            return Ok(couponUsage.Coupon);
        }



    }
}
