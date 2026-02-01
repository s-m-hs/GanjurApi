using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_BM
{
    public class BalanceDTO
    {
        public double Balance { get; set; }

        // جهت خوانایی
        public string BalanceStatus =>
            Balance > 0 ? "بدهکار" :
            Balance < 0 ? "بستانکار" :
            "تسویه";
    }
}
