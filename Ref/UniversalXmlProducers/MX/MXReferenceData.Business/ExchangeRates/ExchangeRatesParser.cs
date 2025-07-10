using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.MXReferenceData.Business
{
	public class ExchangeRatesParser : BaseParser
	{
		public ExchangeRatesParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(Stream inputStream, string outputFileName, string rateType, DateTime publicationTime)
		{
			Argument.NotNull(inputStream, nameof(inputStream));
			Argument.NotNullOrEmpty(outputFileName, nameof(outputFileName));
			Argument.NotNullOrEmpty(rateType, nameof(rateType));

			var refExchangeRateZZList = GetRefExchangeRateZZList(inputStream, rateType);
			var writer = Helper.GetRefExchangeRateZZWriterConfiguration(rateType, rateType == Constants.ExchangeRateTypes.CustomsExport ? Constants.ExchangeCurrencyCodes.Dolar : null);
			Helper.ExportToXMLFile(outputFileName, $"{dataSource} {rateType}", publicationTime, writer, refExchangeRateZZList, updateType: Common.UniversalXmlWriter.UpdateType.Partial);
		}

		static List<RefExchangeRateZZ> GetRefExchangeRateZZList(Stream stream, string codeType)
		{
			var result = new List<RefExchangeRateZZ>();
			var refExchangeRateZZSet = new HashSet<string>();
			var xmlCtarDepais = XDocument.Load(stream);

			Contract.Assume(xmlCtarDepais != null);

			var xml = xmlCtarDepais.Root?.Descendants("Row");

			foreach (XElement element in xml)
			{
				var original = element.Descendants("Original").FirstOrDefault();
				if (GetRefExchangeRate(original, codeType) is RefExchangeRateZZ exchangeRateZZ)
				{
					result.Add(exchangeRateZZ);
				}
			}
				

			return result;
		}

		static RefExchangeRateZZ GetRefExchangeRate(XElement element, string codeType)
		{
			var rate = codeType switch
			{
				Constants.ExchangeRateTypes.Customs => (decimal)double.Parse(element.Attribute("EQU_DLLS").Value.Trim(), CultureInfo.InvariantCulture),
				Constants.ExchangeRateTypes.CustomsExport => (decimal)double.Parse(element.Attribute("TIP_CAM").Value.Trim(), CultureInfo.InvariantCulture),
				_ => decimal.Zero
			};
			var currency = codeType switch
			{
				Constants.ExchangeRateTypes.Customs => element.Attribute("MON_PAI").Value.Trim(),
				_ => string.Empty
			};
			var startDate = codeType switch
			{
				Constants.ExchangeRateTypes.Customs => DateTime.ParseExact(element.Attribute("FEC_DOF").Value.Trim(), "yyyyMMddTHHmmss", CultureInfo.InvariantCulture),
				Constants.ExchangeRateTypes.CustomsExport => DateTime.ParseExact(element.Attribute("FEC_CAM").Value.Trim(), "yyyyMMddTHHmmss", CultureInfo.InvariantCulture),
				_ => DateTime.MinValue
			};
			var endDate = codeType switch
			{
				Constants.ExchangeRateTypes.Customs => element.Attribute("VIG_HAST")?.Value.Trim() is string endDateString ? DateTime.ParseExact(endDateString, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture).AddDays(-2) : startDate.AddMonths(2),
				Constants.ExchangeRateTypes.CustomsExport => startDate,
				_ => DateTime.MinValue
			};
			if (rate != decimal.Zero)
			{
				return new RefExchangeRateZZ
				{
					ZZN_ExRateType = codeType,
					ZZN_StartDate = startDate,
					ZZN_EndDate = endDate,
					ZZN_Rate = rate,
					ZZN_RX_NKExCurrency = currency,
					ZZN_RN_NKCountry = Constants.DataGroupingCodes.Mexico,
					ZZN_AsPublished = string.Empty
				};
			}
			return null;
		}
	}
}
