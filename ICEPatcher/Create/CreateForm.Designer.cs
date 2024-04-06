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
            SuspendLayout();
            // 
            // inputTextBox
            // 
            inputTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            inputTextBox.Location = new Point(12, 27);
            inputTextBox.Name = "inputTextBox";
            inputTextBox.Size = new Size(391, 23);
            inputTextBox.TabIndex = 0;
            // 
            // browseButton
            // 
            browseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browseButton.Location = new Point(409, 27);
            browseButton.Name = "browseButton";
            browseButton.Size = new Size(75, 23);
            browseButton.TabIndex = 1;
            browseButton.Text = "Browse";
            browseButton.UseVisualStyleBackColor = true;
            browseButton.Click += browseButton_Click;
            // 
            // inputLabel
            // 
            inputLabel.AutoSize = true;
            inputLabel.Location = new Point(12, 9);
            inputLabel.Name = "inputLabel";
            inputLabel.Size = new Size(56, 15);
            inputLabel.TabIndex = 2;
            inputLabel.Text = "Input File";
            // 
            // createButton
            // 
            createButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            createButton.Location = new Point(409, 311);
            createButton.Name = "createButton";
            createButton.Size = new Size(75, 23);
            createButton.TabIndex = 8;
            createButton.Text = "Create";
            createButton.UseVisualStyleBackColor = true;
            createButton.Click += createButton_Click;
            // 
            // progressBar1
            // 
            progressBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(12, 282);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(472, 23);
            progressBar1.Step = 1;
            progressBar1.TabIndex = 11;
            // 
            // processFolderCheckBox
            // 
            processFolderCheckBox.AutoSize = true;
            processFolderCheckBox.Location = new Point(12, 56);
            processFolderCheckBox.Name = "processFolderCheckBox";
            processFolderCheckBox.Size = new Size(102, 19);
            processFolderCheckBox.TabIndex = 12;
            processFolderCheckBox.Text = "Process Folder";
            processFolderCheckBox.UseVisualStyleBackColor = true;
            processFolderCheckBox.CheckedChanged += processFolderCheckBox_CheckedChanged;
            // 
            // nameTextBox
            // 
            nameTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            nameTextBox.Location = new Point(12, 96);
            nameTextBox.Name = "nameTextBox";
            nameTextBox.Size = new Size(472, 23);
            nameTextBox.TabIndex = 13;
            nameTextBox.TextChanged += nameTextBox_TextChanged;
            // 
            // nameLabel
            // 
            nameLabel.AutoSize = true;
            nameLabel.Location = new Point(12, 78);
            nameLabel.Name = "nameLabel";
            nameLabel.Size = new Size(72, 15);
            nameLabel.TabIndex = 14;
            nameLabel.Text = "Patch Name";
            // 
            // convertTextToYamlCheckBox
            // 
            convertTextToYamlCheckBox.AutoSize = true;
            convertTextToYamlCheckBox.Location = new Point(12, 125);
            convertTextToYamlCheckBox.Name = "convertTextToYamlCheckBox";
            convertTextToYamlCheckBox.Size = new Size(140, 19);
            convertTextToYamlCheckBox.TabIndex = 15;
            convertTextToYamlCheckBox.Text = "Convert .text to .yaml";
            convertTextToYamlCheckBox.UseVisualStyleBackColor = true;
            convertTextToYamlCheckBox.CheckedChanged += convertTextToYamlCheckBox_CheckedChanged;
            // 
            // keepTextFilesCheckBox
            // 
            keepTextFilesCheckBox.AutoSize = true;
            keepTextFilesCheckBox.Enabled = false;
            keepTextFilesCheckBox.Location = new Point(31, 150);
            keepTextFilesCheckBox.Name = "keepTextFilesCheckBox";
            keepTextFilesCheckBox.Size = new Size(102, 19);
            keepTextFilesCheckBox.TabIndex = 16;
            keepTextFilesCheckBox.Text = "Keep .text files";
            keepTextFilesCheckBox.UseVisualStyleBackColor = true;
            // 
            // CreateForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(496, 346);
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
            MinimumSize = new Size(512, 0);
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
    }
}