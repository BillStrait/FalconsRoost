using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconsRoost.Models.Restaurant
{
    public class CustomizationOption
    {
        public int CustomizationOptionId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal PriceChange { get; set; }
        public bool IsCustomerRequest { get; set; } = false;

        public CustomizationOption() { }
        public CustomizationOption(string customerRequest)
        {
            Description = customerRequest;
            IsCustomerRequest = true;
        }
    }
}
