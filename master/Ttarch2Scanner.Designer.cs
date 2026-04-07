namespace TTG_Tools
{
    partial class Ttarch2Scanner
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being being used.
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scanFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.archivesListView = new System.Windows.Forms.ListView();
            this.archiveNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.fileCountHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlRight = new System.Windows.Forms.Panel();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.filesListView = new System.Windows.Forms.ListView();
            this.fileNameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.fileSizeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.fileOffsetHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblFileInfo = new System.Windows.Forms.Label();
            this.pnlViewer = new System.Windows.Forms.Panel();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.txtHexViewer = new System.Windows.Forms.TextBox();
            this.lblPreviewInfo = new System.Windows.Forms.Label();
            this.pathHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pnlTop = new System.Windows.Forms.Panel();
            this.lblArchiveCount = new System.Windows.Forms.Label();
            this.lblScanning = new System.Windows.Forms.Label();
            this.cmbGameKey = new System.Windows.Forms.ComboBox();
            this.lblGameKey = new System.Windows.Forms.Label();
            this.btnSearchArchive = new System.Windows.Forms.Button();
            this.btnSearchFiles = new System.Windows.Forms.Button();
            this.txtSearchFiles = new System.Windows.Forms.TextBox();
            this.txtSearchArchive = new System.Windows.Forms.TextBox();
            this.lblSearchFiles = new System.Windows.Forms.Label();
            this.lblSearchArchive = new System.Windows.Forms.Label();
            this.cmbFileExtension = new System.Windows.Forms.ComboBox();
            this.lblFileExtension = new System.Windows.Forms.Label();
            this.chkSwitchSwizzle = new System.Windows.Forms.CheckBox();
            this.scanProgressBar = new System.Windows.Forms.ProgressBar();
            this.lblScanProgress = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.pnlRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.pnlViewer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.pnlTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1100, 25);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scanFolderToolStripMenuItem,
            this.exportResultsToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(39, 21);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // scanFolderToolStripMenuItem
            // 
            this.scanFolderToolStripMenuItem.Name = "scanFolderToolStripMenuItem";
            this.scanFolderToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.scanFolderToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.scanFolderToolStripMenuItem.Text = "Scan Folder...";
            this.scanFolderToolStripMenuItem.Click += new System.EventHandler(this.scanFolderToolStripMenuItem_Click);
            // 
            // exportResultsToolStripMenuItem
            // 
            this.exportResultsToolStripMenuItem.Name = "exportResultsToolStripMenuItem";
            this.exportResultsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.exportResultsToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.exportResultsToolStripMenuItem.Text = "Export Results...";
            this.exportResultsToolStripMenuItem.Click += new System.EventHandler(this.exportResultsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(209, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 155);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.archivesListView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pnlRight);
            this.splitContainer1.Size = new System.Drawing.Size(1100, 545);
            this.splitContainer1.SplitterDistance = 400;
            this.splitContainer1.TabIndex = 1;
            // 
            // archivesListView
            // 
            this.archivesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.archiveNameHeader,
            this.fileCountHeader});
            this.archivesListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.archivesListView.FullRowSelect = true;
            this.archivesListView.GridLines = true;
            this.archivesListView.HideSelection = false;
            this.archivesListView.Location = new System.Drawing.Point(0, 0);
            this.archivesListView.Name = "archivesListView";
            this.archivesListView.Size = new System.Drawing.Size(400, 545);
            this.archivesListView.TabIndex = 0;
            this.archivesListView.UseCompatibleStateImageBehavior = false;
            this.archivesListView.View = System.Windows.Forms.View.Details;
            this.archivesListView.SelectedIndexChanged += new System.EventHandler(this.archivesListView_SelectedIndexChanged);
            this.archivesListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.archivesListView_MouseClick);
            // 
            // archiveNameHeader
            // 
            this.archiveNameHeader.Text = "Archive Name";
            this.archiveNameHeader.Width = 280;
            // 
            // fileCountHeader
            // 
            this.fileCountHeader.Text = "Files";
            this.fileCountHeader.Width = 80;
            // 
            // pnlRight
            // 
            this.pnlRight.Controls.Add(this.splitContainer2);
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRight.Location = new System.Drawing.Point(0, 0);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(696, 545);
            this.pnlRight.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.filesListView);
            this.splitContainer2.Panel1.Controls.Add(this.lblFileInfo);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.pnlViewer);
            this.splitContainer2.Size = new System.Drawing.Size(696, 545);
            this.splitContainer2.SplitterDistance = 300;
            this.splitContainer2.TabIndex = 0;
            // 
            // filesListView
            // 
            this.filesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.fileNameHeader,
            this.fileSizeHeader,
            this.fileOffsetHeader});
            this.filesListView.FullRowSelect = true;
            this.filesListView.GridLines = true;
            this.filesListView.HideSelection = false;
            this.filesListView.Location = new System.Drawing.Point(3, 30);
            this.filesListView.Name = "filesListView";
            this.filesListView.Size = new System.Drawing.Size(294, 466);
            this.filesListView.TabIndex = 1;
            this.filesListView.UseCompatibleStateImageBehavior = false;
            this.filesListView.View = System.Windows.Forms.View.Details;
            this.filesListView.SelectedIndexChanged += new System.EventHandler(this.filesListView_SelectedIndexChanged);
            // 
            // fileNameHeader
            // 
            this.fileNameHeader.Text = "File Name";
            this.fileNameHeader.Width = 420;
            // 
            // fileSizeHeader
            // 
            this.fileSizeHeader.Text = "Size";
            this.fileSizeHeader.Width = 100;
            // 
            // fileOffsetHeader
            // 
            this.fileOffsetHeader.Text = "Offset";
            this.fileOffsetHeader.Width = 120;
            // 
            // lblFileInfo
            // 
            this.lblFileInfo.AutoSize = true;
            this.lblFileInfo.Location = new System.Drawing.Point(3, 3);
            this.lblFileInfo.Name = "lblFileInfo";
            this.lblFileInfo.Size = new System.Drawing.Size(101, 13);
            this.lblFileInfo.TabIndex = 0;
            this.lblFileInfo.Text = "Select an archive...";
            // 
            // pnlViewer
            // 
            this.pnlViewer.Controls.Add(this.pictureBoxPreview);
            this.pnlViewer.Controls.Add(this.txtHexViewer);
            this.pnlViewer.Controls.Add(this.lblPreviewInfo);
            this.pnlViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlViewer.Location = new System.Drawing.Point(0, 0);
            this.pnlViewer.Name = "pnlViewer";
            this.pnlViewer.Size = new System.Drawing.Size(392, 545);
            this.pnlViewer.TabIndex = 0;
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxPreview.BackColor = System.Drawing.Color.Black;
            this.pictureBoxPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxPreview.Location = new System.Drawing.Point(3, 20);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(386, 522);
            this.pictureBoxPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxPreview.TabIndex = 2;
            this.pictureBoxPreview.TabStop = false;
            // 
            // txtHexViewer
            // 
            this.txtHexViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHexViewer.Font = new System.Drawing.Font("Consolas", 8F);
            this.txtHexViewer.Location = new System.Drawing.Point(3, 20);
            this.txtHexViewer.Multiline = true;
            this.txtHexViewer.Name = "txtHexViewer";
            this.txtHexViewer.ReadOnly = true;
            this.txtHexViewer.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtHexViewer.Size = new System.Drawing.Size(386, 522);
            this.txtHexViewer.TabIndex = 1;
            this.txtHexViewer.Visible = false;
            // 
            // lblPreviewInfo
            // 
            this.lblPreviewInfo.AutoSize = true;
            this.lblPreviewInfo.Location = new System.Drawing.Point(3, 3);
            this.lblPreviewInfo.Name = "lblPreviewInfo";
            this.lblPreviewInfo.Size = new System.Drawing.Size(45, 13);
            this.lblPreviewInfo.TabIndex = 0;
            this.lblPreviewInfo.Text = "Preview";
            // 
            // pathHeader
            // 
            this.pathHeader.Text = "Path";
            this.pathHeader.Width = 200;
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.lblArchiveCount);
            this.pnlTop.Controls.Add(this.lblScanning);
            this.pnlTop.Controls.Add(this.cmbGameKey);
            this.pnlTop.Controls.Add(this.lblGameKey);
            this.pnlTop.Controls.Add(this.btnSearchArchive);
            this.pnlTop.Controls.Add(this.btnSearchFiles);
            this.pnlTop.Controls.Add(this.txtSearchFiles);
            this.pnlTop.Controls.Add(this.txtSearchArchive);
            this.pnlTop.Controls.Add(this.lblSearchFiles);
            this.pnlTop.Controls.Add(this.lblSearchArchive);
            this.pnlTop.Controls.Add(this.cmbFileExtension);
            this.pnlTop.Controls.Add(this.lblFileExtension);
            this.pnlTop.Controls.Add(this.chkSwitchSwizzle);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 25);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1100, 130);
            this.pnlTop.TabIndex = 2;
            // 
            // lblArchiveCount
            // 
            this.lblArchiveCount.AutoSize = true;
            this.lblArchiveCount.Location = new System.Drawing.Point(12, 106);
            this.lblArchiveCount.Name = "lblArchiveCount";
            this.lblArchiveCount.Size = new System.Drawing.Size(61, 13);
            this.lblArchiveCount.TabIndex = 10;
            this.lblArchiveCount.Text = "Archives: 0";
            // 
            // lblScanning
            // 
            this.lblScanning.AutoSize = true;
            this.lblScanning.ForeColor = System.Drawing.Color.Blue;
            this.lblScanning.Location = new System.Drawing.Point(100, 106);
            this.lblScanning.Name = "lblScanning";
            this.lblScanning.Size = new System.Drawing.Size(62, 13);
            this.lblScanning.TabIndex = 9;
            this.lblScanning.Text = "Scanning...";
            this.lblScanning.Visible = false;
            // 
            // cmbGameKey
            // 
            this.cmbGameKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbGameKey.FormattingEnabled = true;
            this.cmbGameKey.Location = new System.Drawing.Point(100, 9);
            this.cmbGameKey.Name = "cmbGameKey";
            this.cmbGameKey.Size = new System.Drawing.Size(280, 21);
            this.cmbGameKey.TabIndex = 0;
            // 
            // lblGameKey
            // 
            this.lblGameKey.AutoSize = true;
            this.lblGameKey.Location = new System.Drawing.Point(12, 12);
            this.lblGameKey.Name = "lblGameKey";
            this.lblGameKey.Size = new System.Drawing.Size(59, 13);
            this.lblGameKey.TabIndex = 11;
            this.lblGameKey.Text = "Game Key:";
            // 
            // btnSearchArchive
            // 
            this.btnSearchArchive.Location = new System.Drawing.Point(380, 35);
            this.btnSearchArchive.Name = "btnSearchArchive";
            this.btnSearchArchive.Size = new System.Drawing.Size(75, 21);
            this.btnSearchArchive.TabIndex = 7;
            this.btnSearchArchive.Text = "Search";
            this.btnSearchArchive.UseVisualStyleBackColor = true;
            this.btnSearchArchive.Click += new System.EventHandler(this.btnSearchArchive_Click);
            // 
            // btnSearchFiles
            // 
            this.btnSearchFiles.Location = new System.Drawing.Point(380, 62);
            this.btnSearchFiles.Name = "btnSearchFiles";
            this.btnSearchFiles.Size = new System.Drawing.Size(75, 21);
            this.btnSearchFiles.TabIndex = 6;
            this.btnSearchFiles.Text = "Search";
            this.btnSearchFiles.UseVisualStyleBackColor = true;
            this.btnSearchFiles.Click += new System.EventHandler(this.btnSearchFiles_Click);
            // 
            // txtSearchFiles
            // 
            this.txtSearchFiles.Location = new System.Drawing.Point(100, 65);
            this.txtSearchFiles.Name = "txtSearchFiles";
            this.txtSearchFiles.Size = new System.Drawing.Size(274, 21);
            this.txtSearchFiles.TabIndex = 5;
            // 
            // txtSearchArchive
            // 
            this.txtSearchArchive.Location = new System.Drawing.Point(100, 38);
            this.txtSearchArchive.Name = "txtSearchArchive";
            this.txtSearchArchive.Size = new System.Drawing.Size(274, 21);
            this.txtSearchArchive.TabIndex = 4;
            // 
            // lblSearchFiles
            // 
            this.lblSearchFiles.AutoSize = true;
            this.lblSearchFiles.Location = new System.Drawing.Point(12, 68);
            this.lblSearchFiles.Name = "lblSearchFiles";
            this.lblSearchFiles.Size = new System.Drawing.Size(77, 13);
            this.lblSearchFiles.TabIndex = 3;
            this.lblSearchFiles.Text = "Search in files:";
            // 
            // lblSearchArchive
            // 
            this.lblSearchArchive.AutoSize = true;
            this.lblSearchArchive.Location = new System.Drawing.Point(12, 41);
            this.lblSearchArchive.Name = "lblSearchArchive";
            this.lblSearchArchive.Size = new System.Drawing.Size(87, 13);
            this.lblSearchArchive.TabIndex = 2;
            this.lblSearchArchive.Text = "Search archives:";
            // 
            // cmbFileExtension
            // 
            this.cmbFileExtension.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFileExtension.FormattingEnabled = true;
            this.cmbFileExtension.Location = new System.Drawing.Point(450, 9);
            this.cmbFileExtension.Name = "cmbFileExtension";
            this.cmbFileExtension.Size = new System.Drawing.Size(150, 21);
            this.cmbFileExtension.TabIndex = 1;
            this.cmbFileExtension.SelectedIndexChanged += new System.EventHandler(this.cmbFileExtension_SelectedIndexChanged);
            // 
            // lblFileExtension
            //
            this.lblFileExtension.AutoSize = true;
            this.lblFileExtension.Location = new System.Drawing.Point(390, 12);
            this.lblFileExtension.Name = "lblFileExtension";
            this.lblFileExtension.Size = new System.Drawing.Size(35, 13);
            this.lblFileExtension.TabIndex = 2;
            this.lblFileExtension.Text = "Filter:";
            //
            // chkSwitchSwizzle
            //
            this.chkSwitchSwizzle.AutoSize = true;
            this.chkSwitchSwizzle.Location = new System.Drawing.Point(620, 12);
            this.chkSwitchSwizzle.Name = "chkSwitchSwizzle";
            this.chkSwitchSwizzle.Size = new System.Drawing.Size(130, 17);
            this.chkSwitchSwizzle.TabIndex = 3;
            this.chkSwitchSwizzle.Text = "Switch De-Swizzle";
            //
            // scanProgressBar
            // 
            this.scanProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.scanProgressBar.Location = new System.Drawing.Point(0, 680);
            this.scanProgressBar.Name = "scanProgressBar";
            this.scanProgressBar.Size = new System.Drawing.Size(1100, 20);
            this.scanProgressBar.TabIndex = 3;
            // 
            // lblScanProgress
            // 
            this.lblScanProgress.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblScanProgress.Location = new System.Drawing.Point(0, 654);
            this.lblScanProgress.Name = "lblScanProgress";
            this.lblScanProgress.Size = new System.Drawing.Size(1100, 26);
            this.lblScanProgress.TabIndex = 4;
            this.lblScanProgress.Text = "Ready";
            this.lblScanProgress.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Ttarch2Scanner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1100, 700);
            this.Controls.Add(this.lblScanProgress);
            this.Controls.Add(this.scanProgressBar);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "Ttarch2Scanner";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TTArch2 Scanner";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Ttarch2Scanner_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.pnlViewer.ResumeLayout(false);
            this.pnlViewer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem scanFolderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportResultsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView archivesListView;
        private System.Windows.Forms.ColumnHeader archiveNameHeader;
        private System.Windows.Forms.ColumnHeader fileCountHeader;
        private System.Windows.Forms.ColumnHeader pathHeader;
        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.ComboBox cmbFileExtension;
        private System.Windows.Forms.Label lblFileExtension;
        private System.Windows.Forms.CheckBox chkSwitchSwizzle;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.ListView filesListView;
        private System.Windows.Forms.ColumnHeader fileNameHeader;
        private System.Windows.Forms.ColumnHeader fileSizeHeader;
        private System.Windows.Forms.ColumnHeader fileOffsetHeader;
        private System.Windows.Forms.Label lblFileInfo;
        private System.Windows.Forms.TextBox txtSearchArchive;
        private System.Windows.Forms.TextBox txtSearchFiles;
        private System.Windows.Forms.Label lblSearchArchive;
        private System.Windows.Forms.Label lblSearchFiles;
        private System.Windows.Forms.Button btnSearchArchive;
        private System.Windows.Forms.Button btnSearchFiles;
        private System.Windows.Forms.ProgressBar scanProgressBar;
        private System.Windows.Forms.Label lblScanProgress;
        private System.Windows.Forms.Label lblScanning;
        private System.Windows.Forms.Label lblArchiveCount;
        private System.Windows.Forms.ComboBox cmbGameKey;
        private System.Windows.Forms.Label lblGameKey;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Panel pnlViewer;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.TextBox txtHexViewer;
        private System.Windows.Forms.Label lblPreviewInfo;
    }
}
