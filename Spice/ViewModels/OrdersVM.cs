using Spice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.ViewModels
{
    public class OrdersVM
    {
        public List<ShoppingCart> lstshoppingCarts { get; set; }
        public Orders Orders { get; set; }

    }
}
