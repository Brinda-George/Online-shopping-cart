//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Online_SHopping_Cart.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrderDetail_Table
    {
        public int OrderDetailId { get; set; }
        public int Orderid { get; set; }
        public Nullable<int> Serviceid { get; set; }
        public int Productid { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<decimal> Amount { get; set; }
    
        public virtual Order_Table Order_Table { get; set; }
        public virtual Product_Table Product_Table { get; set; }
        public virtual Service_Table Service_Table { get; set; }
    }
}