using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_BM
{
    public class CyCouponUsageDTO
    {
        public int ID { get; set; }
        public  bool IsVisible { get; set; }
        public virtual DateTime CreateDate { get; set; }
        public DateTime AssignedAt { get; set; } // تاریخ اختصاص کد
        public DateTime? UsedAt { get; set; } // تاریخ استفاده (اگر هنوز استفاده نشده، مقدار null خواهد بود)
    }
}
