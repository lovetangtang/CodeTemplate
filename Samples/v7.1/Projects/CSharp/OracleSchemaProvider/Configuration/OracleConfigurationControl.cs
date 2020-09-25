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
using System.Windows.Forms;

using CodeSmith.Gui.Options;

namespace SchemaExplorer
{
    public partial class OracleConfigurationControl : UserControl, ISettingsControl
    {
        public OracleConfigurationControl()
        {
            InitializeComponent();
        }

        public string SettingPath
        {
            get { return "Schema Provider|Oracle"; }
        }

        public int SettingOrder
        {
            get { return 1200; }
        }

        public bool IsVisible
        {
            get { return true; }
        }

        public void LoadSettings()
        {
            configPropertyGrid.SelectedObject = OracleConfiguration.Instance;
        }

        public void CancelSettings()
        {

        }

        public void SaveSettings()
        {

        }
    }
}
