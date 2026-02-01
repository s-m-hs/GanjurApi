using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CY_BM;

namespace CY_DM
{
    public class CyGuarantee : BaseDM
    {
        [Required]
        public int GuaranteeID { get; set; }
        public int? CyUserID { get; set; }
        public virtual CyUser? CyUser { get; set; }
        public string? ProductName { get; set; }
        public string? ProductStatus { get; set; }
        public string? GuaranteeCompany {  get; set; }
        public string? GuarantreePrice { get; set; }
        public DateTime? RecievedDate { get; set; }
        public string? ProductProblem {  get; set; }
        public string? Details {  get; set; }
        public string? CompanyExplaination { get; set; }
        public GuaranteeType Type {  get; set; }
    }
}
