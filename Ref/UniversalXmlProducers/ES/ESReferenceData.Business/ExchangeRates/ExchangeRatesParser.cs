using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ESReferenceData.Services;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class ExchangeRatesParser : CommonParser
	{
		public ExchangeRatesParser(IDateTimeProvider dateTimeProvider) : base(dateTimeProvider) { }

		public string ConvertRatesForPenultimateWednesdayToXMLFile(string outPutFileWithPath, XmlDocument xmlDocument)
		{
			ErrorBuilder.Clear();
			var currentLocalDate = DateTimeProvider.CurrentLocalDate;
			var penultimateWednesday = currentLocalDate.GetLatestValidPenultimateWednesday();
			var penultimateWednesdayNodeFormat = penultimateWednesday.ToString(ExchangeRatesDateFormat, CultureInfo.InvariantCulture);
			var xmlRates = GetExchangeRatesNodes(xmlDocument, penultimateWednesdayNodeFormat);
			if (xmlRates != null)
			{
				var result = new List<RefExchangeRateZZ>();
				foreach (XmlNode rate in xmlRates.ChildNodes)
				{
					AddToRefList(result, rate);
				}
				Helper.ExportToXMLFile(Constants.DataSources.ExchangeRates, outPutFileWithPath, XMLWriterConfiguration, penultimateWednesday, result);
			}
			else
			{
				throw new ExchangeRatesException($"Unable to find the following date: {penultimateWednesdayNodeFormat} in Exchange Rates Document. Download date: {currentLocalDate.ToString(ExchangeRatesDateFormat, CultureInfo.InvariantCulture)}");
			}
			return ErrorBuilder.ToString();
		}

		static XmlNode GetExchangeRatesNodes(XmlDocument xmlDocument, string penultimateWednesdayNodeFormat)
		{
			var xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
			xmlNamespaceManager.AddNamespace("gesmes", "http://www.gesmes.org/xml/2002-08-01");
			xmlNamespaceManager.AddNamespace("lo", "http://www.ecb.int/vocabulary/2002-08-01/eurofxref");
			if (xmlDocument.SelectSingleNode($"//lo:Cube[@time]", xmlNamespaceManager) == null)
			{
				throw new ExchangeRatesException("XML Format has changed for Exchange Rates");
			}
			return xmlDocument.SelectSingleNode($"//lo:Cube[@time='{penultimateWednesdayNodeFormat}']", xmlNamespaceManager);
		}

		void AddToRefList(List<RefExchangeRateZZ> result, XmlNode rate)
		{
			var parsedExchangeRate = decimal.Zero;
			var exchangeRate = rate.Attributes["rate"]?.Value;
			var exchangeRateSuccessfullyParsed = exchangeRate != null && decimal.TryParse(exchangeRate, NumberStyles.AllowDecimalPoint, new CultureInfo("en-US"), out parsedExchangeRate);
			var exchangeRateCurrency = rate.Attributes["currency"]?.Value;
			if (CheckDataIsValid(exchangeRate, exchangeRateCurrency, exchangeRateSuccessfullyParsed))
			{
				result.Add(new RefExchangeRateZZ
				{
					ZZN_Rate = parsedExchangeRate,
					ZZN_RX_NKExCurrency = exchangeRateCurrency
				});
			}
		}

		bool CheckDataIsValid(string exchangeRate, string exchangeRateCurrency, bool exchangeRateSuccessfullyParsed)
		{
			var result = true;
			if (string.IsNullOrEmpty(exchangeRate) || string.IsNullOrEmpty(exchangeRateCurrency))
			{
				ErrorBuilder.AppendLine("Unable to import Exchange rate as missing attribute 'rate' or 'currency'. Details:");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Rate: {exchangeRate}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Currency: {exchangeRateCurrency}");
				result = false;
			}
			else if (!exchangeRateSuccessfullyParsed)
			{
				ErrorBuilder.AppendLine("Unable to parse Exchange rate into a decimal. Details:");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Rate: {exchangeRate}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Currency: {exchangeRateCurrency}");
				result = false;
			}
			return result;
		}

		protected override XmlWriterConfiguration XMLWriterConfiguration
		{
			get
			{
				var penultimateWednesday = DateTimeProvider.CurrentLocalDate.GetLatestValidPenultimateWednesday();
				var writerConfiguration = new XmlWriterConfiguration();
				var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
				exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, "CUS");
				exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
				exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
				exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, "ES");
				exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_StartDate, true, penultimateWednesday.GetFirstDayOfNextMonth(), IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
				exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_EndDate, false, penultimateWednesday.GetLastDayOfNextMonth());
				writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);
				return writerConfiguration;
			}
		}

		const string ExchangeRatesDateFormat = "yyyy-MM-dd";
	}
}
