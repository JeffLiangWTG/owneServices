using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Staging.Common;
using ZAReferenceData.Services;

namespace ZAReferenceData.Services
{
	public class CSVParser : ICSVParser
	{
		public List<T> Parse<T>(string filePath) where T : new()
		{
			var result = new List<T>();
			Stream data = File.OpenRead(filePath);

			if (!File.Exists(filePath))
			{
				return result;
			}
			var csvContent = CSVHelper.GetCSVLinesInArrays(true, data);

			var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

			foreach (var row in csvContent)
			{
				var item = new T();
				var colCount = row.Length;
				var hasBlankCell = false;

				for (var col = 0; col < colCount; col++)
				{
					var header = col == 0 ? "Code" : "Description";
					var property = properties.FirstOrDefault(x => x.Name == header);

					if (property != null)
					{
						var value = row[col];

						if (string.IsNullOrEmpty(value))
						{
							hasBlankCell = true;
						}
						try
						{
							var convertedValue = Convert(value, property.PropertyType);
							property.SetValue(item, convertedValue);
						}
						catch (Exception e)
						{
							ServicesProvider.Logger.Warn(e.Message);
						}
					}
				}

				if (!hasBlankCell)
				{
					result.Add(item);
				}
			}
			return result;
		}

		object Convert(string source, Type targetType)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source), "The input source should not be null.");
			}

			if (targetType == null)
			{
				throw new ArgumentNullException(nameof(targetType), "The input target type should not be null.");
			}

			var typeConverter = TypeDescriptor.GetConverter(targetType);

			try
			{
				return typeConverter.ConvertFromString(null, new CultureInfo("en-US"), source);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"The conversion from {source} to {targetType} cannot performed, see the inner exception for details.", e);
			}
		}
	}
}
