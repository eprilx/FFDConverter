using Mono.Options;
using System;
using System.Collections.Generic;

namespace FFDConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            string ToolVersion = "0.0.2";
            string originalFFD = null;
            string fntBMF = null;
            string output = null;
            string version = null;
            bool show_help = false;
            List<string> SupportedGame = new()
                    {
                        "FC5"
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

            FFDFormat.CreateFFD(originalFFD, fntBMF, output);
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
