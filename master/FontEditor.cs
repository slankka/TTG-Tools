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
    public partial class FontEditor : Form
    {
        [DllImport("kernel32.dll")]
        public static extern void SetProcessWorkingSetSize(IntPtr hWnd, int i, int j);

        public FontEditor()
        {
            InitializeComponent();
            SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);

            AllowDrop = true;
            DragEnter += FontEditor_DragEnter;
            DragDrop += FontEditor_DragDrop;
            EnableDragDropForControls(this);
        }

        OpenFileDialog ofd = new OpenFileDialog();
        bool edited; //Проверка на изменения в шрифте
        bool encrypted; //В случае, если шрифт был зашифрован
        byte[] encKey;
        int version;
        byte[] tmpHeader;
        byte[] check_header;
        bool someTexData;
        bool AddInfo;
        string droppedFontPath;
        private Bitmap basePreviewBitmap;
        private Graphics.WiiSupport.WiiFontData wiiFontData;
        private List<char> lastDetectedMissingChars = new List<char>(); // Store last detected missing characters
        private int lastGeneratedPagesStartIndex = -1; // Track where new pages were added
        private int lastGeneratedPagesCount = 0; // Track how many pages were generated
        private int lastGeneratedCharCount = 0; // Track how many characters were actually generated
        private int lastOriginalPagesCount = -1; // Track original page count before first generation
        private string lastGeneratedFontFamily = ""; // Track the font family used for generation
        private string lastGeneratedSavePath = ""; // Track the save path
        private int lastModifiedExistingPageIndex = -1; // Index of existing page that was modified to fill remaining slots
        private byte[] lastModifiedPageOriginalData = null; // Backup of original page DDS content before modification
        private string selectedFontFamilyName = ""; // Font family name selected for character generation
        private string selectedFontFilePath = ""; // Font file path (empty if system font)
        private System.Drawing.FontStyle selectedFontStyle = System.Drawing.FontStyle.Regular;

        private void EnableDragDropForControls(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                control.AllowDrop = true;
                control.DragEnter += FontEditor_DragEnter;
                control.DragDrop += FontEditor_DragDrop;

                if (control.HasChildren)
                {
                    EnableDragDropForControls(control);
                }
            }
        }

        private void FontEditor_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            bool hasFontFile = files != null && files.Any(file => Path.GetExtension(file).Equals(".font", StringComparison.OrdinalIgnoreCase));
            e.Effect = hasFontFile ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void FontEditor_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0)
            {
                return;
            }

            string firstFontFile = files.FirstOrDefault(file => Path.GetExtension(file).Equals(".font", StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrEmpty(firstFontFile))
            {
                MessageBox.Show("Please drop a .font file.", "Unsupported file type");
                return;
            }

            droppedFontPath = firstFontFile;
            openToolStripMenuItem_Click(this, EventArgs.Empty);
        }

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
            droppedFontPath = null;

            // Initialize headers for new font (6VSM format)
            check_header = Encoding.ASCII.GetBytes("6VSM");
            tmpHeader = Encoding.ASCII.GetBytes("6VSM");
            version = 2;
            encKey = null;

            // Create a minimal font object for Save functionality
            font = new ClassesStructs.FontClass.ClassFont();
            font.NewFormat = true;
            font.NewTex = new TextureClass.NewT3Texture[0];
            font.glyph = new ClassesStructs.FontClass.ClassFont.GlyphInfo();
            font.glyph.Pages = 0;
            font.glyph.CharCount = 0;
            font.glyph.charsNew = new ClassesStructs.FontClass.ClassFont.TRectNew[0];
            font.glyph.BlockCoordSize = 12; // Minimal block size (4 + 4 + 4)
            font.One = 0x31; // 0x31 for new format
            font.FontName = "NewFont";
            font.BaseSize = 32;
            font.hasLineHeight = false;
            font.blockSize = true;
            font.headerSize = 0;
            font.texSize = 0;
            font.hasOneFloatValue = false;
            font.LastZero = 0;
            font.NewSomeValue = 0;
            font.TexCount = 0;
            font.feedFace = null;

            // Initialize default 6VSM elements for game runtime compatibility.
            InitializeDefault6VsmElements(font);

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
            scaleFontToolStripMenuItem.Enabled = false;
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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void FontEditor_Load(object sender, EventArgs e)
        {
            edited = false; //Tell a program about first launch window form so font is not modified.
            
            if(MainMenu.settings.swizzlePS4 || MainMenu.settings.swizzleNintendoSwitch || MainMenu.settings.swizzleXbox360 || MainMenu.settings.swizzlePSVita || MainMenu.settings.swizzleNintendoWii)
            {
                if (MainMenu.settings.swizzlePS4) rbPS4Swizzle.Checked = true;
                else if (MainMenu.settings.swizzlePSVita) rbPSVitaSwizzle.Checked = true;
                else if (MainMenu.settings.swizzleXbox360) rbXbox360Swizzle.Checked = true;
                else if (MainMenu.settings.swizzleNintendoWii) rbWiiSwizzle.Checked = true;
                else rbSwitchSwizzle.Checked = true;
            }
            else
            {
                rbNoSwizzle.Checked = true;
            }

            // Load font profiles
            FontProfileList profiles = FontProfileList.Load();
            RefreshProfileComboBox(profiles, null);
        }

        private void RefreshProfileComboBox(FontProfileList profiles, string selectName)
        {
            comboBoxProfiles.Items.Clear();
            foreach (var profile in profiles.Profiles)
                comboBoxProfiles.Items.Add(profile);

            if (selectName != null)
            {
                for (int i = 0; i < comboBoxProfiles.Items.Count; i++)
                {
                    if (((FontProfile)comboBoxProfiles.Items[i]).Name == selectName)
                    {
                        comboBoxProfiles.SelectedIndex = i;
                        return;
                    }
                }
            }
        }

        private void comboBoxProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxProfiles.SelectedItem == null) return;
            FontProfile profile = (FontProfile)comboBoxProfiles.SelectedItem;
            textBoxYoffset.Text = profile.YOffset.ToString();
            textBoxFontSizeAdjust.Text = profile.FontSizeAdjust.ToString();
            if (!string.IsNullOrEmpty(profile.FontFamilyName))
            {
                selectedFontFamilyName = profile.FontFamilyName;
                selectedFontFilePath = profile.FontFilePath ?? "";
                selectedFontStyle = (System.Drawing.FontStyle)(profile.FontStyleIndex);
                string[] styleNames = { "", " Bold", " Italic", " Bold Italic" };
                string styleSuffix = profile.FontStyleIndex > 0 ? styleNames[profile.FontStyleIndex] : "";
                textBoxGenFont.Text = string.IsNullOrEmpty(selectedFontFilePath)
                    ? selectedFontFamilyName + styleSuffix
                    : Path.GetFileName(selectedFontFilePath) + " (" + selectedFontFamilyName + styleSuffix + ")";
            }
        }

        private void buttonSaveProfile_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBoxYoffset.Text, out int yOffset)) return;
            if (!int.TryParse(textBoxFontSizeAdjust.Text, out int fontSizeAdjust)) return;

            using (Form inputForm = new Form())
            {
                inputForm.Text = "Save Profile";
                inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputForm.StartPosition = FormStartPosition.CenterParent;
                inputForm.ClientSize = new Size(250, 80);
                inputForm.MaximizeBox = false;
                inputForm.MinimizeBox = false;

                Label lbl = new Label() { Text = "Profile name:", Left = 10, Top = 10, Width = 80 };
                TextBox txt = new TextBox() { Left = 90, Top = 8, Width = 145 };
                Button okBtn = new Button() { Text = "OK", DialogResult = DialogResult.OK, Left = 80, Top = 42, Width = 75 };
                Button cancelBtn = new Button() { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 160, Top = 42, Width = 75 };

                inputForm.Controls.AddRange(new Control[] { lbl, txt, okBtn, cancelBtn });
                inputForm.AcceptButton = okBtn;
                inputForm.CancelButton = cancelBtn;

                if (inputForm.ShowDialog() != DialogResult.OK) return;
                string name = txt.Text.Trim();
                if (string.IsNullOrEmpty(name)) return;

                FontProfileList profileList = FontProfileList.Load();
                var existing = profileList.Profiles.FirstOrDefault(p => p.Name == name);
                if (existing != null)
                {
                    if (MessageBox.Show($"Profile \"{name}\" already exists. Overwrite?", "Overwrite Profile",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;
                    existing.YOffset = yOffset;
                    existing.FontSizeAdjust = fontSizeAdjust;
                    existing.FontFamilyName = selectedFontFamilyName;
                    existing.FontFilePath = selectedFontFilePath;
                    existing.FontStyleIndex = (int)selectedFontStyle;
                }
                else
                {
                    profileList.Profiles.Add(new FontProfile(name, yOffset, fontSizeAdjust, selectedFontFamilyName, selectedFontFilePath, (int)selectedFontStyle));
                }

                profileList.Save();
                RefreshProfileComboBox(profileList, name);
            }
        }

        private void buttonDeleteProfile_Click(object sender, EventArgs e)
        {
            if (comboBoxProfiles.SelectedIndex < 0) return;
            string name = ((FontProfile)comboBoxProfiles.SelectedItem).Name;

            if (MessageBox.Show($"Delete profile \"{name}\"?", "Delete Profile",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            FontProfileList profileList = FontProfileList.Load();
            profileList.Profiles.RemoveAll(p => p.Name == name);
            profileList.Save();
            RefreshProfileComboBox(profileList, null);
        }

        private void buttonPickFont_Click(object sender, EventArgs e)
        {
            using (FontPickerDialog pickForm = new FontPickerDialog())
            {
                if (pickForm.ShowDialog() != DialogResult.OK)
                    return;

                selectedFontFamilyName = pickForm.SelectedFontFamilyName;
                selectedFontFilePath = pickForm.SelectedFontFilePath;
                selectedFontStyle = pickForm.SelectedFontStyle;

                string[] styleNames = { "", " Bold", " Italic", " Bold Italic" };
                string info = selectedFontFamilyName;
                if (selectedFontStyle != FontStyle.Regular)
                    info += styleNames[(int)selectedFontStyle];
                if (!string.IsNullOrEmpty(selectedFontFilePath))
                    info = Path.GetFileName(selectedFontFilePath) + " (" + info + ")";
                textBoxGenFont.Text = info;
            }
        }

        public List<byte[]> head = new List<byte[]>();
        public ClassesStructs.FlagsClass fontFlags;
        FontClass.ClassFont font = null;

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

            if (tex.ObjectName == null)
            {
                tex.ObjectName = string.Empty;
            }

            if (tex.SubObjectName == null)
            {
                tex.SubObjectName = string.Empty;
            }

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
                    SetPixelFromBlock(dst, width, height, bx, by, px, py, palette[code], palette[code].A);
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

        private Bitmap BuildBitmapFromRgbaBuffer(byte[] rgbaPixels, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var data = bitmap.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(rgbaPixels, 0, data.Scan0, rgbaPixels.Length);
            bitmap.UnlockBits(data);
            return bitmap;
        }

        /// <summary>
        /// Load a texture page's DDS content as a Bitmap for editing (e.g. filling remaining slots).
        /// Returns null if decoding fails.
        /// </summary>
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

        /// <summary>
        /// Scans a texture page bitmap for the first empty cell starting from the given slot.
        /// An empty cell is one where all pixels are fully transparent (Alpha == 0).
        /// Returns the 0-based slot index, or -1 if all cells from startSlot onward are occupied.
        /// </summary>
        private int FindFirstEmptySlotFrom(Bitmap bitmap, int cellWidth, int cellHeight, int charsPerRow, int charsPerCol, int startSlot)
        {
            int totalCells = charsPerRow * charsPerCol;
            for (int slot = startSlot; slot < totalCells; slot++)
            {
                int row = slot / charsPerRow;
                int col = slot % charsPerRow;
                int startX = col * cellWidth;
                int startY = row * cellHeight;

                // Clamp to bitmap bounds
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
                                    textBoxLogOutput.AppendText($"[OpenFontDiag] Tex[{i}] platform={font.NewTex[i].platform.platform}, someValue={font.NewTex[i].SomeValue}, oneByte=0x{font.NewTex[i].OneByte:x2}, mip={font.NewTex[i].Mip}, mipCount={font.NewTex[i].Tex.MipCount}, someData={font.NewTex[i].Tex.SomeData}, texSize={font.NewTex[i].Tex.TexSize}, nextHeaderPoz=0x{poz:x}, nextTexDataPoz=0x{tmpPosition:x}\r\n");
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
                        scaleFontToolStripMenuItem.Enabled = font.NewFormat;
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
                // Write TexCount placeholder at offset 8 (matches original format)
                bw.Write(0); // offset 8: TexCount placeholder
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
                // Debug output for texture information before saving
                textBoxLogOutput.AppendText("\r\n=== SaveFont Debug Info ===\r\n");
                textBoxLogOutput.AppendText($"font.NewTex.Length: {font.NewTex.Length}\r\n");
                textBoxLogOutput.AppendText($"font.glyph.CharCount: {font.glyph.CharCount}\r\n");
                textBoxLogOutput.AppendText($"font.TexCount: {font.TexCount}\r\n");
                textBoxLogOutput.AppendText($"check_header: {Encoding.ASCII.GetString(check_header)}\r\n");
                textBoxLogOutput.AppendText("===================================\r\n");

                // Check TexNum bounds for first and last few characters
                for (int checkIdx = 0; checkIdx < Math.Min(10, font.glyph.CharCount); checkIdx++)
                {
                    int texNum = font.glyph.charsNew[checkIdx].TexNum;
                    if (texNum >= 0 && texNum < font.NewTex.Length)
                    {
                        textBoxLogOutput.AppendText($"Char {checkIdx}: TexNum={texNum}, Width={font.NewTex[texNum].Width}, Height={font.NewTex[texNum].Height}\r\n");
                    }
                    else
                    {
                        textBoxLogOutput.AppendText($"ERROR: Char {checkIdx}: TexNum={texNum} OUT OF BOUNDS (0-{font.NewTex.Length - 1})\r\n");
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

                        // LastZero belongs to the gap after texture headers and before pixel data.
                        if (font.NewTex[0].SomeValue > 4)
                        {
                            bw.Write(font.LastZero);
                        }

                        // Record position after all texture headers (before pixel data)
                        long posAfterHeaders = bw.BaseStream.Position;

                        // Calculate BlockTexSize: size from BlockTexSize field to end of texture headers
                        // This is: posAfterHeaders - posBeforeBlock
                        int blockTexSize = (int)(posAfterHeaders - posBeforeBlock);

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

                        // Calculate headerSize using formula from docs
                        // Formula: tex_data_start = headerSize + 16 + countElements*12 + 4
                        // Where tex_data_start is the position where texture pixel data starts
                        // We know: tex_data_start = posBeforeBlock + blockTexSize
                        // Therefore: headerSize = tex_data_start - 16 - countElements*12 - 4
                        long pixelDataStart = posBeforeBlock + blockTexSize;
                        int countElements = (font.binElements != null) ? font.binElements.Length : 0;
                        font.headerSize = (int)(pixelDataStart - 16 - countElements * 12 - 4);
                    }

                    bw.BaseStream.Seek(4, SeekOrigin.Begin);
                    bw.Write(font.headerSize);
                    bw.BaseStream.Seek(8, SeekOrigin.Begin);
                    bw.Write(font.TexCount);
                    bw.BaseStream.Seek(12, SeekOrigin.Begin);
                    bw.Write(font.texSize);
                }
                
            }

            bw.Close();
            fs.Close();
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            label1.Text = "(" + textBox8.Text.Length.ToString() + ")";
        }
        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            label2.Text = "(" + textBox9.Text.Length.ToString() + ")";
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBox8.Text = "";
            textBox9.Text = "";
            label1.Text = "(0)";
            label2.Text = "(0)";
            checkBox1.Checked = true;
            checkBox2.Checked = true;

        }

        private void buttonCopyCoordinates_Click(object sender, EventArgs e)
        {
            string ch1 = textBox8.Text;
            string ch2 = textBox9.Text;
            if (ch1.Length == ch2.Length)
            {
                for (int i = 0; i < ch1.Length; i++)
                {
                    int f = Convert.ToInt32(ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N).GetBytes(ch1[i].ToString())[0]);
                    int s = Convert.ToInt32(ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N).GetBytes(ch2[i].ToString())[0]);
                    int first = 0;
                    int second = 0;
                    for (int j = 0; j < dataGridViewWithCoord.RowCount; j++)
                    {
                        if (Convert.ToInt32(dataGridViewWithCoord[0, j].Value) == f)
                        {
                            first = j;
                        }
                        if (Convert.ToInt32(dataGridViewWithCoord[0, j].Value) == s)
                        {
                            second = j;
                        }
                    }


                    CopyDataIndataGridViewWithCoord(6, first, second);
                    CopyDataIndataGridViewWithCoord(7, first, second);
                    CopyDataIndataGridViewWithCoord(8, first, second);
                    CopyDataIndataGridViewWithCoord(9, first, second);
                    CopyDataIndataGridViewWithCoord(10, first, second);
                    CopyDataIndataGridViewWithCoord(11, first, second);
                    CopyDataIndataGridViewWithCoord(12, first, second);

                    if (checkBox1.Checked == true)
                    {
                        CopyDataIndataGridViewWithCoord(2, first, second);
                        CopyDataIndataGridViewWithCoord(3, first, second);
                    }
                    if (checkBox2.Checked == true)
                    {
                        CopyDataIndataGridViewWithCoord(4, first, second);
                        CopyDataIndataGridViewWithCoord(5, first, second);
                    }
                }
            }
            else if (ch1.Length == 1)
            {
                for (int i = 0; i < ch2.Length; i++)
                {
                    int f = Convert.ToInt32(ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N).GetBytes(ch1[i].ToString())[0]);
                    int s = Convert.ToInt32(ASCIIEncoding.GetEncoding(MainMenu.settings.ASCII_N).GetBytes(ch2[i].ToString())[0]);
                    int first = 0;
                    int second = 0;
                    for (int j = 0; j < dataGridViewWithCoord.RowCount; j++)
                    {
                        if (Convert.ToInt32(dataGridViewWithCoord[0, j].Value) == f)
                        {
                            first = j;
                        }
                        if (Convert.ToInt32(dataGridViewWithCoord[0, j].Value) == s)
                        {
                            second = j;
                        }
                    }

                    CopyDataIndataGridViewWithCoord(6, first, second);
                    CopyDataIndataGridViewWithCoord(7, first, second);
                    CopyDataIndataGridViewWithCoord(8, first, second);
                    CopyDataIndataGridViewWithCoord(9, first, second);
                    CopyDataIndataGridViewWithCoord(10, first, second);
                    CopyDataIndataGridViewWithCoord(11, first, second);
                    CopyDataIndataGridViewWithCoord(12, first, second);

                    if (checkBox1.Checked == true)
                    {
                        CopyDataIndataGridViewWithCoord(2, first, second);
                        CopyDataIndataGridViewWithCoord(3, first, second);
                    }
                    if (checkBox2.Checked == true)
                    {
                        CopyDataIndataGridViewWithCoord(4, first, second);
                        CopyDataIndataGridViewWithCoord(5, first, second);
                    }
                }
            }
        }

        private void CopyDataIndataGridViewWithCoord(int column, int first, int second)
        {
            // Check if cells exist before accessing them
            if (dataGridViewWithCoord.RowCount > first && dataGridViewWithCoord.RowCount > second &&
                dataGridViewWithCoord.ColumnCount > column)
            {
                var sourceCell = dataGridViewWithCoord[column, first];
                var destCell = dataGridViewWithCoord[column, second];

                if (sourceCell != null && destCell != null)
                {
                    destCell.Value = sourceCell.Value;
                    destCell.Style.BackColor = System.Drawing.Color.Green;
                }
            }
        }

        private void contextMenuStripExport_Import_Opening(object sender, CancelEventArgs e)
        {
            if (dataGridViewWithTextures.Rows.Count > 0)
            {
                if (dataGridViewWithTextures.SelectedCells[0].RowIndex >= 0)
                {
                    exportToolStripMenuItem.Enabled = true;
                    importDDSToolStripMenuItem.Enabled = true;
                }
                else
                {
                    exportToolStripMenuItem.Enabled = false;
                    importDDSToolStripMenuItem.Enabled = false;
                    exportCoordinatesToolStripMenuItem1.Enabled = false;
                    toolStripImportFNT.Enabled = false;
                }
            }
        }

        private void dataGridViewWithTextures_RowContextMenuStripNeeded(object sender, DataGridViewRowContextMenuStripNeededEventArgs e)
        {
            dataGridViewWithTextures.Rows[e.RowIndex].Selected = true;
            MessageBox.Show(dataGridViewWithTextures.Rows[e.RowIndex].Selected.ToString());
        }

        private void dataGridViewWithTextures_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridViewWithTextures.Rows[e.RowIndex].Selected = true;
            }
            if (e.Button == MouseButtons.Left && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // получаем координаты
                Point pntCell = dataGridViewWithTextures.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true).Location;
                pntCell.X += e.Location.X;
                pntCell.Y += e.Location.Y;

                // вызываем менюшку
                contextMenuStripExport_Import.Show(dataGridViewWithTextures, pntCell);
            }

            UpdateTexturePreview();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int file_n = dataGridViewWithTextures.SelectedCells[0].RowIndex;
            SaveFileDialog saveFD = new SaveFileDialog();
            if ((font.tex != null && font.tex[file_n].isIOS) || (font.NewTex != null && font.NewTex[file_n].isPVR))
            {
                saveFD.Filter = "PVR files (*.pvr)|*.pvr";
                saveFD.FileName = font.FontName + "_" + file_n.ToString() + ".pvr";
            }
            else
            {
                saveFD.Filter = "dds files (*.dds)|*.dds";
                saveFD.FileName = font.FontName + "_" + file_n.ToString() + ".dds";
            }

            if (saveFD.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(saveFD.FileName, FileMode.Create);
                Methods.DeleteCurrentFile(saveFD.FileName);

                switch (font.NewFormat)
                {
                    case true:
                        fs.Write(font.NewTex[file_n].Tex.Content, 0, font.NewTex[file_n].Tex.Content.Length);
                        break;

                    default:
                        fs.Write(font.tex[file_n].Content, 0, font.tex[file_n].Content.Length);
                        break;
                }

                fs.Close();
            }
        }

        private void importDDSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int file_n = dataGridViewWithTextures.SelectedCells[0].RowIndex;
            OpenFileDialog openFD = new OpenFileDialog();

            openFD.Filter = "dds files (*.dds)|*.dds";


            if (openFD.ShowDialog() == DialogResult.OK)
            {
                if (font.NewFormat) ReplaceTexture(openFD.FileName, font.NewTex[file_n]);
                else ReplaceTexture(openFD.FileName, font.tex[file_n]);

                fillTableofTextures(font);
                edited = true; //Отмечаем, что шрифт изменился
                UpdateTexturePreview();
            }
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

        private void FontEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (edited == true)
            {
                DialogResult status = MessageBox.Show("Save font before closing Font Editor?", "Exit", MessageBoxButtons.YesNoCancel);
                if (status == DialogResult.Cancel)
                // если (состояние == DialogResult.Отмена)
                {
                    e.Cancel = true; // Отмена = истина
                }
                else if (status == DialogResult.Yes) //Если (состояние == DialogResult.Да)
                {
                    // If no font file is currently open, use Save As instead
                    if (string.IsNullOrEmpty(ofd.FileName))
                    {
                        MessageBox.Show("No font file is currently open. Please use 'Save As...' to save the font.", "Save Required",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        saveAsToolStripMenuItem_Click(sender, e);
                        e.Cancel = true; // Don't close until user saves
                        return;
                    }

                    FileStream fs = new FileStream(ofd.SafeFileName, FileMode.Create); //Сохраняем в открытый файл.
                    SaveFont(fs, font);
                    //После соханения чистим списки
                }
                else //А иначе просто закрываем программу и чистим списки
                {
                }
            }

            if (basePreviewBitmap != null)
            {
                basePreviewBitmap.Dispose();
                basePreviewBitmap = null;
            }
        }

        private void dataGridViewWithCoord_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int end_edit_column = e.ColumnIndex;
            int end_edit_row = e.RowIndex;
            bool success = false;
            if (old_data != "")
            {
                if ((end_edit_column >= 2 && end_edit_column <= dataGridViewWithCoord.ColumnCount) && Methods.IsNumeric(dataGridViewWithCoord[end_edit_column, end_edit_row].Value.ToString()))
                {
                    if (dataGridViewWithCoord[end_edit_column, end_edit_row].Value.ToString() != old_data)
                    {
                        if (end_edit_column == 2 || end_edit_column == 3) //X
                        {
                            dataGridViewWithCoord[7, end_edit_row].Value = (Convert.ToInt32(dataGridViewWithCoord[3, end_edit_row].Value) - Convert.ToInt32(dataGridViewWithCoord[2, end_edit_row].Value));
                            success = true;

                        }
                        else if (end_edit_column == 4 || end_edit_column == 5) //Y
                        {
                            dataGridViewWithCoord[8, end_edit_row].Value = (Convert.ToInt32(dataGridViewWithCoord[5, end_edit_row].Value) - Convert.ToInt32(dataGridViewWithCoord[4, end_edit_row].Value));
                            success = true;
                        }
                        else if (end_edit_column == 6) //dds
                        {
                            success = true;
                            if (Convert.ToInt32(dataGridViewWithCoord[end_edit_column, end_edit_row].Value) >= dataGridViewWithTextures.RowCount)
                            {
                                dataGridViewWithCoord[end_edit_column, end_edit_row].Value = old_data;
                                success = false;
                            }
                        }
                        else if (end_edit_column > 6 && end_edit_column < 8)
                        {
                            dataGridViewWithCoord[end_edit_column, end_edit_row].Value = old_data;
                        }
                    }
                }
                else
                {
                    dataGridViewWithCoord[end_edit_column, end_edit_row].Value = old_data;
                }
            }
            if(success)
            {
                dataGridViewWithCoord[end_edit_column,end_edit_row].Style.BackColor = Color.DarkCyan;
                if (!font.NewFormat) {
                    float.TryParse(dataGridViewWithCoord[2, end_edit_row].Value.ToString(), out font.glyph.chars[end_edit_row].XStart);
                    float.TryParse(dataGridViewWithCoord[3, end_edit_row].Value.ToString(), out font.glyph.chars[end_edit_row].XEnd);
                    float.TryParse(dataGridViewWithCoord[4, end_edit_row].Value.ToString(), out font.glyph.chars[end_edit_row].YStart);
                    float.TryParse(dataGridViewWithCoord[5, end_edit_row].Value.ToString(), out font.glyph.chars[end_edit_row].YEnd);
                    int.TryParse(dataGridViewWithCoord[6, end_edit_row].Value.ToString(), out font.glyph.chars[end_edit_row].TexNum);

                    if (font.hasScaleValue)
                    {
                        float.TryParse(dataGridViewWithCoord[7, end_edit_row].Value.ToString(), out font.glyph.chars[end_edit_row].CharWidth);
                        float.TryParse(dataGridViewWithCoord[8, end_edit_row].Value.ToString(), out font.glyph.chars[end_edit_row].CharHeight);
                    }
                }
                else
                {
                   
                    float.TryParse(dataGridViewWithCoord[4, end_edit_row].Value.ToString(), out font.glyph.charsNew[end_edit_row].YStart);
                    float.TryParse(dataGridViewWithCoord[5, end_edit_row].Value.ToString(), out font.glyph.charsNew[end_edit_row].YEnd);
                    int.TryParse(dataGridViewWithCoord[6, end_edit_row].Value.ToString(), out font.glyph.charsNew[end_edit_row].TexNum);
                    float.TryParse(dataGridViewWithCoord[7, end_edit_row].Value.ToString(), out font.glyph.charsNew[end_edit_row].CharWidth);
                    float.TryParse(dataGridViewWithCoord[8, end_edit_row].Value.ToString(), out font.glyph.charsNew[end_edit_row].CharHeight);
                    float.TryParse(dataGridViewWithCoord[9, end_edit_row].Value.ToString(), out font.glyph.charsNew[end_edit_row].XOffset);
                    float.TryParse(dataGridViewWithCoord[10, end_edit_row].Value.ToString(), out font.glyph.charsNew[end_edit_row].YOffset);
                    float.TryParse(dataGridViewWithCoord[11, end_edit_row].Value.ToString(), out font.glyph.charsNew[end_edit_row].XAdvance);
                    int.TryParse(dataGridViewWithCoord[12, end_edit_row].Value.ToString(), out font.glyph.charsNew[end_edit_row].Channel);
                }
            }
            if (!edited && success)
            {
                edited = success;
            }

            UpdateTexturePreview();
        }
        public static string old_data;

        private void dataGridViewWithCoord_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            int now_edit_column = e.ColumnIndex;
            int now_edit_row = e.RowIndex;
            old_data = dataGridViewWithCoord[now_edit_column, now_edit_row].Value.ToString();
        }

        private void dataGridViewWithCoord_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridViewWithCoord.Rows[e.RowIndex].Selected = true;
            }
            if (e.Button == MouseButtons.Right && e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // получаем координаты
                Point pntCell = dataGridViewWithCoord.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true).Location;
                pntCell.X += e.Location.X;
                pntCell.Y += e.Location.Y;

                // вызываем менюшку
                contextMenuStripExp_imp_Coord.Show(dataGridViewWithCoord, pntCell);
            }

            UpdateTexturePreview();
        }

        private void dataGridViewWithTextures_SelectionChanged(object sender, EventArgs e)
        {
            UpdateTexturePreview();
        }

        private void dataGridViewWithCoord_SelectionChanged(object sender, EventArgs e)
        {
            // Auto-select the corresponding texture page when a character is selected
            if (dataGridViewWithCoord.SelectedCells.Count > 0)
            {
                int rowIndex = dataGridViewWithCoord.SelectedCells[0].RowIndex;
                if (rowIndex >= 0 && rowIndex < dataGridViewWithCoord.RowCount)
                {
                    float xStart, xEnd, yStart, yEnd;
                    int texNum;
                    if (TryGetGlyphRectFromRow(rowIndex, out xStart, out xEnd, out yStart, out yEnd, out texNum))
                    {
                        if (texNum >= 0 && texNum < dataGridViewWithTextures.RowCount)
                        {
                            dataGridViewWithTextures.ClearSelection();
                            dataGridViewWithTextures.Rows[texNum].Selected = true;
                        }
                    }
                }
            }

            UpdateTexturePreview();
        }

        private void exportCoordinatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportCoordinatesToolStripMenuItem1_Click(sender, e);
        }

        private void buttonPreviewChar_Click(object sender, EventArgs e)
        {
            if (font == null)
            {
                MessageBox.Show("Please open a font file first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string searchChar = textBoxSearchChar.Text.Trim();
            if (string.IsNullOrEmpty(searchChar))
            {
                MessageBox.Show("Please enter a character to search.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Get the encoding of the search character
            byte[] charBytes = Encoding.GetEncoding(MainMenu.settings.ASCII_N).GetBytes(searchChar);
            uint charCode = 0;

            if (font.NewFormat)
            {
                // New format: Use Unicode or ASCII
                if (MainMenu.settings.unicodeSettings == 0)
                {
                    charBytes = Encoding.Unicode.GetBytes(searchChar);
                    if (charBytes.Length >= 2)
                    {
                        charCode = BitConverter.ToUInt16(charBytes, 0);
                    }
                }
                else
                {
                    if (charBytes.Length > 0)
                    {
                        charCode = charBytes[0];
                    }
                }
            }
            else
            {
                // Old format: Use ASCII encoding
                if (charBytes.Length > 0)
                {
                    charCode = charBytes[0];
                }
            }

            textBoxLogOutput.AppendText($"\r\n=== Preview: '{searchChar}' ===\r\n");
            textBoxLogOutput.AppendText($"  CharCode: {charCode} (0x{charCode:X4})\r\n");
            textBoxLogOutput.AppendText($"  Grid rows: {dataGridViewWithCoord.RowCount}\r\n");

            // Search for this character in font data
            int foundRow = -1;
            int foundTexNum = -1;
            int matchCount = 0;

            for (int i = 0; i < dataGridViewWithCoord.RowCount; i++)
            {
                uint rowCharId;
                if (uint.TryParse(Convert.ToString(dataGridViewWithCoord[0, i].Value), out rowCharId))
                {
                    if (rowCharId == charCode)
                    {
                        matchCount++;
                        if (foundRow < 0)
                        {
                            foundRow = i;
                            int.TryParse(Convert.ToString(dataGridViewWithCoord[6, i].Value), out foundTexNum);
                        }
                    }
                }
            }

            if (matchCount > 1)
            {
                textBoxLogOutput.AppendText($"  WARNING: Character found {matchCount} times (duplicates)\r\n");
            }

            if (foundRow >= 0 && foundTexNum >= 0)
            {
                // Select the found row
                dataGridViewWithCoord.ClearSelection();
                dataGridViewWithCoord.Rows[foundRow].Selected = true;
                dataGridViewWithCoord.FirstDisplayedScrollingRowIndex = foundRow;

                // Select the corresponding texture
                if (foundTexNum < dataGridViewWithTextures.RowCount)
                {
                    dataGridViewWithTextures.ClearSelection();
                    dataGridViewWithTextures.Rows[foundTexNum].Selected = true;
                }

                // Update preview
                UpdateTexturePreview();

                textBoxLogOutput.AppendText($"  Found at row {foundRow + 1}, texture page {foundTexNum}\r\n");

                // Log character details
                if (font.NewFormat && foundRow < font.glyph.charsNew.Length)
                {
                    var ch = font.glyph.charsNew[foundRow];
                    textBoxLogOutput.AppendText($"  XStart={ch.XStart} YStart={ch.YStart} Width={ch.CharWidth} Height={ch.CharHeight}\r\n");
                    textBoxLogOutput.AppendText($"  XOffset={ch.XOffset} YOffset={ch.YOffset} XAdvance={ch.XAdvance}\r\n");
                }

                MessageBox.Show($"Found character '{searchChar}' at row {foundRow + 1} in texture {foundTexNum}.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                textBoxLogOutput.AppendText($"  NOT FOUND in {dataGridViewWithCoord.RowCount} rows\r\n");
                MessageBox.Show($"Character '{searchChar}' (code: {charCode}) not found in this font.",
                    "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void textBoxSearchChar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonPreviewChar_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void exportCoordinatesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "FNT file (*.fnt) | *.fnt";
            sfd.FileName = font.FontName + ".fnt";

            if(sfd.ShowDialog() == DialogResult.OK)
            {
                string info = "info face=\"" + font.FontName + "\" size=" + font.BaseSize + " bold=0 italic=0 charset=\"\" unicode=";
                switch (font.NewFormat)
                {
                    case true:
                        info += "1\r\n";
                        break;

                    default:
                        info += "0\r\n";
                        break;
                }

                info += "common lineHeight=" + font.BaseSize;

                if ((font.One == 0x31 && (Encoding.ASCII.GetString(check_header) == "5VSM"))
                        || (Encoding.ASCII.GetString(check_header) == "6VSM"))
                {
                    info += " base=" + font.NewSomeValue;
                }
                else info += " base=" + font.BaseSize;
                
                info += " pages=" + font.TexCount + "\r\n";

                if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
                FileStream fs = new FileStream(sfd.FileName, FileMode.CreateNew);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(info);
                info = "";

                for(int i = 0; i < font.TexCount; i++)
                {
                    info = "page id=" + i + " file=\"" + font.FontName + "_" + i + ".dds\"\r\n";
                    sw.Write(info);
                }

                info = "chars count=" + font.glyph.CharCount + "\r\n";
                sw.Write(info);

                if (!font.NewFormat)
                {
                    for(int i = 0; i < font.glyph.CharCount; i++)
                    {
                        info = "char id=" + i + " x=" + font.glyph.chars[i].XStart + " y=" + font.glyph.chars[i].YStart;
                        info += " width=";

                        if (font.hasScaleValue)
                        {
                            info += font.glyph.chars[i].CharWidth;
                        }
                        else
                        {
                            info += font.glyph.chars[i].XEnd - font.glyph.chars[i].XStart;
                        }

                        info += " height=";

                        if (font.hasScaleValue)
                        {
                            info += font.glyph.chars[i].CharHeight;
                        }
                        else
                        {
                            info += font.glyph.chars[i].YEnd - font.glyph.chars[i].YStart;
                        }

                        info += " xoffset=0 yoffset=0 xadvance=";

                        if (font.hasScaleValue)
                        {
                            info += font.glyph.chars[i].CharWidth;
                        }
                        else
                        {
                            info += font.glyph.chars[i].XEnd - font.glyph.chars[i].XStart;
                        }

                        info += " page=" + font.glyph.chars[i].TexNum + " chnl=15\r\n";

                        sw.Write(info);
                    }
                }
                else
                {
                    for (int i = 0; i < font.glyph.CharCount; i++)
                    {
                        info = "char id=" + font.glyph.charsNew[i].charId + " x=" + font.glyph.charsNew[i].XStart + " y=" + font.glyph.charsNew[i].YStart;
                        float xOffset = rbNoKerning.Checked ? 0 : font.glyph.charsNew[i].XOffset;
                        float yOffset = rbNoKerning.Checked ? 0 : font.glyph.charsNew[i].YOffset;
                        float xAdvance = rbNoKerning.Checked ? font.glyph.charsNew[i].CharWidth : font.glyph.charsNew[i].XAdvance;

                        info += " width=" + font.glyph.charsNew[i].CharWidth + " height=" + font.glyph.charsNew[i].CharHeight;
                        info += " xoffset=" + xOffset + " yoffset=" + yOffset + " xadvance=";
                        info += xAdvance + " page=" + font.glyph.charsNew[i].TexNum + " chnl=" + font.glyph.charsNew[i].Channel + "\r\n";

                        sw.Write(info);
                    }
                }

                sw.Close();
                fs.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Methods.IsNumeric(textBox1.Text))
            {
                int w = Convert.ToInt32(textBox1.Text);
                for (int i = 0; i < dataGridViewWithCoord.RowCount; i++)
                {
                    if (radioButtonXend.Checked)
                    {
                        dataGridViewWithCoord[3, i].Value = Convert.ToInt32(dataGridViewWithCoord[3, i].Value) + w;
                    }
                    else
                    {
                        dataGridViewWithCoord[2, i].Value = Convert.ToInt32(dataGridViewWithCoord[2, i].Value) + w;
                    }
                    dataGridViewWithCoord[7, i].Value = Convert.ToInt32(dataGridViewWithCoord[7, i].Value) + w;
                    dataGridViewWithCoord[12, i].Value = Convert.ToInt32(dataGridViewWithCoord[12, i].Value) + w;
                }
            }
        }

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
                        // Read font name
                        if (strings[m].ToLower().Contains("info face"))
                        {
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });
                            for (int k = 0; k < splitted.Length; k++)
                            {
                                if (splitted[k].ToLower() == "face")
                                {
                                    font.FontName = splitted[k + 1];
                                    break;
                                }
                            }
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
                                            // If font.NewTex has at least one texture, use it as template
                                            if (font.NewTex != null && font.NewTex.Length > 0)
                                            {
                                                tmpNewTex[j] = new TextureClass.NewT3Texture(font.NewTex[0]);
                                            }
                                            else
                                            {
                                                // Create a default texture template
                                                tmpNewTex[j] = new TextureClass.NewT3Texture();
                                                tmpNewTex[j].Tex = new TextureClass.NewT3Texture.TextureInfo();
                                                // Initialize required string fields to prevent null reference
                                                tmpNewTex[j].ObjectName = "";
                                                tmpNewTex[j].SubObjectName = "";
                                            }
                                        }

                                        textBoxLogOutput.AppendText($"Appending {fntPageCount} new textures to existing {existingTexCount} textures. Total: {totalTexCount}\r\n");
                                        break;
                                }
                            }
                        }

                            if(strings[m].Contains("page id"))
                            {
                                string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });
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
                                                // Adjust idNum for append mode (offset by existingTexCount)
                                                int adjustedIdNum = idNum + existingTexCount;
                                                ReplaceTexture(fi.DirectoryName + Path.DirectorySeparatorChar + fileName, tmpNewTex[adjustedIdNum]);
                                                textBoxLogOutput.AppendText($"  Loading FNT page {idNum} -> texture slot {adjustedIdNum}: {Path.GetFileName(fileName)}\r\n");
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
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });

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
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });

                            for (int k = 0; k < splitted.Length; k++)
                            {
                                if (splitted[k].ToLower() == "face")
                                {
                                    font.FontName = splitted[k + 1];
                                }
                                else if (splitted[k].ToLower() == "unicode" && splitted[k + 1] != "")
                                {
                                    isUnicodeFnt = Convert.ToInt32(splitted[k + 1]) == 1;
                                }
                            }
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
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });
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
                            string[] splitted = strings[m].Split(new char[] { ' ', '=', '\"', ',' });

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
                    scaleFontToolStripMenuItem.Enabled = true;

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

        private void rbNoSwizzle_CheckedChanged(object sender, EventArgs e)
        {
            MainMenu.settings.swizzleXbox360 = false;
            MainMenu.settings.swizzlePS4 = false;
            MainMenu.settings.swizzleNintendoSwitch = false;
            MainMenu.settings.swizzlePSVita = false;
            MainMenu.settings.swizzleNintendoWii = false;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void rbPS4Swizzle_CheckedChanged(object sender, EventArgs e)
        {
            MainMenu.settings.swizzleXbox360 = false;
            MainMenu.settings.swizzlePS4 = true;
            MainMenu.settings.swizzleNintendoSwitch = false;
            MainMenu.settings.swizzlePSVita = false;
            MainMenu.settings.swizzleNintendoWii = false;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void rbSwitchSwizzle_CheckedChanged(object sender, EventArgs e)
        {
            MainMenu.settings.swizzleXbox360 = false;
            MainMenu.settings.swizzlePS4 = false;
            MainMenu.settings.swizzleNintendoSwitch = true;
            MainMenu.settings.swizzlePSVita = false;
            MainMenu.settings.swizzleNintendoWii = false;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void rbXbox360Swizzle_CheckedChanged(object sender, EventArgs e)
        {
            if (rbXbox360Swizzle.Checked)
            {
                MainMenu.settings.swizzleXbox360 = true;
                MainMenu.settings.swizzlePS4 = false;
                MainMenu.settings.swizzleNintendoSwitch = false;
                MainMenu.settings.swizzlePSVita = false;
                MainMenu.settings.swizzleNintendoWii = false;
                Settings.SaveConfig(MainMenu.settings);
            }
        }

        private void rbPSVitaSwizzle_CheckedChanged(object sender, EventArgs e)
        {
            if (rbPSVitaSwizzle.Checked)
            {
                MainMenu.settings.swizzlePSVita = true;
                MainMenu.settings.swizzlePS4 = false;
                MainMenu.settings.swizzleNintendoSwitch = false;
                MainMenu.settings.swizzleXbox360 = false;
                MainMenu.settings.swizzleNintendoWii = false;
                Settings.SaveConfig(MainMenu.settings);
            }
        }


        private void rbWiiSwizzle_CheckedChanged(object sender, EventArgs e)
        {
            if (rbWiiSwizzle.Checked)
            {
                MainMenu.settings.swizzlePSVita = false;
                MainMenu.settings.swizzlePS4 = false;
                MainMenu.settings.swizzleNintendoSwitch = false;
                MainMenu.settings.swizzleXbox360 = false;
                MainMenu.settings.swizzleNintendoWii = true;
                Settings.SaveConfig(MainMenu.settings);
            }
        }

        private void convertArgb8888CB_CheckedChanged(object sender, EventArgs e)
        {
        }

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

        private void buttonSaveLogAs_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxLogOutput.Text))
            {
                MessageBox.Show("No log content to save.", "No Content",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|Log files (*.log)|*.log|All files (*.*)|*.*";
                saveFileDialog.Title = "Save Detection Log As";
                saveFileDialog.FileName = $"missing_textures_detection_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(saveFileDialog.FileName, textBoxLogOutput.Text, Encoding.UTF8);
                        MessageBox.Show($"Log saved successfully to:\n{saveFileDialog.FileName}", "Save Successful",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to save log:\n{ex.Message}", "Save Failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

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

        private void scaleFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            buttonScaleFont_Click(sender, e);
        }

        private void buttonScaleFont_Click(object sender, EventArgs e)
        {
            if (font == null)
            {
                MessageBox.Show("Please open a font file first.", "No Font Loaded",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!font.NewFormat)
            {
                MessageBox.Show("Scale Font currently only supports NewFormat (5VSM/6VSM) fonts.",
                    "Unsupported Format", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (font.glyph.charsNew == null || font.glyph.charsNew.Length == 0)
            {
                MessageBox.Show("This font has no characters to scale.", "Empty Font",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(selectedFontFamilyName))
            {
                MessageBox.Show("Please select a font using the 'Choose' button in Match Textures first.\n" +
                    "The selected font will be used to re-render all characters at the scaled size.",
                    "No Font Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Parse scale factor
            float scaleFactor;
            if (comboBoxScaleFactor.SelectedItem?.ToString() == "Custom")
            {
                // Simple input dialog for custom scale
                using (Form inputForm = new Form())
                {
                    inputForm.Text = "Custom Scale Factor";
                    inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                    inputForm.StartPosition = FormStartPosition.CenterParent;
                    inputForm.ClientSize = new Size(250, 100);
                    inputForm.MaximizeBox = false;
                    inputForm.MinimizeBox = false;

                    var label = new Label() { Text = "Enter scale factor (0.01 - 10.0):", Left = 10, Top = 15, Width = 220 };
                    var textBox = new TextBox() { Text = "1.5", Left = 10, Top = 40, Width = 220 };
                    var btnOk = new Button() { Text = "OK", DialogResult = DialogResult.OK, Left = 60, Top = 65, Width = 75 };
                    var btnCancel = new Button() { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 145, Top = 65, Width = 75 };

                    inputForm.Controls.AddRange(new Control[] { label, textBox, btnOk, btnCancel });
                    inputForm.AcceptButton = btnOk;
                    inputForm.CancelButton = btnCancel;

                    if (inputForm.ShowDialog() != DialogResult.OK) return;
                    if (!float.TryParse(textBox.Text, out scaleFactor) || scaleFactor <= 0 || scaleFactor > 10)
                    {
                        MessageBox.Show("Please enter a valid scale factor (0.01 - 10.0).", "Invalid Scale",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }
            else
            {
                if (!float.TryParse(comboBoxScaleFactor.SelectedItem?.ToString(), out scaleFactor))
                {
                    scaleFactor = 1.5f;
                }
            }

            // Parse texture size
            int textureSize;
            if (!int.TryParse(comboBoxTextureSize.SelectedItem?.ToString(), out textureSize))
            {
                textureSize = 2048;
            }

            // Confirm with user
            DialogResult result = MessageBox.Show(
                $"Scale all {font.glyph.CharCount} characters by {scaleFactor}x?\r\n\r\n" +
                $"Texture size: {textureSize}x{textureSize}\r\n" +
                $"This will replace ALL existing textures and character data.\r\n\r\n" +
                $"A Save As dialog will follow.",
                "Confirm Scale Font", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            // Prompt for save location
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Font Files (*.font)|*.font|All Files (*.*)|*.*";
                saveDialog.Title = "Save Scaled Font As";
                if (!string.IsNullOrEmpty(ofd.FileName))
                    saveDialog.FileName = Path.GetFileNameWithoutExtension(ofd.FileName) + $"_x{scaleFactor}.font";
                else
                    saveDialog.FileName = "scaled.font";

                if (saveDialog.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    ScaleFont(saveDialog.FileName, scaleFactor, textureSize);
                }
                catch (Exception ex)
                {
                    ShowErrorDialog("Scale Failed", $"Failed to scale font:\n\n{ex.ToString()}");
                }
            }
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

        private void ScaleFont(string savePath, float scaleFactor, int textureSize)
        {
            textBoxLogOutput.AppendText($"\r\n=== Scaling Font by {scaleFactor}x ===\r\n");
            textBoxLogOutput.AppendText($"Font family: {selectedFontFamilyName}\r\n");
            textBoxLogOutput.AppendText($"Total characters: {font.glyph.CharCount}\r\n");
            textBoxLogOutput.AppendText($"Texture size: {textureSize}x{textureSize}\r\n");
            textBoxLogOutput.AppendText($"Target: {savePath}\r\n");
            textBoxLogOutput.AppendText("===========================================\r\n\r\n");

            // --- Step 1: Save original metrics ---
            string originalFontName = font.FontName;
            float originalBaseSize = font.BaseSize;
            float originalLineHeight = font.lineHeight;
            float originalNewSomeValue = font.NewSomeValue;
            int originalCharCount = font.glyph.CharCount;

            // Deep copy original charsNew for reference
            var originalChars = new FontClass.ClassFont.TRectNew[font.glyph.charsNew.Length];
            Array.Copy(font.glyph.charsNew, originalChars, originalChars.Length);

            // --- Step 2: Analyze existing characters to get rendering params ---
            int charWidth = 28, charHeight = 27, fontSize = 27, xAdvance = 25;
            Dictionary<float, int> xoffsetStats = new Dictionary<float, int>();
            Dictionary<float, int> yoffsetStats = new Dictionary<float, int>();
            Dictionary<float, int> xadvanceStats = new Dictionary<float, int>();

            var sizeStats = new Dictionary<string, int>();
            int cjkCharCount = 0;

            foreach (var ch in font.glyph.charsNew)
            {
                if (ch.charId >= 0x4E00 && ch.charId <= 0x9FFF)
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

                    cjkCharCount++;
                }
            }

            if (sizeStats.Count > 0)
            {
                var mostCommonSize = sizeStats.OrderByDescending(x => x.Value).First();
                string[] dimensions = mostCommonSize.Key.Split('x');
                charWidth = int.Parse(dimensions[0]);
                charHeight = int.Parse(dimensions[1]);
                fontSize = charHeight;
                xAdvance = (int)xadvanceStats.OrderByDescending(x => x.Value).First().Key;
            }
            else
            {
                // No CJK: use average metrics from all non-zero characters
                var validChars = font.glyph.charsNew.Where(c => c.charId != 0 && c.CharWidth > 0).ToList();
                if (validChars.Count > 0)
                {
                    charWidth = (int)Math.Round(validChars.Average(c => c.CharWidth));
                    charHeight = (int)Math.Round(validChars.Average(c => c.CharHeight));
                    fontSize = charHeight;
                    xAdvance = (int)Math.Round(validChars.Average(c => c.XAdvance));
                }
            }

            float mostCommonXOffset = xoffsetStats.Count > 0
                ? xoffsetStats.OrderByDescending(x => x.Value).First().Key : 0;
            float mostCommonYOffset = yoffsetStats.Count > 0
                ? yoffsetStats.OrderByDescending(x => x.Value).First().Key : 0;

            // --- Step 3: Compute scaled metrics ---
            // Read Font Size Adjust from UI (same as GenerateMissingCharacters)
            int fontSizeAdjustment = 0;
            if (int.TryParse(textBoxFontSizeAdjust.Text, out int parsedSizeAdjust))
                fontSizeAdjustment = parsedSizeAdjust;

            int scaledCharWidth = Math.Max(1, (int)Math.Round(charWidth * scaleFactor));
            int scaledCharHeight = Math.Max(1, (int)Math.Round(charHeight * scaleFactor));
            float scaledFontSize = Math.Max(1f, fontSize * scaleFactor + fontSizeAdjustment);
            int scaledXAdvance = Math.Max(1, (int)Math.Round(xAdvance * scaleFactor));
            short scaledXOffset = (short)Math.Round(mostCommonXOffset * scaleFactor);
            short scaledYOffset = (short)Math.Round(mostCommonYOffset * scaleFactor);
            float scaledBaseSize = (float)Math.Round(originalBaseSize * scaleFactor);
            float scaledLineHeight = originalLineHeight > 0
                ? (float)Math.Round(originalLineHeight * scaleFactor) : scaledBaseSize;
            float scaledNewSomeValue = originalNewSomeValue > 0
                ? (float)Math.Round(originalNewSomeValue * scaleFactor) : 0;

            int padding = 0;
            int cellWidth = scaledCharWidth + padding * 2;
            int cellHeight = scaledCharHeight + padding * 2;

            textBoxLogOutput.AppendText($"Original metrics: {charWidth}x{charHeight}, fontSize={fontSize}, xAdvance={xAdvance}\r\n");
            textBoxLogOutput.AppendText($"Scaled metrics: {scaledCharWidth}x{scaledCharHeight}, fontSize={scaledFontSize:F1}, xAdvance={scaledXAdvance}\r\n");
            textBoxLogOutput.AppendText($"Font size adjust: {fontSizeAdjustment}, Y offset: {textBoxYoffset.Text}\r\n");
            textBoxLogOutput.AppendText($"Scaled BaseSize: {scaledBaseSize}, lineHeight: {scaledLineHeight}, NewSomeValue: {scaledNewSomeValue}\r\n\r\n");

            // --- Step 4: Validate texture layout ---
            if (cellWidth > textureSize || cellHeight > textureSize)
            {
                MessageBox.Show($"Scaled character size ({scaledCharWidth}x{scaledCharHeight}) exceeds texture size ({textureSize}x{textureSize}).\n" +
                    "Choose a larger texture size or smaller scale factor.",
                    "Invalid Configuration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int charsPerRow = Math.Max(1, textureSize / cellWidth);
            int charsPerCol = Math.Max(1, textureSize / cellHeight);
            int charsPerTexture = charsPerRow * charsPerCol;
            int numTexturesNeeded = (int)Math.Ceiling((double)originalCharCount / charsPerTexture);

            textBoxLogOutput.AppendText($"Layout: {charsPerRow}x{charsPerCol} per texture ({charsPerTexture} chars)\r\n");
            textBoxLogOutput.AppendText($"Textures needed: {numTexturesNeeded}\r\n\r\n");

            if (numTexturesNeeded > 50)
            {
                DialogResult bigResult = MessageBox.Show(
                    $"Scaling will create {numTexturesNeeded} texture pages. This may be slow and produce large files.\nContinue?",
                    "Many Pages Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (bigResult != DialogResult.Yes) return;
            }

            // --- Step 5: Load source font ---
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
                fontFamily = new System.Drawing.FontFamily(selectedFontFamilyName);
            }

            // --- Step 6: Re-render all characters ---
            int yOffsetAdjustment = 0;
            if (int.TryParse(textBoxYoffset.Text, out int parsedOffset))
                yOffsetAdjustment = parsedOffset;

            var newChars = new FontClass.ClassFont.TRectNew[originalCharCount];
            string saveDir = Path.GetDirectoryName(savePath);
            string saveBaseName = Path.GetFileNameWithoutExtension(savePath);

            for (int texIndex = 0; texIndex < numTexturesNeeded; texIndex++)
            {
                int startChar = texIndex * charsPerTexture;
                int endChar = Math.Min(startChar + charsPerTexture, originalCharCount);
                int charsInThisTexture = endChar - startChar;

                textBoxLogOutput.AppendText($"Rendering texture {texIndex + 1}/{numTexturesNeeded} (chars {startChar + 1}-{endChar})...\r\n");

                using (Bitmap textureBitmap = new Bitmap(textureSize, textureSize))
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(textureBitmap))
                {
                    g.Clear(Color.Transparent);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                    using (System.Drawing.Font drawFont = new System.Drawing.Font(
                        fontFamily, scaledFontSize, selectedFontStyle, GraphicsUnit.Pixel))
                    {
                        for (int i = 0; i < charsInThisTexture; i++)
                        {
                            int charArrayIndex = startChar + i;
                            var origChar = originalChars[charArrayIndex];

                            // Skip null/empty characters
                            if (origChar.charId == 0) continue;

                            char c = (char)origChar.charId;

                            int row = i / charsPerRow;
                            int col = i % charsPerRow;

                            int x = col * cellWidth + padding;
                            int y = row * cellHeight + padding;

                            // Scale per-character metrics proportionally
                            int perCharWidth = Math.Max(1, (int)Math.Round(origChar.CharWidth * scaleFactor));
                            int perCharHeight = Math.Max(1, (int)Math.Round(origChar.CharHeight * scaleFactor));
                            int perCharXAdvance = Math.Max(1, (int)Math.Round(origChar.XAdvance * scaleFactor));
                            short perCharXOffset = (short)Math.Round(origChar.XOffset * scaleFactor);
                            short perCharYOffset = (short)Math.Round(origChar.YOffset * scaleFactor);

                            // Draw character
                            g.DrawString(c.ToString(), drawFont, Brushes.White,
                                x, y - yOffsetAdjustment, StringFormat.GenericTypographic);

                            // Create new character entry with scaled metrics
                            newChars[charArrayIndex] = new FontClass.ClassFont.TRectNew
                            {
                                charId = origChar.charId,
                                XStart = (short)x,
                                XEnd = (short)(x + perCharWidth),
                                YStart = (short)y,
                                YEnd = (short)(y + perCharHeight),
                                CharWidth = perCharWidth,
                                CharHeight = perCharHeight,
                                XOffset = perCharXOffset,
                                YOffset = perCharYOffset,
                                XAdvance = perCharXAdvance,
                                Channel = origChar.Channel,
                                TexNum = texIndex
                            };
                        }
                    }

                    // Save texture as DDS
                    string texturePath = Path.Combine(saveDir, saveBaseName + $"_page{texIndex}.dds");
                    SaveBitmapAsDDS(textureBitmap, texturePath, texIndex);
                }

                textBoxLogOutput.AppendText($"  Saved: {saveBaseName}_page{texIndex}.dds\r\n");
            }

            // --- Step 7: Update font in-memory state ---
            font.glyph.charsNew = newChars;
            font.glyph.CharCount = originalCharCount;
            font.glyph.Pages = numTexturesNeeded;
            font.TexCount = numTexturesNeeded;
            font.BaseSize = scaledBaseSize;
            font.lineHeight = scaledLineHeight;
            font.NewSomeValue = scaledNewSomeValue;

            // Build new texture array - deep copy metadata from original first texture
            TextureClass.NewT3Texture[] newTexArray = new TextureClass.NewT3Texture[numTexturesNeeded];
            TextureClass.NewT3Texture templateTex = (font.NewTex != null && font.NewTex.Length > 0)
                ? font.NewTex[0] : null;

            for (int i = 0; i < numTexturesNeeded; i++)
            {
                if (templateTex != null)
                {
                    // Use copy constructor to preserve ALL metadata (block, subBlock, platform, etc.)
                    newTexArray[i] = new TextureClass.NewT3Texture(templateTex);
                }
                else
                {
                    newTexArray[i] = new TextureClass.NewT3Texture();
                    newTexArray[i].Tex = new TextureClass.NewT3Texture.TextureInfo();
                    newTexArray[i].ObjectName = "";
                    newTexArray[i].SubObjectName = "";
                    newTexArray[i].SomeValue = 0;
                    newTexArray[i].Zero = 0;
                    newTexArray[i].HasOneValueTex = false;
                }

                string ddsPath = Path.Combine(saveDir, saveBaseName + $"_page{i}.dds");
                if (File.Exists(ddsPath))
                {
                    ReplaceTexture(ddsPath, newTexArray[i]);
                }
            }
            font.NewTex = newTexArray;

            // Clean up font collection
            fontCollection?.Dispose();

            // Reset generation tracking state
            lastGeneratedPagesStartIndex = -1;
            lastGeneratedPagesCount = 0;
            lastGeneratedCharCount = 0;
            lastOriginalPagesCount = -1;
            lastModifiedExistingPageIndex = -1;
            lastModifiedPageOriginalData = null;

            // --- Step 8: Save FNT file (all characters) ---
            SaveScaledFontFNT(savePath, originalFontName, scaledBaseSize, scaledLineHeight, numTexturesNeeded);

            // --- Step 9: Save .font binary ---
            // Ensure critical fields are not null before saving
            if (string.IsNullOrEmpty(font.FontName))
                font.FontName = originalFontName ?? "ScaledFont";
            if (check_header == null)
                check_header = Encoding.ASCII.GetBytes("6VSM");
            if (tmpHeader == null)
                tmpHeader = Encoding.ASCII.GetBytes("6VSM");
            Methods.DeleteCurrentFile(savePath);
            using (FileStream fs = new FileStream(savePath, FileMode.Create))
            {
                SaveFont(fs, font);
            }
            encFunc(savePath);

            // --- Step 10: Refresh UI ---
            fillTableofCoordinates(font, true);
            fillTableofTextures(font);
            UpdateTexturePreview();
            edited = true;

            textBoxLogOutput.AppendText($"\r\n=== Scale Complete ===\r\n");
            textBoxLogOutput.AppendText($"Scaled {originalCharCount} characters by {scaleFactor}x\r\n");
            textBoxLogOutput.AppendText($"Texture pages: {numTexturesNeeded} ({textureSize}x{textureSize})\r\n");
            textBoxLogOutput.AppendText($"Saved to: {savePath}\r\n");
            textBoxLogOutput.AppendText($"FNT saved to: {Path.ChangeExtension(savePath, ".fnt")}\r\n");

            MessageBox.Show($"Font scaled by {scaleFactor}x successfully!\r\n\r\n" +
                $"Characters: {originalCharCount}\r\n" +
                $"Texture pages: {numTexturesNeeded} ({textureSize}x{textureSize})\r\n" +
                $"BaseSize: {originalBaseSize} -> {scaledBaseSize}\r\n\r\n" +
                $"Saved to:\r\n{savePath}",
                "Scale Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SaveScaledFontFNT(string savePath, string fontName,
            float scaledBaseSize, float scaledLineHeight, int totalPages)
        {
            string fntPath = Path.ChangeExtension(savePath, ".fnt");
            string baseName = Path.GetFileNameWithoutExtension(savePath);

            using (FileStream fs = new FileStream(fntPath, FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
            {
                sw.WriteLine($"info face=\"{fontName}\" size={(int)scaledBaseSize} bold=0 italic=0 charset=\"\" unicode=1");
                sw.WriteLine($"common lineHeight={(int)scaledLineHeight} base={(int)scaledBaseSize} pages={totalPages}");

                for (int i = 0; i < totalPages; i++)
                {
                    string pageFileName = $"{baseName}_page{i}.dds";
                    sw.WriteLine($"page id={i} file=\"{pageFileName}\"");
                }

                sw.WriteLine($"chars count={font.glyph.CharCount}");

                for (int i = 0; i < font.glyph.CharCount; i++)
                {
                    var ch = font.glyph.charsNew[i];
                    if (ch.charId != 0)
                    {
                        sw.WriteLine($"char id={ch.charId} x={ch.XStart} y={ch.YStart} " +
                            $"width={ch.CharWidth} height={ch.CharHeight} " +
                            $"xoffset={ch.XOffset} yoffset={ch.YOffset} " +
                            $"xadvance={ch.XAdvance} page={ch.TexNum} chnl={ch.Channel}");
                    }
                }
            }

            textBoxLogOutput.AppendText($"  FNT file: {Path.GetFileName(fntPath)}\r\n");
            textBoxLogOutput.AppendText($"  Characters in FNT: {font.glyph.CharCount}\r\n");
            textBoxLogOutput.AppendText($"  Pages in FNT: {totalPages}\r\n");
        }
    }
}
