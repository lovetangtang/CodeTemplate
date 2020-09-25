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
using VistaDB;
using VistaDB.Provider;

namespace SchemaExplorer.Designer
{
    public partial class VistaDBConnectionStringEditor : Form
    {
        private readonly VistaDBConnectionStringBuilder _connectionStringBuilder = new VistaDBConnectionStringBuilder();
        private bool _disposed;

        public VistaDBConnectionStringEditor()
        {
            InitializeComponent();
        }

        public VistaDBConnectionStringEditor(string connectionString) : this()
        {
            // If the connection string is garbage then the following code will throw an exception.
            try
            {
                _connectionStringBuilder.ConnectionString = connectionString;
            }
            catch (ArgumentException) { }
        }

        private void UpdateControls()
        {
            dataSourceTextBox.Text = _connectionStringBuilder.DataSource;
            poolCheckBox.Checked = _connectionStringBuilder.Pooling;
            minNumericUpDown.Value = _connectionStringBuilder.MinPoolSize;
            maxNumericUpDown.Value = _connectionStringBuilder.MaxPoolSize;
            passwordTextBox.Text = _connectionStringBuilder.Password;
            modeComboBox.SelectedItem = _connectionStringBuilder.OpenMode.ToString();
        }

        private void UpdateBuilder()
        {
            _connectionStringBuilder.DataSource = dataSourceTextBox.Text;
            if (_connectionStringBuilder.Pooling != poolCheckBox.Checked)
                _connectionStringBuilder.Pooling = poolCheckBox.Checked;
            if (_connectionStringBuilder.MinPoolSize != Convert.ToInt32(minNumericUpDown.Value))
                _connectionStringBuilder.MinPoolSize = Convert.ToInt32(minNumericUpDown.Value);
            if (_connectionStringBuilder.MaxPoolSize != Convert.ToInt32(maxNumericUpDown.Value))
                _connectionStringBuilder.MaxPoolSize = Convert.ToInt32(maxNumericUpDown.Value);
            if (_connectionStringBuilder.Password != passwordTextBox.Text)
                _connectionStringBuilder.Password = passwordTextBox.Text;

            VistaDBDatabaseOpenMode selectedMode = (VistaDBDatabaseOpenMode)Enum.Parse(
                typeof(VistaDBDatabaseOpenMode), modeComboBox.SelectedItem.ToString());

            if (_connectionStringBuilder.OpenMode != selectedMode)
                _connectionStringBuilder.OpenMode = selectedMode;
        }

        public string ConnectionString
        {
            get
            {
                UpdateBuilder();
                return _connectionStringBuilder.ConnectionString;
            }
        }

        private void VistaDBConnectionStringEditor_Load(object sender, EventArgs e)
        {
            modeComboBox.BeginUpdate();
            modeComboBox.Items.Clear();
            modeComboBox.Items.AddRange(Enum.GetNames(typeof(VistaDBDatabaseOpenMode)));
            modeComboBox.EndUpdate();
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
            using (VistaDBConnection connection = new VistaDBConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    MessageBox.Show(this, "Connection Test Was Successful.",
                        "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Connection Test Failed." + Environment.NewLine + Environment.NewLine + ex.GetBaseException().Message,
                        "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
