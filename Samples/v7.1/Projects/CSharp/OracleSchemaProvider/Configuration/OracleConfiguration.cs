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
using System.ComponentModel;
using System.Xml.Serialization;

using CodeSmith.Engine;

namespace SchemaExplorer
{
    [XmlRoot("oracleSchemaProvider")]
    public class OracleConfiguration : CachedConfiguration
    {
        internal OracleConfiguration() {}

        #region Properties

        /// <summary>
        /// If true, allows the discovery of CommandResultSchema's. Warning This may modify data in your database.
        /// </summary>
        [XmlElement("allowGetCommandResultSchemas")]
        [DefaultValue(false)]
        [Description("If true, allows the discovery of CommandResultSchema's. Warning This may modify data in your database.")]
        public bool AllowGetCommandResultSchemas
        {
            get { return GetCachedValue("AllowGetCommandResultSchemas", false); }
            set { SetValue("AllowGetCommandResultSchemas", value); }
        }

        /// <summary>
        /// If true, this will only show the schema for the current connected user.
        /// </summary>
        [XmlElement("showMySchemaOnly")]
        [DefaultValue(true)]
        [Description("If true, only schema's for the current logged in user will be shown.")]
        public bool ShowMySchemaOnly
        {
            get { return GetCachedValue("ShowMySchemaOnly", true); }
            set { SetValue("ShowMySchemaOnly", value); }
        }

        /// <summary>
        /// If true, the Extended Properties table will be created if it does not exist.
        /// </summary>
        [XmlElement("autoCreateExtendedPropertiesTable")]
        [DefaultValue(true)]
        [Description("Set this to true if you want the Extended Properties Table to automatically created.")]
        public bool AutoCreateExtendedPropertiesTable
        {
            get { return GetCachedValue("autoCreateExtendedPropertiesTable", true); }
            set { SetValue("autoCreateExtendedPropertiesTable", value); }
        }

        /// <summary>
        /// The owner of the extended properties table (e.g. HR).
        /// </summary>
        [XmlElement("extendedPropertiesTableOwner")]
        [DefaultValue("")]
        [Description("This is the schema in which the Extended Properties Table is located on the Oracle server.")]
        public string ExtendedPropertiesTableSchema
        {
            get { return GetCachedValue("ExtendedPropertiesTableOwner", ""); }
            set
            {
                if (value != null)
                {
                    SetValue("ExtendedPropertiesTableOwner", value);
                }
            }
        }

        #endregion

        #region Overrides of ConfigurationBase

        protected override string NamePrefix
        {
            get { return "SchemaExplorer\\OracleSchemaProvider"; }
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
            ExtendedPropertiesTableSchema = String.Empty;
            ShowMySchemaOnly = true;
        }

        public override void ResetDefaults() {}

        public override void Upgrade()
        {
            UpgradeFile<OracleConfiguration>("OracleSchemaProvider.config");
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
        public static OracleConfiguration Instance
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
                Current = new OracleConfiguration();
                Current.Initialize();
            }

            /// <summary>
            /// Current singleton instance.
            /// </summary>
            internal static readonly OracleConfiguration Current;
        }

        #endregion
    }
}