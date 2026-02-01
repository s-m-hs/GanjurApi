using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CY_BM;

namespace CY_DM
{
    public class CyTask : BaseDM
    {
        public required string Title { get; set; }

        public string? Description { get; set; }

        [DefaultValue(false)]
        public Boolean? Hidden { get; set; } 

        public required TaskKind TaskKind { get; set; }
        public Score? Score { get; set; }
        public TaskState TaskState { get; set; }

        public string? Color { get; set; }

        public Important? Important { get; set; } 

        public DateTime?  CompletionDate { get; set; }
        /// <summary>
        /// /////ایجادکننده تسک
        /// </summary>
        public int? AdminId { get; set; }
        public virtual CyUser? Admin { get; set; }

        /// <summary>
        /// مامور انجام تسک
        /// </summary>
        public int? UserId { get; set; }
        public virtual CyUser? CyUser { get; set; }
    }
}
