using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using CargoWise.RefDbRepo.KRReferenceData.ExchangeRates.Services;

namespace CargoWise.RefDbRepo.KRReferenceData.ExchangeRates.Business
{
	public class ExchangeRateParser
	{
		public ExchangeRateParser(string downloadResult)
		{
			this.downloadResult = downloadResult;
		}
		readonly string downloadResult;
		public void ConvertToXMLFile(string outputFilePath, string dataSource, ExchangeRateTypes rateType)
		{
			ErrorBuilder.Clear();
			trifFxrtInfoQryRtnVo xml = Helper.DeserializeFromString<trifFxrtInfoQryRtnVo>(downloadResult);

			if (!int.TryParse(xml.tCnt, out int count) || count < 0)
			{
				throw new FormatException(xml.ntceInfo);
			}

			var startDate = DateTime.MinValue;
			var exchangeRates = new List<RefExchangeRateZZ>();
			foreach (var exchangeRate in xml.trifFxrtInfoQryRsltVo)
			{
				var (exchangeRateZZ, stDate) = Convert(exchangeRate, rateType);
				startDate = stDate;
				exchangeRates.Add(exchangeRateZZ);
			}
			var writerConfiguration = GetRefExchangeRateWriterConfiguration(rateType == ExchangeRateTypes.Export ? Constants.RateTypes.ExportExRateType : Constants.RateTypes.ImportExRateType, startDate);
			Helper.ExportToXMLFile(dataSource, outputFilePath, writerConfiguration, startDate, exchangeRates);
		}

		static (RefExchangeRateZZ exchangeRateZZ, DateTime startDate) Convert(trifFxrtInfoQryRsltVo exchangeRateXml, ExchangeRateTypes expectedRateType)
		{
			var (startDateOk, startDate) = exchangeRateXml.aplyBgnDt.GetDateTime("yyyyMMdd");
			var currency = exchangeRateXml.currSgn;
			var rateOk = decimal.TryParse(exchangeRateXml.fxrt, NumberStyles.Number, new CultureInfo("ko-KR"), out var rate);

			if (rateOk && rate > decimal.Zero && startDateOk && !string.IsNullOrEmpty(currency) && int.TryParse(exchangeRateXml.imexTp, out int convertedRateType) && convertedRateType == (int)expectedRateType)
			{
				return
					(new RefExchangeRateZZ
					{
						ZZN_Rate = rate,
						ZZN_RX_NKExCurrency = currency
					},
					startDate);
				;
			}

			var errorBuilder = new StringBuilder();
			errorBuilder.AppendLine("Unable to parse Exchange Rate due to empty currency, zero/negative rate or invalid Dates or mismatched rate type.");
			errorBuilder.AppendLine("DETAILS:");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Currency: {exchangeRateXml.currSgn}");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Rate: {exchangeRateXml.fxrt}");
			errorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Start Date: {exchangeRateXml.aplyBgnDt}");
			throw new MethodAccessException(errorBuilder.ToString());
		}

		static XmlWriterConfiguration GetRefExchangeRateWriterConfiguration(string zzn_ExRateType, DateTime startDate)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, zzn_ExRateType);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, Constants.CountryCodes.KoreaSouth);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_StartDate, true, startDate, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_EndDate, false, startDate.AddDays(6));
			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);

			return writerConfiguration;
		}

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;
	}
}
