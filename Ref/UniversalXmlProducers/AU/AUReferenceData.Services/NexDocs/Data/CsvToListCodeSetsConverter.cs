using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace CargoWise.RefDbRepo.AUReferenceData.Services
{
	public class CsvToListCodeSetsConverter<T>
		where T : CsvToItemCodeSetsConverter
	{
		public IEnumerable<IListCodeSet> Convert(CsvReader csvReader)
		{
			ErrorBuilder.Clear();
			var configuration = csvReader.Configuration;
			configuration.PrepareHeaderForMatch = (string header, int index) => header.ToUpperInvariant();
			configuration.TypeConverterCache.AddConverter<DateTime>(new MinValueDateTimeConverter());
			configuration.BadDataFound = (x) =>
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Bad Data Found (Row: {x.Row}, CurrentIndex:{x.CurrentIndex}, RawRecord:{x.RawRecord.TrimEnd()})");
			};
			configuration.MissingFieldFound = (x, y, z) =>
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Missing Field Found (Row: {z.Row}, Index:{y}{(x == null ? "" : ", Data:" + string.Join("|", x))}, RawRecord:{z.RawRecord.TrimEnd()})");
			};
			configuration.HeaderValidated = (v, x, y, z) =>
			{
				if (!v)
				{
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Header Validation Error (Index:{y}{(x == null ? "" : ", Data:" + string.Join("|", x))}, RawRecord:{z.RawRecord.TrimEnd()})");
				}
			};
			var position = 1;
			while (csvReader.Read())
			{
				T record = null;
				try
				{
					record = csvReader.GetRecord<T>();
				}
				catch (CsvHelperException e)
				{
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Error Parsing Row (Row:{e.ReadingContext.Row}, CurrentIndex:{e.ReadingContext.CurrentIndex}, RawRecord:{e.ReadingContext.RawRecord.TrimEnd()}):\r\n{GetCompleteExceptionMessage(e)}");
				}
				if (record != null)
				{
					yield return new ListCodeSet()
					{
						Position = position,
						Items = record.Items.ToArray()
					};
				}
				position++;
			}
		}

		static string GetCompleteExceptionMessage(Exception e)
		{
			var message = new StringBuilder();
			var ex = e;
			while (ex != null)
			{
				message.AppendLine(ex.Message);
				ex = ex.InnerException;
			}
			return message.ToString();
		}

		public string ErrorNotification => ErrorBuilder.ToString();

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;

		class MinValueDateTimeConverter : DateTimeConverter
		{
			public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
			{
				if (string.IsNullOrWhiteSpace(text))
				{
					return DateTime.MinValue;
				}
				try
				{
					return base.ConvertFromString(text, row, memberMapData);
				}
				catch (Exception e)
				{
					throw new CsvHelperException(row.Context, $"Error converting DateTime for {memberMapData.Member.Name}: Text:'{text}'", e);
				}
			}
		}

		class ListCodeSet : IListCodeSet
		{
			public int Position { get; set; }
			public IItemCodeSet[] Items { get; set; }
		}
	}
}
