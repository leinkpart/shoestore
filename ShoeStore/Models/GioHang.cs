using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;

namespace ShoeStore.Models
{
    public class GioHang
    {

        public int ProductID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageURL { get; set; }

        public decimal Total => Price * Quantity;
    }

    public class CheckoutViewModel
    {
        public List<GioHang> CartItems { get; set; }
        public decimal TotalAmount { get; set; }

        //// Thông tin người đặt
        //public string FullName { get; set; }
        //public string Address { get; set; }
        //public string PhoneNumber { get; set; }
        //public string Email { get; set; }
    }
}