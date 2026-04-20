using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TTG_Tools.ClassesStructs;
using TTG_Tools.Graphics.Swizzles;
using ImageMagick;

namespace TTG_Tools
{
    public partial class FontEditor
    {
        private void newFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if there are unsaved changes
            if (edited)
            {
                DialogResult result = MessageBox.Show(
                    "You have unsaved changes. Do you want to save them before creating a new font?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Cancel)
                    return;

                if (result == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(sender, e);
                }
            }

            // Reset all font-related data
            font = null;
            wiiFontData = null;
            fontFlags = null;
            encrypted = false;
            edited = false;
            isNewFontMode = true;
            droppedFontPath = null;

            // Initialize headers for new font (6VSM format)
            check_header = Encoding.ASCII.GetBytes("6VSM");
            tmpHeader = Encoding.ASCII.GetBytes("6VSM");
            version = 2;
            encKey = null;

            // Create a minimal font object for Save functionality.
            font = CreateEmptyNewFormatTemplateFont("NewFont");

            // Clear the coordinates grid
            dataGridViewWithCoord.Rows.Clear();
            dataGridViewWithCoord.Refresh();

            // Clear the textures grid
            dataGridViewWithTextures.Rows.Clear();
            dataGridViewWithTextures.Refresh();

            // Clear texture preview
            if (pictureBoxTexturePreview.Image != null)
            {
                pictureBoxTexturePreview.Image.Dispose();
                pictureBoxTexturePreview.Image = null;
            }
            pictureBoxTexturePreview.Invalidate();

            // Clear log output
            if (textBoxLogOutput != null)
                textBoxLogOutput.Clear();

            // Update UI state
            saveToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Enabled = false;
            exportToolStripMenuItem.Enabled = false;
            exportCoordinatesToolStripMenuItem1.Enabled = false;
            exportCoordinatesToolStripMenuItem.Enabled = false;

            // Enable import functions for new font
            importDDSToolStripMenuItem.Enabled = false; // Will enable after coordinates import
            toolStripImportFNT.Enabled = true;
            importCoordinatesToolStripMenuItem.Enabled = true;

            // Enable kerning radio buttons (they can be set for new font)
            rbKerning.Enabled = true;
            rbNoKerning.Enabled = true;

            // Reset window title
            if (Form.ActiveForm != null)
                Form.ActiveForm.Text = "Font Editor - New Font";

            // Log the action
            if (textBoxLogOutput != null)
                textBoxLogOutput.AppendText("=== New Font Created ===\r\n" +
                    "Ready to import coordinates from .fnt file.\r\n" +
                    "Use right-click on coordinates grid and select 'Import coordinates'.\r\n");
        }

private ClassesStructs.FontClass.ClassFont CreateEmptyNewFormatTemplateFont(string fontName)
        {
            check_header = Encoding.ASCII.GetBytes("6VSM");
            tmpHeader = Encoding.ASCII.GetBytes("6VSM");

            ClassesStructs.FontClass.ClassFont template = new ClassesStructs.FontClass.ClassFont();
            template.NewFormat = true;
            template.NewTex = new TextureClass.NewT3Texture[0];
            template.glyph = new ClassesStructs.FontClass.ClassFont.GlyphInfo();
            template.glyph.Pages = 0;
            template.glyph.CharCount = 0;
            template.glyph.charsNew = new ClassesStructs.FontClass.ClassFont.TRectNew[0];
            template.glyph.BlockCoordSize = 12;
            template.One = 0x31;
            template.FontName = string.IsNullOrEmpty(fontName) ? "NewFont" : fontName;
            template.BaseSize = 32;
            template.hasLineHeight = false;
            template.blockSize = true;
            template.headerSize = 0;
            template.texSize = 0;
            template.hasOneFloatValue = false;
            template.LastZero = 0;
            template.NewSomeValue = 0;
            template.TexCount = 0;
            template.feedFace = null;

            InitializeDefault6VsmElements(template);
            return template;
        }

        private void clearExistingFntDdsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (font == null)
            {
                MessageBox.Show("Please open or create a font first.", "No font loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!font.NewFormat)
            {
                MessageBox.Show("Clear Existing FNT+DDS is available only for 5VSM/6VSM NewFormat fonts.", "Unsupported format", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int charCount = font.glyph.CharCount;
            int texCount = (font.NewTex != null) ? font.NewTex.Length : font.TexCount;

            DialogResult result = MessageBox.Show(
                "This will remove all existing glyph coordinates and DDS textures from the current font while preserving metadata loaded from the source .font." +
                "\r\n\r\n" +
                "Current state:\r\n" +
                $"Chars: {charCount}\r\n" +
                $"Textures: {texCount}\r\n\r\n" +
                "Continue?",
                "Clear Existing FNT+DDS",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
            {
                return;
            }

            // Preserve metadata loaded from the original font and only clear import payload
            // (glyph records + texture records) to avoid masking unknown platform-specific fields.
            font.glyph.Pages = 0;
            font.glyph.CharCount = 0;
            font.glyph.charsNew = new ClassesStructs.FontClass.ClassFont.TRectNew[0];
            font.glyph.BlockCoordSize = 12;

            font.NewFormat = true;
            // Save first texture as metadata template before discarding.
            savedTexTemplate = (font.NewTex != null && font.NewTex.Length > 0) ? font.NewTex[0] : null;
            font.NewTex = new TextureClass.NewT3Texture[0];
            font.TexCount = 0;
            font.texSize = 0;
            font.BlockTexSize = 0;
            font.headerSize = 0;

            // Keep original header magic if available; fallback to 6VSM when unavailable.
            if (check_header == null || check_header.Length != 4)
            {
                check_header = Encoding.ASCII.GetBytes("6VSM");
            }
            if (tmpHeader == null || tmpHeader.Length != 4)
            {
                tmpHeader = new byte[4];
                Array.Copy(check_header, tmpHeader, 4);
            }

            if (string.IsNullOrEmpty(font.FontName))
            {
                font.FontName = "NewFont";
            }

            fontFlags = null;
            wiiFontData = null;
            encrypted = false;
            edited = true;

            fillTableofCoordinates(font, false);
            fillTableofTextures(font);

            saveToolStripMenuItem.Enabled = true;
            saveAsToolStripMenuItem.Enabled = true;
            exportToolStripMenuItem.Enabled = false;
            importDDSToolStripMenuItem.Enabled = false;
            exportCoordinatesToolStripMenuItem1.Enabled = false;
            exportCoordinatesToolStripMenuItem.Enabled = false;
            toolStripImportFNT.Enabled = true;
            importCoordinatesToolStripMenuItem.Enabled = true;
            rbKerning.Enabled = true;
            rbNoKerning.Enabled = true;

            if (textBoxLogOutput != null)
            {
                textBoxLogOutput.AppendText("\r\n=== Cleared Existing FNT+DDS ===\r\n");
                textBoxLogOutput.AppendText("Cleared glyph and texture payload while preserving loaded font metadata. Ready for full re-import of .fnt + .dds.\r\n");
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
                ofd.Filter = "Font files (*.font)|*.font";
                ofd.RestoreDirectory = true;
                ofd.Title = "Open font file";
                ofd.DereferenceLinks = false;
                byte[] binContent = new byte[0];
                string FileName = "";

                string selectedFontPath = droppedFontPath;
                droppedFontPath = null;

                if (string.IsNullOrEmpty(selectedFontPath) && ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedFontPath = ofd.FileName;
                }

                if (!string.IsNullOrEmpty(selectedFontPath))
                {
                    encrypted = false;
                    bool read = false;

                    FileStream fs;
                    try
                    {
                        FileName = selectedFontPath;
                        ofd.FileName = selectedFontPath;
                        fs = new FileStream(selectedFontPath, FileMode.Open);
                        binContent = Methods.ReadFull(fs);
                        fs.Close();
                        read = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error!");
                        saveToolStripMenuItem.Enabled = false;
                        saveAsToolStripMenuItem.Enabled = false;
                        exportCoordinatesToolStripMenuItem1.Enabled = false;
                        Form.ActiveForm.Text = "Font Editor";
                    }


                    if (read)
                    {
                        isNewFontMode = false;
                    try
                    {
                        if (MainMenu.settings.swizzleNintendoWii
                            && Path.GetExtension(selectedFontPath).Equals(".font", StringComparison.OrdinalIgnoreCase)
                            && Graphics.WiiSupport.TryLoadWiiFontForEditor(selectedFontPath, out wiiFontData))
                        {
                            fontFlags = null;
                            font = new FontClass.ClassFont();
                            font.NewFormat = false;
                            font.blockSize = wiiFontData.IsBlockSizeFont;
                            font.hasScaleValue = wiiFontData.HasScaleValue;
                            font.FontName = wiiFontData.FontName;
                            font.BaseSize = wiiFontData.BaseSize;
                            font.TexCount = Math.Max(1, wiiFontData.TexCount);
                            font.glyph.CharCount = wiiFontData.CharCount;
                            font.glyph.chars = new FontClass.ClassFont.TRect[font.glyph.CharCount];

                            int maxTex = 0;
                            for (int i = 0; i < wiiFontData.Glyphs.Count; i++)
                            {
                                var g = wiiFontData.Glyphs[i];
                                maxTex = Math.Max(maxTex, g.TexNum);
                                font.glyph.chars[i] = new FontClass.ClassFont.TRect
                                {
                                    TexNum = g.TexNum,
                                    XStart = g.XStart,
                                    XEnd = g.XEnd,
                                    YStart = g.YStart,
                                    YEnd = g.YEnd,
                                    CharWidth = g.CharWidth,
                                    CharHeight = g.CharHeight
                                };
                            }

                            font.TexCount = Math.Max(font.TexCount, maxTex + 1);
                            font.tex = new TextureClass.OldT3Texture[font.TexCount];
                            for (int i = 0; i < font.TexCount; i++)
                            {
                                font.tex[i] = new TextureClass.OldT3Texture
                                {
                                    Width = wiiFontData.TextureWidth,
                                    Height = wiiFontData.TextureHeight,
                                    OriginalWidth = wiiFontData.TextureWidth,
                                    OriginalHeight = wiiFontData.TextureHeight,
                                    TexSize = 0,
                                    Content = new byte[0]
                                };
                            }

                            check_header = Encoding.ASCII.GetBytes("ERTM");
                            fillTableofCoordinates(font, false);
                            fillTableofTextures(font);
                            UpdateTexturePreview();
                            saveToolStripMenuItem.Enabled = true;
                            saveAsToolStripMenuItem.Enabled = true;
                            exportCoordinatesToolStripMenuItem1.Enabled = true;
                            rbKerning.Enabled = false;
                            rbNoKerning.Enabled = false;
                            edited = false;
                            FileInfo fiWii = new FileInfo(FileName);
                            if (Form.ActiveForm != null) Form.ActiveForm.Text = "Font Editor. Opened file " + fiWii.FullName + " (Wii)";
                            return;
                        }

                        wiiFontData = null;
                        fontFlags = null;

                        byte[] header = new byte[4];
                        Array.Copy(binContent, 0, header, 0, 4);

                        if (textBoxLogOutput != null && !textBoxLogOutput.IsDisposed)
                        {
                            textBoxLogOutput.AppendText("\r\n=== [OpenFontDiag] Begin ===\r\n");
                            textBoxLogOutput.AppendText($"[OpenFontDiag] File={FileName}\r\n");
                            textBoxLogOutput.AppendText($"[OpenFontDiag] FileSize={binContent.Length}\r\n");
                            textBoxLogOutput.AppendText($"[OpenFontDiag] Magic={Encoding.ASCII.GetString(header)}\r\n");
                        }

                        int poz = 0;

                        //Experiments with too old fonts
                        font = new FontClass.ClassFont();
                        font.hasOneFloatValue = false;
                        font.blockSize = false;
                        font.hasScaleValue = false;
                        AddInfo = false;

                        font.headerSize = 0;
                        font.texSize = 0;

                        poz = 4; //Begin position

                        check_header = new byte[4];
                        Array.Copy(binContent, 0, check_header, 0, check_header.Length);
                        encKey = null;
                        version = 2;

                        if ((Encoding.ASCII.GetString(check_header) != "5VSM") && (Encoding.ASCII.GetString(check_header) != "ERTM")
                        && (Encoding.ASCII.GetString(check_header) != "6VSM") && (Encoding.ASCII.GetString(check_header) != "NIBM")) //Supposed this font encrypted
                        {
                            //First trying decrypt probably encrypted font
                            try
                            {
                                string info = Methods.FindingDecrytKey(binContent, "font", ref encKey, ref version);
                                if (info != null)
                                {
                                    MessageBox.Show("Font was encrypted, but I decrypted.\r\n" + info);
                                    encrypted = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Maybe that font encrypted. Try to decrypt first.", "Error " + ex.Message);
                                poz = -1;
                                return;
                            }
                        }

                        if ((Encoding.ASCII.GetString(check_header) == "5VSM") || (Encoding.ASCII.GetString(check_header) == "6VSM"))
                        {
                            byte[] tmpBytes = new byte[4];
                            Array.Copy(binContent, 4, tmpBytes, 0, tmpBytes.Length);
                            font.NewFormat = true;
                            font.headerSize = BitConverter.ToInt32(tmpBytes, 0);

                            tmpBytes = new byte[4];
                            Array.Copy(binContent, 12, tmpBytes, 0, tmpBytes.Length);
                            font.texSize = BitConverter.ToUInt32(tmpBytes, 0);

                            poz = 16;

                            if (textBoxLogOutput != null && !textBoxLogOutput.IsDisposed)
                            {
                                textBoxLogOutput.AppendText($"[OpenFontDiag] headerSize=0x{font.headerSize:x} ({font.headerSize})\r\n");
                                textBoxLogOutput.AppendText($"[OpenFontDiag] texSize=0x{font.texSize:x} ({font.texSize})\r\n");
                            }
                        }

                        byte[] tmp = new byte[4];
                        Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                        poz += 4;
                        int countElements = BitConverter.ToInt32(tmp, 0);

                        if (textBoxLogOutput != null && !textBoxLogOutput.IsDisposed)
                        {
                            textBoxLogOutput.AppendText($"[OpenFontDiag] countElements(raw)={countElements}\r\n");
                        }

                        // Detect fonts saved without elements section (old SaveFont bug).
                        // In those files, the bytes at poz are FontName/One data, not countElements.
                        bool noElements = (countElements > 10000);
                        if (noElements)
                            countElements = 0;

                        if (textBoxLogOutput != null && !textBoxLogOutput.IsDisposed)
                        {
                            textBoxLogOutput.AppendText($"[OpenFontDiag] noElements={noElements}, countElements(effective)={countElements}\r\n");
                        }

                        font.elements = new string[countElements];
                        font.binElements = new byte[countElements][];
                        int lenStr;
                        someTexData = false;

                        tmp = new byte[8];
                        Array.Copy(binContent, poz, tmp, 0, tmp.Length);

                        if (!noElements)
                        {
                            if ((BitConverter.ToString(tmp) == "81-53-37-63-9E-4A-3A-9A") && (countElements == 1) && (Encoding.ASCII.GetString(check_header) == "ERTM"))
                            {
                                MessageBox.Show("This font is empty!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                font = null;
                                GC.Collect();
                                edited = false;
                                return;
                            }

                            if (BitConverter.ToString(tmp) == "81-53-37-63-9E-4A-3A-9A")
                            {
                                if((countElements == 1) && (Encoding.ASCII.GetString(check_header) == "6VSM"))
                                {
                                    MessageBox.Show("This font is a vector font. Try use Auto (De)Packer.");
                                    font = null;
                                    GC.Collect();
                                    edited = false;
                                    return;
                                }

                                for (int i = 0; i < countElements; i++)
                                {
                                    font.binElements[i] = new byte[12];
                                    Array.Copy(binContent, poz, font.binElements[i], 0, font.binElements[i].Length);
                                    poz += 12;

                                    byte[] guidBytes = new byte[8];
                                    Array.Copy(font.binElements[i], guidBytes, 8);
                                    switch (BitConverter.ToString(guidBytes))
                                    {
                                        case "41-16-D7-79-B9-3C-28-84":
                                            fontFlags = new FlagsClass();
                                            break;

                                        case "E3-88-09-7A-48-5D-7F-93":
                                            someTexData = true;
                                            font.hasScaleValue = true;
                                            break;

                                        case "0F-F4-20-E6-20-BA-A1-EF":
                                            font.NewFormat = true;
                                            break;

                                        case "7A-BA-6E-87-89-88-6C-FA":
                                            AddInfo = true;
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < countElements; i++)
                                {
                                    tmp = new byte[4];
                                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                    poz += 4;
                                    lenStr = BitConverter.ToInt32(tmp, 0);
                                    tmp = new byte[lenStr];
                                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                    poz += lenStr + 4; //Length element's name and 4 bytes data for Telltale Tool
                                    font.elements[i] = Encoding.ASCII.GetString(tmp);

                                    if (font.elements[i] == "class Flags")
                                    {
                                        fontFlags = new FlagsClass();
                                    }
                                }
                            }
                        }

                        tmpHeader = new byte[poz];
                        Array.Copy(binContent, 0, tmpHeader, 0, tmpHeader.Length);

                        if (noElements)
                        {
                            // Font saved without elements: FontName at offset 8 was partially
                            // corrupted by texSize Seek(12), so it cannot be recovered.
                            // poz is at 16, where One byte resides.
                            font.FontName = "";
                            font.blockSize = true;
                        }
                        else
                        {
                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            int nameLen = BitConverter.ToInt32(tmp, 0);
                            poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            if (nameLen - BitConverter.ToInt32(tmp, 0) == 8)
                            {
                                nameLen = BitConverter.ToInt32(tmp, 0);
                                poz += 4;
                                font.blockSize = true;
                            }

                            tmp = new byte[nameLen];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.FontName = Encoding.ASCII.GetString(tmp);
                            poz += nameLen;
                        }

                        font.One = binContent[poz];
                        poz++;

                        //Temporary solution
                        if ((font.One == 0x31 && (Encoding.ASCII.GetString(check_header) == "5VSM"))
                            || (Encoding.ASCII.GetString(check_header) == "6VSM"))
                        {
                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            poz += 4;

                            font.NewSomeValue = BitConverter.ToSingle(tmp, 0);
                        }

                        tmp = new byte[4];
                        Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                        poz += 4;
                        font.BaseSize = BitConverter.ToSingle(tmp, 0);

                        tmp = new byte[4];
                        Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                        font.halfValue = 0.0f;
                        font.lineHeight = 0.0f;
                        font.feedFace = null;
                        font.hasLineHeight = false;

                        if(BitConverter.ToString(tmp) == "CE-FA-ED-FE")
                        {
                            font.feedFace = new byte[4];
                            Array.Copy(binContent, poz, font.feedFace, 0, font.feedFace.Length);
                            poz += 4;
                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                        }

                        if (font.hasScaleValue && Encoding.ASCII.GetString(header) == "5VSM")
                        {
                            //Check for Back to the Future for PS4

                            int tmpPos = poz;
                            tmp = new byte[4];
                            Array.Copy(binContent, tmpPos + 12, tmp, 0, tmp.Length);
                            int checkBlockSize = BitConverter.ToInt32(tmp, 0);

                            tmp = new byte[4];
                            Array.Copy(binContent, tmpPos + 16, tmp, 0, tmp.Length);
                            int checkCharCount = BitConverter.ToInt32(tmp, 0);

                            if ((checkCharCount * (4 * 12)) + 8 == checkBlockSize)
                            {
                                font.hasLineHeight = true;
                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                poz += 4;
                                font.lineHeight = BitConverter.ToSingle(tmp, 0);

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            }
                            else
                            {
                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            }
                        }

                        if ((BitConverter.ToSingle(tmp, 0) == 0.5)
                            || (BitConverter.ToSingle(tmp, 0) == 1.0))
                        {
                            font.halfValue = BitConverter.ToSingle(tmp, 0);
                            poz += 4;
                        }

                        if (font.hasScaleValue)
                        {
                            //very strange check method about 1.0f value
                            int tmp_poz = poz;
                            tmp = new byte[4];
                            Array.Copy(binContent, tmp_poz, tmp, 0, tmp.Length);
                            font.glyph.BlockCoordSize = BitConverter.ToInt32(tmp, 0);
                            tmp_poz += 4;

                            tmp = new byte[4];
                            Array.Copy(binContent, tmp_poz, tmp, 0, tmp.Length);
                            font.glyph.CharCount = BitConverter.ToInt32(tmp, 0);
                            tmp_poz += 4;

                            //check if it size of chars + 8 bytes of block size and count of characters
                            if ((font.glyph.CharCount * (4 * 12)) + 8 != font.glyph.BlockCoordSize)
                            {
                                font.glyph.BlockCoordSize = 0;
                                font.glyph.CharCount = 0;
                                font.hasOneFloatValue = true;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);

                                font.oneValue = BitConverter.ToSingle(tmp, 0);
                                poz += 4;
                            }
                        }

                        font.glyph.BlockCoordSize = 0;

                        if (font.blockSize)
                        {
                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.glyph.BlockCoordSize = BitConverter.ToInt32(tmp, 0);
                            poz += 4;
                        }

                        tmp = new byte[4];
                        Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                        font.glyph.CharCount = BitConverter.ToInt32(tmp, 0);
                        poz += 4;

                        if (!font.NewFormat)
                        {
                            font.glyph.chars = new FontClass.ClassFont.TRect[font.glyph.CharCount];
                            font.glyph.charsNew = null;

                            for (int i = 0; i < font.glyph.CharCount; i++)
                            {
                                font.glyph.chars[i] = new FontClass.ClassFont.TRect();

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.chars[i].TexNum = BitConverter.ToInt32(tmp, 0);
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.chars[i].XStart = BitConverter.ToSingle(tmp, 0);
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.chars[i].XEnd = BitConverter.ToSingle(tmp, 0);
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.chars[i].YStart = BitConverter.ToSingle(tmp, 0);
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.chars[i].YEnd = BitConverter.ToSingle(tmp, 0);
                                poz += 4;

                                if (font.hasScaleValue)
                                {
                                    tmp = new byte[4];
                                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                    font.glyph.chars[i].CharWidth = (float)Math.Round(BitConverter.ToSingle(tmp, 0));
                                    poz += 4;

                                    tmp = new byte[4];
                                    Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                    font.glyph.chars[i].CharHeight = (float)Math.Round(BitConverter.ToSingle(tmp, 0));
                                    poz += 4;
                                }
                            }
                        }
                        else
                        {
                            font.glyph.chars = null;
                            font.glyph.charsNew = new ClassesStructs.FontClass.ClassFont.TRectNew[font.glyph.CharCount];

                            for (int i = 0; i < font.glyph.CharCount; i++)
                            {
                                font.glyph.charsNew[i] = new FontClass.ClassFont.TRectNew();

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.charsNew[i].charId = BitConverter.ToUInt32(tmp, 0);
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.charsNew[i].TexNum = BitConverter.ToInt32(tmp, 0);
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.charsNew[i].Channel = BitConverter.ToInt32(tmp, 0);
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.charsNew[i].XStart = BitConverter.ToSingle(tmp, 0);
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.charsNew[i].XEnd = BitConverter.ToSingle(tmp, 0);
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.charsNew[i].YStart = BitConverter.ToSingle(tmp, 0);
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.charsNew[i].YEnd = BitConverter.ToSingle(tmp, 0);
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.charsNew[i].CharWidth = (float)Math.Round(BitConverter.ToSingle(tmp, 0));
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.charsNew[i].CharHeight = (float)Math.Round(BitConverter.ToSingle(tmp, 0));
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.charsNew[i].XOffset = (float)Math.Round(BitConverter.ToSingle(tmp, 0));
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.charsNew[i].YOffset = (float)Math.Round(BitConverter.ToSingle(tmp, 0));
                                poz += 4;

                                tmp = new byte[4];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.glyph.charsNew[i].XAdvance = (float)Math.Round(BitConverter.ToSingle(tmp, 0));
                                poz += 4;
                            }
                        }

                        if (font.blockSize)
                        {
                            tmp = new byte[4];
                            Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                            font.BlockTexSize = BitConverter.ToInt32(tmp, 0);
                            poz += 4;

                            if (textBoxLogOutput != null && !textBoxLogOutput.IsDisposed)
                            {
                                textBoxLogOutput.AppendText($"[OpenFontDiag] BlockTexSize=0x{font.BlockTexSize:x} ({font.BlockTexSize}) at poz=0x{(poz - 4):x}\r\n");
                            }
                        }

                        tmp = new byte[4];
                        Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                        font.TexCount = BitConverter.ToInt32(tmp, 0);
                        poz += 4;

                        if (textBoxLogOutput != null && !textBoxLogOutput.IsDisposed)
                        {
                            textBoxLogOutput.AppendText($"[OpenFontDiag] NewFormat={font.NewFormat}, CharCount={font.glyph.CharCount}, TexCount={font.TexCount}\r\n");
                            textBoxLogOutput.AppendText($"[OpenFontDiag] FontUnknowns one=0x{font.One:x2}, newSomeValue={font.NewSomeValue}, baseSize={font.BaseSize}, lineHeight={font.lineHeight}, halfValue={font.halfValue}, oneValue={font.oneValue}, hasScaleValue={font.hasScaleValue}, hasOneFloatValue={font.hasOneFloatValue}, hasLineHeight={font.hasLineHeight}, lastZero=0x{font.LastZero:x2}, feedFace={FormatBytesPreview(font.feedFace)}\r\n");
                            textBoxLogOutput.AppendText($"[OpenFontDiag] TextureHeaderStartPoz=0x{poz:x}\r\n");
                        }

                        if (!font.NewFormat)
                        {
                            font.tex = new TextureClass.OldT3Texture[font.TexCount];
                            font.NewTex = null;

                            for (int i = 0; i < font.TexCount; i++)
                            {
                                font.tex[i] = Graphics.TextureWorker.GetOldTextures(binContent, ref poz, fontFlags != null, someTexData);
                                if (font.tex[i] == null)
                                {
                                    MessageBox.Show("Maybe unsupported font.", "Error");
                                    return;
                                }
                            }

                            for (int k = 0; k < font.glyph.CharCount; k++)
                            {
                                font.glyph.chars[k].XStart *= font.tex[font.glyph.chars[k].TexNum].Width;
                                font.glyph.chars[k].XStart = (float)Math.Round(font.glyph.chars[k].XStart);
                                font.glyph.chars[k].XEnd *= font.tex[font.glyph.chars[k].TexNum].Width;
                                font.glyph.chars[k].XEnd = (float)Math.Round(font.glyph.chars[k].XEnd);

                                font.glyph.chars[k].YStart *= font.tex[font.glyph.chars[k].TexNum].Height;
                                font.glyph.chars[k].YStart = (float)Math.Round(font.glyph.chars[k].YStart);
                                font.glyph.chars[k].YEnd *= font.tex[font.glyph.chars[k].TexNum].Height;
                                font.glyph.chars[k].YEnd = (float)Math.Round(font.glyph.chars[k].YEnd);
                            }
                        }
                        else
                        {
                            font.tex = null;
                            font.NewTex = new TextureClass.NewT3Texture[font.TexCount];
                            string format = "";
                            uint tmpPosition = 0;

                            if (font.headerSize != 0)
                            {
                                tmpPosition = (uint)font.headerSize + 16 + ((uint)countElements * 12) + 4;
                            }

                            if (textBoxLogOutput != null && !textBoxLogOutput.IsDisposed)
                            {
                                textBoxLogOutput.AppendText($"[OpenFontDiag] texDataStart(tmpPosition)=0x{tmpPosition:x}\r\n");
                            }

                            for (int i = 0; i < font.TexCount; i++)
                            {
                                // Create log callback to output logs to textBoxLogOutput
                                Action<string> logCallback = (msg) => {
                                    if (textBoxLogOutput != null && !textBoxLogOutput.IsDisposed)
                                    {
                                        textBoxLogOutput.AppendText(msg + "\r\n");
                                    }
                                };

                                font.NewTex[i] = Graphics.TextureWorker.GetNewTextures(binContent, ref poz, ref tmpPosition, fontFlags != null, someTexData, true, ref format, AddInfo, logCallback);

                                // For 6VSM/5VSM fonts, GetNewTextures leaves poz pointing to the next
                                // texture header (correct), and only updates tmpPosition (texFontPoz)
                                // to point past the pixel data. We should NOT sync poz with tmpPosition
                                // for these formats. poz is already at the correct position for the
                                // next texture header.
                                // For ERTM format, GetNewTextures updates poz to point past the pixel
                                // data, so no sync is needed either way.

                                if (font.NewTex[i] == null)
                                {
                                    MessageBox.Show("Maybe unsupported font.", "Error");
                                    return;
                                }

                                if (textBoxLogOutput != null && !textBoxLogOutput.IsDisposed)
                                {
                                    var unknownData = font.NewTex[i].UnknownData;
                                    textBoxLogOutput.AppendText($"[OpenFontDiag] Tex[{i}] platform={font.NewTex[i].platform.platform}, someValue={font.NewTex[i].SomeValue}, oneByte=0x{font.NewTex[i].OneByte:x2}, mip={font.NewTex[i].Mip}, mipCount={font.NewTex[i].Tex.MipCount}, someData={font.NewTex[i].Tex.SomeData}, texSize={font.NewTex[i].Tex.TexSize}, nextHeaderPoz=0x{poz:x}, nextTexDataPoz=0x{tmpPosition:x}\r\n");
                                    textBoxLogOutput.AppendText($"[OpenFontDiag] Tex[{i}] Unknowns platformBlockSize={font.NewTex[i].platform.blockSize}, unknownFlags.blockSize={font.NewTex[i].unknownFlags.blockSize}, unknownFlags.block=0x{font.NewTex[i].unknownFlags.block:x8}, oneValue={font.NewTex[i].OneValue}, zero={font.NewTex[i].Zero}, unknown1={font.NewTex[i].Unknown1}, faces={font.NewTex[i].Faces}, arrayMembers={font.NewTex[i].ArrayMembers}, hasOneValueTex={font.NewTex[i].HasOneValueTex}, objectNameLen={(font.NewTex[i].ObjectName == null ? -1 : font.NewTex[i].ObjectName.Length)}, subObjectNameLen={(font.NewTex[i].SubObjectName == null ? -1 : font.NewTex[i].SubObjectName.Length)}\r\n");
                                    textBoxLogOutput.AppendText($"[OpenFontDiag] Tex[{i}] ObjectName=\"{font.NewTex[i].ObjectName ?? "(null)"}\" SubObjectName=\"{font.NewTex[i].SubObjectName ?? "(null)"}\"\r\n");
                                    textBoxLogOutput.AppendText($"[OpenFontDiag] Tex[{i}] Blocks blockLen={(font.NewTex[i].block == null ? -1 : font.NewTex[i].block.Length)}, subBlockSize={font.NewTex[i].subBlock.Size}, subBlockPreview={FormatBytesPreview(font.NewTex[i].subBlock.Block)}, subBlock2Size={font.NewTex[i].subBlock2.Size}, subBlock2Preview={FormatBytesPreview(font.NewTex[i].subBlock2.Block)}\r\n");
                                    textBoxLogOutput.AppendText($"[OpenFontDiag] Tex[{i}] UnknownData count={(unknownData == null ? -1 : unknownData.count)}, unknown1={(unknownData == null ? -1 : unknownData.Unknown1)}, len={(unknownData == null ? -1 : unknownData.len)}, preview={FormatBytesPreview(unknownData == null ? null : unknownData.someData)}\r\n");
                                }
                            }

                            if(font.NewTex[0].SomeValue > 4)
                            {
                                tmp = new byte[1];
                                Array.Copy(binContent, poz, tmp, 0, tmp.Length);
                                font.LastZero = tmp[0];
                                poz++;
                            }

                            for (int k = 0; k < font.glyph.CharCount; k++)
                            {
                                font.glyph.charsNew[k].XStart *= font.NewTex[font.glyph.charsNew[k].TexNum].Width;
                                font.glyph.charsNew[k].XStart = (float)Math.Round(font.glyph.charsNew[k].XStart);
                                font.glyph.charsNew[k].XEnd *= font.NewTex[font.glyph.charsNew[k].TexNum].Width;
                                font.glyph.charsNew[k].XEnd = (float)Math.Round(font.glyph.charsNew[k].XEnd);

                                font.glyph.charsNew[k].YStart *= font.NewTex[font.glyph.charsNew[k].TexNum].Height;
                                font.glyph.charsNew[k].YStart = (float)Math.Round(font.glyph.charsNew[k].YStart);
                                font.glyph.charsNew[k].YEnd *= font.NewTex[font.glyph.charsNew[k].TexNum].Height;
                                font.glyph.charsNew[k].YEnd = (float)Math.Round(font.glyph.charsNew[k].YEnd);
                            }
                        }

                        fillTableofCoordinates(font, false);
                        fillTableofTextures(font);
                        UpdateTexturePreview();

                        saveToolStripMenuItem.Enabled = true;
                        saveAsToolStripMenuItem.Enabled = true;
                        exportCoordinatesToolStripMenuItem1.Enabled = true;
                        rbKerning.Enabled = font.NewFormat;
                        rbNoKerning.Enabled = font.NewFormat;
                        edited = false;
                        FileInfo fi = new FileInfo(FileName);
                        if(Form.ActiveForm != null) Form.ActiveForm.Text = "Font Editor. Opened file " + fi.FullName;

                        if (textBoxLogOutput != null && !textBoxLogOutput.IsDisposed)
                        {
                            textBoxLogOutput.AppendText("=== [OpenFontDiag] End ===\r\n");
                        }

                    }
                    catch(Exception ex)
                    {
                        binContent = null;
                        GC.Collect();
                        MessageBox.Show("Unknown error:\r\n" + ex.ToString(), "Error");
                    }
                    }
                }
        }

        public int FindStartOfStringSomething(byte[] array, int offset, string string_something)
        {
            int poz = offset;
            while (Methods.ConvertHexToString(array, poz, string_something.Length, MainMenu.settings.ASCII_N, 1) != string_something)
            {
                poz++;
                if (Methods.ConvertHexToString(array, poz, string_something.Length, MainMenu.settings.ASCII_N, 1) == string_something)
                {
                    return poz;
                }
                if ((poz + string_something.Length + 1) > array.Length)
                {
                    break;
                }
            }
            return poz;
        }

        private void encFunc(string path) //Encrypts full font
        {
            if (encrypted == true) //Ask about a full enryption if you don't want build archive
            {
                if (MessageBox.Show("Do you want to make a full encryption?", "About encrypted font...",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    FileStream fs = new FileStream(path, FileMode.Open);
                    byte[] fontContent = Methods.ReadFull(fs);
                    fs.Close();

                    Methods.meta_crypt(fontContent, encKey, version, false);

                    if (File.Exists(path)) File.Delete(path);
                    fs = new FileStream(path, FileMode.Create);
                    fs.Write(fontContent, 0, fontContent.Length);
                    fs.Close();
                }

            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!edited) return;

            // If no font file is currently open, use Save As instead
            if (string.IsNullOrEmpty(ofd.FileName))
            {
                saveAsToolStripMenuItem_Click(sender, e);
                return;
            }

            Methods.DeleteCurrentFile(ofd.FileName);

            FileStream fs = new FileStream(ofd.FileName, FileMode.OpenOrCreate);
            SaveFont(fs, font);
            fs.Close();

            encFunc(ofd.FileName);
            fillTableofCoordinates(font, false);
            edited = false; //After saving return trigger to FALSE
        }

        private void SaveFont(Stream fs, ClassesStructs.FontClass.ClassFont font)
        {
            if (wiiFontData != null)
            {
                fs.Close();
                for (int i = 0; i < font.glyph.CharCount && i < wiiFontData.Glyphs.Count; i++)
                {
                    var src = font.glyph.chars[i];
                    var dst = wiiFontData.Glyphs[i];
                    dst.TexNum = src.TexNum;
                    dst.XStart = src.XStart;
                    dst.XEnd = src.XEnd;
                    dst.YStart = src.YStart;
                    dst.YEnd = src.YEnd;
                    if (wiiFontData.HasScaleValue)
                    {
                        dst.CharWidth = src.CharWidth;
                        dst.CharHeight = src.CharHeight;
                    }
                }
                wiiFontData.Save(ofd.FileName);
                return;
            }

            BinaryWriter bw = new BinaryWriter(fs);

            // Ensure tmpHeader is not null
            if (tmpHeader == null || tmpHeader.Length != 4)
            {
                tmpHeader = Encoding.ASCII.GetBytes("6VSM");
            }

            string checkHeaderStr = Encoding.ASCII.GetString(check_header);
            bool isNewFormat = (checkHeaderStr == "5VSM" || checkHeaderStr == "6VSM");

            bw.Write(tmpHeader); // offset 0: magic (4 bytes)

            //First need check textures import
            font.texSize = 0;
            font.headerSize = 0;

            if (isNewFormat)
            {
                // Write headerSize placeholder (will be overwritten at the end)
                bw.Write(0); // offset 4: headerSize placeholder
                // Write fixed-0 separator at offset 8 (always 0 in all official NewFormat fonts)
                bw.Write(0); // offset 8: structural separator, not TexCount
                // Write texSize placeholder (will be overwritten at the end)
                bw.Write(0); // offset 12: texSize placeholder

                // Write elements section (preserved from original file)
                int countElements = (font.binElements != null) ? font.binElements.Length : 0;
                bw.Write(countElements); // offset 16: countElements

                for (int i = 0; i < countElements; i++)
                {
                    if (font.binElements[i] != null)
                    {
                        bw.Write(font.binElements[i]);
                    }
                    else
                    {
                        bw.Write(new byte[12]);
                    }
                }
            }

            // Ensure FontName is not null
            if (string.IsNullOrEmpty(font.FontName))
            {
                font.FontName = "NewFont";
            }
            int len = Encoding.ASCII.GetBytes(font.FontName).Length;

            // Record position BEFORE writing FontName (this is where headerSize should start counting from)
            long headerSizeStartPosition = bw.BaseStream.Position;

            if (!isNewFormat)
            {
                // Only used for old format, not 6VSM/5VSM
            }

            if (font.blockSize)
            {
                int subLen = len + 8;
                bw.Write(subLen);
            }

            bw.Write(len);
            bw.Write(Encoding.ASCII.GetBytes(font.FontName));

            bw.Write(font.One);

            if ((font.One == 0x31 && (Encoding.ASCII.GetString(check_header) == "5VSM"))
                        || (Encoding.ASCII.GetString(check_header) == "6VSM"))
            {
                bw.Write(font.NewSomeValue);
            }

            bw.Write(font.BaseSize);

            if(font.feedFace != null)
            {
                bw.Write(font.feedFace);
            }

            // Ensure check_header is not null
            if (check_header == null || check_header.Length != 4)
            {
                check_header = Encoding.ASCII.GetBytes("6VSM");
            }

            if(Encoding.ASCII.GetString(check_header) == "5VSM"
                && font.hasLineHeight)
            {
                bw.Write(font.lineHeight);
            }

            if(font.halfValue == 0.5f || font.halfValue == 1.0f)
            {
                bw.Write(font.halfValue);
            }

            if (font.hasScaleValue && font.hasOneFloatValue)
            {
                bw.Write(font.oneValue);
            }

            if (font.blockSize)
            {
                if (!font.NewFormat)
                {
                    font.glyph.BlockCoordSize = font.glyph.CharCount * (5 * 4);

                    if (font.hasScaleValue) font.glyph.BlockCoordSize = font.glyph.CharCount * (7 * 4);

                    font.glyph.BlockCoordSize += 4; //Includes char count block
                }
                else
                {
                    font.glyph.BlockCoordSize = font.glyph.CharCount * (12 * 4);
                    font.glyph.BlockCoordSize += 4; //Includes char count block
                }

                font.glyph.BlockCoordSize += 4; //And block size itself

                bw.Write(font.glyph.BlockCoordSize);
            }

            bw.Write(font.glyph.CharCount);

            if (!font.NewFormat)
            {
                for (int i = 0; i < font.glyph.CharCount; i++)
                {
                    bw.Write(font.glyph.chars[i].TexNum);
                    bw.Write(font.glyph.chars[i].XStart / font.tex[font.glyph.chars[i].TexNum].OriginalWidth);
                    bw.Write(font.glyph.chars[i].XEnd / font.tex[font.glyph.chars[i].TexNum].OriginalWidth);
                    bw.Write(font.glyph.chars[i].YStart / font.tex[font.glyph.chars[i].TexNum].OriginalHeight);
                    bw.Write(font.glyph.chars[i].YEnd / font.tex[font.glyph.chars[i].TexNum].OriginalHeight);

                    if (font.hasScaleValue)
                    {
                        bw.Write(font.glyph.chars[i].CharWidth);
                        bw.Write(font.glyph.chars[i].CharHeight);
                    }
                }

                if (font.blockSize)
                {
                    font.BlockTexSize = 0;

                    for (int j = 0; j < font.TexCount; j++)
                    {
                        font.BlockTexSize += font.tex[j].BlockPos + font.tex[j].TexSize;
                    }

                    font.BlockTexSize += 8; //4 bytes of block size and 4 bytes of block (if it empty)

                    bw.Write(font.BlockTexSize);
                }

                bw.Write(font.TexCount);

                for (int i = 0; i < font.TexCount; i++)
                {
                    Graphics.TextureWorker.ReplaceOldTextures(fs, font.tex[i], someTexData, encrypted, encKey, version);
                }
            }
            else
            {
                // Keep TexCount synchronized with actual texture array before writing headers.
                int actualNewTexCount = (font.NewTex != null) ? font.NewTex.Length : 0;
                if (font.TexCount != actualNewTexCount)
                {
                    textBoxLogOutput.AppendText($"[SaveFont] Correcting TexCount {font.TexCount} -> {actualNewTexCount} based on NewTex.Length.\r\n");
                    font.TexCount = actualNewTexCount;
                }

                // Debug output for texture information before saving
                textBoxLogOutput.AppendText("\r\n=== SaveFont Debug Info ===\r\n");
                textBoxLogOutput.AppendText($"font.NewTex.Length: {actualNewTexCount}\r\n");
                textBoxLogOutput.AppendText($"font.glyph.CharCount: {font.glyph.CharCount}\r\n");
                textBoxLogOutput.AppendText($"font.TexCount: {font.TexCount}\r\n");
                textBoxLogOutput.AppendText($"check_header: {Encoding.ASCII.GetString(check_header)}\r\n");
                textBoxLogOutput.AppendText("===================================\r\n");

                // Check TexNum bounds for first and last few characters
                for (int checkIdx = 0; checkIdx < Math.Min(10, font.glyph.CharCount); checkIdx++)
                {
                    int texNum = font.glyph.charsNew[checkIdx].TexNum;
                    if (font.NewTex != null && texNum >= 0 && texNum < font.NewTex.Length)
                    {
                        textBoxLogOutput.AppendText($"Char {checkIdx}: TexNum={texNum}, Width={font.NewTex[texNum].Width}, Height={font.NewTex[texNum].Height}\r\n");
                    }
                    else
                    {
                        textBoxLogOutput.AppendText($"ERROR: Char {checkIdx}: TexNum={texNum} OUT OF BOUNDS (0-{Math.Max(0, actualNewTexCount - 1)})\r\n");
                    }
                }

                for (int i = 0; i < font.glyph.CharCount; i++)
                {
                    bw.Write(font.glyph.charsNew[i].charId);
                    int safeTexNum = font.glyph.charsNew[i].TexNum;
                    if (font.NewTex == null || font.NewTex.Length == 0)
                    {
                        safeTexNum = 0;
                    }
                    else if (safeTexNum < 0 || safeTexNum >= font.NewTex.Length)
                    {
                        textBoxLogOutput.AppendText($"WARNING: Char {i} has invalid TexNum {safeTexNum}. Using 0 instead.\r\n");
                        safeTexNum = 0;
                    }

                    bw.Write(safeTexNum);
                    bw.Write(font.glyph.charsNew[i].Channel);

                    var xSt = font.glyph.charsNew[i].XStart / font.NewTex[safeTexNum].Width;
                    bw.Write(xSt);
                    var xEn = font.glyph.charsNew[i].XEnd / font.NewTex[safeTexNum].Width;
                    bw.Write(xEn);
                    var ySt = font.glyph.charsNew[i].YStart / font.NewTex[safeTexNum].Height;
                    bw.Write(ySt);
                    var yEn = font.glyph.charsNew[i].YEnd / font.NewTex[safeTexNum].Height;
                    bw.Write(yEn);

                    float xOffset = rbNoKerning.Checked ? 0 : font.glyph.charsNew[i].XOffset;
                    float yOffset = rbNoKerning.Checked ? 0 : font.glyph.charsNew[i].YOffset;
                    float xAdvance = rbNoKerning.Checked ? font.glyph.charsNew[i].CharWidth : font.glyph.charsNew[i].XAdvance;

                    bw.Write(font.glyph.charsNew[i].CharWidth);
                    bw.Write(font.glyph.charsNew[i].CharHeight);
                    bw.Write(xOffset);
                    bw.Write(yOffset);
                    bw.Write(xAdvance);
                }

                font.texSize = 0;

                // Ensure check_header is not null before using it
                if (check_header == null || check_header.Length != 4)
                {
                    check_header = Encoding.ASCII.GetBytes("6VSM");
                    checkHeaderStr = Encoding.ASCII.GetString(check_header);
                }

                // texSize: sum of all mip pixel sizes (same as before)
                for (int i = 0; i < font.TexCount; i++)
                {
                    for (int k = 0; k < font.NewTex[i].Mip; k++)
                    {
                        font.texSize += (uint)font.NewTex[i].Tex.Textures[k].MipSize;
                    }
                }

                // Record position before BlockTexSize+TexCount
                long posBeforeBlock = bw.BaseStream.Position;

                // Write placeholder for BlockTexSize (will be overwritten later)
                bw.Write(0); // BlockTexSize placeholder
                bw.Write(font.TexCount);

                int c = 1;

                if (checkHeaderStr == "ERTM")
                {
                    for (int i = 0; i < font.TexCount; i++) {
                        Graphics.TextureWorker.ReplaceNewTextures(fs, c, checkHeaderStr, font.NewTex[i], true);
                    }
                }
                else
                {
                    if (font.NewTex != null && font.NewTex.Length > 0)
                    {
                        // mode=2: write texture headers for all textures
                        for(int i = 0; i < font.TexCount; i++)
                        {
                            Graphics.TextureWorker.ReplaceNewTextures(fs, 2, checkHeaderStr, font.NewTex[i], true);
                        }

                        // Record position BEFORE LastZero: BlockTexSize must NOT include LastZero.
                        // The Telltale engine reads exactly BlockTexSize bytes as the texture header block
                        // (starting from the BlockTexSize field itself). Including LastZero shifts all
                        // subsequent mip/width/height fields by 1 byte → wrong texture → invisible font.
                        long posAfterHeaders = bw.BaseStream.Position;

                        // Calculate BlockTexSize: bytes from BlockTexSize field to end of texture headers,
                        // NOT including the trailing LastZero padding byte.
                        int blockTexSize = (int)(posAfterHeaders - posBeforeBlock);

                        // LastZero belongs to the gap after texture headers and before pixel data.
                        if (font.NewTex[0].SomeValue > 4)
                        {
                            bw.Write(font.LastZero);
                        }

                        // Pixel data starts immediately after the optional LastZero byte.
                        long pixelDataStart = bw.BaseStream.Position;

                        // mode=3: write pixel data for all textures
                        for(int i = 0; i < font.TexCount; i++)
                        {
                            Graphics.TextureWorker.ReplaceNewTextures(fs, 3, checkHeaderStr, font.NewTex[i], true);
                        }

                        // Write back BlockTexSize to file (it was a placeholder before)
                        long currentPos = bw.BaseStream.Position;
                        bw.BaseStream.Seek(posBeforeBlock, SeekOrigin.Begin);
                        bw.Write(blockTexSize);
                        bw.BaseStream.Seek(currentPos, SeekOrigin.Begin);

                        // Calculate headerSize using formula:
                        // tex_data_start = headerSize + 16 + countElements*12 + 4
                        int countElements = (font.binElements != null) ? font.binElements.Length : 0;
                        font.headerSize = (int)(pixelDataStart - 16 - countElements * 12 - 4);
                    }

                    bw.BaseStream.Seek(4, SeekOrigin.Begin);
                    bw.Write(font.headerSize);
                    bw.BaseStream.Seek(8, SeekOrigin.Begin);
                    bw.Write(0); // offset 8: must be 0 in all official NewFormat fonts (not TexCount)
                    bw.BaseStream.Seek(12, SeekOrigin.Begin);
                    bw.Write(font.texSize);
                }

            }

            bw.Close();
            fs.Close();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFD = new SaveFileDialog();
            saveFD.Filter = "font files (*.font)|*.font";
            saveFD.FileName = ofd.SafeFileName.ToString();
            if (saveFD.ShowDialog() == DialogResult.OK)
            {
                Methods.DeleteCurrentFile((saveFD.FileName));
                FileStream fs = new FileStream((saveFD.FileName), FileMode.OpenOrCreate);
                SaveFont(fs, font);
                fs.Close();

                encFunc(saveFD.FileName);

                // Copy generated DDS texture files if they exist
                if (lastGeneratedPagesCount > 0 && !string.IsNullOrEmpty(lastGeneratedSavePath))
                {
                    string oldDir = Path.GetDirectoryName(lastGeneratedSavePath);
                    string oldBaseName = Path.GetFileNameWithoutExtension(lastGeneratedSavePath);
                    string newDir = Path.GetDirectoryName(saveFD.FileName);
                    string newBaseName = Path.GetFileNameWithoutExtension(saveFD.FileName);

                    for (int i = 0; i < lastGeneratedPagesCount; i++)
                    {
                        int texIdx = (lastGeneratedPagesStartIndex >= 0 ? lastGeneratedPagesStartIndex : 0) + i;
                        string oldDdsPath = Path.Combine(oldDir, $"{oldBaseName}_page{texIdx}.dds");
                        if (File.Exists(oldDdsPath))
                        {
                            string newDdsPath = Path.Combine(newDir, $"{newBaseName}_page{texIdx}.dds");
                            File.Copy(oldDdsPath, newDdsPath, true);
                            textBoxLogOutput.AppendText($"Copied DDS: {Path.GetFileName(newDdsPath)}\r\n");
                        }
                    }
                }

                edited = false; //Файл сохранили, так что вернули флаг на ЛОЖЬ
            }
        }
    }
}
