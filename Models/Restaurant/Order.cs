using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Models.Restaurant
{
    public class Order
    {
        public int OrderId { get; set; }
        public int BrandId { get; set; }
        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; }
        public int StoreId { get; set; }
        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }
        public OrderStatus Status { get; set; }

    }

    public enum OrderStatus
    {

    }
}
