using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                XDocument xmlDoc = new XDocument();
                xmlDoc = XDocument.Load(inputBMF);

            }
            catch (XmlException exception)
            {
                //return ParseTextBMF(inputBMF);
                System.Environment.Exit(1);
            }
            return LoadXMLBMF(inputBMF);
        }
        public static (generalInfoBMF, List<charDescBMF>, List<kernelDescBMF>) LoadXMLBMF(string inputBMF)
        {
            List<charDescBMF> charDescList = new();
            List<kernelDescBMF> kernelDescList = new();
            generalInfoBMF BMFinfo = new();

            XDocument xmlDoc = new XDocument();
            xmlDoc = XDocument.Load(inputBMF);
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
            int CountChar = int.Parse(xmlSections.First().Attribute("count").Value);
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
                        w = int.Parse(xmlString.Attribute("width").Value),
                        h = int.Parse(xmlString.Attribute("height").Value),
                        xoff = int.Parse(xmlString.Attribute("xoffset").Value),
                        yoff = int.Parse(xmlString.Attribute("yoffset").Value),
                        xadvance = int.Parse(xmlString.Attribute("xadvance").Value),
                        page = int.Parse(xmlString.Attribute("page").Value),
                        chnl = int.Parse(xmlString.Attribute("chnl").Value),
                    });
                }
            }


            IEnumerable<XElement> xmlSectionsKern = xmlRoot.Elements("kernings");
            if (!(xmlSectionsKern == null || !xmlSectionsKern.Any()))
            {
                int CountKern = int.Parse(xmlSectionsKern.First().Attribute("count").Value);
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
            return ( BMFinfo, charDescList, kernelDescList);
            


        }
        /*
        public static (generalInfoBMF, List<charDescBMF>, List<kernelDescBMF>) LoadTextBMF(string inputBMF)
        {
            System.Environment.Exit(1);
        }*/

    }
}
