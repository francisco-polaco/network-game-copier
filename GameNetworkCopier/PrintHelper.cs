using System;
using System.Collections.Generic;

namespace GameNetworkCopier
{
    internal static class PrintHelper
    {
        public static void printList(List<string> l)
        {
            String res = "[ ";
            foreach (var el in l)
            {
                res += el + ", ";
            }
            Console.WriteLine(res + "]");
        }

        public static void printArray(object[] l)
        {
            String res = "[ ";
            foreach (var el in l)
            {
                res += el + ", ";
            }
            Console.WriteLine(res + "]");
        }
    }
}