namespace TTG_Tools
{
    partial class AutoPacker
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoPacker));
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.RichTextBox();
            this.buttonDecrypt = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbSwitchSwizzle = new System.Windows.Forms.RadioButton();
            this.rbPS4Swizzle = new System.Windows.Forms.RadioButton();
            this.rbXbox360Swizzle = new System.Windows.Forms.RadioButton();
            this.rbPSVitaSwizzle = new System.Windows.Forms.RadioButton();
            this.rbWiiSwizzle = new System.Windows.Forms.RadioButton();
            this.rbNoSwizzle = new System.Windows.Forms.RadioButton();
            this.labelUnicode = new System.Windows.Forms.Label();
            this.checkIOS = new System.Windows.Forms.CheckBox();
            this.CheckNewEngine = new System.Windows.Forms.CheckBox();
            this.checkEncDDS = new System.Windows.Forms.CheckBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkEncLangdb = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkCustomKey = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortLabel = new System.Windows.Forms.Label();
            this.checkRemoveBlanksBetweenCjk = new System.Windows.Forms.CheckBox();
            this.checkReplaceDotToChinesePeriod = new System.Windows.Forms.CheckBox();
            this.checkNormalizeNewlinePunctuation = new System.Windows.Forms.CheckBox();
            this.checkAutoInsertSubtitleNewline = new System.Windows.Forms.CheckBox();
            this.groupBoxImportTextTransforms = new System.Windows.Forms.GroupBox();
            this._btnInvertReplaceRules = new System.Windows.Forms.Button();
            this._btnEnableAllReplaceRules = new System.Windows.Forms.Button();
            this._btnRemoveReplaceRule = new System.Windows.Forms.Button();
            this._btnAddReplaceRule = new System.Windows.Forms.Button();
            this._importReplaceRulesGrid = new System.Windows.Forms.DataGridView();
            this.colRuleEnabled = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colRuleFind = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRuleReplaceWith = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textBoxImportTextReplaceWith = new System.Windows.Forms.TextBox();
            this.labelImportTextReplaceWith = new System.Windows.Forms.Label();
            this.textBoxImportTextReplaceFind = new System.Windows.Forms.TextBox();
            this.labelImportTextReplaceFind = new System.Windows.Forms.Label();
            this.checkEnableImportTextReplace = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.groupBoxImportTextTransforms.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._importReplaceRulesGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.DisplayMember = "0";
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Before The Wolf Among Us",
            "Sam & Max 201",
            "Sam & Max 202",
            "Sam & Max 203",
            "Sam & Max 204",
            "Sam & Max 205",
            "After The Wolf Among Us"});
            this.comboBox1.Location = new System.Drawing.Point(22, 54);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(328, 21);
            this.comboBox1.TabIndex = 0;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(547, 359);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(124, 21);
            this.button1.TabIndex = 1;
            this.button1.Text = "Encrypt, Pack, Import";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.Location = new System.Drawing.Point(10, 392);
            this.listBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.listBox1.Name = "listBox1";
            this.listBox1.ReadOnly = true;
            this.listBox1.Size = new System.Drawing.Size(801, 199);
            this.listBox1.TabIndex = 2;
            this.listBox1.Text = "";
            // 
            // buttonDecrypt
            // 
            this.buttonDecrypt.Location = new System.Drawing.Point(686, 359);
            this.buttonDecrypt.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonDecrypt.Name = "buttonDecrypt";
            this.buttonDecrypt.Size = new System.Drawing.Size(124, 21);
            this.buttonDecrypt.TabIndex = 1;
            this.buttonDecrypt.Text = "Decrypt, Export";
            this.buttonDecrypt.UseVisualStyleBackColor = true;
            this.buttonDecrypt.Click += new System.EventHandler(this.buttonDecrypt_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Controls.Add(this.labelUnicode);
            this.groupBox1.Controls.Add(this.checkIOS);
            this.groupBox1.Controls.Add(this.CheckNewEngine);
            this.groupBox1.Controls.Add(this.checkEncDDS);
            this.groupBox1.Controls.Add(this.comboBox2);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.checkEncLangdb);
            this.groupBox1.Location = new System.Drawing.Point(368, 35);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(442, 147);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Some functions";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rbSwitchSwizzle);
            this.groupBox2.Controls.Add(this.rbPS4Swizzle);
            this.groupBox2.Controls.Add(this.rbXbox360Swizzle);
            this.groupBox2.Controls.Add(this.rbPSVitaSwizzle);
            this.groupBox2.Controls.Add(this.rbWiiSwizzle);
            this.groupBox2.Controls.Add(this.rbNoSwizzle);
            this.groupBox2.Location = new System.Drawing.Point(276, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(126, 142);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Swizzle methods";
            // 
            // rbSwitchSwizzle
            // 
            this.rbSwitchSwizzle.AutoSize = true;
            this.rbSwitchSwizzle.Location = new System.Drawing.Point(16, 60);
            this.rbSwitchSwizzle.Name = "rbSwitchSwizzle";
            this.rbSwitchSwizzle.Size = new System.Drawing.Size(102, 17);
            this.rbSwitchSwizzle.TabIndex = 2;
            this.rbSwitchSwizzle.TabStop = true;
            this.rbSwitchSwizzle.Text = "Nintendo Switch";
            this.rbSwitchSwizzle.UseVisualStyleBackColor = true;
            this.rbSwitchSwizzle.CheckedChanged += new System.EventHandler(this.rbSwitchSwizzle_CheckedChanged);
            // 
            // rbPS4Swizzle
            // 
            this.rbPS4Swizzle.AutoSize = true;
            this.rbPS4Swizzle.Location = new System.Drawing.Point(16, 42);
            this.rbPS4Swizzle.Name = "rbPS4Swizzle";
            this.rbPS4Swizzle.Size = new System.Drawing.Size(43, 17);
            this.rbPS4Swizzle.TabIndex = 1;
            this.rbPS4Swizzle.TabStop = true;
            this.rbPS4Swizzle.Text = "PS4";
            this.rbPS4Swizzle.UseVisualStyleBackColor = true;
            this.rbPS4Swizzle.CheckedChanged += new System.EventHandler(this.rbPS4Swizzle_CheckedChanged);
            // 
            // rbXbox360Swizzle
            // 
            this.rbXbox360Swizzle.AutoSize = true;
            this.rbXbox360Swizzle.Location = new System.Drawing.Point(16, 97);
            this.rbXbox360Swizzle.Name = "rbXbox360Swizzle";
            this.rbXbox360Swizzle.Size = new System.Drawing.Size(70, 17);
            this.rbXbox360Swizzle.TabIndex = 4;
            this.rbXbox360Swizzle.TabStop = true;
            this.rbXbox360Swizzle.Text = "Xbox 360";
            this.rbXbox360Swizzle.UseVisualStyleBackColor = true;
            this.rbXbox360Swizzle.CheckedChanged += new System.EventHandler(this.rbXbox360Swizzle_CheckedChanged);
            // 
            // rbPSVitaSwizzle
            // 
            this.rbPSVitaSwizzle.AutoSize = true;
            this.rbPSVitaSwizzle.Location = new System.Drawing.Point(16, 78);
            this.rbPSVitaSwizzle.Name = "rbPSVitaSwizzle";
            this.rbPSVitaSwizzle.Size = new System.Drawing.Size(58, 17);
            this.rbPSVitaSwizzle.TabIndex = 3;
            this.rbPSVitaSwizzle.TabStop = true;
            this.rbPSVitaSwizzle.Text = "PS Vita";
            this.rbPSVitaSwizzle.UseVisualStyleBackColor = true;
            this.rbPSVitaSwizzle.CheckedChanged += new System.EventHandler(this.rbPSVitaSwizzle_CheckedChanged);
            // 
            // rbWiiSwizzle
            // 
            this.rbWiiSwizzle.AutoSize = true;
            this.rbWiiSwizzle.Location = new System.Drawing.Point(16, 116);
            this.rbWiiSwizzle.Name = "rbWiiSwizzle";
            this.rbWiiSwizzle.Size = new System.Drawing.Size(85, 17);
            this.rbWiiSwizzle.TabIndex = 5;
            this.rbWiiSwizzle.TabStop = true;
            this.rbWiiSwizzle.Text = "Nintendo Wii";
            this.rbWiiSwizzle.UseVisualStyleBackColor = true;
            this.rbWiiSwizzle.CheckedChanged += new System.EventHandler(this.rbWiiSwizzle_CheckedChanged);
            // 
            // rbNoSwizzle
            // 
            this.rbNoSwizzle.AutoSize = true;
            this.rbNoSwizzle.Location = new System.Drawing.Point(16, 19);
            this.rbNoSwizzle.Name = "rbNoSwizzle";
            this.rbNoSwizzle.Size = new System.Drawing.Size(50, 17);
            this.rbNoSwizzle.TabIndex = 0;
            this.rbNoSwizzle.TabStop = true;
            this.rbNoSwizzle.Text = "None";
            this.rbNoSwizzle.UseVisualStyleBackColor = true;
            this.rbNoSwizzle.CheckedChanged += new System.EventHandler(this.rbNoSwizzle_CheckedChanged);
            // 
            // labelUnicode
            // 
            this.labelUnicode.AutoSize = true;
            this.labelUnicode.Location = new System.Drawing.Point(25, 129);
            this.labelUnicode.Name = "labelUnicode";
            this.labelUnicode.Size = new System.Drawing.Size(70, 13);
            this.labelUnicode.TabIndex = 16;
            this.labelUnicode.Text = "Unicode label";
            // 
            // checkIOS
            // 
            this.checkIOS.AutoSize = true;
            this.checkIOS.Location = new System.Drawing.Point(8, 107);
            this.checkIOS.Margin = new System.Windows.Forms.Padding(2);
            this.checkIOS.Name = "checkIOS";
            this.checkIOS.Size = new System.Drawing.Size(124, 17);
            this.checkIOS.TabIndex = 14;
            this.checkIOS.Text = "iOS (for new games)";
            this.checkIOS.UseVisualStyleBackColor = true;
            this.checkIOS.CheckedChanged += new System.EventHandler(this.checkIOS_CheckedChanged);
            // 
            // CheckNewEngine
            // 
            this.CheckNewEngine.AutoSize = true;
            this.CheckNewEngine.Location = new System.Drawing.Point(8, 86);
            this.CheckNewEngine.Margin = new System.Windows.Forms.Padding(2);
            this.CheckNewEngine.Name = "CheckNewEngine";
            this.CheckNewEngine.Size = new System.Drawing.Size(152, 17);
            this.CheckNewEngine.TabIndex = 13;
            this.CheckNewEngine.Text = "Lua scripts for new engine";
            this.CheckNewEngine.UseVisualStyleBackColor = true;
            this.CheckNewEngine.CheckedChanged += new System.EventHandler(this.CheckNewEngine_CheckedChanged);
            // 
            // checkEncDDS
            // 
            this.checkEncDDS.AutoSize = true;
            this.checkEncDDS.Location = new System.Drawing.Point(8, 38);
            this.checkEncDDS.Margin = new System.Windows.Forms.Padding(2);
            this.checkEncDDS.Name = "checkEncDDS";
            this.checkEncDDS.Size = new System.Drawing.Size(146, 17);
            this.checkEncDDS.TabIndex = 12;
            this.checkEncDDS.Text = "Encrypt DDS header only";
            this.checkEncDDS.UseVisualStyleBackColor = true;
            this.checkEncDDS.CheckedChanged += new System.EventHandler(this.checkEncDDS_CheckedChanged);
            // 
            // comboBox2
            // 
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Items.AddRange(new object[] {
            "Versions 2-6",
            "Versions 7-9"});
            this.comboBox2.Location = new System.Drawing.Point(120, 61);
            this.comboBox2.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(108, 21);
            this.comboBox2.TabIndex = 9;
            this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 64);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Method encryption:";
            // 
            // checkEncLangdb
            // 
            this.checkEncLangdb.AutoSize = true;
            this.checkEncLangdb.Location = new System.Drawing.Point(8, 15);
            this.checkEncLangdb.Margin = new System.Windows.Forms.Padding(2);
            this.checkEncLangdb.Name = "checkEncLangdb";
            this.checkEncLangdb.Size = new System.Drawing.Size(273, 17);
            this.checkEncLangdb.TabIndex = 7;
            this.checkEncLangdb.Text = "Encrypt langdb/dlog/d3dtx (fully encrypt d3dtx file)";
            this.checkEncLangdb.UseVisualStyleBackColor = true;
            this.checkEncLangdb.CheckedChanged += new System.EventHandler(this.checkEncLangdb_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 35);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(352, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Blowfish keys for encryption some old files or new compressed archives:";
            // 
            // checkCustomKey
            // 
            this.checkCustomKey.AutoSize = true;
            this.checkCustomKey.Location = new System.Drawing.Point(11, 80);
            this.checkCustomKey.Margin = new System.Windows.Forms.Padding(2);
            this.checkCustomKey.Name = "checkCustomKey";
            this.checkCustomKey.Size = new System.Drawing.Size(103, 17);
            this.checkCustomKey.TabIndex = 12;
            this.checkCustomKey.Text = "Set custom key:";
            this.checkCustomKey.UseVisualStyleBackColor = true;
            this.checkCustomKey.CheckedChanged += new System.EventHandler(this.checkCustomKey_CheckedChanged);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(111, 78);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(239, 21);
            this.textBox1.TabIndex = 13;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(821, 25);
            this.menuStrip1.TabIndex = 15;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(66, 21);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // sortLabel
            // 
            this.sortLabel.AutoSize = true;
            this.sortLabel.Location = new System.Drawing.Point(12, 363);
            this.sortLabel.Name = "sortLabel";
            this.sortLabel.Size = new System.Drawing.Size(52, 13);
            this.sortLabel.TabIndex = 16;
            this.sortLabel.Text = "Sort label";
            // 
            // checkRemoveBlanksBetweenCjk
            // 
            this.checkRemoveBlanksBetweenCjk.AutoSize = true;
            this.checkRemoveBlanksBetweenCjk.Location = new System.Drawing.Point(175, 20);
            this.checkRemoveBlanksBetweenCjk.Name = "checkRemoveBlanksBetweenCjk";
            this.checkRemoveBlanksBetweenCjk.Size = new System.Drawing.Size(190, 17);
            this.checkRemoveBlanksBetweenCjk.TabIndex = 17;
            this.checkRemoveBlanksBetweenCjk.Text = "remove blanks between CJK chars";
            this.checkRemoveBlanksBetweenCjk.UseVisualStyleBackColor = true;
            this.checkRemoveBlanksBetweenCjk.CheckedChanged += new System.EventHandler(this.checkRemoveBlanksBetweenCjk_CheckedChanged);
            // 
            // checkReplaceDotToChinesePeriod
            // 
            this.checkReplaceDotToChinesePeriod.AutoSize = true;
            this.checkReplaceDotToChinesePeriod.Location = new System.Drawing.Point(8, 20);
            this.checkReplaceDotToChinesePeriod.Name = "checkReplaceDotToChinesePeriod";
            this.checkReplaceDotToChinesePeriod.Size = new System.Drawing.Size(167, 17);
            this.checkReplaceDotToChinesePeriod.TabIndex = 18;
            this.checkReplaceDotToChinesePeriod.Text = "replace dot to Chinese period";
            this.checkReplaceDotToChinesePeriod.UseVisualStyleBackColor = true;
            this.checkReplaceDotToChinesePeriod.CheckedChanged += new System.EventHandler(this.checkReplaceDotToChinesePeriod_CheckedChanged);
            // 
            // checkNormalizeNewlinePunctuation
            // 
            this.checkNormalizeNewlinePunctuation.AutoSize = true;
            this.checkNormalizeNewlinePunctuation.Location = new System.Drawing.Point(571, 20);
            this.checkNormalizeNewlinePunctuation.Name = "checkNormalizeNewlinePunctuation";
            this.checkNormalizeNewlinePunctuation.Size = new System.Drawing.Size(205, 17);
            this.checkNormalizeNewlinePunctuation.TabIndex = 19;
            this.checkNormalizeNewlinePunctuation.Text = "normalize punctuation before explicit \n";
            this.checkNormalizeNewlinePunctuation.UseVisualStyleBackColor = true;
            this.checkNormalizeNewlinePunctuation.CheckedChanged += new System.EventHandler(this.checkNormalizeNewlinePunctuation_CheckedChanged);
            // 
            // checkAutoInsertSubtitleNewline
            // 
            this.checkAutoInsertSubtitleNewline.AutoSize = true;
            this.checkAutoInsertSubtitleNewline.Location = new System.Drawing.Point(371, 20);
            this.checkAutoInsertSubtitleNewline.Name = "checkAutoInsertSubtitleNewline";
            this.checkAutoInsertSubtitleNewline.Size = new System.Drawing.Size(194, 17);
            this.checkAutoInsertSubtitleNewline.TabIndex = 29;
            this.checkAutoInsertSubtitleNewline.Text = "Auto-wrap long subtitles (insert \\n)";
            this.checkAutoInsertSubtitleNewline.UseVisualStyleBackColor = true;
            this.checkAutoInsertSubtitleNewline.CheckedChanged += new System.EventHandler(this.checkAutoInsertSubtitleNewline_CheckedChanged);
            // 
            // groupBoxImportTextTransforms
            // 
            this.groupBoxImportTextTransforms.Controls.Add(this.checkAutoInsertSubtitleNewline);
            this.groupBoxImportTextTransforms.Controls.Add(this.checkNormalizeNewlinePunctuation);
            this.groupBoxImportTextTransforms.Controls.Add(this._btnInvertReplaceRules);
            this.groupBoxImportTextTransforms.Controls.Add(this._btnEnableAllReplaceRules);
            this.groupBoxImportTextTransforms.Controls.Add(this._btnRemoveReplaceRule);
            this.groupBoxImportTextTransforms.Controls.Add(this._btnAddReplaceRule);
            this.groupBoxImportTextTransforms.Controls.Add(this._importReplaceRulesGrid);
            this.groupBoxImportTextTransforms.Controls.Add(this.textBoxImportTextReplaceWith);
            this.groupBoxImportTextTransforms.Controls.Add(this.labelImportTextReplaceWith);
            this.groupBoxImportTextTransforms.Controls.Add(this.textBoxImportTextReplaceFind);
            this.groupBoxImportTextTransforms.Controls.Add(this.labelImportTextReplaceFind);
            this.groupBoxImportTextTransforms.Controls.Add(this.checkEnableImportTextReplace);
            this.groupBoxImportTextTransforms.Controls.Add(this.checkReplaceDotToChinesePeriod);
            this.groupBoxImportTextTransforms.Controls.Add(this.checkRemoveBlanksBetweenCjk);
            this.groupBoxImportTextTransforms.Location = new System.Drawing.Point(10, 188);
            this.groupBoxImportTextTransforms.Name = "groupBoxImportTextTransforms";
            this.groupBoxImportTextTransforms.Size = new System.Drawing.Size(800, 159);
            this.groupBoxImportTextTransforms.TabIndex = 19;
            this.groupBoxImportTextTransforms.TabStop = false;
            this.groupBoxImportTextTransforms.Text = "Text Normalization (Import only)";
            // 
            // _btnInvertReplaceRules
            // 
            this._btnInvertReplaceRules.Location = new System.Drawing.Point(689, 131);
            this._btnInvertReplaceRules.Name = "_btnInvertReplaceRules";
            this._btnInvertReplaceRules.Size = new System.Drawing.Size(103, 21);
            this._btnInvertReplaceRules.TabIndex = 28;
            this._btnInvertReplaceRules.Text = "Invert";
            this._btnInvertReplaceRules.UseVisualStyleBackColor = true;
            this._btnInvertReplaceRules.Click += new System.EventHandler(this.BtnInvertReplaceRules_Click);
            // 
            // _btnEnableAllReplaceRules
            // 
            this._btnEnableAllReplaceRules.Location = new System.Drawing.Point(585, 131);
            this._btnEnableAllReplaceRules.Name = "_btnEnableAllReplaceRules";
            this._btnEnableAllReplaceRules.Size = new System.Drawing.Size(98, 21);
            this._btnEnableAllReplaceRules.TabIndex = 27;
            this._btnEnableAllReplaceRules.Text = "Select All";
            this._btnEnableAllReplaceRules.UseVisualStyleBackColor = true;
            this._btnEnableAllReplaceRules.Click += new System.EventHandler(this.BtnEnableAllReplaceRules_Click);
            // 
            // _btnRemoveReplaceRule
            // 
            this._btnRemoveReplaceRule.Location = new System.Drawing.Point(87, 131);
            this._btnRemoveReplaceRule.Name = "_btnRemoveReplaceRule";
            this._btnRemoveReplaceRule.Size = new System.Drawing.Size(75, 21);
            this._btnRemoveReplaceRule.TabIndex = 26;
            this._btnRemoveReplaceRule.Text = "Remove";
            this._btnRemoveReplaceRule.UseVisualStyleBackColor = true;
            this._btnRemoveReplaceRule.Click += new System.EventHandler(this.BtnRemoveReplaceRule_Click);
            // 
            // _btnAddReplaceRule
            // 
            this._btnAddReplaceRule.Location = new System.Drawing.Point(8, 131);
            this._btnAddReplaceRule.Name = "_btnAddReplaceRule";
            this._btnAddReplaceRule.Size = new System.Drawing.Size(75, 21);
            this._btnAddReplaceRule.TabIndex = 25;
            this._btnAddReplaceRule.Text = "Add";
            this._btnAddReplaceRule.UseVisualStyleBackColor = true;
            this._btnAddReplaceRule.Click += new System.EventHandler(this.BtnAddReplaceRule_Click);
            // 
            // _importReplaceRulesGrid
            // 
            this._importReplaceRulesGrid.AllowUserToAddRows = false;
            this._importReplaceRulesGrid.AllowUserToDeleteRows = false;
            this._importReplaceRulesGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this._importReplaceRulesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this._importReplaceRulesGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRuleEnabled,
            this.colRuleFind,
            this.colRuleReplaceWith});
            this._importReplaceRulesGrid.Location = new System.Drawing.Point(8, 44);
            this._importReplaceRulesGrid.Name = "_importReplaceRulesGrid";
            this._importReplaceRulesGrid.RowHeadersVisible = false;
            this._importReplaceRulesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this._importReplaceRulesGrid.Size = new System.Drawing.Size(784, 83);
            this._importReplaceRulesGrid.TabIndex = 24;
            this._importReplaceRulesGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.ImportReplaceRulesGrid_CellEndEdit);
            this._importReplaceRulesGrid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.ImportReplaceRulesGrid_CellValueChanged);
            this._importReplaceRulesGrid.CurrentCellDirtyStateChanged += new System.EventHandler(this.ImportReplaceRulesGrid_CurrentCellDirtyStateChanged);
            this._importReplaceRulesGrid.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.ImportReplaceRulesGrid_EditingControlShowing);
            // 
            // colRuleEnabled
            // 
            this.colRuleEnabled.FillWeight = 18F;
            this.colRuleEnabled.HeaderText = "Use";
            this.colRuleEnabled.Name = "colRuleEnabled";
            // 
            // colRuleFind
            // 
            this.colRuleFind.FillWeight = 41F;
            this.colRuleFind.HeaderText = "Find";
            this.colRuleFind.Name = "colRuleFind";
            // 
            // colRuleReplaceWith
            // 
            this.colRuleReplaceWith.FillWeight = 41F;
            this.colRuleReplaceWith.HeaderText = "With";
            this.colRuleReplaceWith.Name = "colRuleReplaceWith";
            // 
            // textBoxImportTextReplaceWith
            // 
            this.textBoxImportTextReplaceWith.Location = new System.Drawing.Point(236, 45);
            this.textBoxImportTextReplaceWith.Name = "textBoxImportTextReplaceWith";
            this.textBoxImportTextReplaceWith.Size = new System.Drawing.Size(108, 21);
            this.textBoxImportTextReplaceWith.TabIndex = 23;
            this.textBoxImportTextReplaceWith.Visible = false;
            this.textBoxImportTextReplaceWith.TextChanged += new System.EventHandler(this.textBoxImportTextReplaceWith_TextChanged);
            // 
            // labelImportTextReplaceWith
            // 
            this.labelImportTextReplaceWith.AutoSize = true;
            this.labelImportTextReplaceWith.Location = new System.Drawing.Point(202, 48);
            this.labelImportTextReplaceWith.Name = "labelImportTextReplaceWith";
            this.labelImportTextReplaceWith.Size = new System.Drawing.Size(29, 13);
            this.labelImportTextReplaceWith.TabIndex = 22;
            this.labelImportTextReplaceWith.Text = "With";
            this.labelImportTextReplaceWith.Visible = false;
            // 
            // textBoxImportTextReplaceFind
            // 
            this.textBoxImportTextReplaceFind.Location = new System.Drawing.Point(103, 45);
            this.textBoxImportTextReplaceFind.Name = "textBoxImportTextReplaceFind";
            this.textBoxImportTextReplaceFind.Size = new System.Drawing.Size(94, 21);
            this.textBoxImportTextReplaceFind.TabIndex = 21;
            this.textBoxImportTextReplaceFind.Visible = false;
            this.textBoxImportTextReplaceFind.TextChanged += new System.EventHandler(this.textBoxImportTextReplaceFind_TextChanged);
            // 
            // labelImportTextReplaceFind
            // 
            this.labelImportTextReplaceFind.AutoSize = true;
            this.labelImportTextReplaceFind.Location = new System.Drawing.Point(75, 48);
            this.labelImportTextReplaceFind.Name = "labelImportTextReplaceFind";
            this.labelImportTextReplaceFind.Size = new System.Drawing.Size(27, 13);
            this.labelImportTextReplaceFind.TabIndex = 20;
            this.labelImportTextReplaceFind.Text = "Find";
            this.labelImportTextReplaceFind.Visible = false;
            // 
            // checkEnableImportTextReplace
            // 
            this.checkEnableImportTextReplace.AutoSize = true;
            this.checkEnableImportTextReplace.Location = new System.Drawing.Point(8, 47);
            this.checkEnableImportTextReplace.Name = "checkEnableImportTextReplace";
            this.checkEnableImportTextReplace.Size = new System.Drawing.Size(64, 17);
            this.checkEnableImportTextReplace.TabIndex = 19;
            this.checkEnableImportTextReplace.Text = "Replace";
            this.checkEnableImportTextReplace.UseVisualStyleBackColor = true;
            this.checkEnableImportTextReplace.CheckedChanged += new System.EventHandler(this.checkEnableImportTextReplace_CheckedChanged);
            // 
            // AutoPacker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(821, 612);
            this.Controls.Add(this.groupBoxImportTextTransforms);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.sortLabel);
            this.Controls.Add(this.checkCustomKey);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.buttonDecrypt);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(414, 293);
            this.Name = "AutoPacker";
            this.Text = "Auto(De)Packer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AutoPacker_FormClosing);
            this.Load += new System.EventHandler(this.AutoPacker_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBoxImportTextTransforms.ResumeLayout(false);
            this.groupBoxImportTextTransforms.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._importReplaceRulesGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox listBox1;
        private System.Windows.Forms.Button buttonDecrypt;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkEncLangdb;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox CheckNewEngine;
        private System.Windows.Forms.CheckBox checkEncDDS;
        private System.Windows.Forms.CheckBox checkCustomKey;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.CheckBox checkIOS;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.Label labelUnicode;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbSwitchSwizzle;
        private System.Windows.Forms.RadioButton rbPS4Swizzle;
        private System.Windows.Forms.RadioButton rbXbox360Swizzle;
        private System.Windows.Forms.RadioButton rbPSVitaSwizzle;
        private System.Windows.Forms.RadioButton rbWiiSwizzle;
        private System.Windows.Forms.RadioButton rbNoSwizzle;
        private System.Windows.Forms.Label sortLabel;
        private System.Windows.Forms.CheckBox checkRemoveBlanksBetweenCjk;
        private System.Windows.Forms.CheckBox checkReplaceDotToChinesePeriod;
        private System.Windows.Forms.CheckBox checkNormalizeNewlinePunctuation;
        private System.Windows.Forms.CheckBox checkAutoInsertSubtitleNewline;
        private System.Windows.Forms.GroupBox groupBoxImportTextTransforms;
        private System.Windows.Forms.CheckBox checkEnableImportTextReplace;
        private System.Windows.Forms.Label labelImportTextReplaceFind;
        private System.Windows.Forms.TextBox textBoxImportTextReplaceFind;
        private System.Windows.Forms.Label labelImportTextReplaceWith;
        private System.Windows.Forms.TextBox textBoxImportTextReplaceWith;
        private System.Windows.Forms.Button _btnInvertReplaceRules;
        private System.Windows.Forms.Button _btnEnableAllReplaceRules;
        private System.Windows.Forms.Button _btnRemoveReplaceRule;
        private System.Windows.Forms.Button _btnAddReplaceRule;
        private System.Windows.Forms.DataGridView _importReplaceRulesGrid;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colRuleEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRuleFind;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRuleReplaceWith;
    }
}

