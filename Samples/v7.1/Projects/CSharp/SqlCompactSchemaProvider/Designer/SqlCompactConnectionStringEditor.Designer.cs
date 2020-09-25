namespace SchemaExplorer
{
    partial class SqlCompactConnectionStringEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region IDisposable Members

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                if (browseButton != null)
                {
                    browseButton.Click -= browseButton_Click;
                }
                if (cancelButton != null)
                {
                    cancelButton.Click -= cancelButton_Click;
                }
                if (okButton != null)
                {
                    okButton.Click -= okButton_Click;
                }
                if (testButton != null)
                {
                    testButton.Click -= testButton_Click;
                }
                if (components != null)
                {
                    components.Dispose();
                }
            }

            _disposed = true;
            base.Dispose(disposing);
        }

        #endregion

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.databaseGroupBox = new System.Windows.Forms.GroupBox();
            this.dataSourceTextBox = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.testButton = new System.Windows.Forms.Button();
            this.poolingGroupBox = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.minLabel = new System.Windows.Forms.Label();
            this.minNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.encryptionGroupBox = new System.Windows.Forms.GroupBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.databaseGroupBox.SuspendLayout();
            this.poolingGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.minNumericUpDown)).BeginInit();
            this.encryptionGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // databaseGroupBox
            // 
            this.databaseGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.databaseGroupBox.Controls.Add(this.dataSourceTextBox);
            this.databaseGroupBox.Controls.Add(this.browseButton);
            this.databaseGroupBox.Location = new System.Drawing.Point(12, 13);
            this.databaseGroupBox.Name = "databaseGroupBox";
            this.databaseGroupBox.Size = new System.Drawing.Size(300, 59);
            this.databaseGroupBox.TabIndex = 0;
            this.databaseGroupBox.TabStop = false;
            this.databaseGroupBox.Text = "&Database";
            // 
            // dataSourceTextBox
            // 
            this.dataSourceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataSourceTextBox.Location = new System.Drawing.Point(7, 23);
            this.dataSourceTextBox.Name = "dataSourceTextBox";
            this.dataSourceTextBox.Size = new System.Drawing.Size(256, 20);
            this.dataSourceTextBox.TabIndex = 0;
            // 
            // browseButton
            // 
            this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseButton.Location = new System.Drawing.Point(269, 22);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(24, 23);
            this.browseButton.TabIndex = 1;
            this.browseButton.Text = "...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(237, 239);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(153, 239);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 1;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // testButton
            // 
            this.testButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.testButton.Location = new System.Drawing.Point(19, 239);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(75, 23);
            this.testButton.TabIndex = 0;
            this.testButton.Text = "&Test";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.testButton_Click);
            // 
            // poolingGroupBox
            // 
            this.poolingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.poolingGroupBox.Controls.Add(this.label1);
            this.poolingGroupBox.Controls.Add(this.minLabel);
            this.poolingGroupBox.Controls.Add(this.minNumericUpDown);
            this.poolingGroupBox.Location = new System.Drawing.Point(12, 141);
            this.poolingGroupBox.Name = "poolingGroupBox";
            this.poolingGroupBox.Size = new System.Drawing.Size(300, 64);
            this.poolingGroupBox.TabIndex = 1;
            this.poolingGroupBox.TabStop = false;
            this.poolingGroupBox.Text = "&Advanced";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(265, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "MB";
            // 
            // minLabel
            // 
            this.minLabel.AutoSize = true;
            this.minLabel.Location = new System.Drawing.Point(7, 21);
            this.minLabel.Name = "minLabel";
            this.minLabel.Size = new System.Drawing.Size(119, 13);
            this.minLabel.TabIndex = 1;
            this.minLabel.Text = "Ma&ximum database size";
            // 
            // minNumericUpDown
            // 
            this.minNumericUpDown.Location = new System.Drawing.Point(151, 19);
            this.minNumericUpDown.Maximum = new decimal(new int[] {
            4096,
            0,
            0,
            0});
            this.minNumericUpDown.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.minNumericUpDown.Name = "minNumericUpDown";
            this.minNumericUpDown.Size = new System.Drawing.Size(78, 20);
            this.minNumericUpDown.TabIndex = 0;
            this.minNumericUpDown.Value = new decimal(new int[] {
            128,
            0,
            0,
            0});
            // 
            // encryptionGroupBox
            // 
            this.encryptionGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.encryptionGroupBox.Controls.Add(this.passwordLabel);
            this.encryptionGroupBox.Controls.Add(this.passwordTextBox);
            this.encryptionGroupBox.Location = new System.Drawing.Point(12, 78);
            this.encryptionGroupBox.Name = "encryptionGroupBox";
            this.encryptionGroupBox.Size = new System.Drawing.Size(299, 57);
            this.encryptionGroupBox.TabIndex = 2;
            this.encryptionGroupBox.TabStop = false;
            this.encryptionGroupBox.Text = "&Encryption";
            // 
            // passwordLabel
            // 
            this.passwordLabel.AutoSize = true;
            this.passwordLabel.Location = new System.Drawing.Point(7, 25);
            this.passwordLabel.Name = "passwordLabel";
            this.passwordLabel.Size = new System.Drawing.Size(53, 13);
            this.passwordLabel.TabIndex = 0;
            this.passwordLabel.Text = "Pass&word";
            // 
            // passwordTextBox
            // 
            this.passwordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.passwordTextBox.Location = new System.Drawing.Point(67, 21);
            this.passwordTextBox.Name = "passwordTextBox";
            this.passwordTextBox.Size = new System.Drawing.Size(226, 20);
            this.passwordTextBox.TabIndex = 0;
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "sdf";
            this.openFileDialog.Filter = "SQL Server Compact Edition Database File (*.sdf)|*.sdf|All Files (*.*)|*.*";
            this.openFileDialog.Title = "Open SQL Compact Database";
            // 
            // SqlCompactConnectionStringEditor
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(332, 274);
            this.Controls.Add(this.databaseGroupBox);
            this.Controls.Add(this.poolingGroupBox);
            this.Controls.Add(this.encryptionGroupBox);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(340, 300);
            this.Name = "SqlCompactConnectionStringEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SQL Compact Connection String Editor";
            this.Load += new System.EventHandler(this.SqlCompactConnectionStringEditor_Load);
            this.databaseGroupBox.ResumeLayout(false);
            this.databaseGroupBox.PerformLayout();
            this.poolingGroupBox.ResumeLayout(false);
            this.poolingGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.minNumericUpDown)).EndInit();
            this.encryptionGroupBox.ResumeLayout(false);
            this.encryptionGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox databaseGroupBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox dataSourceTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button testButton;
        private System.Windows.Forms.GroupBox poolingGroupBox;
        private System.Windows.Forms.NumericUpDown minNumericUpDown;
        private System.Windows.Forms.Label minLabel;
        private System.Windows.Forms.GroupBox encryptionGroupBox;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label label1;
    }
}