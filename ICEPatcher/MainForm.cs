using ICEPatcher.Create;
using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Windows.Forms;
using Zamboni;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ICEPatcher
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            refreshButton_Click(null, null);
            Patching.progressBar = progressBar1;
        }

        public System.Windows.Forms.ProgressBar MainFormProgressBar
        {
            get { return progressBar1; }
        }

        private void inputFileBrowseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    inputFileTextBox.Text = dialog.SelectedPath;
                    statusLabel.Text = "Ready";
                }
            }
        }

        private void UpdateProgressBar()
        {
            for (int i = 0; i < patchesListBox.Items.Count; i++)
            {
                if (patchesListBox.GetItemCheckState(i) == CheckState.Checked)
                {
                    string checkedItem = patchesListBox.Items[i].ToString();
                    int filesCount = GetFilesCount(checkedItem) - 1;
                    if (filesCount > 0)
                    {
                        progressBar1.Maximum += filesCount;
                    }
                }
            }
        }
        private void ApplyPatches()
        {

            string currentTime = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            string backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup", currentTime);

            Patching.SetBackupPath(backupPath);

            for (int i = 0; i < patchesListBox.Items.Count; i++)
            {
                if (patchesListBox.GetItemCheckState(i) == CheckState.Checked)
                {
                    string checkedItem = patchesListBox.Items[i].ToString();

                    if (exportCheckBox.Checked)
                    {
                        string exportsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export", checkedItem);
                        if (!Directory.Exists(exportsPath))
                        {
                            Directory.CreateDirectory(exportsPath);
                        }

                        Patching.ApplyPatch(checkedItem, exportsPath);
                    }
                    else
                    {
                        Patching.ApplyPatch(checkedItem);
                    }

                }
            }
        }

        private async void patchButton_Click(object sender, EventArgs e)
        {
            patchButton.Enabled = false;

            string inputFile = inputFileTextBox.Text;

            if (string.IsNullOrEmpty(inputFile))
            {
                MessageBox.Show("pso2_bin folder not selected.", "ICEPatcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                patchButton.Enabled = true;
                return;
            }

            if (!exportCheckBox.Checked)
            {
                if (!backupCheckBox.Checked)
                {
                    DialogResult result = MessageBox.Show("Backup disabled. Patches will be applied without a backup.\n\nDo you wish to continue?", "Warning", MessageBoxButtons.YesNo);
                    if (result == DialogResult.No)
                    {
                        patchButton.Enabled = true;
                        return;
                    }
                    Patching.AllowBackup = false;
                }
                else
                {
                    DialogResult result = MessageBox.Show("Backup enabled. They will be saved in \"Backup\" folder.\n" +
                        "To restore a backup, manually copy the contents of the folder to your game installation.\n" +
                        "Please make sure that you haven't applied any patches using this tool on your game installation yet.\n" +
                        "Also please note that backups may become outdated if a game update overwrites them.\n\n" +
                        "Do you wish to continue?", "Caution", MessageBoxButtons.YesNo);
                    if (result == DialogResult.No)
                    {
                        patchButton.Enabled = true;
                        return;
                    }
                    Patching.AllowBackup = true;
                }
            }



            Patching.SetPSO2BinPath(inputFile);

            if (patchesListBox.CheckedItems.Count == 0)
            {
                MessageBox.Show("No patches selected.", "ICEPatcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                patchButton.Enabled = true;
                return;
            }

            if (exportCheckBox.Checked)
                statusLabel.Text = "Exporting patches...";
            else
                statusLabel.Text = "Applying patches...";

            progressBar1.Value = 0;
            progressBar1.Step = 1;

            UpdateProgressBar();
            await Task.Run(() =>
            {
                ApplyPatches();
            });

            progressBar1.Value = progressBar1.Maximum;

            statusLabel.Text = "Done";
            if (exportCheckBox.Checked)
            {
                MessageBox.Show("Patches exported successfully.", "ICEPatcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Patches applied successfully.", "ICEPatcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (Patching.AllowBackup)
            {
                MessageBox.Show("Backup created in \"Backup\" folder.\n" + Patching.GetBackupPath(), "ICEPatcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            patchButton.Enabled = true;
        }


        public string[] GetPatches()
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string patchPath = Path.Combine(executablePath, "Patches");

            try
            {
                string[] subFolders = Directory.GetDirectories(patchPath);
                string[] folderNames = new string[subFolders.Length];
                for (int i = 0; i < subFolders.Length; i++)
                {
                    folderNames[i] = Path.GetFileName(subFolders[i]);
                }

                string[] zipFiles = Directory.GetFiles(patchPath, "*.zip");

                string[] zipNames = new string[zipFiles.Length];
                for (int i = 0; i < zipFiles.Length; i++)
                {
                    zipNames[i] = Path.GetFileName(zipFiles[i]);
                }

                string[] allNames = new string[subFolders.Length + zipFiles.Length];
                Array.Copy(zipNames, allNames, zipNames.Length);
                Array.Copy(folderNames, 0, allNames, zipNames.Length, subFolders.Length);

                return allNames;
            }
            catch (Exception err)
            {
                Debug.WriteLine("Error reading folder: " + err.Message);
            }

            return null;
        }

        public int GetFilesCount(string patchDir)
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string patchPath = Path.Combine(executablePath, "Patches", patchDir);

            if (patchDir.EndsWith(".zip"))
            {
                using (ZipArchive archive = ZipFile.OpenRead(patchPath))
                {
                    return archive.Entries.Count;
                }
            }
            else
            {
                return Directory.GetFiles(patchPath, "*.*", SearchOption.AllDirectories).ToList().Count;
            }
        }

        private List<bool> checkedStateList = new List<bool>();

        private void refreshButton_Click(object sender, EventArgs e)
        {
            SaveCheckedState();
            var previousItems = patchesListBox.Items.Cast<object>().ToList();
            patchesListBox.Items.Clear();
            string[] patches = GetPatches();
            if (patches != null)
            {
                patchesListBox.Items.AddRange(patches);

            }
            UpdateCheckedState(previousItems);
        }

        private void SaveCheckedState()
        {
            checkedStateList.Clear();
            foreach (var item in patchesListBox.Items)
            {
                checkedStateList.Add(patchesListBox.CheckedItems.Contains(item));
            }
        }

        private void UpdateCheckedState(List<object> previousItems)
        {
            for (int i = 0; i < patchesListBox.Items.Count; i++)
            {
                var item = patchesListBox.Items[i];
                if (previousItems.Contains(item))
                {
                    patchesListBox.SetItemChecked(i, checkedStateList[previousItems.IndexOf(item)]);
                }
            }
        }

        private void openFolderButton_Click(object sender, EventArgs e)
        {
            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string patchPath = Path.Combine(executablePath, "Patches");

            if (!Directory.Exists(patchPath))
            {
                DialogResult result = MessageBox.Show("The Patches directory does not exist. Do you want to create it?", "Create Patches Directory", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    Directory.CreateDirectory(patchPath);
                    System.Diagnostics.Process.Start("explorer.exe", patchPath);
                }
                return;
            }

            // open folder patchPath in file explorer
            System.Diagnostics.Process.Start("explorer.exe", patchPath);
        }

        private void exportCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (exportCheckBox.Checked)
            {
                backupCheckBox.Enabled = false;
                backupCheckBox.Checked = false;
            }
            else
            {
                backupCheckBox.Enabled = true;
            }
        }

        private void createPatchButton_Click(object sender, EventArgs e)
        {
            if (inputFileTextBox.Text == "")
            {
                MessageBox.Show("pso2_bin folder not selected.", "ICEPatcher", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Patching.SetPSO2BinPath(inputFileTextBox.Text);

            CreateForm createForm = new CreateForm();
            createForm.ShowDialog();
        }
    }
}
