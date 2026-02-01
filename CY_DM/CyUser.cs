using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CY_BM;
namespace CY_DM
{
    public class CyUser : BaseDM
    {
        public required string CyUsNm { get; set; }
        public  string? CyHsPs { get; set; }


        public string? UserCodeA { get; set; }
        public string? UserCodeB { get; set; }

        public string? Mobile { get; set; }
        public string? MelliCode { get; set; }

        public string? Phone { get; set; }


        public bool? isPartner { get; set; }
        public double? AccountBalance { get; set; } = 0;

        public string? UserAddress { get; set; }
        public UserStatus Status { get; set; }
        public UserType userType { get; set; } 
        public PartnerStatus? PartnerStatus { get; set; } 
        public UserBalanceStatus UserBalanceStatus { get; set; } = UserBalanceStatus.Tasvieh;


        public int? AccountId { get; set; }
        [JsonIgnore]
        public virtual Account? Account { get; set; }


    }

}
