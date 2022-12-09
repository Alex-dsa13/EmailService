using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ClientApp.Models
{
    public class EmailData
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }


        [Required]
        [StringLength(1000)]
        [Display(Name = "Message")]
        public string Message { get; set; }
    }
}
