using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DTO_Models.Enums;

namespace DTO_Models
{
    public class OtherDTO
    {
        public class GIDTO
        {
            public Guid? GID { get; set; }
            public int? ID { get; set; }
            public string? Str { get; set; }
        }

        public class RegisterModel
        {
            public required string Un { get; set; }
            public required string Pw { get; set; }
            public required string Name { get; set; }
        }

        public class LoginModel
        {
            public required string Un { get; set; }
            public required string Pw { get; set; }
        }

        public class PageDTO
        {
            public string Cat { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; } = 0;
        }

        public class PageSubCatDTO
        {
            public int CatId { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; } = 0;
        }

        /// <summary>
        /// ورودی برای دریافت سفارشاتی که فیلد مشخص شده خالی باشد
        /// </summary>
        public class PageDTOprodWithNull
        {
            public ProductProperty Property { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; } = 0;
        }

        /// <summary>
        /// ورودی برای صفحه بندی
        /// </summary>
        public class PagedProdCatDTO
        {
            public int PageNumber { get; set; }
            public int PageSize { get; set; } = 10;
        }

        /// <summary>
        /// ورودی برای صفحه بندی و سرچ
        /// </summary>
        public class SearchSubjectDTO
        {
            public string SearchStr { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; } = 10;
        }

        public class PagedOrdersDto
        {
            public OrderStatus OrderStatus { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }


        public class PageResultDTO<T> where T : class
        {
            public string? PageTitle { get; set; }
            public List<T> ItemList { get; set; }
            public int AllCount { get; set; }
        }

        /// <summary>
        /// برای آپدیت کردن استاتوس یک orderItem
        /// </summary>
        public class UpdatePcbDTO
        {
            public int OrderId { get; set; }
            public string? StatusText { get; set; }
            public OrderStatus Status { get; set; }
        }

        /// <summary>
        /// خروجی فایل
        /// </summary>
        public class OutFileModel
        {
            public Guid? ID { get; set; }
            public string? Adress { get; set; }
        }
    }
}
