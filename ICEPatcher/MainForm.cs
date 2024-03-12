using System;
using System.Windows.Forms;
using Zamboni;

namespace ICEPatcher
{
    public partial class MainForm : Form
    {
        private ICEPatcherCommon icePatcherCommon = new ICEPatcherCommon();

        public MainForm()
        {
            InitializeComponent();
            Logger.SetTextBox(logTextBox);
            refreshButton_Click(null, null);
        }

        private void inputFileBrowseButton_Click(object sender, EventArgs e)
        {
            string inputFile = icePatcherCommon.OpenFolder();

            if (inputFile != null) { inputFileTextBox.Text = inputFile; }
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
                    icePatcherCommon.ApplyPatch(inputFile, checkedItem, progressBar1);
                }
            }
        }

        private async void patchButton_Click(object sender, EventArgs e)
        {
            patchButton.Enabled = false;

            string inputFile = inputFileTextBox.Text;

            if (string.IsNullOrEmpty(inputFile))
            {
                MessageBox.Show("pso2_bin folder not selected.", "ICE Patcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                patchButton.Enabled = true;
                return;
            }

            if (patchesListBox.CheckedItems.Count == 0)
            {
                MessageBox.Show("No patches selected.", "ICE Patcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                patchButton.Enabled = true;
                return;
            }

            progressBar1.Value = 0;
            progressBar1.Step = 1;

            UpdateProgressBar();
            await Task.Run(() =>
            {
                ApplyPatches();
            });

            progressBar1.Value = progressBar1.Maximum;

            patchButton.Enabled = true;
        }




        private List<bool> checkedStateList = new List<bool>();

        private void refreshButton_Click(object sender, EventArgs e)
        {
            SaveCheckedState();
            var previousItems = patchesListBox.Items.Cast<object>().ToList();
            patchesListBox.Items.Clear();
            patchesListBox.Items.AddRange(icePatcherCommon.GetPatches());
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
    }
}
