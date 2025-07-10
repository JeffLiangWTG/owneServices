using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.TNT
{
	class FieldPopulator
	{
		internal FieldPopulator(object dataObject)
		{
			this.dataObject = dataObject;
		}
		readonly object dataObject;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:Do not raise reserved exception types")]
		internal void PopulateFields(string rowData)
		{
			var chars = rowData.ToCharArray();
			int index = 0;

			foreach (var fieldAttribute in FieldAttributes)
			{
				try
				{
					int fieldLength = fieldAttribute.Length + index > chars.Length ? chars.Length - index : fieldAttribute.Length;
					var fieldValue = new string(chars, index, fieldLength);
					index += fieldLength;

					Type destinationType = fieldAttribute.DataPropertyInfo.PropertyType;
					if (destinationType == typeof(string))
					{
						fieldAttribute.DataPropertyInfo.SetValue(dataObject, fieldValue.Trim(), null);
					}
					else if (destinationType == typeof(int))
					{
						int fieldValueAsInt;
						if (int.TryParse(fieldValue, out fieldValueAsInt))
						{
							fieldAttribute.DataPropertyInfo.SetValue(dataObject, fieldValueAsInt, null);
						}
					}
					else if (destinationType == typeof(decimal))
					{
						var fieldDecimalDigits = new string(chars, index, fieldAttribute.Decimals);
						index += fieldAttribute.Decimals;

						decimal fieldValueAsDecimal;
						if (decimal.TryParse(fieldValue + "." + fieldDecimalDigits, out fieldValueAsDecimal))
						{
							fieldAttribute.DataPropertyInfo.SetValue(dataObject, fieldValueAsDecimal, null);
						}
					}
					else
					{
						throw new InvalidOperationException("Tried to parse out an unhandled field Type: " + destinationType.Name);
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					StringBuilder errorBuilder = new StringBuilder();
					errorBuilder.AppendFormat((NoResString)"Unable to parse field: {0}", fieldAttribute.DataPropertyInfo.Name); // Exception Only
					errorBuilder.AppendLine();
					errorBuilder.AppendLine(e.Message);
					errorBuilder.AppendLine((NoResString)"Row data parsed was:"); // Exception Only
					errorBuilder.AppendLine(rowData);

					throw new Exception(errorBuilder.ToString(), e);
				}
			}
		}

		List<FieldDefinitionAttribute> FieldAttributes
		{
			get { return GetFieldAttributes(dataObject.GetType()); }
		}

		static List<FieldDefinitionAttribute> GetFieldAttributes(Type dataStructureType)
		{
			var fieldAttributes = new List<FieldDefinitionAttribute>();
			foreach (PropertyInfo info in dataStructureType.GetProperties())
			{
				FieldDefinitionAttribute fieldAttribute = Attribute.GetCustomAttribute(info, typeof(FieldDefinitionAttribute)) as FieldDefinitionAttribute;
				if (fieldAttribute != null)
				{
					fieldAttribute.DataPropertyInfo = info;
					fieldAttributes.Add(fieldAttribute);
				}
			}
			fieldAttributes.Sort(delegate(FieldDefinitionAttribute x, FieldDefinitionAttribute y)
			{ return x.Order.CompareTo(y.Order); });

			return fieldAttributes;
		}
	}
}
