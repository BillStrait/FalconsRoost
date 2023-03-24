using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Models.Restaurant
{
    public interface IFoodItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PriceInUSD { get; set; }
        public string ImageURL { get; set; }
        public string MenuCategory { get; set; }
        public ICollection<CustomizationOption> Customizations { get; set; }
    }
}
