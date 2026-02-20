using AutoMapper;
using CY_BM;
using CY_DM;
using CY_WebApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly CyContext _db;
        public VoucherController(IMapper mapper, CyContext db)
        {
            _mapper = mapper;
            _db = db;
        }



        [HttpGet]
        public async Task<IActionResult> GetAll(DateTime fromDate,DateTime toDate)
        {
            var vouchers = await _db.Voucher.Where(x=>x.IsVisible && x.VoucherDate >= fromDate && x.VoucherDate <= toDate )
                .Include(v => v.Items).ThenInclude(i=>i.Account)
                .OrderByDescending(v => v.VoucherDate)
                .ToListAsync();

            var dto = _mapper.Map<List<VoucherDTOB>>(vouchers);

            foreach (var item in dto)
            {
                foreach (var item1 in item.Items)
                {
                    item1.AccountName = item1.Account.Title;
                }
            }

            return Ok(dto);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var voucher = await _db.Voucher.Where(x=>x.IsVisible)
                .Include(v => v.Items)
                .FirstOrDefaultAsync(v => v.ID == id);

            if (voucher == null)
                return NotFound();

            return Ok(voucher);
        }

        [HttpPost]
        public async Task<IActionResult> Create(VoucherDTO dto)
        {
            if (dto.Items.Sum(i => i.Debit) != dto.Items.Sum(i => i.Credit))
                return BadRequest("سند تراز نیست");

            using var tx = await _db.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {

                var voucher = new Voucher
                {
                    VoucherDate = dto.VoucherDate,
                    Description = dto.Description,
                    ReferenceType = dto.ReferenceType,
                    ReferenceId = dto.ReferenceId,
                    Items = dto.Items.Select(i => new VoucherItem
                    {
                        AccountId = i.AccountId,
                        ToAccountId = i.ToAccountId,
                        Debit = i.Debit,
                        Credit = i.Credit
                    }).ToList()
                };

                foreach (var item in voucher.Items)
                {
                    var account = await _db.Account
                        .FirstOrDefaultAsync(x => x.IsVisible && x.ID == item.AccountId);

                    if (account == null)
                        return BadRequest("account not found");

                    account.MandehHesab += item.Debit - item.Credit;
                    item.MandehHesab = account.MandehHesab;
                }

                await _db.Voucher.AddAsync(voucher);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(voucher);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> Create(VoucherDTO dto)
        //{
        //    if (dto.Items.Sum(i => i.Debit) != dto.Items.Sum(i => i.Credit))
        //        return BadRequest("سند تراز نیست");



        //    var voucher = new Voucher
        //    {
        //        VoucherDate = dto.VoucherDate,
        //        Description = dto.Description,
        //        ReferenceType = dto.ReferenceType,
        //        ReferenceId = dto.ReferenceId,
        //        Items = dto.Items.Select(i => new VoucherItem
        //        {
        //            AccountId = i.AccountId,
        //            ToAccountId = i.ToAccountId,
        //            Debit = i.Debit,
        //            Credit = i.Credit,

        //        }).ToList()
        //    };

        //    foreach (var item in voucher.Items)
        //    {
        //        var currenAccount = await _db.Account.Where(x => x.IsVisible && x.ID == item.AccountId).FirstOrDefaultAsync();
        //        if (currenAccount == null) return BadRequest("account not found");
        //        currenAccount.MandehHesab = currenAccount.MandehHesab + item.Debit - item.Credit;
        //        item.MandehHesab = currenAccount.MandehHesab;

        //    }

        //    await _db.Voucher.AddAsync(voucher);
        //    await _db.SaveChangesAsync();

        //    return Ok(voucher);
        //}


        [HttpPut("editeVoucher")]
        public async Task<ActionResult> editeVoucher([FromBody] VoucherDTOB dto)
        {
            var currentVoucher = await _db.Voucher.Where(x => x.IsVisible && x.ID == dto.ID).Include(i => i.Items).ThenInclude(i => i.Account
            ).FirstOrDefaultAsync();
            if (currentVoucher == null) return BadRequest(new { msg = "سند یافت نشد" });


            var oldItems = currentVoucher.Items;

            foreach (var item in oldItems)
            {

                item.MandehHesab = item.MandehHesab - item.Debit + item.Credit;
                item.Account.MandehHesab = item.Account.MandehHesab - item.Debit + item.Credit;
            }

            currentVoucher.VoucherDate = dto.VoucherDate;
            currentVoucher.ReferenceType = dto.ReferenceType;
            currentVoucher.Description = dto.Description;


            foreach (var item in dto.Items)
            {
                var balance = await _db.VoucherItem
               .Where(v => v.AccountId == item.AccountId ).Include(i=>i.Voucher).Where(x=>x.Voucher.VoucherDate < dto.VoucherDate)
               .SumAsync(v => v.Debit - v.Credit);
                var voucherItem = await _db.VoucherItem.Where(x => x.IsVisible && x.ID == item.ID).Include(i => i.Account).FirstOrDefaultAsync();
         
                if (voucherItem == null || voucherItem.Account==null) return BadRequest("voucherItem not Found");
                voucherItem.AccountId = item.AccountId;
                voucherItem.ToAccountId = item.ToAccountId;
                voucherItem.Credit=item.Credit;
                voucherItem.Debit=item.Debit;
                voucherItem.IsEdited = true;
                voucherItem.MandehHesab = balance + item.Debit - item.Credit;
                voucherItem.Account.MandehHesab = voucherItem.Account.MandehHesab + item.Debit - item.Credit;
        
            }

            await _db.SaveChangesAsync();


            return Ok(new {msg="ویرایش انجام شد"});
        }


        [HttpDelete("deleteVoucher")]
        public async Task<ActionResult> deleteVoucher(int id)
        {
            var currentVoucher = await _db.Voucher.Where(x => x.IsVisible && x.ID == id).Include(i=>i.Items).FirstOrDefaultAsync();
            if (currentVoucher == null) return BadRequest(new { msg = "سند یافت نشد" });


            var oldItems = currentVoucher.Items;
            if (oldItems.Count > 2) return BadRequest("امکان حذف این سند وجود ندارد");

            foreach (var item in oldItems)
            {
                var currentVoucherItem=await _db.VoucherItem.Where(x=>x.IsVisible && x.ID== item.ID).Include(i=>i.Account).FirstOrDefaultAsync();
                currentVoucherItem.Account.MandehHesab = currentVoucherItem.Account.MandehHesab - item.Debit + item.Credit;
                item.IsEdited = true;
                item.IsVisible = false;
            }

            currentVoucher.IsVisible=false;
            await _db.SaveChangesAsync();


            return Ok(new { msg = "حذف انجام شد" });
        }


        /// <summary>
        /// گزارش عملکرد مالی
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        [HttpGet("financial-performance")]
        public async Task<IActionResult> GetFinancialPerformance(
    DateTime from,
    DateTime to)
        {
            var query = _db.VoucherItem
                .Where(x => x.IsVisible &&  x.Voucher != null &&
                            x.Voucher.VoucherDate >= from &&
                            x.Voucher.VoucherDate <= to);

            // فروش ناخالص (فقط فروش کالا - Id = 15)
            var DarAmad = await query
                .Where(x => x.AccountId == 15)
                .SumAsync(x => x.Credit - x.Debit);

       
            
            // فروش خالص (کل درآمدها AccountType = 4)
            var netSales = await query
                .Where(x => x.Account != null &&
                            x.Account.AccountType == AccountType.Revenue)
                .SumAsync(x => x.Credit - x.Debit);

            // بهای تمام شده (Id = 14)
            var BahayKala = await query
                .Where(x => x.AccountId == 14)
                .SumAsync(x => x.Debit - x.Credit);

            // سایر هزینه ها (AccountType = 5 به جز 14)
            var otherExpenses = await query
                .Where(x => x.Account != null &&
                            x.Account.AccountType == AccountType.Expense &&
                            x.AccountId != 14)
                .SumAsync(x => x.Debit - x.Credit);

            var result = new FinancialPerformanceDto
            {
                DarAmad = DarAmad,
                NetSales = netSales,
                BahayKala = BahayKala,
                OtherExpenses = otherExpenses,
                NetProfit = netSales - BahayKala - otherExpenses
            };

            return Ok(result);
        }

        public class FinancialPerformanceDto
        {
            public double DarAmad { get; set; }
            public double NetSales { get; set; }
            public double BahayKala { get; set; }
            public double OtherExpenses { get; set; }
            public double NetProfit { get; set; }
        }


    }
}