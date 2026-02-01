using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_BM
{
    public class CouponDTO
    {
        public int Id { get; set; }

        public string? Code { get; set; }
        public double? DiscountAmount { get; set; }

        public  DateTime ExpireDate { get; set; }

        public bool IsActive { get; set; }
    }
}
