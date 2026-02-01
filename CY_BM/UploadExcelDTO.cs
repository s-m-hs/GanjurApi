using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_BM
{
    /// <summary>
    ///bom بارگذاری اکسل نتیجه سفارش برای ادمین در
    /// </summary>
    public class UploadExcelDTO
    {
        public Guid fileId { get; set; }
        public int orderId { get; set; }
    }
}
