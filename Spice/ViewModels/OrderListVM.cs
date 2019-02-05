using Spice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.ViewModels
{
    public class OrderListVM
    {
        public IList<OrderDeatilsVM> Orders { get; set; }
        public PagingInfo pagingInfo { get; set; }
    }
}
