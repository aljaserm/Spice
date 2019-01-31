using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Models
{
    public class ShoppingCart
    {
        public ShoppingCart()
        {
            Count = 1;
        }
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        [NotMapped]
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
        public int MenuId { get; set; }
        [NotMapped]
        [ForeignKey("MenuId")]
        public virtual Menu Menu { get; set; }
        [Range(1,int.MaxValue, ErrorMessage ="has to be more than 0")]
        public int Count { get; set; }
    }
}
