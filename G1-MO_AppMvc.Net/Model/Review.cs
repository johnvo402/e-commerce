using System;
using System.Collections.Generic;

#nullable disable

namespace App.Model
{
    public partial class Review
    {
        public string IdReview { get; set; }
        public string IdAcc { get; set; }
        public string IdPro { get; set; }
        public double RatingValue { get; set; }
        public string Comment { get; set; }
        public DateTime? ReviewDate { get; set; }
        public virtual User IdAccNavigation { get; set; }
        public virtual Product IdProNavigation { get; set; }
    }
}
