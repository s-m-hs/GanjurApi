using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_DM
{
    public class Inventory:BaseDM
    {

        public int CyProductId { get; set; }
        public CyProduct CyProduct { get; set; } = null!;

        public int Quantity { get; set; }
    }
}
