using System;
using System.Windows.Forms;
using Zamboni;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ICEPatcher
{
    public partial class MainForm : Form
    {
        private ICEPatcherCommon icePatcherCommon;

        public MainForm()
        {
            InitializeComponent();
            icePatcherCommon = new ICEPatcherCommon(this);
            refreshButton_Click(null, null);
            Patching.progressBar = progressBar1;
        }

        public System.Windows.Forms.ProgressBar MainFormProgressBar
        {
            get { return progressBar1; }
        }

        private void inputFileBrowseButton_Click(object sender, EventArgs e)
        {
            string inputFile = icePatcherCommon.OpenFolder();

            if (inputFile != null)
            {
                inputFileTextBox.Text = inputFile;
                statusLabel.Text = "Ready";
            }
        }

        private void UpdateProgressBar()
        {
            for (int i = 0; i < patchesListBox.Items.Count; i++)
            {
                if (patchesListBox.GetItemCheckState(i) == CheckState.Checked)
                {
                    string checkedItem = patchesListBox.Items[i].ToString();
                    int filesCount = icePatcherCommon.GetFilesCount(checkedItem) - 1;
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
                    DialogResult result = MessageBox.Show("Backup disabled. Patches will be applied without backup. Do you wish to continue?", "Warning", MessageBoxButtons.YesNo);
                    if (result == DialogResult.No)
                    {
                        patchButton.Enabled = true;
                        return;
                    }
                    Patching.AllowBackup = false;
                }
                else
                {
                    DialogResult result = MessageBox.Show("Backup enabled. They will be saved in \"Backups\" folder." +
                        "Please make sure your game installation doesn't have any modified files.\n" +
                        "Also please note that backups may become outdated if a game update changes files.\n" +
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

            patchButton.Enabled = true;
        }




        private List<bool> checkedStateList = new List<bool>();

        private void refreshButton_Click(object sender, EventArgs e)
        {
            SaveCheckedState();
            var previousItems = patchesListBox.Items.Cast<object>().ToList();
            patchesListBox.Items.Clear();
            string[] patches = icePatcherCommon.GetPatches();
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
    }
}
