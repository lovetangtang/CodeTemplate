namespace SchemaExplorer.Designer
{
    partial class SQLiteConnectionStringEditor
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
            this.poolCheckBox = new System.Windows.Forms.CheckBox();
            this.DataSourceTextBox = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.pageLabel = new System.Windows.Forms.Label();
            this.pageSizeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.cacheLabel = new System.Windows.Forms.Label();
            this.cacheSizeNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.encryptionGroupBox = new System.Windows.Forms.GroupBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.testButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.cancelButton = new System.Windows.Forms.Button();
            this.encodingGroupBox = new System.Windows.Forms.GroupBox();
            this.utf8RadioButton = new System.Windows.Forms.RadioButton();
            this.utf16RadioButton = new System.Windows.Forms.RadioButton();
            this.dateFormatGroupBox = new System.Windows.Forms.GroupBox();
            this.iso8601RadioButton = new System.Windows.Forms.RadioButton();
            this.julianRadioButton = new System.Windows.Forms.RadioButton();
            this.ticksRadioButton = new System.Windows.Forms.RadioButton();
            this.syncGroupBox = new System.Windows.Forms.GroupBox();
            this.fullSyncRadioButton = new System.Windows.Forms.RadioButton();
            this.normalSyncRadioButton = new System.Windows.Forms.RadioButton();
            this.offSyncRadioButton = new System.Windows.Forms.RadioButton();
            this.databaseGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pageSizeNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cacheSizeNumericUpDown)).BeginInit();
            this.encryptionGroupBox.SuspendLayout();
            this.encodingGroupBox.SuspendLayout();
            this.dateFormatGroupBox.SuspendLayout();
            this.syncGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // databaseGroupBox
            // 
            this.databaseGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.databaseGroupBox.Controls.Add(this.poolCheckBox);
            this.databaseGroupBox.Controls.Add(this.DataSourceTextBox);
            this.databaseGroupBox.Controls.Add(this.browseButton);
            this.databaseGroupBox.Controls.Add(this.pageLabel);
            this.databaseGroupBox.Controls.Add(this.pageSizeNumericUpDown);
            this.databaseGroupBox.Controls.Add(this.cacheLabel);
            this.databaseGroupBox.Controls.Add(this.cacheSizeNumericUpDown);
            this.databaseGroupBox.Location = new System.Drawing.Point(12, 12);
            this.databaseGroupBox.Name = "databaseGroupBox";
            this.databaseGroupBox.Size = new System.Drawing.Size(300, 117);
            this.databaseGroupBox.TabIndex = 0;
            this.databaseGroupBox.TabStop = false;
            this.databaseGroupBox.Text = "&Database";
            // 
            // poolCheckBox
            // 
            this.poolCheckBox.AutoSize = true;
            this.poolCheckBox.Location = new System.Drawing.Point(12, 88);
            this.poolCheckBox.Name = "poolCheckBox";
            this.poolCheckBox.Size = new System.Drawing.Size(154, 17);
            this.poolCheckBox.TabIndex = 8;
            this.poolCheckBox.Text = "Enable Connection &Pooling";
            this.poolCheckBox.UseVisualStyleBackColor = true;
            // 
            // DataSourceTextBox
            // 
            this.DataSourceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.DataSourceTextBox.Location = new System.Drawing.Point(7, 23);
            this.DataSourceTextBox.Name = "DataSourceTextBox";
            this.DataSourceTextBox.Size = new System.Drawing.Size(256, 20);
            this.DataSourceTextBox.TabIndex = 0;
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
            // pageLabel
            // 
            this.pageLabel.AutoSize = true;
            this.pageLabel.Location = new System.Drawing.Point(12, 59);
            this.pageLabel.Name = "pageLabel";
            this.pageLabel.Size = new System.Drawing.Size(55, 13);
            this.pageLabel.TabIndex = 2;
            this.pageLabel.Text = "Pa&ge Size";
            // 
            // pageSizeNumericUpDown
            // 
            this.pageSizeNumericUpDown.Location = new System.Drawing.Point(71, 55);
            this.pageSizeNumericUpDown.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.pageSizeNumericUpDown.Name = "pageSizeNumericUpDown";
            this.pageSizeNumericUpDown.Size = new System.Drawing.Size(61, 20);
            this.pageSizeNumericUpDown.TabIndex = 3;
            this.pageSizeNumericUpDown.Value = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            // 
            // cacheLabel
            // 
            this.cacheLabel.AutoSize = true;
            this.cacheLabel.Location = new System.Drawing.Point(151, 59);
            this.cacheLabel.Name = "cacheLabel";
            this.cacheLabel.Size = new System.Drawing.Size(61, 13);
            this.cacheLabel.TabIndex = 4;
            this.cacheLabel.Text = "Cac&he Size";
            // 
            // cacheSizeNumericUpDown
            // 
            this.cacheSizeNumericUpDown.Location = new System.Drawing.Point(216, 55);
            this.cacheSizeNumericUpDown.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.cacheSizeNumericUpDown.Name = "cacheSizeNumericUpDown";
            this.cacheSizeNumericUpDown.Size = new System.Drawing.Size(61, 20);
            this.cacheSizeNumericUpDown.TabIndex = 5;
            this.cacheSizeNumericUpDown.Value = new decimal(new int[] {
            2000,
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
            this.encryptionGroupBox.Location = new System.Drawing.Point(12, 236);
            this.encryptionGroupBox.Name = "encryptionGroupBox";
            this.encryptionGroupBox.Size = new System.Drawing.Size(300, 57);
            this.encryptionGroupBox.TabIndex = 4;
            this.encryptionGroupBox.TabStop = false;
            this.encryptionGroupBox.Text = "Encryption";
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
            this.passwordTextBox.Size = new System.Drawing.Size(227, 20);
            this.passwordTextBox.TabIndex = 1;
            // 
            // testButton
            // 
            this.testButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.testButton.Location = new System.Drawing.Point(13, 301);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(75, 23);
            this.testButton.TabIndex = 5;
            this.testButton.Text = "&Test";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.testButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(155, 301);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 6;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "db3";
            this.openFileDialog.Filter = "SQLite Database (*.db3)|*.db3|SQLite Database (*.db)|*.db|All Files (*.*)|*.*";
            this.openFileDialog.Title = "Open SQLite Database";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(236, 301);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // encodingGroupBox
            // 
            this.encodingGroupBox.Controls.Add(this.utf8RadioButton);
            this.encodingGroupBox.Controls.Add(this.utf16RadioButton);
            this.encodingGroupBox.Location = new System.Drawing.Point(12, 136);
            this.encodingGroupBox.Name = "encodingGroupBox";
            this.encodingGroupBox.Size = new System.Drawing.Size(88, 94);
            this.encodingGroupBox.TabIndex = 1;
            this.encodingGroupBox.TabStop = false;
            this.encodingGroupBox.Text = "&Encoding";
            // 
            // utf8RadioButton
            // 
            this.utf8RadioButton.AutoSize = true;
            this.utf8RadioButton.Checked = true;
            this.utf8RadioButton.Location = new System.Drawing.Point(12, 22);
            this.utf8RadioButton.Name = "utf8RadioButton";
            this.utf8RadioButton.Size = new System.Drawing.Size(55, 17);
            this.utf8RadioButton.TabIndex = 0;
            this.utf8RadioButton.TabStop = true;
            this.utf8RadioButton.Text = "UTF-8";
            this.utf8RadioButton.UseVisualStyleBackColor = true;
            // 
            // utf16RadioButton
            // 
            this.utf16RadioButton.AutoSize = true;
            this.utf16RadioButton.Location = new System.Drawing.Point(12, 44);
            this.utf16RadioButton.Name = "utf16RadioButton";
            this.utf16RadioButton.Size = new System.Drawing.Size(61, 17);
            this.utf16RadioButton.TabIndex = 1;
            this.utf16RadioButton.Text = "UTF-16";
            this.utf16RadioButton.UseVisualStyleBackColor = true;
            // 
            // dateFormatGroupBox
            // 
            this.dateFormatGroupBox.Controls.Add(this.iso8601RadioButton);
            this.dateFormatGroupBox.Controls.Add(this.julianRadioButton);
            this.dateFormatGroupBox.Controls.Add(this.ticksRadioButton);
            this.dateFormatGroupBox.Location = new System.Drawing.Point(106, 136);
            this.dateFormatGroupBox.Name = "dateFormatGroupBox";
            this.dateFormatGroupBox.Size = new System.Drawing.Size(96, 94);
            this.dateFormatGroupBox.TabIndex = 2;
            this.dateFormatGroupBox.TabStop = false;
            this.dateFormatGroupBox.Text = "Date &Format";
            // 
            // iso8601RadioButton
            // 
            this.iso8601RadioButton.AutoSize = true;
            this.iso8601RadioButton.Checked = true;
            this.iso8601RadioButton.Location = new System.Drawing.Point(10, 22);
            this.iso8601RadioButton.Name = "iso8601RadioButton";
            this.iso8601RadioButton.Size = new System.Drawing.Size(70, 17);
            this.iso8601RadioButton.TabIndex = 0;
            this.iso8601RadioButton.TabStop = true;
            this.iso8601RadioButton.Text = "ISO-8601";
            this.iso8601RadioButton.UseVisualStyleBackColor = true;
            // 
            // julianRadioButton
            // 
            this.julianRadioButton.AutoSize = true;
            this.julianRadioButton.Location = new System.Drawing.Point(10, 44);
            this.julianRadioButton.Name = "julianRadioButton";
            this.julianRadioButton.Size = new System.Drawing.Size(74, 17);
            this.julianRadioButton.TabIndex = 1;
            this.julianRadioButton.Text = "Julian Day";
            this.julianRadioButton.UseVisualStyleBackColor = true;
            // 
            // ticksRadioButton
            // 
            this.ticksRadioButton.AutoSize = true;
            this.ticksRadioButton.Location = new System.Drawing.Point(10, 67);
            this.ticksRadioButton.Name = "ticksRadioButton";
            this.ticksRadioButton.Size = new System.Drawing.Size(51, 17);
            this.ticksRadioButton.TabIndex = 2;
            this.ticksRadioButton.Text = "Ticks";
            this.ticksRadioButton.UseVisualStyleBackColor = true;
            // 
            // syncGroupBox
            // 
            this.syncGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.syncGroupBox.Controls.Add(this.fullSyncRadioButton);
            this.syncGroupBox.Controls.Add(this.normalSyncRadioButton);
            this.syncGroupBox.Controls.Add(this.offSyncRadioButton);
            this.syncGroupBox.Location = new System.Drawing.Point(208, 136);
            this.syncGroupBox.Name = "syncGroupBox";
            this.syncGroupBox.Size = new System.Drawing.Size(104, 94);
            this.syncGroupBox.TabIndex = 3;
            this.syncGroupBox.TabStop = false;
            this.syncGroupBox.Text = "&Synchronization";
            // 
            // fullSyncRadioButton
            // 
            this.fullSyncRadioButton.AutoSize = true;
            this.fullSyncRadioButton.Location = new System.Drawing.Point(11, 22);
            this.fullSyncRadioButton.Name = "fullSyncRadioButton";
            this.fullSyncRadioButton.Size = new System.Drawing.Size(41, 17);
            this.fullSyncRadioButton.TabIndex = 0;
            this.fullSyncRadioButton.TabStop = true;
            this.fullSyncRadioButton.Text = "Full";
            this.fullSyncRadioButton.UseVisualStyleBackColor = true;
            // 
            // normalSyncRadioButton
            // 
            this.normalSyncRadioButton.AutoSize = true;
            this.normalSyncRadioButton.Checked = true;
            this.normalSyncRadioButton.Location = new System.Drawing.Point(11, 44);
            this.normalSyncRadioButton.Name = "normalSyncRadioButton";
            this.normalSyncRadioButton.Size = new System.Drawing.Size(58, 17);
            this.normalSyncRadioButton.TabIndex = 1;
            this.normalSyncRadioButton.TabStop = true;
            this.normalSyncRadioButton.Text = "Normal";
            this.normalSyncRadioButton.UseVisualStyleBackColor = true;
            // 
            // offSyncRadioButton
            // 
            this.offSyncRadioButton.AutoSize = true;
            this.offSyncRadioButton.Location = new System.Drawing.Point(11, 67);
            this.offSyncRadioButton.Name = "offSyncRadioButton";
            this.offSyncRadioButton.Size = new System.Drawing.Size(39, 17);
            this.offSyncRadioButton.TabIndex = 2;
            this.offSyncRadioButton.TabStop = true;
            this.offSyncRadioButton.Text = "Off";
            this.offSyncRadioButton.UseVisualStyleBackColor = true;
            // 
            // SQLiteConnectionStringEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 336);
            this.Controls.Add(this.databaseGroupBox);
            this.Controls.Add(this.encodingGroupBox);
            this.Controls.Add(this.dateFormatGroupBox);
            this.Controls.Add(this.syncGroupBox);
            this.Controls.Add(this.encryptionGroupBox);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(340, 370);
            this.Name = "SQLiteConnectionStringEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SQLite Connection String Editor";
            this.Load += new System.EventHandler(this.SQLiteConnectionStringEditor_Load);
            this.databaseGroupBox.ResumeLayout(false);
            this.databaseGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pageSizeNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cacheSizeNumericUpDown)).EndInit();
            this.encryptionGroupBox.ResumeLayout(false);
            this.encryptionGroupBox.PerformLayout();
            this.encodingGroupBox.ResumeLayout(false);
            this.encodingGroupBox.PerformLayout();
            this.dateFormatGroupBox.ResumeLayout(false);
            this.dateFormatGroupBox.PerformLayout();
            this.syncGroupBox.ResumeLayout(false);
            this.syncGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox databaseGroupBox;
        private System.Windows.Forms.TextBox DataSourceTextBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.GroupBox encryptionGroupBox;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Button testButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label cacheLabel;
        private System.Windows.Forms.Label pageLabel;
        private System.Windows.Forms.GroupBox encodingGroupBox;
        private System.Windows.Forms.GroupBox dateFormatGroupBox;
        private System.Windows.Forms.GroupBox syncGroupBox;
        private System.Windows.Forms.RadioButton utf16RadioButton;
        private System.Windows.Forms.RadioButton utf8RadioButton;
        private System.Windows.Forms.RadioButton ticksRadioButton;
        private System.Windows.Forms.RadioButton julianRadioButton;
        private System.Windows.Forms.RadioButton iso8601RadioButton;
        private System.Windows.Forms.RadioButton offSyncRadioButton;
        private System.Windows.Forms.RadioButton normalSyncRadioButton;
        private System.Windows.Forms.RadioButton fullSyncRadioButton;
        private System.Windows.Forms.NumericUpDown cacheSizeNumericUpDown;
        private System.Windows.Forms.NumericUpDown pageSizeNumericUpDown;
        private System.Windows.Forms.CheckBox poolCheckBox;
    }
}