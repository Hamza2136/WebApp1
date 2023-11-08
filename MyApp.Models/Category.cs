using System.ComponentModel.DataAnnotations;
namespace MyApp.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name{ get; set; }
        [Display(Name = "Display Order")]
        public int DispalyOrder { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
    }
}
