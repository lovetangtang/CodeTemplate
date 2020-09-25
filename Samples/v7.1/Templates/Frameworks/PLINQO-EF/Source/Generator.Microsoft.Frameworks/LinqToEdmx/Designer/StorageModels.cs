using System;
using System.Collections.Generic;
using System.Xml.Linq;
using LinqToEdmx.Model.Storage;
using Xml.Schema.Linq;

namespace LinqToEdmx.Designer
{
  /// <summary>
  /// <para>
  /// Regular expression: (Schema?)
  /// </para>
  /// </summary>
  public class StorageModels : XTypedElement, IXMetaData
  {
    private static readonly Dictionary<XName, Type> LocalElementDictionary = new Dictionary<XName, Type>();

    private static readonly ContentModelEntity ContentModel;

    static StorageModels()
    {
      BuildElementDictionary();
      ContentModel = new SequenceContentModelEntity(new NamedContentModelEntity(XName.Get("Schema", XMLNamespaceFactory.SSDL)));
    }

    /// <summary>
    /// <para>
    /// Occurrence: optional
    /// </para>
    /// <para>
    /// Regular expression: (Schema?)
    /// </para>
    /// </summary>
    public StorageSchema StorageSchema
    {
      get
      {
        return ((StorageSchema) GetElement(XName.Get("Schema", XMLNamespaceFactory.SSDL)));
      }
      set
      {
        SetElement(XName.Get("Schema", XMLNamespaceFactory.SSDL), value);
      }
    }

    #region IXMetaData Members

    Dictionary<XName, Type> IXMetaData.LocalElementsDictionary
    {
      get
      {
        return LocalElementDictionary;
      }
    }

    XName IXMetaData.SchemaName
    {
      get
      {
        return XName.Get("TRuntimeStorageModels", XMLNamespaceFactory.Edmx);
      }
    }

    SchemaOrigin IXMetaData.TypeOrigin
    {
      get
      {
        return SchemaOrigin.Fragment;
      }
    }

    ILinqToXsdTypeManager IXMetaData.TypeManager
    {
      get
      {
        return LinqToXsdTypeManager.Instance;
      }
    }

    ContentModelEntity IXMetaData.GetContentModel()
    {
      return ContentModel;
    }

    #endregion

    public static explicit operator StorageModels(XElement xe)
    {
      return XTypedServices.ToXTypedElement<StorageModels>(xe, LinqToXsdTypeManager.Instance);
    }

    public override XTypedElement Clone()
    {
      return XTypedServices.CloneXTypedElement(this);
    }

    private static void BuildElementDictionary()
    {
      LocalElementDictionary.Add(XName.Get("Schema", XMLNamespaceFactory.SSDL), typeof (StorageSchema));
    }
  }
}