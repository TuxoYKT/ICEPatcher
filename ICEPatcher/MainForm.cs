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
        }

        public System.Windows.Forms.ProgressBar MainFormProgressBar
        {
            get { return progressBar1; }
        }

        private void inputFileBrowseButton_Click(object sender, EventArgs e)
        {
            string inputFile = icePatcherCommon.OpenFolder();

            if (inputFile != null) { 
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
            string inputFile = inputFileTextBox.Text;

            for (int i = 0; i < patchesListBox.Items.Count; i++)
            {
                if (patchesListBox.GetItemCheckState(i) == CheckState.Checked)
                {
                    string checkedItem = patchesListBox.Items[i].ToString();
                    string exportsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Export", checkedItem);
                    if (!Directory.Exists(exportsPath))
                    {
                        Directory.CreateDirectory(exportsPath);
                    }

                    Patching.ApplyPatch(checkedItem, exportsPath);
                }
            }
        }

        private async void patchButton_Click(object sender, EventArgs e)
        {
            patchButton.Enabled = false;

            string inputFile = inputFileTextBox.Text;

            Patching.SetPSO2BinPath(inputFile);

            if (string.IsNullOrEmpty(inputFile))
            {
                MessageBox.Show("pso2_bin folder not selected.", "ICEPatcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                patchButton.Enabled = true;
                return;
            }

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
            MessageBox.Show("Patches applied successfully.", "ICEPatcher", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

            if (!Directory.Exists(patchPath)) {
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
    }
}
