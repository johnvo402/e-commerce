using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace App.Model
{
	public class ProProItems
	{
		public Product products { get; set; }
		public ProductItem productItems { get; set; }

		public ImgPro imgPros { get; set; }
		[Required(ErrorMessage = "File img not null!!")]
		
		[DisplayName("Product img")]
		public IFormFile imgFiles { get; set; }

	}
}