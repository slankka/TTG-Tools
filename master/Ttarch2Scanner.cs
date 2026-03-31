using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using TTG_Tools.ClassesStructs;

namespace TTG_Tools
{
    public partial class Ttarch2Scanner : Form
    {
        // 扫描到的 ttarch2 文件信息
        private class ArchiveInfo
        {
            public string FilePath { get; set; }
            public string FileName { get; set; }
            public int FileCount { get; set; }
            public Ttarch2Class.Ttarch2files[] Files { get; set; }
            public List<string> FileExtensions { get; set; } = new List<string>();
        }

        private List<ArchiveInfo> _scannedArchives = new List<ArchiveInfo>();
        private HashSet<string> _allFileExtensions = new HashSet<string>();
        private ArchiveInfo _selectedArchive;
        private CancellationTokenSource _scanCancellationTokenSource;
        private string _currentScanFolder = "";
        private int _scanCompletedCount = 0;  // 实际完成的扫描数量
        private int _displayProgress = 0;      // 显示的进度值
        private System.Windows.Forms.Timer _progressAnimationTimer;
        private List<string> _failedFiles = new List<string>();  // 扫描失败的文件

        public Ttarch2Scanner()
        {
            InitializeComponent();
            InitializeExtensionComboBox();
            InitializeProgressAnimationTimer();
            InitializeGameKeyComboBox();
        }

        private void InitializeGameKeyComboBox()
        {
            cmbGameKey.Items.Clear();
            cmbGameKey.Items.Add("Auto-detect (try all keys)");

            for (int i = 0; i < MainMenu.gamelist.Count; i++)
            {
                cmbGameKey.Items.Add(i + ". " + MainMenu.gamelist[i].gamename);
            }

            cmbGameKey.SelectedIndex = 0;
        }

        private void InitializeProgressAnimationTimer()
        {
            _progressAnimationTimer = new System.Windows.Forms.Timer();
            _progressAnimationTimer.Interval = 30; // 30ms 更新一次
            _progressAnimationTimer.Tick += ProgressAnimationTimer_Tick;
        }

        private void ProgressAnimationTimer_Tick(object sender, EventArgs e)
        {
            if (_displayProgress < scanProgressBar.Maximum)
            {
                _displayProgress++;
                scanProgressBar.Value = _displayProgress;
            }
            else
            {
                _progressAnimationTimer.Stop();
            }
        }

        private void InitializeExtensionComboBox()
        {
            cmbFileExtension.Items.Clear();
            cmbFileExtension.Items.Add("All");
            cmbFileExtension.SelectedIndex = 0;
        }

        private string SelectFolder()
        {
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();
            folderDialog.IsFolderPicker = true;
            folderDialog.EnsurePathExists = true;
            return folderDialog.ShowDialog() == CommonFileDialogResult.Ok ? folderDialog.FileName : null;
        }

        private async void scanFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string folderPath = SelectFolder();
            if (string.IsNullOrEmpty(folderPath))
                return;

            // 取消之前的扫描任务
            _scanCancellationTokenSource?.Cancel();
            _progressAnimationTimer?.Stop();
            _scanCancellationTokenSource = new CancellationTokenSource();

            await ScanFolderAsync(folderPath, _scanCancellationTokenSource.Token);
        }

        private async Task ScanFolderAsync(string folderPath, CancellationToken cancellationToken)
        {
            try
            {
                // 清空之前的结果
                _scannedArchives.Clear();
                _allFileExtensions.Clear();
                archivesListView.Items.Clear();
                filesListView.Items.Clear();
                _selectedArchive = null;
                _currentScanFolder = folderPath;
                _failedFiles.Clear();

                // 更新UI状态
                lblScanning.Visible = true;
                lblScanning.Text = "Detecting game keys...";
                lblScanProgress.Text = "Scanning for ttarch2 files...";
                Enabled = false;

                // 查找所有 ttarch2 文件
                var ttarch2Files = await Task.Run(() =>
                {
                    return Directory.GetFiles(folderPath, "*.ttarch2", SearchOption.AllDirectories)
                        .Concat(Directory.GetFiles(folderPath, "*.obb", SearchOption.AllDirectories))
                        .ToArray();
                }, cancellationToken);

                if (ttarch2Files.Length == 0)
                {
                    MessageBox.Show("No ttarch2 or obb files found in the selected folder.", "No Files Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ResetUIState();
                    return;
                }

                // 初始化进度条
                scanProgressBar.Minimum = 0;
                scanProgressBar.Maximum = ttarch2Files.Length;
                scanProgressBar.Value = 0;
                _scanCompletedCount = 0;
                _displayProgress = 0;

                // 停止之前的动画
                _progressAnimationTimer.Stop();

                // 获取选中的游戏密钥
                byte[] selectedKey = null;
                if (cmbGameKey.SelectedIndex > 0 && cmbGameKey.SelectedIndex - 1 < MainMenu.gamelist.Count)
                {
                    selectedKey = MainMenu.gamelist[cmbGameKey.SelectedIndex - 1].key;
                }

                // 并行扫描每个文件
                var scanTasks = ttarch2Files.Select(file => Task.Run(() =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return null;

                    var info = ScanTtarch2File(file, selectedKey);
                    if (info != null)
                    {
                        int completed = Interlocked.Increment(ref _scanCompletedCount);
                        UpdateScanProgress(completed, ttarch2Files.Length, info.FileName);
                    }
                    else
                    {
                        // 记录失败的文件
                        lock (_failedFiles)
                        {
                            _failedFiles.Add(file);
                        }
                        // 仍然更新进度（虽然失败了）
                        int completed = Interlocked.Increment(ref _scanCompletedCount);
                        UpdateScanProgress(completed, ttarch2Files.Length, Path.GetFileName(file) + " (failed)");
                    }
                    return info;
                }, cancellationToken));

                var results = await Task.WhenAll(scanTasks);

                // 扫描完成，启动进度条动画到100%
                _displayProgress = _scanCompletedCount;
                _progressAnimationTimer.Start();

                // 过滤掉 null 结果并添加到列表
                foreach (var result in results.Where(r => r != null))
                {
                    _scannedArchives.Add(result);
                }

                // 更新文件扩展名列表
                UpdateFileExtensionsList();

                // 更新UI显示
                UpdateArchivesListView();
                ResetUIState();

                // 更新窗体标题显示扫描的文件夹
                this.Text = $"TTArch2 Scanner - {_currentScanFolder}";

                // 显示扫描结果统计
                int totalFiles = ttarch2Files.Length;
                int successCount = _scannedArchives.Count;
                int failedCount = totalFiles - successCount;

                string resultMsg = $"Scan complete!\n\nFound {_scannedArchives.Count} valid archive(s) out of {totalFiles} ttarch2 file(s).";
                if (failedCount > 0)
                {
                    resultMsg += $"\n\n{failedCount} file(s) failed to scan (wrong format or missing encryption key).";
                }
                lblScanProgress.Text = $"Complete: {successCount}/{totalFiles} archives scanned successfully";

                MessageBox.Show(resultMsg, "Scan Complete", MessageBoxButtons.OK,
                    failedCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                ResetUIState();
                lblScanProgress.Text = "Scan cancelled.";
            }
            catch (Exception ex)
            {
                ResetUIState();
                MessageBox.Show($"Error scanning folder: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private ArchiveInfo ScanTtarch2File(string filePath, byte[] providedKey)
        {
            // 使用与Unpacker相同的逻辑
            byte[] key = providedKey;

            // 如果没有提供key但选择了Auto-detect，尝试所有key
            if (key == null && cmbGameKey.SelectedIndex == 0 && MainMenu.gamelist.Count > 0)
            {
                // 先尝试不使用key（未加密文件）
                ArchiveInfo result = ReadTtarch2FileDirect(filePath, null);
                if (result != null)
                {
                    return result;
                }

                // 尝试每个key
                foreach (var game in MainMenu.gamelist)
                {
                    result = ReadTtarch2FileDirect(filePath, game.key);
                    if (result != null)
                    {
                        return result;
                    }
                }
                return null;
            }
            else
            {
                return ReadTtarch2FileDirect(filePath, key);
            }
        }

        private ArchiveInfo ReadTtarch2FileDirect(string path, byte[] key)
        {
            // 直接使用Unpacker的ReadHeaderTtarch2逻辑
            try
            {
                var ttarch2 = new Ttarch2Class();
                ttarch2.fileFormats = new List<string>();
                ttarch2.fileName = path;

                using (FileStream fs = new FileStream(path, FileMode.Open))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    ulong foffset = 0;

                    byte[] header = br.ReadBytes(4);
                    foffset += 4;
                    ttarch2.compressAlgorithm = -1;
                    ttarch2.isCompressed = Encoding.ASCII.GetString(header) != "NCTT";
                    ttarch2.isEncrypted = Encoding.ASCII.GetString(header) == "ECTT" || Encoding.ASCII.GetString(header) == "eCTT";

                    if (ttarch2.isCompressed)
                    {
                        ttarch2.compressAlgorithm = Encoding.ASCII.GetString(header) == "eCTT" || Encoding.ASCII.GetString(header) == "zCTT" ? 2 : 1;
                        if (Encoding.ASCII.GetString(header) == "eCTT" || Encoding.ASCII.GetString(header) == "zCTT")
                        {
                            int one = br.ReadInt32();
                            foffset += 4;
                        }
                        ttarch2.chunkSize = br.ReadUInt32();
                        int blocksCount = br.ReadInt32();
                        foffset += 4 + 4;
                        ttarch2.compressedBlocks = new ulong[blocksCount];

                        ulong val1 = br.ReadUInt64();
                        ulong val2 = 0;

                        for (int i = 0; i < blocksCount; i++)
                        {
                            val2 = br.ReadUInt64();
                            ttarch2.compressedBlocks[i] = val2 - val1;
                            val1 = val2;
                            foffset += 8;
                        }

                        long pos = br.BaseStream.Position;
                        ttarch2.cFilesOffset = (ulong)br.BaseStream.Position;

                        byte[] tmp = br.ReadBytes((int)ttarch2.compressedBlocks[0]);

                        if (ttarch2.isEncrypted && key != null)
                        {
                            BlowFishCS.BlowFish dec = new BlowFishCS.BlowFish(key, 7);
                            tmp = dec.Crypt_ECB(tmp, 7, true);
                        }

                        tmp = DecompressBlockUnpacker(tmp, ttarch2.compressAlgorithm, ttarch2.chunkSize);
                        if (tmp == null)
                        {
                            return null;
                        }

                        int suboff = 0;
                        uint filesCount = 0;

                        using (MemoryStream ms = new MemoryStream(tmp))
                        using (BinaryReader mbr = new BinaryReader(ms))
                        {
                            byte[] subHeader = mbr.ReadBytes(4);
                            suboff += 4;
                            if (Encoding.ASCII.GetString(subHeader) == "3ATT")
                            {
                                int two = mbr.ReadInt32();
                                suboff += 4;
                            }

                            ttarch2.version = Encoding.ASCII.GetString(subHeader) == "3ATT" ? 1 : 2;

                            uint nameSize = mbr.ReadUInt32();
                            suboff += 4;
                            filesCount = mbr.ReadUInt32();
                            suboff += 4;

                            ttarch2.filesOffset = (ulong)suboff + (28 * filesCount) + nameSize;
                            ttarch2.files = new Ttarch2Class.Ttarch2files[filesCount];
                        }

                        if (ttarch2.filesOffset > (ulong)tmp.Length)
                        {
                            br.BaseStream.Seek(pos, SeekOrigin.Begin);
                            int index = (int)(ttarch2.filesOffset / ttarch2.chunkSize) + 1;

                            using (MemoryStream ms = new MemoryStream())
                            {
                                for (int i = 0; i < index; i++)
                                {
                                    tmp = br.ReadBytes((int)ttarch2.compressedBlocks[i]);

                                    if (ttarch2.isEncrypted && key != null)
                                    {
                                        BlowFishCS.BlowFish dec = new BlowFishCS.BlowFish(key, 7);
                                        tmp = dec.Crypt_ECB(tmp, 7, true);
                                    }

                                    tmp = DecompressBlockUnpacker(tmp, ttarch2.compressAlgorithm, ttarch2.chunkSize);
                                    if (tmp == null)
                                    {
                                        return null;
                                    }

                                    ms.Write(tmp, 0, tmp.Length);
                                }

                                tmp = ms.ToArray();
                            }
                        }

                        using (MemoryStream ms = new MemoryStream(tmp))
                        using (BinaryReader mbr = new BinaryReader(ms))
                        {
                            mbr.BaseStream.Seek(suboff, SeekOrigin.Begin);

                            for (int i = 0; i < (int)filesCount; i++)
                            {
                                ttarch2.files[i].fileNameCRC64 = mbr.ReadUInt64();
                                ttarch2.files[i].fileOffset = mbr.ReadUInt64();
                                ttarch2.files[i].fileSize = mbr.ReadInt32();
                                int unknown = mbr.ReadInt32();
                                ushort nameBlock = mbr.ReadUInt16();
                                ushort nameOff = mbr.ReadUInt16();
                                pos = mbr.BaseStream.Position;
                                ulong nameOffset = (ulong)suboff + (28 * (ulong)filesCount) + (ulong)nameOff + ((ulong)nameBlock * 0x10000);
                                mbr.BaseStream.Seek((long)nameOffset, SeekOrigin.Begin);

                                using (MemoryStream mms = new MemoryStream())
                                {
                                    byte[] bytes = null;
                                    while (true)
                                    {
                                        bytes = mbr.ReadBytes(1);
                                        if (bytes[0] == 0) break;
                                        mms.Write(bytes, 0, bytes.Length);
                                    }

                                    bytes = mms.ToArray();
                                    ttarch2.files[i].fileName = Encoding.ASCII.GetString(bytes);

                                    string ext = GetExtension(ttarch2.files[i].fileName);
                                    if (!string.IsNullOrEmpty(ext) && !ttarch2.fileFormats.Contains(ext))
                                    {
                                        ttarch2.fileFormats.Add(ext);
                                    }
                                }

                                mbr.BaseStream.Seek(pos, SeekOrigin.Begin);
                            }
                        }
                    }
                    else
                    {
                        ulong archSize = br.ReadUInt64();
                        foffset += 8;
                        byte[] subHeader = br.ReadBytes(4);
                        foffset += 4;
                        if (Encoding.ASCII.GetString(subHeader) == "3ATT")
                        {
                            int two = br.ReadInt32();
                            foffset += 4;
                        }

                        ttarch2.version = Encoding.ASCII.GetString(subHeader) == "3ATT" ? 1 : 2;

                        uint nameSize = br.ReadUInt32();
                        foffset += 4;
                        uint filesCount = br.ReadUInt32();
                        foffset += 4;
                        ttarch2.files = new Ttarch2Class.Ttarch2files[filesCount];
                        ttarch2.filesOffset = foffset + (28 * (ulong)filesCount) + (ulong)nameSize;

                        for (int i = 0; i < filesCount; i++)
                        {
                            ttarch2.files[i].fileNameCRC64 = br.ReadUInt64();
                            ttarch2.files[i].fileOffset = br.ReadUInt64();
                            ttarch2.files[i].fileSize = br.ReadInt32();
                            int unknown = br.ReadInt32();
                            ushort nameBlock = br.ReadUInt16();
                            ushort nameOff = br.ReadUInt16();
                            long pos = br.BaseStream.Position;
                            ulong nameOffset = foffset + (28 * (ulong)filesCount) + (ulong)nameOff + ((ulong)nameBlock * 0x10000);
                            br.BaseStream.Seek((long)nameOffset, SeekOrigin.Begin);

                            using (MemoryStream ms = new MemoryStream())
                            {
                                byte[] bytes = null;
                                while (true)
                                {
                                    bytes = br.ReadBytes(1);
                                    if (bytes[0] == 0) break;
                                    ms.Write(bytes, 0, bytes.Length);
                                }

                                bytes = ms.ToArray();
                                ttarch2.files[i].fileName = Encoding.ASCII.GetString(bytes);

                                string ext = GetExtension(ttarch2.files[i].fileName);
                                if (!string.IsNullOrEmpty(ext) && !ttarch2.fileFormats.Contains(ext))
                                {
                                    ttarch2.fileFormats.Add(ext);
                                }
                            }

                            br.BaseStream.Seek(pos, SeekOrigin.Begin);
                        }
                    }
                }

                // 创建 ArchiveInfo
                var info = new ArchiveInfo
                {
                    FilePath = path,
                    FileName = Path.GetFileName(path),
                    FileCount = ttarch2.files.Length,
                    Files = ttarch2.files,
                    FileExtensions = ttarch2.fileFormats
                };

                // 收集文件扩展名
                foreach (var ext in ttarch2.fileFormats)
                {
                    lock (_allFileExtensions)
                    {
                        _allFileExtensions.Add(ext);
                    }
                }

                return info;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // 使用与Unpacker相同的解压逻辑
        private byte[] DecompressBlockUnpacker(byte[] bytes, int algorithmCompress, ulong chunkSize)
        {
            try
            {
                switch (algorithmCompress)
                {
                    case 0: // ZLib
                        using (Stream inMemoryStream = new MemoryStream(bytes))
                        using (Joveler.ZLibWrapper.ZLibStream inZStream = new Joveler.ZLibWrapper.ZLibStream(inMemoryStream, Joveler.ZLibWrapper.ZLibMode.Decompress))
                        using (MemoryStream outMemoryStream = new MemoryStream())
                        {
                            inZStream.Flush();
                            outMemoryStream.Flush();
                            inZStream.CopyTo(outMemoryStream);
                            return outMemoryStream.ToArray();
                        }
                    case 1: // Deflate
                        using (MemoryStream decompressedMemoryStream = new MemoryStream(bytes))
                        using (System.IO.Compression.DeflateStream decompressStream = new System.IO.Compression.DeflateStream(decompressedMemoryStream, System.IO.Compression.CompressionMode.Decompress))
                        using (MemoryStream memOutStream = new MemoryStream())
                        {
                            decompressStream.CopyTo(memOutStream);
                            return memOutStream.ToArray();
                        }
                    case 2: // Oodle
                        long decBufSize = chunkSize > 0 ? (long)chunkSize : bytes.Length;
                        byte[] retBytes = new byte[decBufSize];
                        int size = OodleTools.Imports.OodleLZ_Decompress(bytes, bytes.Length, retBytes, decBufSize, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3);
                        if (size > 0)
                        {
                            byte[] tmp = new byte[size];
                            Array.Copy(retBytes, 0, tmp, 0, tmp.Length);
                            return tmp;
                        }
                        return null;
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private string GetExtension(string fileName)
        {
            int dotIndex = fileName.LastIndexOf('.');
            if (dotIndex > 0 && dotIndex < fileName.Length - 1)
            {
                return fileName.Substring(dotIndex);
            }
            return "";
        }

        private void UpdateScanProgress(int current, int total, string fileName)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int, int, string>(UpdateScanProgress), current, total, fileName);
                return;
            }

            // 只更新文本，进度条值由定时器控制
            lblScanProgress.Text = $"Scanning... {current}/{total} - {fileName}";

            // 实时更新进度条到当前完成数量（但不超过显示值）
            int targetValue = Math.Min(current, scanProgressBar.Maximum);
            if (targetValue > _displayProgress)
            {
                _displayProgress = targetValue;
                scanProgressBar.Value = _displayProgress;
            }
        }

        private void UpdateFileExtensionsList()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateFileExtensionsList));
                return;
            }

            var sortedExtensions = _allFileExtensions.OrderBy(e => e).ToList();
            cmbFileExtension.Items.Clear();
            cmbFileExtension.Items.Add("All");
            foreach (var ext in sortedExtensions)
            {
                cmbFileExtension.Items.Add(ext);
            }
            cmbFileExtension.SelectedIndex = 0;
        }

        private void UpdateArchivesListView()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateArchivesListView));
                return;
            }

            archivesListView.Items.Clear();

            foreach (var archive in _scannedArchives)
            {
                var item = new ListViewItem(archive.FileName);
                item.SubItems.Add(archive.FileCount.ToString());
                item.Tag = archive;
                archivesListView.Items.Add(item);
            }

            lblArchiveCount.Text = $"Archives: {_scannedArchives.Count}";
        }

        private void ResetUIState()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(ResetUIState));
                return;
            }

            lblScanning.Visible = false;
            // 不要重置进度条，保持显示完成状态
            // scanProgressBar.Value = 0;
            Enabled = true;
        }

        private void archivesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 清除搜索高亮
            foreach (ListViewItem item in archivesListView.Items)
            {
                item.BackColor = System.Drawing.Color.White;
            }

            // 移除来源列（如果存在）
            if (filesListView.Columns.Count > 3)
            {
                filesListView.Columns.RemoveAt(3);
            }

            if (archivesListView.SelectedItems.Count == 0)
            {
                _selectedArchive = null;
                filesListView.Items.Clear();
                lblFileInfo.Text = "Select an archive or search in all files";
                return;
            }

            var selectedItem = archivesListView.SelectedItems[0];
            _selectedArchive = selectedItem.Tag as ArchiveInfo;

            if (_selectedArchive != null)
            {
                DisplayFilesList(_selectedArchive);
            }
        }

        private void archivesListView_MouseClick(object sender, MouseEventArgs e)
        {
            // 检查点击是否在空白区域（没有选中item）
            ListViewItem clickedItem = archivesListView.GetItemAt(e.X, e.Y);
            if (clickedItem == null)
            {
                // 点击了空白区域，清除选择
                archivesListView.SelectedItems.Clear();
                _selectedArchive = null;
                filesListView.Items.Clear();
                lblFileInfo.Text = "Select an archive or search in all files";
            }
        }

        private void DisplayFilesList(ArchiveInfo archive)
        {
            filesListView.Items.Clear();

            string selectedExtension = cmbFileExtension.SelectedItem?.ToString();
            bool filterByExtension = selectedExtension != "All" && !string.IsNullOrEmpty(selectedExtension);

            var filesToDisplay = archive.Files;

            // 按扩展名过滤
            if (filterByExtension)
            {
                filesToDisplay = filesToDisplay
                    .Where(f => GetExtension(f.fileName) == selectedExtension)
                    .ToArray();
            }

            // 按文件名搜索过滤
            string searchText = txtSearchFiles.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                filesToDisplay = filesToDisplay
                    .Where(f => f.fileName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToArray();
            }

            foreach (var file in filesToDisplay)
            {
                var item = new ListViewItem(file.fileName);
                item.SubItems.Add(FormatFileSize(file.fileSize));
                item.SubItems.Add($"0x{file.fileOffset:X8}");
                filesListView.Items.Add(item);
            }

            lblFileInfo.Text = $"{archive.FileName} - {filesToDisplay.Length} file(s)";
        }

        private string FormatFileSize(int bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            else if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:F2} KB";
            else
                return $"{bytes / (1024.0 * 1024.0):F2} MB";
        }

        private void btnSearchArchive_Click(object sender, EventArgs e)
        {
            string searchText = txtSearchArchive.Text.Trim();

            // 保存当前选中项
            ArchiveInfo selectedBefore = _selectedArchive;

            // 清空并重新填充列表
            archivesListView.Items.Clear();

            foreach (var archive in _scannedArchives)
            {
                bool matches = string.IsNullOrEmpty(searchText) ||
                    archive.FileName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    archive.FilePath.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                if (matches)
                {
                    var item = new ListViewItem(archive.FileName);
                    item.SubItems.Add(archive.FileCount.ToString());
                    item.Tag = archive;
                    archivesListView.Items.Add(item);

                    // 恢复之前选中的项
                    if (selectedBefore != null && archive.FileName == selectedBefore.FileName)
                    {
                        item.Selected = true;
                        item.Focused = true;
                    }
                }
            }
        }

        private void btnSearchFiles_Click(object sender, EventArgs e)
        {
            string searchText = txtSearchFiles.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                // 清空搜索，恢复显示
                if (_selectedArchive != null)
                {
                    DisplayFilesList(_selectedArchive);
                }
                return;
            }

            // 总是在所有archives中搜索文件
            // 这样用户不需要清除archive选择就可以搜索所有文件
            SearchAllArchives(searchText);
        }

        private void SearchAllArchives(string searchText)
        {
            filesListView.Items.Clear();

            // 获取当前选择的文件扩展名过滤
            string selectedExtension = cmbFileExtension.SelectedItem?.ToString();
            bool filterByExtension = selectedExtension != "All" && !string.IsNullOrEmpty(selectedExtension);

            string filterText = filterByExtension ? $" (extension: {selectedExtension})" : "";
            lblFileInfo.Text = $"Searching for \"{searchText}\"{filterText} in all archives...";

            int totalMatches = 0;
            List<ArchiveInfo> matchingArchives = new List<ArchiveInfo>();

            foreach (var archive in _scannedArchives)
            {
                int matchCount = 0;
                List<ListViewItem> matchingItems = new List<ListViewItem>();

                foreach (var file in archive.Files)
                {
                    // 检查搜索词匹配
                    bool matchesSearch = file.fileName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                    // 检查文件扩展名匹配
                    bool matchesExtension = true;
                    if (filterByExtension)
                    {
                        matchesExtension = GetExtension(file.fileName) == selectedExtension;
                    }

                    // 同时满足搜索词和扩展名过滤
                    if (matchesSearch && matchesExtension)
                    {
                        var item = new ListViewItem(file.fileName);
                        item.SubItems.Add(FormatFileSize(file.fileSize));
                        item.SubItems.Add($"0x{file.fileOffset:X8}");
                        item.SubItems.Add(archive.FileName); // 添加来源archive
                        matchingItems.Add(item);
                        matchCount++;
                    }
                }

                if (matchCount > 0)
                {
                    matchingArchives.Add(archive);
                    foreach (var item in matchingItems)
                    {
                        filesListView.Items.Add(item);
                    }
                    totalMatches += matchCount;
                }
            }

            // 添加来源列
            if (filesListView.Columns.Count == 3)
            {
                filesListView.Columns.Add("Source Archive", 150);
            }

            // 高亮显示包含匹配结果的archive
            foreach (ListViewItem item in archivesListView.Items)
            {
                var archive = item.Tag as ArchiveInfo;
                if (archive != null)
                {
                    item.BackColor = matchingArchives.Contains(archive) ?
                        System.Drawing.Color.LightYellow : System.Drawing.Color.White;
                }
            }

            lblFileInfo.Text = $"Found {totalMatches} file(s) matching \"{searchText}\" in {matchingArchives.Count} archive(s)";
        }

        private void cmbFileExtension_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 清除搜索高亮
            foreach (ListViewItem item in archivesListView.Items)
            {
                item.BackColor = System.Drawing.Color.White;
            }

            // 移除来源列（如果存在）
            if (filesListView.Columns.Count > 3)
            {
                filesListView.Columns.RemoveAt(3);
            }

            // 检查是否有搜索词
            string searchText = txtSearchFiles.Text.Trim();

            if (!string.IsNullOrEmpty(searchText))
            {
                // 如果有搜索词，重新执行搜索（会应用新的扩展名过滤）
                SearchAllArchives(searchText);
            }
            else if (_selectedArchive != null)
            {
                // 如果没有搜索词但有选中的archive，显示该archive的文件（应用新的扩展名过滤）
                DisplayFilesList(_selectedArchive);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void exportResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_scannedArchives.Count == 0)
            {
                MessageBox.Show("No scan results to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            sfd.FileName = "ttarch2_scan_results.csv";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ExportResults(sfd.FileName);
            }
        }

        private void ExportResults(string filePath)
        {
            try
            {
                bool isCsv = Path.GetExtension(filePath).ToLower() == ".csv";
                StringBuilder sb = new StringBuilder();

                if (isCsv)
                {
                    sb.AppendLine("Archive Name,Archive Path,File Count,File Name,File Size,File Offset");
                }
                else
                {
                    sb.AppendLine("TTArch2 Scanner Results");
                    sb.AppendLine("========================");
                    sb.AppendLine();
                }

                foreach (var archive in _scannedArchives)
                {
                    if (!isCsv)
                    {
                        sb.AppendLine($"Archive: {archive.FileName}");
                        sb.AppendLine($"Path: {archive.FilePath}");
                        sb.AppendLine($"Files: {archive.FileCount}");
                        sb.AppendLine("--------------------------------------------------");
                    }

                    foreach (var file in archive.Files)
                    {
                        if (isCsv)
                        {
                            sb.AppendLine($"\"{archive.FileName}\",\"{archive.FilePath}\",{archive.FileCount},\"{file.fileName}\",{file.fileSize},{file.fileOffset}");
                        }
                        else
                        {
                            sb.AppendLine($"  {file.fileName} ({FormatFileSize(file.fileSize)}) - Offset: 0x{file.fileOffset:X8}");
                        }
                    }

                    if (!isCsv)
                    {
                        sb.AppendLine();
                    }
                }

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"Results exported to:\n{filePath}", "Export Complete",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting results: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Ttarch2Scanner_FormClosing(object sender, FormClosingEventArgs e)
        {
            _scanCancellationTokenSource?.Cancel();
            _progressAnimationTimer?.Stop();
        }
    }
}
