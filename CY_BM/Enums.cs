namespace CY_BM
{

    public enum AccountType
    {
        Asset = 1,       // دارایی
        Liability = 2,   // بدهی
        Equity = 3,      // سرمایه
        Revenue = 4,     // درآمد
        Expense = 5,      // هزینه
             Person = 6 //اشخاص
    }
    public enum TicketStatus
    {
        Waiting = 1,
        Answered = 2,
        Closed = 3,
    }
    public enum GuaranteeType
    {
        Guarantee = 1,
        Repairs = 2,

    }
    public enum UserStatus
    {
        Active = 0,
        Blocked = 1,
        Deactive = 2,
        UnConfirmed = 3,
    }
    public enum textEditorState
    {
        oldEditor = 0,
        newEditor = 1,
    }

    public enum UserType
    {
        SysAdmin = 8,
     
        


    /// <summary>
    /// مدیر 
    /// </summary>
        Manager = 1,

        /// <summary>
        /// کارمند
        /// </summary>
        Employee=2,
       
        /// <summary>
        /// تامین کننده
        /// </summary>
        Supplier=3,

        /// <summary>
        /// مشتری
        /// </summary>
        Customer = 4,


        /// <summary>
        /// همکار
        /// </summary>
        Partner = 5,


    }
    public enum ProductStatus
    {
        InStock = 1,
        Enquiry = 2
        //Inactive=1,
        //Active=2,
        //OutOfStock=3,
        //Discontinued=4
    }

    public enum OrderItemStatus
    {
        Pending = 0,
        Active = 1,
        Deactive = 2,
    }

    public enum OrderStatus
    {
        InBasket = 0,
        Pending = 1,
        Finalized = 2,
        Verified = 3,
        InProcess = 4,
        Delivered = 5,
        Canceled = 6,
        Draft = 7,
        InBOM = 8,
        IsSent = 9
    }

    public enum OrderMessageStatus
    {
        Sent = 1,
        Seen = 2
    }

    public enum InspectionLab
    {
        Iran = 1,
        China = 2
    }

    public enum Ordermode
    {
        
        /// <summary>
        /// فروش به مشتری
        /// </summary>
        SaleToCustomer = 1,
        
        
        /// <summary>
        /// خرید از 
        /// </summary>
        ShopFromCustomer = 2,
        
        /// <summary>
        /// برگشت از فروش
        /// </summary>
        BackSaleToCustomer = 3,

        /// <summary>
        /// برگشت از خرید
        /// </summary>
        BackShopFrom = 4,
       
        PishFactor = 5
    }

    public enum ProductProperty
    {
        manufacturer = 1,
        picture = 2,
        category = 3,
    }

    public enum MainProductCategory
    {
        hardWare = 2,
        Accessories = 3
    }


    public enum PublicState
    {
        state0 = 0,
        state1 = 1,
        state2 = 2,
        state3 = 3,
        state4 = 4,
        state5 = 5,
    }


    public enum PostState
    {
        post = 1,
        noPost = 2
    }


    public enum TaskState
    {
        Wating = 0,
        Started = 1,
        InTest = 2,
        Completed = 3,
        Cannceled = 4,

    }
    public enum Score
    {
        Excellent=5,
        Good=4,
        MoreEffort=3,
        Bad=2,
        VeryBad=1

    }

    public enum TaskKind
    {
        Note=1 ,
        Task=2
    }

    public enum Important
    {
 
        Considerable=0 ,
        Important=1 ,
        TooImportant=2
    }


    public enum PartnerStatus
    {
        NoPartner = 0,
        Partner = 1,
        Partner2 = 2,
        Partner3 = 3,
        Partner4 = 4,

    }

    public enum UserBalanceStatus
    {
        Tasvieh = 0,
        Bestankar = 1,
        Bedehkar = 2
    }
}
