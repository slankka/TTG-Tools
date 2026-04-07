using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace TTG_Tools
{
    [Serializable()]
    public class FontProfile
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("yOffset")]
        public int YOffset { get; set; }

        [XmlAttribute("fontSizeAdjust")]
        public int FontSizeAdjust { get; set; }

        [XmlAttribute("fontFamily")]
        public string FontFamilyName { get; set; }

        [XmlAttribute("fontFilePath")]
        public string FontFilePath { get; set; }

        [XmlAttribute("fontStyle")]
        public int FontStyleIndex { get; set; } // 0=Regular, 1=Bold, 2=Italic, 3=BoldItalic

        public FontProfile() { }

        public FontProfile(string name, int yOffset, int fontSizeAdjust, string fontFamilyName = null, string fontFilePath = null, int fontStyleIndex = 0)
        {
            Name = name;
            YOffset = yOffset;
            FontSizeAdjust = fontSizeAdjust;
            FontFamilyName = fontFamilyName;
            FontFilePath = fontFilePath;
            FontStyleIndex = fontStyleIndex;
        }
    }

    [Serializable()]
    [XmlRoot("FontProfiles")]
    public class FontProfileList
    {
        [XmlElement("Profile")]
        public List<FontProfile> Profiles { get; set; }

        public FontProfileList()
        {
            Profiles = new List<FontProfile>();
        }

        public static FontProfileList Load()
        {
            string xmlPath = AppDomain.CurrentDomain.BaseDirectory + "font_profiles.xml";
            if (!File.Exists(xmlPath))
                return new FontProfileList();

            try
            {
                XmlSerializer xmlS = new XmlSerializer(typeof(FontProfileList));
                using (TextReader xmlR = new StreamReader(xmlPath))
                {
                    return (FontProfileList)xmlS.Deserialize(xmlR);
                }
            }
            catch
            {
                return new FontProfileList();
            }
        }

        public void Save()
        {
            string xmlPath = AppDomain.CurrentDomain.BaseDirectory + "font_profiles.xml";
            XmlSerializer xmlS = new XmlSerializer(typeof(FontProfileList));
            TextWriter xmlW = new StreamWriter(xmlPath);
            xmlS.Serialize(xmlW, this);
            xmlW.Flush();
            xmlW.Close();
        }
    }
}
