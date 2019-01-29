using Spice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.ViewModels
{
    public class IndexVM
    {
        public IEnumerable<Menu> menus { get; set; }
        public IEnumerable<Category> categories { get; set; }
        public IEnumerable<Coupon> coupons { get; set; }
    }
}
