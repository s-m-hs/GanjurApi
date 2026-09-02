using AutoMapper;
using AutoMapper.Configuration.Annotations;
using ClosedXML.Excel;
using CY_BM;
using CY_DM;
using CY_WebApi.Models;
using CY_WebApi.Services;
using Google.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.OpenApi.Models;
namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[TypeAuthorize([UserType.SysAdmin,UserType.Manager,UserType.Employee])]
    public class AccountController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly CyContext _db;
        public AccountController(IMapper mapper, CyContext db)
        {
            _mapper = mapper;
            _db = db;
        }



        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var accounts = await _db.Account
                .Where(a => a.IsActive && a.IsVisible)
                //.OrderBy(a => a.Code)
                .ToListAsync();

            return Ok(accounts);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var account = await _db.Account.FindAsync(id);
            if (account == null)
                return NotFound();

            return Ok(account);
        }
        [HttpPost]
        public async Task<IActionResult> Create(AccountDTO dto)
        {
            var account = new Account
            {
                Code = dto.Code,
                Title = dto.Title,
                AccountType = dto.AccountType,
                ParentId = dto.ParentId,
                IsActive = true,
                MandehHesab = 0
            };

            _db.Account.Add(account);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = account.ID }, account);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, AccountDTO dto)
        {
            var account = await _db.Account.Where(x => x.IsVisible && x.ID == id).Include(u => u.Voucher).FirstOrDefaultAsync();
            if (account == null)
                return NotFound();

            // ❌ تغییر نوع حساب بعد از استفاده ممنوع
            bool hasVoucher = await _db.VoucherItem.AnyAsync(v => v.AccountId == id);
            if (hasVoucher && account.AccountType != dto.AccountType)
                return BadRequest("امکان تغییر نوع حساب بعد از ثبت سند وجود ندارد");

            account.Code = dto.Code;
            account.Title = dto.Title;
            account.ParentId = dto.ParentId;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var account = await _db.Account.FindAsync(id);
            if (account == null)
                return NotFound();

            bool hasVoucher = await _db.VoucherItem.AnyAsync(v => v.AccountId == id);
            if (hasVoucher)
                return BadRequest("حساب دارای سند است و قابل حذف نیست");

            account.IsActive = false;
            await _db.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("accountBalance")]
        public async Task<ActionResult> accountBalance(int accountId)
        {
            // 1. پیدا کردن شخص
            var account = await _db.Account.Where(x => x.ID == accountId).Select(s => new
            {
                id = s.ID,
                code = s.Code,
                title = s.Title,
                accountType = s.AccountType,
                parentId = s.ParentId,
                Mandeh = s.MandehHesab

            })
                .FirstOrDefaultAsync();

            if (account == null)
                return NotFound("شخص مورد نظر یافت نشد");

            // 2. محاسبه مانده حساب
            var balance = await _db.VoucherItem
                .Where(v => v.IsVisible && v.AccountId == accountId)
                .SumAsync(v => v.Debit - v.Credit);

            // 3. خروجی
            var result = new BalanceDTO
            {
                Balance = balance
            };

            return Ok(new { account = account, result = result });
        }


        [HttpGet("accountBalanceByUserId")]
        public async Task<ActionResult> accountBalanceByUserId(int userId)
        {

            var currentUser = _db.CyUser.Where(x => x.IsVisible && x.ID == userId).Select(s => new
            {
                id = s.ID,
                accountId = s.AccountId,
                PartnerStatus = s.PartnerStatus,

            }).FirstOrDefault();
            if (currentUser == null) return BadRequest("usernotfound");

            var accountId = currentUser.accountId;

            // 1. پیدا کردن شخص
            var account = await _db.Account.Where(x => x.ID == accountId).Select(s => new
            {
                id = s.ID,
                code = s.Code,
                title = s.Title,
                accountType = s.AccountType,
                parentId = s.ParentId,
                Mandeh = s.MandehHesab
            })
                .FirstOrDefaultAsync();

            if (account == null)
                return NotFound("شخص مورد نظر یافت نشد");

            // 2. محاسبه مانده حساب
            var balance = await _db.VoucherItem
                .Where(v => v.IsVisible && v.AccountId == accountId)
                .SumAsync(v => v.Debit - v.Credit);

            // 3. خروجی
            var result = new BalanceDTO
            {
                Balance = balance
            };

            return Ok(new { currentUser = currentUser, result = result });
        }






        [HttpGet("getMoeinHesab")]
        public async Task<ActionResult> getMoeinHesab(int accountId)
        {

            var voucheItems = await _db.VoucherItem.Where(x => x.IsVisible && (x.AccountId == accountId || x.ToAccountId == accountId)).ToListAsync();

            return Ok(voucheItems);
        }



        [HttpGet("getParenetAccount")]
        async public Task<ActionResult> getParenetAccount()
        {
            var accounts = await _db.Account.AsNoTracking().Where(x => x.IsVisible && x.ParentId == null).Select(s => new
            {
                ID = s.ID,
                Code = s.Code,
                Title = s.Title,
                AccountType = s.AccountType
            }).ToListAsync();

            //var dto=_mapper.Map<List<AccountDTO>>(accounts);

            return Ok(accounts);


        }


        [HttpGet("getAccountByCode")]
        async public Task<ActionResult> getAccountByCode(string code)
        {
            var accounts = await _db.Account.Where(x => x.IsVisible && x.Code.StartsWith(code)).Include(i => i.Voucher).ToListAsync();

            var dto = _mapper.Map<List<AccountDTO>>(accounts);

            return Ok(dto);
        }

        [HttpGet("getUserAccountByCode")]
        async public Task<ActionResult> getUserAccountByCode(string code, bool isDebit = true)
        {
            var accounts =await _db.Account.Where(x => x.IsVisible && x.Code.StartsWith(code)).Include(i => i.CyUser).OrderBy(o => o.Title).ToListAsync();

            var accountIdList = accounts.Select(x => x.ID).ToList();

            var allVoucherItems = await _db.VoucherItem.Where(x => x.IsVisible && accountIdList.Contains((int)x.AccountId)).ToListAsync();

            foreach (var item in accounts)
            {

                var balance = await _db.VoucherItem
                .Where(v => v.IsVisible && v.AccountId == item.ID)
                .SumAsync(v => v.Debit - v.Credit);

                item.MandehHesab=balance;
            }

            await _db.SaveChangesAsync();

            if (isDebit)
            {
                var debitUsers = accounts.Where(x => x.MandehHesab > 0)?.Select(s => new
                {
                    id = s.ID,
                    userName = s.Title,
                    mandeh = s.MandehHesab,
                    mobile = s.CyUser.Mobile,
                    phone = s.CyUser.Phone,
                    userId = s.CyUser.ID,

                }).ToList();

                return Ok(debitUsers);
            }



            var creditUsers = accounts.Where(x => x.MandehHesab < 0).Select(s => new
            {
                id = s.ID,
                userName = s.Title,
                mandeh = s.MandehHesab,
                mobile = s.CyUser.Mobile,
                phone = s.CyUser.Phone,
                userId = s.CyUser.ID,

            }).ToList();


            return Ok(creditUsers);
        }

        [HttpGet("getAccountById")]
        async public Task<ActionResult> getAccountById(int id, DateTime? fromDate, DateTime? toDate)
        {
            //var currentAccount=_db.Account.Where(x=>x.IsVisible && x.)


            var voucheeItemS = await _db.VoucherItem.Where(x => x.IsVisible && x.Voucher.VoucherDate >= fromDate && x.Voucher.VoucherDate <= toDate && (x.AccountId == id || x.ToAccountId == id)).
                Include(i => i.Account).Include(i => i.Voucher).Select(s => new VoucherItemDTO
                {
                    AccountId = s.AccountId,
                    ToAccountId = s.ToAccountId,
                    Debit = s.Debit,
                    Credit = s.Credit,
                    RefrenceType = s.Voucher.ReferenceType,
                    RefrencId = s.Voucher.ReferenceId,
                    Description = s.Voucher.Description,
                    AccountName = s.Account.Title,
                    CreatDate = s.CreateDate,
                    VoucherDate = s.Voucher.VoucherDate
                }).OrderByDescending(o => o.CreatDate).ToArrayAsync();


            return Ok(voucheeItemS);
        }


        [HttpGet("mandeHesab")]
        async public Task<ActionResult> mandeHesab()
        {
            var AllAccounts = await _db.Account.Where(x => x.IsVisible).ToListAsync();
            foreach (var item in AllAccounts)
            {
                var balance = await _db.VoucherItem
                 .Where(v => v.IsVisible && v.AccountId == item.ID)
                 .SumAsync(v => v.Debit - v.Credit);

                // 3. خروجی
                var result = new BalanceDTO
                {
                    Balance = balance
                };

                item.MandehHesab = balance;
            }

            await _db.SaveChangesAsync();

            return Ok(AllAccounts);



        }

        [HttpGet("mandeHesabB")]
        async public Task<ActionResult> mandeHesabB()
        {
            var AllAccounts = await _db.Account.Where(x => x.IsVisible).ToListAsync();

            foreach (var item in AllAccounts)
            {
                double mandeh = 0;
                var allVoucherItem = await _db.VoucherItem.Where(x => x.IsVisible && x.AccountId == item.ID).OrderBy(o => o.ID).ToArrayAsync();
                foreach (var item1 in allVoucherItem)
                {
                    mandeh = mandeh + item1.Debit - item1.Credit;
                    item1.MandehHesab = mandeh;
                }

            }
            await _db.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// محاسبه مانده حساب برای ایدی هایی که مانده صفر دارند
        /// </summary>
        /// <returns></returns>
        [HttpGet("mandeHesabC")]
        async public Task<ActionResult> mandeHesabC()
        {
            var AllAccounts = await _db.Account.Where(x => x.IsVisible && x.MandehHesab == 0).ToListAsync();

            foreach (var item in AllAccounts)
            {

                var balance = await _db.VoucherItem
    .Where(v => v.IsVisible && v.AccountId == item.ID)
    .SumAsync(v => v.Debit - v.Credit);

                item.MandehHesab = balance;
            }
            await _db.SaveChangesAsync();

            return Ok();
        }



        [HttpGet("mizan")]
        async public Task<ActionResult> mizan()
        {
            var factors = await _db.CyOrder.Where(x => x.IsVisible && x.OrderMode == Ordermode.ShopFromCustomer).Select(s => s.FactorNumber).ToListAsync();
            var vouchers = await _db.Voucher.Where(x => factors.Contains((int)x.ReferenceId)).Include(i => i.Items).Select(s => s.Items).ToListAsync();

            int count = 0;

            foreach (var item in vouchers)
            {
                foreach (var item2 in item)
                {
                    if (item2.AccountId == 24 && item2.Debit == 0 && item2.Credit != 0)
                    {
                        item2.Debit = item2.Credit;
                        item2.Credit = 0;
                        count = count + 1;
                        item2.IsEdited = true;
                    }
                }
            }

            await _db.SaveChangesAsync();


            return Ok(new { count = vouchers.Count, count2 = count, vouchers = vouchers });

        }


        //[HttpGet("getMoeinHesab")]
        //public async Task<ActionResult> getMoeinHesab(int accountId,int userId) {

        //    var vouchurS = await _db.VoucherItem.Where(x => x.AccountId == accountId).Include(s => s.Voucher).ThenInclude(s=>s.Items).Select(s => s.Voucher).ToListAsync();


        //    //var vouchurS = await _db.VoucherItem.Where(x => x.AccountId == accountId).Include(s => s.Voucher).Select(s => new
        //    //{
        //    //    id = s.ID,
        //    //    vItemId=s.AccountId,
        //    //    creatDate = s.CreateDate,
        //    //    OrderId = s.Voucher.ReferenceId !=null ? s.Voucher.ReferenceId : null,
        //    //    ReferenceType=s.Voucher.ReferenceType !=null ? s.Voucher.ReferenceType : null,
        //    //    Debit=s.Debit,
        //    //    credit=s.Credit

        //    //}).ToListAsync();
        //    var Orders =await  _db.CyOrder.Where(x => x.IsVisible && x.UserID == userId).Include(s=>s.OrderItems).ToListAsync();
        //    List<object> orderItemList = new();
        //    foreach (var item in vouchurS)
        //    {
        //        if(item.ReferenceId != null)
        //        {
        //            var orderId= item.ReferenceId;
        //        var orderItems= Orders.Where(x => x.ID == orderId).Select(s=>s.OrderItems).ToList();

        //            foreach (var item1 in orderItems)
        //            {

        //                orderItemList.AddRange(item1);
        //            }

        //        }

        //    }

        //    orderItemList.AddRange(vouchurS);


        //    return Ok(orderItemList); 
        //}



        /// <summary>
        /// اکسل از مشتریان بدهکار و بستانکار
        /// </summary>
        /// <param name="code"></param>
        /// <param name="isDebit"></param>
        /// <returns></returns>
        [HttpGet("getExellFromUserBalance")]
        async public Task<ActionResult> getExellFromUserBalance(string code)
        {
            var accounts = await _db.Account.Where(x => x.IsVisible && x.Code.StartsWith(code)).Include(i => i.CyUser).OrderBy(o => o.Title).ToListAsync();

            var accountIdList = accounts.Select(x => x.ID).ToList();

            var allVoucherItems = await _db.VoucherItem.Where(x => x.IsVisible && accountIdList.Contains((int)x.AccountId)).ToListAsync();

            foreach (var item in accounts)
            {

                var balance = await _db.VoucherItem
                .Where(v => v.IsVisible && v.AccountId == item.ID)
                .SumAsync(v => v.Debit - v.Credit);

                item.MandehHesab = balance;
            }

            var Users = accounts.Where(x => x.MandehHesab != 0).Select(s => new
            {
                id = s.ID,
                userName = s.Title,
                mandeh = s.MandehHesab,
                mobile = s.CyUser.Mobile,
                phone = s.CyUser.Phone,
                userId = s.CyUser.ID,

            }).OrderBy(o => o.userName).OrderByDescending(o=>o.mandeh).ToList();



            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Users");

            // Header
            worksheet.Cell(1, 1).Value = "نام طرف حساب";
            worksheet.Cell(1, 2).Value = "همراه";
            worksheet.Cell(1, 3).Value = "تلفن ";
            worksheet.Cell(1, 4).Value = " مانده حساب";
            worksheet.Cell(1, 5).Value = "ID ";
   

            // Data
            int row = 2;
            foreach (var item in Users)
            {
                worksheet.Cell(row, 1).Value = item.userName;
                worksheet.Cell(row, 2).Value = item.mobile;
                worksheet.Cell(row, 3).Value = item.phone;
                if (decimal.TryParse(item.mandeh.ToString(), out decimal mandehValue))
                {
                    worksheet.Cell(row, 4).Value = mandehValue.ToString("N0");
                }
                else
                {
                    worksheet.Cell(row, 4).Value = item.mandeh; // fallback به مقدار اصلی
                }
                worksheet.Cell(row, 5).Value = item.id;
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
                "Users.xlsx"
            );
        }





    }
}
