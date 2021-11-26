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
    class BMFontFormat : BMFontStruct
    {
        public static BMFontStruct Load(string inputBMF)
        {
            try
            {
                XDocument.Load(inputBMF);
            }
            catch (XmlException)
            {
                return LoadText(inputBMF);
            }
            return LoadXML(inputBMF);
        }

        private static BMFontStruct LoadXML(string inputBMF)
        {
            BMFontStruct bmf = new();

            XDocument xmlDoc = XDocument.Load(inputBMF);
            XElement xmlRoot = xmlDoc.Element("font");

            IEnumerable<XElement> _xmlStringsCommon = xmlRoot.Elements("common");
            foreach (XElement xmlString in _xmlStringsCommon)
            {

                bmf.generalInfo.lineHeight = int.Parse(xmlString.Attribute("lineHeight").Value);
                bmf.generalInfo._base = int.Parse(xmlString.Attribute("base").Value);
                bmf.generalInfo.WidthImg = int.Parse(xmlString.Attribute("scaleW").Value);
                bmf.generalInfo.HeightImg = int.Parse(xmlString.Attribute("scaleH").Value);
            }

            IEnumerable<XElement> _xmlStringinfo = xmlRoot.Elements("info");
            foreach (XElement xmlString in _xmlStringinfo)
            {
                bmf.generalInfo.face = xmlString.Attribute("face").Value;
                bmf.generalInfo.size = int.Parse(xmlString.Attribute("size").Value);
                bmf.generalInfo.bold = int.Parse(xmlString.Attribute("bold").Value);
                bmf.generalInfo.italic = int.Parse(xmlString.Attribute("italic").Value);
            }

            IEnumerable<XElement> xmlSections = xmlRoot.Elements("chars");
            bmf.generalInfo.charsCount = int.Parse(xmlSections.First().Attribute("count").Value);
            foreach (XElement xmlSection in xmlSections)
            {
                IEnumerable<XElement> xmlStrings = xmlSection.Elements("char");
                foreach (XElement xmlString in xmlStrings)
                {
                    bmf.charDescList.Add(new charDesc
                    {
                        id = int.Parse(xmlString.Attribute("id").Value),
                        x = float.Parse(xmlString.Attribute("x").Value),
                        y = float.Parse(xmlString.Attribute("y").Value),
                        width = float.Parse(xmlString.Attribute("width").Value),
                        height = float.Parse(xmlString.Attribute("height").Value),
                        xoffset = float.Parse(xmlString.Attribute("xoffset").Value),
                        yoffset = float.Parse(xmlString.Attribute("yoffset").Value),
                        xadvance = float.Parse(xmlString.Attribute("xadvance").Value),
                        page = int.Parse(xmlString.Attribute("page").Value),
                        chnl = int.Parse(xmlString.Attribute("chnl").Value),
                    });
                }
            }

            IEnumerable<XElement> xmlSectionsKern = xmlRoot.Elements("kernings");
            bmf.generalInfo.kernsCount = 0;
            if (!(xmlSectionsKern == null || !xmlSectionsKern.Any()))
            {
                bmf.generalInfo.kernsCount = int.Parse(xmlSectionsKern.First().Attribute("count").Value);
                foreach (XElement xmlSection in xmlSectionsKern)
                {
                    IEnumerable<XElement> xmlStrings = xmlSection.Elements("kerning");
                    foreach (XElement xmlString in xmlStrings)
                    {
                        bmf.kernelDescList.Add(new kernelDesc
                        {
                            first = int.Parse(xmlString.Attribute("first").Value),
                            second = int.Parse(xmlString.Attribute("second").Value),
                            amount = float.Parse(xmlString.Attribute("amount").Value),
                        });
                    }
                }
            }
            return bmf;
        }

        private static BMFontStruct LoadText(string inputBMF)
        {
            BMFontStruct bmf = new();
            List<string> input = new();
            foreach (string line in File.ReadLines(inputBMF))
            {
                input.Add(line);
            }

            string info = input[0];
            bmf.generalInfo.face = Ulities.StringBetween(info, "face=\"", "\" ");
            bmf.generalInfo.size = int.Parse(Ulities.StringBetween(info, "size=", " "));
            bmf.generalInfo.bold = int.Parse(Ulities.StringBetween(info, "bold=", " "));
            bmf.generalInfo.italic = int.Parse(Ulities.StringBetween(info, "italic=", " "));

            string common = input[1];
            bmf.generalInfo.lineHeight = int.Parse(Ulities.StringBetween(common, "lineHeight=", " "));
            bmf.generalInfo._base = int.Parse(Ulities.StringBetween(common, "base=", " "));
            bmf.generalInfo.WidthImg = int.Parse(Ulities.StringBetween(common, "scaleW=", " "));
            bmf.generalInfo.HeightImg = int.Parse(Ulities.StringBetween(common, "scaleH=", " "));
            bmf.generalInfo.pages = int.Parse(Ulities.StringBetween(common, "pages=", " "));

            string page = input[2];

            for (int i = 0; i < bmf.generalInfo.pages; i++)
            {
                bmf.generalInfo.idImg.Add(int.Parse(Ulities.StringBetween(page, "id=", " ")));
                bmf.generalInfo.fileImg.Add(Ulities.StringBetween(page, "file=\"", "\""));
                page = input[2 + i];
            }

            string chars = input[2 + bmf.generalInfo.pages];
            bmf.generalInfo.charsCount = int.Parse(Ulities.StringBetween(chars, "count=", " "));

            for (int i = 1; i <= bmf.generalInfo.charsCount; i++)
            {
                string _char = input[2 + bmf.generalInfo.pages + i];
                bmf.charDescList.Add(new charDesc
                {
                    id = int.Parse(Ulities.StringBetween(_char, "id=", " ")),
                    x = float.Parse(Ulities.StringBetween(_char, "x=", " ")),
                    y = float.Parse(Ulities.StringBetween(_char, "y=", " ")),
                    width = float.Parse(Ulities.StringBetween(_char, "width=", " ")),
                    height = float.Parse(Ulities.StringBetween(_char, "height=", " ")),
                    xoffset = float.Parse(Ulities.StringBetween(_char, "xoffset=", " ")),
                    yoffset = float.Parse(Ulities.StringBetween(_char, "yoffset=", " ")),
                    xadvance = float.Parse(Ulities.StringBetween(_char, "xadvance=", " ")),
                    page = int.Parse(Ulities.StringBetween(_char, "page=", " ")),
                    chnl = int.Parse(Ulities.StringBetween(_char, "chnl=", " "))
                });
            }

            try
            {
                int kernLine = 2 + bmf.generalInfo.pages + bmf.generalInfo.charsCount + 1;
                string kernings = input[kernLine];
                bmf.generalInfo.kernsCount = int.Parse(Ulities.StringBetween(kernings, "count=", " "));
                for (int i = 1; i <= bmf.generalInfo.kernsCount; i++)
                {
                    string kern = input[kernLine + i];
                    bmf.kernelDescList.Add(new kernelDesc
                    {
                        first = int.Parse(Ulities.StringBetween(kern, "first=", " ")),
                        second = int.Parse(Ulities.StringBetween(kern, "second=", " ")),
                        amount = float.Parse(Ulities.StringBetween(kern, "amount=", " "))
                    });
                }
            }
            catch
            {
                bmf.generalInfo.kernsCount = 0;
            }
            return bmf;
        }

        public static void CreateText(string outputBMF, BMFontStruct bmf)
        {
            var output = File.CreateText(outputBMF);
            //info face = "TITANESE Regular" size = 32 bold = 0 italic = 0 charset = "" unicode = 0 stretchH = 100 smooth = 1 aa = 1 padding = 4,4,4,4 spacing = -8,-8
            output.WriteLine(String.Format("info face=\"{0}\" size={1} bold={2} italic={3} ", bmf.generalInfo.face, bmf.generalInfo.size, bmf.generalInfo.bold, bmf.generalInfo.italic));
            //common lineHeight=44 base=26 scaleW=512 scaleH=512 pages=1 packed=0
            output.WriteLine(String.Format("common lineHeight={0} base={1} scaleW={2} scaleH={3} pages={4} ", bmf.generalInfo.lineHeight, bmf.generalInfo._base, bmf.generalInfo.WidthImg, bmf.generalInfo.HeightImg, bmf.generalInfo.pages));

            for (int i = 0; i < bmf.generalInfo.pages; i++)
            {
                //page id=0 file="test.png"
                output.WriteLine(String.Format("page id={0} file=\"{1}\"", bmf.generalInfo.idImg[i], bmf.generalInfo.fileImg[i]));
            }

            //chars count=97
            output.WriteLine(String.Format("chars count={0}", bmf.generalInfo.charsCount));

            foreach (charDesc _char in bmf.charDescList)
            {
                //char id=0       x=169  y=0    width=34   height=67   xoffset=4    yoffset=16   xadvance=42   page=0    chnl=0 
                output.WriteLine(String.Format("char id={0,-8}x={1,-8:0.00}y={2,-8:0.00}width={3,-8:0.00}height={4,-8:0.00}xoffset={5,-8:0.00}yoffset={6,-8:0.00}xadvance={7,-8:0.00}page={8,-8}chnl={9,-8}", _char.id, _char.x, _char.y, _char.width, _char.height, _char.xoffset, _char.yoffset, _char.xadvance, _char.page, _char.chnl));
            }

            //kernings count=667
            output.WriteLine(String.Format("kernings count={0}", bmf.generalInfo.kernsCount));
            foreach (kernelDesc kerning in bmf.kernelDescList)
            {
                //kerning first=57 second=56 amount=-2
                output.WriteLine(String.Format("kerning first={0,-5} second={1,-5} amount={2,-5:0.0}", kerning.first, kerning.second, kerning.amount));
            }
            output.Close();
        }
    }
}
