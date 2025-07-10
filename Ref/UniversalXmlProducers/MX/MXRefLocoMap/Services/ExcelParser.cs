using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.MXRefLocoMap.Business
{
	public class ExcelParser : IExcelParser
	{
		public List<T> Parse<T>(string filePath, ExcelParserConfiguration config = null, BackupMapper backupMapper = null) where T : new()
		{
			var result = new List<T>();

			if (!File.Exists(filePath))
			{
				return result;
			}

			config = config ?? new ExcelParserConfiguration();

			var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

			var xls = new XlsFile(filePath, false)
			{
				ActiveSheet = config.SheetIndex
			};
			var rowCount = xls.GetRowCount(xls.ActiveSheet);
			rowCount = Math.Min(rowCount, config.LastRow);

			for (var row = config.StartingRow; row <= rowCount; row++)
			{
				var item = new T();
				var colCount = xls.ColCountInRow(row);
				var isBlankRow = true;

				for (var col = 1; col <= colCount; col++)
				{
					var header = xls.GetStringFromCell(config.HeaderRow, col);
					var property =
						properties.FirstOrDefault(x => x.Name == header.Value)
						?? properties.FirstOrDefault(x => backupMapper != null && backupMapper.Invoke(header.Value, x.Name));
					if (property != null)
					{
						var cell = xls.GetStringFromCell(row, col);

						if (!string.IsNullOrEmpty(cell))
						{
							isBlankRow = false;
						}

						try
						{
							var value = Convert(cell, property.PropertyType);
							property.SetValue(item, value);
						}
						catch (Exception e)
						{
							ServicesProvider.Logger.Warn(e.Message);
							throw;
						}
					}
				}

				if (!isBlankRow)
				{
					result.Add(item);
				}
			}

			return result;
		}

		static object Convert(string source, Type targetType)
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
