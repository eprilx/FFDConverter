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
            try
            {
                ToolVersion = ToolVersion.Remove(ToolVersion.Length - 2);
            }
            catch
            {
                ToolVersion = "1.0.0";
            }
            string originalFFD = null;
            string fntBMF = null;
            string output = null;
            string version = null;
            bool show_list = false;
            string command = null;

            // Change current culture
            CultureInfo culture;
            culture = CultureInfo.CreateSpecificCulture("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            List<string> SupportedGame = DefaultConfig.GetSupportedList();

            var p = new OptionSet()
            {
                {"fnt2ffd", "Convert FNT to FFD",
                v => {command = "fnt2ffd"; } },
                {"ffd2fnt", "Convert FFD to FNT",
                v=> {command = "ffd2fnt"; } },
                { "l|list", "show list supported games",
                    v => show_list = v != null }
            };
            p.Parse(args);

            switch(command)
            {
                case "fnt2ffd":
                    p = new OptionSet() {
                { "v|version=", "(required) Name of game. (FC2,FC3,...)",
                   v => version = v  },
                { "f|originalFFD=", "(required) Original FFD file (*.ffd|*.Fire_Font_Descriptor)",
                    v => originalFFD = v },
                { "b|charDesc=", "(required) Character description file (*.fnt)",
                    v => fntBMF = v },
                { "o|NewFFD=",
                   "(optional) Output new FFD file",
                    v => output = v },
                };
                    break;
                case "ffd2fnt":
                    p = new OptionSet() {
                { "v|version=", "(required) Name of game. (FC2,FC3,...)",
                   v => version = v  },
                { "f|originalFFD=", "(required) Original FFD file (*.ffd|*.Fire_Font_Descriptor)",
                    v => originalFFD = v },
                { "o|NewFNT=",
                   "(optional) Output FNT file",
                    v => output = v },
                };
                    break;
            }
            p.Parse(args);


            if (show_list)
            {
                PrintSupportedGame();
                Console.ReadKey();
                return;
            }
            else if (args.Length == 0 || version == null || originalFFD == null || (fntBMF == null && command == "fnt2ffd"))
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

            if (command == "fnt2ffd")
            {
                if (!fntBMF.EndsWith(".fnt"))
                {
                    Console.WriteLine("Unknown character description file.");
                    ShowHelp(p);
                    return;
                }
            }

            // CreateFFD
            try
            {
                switch (command)
                {
                    case "fnt2ffd":
                        if (output == null)
                            output = originalFFD + ".new";
                        FNTtoFFD.CreateFFDfromFNT(originalFFD, fntBMF, output, version);
                        break;
                    case "ffd2fnt":
                        if (output == null)
                            output = originalFFD + ".FNT";
                        FFDtoFNT.ConvertFFDtoFNT(originalFFD, output, version);
                        break;
                }
                Done();
            }
            finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            
            void ShowHelp(OptionSet p)
            {
                switch (command)
                {
                    case "fnt2ffd":
                        Console.WriteLine("\nUsage: FFDConverter --fnt2ffd [OPTIONS]");
                        break;
                    case "ffd2fnt":
                        Console.WriteLine("\nUsage: FFDConverter --ffd2fnt [OPTIONS]");
                        break;
                    default:
                        PrintCredit();
                        Console.WriteLine("\nUsage: FFDConverter [OPTIONS]");
                        break;
                }
                
                Console.WriteLine("Options:");
                p.WriteOptionDescriptions(Console.Out);

                if (command == null)
                {
                    Console.WriteLine("\nExample: \nFFDConverter -l");
                    Console.WriteLine("FFDConverter --fnt2ffd -v FC5 -f fcz_bold_default.ffd -b arialFC5.fnt -o fcz_bold_default.new.ffd");
                    Console.WriteLine("FFDConverter --ffd2fnt -v FC5 -f fcz_bold_default.ffd -o fcz_bold_default.ffd.fnt");
                    Console.WriteLine("\nMore usage: https://github.com/eprilx/FFDConverter#usage");
                    Console.Write("More update: ");
                    Console.WriteLine("https://github.com/eprilx/FFDConverter/releases");
                }
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
                Console.WriteLine("abodora, rezamms, Eirlys, halfway, ramyzahran, shadow_lonely, Rick Gibbed");
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
