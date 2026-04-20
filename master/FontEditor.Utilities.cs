using System;
using System.IO;
using System.Linq;
using System.Text;
using TTG_Tools.ClassesStructs;

namespace TTG_Tools
{
    public partial class FontEditor
    {
        private static string GetFntAttributeValue(string line, string attribute)
        {
            int idx = line.IndexOf(attribute + "=", StringComparison.OrdinalIgnoreCase);
            if (idx < 0)
                return string.Empty;

            int valueStart = idx + attribute.Length + 1;
            int quoteStart = line.IndexOf('"', valueStart);
            if (quoteStart >= 0)
            {
                int quoteEnd = line.IndexOf('"', quoteStart + 1);
                if (quoteEnd > quoteStart)
                {
                    return line.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
                }
            }

            // No quotes found - value may contain spaces (e.g. face=Source Han Serif Medium size=64)
            // Value ends when we hit another attribute= pattern
            // Common FNT attributes that follow 'face='
            string[] attrNames = { "size", "bold", "italic", "charset", "stretchH", "smooth", "aa", "padding", "spacing", "lineheight", "base", "scaleW", "scaleH", "pages", "alphaChnl", "redChnl", "greenChnl", "blueChnl" };

            int bestEnd = -1;
            for (int s = valueStart + 1; s < line.Length; s++)
            {
                if (line[s] == ' ')
                {
                    // Check if this space is followed by an attribute name and =
                    foreach (string attrName in attrNames)
                    {
                        if (s + 1 + attrName.Length < line.Length &&
                            line.Substring(s + 1, attrName.Length) == attrName &&
                            s + 1 + attrName.Length < line.Length &&
                            line[s + 1 + attrName.Length] == '=')
                        {
                            bestEnd = s;
                            break;
                        }
                    }
                    if (bestEnd >= 0) break;
                }
            }

            if (bestEnd > valueStart)
                return line.Substring(valueStart, bestEnd - valueStart).Trim();

            return line.Substring(valueStart).Trim();
        }

        private static string SanitizeObjectHeaderName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            return name.Replace(' ', '_');
        }

        private static string GetTextureNameBase(FontClass.ClassFont font, ClassesStructs.TextureClass.NewT3Texture templateTex)
        {
            string fontBase = SanitizeObjectHeaderName(font.FontName ?? string.Empty);
            if (!string.IsNullOrEmpty(fontBase) && fontBase != "NewFont")
            {
                return fontBase;
            }

            string templateName = templateTex?.ObjectName ?? templateTex?.SubObjectName ?? string.Empty;
            if (!string.IsNullOrEmpty(templateName))
            {
                templateName = SanitizeObjectHeaderName(templateName);
                if (templateName.EndsWith(".font", StringComparison.OrdinalIgnoreCase))
                {
                    templateName = templateName.Substring(0, templateName.Length - 5);
                }
                else if (templateName.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
                {
                    string stem = templateName.Substring(0, templateName.Length - 4);
                    int lastUs = stem.LastIndexOf('_');
                    if (lastUs >= 0 && lastUs < stem.Length - 1 && stem.Substring(lastUs + 1).All(char.IsDigit))
                    {
                        templateName = stem.Substring(0, lastUs);
                    }
                    else
                    {
                        templateName = stem;
                    }
                }
                else
                {
                    templateName = Path.GetFileNameWithoutExtension(templateName);
                }

                if (!string.IsNullOrEmpty(templateName))
                {
                    return templateName;
                }
            }

            return "font";
        }

        private static string GetTextureObjectName(FontClass.ClassFont font, ClassesStructs.TextureClass.NewT3Texture templateTex, string fntFaceName = null)
        {
            // FNT face name takes priority when importing (may contain spaces which need sanitization)
            string effectiveFaceName = fntFaceName ?? font.FontName;
            if (!string.IsNullOrEmpty(effectiveFaceName) && effectiveFaceName != "NewFont")
            {
                return SanitizeObjectHeaderName(effectiveFaceName) + ".font";
            }

            string templateName = templateTex?.ObjectName ?? templateTex?.SubObjectName ?? string.Empty;
            if (!string.IsNullOrEmpty(templateName))
            {
                templateName = SanitizeObjectHeaderName(templateName);
                if (templateName.EndsWith(".font", StringComparison.OrdinalIgnoreCase))
                {
                    templateName = templateName.Substring(0, templateName.Length - 5);
                }
                else if (templateName.EndsWith(".tga", StringComparison.OrdinalIgnoreCase))
                {
                    string stem = templateName.Substring(0, templateName.Length - 4);
                    int lastUs = stem.LastIndexOf('_');
                    if (lastUs >= 0 && lastUs < stem.Length - 1 && stem.Substring(lastUs + 1).All(char.IsDigit))
                    {
                        templateName = stem.Substring(0, lastUs);
                    }
                    else
                    {
                        templateName = stem;
                    }
                }
                else
                {
                    templateName = Path.GetFileNameWithoutExtension(templateName);
                }

                if (!string.IsNullOrEmpty(templateName))
                {
                    return templateName + ".font";
                }
            }

            return "font.font";
        }

        private static string GetTextureSlotName(FontClass.ClassFont font, ClassesStructs.TextureClass.NewT3Texture templateTex, int slotIndex)
        {
            string textureBase = GetTextureNameBase(font, templateTex);
            return SanitizeObjectHeaderName($"{textureBase}_{slotIndex}.tga");
        }

        private void InitializeDefault6VsmElements(ClassesStructs.FontClass.ClassFont targetFont)
        {
            targetFont.elements = new string[0];
            byte[][] defaults = GetDefault6VsmElementTemplate();
            targetFont.binElements = new byte[defaults.Length][];

            for (int i = 0; i < defaults.Length; i++)
            {
                targetFont.binElements[i] = new byte[12];
                Array.Copy(defaults[i], targetFont.binElements[i], 12);
            }

            // Template includes AddInfo GUID; keep write/read rules aligned.
            AddInfo = true;
        }

        private static byte[][] GetDefault6VsmElementTemplate()
        {
            return new byte[][]
            {
                new byte[] { 0x81, 0x53, 0x37, 0x63, 0x9E, 0x4A, 0x3A, 0x9A, 0x12, 0x3A, 0xBA, 0x1B },
                new byte[] { 0x2C, 0x29, 0xC2, 0x04, 0x23, 0xFA, 0x4B, 0xAB, 0x01, 0x12, 0xE9, 0x3F },
                new byte[] { 0x95, 0x38, 0x98, 0x86, 0xAA, 0xB3, 0xA0, 0x53, 0x81, 0xAB, 0x6C, 0x37 },
                new byte[] { 0xE2, 0xCC, 0x38, 0x6F, 0x7E, 0x9E, 0x24, 0x3E, 0x61, 0xAB, 0x30, 0xA7 },
                new byte[] { 0xE3, 0x88, 0x09, 0x7A, 0x48, 0x5D, 0x7F, 0x93, 0xB0, 0xCE, 0xE3, 0xB2 },
                new byte[] { 0x8C, 0x59, 0x05, 0x84, 0xB7, 0xFB, 0x88, 0x8E, 0xAF, 0x7D, 0xAC, 0xA4 },
                new byte[] { 0x7A, 0xBA, 0x6E, 0x87, 0x89, 0x88, 0x6C, 0xFA, 0x05, 0x49, 0x48, 0x5B },
                new byte[] { 0x07, 0x1A, 0x1F, 0xE6, 0x44, 0xA2, 0xBC, 0x7B, 0x02, 0xCC, 0x9F, 0xE1 },
                new byte[] { 0x0F, 0xF4, 0x20, 0xE6, 0x20, 0xBA, 0xA1, 0xEF, 0x40, 0xFC, 0xF4, 0x9A },
                new byte[] { 0xEA, 0x0E, 0x30, 0xAE, 0xF1, 0x19, 0x46, 0x58, 0x61, 0xEA, 0x57, 0x50 }
            };
        }

        private static bool IsKnownTexturePlatform(uint platform)
        {
            // Keep in sync with parser/extractor supported platforms.
            return platform == 2u || platform == 4u || platform == 7u || platform == 9u
                || platform == 10u || platform == 11u || platform == 13u || platform == 15u;
        }

        private uint ResolveTargetPlatformForImportedDds(uint existingPlatform)
        {
            // Explicit swizzle selection has the highest priority.
            if (MainMenu.settings.swizzleNintendoSwitch) return 15u;
            if (MainMenu.settings.swizzlePS4) return 11u;
            if (MainMenu.settings.swizzleXbox360) return 4u;
            if (MainMenu.settings.swizzlePSVita) return 9u;

            // Reuse platform parsed from source font/new texture template when valid.
            if (IsKnownTexturePlatform(existingPlatform)) return existingPlatform;

            // No explicit method selected and no known existing platform: default to PC.
            return 2u;
        }

        private string ConvertToString(byte[] mas)
        {
            string str = "";
            foreach (byte b in mas)
            { str += b.ToString("x") + " "; }

            return str;
        }

        public bool CompareArray(byte[] arr0, byte[] arr1)
        {
            int i = 0;
            while ((i < arr0.Length) && (arr0[i] == arr1[i])) i++;
            return (i == arr0.Length);
        }
    }
}
