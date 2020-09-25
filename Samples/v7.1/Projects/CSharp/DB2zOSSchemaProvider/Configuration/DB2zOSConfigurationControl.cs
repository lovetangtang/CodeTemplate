//------------------------------------------------------------------------------
//
// Copyright (c) 2002-2010 CodeSmith Tools, LLC.  All rights reserved.
// 
// The terms of use for this software are contained in the file
// named sourcelicense.txt, which can be found in the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by the
// terms of this license.
// 
// You must not remove this notice, or any other, from this software.
// 
// Credits:
//    Provider was written by Bill Hall <bhall@dayspring.com> and Dan Gowin <dang@dayspring.com> from DaySpring Cards (http://www.dayspring.com/).
//    Some original work may have been used by DaySpring as reference by Geoff McElhanon's DB2 Provider.
//
// Notes:
//    A user must have the IBM iSeries Access Client installed to use this provider.
//------------------------------------------------------------------------------

using System;
using System.Windows.Forms;
using CodeSmith.Gui.Options;

namespace SchemaExplorer
{
    public partial class DB2zOSConfigurationControl : UserControl, ISettingsControl
    {
        public DB2zOSConfigurationControl()
        {
            InitializeComponent();
        }

        public string SettingPath
        {
            get { return "Schema Provider|DB2 zOS"; }
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

            configPropertyGrid.SelectedObject = DB2zOSConfiguration.Instance;
        }

        public void CancelSettings()
        {

        }

        public void SaveSettings()
        {

        }
    }
}
