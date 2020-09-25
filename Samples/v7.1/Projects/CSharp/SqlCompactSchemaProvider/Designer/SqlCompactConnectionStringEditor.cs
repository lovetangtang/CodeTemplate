//------------------------------------------------------------------------------
//
// Copyright (c) 2002-2012 CodeSmith Tools, LLC.  All rights reserved.
// 
// The terms of use for this software are contained in the file
// named sourcelicense.txt, which can be found in the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by the
// terms of this license.
// 
// You must not remove this notice, or any other, from this software.
//
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Windows.Forms;

namespace SchemaExplorer
{
    public partial class SqlCompactConnectionStringEditor : Form
    {
        private readonly SqlCompactConnectionStringBuilder _connectionStringBuilder = null;
        private bool _disposed;

        public SqlCompactConnectionStringEditor(string connectionString)
        {
            _connectionStringBuilder = new SqlCompactConnectionStringBuilder(connectionString);
            InitializeComponent();
        }

        private void UpdateControls()
        {
            dataSourceTextBox.Text = _connectionStringBuilder.DataSource;
            if (_connectionStringBuilder.MaxDatabaseSize > 127 && _connectionStringBuilder.MaxDatabaseSize < 4097)
            {
                minNumericUpDown.Value = _connectionStringBuilder.MaxDatabaseSize;
            }
            else
            {
                minNumericUpDown.Value = 128;
            }
            passwordTextBox.Text = _connectionStringBuilder.Password;
        }

        private void UpdateBuilder()
        {
            _connectionStringBuilder.DataSource = dataSourceTextBox.Text;
            if (_connectionStringBuilder.Password != passwordTextBox.Text)
                _connectionStringBuilder.Password = passwordTextBox.Text;
            if (_connectionStringBuilder.MaxDatabaseSize != minNumericUpDown.Value)
                _connectionStringBuilder.MaxDatabaseSize = (int)minNumericUpDown.Value;
        }

        public string ConnectionString
        {
            get
            {
                UpdateBuilder();
                return _connectionStringBuilder.ConnectionString;
            }
        }

        private void SqlCompactConnectionStringEditor_Load(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void testButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (System.Data.SqlServerCe.SqlCeConnection connection = new System.Data.SqlServerCe.SqlCeConnection(ConnectionString))
                {
                    connection.Open();
                    MessageBox.Show(this, "Connection Test Was Successful.",
                        "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Connection Test Failed." + Environment.NewLine + Environment.NewLine + ex.GetBaseException().Message,
                    "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (File.Exists(dataSourceTextBox.Text))
                openFileDialog.FileName = dataSourceTextBox.Text;
            if (Path.IsPathRooted(dataSourceTextBox.Text) && Directory.Exists(Path.GetDirectoryName(dataSourceTextBox.Text)))
                openFileDialog.InitialDirectory = Path.GetDirectoryName(dataSourceTextBox.Text);

            DialogResult result = openFileDialog.ShowDialog(this);
            if (result != DialogResult.OK)
                return;

            dataSourceTextBox.Text = openFileDialog.FileName;
        }
    }
}
