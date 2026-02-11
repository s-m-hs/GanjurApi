using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_BM
{
    public class ServiceDTO
    {
        public virtual int ID { get; set; }

        public virtual DateTime CreateDate { get; set; } = DateTime.Now;
        public string? CustomerName { get; set; }
        public string? Mobile { get; set; }
        public string? ProductType { get; set; }
        public string? ProductBrand { get; set; }
        public string? ProductModel { get; set; }

        public List<string>? AcceDevices { get; set; }

        public List<string>? ProductProblem { get; set; }
        public string? ProductProblemB { get; set; }
        public string? ServiceDescription { get; set; }
        public string? ServicePrice { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public GuaranteeType Type { get; set; }
    }

}
