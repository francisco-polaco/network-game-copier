using System;
using System.Collections.Generic;
using System.Windows;

namespace GameNetworkCopier
{
    internal static class PrintHelper
    {
        public static void PrintList(List<string> l)
        {
            String res = "[ ";
            foreach (var el in l)
            {
                res += el + ", ";
            }
            Console.WriteLine(res + "]");
        }

        public static void PrintArray(object[] l)
        {
            String res = "[ ";
            foreach (var el in l)
            {
                res += el + ", ";
            }
            Console.WriteLine(res + "]");
        }
    }

    public class AsyncPack
    {
        public Window Window { get; set; }
        public Delegate ToExecute { get; set; }
    }
}