using Spice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.ViewModels
{
    public class OrderDeatilsVM
    {
        public Orders orders { get; set; }
        public List<OrderDetails> lstOrderDetails { get; set; }
    }
}
