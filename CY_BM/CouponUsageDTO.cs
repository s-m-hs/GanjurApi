using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_BM
{
    public class CouponUsageDTO
    {
        public  int ID { get; set; }

        public int UserId { get; set; }


        public int CouponId { get; set; }


        public Boolean? IsRequested { get; set; }
        public DateTime AssignedAt { get; set; } // تاریخ اختصاص کد
        public DateTime? UsedAt { get; set; } // تاریخ استفاده (اگر هنوز استفاده نشده، مقدار null خواهد بود)

    }
}
