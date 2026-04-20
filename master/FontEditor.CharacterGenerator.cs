using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TTG_Tools.ClassesStructs;
using ImageMagick;

namespace TTG_Tools
{
    public partial class FontEditor
    {
        // Generate missing characters and append to the end of textures
        private void buttonGenerateMissingChars_Click(object sender, EventArgs e)
        {
            if (font == null)
            {
                MessageBox.Show("Please open a font file first.", "No Font Loaded",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (lastDetectedMissingChars == null || lastDetectedMissingChars.Count == 0)
            {
                MessageBox.Show("Please run 'Detect Missing Textures' first to find missing characters.", "No Missing Characters",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(selectedFontFamilyName))
            {
                MessageBox.Show("Please select a font using the 'Pick Font' button first.", "No Font Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if this is a regeneration (reuse previous save path)
            if (lastGeneratedPagesStartIndex >= 0 && !string.IsNullOrEmpty(lastGeneratedSavePath))
            {
                try
                {
                    GenerateMissingCharacters(selectedFontFamilyName, lastGeneratedSavePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to regenerate characters:\n{ex.Message}", "Regeneration Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            // First time generation - only show save dialog
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Font Files (*.font)|*.font|All Files (*.*)|*.*";
                saveDialog.Title = "Save Updated Font As";
                saveDialog.FileName = Path.GetFileNameWithoutExtension(ofd.FileName) + "_updated.font";

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    GenerateMissingCharacters(selectedFontFamilyName, saveDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to generate missing characters:\n{ex.Message}", "Generation Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GenerateMissingCharacters(string fontFamilyName, string savePath)
        {
            // Regeneration: if we generated before, always remove old results and redo with current settings
            bool isRegeneration = (lastGeneratedPagesStartIndex >= 0 &&
                                   lastGeneratedSavePath == savePath);

            if (isRegeneration)
            {
                textBoxLogOutput.AppendText("\r\n=== Regenerating Characters (New Offset) ===\r\n");
                textBoxLogOutput.AppendText($"Y Offset: {textBoxYoffset.Text}, Font Size Adj: {textBoxFontSizeAdjust.Text}\r\n");
            }
            else
            {
                textBoxLogOutput.AppendText("\r\n=== Starting Character Generation ===\r\n");
                textBoxLogOutput.AppendText($"Font family: {fontFamilyName}\r\n");
                textBoxLogOutput.AppendText($"Missing characters: {lastDetectedMissingChars.Count}\r\n");
                textBoxLogOutput.AppendText($"Target font: {savePath}\r\n");
            }
            textBoxLogOutput.AppendText("===========================================\r\n\r\n");

            // Save original font parameters before any modifications
            string originalFontName = font.FontName;
            float originalBaseSize = font.BaseSize;
            float originalLineHeight = font.lineHeight;
            float originalNewSomeValue = font.NewSomeValue;
            // On regeneration, use the saved original page count (before first generation added pages)
            int originalPages = isRegeneration && lastOriginalPagesCount >= 0
                ? lastOriginalPagesCount
                : ((font.NewTex != null) ? font.NewTex.Length : font.glyph.Pages);

            textBoxLogOutput.AppendText($"Original font parameters:\r\n");
            textBoxLogOutput.AppendText($"  FontName: {originalFontName}\r\n");
            textBoxLogOutput.AppendText($"  BaseSize: {originalBaseSize}\r\n");
            textBoxLogOutput.AppendText($"  lineHeight: {originalLineHeight}\r\n");
            textBoxLogOutput.AppendText($"  NewSomeValue: {originalNewSomeValue}\r\n");
            textBoxLogOutput.AppendText($"  Pages: {originalPages} (NewTex.Length={font.NewTex?.Length ?? 0}, glyph.Pages={font.glyph.Pages})\r\n\r\n");

            // If regenerating, remove previously added characters and clear their pages for redraw
            int regenerationStartPage = -1;
            if (isRegeneration && lastGeneratedCharCount > 0)
            {
                if (font.glyph.charsNew.Length >= lastGeneratedCharCount &&
                    lastGeneratedPagesStartIndex < font.NewTex.Length)
                {
                    // Remove previously generated characters (using actual count, not current missing list)
                    Array.Resize(ref font.glyph.charsNew, font.glyph.charsNew.Length - lastGeneratedCharCount);
                    font.glyph.CharCount -= lastGeneratedCharCount;

                    // Clear the generated NEW pages (fill with transparent)
                    regenerationStartPage = lastGeneratedPagesStartIndex;
                    for (int p = regenerationStartPage; p < regenerationStartPage + lastGeneratedPagesCount; p++)
                    {
                        if (p < font.NewTex.Length)
                        {
                            using (Bitmap clearBitmap = new Bitmap(512, 512))
                            {
                                string clearDdsPath = Path.Combine(Path.GetDirectoryName(savePath),
                                    Path.GetFileNameWithoutExtension(savePath) + $"_page{p}.dds");
                                SaveBitmapAsDDS(clearBitmap, clearDdsPath, p);
                                if (File.Exists(clearDdsPath))
                                    ReplaceTexture(clearDdsPath, font.NewTex[p]);
                            }
                        }
                    }

                    // Also restore the existing page if it was modified to fill slots
                    if (lastModifiedExistingPageIndex >= 0 &&
                        lastModifiedPageOriginalData != null &&
                        lastModifiedExistingPageIndex < font.NewTex.Length)
                    {
                        // Restore original page content
                        Array.Copy(lastModifiedPageOriginalData, font.NewTex[lastModifiedExistingPageIndex].Tex.Content,
                            Math.Min(lastModifiedPageOriginalData.Length, font.NewTex[lastModifiedExistingPageIndex].Tex.Content.Length));
                        lastModifiedPageOriginalData = null;
                        lastModifiedExistingPageIndex = -1;
                        textBoxLogOutput.AppendText($"Restored existing page {lastModifiedExistingPageIndex} to original state\r\n");
                    }

                    fillTableofTextures(font);

                    textBoxLogOutput.AppendText($"Cleared {lastGeneratedPagesCount} new pages (index {regenerationStartPage}-{regenerationStartPage + lastGeneratedPagesCount - 1}) for redraw\r\n");
                    textBoxLogOutput.AppendText($"Removed {lastGeneratedCharCount} previously generated characters\r\n");
                }
                else
                {
                    // Cannot safely clean up previous generation — reset and do fresh generation
                    textBoxLogOutput.AppendText("WARNING: Cannot clean up previous generation. Doing fresh generation.\r\n");
                    isRegeneration = false;
                    lastGeneratedPagesStartIndex = -1;
                    lastGeneratedPagesCount = 0;
                    lastGeneratedCharCount = 0;
                }
            }

            // Ensure charsNew is initialized
            int initialCharCount = 0;
            if (font.glyph.charsNew == null)
            {
                font.glyph.charsNew = new FontClass.ClassFont.TRectNew[0];
                textBoxLogOutput.AppendText("Initialized charsNew array\r\n");
            }
            else
            {
                initialCharCount = font.glyph.charsNew.Length;
                textBoxLogOutput.AppendText($"Existing charsNew array length: {initialCharCount}\r\n");
            }

            // Load the source font: from file if a path is set, otherwise from system fonts
            System.Drawing.FontFamily fontFamily;
            System.Drawing.Text.PrivateFontCollection fontCollection = null;
            if (!string.IsNullOrEmpty(selectedFontFilePath) && File.Exists(selectedFontFilePath))
            {
                fontCollection = new System.Drawing.Text.PrivateFontCollection();
                fontCollection.AddFontFile(selectedFontFilePath);
                fontFamily = fontCollection.Families[0];
            }
            else
            {
                fontFamily = new System.Drawing.FontFamily(fontFamilyName);
            }

            // Get character size from existing font (analyze Chinese characters)
            int charWidth = 28;
            int charHeight = 27;
            int fontSize = 27;
            int xAdvance = 25;
            Dictionary<float, int> xoffsetStats = new Dictionary<float, int>();
            Dictionary<float, int> yoffsetStats = new Dictionary<float, int>();
            Dictionary<float, int> xadvanceStats = new Dictionary<float, int>();

            if (font.glyph.charsNew != null && font.glyph.charsNew.Length > 0)
            {
                // Find the most common character parameters among Chinese characters (U+4E00-U+9FFF)
                var sizeStats = new Dictionary<string, int>();
                int chineseCharCount = 0;

                foreach (var ch in font.glyph.charsNew)
                {
                    if (ch.charId >= 0x4E00 && ch.charId <= 0x9FFF)  // CJK Unified Ideographs
                    {
                        string sizeKey = $"{ch.CharWidth}x{ch.CharHeight}";
                        if (sizeStats.ContainsKey(sizeKey))
                            sizeStats[sizeKey]++;
                        else
                            sizeStats[sizeKey] = 1;

                        if (xoffsetStats.ContainsKey(ch.XOffset))
                            xoffsetStats[ch.XOffset]++;
                        else
                            xoffsetStats[ch.XOffset] = 1;

                        if (yoffsetStats.ContainsKey(ch.YOffset))
                            yoffsetStats[ch.YOffset]++;
                        else
                            yoffsetStats[ch.YOffset] = 1;

                        if (xadvanceStats.ContainsKey(ch.XAdvance))
                            xadvanceStats[ch.XAdvance]++;
                        else
                            xadvanceStats[ch.XAdvance] = 1;

                        chineseCharCount++;
                    }
                }

                if (sizeStats.Count > 0)
                {
                    var mostCommonSize = sizeStats.OrderByDescending(x => x.Value).First();
                    string[] dimensions = mostCommonSize.Key.Split('x');
                    charWidth = int.Parse(dimensions[0]);
                    charHeight = int.Parse(dimensions[1]);
                    fontSize = charHeight;

                    float mostCommonXOffset = xoffsetStats.OrderByDescending(x => x.Value).First().Key;
                    float mostCommonYOffset = yoffsetStats.OrderByDescending(x => x.Value).First().Key;
                    float mostCommonXAdvance = xadvanceStats.OrderByDescending(x => x.Value).First().Key;
                    xAdvance = (int)mostCommonXAdvance;

                    textBoxLogOutput.AppendText($"Analyzed {chineseCharCount} Chinese characters\r\n");
                    textBoxLogOutput.AppendText($"Most common size: {mostCommonSize.Key} ({mostCommonSize.Value} chars)\r\n");
                    textBoxLogOutput.AppendText($"Common XOffset: {mostCommonXOffset}, YOffset: {mostCommonYOffset}, XAdvance: {mostCommonXAdvance}\r\n");
                }
                else
                {
                    // Fallback to first character if no Chinese characters found
                    charWidth = (int)font.glyph.charsNew[0].CharWidth;
                    charHeight = (int)font.glyph.charsNew[0].CharHeight;
                    fontSize = charHeight;
                    xAdvance = (int)font.glyph.charsNew[0].XAdvance;
                }
            }

            // Add padding around each glyph cell to prevent overlap
            int padding = 0;
            int cellWidth = charWidth + padding * 2;
            int cellHeight = charHeight + padding * 2;

            textBoxLogOutput.AppendText($"Character size: {charWidth}x{charHeight}\r\n");
            textBoxLogOutput.AppendText($"Cell size (with padding): {cellWidth}x{cellHeight}\r\n");
            textBoxLogOutput.AppendText($"Font size for drawing: {fontSize}\r\n");
            textBoxLogOutput.AppendText($"XAdvance: {xAdvance}\r\n");

            // Calculate texture layout using cell size (includes padding)
            int charsPerRow = 512 / cellWidth;
            int charsPerCol = 512 / cellHeight;
            int charsPerTexture = charsPerRow * charsPerCol;

            // --- Check if we can fill remaining space on the last page ---
            // Use FNT table to find the last character on the last page, then pixel-verify from there
            int lastPageRemainingSlots = 0;
            int lastPageFirstEmptySlot = -1;
            int lastToolPageIndex = -1;
            Bitmap existingPageBitmap = null;
            bool modifiedExistingPage = false;

            if (!isRegeneration && font.NewTex != null && font.NewTex.Length > 0)
            {
                lastToolPageIndex = font.NewTex.Length - 1;

                // Query FNT table: find the last occupied slot on this page
                int lastOccupiedSlot = -1;
                foreach (var ch in font.glyph.charsNew)
                {
                    if (ch.TexNum == lastToolPageIndex)
                    {
                        int slot = (int)(ch.YStart / cellHeight) * charsPerRow + (int)(ch.XStart / cellWidth);
                        if (slot > lastOccupiedSlot)
                            lastOccupiedSlot = slot;
                    }
                }

                if (lastOccupiedSlot >= 0 && lastOccupiedSlot < charsPerTexture - 1)
                {
                    // Load the last page bitmap and verify from lastOccupiedSlot+1 onward
                    existingPageBitmap = LoadPageAsBitmap(lastToolPageIndex);

                    if (existingPageBitmap != null)
                    {
                        lastPageFirstEmptySlot = FindFirstEmptySlotFrom(existingPageBitmap, cellWidth, cellHeight,
                            charsPerRow, charsPerCol, lastOccupiedSlot + 1);

                        if (lastPageFirstEmptySlot >= 0)
                        {
                            lastPageRemainingSlots = charsPerTexture - lastPageFirstEmptySlot;
                            textBoxLogOutput.AppendText(
                                $"Last page {lastToolPageIndex}: last char at slot {lastOccupiedSlot}, " +
                                $"first empty slot at {lastPageFirstEmptySlot}, " +
                                $"{lastPageRemainingSlots} remaining\r\n");
                        }
                        else
                        {
                            textBoxLogOutput.AppendText(
                                $"Last page {lastToolPageIndex}: slots after {lastOccupiedSlot} are occupied. Creating new pages.\r\n");
                            existingPageBitmap.Dispose();
                            existingPageBitmap = null;
                        }
                    }
                    else
                    {
                        textBoxLogOutput.AppendText(
                            $"WARNING: Could not decode last page {lastToolPageIndex}. Creating new pages instead.\r\n");
                    }
                }
                else if (lastOccupiedSlot < 0)
                {
                    textBoxLogOutput.AppendText(
                        $"Last page {lastToolPageIndex}: no characters found on this page. Skipping fill.\r\n");
                }
                else
                {
                    textBoxLogOutput.AppendText(
                        $"Last page {lastToolPageIndex} is fully occupied. Creating new pages.\r\n");
                }
            }

            int charsForExistingPage = Math.Min(lastPageRemainingSlots, lastDetectedMissingChars.Count);
            int charsForNewPages = lastDetectedMissingChars.Count - charsForExistingPage;

            int numTexturesNeeded = (charsForNewPages > 0)
                ? (int)Math.Ceiling((double)charsForNewPages / charsPerTexture)
                : 0;

            textBoxLogOutput.AppendText($"Chars per texture: {charsPerTexture}\r\n");
            textBoxLogOutput.AppendText($"Chars for existing page: {charsForExistingPage}\r\n");
            textBoxLogOutput.AppendText($"Chars for new pages: {charsForNewPages}\r\n");
            textBoxLogOutput.AppendText($"New textures needed: {numTexturesNeeded}\r\n\r\n");

            // Generate new texture pages
            List<int> newTextureIndices = new List<int>();  // Store the actual indices used for generation
            int currentTextureIndex = (regenerationStartPage >= 0) ? regenerationStartPage : font.TexCount;

            textBoxLogOutput.AppendText($"Starting texture index: {currentTextureIndex} (TexCount: {font.TexCount}, glyph.Pages: {font.glyph.Pages})\r\n");

            // --- Fill remaining slots on existing last tool-generated page ---
            if (charsForExistingPage > 0 && existingPageBitmap != null)
            {
                textBoxLogOutput.AppendText($"Filling {charsForExistingPage} chars on existing page {lastToolPageIndex}...\r\n");

                // Backup original page data for potential restoration during regeneration
                if (font.NewTex[lastToolPageIndex].Tex.Content != null)
                {
                    lastModifiedPageOriginalData = new byte[font.NewTex[lastToolPageIndex].Tex.Content.Length];
                    Array.Copy(font.NewTex[lastToolPageIndex].Tex.Content, lastModifiedPageOriginalData,
                        lastModifiedPageOriginalData.Length);
                }
                lastModifiedExistingPageIndex = lastToolPageIndex;

                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(existingPageBitmap))
                {
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                    int fontSizeAdjustment = 0;
                    if (int.TryParse(textBoxFontSizeAdjust.Text, out int parsedSizeAdjust))
                        fontSizeAdjustment = parsedSizeAdjust;
                    float adjustedFontSize = Math.Max(1, fontSize + fontSizeAdjustment);

                    using (System.Drawing.Font drawFont = new System.Drawing.Font(fontFamily, adjustedFontSize, selectedFontStyle, GraphicsUnit.Pixel))
                    {
                        int filledCount = 0;
                        for (int i = 0; i < lastDetectedMissingChars.Count && filledCount < charsForExistingPage; i++)
                        {
                            char c = lastDetectedMissingChars[i];

                            // Skip if character already exists in font
                            if (font.glyph.charsNew != null)
                            {
                                bool charExists = false;
                                foreach (var existingChar in font.glyph.charsNew)
                                {
                                    if (existingChar.charId == c) { charExists = true; break; }
                                }
                                if (charExists) continue;
                            }

                            int slotIndex = lastPageFirstEmptySlot + filledCount;
                            int row = slotIndex / charsPerRow;
                            int col = slotIndex % charsPerRow;

                            // Safety: don't exceed page capacity
                            if (row >= charsPerCol || col >= charsPerRow) break;

                            int x = col * cellWidth + padding;
                            int y = row * cellHeight + padding;

                            float mostCommonXOffset = xoffsetStats.Count > 0
                                ? xoffsetStats.OrderByDescending(entry => entry.Value).First().Key : -1;
                            float mostCommonYOffset = yoffsetStats.Count > 0
                                ? yoffsetStats.OrderByDescending(entry => entry.Value).First().Key : 4;

                            int yOffsetAdjustment = 0;
                            if (int.TryParse(textBoxYoffset.Text, out int parsedOffset))
                                yOffsetAdjustment = parsedOffset;
                            g.DrawString(c.ToString(), drawFont, Brushes.White, x, y - yOffsetAdjustment,
                                StringFormat.GenericTypographic);

                            FontClass.ClassFont.TRectNew newChar = new FontClass.ClassFont.TRectNew
                            {
                                charId = c,
                                XStart = (short)x,
                                XEnd = (short)(x + charWidth),
                                YStart = (short)y,
                                YEnd = (short)(y + charHeight),
                                CharWidth = (byte)charWidth,
                                CharHeight = (byte)charHeight,
                                XOffset = (short)mostCommonXOffset,
                                YOffset = (short)mostCommonYOffset,
                                XAdvance = (short)xAdvance,
                                Channel = 15,
                                TexNum = lastToolPageIndex
                            };

                            Array.Resize(ref font.glyph.charsNew, font.glyph.charsNew.Length + 1);
                            font.glyph.charsNew[font.glyph.charsNew.Length - 1] = newChar;
                            font.glyph.CharCount++;

                            filledCount++;
                        }

                        textBoxLogOutput.AppendText($"  Filled {filledCount} characters on page {lastToolPageIndex}\r\n");
                    }
                }

                // Re-save the modified existing page as DDS
                string existingPageDdsPath = Path.Combine(Path.GetDirectoryName(savePath),
                    Path.GetFileNameWithoutExtension(savePath) + $"_page{lastToolPageIndex}.dds");
                SaveBitmapAsDDS(existingPageBitmap, existingPageDdsPath, lastToolPageIndex);

                // Update the in-memory texture data for this page
                ReplaceTexture(existingPageDdsPath, font.NewTex[lastToolPageIndex]);

                textBoxLogOutput.AppendText($"  Updated: {Path.GetFileName(existingPageDdsPath)}\r\n");
                modifiedExistingPage = true;

                newTextureIndices.Add(lastToolPageIndex);

                existingPageBitmap.Dispose();
                existingPageBitmap = null;
            }

            for (int texIndex = 0; texIndex < numTexturesNeeded; texIndex++)
            {
                int startChar = charsForExistingPage + texIndex * charsPerTexture;
                int endChar = Math.Min(startChar + charsPerTexture, lastDetectedMissingChars.Count);
                int charsInThisTexture = endChar - startChar;

                textBoxLogOutput.AppendText($"Generating texture {texIndex + 1}/{numTexturesNeeded} (chars {startChar + 1}-{endChar})...\r\n");

                // Create bitmap for this texture
                Bitmap textureBitmap = new Bitmap(512, 512);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(textureBitmap))
                {
                    g.Clear(Color.Transparent);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                    // Apply font size adjustment from textBoxFontSizeAdjust
                    int fontSizeAdjustment = 0;
                    if (int.TryParse(textBoxFontSizeAdjust.Text, out int parsedSizeAdjust))
                        fontSizeAdjustment = parsedSizeAdjust;
                    float adjustedFontSize = Math.Max(1, fontSize + fontSizeAdjustment);

                    using (System.Drawing.Font drawFont = new System.Drawing.Font(fontFamily, adjustedFontSize, selectedFontStyle, GraphicsUnit.Pixel))
                    {
                        for (int i = 0; i < charsInThisTexture; i++)
                        {
                            char c = lastDetectedMissingChars[startChar + i];

                            // Check if character already exists in font to prevent duplicates
                            // Skip this check in regeneration mode since we already removed old chars
                            if (!isRegeneration && font.glyph.charsNew != null)
                            {
                                bool charExists = false;
                                foreach (var existingChar in font.glyph.charsNew)
                                {
                                    if (existingChar.charId == c)
                                    {
                                        charExists = true;
                                        break;
                                    }
                                }

                                if (charExists)
                                {
                                    textBoxLogOutput.AppendText($"  Skipping existing character: [{c}] U+{(int)c:X4}\r\n");
                                    continue;
                                }
                            }

                            int row = i / charsPerRow;
                            int col = i % charsPerRow;

                            int x = col * cellWidth + padding;
                            int y = row * cellHeight + padding;

                            // Get most common offsets from analysis (used for character metadata only)
                            float mostCommonXOffset = -1;  // Default
                            float mostCommonYOffset = 4;   // Default

                            if (xoffsetStats.Count > 0)
                                mostCommonXOffset = xoffsetStats.OrderByDescending(entry => entry.Value).First().Key;
                            if (yoffsetStats.Count > 0)
                                mostCommonYOffset = yoffsetStats.OrderByDescending(entry => entry.Value).First().Key;

                            // Draw character at grid position (x, y) - xoffset/yoffset are
                            // for text layout positioning only, NOT texture positioning.
                            // Use GenericTypographic to avoid GDI+ default 1/6 em padding.
                            // Adjust Y position using user-specified offset from textBoxYoffset
                            int yOffsetAdjustment = 0;
                            if (int.TryParse(textBoxYoffset.Text, out int parsedOffset))
                                yOffsetAdjustment = parsedOffset;
                            g.DrawString(c.ToString(), drawFont, Brushes.White, x, y - yOffsetAdjustment, StringFormat.GenericTypographic);

                            // Add to font data
                            FontClass.ClassFont.TRectNew newChar = new FontClass.ClassFont.TRectNew
                            {
                                charId = c,
                                XStart = (short)x,
                                XEnd = (short)(x + charWidth),
                                YStart = (short)y,
                                YEnd = (short)(y + charHeight),
                                CharWidth = (byte)charWidth,
                                CharHeight = (byte)charHeight,
                                XOffset = (short)mostCommonXOffset,
                                YOffset = (short)mostCommonYOffset,
                                XAdvance = (short)xAdvance,
                                Channel = 15,
                                TexNum = currentTextureIndex
                            };

                            // Add to font
                            Array.Resize(ref font.glyph.charsNew, font.glyph.charsNew.Length + 1);
                            font.glyph.charsNew[font.glyph.charsNew.Length - 1] = newChar;
                            font.glyph.CharCount++;
                        }
                    }
                }

                // Save texture as DDS using Magick.NET (DXT5 compressed)
                // Use same naming convention as SaveFontWithNewPages so import can find the files
                string texturePath = Path.Combine(Path.GetDirectoryName(savePath),
                    Path.GetFileNameWithoutExtension(savePath) + $"_page{currentTextureIndex}.dds");

                SaveBitmapAsDDS(textureBitmap, texturePath, currentTextureIndex);
                textureBitmap.Dispose();

                // Record the actual index used for this texture
                newTextureIndices.Add(currentTextureIndex);

                textBoxLogOutput.AppendText($"  Saved: {Path.GetFileName(texturePath)}\r\n");

                if (!isRegeneration)
                {
                    // Add new page to font
                    font.glyph.Pages++;
                }
                currentTextureIndex++;
            }

            // Load generated DDS textures into font.NewTex so they're available for preview/save
            int oldTexCount = (font.NewTex != null) ? font.NewTex.Length : 0;

            // Calculate total pages needed — regeneration may need more pages than before
            // if missing chars count changed (e.g. user switched profile or re-detected)
            int maxIndexNeeded = oldTexCount;
            foreach (int idx in newTextureIndices)
                if (idx + 1 > maxIndexNeeded) maxIndexNeeded = idx + 1;
            int totalTexCount = maxIndexNeeded;

            TextureClass.NewT3Texture[] expandedTex = new TextureClass.NewT3Texture[totalTexCount];

            // Copy existing textures
            for (int i = 0; i < oldTexCount; i++)
            {
                expandedTex[i] = font.NewTex[i];
            }

            // Load each newly generated DDS into the appropriate texture slots
            string saveDir = Path.GetDirectoryName(savePath);
            string baseName = Path.GetFileNameWithoutExtension(savePath);
            int newSlotOffset = 0; // Tracks how many NEW slots we've used (excluding modified existing pages)
            for (int i = 0; i < newTextureIndices.Count; i++)
            {
                int texIdx = newTextureIndices[i];

                // Skip the modified existing page - it was already updated in-place above
                if (modifiedExistingPage && texIdx == lastToolPageIndex)
                    continue;

                string ddsPath = Path.Combine(saveDir, $"{baseName}_page{texIdx}.dds");

                int targetSlot;
                if (isRegeneration)
                {
                    // Regeneration: overwrite existing slot
                    targetSlot = texIdx;
                }
                else
                {
                    // New generation: append after existing textures
                    targetSlot = oldTexCount + newSlotOffset;
                    newSlotOffset++;
                }

                // Initialize slot if it doesn't exist yet (can happen during regeneration
                // when more pages are needed than before)
                if (targetSlot >= oldTexCount || expandedTex[targetSlot] == null)
                {
                    if (oldTexCount > 0)
                        expandedTex[targetSlot] = new TextureClass.NewT3Texture(font.NewTex[0]);
                    else
                    {
                        expandedTex[targetSlot] = new TextureClass.NewT3Texture();
                        expandedTex[targetSlot].Tex = new TextureClass.NewT3Texture.TextureInfo();
                    }
                }

                if (File.Exists(ddsPath))
                {
                    ReplaceTexture(ddsPath, expandedTex[targetSlot]);
                    textBoxLogOutput.AppendText($"  Loaded DDS into texture slot {targetSlot}: {Path.GetFileName(ddsPath)}\r\n");
                }
                else
                {
                    textBoxLogOutput.AppendText($"  WARNING: DDS not found for slot {targetSlot}: {Path.GetFileName(ddsPath)}\r\n");
                }
            }

            font.NewTex = expandedTex;
            font.TexCount = totalTexCount;
            font.glyph.Pages = totalTexCount;
            fillTableofTextures(font);
            textBoxLogOutput.AppendText($"Font textures updated: {oldTexCount} -> {totalTexCount}\r\n");

            // Record generation info for potential regeneration
            lastGeneratedPagesStartIndex = isRegeneration ? regenerationStartPage : originalPages;
            lastGeneratedPagesCount = numTexturesNeeded;
            if (!isRegeneration)
                lastOriginalPagesCount = originalPages;
            lastGeneratedFontFamily = fontFamilyName;
            lastGeneratedSavePath = savePath;

            // Clean up
            fontCollection?.Dispose();

            // Sync CharCount with actual charsNew length to ensure accuracy
            font.glyph.CharCount = font.glyph.charsNew.Length;

            // Calculate how many characters were actually generated
            int generatedCharCount = font.glyph.CharCount - initialCharCount;
            lastGeneratedCharCount = generatedCharCount;

            // Log character count statistics
            textBoxLogOutput.AppendText("\r\n=== Character Count Statistics ===\r\n");
            textBoxLogOutput.AppendText($"Initial characters: {initialCharCount}\r\n");
            textBoxLogOutput.AppendText($"Generated characters: {lastDetectedMissingChars.Count}\r\n");
            textBoxLogOutput.AppendText($"Actual generated: {generatedCharCount}\r\n");
            textBoxLogOutput.AppendText($"Expected total: {initialCharCount + lastDetectedMissingChars.Count}\r\n");
            textBoxLogOutput.AppendText($"Actual charsNew.Length: {font.glyph.charsNew.Length}\r\n");
            textBoxLogOutput.AppendText($"font.glyph.CharCount: {font.glyph.CharCount}\r\n");

            if (font.glyph.CharCount != initialCharCount + lastDetectedMissingChars.Count)
            {
                textBoxLogOutput.AppendText("WARNING: Character count mismatch detected!\r\n");
            }
            else
            {
                textBoxLogOutput.AppendText("Character count matches expected value.\r\n");
            }
            textBoxLogOutput.AppendText("==========================================\r\n\r\n");

            // Save FNT file with character data
            SaveFontWithNewPages(savePath, initialCharCount, originalFontName, originalBaseSize, originalLineHeight, originalNewSomeValue, originalPages);

            // Save complete .font file (contains all data including original + new characters)
            Methods.DeleteCurrentFile(savePath);
            using (FileStream fs = new FileStream(savePath, FileMode.Create))
            {
                SaveFont(fs, font);
            }
            encFunc(savePath);

            // Refresh the grid to show all characters (old + newly generated)
            fillTableofCoordinates(font, true);

            textBoxLogOutput.AppendText("\r\n=== Generation Complete ===\r\n");
            textBoxLogOutput.AppendText($"Generated {lastDetectedMissingChars.Count} new characters\r\n");
            textBoxLogOutput.AppendText($"Total characters in font: {font.glyph.CharCount}\r\n");
            textBoxLogOutput.AppendText($"Created {numTexturesNeeded} new texture pages (DXT5 compressed DDS)\r\n");
            textBoxLogOutput.AppendText($"Font file saved to: {savePath}\r\n");
            textBoxLogOutput.AppendText($"FNT file saved to: {Path.ChangeExtension(savePath, ".fnt")}\r\n");

            MessageBox.Show($"Successfully generated {lastDetectedMissingChars.Count} missing characters!\r\n\r\n" +
                $"New textures and font saved to:\r\n{savePath}\r\n\r\n" +
                $"Total characters: {font.glyph.CharCount}",
                "Generation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveBitmapAsDDS(Bitmap bitmap, string outputPath, int pageIndex)
        {
            try
            {
                // Save Bitmap to a memory stream first
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;

                    // Load with ImageMagick
                    using (MagickImage magickImage = new MagickImage(ms))
                    {
                        // Set the compression format to DXT5 (same as original font)
                        magickImage.Settings.SetDefine(MagickFormat.Dds, "compression", "dxt5");
                        // Disable mipmaps - 0 means no mipmaps, only the base level
                        magickImage.Settings.SetDefine(MagickFormat.Dds, "mipmaps", "0");

                        // Save as DDS
                        magickImage.Write(outputPath, MagickFormat.Dds);
                    }
                }

                // Verify the file was created
                if (File.Exists(outputPath))
                {
                    FileInfo fileInfo = new FileInfo(outputPath);
                    textBoxLogOutput.AppendText($"  Saved DDS: {Path.GetFileName(outputPath)} ({fileInfo.Length} bytes)\r\n");
                    textBoxLogOutput.AppendText($"  Format: DXT5 compressed\r\n");
                }
                else
                {
                    throw new Exception("DDS file was not created");
                }
            }
            catch (Exception ex)
            {
                // Fallback to PNG if DDS save fails
                textBoxLogOutput.AppendText($"  Warning: DDS save failed: {ex.Message}\r\n");
                textBoxLogOutput.AppendText($"  Falling back to PNG format...\r\n");

                string pngPath = Path.ChangeExtension(outputPath, ".png");
                bitmap.Save(pngPath, System.Drawing.Imaging.ImageFormat.Png);
                textBoxLogOutput.AppendText($"  Saved PNG: {Path.GetFileName(pngPath)}\r\n");
            }
        }

        private void SaveFontWithNewPages(string savePath, int startCharIndex = 0,
            string originalFontName = null, float originalBaseSize = 0,
            float originalLineHeight = 0, float originalNewSomeValue = 0, int originalPages = 0)
        {
            // Use original font parameters for FNT file
            string fontName = originalFontName ?? font.FontName;
            float baseSize = (originalNewSomeValue > 0) ? originalNewSomeValue : originalBaseSize;
            float lineHeight = (originalLineHeight > 0) ? originalLineHeight : originalBaseSize;

            // Only list NEW pages in the FNT (pages that were generated, not the original ones)
            int newPageCount = font.glyph.Pages - originalPages;

            // Create FNT file path
            string fntPath = Path.ChangeExtension(savePath, ".fnt");

            // Calculate how many characters to include in FNT
            int fntCharCount = font.glyph.CharCount - startCharIndex;

            // Export FNT file (only generated characters)
            using (FileStream fs = new FileStream(fntPath, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
            {
                // Write header - only list NEW pages count
                sw.WriteLine($"info face=\"{fontName}\" size={originalBaseSize} bold=0 italic=0 charset=\"\" unicode=1");
                sw.WriteLine($"common lineHeight={lineHeight} base={baseSize} pages={newPageCount}");

                // Write only NEW page info (page id is 0-indexed relative to new pages)
                for (int i = 0; i < newPageCount; i++)
                {
                    int absolutePageIndex = originalPages + i;
                    string pageFileName = $"{Path.GetFileNameWithoutExtension(savePath)}_page{absolutePageIndex}.dds";
                    sw.WriteLine($"page id={i} file=\"{pageFileName}\"");
                }

                sw.WriteLine($"chars count={fntCharCount}");

                // Write only generated characters (from startCharIndex to end)
                // Adjust TexNum to be relative to new pages (subtract originalPages)
                for (int i = startCharIndex; i < font.glyph.CharCount; i++)
                {
                    var charData = font.glyph.charsNew[i];
                    if (charData.charId != 0)
                    {
                        int adjustedPage = charData.TexNum - originalPages;
                        sw.WriteLine($"char id={charData.charId} x={charData.XStart} y={charData.YStart} " +
                            $"width={charData.CharWidth} height={charData.CharHeight} " +
                            $"xoffset={charData.XOffset} yoffset={charData.YOffset} " +
                            $"xadvance={charData.XAdvance} page={adjustedPage} chnl={charData.Channel}");
                    }
                }
            }

            textBoxLogOutput.AppendText($"  FNT file: {Path.GetFileName(fntPath)}\r\n");
            textBoxLogOutput.AppendText($"  Font name: {fontName}\r\n");
            textBoxLogOutput.AppendText($"  Size: {originalBaseSize}\r\n");
            textBoxLogOutput.AppendText($"  Base: {baseSize}\r\n");
            textBoxLogOutput.AppendText($"  LineHeight: {lineHeight}\r\n");
            textBoxLogOutput.AppendText($"  New pages in FNT: {newPageCount}\r\n");
            textBoxLogOutput.AppendText($"  Characters in FNT: {fntCharCount} (generated only)\r\n");
            textBoxLogOutput.AppendText($"  Total characters in font: {font.glyph.CharCount}\r\n");
        }

        private static void ShowErrorDialog(string title, string message)
        {
            using (Form dlg = new Form())
            {
                dlg.Text = title;
                dlg.Size = new Size(520, 340);
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;

                var textBox = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    Dock = DockStyle.Top,
                    Height = 240,
                    Text = message,
                    Font = new Font("Consolas", 9F),
                    BackColor = SystemColors.Window,
                    WordWrap = true
                };

                var btnCopy = new Button
                {
                    Text = "Copy",
                    DialogResult = DialogResult.None,
                    Width = 80,
                    Height = 30
                };
                btnCopy.Click += (s, e) =>
                {
                    textBox.SelectAll();
                    textBox.Copy();
                    btnCopy.Text = "Copied!";
                };

                var btnOk = new Button
                {
                    Text = "OK",
                    DialogResult = DialogResult.OK,
                    Width = 80,
                    Height = 30
                };

                var bottomPanel = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 44
                };
                bottomPanel.Controls.Add(btnOk);
                bottomPanel.Controls.Add(btnCopy);
                btnOk.Location = new Point(520 - 80 - 12, 8);
                btnCopy.Location = new Point(520 - 80 - 12 - 80 - 8, 8);

                dlg.Controls.Add(textBox);
                dlg.Controls.Add(bottomPanel);
                dlg.AcceptButton = btnOk;

                dlg.ShowDialog();
            }
        }
    }
}