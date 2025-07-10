using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public class LineToEntityConverter<T>
		where T : RefDataRepoModelEntityType, new()
	{
		readonly Dictionary<string, Func<string, object>> _propertyConverterCache = new Dictionary<string, Func<string, object>>();

		public LineToEntityConverter(IEnumerable<PropertyMapping<T>> mappings, int leadingPosition = 0)
		{
			Mappings = mappings;
			LeadingPosition = leadingPosition;
		}

		public int LeadingPosition { get; }

		public IEnumerable<PropertyMapping<T>> Mappings { get; }

		public DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);

		public DateTime MinimumDateTime => new DateTime(1900, 01, 01, 00, 00, 00);

		public T Convert(string line)
		{
			var entity = new T();
			foreach (var mapping in Mappings)
			{
				var propertyString = string.Empty;
				var startingIndex = mapping.StartPosition - LeadingPosition;
				if (mapping.Length > 0)
				{
					if (line.Length >= startingIndex + mapping.Length)
					{
						propertyString = line.Substring(startingIndex, mapping.Length);
					}
					else if (line.Length > startingIndex)
					{
						propertyString = line.Substring(startingIndex, line.Length - startingIndex);
					}
				}
				else
				{
					propertyString = line.Substring(startingIndex, line.Length - startingIndex);
				}
				propertyString = KeepChars(propertyString.Trim(), CharactersToKeep, "");

				var propertyValue = mapping.DefaultValue;
				if (!string.IsNullOrWhiteSpace(propertyString))
				{
					var typeConverter = GetTypeConverter(mapping.PropertyInfo);
					propertyValue = typeConverter(propertyString);
				}
				mapping.PropertyInfo.SetValue(entity, propertyValue);
			}
			return entity;
		}

		Func<string, object> GetTypeConverter(PropertyInfo propertyInfo)
		{
			var propertyName = propertyInfo.Name;
			if (!_propertyConverterCache.ContainsKey(propertyName))
			{
				var propertyType = propertyInfo.PropertyType;
				if (propertyType == typeof(DateTime))
				{
					_propertyConverterCache[propertyName] = s =>
					{
						DateTime parsedDate;
						if (s.Length == 8)
						{
							parsedDate = DateTime.ParseExact(s, "yyyyMMdd", CultureInfo.InvariantCulture);
						}
						else
						{
							parsedDate = DateTime.ParseExact(s, "yyyyMMddHHmm", CultureInfo.InvariantCulture);
						}
						parsedDate = DateTime.Compare(parsedDate, MaximumDateTime) > 0 ? MaximumDateTime : parsedDate;
						return DateTime.Compare(parsedDate, MinimumDateTime) < 0 ? MinimumDateTime : parsedDate;
					};
				}
				else
				{
					_propertyConverterCache[propertyName] = TypeDescriptor.GetConverter(propertyType).ConvertFromString;
				}
			}
			return _propertyConverterCache[propertyName];
		}

		protected string KeepChars(string originalString, string charactersToKeep, string replacementString)
		{
			StringBuilder stringBuilder = new StringBuilder(originalString.Length);
			foreach (char value in originalString)
			{
				if (charactersToKeep.Contains(value))
				{
					stringBuilder.Append(value);
				}
				else
				{
					stringBuilder.Append(replacementString);
				}
			}

			return stringBuilder.ToString();
		}

		public const string CharactersToKeep = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz 0123456798.<>()*&^%$#@![]\|{}+=-_\/?,~`:;'";
	}
}
