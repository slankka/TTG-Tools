using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TTG_Tools.ClassesStructs;

namespace TTG_Tools
{
    public partial class FontEditor
    {
        private void ReplaceTexture(string DdsFile, ClassesStructs.TextureClass.OldT3Texture tex)
        {
            FileStream fs = new FileStream(DdsFile, FileMode.Open);
            byte[] temp = Methods.ReadFull(fs);
            fs.Close();

            tex.Content = new byte[temp.Length];
            Array.Copy(temp, 0, tex.Content, 0, temp.Length);

            MemoryStream ms = new MemoryStream(tex.Content);
            Graphics.TextureWorker.ReadDDSHeader(ms, ref tex.Width, ref tex.Height, ref tex.Mip, ref tex.TextureFormat, false);
            ms.Close();

            /*if (tex.isPS3)
            {
                int tmpPos = tex.block.Length;

                byte texFormat = 0;

                int texSize = tex.Content.Length;
                int paddedSize = Methods.pad_size(texSize, 128);

                //cut dds header and copy to padded block
                byte[] tmp = new byte[paddedSize - 128];
                Array.Copy(tex.Content, 128, tmp, 0, tex.Content.Length - 128);
                tex.Content = new byte[tmp.Length];
                Array.Copy(tmp, 0, tex.Content, 0, tmp.Length);

                switch (tex.TextureFormat)
                {
                    case (uint)ClassesStructs.TextureClass.OldTextureFormat.DX_DXT1:
                        texFormat = 0x86;
                        break;

                    case (uint)ClassesStructs.TextureClass.OldTextureFormat.DX_DXT5:
                        texFormat = 0x88;
                        break;
                }

                tmp = new byte[1];
                tmp[0] = Convert.ToByte(tex.Mip);
                Array.Copy(tmp, 0, tex.block, tmpPos - 103, tmp.Length);

                tmp = new byte[1];
                tmp[0] = texFormat;
                Array.Copy(tmp, 0, tex.block, tmpPos - 104, tmp.Length);

                tmp = new byte[1];
                tmp[0] = Convert.ToByte(tex.Mip);
                Array.Copy(tmp, 0, tex.block, tmpPos - 103, tmp.Length);

                tmp = BitConverter.GetBytes(tex.Width).Reverse().ToArray();
                Array.Copy(tmp, 2, tex.block, tmpPos - 96, 2);

                tmp = BitConverter.GetBytes(tex.Height).Reverse().ToArray();
                Array.Copy(tmp, 2, tex.block, tmpPos - 94, 2);


                tex.TexSize = texSize;

                tmp = BitConverter.GetBytes(texSize - 128).Reverse().ToArray();
                Array.Copy(tmp, 0, tex.block, tmpPos - 124, tmp.Length);

                tmp = BitConverter.GetBytes(paddedSize - 128).Reverse().ToArray();
                Array.Copy(tmp, 0, tex.block, tmpPos - 108, tmp.Length);

                paddedSize += 4; //Add 4 bytes for common size block
                tmp = BitConverter.GetBytes(paddedSize);
                Array.Copy(tmp, 0, tex.block, tmpPos - 132, tmp.Length);
            }*/

            tex.OriginalHeight = tex.Height;
            tex.OriginalWidth = tex.Width;
            font.BlockTexSize += tex.Content.Length - tex.TexSize;
            if(!tex.isPS3) tex.TexSize = tex.Content.Length;
        }

        private void ReplaceTexture(string DdsFile, ClassesStructs.TextureClass.NewT3Texture NewTex)
        {
            byte[] temp = File.ReadAllBytes(DdsFile);
            NewTex.Tex.Content = new byte[temp.Length];
            Array.Copy(temp, 0, NewTex.Tex.Content, 0, temp.Length);

            MemoryStream ms = new MemoryStream(NewTex.Tex.Content);

            FileInfo fi = new FileInfo(DdsFile);

            if (fi.Extension.ToLower() == ".dds")
            {
                Graphics.TextureWorker.ReadDDSHeader(ms, ref NewTex.Width, ref NewTex.Height, ref NewTex.Mip, ref NewTex.TextureFormat, true);
                NewTex.platform.platform = ResolveTargetPlatformForImportedDds(NewTex.platform.platform);

                uint ddsCaps;
                uint ddsCaps2;
                if (TryReadDdsCaps(temp, out ddsCaps, out ddsCaps2))
                {
                    bool hasCubeMap = (ddsCaps2 & 0x200) != 0;
                    textBoxLogOutput.AppendText($"[ImportDDS] {Path.GetFileName(DdsFile)} Caps=0x{ddsCaps:X8}, Caps2=0x{ddsCaps2:X8}, Cubemap={hasCubeMap}\r\n");

                    if (hasCubeMap)
                    {
                        textBoxLogOutput.AppendText("[ImportDDS] Note: FNT workflow imports DDS as 2D atlas; cubemap flags will not be preserved.\r\n");
                    }
                }

                // Font atlas import must stay 2D. Prevent stale cubemap/array metadata
                // from previously loaded textures from leaking into save output.
                NewTex.Faces = 1;
                NewTex.ArrayMembers = 1;
            }
            else
            {
                Graphics.TextureWorker.ReadPvrHeader(ms, ref NewTex.Width, ref NewTex.Height, ref NewTex.Mip, ref NewTex.platform.platform, true);
                if (NewTex.platform.platform != 7u && NewTex.platform.platform != 9u)
                {
                    NewTex.platform.platform = 7u;
                }
            }

            NewTex.Mip = 1; //There is no need more than one mip map!
            NewTex.Tex.MipCount = NewTex.Mip;
            NewTex.Tex.Textures = new ClassesStructs.TextureClass.NewT3Texture.TextureStruct[NewTex.Mip];

            // New font + Import DDS may produce textures without a valid NewT3 metadata shell.
            // Fill a minimal, self-consistent header so Save/Open stays byte-aligned.
            EnsureNewTextureHeaderDefaults(NewTex);

            int w = NewTex.Width;
            int h = NewTex.Height;

            int pos = (int)ms.Position;
            ms.Close();

            NewTex.Tex.TexSize = 0;

            int blockSize = NewTex.TextureFormat == 0x40 || NewTex.TextureFormat == 0x43 ? 8 : 16;

            for (int i = 0; i < NewTex.Tex.MipCount; i++)
            {
                NewTex.Tex.Textures[i].CurrentMip = i;
                Methods.getSizeAndKratnost(w, h, (int)NewTex.TextureFormat, ref NewTex.Tex.Textures[i].MipSize, ref NewTex.Tex.Textures[i].BlockSize);
                int sourceMipSize = NewTex.Tex.Textures[i].MipSize;

                NewTex.Tex.Textures[i].Block = new byte[NewTex.Tex.Textures[i].MipSize];

                Array.Copy(NewTex.Tex.Content, pos, NewTex.Tex.Textures[i].Block, 0, NewTex.Tex.Textures[i].Block.Length);

                // Block stays linear in memory; swizzle is applied during Save As (ReplaceNewTextures).

                pos += sourceMipSize;
                NewTex.Tex.TexSize += (uint)NewTex.Tex.Textures[i].MipSize;

                if (NewTex.SomeValue >= 5) NewTex.Tex.Textures[i].SubTexNum = 0;
                if (NewTex.HasOneValueTex) NewTex.Tex.Textures[i].One = 1;

                if (w > 1) w /= 2;
                if (h > 1) h /= 2;
            }
        }

        private static bool TryReadDdsCaps(byte[] ddsContent, out uint caps, out uint caps2)
        {
            caps = 0;
            caps2 = 0;

            if (ddsContent == null || ddsContent.Length < 128)
            {
                return false;
            }

            if (Encoding.ASCII.GetString(ddsContent, 0, 4) != "DDS ")
            {
                return false;
            }

            caps = BitConverter.ToUInt32(ddsContent, 108);
            caps2 = BitConverter.ToUInt32(ddsContent, 112);
            return true;
        }

        private static string FormatBytesPreview(byte[] data, int maxBytes = 16)
        {
            if (data == null)
            {
                return "null";
            }

            if (data.Length == 0)
            {
                return "empty";
            }

            int len = Math.Min(maxBytes, data.Length);
            byte[] preview = new byte[len];
            Array.Copy(data, 0, preview, 0, len);
            string suffix = data.Length > len ? "..." : string.Empty;
            return BitConverter.ToString(preview) + suffix;
        }

        private static byte[] CreateMinimalSubBlock()
        {
            // Parser expects size-at-start and advances by this value.
            // The minimal valid empty block is a 4-byte self-sized block.
            return BitConverter.GetBytes(4);
        }

        private static int GetDefaultMainBlockSize(int someValue)
        {
            switch (someValue)
            {
                case 3:
                case 4:
                    return 0x28;
                case 5:
                case 7:
                    return 0x34;
                case 8:
                case 9:
                    return 0x38;
                default:
                    return 0x24;
            }
        }

        private void EnsureNewTextureHeaderDefaults(ClassesStructs.TextureClass.NewT3Texture tex)
        {
            if (tex.SomeValue < 3)
            {
                tex.SomeValue = 9;
            }

            if (tex.unknownFlags.blockSize <= 0)
            {
                tex.unknownFlags.blockSize = 8;
            }

            if (tex.platform.blockSize <= 0)
            {
                tex.platform.blockSize = 8;
            }

            if (tex.OneByte == 0)
            {
                tex.OneByte = 0x30;
            }

            tex.ObjectName = SanitizeObjectHeaderName(tex.ObjectName ?? string.Empty);
            // Keep SubObjectName raw so full texture paths and material names remain intact.
            tex.SubObjectName = tex.SubObjectName ?? string.Empty;

            if (tex.SomeValue >= 8)
            {
                if (tex.Faces <= 0) tex.Faces = 1;
                if (tex.ArrayMembers <= 0) tex.ArrayMembers = 1;
            }

            int defaultBlockSize = GetDefaultMainBlockSize(tex.SomeValue);
            if (AddInfo)
            {
                defaultBlockSize += 4;
            }
            if (tex.block == null || tex.block.Length == 0)
            {
                tex.block = new byte[defaultBlockSize];
            }

            if (tex.subBlock.Block == null || tex.subBlock.Block.Length < 4)
            {
                tex.subBlock.Block = CreateMinimalSubBlock();
            }
            tex.subBlock.Size = tex.subBlock.Block.Length;

            if (tex.SomeValue >= 8)
            {
                if (tex.subBlock2.Block == null || tex.subBlock2.Block.Length < 4)
                {
                    tex.subBlock2.Block = CreateMinimalSubBlock();
                }
                tex.subBlock2.Size = tex.subBlock2.Block.Length;
            }

            if (tex.Tex.SubBlocks == null)
            {
                tex.Tex.SubBlocks = new ClassesStructs.TextureClass.NewT3Texture.SubBlock[0];
            }

            tex.Tex.SomeData = 0;

            // Nintendo Switch requires specific texture header fields to match the official format.
            // Official Switch fonts always have: unknownFlags.block=0x111, HasOneValueTex=true,
            // OneValue=1, subBlock/subBlock2 = 8-byte blocks.
            // Without HasOneValueTex=true, the per-mip "One" field is omitted, causing the game
            // to misread every subsequent byte in the texture header and crash on load.
            if (tex.platform.platform == 15)
            {
                if (tex.unknownFlags.block == 0)
                    tex.unknownFlags.block = 0x00000111;
                if (!tex.HasOneValueTex)
                    tex.HasOneValueTex = true;
                if (tex.OneValue == 0)
                    tex.OneValue = 1;
                // Switch expects 8-byte subBlocks: first 4 bytes = size (8), next 4 bytes = padding.
                if (tex.subBlock.Block == null || tex.subBlock.Block.Length < 8)
                    tex.subBlock.Block = new byte[] { 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                tex.subBlock.Size = tex.subBlock.Block.Length;
                if (tex.SomeValue >= 8)
                {
                    if (tex.subBlock2.Block == null || tex.subBlock2.Block.Length < 8)
                        tex.subBlock2.Block = new byte[] { 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                    tex.subBlock2.Size = tex.subBlock2.Block.Length;
                }
                // Set Switch-specific main block if uninitialized (all zeros = game crash/artifacts).
                // Template from Normal/cheapSignage_medium.font (2048x2048, BC3, platform=15, SomeValue=9).
                if (tex.block == null || tex.block.Length == 0 || Array.TrueForAll(tex.block, b => b == 0))
                {
                    tex.block = new byte[]
                    {
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x0f, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03,
                        0x00, 0x00, 0x00, 0x41, 0x00, 0x00, 0xc0, 0x40, 0x00, 0x00, 0x80, 0xbf,
                        0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x3f, 0x00, 0x00, 0x80, 0x3f
                    };
                }
            }
        }

        private void fillTableofCoordinates(FontClass.ClassFont font, bool Modified)
        {
            if (!font.NewFormat)
            {
                dataGridViewWithCoord.RowCount = font.glyph.CharCount;
                dataGridViewWithCoord.ColumnCount = 7;
                if (font.hasScaleValue)
                {
                    dataGridViewWithCoord.ColumnCount = 9;
                    dataGridViewWithCoord.Columns[7].HeaderText = "Width";
                    dataGridViewWithCoord.Columns[8].HeaderText = "Height";
                }

                for (int i = 0; i < font.glyph.CharCount; i++)
                {
                    dataGridViewWithCoord.Rows[i].HeaderCell.Value = Convert.ToString(i + 1);
                    dataGridViewWithCoord[0, i].Value = i;
                    dataGridViewWithCoord[1, i].Value = Encoding.GetEncoding(MainMenu.settings.ASCII_N).GetString(BitConverter.GetBytes(i)).Replace("\0", string.Empty);
                    dataGridViewWithCoord[2, i].Value = font.glyph.chars[i].XStart;
                    dataGridViewWithCoord[3, i].Value = font.glyph.chars[i].XEnd;
                    dataGridViewWithCoord[4, i].Value = font.glyph.chars[i].YStart;
                    dataGridViewWithCoord[5, i].Value = font.glyph.chars[i].YEnd;
                    dataGridViewWithCoord[6, i].Value = font.glyph.chars[i].TexNum;

                    if (font.hasScaleValue)
                    {
                        dataGridViewWithCoord[7, i].Value = font.glyph.chars[i].CharWidth;
                        dataGridViewWithCoord[8, i].Value = font.glyph.chars[i].CharHeight;
                    }
                }
            }
            else
            {
                dataGridViewWithCoord.RowCount = font.glyph.CharCount;
                dataGridViewWithCoord.ColumnCount = 13;
                dataGridViewWithCoord.Columns[7].HeaderText = "Width";
                dataGridViewWithCoord.Columns[8].HeaderText = "Height";
                dataGridViewWithCoord.Columns[9].HeaderText = "Offset by X";
                dataGridViewWithCoord.Columns[10].HeaderText = "Offset by Y";
                dataGridViewWithCoord.Columns[11].HeaderText = "X advance";
                dataGridViewWithCoord.Columns[12].HeaderText = "Channel";

                // Check if charsNew is null
                if (font.glyph.charsNew == null || font.glyph.charsNew.Length == 0)
                {
                    textBoxLogOutput.AppendText("Warning: charsNew is null or empty. Cannot display font data.\r\n");
                    return;
                }

                for (int i = 0; i < font.glyph.CharCount; i++)
                {
                    dataGridViewWithCoord.Rows[i].HeaderCell.Value = Convert.ToString(i + 1);

                    if (font.glyph.charsNew[i] != null)
                    {
                        dataGridViewWithCoord[0, i].Value = font.glyph.charsNew[i].charId;

                        dataGridViewWithCoord[1, i].Value = Encoding.GetEncoding(MainMenu.settings.ASCII_N).GetString(BitConverter.GetBytes(font.glyph.charsNew[i].charId)).Replace("\0", string.Empty);

                        if(MainMenu.settings.unicodeSettings == 0)
                        {
                            dataGridViewWithCoord[1, i].Value = Encoding.Unicode.GetString(BitConverter.GetBytes(font.glyph.charsNew[i].charId)).Replace("\0", string.Empty);
                        }

                        dataGridViewWithCoord[2, i].Value = font.glyph.charsNew[i].XStart;
                        dataGridViewWithCoord[3, i].Value = font.glyph.charsNew[i].XEnd;
                        dataGridViewWithCoord[4, i].Value = font.glyph.charsNew[i].YStart;
                        dataGridViewWithCoord[5, i].Value = font.glyph.charsNew[i].YEnd;
                        dataGridViewWithCoord[6, i].Value = font.glyph.charsNew[i].TexNum;
                        dataGridViewWithCoord[7, i].Value = font.glyph.charsNew[i].CharWidth;
                        dataGridViewWithCoord[8, i].Value = font.glyph.charsNew[i].CharHeight;
                        dataGridViewWithCoord[9, i].Value = font.glyph.charsNew[i].XOffset;
                        dataGridViewWithCoord[10, i].Value = font.glyph.charsNew[i].YOffset;
                        dataGridViewWithCoord[11, i].Value = font.glyph.charsNew[i].XAdvance;
                        dataGridViewWithCoord[12, i].Value = font.glyph.charsNew[i].Channel;
                    }
                    else
                    {
                        textBoxLogOutput.AppendText($"Warning: Character at index {i} is not initialized (null). Skipping.\r\n");
                    }
                }
            }

            for(int k = 0; k < dataGridViewWithCoord.RowCount; k++)
            {
                for(int l = 0; l < dataGridViewWithCoord.ColumnCount; l++)
                {
                    dataGridViewWithCoord[l, k].Style.BackColor = Modified ? Color.GreenYellow : Color.White;
                }
            }

            // Update font name display
            if (!string.IsNullOrEmpty(font.FontName))
            {
                labelFontName.Text = "Font: " + font.FontName;
            }
            else
            {
                labelFontName.Text = "Font: N/A";
            }
        }

        private void fillTableofTextures(FontClass.ClassFont font)
        {
            dataGridViewWithTextures.RowCount = font.TexCount;

            if (!font.NewFormat)
            {
                for (int i = 0; i < font.TexCount; i++)
                {
                    dataGridViewWithTextures[0, i].Value = i;
                    dataGridViewWithTextures[1, i].Value = font.tex[i].Height;
                    dataGridViewWithTextures[2, i].Value = font.tex[i].Width;
                    dataGridViewWithTextures[3, i].Value = font.tex[i].TexSize;
                }
            }
            else
            {
                for (int i = 0; i < font.TexCount; i++)
                {
                    dataGridViewWithTextures[0, i].Value = i;
                    dataGridViewWithTextures[1, i].Value = font.NewTex[i].Height;
                    dataGridViewWithTextures[2, i].Value = font.NewTex[i].Width;
                    dataGridViewWithTextures[3, i].Value = font.NewTex[i].Tex.TexSize;
                }
            }

            if (dataGridViewWithTextures.RowCount > 0)
            {
                dataGridViewWithTextures.Rows[0].Selected = true;
            }

            UpdateTexturePreview();
        }

        private void UpdateTexturePreview()
        {
            if (font == null)
            {
                SetPreviewImage(null);
                return;
            }

            int texIndex = GetSelectedTextureIndex();
            if (texIndex < 0)
            {
                SetPreviewImage(null);
                return;
            }

            int texWidth = 0;
            int texHeight = 0;

            if (!font.NewFormat && font.tex != null && texIndex < font.tex.Length)
            {
                texWidth = font.tex[texIndex].Width;
                texHeight = font.tex[texIndex].Height;
                Bitmap preview = BuildBitmapPreview(font.tex[texIndex].Content, font.tex[texIndex].TextureFormat, texWidth, texHeight);
                if (basePreviewBitmap != null) basePreviewBitmap.Dispose();
                basePreviewBitmap = preview;
            }
            else if (font.NewFormat && font.NewTex != null && texIndex < font.NewTex.Length)
            {
                texWidth = font.NewTex[texIndex].Width;
                texHeight = font.NewTex[texIndex].Height;
                Bitmap preview = BuildBitmapPreview(font.NewTex[texIndex].Tex.Content, font.NewTex[texIndex].TextureFormat, texWidth, texHeight);
                if (basePreviewBitmap != null) basePreviewBitmap.Dispose();
                basePreviewBitmap = preview;
            }

            if (basePreviewBitmap == null && texWidth > 0 && texHeight > 0)
            {
                if (basePreviewBitmap != null) basePreviewBitmap.Dispose();
                basePreviewBitmap = CreateFallbackPreview(texWidth, texHeight);
            }

            if (basePreviewBitmap == null)
            {
                SetPreviewImage(null);
                return;
            }

            Bitmap rendered = (Bitmap)basePreviewBitmap.Clone();
            DrawSelectedCharacterBounds(rendered, texIndex);
            SetPreviewImage(rendered);
        }

        private void SetPreviewImage(Image image)
        {
            if (pictureBoxTexturePreview.Image != null)
            {
                var oldImage = pictureBoxTexturePreview.Image;
                pictureBoxTexturePreview.Image = null;
                oldImage.Dispose();
            }

            pictureBoxTexturePreview.Image = image;
        }

        private int GetSelectedTextureIndex()
        {
            if (dataGridViewWithTextures.SelectedCells.Count == 0)
            {
                return -1;
            }

            int rowIndex = dataGridViewWithTextures.SelectedCells[0].RowIndex;
            if (rowIndex < 0 || rowIndex >= dataGridViewWithTextures.RowCount)
            {
                return -1;
            }

            return rowIndex;
        }

        private void DrawSelectedCharacterBounds(Bitmap bitmap, int selectedTexture)
        {
            if (dataGridViewWithCoord.SelectedCells.Count == 0)
            {
                return;
            }

            int rowIndex = dataGridViewWithCoord.SelectedCells[0].RowIndex;
            if (rowIndex < 0 || rowIndex >= dataGridViewWithCoord.RowCount)
            {
                return;
            }

            int texNum;
            float xStart;
            float xEnd;
            float yStart;
            float yEnd;

            if (!TryGetGlyphRectFromRow(rowIndex, out xStart, out xEnd, out yStart, out yEnd, out texNum) || texNum != selectedTexture)
            {
                return;
            }

            int left = Math.Max(0, Math.Min(bitmap.Width - 1, (int)Math.Round(xStart)));
            int top = Math.Max(0, Math.Min(bitmap.Height - 1, (int)Math.Round(yStart)));
            int right = Math.Max(left + 1, Math.Min(bitmap.Width, (int)Math.Round(xEnd)));
            int bottom = Math.Max(top + 1, Math.Min(bitmap.Height, (int)Math.Round(yEnd)));

            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
            using (Pen pen = new Pen(Color.Red, 2f))
            {
                g.DrawRectangle(pen, left, top, Math.Max(1, right - left), Math.Max(1, bottom - top));
            }
        }

        private bool TryGetGlyphRectFromRow(int rowIndex, out float xStart, out float xEnd, out float yStart, out float yEnd, out int texNum)
        {
            xStart = xEnd = yStart = yEnd = 0;
            texNum = -1;

            if (rowIndex < 0 || rowIndex >= dataGridViewWithCoord.RowCount)
            {
                return false;
            }

            if (!float.TryParse(Convert.ToString(dataGridViewWithCoord[2, rowIndex].Value), out xStart)) return false;
            if (!float.TryParse(Convert.ToString(dataGridViewWithCoord[3, rowIndex].Value), out xEnd)) return false;
            if (!float.TryParse(Convert.ToString(dataGridViewWithCoord[4, rowIndex].Value), out yStart)) return false;
            if (!float.TryParse(Convert.ToString(dataGridViewWithCoord[5, rowIndex].Value), out yEnd)) return false;
            if (!int.TryParse(Convert.ToString(dataGridViewWithCoord[6, rowIndex].Value), out texNum)) return false;

            return true;
        }

        private Bitmap CreateFallbackPreview(int width, int height)
        {
            Bitmap bmp = new Bitmap(Math.Max(1, width), Math.Max(1, height));
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp))
            {
                g.Clear(Color.DimGray);
            }

            return bmp;
        }

        private Bitmap BuildBitmapPreview(byte[] texContent, uint texFormat, int width, int height)
        {
            if (texContent == null || texContent.Length == 0 || width <= 0 || height <= 0)
            {
                return null;
            }

            byte[] ddsPixels;
            int ddsWidth;
            int ddsHeight;
            if (TryDecodeDdsToBgra(texContent, out ddsPixels, out ddsWidth, out ddsHeight))
            {
                return BuildBitmapFromRgbaBuffer(ddsPixels, ddsWidth, ddsHeight);
            }

            int dataOffset = 0;
            byte[] pixels = new byte[width * height * 4];

            if (texFormat == (uint)TextureClass.OldTextureFormat.DX_ARGB8888 || texFormat == (uint)TextureClass.NewTextureFormat.ARGB8)
            {
                int needed = width * height * 4;
                if (texContent.Length - dataOffset < needed)
                {
                    return null;
                }

                for (int i = 0; i < needed; i += 4)
                {
                    int src = dataOffset + i;
                    pixels[i] = texContent[src + 2];
                    pixels[i + 1] = texContent[src + 1];
                    pixels[i + 2] = texContent[src];
                    pixels[i + 3] = texContent[src + 3];
                }

                return BuildBitmapFromRgbaBuffer(pixels, width, height);
            }

            if (texFormat == (uint)TextureClass.OldTextureFormat.DX_L8 || texFormat == (uint)TextureClass.NewTextureFormat.IL8 || texFormat == (uint)TextureClass.NewTextureFormat.A8)
            {
                int needed = width * height;
                if (texContent.Length - dataOffset < needed)
                {
                    return null;
                }

                for (int i = 0; i < needed; i++)
                {
                    byte value = texContent[dataOffset + i];
                    int dst = i * 4;
                    pixels[dst] = value;
                    pixels[dst + 1] = value;
                    pixels[dst + 2] = value;
                    pixels[dst + 3] = 255;
                }

                return BuildBitmapFromRgbaBuffer(pixels, width, height);
            }

            return null;
        }

        private bool TryDecodeDdsToBgra(byte[] content, out byte[] pixels, out int width, out int height)
        {
            pixels = null;
            width = 0;
            height = 0;

            if (content.Length < 128 || Encoding.ASCII.GetString(content, 0, 4) != "DDS ")
            {
                return false;
            }

            width = BitConverter.ToInt32(content, 16);
            height = BitConverter.ToInt32(content, 12);
            if (width <= 0 || height <= 0)
            {
                return false;
            }

            int fourCc = BitConverter.ToInt32(content, 84);
            int rgbBitCount = BitConverter.ToInt32(content, 88);
            int rMask = BitConverter.ToInt32(content, 92);
            int gMask = BitConverter.ToInt32(content, 96);
            int bMask = BitConverter.ToInt32(content, 100);
            int aMask = BitConverter.ToInt32(content, 104);

            int dataOffset = 128;

            if (fourCc == 0x31545844) // DXT1
            {
                return DecodeDxt1(content, dataOffset, width, height, out pixels);
            }

            if (fourCc == 0x33545844) // DXT3
            {
                return DecodeDxt3(content, dataOffset, width, height, out pixels);
            }

            if (fourCc == 0x35545844) // DXT5
            {
                return DecodeDxt5(content, dataOffset, width, height, out pixels);
            }

            if (fourCc == 0 && rgbBitCount == 32)
            {
                return DecodeBgra32(content, dataOffset, width, height, rMask, gMask, bMask, aMask, out pixels);
            }

            if (fourCc == 0 && rgbBitCount == 8)
            {
                int required = width * height;
                if (content.Length - dataOffset < required)
                {
                    return false;
                }

                pixels = new byte[width * height * 4];
                for (int i = 0; i < required; i++)
                {
                    byte v = content[dataOffset + i];
                    int d = i * 4;
                    pixels[d] = v;
                    pixels[d + 1] = v;
                    pixels[d + 2] = v;
                    pixels[d + 3] = 255;
                }

                return true;
            }

            return false;
        }

        private bool DecodeBgra32(byte[] content, int dataOffset, int width, int height, int rMask, int gMask, int bMask, int aMask, out byte[] pixels)
        {
            pixels = null;
            int required = width * height * 4;
            if (content.Length - dataOffset < required)
            {
                return false;
            }

            pixels = new byte[required];
            bool standardBgra = rMask == unchecked((int)0x00ff0000) && gMask == 0x0000ff00 && bMask == 0x000000ff;
            for (int i = 0; i < width * height; i++)
            {
                int s = dataOffset + i * 4;
                int d = i * 4;

                if (standardBgra)
                {
                    pixels[d] = content[s];
                    pixels[d + 1] = content[s + 1];
                    pixels[d + 2] = content[s + 2];
                    pixels[d + 3] = aMask == 0 ? (byte)255 : content[s + 3];
                }
                else
                {
                    uint packed = BitConverter.ToUInt32(content, s);
                    byte r = ExtractMaskedByte(packed, (uint)rMask);
                    byte g = ExtractMaskedByte(packed, (uint)gMask);
                    byte b = ExtractMaskedByte(packed, (uint)bMask);
                    byte a = aMask == 0 ? (byte)255 : ExtractMaskedByte(packed, (uint)aMask);

                    pixels[d] = b;
                    pixels[d + 1] = g;
                    pixels[d + 2] = r;
                    pixels[d + 3] = a;
                }
            }

            return true;
        }

        private byte ExtractMaskedByte(uint value, uint mask)
        {
            if (mask == 0)
            {
                return 0;
            }

            int shift = 0;
            while (((mask >> shift) & 1u) == 0u && shift < 32)
            {
                shift++;
            }

            uint raw = (value & mask) >> shift;
            uint max = mask >> shift;
            if (max == 0)
            {
                return 0;
            }

            return (byte)((raw * 255u) / max);
        }

        private bool DecodeDxt1(byte[] content, int dataOffset, int width, int height, out byte[] pixels)
        {
            pixels = new byte[width * height * 4];
            int blockCountX = (width + 3) / 4;
            int blockCountY = (height + 3) / 4;
            int offset = dataOffset;

            for (int by = 0; by < blockCountY; by++)
            {
                for (int bx = 0; bx < blockCountX; bx++)
                {
                    if (offset + 8 > content.Length)
                    {
                        return false;
                    }

                    ushort c0 = BitConverter.ToUInt16(content, offset);
                    ushort c1 = BitConverter.ToUInt16(content, offset + 2);
                    uint indices = BitConverter.ToUInt32(content, offset + 4);
                    offset += 8;

                    Color32[] palette = BuildDxt1Palette(c0, c1);
                    WriteColorBlock(pixels, width, height, bx, by, indices, palette);
                }
            }

            return true;
        }

        private bool DecodeDxt3(byte[] content, int dataOffset, int width, int height, out byte[] pixels)
        {
            pixels = new byte[width * height * 4];
            int blockCountX = (width + 3) / 4;
            int blockCountY = (height + 3) / 4;
            int offset = dataOffset;

            for (int by = 0; by < blockCountY; by++)
            {
                for (int bx = 0; bx < blockCountX; bx++)
                {
                    if (offset + 16 > content.Length)
                    {
                        return false;
                    }

                    ulong alphaBits = BitConverter.ToUInt64(content, offset);
                    ushort c0 = BitConverter.ToUInt16(content, offset + 8);
                    ushort c1 = BitConverter.ToUInt16(content, offset + 10);
                    uint indices = BitConverter.ToUInt32(content, offset + 12);
                    offset += 16;

                    Color32[] palette = BuildDxt1PaletteOpaque(c0, c1);
                    for (int py = 0; py < 4; py++)
                    {
                        for (int px = 0; px < 4; px++)
                        {
                            int pixelIndex = py * 4 + px;
                            int alpha4 = (int)((alphaBits >> (pixelIndex * 4)) & 0xF);
                            byte alpha = (byte)(alpha4 * 17);
                            int code = (int)((indices >> (2 * pixelIndex)) & 0x3);
                            SetPixelFromBlock(pixels, width, height, bx, by, px, py, palette[code], alpha);
                        }
                    }
                }
            }

            return true;
        }

        private bool DecodeDxt5(byte[] content, int dataOffset, int width, int height, out byte[] pixels)
        {
            pixels = new byte[width * height * 4];
            int blockCountX = (width + 3) / 4;
            int blockCountY = (height + 3) / 4;
            int offset = dataOffset;

            for (int by = 0; by < blockCountY; by++)
            {
                for (int bx = 0; bx < blockCountX; bx++)
                {
                    if (offset + 16 > content.Length)
                    {
                        return false;
                    }

                    byte a0 = content[offset];
                    byte a1 = content[offset + 1];
                    ulong alphaBits = 0;
                    for (int i = 0; i < 6; i++)
                    {
                        alphaBits |= ((ulong)content[offset + 2 + i]) << (8 * i);
                    }

                    ushort c0 = BitConverter.ToUInt16(content, offset + 8);
                    ushort c1 = BitConverter.ToUInt16(content, offset + 10);
                    uint indices = BitConverter.ToUInt32(content, offset + 12);
                    offset += 16;

                    byte[] alphaPalette = BuildDxt5AlphaPalette(a0, a1);
                    Color32[] colorPalette = BuildDxt1PaletteOpaque(c0, c1);

                    for (int py = 0; py < 4; py++)
                    {
                        for (int px = 0; px < 4; px++)
                        {
                            int pixelIndex = py * 4 + px;
                            int alphaCode = (int)((alphaBits >> (3 * pixelIndex)) & 0x7);
                            byte alpha = alphaPalette[alphaCode];
                            int colorCode = (int)((indices >> (2 * pixelIndex)) & 0x3);
                            SetPixelFromBlock(pixels, width, height, bx, by, px, py, colorPalette[colorCode], alpha);
                        }
                    }
                }
            }

            return true;
        }

        private struct Color32
        {
            public byte B;
            public byte G;
            public byte R;
            public byte A;
        }

        private Color32[] BuildDxt1Palette(ushort c0, ushort c1)
        {
            Color32[] palette = new Color32[4];
            palette[0] = Rgb565ToColor(c0);
            palette[1] = Rgb565ToColor(c1);

            if (c0 > c1)
            {
                palette[2] = LerpColor(palette[0], palette[1], 2, 1, 3);
                palette[3] = LerpColor(palette[0], palette[1], 1, 2, 3);
            }
            else
            {
                palette[2] = LerpColor(palette[0], palette[1], 1, 1, 2);
                palette[3] = new Color32 { B = 0, G = 0, R = 0, A = 0 };
            }

            return palette;
        }

        private Color32[] BuildDxt1PaletteOpaque(ushort c0, ushort c1)
        {
            Color32[] palette = new Color32[4];
            palette[0] = Rgb565ToColor(c0);
            palette[1] = Rgb565ToColor(c1);
            palette[2] = LerpColor(palette[0], palette[1], 2, 1, 3);
            palette[3] = LerpColor(palette[0], palette[1], 1, 2, 3);
            return palette;
        }

        private byte[] BuildDxt5AlphaPalette(byte a0, byte a1)
        {
            byte[] p = new byte[8];
            p[0] = a0;
            p[1] = a1;

            if (a0 > a1)
            {
                p[2] = (byte)((6 * a0 + 1 * a1) / 7);
                p[3] = (byte)((5 * a0 + 2 * a1) / 7);
                p[4] = (byte)((4 * a0 + 3 * a1) / 7);
                p[5] = (byte)((3 * a0 + 4 * a1) / 7);
                p[6] = (byte)((2 * a0 + 5 * a1) / 7);
                p[7] = (byte)((1 * a0 + 6 * a1) / 7);
            }
            else
            {
                p[2] = (byte)((4 * a0 + 1 * a1) / 5);
                p[3] = (byte)((3 * a0 + 2 * a1) / 5);
                p[4] = (byte)((2 * a0 + 3 * a1) / 5);
                p[5] = (byte)((1 * a0 + 4 * a1) / 5);
                p[6] = 0;
                p[7] = 255;
            }

            return p;
        }

        private Color32 Rgb565ToColor(ushort value)
        {
            byte r = (byte)((((value >> 11) & 0x1F) * 255 + 15) / 31);
            byte g = (byte)((((value >> 5) & 0x3F) * 255 + 31) / 63);
            byte b = (byte)(((value & 0x1F) * 255 + 15) / 31);
            return new Color32 { B = b, G = g, R = r, A = 255 };
        }

        private Color32 LerpColor(Color32 a, Color32 b, int wa, int wb, int div)
        {
            return new Color32
            {
                B = (byte)((a.B * wa + b.B * wb) / div),
                G = (byte)((a.G * wa + b.G * wb) / div),
                R = (byte)((a.R * wa + b.R * wb) / div),
                A = 255
            };
        }

        private void WriteColorBlock(byte[] dst, int width, int height, int bx, int by, uint indices, Color32[] palette)
        {
            for (int py = 0; py < 4; py++)
            {
                for (int px = 0; px < 4; px++)
                {
                    int pixelIndex = py * 4 + px;
                    int code = (int)((indices >> (2 * pixelIndex)) & 0x3);
                    Color32 color = palette[code];
                    SetPixelFromBlock(dst, width, height, bx, by, px, py, color, 255);
                }
            }
        }

        private void SetPixelFromBlock(byte[] dst, int width, int height, int bx, int by, int px, int py, Color32 color, byte alpha)
        {
            int x = bx * 4 + px;
            int y = by * 4 + py;
            if (x >= width || y >= height)
            {
                return;
            }

            int d = (y * width + x) * 4;
            dst[d] = color.B;
            dst[d + 1] = color.G;
            dst[d + 2] = color.R;
            dst[d + 3] = alpha;
        }

        private Bitmap LoadPageAsBitmap(int pageIndex)
        {
            if (font.NewTex == null || pageIndex < 0 || pageIndex >= font.NewTex.Length)
                return null;

            byte[] texContent = font.NewTex[pageIndex].Tex.Content;
            if (texContent == null || texContent.Length == 0)
                return null;

            byte[] pixels;
            int width, height;
            if (TryDecodeDdsToBgra(texContent, out pixels, out width, out height))
            {
                return BuildBitmapFromRgbaBuffer(pixels, width, height);
            }

            return null;
        }

        private int FindFirstEmptySlotFrom(Bitmap bitmap, int cellWidth, int cellHeight, int charsPerRow, int charsPerCol, int startSlot)
        {
            int totalCells = charsPerRow * charsPerCol;
            for (int slot = startSlot; slot < totalCells; slot++)
            {
                int row = slot / charsPerRow;
                int col = slot % charsPerRow;
                int startX = col * cellWidth;
                int startY = row * cellHeight;

                int endX = Math.Min(startX + cellWidth, bitmap.Width);
                int endY = Math.Min(startY + cellHeight, bitmap.Height);
                if (startX >= bitmap.Width || startY >= bitmap.Height)
                    return slot;

                bool allTransparent = true;
                for (int py = startY; py < endY && allTransparent; py++)
                {
                    for (int px = startX; px < endX && allTransparent; px++)
                    {
                        if (bitmap.GetPixel(px, py).A != 0)
                            allTransparent = false;
                    }
                }

                if (allTransparent)
                    return slot;
            }

            return -1;
        }

        private Bitmap BuildBitmapFromRgbaBuffer(byte[] rgbaPixels, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(rgbaPixels, 0, data.Scan0, rgbaPixels.Length);
            bitmap.UnlockBits(data);
            return bitmap;
        }
    }
}