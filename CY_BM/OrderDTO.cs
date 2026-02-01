using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CY_BM
{
    public class OrderDTO
    {
        public int ID { get; set; }
        public int? CyUserID { get; set; }

        public int? FactorNumber { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatDate { get; set; } = DateTime.Now;

        public Ordermode? OrderMode { get; set; }

        public DateTime? OrderToTasvieh { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.InProcess;

        /// <summary>
        /// توضیحات فاکتور
        /// </summary>
        public string? StatusText { get; set; }
        public double? TotalAmount { get; set; }
        public double? FanalTotalAmount { get; set; }

        public double Taxes { get; set; } = 0;

        public double Discount { get; set; } = 0;

        public double Cost { get; set; } = 0;

        public ICollection<OrderItemDTO>? OrderItems { get; set; }
    }
    public class OrderBDTO
    {
        public int ID { get; set; }
        public int? CyUserID { get; set; }

        public int? FactorNumber { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatDate { get; set; }

        public Ordermode? OrderMode { get; set; }

        public DateTime? OrderToTasvieh { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.InProcess;

        /// <summary>
        /// توضیحات فاکتور
        /// </summary>
        public string? StatusText { get; set; }
        public double? TotalAmount { get; set; }
        public double? FanalTotalAmount { get; set; }

        public double Taxes { get; set; } = 0;

        public double Discount { get; set; } = 0;

        public double Cost { get; set; } = 0;

        public ICollection<OrderItemByIdDTO>? OrderItems { get; set; }
    }
}
