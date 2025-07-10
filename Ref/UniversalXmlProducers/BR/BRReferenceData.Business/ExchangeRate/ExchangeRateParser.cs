using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class ExchangeRateParser : BaseParser
	{
		public ExchangeRateParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(Stream inputStream, string outputFileName, string rateType, DateTime publicationTime)
		{
			Argument.NotNull(inputStream, nameof(inputStream));
			Argument.NotNullOrEmpty(outputFileName, nameof(outputFileName));
			Argument.NotNullOrEmpty(rateType, nameof(rateType));

			var refExchangeRateZZList = GetRefExchangeRateZZList(inputStream, rateType);
			var writer = Helper.GetRefExchangeRateWriterConfiguration(rateType);
			Helper.ExportToXMLFile(outputFileName, $"{dataSource} {rateType}", publicationTime, writer, refExchangeRateZZList, updateType: Common.UniversalXmlWriter.UpdateType.Partial);
		}

		static List<RefExchangeRateZZ> GetRefExchangeRateZZList(Stream stream, string codeType)
		{
			var result = new List<RefExchangeRateZZ>();
			var refExchangeRateZZSet = new HashSet<string>();

			using (var file = new StreamReader(stream))
			{
				string line;
				while ((line = file.ReadLine()) != null)
				{
					string[] data = line.Split(';');

					if (data.Length < 6)
					{
						ParserErrorCollector.Instance.AppendLine($"Please check the format of the csv file: {line}");
					}
					else if (!DateTime.TryParseExact(data[0], "ddMMyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
					{
						ParserErrorCollector.Instance.AppendLine($"{data[0]} is not a invalid data, please check the format of the csv file: {line}");
					}
					else
					{
						if (!refExchangeRateZZSet.Add(data[0] + ";" + data[3]))
						{
							continue;
						}

						var nextWorkDate = CalendarUtils.GetBRNextWorkDay(date);
						var exchangeRate = new RefExchangeRateZZ()
						{
							ZZN_RX_NKExCurrency = data[3],
							ZZN_Rate = Convert.ToDecimal((Constants.ExchangeRateTypes.CustomsExport.Equals(codeType, StringComparison.Ordinal) ? data[4] : data[5]).Replace(",", "."), CultureInfo.CurrentCulture),
							ZZN_StartDate = nextWorkDate,
							ZZN_EndDate = nextWorkDate
						};

						result.Add(exchangeRate);
					}
				}
			}

			return result;
		}
	}
}
