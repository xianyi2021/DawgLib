using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawgLib
{
    class Program
    {
        static void Main(string[] args)
        {
            Test();
            System.Console.ReadKey();
        }
        static void Test()
        {
            string[] words = { "top", "tops", "tap" };
            string[] words2 = { "top", "tops", "taps" };
            string[] words3 = { "top", "taps" };
            string[] query = { "top", "tops", "tap", "taps" };
            TestDawg("\r\nTestCase1", words, query);
            TestDawg("\r\nTestCase2", words2, query);
            TestDawg("\r\nTestCase3", words3, query);
        }

        static void TestDawg(string casename, string[] words, string[] query)
        {
            System.Console.WriteLine(casename);
            System.Console.WriteLine("Input Words:");
            foreach (string word in words)
                System.Console.Write(word + ",");
            System.Console.Write("\r\n");
            Dawg dawg = (new Dawg.Constructor()).Construct(words);
            System.Console.WriteLine("Query results:");
            foreach (string word in query)
            {
                System.Console.WriteLine(word + " " + dawg.Search(word));
            }
        }


    }
}
