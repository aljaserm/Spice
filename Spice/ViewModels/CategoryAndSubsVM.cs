using Spice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.ViewModels
{
    public class CategoryAndSubsVM
    {
        public IEnumerable<Category> lstCategory { get; set; }
        public SubCategory SubCategory { get; set; }
        public List<string> lstSubCategory { get; set; }
        public string StatusMsg { get; set; }
    }
}
