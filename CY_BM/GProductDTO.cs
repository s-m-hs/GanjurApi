using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_BM
{
    public class GProductDTO
    {
        public long ID { get; set; }
        //public required global MyProperty { get; set; }
        public required string PartNumber { get; set; }
        public string? Description { get; set; }
        public string? Price { get; set; }
        public string? Stock { get; set; }
        public string? DatasheetUrl { get; set; }
        public string? SmartImageUrl { get; set; }
        public string Url { get; set; }

        public int? GrCategoryID { get; set; }
        public int? GrManufacturerID { get; set; }

        public List<GKeyValueDTO>? KeyValues { get; set;}
        //public virtual ICollection<GrKeyValue>? GrKeyValue { get; set; }
    }

    public class GManufacturerDTO
    {
        public int ID { get; set; }
        //public required global MyProperty { get; set; }
        public required string Name { get; set; }
        public string? Image { get; set; }
        public string? Url { get; set; }
    }
    public class GCategoryDTO
    {
        public int ID { get; set; }

        [Display(Name = "نام نمایشی")]
        public required string Text { get; set; }

        public required string Code { get; set; }

        [Display(Name = "الویت")]
        public int OrderValue { get; set; }

        public string? ImageUrl { get; set; }
        public int ProductCount { get; set; }
        public string? Url { get; set; }
        public int? rootId { get; set; }

    }
    public class GKeyValueDTO
    {
        public long ID { get; set; }
        //public required global MyProperty { get; set; }
        public required string Key { get; set; }
        public string? Value { get; set; }

        public long? GrProductID { get; set; }
     
    }

}
