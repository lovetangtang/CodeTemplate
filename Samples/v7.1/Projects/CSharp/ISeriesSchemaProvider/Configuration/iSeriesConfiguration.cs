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
// Credits:
//    Provider was written by Bill Hall <bhall@dayspring.com> and Dan Gowin <dang@dayspring.com> from DaySpring Cards (http://www.dayspring.com/).
//    Some original work may have been used by DaySpring as reference by Geoff McElhanon's DB2 Provider.
//
// Notes:
//    A user must have the IBM iSeries Access Client installed to use this provider.
//------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using CodeSmith.Engine;

namespace SchemaExplorer
{
    class iSeriesConfiguration : CachedConfiguration
    {
        #region Properties

        /// <summary>
        /// If true, allows the discovery of CommandResultSchema's. Warning This may modify data in your database.
        /// </summary>
        [DefaultValue(false)]
        [Description("If true, allows the discovery of CommandResultSchema's. Warning This may modify data in your database.")]
        public bool AllowGetCommandResultSchemas
        {
            get
            {
                return GetCachedValue("AllowGetCommandResultSchemas", false);
            }
            set
            {
                SetValue("AllowGetCommandResultSchemas", value);
            }
        }

        /// <summary>
        /// If true, the Extended Properties table will be created if it does not exist.
        /// </summary>
        [DefaultValue(true)]
        [Description("Set this to true if you want the Extended Properties Table to automatically created.")]
        public bool AutoCreateExtendedPropertiesTable
        {
            get
            {
                return GetCachedValue("AutoCreateExtendedPropertiesTable", true);
            }
            set
            {
                SetValue("AutoCreateExtendedPropertiesTable", value);
            }
        }

        /// <summary>
        /// The owner of the extended properties table (e.g. HR).
        /// </summary>
        [DefaultValue("")]
        [Description("This is the schema in which the Extended Properties Table is located on the iSeries server.")]
        public string ExtendedPropertiesTableSchema
        {
            get
            {
                return GetCachedValue("ExtendedPropertiesTableSchema", "").ToUpper();
            }
            set
            {
                if (value != null)
                {
                    SetValue("ExtendedPropertiesTableSchema", value.ToUpper());
                }
            }
        }

        /// <summary>
        /// The schema or library you wish to filter on.
        /// </summary>
        [DefaultValue("")]
        [Description("The schema that CodeSmith Generator will retrieve tables from. If this is left blank then all objects on the server will be analyzed.  You can put multiples in separated by a comma.  Use % for wildcard.  _ stands for any single character.  For instance IIM_%.")]
        public string FilterSchema
        {
            get
            {
                return GetCachedValue("FilterSchema", "").ToUpper();
            }
            set
            {
                SetValue("FilterSchema", value.ToUpper());
            }
        }

        [DefaultValue("")]
        [Description("The tables that CodeSmith Generator will analyze. If this is left blank then all tables on the server, or in the filtered schema will be analyzed.  You can put multiples in separated by a comma.  Use % for wildcard.  _ stands for any single character.  For instance IIM_%.")]
        public string FilterTables
        {
            get
            {
                return GetCachedValue("FilterTables", "").ToUpper();
            }
            set
            {
                SetValue("FilterTables", value.ToUpper());
            }
        }

        [DefaultValue("")]
        [Description("The views that CodeSmith Generator will analyze. If this is left blank then all views on the server, or in the filtered schema will be analyzed.  You can put multiples in separated by a comma.  Use % for wildcard.  _ stands for any single character.  For instance IIM_%.")]
        public string FilterViews
        {
            get
            {
                return GetCachedValue("FilterViews", "").ToUpper();
            }
            set
            {
                SetValue("FilterViews", value.ToUpper());
            }
        }

        [DefaultValue(0)]
        [Description("The number in seconds for iDB2Commands to run before a timeout occurs (0 = no limit). The maximum timeout is 5 minutes (300 sec).")]
        public int CommandTimeout
        {
            get
            {
                return GetCachedValue("commandTimeout", 0);
            }
            set
            {
                if ((value >= 0) && (value <= 300))
                {
                    SetValue("commandTimeout", value);
                }
                else if (value > 300)
                    SetValue("commandTimeout", 300);
                else
                    SetValue("commandTimeout", 0);
            }
        }

        [DefaultValue("CS_IsPrimaryKey")]
        [Description("This is the extended property name you would set to a specific column to be a primary key.")]
        public string FakePrimaryKey {
            get {
                return GetCachedValue("IsPrimaryKeyExtendedPropertyName", "CS_IsPrimaryKey");
            }
        }

        #endregion

        #region Overrides of ConfigurationBase

        protected override string NamePrefix
        {
            get { return "SchemaExplorer\\ISeriesSchemaProvider"; }
        }

        public override void Initialize()
        {
            CodeSmith.Engine.Configuration.Instance.EnsureInitialize();

            if (ConfigurationVersion != VersionCheck.GetShortBuild())
                Upgrade();
        }

        public override void LoadDefaults()
        {
            AllowGetCommandResultSchemas = false;
            AutoCreateExtendedPropertiesTable = true;
            ExtendedPropertiesTableSchema = "";
            FilterSchema = "";
            FilterTables = "";
            FilterViews = "";
            CommandTimeout = 1800;
        }

        public override void ResetDefaults()
        {
        }

        public override void Upgrade()
        {
            ConfigurationVersion = VersionCheck.GetShortBuild();
        }

        #endregion

        #region Singleton

        /// <summary>
        /// Gets the current singleton instance of Configuration.
        /// </summary>
        /// <value>The current singleton instance.</value>
        /// <remarks>
        /// An instance of Configuration wont be created until the very first 
        /// call to the sealed class. This is a CLR optimization that
        /// provides a properly lazy-loading singleton. 
        /// </remarks>
        public static iSeriesConfiguration Instance
        {
            get { return Nested.Current; }
        }

        /// <summary>
        /// Nested class to lazy-load singleton.
        /// </summary>
        private class Nested
        {
            /// <summary>
            /// Initializes the Nested class.
            /// </summary>
            /// <remarks>
            /// Explicit static constructor to tell C# compiler not to mark type as beforefieldinit.
            /// </remarks>
            static Nested()
            {
                Current = new iSeriesConfiguration();
                Current.Initialize();
            }

            /// <summary>
            /// Current singleton instance.
            /// </summary>
            internal static readonly iSeriesConfiguration Current;
        }

        #endregion
    }
}
