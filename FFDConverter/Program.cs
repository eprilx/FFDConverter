using Mono.Options;
using System;
using System.Collections.Generic;

namespace FFDConverter
{
    class Program
    {
        static void Main(string[] args)
        {

            bool show_help = false;

            string ToolVersion = "0.0.1";
            string originalFFD = null;
            string charDesBMF = null;
            string output = null;
            string version = null;
            List<string> SupportedGame = new List<string>()
                    {
                        "FC5"
                        /*"FC2","FC3","FC4","FC5","FCND",
                        "AC2", "ACBr", "ACRe", "AC3", "AC4",
                        "WD1", "WD2"*/
                    };


            var p = new OptionSet() {
                { "v|version=", "(require) Name of game. (FC2,FC3,...)",
                   v => version = v  },
                { "f|originalFFD=",
                   "(require) Original FFD file (*.ffd)",
                    v => originalFFD = v },
                { "b|bmfontDesc=",
                    "(require) Bmfont descriptor file (*.fnt)",
                    v => charDesBMF = v },
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
            catch (OptionException e)
            {
                Console.WriteLine("Try 'FFDConverter --help' for more information.");
                return;
            }

            if (output == null)
            {
                output = originalFFD + ".new.ffd";
            }

            if (version == null || originalFFD == null || charDesBMF == null || show_help )
            {
                ShowHelp(p);
                return;
            }

            if(!originalFFD.EndsWith(".ffd"))
            {
                Console.WriteLine("Unknown FFD file.");
                return;
            }

            if (!originalFFD.EndsWith(".fnt"))
            {
                Console.WriteLine("Unknown Bmfont descriptor file.");
                return;
            }

            if (!SupportedGame.Contains(version))
            {
                Console.WriteLine("Unknown game.");
                Console.WriteLine("List:");
                foreach(string gamename in SupportedGame)
                {
                    Console.WriteLine("- " + gamename);
                }
                return;
            }

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

            (generalInfoBMF BMFinfo, List<charDescBMF> charDescList_, List<kernelDescBMF> kernelDescList) = BMFontFormat.LoadBMF(charDesBMF);

            List<charDescFFD> ffdDescList_ = FFDFormat.LoadFFD(originalFFD);
            foreach (charDescFFD item in ffdDescList_)
            {
                //Console.WriteLine(String.Format("first={0,-5} second={1,-5} amount={2,-5}", item.id, item.widthScale, item.heightScale));
            }


        }


    }
}
