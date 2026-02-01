using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CY_DM
{
    public class Voucher : BaseDM
    {

        public DateTime VoucherDate { get; set; }
        public string Description { get; set; } = null!;

        public string? ReferenceType { get; set; } // Invoice, Payment
        public int? ReferenceId { get; set; }

        public virtual ICollection<VoucherItem> Items { get; set; } = new List<VoucherItem>();
    }


    public class VoucherItem : BaseDM
    {


        public int? VoucherId { get; set; }
        [JsonIgnore]
        public Voucher? Voucher { get; set; } = null!;

        public int? AccountId { get; set; }
        [JsonIgnore]
        public Account? Account { get; set; } = null!;

        public double? MandehHesab { get; set; }

        public bool? IsEdited { get; set; }

        public int? ToAccountId { get; set; }


        public double Debit { get; set; }
        public double Credit { get; set; }
    }
}
