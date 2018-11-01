using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxillaryQuestionAnswerHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            IEnumerable<string> everyOtherLine = System.IO.File.ReadAllLines(@"C:\Users\Justin\Desktop\wip.txt")
                                   .Where((s, i) => i % 3 == 0);

            IEnumerator<string> enumerator = everyOtherLine.GetEnumerator();

            IEnumerable<string> everyOtherLineDatabase = System.IO.File.ReadAllLines(@"C:\Users\Justin\Desktop\database2.txt")
                                   .Where((s, i) => i % 3 == 0);

            IEnumerator<string> enumeratorDatabase = everyOtherLineDatabase.GetEnumerator();

            StreamWriter writer = new StreamWriter(@"C:\Users\Justin\Desktop\temp.txt");
        
            int questionPosition = 0, enumeratorPosition = 0;

           /*#region fix improper quizlet
            IEnumerable<string> answerEnumerator = System.IO.File.ReadLines(@"C:\Users\Justin\Desktop\wip.txt");
            while (enumerator.MoveNext())
            {
                questionPosition = enumeratorPosition * 3 + 1;
                string question = answerEnumerator.Skip(questionPosition).First();
                string answer = enumerator.Current;
                writer.WriteLine(question);
                writer.WriteLine(answer);
                writer.WriteLine();
                enumeratorPosition++;
            }
           #endregion*/

            while (enumerator.MoveNext())
            {
                bool duplicate = false;
                questionPosition = enumeratorPosition * 3 + 1;
                while (enumeratorDatabase.MoveNext())
                {
                    if (AoDQuestionReader.CalculateSimilarity(enumerator.Current, enumeratorDatabase.Current) > .9)
                    {
                        Console.WriteLine("\nPOSSIBLE DUPLICATE:\n{0} // LINE {1}\n{2}", enumerator.Current, questionPosition, enumeratorDatabase.Current);
                        ConsoleKeyInfo key = Console.ReadKey();
                        if(key.KeyChar == 'y')
                        {
                            duplicate = true;
                            break;
                        }
                    }
                }

                enumeratorDatabase = everyOtherLineDatabase.GetEnumerator();

                if (!duplicate)
                {
                    IEnumerable<string> answerEnumerator = System.IO.File.ReadLines(@"C:\Users\Justin\Desktop\wip.txt");
                    string answer = answerEnumerator.Skip(questionPosition).First();
                    writer.WriteLine(enumerator.Current);
                    writer.WriteLine(answer);
                    writer.WriteLine();
                }
                enumeratorPosition++;
            }
            writer.Flush();
            writer.Dispose();
        }
    }
}
