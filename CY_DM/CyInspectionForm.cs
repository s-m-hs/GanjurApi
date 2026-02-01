using CY_BM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CY_DM
{
    /// <summary>
    /// فرم ارزیابی اصالت
    /// </summary>
    public class CyInspectionForm : BaseDM
    {
        public int UserID { get; set; }
        public virtual CyUser? User { get; set; }
        public InspectionLab? Lab { get; set; }
        public ICollection<CyInspectionItem>? Items { get; set; } //??
        public int? ExternalVisualInspection { get; set; }
        /// <summary>
        ///  تست همبستگی پین
        /// </summary>
        public int? PinCorrelationTest { get; set; }
        /// <summary>
        ///  تست بارگذاری برنامه
        /// </summary>
        public int? ProgrammingTest { get; set; }
        /// <summary>
        ///  بازرسی قابلیت لحیم کاری
        /// </summary>
        public int? SolderabilityAnalysis { get; set; }
        /// <summary>
        ///  بازرسی به وسیله رادیوگرافی
        /// </summary>
        public int? Radiography { get; set; }
        /// <summary>
        ///  تست xrft
        /// </summary>
        public int? XRFTest { get; set; }
        /// <summary>
        ///  تست عملکردی
        /// </summary>
        public int? KeyFunctional { get; set; }
        /// <summary>
        ///  خشک سازی
        /// </summary>
        public int? Baking { get; set; }
        /// <summary>
        ///  بسته بندی
        /// </summary>
        public int? TapeAndReel { get; set; }
        /// <summary>
        ///  بازرسی بصری داخلی
        /// </summary>
        public int? InternalVisualInspection { get; set; }
        public int? HeatedChemicalTest { get; set; }
        public int? CyAddressID { get; set; }
        public Guid? File { get; set; }
        public virtual CyAddress? CyAddress { get; set; }
    }
}