using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_TPR4
{
    public class Candidate : IEquatable<Candidate>
    {
        private static Int32 count = 0;
        private Int32 id;
        public Int32 Id => id;
        public String Name { get; set; }

        public Candidate()
        {
            id = count;
            count++;
            Name = String.Format("[#{0}]", count);
        }

        public override Int32 GetHashCode()
        {
            return id.GetHashCode();
        }

        public Boolean Equals(Candidate other)
        {
            return other.id == id;
        }

        public override Boolean Equals(Object obj)
        {
            if (obj is Candidate)
            {
                return (obj as Candidate).id == id;
            }
            return false;
        }

        public override String ToString()
        {
            return Name;
        }
    }

    public class Vote : Dictionary<Candidate, Int32>, ICloneable
    {
        public Int32 VotesCount { get; set; }

        public Object Clone()
        {
            var result = new Vote();
            foreach (var pair in this)
            {
                result.Add(pair.Key, pair.Value);
            }
            return result;
        }

        public new void Remove(Candidate key)
        {
            base.Remove(key);
            var ordered = this
                .OrderBy(x => x.Value)
                .Select(x => x.Key);

            Int32 counter = 1; 
            foreach (var elem in ordered)
            {
                this[elem] = counter;
                counter++;
            }
        }

        public Candidate Best()
        {
            var max = this.Max(x => x.Value);
            return this.First(x => x.Value == max).Key;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Candidate a, b, c, d;
            Candidate[] candidates = new Candidate[]
            {
                a = new Candidate() { Name = "a" },
                b = new Candidate() { Name = "b" },
                c = new Candidate() { Name = "c" },
                d = new Candidate() { Name = "d" }
            };

            Vote v1 = new Vote() { VotesCount = 2 };
            v1[a] = 4;
            v1[b] = 3;
            v1[c] = 2;
            v1[d] = 1;

            Vote v2 = new Vote() { VotesCount = 6 };
            v2[a] = 3;
            v2[b] = 2;
            v2[c] = 4;
            v2[d] = 1;

            Vote v3 = new Vote() { VotesCount = 3 };
            v3[a] = 3;
            v3[b] = 4;
            v3[c] = 2;
            v3[d] = 1;

            Vote[] votes = new Vote[] { v1, v2, v3 };
            Console.WriteLine(TwoRounds(candidates, votes));
            Console.WriteLine(SimpsonGrade(candidates, votes));
            Console.WriteLine(ConsistentExclusion(candidates, votes));
            Console.WriteLine(Condorse(candidates, votes)?.ToString() ?? "No winner");
            AlternativeVotes(candidates, votes).ForEach(x => Console.Write(x));
            Console.WriteLine();
        }

        static Candidate TwoRounds(Candidate[] candidates, Vote[] votes)
        {
            var firstPlaceVotes =
                 from vote in votes
                 let max = vote.Max(x => x.Value)
                 let cand = vote.First(x => x.Value == max)
                 select new { Candidate = max, VotesCount = vote.VotesCount };

            var distinctVotes =
                from candidate in candidates
                select new
                {
                    Candidate = candidate,
                    VotesCount = firstPlaceVotes
                        .Where(x => x.Equals(candidate))
                        .Sum(x => x.VotesCount)
                };

            var roundWinner = distinctVotes.First(
                y => y.VotesCount == distinctVotes.Max(x => x.VotesCount)
            );
            //return roundWinner.Candidate;
            var votesCount = distinctVotes.Sum(x => x.VotesCount);
            if (roundWinner.VotesCount * 1.0 / votesCount > 0.5)
            {
                return roundWinner.Candidate;
            }

            var ordered = distinctVotes.OrderByDescending(x => x.VotesCount);
            var firstCandidate = (
                from vote in votes
                where vote[candidates[0]] > vote[candidates[1]]
                select vote.VotesCount
                ).Sum();

            var secondCandidate = votes.Sum(x => x.VotesCount) - firstCandidate;
            return firstCandidate > secondCandidate ? candidates[0] : candidates[1]; 
        }

        static Candidate SimpsonGrade(Candidate[] candidates, Vote[] votes)
        {
            var points = new Dictionary<Candidate, Int32>();
            foreach (var candToCheck in candidates)
            {
                foreach (var candidate in candidates)
                {
                    points[candToCheck] = votes
                        .Where(x => x[candidate] > x[candToCheck])
                        .Sum(x => x.VotesCount);
                }
            }

            var min = points.Min(x => x.Value);
            return points.First(x => x.Value == min).Key;
        }

        static Candidate ConsistentExclusion(Candidate[] candidates, Vote[] votes)
        {
            Int32 first = 0;
            Int32 second = 1;
            Int32 count = 2;
            var losers = new HashSet<Candidate>();
            while (losers.Count < candidates.Length - 1)
            {
                Int32 firstPoints = 0;
                Int32 secondPoints = 0;

                foreach (var vote in votes)
                {
                    firstPoints += vote[candidates[first]] > vote[candidates[second]] ? vote.VotesCount : 0;
                    secondPoints += vote.VotesCount - firstPoints;
                }

                if (firstPoints > secondPoints)
                {
                    losers.Add(candidates[second]);
                    second = count;
                }
                else
                {
                    losers.Add(candidates[first]);
                    first = count;
                }
                count++;
            }
            return candidates.First(x => !losers.Contains(x));
        }

        //////////////////////////////////////////////////////////////////////

        static Candidate RelativeMajority(Candidate[] candidates, Vote[] votes)
        {
            var firstPlaceVotes =
                 from vote in votes
                 let max = vote.Max(x => x.Value)
                 let cand = vote.First(x => x.Value == max)
                 select new { Candidate = max, VotesCount = vote.VotesCount };

            var distinctVotes =
                from candidate in candidates
                select new
                {
                    Candidate = candidate,
                    VotesCount = firstPlaceVotes
                        .Where(x => x.Equals(candidate))
                        .Sum(x => x.VotesCount)
                };

            var roundWinner = distinctVotes.First(
                y => y.VotesCount == distinctVotes.Max(x => x.VotesCount)
                );

            return roundWinner.Candidate;
        }

        static Candidate Condorse(Candidate[] candidates, Vote[] votes)
        {
            var winnersMatrix = new Boolean[candidates.Length][];
            for (Int32 i = 0; i < candidates.Length; i++)
            {
                winnersMatrix[i] = new Boolean[candidates.Length];
            } 

            for (Int32 i = 0; i < candidates.Length; i++)
            {
                for (Int32 j = i + 1; j < candidates.Length; j++)
                {
                    Int32 firstPoints = votes
                        .Where(x => x[candidates[i]] > x[candidates[j]])
                        .Sum(x => x.VotesCount);

                    Int32 secondPoints = votes
                        .Where(x => x[candidates[i]] > x[candidates[j]])
                        .Sum(x => x.VotesCount);

                    winnersMatrix[i][j] = firstPoints > secondPoints;
                    winnersMatrix[j][i] = firstPoints == secondPoints ? false : !winnersMatrix[i][j];
                }
            }

            for (Int32 i = 0; i < winnersMatrix.Length; i++)
            {
                if (winnersMatrix[i].All(x => x == true))
                {
                    return candidates[i];
                }
            }
            return null;
        }

        static Candidate[] AlternativeVotes(Candidate[] candidates, Vote[] votes)
        {
            var clonedVotes = votes.Select(x => x.Clone() as Vote);
            var losers = new HashSet<Candidate>();
            IEnumerable<Int32> points;
            Int32 min;

            do
            {
                var candidatePoints =
                    from x in candidates
                    where !losers.Contains(x)
                    select new
                    {
                        Candidate = x,
                        Points = votes.Sum(vote => vote.Best() == x ? vote.VotesCount : 0)
                    };

                min = candidatePoints.Min(x => x.Points);
                points = candidatePoints.Select(x => x.Points).ToArray();

                candidatePoints
                    .Where(x => x.Points == min)
                    .ForEach(x =>
                    {
                        losers.Add(x.Candidate);
                        foreach (var vote in clonedVotes)
                        {
                            vote.Remove(x.Candidate);
                        }
                    });
            } while (points.All(x => x != min));

            return candidates.Where(x => !losers.Contains(x)).ToArray();
        }
    }

    public static class EnumerableOperations
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var elem in enumerable) { action(elem); }
        }
    }
}
