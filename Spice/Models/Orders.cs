using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.Models
{
    public class Orders
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        [Required]
        public double OrderTotal  { get; set; }
        [Required]
        [DisplayFormat(DataFormatString ="{0:C}")]
        [Display(Name ="Order Total")]
        public double OrderTotalOriginal { get; set; }
        [Required]
        [Display(Name ="Pickup Time")]
        public DateTime PicupTime { get; set; }
        [Required]
        [NotMapped]
        public DateTime PicupDate { get; set; }
        [Display(Name ="CouponCode")]
        public string CouponCode { get; set; }
        public double CouponCodeDiscount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string Comments { get; set; }
        [Display(Name ="Pickup Name")]
        public string PickupName { get; set; }
        public string Phone { get; set; }
        public string TransactionId { get; set; }
    }
}
