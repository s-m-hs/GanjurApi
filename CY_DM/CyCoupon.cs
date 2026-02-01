using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CY_DM
{
    public class CyCoupon :BaseDM
    {
        
            public string Code { get; set; }
            public double DiscountAmount { get; set; }
            public DateTime ExpireDate { get; set; }


        //for set Automaticly Isactive 
            [NotMapped]
            public bool IsActive => ExpireDate > DateTime.UtcNow;

        public virtual List<CyCouponUsage>? Coupons { get; set; }

    }
}
