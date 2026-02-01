using AutoMapper;
using CY_BM;
using CY_DM;
using CY_WebApi.Models;
using CY_WebApi.Services;
using Google.Cloud.RecaptchaEnterprise.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using static Google.Cloud.RecaptchaEnterprise.V1.TransactionData.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly CyContext _db;
        public CustomerController(IMapper mapper, CyContext db)
        {
            _mapper = mapper;
            _db = db;
        }


        [HttpPost("addUserB")]
        public async Task<ActionResult> addUserB([FromBody] UserDTO dto)
        {

            var CurentUser = await _db.CyUser.Where(x => x.IsVisible && x.CyUsNm == dto.CyUsNm.Trim()).FirstOrDefaultAsync();

            if (CurentUser != null) return BadRequest("this userName is in repeat");

            var newUser = _mapper.Map<CyUser>(dto);

            // آخرین کد اشخاص
            var lastCode = _db.Account
                .Where(a => a.Code.StartsWith("80000"))
                .OrderByDescending(a => a.Code)
                .Select(a => a.Code)
                .FirstOrDefault();

            int next = int.Parse(lastCode) + 1;

            //int next = lastCode == null
            //    ? 8000101
            //    : int.Parse(lastCode) + 1;



            Account newAccount = new Account()
            {
                ID = 0,
                AccountType = AccountType.Person,
                Code = next.ToString(),
                Title = dto.CyUsNm,
                ParentId = 1,
                IsActive = true,
            };

            await _db.Account.AddAsync(newAccount);
            await _db.SaveChangesAsync();

            newUser.AccountId = newAccount.ID;

            await _db.CyUser.AddAsync(newUser);
            await _db.SaveChangesAsync();

            return Ok(dto);
        }
        [HttpGet("update")]
        public async Task<ActionResult> update()
        {
            var account = await _db.Account
            .Where(a => a.ID == 10)
            .FirstAsync();
            account.Code = "80012";
            await _db.SaveChangesAsync();
            return Ok(account);
        }







    }
}