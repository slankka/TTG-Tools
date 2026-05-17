namespace TTG_Tools
{
    partial class FontEditor
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FontEditor));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newFontToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.archivePackerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.archiveUnpackerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ttarch2ScannerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quickToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsFormToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridViewWithTextures = new System.Windows.Forms.DataGridView();
            this.N = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Height = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Width = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Size = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStripExport_Import = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importDDSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportCoordinatesToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripImportFNT = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridViewWithCoord = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStripExp_imp_Coord = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exportCoordinatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importCoordinatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearExistingFntDdsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeDuplicatesCharsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelFontName = new System.Windows.Forms.Label();
            this.labelSearchChar = new System.Windows.Forms.Label();
            this.textBoxSearchChar = new System.Windows.Forms.TextBox();
            this.buttonPreviewChar = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbNoKerning = new System.Windows.Forms.RadioButton();
            this.rbKerning = new System.Windows.Forms.RadioButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.rbSwitchSwizzle = new System.Windows.Forms.RadioButton();
            this.rbPS4Swizzle = new System.Windows.Forms.RadioButton();
            this.rbXbox360Swizzle = new System.Windows.Forms.RadioButton();
            this.rbPSVitaSwizzle = new System.Windows.Forms.RadioButton();
            this.rbWiiSwizzle = new System.Windows.Forms.RadioButton();
            this.rbNoSwizzle = new System.Windows.Forms.RadioButton();
            this.pictureBoxTexturePreview = new System.Windows.Forms.PictureBox();
            this.labelTexturePreview = new System.Windows.Forms.Label();
            this.groupBoxMatchTextures = new System.Windows.Forms.GroupBox();
            this.buttonGenerateMissingChars = new System.Windows.Forms.Button();
            this.labelYoffsetAdjust = new System.Windows.Forms.Label();
            this.buttonDetectMissingTextures = new System.Windows.Forms.Button();
            this.textBoxYoffset = new System.Windows.Forms.TextBox();
            this.labelFontSizeAdjust = new System.Windows.Forms.Label();
            this.textBoxFontSizeAdjust = new System.Windows.Forms.TextBox();
            this.labelProfile = new System.Windows.Forms.Label();
            this.buttonDeleteProfile = new System.Windows.Forms.Button();
            this.buttonSaveProfile = new System.Windows.Forms.Button();
            this.labelGenFont = new System.Windows.Forms.Label();
            this.comboBoxProfiles = new System.Windows.Forms.ComboBox();
            this.buttonPickFont = new System.Windows.Forms.Button();
            this.textBoxGenFont = new System.Windows.Forms.TextBox();
            this.groupBoxFntAdjust = new System.Windows.Forms.GroupBox();
            this.labelSizeAdj = new System.Windows.Forms.Label();
            this.textBoxSizeAdj = new System.Windows.Forms.TextBox();
            this.buttonSizeApply = new System.Windows.Forms.Button();
            this.labelLineHeightAdj = new System.Windows.Forms.Label();
            this.textBoxLineHeightAdj = new System.Windows.Forms.TextBox();
            this.buttonLHApply = new System.Windows.Forms.Button();
            this.labelBaseAdj = new System.Windows.Forms.Label();
            this.textBoxBaseAdj = new System.Windows.Forms.TextBox();
            this.buttonBaseApply = new System.Windows.Forms.Button();
            this.labelHeightAdj = new System.Windows.Forms.Label();
            this.textBoxHeightAdj = new System.Windows.Forms.TextBox();
            this.buttonHeightApply = new System.Windows.Forms.Button();
            this.labelChannelAdj = new System.Windows.Forms.Label();
            this.textBoxChannelAdj = new System.Windows.Forms.TextBox();
            this.buttonChannelApply = new System.Windows.Forms.Button();
            this.buttonYoffsetUp = new System.Windows.Forms.Button();
            this.buttonYoffsetDown = new System.Windows.Forms.Button();
            this.buttonYadjUp = new System.Windows.Forms.Button();
            this.buttonYadjDown = new System.Windows.Forms.Button();
            this.labelYAdjust = new System.Windows.Forms.Label();
            this.textBoxYAdjust = new System.Windows.Forms.TextBox();
            this.buttonApplyYAdjust = new System.Windows.Forms.Button();
            this.labelYoffsetAdjust2 = new System.Windows.Forms.Label();
            this.textBoxYoffsetAdjust = new System.Windows.Forms.TextBox();
            this.buttonApplyYoffsetAdjust = new System.Windows.Forms.Button();
            this.textBoxLogOutput = new System.Windows.Forms.TextBox();
            this.buttonSaveLogAs = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewWithTextures)).BeginInit();
            this.contextMenuStripExport_Import.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewWithCoord)).BeginInit();
            this.contextMenuStripExp_imp_Coord.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTexturePreview)).BeginInit();
            this.groupBoxMatchTextures.SuspendLayout();
            this.groupBoxFntAdjust.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1373, 25);
            this.menuStrip1.TabIndex = 24;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newFontToolStripMenuItem,
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(39, 21);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newFontToolStripMenuItem
            // 
            this.newFontToolStripMenuItem.Name = "newFontToolStripMenuItem";
            this.newFontToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.newFontToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.newFontToolStripMenuItem.Text = "New Font";
            this.newFontToolStripMenuItem.Click += new System.EventHandler(this.newFontToolStripMenuItem_Click);
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.openToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.saveToolStripMenuItem.Text = "&Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.saveAsToolStripMenuItem.Text = "Save As ...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(175, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.archivePackerToolStripMenuItem,
            this.archiveUnpackerToolStripMenuItem,
            this.ttarch2ScannerToolStripMenuItem,
            this.quickToolsToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(52, 21);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // archivePackerToolStripMenuItem
            // 
            this.archivePackerToolStripMenuItem.Name = "archivePackerToolStripMenuItem";
            this.archivePackerToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.archivePackerToolStripMenuItem.Text = "Archive Packer";
            this.archivePackerToolStripMenuItem.Click += new System.EventHandler(this.archivePackerToolStripMenuItem_Click);
            // 
            // archiveUnpackerToolStripMenuItem
            // 
            this.archiveUnpackerToolStripMenuItem.Name = "archiveUnpackerToolStripMenuItem";
            this.archiveUnpackerToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.archiveUnpackerToolStripMenuItem.Text = "Archive Unpacker";
            this.archiveUnpackerToolStripMenuItem.Click += new System.EventHandler(this.archiveUnpackerToolStripMenuItem_Click);
            // 
            // ttarch2ScannerToolStripMenuItem
            // 
            this.ttarch2ScannerToolStripMenuItem.Name = "ttarch2ScannerToolStripMenuItem";
            this.ttarch2ScannerToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.ttarch2ScannerToolStripMenuItem.Text = "Ttarch2 Scanner";
            this.ttarch2ScannerToolStripMenuItem.Click += new System.EventHandler(this.ttarch2ScannerToolStripMenuItem_Click);
            // 
            // quickToolsToolStripMenuItem
            // 
            this.quickToolsToolStripMenuItem.Name = "quickToolsToolStripMenuItem";
            this.quickToolsToolStripMenuItem.Size = new System.Drawing.Size(178, 22);
            this.quickToolsToolStripMenuItem.Text = "Quick Tools...";
            this.quickToolsToolStripMenuItem.Click += new System.EventHandler(this.quickToolsToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsFormToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(66, 21);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // settingsFormToolStripMenuItem
            // 
            this.settingsFormToolStripMenuItem.Name = "settingsFormToolStripMenuItem";
            this.settingsFormToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.settingsFormToolStripMenuItem.Text = "Settings";
            this.settingsFormToolStripMenuItem.Click += new System.EventHandler(this.settingsFormToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(47, 21);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // dataGridViewWithTextures
            // 
            this.dataGridViewWithTextures.AllowUserToAddRows = false;
            this.dataGridViewWithTextures.AllowUserToDeleteRows = false;
            this.dataGridViewWithTextures.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewWithTextures.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewWithTextures.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewWithTextures.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.N,
            this.Height,
            this.Width,
            this.Size});
            this.dataGridViewWithTextures.ContextMenuStrip = this.contextMenuStripExport_Import;
            this.dataGridViewWithTextures.Location = new System.Drawing.Point(222, 338);
            this.dataGridViewWithTextures.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dataGridViewWithTextures.Name = "dataGridViewWithTextures";
            this.dataGridViewWithTextures.ReadOnly = true;
            this.dataGridViewWithTextures.RowHeadersWidth = 51;
            this.dataGridViewWithTextures.Size = new System.Drawing.Size(629, 246);
            this.dataGridViewWithTextures.TabIndex = 26;
            this.dataGridViewWithTextures.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridViewWithTextures_CellMouseClick);
            this.dataGridViewWithTextures.RowContextMenuStripNeeded += new System.Windows.Forms.DataGridViewRowContextMenuStripNeededEventHandler(this.dataGridViewWithTextures_RowContextMenuStripNeeded);
            this.dataGridViewWithTextures.SelectionChanged += new System.EventHandler(this.dataGridViewWithTextures_SelectionChanged);
            // 
            // N
            // 
            this.N.HeaderText = "№";
            this.N.MinimumWidth = 10;
            this.N.Name = "N";
            this.N.ReadOnly = true;
            // 
            // Height
            // 
            this.Height.HeaderText = "Height";
            this.Height.MinimumWidth = 10;
            this.Height.Name = "Height";
            this.Height.ReadOnly = true;
            // 
            // Width
            // 
            this.Width.HeaderText = "Width";
            this.Width.MinimumWidth = 10;
            this.Width.Name = "Width";
            this.Width.ReadOnly = true;
            // 
            // Size
            // 
            this.Size.HeaderText = "Size";
            this.Size.MinimumWidth = 10;
            this.Size.Name = "Size";
            this.Size.ReadOnly = true;
            // 
            // contextMenuStripExport_Import
            // 
            this.contextMenuStripExport_Import.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStripExport_Import.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToolStripMenuItem,
            this.importDDSToolStripMenuItem,
            this.exportCoordinatesToolStripMenuItem1,
            this.toolStripImportFNT});
            this.contextMenuStripExport_Import.Name = "contextMenuStripExport_Import";
            this.contextMenuStripExport_Import.Size = new System.Drawing.Size(478, 92);
            this.contextMenuStripExport_Import.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripExport_Import_Opening);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Enabled = false;
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(477, 22);
            this.exportToolStripMenuItem.Text = "Export texture";
            this.exportToolStripMenuItem.Click += new System.EventHandler(this.exportToolStripMenuItem_Click);
            // 
            // importDDSToolStripMenuItem
            // 
            this.importDDSToolStripMenuItem.Enabled = false;
            this.importDDSToolStripMenuItem.Name = "importDDSToolStripMenuItem";
            this.importDDSToolStripMenuItem.Size = new System.Drawing.Size(477, 22);
            this.importDDSToolStripMenuItem.Text = "Import texture";
            this.importDDSToolStripMenuItem.Click += new System.EventHandler(this.importDDSToolStripMenuItem_Click);
            // 
            // exportCoordinatesToolStripMenuItem1
            // 
            this.exportCoordinatesToolStripMenuItem1.Enabled = false;
            this.exportCoordinatesToolStripMenuItem1.Name = "exportCoordinatesToolStripMenuItem1";
            this.exportCoordinatesToolStripMenuItem1.Size = new System.Drawing.Size(477, 22);
            this.exportCoordinatesToolStripMenuItem1.Text = "Export coordinates";
            this.exportCoordinatesToolStripMenuItem1.Click += new System.EventHandler(this.exportCoordinatesToolStripMenuItem1_Click);
            // 
            // toolStripImportFNT
            // 
            this.toolStripImportFNT.Name = "toolStripImportFNT";
            this.toolStripImportFNT.Size = new System.Drawing.Size(477, 22);
            this.toolStripImportFNT.Text = "Import coordinates from Bitmap Font Generator (*.fnt XML-like type)";
            this.toolStripImportFNT.Click += new System.EventHandler(this.toolStripImportFNT_Click);
            // 
            // dataGridViewWithCoord
            // 
            this.dataGridViewWithCoord.AllowUserToAddRows = false;
            this.dataGridViewWithCoord.AllowUserToDeleteRows = false;
            this.dataGridViewWithCoord.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewWithCoord.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewWithCoord.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewWithCoord.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewWithCoord.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column3,
            this.Column4,
            this.Column5,
            this.Column6,
            this.Column7,
            this.Column8,
            this.Column9,
            this.Column10,
            this.Column11,
            this.Column12,
            this.Column13});
            this.dataGridViewWithCoord.Location = new System.Drawing.Point(222, 28);
            this.dataGridViewWithCoord.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.dataGridViewWithCoord.Name = "dataGridViewWithCoord";
            this.dataGridViewWithCoord.RowHeadersWidth = 51;
            this.dataGridViewWithCoord.Size = new System.Drawing.Size(1147, 282);
            this.dataGridViewWithCoord.TabIndex = 27;
            this.dataGridViewWithCoord.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dataGridViewWithCoord_CellBeginEdit);
            this.dataGridViewWithCoord.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewWithCoord_CellEndEdit);
            this.dataGridViewWithCoord.CellMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridViewWithCoord_CellMouseClick);
            this.dataGridViewWithCoord.SelectionChanged += new System.EventHandler(this.dataGridViewWithCoord_SelectionChanged);
            // 
            // Column1
            // 
            this.Column1.HeaderText = "№";
            this.Column1.MinimumWidth = 6;
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "Char";
            this.Column2.MinimumWidth = 6;
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            // 
            // Column3
            // 
            this.Column3.HeaderText = "X start";
            this.Column3.MinimumWidth = 6;
            this.Column3.Name = "Column3";
            // 
            // Column4
            // 
            this.Column4.HeaderText = "X end";
            this.Column4.MinimumWidth = 6;
            this.Column4.Name = "Column4";
            // 
            // Column5
            // 
            this.Column5.HeaderText = "Y start";
            this.Column5.MinimumWidth = 6;
            this.Column5.Name = "Column5";
            // 
            // Column6
            // 
            this.Column6.HeaderText = "Y end";
            this.Column6.MinimumWidth = 6;
            this.Column6.Name = "Column6";
            // 
            // Column7
            // 
            this.Column7.HeaderText = "№ dds";
            this.Column7.MinimumWidth = 6;
            this.Column7.Name = "Column7";
            // 
            // Column8
            // 
            this.Column8.HeaderText = "Width";
            this.Column8.MinimumWidth = 6;
            this.Column8.Name = "Column8";
            this.Column8.ReadOnly = true;
            // 
            // Column9
            // 
            this.Column9.HeaderText = "Height";
            this.Column9.MinimumWidth = 6;
            this.Column9.Name = "Column9";
            this.Column9.ReadOnly = true;
            // 
            // Column10
            // 
            this.Column10.HeaderText = "Channel";
            this.Column10.MinimumWidth = 6;
            this.Column10.Name = "Column10";
            // 
            // Column11
            // 
            this.Column11.HeaderText = "X offset";
            this.Column11.MinimumWidth = 6;
            this.Column11.Name = "Column11";
            // 
            // Column12
            // 
            this.Column12.HeaderText = "Y offset";
            this.Column12.MinimumWidth = 6;
            this.Column12.Name = "Column12";
            // 
            // Column13
            // 
            this.Column13.HeaderText = "X advanced";
            this.Column13.MinimumWidth = 6;
            this.Column13.Name = "Column13";
            // 
            // contextMenuStripExp_imp_Coord
            // 
            this.contextMenuStripExp_imp_Coord.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStripExp_imp_Coord.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportCoordinatesToolStripMenuItem,
            this.importCoordinatesToolStripMenuItem,
            this.clearExistingFntDdsToolStripMenuItem,
            this.removeDuplicatesCharsToolStripMenuItem});
            this.contextMenuStripExp_imp_Coord.Name = "contextMenuStripExp_imp_Coord";
            this.contextMenuStripExp_imp_Coord.Size = new System.Drawing.Size(222, 92);
            // 
            // exportCoordinatesToolStripMenuItem
            // 
            this.exportCoordinatesToolStripMenuItem.Name = "exportCoordinatesToolStripMenuItem";
            this.exportCoordinatesToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.exportCoordinatesToolStripMenuItem.Text = "Export coordinates";
            this.exportCoordinatesToolStripMenuItem.Click += new System.EventHandler(this.exportCoordinatesToolStripMenuItem_Click);
            // 
            // importCoordinatesToolStripMenuItem
            // 
            this.importCoordinatesToolStripMenuItem.Name = "importCoordinatesToolStripMenuItem";
            this.importCoordinatesToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.importCoordinatesToolStripMenuItem.Text = "Import coordinates";
            this.importCoordinatesToolStripMenuItem.Click += new System.EventHandler(this.importCoordinatesToolStripMenuItem_Click);
            // 
            // clearExistingFntDdsToolStripMenuItem
            // 
            this.clearExistingFntDdsToolStripMenuItem.Name = "clearExistingFntDdsToolStripMenuItem";
            this.clearExistingFntDdsToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.clearExistingFntDdsToolStripMenuItem.Text = "Clear Existing FNT+DDS";
            this.clearExistingFntDdsToolStripMenuItem.Click += new System.EventHandler(this.clearExistingFntDdsToolStripMenuItem_Click);
            // 
            // removeDuplicatesCharsToolStripMenuItem
            // 
            this.removeDuplicatesCharsToolStripMenuItem.Name = "removeDuplicatesCharsToolStripMenuItem";
            this.removeDuplicatesCharsToolStripMenuItem.Size = new System.Drawing.Size(221, 22);
            this.removeDuplicatesCharsToolStripMenuItem.Text = "Remove duplicates chars";
            this.removeDuplicatesCharsToolStripMenuItem.Click += new System.EventHandler(this.removeDuplicatesCharsToolStripMenuItem_Click);
            // 
            // labelFontName
            // 
            this.labelFontName.AutoSize = true;
            this.labelFontName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.labelFontName.Location = new System.Drawing.Point(7, 87);
            this.labelFontName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelFontName.Name = "labelFontName";
            this.labelFontName.Size = new System.Drawing.Size(54, 13);
            this.labelFontName.TabIndex = 2;
            this.labelFontName.Text = "Font: N/A";
            // 
            // labelSearchChar
            // 
            this.labelSearchChar.AutoSize = true;
            this.labelSearchChar.Location = new System.Drawing.Point(7, 108);
            this.labelSearchChar.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSearchChar.Name = "labelSearchChar";
            this.labelSearchChar.Size = new System.Drawing.Size(70, 13);
            this.labelSearchChar.TabIndex = 3;
            this.labelSearchChar.Text = "Search Char:";
            // 
            // textBoxSearchChar
            // 
            this.textBoxSearchChar.Location = new System.Drawing.Point(7, 126);
            this.textBoxSearchChar.Name = "textBoxSearchChar";
            this.textBoxSearchChar.Size = new System.Drawing.Size(70, 21);
            this.textBoxSearchChar.TabIndex = 4;
            this.textBoxSearchChar.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxSearchChar_KeyDown);
            // 
            // buttonPreviewChar
            // 
            this.buttonPreviewChar.Location = new System.Drawing.Point(83, 122);
            this.buttonPreviewChar.Name = "buttonPreviewChar";
            this.buttonPreviewChar.Size = new System.Drawing.Size(75, 25);
            this.buttonPreviewChar.TabIndex = 5;
            this.buttonPreviewChar.Text = "Preview";
            this.buttonPreviewChar.UseVisualStyleBackColor = true;
            this.buttonPreviewChar.Click += new System.EventHandler(this.buttonPreviewChar_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.labelSearchChar);
            this.groupBox2.Controls.Add(this.textBoxSearchChar);
            this.groupBox2.Controls.Add(this.buttonPreviewChar);
            this.groupBox2.Controls.Add(this.labelFontName);
            this.groupBox2.Controls.Add(this.rbNoKerning);
            this.groupBox2.Controls.Add(this.rbKerning);
            this.groupBox2.Location = new System.Drawing.Point(13, 28);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox2.Size = new System.Drawing.Size(185, 150);
            this.groupBox2.TabIndex = 28;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Font Info:";
            // 
            // rbNoKerning
            // 
            this.rbNoKerning.AutoSize = true;
            this.rbNoKerning.Location = new System.Drawing.Point(7, 59);
            this.rbNoKerning.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbNoKerning.Name = "rbNoKerning";
            this.rbNoKerning.Size = new System.Drawing.Size(100, 17);
            this.rbNoKerning.TabIndex = 1;
            this.rbNoKerning.Text = "without Kerning";
            this.rbNoKerning.UseVisualStyleBackColor = true;
            // 
            // rbKerning
            // 
            this.rbKerning.AutoSize = true;
            this.rbKerning.Checked = true;
            this.rbKerning.Location = new System.Drawing.Point(7, 36);
            this.rbKerning.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.rbKerning.Name = "rbKerning";
            this.rbKerning.Size = new System.Drawing.Size(84, 17);
            this.rbKerning.TabIndex = 0;
            this.rbKerning.TabStop = true;
            this.rbKerning.Text = "with Kerning";
            this.rbKerning.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.rbSwitchSwizzle);
            this.groupBox4.Controls.Add(this.rbPS4Swizzle);
            this.groupBox4.Controls.Add(this.rbXbox360Swizzle);
            this.groupBox4.Controls.Add(this.rbPSVitaSwizzle);
            this.groupBox4.Controls.Add(this.rbWiiSwizzle);
            this.groupBox4.Controls.Add(this.rbNoSwizzle);
            this.groupBox4.Location = new System.Drawing.Point(7, 706);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(186, 153);
            this.groupBox4.TabIndex = 30;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Swizzle methods (Save)";
            // 
            // rbSwitchSwizzle
            // 
            this.rbSwitchSwizzle.AutoSize = true;
            this.rbSwitchSwizzle.Location = new System.Drawing.Point(7, 59);
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
            this.rbPS4Swizzle.Location = new System.Drawing.Point(7, 36);
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
            this.rbXbox360Swizzle.Location = new System.Drawing.Point(7, 82);
            this.rbXbox360Swizzle.Name = "rbXbox360Swizzle";
            this.rbXbox360Swizzle.Size = new System.Drawing.Size(70, 17);
            this.rbXbox360Swizzle.TabIndex = 3;
            this.rbXbox360Swizzle.TabStop = true;
            this.rbXbox360Swizzle.Text = "Xbox 360";
            this.rbXbox360Swizzle.UseVisualStyleBackColor = true;
            this.rbXbox360Swizzle.CheckedChanged += new System.EventHandler(this.rbXbox360Swizzle_CheckedChanged);
            // 
            // rbPSVitaSwizzle
            // 
            this.rbPSVitaSwizzle.AutoSize = true;
            this.rbPSVitaSwizzle.Location = new System.Drawing.Point(7, 103);
            this.rbPSVitaSwizzle.Name = "rbPSVitaSwizzle";
            this.rbPSVitaSwizzle.Size = new System.Drawing.Size(58, 17);
            this.rbPSVitaSwizzle.TabIndex = 4;
            this.rbPSVitaSwizzle.TabStop = true;
            this.rbPSVitaSwizzle.Text = "PS Vita";
            this.rbPSVitaSwizzle.UseVisualStyleBackColor = true;
            this.rbPSVitaSwizzle.CheckedChanged += new System.EventHandler(this.rbPSVitaSwizzle_CheckedChanged);
            // 
            // rbWiiSwizzle
            // 
            this.rbWiiSwizzle.AutoSize = true;
            this.rbWiiSwizzle.Location = new System.Drawing.Point(7, 126);
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
            this.rbNoSwizzle.Location = new System.Drawing.Point(7, 18);
            this.rbNoSwizzle.Name = "rbNoSwizzle";
            this.rbNoSwizzle.Size = new System.Drawing.Size(50, 17);
            this.rbNoSwizzle.TabIndex = 0;
            this.rbNoSwizzle.TabStop = true;
            this.rbNoSwizzle.Text = "None";
            this.rbNoSwizzle.UseVisualStyleBackColor = true;
            this.rbNoSwizzle.CheckedChanged += new System.EventHandler(this.rbNoSwizzle_CheckedChanged);
            // 
            // pictureBoxTexturePreview
            // 
            this.pictureBoxTexturePreview.BackColor = System.Drawing.Color.Black;
            this.pictureBoxTexturePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxTexturePreview.Location = new System.Drawing.Point(857, 347);
            this.pictureBoxTexturePreview.Name = "pictureBoxTexturePreview";
            this.pictureBoxTexturePreview.Size = new System.Drawing.Size(512, 512);
            this.pictureBoxTexturePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxTexturePreview.TabIndex = 31;
            this.pictureBoxTexturePreview.TabStop = false;
            // 
            // labelTexturePreview
            // 
            this.labelTexturePreview.AutoSize = true;
            this.labelTexturePreview.Location = new System.Drawing.Point(854, 328);
            this.labelTexturePreview.Name = "labelTexturePreview";
            this.labelTexturePreview.Size = new System.Drawing.Size(143, 13);
            this.labelTexturePreview.TabIndex = 32;
            this.labelTexturePreview.Text = "Texture preview (read-only)";
            // 
            // groupBoxMatchTextures
            // 
            this.groupBoxMatchTextures.Controls.Add(this.buttonGenerateMissingChars);
            this.groupBoxMatchTextures.Controls.Add(this.labelYoffsetAdjust);
            this.groupBoxMatchTextures.Controls.Add(this.buttonDetectMissingTextures);
            this.groupBoxMatchTextures.Controls.Add(this.textBoxYoffset);
            this.groupBoxMatchTextures.Controls.Add(this.labelFontSizeAdjust);
            this.groupBoxMatchTextures.Controls.Add(this.textBoxFontSizeAdjust);
            this.groupBoxMatchTextures.Controls.Add(this.labelProfile);
            this.groupBoxMatchTextures.Controls.Add(this.buttonDeleteProfile);
            this.groupBoxMatchTextures.Controls.Add(this.buttonSaveProfile);
            this.groupBoxMatchTextures.Controls.Add(this.labelGenFont);
            this.groupBoxMatchTextures.Controls.Add(this.comboBoxProfiles);
            this.groupBoxMatchTextures.Controls.Add(this.buttonPickFont);
            this.groupBoxMatchTextures.Controls.Add(this.textBoxGenFont);
            this.groupBoxMatchTextures.Location = new System.Drawing.Point(13, 184);
            this.groupBoxMatchTextures.Name = "groupBoxMatchTextures";
            this.groupBoxMatchTextures.Size = new System.Drawing.Size(185, 226);
            this.groupBoxMatchTextures.TabIndex = 33;
            this.groupBoxMatchTextures.TabStop = false;
            this.groupBoxMatchTextures.Text = "Match Textures";
            // 
            // buttonGenerateMissingChars
            // 
            this.buttonGenerateMissingChars.Location = new System.Drawing.Point(6, 189);
            this.buttonGenerateMissingChars.Name = "buttonGenerateMissingChars";
            this.buttonGenerateMissingChars.Size = new System.Drawing.Size(172, 31);
            this.buttonGenerateMissingChars.TabIndex = 1;
            this.buttonGenerateMissingChars.Text = "Generate Missing Textures";
            this.buttonGenerateMissingChars.UseVisualStyleBackColor = true;
            this.buttonGenerateMissingChars.Click += new System.EventHandler(this.buttonGenerateMissingChars_Click);
            // 
            // labelYoffsetAdjust
            // 
            this.labelYoffsetAdjust.AutoSize = true;
            this.labelYoffsetAdjust.Location = new System.Drawing.Point(7, 57);
            this.labelYoffsetAdjust.Name = "labelYoffsetAdjust";
            this.labelYoffsetAdjust.Size = new System.Drawing.Size(72, 13);
            this.labelYoffsetAdjust.TabIndex = 2;
            this.labelYoffsetAdjust.Text = "DDS Y offset:";
            this.labelYoffsetAdjust.Click += new System.EventHandler(this.labelYoffsetAdjust_Click);
            // 
            // buttonDetectMissingTextures
            // 
            this.buttonDetectMissingTextures.Location = new System.Drawing.Point(5, 152);
            this.buttonDetectMissingTextures.Name = "buttonDetectMissingTextures";
            this.buttonDetectMissingTextures.Size = new System.Drawing.Size(172, 31);
            this.buttonDetectMissingTextures.TabIndex = 0;
            this.buttonDetectMissingTextures.Text = "Detect Missing Chars";
            this.buttonDetectMissingTextures.UseVisualStyleBackColor = true;
            this.buttonDetectMissingTextures.Click += new System.EventHandler(this.buttonDetectMissingTextures_Click);
            // 
            // textBoxYoffset
            // 
            this.textBoxYoffset.Location = new System.Drawing.Point(106, 54);
            this.textBoxYoffset.Name = "textBoxYoffset";
            this.textBoxYoffset.Size = new System.Drawing.Size(73, 21);
            this.textBoxYoffset.TabIndex = 3;
            this.textBoxYoffset.Text = "2";
            // 
            // labelFontSizeAdjust
            // 
            this.labelFontSizeAdjust.AutoSize = true;
            this.labelFontSizeAdjust.Location = new System.Drawing.Point(6, 84);
            this.labelFontSizeAdjust.Name = "labelFontSizeAdjust";
            this.labelFontSizeAdjust.Size = new System.Drawing.Size(72, 13);
            this.labelFontSizeAdjust.TabIndex = 4;
            this.labelFontSizeAdjust.Text = "Font size adj:";
            // 
            // textBoxFontSizeAdjust
            // 
            this.textBoxFontSizeAdjust.Location = new System.Drawing.Point(106, 81);
            this.textBoxFontSizeAdjust.Name = "textBoxFontSizeAdjust";
            this.textBoxFontSizeAdjust.Size = new System.Drawing.Size(74, 21);
            this.textBoxFontSizeAdjust.TabIndex = 5;
            this.textBoxFontSizeAdjust.Text = "0";
            // 
            // labelProfile
            // 
            this.labelProfile.AutoSize = true;
            this.labelProfile.Location = new System.Drawing.Point(4, 17);
            this.labelProfile.Name = "labelProfile";
            this.labelProfile.Size = new System.Drawing.Size(41, 13);
            this.labelProfile.TabIndex = 6;
            this.labelProfile.Text = "Profile:";
            // 
            // buttonDeleteProfile
            // 
            this.buttonDeleteProfile.Location = new System.Drawing.Point(142, 32);
            this.buttonDeleteProfile.Name = "buttonDeleteProfile";
            this.buttonDeleteProfile.Size = new System.Drawing.Size(36, 21);
            this.buttonDeleteProfile.TabIndex = 9;
            this.buttonDeleteProfile.Text = "Del";
            this.buttonDeleteProfile.UseVisualStyleBackColor = true;
            this.buttonDeleteProfile.Click += new System.EventHandler(this.buttonDeleteProfile_Click);
            // 
            // buttonSaveProfile
            // 
            this.buttonSaveProfile.Location = new System.Drawing.Point(106, 32);
            this.buttonSaveProfile.Name = "buttonSaveProfile";
            this.buttonSaveProfile.Size = new System.Drawing.Size(36, 21);
            this.buttonSaveProfile.TabIndex = 8;
            this.buttonSaveProfile.Text = "Save";
            this.buttonSaveProfile.UseVisualStyleBackColor = true;
            this.buttonSaveProfile.Click += new System.EventHandler(this.buttonSaveProfile_Click);
            // 
            // labelGenFont
            // 
            this.labelGenFont.AutoSize = true;
            this.labelGenFont.Location = new System.Drawing.Point(6, 109);
            this.labelGenFont.Name = "labelGenFont";
            this.labelGenFont.Size = new System.Drawing.Size(33, 13);
            this.labelGenFont.TabIndex = 10;
            this.labelGenFont.Text = "Font:";
            // 
            // comboBoxProfiles
            // 
            this.comboBoxProfiles.DisplayMember = "Name";
            this.comboBoxProfiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxProfiles.Location = new System.Drawing.Point(4, 33);
            this.comboBoxProfiles.Name = "comboBoxProfiles";
            this.comboBoxProfiles.Size = new System.Drawing.Size(96, 21);
            this.comboBoxProfiles.TabIndex = 7;
            this.comboBoxProfiles.SelectedIndexChanged += new System.EventHandler(this.comboBoxProfiles_SelectedIndexChanged);
            // 
            // buttonPickFont
            // 
            this.buttonPickFont.Location = new System.Drawing.Point(129, 125);
            this.buttonPickFont.Name = "buttonPickFont";
            this.buttonPickFont.Size = new System.Drawing.Size(51, 22);
            this.buttonPickFont.TabIndex = 12;
            this.buttonPickFont.Text = "Choose";
            this.buttonPickFont.UseVisualStyleBackColor = true;
            this.buttonPickFont.Click += new System.EventHandler(this.buttonPickFont_Click);
            // 
            // textBoxGenFont
            // 
            this.textBoxGenFont.Location = new System.Drawing.Point(5, 125);
            this.textBoxGenFont.Name = "textBoxGenFont";
            this.textBoxGenFont.ReadOnly = true;
            this.textBoxGenFont.Size = new System.Drawing.Size(116, 21);
            this.textBoxGenFont.TabIndex = 11;
            // 
            // groupBoxFntAdjust
            // 
            this.groupBoxFntAdjust.Controls.Add(this.labelSizeAdj);
            this.groupBoxFntAdjust.Controls.Add(this.textBoxSizeAdj);
            this.groupBoxFntAdjust.Controls.Add(this.buttonSizeApply);
            this.groupBoxFntAdjust.Controls.Add(this.labelLineHeightAdj);
            this.groupBoxFntAdjust.Controls.Add(this.textBoxLineHeightAdj);
            this.groupBoxFntAdjust.Controls.Add(this.buttonLHApply);
            this.groupBoxFntAdjust.Controls.Add(this.labelBaseAdj);
            this.groupBoxFntAdjust.Controls.Add(this.textBoxBaseAdj);
            this.groupBoxFntAdjust.Controls.Add(this.buttonBaseApply);
            this.groupBoxFntAdjust.Controls.Add(this.labelHeightAdj);
            this.groupBoxFntAdjust.Controls.Add(this.textBoxHeightAdj);
            this.groupBoxFntAdjust.Controls.Add(this.buttonHeightApply);
            this.groupBoxFntAdjust.Controls.Add(this.labelChannelAdj);
            this.groupBoxFntAdjust.Controls.Add(this.textBoxChannelAdj);
            this.groupBoxFntAdjust.Controls.Add(this.buttonChannelApply);
            this.groupBoxFntAdjust.Controls.Add(this.labelYAdjust);
            this.groupBoxFntAdjust.Controls.Add(this.textBoxYAdjust);
            this.groupBoxFntAdjust.Controls.Add(this.buttonYadjUp);
            this.groupBoxFntAdjust.Controls.Add(this.buttonYadjDown);
            this.groupBoxFntAdjust.Controls.Add(this.labelYoffsetAdjust2);
            this.groupBoxFntAdjust.Controls.Add(this.textBoxYoffsetAdjust);
            this.groupBoxFntAdjust.Controls.Add(this.buttonYoffsetUp);
            this.groupBoxFntAdjust.Controls.Add(this.buttonYoffsetDown);
            this.groupBoxFntAdjust.Location = new System.Drawing.Point(13, 416);
            this.groupBoxFntAdjust.Name = "groupBoxFntAdjust";
            this.groupBoxFntAdjust.Size = new System.Drawing.Size(185, 230);
            this.groupBoxFntAdjust.TabIndex = 35;
            this.groupBoxFntAdjust.TabStop = false;
            this.groupBoxFntAdjust.Text = "FNT Adjust";
            // 
            // labelYAdjust
            // 
            this.labelYAdjust.AutoSize = true;
            this.labelYAdjust.Location = new System.Drawing.Point(7, 190);
            this.labelYAdjust.Name = "labelYAdjust";
            this.labelYAdjust.Size = new System.Drawing.Size(36, 13);
            this.labelYAdjust.TabIndex = 3;
            this.labelYAdjust.Text = "Y Adj:";
            // 
            // textBoxYAdjust
            // 
            this.textBoxYAdjust.Location = new System.Drawing.Point(60, 187);
            this.textBoxYAdjust.Name = "textBoxYAdjust";
            this.textBoxYAdjust.Size = new System.Drawing.Size(44, 21);
            this.textBoxYAdjust.TabIndex = 4;
            this.textBoxYAdjust.Text = "20";
            // 
            // buttonApplyYAdjust
            // 
            this.buttonApplyYAdjust.Location = new System.Drawing.Point(134, 42);
            this.buttonApplyYAdjust.Name = "buttonApplyYAdjust";
            this.buttonApplyYAdjust.Size = new System.Drawing.Size(24, 23);
            this.buttonApplyYAdjust.TabIndex = 5;
            this.buttonApplyYAdjust.Text = ">";
            this.buttonApplyYAdjust.UseVisualStyleBackColor = true;
            this.buttonApplyYAdjust.Click += new System.EventHandler(this.buttonApplyYAdjust_Click);
            // 
            // labelSizeAdj
            // 
            this.labelSizeAdj.AutoSize = true;
            this.labelSizeAdj.Location = new System.Drawing.Point(7, 20);
            this.labelSizeAdj.Name = "labelSizeAdj";
            this.labelSizeAdj.Size = new System.Drawing.Size(33, 13);
            this.labelSizeAdj.TabIndex = 10;
            this.labelSizeAdj.Text = "Size:";
            // 
            // textBoxSizeAdj
            // 
            this.textBoxSizeAdj.Location = new System.Drawing.Point(60, 17);
            this.textBoxSizeAdj.Name = "textBoxSizeAdj";
            this.textBoxSizeAdj.Size = new System.Drawing.Size(50, 21);
            this.textBoxSizeAdj.TabIndex = 11;
            this.textBoxSizeAdj.Text = "0";
            // 
            // buttonSizeApply
            // 
            this.buttonSizeApply.Location = new System.Drawing.Point(116, 16);
            this.buttonSizeApply.Name = "buttonSizeApply";
            this.buttonSizeApply.Size = new System.Drawing.Size(60, 23);
            this.buttonSizeApply.TabIndex = 12;
            this.buttonSizeApply.Text = "Apply";
            this.buttonSizeApply.UseVisualStyleBackColor = true;
            this.buttonSizeApply.Click += new System.EventHandler(this.buttonSizeApply_Click);
            // 
            // labelLineHeightAdj
            // 
            this.labelLineHeightAdj.AutoSize = true;
            this.labelLineHeightAdj.Location = new System.Drawing.Point(7, 46);
            this.labelLineHeightAdj.Name = "labelLineHeightAdj";
            this.labelLineHeightAdj.Size = new System.Drawing.Size(49, 13);
            this.labelLineHeightAdj.TabIndex = 13;
            this.labelLineHeightAdj.Text = "LineHgt:";
            // 
            // textBoxLineHeightAdj
            // 
            this.textBoxLineHeightAdj.Location = new System.Drawing.Point(60, 43);
            this.textBoxLineHeightAdj.Name = "textBoxLineHeightAdj";
            this.textBoxLineHeightAdj.Size = new System.Drawing.Size(50, 21);
            this.textBoxLineHeightAdj.TabIndex = 14;
            this.textBoxLineHeightAdj.Text = "0";
            // 
            // buttonLHApply
            // 
            this.buttonLHApply.Location = new System.Drawing.Point(116, 42);
            this.buttonLHApply.Name = "buttonLHApply";
            this.buttonLHApply.Size = new System.Drawing.Size(60, 23);
            this.buttonLHApply.TabIndex = 15;
            this.buttonLHApply.Text = "Apply";
            this.buttonLHApply.UseVisualStyleBackColor = true;
            this.buttonLHApply.Click += new System.EventHandler(this.buttonLHApply_Click);
            // 
            // labelHeightAdj
            // 
            this.labelHeightAdj.AutoSize = true;
            this.labelHeightAdj.Location = new System.Drawing.Point(7, 133);
            this.labelHeightAdj.Name = "labelHeightAdj";
            this.labelHeightAdj.Size = new System.Drawing.Size(43, 13);
            this.labelHeightAdj.TabIndex = 16;
            this.labelHeightAdj.Text = "Height:";
            // 
            // textBoxHeightAdj
            // 
            this.textBoxHeightAdj.Location = new System.Drawing.Point(60, 130);
            this.textBoxHeightAdj.Name = "textBoxHeightAdj";
            this.textBoxHeightAdj.Size = new System.Drawing.Size(50, 21);
            this.textBoxHeightAdj.TabIndex = 17;
            this.textBoxHeightAdj.Text = "0";
            // 
            // buttonHeightApply
            // 
            this.buttonHeightApply.Location = new System.Drawing.Point(116, 129);
            this.buttonHeightApply.Name = "buttonHeightApply";
            this.buttonHeightApply.Size = new System.Drawing.Size(60, 23);
            this.buttonHeightApply.TabIndex = 18;
            this.buttonHeightApply.Text = "Apply";
            this.buttonHeightApply.UseVisualStyleBackColor = true;
            this.buttonHeightApply.Click += new System.EventHandler(this.buttonHeightApply_Click);
            // 
            // labelChannelAdj
            // 
            this.labelChannelAdj.AutoSize = true;
            this.labelChannelAdj.Location = new System.Drawing.Point(7, 104);
            this.labelChannelAdj.Name = "labelChannelAdj";
            this.labelChannelAdj.Size = new System.Drawing.Size(47, 13);
            this.labelChannelAdj.TabIndex = 19;
            this.labelChannelAdj.Text = "Chnl:";
            // 
            // textBoxChannelAdj
            // 
            this.textBoxChannelAdj.Location = new System.Drawing.Point(60, 101);
            this.textBoxChannelAdj.Name = "textBoxChannelAdj";
            this.textBoxChannelAdj.Size = new System.Drawing.Size(50, 21);
            this.textBoxChannelAdj.TabIndex = 20;
            this.textBoxChannelAdj.Text = "0";
            // 
            // buttonChannelApply
            // 
            this.buttonChannelApply.Location = new System.Drawing.Point(116, 100);
            this.buttonChannelApply.Name = "buttonChannelApply";
            this.buttonChannelApply.Size = new System.Drawing.Size(60, 23);
            this.buttonChannelApply.TabIndex = 21;
            this.buttonChannelApply.Text = "Apply";
            this.buttonChannelApply.UseVisualStyleBackColor = true;
            this.buttonChannelApply.Click += new System.EventHandler(this.buttonChannelApply_Click);
            // 
            // labelBaseAdj
            // 
            this.labelBaseAdj.AutoSize = true;
            this.labelBaseAdj.Location = new System.Drawing.Point(7, 75);
            this.labelBaseAdj.Name = "labelBaseAdj";
            this.labelBaseAdj.Size = new System.Drawing.Size(37, 13);
            this.labelBaseAdj.TabIndex = 26;
            this.labelBaseAdj.Text = "Base:";
            // 
            // textBoxBaseAdj
            // 
            this.textBoxBaseAdj.Location = new System.Drawing.Point(60, 72);
            this.textBoxBaseAdj.Name = "textBoxBaseAdj";
            this.textBoxBaseAdj.Size = new System.Drawing.Size(50, 21);
            this.textBoxBaseAdj.TabIndex = 27;
            this.textBoxBaseAdj.Text = "0";
            // 
            // buttonBaseApply
            // 
            this.buttonBaseApply.Location = new System.Drawing.Point(116, 71);
            this.buttonBaseApply.Name = "buttonBaseApply";
            this.buttonBaseApply.Size = new System.Drawing.Size(60, 23);
            this.buttonBaseApply.TabIndex = 28;
            this.buttonBaseApply.Text = "Apply";
            this.buttonBaseApply.UseVisualStyleBackColor = true;
            this.buttonBaseApply.Click += new System.EventHandler(this.buttonBaseApply_Click);
            // 
            // buttonYoffsetUp
            // 
            this.buttonYoffsetUp.Location = new System.Drawing.Point(116, 158);
            this.buttonYoffsetUp.Name = "buttonYoffsetUp";
            this.buttonYoffsetUp.Size = new System.Drawing.Size(30, 23);
            this.buttonYoffsetUp.TabIndex = 22;
            this.buttonYoffsetUp.Text = "▲";
            this.buttonYoffsetUp.UseVisualStyleBackColor = true;
            this.buttonYoffsetUp.Click += new System.EventHandler(this.buttonYoffsetUp_Click);
            // 
            // buttonYoffsetDown
            // 
            this.buttonYoffsetDown.Location = new System.Drawing.Point(150, 158);
            this.buttonYoffsetDown.Name = "buttonYoffsetDown";
            this.buttonYoffsetDown.Size = new System.Drawing.Size(30, 23);
            this.buttonYoffsetDown.TabIndex = 23;
            this.buttonYoffsetDown.Text = "▼";
            this.buttonYoffsetDown.UseVisualStyleBackColor = true;
            this.buttonYoffsetDown.Click += new System.EventHandler(this.buttonYoffsetDown_Click);
            // 
            // buttonYadjUp
            // 
            this.buttonYadjUp.Location = new System.Drawing.Point(116, 186);
            this.buttonYadjUp.Name = "buttonYadjUp";
            this.buttonYadjUp.Size = new System.Drawing.Size(30, 23);
            this.buttonYadjUp.TabIndex = 24;
            this.buttonYadjUp.Text = "▲";
            this.buttonYadjUp.UseVisualStyleBackColor = true;
            this.buttonYadjUp.Click += new System.EventHandler(this.buttonYadjUp_Click);
            // 
            // buttonYadjDown
            // 
            this.buttonYadjDown.Location = new System.Drawing.Point(150, 186);
            this.buttonYadjDown.Name = "buttonYadjDown";
            this.buttonYadjDown.Size = new System.Drawing.Size(30, 23);
            this.buttonYadjDown.TabIndex = 25;
            this.buttonYadjDown.Text = "▼";
            this.buttonYadjDown.UseVisualStyleBackColor = true;
            this.buttonYadjDown.Click += new System.EventHandler(this.buttonYadjDown_Click);
            // 
            // labelYoffsetAdjust2
            // 
            this.labelYoffsetAdjust2.AutoSize = true;
            this.labelYoffsetAdjust2.Location = new System.Drawing.Point(7, 163);
            this.labelYoffsetAdjust2.Name = "labelYoffsetAdjust2";
            this.labelYoffsetAdjust2.Size = new System.Drawing.Size(39, 13);
            this.labelYoffsetAdjust2.TabIndex = 0;
            this.labelYoffsetAdjust2.Text = "YOff:";
            // 
            // textBoxYoffsetAdjust
            // 
            this.textBoxYoffsetAdjust.Location = new System.Drawing.Point(60, 160);
            this.textBoxYoffsetAdjust.Name = "textBoxYoffsetAdjust";
            this.textBoxYoffsetAdjust.Size = new System.Drawing.Size(44, 21);
            this.textBoxYoffsetAdjust.TabIndex = 1;
            this.textBoxYoffsetAdjust.Text = "0";
            // 
            // buttonApplyYoffsetAdjust
            // 
            this.buttonApplyYoffsetAdjust.Location = new System.Drawing.Point(134, 19);
            this.buttonApplyYoffsetAdjust.Name = "buttonApplyYoffsetAdjust";
            this.buttonApplyYoffsetAdjust.Size = new System.Drawing.Size(24, 23);
            this.buttonApplyYoffsetAdjust.TabIndex = 2;
            this.buttonApplyYoffsetAdjust.Text = ">";
            this.buttonApplyYoffsetAdjust.UseVisualStyleBackColor = true;
            this.buttonApplyYoffsetAdjust.Click += new System.EventHandler(this.buttonApplyYoffsetAdjust_Click);
            // 
            // textBoxLogOutput
            // 
            this.textBoxLogOutput.Location = new System.Drawing.Point(222, 590);
            this.textBoxLogOutput.Multiline = true;
            this.textBoxLogOutput.Name = "textBoxLogOutput";
            this.textBoxLogOutput.ReadOnly = true;
            this.textBoxLogOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLogOutput.Size = new System.Drawing.Size(629, 269);
            this.textBoxLogOutput.TabIndex = 34;
            // 
            // buttonSaveLogAs
            // 
            this.buttonSaveLogAs.Location = new System.Drawing.Point(222, 860);
            this.buttonSaveLogAs.Name = "buttonSaveLogAs";
            this.buttonSaveLogAs.Size = new System.Drawing.Size(90, 27);
            this.buttonSaveLogAs.TabIndex = 35;
            this.buttonSaveLogAs.Text = "Save Log As...";
            this.buttonSaveLogAs.UseVisualStyleBackColor = true;
            this.buttonSaveLogAs.Click += new System.EventHandler(this.buttonSaveLogAs_Click);
            // 
            // FontEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1373, 887);
            this.Controls.Add(this.textBoxLogOutput);
            this.Controls.Add(this.buttonSaveLogAs);
            this.Controls.Add(this.groupBoxMatchTextures);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.labelTexturePreview);
            this.Controls.Add(this.pictureBoxTexturePreview);
            this.Controls.Add(this.dataGridViewWithCoord);
            this.Controls.Add(this.dataGridViewWithTextures);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.groupBoxFntAdjust);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "FontEditor";
            this.Text = "TTG Font Creator";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FontEditor_FormClosing);
            this.Load += new System.EventHandler(this.FontEditor_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewWithTextures)).EndInit();
            this.contextMenuStripExport_Import.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewWithCoord)).EndInit();
            this.contextMenuStripExp_imp_Coord.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxTexturePreview)).EndInit();
            this.groupBoxMatchTextures.ResumeLayout(false);
            this.groupBoxMatchTextures.PerformLayout();
            this.groupBoxFntAdjust.ResumeLayout(false);
            this.groupBoxFntAdjust.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.DataGridView dataGridViewWithTextures;
        internal System.Windows.Forms.DataGridView dataGridViewWithCoord;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripExport_Import;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importDDSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem archivePackerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem archiveUnpackerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ttarch2ScannerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quickToolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsFormToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripExp_imp_Coord;
        private System.Windows.Forms.ToolStripMenuItem exportCoordinatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importCoordinatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearExistingFntDdsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportCoordinatesToolStripMenuItem1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbNoKerning;
        private System.Windows.Forms.RadioButton rbKerning;
        private System.Windows.Forms.Label labelFontName;
        private System.Windows.Forms.Label labelSearchChar;
        private System.Windows.Forms.TextBox textBoxSearchChar;
        private System.Windows.Forms.Button buttonPreviewChar;
        private System.Windows.Forms.ToolStripMenuItem toolStripImportFNT;
        private System.Windows.Forms.ToolStripMenuItem removeDuplicatesCharsToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton rbSwitchSwizzle;
        private System.Windows.Forms.RadioButton rbPS4Swizzle;
        private System.Windows.Forms.RadioButton rbXbox360Swizzle;
        private System.Windows.Forms.RadioButton rbPSVitaSwizzle;
        private System.Windows.Forms.RadioButton rbWiiSwizzle;
        private System.Windows.Forms.RadioButton rbNoSwizzle;
        private System.Windows.Forms.PictureBox pictureBoxTexturePreview;
        private System.Windows.Forms.Label labelTexturePreview;
        private System.Windows.Forms.GroupBox groupBoxMatchTextures;
        private System.Windows.Forms.GroupBox groupBoxFntAdjust;
        private System.Windows.Forms.Button buttonDetectMissingTextures;
        private System.Windows.Forms.Label labelYoffsetAdjust;
        private System.Windows.Forms.TextBox textBoxYoffset;
        private System.Windows.Forms.Label labelYoffsetAdjust2;
        private System.Windows.Forms.TextBox textBoxYoffsetAdjust;
        private System.Windows.Forms.Button buttonApplyYoffsetAdjust;
        private System.Windows.Forms.Label labelSizeAdj;
        private System.Windows.Forms.TextBox textBoxSizeAdj;
        private System.Windows.Forms.Button buttonSizeApply;
        private System.Windows.Forms.Label labelLineHeightAdj;
        private System.Windows.Forms.TextBox textBoxLineHeightAdj;
        private System.Windows.Forms.Button buttonLHApply;
        private System.Windows.Forms.Label labelHeightAdj;
        private System.Windows.Forms.TextBox textBoxHeightAdj;
        private System.Windows.Forms.Button buttonHeightApply;
        private System.Windows.Forms.Label labelChannelAdj;
        private System.Windows.Forms.TextBox textBoxChannelAdj;
        private System.Windows.Forms.Button buttonChannelApply;
        private System.Windows.Forms.Label labelBaseAdj;
        private System.Windows.Forms.TextBox textBoxBaseAdj;
        private System.Windows.Forms.Button buttonBaseApply;
        private System.Windows.Forms.Button buttonYoffsetUp;
        private System.Windows.Forms.Button buttonYoffsetDown;
        private System.Windows.Forms.Button buttonYadjUp;
        private System.Windows.Forms.Button buttonYadjDown;
        private System.Windows.Forms.Label labelYAdjust;
        private System.Windows.Forms.TextBox textBoxYAdjust;
        private System.Windows.Forms.Button buttonApplyYAdjust;
        private System.Windows.Forms.Label labelFontSizeAdjust;
        private System.Windows.Forms.TextBox textBoxFontSizeAdjust;
        private System.Windows.Forms.Button buttonGenerateMissingChars;
        private System.Windows.Forms.Label labelProfile;
        private System.Windows.Forms.ComboBox comboBoxProfiles;
        private System.Windows.Forms.Button buttonSaveProfile;
        private System.Windows.Forms.Button buttonDeleteProfile;
        private System.Windows.Forms.Label labelGenFont;
        private System.Windows.Forms.TextBox textBoxGenFont;
        private System.Windows.Forms.Button buttonPickFont;
        private System.Windows.Forms.TextBox textBoxLogOutput;
        private System.Windows.Forms.Button buttonSaveLogAs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem newFontToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn N;
        private System.Windows.Forms.DataGridViewTextBoxColumn Height;
        private System.Windows.Forms.DataGridViewTextBoxColumn Width;
        private System.Windows.Forms.DataGridViewTextBoxColumn Size;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column5;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column7;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column8;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column9;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column10;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column11;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column12;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column13;
    }
}
