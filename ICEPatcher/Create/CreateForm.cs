using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ICEPatcher.Create
{
    public partial class CreateForm : Form
    {
        public CreateForm()
        {
            InitializeComponent();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (processFolderCheckBox.Checked)
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        inputTextBox.Text = dialog.SelectedPath;
                    }
                }
            }
            else
            {
                using (OpenFileDialog fileDialog = new OpenFileDialog
                {
                    Title = "Select text file",
                    Filter = "Text files (*.txt)|*.txt",
                    FilterIndex = 1,
                    RestoreDirectory = true
                })
                {
                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        inputTextBox.Text = fileDialog.FileName;
                    }
                }
            }
        }

        private void processFolderCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (processFolderCheckBox.Checked)
            {
                inputLabel.Text = "Input Folder";

                if (!inputTextBox.Text.EndsWith("\\"))
                {
                    inputTextBox.Text = Path.GetDirectoryName(inputTextBox.Text) + "\\";
                }
            }
            else
            {
                inputLabel.Text = "Input File";
            }
        }


        private void convertTextToYamlCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (convertTextToYamlCheckBox.Checked)
            {
                keepTextFilesCheckBox.Enabled = true;
            }
            else
            {
                keepTextFilesCheckBox.Enabled = false;
            }
        }


        private void createButton_Click(object sender, EventArgs e)
        {

        }

        private void nameTextBox_TextChanged(object sender, EventArgs e)
        {
            
        }
    }
}
