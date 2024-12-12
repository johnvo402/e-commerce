using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace App.Model
{
    public partial class ImgPro
    {
        public string IdImg { get; set; }
        public string IdPro { get; set; }
        [Required(ErrorMessage = "File img name not null!!")]
        [RegularExpression(@"\.(gif|jpe?g|tiff?|png|webp|bmp)$", ErrorMessage = "Invalid image file format.")]
        public string LinkImg { get; set; }
        public virtual Product IdProNavigation { get; set; }
    }
}
