﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                        inputTextBox.Text = dialog.SelectedPath + "\\";
                        nameTextBox.Text = Path.GetDirectoryName(inputTextBox.Text);
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
                        nameTextBox.Text = Path.GetFileNameWithoutExtension(inputTextBox.Text);
                    }
                }
            }
        }

        private void processFolderCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (processFolderCheckBox.Checked)
            {
                inputLabel.Text = "Input Folder";

                if (!inputTextBox.Text.EndsWith("\\") && inputTextBox.Text != "")
                {
                    inputTextBox.Text = Path.GetDirectoryName(inputTextBox.Text) + "\\";
                }

                eachFileAsFolderCheckbox.Enabled = true;
            }
            else
            {
                inputLabel.Text = "Input File";

                eachFileAsFolderCheckbox.Enabled = false;
                eachFileAsFolderCheckbox.Checked = false;
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

        private void nameTextBox_TextChanged(object sender, EventArgs e)
        {
            nameTextBox.Text = Regex.Replace(nameTextBox.Text, "[\\/:*?\"<>|]", string.Empty);
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            string name = nameTextBox.Text;
            string input = inputTextBox.Text;
            bool processFolder = processFolderCheckBox.Checked;
            bool eachInputFileAsPatch = eachFileAsFolderCheckbox.Checked;
            Creation.ConvertText = convertTextToYamlCheckBox.Checked;
            Creation.KeepTextFiles = keepTextFilesCheckBox.Checked;
            Creation.progressBar = progressBar1;

            string executablePath = AppDomain.CurrentDomain.BaseDirectory;
            string patchesPath = Path.Combine(executablePath, "Patches");
            string patchPath = Path.Combine(patchesPath, name);

            if (!Directory.Exists(patchPath) || !eachInputFileAsPatch)
            {
                Directory.CreateDirectory(patchPath);
            }
            else
            {
                MessageBox.Show("Folder with the same name already exists!");
                return;
            }

            if (!processFolder) 
            {
                if (File.Exists(input))
                {
                    Creation.ProcessFileInput(input, name);
                }
            }
            else
            {
                string[] txtFiles = Directory.GetFiles(input, "*.txt");

                foreach (string file in txtFiles)
                {
                    if (eachInputFileAsPatch) name = Path.GetFileNameWithoutExtension(file);

                    Creation.ProcessFileInput(file, name);
                }
            }

            progressBar1.Value = progressBar1.Maximum;
        }
    }
}
