using App.Model;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;

namespace App.Model
{
    public class Wallet
    {

        public string IdWallet { get; set; }
        public double Balance { get; set; }
        public string IdAcc { get; set; }
        public int Status { get; set; }
        public User IdAccNavigation { get; set; }


        [Range(50000, double.MaxValue, ErrorMessage = "The value must be greater than or equal to 50000.!")]
        public double Request { get; set; }
#nullable enable
        public string? NameCard { get; set; }
        [RegularExpression(@"^\d+$", ErrorMessage = "Input number!")]
        public string? NumberCard { get; set; }


        public string? NameBank { get; set; }

        

    }
}
