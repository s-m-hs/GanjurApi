using AutoMapper;
using CY_BM;
using CY_DM;
using CY_WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var voucher = await _db.Voucher
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
                    Credit = i.Credit,

                }).ToList()
            };

            foreach (var item in voucher.Items)
            {
                var currenAccount = await _db.Account.Where(x => x.IsVisible && x.ID == item.AccountId).FirstOrDefaultAsync();
                if (currenAccount == null) return BadRequest("account not found");
                currenAccount.MandehHesab = currenAccount.MandehHesab + item.Debit - item.Credit;
                item.MandehHesab = currenAccount.MandehHesab;

            }

            _db.Voucher.Add(voucher);
            await _db.SaveChangesAsync();

            return Ok(voucher);
        }


        [HttpPut("editeVoucher")]
        public async Task<ActionResult> editeVoucher([FromBody] Voucher dto)
        {
            var currentVoucher = await _db.Voucher.Where(x => x.IsVisible && x.ID == dto.ID).Include(i => i.Items).ThenInclude(i => i.Account
            ).FirstOrDefaultAsync();
            if (currentVoucher == null) BadRequest(new { msg = "سند یافت نشد" });

            var oldItems = currentVoucher.Items;
            foreach (var item in oldItems)
            {
                item.MandehHesab = item.MandehHesab - item.Debit + item.Credit;
                item.Account.MandehHesab = item.Account.MandehHesab - item.Debit + item.Credit;
            }


            var currentItems = _mapper.Map<List<VoucherItem>>(dto.Items);

            currentVoucher.Items = currentItems;
            currentVoucher.CreateDate = dto.CreateDate;
            currentVoucher.ReferenceType = dto.ReferenceType;
            currentVoucher.Description = dto.Description;

            foreach (var item in currentItems)
            {
                item.MandehHesab = item.MandehHesab + item.Debit - item.Credit;
                item.Account.MandehHesab = item.Account.MandehHesab + item.Debit - item.Credit;
            }



            await _db.SaveChangesAsync();


            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var voucher = await _db.Voucher.FindAsync(id);
            if (voucher == null)
                return NotFound();

            return BadRequest("حذف سند حسابداری مجاز نیست. سند اصلاحی ثبت کنید.");
        }


    }
}