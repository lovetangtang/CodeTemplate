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

namespace SchemaExplorer
{
    partial class OracleConfigurationControl
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
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #endregion
        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.configPropertyGrid = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();
            // 
            // configPropertyGrid
            // 
            this.configPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configPropertyGrid.Location = new System.Drawing.Point(0, 0);
            this.configPropertyGrid.Name = "configPropertyGrid";
            this.configPropertyGrid.Size = new System.Drawing.Size(315, 349);
            this.configPropertyGrid.TabIndex = 0;
            // 
            // AdvancedEngineControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.configPropertyGrid);
            this.Name = "OracleConfigurationControl";
            this.Size = new System.Drawing.Size(315, 349);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.PropertyGrid configPropertyGrid;
    }
}
