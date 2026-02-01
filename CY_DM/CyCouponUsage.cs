using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CY_DM
{
    public class CyCouponUsage : BaseDM
    {
        public int UserId { get; set; }
        [JsonIgnore]

        public virtual  CyUser User { get; set; }


        public int CouponId { get; set; }
        [JsonIgnore]

        public virtual CyCoupon Coupon { get; set; }

        public Boolean? IsRequested { get; set; } = false;
        public DateTime AssignedAt { get; set; } // تاریخ اختصاص کد
        public DateTime? UsedAt { get; set; } // تاریخ استفاده (اگر هنوز استفاده نشده، مقدار null خواهد بود)



    }
}
