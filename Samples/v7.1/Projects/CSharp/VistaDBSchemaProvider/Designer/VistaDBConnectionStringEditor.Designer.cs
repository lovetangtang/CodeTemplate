namespace SchemaExplorer.Designer
{
    partial class VistaDBConnectionStringEditor
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
            this.poolCheckBox = new System.Windows.Forms.CheckBox();
            this.minLabel = new System.Windows.Forms.Label();
            this.minNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.maxLabel = new System.Windows.Forms.Label();
            this.maxNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.encryptionGroupBox = new System.Windows.Forms.GroupBox();
            this.passwordLabel = new System.Windows.Forms.Label();
            this.passwordTextBox = new System.Windows.Forms.TextBox();
            this.modeGroupBox = new System.Windows.Forms.GroupBox();
            this.modeComboBox = new System.Windows.Forms.ComboBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.databaseGroupBox.SuspendLayout();
            this.poolingGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.minNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxNumericUpDown)).BeginInit();
            this.encryptionGroupBox.SuspendLayout();
            this.modeGroupBox.SuspendLayout();
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
            this.browseButton.TabIndex = 2;
            this.browseButton.Text = "...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(236, 302);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(155, 302);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "&OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // testButton
            // 
            this.testButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.testButton.Location = new System.Drawing.Point(13, 300);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(75, 23);
            this.testButton.TabIndex = 6;
            this.testButton.Text = "&Test";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.testButton_Click);
            // 
            // poolingGroupBox
            // 
            this.poolingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.poolingGroupBox.Controls.Add(this.poolCheckBox);
            this.poolingGroupBox.Controls.Add(this.minLabel);
            this.poolingGroupBox.Controls.Add(this.minNumericUpDown);
            this.poolingGroupBox.Controls.Add(this.maxLabel);
            this.poolingGroupBox.Controls.Add(this.maxNumericUpDown);
            this.poolingGroupBox.Location = new System.Drawing.Point(13, 78);
            this.poolingGroupBox.Name = "poolingGroupBox";
            this.poolingGroupBox.Size = new System.Drawing.Size(300, 87);
            this.poolingGroupBox.TabIndex = 1;
            this.poolingGroupBox.TabStop = false;
            this.poolingGroupBox.Text = "Pooling";
            // 
            // poolCheckBox
            // 
            this.poolCheckBox.AutoSize = true;
            this.poolCheckBox.Location = new System.Drawing.Point(13, 26);
            this.poolCheckBox.Name = "poolCheckBox";
            this.poolCheckBox.Size = new System.Drawing.Size(154, 17);
            this.poolCheckBox.TabIndex = 0;
            this.poolCheckBox.Text = "Enable Connection &Pooling";
            this.poolCheckBox.UseVisualStyleBackColor = true;
            // 
            // minLabel
            // 
            this.minLabel.AutoSize = true;
            this.minLabel.Location = new System.Drawing.Point(8, 57);
            this.minLabel.Name = "minLabel";
            this.minLabel.Size = new System.Drawing.Size(47, 13);
            this.minLabel.TabIndex = 1;
            this.minLabel.Text = "Mi&n Size";
            // 
            // minNumericUpDown
            // 
            this.minNumericUpDown.Location = new System.Drawing.Point(61, 53);
            this.minNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.minNumericUpDown.Name = "minNumericUpDown";
            this.minNumericUpDown.Size = new System.Drawing.Size(78, 20);
            this.minNumericUpDown.TabIndex = 2;
            // 
            // maxLabel
            // 
            this.maxLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.maxLabel.AutoSize = true;
            this.maxLabel.Location = new System.Drawing.Point(151, 57);
            this.maxLabel.Name = "maxLabel";
            this.maxLabel.Size = new System.Drawing.Size(50, 13);
            this.maxLabel.TabIndex = 3;
            this.maxLabel.Text = "Ma&x Size";
            // 
            // maxNumericUpDown
            // 
            this.maxNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.maxNumericUpDown.Location = new System.Drawing.Point(207, 53);
            this.maxNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.maxNumericUpDown.Name = "maxNumericUpDown";
            this.maxNumericUpDown.Size = new System.Drawing.Size(78, 20);
            this.maxNumericUpDown.TabIndex = 4;
            this.maxNumericUpDown.Value = new decimal(new int[] {
            100,
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
            this.encryptionGroupBox.Location = new System.Drawing.Point(13, 171);
            this.encryptionGroupBox.Name = "encryptionGroupBox";
            this.encryptionGroupBox.Size = new System.Drawing.Size(299, 57);
            this.encryptionGroupBox.TabIndex = 2;
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
            this.passwordTextBox.Size = new System.Drawing.Size(226, 20);
            this.passwordTextBox.TabIndex = 1;
            // 
            // modeGroupBox
            // 
            this.modeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.modeGroupBox.Controls.Add(this.modeComboBox);
            this.modeGroupBox.Location = new System.Drawing.Point(12, 234);
            this.modeGroupBox.Name = "modeGroupBox";
            this.modeGroupBox.Size = new System.Drawing.Size(299, 56);
            this.modeGroupBox.TabIndex = 3;
            this.modeGroupBox.TabStop = false;
            this.modeGroupBox.Text = "Connection &Mode";
            // 
            // modeComboBox
            // 
            this.modeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.modeComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.modeComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.modeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.modeComboBox.FormattingEnabled = true;
            this.modeComboBox.Location = new System.Drawing.Point(7, 21);
            this.modeComboBox.Name = "modeComboBox";
            this.modeComboBox.Size = new System.Drawing.Size(286, 21);
            this.modeComboBox.TabIndex = 0;
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "vdb3";
            this.openFileDialog.Filter = "VistaDB Database (*.vdb3)|*.vdb3|All Files (*.*)|*.*";
            this.openFileDialog.Title = "Open VistaDB Database";
            // 
            // VistaDBConnectionStringEditor
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(324, 336);
            this.Controls.Add(this.databaseGroupBox);
            this.Controls.Add(this.poolingGroupBox);
            this.Controls.Add(this.encryptionGroupBox);
            this.Controls.Add(this.modeGroupBox);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(340, 370);
            this.Name = "VistaDBConnectionStringEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "VistaDB Connection String Editor";
            this.Load += new System.EventHandler(this.VistaDBConnectionStringEditor_Load);
            this.databaseGroupBox.ResumeLayout(false);
            this.databaseGroupBox.PerformLayout();
            this.poolingGroupBox.ResumeLayout(false);
            this.poolingGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.minNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxNumericUpDown)).EndInit();
            this.encryptionGroupBox.ResumeLayout(false);
            this.encryptionGroupBox.PerformLayout();
            this.modeGroupBox.ResumeLayout(false);
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
        private System.Windows.Forms.Label maxLabel;
        private System.Windows.Forms.Label minLabel;
        private System.Windows.Forms.CheckBox poolCheckBox;
        private System.Windows.Forms.NumericUpDown maxNumericUpDown;
        private System.Windows.Forms.GroupBox encryptionGroupBox;
        private System.Windows.Forms.TextBox passwordTextBox;
        private System.Windows.Forms.Label passwordLabel;
        private System.Windows.Forms.GroupBox modeGroupBox;
        private System.Windows.Forms.ComboBox modeComboBox;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}