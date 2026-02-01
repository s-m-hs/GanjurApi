using System.Security.Claims;
using AutoMapper;
using CY_BM;
using CY_DM;
using CY_WebApi.Models;
using CY_WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CyLoginController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly CyContext _db;
        private readonly IMapper _mapper;

        public CyLoginController(JwtService jwtService, CyContext db, IMapper mapper)
        {
            _jwtService = jwtService;
            _db = db;
            _mapper = mapper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            // Replace with your actual user validation logic (don't store plain text passwords)
            CyUser? user =await _jwtService.IsValidUser(login,_db);
            if (user == null || user.userType == UserType.Customer)
            {
                return Unauthorized();
            }

            //var user = GetUserFromCredentials(login); // Replace with logic to get user object

            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(user);
            _jwtService.SetRefreshTokenInCookie(refreshToken, HttpContext, "Admin");
            _jwtService.SetTokenInCookie(token, HttpContext, "Admin");
            return Ok(new { token = token, type = user.userType ,userId=user.ID});


        }

        [HttpPost("Hash")]
        public async Task<IActionResult> HashTest([FromBody] LoginModel login)
        {
            // Replace with your actual user validation logic (don't store plain text passwords)
            return Ok(new { hash = Crypto.GetStringSha512Hash(login.Pw) });
        }

//Seyyedi Added
        [HttpGet("Numerator")]
        public async Task<ActionResult> getNumerator()
        {


            List<Numerator> numArray = new List<Numerator>();

            var  orderNum=await _db.CyOrder.Where(x => x.IsVisible && x.Status != OrderStatus.InBasket).ToArrayAsync();

            numArray.Add(new Numerator("Order",orderNum.Count()));


            var productNum = await _db.CyProduct.Where(x => x.IsVisible).CountAsync();
            numArray.Add(new Numerator("Product", productNum));

            var userNum = await _db.CyUser.Where(x => x.IsVisible && x.userType == UserType.Customer).CountAsync();
            numArray.Add(new Numerator("User",  userNum));


            var ticketNum = await _db.CyTicket.Where(x => x.IsVisible).CountAsync();
            numArray.Add(new Numerator("Ticket", ticketNum));

            var subjectNum = await _db.CySubject.Where(x => x.IsVisible).CountAsync();
            numArray.Add(new Numerator("Subject", subjectNum));




            return Ok(numArray);
        }

        [HttpGet("refreshToken")]
        public IActionResult refreshToken()
        {
            if (!Request.Cookies.TryGetValue("SaneAdminrefreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized("Refresh token not found");
            }

            var principal = _jwtService.GetPrincipalFromExpiredToken(refreshToken);
            if (principal == null)
            {
                return Unauthorized("Invalid refresh token");
            }
            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID claim not found");
            }
            int userid = int.Parse(userIdClaim.Value);
            var user = _db.CyUser.Where(u => u.IsVisible && u.ID == userid).FirstOrDefault();
            if (user == null) { return BadRequest(); }

            // Generate new tokens
            var newAccessToken = _jwtService.GenerateToken(user);

            //remove old token:
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                IsEssential = true,
                SameSite = SameSiteMode.None,
                Domain = DomainConfig.Domain,

                //Domain = "sapi.sanecomputer.com"
            };
            Response.Cookies.Delete("SaneAdminAccessToken", options);

            // Set new token in cookies
            _jwtService.SetTokenInCookie(newAccessToken, HttpContext,"admin");

            return Ok(new { token = newAccessToken,userId=userid });
        }



        [HttpGet("logoutAdmin")]
        public async Task<ActionResult> Logout()
        {
            var refreshToken = Request.Cookies["SaneAdminrefreshToken"];
            if (refreshToken == null) return Ok();

            // حذف کوکی
            var optins = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMinutes(5),
                HttpOnly = true,
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Domain = DomainConfig.Domain
            };

            Response.Cookies.Delete("SaneAdminAccessToken", optins);
            Response.Cookies.Delete("SaneAdminrefreshToken", optins);

            return Ok(new { msg = "Logged out successfully" });
        }

    }




}
