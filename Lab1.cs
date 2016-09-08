using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var decisionList = 
                from x in GetTask(2)
                select (Decision)x;

            // Поиск решения, оптимального за Парето
            var paretoResult = 
                from xs in decisionList
                where !decisionList.Any(x => x >= xs)
                select xs;

            // Поиск решения, оптимального за Слейтером
            var slaterResult = 
                from xs in decisionList
                where !decisionList.Any(x => x > xs)
                select xs;

            PrintResult(paretoResult, slaterResult);
        }
        
        /// Функция вывода результата
        public static void PrintResult<T>(
            IEnumerable<T> pareto,
            IEnumerable<T> slaterResult)
        {
            Console.WriteLine("--PARETO--");
            pareto.ToList()
                .ForEach(x => Console.WriteLine(x));

            Console.WriteLine("--SLATER--");
            slaterResult.ToList()
                .ForEach(x => Console.WriteLine(x));
        }

        // Получить массив двухцифровых чисел по варианту
        public static Int32[] GetTask(Int32 variant)
        {
            return (new Int32[][] 
            {
                new Int32[] { 05, 04, 33, 99, 34, 19, 15, 35, 87, 53, 69, 50, 87, 02, 37, 62, 89, 10, 05, 60 },
                new Int32[] { 04, 11, 57, 29, 03, 16, 92, 21, 22, 05, 43, 79, 61, 28, 78, 47, 51, 45, 82, 87 },
                new Int32[] { 99, 51, 89, 86, 53, 26, 48, 94, 36, 06, 07, 92, 17, 64, 21, 20, 80, 66, 94, 54 }
            })[variant];
        }
    }


    public struct Decision
    {
        private readonly Int32 alternative1;
        private readonly Int32 alternative2;

        public Int32[] Alternatives => new Int32[] { alternative1, alternative2 };

        public Decision(Int32 alt)
        {
            alternative1 = alt / 10;
            alternative2 = alt % 10; 
        }

        // Перегрузка оператора преобразования из Int в Decision
        public static implicit operator Decision(Int32 num)
        {
            return new Decision(num);
        }

        // Перегрузка операторов сравнения >, >=, <, <=
        public static Boolean operator >(Decision a, Decision b)
        {
            return a.alternative1 > b.alternative1 && a.alternative2 > b.alternative2;
        }

        public static Boolean operator <(Decision a, Decision b)
        {
            return a.alternative1 < b.alternative1 && a.alternative2 < b.alternative2;
        }

        public static Boolean operator >=(Decision a, Decision b)
        {
            Boolean k = a.alternative1 >= b.alternative1 && a.alternative2 > b.alternative2;
            Boolean m = a.alternative1 > b.alternative1 && a.alternative2 >= b.alternative2;
            return k || m;
        }

        public static Boolean operator <=(Decision a, Decision b)
        {
            Boolean k = a.alternative1 <= b.alternative1 && a.alternative2 < b.alternative2;
            Boolean m = a.alternative1 < b.alternative1 && a.alternative2 <= b.alternative2;
            return k || m;
        }

        public override String ToString()
        {
            return String.Format("[{0} {1}]", alternative1, alternative2);
        }
    }
}
