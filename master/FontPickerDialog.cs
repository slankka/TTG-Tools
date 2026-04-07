using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TTG_Tools
{
    public partial class FontPickerDialog : Form
    {
        public string SelectedFontFamilyName { get; private set; } = "";
        public string SelectedFontFilePath { get; private set; } = "";
        public FontStyle SelectedFontStyle { get; private set; } = FontStyle.Regular;

        private string browsedPath = "";

        public FontPickerDialog()
        {
            InitializeComponent();
        }

        private void FontPickerDialog_Load(object sender, EventArgs e)
        {
            // Try to use a CJK-capable font for the font list so all names display correctly
            string[] cjkFonts = { "Microsoft YaHei UI", "Microsoft YaHei", "Yu Gothic UI",
                "Meiryo UI", "Malgun Gothic", "SimSun", "MS UI Gothic", "Arial Unicode MS" };
            foreach (var fontName in cjkFonts)
            {
                try
                {
                    var ff = new System.Drawing.FontFamily(fontName);
                    if (ff.IsStyleAvailable(FontStyle.Regular))
                    {
                        listBoxFonts.Font = new System.Drawing.Font(ff, 9F);
                        break;
                    }
                }
                catch { }
            }

            // Populate with installed font families
            var families = new System.Drawing.Text.InstalledFontCollection().Families;
            foreach (var family in families)
                listBoxFonts.Items.Add(family.Name);
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Font Files (*.ttf;*.otf;*.ttc)|*.ttf;*.otf;*.ttc|All Files (*.*)|*.*";
                ofd.Title = "Select Font File";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    browsedPath = ofd.FileName;
                    try
                    {
                        using (var pfc = new System.Drawing.Text.PrivateFontCollection())
                        {
                            pfc.AddFontFile(browsedPath);
                            listBoxFonts.Items.Clear();
                            listBoxFonts.Items.Add(pfc.Families[0].Name);
                            listBoxFonts.SelectedIndex = 0;
                            textBoxFilter.Text = pfc.Families[0].Name;
                            comboBoxStyle.Enabled = false;
                        }
                    }
                    catch { }
                }
            }
        }

        private void textBoxFilter_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(browsedPath))
            {
                string keyword = textBoxFilter.Text.ToLower();
                var families = new System.Drawing.Text.InstalledFontCollection().Families;
                listBoxFonts.Items.Clear();
                foreach (var family in families)
                {
                    if (family.Name.ToLower().Contains(keyword))
                        listBoxFonts.Items.Add(family.Name);
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (listBoxFonts.SelectedItem == null)
            {
                MessageBox.Show("Please select a font.", "No Font Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            SelectedFontFamilyName = listBoxFonts.SelectedItem.ToString();
            SelectedFontFilePath = browsedPath;

            if (!string.IsNullOrEmpty(browsedPath))
                SelectedFontStyle = FontStyle.Regular;
            else
                SelectedFontStyle = (FontStyle)comboBoxStyle.SelectedIndex;
        }
    }
}
