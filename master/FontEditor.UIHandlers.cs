using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TTG_Tools
{
    public partial class FontEditor
    {
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FontEditor_Load(object sender, EventArgs e)
        {
            edited = false; //Tell a program about first launch window form so font is not modified.

            if (MainMenu.settings.swizzlePS4 || MainMenu.settings.swizzleNintendoSwitch || MainMenu.settings.swizzleXbox360 || MainMenu.settings.swizzlePSVita || MainMenu.settings.swizzleNintendoWii)
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

        private void FontEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (edited == true)
            {
                DialogResult status = MessageBox.Show("Save font before closing Font Editor?", "Exit", MessageBoxButtons.YesNoCancel);
                if (status == DialogResult.Cancel)
                // if (state == DialogResult.Cancel)
                {
                    e.Cancel = true; // Cancel = true
                }
                else if (status == DialogResult.Yes) // If (state == DialogResult.Yes)
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

                    FileStream fs = new FileStream(ofd.SafeFileName, FileMode.Create); // Save to the open file.
                    SaveFont(fs, font);
                    // After saving, clear the lists
                }
                else // Otherwise just close the program and clear the lists
                {
                }
            }

            if (basePreviewBitmap != null)
            {
                basePreviewBitmap.Dispose();
                basePreviewBitmap = null;
            }
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

        private void buttonApplyYoffsetAdjust_Click(object sender, EventArgs e)
        {
            if (font == null || font.glyph.charsNew == null)
            {
                MessageBox.Show("No font loaded.", "Error");
                return;
            }

            string input = textBoxYoffsetAdjust.Text.Trim();
            if (input.StartsWith("+")) input = input.Substring(1);

            if (!int.TryParse(input, out int adjustment))
            {
                MessageBox.Show("Invalid adjustment value. Enter a number (e.g., -5 or +3).", "Error");
                return;
            }

            for (int i = 0; i < font.glyph.CharCount; i++)
            {
                font.glyph.charsNew[i].YOffset += adjustment;
            }

            fillTableofCoordinates(font, false);
            edited = true;
            textBoxLogOutput.AppendText($"[YOffset] Applied adjustment: {adjustment} to all {font.glyph.CharCount} characters.\r\n");
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
                // get coordinates
                Point pntCell = dataGridViewWithTextures.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true).Location;
                pntCell.X += e.Location.X;
                pntCell.Y += e.Location.Y;

                // show context menu
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
                edited = true; // Mark that the font has been modified
                UpdateTexturePreview();
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
            if (success)
            {
                dataGridViewWithCoord[end_edit_column, end_edit_row].Style.BackColor = Color.DarkCyan;
                if (!font.NewFormat)
                {
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
                // get coordinates
                Point pntCell = dataGridViewWithCoord.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true).Location;
                pntCell.X += e.Location.X;
                pntCell.Y += e.Location.Y;

                // show context menu
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

            if (sfd.ShowDialog() == DialogResult.OK)
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

                for (int i = 0; i < font.TexCount; i++)
                {
                    info = "page id=" + i + " file=\"" + font.FontName + "_" + i + ".dds\"\r\n";
                    sw.Write(info);
                }

                info = "chars count=" + font.glyph.CharCount + "\r\n";
                sw.Write(info);

                if (!font.NewFormat)
                {
                    for (int i = 0; i < font.glyph.CharCount; i++)
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
                string prefix = isNewFontMode ? "BUILD" : "REPLACE";
                saveFileDialog.FileName = $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

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
    }
}
