using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_BM
{
    /// <summary>
    /// ورودی جست و جو در مطالب برای پنل ادمین
    /// </summary>
    public class SubjectSearchDTO
    {
        /// <summary>
        /// جست و جو در عنوان
        /// </summary>
        public string? Title { get; set; }
        /// <summary>
        /// کد دسته بندی جست و جو
        /// </summary>
        public string? CategoryCode { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 10;
    }
}
