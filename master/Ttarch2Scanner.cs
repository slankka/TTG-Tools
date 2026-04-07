using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using TTG_Tools.ClassesStructs;
using TTG_Tools.Graphics.Swizzles;

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

        private void filesListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (filesListView.SelectedItems.Count == 0)
            {
                ClearPreview();
                return;
            }

            var selectedItem = filesListView.SelectedItems[0];
            string fileName = selectedItem.Text;

            // 找到对应的文件信息
            Ttarch2Class.Ttarch2files? fileInfo = null;
            ArchiveInfo archiveInfo = null;

            // 首先检查是否有来源列（搜索结果）
            if (selectedItem.SubItems.Count > 3)
            {
                string archiveName = selectedItem.SubItems[3].Text;
                archiveInfo = _scannedArchives.FirstOrDefault(a => a.FileName == archiveName);
                if (archiveInfo != null)
                {
                    fileInfo = archiveInfo.Files.FirstOrDefault(f => f.fileName == fileName);
                }
            }
            else if (_selectedArchive != null)
            {
                fileInfo = _selectedArchive.Files.FirstOrDefault(f => f.fileName == fileName);
                archiveInfo = _selectedArchive;
            }

            if (fileInfo.HasValue && !string.IsNullOrEmpty(fileInfo.Value.fileName) && archiveInfo != null)
            {
                ShowFilePreview(archiveInfo, fileInfo.Value, fileName);
            }
            else
            {
                ClearPreview();
            }
        }

        private async void ShowFilePreview(ArchiveInfo archive, Ttarch2Class.Ttarch2files? file, string fileName)
        {
            lblPreviewInfo.Text = $"Loading: {fileName}...";

            if (!file.HasValue)
            {
                ShowPreviewError("File not found");
                return;
            }

            try
            {
                // 获取游戏密钥
                byte[] key = null;
                if (cmbGameKey.SelectedIndex > 0 && cmbGameKey.SelectedIndex - 1 < MainMenu.gamelist.Count)
                {
                    key = MainMenu.gamelist[cmbGameKey.SelectedIndex - 1].key;
                }

                // 异步读取文件内容
                byte[] fileData = await Task.Run(() => ExtractFileData(archive, file.Value, key));

                if (fileData == null || fileData.Length == 0)
                {
                    ShowPreviewError($"Failed to load file: {fileName}");
                    return;
                }

                // 根据文件扩展名决定如何显示
                string ext = GetExtension(fileName).ToLower();

                if (ext == ".dds" || IsDDSFile(fileData))
                {
                    // 尝试显示DDS图像
                    Bitmap preview = TryLoadDDSPreview(fileData);
                    if (preview != null)
                    {
                        ShowImagePreview(preview, $"{fileName} ({fileData.Length} bytes)");
                    }
                    else
                    {
                        ShowHexPreview(fileData, fileName);
                    }
                }
                else if (ext == ".d3dtx")
                {
                    // 处理d3dtx容器文件
                    Bitmap preview = TryLoadD3dtxPreview(fileData);
                    if (preview != null)
                    {
                        ShowImagePreview(preview, $"{fileName} ({fileData.Length} bytes)");
                    }
                    else
                    {
                        ShowHexPreview(fileData, fileName);
                    }
                }
                else if (ext == ".landb")
                {
                    // 处理landb数据库文件
                    ShowLandbPreview(fileData, fileName);
                }
                else if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".tga" || ext == ".tiff")
                {
                    // 尝试直接加载图像
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(fileData))
                        {
                            Bitmap bitmap = new Bitmap(ms);
                            ShowImagePreview((Bitmap)bitmap.Clone(), $"{fileName} ({fileData.Length} bytes)");
                            bitmap.Dispose();
                        }
                    }
                    catch
                    {
                        ShowHexPreview(fileData, fileName);
                    }
                }
                else
                {
                    // 显示十六进制视图
                    ShowHexPreview(fileData, fileName);
                }
            }
            catch (Exception ex)
            {
                ShowPreviewError($"Error: {ex.Message}");
            }
        }

        private byte[] ExtractFileData(ArchiveInfo archive, Ttarch2Class.Ttarch2files file, byte[] key)
        {
            try
            {
                using (FileStream fs = new FileStream(archive.FilePath, FileMode.Open, FileAccess.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    // 读取文件头部判断格式
                    fs.Seek(0, SeekOrigin.Begin);
                    byte[] header = br.ReadBytes(4);
                    string headerStr = Encoding.ASCII.GetString(header);

                    bool isEncrypted = (headerStr == "ECTT" || headerStr == "eCTT");
                    bool isCompressed = headerStr != "NCTT";

                    if (isCompressed)
                    {
                        // 需要解压缩
                        // 跳到压缩数据区
                        br.ReadBytes(8); // 跳过头部

                        if (headerStr == "eCTT" || headerStr == "zCTT")
                        {
                            br.ReadInt32();
                        }

                        uint chunkSize = br.ReadUInt32();
                        int blocksCount = br.ReadInt32();
                        br.ReadBytes(8 * (blocksCount + 1)); // 跳过块偏移

                        // 计算文件在哪个块中
                        long basePos = br.BaseStream.Position;
                        int targetChunk = (int)(file.fileOffset / chunkSize);
                        ulong offsetInChunk = file.fileOffset % ((ulong)chunkSize);

                        // 读取并解压所有需要的块
                        using (MemoryStream ms = new MemoryStream())
                        {
                            for (int i = 0; i <= targetChunk; i++)
                            {
                                int blockSize = (int)br.ReadInt32();
                                long blockStart = br.BaseStream.Position;

                                byte[] blockData = br.ReadBytes(blockSize);

                                if (isEncrypted && key != null)
                                {
                                    BlowFishCS.BlowFish dec = new BlowFishCS.BlowFish(key, 7);
                                    blockData = dec.Crypt_ECB(blockData, 7, true);
                                }

                                byte[] decompressed = DecompressBlockUnpacker(blockData,
                                    (headerStr == "eCTT" || headerStr == "zCTT") ? 2 : 1, chunkSize);

                                if (decompressed != null)
                                {
                                    ms.Write(decompressed, 0, decompressed.Length);
                                }

                                br.BaseStream.Seek(blockStart + blockSize, SeekOrigin.Begin);
                            }

                            byte[] allData = ms.ToArray();
                            if (file.fileOffset + (ulong)file.fileSize <= (ulong)allData.Length)
                            {
                                byte[] result = new byte[file.fileSize];
                                Array.Copy(allData, (int)file.fileOffset, result, 0, file.fileSize);
                                return result;
                            }
                        }
                    }
                    else
                    {
                        // 未压缩，直接读取
                        br.BaseStream.Seek((long)file.fileOffset, SeekOrigin.Begin);
                        return br.ReadBytes(file.fileSize);
                    }
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        private bool IsDDSFile(byte[] data)
        {
            return data.Length >= 4 && Encoding.ASCII.GetString(data, 0, 4) == "DDS ";
        }

        private Bitmap TryLoadDDSPreview(byte[] ddsData)
        {
            try
            {
                byte[] processedData = ddsData;

                // 如果启用了Switch de-swizzle，先进行de-swizzle处理
                if (chkSwitchSwizzle.Checked && ddsData.Length >= 128)
                {
                    // 读取DDS头部信息
                    int texWidth = BitConverter.ToInt32(ddsData, 16);
                    int texHeight = BitConverter.ToInt32(ddsData, 12);
                    int fourCc = BitConverter.ToInt32(ddsData, 84);
                    int pixelFormat = BitConverter.ToInt32(ddsData, 76); // DXGI format

                    // 将fourCc和DXGI格式转换为NintendoSwitch code
                    int formatCode = GetFormatCodeForSwizzle(fourCc, pixelFormat);

                    if (formatCode > 0)
                    {
                        // 跳过DDS头128字节的数据进行de-swizzle
                        byte[] textureData = new byte[ddsData.Length - 128];
                        Array.Copy(ddsData, 128, textureData, 0, textureData.Length);

                        // 应用Nintendo Switch de-swizzle
                        textureData = NintendoSwitch.NintendoSwizzle(textureData, texWidth, texHeight, formatCode, true);

                        // 重新组合DDS
                        processedData = new byte[128 + textureData.Length];
                        Array.Copy(ddsData, 0, processedData, 0, 128); // 复制DDS头
                        Array.Copy(textureData, 0, processedData, 128, textureData.Length); // 复制de-swizzle后的数据
                    }
                }

                byte[] pixels;
                int width, height;
                if (TryDecodeDdsToBgra(processedData, out pixels, out width, out height))
                {
                    return BuildBitmapFromRgbaBuffer(pixels, width, height);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private int GetFormatCodeForSwizzle(int fourCc, int dxgiFormat)
        {
            // 将DDS格式转换为NintendoSwitch code
            // BC1 = 0x40, BC2 = 0x41, BC3 = 0x42, BC4 = 0x43, BC5 = 0x44, BC6 = 0x45, BC7 = 0x46

            if (fourCc == 0x31545844) // DXT1 = BC1
                return 0x40;
            if (fourCc == 0x33545844) // DXT3 = BC2
                return 0x41;
            if (fourCc == 0x35545844) // DXT5 = BC3
                return 0x42;

            // 基于DXGI格式判断
            switch (dxgiFormat)
            {
                case 70: // DXGI_FORMAT_BC1_UNORM
                case 71: // DXGI_FORMAT_BC1_UNORM_SRGB
                    return 0x40;
                case 72: // DXGI_FORMAT_BC2_UNORM
                case 73: // DXGI_FORMAT_BC2_UNORM_SRGB
                    return 0x41;
                case 74: // DXGI_FORMAT_BC3_UNORM
                case 75: // DXGI_FORMAT_BC3_UNORM_SRGB
                    return 0x42;
                case 77: // DXGI_FORMAT_BC4_UNORM
                    return 0x43;
                case 78: // DXGI_FORMAT_BC5_UNORM
                    return 0x44;
                case 79: // DXGI_FORMAT_B5G6R5_UNORM
                case 82: // DXGI_FORMAT_BC6H_UF16
                case 83: // DXGI_FORMAT_BC6H_SF16
                case 84: // DXGI_FORMAT_BC7_UNORM
                case 85: // DXGI_FORMAT_BC7_UNORM_SRGB
                    // 这些格式可能需要不同的处理
                    break;
            }

            return 0; // 未知的格式
        }

        private Bitmap TryLoadD3dtxPreview(byte[] d3dtxData)
        {
            try
            {
                if (d3dtxData.Length < 4)
                {
                    return null;
                }

                string header = Encoding.ASCII.GetString(d3dtxData, 0, 4);

                // 检查是否是新的d3dtx格式
                int poz = 4;
                bool isNewFormat = (header == "5VSM" || header == "6VSM");

                if (isNewFormat)
                {
                    poz = 16;
                }

                // 读取元素数量
                if (poz + 4 > d3dtxData.Length)
                    return null;

                byte[] tmp = new byte[4];
                Array.Copy(d3dtxData, poz, tmp, 0, 4);
                int countElements = BitConverter.ToInt32(tmp, 0);
                poz += 4;

                // 跳过元素数组，查找纹理数据
                // 简化版本：直接查找DDS或PVR数据
                for (int offset = poz; offset < d3dtxData.Length - 128; offset++)
                {
                    // 查找DDS头
                    if (offset + 4 <= d3dtxData.Length)
                    {
                        string dataHeader = Encoding.ASCII.GetString(d3dtxData, offset, 4);

                        if (dataHeader == "DDS ")
                        {
                            // 找到DDS数据，尝试加载
                            int ddsSize = d3dtxData.Length - offset;
                            byte[] ddsData = new byte[ddsSize];
                            Array.Copy(d3dtxData, offset, ddsData, 0, ddsSize);

                            // 如果启用了Switch de-swizzle，应用它
                            if (chkSwitchSwizzle.Checked)
                            {
                                int width = BitConverter.ToInt32(ddsData, 16);
                                int height = BitConverter.ToInt32(ddsData, 12);
                                int fourCc = BitConverter.ToInt32(ddsData, 84);
                                int formatCode = GetFormatCodeForSwizzle(fourCc, 0);

                                if (formatCode > 0 && width > 0 && height > 0)
                                {
                                    // 只对纹理数据进行de-swizzle
                                    int headerSize = 128;
                                    if (ddsSize > headerSize)
                                    {
                                        byte[] textureData = new byte[ddsSize - headerSize];
                                        Array.Copy(ddsData, headerSize, textureData, 0, textureData.Length);

                                        textureData = NintendoSwitch.NintendoSwizzle(textureData, width, height, formatCode, true);

                                        // 重新组合DDS
                                        byte[] swizzledDds = new byte[headerSize + textureData.Length];
                                        Array.Copy(ddsData, 0, swizzledDds, 0, headerSize);
                                        Array.Copy(textureData, 0, swizzledDds, headerSize, textureData.Length);
                                        ddsData = swizzledDds;
                                    }
                                }
                            }

                            return TryLoadDDSPreview(ddsData);
                        }
                        else if (dataHeader == "PVR\0")
                        {
                            // PVR格式 - 暂不支持显示
                            // 可以考虑后续添加PVR支持
                            break;
                        }
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private void ShowLandbPreview(byte[] landbData, string fileName)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                int landbCount = 0; // Declare before using block for wider scope

                sb.AppendLine($"LAND Database: {fileName}");
                sb.AppendLine($"Size: {landbData.Length} bytes");
                sb.AppendLine();
                sb.AppendLine(new string('=', 70));

                using (MemoryStream ms = new MemoryStream(landbData))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    byte[] checkHeader = br.ReadBytes(4);
                    bool newFormat = (Encoding.ASCII.GetString(checkHeader) == "5VSM" || Encoding.ASCII.GetString(checkHeader) == "6VSM");
                    bool hasCRC64Langres = false;
                    bool isUnicode = false;
                    int pos = 4;

                    if (newFormat)
                    {
                        pos = 16;
                    }

                    br.BaseStream.Seek(pos, SeekOrigin.Begin);
                    int countBlocks = br.ReadInt32();
                    sb.AppendLine($"Format: {(newFormat ? "New" : "Old")}");
                    sb.AppendLine($"Blocks: {countBlocks}");
                    sb.AppendLine();

                    // 读取class信息
                    string[] classes = new string[countBlocks];
                    for (int i = 0; i < countBlocks; i++)
                    {
                        byte[] tmp = br.ReadBytes(8);
                        classes[i] = BitConverter.ToString(tmp);
                        if (classes[i] == "B0-9F-D8-63-34-02-4F-00") hasCRC64Langres = true;
                        if (classes[i] == "53-DC-A5-33-DB-D6-DC-7E") isUnicode = true;
                        br.ReadBytes(4); // 跳过一些值
                    }

                    if (hasCRC64Langres)
                        sb.AppendLine("Has CRC64 Langres: Yes");
                    if (isUnicode)
                        sb.AppendLine("Unicode: Yes");
                    sb.AppendLine();

                    // 读取条目数量
                    int blockSize1 = br.ReadInt32();
                    br.ReadInt32(); // someValue1
                    int blockSize2 = br.ReadInt32();
                    landbCount = br.ReadInt32();

                    sb.AppendLine($"Entries: {landbCount}");
                    sb.AppendLine();
                    sb.AppendLine(new string('-', 70));
                    sb.AppendLine();

                    // 显示前100个条目（避免数据太多）
                    int displayCount = Math.Min(landbCount, 100);
                    long startPosition = br.BaseStream.Position;

                    for (int i = 0; i < displayCount; i++)
                    {
                        sb.AppendLine($"Entry #{i + 1}:");

                        uint wavID = br.ReadUInt32();
                        sb.AppendLine($"  ID: {wavID}");

                        byte[] tmp;

                        if (hasCRC64Langres)
                        {
                            ulong crc64 = br.ReadUInt64();
                            sb.AppendLine($"  CRC64: 0x{crc64:X16}");
                            br.ReadBytes(8); // anmName (8 bytes)
                        }
                        else
                        {
                            br.ReadInt32(); // zero1
                            int anmNameSize = br.ReadInt32();
                            tmp = br.ReadBytes(anmNameSize);
                            string anmName = Methods.DecodeGameText(tmp, false);
                            if (!string.IsNullOrEmpty(anmName))
                                sb.AppendLine($"  Animation: {anmName}");
                        }

                        uint anmID = br.ReadUInt32();
                        sb.AppendLine($"  Animation ID: {anmID}");

                        br.ReadInt32(); // zero2

                        // wavName
                        br.ReadInt32(); // blockWavNameSize
                        if (hasCRC64Langres)
                        {
                            br.ReadBytes(8); // wavNameSize = 8
                            sb.AppendLine($"  Audio: {Encoding.ASCII.GetString(br.ReadBytes(8))}");
                        }
                        else
                        {
                            int wavNameSize = br.ReadInt32();
                            tmp = br.ReadBytes(wavNameSize);
                            string wavName = Methods.DecodeGameText(tmp, false);
                            if (!string.IsNullOrEmpty(wavName))
                                sb.AppendLine($"  Audio: {wavName}");
                        }

                        br.ReadInt32(); // blockUnknownNameSize
                        int unknownNameSize = br.ReadInt32();
                        tmp = br.ReadBytes(unknownNameSize);
                        string unknownName = Methods.DecodeGameText(tmp, false);
                        if (!string.IsNullOrEmpty(unknownName))
                            sb.AppendLine($"  Unknown: {unknownName}");

                        br.ReadInt32(); // zero2

                        // actorName
                        br.ReadInt32(); // blockLangresSize
                        br.ReadInt32(); // blockActorNameSize
                        int actorNameSize = br.ReadInt32();
                        tmp = br.ReadBytes(actorNameSize);
                        string actorName = Methods.DecodeGameText(tmp, isUnicode);
                        if (MainMenu.settings.supportTwdNintendoSwitch && isUnicode)
                        {
                            actorName = Methods.isUTF8String(tmp) ? Encoding.UTF8.GetString(tmp) : actorName;
                        }
                        sb.AppendLine($"  Character: {actorName}");

                        // actorSpeech
                        br.ReadInt32(); // blockActorSpeechSize
                        int actorSpeechSize = br.ReadInt32();
                        tmp = br.ReadBytes(actorSpeechSize);
                        string actorSpeech = Methods.DecodeGameText(tmp, isUnicode);
                        if (MainMenu.settings.supportTwdNintendoSwitch && isUnicode)
                        {
                            actorSpeech = Methods.isUTF8String(tmp) ? Encoding.UTF8.GetString(tmp) : actorSpeech;
                        }

                        // 截断过长的对话文本
                        if (actorSpeech.Length > 200)
                        {
                            actorSpeech = actorSpeech.Substring(0, 200) + "...";
                        }
                        sb.AppendLine($"  Dialog: {actorSpeech}");

                        br.ReadInt32(); // blockSize
                        br.ReadInt32(); // someValue
                        if (isUnicode)
                        {
                            br.ReadInt32(); // blockSizeUni
                            int dataSize = br.ReadInt32() - 4;
                            if (dataSize > 0)
                                br.ReadBytes(dataSize); // someDataUni
                        }
                        br.ReadUInt32(); // flags

                        sb.AppendLine();
                    }

                    if (landbCount > displayCount)
                    {
                        sb.AppendLine($"... ({landbCount - displayCount} more entries)");
                    }
                }

                // 显示在文本框中
                txtHexViewer.Visible = true;
                pictureBoxPreview.Visible = false;
                txtHexViewer.Text = sb.ToString();
                lblPreviewInfo.Text = $"{fileName} ({landbData.Length} bytes, {landbCount} entries)";
            }
            catch (Exception ex)
            {
                ShowPreviewError($"Error reading landb file: {ex.Message}");
            }
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
            int dataOffset = 128;

            if (fourCc == 0x31545844) // DXT1
            {
                return DecodeDxt1(content, dataOffset, width, height, out pixels);
            }
            else if (fourCc == 0x33545844) // DXT3
            {
                return DecodeDxt3(content, dataOffset, width, height, out pixels);
            }
            else if (fourCc == 0x35545844) // DXT5
            {
                return DecodeDxt5(content, dataOffset, width, height, out pixels);
            }

            return false;
        }

        private bool DecodeDxt1(byte[] content, int dataOffset, int width, int height, out byte[] pixels)
        {
            pixels = new byte[width * height * 4];
            int blockCountX = (width + 3) / 4;
            int blockCountY = (height + 3) / 4;

            for (int y = 0; y < blockCountY; y++)
            {
                for (int x = 0; x < blockCountX; x++)
                {
                    int offset = dataOffset + (y * blockCountX + x) * 8;
                    if (offset + 8 > content.Length) break;

                    ushort color0 = BitConverter.ToUInt16(content, offset);
                    ushort color1 = BitConverter.ToUInt16(content, offset + 2);
                    uint codes = BitConverter.ToUInt32(content, offset + 4);

                    byte[] c0 = RGB565ToRGB888(color0);
                    byte[] c1 = RGB565ToRGB888(color1);

                    for (int py = 0; py < 4; py++)
                    {
                        for (int px = 0; px < 4; px++)
                        {
                            int pixelIndex = ((y * 4 + py) * width + (x * 4 + px)) * 4;
                            if (pixelIndex >= pixels.Length) continue;

                            int codeIndex = (py * 4 + px) * 2;
                            int code = (int)((codes >> codeIndex) & 0x03);

                            byte r, g, b, a = 255;
                            if (code == 0)
                            {
                                r = c0[0]; g = c0[1]; b = c0[2];
                            }
                            else if (code == 1)
                            {
                                r = c1[0]; g = c1[1]; b = c1[2];
                            }
                            else if (code == 2)
                            {
                                r = (byte)((2 * c0[0] + c1[0]) / 3);
                                g = (byte)((2 * c0[1] + c1[1]) / 3);
                                b = (byte)((2 * c0[2] + c1[2]) / 3);
                            }
                            else if (code == 3)
                            {
                                r = (byte)((c0[0] + 2 * c1[0]) / 3);
                                g = (byte)((c0[1] + 2 * c1[1]) / 3);
                                b = (byte)((c0[2] + 2 * c1[2]) / 3);
                            }
                            else
                            {
                                r = g = b = 0;
                            }

                            pixels[pixelIndex] = b;
                            pixels[pixelIndex + 1] = g;
                            pixels[pixelIndex + 2] = r;
                            pixels[pixelIndex + 3] = a;
                        }
                    }
                }
            }
            return true;
        }

        private bool DecodeDxt3(byte[] content, int dataOffset, int width, int height, out byte[] pixels)
        {
            pixels = new byte[width * height * 4];
            int blockCountX = (width + 3) / 4;
            int blockCountY = (height + 3) / 4;

            for (int y = 0; y < blockCountY; y++)
            {
                for (int x = 0; x < blockCountX; x++)
                {
                    int offset = dataOffset + (y * blockCountX + x) * 16;
                    if (offset + 16 > content.Length) break;

                    // 读取alpha数据（64位，每个像素4位）
                    ulong alphaData = BitConverter.ToUInt64(content, offset);

                    // 读取颜色数据
                    ushort color0 = BitConverter.ToUInt16(content, offset + 8);
                    ushort color1 = BitConverter.ToUInt16(content, offset + 10);
                    uint codes = BitConverter.ToUInt32(content, offset + 12);

                    byte[] c0 = RGB565ToRGB888(color0);
                    byte[] c1 = RGB565ToRGB888(color1);

                    for (int py = 0; py < 4; py++)
                    {
                        for (int px = 0; px < 4; px++)
                        {
                            int pixelIndex = ((y * 4 + py) * width + (x * 4 + px)) * 4;
                            if (pixelIndex >= pixels.Length) continue;

                            // Alpha
                            int alphaIndex = (py * 4 + px) * 4;
                            byte a = (byte)((alphaData >> alphaIndex) & 0x0F);
                            a = (byte)(a | (a << 4)); // 4位扩展到8位

                            // Color
                            int codeIndex = (py * 4 + px) * 2;
                            int code = (int)((codes >> codeIndex) & 0x03);

                            byte r, g, b;
                            if (code == 0)
                            {
                                r = c0[0]; g = c0[1]; b = c0[2];
                            }
                            else if (code == 1)
                            {
                                r = c1[0]; g = c1[1]; b = c1[2];
                            }
                            else if (code == 2)
                            {
                                r = (byte)((2 * c0[0] + c1[0]) / 3);
                                g = (byte)((2 * c0[1] + c1[1]) / 3);
                                b = (byte)((2 * c0[2] + c1[2]) / 3);
                            }
                            else
                            {
                                r = (byte)((c0[0] + 2 * c1[0]) / 3);
                                g = (byte)((c0[1] + 2 * c1[1]) / 3);
                                b = (byte)((c0[2] + 2 * c1[2]) / 3);
                            }

                            pixels[pixelIndex] = b;
                            pixels[pixelIndex + 1] = g;
                            pixels[pixelIndex + 2] = r;
                            pixels[pixelIndex + 3] = a;
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

            for (int y = 0; y < blockCountY; y++)
            {
                for (int x = 0; x < blockCountX; x++)
                {
                    int offset = dataOffset + (y * blockCountX + x) * 16;
                    if (offset + 16 > content.Length) break;

                    byte alpha0 = content[offset];
                    byte alpha1 = content[offset + 1];
                    ulong alphaCodes = BitConverter.ToUInt64(content, offset + 2) & 0x0000003FFFFFFFFFUL;

                    ushort color0 = BitConverter.ToUInt16(content, offset + 8);
                    ushort color1 = BitConverter.ToUInt16(content, offset + 10);
                    uint codes = BitConverter.ToUInt32(content, offset + 12);

                    byte[] c0 = RGB565ToRGB888(color0);
                    byte[] c1 = RGB565ToRGB888(color1);

                    for (int py = 0; py < 4; py++)
                    {
                        for (int px = 0; px < 4; px++)
                        {
                            int pixelIndex = ((y * 4 + py) * width + (x * 4 + px)) * 4;
                            if (pixelIndex >= pixels.Length) continue;

                            // Alpha interpolation
                            int alphaIndex = (py * 4 + px) * 3;
                            int alphaCode = (int)((alphaCodes >> alphaIndex) & 0x07);
                            byte a;
                            if (alphaCode == 0)
                            {
                                a = alpha0;
                            }
                            else if (alphaCode == 1)
                            {
                                a = alpha1;
                            }
                            else if (alphaCode < 6)
                            {
                                if (alpha0 > alpha1)
                                {
                                    a = (byte)(((8 - alphaCode) * alpha0 + (alphaCode - 1) * alpha1) / 7);
                                }
                                else
                                {
                                    a = (byte)(((6 - alphaCode) * alpha0 + (alphaCode - 1) * alpha1) / 5);
                                }
                            }
                            else
                            {
                                a = (alphaCode == 6) ? (byte)0 : (byte)255;
                            }

                            // Color interpolation
                            int codeIndex = (py * 4 + px) * 2;
                            int code = (int)((codes >> codeIndex) & 0x03);

                            byte r, g, b;
                            if (code == 0)
                            {
                                r = c0[0]; g = c0[1]; b = c0[2];
                            }
                            else if (code == 1)
                            {
                                r = c1[0]; g = c1[1]; b = c1[2];
                            }
                            else if (code == 2)
                            {
                                r = (byte)((2 * c0[0] + c1[0]) / 3);
                                g = (byte)((2 * c0[1] + c1[1]) / 3);
                                b = (byte)((2 * c0[2] + c1[2]) / 3);
                            }
                            else
                            {
                                r = (byte)((c0[0] + 2 * c1[0]) / 3);
                                g = (byte)((c0[1] + 2 * c1[1]) / 3);
                                b = (byte)((c0[2] + 2 * c1[2]) / 3);
                            }

                            pixels[pixelIndex] = b;
                            pixels[pixelIndex + 1] = g;
                            pixels[pixelIndex + 2] = r;
                            pixels[pixelIndex + 3] = a;
                        }
                    }
                }
            }
            return true;
        }

        private byte[] RGB565ToRGB888(ushort color565)
        {
            int r = (color565 >> 11) & 0x1F;
            int g = (color565 >> 5) & 0x3F;
            int b = color565 & 0x1F;

            return new byte[]
            {
                (byte)((r << 3) | (r >> 2)),  // R
                (byte)((g << 2) | (g >> 4)),  // G
                (byte)((b << 3) | (b >> 2))   // B
            };
        }

        private Bitmap BuildBitmapFromRgbaBuffer(byte[] pixels, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(
                new Rectangle(0, 0, width, height),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * height;
            byte[] rgbValues = new byte[bytes];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcIndex = (y * width + x) * 4;
                    int dstIndex = y * Math.Abs(bmpData.Stride) + x * 4;

                    if (srcIndex + 3 < pixels.Length)
                    {
                        rgbValues[dstIndex] = pixels[srcIndex];         // B
                        rgbValues[dstIndex + 1] = pixels[srcIndex + 1]; // G
                        rgbValues[dstIndex + 2] = pixels[srcIndex + 2]; // R
                        rgbValues[dstIndex + 3] = pixels[srcIndex + 3]; // A
                    }
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            bitmap.UnlockBits(bmpData);

            return bitmap;
        }

        private void ShowImagePreview(Bitmap bitmap, string info)
        {
            if (pictureBoxPreview.Image != null)
            {
                pictureBoxPreview.Image.Dispose();
            }

            pictureBoxPreview.Image = bitmap;
            pictureBoxPreview.Visible = true;
            txtHexViewer.Visible = false;

            lblPreviewInfo.Text = info;
        }

        private void ShowHexPreview(byte[] data, string fileName)
        {
            // 限制显示大小（最多64KB）
            int displaySize = Math.Min(data.Length, 65536);
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"File: {fileName}");
            sb.AppendLine($"Size: {data.Length} bytes");
            sb.AppendLine();
            sb.AppendLine("Hex View (first " + displaySize + " bytes):");
            sb.AppendLine(new string('-', 70));

            for (int i = 0; i < displaySize; i += 16)
            {
                sb.AppendFormat("{0:X8}: ", i);

                // Hex
                for (int j = 0; j < 16 && i + j < displaySize; j++)
                {
                    sb.AppendFormat("{0:X2} ", data[i + j]);
                }

                // Padding
                for (int j = 16; j > 0 && i + j > displaySize; j--)
                {
                    sb.Append("   ");
                }
                if (displaySize - i < 16)
                {
                    for (int j = displaySize - i; j < 16; j++)
                    {
                        sb.Append("   ");
                    }
                }

                sb.Append(" | ");

                // ASCII
                for (int j = 0; j < 16 && i + j < displaySize; j++)
                {
                    byte b = data[i + j];
                    sb.Append((b >= 32 && b <= 126) ? (char)b : '.');
                }

                sb.AppendLine();
            }

            if (data.Length > displaySize)
            {
                sb.AppendLine();
                sb.AppendLine($"... ({data.Length - displaySize} more bytes)");
            }

            txtHexViewer.Text = sb.ToString();
            txtHexViewer.Visible = true;
            pictureBoxPreview.Visible = false;

            lblPreviewInfo.Text = $"{fileName} ({data.Length} bytes)";
        }

        private void ShowPreviewError(string message)
        {
            txtHexViewer.Text = message;
            txtHexViewer.Visible = true;
            pictureBoxPreview.Visible = false;
            lblPreviewInfo.Text = "Error";
        }

        private void ClearPreview()
        {
            if (pictureBoxPreview.Image != null)
            {
                pictureBoxPreview.Image.Dispose();
                pictureBoxPreview.Image = null;
            }
            txtHexViewer.Clear();
            txtHexViewer.Visible = false;
            pictureBoxPreview.Visible = false;
            lblPreviewInfo.Text = "Preview";
        }
    }
}
