using Microsoft.EntityFrameworkCore.Update.Internal;
using MyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.DataAccessLayer.Infrastructure.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        public void Update(OrderHeader orderHeader);
        public void UpdateStatus(int Id, string orderStatus, string? paymentStatus = null);
        public void PaymentStatus(int Id, string SessionId, string PaymentIntentId);

    }
}
