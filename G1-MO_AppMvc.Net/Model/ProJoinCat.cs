
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Model
{
	public class ProJoinCat
	{

		public Product products { get; set; }
		public Category categories { get; set; }

		public List<ImgPro> imgPros { get; set; }

	}
}



