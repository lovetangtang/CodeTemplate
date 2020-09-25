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
using System.ComponentModel;
using System.IO;
using CodeSmith.CustomProperties;
using CodeSmith.Engine;

namespace CodeSmith.BaseTemplates
{
    /// <summary>
    /// Templates can be derived from this class to allow their output to be saved to file during generation.
    /// </summary>
    public class OutputFileCodeTemplate : CodeTemplate
    {
        protected string _outputFile = String.Empty;

        /// <summary>
        /// This property is used to specify an output file name that the template output will be saved to.
        /// </summary>
        [EditorAttribute(typeof(FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        [Category("Output")]
        [FileDialog(FileDialogType.Save, Title="Select Output File")]
        [Description("Select a file to output the results")]
        [Optional]
        public virtual string OutputFile
        {
            get {return _outputFile;}
            set {_outputFile = value;}
        }
        
        /// <summary>
        /// Override the OnPostRender method so that we can take the result and save it to a file if an output file was specified.
        /// </summary>
        /// <param name="result"></param>
        protected override void OnPostRender(string result)
        {
            if (!String.IsNullOrEmpty(OutputFile))
            {
                FileStream stream = null;
                try
                {
                    stream = new FileStream(this.OutputFile, FileMode.Create, FileAccess.Write);
                    byte[] output = System.Text.Encoding.UTF8.GetBytes(result);
                    stream.Write(output, 0, output.Length);
                }
                finally
                {
                    if (stream != null) stream.Close();
                }
            }
            
            base.OnPostRender(result);
        }
    }
}
