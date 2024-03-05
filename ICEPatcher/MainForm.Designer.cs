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
            patchBrowseButton = new Button();
            patchTextBox = new TextBox();
            patchLabel = new Label();
            logTextBox = new TextBox();
            patchButton = new Button();
            SuspendLayout();
            // 
            // inputLabel
            // 
            inputLabel.AutoSize = true;
            inputLabel.Location = new Point(12, 9);
            inputLabel.Name = "inputLabel";
            inputLabel.Size = new Size(101, 15);
            inputLabel.TabIndex = 0;
            inputLabel.Text = "File to be patched";
            // 
            // inputFileTextBox
            // 
            inputFileTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            inputFileTextBox.Location = new Point(12, 27);
            inputFileTextBox.Name = "inputFileTextBox";
            inputFileTextBox.Size = new Size(643, 23);
            inputFileTextBox.TabIndex = 1;
            // 
            // inputFileBrowseButton
            // 
            inputFileBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            inputFileBrowseButton.Location = new Point(661, 27);
            inputFileBrowseButton.Name = "inputFileBrowseButton";
            inputFileBrowseButton.Size = new Size(75, 23);
            inputFileBrowseButton.TabIndex = 2;
            inputFileBrowseButton.Text = "Browse";
            inputFileBrowseButton.UseVisualStyleBackColor = true;
            inputFileBrowseButton.Click += inputFileBrowseButton_Click;
            // 
            // patchBrowseButton
            // 
            patchBrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            patchBrowseButton.Location = new Point(661, 71);
            patchBrowseButton.Name = "patchBrowseButton";
            patchBrowseButton.Size = new Size(75, 23);
            patchBrowseButton.TabIndex = 5;
            patchBrowseButton.Text = "Browse";
            patchBrowseButton.UseVisualStyleBackColor = true;
            patchBrowseButton.Click += patchBrowseButton_Click;
            // 
            // patchTextBox
            // 
            patchTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            patchTextBox.Location = new Point(12, 71);
            patchTextBox.Name = "patchTextBox";
            patchTextBox.Size = new Size(643, 23);
            patchTextBox.TabIndex = 4;
            // 
            // patchLabel
            // 
            patchLabel.AutoSize = true;
            patchLabel.Location = new Point(12, 53);
            patchLabel.Name = "patchLabel";
            patchLabel.Size = new Size(37, 15);
            patchLabel.TabIndex = 3;
            patchLabel.Text = "Patch";
            // 
            // logTextBox
            // 
            logTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            logTextBox.Font = new Font("Courier New", 9F, FontStyle.Regular, GraphicsUnit.Point);
            logTextBox.Location = new Point(13, 100);
            logTextBox.Multiline = true;
            logTextBox.Name = "logTextBox";
            logTextBox.ScrollBars = ScrollBars.Vertical;
            logTextBox.Size = new Size(723, 465);
            logTextBox.TabIndex = 6;
            // 
            // patchButton
            // 
            patchButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            patchButton.Location = new Point(661, 571);
            patchButton.Name = "patchButton";
            patchButton.Size = new Size(75, 23);
            patchButton.TabIndex = 7;
            patchButton.Text = "Patch";
            patchButton.UseVisualStyleBackColor = true;
            patchButton.Click += patchButton_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(748, 604);
            Controls.Add(patchButton);
            Controls.Add(logTextBox);
            Controls.Add(patchBrowseButton);
            Controls.Add(patchTextBox);
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
        private Button patchBrowseButton;
        private TextBox patchTextBox;
        private Label patchLabel;
        private TextBox logTextBox;
        private Button patchButton;
    }
}
