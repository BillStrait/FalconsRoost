using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Models.Restaurant
{
    public class Brand
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();
        public virtual ICollection<Store> Stores { get; set; } = new HashSet<Store>();
        public virtual ICollection<IFoodItem> BaseMenu { get; set; } = new HashSet<IFoodItem>();
    }
}
