using System.ComponentModel.DataAnnotations;

namespace App.Models
{
    public class Banner
    {
        [Key]
        public int Id { get; set; }
        [Display(Name ="Images")]
        public string Link { get; set; }
        [Display(Name = "Title")]
        public string Text { get; set; }
    }
}
