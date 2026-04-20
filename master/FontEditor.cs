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
        bool isNewFontMode;
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

        public List<byte[]> head = new List<byte[]>();
        public ClassesStructs.FlagsClass fontFlags;
        FontClass.ClassFont font = null;
        // Saved when "Clear Existing FNT+DDS" is used, so Import DDS can restore platform metadata.
        private ClassesStructs.TextureClass.NewT3Texture savedTexTemplate = null;

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

    }
}
