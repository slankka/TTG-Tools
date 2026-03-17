using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace TTG_Tools.Graphics
{
    internal static class WiiSupport
    {
        internal struct Resolution
        {
            public int Width;
            public int Height;
        }

        private static readonly byte[] HeaderMagic = Encoding.ASCII.GetBytes("ERTM");
        private static readonly byte[] TplMagic = { 0x00, 0x20, 0xAF, 0x30 };
        private static readonly byte[] HashStyleMagic = { 0x81, 0x53, 0x37, 0x63, 0x9E, 0x4A, 0x3A, 0x9A };
        private static readonly byte[] SomeTexDataHash = { 0xE3, 0x88, 0x09, 0x7A, 0x48, 0x5D, 0x7F, 0x93 };

        internal class WiiGlyph
        {
            public int TexNum;
            public float XStart;
            public float XEnd;
            public float YStart;
            public float YEnd;
            public float CharWidth;
            public float CharHeight;
        }

        internal class WiiFontData
        {
            public byte[] SignatureBytes = HeaderMagic;
            public byte[] ElementsDataBytes = new byte[0];
            public byte[] FontNameBlockBytes = new byte[0];
            public byte[] FontMetadataAfterNameBytes = new byte[0];
            public byte[] OptionalOneValueBytes;
            public byte[] BlockCoordSizeValBytes;
            public byte[] CharCountValBytes = new byte[0];
            public byte[] RawGlyphDataBytes = new byte[0];
            public byte[] SuffixDataBytes = new byte[0];
            public string FontName = string.Empty;
            public float BaseSize;
            public bool IsBlockSizeFont;
            public bool HasScaleValue;
            public int CharCount;
            public int TextureWidth;
            public int TextureHeight;
            public int TexCount = 1;
            public readonly List<WiiGlyph> Glyphs = new List<WiiGlyph>();

            public void Parse(string path, int texWidth, int texHeight)
            {
                TextureWidth = texWidth;
                TextureHeight = texHeight;
                bool preliminaryHasScale = false;

                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                using (var br = new BinaryReader(fs))
                {
                    SignatureBytes = br.ReadBytes(4);
                    if (!SignatureBytes.SequenceEqual(HeaderMagic)) throw new InvalidDataException("Not an ERTM file.");

                    byte[] countElementBytes = br.ReadBytes(4);
                    int countElements = BitConverter.ToInt32(countElementBytes, 0);
                    var elementBytes = new List<byte[]> { countElementBytes };
                    var elementContentForCheck = new List<byte[]>();

                    if (countElements > 0)
                    {
                        byte[] peek = br.ReadBytes(8);
                        fs.Seek(-8, SeekOrigin.Current);
                        bool allHash = peek.SequenceEqual(HashStyleMagic);

                        for (int i = 0; i < countElements; i++)
                        {
                            if (allHash)
                            {
                                byte[] hashPart = br.ReadBytes(8);
                                byte[] padPart = br.ReadBytes(4);
                                elementBytes.Add(hashPart);
                                elementBytes.Add(padPart);
                                elementContentForCheck.Add(hashPart);
                            }
                            else
                            {
                                byte[] lenBytes = br.ReadBytes(4);
                                int len = BitConverter.ToInt32(lenBytes, 0);
                                byte[] strBytes = br.ReadBytes(len);
                                byte[] toolBytes = br.ReadBytes(4);
                                elementBytes.Add(lenBytes);
                                elementBytes.Add(strBytes);
                                elementBytes.Add(toolBytes);
                                elementContentForCheck.Add(strBytes);
                            }
                        }
                    }

                    ElementsDataBytes = Concat(elementBytes);
                    preliminaryHasScale = elementContentForCheck.Any(x => x.SequenceEqual(SomeTexDataHash));

                    long fontNameStart = fs.Position;
                    byte[] possibleBlockSizeBytes = br.ReadBytes(4);
                    int possibleBlockSize = BitConverter.ToInt32(possibleBlockSizeBytes, 0);

                    long afterFirst = fs.Position;
                    byte[] maybeNameLenBytes = br.ReadBytes(4);
                    byte[] actualNameLenBytes;

                    if (maybeNameLenBytes.Length < 4)
                    {
                        IsBlockSizeFont = false;
                        actualNameLenBytes = possibleBlockSizeBytes;
                        fs.Seek(afterFirst, SeekOrigin.Begin);
                    }
                    else
                    {
                        int maybeNameLen = BitConverter.ToInt32(maybeNameLenBytes, 0);
                        if (possibleBlockSize > 0 && maybeNameLen >= 0 && possibleBlockSize - maybeNameLen == 8)
                        {
                            IsBlockSizeFont = true;
                            actualNameLenBytes = maybeNameLenBytes;
                        }
                        else
                        {
                            IsBlockSizeFont = false;
                            actualNameLenBytes = possibleBlockSizeBytes;
                            fs.Seek(afterFirst, SeekOrigin.Begin);
                        }
                    }

                    int nameLen = BitConverter.ToInt32(actualNameLenBytes, 0);
                    byte[] nameBytes = br.ReadBytes(nameLen);
                    FontName = Encoding.ASCII.GetString(nameBytes);

                    long nameEnd = fs.Position;
                    fs.Seek(fontNameStart, SeekOrigin.Begin);
                    FontNameBlockBytes = br.ReadBytes((int)(nameEnd - fontNameStart));

                    long metadataStart = fs.Position;
                    br.ReadByte();
                    BaseSize = br.ReadSingle();
                    int consumed = 5;

                    long peekStart = fs.Position;
                    byte[] peekOptional = br.ReadBytes(4);
                    if (peekOptional.Length == 4 && peekOptional.SequenceEqual(new byte[] { 0xCE, 0xFA, 0xED, 0xFE }))
                    {
                        consumed += 4;
                        peekStart = fs.Position;
                        peekOptional = br.ReadBytes(4);
                    }

                    if (peekOptional.Length == 4)
                    {
                        float half = BitConverter.ToSingle(peekOptional, 0);
                        if (Math.Abs(half - 0.5f) < 0.00001f || Math.Abs(half - 1.0f) < 0.00001f) consumed += 4;
                        else fs.Seek(peekStart, SeekOrigin.Begin);
                    }

                    fs.Seek(metadataStart, SeekOrigin.Begin);
                    FontMetadataAfterNameBytes = br.ReadBytes(consumed);
                    fs.Seek(metadataStart + consumed, SeekOrigin.Begin);

                    if (IsBlockSizeFont && preliminaryHasScale)
                    {
                        OptionalOneValueBytes = br.ReadBytes(4);
                        if (OptionalOneValueBytes.Length < 4) OptionalOneValueBytes = null;
                    }

                    int blockCoordSizeParsed = 0;
                    if (IsBlockSizeFont)
                    {
                        BlockCoordSizeValBytes = br.ReadBytes(4);
                        blockCoordSizeParsed = BitConverter.ToInt32(BlockCoordSizeValBytes, 0);
                    }

                    CharCountValBytes = br.ReadBytes(4);
                    CharCount = BitConverter.ToInt32(CharCountValBytes, 0);

                    if (IsBlockSizeFont && CharCount > 0)
                    {
                        float perGlyph = (blockCoordSizeParsed - 8f) / CharCount;
                        HasScaleValue = Math.Abs(perGlyph - 28f) < 0.001f;
                    }
                    else
                    {
                        HasScaleValue = preliminaryHasScale;
                    }

                    int bytesPerGlyph = HasScaleValue ? 28 : 20;
                    int glyphTotal = CharCount * bytesPerGlyph;
                    RawGlyphDataBytes = br.ReadBytes(glyphTotal);

                    using (var ms = new MemoryStream(RawGlyphDataBytes))
                    using (var gbr = new BinaryReader(ms))
                    {
                        Glyphs.Clear();
                        for (int i = 0; i < CharCount; i++)
                        {
                            var g = new WiiGlyph();
                            g.TexNum = gbr.ReadInt32();
                            float x0 = gbr.ReadSingle();
                            float x1 = gbr.ReadSingle();
                            float y0 = gbr.ReadSingle();
                            float y1 = gbr.ReadSingle();
                            g.XStart = (float)Math.Round(x0 * TextureWidth);
                            g.XEnd = (float)Math.Round(x1 * TextureWidth);
                            g.YStart = (float)Math.Round(y0 * TextureHeight);
                            g.YEnd = (float)Math.Round(y1 * TextureHeight);
                            if (HasScaleValue)
                            {
                                g.CharWidth = (float)Math.Round(gbr.ReadSingle());
                                g.CharHeight = (float)Math.Round(gbr.ReadSingle());
                            }
                            Glyphs.Add(g);
                        }
                    }

                    long suffixStart = fs.Position;
                    fs.Seek(0, SeekOrigin.End);
                    long end = fs.Position;
                    fs.Seek(suffixStart, SeekOrigin.Begin);
                    SuffixDataBytes = br.ReadBytes((int)(end - suffixStart));
                }
            }

            public void Save(string outputPath)
            {
                using (var ms = new MemoryStream())
                using (var bw = new BinaryWriter(ms))
                {
                    foreach (var g in Glyphs)
                    {
                        bw.Write(g.TexNum);
                        bw.Write(TextureWidth == 0 ? 0f : g.XStart / TextureWidth);
                        bw.Write(TextureWidth == 0 ? 0f : g.XEnd / TextureWidth);
                        bw.Write(TextureHeight == 0 ? 0f : g.YStart / TextureHeight);
                        bw.Write(TextureHeight == 0 ? 0f : g.YEnd / TextureHeight);
                        if (HasScaleValue)
                        {
                            bw.Write(g.CharWidth);
                            bw.Write(g.CharHeight);
                        }
                    }

                    byte[] glyphBytes = ms.ToArray();
                    using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    using (var outBw = new BinaryWriter(fs))
                    {
                        outBw.Write(SignatureBytes);
                        outBw.Write(ElementsDataBytes);
                        outBw.Write(FontNameBlockBytes);
                        outBw.Write(FontMetadataAfterNameBytes);
                        if (OptionalOneValueBytes != null) outBw.Write(OptionalOneValueBytes);
                        if (IsBlockSizeFont)
                        {
                            int bytesPerGlyph = HasScaleValue ? 28 : 20;
                            int recalculated = (CharCount * bytesPerGlyph) + 8;
                            outBw.Write(recalculated);
                        }
                        outBw.Write(CharCountValBytes);
                        outBw.Write(glyphBytes);
                        outBw.Write(SuffixDataBytes);
                    }
                }
            }

            public void ExportFnt(string path)
            {
                using (var sw = new StreamWriter(path, false, Encoding.UTF8))
                {
                    sw.WriteLine("info face=\"{0}\" size={1} bold=0 italic=0 charset=\"\" unicode=0 smooth=0 aa=0 padding=0,0,0,0 spacing=0,0 outline=0", FontName, (int)BaseSize);
                    sw.WriteLine("common lineHeight={0} base={0} scaleW={1} scaleH={2} pages={3} packed=0 alphaChnl=0 redChnl=0 greenChnl=0 blueChnl=0", (int)BaseSize, TextureWidth, TextureHeight, TexCount);
                    for (int i = 0; i < TexCount; i++) sw.WriteLine("page id={0} file=\"{1}_{0}.dds\"", i, FontName);
                    sw.WriteLine("chars count={0}", Glyphs.Count);

                    for (int i = 0; i < Glyphs.Count; i++)
                    {
                        var g = Glyphs[i];
                        int width = HasScaleValue ? (int)g.CharWidth : (int)(g.XEnd - g.XStart);
                        int height = HasScaleValue ? (int)g.CharHeight : (int)(g.YEnd - g.YStart);
                        sw.WriteLine("char id={0,-5} x={1,-5} y={2,-5} width={3,-5} height={4,-5} xoffset=0    yoffset=0    xadvance={5,-5} page={6,-3} chnl=15", i, (int)g.XStart, (int)g.YStart, width, height, width, g.TexNum);
                    }
                }
            }

            public void ImportFnt(string path)
            {
                var map = new Dictionary<int, WiiGlyph>();
                int maxId = -1;
                foreach (string line in File.ReadLines(path))
                {
                    string trimmed = line.Trim();
                    if (trimmed.StartsWith("common "))
                    {
                        var data = ParseFntPairs(trimmed);
                        if (data.ContainsKey("pages")) TexCount = ParseInt(data["pages"]);
                    }
                    else if (trimmed.StartsWith("char "))
                    {
                        var data = ParseFntPairs(trimmed);
                        int id = ParseInt(data["id"]);
                        maxId = Math.Max(maxId, id);
                        var g = new WiiGlyph
                        {
                            TexNum = ParseInt(data["page"]),
                            XStart = ParseFloat(data["x"]),
                            YStart = ParseFloat(data["y"])
                        };
                        float width = ParseFloat(data["width"]);
                        float height = ParseFloat(data["height"]);
                        g.XEnd = g.XStart + width;
                        g.YEnd = g.YStart + height;
                        if (HasScaleValue)
                        {
                            g.CharWidth = width;
                            g.CharHeight = height;
                        }
                        map[id] = g;
                    }
                }

                if (maxId + 1 != CharCount)
                {
                    // keep original size, just partial replacement
                }

                for (int i = 0; i < CharCount; i++)
                {
                    WiiGlyph g;
                    if (map.TryGetValue(i, out g)) Glyphs[i] = g;
                }
            }
        }

        internal static bool TryExtractWiiContainer(string inputPath, string outputDir, out string result)
        {
            result = null;
            byte[] data; int version; int offSizes; List<int> tplOffsets; bool alt;
            if (!TryParseContainer(inputPath, out data, out version, out offSizes, out tplOffsets, out alt)) return false;

            string name = Path.GetFileNameWithoutExtension(inputPath);
            string ext = Path.GetExtension(inputPath).ToLowerInvariant();

            int size1 = BitConverter.ToInt32(data, offSizes);
            File.WriteAllBytes(Path.Combine(outputDir, name + ".tpl"), Slice(data, tplOffsets[0], size1));

            if (ext == ".font")
            {
                var res = ReadTplResolution(data, tplOffsets[0]);
                if (res.HasValue)
                {
                    var font = new WiiFontData();
                    font.Parse(inputPath, res.Value.Width, res.Value.Height);
                    font.ExportFnt(Path.Combine(outputDir, name + ".fnt"));
                }
            }

            if (!(version == 4 && alt) && (version == 4 || version == 7) && tplOffsets.Count > 1)
            {
                int size2 = BitConverter.ToInt32(data, offSizes + 4);
                if (size2 > 0) File.WriteAllBytes(Path.Combine(outputDir, name + "_alpha.tpl"), Slice(data, tplOffsets[1], size2));
            }

            result = "File " + Path.GetFileName(inputPath) + " successfully extracted (Wii/TPL).";
            return true;
        }

        internal static bool TryRepackWiiContainer(string inputPath, string inputDir, string outputDir, out string result)
        {
            result = null;
            byte[] data; int version; int offSizes; List<int> tplOffsets; bool alt;
            if (!TryParseContainer(inputPath, out data, out version, out offSizes, out tplOffsets, out alt)) return false;

            string name = Path.GetFileNameWithoutExtension(inputPath);
            string ext = Path.GetExtension(inputPath).ToLowerInvariant();
            string tplPath = Path.Combine(inputDir, name + ".tpl");
            if (!File.Exists(tplPath)) return false;

            byte[] tpl1 = File.ReadAllBytes(tplPath);
            byte[] tpl2 = new byte[0];
            if ((version == 4 || version == 7) && !(version == 4 && alt))
            {
                string alphaPath = Path.Combine(inputDir, name + "_alpha.tpl");
                if (File.Exists(alphaPath)) tpl2 = File.ReadAllBytes(alphaPath);
            }

            int tplStart = tplOffsets[0];
            byte[] prefix = new byte[tplStart];
            Array.Copy(data, 0, prefix, 0, prefix.Length);
            Array.Copy(BitConverter.GetBytes(tpl1.Length), 0, prefix, offSizes, 4);
            if ((version == 4 || version == 7) && !(version == 4 && alt)) Array.Copy(BitConverter.GetBytes(tpl2.Length), 0, prefix, offSizes + 4, 4);

            byte[] newData = Concat(new[] { prefix, tpl1, tpl2 });
            string outPath = Path.Combine(outputDir, name + ext);
            File.WriteAllBytes(outPath, newData);

            if (ext == ".font")
            {
                string fntPath = Path.Combine(inputDir, name + ".fnt");
                var res = ReadTplResolution(newData, tplOffsets[0]);
                if (File.Exists(fntPath) && res.HasValue)
                {
                    var font = new WiiFontData();
                    font.Parse(outPath, res.Value.Width, res.Value.Height);
                    font.ImportFnt(fntPath);
                    font.Save(outPath);
                }
            }

            result = "File " + Path.GetFileName(inputPath) + " successfully imported (Wii/TPL).";
            return true;
        }

        internal static bool TryLoadWiiFontForEditor(string fontPath, out WiiFontData fontData)
        {
            fontData = null;
            byte[] data; int version; int offSizes; List<int> tplOffsets; bool alt;
            if (!TryParseContainer(fontPath, out data, out version, out offSizes, out tplOffsets, out alt)) return false;
            if (Path.GetExtension(fontPath).ToLowerInvariant() != ".font") return false;
            var res = ReadTplResolution(data, tplOffsets[0]);
            if (!res.HasValue) return false;
            var parsed = new WiiFontData();
            parsed.Parse(fontPath, res.Value.Width, res.Value.Height);
            fontData = parsed;
            return true;
        }

        private static bool TryParseContainer(string path, out byte[] data, out int version, out int offSizes, out List<int> tplOffsets, out bool alt)
        {
            data = File.ReadAllBytes(path);
            version = 0;
            offSizes = 0;
            tplOffsets = new List<int>();
            alt = false;

            if (data.Length < 8 || !Slice(data, 0, 4).SequenceEqual(HeaderMagic)) return false;

            version = BitConverter.ToInt32(data, 4);
            if (version == 4)
            {
                int @base = 64;
                int length = BitConverter.ToInt32(data, @base);
                int prefixBase = @base + 4 + length;
                int normalOffset = prefixBase + 0x2C;
                int sizeAtNormal = BitConverter.ToInt32(data, normalOffset);
                offSizes = sizeAtNormal == 0 ? (prefixBase + 0xA6) : normalOffset;
                alt = sizeAtNormal == 0;
            }
            else if (version == 2)
            {
                int @base = 32;
                int length = BitConverter.ToInt32(data, @base);
                offSizes = @base + 4 + length + 0x2B;
            }
            else if (version == 7 || version == 5)
            {
                byte[] dxt = Encoding.ASCII.GetBytes("DXT");
                int idx = IndexOf(data, dxt);
                if (idx < 0) return false;
                offSizes = idx + (version == 7 ? 0x21 : 0x1D);
            }
            else return false;

            tplOffsets = FindAll(data, TplMagic);
            return tplOffsets.Count > 0;
        }

        private static Resolution? ReadTplResolution(byte[] data, int tplOffset)
        {
            try
            {
                if (tplOffset + 12 > data.Length) return null;
                uint id = ReadUInt32BE(data, tplOffset);
                if (id != 0x0020AF30) return null;
                uint images = ReadUInt32BE(data, tplOffset + 4);
                uint tableOffset = ReadUInt32BE(data, tplOffset + 8);
                if (images == 0) return null;
                uint imgHeaderOffset = ReadUInt32BE(data, tplOffset + (int)tableOffset);
                ushort height = ReadUInt16BE(data, tplOffset + (int)imgHeaderOffset);
                ushort width = ReadUInt16BE(data, tplOffset + (int)imgHeaderOffset + 2);
                Resolution r = new Resolution();
                r.Width = width * 2;
                r.Height = height * 2;
                return r;
            }
            catch { return null; }
        }

        private static Dictionary<string, string> ParseFntPairs(string line)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts.Skip(1))
            {
                int idx = part.IndexOf('=');
                if (idx <= 0) continue;
                dict[part.Substring(0, idx)] = part.Substring(idx + 1).Trim('"');
            }
            return dict;
        }

        private static int ParseInt(string value) { return int.Parse(value, CultureInfo.InvariantCulture); }
        private static float ParseFloat(string value) { return float.Parse(value, CultureInfo.InvariantCulture); }

        private static ushort ReadUInt16BE(byte[] data, int offset) { return (ushort)((data[offset] << 8) | data[offset + 1]); }
        private static uint ReadUInt32BE(byte[] data, int offset) { return (uint)((data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3]); }

        private static byte[] Slice(byte[] source, int offset, int len)
        {
            if (offset < 0 || len < 0 || offset + len > source.Length) return new byte[0];
            byte[] result = new byte[len];
            Array.Copy(source, offset, result, 0, len);
            return result;
        }

        private static byte[] Concat(IEnumerable<byte[]> arrays)
        {
            int len = 0;
            foreach (byte[] a in arrays)
            {
                if (a != null) len += a.Length;
            }
            byte[] output = new byte[len];
            int pos = 0;
            foreach (byte[] arr in arrays)
            {
                if (arr == null) continue;
                Buffer.BlockCopy(arr, 0, output, pos, arr.Length);
                pos += arr.Length;
            }
            return output;
        }

        private static int IndexOf(byte[] data, byte[] target)
        {
            for (int i = 0; i <= data.Length - target.Length; i++)
            {
                bool ok = true;
                for (int j = 0; j < target.Length; j++)
                {
                    if (data[i + j] != target[j]) { ok = false; break; }
                }
                if (ok) return i;
            }
            return -1;
        }

        private static List<int> FindAll(byte[] data, byte[] target)
        {
            var offsets = new List<int>();
            for (int i = 0; i <= data.Length - target.Length; i++)
            {
                bool ok = true;
                for (int j = 0; j < target.Length; j++)
                {
                    if (data[i + j] != target[j]) { ok = false; break; }
                }
                if (ok) offsets.Add(i);
            }
            return offsets;
        }
    }
}
