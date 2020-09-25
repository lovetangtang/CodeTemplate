

namespace SchemaExplorer
{
    partial class iSeriesConfigurationControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

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
            // iSeriesConfigurationControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.configPropertyGrid);
            this.Name = "iSeriesConfigurationControl";
            this.Size = new System.Drawing.Size(315, 349);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid configPropertyGrid;
    }
}
