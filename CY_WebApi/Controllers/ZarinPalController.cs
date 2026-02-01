using CY_WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using CY_BM;
using CY_WebApi.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Security.Claims;
using CY_DM;
using Google.Cloud.RecaptchaEnterprise.V1;
using DateTime = System.DateTime;
namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ZarinPalController : ControllerBase
    {
        private readonly CyContext _db;
        private readonly HttpClient _httpClient;
        private readonly SmsService _smsService;

        private readonly string merchantId = "df72fda6-1b01-4214-bf60-cdeeb458fd99";

        //public ZarinPalController(CyContext db, SmsService smsService)
        //{
        //    _db = db;
        //    _httpClient = new HttpClient();
        //    _smsService = smsService;
        //}

        //[TypeAuthorize([UserType.Customer])]
        //[HttpGet("pay")]
        //public async Task<ActionResult> Pay(int orderId, int addressId)
        //{
        //    var identity = User.Identities.FirstOrDefault();
        //    if (identity == null)
        //        return Unauthorized("کاربر پیدا نشد");
        //    var userid = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);

        //    var order = _db.CyOrder.Where(o => o.IsVisible && o.ID == orderId && o.CyUserID == userid).Include(o => o.CyOrderItems).FirstOrDefault();
        //    var orderItems = await _db.CyOrderItem.Where(i => i.IsVisible && i.CyOrderID == orderId).Include(i => i.CyProduct).ToListAsync();
        //    if (order == null) { return NotFound(); }
        //    var offer = await _db.CyKeyData.Where(kv => kv.IsVisible && kv.Key == "offer").Select(kv => kv.Value).FirstOrDefaultAsync();
        //    var postPrice=await _db.CyKeyData.Where(x=>x.IsVisible && x.Key== "postPrice").Select(u=>u.Value).FirstOrDefaultAsync();
        //    var postState =order.PostState;


        //    foreach (var item in orderItems)
        //    {
        //        var productSupply = await _db.CyProduct.Where(p => p.IsVisible && p.ID == item.CyProduct.ID).Select(p => p.Supply).FirstOrDefaultAsync();
        //        if (productSupply < item.Quantity)
        //        {
        //            return BadRequest(new { response = $"محصول {item.PartNumber} موجودی ندارد" });
        //        }
        //    }
        //    double? amount = orderItems.Where(i => i.IsVisible && i.CyProduct.NoOffPrice == i.CyProduct.Price).Sum(i => i.TotalPrice);


        //    ///to check  couponItem to be or no 


        //    var availableCoupons = await _db.CyCouponUsages.Where(x => x.IsVisible && x.UserId == userid && x.IsRequested == true
        //    && !x.UsedAt.HasValue && x.Coupon.ExpireDate > DateTime.UtcNow).Include(s => s.Coupon)
        //        .FirstOrDefaultAsync();


        //    ///// set off-price  to coupon  or without Coupon
        //    //if (couponsItem.Any()!=false &&  availableCoupons != null)
        //    if (availableCoupons != null)
        //    {
        //        var discountAmount = availableCoupons.Coupon.DiscountAmount;

        //        var couponOffer = discountAmount;
        //        amount = amount * couponOffer;

        //    }
        //    else
        //    {
        //        if (offer != null && double.TryParse(offer, out double intOffer))
        //        {
        //            amount = amount * intOffer;
        //        }
        //    }


        //    if (postState == PostState.post)
        //    {
        //       double.TryParse(postPrice, out double intPost);
        //       amount = amount + intPost;
        //    }

        //    amount = amount + orderItems.Where(i => i.IsVisible && i.CyProduct.NoOffPrice != i.CyProduct.Price).Sum(i => i.TotalPrice);

        //    order.TotalAmount = amount;

        //    var address = await _db.CyAddress.Where(ad => ad.CyUserID == userid && ad.ID == addressId && ad.IsVisible).FirstOrDefaultAsync();
        //    if (address == null)
        //    {
        //        return NotFound();
        //    }
        //    order.CyAddressID = addressId;
        //    order.OrderDate = DateTime.Now;
        //    await _db.SaveChangesAsync();



        //    string description = "خرید از صانع";

        //    var url = "https://payment.zarinpal.com/pg/v4/payment/request.json";

        //    var requestBody = new
        //    {
        //        merchant_id = merchantId,
        //        amount,
        //        callback_url = "https://saneComputer.com/paymentResult/" + order.ID,
        //        description,
        //    };

        //    var jsonContent = new StringContent(
        //        System.Text.Json.JsonSerializer.Serialize(requestBody),
        //        Encoding.UTF8,
        //        "application/json"
        //    );

        //    _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        //    try
        //    {
        //        var response = await _httpClient.PostAsync(url, jsonContent);
        //        var responseContent = await response.Content.ReadAsStringAsync();
        //        var jsonResponse = JsonConvert.DeserializeObject<ZarianPalResponseDTO>(responseContent);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            //order.Status = OrderStatus.Pending;
        //            //order.LastModified = DateTime.Now;
        //            //await _db.SaveChangesAsync();

        //            var authority = jsonResponse?.Data.Authority;

        //            return Ok(new { Url = "https://payment.zarinpal.com/pg/StartPay/" + authority });
        //        }
        //        else
        //        {
        //            return BadRequest(jsonResponse?.Data);
        //        }
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        return StatusCode(500, $"Error occurred while making the request: {ex.Message}");
        //    }
        //}


        //[TypeAuthorize([UserType.Customer])]
        //[HttpPost("varifyPay")]
        //public async Task<ActionResult> VarifyPay([FromBody] PaymentVerifyRequest request)
        //{
        //    var identity = User.Identities.FirstOrDefault();
        //    if (identity == null)
        //        return Unauthorized("کاربر پیدا نشد");

        //    var userid = Int32.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value);
        //    var phoneNumber = _db.CyUser.Where(u => u.IsVisible && u.ID == userid).Select(u => u.CyUsNm).FirstOrDefault();

        //    var order = _db.CyOrder.Where(o => o.IsVisible && o.ID == request.OrderId).FirstOrDefault();
        //    var orderItems = await _db.CyOrderItem
        //        .Where(i => i.IsVisible && i.CyOrderID == request.OrderId)
        //        .Include(i => i.CyProduct)
        //        .ToListAsync();

        //    if (order == null)
        //        return NotFound();

        //    var offer = _db.CyKeyData.Where(kv => kv.IsVisible && kv.Key == "offer").Select(kv => kv.Value).FirstOrDefault();
        //    var postPrice = await _db.CyKeyData.Where(x => x.IsVisible && x.Key == "postPrice").Select(u => u.Value).FirstOrDefaultAsync();
        //    var postState = order.PostState;

        //    double? amount = orderItems.Where(i => i.IsVisible && i.CyProduct.NoOffPrice == i.CyProduct.Price).Sum(i => i.TotalPrice);


        //    var availableCoupons = await _db.CyCouponUsages.Where(x => x.IsVisible && x.UserId == userid && x.IsRequested == true
        //    && !x.UsedAt.HasValue && x.Coupon.ExpireDate > DateTime.UtcNow).Include(s => s.Coupon)
        //        .FirstOrDefaultAsync();

        //    if (availableCoupons != null)
        //    {
        //        var discountAmount = availableCoupons.Coupon.DiscountAmount;
        //        var couponOffer = discountAmount;
        //        amount = amount * couponOffer;
        //        availableCoupons.UsedAt = DateTime.UtcNow;
                

        //    }
        //    else
        //    {
        //        if (offer != null && double.TryParse(offer, out double intOffer))
        //        {
        //            amount = amount * intOffer;
        //        }
        //    }

        //    if (postState == PostState.post)
        //    {
        //        double.TryParse(postPrice, out double intPost);
        //        amount = amount + intPost;
        //    }


        //    amount = amount + orderItems.Where(i => i.IsVisible && i.CyProduct.NoOffPrice != i.CyProduct.Price).Sum(i => i.TotalPrice);

        //    var url = "https://payment.zarinpal.com/pg/v4/payment/verify.json";

        //    var jsonPayload = new
        //    {
        //        merchant_id = merchantId,
        //        amount,
        //        authority = request.Authority
        //    };
        //    var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(jsonPayload), Encoding.UTF8, "application/json");

        //    var response = await _httpClient.PostAsync(url, content);
        //    var responseContent = await response.Content.ReadAsStringAsync();
        //    var jsonResponse = JsonConvert.DeserializeObject<ZarianPalResponseDTO>(responseContent);

        //    if (response.IsSuccessStatusCode && (jsonResponse?.Data?.code == 100 || jsonResponse?.Data?.code == 101))
        //    {
        //        // اگر قبلاً تایید نشده، کالاها کم شوند
        //        if (order.Status != OrderStatus.Finalized)
        //        {
        //            foreach (var item in orderItems)
        //            {
        //                item.CyProduct.Supply--;
        //            }

        //            order.Status = OrderStatus.Finalized;
        //            order.LastModified = DateTime.Now;
        //            order.FinalizedDate = DateTime.Now;
        //            await _db.SaveChangesAsync();

        //            await _smsService.SendSmsAsync(phoneNumber, "u39o5gmauw098oc", "", order.ID);
        //        }

        //        return Ok(jsonResponse?.Data);
        //    }

        //    return BadRequest(new
        //    {
        //        Error = "تأیید پرداخت با خطا مواجه شد",
        //        Response = responseContent
        //    });
        //}


      
    }
}
