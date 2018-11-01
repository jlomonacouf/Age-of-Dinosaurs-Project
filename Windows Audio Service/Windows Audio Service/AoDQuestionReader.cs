using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Audio_Service
{
    class AoDQuestionReader
    {
        private string sourceFile = @"C:\Users\Justin\Desktop\database2.txt";
        public string[] findQuestionAnswer(string question)
        {
            string[] questionAnswer = new string[2];

            #region Finds most relevant question and its position
            IEnumerable<string> everyOtherLine = System.IO.File.ReadAllLines(sourceFile)
                                   .Where((s, i) => i % 3 == 0);

            IEnumerator<string> enumerator = everyOtherLine.GetEnumerator();

            string bestFitQuestion = "";
            double bestFitRatio = 0;
            int questionPosition = 0, enumeratorPosition = 0;

            while (enumerator.MoveNext())
            {
                double ratio = CalculateSimilarity(enumerator.Current, question);
                enumeratorPosition += 1;
                if (ratio > bestFitRatio)
                {
                    bestFitQuestion = enumerator.Current;
                    questionPosition = enumeratorPosition * 3 - 1;
                    bestFitRatio = ratio;
                }
            }
            #endregion

            questionAnswer[0] = bestFitQuestion;
            questionAnswer[1] = findAnswer(questionPosition);

            return questionAnswer;
        }

        private string findAnswer(int questionPosition)
        {
            IEnumerable<string> enumerator = System.IO.File.ReadLines(sourceFile);
            return enumerator.Skip(questionPosition - 1).First();
        }

        private int ComputeLevenshteinDistance(string source, string target)
        {
            if ((source == null) || (target == null)) return 0;
            if ((source.Length == 0) || (target.Length == 0)) return 0;
            if (source == target) return source.Length;

            int sourceWordCount = source.Length;
            int targetWordCount = target.Length;

            // Step 1
            if (sourceWordCount == 0)
                return targetWordCount;

            if (targetWordCount == 0)
                return sourceWordCount;

            int[,] distance = new int[sourceWordCount + 1, targetWordCount + 1];

            // Step 2
            for (int i = 0; i <= sourceWordCount; distance[i, 0] = i++) ;
            for (int j = 0; j <= targetWordCount; distance[0, j] = j++) ;

            for (int i = 1; i <= sourceWordCount; i++)
            {
                for (int j = 1; j <= targetWordCount; j++)
                {
                    // Step 3
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;

                    // Step 4
                    distance[i, j] = Math.Min(Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1), distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceWordCount, targetWordCount];
        }

        public double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = ComputeLevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }
    }
}
