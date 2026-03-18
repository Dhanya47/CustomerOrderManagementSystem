using System.ComponentModel.DataAnnotations;

namespace Ordering.API.DTOs
{
    public class CreateOrder
    {
        [Required]
        public long OrderId { get; set; }

        [Required]
        public long CustomerId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "TotalAmount must be positive")]
        public double TotalAmount { get; set; }
    }
}
