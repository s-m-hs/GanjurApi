using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CY_BM
{
    public class AccountDTO
    {
        public int Id { get; set; }

        public string Code { get; set; } = null!;   // مثل 1101
        public string Title { get; set; } = null!;  // موجودی کالا

        public AccountType AccountType { get; set; }

        public double? MandehHesab { get; set; }

        public int? ParentId { get; set; }
        public AccountDTO? Parent { get; set; }
        [JsonIgnore]
        public ICollection<AccountDTO> Children { get; set; } = new List<AccountDTO>();

        [JsonIgnore]
        public virtual ICollection<VoucherDTO>? VoucherDTO { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
