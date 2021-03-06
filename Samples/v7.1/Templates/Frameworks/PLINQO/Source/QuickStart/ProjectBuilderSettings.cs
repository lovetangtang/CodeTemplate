﻿using System;
using System.IO;
using CodeSmith.Engine;
using SchemaExplorer;

namespace QuickStartUtils {
    public class ProjectBuilderSettings {
        public ProjectBuilderSettings() {
            QueryPattern = QueryPatternEnum.ManagerClasses;
        }

        public DatabaseSchema SourceDatabase { get; set; }

        public string Location { get; set; }
        public string WorkingDirectory { get; set; }

        public string SolutionName { get; set; }
        public string DataProjectName { get; set; }
        public string InterfaceProjectName { get; set; }
        public string TestProjectName { get; set; }

        public LanguageEnum Language { get; set; }
        public ProjectTypeEnum ProjectType { get; set; }

        public bool IncludeDataServices { get; set; }
        public bool IncludeTestProject { get; set; }
        public bool CopyTemplatesToFolder { get; set; }

        public string WebSkin { get; set; }
        public QueryPatternEnum QueryPattern { get; set; }
        public FrameworkVersion FrameworkVersion { get; set; }

        public string FrameworkFolder {
            get {
                switch (FrameworkVersion) {
                    case FrameworkVersion.v35:
                        return "v3.5";
                    case FrameworkVersion.v40:
                        return "v4.0";
                    case FrameworkVersion.v45:
                        return "v4.5";
                    default:
                        throw new InvalidOperationException("Invalid FrameworkVersion.");
                }
            }
        }

        public string FrameworkString {
            get {
                switch (FrameworkVersion) {
                    case FrameworkVersion.v35:
                        return "3.5";
                    case FrameworkVersion.v40:
                        return "4.0";
                    case FrameworkVersion.v45:
                        return "4.5";
                    default:
                        throw new InvalidOperationException("Invalid FrameworkVersion.");
                }
            }
        }

        public string LanguageFolder {
            get { return (Language == LanguageEnum.CSharp) ? "CSharp" : "VisualBasic"; }
        }

        public string LanguageAppendage {
            get { return (Language == LanguageEnum.CSharp) ? "cs" : "vb"; }
        }

        private string _zipFileFolder;

        public string ZipFileFolder {
            get {
                if (string.IsNullOrEmpty(_zipFileFolder)) {
                    _zipFileFolder = Path.Combine(WorkingDirectory, "Common");
                    _zipFileFolder = Path.Combine(_zipFileFolder, FrameworkFolder);
                    _zipFileFolder = Path.Combine(_zipFileFolder, LanguageFolder);
                }

                return _zipFileFolder;
            }
        }

        private string _databaseName;

        public string DatabaseName {
            get {
                if (string.IsNullOrEmpty(_databaseName))
                    _databaseName = StringUtil.ToPascalCase(SourceDatabase.Database.Name);

                return _databaseName;
            }
        }

        private string _dataContextName;

        public string DataContextName {
            get {
                if (string.IsNullOrEmpty(_dataContextName))
                    _dataContextName = DatabaseName + "DataContext";

                return _dataContextName;
            }
            set { _dataContextName = value; }
        }
    }
}