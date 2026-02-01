using CY_BM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_DM
{
    public class CyOrder : BaseDM
    {

        public int? CyUserID { get; set; }
        public virtual CyUser? CyUser { get; set; }

        public int? FactorNumber { get; set; }


        public Ordermode? OrderMode { get; set; }

        public DateTime? OrderToTasvieh { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.InProcess;

        /// <summary>
        /// توضیحات فاکتور
        /// </summary>
        public string? StatusText { get; set; }
        public double? TotalAmount { get; set; }
        public double? FanalTotalAmount { get; set; }


        /// <summary>
        /// مالیات
        /// </summary>
        public double Taxes { get; set; } = 0;


        /// <summary>
        /// تخفیف
        /// </summary>
        public double Discount { get; set; } = 0;


        /// <summary>
        /// هزینه
        /// </summary>
        public double Cost { get; set; } = 0;

        public ICollection<CyOrderItem>? OrderItems { get; set; }

    }

}
