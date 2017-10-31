using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Task.Data;

namespace Task.Helpers
{
    public static class DataSourceHelper
    {
        public static IEnumerable<Customer> CustomersOrdersFilter(this DataSource dataSource, int total)
            => dataSource.Customers.Where(customer => customer.Orders.Sum(order => order.Total) >= total);
    }
}
