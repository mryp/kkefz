using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace PreKrkrCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                showUsage();
                return;
            }

            //アプリ名を持つプログラムをすべて強制終了する
            string exeName = args[0];
            int sleepTime = 0;
            string sleepTimeText = args[1];
            if (int.TryParse(sleepTimeText, out sleepTime) == false || sleepTime < 0)
            {
                showUsage();
                return;
            }

            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exeName));
            foreach (Process proc in processes)
            {
                try
                {
                    Console.WriteLine("Kill... " + exeName + "(" + proc.Id.ToString() + ") wait " + sleepTime.ToString() + "ms");
                    proc.Kill();
                    Thread.Sleep(sleepTime);
                }
                catch (System.NullReferenceException e)
                {
                    Console.WriteLine("ERROR... No instances of " + exeName + " running.");
                }
            }
        }

        private static void showUsage()
        {
            Console.WriteLine("Usage: KrkrPreCmd.exe EXENAME SLEEP_TIME");
            //Console.ReadKey();
        }
    }
}
