using System;
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
        }

        private void inputFileBrowseButton_Click(object sender, EventArgs e)
        {
            string inputFile = icePatcherCommon.OpenFile();

            if (icePatcherCommon.GetInputFile(inputFile) != null)
            {
                inputFileTextBox.Text = inputFile;
                icePatcherCommon.ListIceContents(inputFile);
            }
        }

        private void patchButton_Click(object sender, EventArgs e)
        {
            string inputFile = inputFileTextBox.Text;
            string patchDir = patchTextBox.Text;

            if (icePatcherCommon.GetInputFile(inputFile) != null)
            {
                icePatcherCommon.Patch(inputFile, patchDir);
            }
        }

        private void patchBrowseButton_Click(object sender, EventArgs e)
        {
            string inputFile = inputFileTextBox.Text;
            string patchDir = icePatcherCommon.OpenFolder();

            if (patchDir != null)
            {
                patchTextBox.Text = patchDir;
                if (icePatcherCommon.GetInputFile(inputFile) != null)
                {
                    icePatcherCommon.ListIceContents(inputFile, patchDir);
                }
            }
        }
    }
}
