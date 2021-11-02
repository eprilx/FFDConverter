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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FFDConverter
{
    public struct Config
    {
        public float scaleXoffset;
        public float scaleYoffset;
        public float scaleWidth;
        public float scaleHeight;
        public float scaleXadvance;
        public int addCustomYoffset;
        public int unkHeader1;
        public int unkHeader2;
        public int unkHeader3;
        public int unkHeaderAC;
    }
    public class DefaultConfig
    {
        public static Config Get(string versionGame)
        {
            Config config = new();
            XDocument xmlDoc = new();
            try
            {
                xmlDoc = XDocument.Load(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),"config.xml"));
            }
            catch (XmlException e)
            {
                throw new InvalidOperationException("config.xml file: " + e.Message);
            }
            XElement xmlRoot = xmlDoc.Element("config");

            IEnumerable<XElement> xmlStringsGame = xmlRoot.Elements("game");
            foreach (XElement xmlString in xmlStringsGame)
            {
                string nameGame = xmlString.Attribute("name").Value;
                if (nameGame.Contains(versionGame))
                {
                    config.scaleXoffset = float.Parse(xmlString.Attribute("scaleXoffset").Value);
                    config.scaleYoffset = float.Parse(xmlString.Attribute("scaleYoffset").Value);
                    config.scaleWidth = float.Parse(xmlString.Attribute("scaleWidth").Value);
                    config.scaleHeight = float.Parse(xmlString.Attribute("scaleHeight").Value);
                    config.scaleXadvance = float.Parse(xmlString.Attribute("scaleXadvance").Value);
                    config.addCustomYoffset = int.Parse(xmlString.Attribute("addCustomYoffset").Value);
                    config.unkHeader1 = int.Parse(xmlString.Attribute("unkHeader1").Value);
                    config.unkHeader2 = int.Parse(xmlString.Attribute("unkHeader2").Value);
                    config.unkHeader3 = int.Parse(xmlString.Attribute("unkHeader3").Value);
                    config.unkHeaderAC = int.Parse(xmlString.Attribute("unkHeaderAC").Value);

                    break;
                }
            }
            return config;
        }

        public static void Set(string versionGame, Config config)
        {
            XDocument xmlDoc = new();
            string configPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.xml");
            try
            {
                xmlDoc = XDocument.Load(configPath);
            }
            catch (XmlException e)
            {
                throw new InvalidOperationException("config.xml file: " + e.Message);
            }
            XElement xmlRoot = xmlDoc.Element("config");

            IEnumerable<XElement> xmlStringsGame = xmlRoot.Elements("game");
            foreach (XElement xmlString in xmlStringsGame)
            {
                string nameGame = xmlString.Attribute("name").Value;
                if (nameGame.Contains(versionGame))
                {
                    xmlString.Attribute("scaleXoffset").Value = config.scaleXoffset.ToString();
                    xmlString.Attribute("scaleYoffset").Value = config.scaleYoffset.ToString();
                    xmlString.Attribute("scaleWidth").Value = config.scaleWidth.ToString();
                    xmlString.Attribute("scaleHeight").Value = config.scaleHeight.ToString();
                    xmlString.Attribute("scaleXadvance").Value = config.scaleXadvance.ToString();
                    xmlString.Attribute("addCustomYoffset").Value = config.addCustomYoffset.ToString();
                    break;
                }
            }
            xmlDoc.Save(configPath);
        }

        public static List<string> GetSupportedList()
        {
            List<string> SupportedGame = new();
            XDocument xmlDoc = new();
            try
            {
                xmlDoc = XDocument.Load(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.xml"));
            }
            catch (XmlException e)
            {
                throw new InvalidOperationException("config.xml file: " + e.Message);
            }
            XElement xmlRoot = xmlDoc.Element("config");

            IEnumerable<XElement> xmlStringsGame = xmlRoot.Elements("game");
            foreach (XElement xmlString in xmlStringsGame)
                SupportedGame.Add(xmlString.Attribute("name").Value);
            return SupportedGame;
        }
    }
}
