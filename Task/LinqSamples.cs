// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;
using Task.Helpers;

// Version Mad01

namespace SampleQueries
{
	[Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{

		private readonly DataSource dataSource = new DataSource();

        [Category("Restriction Operators")]
        [Title("Where - Task 1")]
        [Description("This sample return customers whose have total turnover more than X")]
        public void Linq1()
        {
            int numberOfExamples = 4;
            int maxTurnover = dataSource.Customers.Max(customers => (int)customers.Orders.Sum(order => order.Total));

            foreach (var total in Enumerable.Repeat(maxTurnover, numberOfExamples).ToList().Select(item => item/numberOfExamples--))
            {
                ObjectDumper.Write($"--{total}--");

                foreach (var p in dataSource.CustomersOrdersFilter(total))
                {
                    ObjectDumper.Write(p);
                }
            }
        }

        [Category("Restriction Operators")]
        [Title("Where - Task 3")]
        [Description("This sample return customers whose orders for an amount greater than 10000")]
        public void Linq3()
        {
            foreach (var p in dataSource.CustomersOrdersFilter(10000))
            {
                ObjectDumper.Write(p);
            }
        }

        [Category("Group Operators")]
        [Title("Join - Task 2")]
        [Description("This sample return suppliers for customer whose locate in the same country and city")]
        public void Linq2()
        {
            ObjectDumper.Write("--Groups--");
            foreach (var p in dataSource.Customers.Select(customer => new { City = customer.City, Country = customer.Country }).Intersect(dataSource.Suppliers.Select(supplier => new { City = supplier.City, Country = supplier.Country })))
            {              
                ObjectDumper.Write(p);
            }

            ObjectDumper.Write("--Join--");
            foreach (var p in dataSource.Customers.Join(dataSource.Suppliers
                    , customer => new { City = customer.City, Country = customer.Country }
                    , supplier => new { City = supplier.City, Country = supplier.Country }
                    , (customer, supplier) => new { CustomerName = customer.CompanyName, SupplierName = supplier.SupplierName, City = customer.City, Country = supplier.Country }))
            {
                ObjectDumper.Write(p);
            }
        }

        [Category("Group Operators")]
        [Title("Select - Task 4")]
        [Description("This sample return date when customer join company")]
        public void Linq4()
        {
            foreach (var p in dataSource.Customers.Select(customer => new { Name = customer.CompanyName,
                                                                            Date = customer.Orders.Length > 0 ? customer.Orders.Min(order => order.OrderDate) : DateTime.MinValue}))
            {
                ObjectDumper.Write(p);
            }
        }

        [Category("Group Operators")]
        [Title("Order - Task 5")]
        [Description("This sample return date when customer join company and group by Name, Date and Money flow")]
        public void Linq5()
        {
            foreach (var p in dataSource.Customers.Select(customer => new {
                Name = customer.CompanyName,
                Date = customer.Orders.Length > 0 ? customer.Orders.Min(order => order.OrderDate) : DateTime.MinValue,
                MoneyFlow = customer.Orders.Sum(order => order.Total)
            }).OrderBy(_ => _.Date).ThenByDescending(_ => _.MoneyFlow).ThenBy(_ => _.Name))
            {
                ObjectDumper.Write(p);
            }
        }

        [Category("Group Operators")]
        [Title("Select - Task 6")]
        [Description("This sample return customer with invalid postcode, unknown region or mobile operator code")]
        public void Linq6()
        {
            foreach (var p in dataSource.Customers.Where(customer => !customer.PostalCode?.All(char.IsDigit) ?? false || 
                                                                     string.IsNullOrEmpty(customer.Region) ||
                                                                     !customer.Phone.StartsWith("("))
            )
            {
                ObjectDumper.Write(p);
            }
        }

        [Category("Group Operators")]
        [Title("Group - Task 7")]
        [Description("This sample return grouped products by category and units in stock and sort it by price")]
        public void Linq7()
        {       
            foreach (var p in dataSource.Products.GroupBy(product => product.Category,(category, element) => new
                {
                    Category = category,
                    UnitsInStock = element.GroupBy(item => item.UnitsInStock, (units, products) => new
                                                            {
                                                                Count = units,
                                                                Products = products.OrderByDescending(p => p.UnitPrice).ToList()
                                                            }).ToList()
                }))
            {
                ObjectDumper.Write($"---{p.Category}---");

                foreach (var units in p.UnitsInStock)
                {
                    ObjectDumper.Write($"---{units.Count}---");

                    foreach (var product in units.Products)
                    {
                        ObjectDumper.Write(product);
                    }
                }
            }
        }

        [Category("Group Operators")]
        [Title("Group - Task 8")]
        [Description("This sample return grouped products by price category: «cheap», «average», «expensive»")]
        public void Linq8()
        {
            int averageLimit = 20;
            int expensiveLimit = 40;

            foreach (var p in dataSource.Products.GroupBy(product => product.UnitPrice < averageLimit ? "cheap" : 
                                                                     product.UnitPrice >= averageLimit && product.UnitPrice < expensiveLimit ? "average" : "expensive",
                                                                     (category, products) => new 
                                                                     {
                                                                        Category = category,
                                                                        Products = products.OrderBy(product => product.UnitPrice)
                                                                     }))
            {
                ObjectDumper.Write($"---{p.Category}---");

                foreach (var product in p.Products)
                {
                    ObjectDumper.Write(product);
                }
            }
        }

        [Category("Group Operators")]
        [Title("Group - Task 9")]
        [Description("This sample return average income by city and average intensity per client")]
        public void Linq9()
        {

            foreach (var p in dataSource.Customers.GroupBy(customer => customer.City, (city, customers) => new
            {
                City = city,
                Income = (int)customers.Average(customer => customer.Orders.Sum(order => order.Total)),
                Intensity = (int)customers.Average(customer => customer.Orders.Length)
            }))
            {
                ObjectDumper.Write(p);
            }
        }

        [Category("Group Operators")]
        [Title("Group - Task 10")]
        [Description("This sample return average annual activity statistics of clients by months, by year and by year and month")]
        public void Linq10()
        {
            var orders = dataSource.Customers.SelectMany(customer => customer.Orders);

            ObjectDumper.Write("---by Year and Month---");

            foreach (var p in orders.GroupBy(order => new { Year = order.OrderDate.ToString("yyyy"), Month = order.OrderDate.ToString("MM") },(date, count) => new
                {
                    DateTime = $"{date.Year}-{date.Month}",
                    CountOfOrders = count.Count()
                }).OrderBy(_ => _.DateTime))
            {
                ObjectDumper.Write(p);
            }

            ObjectDumper.Write("---by Year---");

            foreach (var p in orders.GroupBy(order => order.OrderDate.ToString("yyyy"), (year, count) => new
                {
                    Year = year,
                    CountOfOrders = count.Count()
                }).OrderBy(_ => _.Year))
            {
                ObjectDumper.Write(p);
            }

            ObjectDumper.Write("---by Month---");

            foreach (var p in orders.GroupBy(order => order.OrderDate.ToString("MMM"), (month, count) => new
                {
                    Month = month,
                    CountOfOrders = count.Count()
                }).OrderBy(_ => _.Month))
            {
                ObjectDumper.Write(p);
            }
        }
    }
}
