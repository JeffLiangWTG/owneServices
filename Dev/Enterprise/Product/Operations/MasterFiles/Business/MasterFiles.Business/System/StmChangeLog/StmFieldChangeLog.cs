using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class StmFieldChangeLog : NonPersistentBusinessObject, IObsoleteValidation
	{
		public StmFieldChangeLog(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Parse / Format

		public void Format(StringBuilder builder)
		{
			builder.Append(PropertyName);
			builder.Append("|");
			builder.Append(SerializeAndEscapeValue(OldValue));
			builder.Append("|");
			builder.Append(SerializeAndEscapeValue(NewValue));
		}

		public void Parse(string str)
		{
			string[] parts = ParseParts(str);
			if (parts.Length != 3)
			{
				throw new FormatException("Expected 3 values delimited by a '|'");
			}
			PropertyName = parts[0];
			OldValue = DeserializeValue(PropertyName, parts[1]);
			NewValue = DeserializeValue(PropertyName, parts[2]);
		}

		string[] ParseParts(string str)
		{
			List<string> result = new List<string>(3);
			int i = -1;
			while (i < str.Length)
			{
				i++;
				result.Add(ParseOnePart(str, ref i));
			}
			return result.ToArray();
		}

		string ParseOnePart(string str, ref int i)
		{
			int pipeIndex = i - 1;
			do
			{
				pipeIndex = str.IndexOf("|", pipeIndex + 1);
			}
			while (pipeIndex != -1 && pipeIndex != 0 && str[pipeIndex - 1] == '\\');

			string result = pipeIndex == -1 ? str.Substring(i) : str.Substring(i, pipeIndex - i);
			i += result.Length;
			return result.Replace("\\|", "|").Replace("\\r", "\r").Replace("\\n", "\n");
		}

		#endregion

		#region SerializeValue / DeserializeValue

		string SerializeAndEscapeValue(IZType value)
		{
			string serializedValue = SerializeValue(value);
			return serializedValue.Replace("|", "\\|").Replace("\r", "\\r").Replace("\n", "\\n");
		}

		string SerializeValue(IZType value)
		{
			ZString valueAsString = ZString.Empty;
			if (value != null)
			{
				TypeConverter converter = TypeDescriptor.GetConverter(value.GetType());
				valueAsString = (string)converter.ConvertTo(null, CultureInfo.InvariantCulture, value, typeof(string));
			}
			return valueAsString;
		}

		IZType DeserializeValue(string propertyName, string str)
		{
			ITableSchema tableSchema = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(Schema.GetPrefixFromColumnName(propertyName));
			SchemaColumn column = (tableSchema == null ? null : tableSchema.GetSchemaColumn(propertyName))
				?? throw new FormatException("Could not find database column '" + propertyName + "'");
			return ZDataType.InvariantStringToZType(column.GetEquivalentZType(), str);
		}

		#endregion

		#region PropertyName

		[CargoWise.ComponentModel.MaxLength(100)]
		public ZString PropertyName
		{
			get { return propertyName; }
			set
			{
				CheckMaximumLength(PropertyNameInfo, value);
				propertyName = value;
				PropertyNameInfo.RefreshBinding();
			}
		}
		ZString propertyName;

		public ZPropertyInfo PropertyNameInfo
		{
			get { return GetZPropertyInfo(nameof(PropertyName)); }
		}

		#endregion

		#region OldValue / NewValue

		public IZType OldValue
		{
			get { return oldValue; }
			set { oldValue = value; }
		}
		IZType oldValue;

		public IZType NewValue
		{
			get { return newValue; }
			set { newValue = value; }
		}
		IZType newValue;

		#endregion
	}
}
