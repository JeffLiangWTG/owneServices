using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public abstract class CsvToItemCodeSetsConverter
	{
		public IEnumerable<IItemCodeSet> Items
		{
			get
			{
				var type = GetType();
				foreach (var propertyInfo in type.GetProperties())
				{
					var attribute = (CodeSetNameAttribute)propertyInfo.GetCustomAttributes(typeof(CodeSetNameAttribute), true).FirstOrDefault();
					if (attribute != null)
					{
						yield return new ItemCodeSet()
						{
							Key = attribute.Names.First().ToUpperInvariant(),
							Value = GetStringValue(propertyInfo.GetValue(this), attribute.ValueType),
							ValueType = attribute.ValueType
						};
					}
				}
			}
		}
		const string DateFormat = "yyyy-MM-ddTHH:mm:ss.fff";

		static string GetStringValue(object value, CodeSetValueType valueType)
		{
			if (valueType == CodeSetValueType.dateTime && value is DateTime dateValue)
			{
				return dateValue.ToString(DateFormat, null);
			}
			else
			{
				return value == null ? string.Empty : value.ToString();
			}
		}

		class ItemCodeSet : IItemCodeSet
		{
			public string Key { get; set; }
			public string Value { get; set; }
			public CodeSetValueType ValueType { get; set; }
		}
	}
}
