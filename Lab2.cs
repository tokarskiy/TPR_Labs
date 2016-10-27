using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Lab_TPR2
{
    class Program
    {
        static void Main(string[] args)
        {
            var probabilities = new Double[12];/*
            for (Int32 i = 0; i < probabilities.Length; i++)
            {
                probabilities[i] = 1.0 / 12;
            }*/
            probabilities[8] = 1.0 / 3;
            probabilities[9] = 1.0 / 3;
            probabilities[10] = 1.0 / 3;

            Tour tour = new Tour("data.json");
            var suits = tour.Suits;

            foreach (var suit in suits)
            {
                Console.WriteLine($"{suit}: {tour.MakeDecision(suit, probabilities)}");
            }
        }
    }

    public class Clothing
    {
        public String Name { get; set; }
        public Double Weight { get; set; }
        public Double Price { get; set; }

        public override String ToString()
        {
            return $"{Name}[Weight: {Weight}; Price: {Price}]";
        }
    }

    public class Suit : List<Clothing>
    {
        public Suit() { }
        public Suit(IEnumerable<Clothing> clothings)
        {
            StringBuilder name = new StringBuilder();
            foreach (var clothing in clothings)
            {
                Add(clothing);
                name.Append($"{clothing.Name} ");
            }
            Name = name.ToString();
        }

        public String Name { get; set; }
        public Int32 MinTemperature { get; set; }
        public Int32 MaxTemperature { get; set; }

        public Double TransportCost => this.Sum(x => x.Weight) * 10;
        public override String ToString() => Name;

        public Boolean TemperatureSuits(Int32 temperature)
        {
            return MinTemperature <= temperature && MaxTemperature >= temperature;
        }
    }

    public class Tour
    {
        private Clothing[] clothings;
        private Suit[] suits;
        private Int32[] temperatures;

        public IReadOnlyList<Suit> Suits => suits.ToArray();
        public IReadOnlyList<Clothing> Clothings => clothings.ToArray();

        public Tour(String fileName)
        {
            var data = JObject.Parse(File.ReadAllText(fileName));

            clothings = (
                from clothing in data["clothings"]
                let name = (String)clothing["name"]
                let price = (Double)clothing["price"]
                let weight = (Double)clothing["weight"]
                select new Clothing
                {
                    Name = name,
                    Price = price,
                    Weight = weight
                }).ToArray();

            suits = (
                from suit in data["suits"]
                let minTemperature = (Int32)suit["minTemperature"]
                let maxTemperature = (Int32)suit["maxTemperature"]
                let clothingsList = from clothing in suit["clothes"]
                                    select clothings.First(x => x.Name == (String)clothing)
                select new Suit(clothingsList)
                {
                    MinTemperature = minTemperature,
                    MaxTemperature = maxTemperature
                }).ToArray();

            temperatures = data["temperatures"].Select(x => (Int32)x).ToArray();
        }

        public Double MakeDecision(Suit start, Double[] probabilities)
        {
            Double total = 0;
            for (Int32 i = 0; i < temperatures.Count(); i++)
            {
                Double local = 0;
                local += start.TransportCost;

                Suit suit = suits.First(x => x.TemperatureSuits(temperatures[i]));

                foreach (var clothing in suit)
                {
                    if (!start.Contains(clothing))
                    {
                        local += clothing.Price + 2.0;
                    }
                }
                total += local * probabilities[i];
            }
            return total;
        }
    }
}
