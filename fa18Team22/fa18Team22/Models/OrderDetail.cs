using System;
using System.ComponentModel.DataAnnotations;
namespace fa18Team22.Models
{
    public class OrderDetail
    {
    	public Int32 OrderDetailID { get; set; }

    	[Display(Name = "Quantity")]
    	public Int32 Quantity { get; set; }

    	[Display(Name = "Price")]
    	public Decimal Price{ get; set; }


    	//navigational properties
    	public virtual Order Order { get; set; }

    	public virtual Book Book { get; set; }

    }
}