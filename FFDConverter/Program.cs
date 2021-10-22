/*
MIT License

Copyright (c) 2021 eprilx

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Mono.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FFDConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            string ToolVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            ToolVersion = ToolVersion.Remove(ToolVersion.Length - 2);
            string originalFFD = null;
            string fntBMF = null;
            string output = null;
            string version = null;
            bool show_help = false;
            bool show_list = false;
            List<string> SupportedGame = DefaultConfig.GetSupportedList();

            var p = new OptionSet() {
                { "v|version=", "(required) Name of game. (FC2,FC3,...)",
                   v => version = v  },
                { "f|originalFFD=", "(required) Original FFD file (*.ffd|*.Fire_Font_Descriptor)",
                    v => originalFFD = v },
                { "b|charDesc=", "(required) Character description file (*.fnt)",
                    v => fntBMF = v },
                { "o|NewFFD=",
                   "(optional) Output new FFD file",
                    v => output = v },
                { "l|list", "show list supported games",
                    v => show_list = v != null },
                { "h|help",  "show this message and exit",
                   v => show_help = v != null },
            };

            try
            {
                p.Parse(Environment.GetCommandLineArgs());
            }
            catch (OptionException)
            {
                Console.WriteLine("Try 'FFDConverter --help' for more information.");
                return;
            }

            if (show_list)
            {
                PrintSupportedGame();
                return;
            }
            else if (show_help || args.Length == 0 || version == null || originalFFD == null || fntBMF == null)
            {
                ShowHelp(p);
                return;
            }
            else if (SupportedGame.FirstOrDefault(x => x.Contains(version)) == null )
            {
                PrintSupportedGame();
                return;
            }

            if (!originalFFD.EndsWith(".ffd") && !originalFFD.EndsWith(".Fire_Font_Descriptor"))
            {
                Console.WriteLine("Unknown FFD file.");
                ShowHelp(p);
                return;
            }

            if (!fntBMF.EndsWith(".fnt"))
            {
                Console.WriteLine("Unknown character description file.");
                ShowHelp(p);
                return;
            }

            // Change current culture
            CultureInfo culture;
            culture = CultureInfo.CreateSpecificCulture("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            // CreateFFD
            if (output == null)
                output = originalFFD + ".new";
            FFDFormat.CreateFFD(originalFFD, fntBMF, output, version);
            Done();

            void ShowHelp(OptionSet p)
            {
                PrintCredit();
                Console.WriteLine("\nUsage: FFDConverter [OPTIONS]");
                Console.WriteLine("Options:");
                p.WriteOptionDescriptions(Console.Out);

                Console.WriteLine("\nExample: \nFFDConverter -l\nFFDConverter -v FC5 -f fcz_bold_default.ffd -b arialFC5.fnt -o fcz_bold_default.new.ffd");
                Console.WriteLine("\nMore usage: https://github.com/eprilx/FFDConverter#usage");
                Console.Write("More update: ");
                Console.WriteLine("https://github.com/eprilx/FFDConverter/releases");
            }

            void PrintSupportedGame()
            {
                Console.WriteLine("Supported games: ");
                foreach (string game in SupportedGame)
                {
                    Console.WriteLine(game);
                }
            }
            void PrintCredit()
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\nFFDConverter v" + ToolVersion);
                Console.WriteLine(" by eprilx");
                Console.Write("Special thanks to: ");
                Console.WriteLine("abodora, rezamms, Eirlys, halfway, ramyzahran");
                Console.ResetColor();
            }
            void Done()
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n" + output + " has been created!");
                Console.ResetColor();
                Console.Write("\n********************************************");
                PrintCredit();
                Console.WriteLine("********************************************");
            }
        }
    }
}
