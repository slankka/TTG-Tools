using System;
using System.IO;
using System.Text;

namespace TTG_Tools.Graphics
{
    public class FontWorker
    {
        public static string DoWork(string inputFile, bool extract)
        {
            FileInfo fi = new FileInfo(inputFile);

            string wiiResult;
            if (MainMenu.settings.swizzleNintendoWii && extract && WiiSupport.TryExtractWiiContainer(inputFile, MainMenu.settings.pathForOutputFolder, out wiiResult))
            {
                return wiiResult;
            }

            if (MainMenu.settings.swizzleNintendoWii && !extract && WiiSupport.TryRepackWiiContainer(inputFile, fi.DirectoryName, MainMenu.settings.pathForOutputFolder, out wiiResult))
            {
                return wiiResult;
            }

            byte[] vectorFont = null;
            int vecFontSize = -1;
            string modFile = Methods.GetNameOfFileOnly(inputFile, ".font") + ".ttf";

            if (File.Exists(modFile))
            {
                FileInfo fi2 = new FileInfo(modFile);
                vectorFont = File.ReadAllBytes(fi2.FullName);
                vecFontSize = vectorFont.Length;
            }

            FileStream fs = new FileStream(fi.FullName, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            byte[] header = br.ReadBytes(4);

            // Determine read position (consistent with FontEditor.cs lines 1231-1243)
            int poz;
            if (Encoding.ASCII.GetString(header) == "5VSM" || Encoding.ASCII.GetString(header) == "6VSM")
            {
                poz = 16;
            }
            else
            {
                poz = 4;
            }

            // Read countElements (consistent with FontEditor.cs lines 1245-1248)
            br.BaseStream.Seek(poz, SeekOrigin.Begin);
            int countElements = br.ReadInt32();

            // Read tmp marker (consistent with FontEditor.cs lines 1254-1255)
            br.BaseStream.Seek(poz + 4, SeekOrigin.Begin);
            byte[] tmp = br.ReadBytes(8);

            // Detect font type (consistent with FontEditor.cs lines 1267-1276)
            if (BitConverter.ToString(tmp) == "81-53-37-63-9E-4A-3A-9A")
            {
                if (countElements == 1 && Encoding.ASCII.GetString(header) == "6VSM")
                {
                    // This is a vector font, continue processing
                }
                else
                {
                    // This is a bitmap font (.font file with binary element format)
                    fs.Close();
                    br.Close();
                    GC.Collect();
                    return "This is a bitmap font (.font file). Please use FontEditor to edit it: " + fi.Name;
                }
            }
            else
            {
                // This is an old format bitmap font
                fs.Close();
                br.Close();
                GC.Collect();
                return "This is a bitmap font (.font file). Please use FontEditor to edit it: " + fi.Name;
            }

            // Read version for further verification
            br.BaseStream.Seek(16, SeekOrigin.Begin);
            int version = br.ReadInt32();

            if (version != 1)
            {
                vectorFont = null;
                fs.Close();
                br.Close();

                GC.Collect();

                return "This file doesn't have vector fonts: " + fi.Name;
            }

            br.BaseStream.Seek(4, SeekOrigin.Begin);
            int blockSize = br.ReadInt32();
            ulong someValue = br.ReadUInt64();

            br.BaseStream.Seek(4, SeekOrigin.Current);

            ulong crcFontClass = br.ReadUInt64();
            uint someData = br.ReadUInt32();

            int blockNameLen = br.ReadInt32();

            byte[] fontName = br.ReadBytes(blockNameLen - 4); //Skip block of font name
            byte val = br.ReadByte();
            float fontBaseLine = br.ReadSingle(); //Base line?
            float fontCharSize = br.ReadSingle();

            int blockLen1 = br.ReadInt32();
            byte[] block1 = br.ReadBytes(blockLen1 - 4);
            int blockLen2 = br.ReadInt32();
            byte[] block2 = br.ReadBytes(blockLen2 - 4);

            byte[] boolVals = br.ReadBytes(3);

            int blockFontSize = br.ReadInt32();
            int fontSize = br.ReadInt32();

            byte[] font = br.ReadBytes(fontSize);

            byte[] endBlock = br.ReadBytes((int)(fs.Length - br.BaseStream.Position));

            br.Close();
            fs.Close();

            if(extract)
            {
                string outputFile = MainMenu.settings.pathForOutputFolder + Path.DirectorySeparatorChar + fi.Name.Remove(fi.Name.Length - 4, 4) + "ttf";
                File.WriteAllBytes(outputFile, font);
                return "File " + fi.Name + " successfully extracted";
            }

            int diff = vecFontSize - fontSize;

            blockSize += diff;
            blockFontSize += diff;
            fontSize += diff;

            if (File.Exists(MainMenu.settings.pathForOutputFolder + "\\" + fi.Name)) File.Delete(MainMenu.settings.pathForOutputFolder + "\\" + fi.Name);

            fs = new FileStream(MainMenu.settings.pathForOutputFolder + "\\" + fi.Name, FileMode.CreateNew);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(header);
            bw.Write(blockSize);
            bw.Write(someValue);
            bw.Write(version);
            bw.Write(crcFontClass);
            bw.Write(someData);
            bw.Write(blockNameLen);
            bw.Write(fontName);
            bw.Write(val);
            bw.Write(fontBaseLine);
            bw.Write(fontCharSize);
            bw.Write(blockLen1);
            bw.Write(block1);
            bw.Write(blockLen2);
            bw.Write(block2);
            bw.Write(boolVals);
            bw.Write(blockFontSize);
            bw.Write(fontSize);
            bw.Write(vectorFont);
            bw.Write(endBlock);
            bw.Close();
            fs.Close();

            return "File " + fi.Name + " successfully imported";
        }
    }
}
