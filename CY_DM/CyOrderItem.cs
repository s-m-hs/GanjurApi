using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CY_BM;
using System.Text.Json.Serialization;

namespace CY_DM
{
    public class CyOrderItem : BaseDM
    {
        public string? PartNumber { get; set; }
        public string? Manufacturer { get; set; }
        public string? ProductCategory { get; set; }

        public int? ProductID { get; set; }
        public virtual CyProduct? Product { get; set; }


        public int? CyOrderID { get; set; }
        [JsonIgnore]
        public virtual CyOrder? CyOrder { get; set; }


        [Required]
        public int Quantity { get; set; }

        //[Column(TypeName = "int(18,2)")]
        public double? UnitPrice { get; set; }

        // [Column(TypeName = "int(18,2)")]
        public double? TotalPrice { get; set; }
        public OrderItemStatus Status { get; set; }
        public string? StatusText { get; set; }
        public string? Information { get; set; }

    }
}

