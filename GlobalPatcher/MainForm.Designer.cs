namespace GlobalPatcher
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
            browseButton = new Button();
            patchButton = new Button();
            progressBar = new ProgressBar();
            statusLabel = new Label();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // browseButton
            // 
            browseButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            browseButton.Location = new Point(477, 188);
            browseButton.Name = "browseButton";
            browseButton.Size = new Size(75, 23);
            browseButton.TabIndex = 0;
            browseButton.Text = "button1";
            browseButton.UseVisualStyleBackColor = true;
            browseButton.Click += browseButton_Click;
            // 
            // patchButton
            // 
            patchButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            patchButton.Location = new Point(477, 246);
            patchButton.Name = "patchButton";
            patchButton.Size = new Size(75, 23);
            patchButton.TabIndex = 1;
            patchButton.Text = "button2";
            patchButton.UseVisualStyleBackColor = true;
            patchButton.Click += patchButton_Click;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(12, 217);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(540, 23);
            progressBar.TabIndex = 2;
            // 
            // statusLabel
            // 
            statusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            statusLabel.AutoSize = true;
            statusLabel.Location = new Point(12, 196);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(38, 15);
            statusLabel.TabIndex = 3;
            statusLabel.Text = "label1";
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pictureBox1.BackgroundImage = Localization.Strings.globalpatcher_logo;
            pictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            pictureBox1.InitialImage = null;
            pictureBox1.Location = new Point(0, -2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(563, 177);
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(564, 281);
            Controls.Add(pictureBox1);
            Controls.Add(statusLabel);
            Controls.Add(progressBar);
            Controls.Add(patchButton);
            Controls.Add(browseButton);
            MinimumSize = new Size(580, 320);
            Name = "MainForm";
            Text = "GlobalPatcher";
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button browseButton;
        private Button patchButton;
        private ProgressBar progressBar;
        private Label statusLabel;
        private PictureBox pictureBox1;
    }
}
