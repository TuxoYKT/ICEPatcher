namespace ICEPatcher
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            inputLabel = new Label();
            inputFileTextBox = new TextBox();
            inputFileBrowseButton = new Button();
            patchLabel = new Label();
            patchButton = new Button();
            patchesListBox = new CheckedListBox();
            refreshButton = new Button();
            progressBar1 = new ProgressBar();
            openFolderButton = new Button();
            statusLabel = new Label();
            exportCheckBox = new CheckBox();
            backupCheckBox = new CheckBox();
            SuspendLayout();
            // 
            // inputLabel
            // 
            inputLabel.AutoSize = true;
            inputLabel.Location = new Point(12, 9);
            inputLabel.Name = "inputLabel";
            inputLabel.Size = new Size(103, 15);
            inputLabel.TabIndex = 0;
            inputLabel.Text = "pso2_bin Location";
            // 
            // inputFileTextBox
            // 
            inputFileTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            inputFileTextBox.Location = new Point(12, 27);
            inputFileTextBox.Name = "inputFileTextBox";
            inputFileTextBox.Size = new Size(391, 23);
            inputFileTextBox.TabIndex = 1;
            // 
            // inputFileBrowseButton
            // 
            inputFileBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            inputFileBrowseButton.Location = new Point(409, 27);
            inputFileBrowseButton.Name = "inputFileBrowseButton";
            inputFileBrowseButton.Size = new Size(75, 23);
            inputFileBrowseButton.TabIndex = 2;
            inputFileBrowseButton.Text = "Browse";
            inputFileBrowseButton.UseVisualStyleBackColor = true;
            inputFileBrowseButton.Click += inputFileBrowseButton_Click;
            // 
            // patchLabel
            // 
            patchLabel.AutoSize = true;
            patchLabel.Location = new Point(12, 53);
            patchLabel.Name = "patchLabel";
            patchLabel.Size = new Size(99, 15);
            patchLabel.TabIndex = 3;
            patchLabel.Text = "Available Patches";
            // 
            // patchButton
            // 
            patchButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            patchButton.Location = new Point(409, 254);
            patchButton.Name = "patchButton";
            patchButton.Size = new Size(75, 23);
            patchButton.TabIndex = 7;
            patchButton.Text = "Patch";
            patchButton.UseVisualStyleBackColor = true;
            patchButton.Click += patchButton_Click;
            // 
            // patchesListBox
            // 
            patchesListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            patchesListBox.FormattingEnabled = true;
            patchesListBox.Location = new Point(13, 71);
            patchesListBox.Name = "patchesListBox";
            patchesListBox.Size = new Size(390, 112);
            patchesListBox.TabIndex = 8;
            // 
            // refreshButton
            // 
            refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refreshButton.Location = new Point(409, 71);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(75, 23);
            refreshButton.TabIndex = 9;
            refreshButton.Text = "Refresh";
            refreshButton.UseVisualStyleBackColor = true;
            refreshButton.Click += refreshButton_Click;
            // 
            // progressBar1
            // 
            progressBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar1.Location = new Point(12, 225);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(472, 23);
            progressBar1.Step = 1;
            progressBar1.TabIndex = 10;
            // 
            // openFolderButton
            // 
            openFolderButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            openFolderButton.Location = new Point(409, 143);
            openFolderButton.Name = "openFolderButton";
            openFolderButton.Size = new Size(75, 40);
            openFolderButton.TabIndex = 11;
            openFolderButton.Text = "Open Folder";
            openFolderButton.UseVisualStyleBackColor = true;
            openFolderButton.Click += openFolderButton_Click;
            // 
            // statusLabel
            // 
            statusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            statusLabel.AutoSize = true;
            statusLabel.Location = new Point(13, 207);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(127, 15);
            statusLabel.TabIndex = 12;
            statusLabel.Text = "pso2_bin folder not set";
            // 
            // exportCheckBox
            // 
            exportCheckBox.AutoSize = true;
            exportCheckBox.Location = new Point(80, 258);
            exportCheckBox.Name = "exportCheckBox";
            exportCheckBox.Size = new Size(60, 19);
            exportCheckBox.TabIndex = 13;
            exportCheckBox.Text = "Export";
            exportCheckBox.UseVisualStyleBackColor = true;
            exportCheckBox.CheckedChanged += exportCheckBox_CheckedChanged;
            // 
            // backupCheckBox
            // 
            backupCheckBox.AutoSize = true;
            backupCheckBox.Location = new Point(12, 258);
            backupCheckBox.Name = "backupCheckBox";
            backupCheckBox.Size = new Size(65, 19);
            backupCheckBox.TabIndex = 14;
            backupCheckBox.Text = "Backup";
            backupCheckBox.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(496, 289);
            Controls.Add(backupCheckBox);
            Controls.Add(exportCheckBox);
            Controls.Add(statusLabel);
            Controls.Add(openFolderButton);
            Controls.Add(progressBar1);
            Controls.Add(refreshButton);
            Controls.Add(patchesListBox);
            Controls.Add(patchButton);
            Controls.Add(patchLabel);
            Controls.Add(inputFileBrowseButton);
            Controls.Add(inputFileTextBox);
            Controls.Add(inputLabel);
            MinimumSize = new Size(512, 328);
            Name = "MainForm";
            Text = "ICEPatcher";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label inputLabel;
        private TextBox inputFileTextBox;
        private Button inputFileBrowseButton;
        private Label patchLabel;
        private Button patchButton;
        private CheckedListBox patchesListBox;
        private Button refreshButton;
        private ProgressBar progressBar1;
        private Button openFolderButton;
        private Label statusLabel;
        private CheckBox exportCheckBox;
        private CheckBox backupCheckBox;
    }
}
