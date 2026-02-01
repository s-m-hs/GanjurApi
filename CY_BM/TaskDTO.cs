using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_BM
{
    public class TaskDTO
    {
        public virtual int ID { get; set; }
        public virtual DateTime CreateDate { get; set; }
        public required string Title { get; set; }

        public string? Description { get; set; }

        [DefaultValue(false)]
        public Boolean? Hidden { get; set; }
        public required TaskKind TaskKind { get; set; }

        public Score? Score { get; set; }
        public TaskState TaskState { get; set; }

        public string? Color { get; set; }

        public Important? Important { get; set; }
        public DateTime? CompletionDate { get; set; }

        public int? AdminId { get; set; }

        public int? UserId { get; set; }

    }
}
