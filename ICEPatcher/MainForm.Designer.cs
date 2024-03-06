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
            logTextBox = new TextBox();
            patchButton = new Button();
            patchesListBox = new CheckedListBox();
            refreshButton = new Button();
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
            inputFileTextBox.Size = new Size(604, 23);
            inputFileTextBox.TabIndex = 1;
            // 
            // inputFileBrowseButton
            // 
            inputFileBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            inputFileBrowseButton.Location = new Point(622, 27);
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
            // logTextBox
            // 
            logTextBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            logTextBox.Font = new Font("Courier New", 9F, FontStyle.Regular, GraphicsUnit.Point);
            logTextBox.Location = new Point(13, 464);
            logTextBox.Multiline = true;
            logTextBox.Name = "logTextBox";
            logTextBox.ScrollBars = ScrollBars.Vertical;
            logTextBox.Size = new Size(684, 207);
            logTextBox.TabIndex = 6;
            // 
            // patchButton
            // 
            patchButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            patchButton.Location = new Point(622, 677);
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
            patchesListBox.Size = new Size(603, 382);
            patchesListBox.TabIndex = 8;
            // 
            // refreshButton
            // 
            refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            refreshButton.Location = new Point(622, 71);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(75, 23);
            refreshButton.TabIndex = 9;
            refreshButton.Text = "Refresh";
            refreshButton.UseVisualStyleBackColor = true;
            refreshButton.Click += refreshButton_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(709, 710);
            Controls.Add(refreshButton);
            Controls.Add(patchesListBox);
            Controls.Add(patchButton);
            Controls.Add(logTextBox);
            Controls.Add(patchLabel);
            Controls.Add(inputFileBrowseButton);
            Controls.Add(inputFileTextBox);
            Controls.Add(inputLabel);
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
        private TextBox logTextBox;
        private Button patchButton;
        private CheckedListBox patchesListBox;
        private Button refreshButton;
    }
}
