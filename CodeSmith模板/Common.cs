using System;
using SchemaExplorer;
using System.Data;
using CodeSmith.Engine;

public class Common:CodeTemplate
{
	/// <summary>
	/// 数据库类型转换成C#类型
	/// </summary>
	/// <param name="column">列明</param>
	/// <returns></returns>
	public string GetCSharpTypeFromDBFieldType(ColumnSchema column)
	{
		if (column.Name.EndsWith("TypeCode")) return column.Name;
		string type;
		switch (column.DataType)
		{
			case DbType.AnsiString: type= "string";break;
			case DbType.AnsiStringFixedLength: type= "string";break;
			case DbType.Binary: type= "byte[]";break;
			case DbType.Boolean: type= "bool";break;
			case DbType.Byte: type= "byte";break;
			case DbType.Currency: type= "decimal";break;
			case DbType.Date: type= "DateTime";break;
			case DbType.DateTime: type= "DateTime";break;
			case DbType.Decimal: type= "decimal";break;
			case DbType.Double: type= "double";break;
			case DbType.Guid: type= "Guid";break;
			case DbType.Int16: type= "short";break;
			case DbType.Int32: type= "int";break;
			case DbType.Int64: type= "long";break;
			case DbType.Object: type= "object";break;
			case DbType.SByte: type= "sbyte";break;
			case DbType.Single: type= "float";break;
			case DbType.String: type= "string";break;
			case DbType.StringFixedLength: type= "string";break;
			case DbType.Time: type= "TimeSpan";break;
			case DbType.UInt16: type= "ushort";break;
			case DbType.UInt32: type= "uint";break;
			case DbType.UInt64: type= "ulong";break;
			case DbType.VarNumeric: type= "decimal";break;
			default:
			{
				type= "__UNKNOWN__" + column.NativeType;
				break;
			}
		}
		if(column.AllowDBNull&&column.SystemType.IsValueType)
		{
			type=type+"?";
		}
		return type;
	}
	
    
/// <summary>
/// 获取新的TableName(TORDER_RECEIPT_REQUEST   首字母大写，去掉下划线，去掉首字母T)
/// </summary>
/// <param name="name"></param>
/// <returns></returns>
public string GetNewTableName(string name)
{
      string table=name.Substring(1).ToLower();
      string tempTableName=string.Empty;
      if(table.IndexOf('_')>0)
      {          
            string[] temp=table.Split('_');
            for (int i = 0; i < temp.Length; i++)
			{               
			   tempTableName+=System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(temp[i]); //设置首字母大写
			}   
      }
      else
      {
          tempTableName=System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(table); //设置首字母大写          
      }
    return tempTableName;
}


/// <summary>
/// 获取新的ColoumName(ORDER_RECEIPT_REQUEST   首字母大写，去掉下划线)
/// </summary>
/// <param name="name"></param>
/// <returns></returns>
public string GetNewColoumName(string name)
{
      string table=name.ToLower();
      string tempTableName=string.Empty;
      if(table.IndexOf('_')>0)
      {          
            string[] temp=table.Split('_');
            for (int i = 0; i < temp.Length; i++)
			{               
			   tempTableName+=System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(temp[i]); //设置首字母大写
			}   
      }
      else
      {
          tempTableName=System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(table); //设置首字母大写          
      }
    return tempTableName;
}
    
    /// <summary>
    /// 表头模板
    /// </summary>
    /// <param name="name"></param>
	public void PrintHeader(string name)
	{
		Response.WriteLine("//============================================================");
        Response.WriteLine("///Create By:"+name);     
//		Response.WriteLine("///Create Date:" +System.DateTime.Now.ToString());
		Response.WriteLine("//============================================================");
		Response.WriteLine();
	}
}