using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CY_DM
{
    public class SysPC : BaseDM
    {
        //public List<HardWare> HardWare { get; set; } = new();

        public ICollection<HardWare> HardWare { get; set; }

        public string? Description { get; set; }

        public string? CustmerName { get; set; }

        public string? CustmerPhone { get; set; }

        public bool IsFactor { get; set; } = false;


        [NotMapped]
        public double? SalePrice => HardWare.Sum(x=>x.TotalPrice);

        public double? ShopSale { get; set; }

    }


    public class HardWare
    {
        public int Id { get; set; }

    
        public string? Category { get; set; }
        public string? Name{ get; set; }

        public double Price { get; set; } = 0;

        public int Quntity { get; set; } = 1;

        public double TotalPrice => Quntity * Price ;


        public int? SysPCId { get; set; }
        [JsonIgnore]
        public virtual SysPC? SysPC { get; set; }

    }


}
