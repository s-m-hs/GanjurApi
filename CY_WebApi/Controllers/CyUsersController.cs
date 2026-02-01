
using AutoMapper;
using CY_BM;
using CY_DM;
using CY_WebApi.DataAccess;
using CY_WebApi.Models;
using CY_WebApi.Services;
using ExcelDataReader;
using Google.Rpc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;


namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CyUsersController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly CyContext _db;
        public CyUsersController(IMapper mapper, CyContext db)
        {
            _mapper = mapper;
            _db = db;
        }

        // GET: api/CyUsers
        [Route("GetUserByType/{type}")]
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUserByType(int type)
        {
            var resu = new List<CyUser>();
            if (type == 0)
            {
                resu = await _db.CyUser.Where(c => c.IsVisible).ToListAsync();
            }
            else if (type == 1)
            {
                var profiles = await _db.CyUser.
                      Where(c => c.IsVisible && c.userType != UserType.Employee && c.userType != UserType.SysAdmin && c.userType != UserType.Manager).
                      ToListAsync();

                return Ok(profiles);
            }
            else if (type == 2)
            {
                resu = await _db.CyUser.Where(c => c.IsVisible && (c.userType == UserType.Manager || c.userType == UserType.SysAdmin || c.userType == UserType.Employee)).ToListAsync();
            }
            return Ok(resu.Select(c => removPass(_mapper.Map<UserDTO>(c))).ToList());
        }

        private UserDTO removPass(UserDTO userDTO)
        {
            userDTO.CyHsPs = "***";
            return userDTO;
        }



        [HttpPost]
        public async Task<ActionResult<UserDTO>> PostCyUser(UserDTO UserDto)
        {

            var cyUser = _db.CyUser.Where(c => c.CyUsNm == UserDto.CyUsNm && c.IsVisible).FirstOrDefault();
            if (cyUser != null)
            {
                return BadRequest("این نام کاربری قبلا استفاده شده است");
            }

            else
            {
                cyUser = _mapper.Map<CyUser>(UserDto);

                // آخرین کد اشخاص
                var lastCode = _db.Account
                    .Where(a => a.Code.StartsWith("8001"))
                    .OrderByDescending(a => a.Code)
                    .Select(a => a.Code)
                    .FirstOrDefault();

                int next = lastCode == null
                    ? 8001
                    : int.Parse(lastCode) + 1;



                Account newAccount = new Account()
                {
                    ID = 0,
                    AccountType = AccountType.Person,
                    Code = next.ToString(),
                    Title = UserDto.CyUsNm,
                    ParentId = 8,
                    IsActive = true,
                };

                await _db.Account.AddAsync(newAccount);
                await _db.SaveChangesAsync();

                cyUser.CyHsPs = Crypto.EncryptStringAES(UserDto.CyHsPs);
                await _db.CyUser.AddAsync(cyUser);
                await _db.SaveChangesAsync();
                return Ok(UserDto);
                //return CreatedAtAction("GetCyUser", new { id = cyUser.ID }, removPass(_mapper.Map<UserDTO>(cyUser)));

            }
        }






        /// <summary>
        /// for customers
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("addUser")]
        public async Task<ActionResult> addUser([FromBody] UserDTO dto)
        {

            var CurentUser = _db.CyUser.Where(x => x.IsVisible && x.CyUsNm == dto.CyUsNm.Trim());

            if (CurentUser != null) return BadRequest("this userName is in repeat");

            var newUser = _mapper.Map<CyUser>(dto);

            // آخرین کد مشتری
            var lastCode = _db.Account
                .Where(a => a.Code.StartsWith("8100"))
                .OrderByDescending(a => a.Code)
                .Select(a => a.Code)
                .FirstOrDefault();

            int next = lastCode == null
                ? 1
                : int.Parse(lastCode) + 1;



            Account newAccount = new Account()
            {
                AccountType = AccountType.Person,
                Code = next.ToString(),
                Title = dto.CyUsNm,
                ParentId = 8,
                IsActive = true,
            };

            await _db.Account.AddAsync(newAccount);
            await _db.SaveChangesAsync();

            newUser.AccountId = newAccount.ID;

            await _db.CyUser.AddAsync(newUser);
            await _db.SaveChangesAsync();

            return Ok(dto);
        }


        [HttpGet("getUserS")]
        async public Task<ActionResult> getUserS()
        {
            var uesers = await _db.CyUser.Where(x => x.IsVisible).ToListAsync();

            return Ok(uesers);
        }


        [HttpGet("getUserVoucherItem")]
        public async Task<ActionResult> getUserVoucherItem(int AccountId)
        {



            // 2. محاسبه مانده حساب
            var balance = await _db.VoucherItem
                .Where(v => v.AccountId == AccountId)
                .SumAsync(v => v.Debit - v.Credit);

            // 3. خروجی
            var result = new BalanceDTO
            {
                Balance = balance
            };




            var AllAccounts = await _db.Account.Where(x => x.IsVisible && x.ID == 32).ToListAsync();



            var currentVouchurs =  _db.VoucherItem.Where(x => x.IsVisible && x.AccountId == AccountId).Include(u => u.Voucher).Include(i => i.Account).OrderBy(o => o.ID).AsQueryable();

            var isEditedVoItem = currentVouchurs.Where(x => x.IsEdited == true).FirstOrDefault();

            if(isEditedVoItem != null )
            {
             
                    double mandeh = 0;
                    foreach (var item1 in currentVouchurs)
                    {
                        mandeh = mandeh + item1.Debit - item1.Credit;
                        item1.MandehHesab = mandeh;
                        item1.IsEdited = false;
                    }

                

            }
            await _db.SaveChangesAsync();

        var resultB= await currentVouchurs?.Select(x => new
                        {
                            Id = x.ID,
                            Debit = x.Debit,
                            Credit = x.Credit,
                            VoucherDate = x.Voucher!.VoucherDate,
                            Description = x.Voucher.Description,
                            ReferenceType = x.Voucher.ReferenceType,
                            refrenceId = x.Voucher!.ReferenceId,
                            mandeh = x.Account.MandehHesab,
                            mandeh2 = x.MandehHesab,
                            creatDate=x.CreateDate

                        }).OrderByDescending(o=>o.creatDate)
                        .ToListAsync();

            return Ok(new { currentVouchurs = resultB, result = result });
        }


        [HttpPost("import-users-from-excel")]
        public async Task<IActionResult> ImportUsersFromExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("فایل اکسل ارسال نشده است");

            var users = new List<CyUser>();

            using (var stream = file.OpenReadStream())
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var dataSet = reader.AsDataSet();
                var table = dataSet.Tables[0]; // شیت اول

                // شروع از ردیف 1 چون ردیف 0 هدر است
                for (int i = 1; i < table.Rows.Count; i++)
                {
                    var row = table.Rows[i];

                    // اگر ستون اصلی خالی بود، رد شو
                    if (row[2] == DBNull.Value)
                        continue;

                    var user = new CyUser
                    {
                        UserCodeA = row[1]?.ToString(), // ستون 2
                        CyUsNm = row[2]?.ToString() ?? "", // ستون 3 (required)
                        Phone = row[3]?.ToString(), // ستون 4
                        Mobile = row[4]?.ToString(), // ستون 5
                        UserAddress = row[5]?.ToString(), // ستون 6

                        Status = UserStatus.Active,
                        userType = UserType.Customer,
                        AccountBalance = 0,
                        PartnerStatus = PartnerStatus.NoPartner,
                        UserBalanceStatus = UserBalanceStatus.Tasvieh
                    };

                    users.Add(user);
                }
            }

            if (users.Count == 0)
                return BadRequest("هیچ رکورد معتبری در فایل یافت نشد");

            await _db.CyUser.AddRangeAsync(users);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                Count = users.Count,
                Message = "اطلاعات کاربران با موفقیت ثبت شد"
            });
        }



        [HttpDelete("deletUser")]
        public async Task<ActionResult> deletUser(int id)
        {
            var currentUser = _db.CyUser.Where(x => x.IsVisible && x.ID == id).FirstOrDefault();

            if (currentUser == null) return BadRequest();

            currentUser.IsVisible = false;

            await _db.SaveChangesAsync();

            return Ok(id);
        }

    }
}
