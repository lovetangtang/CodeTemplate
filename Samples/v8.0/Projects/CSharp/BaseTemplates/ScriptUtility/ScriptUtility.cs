//------------------------------------------------------------------------------
//
// Copyright (c) 2002-2017 CodeSmith Tools, LLC.  All rights reserved.
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
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace CodeSmith.BaseTemplates
{
    /// <summary>
    /// This class can be used to execute SQL scripts on a database.  It takes a SQL script, parsers out the GO's and executes each batch remembering the correct line numbers.
    /// </summary>
    public class ScriptUtility
    {
        #region Events

        public event SqlInfoMessageEventHandler ScriptInfoMessage;

        #endregion

        #region Private Members

        private readonly Regex _batchRegex = new Regex(@"(?<=^)[ \t]*\bGO\b[ \t\r]*\n?$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
        private readonly Regex _lineRegex = new Regex(@"\r?\n",RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);

        #endregion

        #region Constructor(s)

        public ScriptUtility()
        {
        }
        
        public ScriptUtility(string connectionString, string script)
        {
            ConnectionString = connectionString;
            Script = script;
        }

        #endregion

        #region Public Method(s)

        /// <summary>
        /// Executes the script.
        /// </summary>
        /// <returns></returns>
        public ScriptResult ExecuteScript()
        {
            var errors = new ScriptErrorCollection();
            var success = false;

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.InfoMessage += cn_InfoMessage;
                connection.Open();

                foreach (Batch batch in SplitScript(Script))
                {
                    using (IDbCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = batch.Content;
                        cmd.Connection = connection;

                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            foreach (SqlError error in ex.Errors)
                            {
                                errors.Add(new ScriptError(error.Number, error.State, error.Class, 
                                                           error.Message, error.Procedure, 
                                                           error.LineNumber + batch.StartLineNumber - 1));
                            }
                        }
                    }
                }

                if(connection.State != ConnectionState.Closed)
                    connection.Close();
            }

            if (errors.Count == 0) success = true;

            return new ScriptResult(success, errors);
        }

        /// <summary>
        /// Executes a SQL script using the given connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="script"></param>
        /// <returns></returns>
        public static ScriptResult ExecuteScript(string connectionString, string script)
        {
            ScriptUtility utility = new ScriptUtility(connectionString, script);
            return utility.ExecuteScript();
        }
        
        /// <summary>
        /// Executes a SQL script using the given connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="script"></param>
        /// <param name="infoMessageHandler"></param>
        /// <returns></returns>
        public static ScriptResult ExecuteScript(string connectionString, string script, SqlInfoMessageEventHandler infoMessageHandler)
        {
            ScriptUtility utility = new ScriptUtility(connectionString, script);
            utility.ScriptInfoMessage += infoMessageHandler;
            return utility.ExecuteScript();
        }

        #endregion
        
        #region Private Methods

        private void cn_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            if (ScriptInfoMessage != null)
            {
                ScriptInfoMessage(sender, e);
            }
        }

        private List<Batch> SplitScript(string script)
        {
            List<Batch> batchList = new List<Batch>();

            int currentIndex = 0;
            int currentLine = 1;
            foreach (Match match in _batchRegex.Matches(script))
            {
                string content = script.Substring(currentIndex, match.Index - currentIndex);
                int lineCount = _lineRegex.Matches(content).Count;
                
                if (content.Trim().Length > 0)
                {
                    Batch batch = new Batch(currentLine, lineCount, content);
                    batchList.Add(batch);
                }
                
                currentIndex = match.Index + match.Length;
                currentLine += lineCount + 1;
            }
            
            if (currentIndex < script.Length - 1)
            {
                string content = script.Substring(currentIndex);
                int lineCount = _lineRegex.Matches(content).Count;
                Batch batch = new Batch(currentLine, lineCount, content);
                batchList.Add(batch);
            }

            return batchList;
        }

        #endregion

        #region Public Proerties

        /// <summary>
        /// The connection string to use when executing the script.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The script to execute.
        /// </summary>
        public string Script { get; set; }

        #endregion
    }
}
