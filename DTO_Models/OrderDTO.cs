using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DTO_Models.Enums;

namespace DTO_Models
{
    public class OrderDTO
    {
     
            public int UserId { get; set; }
            public Ordermode OrderMode { get; set; }
        public DateTime? OrderToTasvieh { get; set; }
        public string? StatusText { get; set; }
        public double Discount { get; set; }
            public double Taxes { get; set; }
        public double Cost { get; set; }

        public double? TotalAmount { get; set; }
        public double? FanalTotalAmount { get; set; }
        public List<OrderItemDTO> OrderItemsDTO { get; set; } = new();

        public class OrderItemDTO
        {
            public string? PartNumber { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public double? UnitPrice { get; set; }

            public double? TotalPrice { get; set; }

            public string? Information { get; set; }

        }
    }
}
