using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TTG_Tools.ClassesStructs;

namespace TTG_Tools
{
    public partial class FontEditor
    {
        private void toolStripImportFNT_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFD = new OpenFileDialog();
            openFD.Filter = "fnt files (*.fnt)|*.fnt";

            if (openFD.ShowDialog() == DialogResult.OK)
            {
                FileInfo fi = new FileInfo(openFD.FileName);

                string[] strings = File.ReadAllLines(fi.FullName);

                // First pass: count actual character definitions
                int actualCharCount = 0;
                for (int n = 0; n < strings.Length; n++)
                {
                    if (strings[n].Contains("char id"))
                    {
                        actualCharCount++;
                    }
                }

                // If font is null, create a new one for FNT import
                if (font == null)
                {
                    font = new ClassesStructs.FontClass.ClassFont();
                    font.NewFormat = true;
                    font.NewTex = new TextureClass.NewT3Texture[0];
                    font.glyph = new ClassesStructs.FontClass.ClassFont.GlyphInfo();
                    font.glyph.Pages = 0;
                    font.glyph.CharCount = 0;
                    font.glyph.charsNew = new ClassesStructs.FontClass.ClassFont.TRectNew[0];

                    // Initialize required fields
                    check_header = new byte[4];
                    tmpHeader = new byte[4];
                    font.One = 0x31; // Required for 6VSM NewFormat
                    font.hasLineHeight = false;
                    font.blockSize = true;
                    font.headerSize = 0;
                    font.texSize = 0;
                    font.hasOneFloatValue = false;
                    font.LastZero = 0;
                    font.NewSomeValue = 0;

                    // Initialize default 6VSM elements for game runtime compatibility.
                    InitializeDefault6VsmElements(font);

                    // Set default header based on FNT format
                    // 6VSM is the most common format for newer games
                    check_header = Encoding.ASCII.GetBytes("6VSM");
                    tmpHeader = Encoding.ASCII.GetBytes("6VSM");

                    textBoxLogOutput.AppendText("Created new font object for FNT import.\r\n");
                    textBoxLogOutput.AppendText("Font format: 6VSM (NewFormat)\r\n");
                }

                int ch = -1;
                int existingCharCount = 0;

                // Initialize charsNew if we're in NewFormat and it's null
                if (font.NewFormat)
                {
                    if (font.glyph.charsNew == null || font.glyph.charsNew.Length == 0)
                    {
                        // Empty font - create new array
                        font.glyph.charsNew = new FontClass.ClassFont.TRectNew[actualCharCount];
                        font.glyph.CharCount = actualCharCount;
                        existingCharCount = 0;
                    }
                    else
                    {
                        // Append mode: keep existing characters and add new ones
                        existingCharCount = font.glyph.CharCount;
                        int totalCount = existingCharCount + actualCharCount;

                        // Create new array with combined size
                        FontClass.ClassFont.TRectNew[] tempChars = font.glyph.charsNew;
                        font.glyph.charsNew = new FontClass.ClassFont.TRectNew[totalCount];
                        font.glyph.CharCount = totalCount;

                        // Copy existing characters to new array
                        Array.Copy(tempChars, 0, font.glyph.charsNew, 0, existingCharCount);

                        textBoxLogOutput.AppendText($"Appending {actualCharCount} new characters to existing {existingCharCount} characters. Total: {totalCount}\r\n");
                    }
                }
                else
                {
                    // Old format
                    if (font.glyph.chars == null || font.glyph.chars.Length == 0)
                    {
                        existingCharCount = 0;
                    }
                    else
                    {
                        existingCharCount = font.glyph.CharCount;
                    }
                }

                // Initialize ch to existing count - 1, so first ++ will be existingCount
                ch = existingCharCount - 1;


                //Check for xml tags and removing it for comfortable searching needed data (useful for xml fnt files)
                for (int n = 0; n < strings.Length; n++)
                {
                    if ((strings[n].IndexOf('<') >= 0) || (strings[n].IndexOf('<') >= 0 && strings[n].IndexOf('/') > 0))
                    {
                        strings[n] = strings[n].Remove(strings[n].IndexOf('<'), 1);
                        if (strings[n].IndexOf('/') >= 0) strings[n] = strings[n].Remove(strings[n].IndexOf('/'), 1);
                    }
                    if (strings[n].IndexOf('>') >= 0 || (strings[n].IndexOf('/') >= 0 && strings[n + 1].IndexOf('>') > 0))
                    {
                        strings[n] = strings[n].Remove(strings[n].IndexOf('>'), 1);
                        if (strings[n].IndexOf('/') >= 0) strings[n] = strings[n].Remove(strings[n].IndexOf('/'), 1);
                    }
                    if (strings[n].IndexOf('"') >= 0)
                    {
                        while (strings[n].IndexOf('"') >= 0) strings[n] = strings[n].Remove(strings[n].IndexOf('"'), 1);
                    }
                }

                if (font.NewFormat)
                {
                    TextureClass.NewT3Texture[] tmpNewTex = null;
                    int existingTexCount = 0;
                    int fntPageCount = 0;

                    if (font.NewTex != null && font.NewTex.Length > 0)
                    {
                        existingTexCount = font.NewTex.Length;
                        textBoxLogOutput.AppendText($"Existing texture count: {existingTexCount}\r\n");
                    }
                    else
                    {
                        textBoxLogOutput.AppendText("No existing textures. Creating new texture array.\r\n");
                    }

                    for (int m = 0; m < strings.Length; m++)
                    {
                        // Read font name - only apply if the font doesn't already have a valid name
                        // (preserve the original .font file's FontName, e.g. "CheapSignage_50")
                        if (strings[m].ToLower().Contains("info face"))
                        {
                            string faceName = GetFntAttributeValue(strings[m], "face");
                            if (!string.IsNullOrEmpty(faceName) && (string.IsNullOrEmpty(font.FontName) || font.FontName == "NewFont"))
                                font.FontName = faceName;
                        }

                        if (strings[m].ToLower().Contains("common lineheight"))
                        {
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });
                            for (int k = 0; k < splitted.Length; k++)
                            {
                                switch (splitted[k].ToLower())
                                {
                                    case "lineheight":
                                        font.BaseSize = Convert.ToSingle(splitted[k + 1]);

                                        if(check_header != null && Encoding.ASCII.GetString(check_header) == "5VSM" && font.hasLineHeight)
                                        {
                                            font.lineHeight = Convert.ToSingle(splitted[k + 1]);
                                        }
                                        break;

                                    case "base":
                                        if (check_header != null && ((font.One == 0x31 && (Encoding.ASCII.GetString(check_header) == "5VSM"))
                                            || (Encoding.ASCII.GetString(check_header) == "6VSM")))
                                        {
                                            font.NewSomeValue = Convert.ToSingle(splitted[k + 1]);
                                        }
                                        else font.BaseSize = Convert.ToSingle(splitted[k + 1]);
                                        break;

                                    case "pages":
                                        fntPageCount = Convert.ToInt32(splitted[k + 1]);

                                        // Append mode: create array with existing + new textures
                                        int totalTexCount = existingTexCount + fntPageCount;
                                        tmpNewTex = new TextureClass.NewT3Texture[totalTexCount];

                                        // Copy existing textures to new array
                                        if (existingTexCount > 0 && font.NewTex != null && font.NewTex.Length > 0)
                                        {
                                            for (int j = 0; j < existingTexCount; j++)
                                            {
                                                tmpNewTex[j] = new TextureClass.NewT3Texture(font.NewTex[j]);
                                            }
                                        }

                                        // Initialize new texture slots
                                        for (int j = existingTexCount; j < totalTexCount; j++)
                                        {
                                            ClassesStructs.TextureClass.NewT3Texture templateSource = null;
                                            if (font.NewTex != null && font.NewTex.Length > 0)
                                            {
                                                templateSource = font.NewTex[0];
                                                tmpNewTex[j] = new TextureClass.NewT3Texture(templateSource);
                                            }
                                            else if (savedTexTemplate != null)
                                            {
                                                // Restore metadata from the template saved during Clear.
                                                templateSource = savedTexTemplate;
                                                tmpNewTex[j] = new TextureClass.NewT3Texture(savedTexTemplate);
                                            }
                                            else
                                            {
                                                // Create a default texture template
                                                tmpNewTex[j] = new TextureClass.NewT3Texture();
                                                tmpNewTex[j].Tex = new TextureClass.NewT3Texture.TextureInfo();
                                            }

                                            tmpNewTex[j].ObjectName = GetTextureObjectName(font, templateSource);
                                            tmpNewTex[j].SubObjectName = GetTextureSlotName(font, templateSource, j);
                                        }

                                        textBoxLogOutput.AppendText($"Appending {fntPageCount} new textures to existing {existingTexCount} textures. Total: {totalTexCount}\r\n");
                                    break;
                                }
                            }
                        }

                        if (strings[m].Contains("page id"))
                        {
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '"', ',' });
                            int idNum = 0;

                            for (int k = 0; k < splitted.Length; k++)
                            {
                                switch (splitted[k].ToLower())
                                {
                                    case "id":
                                        idNum = Convert.ToInt32(splitted[k + 1]);
                                        break;

                                    case "file":
                                        string fileName = strings[m].Substring(strings[m].IndexOf("file=") + 5).Replace("\"", string.Empty);

                                        if (fileName.ToLower().Contains(".dds") && File.Exists(fi.DirectoryName + Path.DirectorySeparatorChar + fileName))
                                        {
                                            int adjustedIdNum = idNum + existingTexCount;
                                            ReplaceTexture(fi.DirectoryName + Path.DirectorySeparatorChar + fileName, tmpNewTex[adjustedIdNum]);

                                            ClassesStructs.TextureClass.NewT3Texture templateSource = savedTexTemplate ?? (font.NewTex != null && font.NewTex.Length > 0 ? font.NewTex[0] : null);
                                            tmpNewTex[adjustedIdNum].ObjectName = GetTextureObjectName(font, templateSource);

                                            string subObjectName = Path.Combine(fi.DirectoryName, fileName).Replace('\\', '/');
                                            tmpNewTex[adjustedIdNum].SubObjectName = subObjectName;

                                            string slotName = GetTextureSlotName(font, templateSource, adjustedIdNum);
                                            textBoxLogOutput.AppendText($"  Loading FNT page {idNum} -> texture slot {adjustedIdNum}: {Path.GetFileName(fileName)}  name={slotName}\r\n");
                                        }
                                        break;
                                }
                            }
                        }

                        if (strings[m].Contains("chars count"))
                        {
                            // Skip - we already counted actual characters and created the array
                            // Don't trust the count in FNT file as it may be inaccurate
                        }

                        if (strings[m].Contains("char id"))
                        {
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '"', ',' });

                            for (int k = 0; k < splitted.Length; k++)
                            {
                                switch (splitted[k].ToLower())
                                {
                                    case "id":
                                        ch++;
                                        // Safety check: prevent array out of bounds
                                        if (ch < font.glyph.charsNew.Length)
                                        {
                                            font.glyph.charsNew[ch] = new FontClass.ClassFont.TRectNew();

                                            if (Convert.ToInt32(splitted[k + 1]) < 0)
                                            {
                                                font.glyph.charsNew[ch].charId = 0;
                                            }
                                            else
                                            {
                                                font.glyph.charsNew[ch].charId = Convert.ToUInt32(splitted[k + 1]);
                                            }
                                        }
                                        else
                                        {
                                            textBoxLogOutput.AppendText($"Warning: Character at index {ch} exceeds array size {font.glyph.charsNew.Length}. Skipping.\r\n");
                                        }
                                        break;

                                    case "x":
                                        font.glyph.charsNew[ch].XStart = Convert.ToSingle(splitted[k + 1]);
                                        break;

                                    case "y":
                                        font.glyph.charsNew[ch].YStart = Convert.ToSingle(splitted[k + 1]);
                                        break;

                                    case "width":
                                        font.glyph.charsNew[ch].CharWidth = Convert.ToSingle(splitted[k + 1]);
                                        font.glyph.charsNew[ch].XEnd = font.glyph.charsNew[ch].XStart + font.glyph.charsNew[ch].CharWidth;
                                        break;

                                    case "height":
                                        font.glyph.charsNew[ch].CharHeight = Convert.ToSingle(splitted[k + 1]);
                                        font.glyph.charsNew[ch].YEnd = font.glyph.charsNew[ch].YStart + font.glyph.charsNew[ch].CharHeight;
                                        break;

                                    case "xoffset":
                                        font.glyph.charsNew[ch].XOffset = Convert.ToSingle(splitted[k + 1]);
                                        if (rbNoKerning.Checked) font.glyph.charsNew[ch].XOffset = 0;
                                        break;

                                    case "yoffset":
                                        font.glyph.charsNew[ch].YOffset = Convert.ToSingle(splitted[k + 1]);
                                        if (rbNoKerning.Checked) font.glyph.charsNew[ch].YOffset = 0;
                                        break;

                                    case "xadvance":
                                        font.glyph.charsNew[ch].XAdvance = Convert.ToSingle(splitted[k + 1]);
                                        if (rbNoKerning.Checked) font.glyph.charsNew[ch].XAdvance = font.glyph.charsNew[ch].CharWidth;
                                        break;

                                    case "page":
                                        int originalPageNum = Convert.ToInt32(splitted[k + 1]);
                                        // Adjust TexNum for append mode (offset by existingTexCount)
                                        font.glyph.charsNew[ch].TexNum = originalPageNum + existingTexCount;
                                        // Debug log for first few characters
                                        if (ch < existingCharCount + 5)
                                        {
                                            textBoxLogOutput.AppendText($"  Char {ch}: FNT page {originalPageNum} -> TexNum {font.glyph.charsNew[ch].TexNum} (offset +{existingTexCount})\r\n");
                                        }
                                        break;

                                    case "chnl":
                                        font.glyph.charsNew[ch].Channel = Convert.ToInt32(splitted[k + 1]);
                                        // Auto-fix channel=0 to channel=15 (RGBA)
                                        if (font.glyph.charsNew[ch].Channel == 0)
                                        {
                                            font.glyph.charsNew[ch].Channel = 15;
                                        }
                                        break;
                                }
                            }

                            if (rbNoKerning.Checked)
                            {
                                font.glyph.charsNew[ch].XOffset = 0;
                                font.glyph.charsNew[ch].YOffset = 0;
                                font.glyph.charsNew[ch].XAdvance = font.glyph.charsNew[ch].CharWidth;
                            }
                        }
                    }

                    if(tmpNewTex != null)
                    {
                        font.NewTex = tmpNewTex;
                        font.TexCount = font.NewTex.Length;
                        font.glyph.Pages = font.TexCount;  // Update Pages to match TexCount
                        textBoxLogOutput.AppendText($"Updated font: {font.TexCount} textures, {font.glyph.CharCount} characters\r\n");
                        fillTableofTextures(font);
                    }
                }
                else
                {
                    TextureClass.OldT3Texture[] tmpOldTex = null;

                    //Make all characters as first texture due bug after saving font if font was with multi textures and saves as font with a 1 texture.
                    for(int i = 0; i < font.glyph.CharCount; i++)
                    {
                        font.glyph.chars[i].TexNum = 0;
                    }

                    bool isUnicodeFnt = false;

                    for (int m = 0; m < strings.Length; m++)
                    {
                        if (strings[m].ToLower().Contains("info face"))
                        {
                            string faceName = GetFntAttributeValue(strings[m], "face");
                            if (!string.IsNullOrEmpty(faceName) && (string.IsNullOrEmpty(font.FontName) || font.FontName == "NewFont"))
                                font.FontName = faceName;
                        }
                        else if (strings[m].ToLower().Contains("unicode"))
                        {
                            string unicodeFlag = GetFntAttributeValue(strings[m], "unicode");
                            if (!string.IsNullOrEmpty(unicodeFlag))
                                isUnicodeFnt = Convert.ToInt32(unicodeFlag) == 1;
                        }
                        if (strings[m].ToLower().Contains("common lineheight"))
                        {
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });
                            for (int k = 0; k < splitted.Length; k++)
                            {
                                switch (splitted[k].ToLower())
                                {
                                    case "lineheight":
                                        font.BaseSize = Convert.ToSingle(splitted[k + 1]);
                                        break;

                                    case "pages":
                                        tmpOldTex = new TextureClass.OldT3Texture[Convert.ToInt32(splitted[k + 1])];

                                        if (Convert.ToInt32(splitted[k + 1]) > font.TexCount)
                                        {
                                            for(int c = 0; c < tmpOldTex.Length; c++)
                                            {
                                                tmpOldTex[c] = new TextureClass.OldT3Texture(font.tex[0]);
                                            }
                                        }
                                        else
                                        {
                                            for (int c = 0; c < tmpOldTex.Length; c++)
                                            {
                                                tmpOldTex[c] = new TextureClass.OldT3Texture(font.tex[c]);
                                            }
                                        }

                                        break;
                                }
                            }
                        }

                        if (strings[m].Contains("page id"))
                        {
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '"', ',' });
                            int idNum = 0;

                            for (int k = 0; k < splitted.Length; k++)
                            {
                                switch (splitted[k].ToLower())
                                {
                                    case "id":
                                        idNum = Convert.ToInt32(splitted[k + 1]);
                                        break;

                                    case "file":

                                        string fileName = strings[m].Substring(strings[m].IndexOf("file=") + 5).Replace("\"", string.Empty);

                                        if (fileName.ToLower().Contains(".dds") && File.Exists(fi.DirectoryName + Path.DirectorySeparatorChar +  fileName))
                                        {
                                            ReplaceTexture(fi.DirectoryName + Path.DirectorySeparatorChar + fileName, tmpOldTex[idNum]);
                                        }
                                        break;
                                }
                            }
                        }

                        if (strings[m].Contains("char id"))
                        {
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '"', ',' });

                            for (int k = 0; k < splitted.Length; k++)
                            {
                                switch (splitted[k].ToLower())
                                {
                                    case "id":
                                        uint tmpChar = 0;

                                        if (Convert.ToInt32(splitted[k + 1]) < 0)
                                        {
                                            tmpChar = 0;
                                        }
                                        else
                                        {
                                            tmpChar = Convert.ToUInt32(splitted[k + 1]);

                                            if (isUnicodeFnt)
                                            {
                                                if(tmpChar == 126)
                                                {
                                                    int puase = 1;
                                                }
                                                byte[] tmp_ch = BitConverter.GetBytes(Convert.ToUInt32(splitted[k + 1]));
                                                tmp_ch = Encoding.Convert(Encoding.Unicode, Encoding.GetEncoding(MainMenu.settings.ASCII_N), tmp_ch);
                                                tmpChar = BitConverter.ToUInt16(tmp_ch, 0);
                                            }
                                        }

                                        for(int t = 0; t < font.glyph.CharCount; t++)
                                        {
                                            if(Convert.ToUInt32(dataGridViewWithCoord[0, t].Value) == tmpChar)
                                            {
                                                ch = t;
                                                break;
                                            }
                                        }

                                        break;

                                    case "x":
                                        font.glyph.chars[ch].XStart = Convert.ToSingle(splitted[k + 1]);
                                        break;

                                    case "y":
                                        font.glyph.chars[ch].YStart = Convert.ToSingle(splitted[k + 1]);
                                        break;

                                    case "width":
                                        if (font.hasScaleValue)
                                        {
                                            font.glyph.chars[ch].CharWidth = Convert.ToSingle(splitted[k + 1]);
                                            font.glyph.chars[ch].XEnd = font.glyph.chars[ch].XStart + font.glyph.chars[ch].CharWidth;
                                        }
                                        else
                                        {
                                            font.glyph.chars[ch].XEnd = font.glyph.chars[ch].XStart + Convert.ToSingle(splitted[k + 1]);
                                        }
                                        break;

                                    case "height":
                                        if (font.hasScaleValue)
                                        {
                                            font.glyph.chars[ch].CharHeight = Convert.ToSingle(splitted[k + 1]);
                                            font.glyph.chars[ch].YEnd = font.glyph.chars[ch].YStart + font.glyph.chars[ch].CharHeight;
                                        }
                                        else
                                        {
                                            font.glyph.chars[ch].YEnd = font.glyph.chars[ch].YStart + Convert.ToSingle(splitted[k + 1]);
                                        }
                                        break;

                                    case "page":
                                        font.glyph.chars[ch].TexNum = Convert.ToInt32(splitted[k + 1]);
                                        break;
                                }
                            }
                        }
                    }

                    if (tmpOldTex != null)
                    {
                        font.tex = new TextureClass.OldT3Texture[tmpOldTex.Length];

                        for(int i = 0; i < font.tex.Length; i++)
                        {
                            font.tex[i] = new TextureClass.OldT3Texture(tmpOldTex[i]);
                        }

                        tmpOldTex = null;
                        GC.Collect();

                        font.TexCount = font.tex.Length;
                        fillTableofTextures(font);
                    }
                    }

                    // Update BlockCoordSize after importing characters
                    if (font.NewFormat)
                    {
                        font.glyph.BlockCoordSize = font.glyph.CharCount * (12 * 4);
                        font.glyph.BlockCoordSize += 4; // Includes char count block
                        font.glyph.BlockCoordSize += 4; // And block size itself

                        // Remove duplicate charIds (keep last/newest entry)
                        int beforeDedup = font.glyph.charsNew.Length;
                        Array.Sort(font.glyph.charsNew, (arr1, arr2) => arr1.charId.CompareTo(arr2.charId));
                        font.glyph.charsNew = font.glyph.charsNew.GroupBy(i => i.charId).Select(g => g.Last()).ToArray();
                        font.glyph.CharCount = font.glyph.charsNew.Length;
                        int removedCount = beforeDedup - font.glyph.charsNew.Length;
                        if (removedCount > 0)
                        {
                            textBoxLogOutput.AppendText($"Removed {removedCount} duplicate character(s). Final count: {font.glyph.CharCount}\r\n");
                            // Recalculate BlockCoordSize after dedup
                            font.glyph.BlockCoordSize = font.glyph.CharCount * (12 * 4);
                            font.glyph.BlockCoordSize += 4;
                            font.glyph.BlockCoordSize += 4;
                        }
                    }

                    fillTableofCoordinates(font, true);
                    edited = true;

                    // Enable Save/Export functions after successful import
                    saveToolStripMenuItem.Enabled = true;
                    saveAsToolStripMenuItem.Enabled = true;
                    exportCoordinatesToolStripMenuItem1.Enabled = true;
                    exportCoordinatesToolStripMenuItem.Enabled = true;

                    textBoxLogOutput.AppendText("Font imported successfully. You can now save the font.\r\n");
            }

        }

        private void removeDuplicatesCharsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(font != null && font.glyph.charsNew.Length > 0)
            {

                Array.Sort(font.glyph.charsNew, (arr1, arr2) => arr1.charId.CompareTo(arr2.charId));
                font.glyph.charsNew = font.glyph.charsNew.GroupBy(i => i.charId).Select(g => g.Last()).ToArray();
                font.glyph.CharCount = font.glyph.charsNew.Length;

                if (!edited) edited = true;
                fillTableofCoordinates(font, edited);
            }
        }

        private void importCoordinatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripImportFNT_Click(sender, e);
        }
    }
}