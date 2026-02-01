using CY_BM;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_BM
{
    public class UserDTO
    {
        public int ID { get; set; }
        public required string CyUsNm { get; set; }
        // [StringLength(400)]
        public  string? CyHsPs { get; set; }
        public string? UserCodeA { get; set; }
        public string? UserCodeB { get; set; }

        public string? Mobile { get; set; }
        public string? MelliCode { get; set; }

        public string? Phone { get; set; }

        public int? AccountId { get; set; }
        public double? AccountBalance { get; set; } = 0;

        public string? UserAddress { get; set; }
        public UserStatus Status { get; set; }
        public UserType userType { get; set; }
        public PartnerStatus? PartnerStatus { get; set; } 
        public UserBalanceStatus UserBalanceStatus { get; set; } = UserBalanceStatus.Tasvieh;
    }
   
}




