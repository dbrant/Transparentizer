using System;
using System.Windows.Forms;

namespace Transparentizer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
                return;
            }

            int argIndex = 0;
            string inputFileName = "";
            string outputFileName = "";

            while (argIndex < args.Length)
            {
                if (args[argIndex] == "-o" && argIndex < args.Length - 1)
                {
                    outputFileName = args[argIndex + 1];
                    argIndex++;
                }
                else
                {
                    inputFileName = args[argIndex];
                }
                argIndex++;
            }

            if (inputFileName.Length == 0)
            {
                Console.WriteLine("Usage: transparentizer.exe [-o outputFileName.png] fileName.png");
                return;
            }

            Console.WriteLine("input: " + inputFileName);
            Console.WriteLine("output: " + outputFileName);

        }
    }
}
