using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.USReferenceData.Business.ExchangeRate
{
	public static class ExcelParser
	{
		public static List<ExchangeRateSchema> Parse(Stream stream, ExcelParserConfiguration config, DateTime today, out DateTime? publishDate)
		{
			var result = new List<ExchangeRateSchema>();

			var xls = new XlsFile(stream, false)
			{
				ActiveSheet = config.SheetIndex
			};

			var rowCount = xls.GetRowCount(xls.ActiveSheet);
			rowCount = Math.Min(rowCount, config.LastRow);

			publishDate = GetPublishDate(xls, config, today);

			(PropertyInfo Info, IndexAttribute Attribute)[] propertyWithAttributes = typeof(ExchangeRateSchema)
				.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty)
				.Select(c => (c, c.GetCustomAttribute<IndexAttribute>()))
				.Where(c => c.Item2 != null)
				.ToArray();

			for (var row = config.StartingRow; row <= rowCount; row++)
			{
				var item = new ExchangeRateSchema();

				var isBlankRow = true;

				foreach (var propertyWithAttribute in propertyWithAttributes)
				{
					var columnIndex = propertyWithAttribute.Attribute.ColumnIndex;
					var cell = xls.GetStringFromCell(row, columnIndex)?.Trim();

					if (!string.IsNullOrEmpty(cell))
					{
						isBlankRow = false;
					}

					try
					{
						var info = propertyWithAttribute.Info;

						var value = Convert(cell, info.PropertyType);
						info.SetValue(item, value);
					}
					catch (Exception ex)
					{
						Console.Error.WriteLine($"Set {propertyWithAttribute.Attribute.Header} failed: {ex.Message}");
					}
				}

				if (!isBlankRow)
				{
					result.Add(item);
				}
			}

			return result;
		}

		static DateTime? GetPublishDate(XlsFile xlsFile, ExcelParserConfiguration config, DateTime today)
		{
			var publishDate = xlsFile.GetStringFromCell(config.PublishDateRowIndex, config.PublishDateColumnIndex);
			var value = publishDate?.Value?.Trim() ?? string.Empty;

			var publishDates = GetPublishDates(value);
			return new[] { publishDates.date1, publishDates.date2 }.Where(x => x.HasValue).OrderBy(x => Math.Abs(today.Subtract(x.Value).Days)).FirstOrDefault();
		}

		static (DateTime? date1, DateTime? date2) GetPublishDates(string publishDate)
		{
			DateTime? parsedDate1 = null;
			DateTime? parsedDate2 = null;

			var parseFormats = new string[] { "M-d-yyyy", "M/d/yyyy" };

			if (DateTime.TryParseExact(publishDate, parseFormats, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out var parsedDate))
			{
				parsedDate1 = parsedDate;
			}
			if (DateTime.TryParse(publishDate, out parsedDate))
			{
				parsedDate2 = parsedDate;
			}

			return (parsedDate1, parsedDate2);
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
