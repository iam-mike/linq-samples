using System;
using System.IO;
using System.Linq;
namespace Cars

{
    class Program
    {
        static void Main(string[] args)
        {
            var cars = ProcessFile("fuel.csv");

            var query = cars.OrderByDescending(c => c.Combined)
                            .ThenBy(c => c.Name);

            var query2 = from car in cars
                         where car.Manufacturer == "BMW" && car.Year == 2016
                         orderby car.Combined descending, car.Name ascending
                         select car;

            var top = cars.Where(c => c.Manufacturer == "BMW" && c.Year == 2016)
                          .OrderByDescending(c => c.Combined)
                          .ThenBy(c => c.Name)
                          .Select(c => c)
                          .First();

            Console.WriteLine(top.Name);
                         
            foreach (var car in query2.Take(10))
            {
                Console.WriteLine($"{car.Manufacturer} {car.Name} : {car.Combined}");
            }
        }

        private static System.Collections.Generic.List<Car> ProcessFile(string path)
        {
            var query =
                from line in File.ReadAllLines(path).Skip(1)
                where line.Length > 1
                select Car.ParseFromCsv(line);


                return query.ToList();
        }
    }
}
