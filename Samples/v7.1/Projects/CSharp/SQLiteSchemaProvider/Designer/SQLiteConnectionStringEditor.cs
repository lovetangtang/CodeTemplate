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
using System.Data.Common;
using System.IO;
using System.Windows.Forms;

namespace SchemaExplorer.Designer
{
    public partial class SQLiteConnectionStringEditor : Form
    {
        private class Keys
        {
            internal const string DataSource = "data source";
            internal const string CacheSize = "cache size";
            internal const string PageSize = "page size";
            internal const string Password = "password";
            internal const string Pooling = "pooling";
            internal const string UseUTF16Encoding = "useutf16encoding";
            internal const string DateTimeFormat = "datetimeformat";
            internal const string SyncMode = "synchronous";
        }

        private readonly DbConnectionStringBuilder _builder;
        private bool _disposed;

        public SQLiteConnectionStringEditor()
        {
            _builder = GetConnectionStringBuilder();
            InitializeComponent();
        }

        public SQLiteConnectionStringEditor(string connectionString) : this()
        {
            // If the connection string is garbage then the following code will throw an exception.
            try
            {
                _builder.ConnectionString = connectionString;
            }
            catch (ArgumentException) { }
        }

        private void UpdateControls()
        {
            if (_builder.ContainsKey(Keys.DataSource))
                DataSourceTextBox.Text = _builder[Keys.DataSource].ToString();
            if (_builder.ContainsKey(Keys.CacheSize))
                cacheSizeNumericUpDown.Value = int.Parse(_builder[Keys.CacheSize].ToString());
            if (_builder.ContainsKey(Keys.PageSize))
                pageSizeNumericUpDown.Value = int.Parse(_builder[Keys.PageSize].ToString());
            if (_builder.ContainsKey(Keys.Password))
                passwordTextBox.Text = _builder[Keys.Password].ToString();
            if (_builder.ContainsKey(Keys.Pooling))
                poolCheckBox.Checked = bool.Parse(_builder[Keys.Pooling].ToString());

            if (_builder.ContainsKey(Keys.UseUTF16Encoding))
            {
                utf8RadioButton.Checked = !bool.Parse(_builder[Keys.UseUTF16Encoding].ToString());
                utf16RadioButton.Checked = bool.Parse(_builder[Keys.UseUTF16Encoding].ToString());
            }

            if (_builder.ContainsKey(Keys.DateTimeFormat))
            {
                ticksRadioButton.Checked = false;
                iso8601RadioButton.Checked = false;
                julianRadioButton.Checked = false;

                switch (_builder[Keys.DateTimeFormat].ToString())
                {
                    case "Ticks":
                        ticksRadioButton.Checked = true;
                        break;
                    case "JulianDay":
                        julianRadioButton.Checked = true;
                        break;
                    default:
                        iso8601RadioButton.Checked = true;
                        break;
                }
            }

            if (_builder.ContainsKey(Keys.SyncMode))
            {
                normalSyncRadioButton.Checked = false;
                fullSyncRadioButton.Checked = false;
                offSyncRadioButton.Checked = false;

                switch (_builder[Keys.SyncMode].ToString())
                {
                    case "Full":
                        fullSyncRadioButton.Checked = true;
                        break;
                    case "Off":
                        offSyncRadioButton.Checked = true;
                        break;
                    default:
                        normalSyncRadioButton.Checked = true;
                        break;
                }
            }
        }

        private void UpdateBuilder()
        {
            _builder[Keys.DataSource] = DataSourceTextBox.Text;

            if (cacheSizeNumericUpDown.Value != 2000)
                _builder[Keys.CacheSize] = Convert.ToInt32(cacheSizeNumericUpDown.Value);
            else if (_builder.ContainsKey(Keys.CacheSize))
                _builder.Remove(Keys.CacheSize);

            if (pageSizeNumericUpDown.Value != 1024)
                _builder[Keys.PageSize] = Convert.ToInt32(pageSizeNumericUpDown.Value);
            else if (_builder.ContainsKey(Keys.PageSize))
                _builder.Remove(Keys.PageSize);

            if (poolCheckBox.Checked)
                _builder[Keys.Pooling] = poolCheckBox.Checked;
            else if (_builder.ContainsKey(Keys.Pooling))
                _builder.Remove(Keys.Pooling);

            if (utf16RadioButton.Checked)
                _builder[Keys.UseUTF16Encoding] = utf16RadioButton.Checked;
            else if (_builder.ContainsKey(Keys.UseUTF16Encoding))
                _builder.Remove(Keys.UseUTF16Encoding);

            if (ticksRadioButton.Checked)
                _builder[Keys.DateTimeFormat] = "Ticks";
            else if (julianRadioButton.Checked)
                _builder[Keys.DateTimeFormat] = "JulianDay";
            else if (_builder.ContainsKey(Keys.DateTimeFormat))
                _builder.Remove(Keys.DateTimeFormat);

            if (fullSyncRadioButton.Checked)
                _builder[Keys.SyncMode] = "Full";
            else if (offSyncRadioButton.Checked)
                _builder[Keys.SyncMode] = "Off";
            else if (_builder.ContainsKey(Keys.SyncMode))
                _builder.Remove(Keys.SyncMode);

            if (!string.IsNullOrEmpty(passwordTextBox.Text))
                _builder[Keys.Password] = passwordTextBox.Text;
            else if (_builder.ContainsKey(Keys.Password))
                _builder.Remove(Keys.Password);
        }

        public string ConnectionString
        {
            get
            {
                UpdateBuilder();
                return _builder.ConnectionString;
            }
        }

        private void SQLiteConnectionStringEditor_Load(object sender, EventArgs e)
        {
            UpdateControls();
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (File.Exists(DataSourceTextBox.Text))
                openFileDialog.FileName = DataSourceTextBox.Text;
            if (Path.IsPathRooted(DataSourceTextBox.Text) && Directory.Exists(Path.GetDirectoryName(DataSourceTextBox.Text)))
                openFileDialog.InitialDirectory = Path.GetDirectoryName(DataSourceTextBox.Text);

            DialogResult result = openFileDialog.ShowDialog(this);
            if (result != DialogResult.OK)
                return;

            DataSourceTextBox.Text = openFileDialog.FileName;
        }

        private void testButton_Click(object sender, EventArgs e)
        {
            using (DbConnection connection = CreateConnection(ConnectionString))
            {
                try
                {
                    connection.Open();
                    MessageBox.Show(this, "Connection Test Was Successful.",  "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, string.Format("Connection Test Failed.{0}{0}{1}", Environment.NewLine, ex.GetBaseException().Message), "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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

        private DbConnectionStringBuilder GetConnectionStringBuilder()
        {
            DbProviderFactory factory = CreateDbProviderFactory();
            DbConnectionStringBuilder builder = factory.CreateConnectionStringBuilder();
            builder[Keys.DataSource] = DataSourceTextBox != null ? DataSourceTextBox.Text : string.Empty;
            builder[Keys.Pooling] = true;

            return builder;
        }

        #region DbProvider Helpers

        private static DbProviderFactory CreateDbProviderFactory() {
            DbProviderFactory dbProviderFactory;

            try {
                dbProviderFactory = DbProviderFactories.GetFactory("System.Data.SQLite");
            } catch (ArgumentException ex) {
                throw new ApplicationException("The System.Data.SQLite library is not installed on this computer. Please see the following documentation 'http://docs.codesmithtools.com/display/Generator/Configuring+and+troubleshooting+a+Schema+Provider' for more information. Error: {0}", ex);
            }

            return dbProviderFactory;
        }

        private static DbConnection CreateConnection(string connectionString)
        {
            DbProviderFactory factory = CreateDbProviderFactory();
            DbConnection connection = factory.CreateConnection();
            connection.ConnectionString = connectionString;

            return connection;
        }

        #endregion
    }
}
