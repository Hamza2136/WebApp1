using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.DataAccessLayer.Infrastructure.Repository
{
    public class OrderDetailsRepository : Repository<OrderDetails>, IOrderDetailsRepository
    {
        private ApplicationDbContext _context;
        public OrderDetailsRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderDetails orderDetails)
        {
            _context.OrderDetails.Update(orderDetails);
            //var categoryDb = _context.Categories.FirstOrDefault(x=>x.Id == category.Id);
            //if(categoryDb != null)
            //{
            //    categoryDb.Name = category.Name;
            //    categoryDb.DispalyOrder = category.DispalyOrder;
            //}
        }
    }
}
