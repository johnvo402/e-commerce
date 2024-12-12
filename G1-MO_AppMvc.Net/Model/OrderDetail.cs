using System;
using System.Collections.Generic;

#nullable disable

namespace App.Model
{
	public partial class OrderDetail
	{
		public string IdOrderDetail { get; set; }
		public string IdProItem { get; set; }
		public string IdOrder { get; set; }
		public int Quantity { get; set; }
		public double Price { get; set; }
		public double OrderTotal { get; set; }
		public int? Review {  get; set; }
		public virtual Order IdOrderNavigation { get; set; }
		public virtual ProductItem IdProItemNavigation { get; set; }
	}
}
