using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CY_BM;

namespace CY_DM
{
    public class CyOrderMessage : BaseDM
    {
        //public string Title { get; set; }

        public int? SenderID    { get; set; }
        public virtual CyUser? Sender { get; set; }

        public int? OrderID { get; set; }
        public virtual CyOrder? Order { get; set; }

        public int? TicketID { get; set; }
        public virtual CyTicket? Ticket { get; set; }

        public string? Description   { get; set; }
        public OrderMessageStatus? Status { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? SeenDate { get; set; }

        public Guid? FileID { get; set; }
    }
}
