﻿//------------------------------------------------------------------------------
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
using Generator.Microsoft.Frameworks.Utility;
using LinqToEdmx.Model.Conceptual;

namespace CodeSmith.SchemaHelper
{
    /// <summary>
    /// 
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("FunctionEntity = {Name}, Key = {EntityKeyName}")]
    public sealed class FunctionEntity : EntityBase<EntityContainer.FunctionImportLocalType>
    {
        #region Constructor(s)

        /// <summary>
        /// Constructor that passes in the entity that this class will represent.
        /// </summary>
        public FunctionEntity(EntityContainer.FunctionImportLocalType entity, string @namespace) : base(entity, @namespace)
        {
            EntityKeyName = EntitySource.Name;
            Name = EntitySource.Name;
            TypeAccess = !String.IsNullOrEmpty(EntitySource.MethodAccess) ? EntitySource.MethodAccess : AccessibilityConstants.Public;

            LoadProperties();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize()
        {
            if (EntitySource == null) 
                return;

            //#region Entity Function Imports
            //  <FunctionImport Name="GetCategoryById" EntitySet="Categories" ReturnType="Collection(PetShopModel.Category)">
            //#region Scalar Function Imports
            //  <FunctionImport Name="GetCategoryById" ReturnType="Collection(Int32)">
            //#region Complex Type Function Imports
            //  <FunctionImport Name="GetCategoryById" ReturnType="Collection(PetShopModel.GetCategoryById_Result1)">
            //#region No Return Type Function Imports
            //  <FunctionImportMapping FunctionImportName="GetCategoryById" FunctionName="PetShopModel.Store.GetCategoryById" >
            //<Function Name="usp_test" p5:Name="usp_test" Aggregate="false" BuiltIn="false" NiladicFunction="false" IsComposable="false" ParameterTypeSemantics="AllowImplicitConversion" Schema="dbo" p5:Schema="dbo" xmlns:p5="http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator">
            //  <Parameter Name="UserID" Mode="In" Type="int" />
            //  <Parameter Name="FromDate" Mode="In" Type="datetime" />
            //  <Parameter Name="ToDate" Mode="In" Type="datetime" />
            //</Function>
            if (EntitySource.ReturnType != null)
            {
                var returnType = EntitySource.ReturnType.ToString().Replace("Collection(", String.Empty).Replace(Namespace + ".", String.Empty).Replace(")", String.Empty).Trim();
                if (EntitySource.ReturnType.ToString().Trim().StartsWith("Collection(" + Namespace))
                {
                    AssociatedEntity = EntityStore.Instance.GetEntity(returnType);
                }
                else if (!String.IsNullOrEmpty(returnType) && SystemTypeMapper.EfConceptualTypeToSystemType.ContainsKey(returnType))
                {
                    ReturnType = String.Concat(SystemTypeMapper.EfConceptualTypeToSystemType[returnType, "System.Int32"], "?");
                }
            }

            if(String.IsNullOrEmpty(ReturnType))
                ReturnType = "System.Int32";

            LoadSearchCriteria();
        }

        /// <summary>
        /// Override to populate the properties from the implemented entity.
        /// </summary>
        protected override void LoadProperties() {
        }

        /// <summary>
        /// Override to populate the associations from the implemented entity.
        /// </summary>
        protected override void LoadAssociations()
        {
        }

        /// <summary>
        /// Override to populate the keys from the implemented entity.
        /// </summary>
        protected override void LoadKeys()
        {
        }

        /// <summary>
        /// Load the extended properties for the entity.
        /// </summary>
        protected override void LoadExtendedProperties()
        {
        }

        protected override void PopulateInheritanceProperties()
        {
        }

        /// <summary>
        /// Load the Search Criteria for the entity
        /// </summary>
        protected override void LoadSearchCriteria()
        {
            if (EntitySource != null && EntitySource.Parameters.Count > 0)
            {
                var searchCriteria = new SearchCriteria(SearchCriteriaType.Command);

                foreach (FunctionImportParameter parameter in EntitySource.Parameters)
                {
                    if (parameter != null)
                        searchCriteria.Properties.Add(new FunctionParameter(parameter, this));
                }

                if (!String.IsNullOrEmpty(searchCriteria.Key) && searchCriteria.Properties.Count > 0)
                    SearchCriteria.Add(searchCriteria);
            }
        }

        #endregion

        /// <summary>
        /// The associated Entity for this command. This is usually a Table or a View.
        /// This comes from the name of the Custom command.
        /// </summary>
        public IEntity AssociatedEntity { get; set; }

        /// <summary>
        /// Is this Command associated to an Entity
        /// </summary>
        public bool IsAssociated
        {
            get { return AssociatedEntity != null; }
        }

        public string ReturnType { get; private set; }
    }
}
