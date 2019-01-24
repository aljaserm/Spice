using Spice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.ViewModels
{
    public class MenuVM
    {
        public Menu Menu { get; set; }
        public IEnumerable<Category> lstCategory  { get; set; }
        public IEnumerable<SubCategory> lstSubCategory { get; set; }
    }
}
