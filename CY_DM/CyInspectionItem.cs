using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_DM
{
    public class CyInspectionItem : BaseDM
    {
        public int CyInspectionFormID { get; set; }
        //public virtual CyInspectionForm? CyInspectionForm { get; set; }
        public string? PartNumber { get; set; }
        public int Quantity { get; set; }
        public string? TargetDate { get; set; }
    }
}