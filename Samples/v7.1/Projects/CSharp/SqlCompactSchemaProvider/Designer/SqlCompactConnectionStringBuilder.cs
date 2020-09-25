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
using System.Text;

/// <summary>
/// SQL Compact Connection string builder
/// </summary>
public class SqlCompactConnectionStringBuilder 
{
    private string othervalues;

    public SqlCompactConnectionStringBuilder(string connectionString)
    {
        // set defaults
        this.DataSource = "";
        this.Password = "";
        this.MaxDatabaseSize = 128;
        this.othervalues = "";

        // Parse existing connection string
        if (!string.IsNullOrEmpty(connectionString))
        {
            string[] valuePairs = connectionString.Split(new char[] { ';' });
            foreach (string valuePair in valuePairs)
            {
                string[] keyVals = valuePair.Split(new char[] { '=' });
                if (keyVals.Length == 2)
                { 
                    if (!string.IsNullOrEmpty(keyVals[0]))
                    {
                        switch (keyVals[0].ToLowerInvariant())
                        {
                        case "datasource":
                        case "data source":
                            this.DataSource = keyVals[1];
                            break;

                        case "password":
                        case "pwd":
                        case "database password":
                            this.Password = keyVals[1];
                            break;
                        case "max database size":
                            int res = 128;
                            if (int.TryParse(keyVals[1], out res))
                            {
                                this.MaxDatabaseSize = res;
                            }
                            break;
                        default:
                            this.othervalues = this.othervalues + ";" + valuePair;
                            break;
                        }
                    }
                }
            }
        }
    }

    public SqlCompactConnectionStringBuilder()  : this("")
    {
    }

    /// <summary>
    /// Sets the connection string.
    /// </summary>
    /// <value>The connection string.</value>
    public string ConnectionString
    {
        get
        {
            //TODO create entire string here
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Data Source={0};", this.DataSource));
            if (!string.IsNullOrEmpty(this.Password))
                sb.Append(string.Format("Password={0};", this.Password));
            if (this.MaxDatabaseSize != 128)
                sb.Append(string.Format("Max Database Size={0};", this.MaxDatabaseSize));
            if (this.othervalues.Length > 1 && this.othervalues.StartsWith(";"))
            {
                this.othervalues = this.othervalues.Substring(1);
            }
            sb.Append(this.othervalues);
            return sb.ToString();
        }
    }

    /// <summary>
    /// Gets or sets the data source.
    /// </summary>
    /// <value>The data source.</value>
    public string DataSource { get; set; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    /// <value>The password.</value>
    public string Password { get; set; }

    /// <summary>
    /// Gets or sets the max size of the database.
    /// </summary>
    /// <value>The max size of max database.</value>
    public int MaxDatabaseSize 
    { get; set; }

}

