using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.JPReferenceData.Services;

namespace CargoWise.RefDbRepo.JPReferenceData.Business
{
	public class ExchangeRateParser
	{
		public string ConvertToXMLFile(IExchangeRates exchangeRate, string outputFilePath, string sourcePDFFileAndPath)
		{
#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				var startDateParse = DateTime.TryParseExact(exchangeRate.StartDate, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate);
				var endDateParse = DateTime.TryParseExact(exchangeRate.EndDate, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate);
				if (startDateParse && endDateParse)
				{
					var writerConfiguration = GetWriterConfiguration(startDate, endDate);
					var writer = new XmlWriter(writerConfiguration);
					writer.SetDataSource(DataSource);
					writer.SetPublicationTime(startDate);
					writer.SetUpdateType(UpdateType.Full);
					var addedCodes = new HashSet<string>();

					foreach (var rate in exchangeRate.ExchangeRateDetails)
					{
						if (ValidateData(rate, addedCodes))
						{
							var code = rate.Code;
							var newRate = new RefExchangeRateZZ()
							{
								ZZN_RX_NKExCurrency = code,
								ZZN_Rate = rate.Rate
							};
							writer.PopulateData(newRate);
							addedCodes.Add(code);
						}
						else
						{
							AppendInvalidDataErrorDetails(rate, sourcePDFFileAndPath);
						}
					}
					writer.SaveXml(outputFilePath);
				}
				else
				{
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to parse Start or End date. Start date string: {exchangeRate.StartDate}. End state string: {exchangeRate.EndDate}");
				}
			}
			catch (Exception ex)
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to parse exchange rate./r/n {ex.Message}");
			}
#pragma warning restore CA1031 // Do not catch general exception types
			return ErrorBuilder.ToString();
		}

		static bool ValidateData(IExchangeRateDetails rate, HashSet<string> addedCodes)
		{
			var code = rate.Code;
			return !string.IsNullOrEmpty(code)
				&& rate.Rate != decimal.Zero
				&& !addedCodes.Contains(code);
		}

		void AppendInvalidDataErrorDetails(IExchangeRateDetails invalidRecord, string sourcePDFFileAndPath)
		{
			ErrorBuilder.AppendLine("Unable to import Exchange Rate due to empty Code, Rate or Duplicate Code.");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Source PDF Details: {sourcePDFFileAndPath}");
			ErrorBuilder.AppendLine("DETAILS:");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {invalidRecord.Code}");
			ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Rate: {invalidRecord.Rate}");
		}

		static XmlWriterConfiguration GetWriterConfiguration(DateTime startDate, DateTime endDate)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_ExRateType, true, "CUS");
			exchangeRateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZN_StartDate, true, startDate, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateConfiguration.IncludeColumnWithDefaultValue(x => x.ZZN_EndDate, false, endDate);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, "JP");

			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);
			return writerConfiguration;
		}

		const string DataSource = "JP Exchange Rate";
		const string DateTimeFormat = "yyyyMMdd";

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;
	}
}
