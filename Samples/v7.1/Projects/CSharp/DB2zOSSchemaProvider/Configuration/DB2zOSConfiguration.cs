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
using System.ComponentModel;
using System.Xml.Serialization;
using CodeSmith.Engine;

namespace SchemaExplorer
{
    class DB2zOSConfiguration : CachedConfiguration
    {
        #region Properties

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

        [DefaultValue("")]
        [Description("This is the schema in which the Extended Properties Table is located on the DB2 zOS server.")]
        public string ExtendedPropertiesTableSchema {
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

        [DefaultValue("")]
        [Description("The commands that CodeSmith Generator will analyze. If this is left blank then all views on the server, or in the filtered schema will be analyzed.  You can put multiples in separated by a comma.  Use % for wildcard.  _ stands for any single character.  For instance IIM_%.")]
        public string FilterCommands {
            get { return GetCachedValue("FilterCommands", "").ToUpper(); }
            set { SetValue("FilterCommands", value.ToUpper()); }
        }

        [DefaultValue(false)]
        [Description("If true then an indexed column will be defined as a primary key, if no primary key exists.")]
        public bool UseUniqueIndexWhenNoPrimaryKey {
            get { return GetCachedValue("UseUniqueIndexWhenNoPrimaryKey", false); }
            set { SetValue("UseUniqueIndexWhenNoPrimaryKey", value); }
        }

        #endregion

        #region Overrides of ConfigurationBase

        protected override string NamePrefix
        {
            get { return "SchemaExplorer\\DB2zOSSchemaProvider"; }
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
            UseUniqueIndexWhenNoPrimaryKey = false;
            ExtendedPropertiesTableSchema = "";
            FilterSchema = "";
            FilterTables = "";
            FilterViews = "";
        }

        public override void ResetDefaults()
        {
        }

        public override void Upgrade() {
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
        public static DB2zOSConfiguration Instance
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
                Current = new DB2zOSConfiguration();
                Current.Initialize();
            }

            /// <summary>
            /// Current singleton instance.
            /// </summary>
            internal static readonly DB2zOSConfiguration Current;
        }

        #endregion
    }
}
