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
using System.Threading;

namespace FFDConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            string ToolVersion = "1.0.1";
            string originalFFD = null;
            string fntBMF = null;
            string output = null;
            string version = null;
            bool show_help = false;
            List<string> SupportedGame = new()
                    {
                        "FC5", "FCP", "FC3"
                    };

            var p = new OptionSet() {
                { "v|version=", "(required) Name of game. (FC2,FC3,...)",
                   v => version = v  },
                { "f|originalFFD=",
                   "(required) Original FFD file (*.ffd|*.Fire_Font_Descriptor)",
                    v => originalFFD = v },
                { "b|bmfontDesc=",
                    "(required) Bmfont descriptor file (*.fnt)",
                    v => fntBMF = v },
                { "o|NewFFD=",
                   "(optional) New FFD file",
                    v => output = v },
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

            if (output == null)
            {
                output = originalFFD + ".new";
            }

            if (version == null || originalFFD == null || fntBMF == null || show_help)
            {
                ShowHelp(p);
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
                Console.WriteLine("Unknown BMFont file.");
                ShowHelp(p);
                return;
            }

            if (!SupportedGame.Contains(version))
            {
                Console.WriteLine("Unknown game.");
                Console.WriteLine("List:");
                foreach (string gamename in SupportedGame)
                {
                    Console.WriteLine("- " + gamename);
                }
                return;
            }
            // Change current culture
            CultureInfo culture;
            culture = CultureInfo.CreateSpecificCulture("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            FFDFormat.CreateFFD(originalFFD, fntBMF, output, version);
            Done();

            void ShowHelp(OptionSet p)
            {
                Console.Write("\nFFDConverter v" + ToolVersion);
                Console.WriteLine(" by eprilx");
                Console.Write("Check for more update: ");
                Console.WriteLine("https://github.com/eprilx/FFDConverter");
                Console.Write("Supported game: ");
                foreach (string game in SupportedGame)
                {
                    Console.Write(game + " ");
                }
                Console.WriteLine("\nUsage: FFDConverter [OPTIONS]");
                Console.WriteLine("Options:");
                p.WriteOptionDescriptions(Console.Out);
            }

            void Done()
            {
                Console.Write("\nFFDConverter v" + ToolVersion);
                Console.WriteLine(" by eprilx");
                Console.WriteLine("Done!");
            }
        }
    }
}
