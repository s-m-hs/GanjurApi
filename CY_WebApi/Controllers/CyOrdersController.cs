using AutoMapper;
using CY_BM;
using CY_DM;
using CY_WebApi.DataAccess;
using CY_WebApi.Migrations;
using CY_WebApi.Models;
using CY_WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using static Google.Cloud.RecaptchaEnterprise.V1.TransactionData.Types;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CyOrdersController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly CyContext _db;
        public CyOrdersController(IMapper mapper, CyContext db)
        {
            _mapper = mapper;
            _db = db;
        }

        /// <summary>
        /// فاکتورفروش
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("addOrder")]
        async public Task<ActionResult> addOrder([FromBody] OrderDTO dto, Ordermode ordermode)
        {
            if (dto.OrderItems == null || !dto.OrderItems.Any()) return BadRequest(new { msg = "محصولی اضافه نشده است " });

            var user = _db.CyUser.Where(x => x.IsVisible && x.ID == dto.CyUserID).Include(u => u.Account).FirstOrDefault();

            var order = _mapper.Map<CyOrder>(dto);

            order.CreateDate = dto.CreatDate;

            var orderItems = _mapper.Map<List<CyOrderItem>>(dto.OrderItems);

            order.OrderItems = orderItems;

            var query = _db.CyProduct.Where(x => x.IsVisible && x.Supply != 0).ToList();


            ///بهای تمام شده محصولات ==مجموع قیمتهای خرید
            double? shopPrice = 0;



            ///کم کردن از موجودی کالا 
            foreach (var item in dto.OrderItems)
            {
                var currentProductAvalable = query.Where(x => x.ID == item.ProductID).FirstOrDefault();
                if (currentProductAvalable == null) return BadRequest(new { msg = "این محصول درانبار موججود نیست", product = item.PartNumber });


                var currentProduct = query.Where(x => x.ID == item.ProductID && x.Supply >= item.Quantity).FirstOrDefault();

                if (currentProduct == null) return BadRequest(new { msg = "موجودی کالا کمتر از تعداد وارد شده است ", product = item.PartNumber });

                currentProduct.Supply -= item.Quantity;
                shopPrice += currentProduct.ShopPrice;
            }


            /////ثبت سند حسابداری
            Voucher newVoucher = new Voucher()
            {
                VoucherDate = DateTime.Now,
                ReferenceType = ordermode == Ordermode.SaleToCustomer ? "فاکتور فروش" : "فاکتور برگشت از خرید",
                ReferenceId = 0,
                Description = dto.StatusText,
                Items =
                {
                    ///////سند کسر از موجودی 
                   
                    ///کسر از موجودی کالا
                    new VoucherItem{
                        //AccountId=24,
                        //ToAccountId=14,

                        ////snDb2
                        AccountId=AccountSnDb.MojodiKala,
                        ToAccountId=AccountSnDb.BahayKala,
                        Debit=0,
                        Credit=shopPrice !=null ? (double)shopPrice : 0
                    },
                    
                    ///ثبت هزینه بهای تمام‌شده کالای فروش‌رفته
                    new VoucherItem
                    {
                        //AccountId=14,
                        //ToAccountId=24,

                        ////snDb2
                        AccountId=AccountSnDb.BahayKala,
                        ToAccountId=AccountSnDb.MojodiKala,
                        Debit=shopPrice !=null ? (double)shopPrice : 0,
                        Credit=0
                    }, 



                    ////ثبت سند مشتری 
                    
                    //بدهکار کردن مشتری
                    new VoucherItem
                    {
                        //ToAccountId=15,
                        AccountId=(int)user.AccountId,
                        ToAccountId=AccountSnDb.DarAmad,
                        Debit=(double)dto.FanalTotalAmount,
                        Credit=0
                    },
                    ///ثبت درآمد
                    new VoucherItem
                    {
                        //AccountId=15,
                        AccountId=AccountSnDb.DarAmad,
                        ToAccountId=(int)user.AccountId,
                        Debit=0,
                        Credit=(double)dto.FanalTotalAmount
                    },
                }
            };


            foreach (var item in newVoucher.Items)
            {
                var currentAccount = await _db.Account.Where(x => x.IsVisible && x.ID == item.AccountId).FirstOrDefaultAsync();
                currentAccount.MandehHesab = currentAccount.MandehHesab + item.Debit - item.Credit;
                item.MandehHesab = currentAccount.MandehHesab;

            }


            ////جنریت کردن کد فاکتور
            int nextFactorNum = await _db.CyOrder
           .Select(f => (int?)f.FactorNumber)
           .MaxAsync() ?? 0;
            nextFactorNum += 1;
            order.FactorNumber = nextFactorNum;
            order.OrderMode = ordermode;

            await _db.CyOrderItem.AddRangeAsync(orderItems);
            await _db.CyOrder.AddAsync(order);

            newVoucher.ReferenceId = order.FactorNumber;

            await _db.AddRangeAsync(newVoucher.Items);
            await _db.Voucher.AddAsync(newVoucher);


            await _db.SaveChangesAsync();

            return Ok(new { order = order, factoNumber = nextFactorNum });
        }





        [HttpPut("editOrder")]
        public async Task<ActionResult> editOrder([FromBody] OrderDTO dto)
        {
            var allProducS = await _db.CyProduct.Where(x => x.IsVisible && x.CyProductCategoryId != null).ToListAsync();

            var currentOrder = await _db.CyOrder.Where(x => x.IsVisible && x.FactorNumber == dto.FactorNumber && x.ID == dto.ID).Include(i => i.OrderItems).Include(i => i.CyUser).FirstOrDefaultAsync();

            var currentVoucher = await _db.Voucher.Where(x => x.IsVisible && x.ReferenceId == dto.FactorNumber).Include(i => i.Items).ThenInclude(i => i.Account).FirstOrDefaultAsync();

            var userVo = currentVoucher.Items.Where(x => x.AccountId == currentOrder.CyUser.AccountId).FirstOrDefault();

            var user = await _db.CyUser.Where(x => x.IsVisible && x.ID == dto.CyUserID).Include(u => u.Account).FirstOrDefaultAsync();



            if (currentOrder == null || currentVoucher == null) return BadRequest("order not Found");

            /// افزودن به موجودی کالا 
            foreach (var item in currentOrder.OrderItems)
            {
                var currentProductAvalable = allProducS.Where(x => x.IsVisible && x.ID == item.ProductID).FirstOrDefault();
                if (currentProductAvalable == null) return BadRequest(new { msg = "این محصول درانبار مو`جود نیست", product = item.PartNumber });

                if (currentOrder.OrderMode == Ordermode.SaleToCustomer || currentOrder.OrderMode == Ordermode.BackShopFrom)
                {

                    currentProductAvalable.Supply += item.Quantity;
                }
            }

            var orderItems = _mapper.Map<List<CyOrderItem>>(dto.OrderItems);
            currentOrder.OrderItems = orderItems;
            currentOrder.CreateDate = dto.CreatDate;
            currentOrder.Cost = dto.Cost;
            currentOrder.Taxes = dto.Taxes;
            currentOrder.Discount = dto.Discount;
            currentOrder.CyUserID = dto.CyUserID;
            currentOrder.TotalAmount = dto.TotalAmount;
            currentOrder.FanalTotalAmount = dto.FanalTotalAmount;
            currentOrder.Status = dto.Status;
            currentOrder.StatusText = dto.StatusText;

            var query = allProducS.Where(x => x.Supply != 0).ToList();
            //var query = _db.CyProduct.Where(x => x.IsVisible && x.Supply != 0).ToList();
            ///بهای تمام شده محصولات ==مجموع قیمتهای خرید
            double? shopPrice = 0;


            ///کم کردن از موجودی کالا 
            foreach (var item in dto.OrderItems)
            {
                var currentProductAvalable = query.Where(x => x.IsVisible && x.ID == item.ProductID).FirstOrDefault(); ;
                if (currentProductAvalable == null) return BadRequest(new { msg = "این محصول درانبار مو`جود نیست", product = item.PartNumber });


                var currentProduct = allProducS.Where(x => x.IsVisible && x.ID == item.ProductID && x.Supply >= item.Quantity).FirstOrDefault();

                if (currentProduct == null) return BadRequest(new { msg = "موجودی کالا کمتر از تعداد وارد شده است ", product = item.PartNumber });

                currentProduct.Supply -= item.Quantity;
                shopPrice += currentProduct.ShopPrice;
            }


            //var mojodiKalaVo = currentVoucher.Items.Where(x => x.AccountId == 24).FirstOrDefault();
            //var bahayeKalaVo = currentVoucher.Items.Where(x => x.AccountId == 14).FirstOrDefault();
            var darAmadVo = currentVoucher.Items.Where(x => x.AccountId == AccountSnDb.DarAmad).FirstOrDefault();

            ///sndb2
            var mojodiKalaVo = currentVoucher.Items.Where(x => x.AccountId == AccountSnDb.MojodiKala).FirstOrDefault();
            var bahayeKalaVo = currentVoucher.Items.Where(x => x.AccountId == AccountSnDb.BahayKala).FirstOrDefault();

            darAmadVo.IsEdited = true;
            mojodiKalaVo.IsEdited = true;
            bahayeKalaVo.IsEdited = true;
            userVo.IsEdited = true;


            mojodiKalaVo.Account.MandehHesab = mojodiKalaVo.Account.MandehHesab - mojodiKalaVo.Debit + mojodiKalaVo.Credit;
            bahayeKalaVo.Account.MandehHesab = bahayeKalaVo.Account.MandehHesab - bahayeKalaVo.Debit + bahayeKalaVo.Credit;
            userVo.Account.MandehHesab = userVo.Account.MandehHesab - userVo.Debit + userVo.Credit;
            darAmadVo.Account.MandehHesab = darAmadVo.Account.MandehHesab - darAmadVo.Debit + darAmadVo.Credit;

            mojodiKalaVo.MandehHesab = mojodiKalaVo.MandehHesab - mojodiKalaVo.Debit + mojodiKalaVo.Credit;
            bahayeKalaVo.MandehHesab = bahayeKalaVo.MandehHesab - bahayeKalaVo.Debit + bahayeKalaVo.Credit;
            userVo.MandehHesab = userVo.MandehHesab - userVo.Debit + userVo.Credit;
            darAmadVo.MandehHesab = darAmadVo.MandehHesab - darAmadVo.Debit + darAmadVo.Credit;


            if (mojodiKalaVo == null || bahayeKalaVo == null || userVo == null || darAmadVo == null) return BadRequest(new { msg = "یک یا تعدادی از اسناد یافت نشد" });

            ///////سند کسر از موجودی 

            ///کسر از موجودی کالا
            mojodiKalaVo.Debit = 0;
            mojodiKalaVo.Credit = shopPrice != null ? (double)shopPrice : 0;
            mojodiKalaVo.MandehHesab = mojodiKalaVo.MandehHesab + mojodiKalaVo.Debit - mojodiKalaVo.Credit;
            mojodiKalaVo.Account.MandehHesab = mojodiKalaVo.Account.MandehHesab - mojodiKalaVo.Credit;



            ///ثبت هزینه بهای تمام‌شده کالای فروش‌رفته
            bahayeKalaVo.Debit = shopPrice != null ? (double)shopPrice : 0;
            bahayeKalaVo.Credit = 0;
            bahayeKalaVo.MandehHesab = bahayeKalaVo.MandehHesab + bahayeKalaVo.Debit - bahayeKalaVo.Credit;
            bahayeKalaVo.Account.MandehHesab = bahayeKalaVo.Account.MandehHesab + bahayeKalaVo.Debit;


            ////ثبت سند مشتری 


            //بدهکار کردن مشتری
            userVo.AccountId = user.AccountId;
            userVo.Debit = (double)dto.FanalTotalAmount;
            userVo.Credit = 0;
            userVo.MandehHesab = userVo.MandehHesab + userVo.Debit - userVo.Credit;
            userVo.Account.MandehHesab = userVo.Account.MandehHesab + userVo.Debit;

            ///ثبت درآمد
            darAmadVo.ToAccountId = user.AccountId;
            darAmadVo.Debit = 0;
            darAmadVo.Credit = (double)dto.FanalTotalAmount;
            darAmadVo.MandehHesab = darAmadVo.MandehHesab + darAmadVo.Debit - darAmadVo.Credit;
            darAmadVo.Account.MandehHesab = darAmadVo.Account.MandehHesab - darAmadVo.Credit;



            //_db.CyOrder.Update(currentOrder);
            await _db.SaveChangesAsync();
            return Ok(currentOrder);


        }

        /// <summary>
        /// فاکتورخرید
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("addOrderB")]
        async public Task<ActionResult> addOrderB([FromBody] OrderDTO dto, Ordermode ordermode)
        {
            if (dto.OrderItems == null || !dto.OrderItems.Any()) return BadRequest(new { msg = "محصولی اضافه نشده است " });

            var user = _db.CyUser.Where(x => x.IsVisible && x.ID == dto.CyUserID).Include(u => u.Account).FirstOrDefault();

            var order = _mapper.Map<CyOrder>(dto);

            order.CreateDate = dto.CreatDate;


            var orderItems = _mapper.Map<List<CyOrderItem>>(dto.OrderItems);



            order.OrderItems = orderItems;

            var query = _db.CyProduct.Where(x => x.IsVisible && x.CyProductCategoryId != null).ToList();




            ///اضافه کردن به موجودی کالا 
            foreach (var item in dto.OrderItems)
            {
                var currentProductAvalable = query.Where(x => x.ID == item.ProductID).FirstOrDefault();
                if (currentProductAvalable == null) return BadRequest(new { msg = "این محصول درانبار موججود نیست", product = item.PartNumber });


                var currentProduct = query.Where(x => x.ID == item.ProductID).FirstOrDefault();

                if (currentProduct == null) return BadRequest(new { msg = "این محصول درانبار موججود نیست", product = item.PartNumber });

                double baePrice = item.UnitPrice;

                currentProduct.Supply += item.Quantity;
                currentProduct.ShopPrice = item.UnitPrice;

                currentProduct.Price = baePrice + (baePrice * 40 / 100);   ///15-2   20-3 30-4   40-1    
                currentProduct.Price2 = baePrice + (baePrice * 15 / 100);   ///15-2   20-3 30-4   40-1    
                currentProduct.Price3 = baePrice + (baePrice * 20 / 100);   ///15-2   20-3 30-4   40-1    
                currentProduct.Price4 = baePrice + (baePrice * 30 / 100);   ///15-2   20-3 30-4   40-1    

            }


            /////ثبت سند حسابداری
            Voucher newVoucher = new Voucher()
            {
                VoucherDate = DateTime.Now,
                ReferenceType = ordermode == Ordermode.ShopFromCustomer ? "فاکتور خرید" : "فاکتور برگشت از فروش",
                ReferenceId = 0,
                Description = dto.StatusText,
                Items =
                {
                    ///////سندافزودن به موجودی 
                   
                    ///افزودن به موجودی کالا
                    new VoucherItem{
                        //AccountId=24,

                        ///sndb2
                        AccountId=AccountSnDb.MojodiKala,
                        ToAccountId=(int)user.AccountId,
                        Debit=(double)dto.FanalTotalAmount,
                        Credit=0
                    },

                            ////بستانکار کردن تامین کننده
                    new VoucherItem
                    {
                        //ToAccountId=24,

                        ///sndb2
                        AccountId=(int)user.AccountId,
                        ToAccountId=AccountSnDb.MojodiKala,
                        Debit=0,
                        Credit=(double)dto.FanalTotalAmount
                    },

                }
            };
            foreach (var item in newVoucher.Items)
            {
                var currentAccount = await _db.Account.Where(x => x.IsVisible && x.ID == item.AccountId).FirstOrDefaultAsync();
                currentAccount.MandehHesab = currentAccount.MandehHesab + item.Debit - item.Credit;
                item.MandehHesab = currentAccount.MandehHesab;

            }

            ////جنریت کردن کد فاکتور
            int nextFactorNum = await _db.CyOrder
           .Select(f => (int?)f.FactorNumber)
           .MaxAsync() ?? 0;
            nextFactorNum += 1;
            order.FactorNumber = nextFactorNum;
            order.OrderMode = ordermode;

            await _db.CyOrderItem.AddRangeAsync(orderItems);
            await _db.CyOrder.AddAsync(order);

            newVoucher.ReferenceId = order.FactorNumber;

            await _db.AddRangeAsync(newVoucher.Items);
            await _db.Voucher.AddAsync(newVoucher);


            await _db.SaveChangesAsync();

            return Ok(new { order = order, factoNumber = nextFactorNum });
        }

        [HttpPut("editOrderB")]
        public async Task<ActionResult> editOrderB([FromBody] OrderDTO dto)
        {
            var allProducS = await _db.CyProduct.Where(x => x.IsVisible && x.CyProductCategoryId != null).ToListAsync();

            var currentOrder = await _db.CyOrder.Where(x => x.IsVisible && x.FactorNumber == dto.FactorNumber && x.ID == dto.ID).Include(i => i.OrderItems).Include(i => i.CyUser).FirstOrDefaultAsync();

            var currentVoucher = await _db.Voucher.Where(x => x.IsVisible && x.ReferenceId == dto.FactorNumber).Include(i => i.Items).ThenInclude(i => i.Account).FirstOrDefaultAsync();

            var userVo = currentVoucher.Items.Where(x => x.AccountId == currentOrder.CyUser.AccountId).FirstOrDefault();

            var user = await _db.CyUser.Where(x => x.IsVisible && x.ID == dto.CyUserID).Include(u => u.Account).FirstOrDefaultAsync();



            if (currentOrder == null || currentVoucher == null) return BadRequest("order not Found");

            /// افزودن به موجودی کالا 
            foreach (var item in currentOrder.OrderItems)
            {
                var currentProductAvalable = allProducS.Where(x => x.ID == item.ProductID).FirstOrDefault();
                if (currentProductAvalable == null) return BadRequest(new { msg = "این محصول درانبار مو`جود نیست", product = item.PartNumber });

                if (currentOrder.OrderMode == Ordermode.ShopFromCustomer || currentOrder.OrderMode == Ordermode.BackSaleToCustomer)
                {

                    currentProductAvalable.Supply -= item.Quantity;
                }
            }



            var orderItems = _mapper.Map<List<CyOrderItem>>(dto.OrderItems);
            currentOrder.CreateDate = dto.CreatDate;
            currentOrder.Cost = dto.Cost;
            currentOrder.Taxes = dto.Taxes;
            currentOrder.Discount = dto.Discount;
            currentOrder.CyUserID = dto.CyUserID;
            currentOrder.TotalAmount = dto.TotalAmount;
            currentOrder.FanalTotalAmount = dto.FanalTotalAmount;
            currentOrder.Status = dto.Status;
            currentOrder.StatusText = dto.StatusText;
            currentOrder.OrderItems = orderItems;


            ///بهای تمام شده محصولات ==مجموع قیمتهای خرید
            double? shopPrice = 0;


            ///اضافه کردن از موجودی کالا 
            foreach (var item in dto.OrderItems)
            {
                var currentProductAvalable = allProducS.Where(x => x.ID == item.ProductID).FirstOrDefault();
                if (currentProductAvalable == null) return BadRequest(new { msg = "این محصول درانبار مو`جود نیست", product = item.PartNumber });




                double baePrice = item.UnitPrice;

                currentProductAvalable.Supply += item.Quantity;
                currentProductAvalable.ShopPrice = item.UnitPrice;

                currentProductAvalable.Price = baePrice + (baePrice * 40 / 100);   ///15-2   20-3 30-4   40-1    
                currentProductAvalable.Price2 = baePrice + (baePrice * 15 / 100);   ///15-2   20-3 30-4   40-1    
                currentProductAvalable.Price3 = baePrice + (baePrice * 20 / 100);   ///15-2   20-3 30-4   40-1    
                currentProductAvalable.Price4 = baePrice + (baePrice * 30 / 100);   ///15-2   20-3 30-4   40-1
            }



            //var mojodiKalaVo = currentVoucher.Items.Where(x => x.AccountId == 24).FirstOrDefault();


            ///sndb2
            var mojodiKalaVo = currentVoucher.Items.Where(x => x.AccountId == AccountSnDb.MojodiKala).FirstOrDefault();
            mojodiKalaVo.Account.MandehHesab = mojodiKalaVo.Account.MandehHesab - mojodiKalaVo.Debit + mojodiKalaVo.Credit;
            userVo.Account.MandehHesab = userVo.Account.MandehHesab - userVo.Debit + userVo.Credit;

            mojodiKalaVo.MandehHesab = mojodiKalaVo.MandehHesab - mojodiKalaVo.Debit + mojodiKalaVo.Credit;
            userVo.MandehHesab = userVo.MandehHesab - userVo.Debit + userVo.Credit;



            mojodiKalaVo.IsEdited = true;
            userVo.IsEdited = true;

            if (mojodiKalaVo == null || userVo == null) return BadRequest(new { msg = "یک یا تعدادی از اسناد یافت نشد" });

            ///////سند کسر از موجودی 

            ///افزودن به موجودی کالا
            mojodiKalaVo.Debit = (double)dto.FanalTotalAmount;
            mojodiKalaVo.Credit = 0;
            mojodiKalaVo.MandehHesab = mojodiKalaVo.MandehHesab + mojodiKalaVo.Debit - mojodiKalaVo.Credit;

            mojodiKalaVo.Account.MandehHesab = mojodiKalaVo.Account.MandehHesab + mojodiKalaVo.Debit;


            ////ثبت سند مشتری 

            //بدهکار کردن مشتری
            userVo.AccountId = user.AccountId;
            userVo.Debit = 0;
            userVo.Credit = (double)dto.FanalTotalAmount;
            userVo.MandehHesab = userVo.MandehHesab + userVo.Debit - userVo.Credit;

            userVo.Account.MandehHesab = userVo.Account.MandehHesab - userVo.Credit;


            //_db.CyOrder.Update(currentOrder);
            await _db.SaveChangesAsync();
            return Ok(currentOrder);


        }


        [HttpPost("addOrderC")]
        async public Task<ActionResult> addOrderC([FromBody] OrderDTO dto, Ordermode ordermode)
        {
            if (dto.OrderItems == null || !dto.OrderItems.Any()) return BadRequest(new { msg = "محصولی اضافه نشده است " });

            var user = _db.CyUser.Where(x => x.IsVisible && x.ID == dto.CyUserID).Include(u => u.Account).FirstOrDefault();

            var order = _mapper.Map<CyOrder>(dto);

            order.CreateDate = dto.CreatDate;


            var orderItems = _mapper.Map<List<CyOrderItem>>(dto.OrderItems);



            order.OrderItems = orderItems;

            var query = _db.CyProduct.Where(x => x.IsVisible && x.CyProductCategoryId != null).ToList();


            double? shopPrice = 0;

            ///اضافه کردن به موجودی کالا 
            foreach (var item in dto.OrderItems)
            {
                var currentProductAvalable = query.Where(x => x.ID == item.ProductID).FirstOrDefault();
                if (currentProductAvalable == null) return BadRequest(new { msg = "این محصول درانبار موججود نیست", product = item.PartNumber });


                var currentProduct = query.Where(x => x.ID == item.ProductID).FirstOrDefault();

                if (currentProduct == null) return BadRequest(new { msg = "این محصول درانبار موججود نیست", product = item.PartNumber });

                double baePrice = item.UnitPrice;

                currentProduct.Supply += item.Quantity;
                shopPrice += currentProduct.ShopPrice;
            }


            /////ثبت سند حسابداری
            Voucher newVoucher = new Voucher()
            {
                VoucherDate = DateTime.Now,
                ReferenceType = "فاکتور برگشت از فروش",
                ReferenceId = 0,
                Description = dto.StatusText,
                Items =
                {
                    ///////سندافزودن به موجودی 
                   
                    ///افزودن به موجودی کالا
                    new VoucherItem{
                        //AccountId=24,

                        ///sndb2
                        AccountId=AccountSnDb.MojodiKala,
                        ToAccountId=AccountSnDb.BahayKala,
                        Debit=shopPrice!=null ? (double)shopPrice : 0,
                        Credit=0
                    },

                   //// کسر از بهای تمام شده کالا
                    new VoucherItem{
                        //AccountId=24,

                        ///sndb2
                        AccountId=AccountSnDb.BahayKala,
                        ToAccountId=AccountSnDb.MojodiKala,
                        Debit=0,
                        Credit=shopPrice!=null ? (double)shopPrice : 0
                    },


                            ////بستانکار کردن تامین کننده
                    new VoucherItem
                    {
                        //ToAccountId=24,

                        ///sndb2
                        AccountId=(int)user.AccountId,
                        ToAccountId=AccountSnDb.bargashtAzFrosh,
                        Debit=0,
                        Credit=(double)dto.FanalTotalAmount
                    },
          

                            ////حساب برگشت از فروش   
                    new VoucherItem
                    {
                        //ToAccountId=24,

                        ///sndb2
                        AccountId=AccountSnDb.bargashtAzFrosh,
                        ToAccountId=(int)user.AccountId,
                        Debit=(double)dto.FanalTotalAmount,
                        Credit=0
                    },

                }
            };
            foreach (var item in newVoucher.Items)
            {
                var currentAccount = await _db.Account.Where(x => x.IsVisible && x.ID == item.AccountId).FirstOrDefaultAsync();
                currentAccount.MandehHesab = currentAccount.MandehHesab + item.Debit - item.Credit;
                item.MandehHesab = currentAccount.MandehHesab;

            }

            ////جنریت کردن کد فاکتور
            int nextFactorNum = await _db.CyOrder
           .Select(f => (int?)f.FactorNumber)
           .MaxAsync() ?? 0;
            nextFactorNum += 1;
            order.FactorNumber = nextFactorNum;
            order.OrderMode = ordermode;

            await _db.CyOrderItem.AddRangeAsync(orderItems);
            await _db.CyOrder.AddAsync(order);

            newVoucher.ReferenceId = order.FactorNumber;

            await _db.AddRangeAsync(newVoucher.Items);
            await _db.Voucher.AddAsync(newVoucher);
            await _db.SaveChangesAsync();

            return Ok(new { order = order, factoNumber = nextFactorNum });
        }

        [HttpGet("getAllOrders")]
        async public Task<ActionResult> getAllOrders(DateTime fromDate, DateTime toDate)
        {
            var orders = await _db.CyOrder.AsNoTracking().
                Where(x => x.IsVisible).
                Include(i => i.CyUser)
                //.Select(s => new
                //{
                //    id = s.ID,
                //    date = s.CreateDate,
                //    factorNumber = s.FactorNumber,
                //    fanalTotalAmount = s.FanalTotalAmount,
                //    user = s.CyUser.CyUsNm
                //})
                .ToListAsync();

            orders.Reverse();

            return Ok(orders);
        }


        [HttpGet("getOrders")]
        async public Task<ActionResult> getOrders(DateTime fromDate, DateTime toDate, Ordermode ordermode)
        {
            var orders = await _db.CyOrder.AsNoTracking().
                Where(x => x.IsVisible && x.OrderMode == ordermode && x.CreateDate >= fromDate && x.CreateDate <= toDate).
                Include(i => i.CyUser).Select(s => new
                {
                    id = s.ID,
                    date = s.CreateDate,
                    factorNumber = s.FactorNumber,
                    fanalTotalAmount = s.FanalTotalAmount,
                    user = s.CyUser.CyUsNm
                }).ToListAsync();

            orders.Reverse();

            return Ok(orders);
        }
        [HttpGet("getOrdersB")]
        async public Task<ActionResult> getOrdersB(DateTime fromDate, DateTime toDate)
        {
            var orders = await _db.CyOrder.AsNoTracking().
                Where(x => x.IsVisible && x.OrderMode == Ordermode.ShopFromCustomer && x.CreateDate >= fromDate && x.CreateDate <= toDate).
                Include(i => i.CyUser).Select(s => new
                {
                    id = s.ID,
                    date = s.CreateDate,
                    factorNumber = s.FactorNumber,
                    fanalTotalAmount = s.FanalTotalAmount,
                    user = s.CyUser.CyUsNm
                }).ToListAsync();

            orders.Reverse();

            return Ok(orders);
        }


        [HttpGet("getOrderDetail")]
        public async Task<ActionResult> getOrderDetail(int factorNum)
        {
            var currentOrder = await _db.CyOrder.
                Where(x => x.IsVisible && x.FactorNumber == factorNum).Include(i => i.OrderItems).Include(i => i.CyUser).FirstOrDefaultAsync();

            if (currentOrder == null) return BadRequest("order NotDound");

            var dto = _mapper.Map<OrderBDTO>(currentOrder);

            dto.UserName = currentOrder?.CyUser?.CyUsNm;
            dto.CreatDate = currentOrder.CreateDate;

            return Ok(dto);
        }
        [HttpGet("getOrderDetailId")]
        public async Task<ActionResult> getOrderDetailId(int id)
        {
            var currentOrder = await _db.CyOrder.
                Where(x => x.IsVisible && x.ID == id).Include(i => i.OrderItems).Include(i => i.CyUser).FirstOrDefaultAsync();

            if (currentOrder == null) return BadRequest("order NotDound");

            var dto = _mapper.Map<OrderDTO>(currentOrder);

            dto.UserName = currentOrder?.CyUser?.CyUsNm;
            dto.CreatDate = currentOrder.CreateDate;

            return Ok(dto);
        }

    }
}