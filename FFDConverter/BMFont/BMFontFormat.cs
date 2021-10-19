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
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace FFDConverter
{
    class BMFontFormat
    {
        public static (generalInfoBMF, List<charDescBMF>, List<kernelDescBMF>) LoadBMF(string inputBMF)
        {
            try
            {
                XDocument.Load(inputBMF);
            }
            catch (XmlException)
            {
                return LoadTextBMF(inputBMF);
            }
            return LoadXMLBMF(inputBMF);
        }

        private static (generalInfoBMF, List<charDescBMF>, List<kernelDescBMF>) LoadXMLBMF(string inputBMF)
        {
            List<charDescBMF> charDescList = new();
            List<kernelDescBMF> kernelDescList = new();
            generalInfoBMF BMFinfo = new();

            XDocument xmlDoc = XDocument.Load(inputBMF);
            XElement xmlRoot = xmlDoc.Element("font");

            IEnumerable<XElement> _xmlStringsCommon = xmlRoot.Elements("common");
            foreach (XElement xmlString in _xmlStringsCommon)
            {
                BMFinfo.lineHeight = int.Parse(xmlString.Attribute("lineHeight").Value);
                BMFinfo._base = int.Parse(xmlString.Attribute("base").Value);
                BMFinfo.WidthImg = int.Parse(xmlString.Attribute("scaleW").Value);
                BMFinfo.HeightImg = int.Parse(xmlString.Attribute("scaleH").Value);
            }

            IEnumerable<XElement> _xmlStringinfo = xmlRoot.Elements("info");
            foreach (XElement xmlString in _xmlStringinfo)
            {
                BMFinfo.face = (string)xmlString.Attribute("face").Value;
                BMFinfo.size = int.Parse(xmlString.Attribute("size").Value);
                BMFinfo.bold = int.Parse(xmlString.Attribute("bold").Value);
                BMFinfo.italic = int.Parse(xmlString.Attribute("italic").Value);
            }

            IEnumerable<XElement> xmlSections = xmlRoot.Elements("chars");
            BMFinfo.charsCount = int.Parse(xmlSections.First().Attribute("count").Value);
            foreach (XElement xmlSection in xmlSections)
            {
                IEnumerable<XElement> xmlStrings = xmlSection.Elements("char");
                foreach (XElement xmlString in xmlStrings)
                {
                    charDescList.Add(new charDescBMF
                    {
                        id = int.Parse(xmlString.Attribute("id").Value),
                        x = int.Parse(xmlString.Attribute("x").Value),
                        y = int.Parse(xmlString.Attribute("y").Value),
                        width = int.Parse(xmlString.Attribute("width").Value),
                        height = int.Parse(xmlString.Attribute("height").Value),
                        xoffset = int.Parse(xmlString.Attribute("xoffset").Value),
                        yoffset = int.Parse(xmlString.Attribute("yoffset").Value),
                        xadvance = int.Parse(xmlString.Attribute("xadvance").Value),
                        page = int.Parse(xmlString.Attribute("page").Value),
                        chnl = int.Parse(xmlString.Attribute("chnl").Value),
                    });
                }
            }

            IEnumerable<XElement> xmlSectionsKern = xmlRoot.Elements("kernings");
            BMFinfo.kernsCount = 0;
            if (!(xmlSectionsKern == null || !xmlSectionsKern.Any()))
            {
                BMFinfo.kernsCount = int.Parse(xmlSectionsKern.First().Attribute("count").Value);
                foreach (XElement xmlSection in xmlSectionsKern)
                {
                    IEnumerable<XElement> xmlStrings = xmlSection.Elements("kerning");
                    foreach (XElement xmlString in xmlStrings)
                    {
                        kernelDescList.Add(new kernelDescBMF
                        {
                            first = int.Parse(xmlString.Attribute("first").Value),
                            second = int.Parse(xmlString.Attribute("second").Value),
                            amount = int.Parse(xmlString.Attribute("amount").Value),
                        });
                    }
                }
            }
            return (BMFinfo, charDescList, kernelDescList);
        }

        private static (generalInfoBMF, List<charDescBMF>, List<kernelDescBMF>) LoadTextBMF(string inputBMF)
        {
            List<charDescBMF> charDescList = new();
            List<kernelDescBMF> kernelDescList = new();
            generalInfoBMF BMFinfo = new();
            List<string> input = new();
            foreach (string line in File.ReadLines(inputBMF))
            {
                input.Add(line);
            }

            string info = input[0];
            BMFinfo.face = Ulities.StringBetween(info, "face=\"", "\" ");
            BMFinfo.size = int.Parse(Ulities.StringBetween(info, "size=", " "));
            BMFinfo.bold = int.Parse(Ulities.StringBetween(info, "bold=", " "));
            BMFinfo.italic = int.Parse(Ulities.StringBetween(info, "italic=", " "));

            string common = input[1];
            BMFinfo.lineHeight = int.Parse(Ulities.StringBetween(common, "lineHeight=", " "));
            BMFinfo._base = int.Parse(Ulities.StringBetween(common, "base=", " "));
            BMFinfo.WidthImg = int.Parse(Ulities.StringBetween(common, "scaleW=", " "));
            BMFinfo.HeightImg = int.Parse(Ulities.StringBetween(common, "scaleH=", " "));

            string page = input[2];
            List<int> idImg = new();
            List<string> fileImg = new();
            int pageLine = 0;
            do
            {
                idImg.Add(int.Parse(Ulities.StringBetween(page, "id=", " ")));
                fileImg.Add(Ulities.StringBetween(page, "file=\"", "\""));
                pageLine += 1;
                page = input[2 + pageLine];
            } while (page.Contains("page"));

            string chars = input[2 + pageLine];
            BMFinfo.charsCount = int.Parse(Ulities.StringBetween(chars, "count=", " "));

            for (int i = 1; i <= BMFinfo.charsCount; i++)
            {
                string _char = input[2 + pageLine + i];
                charDescList.Add(new charDescBMF
                {
                    id = int.Parse(Ulities.StringBetween(_char, "id=", " ")),
                    x = int.Parse(Ulities.StringBetween(_char, "x=", " ")),
                    y = int.Parse(Ulities.StringBetween(_char, "y=", " ")),
                    width = int.Parse(Ulities.StringBetween(_char, "width=", " ")),
                    height = int.Parse(Ulities.StringBetween(_char, "height=", " ")),
                    xoffset = int.Parse(Ulities.StringBetween(_char, "xoffset=", " ")),
                    yoffset = int.Parse(Ulities.StringBetween(_char, "yoffset=", " ")),
                    xadvance = int.Parse(Ulities.StringBetween(_char, "xadvance=", " ")),
                    page = int.Parse(Ulities.StringBetween(_char, "page=", " ")),
                    chnl = int.Parse(Ulities.StringBetween(_char, "chnl=", " "))
                });
            }

            int kernLine = 2 + pageLine + BMFinfo.charsCount + 1;
            string kernings = input[kernLine];
            BMFinfo.kernsCount = int.Parse(Ulities.StringBetween(kernings, "count=", " "));
            for (int i = 1; i <= BMFinfo.kernsCount; i++)
            {
                string kern = input[kernLine + i];
                kernelDescList.Add(new kernelDescBMF
                {
                    first = int.Parse(Ulities.StringBetween(kern, "first=", " ")),
                    second = int.Parse(Ulities.StringBetween(kern, "second=", " ")),
                    amount = int.Parse(Ulities.StringBetween(kern, "amount=", " "))
                });
            }
            //foreach (kernelDescBMF _infochar in kernelDescList)
            //{
            //    Console.WriteLine(String.Format("{0,-5} {1,-5}", _infochar.first, _infochar.amount));
            //}
            return (BMFinfo, charDescList, kernelDescList);
        }
    }
}
