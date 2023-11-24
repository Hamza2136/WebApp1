using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Models.ViewModel
{
    public class CartVM
    {
        public IEnumerable<Cart> ListofCart { get; set; }
        public OrderHeader OrderHeader { get; set; }
    }
}
