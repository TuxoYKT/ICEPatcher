namespace ICEPatcher.Create
{
    partial class CreateForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            inputTextBox = new TextBox();
            browseButton = new Button();
            inputLabel = new Label();
            createButton = new Button();
            progressBar1 = new ProgressBar();
            processFolderCheckBox = new CheckBox();
            nameTextBox = new TextBox();
            nameLabel = new Label();
            convertTextToYamlCheckBox = new CheckBox();
            keepTextFilesCheckBox = new CheckBox();
            eachFileAsFolderCheckbox = new CheckBox();
            SuspendLayout();
            // 
            // inputTextBox
            // 
            inputTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            inputTextBox.Location = new Point(14, 36);
            inputTextBox.Margin = new Padding(3, 4, 3, 4);
            inputTextBox.Name = "inputTextBox";
            inputTextBox.Size = new Size(446, 27);
            inputTextBox.TabIndex = 0;
            // 
            // browseButton
            // 
            browseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browseButton.Location = new Point(467, 36);
            browseButton.Margin = new Padding(3, 4, 3, 4);
            browseButton.Name = "browseButton";
            browseButton.Size = new Size(86, 31);
            browseButton.TabIndex = 1;
            browseButton.Text = "Browse";
            browseButton.UseVisualStyleBackColor = true;
            browseButton.Click += browseButton_Click;
            // 
            // inputLabel
            // 
            inputLabel.AutoSize = true;
            inputLabel.Location = new Point(14, 12);
            inputLabel.Name = "inputLabel";
            inputLabel.Size = new Size(70, 20);
            inputLabel.TabIndex = 2;
            inputLabel.Text = "Input File";
            // 
            // createButton
            // 
            createButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            createButton.Location = new Point(467, 415);
            createButton.Margin = new Padding(3, 4, 3, 4);
            createButton.Name = "createButton";
            createButton.Size = new Size(86, 31);
            createButton.TabIndex = 8;
            createButton.Text = "Create";
            createButton.UseVisualStyleBackColor = true;
            createButton.Click += createButton_Click;
            // 
            // progressBar1
            // 
            progressBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(14, 376);
            progressBar1.Margin = new Padding(3, 4, 3, 4);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(539, 31);
            progressBar1.Step = 1;
            progressBar1.TabIndex = 11;
            // 
            // processFolderCheckBox
            // 
            processFolderCheckBox.AutoSize = true;
            processFolderCheckBox.Location = new Point(14, 75);
            processFolderCheckBox.Margin = new Padding(3, 4, 3, 4);
            processFolderCheckBox.Name = "processFolderCheckBox";
            processFolderCheckBox.Size = new Size(126, 24);
            processFolderCheckBox.TabIndex = 12;
            processFolderCheckBox.Text = "Process Folder";
            processFolderCheckBox.UseVisualStyleBackColor = true;
            processFolderCheckBox.CheckedChanged += processFolderCheckBox_CheckedChanged;
            // 
            // nameTextBox
            // 
            nameTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            nameTextBox.Location = new Point(14, 128);
            nameTextBox.Margin = new Padding(3, 4, 3, 4);
            nameTextBox.Name = "nameTextBox";
            nameTextBox.Size = new Size(539, 27);
            nameTextBox.TabIndex = 13;
            nameTextBox.TextChanged += nameTextBox_TextChanged;
            // 
            // nameLabel
            // 
            nameLabel.AutoSize = true;
            nameLabel.Location = new Point(14, 104);
            nameLabel.Name = "nameLabel";
            nameLabel.Size = new Size(88, 20);
            nameLabel.TabIndex = 14;
            nameLabel.Text = "Patch Name";
            // 
            // convertTextToYamlCheckBox
            // 
            convertTextToYamlCheckBox.AutoSize = true;
            convertTextToYamlCheckBox.Location = new Point(14, 167);
            convertTextToYamlCheckBox.Margin = new Padding(3, 4, 3, 4);
            convertTextToYamlCheckBox.Name = "convertTextToYamlCheckBox";
            convertTextToYamlCheckBox.Size = new Size(171, 24);
            convertTextToYamlCheckBox.TabIndex = 15;
            convertTextToYamlCheckBox.Text = "Convert .text to .yaml";
            convertTextToYamlCheckBox.UseVisualStyleBackColor = true;
            convertTextToYamlCheckBox.CheckedChanged += convertTextToYamlCheckBox_CheckedChanged;
            // 
            // keepTextFilesCheckBox
            // 
            keepTextFilesCheckBox.AutoSize = true;
            keepTextFilesCheckBox.Enabled = false;
            keepTextFilesCheckBox.Location = new Point(35, 200);
            keepTextFilesCheckBox.Margin = new Padding(3, 4, 3, 4);
            keepTextFilesCheckBox.Name = "keepTextFilesCheckBox";
            keepTextFilesCheckBox.Size = new Size(128, 24);
            keepTextFilesCheckBox.TabIndex = 16;
            keepTextFilesCheckBox.Text = "Keep .text files";
            keepTextFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // eachFileAsFolderCheckbox
            // 
            eachFileAsFolderCheckbox.AutoSize = true;
            eachFileAsFolderCheckbox.Enabled = false;
            eachFileAsFolderCheckbox.Location = new Point(146, 75);
            eachFileAsFolderCheckbox.Name = "eachFileAsFolderCheckbox";
            eachFileAsFolderCheckbox.Size = new Size(186, 24);
            eachFileAsFolderCheckbox.TabIndex = 17;
            eachFileAsFolderCheckbox.Text = "Each Input File As Patch";
            eachFileAsFolderCheckbox.UseVisualStyleBackColor = true;
            // 
            // CreateForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(567, 461);
            Controls.Add(eachFileAsFolderCheckbox);
            Controls.Add(keepTextFilesCheckBox);
            Controls.Add(convertTextToYamlCheckBox);
            Controls.Add(nameLabel);
            Controls.Add(nameTextBox);
            Controls.Add(processFolderCheckBox);
            Controls.Add(progressBar1);
            Controls.Add(createButton);
            Controls.Add(inputLabel);
            Controls.Add(browseButton);
            Controls.Add(inputTextBox);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(583, 47);
            Name = "CreateForm";
            Text = "Create a Patch";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox inputTextBox;
        private Button browseButton;
        private Label inputLabel;
        private Button createButton;
        private ProgressBar progressBar1;
        private CheckBox processFolderCheckBox;
        private TextBox nameTextBox;
        private Label nameLabel;
        private CheckBox convertTextToYamlCheckBox;
        private CheckBox keepTextFilesCheckBox;
        private CheckBox eachFileAsFolderCheckbox;
    }
}