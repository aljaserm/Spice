using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Type { get; set; }
        public enum ECouponType { Percent=0, Dollar=1 }
        [Required]
        public double Discount { get; set; }
        [Required]
        public double MinAmount { get; set; }
        public byte[] img { get; set; }
        public bool Active { get; set; }
    }
}
