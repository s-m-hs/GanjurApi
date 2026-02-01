using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CY_BM
{
    public class OrderItemDTO
    {
        //public virtual int ID { get; set; }
        public string? PartNumber { get; set; }
        public int? ProductID { get; set; }
        public int? CyOrderID { get; set; }


        [Required]
        public int Quantity { get; set; }

        public int UnitPrice { get; set; }
        public int TotalPrice { get; set; }

        public string? Manufacturer { get; set; }
        public string? ProductCategory { get; set; }
        public string? Information { get; set; }
    }


    public class OrderItemByIdDTO
    {
        public  int ID { get; set; }
        public string? PartNumber { get; set; }
        public int? ProductID { get; set; }
        public int? CyOrderID { get; set; }


        [Required]
        public int Quantity { get; set; }

        public int UnitPrice { get; set; }
        public int TotalPrice { get; set; }

        public string? Manufacturer { get; set; }
        public string? ProductCategory { get; set; }
        public string? Information { get; set; }
    }

}
