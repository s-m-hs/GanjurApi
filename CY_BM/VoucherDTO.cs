using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CY_BM
{
    public class VoucherDTO
    {
        public DateTime VoucherDate { get; set; } = DateTime.Now;
        public string Description { get; set; } = null!;
        public string? ReferenceType { get; set; }
        public int? ReferenceId { get; set; }
        public List<VoucherItemDTO> Items { get; set; } = new();


    }


    public class VoucherItemDTO
    {
        public DateTime CreatDate { get; set; } = DateTime.Now;
        public DateTime VoucherDate { get; set; }

        public int AccountId { get; set; }

        public string? AccountName { get; set; }
        public bool? IsEdited { get; set; }

        public double? MandehHesab { get; set; }


        [JsonIgnore]
        public VoucherDTO? Voucher { get; set; } = null!;
        public int? ToAccountId { get; set; }
        public string? ToAccountName { get; set; }
        public double Debit { get; set; }
        public double Credit { get; set; }



        public string? RefrenceType { get; set; }
        public int? RefrencId { get; set; }

        public string? Description { get; set; }


    }



    public class VoucherDTOB
    {
        public int ID { get; set; }
        public DateTime CreatDate { get; set; } = DateTime.Now;
        public DateTime VoucherDate { get; set; }


        public string Description { get; set; } = null!;
        public string? ReferenceType { get; set; }
        public int? ReferenceId { get; set; }


        public List<VoucherItemDTOB> Items { get; set; } = new();


    }
    public class VoucherItemDTOB
    {
        public DateTime CreatDate { get; set; } = DateTime.Now;
        public DateTime VoucherDate { get; set; }

        public int AccountId { get; set; }
        public AccountDTO? Account { get; set; } 

        public string? AccountName { get; set; }
        public bool? IsEdited { get; set; }

        public double? MandehHesab { get; set; }


        [JsonIgnore]
        public VoucherDTO? Voucher { get; set; } = null!;
        public int? ToAccountId { get; set; }
        public string? ToAccountName { get; set; }
        public double Debit { get; set; }
        public double Credit { get; set; }



        public string? RefrenceType { get; set; }
        public int? RefrencId { get; set; }

        public string? Description { get; set; }
    }


}
