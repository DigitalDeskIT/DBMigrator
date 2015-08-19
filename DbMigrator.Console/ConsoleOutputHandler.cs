using DbMigrator.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMigrator.ConsoleApp
{
    class ConsoleOutputHandler : IMigrationRunnerOutputHandler
    {
        string progressStyle = "d";

        public ConsoleOutputHandler(string progressStyle = "d")
        {
            this.progressStyle = progressStyle;
        }

        string tab = "";

        public void EventBegin(string eventKey, string eventName, string info = null)
        {
            Console.WriteLine(string.Concat(tab, "Starting ", eventName,
                info != null ? string.Concat(" - ", info) : ""
            ));
            tab += "  ";
        }

        public void EventEnd(string eventKey, string eventName, string info = null)
        {
            tab = tab.Substring(2);
            Console.WriteLine(string.Concat(tab, "Completed ", eventName,
                info != null ? string.Concat(" - ", info) : ""
            ));
        }

        public void Info(string info)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(string.Concat(tab, info));
            Console.ResetColor();
        }

        private string dynamicLoadingString = "|--------------------|";
        private int dynamicLoadingSeed = 0;
        int lastPercent = -1;

        public void ProgressUpdate(string processName, int percent, string currentMessage)
        {
            if (progressStyle == "d")//dynamic
            {
                string currentLoadingString = dynamicLoadingString;
                dynamicLoadingSeed++;
                int start = dynamicLoadingSeed - 10;
                if (start < 1) { start = 1; }
                for (var i = start; i < dynamicLoadingSeed; i++)
                {
                    currentLoadingString = currentLoadingString.Remove(i + 1, 1).Insert(i, "o");
                }
                if (dynamicLoadingSeed > 19)
                {
                    dynamicLoadingSeed = 0;
                }

                string message = string.Concat(processName, ": ", percent, "%");
                if (percent < 100)
                {
                    Console.Write(string.Concat("\r", tab, currentLoadingString, " ", message));
                }
                else
                {
                    Console.Write(string.Concat("\r", tab, dynamicLoadingString, " ", message, "\n"));
                }
            }
            else if (progressStyle == "s")//simple
            {
                if (lastPercent != percent)
                {
                    Console.WriteLine("{0}{1}%", tab, percent);
                }
            }
            else if (progressStyle == "m")//minimal
            {
                if (percent == 0 && lastPercent != percent)
                {
                    Console.Write("{0}", tab);
                }
                else if (percent == 100)
                {
                    Console.Write(".{0}", Environment.NewLine);
                }
                else
                {
                    Console.Write(".");
                }

            }
            else
            {
                throw new NotImplementedException("Progress bar style not implemented.");
            }
            lastPercent = percent;
        }
    }
}
