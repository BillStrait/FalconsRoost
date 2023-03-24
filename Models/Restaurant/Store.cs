using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Models.Restaurant
{
    public class Store
    {
        public int StoreId { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; } = string.Empty;
        public string Address2 { get; set; } = string.Empty;
        public string Address3 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int BrandId { get; set; }
        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; }
        public virtual ICollection<IFoodItem> Menu { get; set; } = new HashSet<IFoodItem>();

        public Store() { }
        public Store(Brand brand)
        {
            BrandId = brand.BrandId;
            Menu = brand.BaseMenu;
        }
    }
}
