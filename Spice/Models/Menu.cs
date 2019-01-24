using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Models
{
    public class Menu
    {

        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Descrption { get; set; }
        public string SpicyLevel { get; set; }
        public enum ESpicy { NA=0, NotSpicy=1,Spicy=2,VerSpicy=3 }
        [Display(Name = "Image")]
        public string Img { get; set; }
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        [Display(Name = "Sub Category")]
        public int SubCategoryId { get; set; }
        [Range(1,int.MaxValue,ErrorMessage ="Nothing is less than a ${1}")]
        public double Price { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }
        [ForeignKey("SubCategoryId")]
        public virtual SubCategory SubCategory { get; set; }

    }
}
