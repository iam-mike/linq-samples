using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;

namespace Cars
{
    class Program
    {
        static void Main(string[] args)
        {
            // DEMOING FUNC VS EXPRESSION
            //Func<int, int> square = x => x * x;
            //Expression<Func<int, int, int>> add = (x, y) => x + y; //cannot be executed
            //Func<int, int, int> addI = add.Compile(); // how to execute Expression

            //var resultSq = square(9);
            //var resultAdd = addI(3, 14);


            //Console.WriteLine(square);
            //Console.WriteLine(resultSq);
            //Console.WriteLine(add);
            //Console.WriteLine(resultAdd);
            
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CarDb>());
            InsertData();
            QueryData();
        }

        private static void QueryData()
        {
            var db = new CarDb();
            
            db.Database.Log = Console.WriteLine;

            var query =
                from car in db.Cars
                group car by car.Manufacturer into manufacturer
                select new
                {
                    Name = manufacturer.Key,
                    Cars = (from car in manufacturer
                            orderby car.Combined descending
                            select car).Take(2)
                };

                //db.Cars.GroupBy(c => c.Manufacturer)
                //        .Select(g => new
                //        {
                //            Name = g.Key,
                //            Cars = g.OrderByDescending(c => c.Combined)
                //            .Take(2)
                //        }) ;

            //.Where(c => c.Manufacturer == "BMW")
            //.OrderByDescending(c => c.Combined)
            //.ThenBy(c => c.Name)
            //.Select(c => new { Name = c.Name.ToUpper()})
            //.Take(10)
            //.ToList(); // changes from IQueryable to IEnumerable so shifts this to inmemory;


            //from car in db.Cars
            //        orderby car.Combined descending, car.Name ascending
            //        select car;

            foreach (var group in query)
            {
                Console.WriteLine(group.Name);
                foreach (var car in group.Cars)
                {
                    Console.WriteLine($"\t{car.Name} : {car.Combined}");
                }
            }

            //foreach (var car in query.Take(10))
            //{
            //    Console.WriteLine(car.Name);
            //}

        }

        private static void InsertData()
        {
            var cars = ProcessCars("fuel.csv");
            var db = new CarDb();
            //db.Database.Log = Console.WriteLine;
            if (!db.Cars.Any())
            {
                foreach(var car in cars)
                {
                    db.Cars.Add(car);
                }
                db.SaveChanges();
            }
        }

        private static void QueryXML()
        {
            var ns = (XNamespace)"http://pluralsight.com/cars/2016";
            var ex = (XNamespace)"http://pluralsight.com/cars/2016/ex";
            var doc = XDocument.Load("fuel.xml");

            var query =
                from element in doc.Element(ns + "Cars")?.Elements(ex + "Car") 
                                                        ?? Enumerable.Empty<XElement>()
                where element.Attribute("Manufacturer")?.Value == "BMW"
                select element.Attribute("Name")?.Value;

            var query2 =
                from element in doc.Descendants(ex + "Car")
                where element.Attribute("Manufacturer")?.Value == "BMW"
                select element.Attribute("Name")?.Value;

            foreach (var name in query)
            {
                Console.WriteLine(name);
            }

        }

        private static void CreateXML()
        {
            var records = ProcessCars("fuel.csv");

            var ns = (XNamespace)"http://pluralsight.com/cars/2016";
            var ex = (XNamespace)"http://pluralsight.com/cars/2016/ex";
            var document = new XDocument();
            var cars = new XElement(ns + "Cars",

                from record in records
                select new XElement(ex + "Car",
                                new XAttribute("Name", record.Name),
                                new XAttribute("Combined", record.Combined),
                                new XAttribute("Manufacturer", record.Manufacturer))
                );

            cars.Add(new XAttribute(XNamespace.Xmlns + "ex", ex));

            //foreach(var record in records)
            //{
            //    var car = new XElement("Car",
            //                    new XAttribute("Name", record.Name),
            //                    new XAttribute("Combined", record.Combined),
            //            new XAttribute("Manufacturer", record.Manufacturer));
            //            cars.Add(car);
            //}
            document.Add(cars);
            document.Save("fuel.xml");
        }

        private static List<Manufacturer> ProcessManufacturers(string path)
        {
            var query =
                File.ReadAllLines(path)
                .Where(l => l.Length > 1)
                .Select(l =>
                {
                    var columns = l.Split(",");
                    return new Manufacturer
                    {
                        Name = columns[0],
                        Headquarters = columns[1],
                        Year = int.Parse(columns[2])
                    };
                });
            return query.ToList();
        }

        private static List<Car> ProcessCars(string path)
        {
            var query =

                File.ReadAllLines(path)
                    .Skip(1)
                    .Where(l => l.Length > 1)
                    .ToCar();

            //from line in File.ReadAllLines(path).Skip(1)
            //where line.Length > 1
            //select Car.ParseFromCsv(line);


            return query.ToList();
        }
    }
    public static class CarExtensions
    {
        public static IEnumerable<Car> ToCar(this IEnumerable<string> source)
        {
            foreach (var line in source)
            {
                var columns = line.Split(',');

                yield return new Car
                {
                    Year = int.Parse(columns[0]),
                    Manufacturer = columns[1],
                    Name = columns[2],
                    Displacement = double.Parse(columns[3]),
                    Cylinders = int.Parse(columns[4]),
                    City = int.Parse(columns[5]),
                    Highway = int.Parse(columns[6]),
                    Combined = int.Parse(columns[7])
                };
            }
        }


    }

    public class CarStatistics
    {
        public CarStatistics()
        {
            Max = Int32.MaxValue;
            Min = Int32.MaxValue;
        }
        public CarStatistics Accumulate(Car car)
        {
            Count++;
            Total += car.Combined;
            Max = Math.Max(Max, car.Combined);
            Min = Math.Min(Min, car.Combined);
            return this;
        }

        public CarStatistics Compute()
        {
            Average = Total / Count;
            return this;
        }

        public int Max { get; set; }
        public int Min { get; set; }
        public int Total { get; set; }
        public int Count { get; set; }
        public double Average { get; set; }

    }
}
