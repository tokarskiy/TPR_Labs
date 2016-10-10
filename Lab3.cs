using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPR_Lab3
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Choose the line: ");
            var goodsList = GetVariant(Convert.ToInt32(Console.ReadLine()));
        actionMoment:
            Console.Write("> ");
            var command = Console.ReadLine().Split(' ');
            var param = command.Length > 1 ? command[1] : "";
            var start = DateTime.Now;
            switch(command.First())
            {
                case "NFA":
                    NFA(param == "sort" ? goodsList.OrderBy(x => x.Weight).ToArray() : goodsList).Output();
                    Console.WriteLine("Время: {0}", (DateTime.Now - start).Ticks);
                    goto actionMoment;
                case "FFA":
                    FFA(param == "sort" ? goodsList.OrderBy(x => x.Weight).ToArray() : goodsList).Output();
                    Console.WriteLine("Время: {0}", (DateTime.Now - start).Ticks);
                    goto actionMoment;
                case "WFA":
                    WFA(param == "sort" ? goodsList.OrderBy(x => x.Weight).ToArray() : goodsList).Output();
                    Console.WriteLine("Время: {0}", (DateTime.Now - start).Ticks);
                    goto actionMoment;
                case "BFA":
                    BFA(param == "sort" ? goodsList.OrderBy(x => x.Weight).ToArray() : goodsList).Output();
                    Console.WriteLine("Время: {0}", (DateTime.Now - start).Ticks);
                    goto actionMoment;
                default:
                    goto actionMoment;
            }
        }

        static Goods[] GetVariant(Int32 mode)
        {
            var variants = new Goods[][]
            {
                new Goods[] { 13, 91, 38, 70, 21, 87, 29, 71, 80, 43, 95, 99, 24, 88, 54, 86, 69, 32, 69, 10 },
                new Goods[] { 73, 30, 33, 63, 87, 79, 94, 49, 99, 51, 39, 64, 42, 30, 86, 15, 49, 15, 86, 81 },
                new Goods[] { 11, 34, 33, 87, 22, 87, 73, 43, 19, 42, 54, 44, 24, 39, 59, 63, 18, 53, 12, 69 },
            };

            Goods[] result;

            if (mode == 3)
            {
                result = variants[0].Concat(variants[1]).Concat(variants[2]).ToArray();
            }
            else
            {
                result = variants[mode].ToArray();
            }

            for (Int32 i = 0; i < result.Length; i++)
            {
                result[i].Id = i;
            }
            return result;
        }

        static Int32[,] NFA(Goods[] goodsList)
        {
            var containers = new List<Container>();
            foreach (var elem in goodsList)
            {
                if (containers.Count == 0)
                {
                    containers.Add(new Container());
                }
                else if (!containers.Last().CanBeFilled(elem))
                {
                    containers.Add(new Container());
                }
                containers.Last().Add(elem);
            }

            var result = new Int32[containers.Count, goodsList.Length];
            Int32 i = 0;
            foreach (var container in containers)
            {
                foreach (var goods in container)
                {
                    result[i, goods.Id] = goods.Weight;
                }
                i++;
            }
            return result;
        }

        static Int32[,] FFA(Goods[] goodsList)
        {
            var containers = new List<Container>();
            foreach (var elem in goodsList)
            {
                Boolean added = false;
                if (containers.Count == 0)
                {
                    containers.Add(new Container() { elem });
                }
                else
                {
                    if (containers.Last().CanBeFilled(elem))
                    {
                        containers.Last().Add(elem);
                        added = true;
                    }

                    if (!added)
                    {
                        foreach (var container in containers)
                        {
                            if (container.CanBeFilled(elem))
                            {
                                container.Add(elem);
                                added = true;
                                break;
                            }
                        }
                    }

                    if (!added)
                    {
                        containers.Add(new Container() { elem });
                    }
                }
            }


            var result = new Int32[containers.Count, goodsList.Length];
            Int32 i = 0;
            foreach (var conteiner in containers)
            {
                foreach (var goods in conteiner)
                {
                    result[i, goods.Id] = goods.Weight;
                }
                i++;
            }

            return result;
        }
        
        static Int32[,] WFA(Goods[] goodsList)
        {
            var containers = new List<Container>();
            foreach (var elem in goodsList)
            {
                if (containers.Count == 0)
                {
                    containers.Add(new Container() { elem });
                }
                else
                {
                    var orderedContainers = containers
                        .Where(x => x.CanBeFilled(elem))
                        .OrderBy(x => x.FreeWeight);

                    if (orderedContainers.Count() == 0)
                    {
                        containers.Add(new Container() { elem });
                    }
                    else
                    {
                        orderedContainers.LastOrDefault().Add(elem);
                    }
                }
            }

            var result = new Int32[containers.Count, goodsList.Length];
            Int32 i = 0;
            foreach (var conteiner in containers)
            {
                foreach (var goods in conteiner)
                {
                    result[i, goods.Id] = goods.Weight;
                }
                i++;
            }

            return result;
        }

        static Int32[,] BFA(Goods[] goodsList)
        {
            var containers = new List<Container>();
            foreach (var elem in goodsList)
            {
                if (containers.Count == 0)
                {
                    containers.Add(new Container() { elem });
                }
                else
                {
                    var orderedContainers = containers
                        .Where(x => x.CanBeFilled(elem))
                        .OrderBy(x => x.FreeWeight);

                    if (orderedContainers.Count() == 0)
                    {
                        containers.Add(new Container() { elem });
                    }
                    else
                    {
                        orderedContainers.FirstOrDefault().Add(elem);
                    }
                }
            }

            var result = new Int32[containers.Count, goodsList.Length];
            Int32 i = 0;
            foreach (var conteiner in containers)
            {
                foreach (var goods in conteiner)
                {
                    result[i, goods.Id] = goods.Weight;
                }
                i++;
            }

            return result;
        }

    }

    public class Container : List<Goods>
    {
        private Int32 maxWeight = 100;

        public Int32 MaxWeight => maxWeight;
        public Int32 TotalWeight => this.Sum(x => x.Weight);
        public Int32 FreeWeight => maxWeight - this.TotalWeight;

        public Boolean CanBeFilled(Goods goods)
        {
            return maxWeight - TotalWeight >= goods.Weight;
        }
    }

    public class Goods
    {
        public Int32 Weight { get; set; }
        public Int32 Id { get; set; }

        public Goods(Int32 weight)
        {
            Weight = weight;
        }

        public static implicit operator Goods(Int32 weight)
        {
            return new Goods(weight);
        }
    }

    public static class ResultActions
    {
        public static void Output(this Int32[,] result)
        {
            var width = result.GetLength(1);
            var height = result.GetLength(0);

            Console.WriteLine("Количество контейнеров: {0}", height);
            /*
            for (Int32 i = 0; i < height; i++)
            {
                for (Int32 j = 0; j < width; j++)
                {
                    Console.Write($"{result[i, j],2} ");
                }
                Console.WriteLine();
            }
            */
        }
    }
}
