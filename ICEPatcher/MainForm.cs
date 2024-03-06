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

        private void patchButton_Click(object sender, EventArgs e)
        {
            string inputFile = inputFileTextBox.Text;

            if (inputFile == "") 
            { 
                MessageBox.Show("pso2_bin folder not selected.", "ICE Patcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (patchesListBox.CheckedItems.Count == 0)
            {
                MessageBox.Show("No patches selected.", "ICE Patcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            for (int i = 0; i < patchesListBox.Items.Count; i++)
            {
                if (patchesListBox.GetItemCheckState(i) == CheckState.Checked)
                {
                    string checkedItem = patchesListBox.Items[i].ToString();
                    // Your code logic for the checked item goes here

                    icePatcherCommon.ApplyPatch(inputFile, checkedItem);
                }
            }
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
