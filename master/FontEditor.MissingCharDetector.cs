using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TTG_Tools
{
    public partial class FontEditor
    {
        private void buttonDetectMissingTextures_Click(object sender, EventArgs e)
        {
            if (font == null)
            {
                MessageBox.Show("Please open a font file first.", "No Font Loaded",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MainMenu.settings.scanTextFilePaths == null || MainMenu.settings.scanTextFilePaths.Count == 0)
            {
                MessageBox.Show("No scan paths configured. Please add scan paths in Settings.", "No Scan Paths",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            textBoxLogOutput.Clear();
            textBoxLogOutput.AppendText("=== Missing Textures Detection Report ===\r\n");
            textBoxLogOutput.AppendText($"Scan Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n");
            textBoxLogOutput.AppendText($"Font: {font.FontName}\r\n");
            textBoxLogOutput.AppendText($"Scan Paths: {MainMenu.settings.scanTextFilePaths.Count}\r\n");
            textBoxLogOutput.AppendText("===========================================\r\n\r\n");

            try
            {
                // Collect all characters from the font
                HashSet<char> fontChars = new HashSet<char>();

                if (font.NewFormat && font.glyph.charsNew != null && font.glyph.charsNew.Length > 0)
                {
                    // New format: charsNew with charId field
                    foreach (var charData in font.glyph.charsNew)
                    {
                        if (charData.charId != 0)
                        {
                            // charId is the Unicode code point, convert directly to char
                            fontChars.Add((char)charData.charId);
                        }
                    }
                }
                else if (!font.NewFormat && font.glyph.chars != null && font.glyph.chars.Length > 0)
                {
                    // Old format: chars array where index is the character code
                    for (int i = 0; i < font.glyph.chars.Length; i++)
                    {
                        if (i > 0 && i <= 0xFFFF) // Valid Unicode range
                        {
                            try
                            {
                                fontChars.Add((char)i);
                            }
                            catch { }
                        }
                    }
                }

                if (fontChars.Count == 0)
                {
                    textBoxLogOutput.AppendText("Warning: No characters found in font!\r\n");
                    return;
                }

                textBoxLogOutput.AppendText($"Font contains {fontChars.Count} unique characters.\r\n\r\n");

                // Scan all text files and extract all unique characters
                HashSet<char> allUniqueChars = new HashSet<char>();
                int filesScanned = 0;
                int totalTextsFound = 0;

                foreach (string scanPath in MainMenu.settings.scanTextFilePaths)
                {
                    if (!Directory.Exists(scanPath))
                    {
                        textBoxLogOutput.AppendText($"Warning: Path does not exist: {scanPath}\r\n");
                        continue;
                    }

                    textBoxLogOutput.AppendText($"Scanning: {scanPath}\r\n");
                    ScanDirectoryForTextFiles(scanPath, allUniqueChars, ref filesScanned, ref totalTextsFound, 0);
                }

                textBoxLogOutput.AppendText($"\r\nScanned {filesScanned} .txt files.\r\n");
                textBoxLogOutput.AppendText($"Found {totalTextsFound} text entries.\r\n");
                textBoxLogOutput.AppendText($"Extracted {allUniqueChars.Count} unique characters from all texts.\r\n");
                textBoxLogOutput.AppendText("===========================================\r\n\r\n");

                // Find characters that exist in texts but not in font
                List<char> missingChars = allUniqueChars.Where(c => !fontChars.Contains(c)).OrderBy(c => (int)c).ToList();

                // Store missing characters for later use
                lastDetectedMissingChars = missingChars;

                // Find characters that exist in font but not in texts
                List<char> unusedChars = fontChars.Where(c => !allUniqueChars.Contains(c)).OrderBy(c => (int)c).ToList();

                // Find characters that exist in both
                List<char> matchedChars = allUniqueChars.Where(c => fontChars.Contains(c)).OrderBy(c => (int)c).ToList();

                // Output results
                textBoxLogOutput.AppendText("=== Detection Results ===\r\n");
                textBoxLogOutput.AppendText($"Characters in texts: {allUniqueChars.Count}\r\n");
                textBoxLogOutput.AppendText($"Characters in font: {fontChars.Count}\r\n");
                textBoxLogOutput.AppendText($"Matched characters: {matchedChars.Count}\r\n");
                textBoxLogOutput.AppendText($"Missing characters (in texts but not in font): {missingChars.Count}\r\n");
                textBoxLogOutput.AppendText($"Unused characters (in font but not in texts): {unusedChars.Count}\r\n");

                if (allUniqueChars.Count > 0)
                {
                    double coverageRate = (matchedChars.Count * 100.0 / allUniqueChars.Count);
                    textBoxLogOutput.AppendText($"Font coverage rate: {coverageRate:F2}%\r\n");
                }

                textBoxLogOutput.AppendText("===========================================\r\n\r\n");

                if (missingChars.Count > 0)
                {
                    textBoxLogOutput.AppendText("=== Missing Characters (In Texts But Not In Font) ===\r\n");
                    int count = 0;
                    int column = 0;
                    string line = "";

                    foreach (char c in missingChars)
                    {
                        string charInfo = $"[{c}]U+{(int)c:X4}({((c >= 0x4E00 && c <= 0x9FFF) ? "CJK" : (char.IsLetter(c) ? "Char" : "Sym"))})";
                        line += $"{charInfo,-16}";

                        if (++column >= 5)
                        {
                            textBoxLogOutput.AppendText($"{line}\r\n");
                            line = "";
                            column = 0;
                            count++;

                            if (count >= 80) // Show first 80 lines (400 chars)
                            {
                                if (missingChars.Count > 400)
                                {
                                    textBoxLogOutput.AppendText($"\r\n... and {missingChars.Count - 400} more missing characters.\r\n");
                                }
                                break;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(line))
                    {
                        textBoxLogOutput.AppendText($"{line}\r\n");
                    }

                    textBoxLogOutput.AppendText("===========================================\r\n\r\n");
                    textBoxLogOutput.AppendText("Note: Use 'Save As...' button to export full character list.\r\n");
                }
                else
                {
                    textBoxLogOutput.AppendText("✓ All characters from texts are available in the font!\r\n");
                }

                if (unusedChars.Count > 0)
                {
                    textBoxLogOutput.AppendText($"\r\nNote: Font contains {unusedChars.Count} characters that are not used in the scanned texts.\r\n");
                }

                textBoxLogOutput.AppendText("\r\n=== Detection Complete ===\r\n");
            }
            catch (Exception ex)
            {
                textBoxLogOutput.AppendText($"\r\nError during detection:\r\n{ex.Message}\r\n");
                MessageBox.Show($"Error during detection: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ScanDirectoryForTextFiles(string directory, HashSet<char> uniqueChars, ref int filesScanned, ref int totalTextsFound, int currentDepth)
        {
            if (currentDepth >= 2) // Maximum 2 levels deep
                return;

            try
            {
                // Scan .txt files in current directory
                foreach (string file in Directory.GetFiles(directory, "*.txt"))
                {
                    try
                    {
                        string[] lines = File.ReadAllLines(file, Encoding.GetEncoding(MainMenu.settings.ASCII_N));
                        foreach (string line in lines)
                        {
                            if (line.StartsWith("speechTranslation="))
                            {
                                string text = line.Substring("speechTranslation=".Length).Trim();

                                // Filter out invalid or meaningless texts
                                if (!string.IsNullOrWhiteSpace(text) && IsMeaningfulText(text))
                                {
                                    totalTextsFound++;

                                    // Extract all characters from the text
                                    foreach (char c in text)
                                    {
                                        // Only add meaningful characters (skip whitespace, control chars)
                                        if (!char.IsWhiteSpace(c) && !char.IsControl(c))
                                        {
                                            uniqueChars.Add(c);
                                        }
                                    }
                                }
                            }
                        }
                        filesScanned++;
                    }
                    catch (Exception ex)
                    {
                        textBoxLogOutput.AppendText($"  Warning: Could not read file {Path.GetFileName(file)}: {ex.Message}\r\n");
                    }
                }

                // Recursively scan subdirectories (up to 2 levels deep)
                if (currentDepth < 2)
                {
                    foreach (string subDir in Directory.GetDirectories(directory))
                    {
                        ScanDirectoryForTextFiles(subDir, uniqueChars, ref filesScanned, ref totalTextsFound, currentDepth + 1);
                    }
                }
            }
            catch (Exception ex)
            {
                textBoxLogOutput.AppendText($"  Warning: Could not scan directory {directory}: {ex.Message}\r\n");
            }
        }

        private bool IsMeaningfulText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            // Remove whitespace for checking
            string trimmed = text.Trim();

            // Must have at least 2 characters
            if (trimmed.Length < 2)
                return false;

            // Check if text contains at least one letter/character (not just punctuation/symbols)
            bool hasAlphanumeric = false;
            int charCount = 0;

            foreach (char c in trimmed)
            {
                // Count CJK characters (Chinese, Japanese, Korean)
                if (c >= 0x4E00 && c <= 0x9FFF) // CJK Unified Ideographs
                {
                    hasAlphanumeric = true;
                    charCount++;
                }
                // Count letters and digits
                else if (char.IsLetterOrDigit(c))
                {
                    hasAlphanumeric = true;
                    charCount++;
                }
                // Skip whitespace and punctuation
                else if (!char.IsWhiteSpace(c) && !char.IsPunctuation(c))
                {
                    charCount++;
                }
            }

            // Must have at least 2 meaningful characters
            return hasAlphanumeric && charCount >= 2;
        }
    }
}