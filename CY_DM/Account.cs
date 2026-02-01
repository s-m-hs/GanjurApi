using CY_BM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CY_DM
{
    public class Account : BaseDM
    {

        public string Code { get; set; } = null!;   // مثل 1101
        public string Title { get; set; } = null!;  // موجودی کالا

        public AccountType AccountType { get; set; }


        public double? MandehHesab { get; set; }


        public int? ParentId { get; set; }
        [JsonIgnore]
        public Account? Parent { get; set; }

        [JsonIgnore]
        public virtual ICollection<Account> Children { get; set; } = new List<Account>();


        [JsonIgnore]
        public virtual ICollection<Voucher>? Voucher { get; set; }
        public virtual CyUser? CyUser { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
