using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace TTG_Tools
{
    public partial class AutoPacker : Form
    {
        public AutoPacker()
        {
            InitializeComponent();
        }

        public static FileInfo[] fi;
        public static FileInfo[] fi_temp;

        public static int numKey;
        public static int selected_index;
        public static int EncVersion;

        Thread threadExport;
        Thread threadImport;
        private bool _suppressImportReplaceRuleEvents;
        private DataGridViewTextBoxEditingControl _activeRuleEditingControl;
        private int _editingRuleRowIndex = -1;
        private int _editingRuleColumnIndex = -1;
        private string _editingRuleTextDraft;

        public struct langdb
        {
            public byte[] head;
            public byte[] hz_data;
            public byte[] lenght_of_name;
            public string name;
            public byte[] lenght_of_text;
            public string text;
            public byte[] lenght_of_waw;
            public string waw;
            public byte[] lenght_of_animation;
            public string animation;
            public byte[] magic_bytes;
            public byte[] realID;
        }


        public static int number;
        langdb[] database = new langdb[5000];

        // MODIFICADO: Lógica do Pop-up adicionada aqui
        public void AddNewReport(string report)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new ReportHandler(AddNewReport), report);
            }
            else
            {
                // DETECTA A MENSAGEM ESPECIAL DE ERRO
                if (report.StartsWith("##POPUP##"))
                {
                    string realMessage = report.Substring(9); // Remove o prefixo
                    MessageBox.Show(realMessage, "Error Report", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    // Adiciona um aviso no log apenas para constar
                    listBox1.AppendText(">>> Error report displayed on screen. <<<" + Environment.NewLine);
                    listBox1.SelectionStart = listBox1.TextLength;
                    listBox1.ScrollToCaret();
                    return;
                }

                // Comportamento normal para mensagens que não são de erro crítico
                listBox1.AppendText(report + Environment.NewLine);
                listBox1.SelectionStart = listBox1.TextLength;
                listBox1.ScrollToCaret();
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (MainMenu.settings.clearMessages) listBox1.Clear();

            // Keep runtime setting in sync with UI checkbox state.
            MainMenu.settings.enableImportTextReplace = checkEnableImportTextReplace.Checked;

            SaveImportReplaceRulesFromGrid(false);

            if (checkEnableImportTextReplace.Checked && !Methods.HasEnabledImportReplaceRules())
            {
                MessageBox.Show("At least one enabled Replace rule must have non-empty Find text.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                DirectoryInfo di = new DirectoryInfo(MainMenu.settings.pathForInputFolder);
                fi = di.GetFiles();
            }
            catch
            {
                MessageBox.Show("Open and close program or fix path in config.xml!", "Error!");
                return;
            }

            /*if (checkUnicode.Checked) MainMenu.settings.unicodeSettings = 0;
            else MainMenu.settings.unicodeSettings = 1;*/

            EncVersion = comboBox2.SelectedIndex != 1 ? 2 : 7;

            string versionOfGame = MainMenu.gamelist[comboBox1.SelectedIndex].gamename;
            numKey = comboBox1.SelectedIndex;
            selected_index = comboBox2.SelectedIndex;
            byte[] encKey = MainMenu.settings.customKey ? Methods.stringToKey(MainMenu.settings.encCustomKey) : MainMenu.gamelist[numKey].key;

            //Create import files thread
            var processImport = new ForThreads();
            processImport.ReportForWork += AddNewReport;
            List<string> parametresImport = new List<string>();
            parametresImport.Add(versionOfGame);
            parametresImport.Add(".dds");
            parametresImport.Add(MainMenu.settings.pathForInputFolder);
            parametresImport.Add(MainMenu.settings.pathForOutputFolder);
            parametresImport.Add(MainMenu.settings.deleteD3DTXafterImport.ToString());
            parametresImport.Add(MainMenu.settings.deleteDDSafterImport.ToString());
            parametresImport.Add(Convert.ToString(EncVersion));
            parametresImport.Add(MainMenu.settings.encLangdb.ToString());
            parametresImport.Add(MainMenu.settings.encNewLua.ToString());
            parametresImport.Add(BitConverter.ToString(encKey).Replace("-", ""));

            threadImport = new Thread(new ParameterizedThreadStart(processImport.DoImportEncoding));
            threadImport.Start(parametresImport);
        }

        public static string GetNameOnly(int i)
        {
            return fi[i].Name.Substring(0, (fi[i].Name.Length - fi[i].Extension.Length));
        }

        private void buttonDecrypt_Click(object sender, EventArgs e)
        {
            if (MainMenu.settings.clearMessages) listBox1.Clear();

            string versionOfGame = MainMenu.gamelist[comboBox1.SelectedIndex].gamename;
            numKey = comboBox1.SelectedIndex;
            selected_index = comboBox2.SelectedIndex;

            byte[] encKey = MainMenu.settings.customKey ? Methods.stringToKey(MainMenu.settings.encCustomKey) : MainMenu.gamelist[comboBox1.SelectedIndex].key;

            string debug = null;

            int arc_version = comboBox2.SelectedIndex != 1 ? 2 : 7;

            Methods.DeleteCurrentFile("\\del.me");
            try
            {
                DirectoryInfo di = new DirectoryInfo(MainMenu.settings.pathForInputFolder);
                fi = di.GetFiles();
            }
            catch
            {
                MessageBox.Show("Open and close program or fix path in config.xml!", "Error!");
                return;
            }

            //Создаем нить для экспорта текста из LANGDB
            var processExport = new ForThreads();
            processExport.ReportForWork += AddNewReport;
            List<string> parametresExport = new List<string>();
            parametresExport.Add(MainMenu.settings.pathForInputFolder);
            parametresExport.Add(MainMenu.settings.pathForOutputFolder);
            parametresExport.Add(versionOfGame);
            parametresExport.Add(BitConverter.ToString(encKey).Replace("-", ""));
            parametresExport.Add(Convert.ToString(arc_version));

            threadExport = new Thread(new ParameterizedThreadStart(processExport.DoExportEncoding));
            threadExport.Start(parametresExport);

            if (debug != null)
            {
                StreamWriter sw = new StreamWriter(MainMenu.settings.pathForOutputFolder + "\\bugs.txt");
                sw.Write(debug);
                sw.Close();
                AddNewReport("Bugs have been written in file " + MainMenu.settings.pathForOutputFolder + "\\bugs.txt");
            }
        }

        public class Prop
        {
            public byte[] id;
            public byte[] lenght_of_text;
            public string text;

            public Prop() { }
            public Prop(byte[] id, byte[] lenght_of_text, string text)
            {
                this.id = id;
                this.lenght_of_text = lenght_of_text;
                this.text = text;
            }
        }

        private void AutoPacker_Load(object sender, EventArgs e)
        {

            #region Load blowfish key list

            comboBox1.Items.Clear();

            for (int i = 0; i < MainMenu.gamelist.Count; i++)
            {
                comboBox1.Items.Add(i + ". " + MainMenu.gamelist[i].gamename);
            }

            #endregion

            comboBox1.SelectedIndex = MainMenu.settings.encKeyIndex;
            comboBox2.SelectedIndex = MainMenu.settings.versionEnc;
            labelUnicode.Text = "Unicode is ";
            labelUnicode.Text += MainMenu.settings.unicodeSettings == 0 ? "set." : "not set.";
            sortLabel.Text = MainMenu.settings.sortSameString ? "Warning! Some files may be slowly extract due enabled sort strings." : "";
            checkEncDDS.Checked = MainMenu.settings.encDDSonly;
            checkIOS.Checked = MainMenu.settings.iOSsupport;
            checkEncLangdb.Checked = MainMenu.settings.encLangdb;
            CheckNewEngine.Checked = MainMenu.settings.encNewLua;
            checkRemoveBlanksBetweenCjk.Checked = MainMenu.settings.removeBlanksBetweenCjkCharsInImport;
            checkReplaceDotToChinesePeriod.Checked = MainMenu.settings.replaceDotToChinesePeriodInImport;
            checkNormalizeNewlinePunctuation.Checked = MainMenu.settings.normalizePunctuationBeforeNewlineInImport;
            checkAutoInsertSubtitleNewline.Checked = MainMenu.settings.autoInsertSubtitleNewlineInImport;
            checkEnableImportTextReplace.Checked = MainMenu.settings.enableImportTextReplace;
            textBoxImportTextReplaceFind.Text = MainMenu.settings.importTextReplaceFind ?? "";
            textBoxImportTextReplaceWith.Text = MainMenu.settings.importTextReplaceWith ?? "";
            LoadImportReplaceRulesToGrid();

            if (MainMenu.settings.swizzlePS4 || MainMenu.settings.swizzleNintendoSwitch || MainMenu.settings.swizzleXbox360 || MainMenu.settings.swizzlePSVita || MainMenu.settings.swizzleNintendoWii)
            {
                if (MainMenu.settings.swizzleNintendoSwitch) rbSwitchSwizzle.Checked = true;
                else if (MainMenu.settings.swizzlePS4) rbPS4Swizzle.Checked = true;
                else if (MainMenu.settings.swizzleXbox360) rbXbox360Swizzle.Checked = true;
                else if (MainMenu.settings.swizzlePSVita) rbPSVitaSwizzle.Checked = true;
                else if (MainMenu.settings.swizzleNintendoWii) rbWiiSwizzle.Checked = true;
            }
            else rbNoSwizzle.Checked = true;

            if (MainMenu.settings.customKey && Methods.stringToKey(MainMenu.settings.encCustomKey) != null)
            {
                checkCustomKey.Checked = MainMenu.settings.customKey;
                textBox1.Text = MainMenu.settings.encCustomKey;
            }

            if (MainMenu.settings.ASCII_N == 1252)
            {
                //Make unvisible that option for users with windows-1252 encoding
                labelUnicode.Visible = false;
            }
        }

        private void AutoPacker_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Validate();

                if (_importReplaceRulesGrid != null)
                {
                    CaptureCurrentEditingTextCellValue();
                    _importReplaceRulesGrid.EndEdit();
                }

                SaveImportReplaceRulesFromGrid(false);
            }
            catch
            {
                // Ignore grid commit errors on close to avoid blocking form shutdown.
            }
            finally
            {
                Settings.SaveConfig(MainMenu.settings);
            }

            if ((threadExport != null) && threadExport.IsAlive)
            {
                threadExport.Abort();
            }

            if ((threadImport != null) && threadImport.IsAlive)
            {
                threadImport.Abort();
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void CheckNewEngine_CheckedChanged(object sender, EventArgs e)
        {
            MainMenu.settings.encNewLua = CheckNewEngine.Checked;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void checkEncDDS_CheckedChanged(object sender, EventArgs e)
        {
            MainMenu.settings.encDDSonly = checkEncDDS.Checked;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void checkEncLangdb_CheckedChanged(object sender, EventArgs e)
        {
            MainMenu.settings.encLangdb = checkEncLangdb.Checked;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void checkIOS_CheckedChanged(object sender, EventArgs e)
        {
            MainMenu.settings.iOSsupport = checkIOS.Checked;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void checkCustomKey_CheckedChanged(object sender, EventArgs e)
        {
            MainMenu.settings.customKey = checkCustomKey.Checked;
            Settings.SaveConfig(MainMenu.settings);

            if ((MainMenu.settings.customKey == true) &&
                ((MainMenu.settings.encCustomKey != "") && (MainMenu.settings.encCustomKey != null)))
            {
                textBox1.Text = MainMenu.settings.encCustomKey;
            }
            else
            {
                textBox1.Text = "";
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            MainMenu.settings.encKeyIndex = comboBox1.SelectedIndex;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            MainMenu.settings.versionEnc = comboBox2.SelectedIndex;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (checkCustomKey.Checked && Methods.stringToKey(textBox1.Text) != null)
            {
                MainMenu.settings.customKey = checkCustomKey.Checked;
                MainMenu.settings.encCustomKey = textBox1.Text;
                Settings.SaveConfig(MainMenu.settings);
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AutoDePackerSettings settingsForm = new AutoDePackerSettings();
            settingsForm.FormClosed += new FormClosedEventHandler(Form_Closed);
            settingsForm.Show(this);
        }

        private void Form_Closed(object sender, FormClosedEventArgs e)
        {
            AutoDePackerSettings settingsForm = (AutoDePackerSettings)sender;

            labelUnicode.Text = "Unicode is ";
            labelUnicode.Text += MainMenu.settings.unicodeSettings == 0 ? "set." : "not set.";

            sortLabel.Text = MainMenu.settings.sortSameString ? "Warning! Some files may be slowly extract due enabled sort strings." : "";
        }

        private void SettingsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void rbNoSwizzle_CheckedChanged(object sender, EventArgs e)
        {
            if (rbNoSwizzle.Checked)
            {
                MainMenu.settings.swizzleNintendoSwitch = false;
                MainMenu.settings.swizzlePS4 = false;
                MainMenu.settings.swizzleXbox360 = false;
                MainMenu.settings.swizzlePSVita = false;
                MainMenu.settings.swizzleNintendoWii = false;
                Settings.SaveConfig(MainMenu.settings);
            }
        }

        private void rbPS4Swizzle_CheckedChanged(object sender, EventArgs e)
        {
            if (rbPS4Swizzle.Checked)
            {
                MainMenu.settings.swizzlePS4 = true;
                MainMenu.settings.swizzleNintendoSwitch = false;
                MainMenu.settings.swizzleXbox360 = false;
                MainMenu.settings.swizzlePSVita = false;
                MainMenu.settings.swizzleNintendoWii = false;
                Settings.SaveConfig(MainMenu.settings);
            }
        }

        private void rbSwitchSwizzle_CheckedChanged(object sender, EventArgs e)
        {
            if (rbSwitchSwizzle.Checked)
            {
                MainMenu.settings.swizzleNintendoSwitch = true;
                MainMenu.settings.swizzlePS4 = false;
                MainMenu.settings.swizzleXbox360 = false;
                MainMenu.settings.swizzlePSVita = false;
                MainMenu.settings.swizzleNintendoWii = false;
                Settings.SaveConfig(MainMenu.settings);
            }
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

        private void checkRemoveBlanksBetweenCjk_CheckedChanged(object sender, EventArgs e)
        {
            MainMenu.settings.removeBlanksBetweenCjkCharsInImport = checkRemoveBlanksBetweenCjk.Checked;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void checkReplaceDotToChinesePeriod_CheckedChanged(object sender, EventArgs e)
        {
            MainMenu.settings.replaceDotToChinesePeriodInImport = checkReplaceDotToChinesePeriod.Checked;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void checkNormalizeNewlinePunctuation_CheckedChanged(object sender, EventArgs e)
        {
            MainMenu.settings.normalizePunctuationBeforeNewlineInImport = checkNormalizeNewlinePunctuation.Checked;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void checkAutoInsertSubtitleNewline_CheckedChanged(object sender, EventArgs e)
        {
            MainMenu.settings.autoInsertSubtitleNewlineInImport = checkAutoInsertSubtitleNewline.Checked;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void checkEnableImportTextReplace_CheckedChanged(object sender, EventArgs e)
        {
            MainMenu.settings.enableImportTextReplace = checkEnableImportTextReplace.Checked;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void textBoxImportTextReplaceFind_TextChanged(object sender, EventArgs e)
        {
            MainMenu.settings.importTextReplaceFind = textBoxImportTextReplaceFind.Text;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void textBoxImportTextReplaceWith_TextChanged(object sender, EventArgs e)
        {
            MainMenu.settings.importTextReplaceWith = textBoxImportTextReplaceWith.Text;
            Settings.SaveConfig(MainMenu.settings);
        }

        private void LoadImportReplaceRulesToGrid()
        {
            if (_importReplaceRulesGrid == null) return;

            _suppressImportReplaceRuleEvents = true;

            _importReplaceRulesGrid.Rows.Clear();

            List<ImportTextReplaceRule> rules = MainMenu.settings.importTextReplaceRules;

            if (rules != null && rules.Count > 0)
            {
                for (int i = 0; i < rules.Count; i++)
                {
                    ImportTextReplaceRule rule = rules[i] ?? new ImportTextReplaceRule();
                    _importReplaceRulesGrid.Rows.Add(rule.enabled, rule.find ?? "", rule.replaceWith ?? "");
                }
            }
            else if (!String.IsNullOrEmpty(MainMenu.settings.importTextReplaceFind))
            {
                _importReplaceRulesGrid.Rows.Add(true, MainMenu.settings.importTextReplaceFind, MainMenu.settings.importTextReplaceWith ?? "");
            }
            else
            {
                _importReplaceRulesGrid.Rows.Add(true, "", "");
            }

            _suppressImportReplaceRuleEvents = false;
        }

        private void SaveImportReplaceRulesFromGrid(bool saveConfig = true)
        {
            if (_importReplaceRulesGrid == null) return;

            CaptureCurrentEditingTextCellValue();
            try
            {
                _importReplaceRulesGrid.EndEdit();
            }
            catch
            {
            }

            List<ImportTextReplaceRule> rules = new List<ImportTextReplaceRule>();
            List<ImportTextReplaceRule> existingRules = MainMenu.settings.importTextReplaceRules == null
                ? new List<ImportTextReplaceRule>()
                : MainMenu.settings.importTextReplaceRules;

            for (int i = 0; i < _importReplaceRulesGrid.Rows.Count; i++)
            {
                DataGridViewRow row = _importReplaceRulesGrid.Rows[i];
                // Keep the active editing placeholder row (new row) so typed data is not dropped.
                if (row.IsNewRow && row.Index != _editingRuleRowIndex) continue;

                bool enabled = false;
                if (row.Cells[0].Value != null)
                {
                    bool.TryParse(row.Cells[0].Value.ToString(), out enabled);
                }
                else if (row.Index < MainMenu.settings.importTextReplaceRules.Count && MainMenu.settings.importTextReplaceRules[row.Index] != null)
                {
                    enabled = MainMenu.settings.importTextReplaceRules[row.Index].enabled;
                }

                string find = row.Cells[1].Value == null ? "" : row.Cells[1].Value.ToString();
                string replaceWith = row.Cells[2].Value == null ? "" : row.Cells[2].Value.ToString();

                // If current edit is not committed yet (e.g. IME composition / close via [X]),
                // prefer the in-memory draft text for the active cell.
                if (row.Index == _editingRuleRowIndex && _editingRuleTextDraft != null)
                {
                    if (_editingRuleColumnIndex == 1)
                    {
                        find = _editingRuleTextDraft;
                    }
                    else if (_editingRuleColumnIndex == 2)
                    {
                        replaceWith = _editingRuleTextDraft;
                    }
                }

                if (String.IsNullOrEmpty(find) && String.IsNullOrEmpty(replaceWith) && !enabled) continue;

                rules.Add(new ImportTextReplaceRule
                {
                    enabled = enabled,
                    find = find,
                    replaceWith = replaceWith
                });
            }

            // If grid parsing yields no meaningful rules, do not wipe existing effective rules.
            // This protects against shutdown timing where DataGridView has not committed text yet.
            if (!HasMeaningfulImportReplaceRules(rules) && HasMeaningfulImportReplaceRules(existingRules))
            {
                rules = CloneImportReplaceRules(existingRules);
            }

            MainMenu.settings.importTextReplaceRules = rules;

            ImportTextReplaceRule firstActive = null;
            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i] != null && rules[i].enabled && !String.IsNullOrEmpty(rules[i].find))
                {
                    firstActive = rules[i];
                    break;
                }
            }

            MainMenu.settings.importTextReplaceFind = firstActive == null ? "" : firstActive.find;
            MainMenu.settings.importTextReplaceWith = firstActive == null ? "" : (firstActive.replaceWith ?? "");

            // If user has configured effective rules, auto-enable Replace to avoid silent no-op.
            if (HasMeaningfulImportReplaceRules(rules) && !MainMenu.settings.enableImportTextReplace)
            {
                MainMenu.settings.enableImportTextReplace = true;
                if (checkEnableImportTextReplace != null && !checkEnableImportTextReplace.Checked)
                {
                    checkEnableImportTextReplace.Checked = true;
                }
            }

            if (saveConfig)
            {
                Settings.SaveConfig(MainMenu.settings);
            }
        }

        private static bool HasMeaningfulImportReplaceRules(List<ImportTextReplaceRule> rules)
        {
            if (rules == null || rules.Count == 0) return false;

            for (int i = 0; i < rules.Count; i++)
            {
                ImportTextReplaceRule rule = rules[i];
                if (rule == null) continue;

                if (rule.enabled && !String.IsNullOrEmpty(rule.find)) return true;
                if (!String.IsNullOrEmpty(rule.find) || !String.IsNullOrEmpty(rule.replaceWith)) return true;
            }

            return false;
        }

        private static List<ImportTextReplaceRule> CloneImportReplaceRules(List<ImportTextReplaceRule> rules)
        {
            if (rules == null) return new List<ImportTextReplaceRule>();

            return rules.Select(r => new ImportTextReplaceRule
            {
                enabled = r != null && r.enabled,
                find = r == null ? "" : (r.find ?? ""),
                replaceWith = r == null ? "" : (r.replaceWith ?? "")
            }).ToList();
        }

        private void CaptureCurrentEditingTextCellValue()
        {
            if (_importReplaceRulesGrid == null) return;

            DataGridViewCell currentCell = _importReplaceRulesGrid.CurrentCell;
            if (currentCell == null) return;
            if (!(currentCell is DataGridViewTextBoxCell)) return;

            TextBox editingTextBox = _importReplaceRulesGrid.EditingControl as TextBox;
            if (editingTextBox == null) return;

            currentCell.Value = editingTextBox.Text;
            _editingRuleRowIndex = currentCell.RowIndex;
            _editingRuleColumnIndex = currentCell.ColumnIndex;
            _editingRuleTextDraft = editingTextBox.Text;
        }

        private void ImportReplaceRulesGrid_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (_activeRuleEditingControl != null)
            {
                _activeRuleEditingControl.TextChanged -= ImportReplaceRulesEditingControl_TextChanged;
            }

            _activeRuleEditingControl = e.Control as DataGridViewTextBoxEditingControl;
            if (_activeRuleEditingControl == null) return;

            if (_importReplaceRulesGrid != null && _importReplaceRulesGrid.CurrentCell != null)
            {
                _editingRuleRowIndex = _importReplaceRulesGrid.CurrentCell.RowIndex;
                _editingRuleColumnIndex = _importReplaceRulesGrid.CurrentCell.ColumnIndex;
                _editingRuleTextDraft = _activeRuleEditingControl.Text;
            }

            _activeRuleEditingControl.ImeMode = ImeMode.On;
            _activeRuleEditingControl.TextChanged += ImportReplaceRulesEditingControl_TextChanged;
        }

        private void ImportReplaceRulesEditingControl_TextChanged(object sender, EventArgs e)
        {
            if (_suppressImportReplaceRuleEvents) return;

            TextBox editingTextBox = sender as TextBox;
            if (editingTextBox == null) return;

            _editingRuleTextDraft = editingTextBox.Text;
            ApplyEditingDraftToSettings();
            PersistImportReplaceRulesDraftToConfig();
        }

        private void ApplyEditingDraftToSettings()
        {
            if (_editingRuleRowIndex < 0) return;
            if (_editingRuleColumnIndex != 1 && _editingRuleColumnIndex != 2) return;

            List<ImportTextReplaceRule> rules = MainMenu.settings.importTextReplaceRules;
            while (rules.Count <= _editingRuleRowIndex)
            {
                rules.Add(new ImportTextReplaceRule());
            }

            ImportTextReplaceRule rule = rules[_editingRuleRowIndex] ?? new ImportTextReplaceRule();
            if (_editingRuleColumnIndex == 1)
            {
                rule.find = _editingRuleTextDraft ?? "";
            }
            else
            {
                rule.replaceWith = _editingRuleTextDraft ?? "";
            }

            rules[_editingRuleRowIndex] = rule;
            MainMenu.settings.importTextReplaceRules = rules;
        }

        private void PersistImportReplaceRulesDraftToConfig()
        {
            List<ImportTextReplaceRule> rules = MainMenu.settings.importTextReplaceRules;

            ImportTextReplaceRule firstActive = null;
            for (int i = 0; i < rules.Count; i++)
            {
                if (rules[i] != null && rules[i].enabled && !String.IsNullOrEmpty(rules[i].find))
                {
                    firstActive = rules[i];
                    break;
                }
            }

            MainMenu.settings.importTextReplaceFind = firstActive == null ? "" : firstActive.find;
            MainMenu.settings.importTextReplaceWith = firstActive == null ? "" : (firstActive.replaceWith ?? "");

            if (HasMeaningfulImportReplaceRules(rules) && !MainMenu.settings.enableImportTextReplace)
            {
                MainMenu.settings.enableImportTextReplace = true;
                if (checkEnableImportTextReplace != null && !checkEnableImportTextReplace.Checked)
                {
                    checkEnableImportTextReplace.Checked = true;
                }
            }

            Settings.SaveConfig(MainMenu.settings);
        }

        private void ImportReplaceRulesGrid_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (_importReplaceRulesGrid == null) return;

            if (!_importReplaceRulesGrid.IsCurrentCellDirty) return;

            DataGridViewCell currentCell = _importReplaceRulesGrid.CurrentCell;
            if (currentCell == null) return;

            // Only commit immediately for checkbox cells.
            // Text cells should keep the IME composition flow (space confirms candidate).
            if (currentCell is DataGridViewCheckBoxCell)
            {
                _importReplaceRulesGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void ImportReplaceRulesGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_suppressImportReplaceRuleEvents) return;
            SaveImportReplaceRulesFromGrid();
        }

        private void ImportReplaceRulesGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (_suppressImportReplaceRuleEvents) return;
            SaveImportReplaceRulesFromGrid();

            _editingRuleRowIndex = -1;
            _editingRuleColumnIndex = -1;
            _editingRuleTextDraft = null;
        }

        private void BtnAddReplaceRule_Click(object sender, EventArgs e)
        {
            if (_importReplaceRulesGrid == null) return;

            _importReplaceRulesGrid.Rows.Add(true, "", "");
            SaveImportReplaceRulesFromGrid();
        }

        private void BtnRemoveReplaceRule_Click(object sender, EventArgs e)
        {
            if (_importReplaceRulesGrid == null) return;

            if (_importReplaceRulesGrid.SelectedRows.Count == 0)
            {
                if (_importReplaceRulesGrid.CurrentRow != null && !_importReplaceRulesGrid.CurrentRow.IsNewRow)
                    _importReplaceRulesGrid.Rows.Remove(_importReplaceRulesGrid.CurrentRow);
            }
            else
            {
                for (int i = _importReplaceRulesGrid.SelectedRows.Count - 1; i >= 0; i--)
                {
                    DataGridViewRow row = _importReplaceRulesGrid.SelectedRows[i];
                    if (!row.IsNewRow) _importReplaceRulesGrid.Rows.Remove(row);
                }
            }

            if (_importReplaceRulesGrid.Rows.Count == 0)
            {
                _importReplaceRulesGrid.Rows.Add(true, "", "");
            }

            SaveImportReplaceRulesFromGrid();
        }

        private void BtnEnableAllReplaceRules_Click(object sender, EventArgs e)
        {
            if (_importReplaceRulesGrid == null) return;

            _suppressImportReplaceRuleEvents = true;
            for (int i = 0; i < _importReplaceRulesGrid.Rows.Count; i++)
            {
                DataGridViewRow row = _importReplaceRulesGrid.Rows[i];
                if (!row.IsNewRow) row.Cells[0].Value = true;
            }
            _suppressImportReplaceRuleEvents = false;

            SaveImportReplaceRulesFromGrid();
        }

        private void BtnInvertReplaceRules_Click(object sender, EventArgs e)
        {
            if (_importReplaceRulesGrid == null) return;

            _suppressImportReplaceRuleEvents = true;
            for (int i = 0; i < _importReplaceRulesGrid.Rows.Count; i++)
            {
                DataGridViewRow row = _importReplaceRulesGrid.Rows[i];
                if (row.IsNewRow) continue;

                bool enabled = false;
                if (row.Cells[0].Value != null)
                    bool.TryParse(row.Cells[0].Value.ToString(), out enabled);

                row.Cells[0].Value = !enabled;
            }
            _suppressImportReplaceRuleEvents = false;

            SaveImportReplaceRulesFromGrid();
        }

        private void convertArgb8888Cb_CheckedChanged(object sender, EventArgs e)
        {
        }
    }
}
