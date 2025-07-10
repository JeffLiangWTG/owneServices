using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Staging.Schema_New;
using RefExchangeRateZZ = CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefExchangeRateZZ;

namespace CargoWise.RefDbRepo.Staging.IFTRINMessageProcessor
{
	public abstract class BaseMessageProcessor
	{
		protected BaseMessageProcessor(string country, string outputPath)
		{
			this.country = country;
			this.outputPath = outputPath;
		}

		readonly string country;
		readonly string outputPath;

		#region Process

		public int Process(SourceData sourceData)
		{
			Argument.NotNull(sourceData, nameof(sourceData));
			var messageText = sourceData.SDA_ContentText.Trim();

			ExchangeRateList.Clear();

			(bool success, DateTime issueDate) = ProcessCore(messageText);

			sourceData.SDA_Status = success
				? StatusProvider.GetMERStatus()
				: StatusProvider.GetERRStatus();

			var result = ExchangeRateList.Count;
			if (result > 0)
			{
				if (issueDate == DateTime.MinValue)
				{
					issueDate = sourceData.SDA_CreatedTime.Date;
				}

				ExportToXMLFile(issueDate);
			}
			return result;
		}

		protected abstract (bool success, DateTime issueDate) ProcessCore(string messageText);

		#endregion

		#region Add Exchange Rate

		protected void AddExchangeRate(string currencyCode, decimal currencyExchangeRate, DateTime startDate, DateTime endDate)
		{
			var exchangeRate = ExchangeRateList.FirstOrDefault(x => x.ZZN_RX_NKExCurrency.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));

			if (exchangeRate == null)
			{
				ExchangeRateList.Add(new RefExchangeRateZZ()
				{
					ZZN_StartDate = startDate,
					ZZN_EndDate = endDate,
					ZZN_RX_NKExCurrency = currencyCode,
					ZZN_Rate = currencyExchangeRate,
				});
			}
		}

		#endregion

		#region Export To XML File

		void ExportToXMLFile(DateTime issueDate)
		{
			ExportToXMLFileCore("Exchange Rates", GetExchangeRateWriterConfiguration(), UpdateType.Partial, ExchangeRateList, issueDate);
		}

		void ExportToXMLFileCore<T>(string sourceType, XmlWriterConfiguration xmlWriterConfig, UpdateType updateType, IEnumerable<T> dataList, DateTime issueDate)
			where T : RefDataRepoModelEntityType
		{
			if (dataList.Any())
			{
				var fileName = Path.Combine(outputPath, $"{typeof(T).Name}_{country}_{sourceType}_{issueDate.ToString(DateFormat, CultureInfo.InvariantCulture)}.xml");
				Helper.ExportToXMLFile($"{country} IFTRIN {sourceType}", fileName, xmlWriterConfig, issueDate, updateType, dataList);
			}
		}

		protected XmlWriterConfiguration GetExchangeRateWriterConfiguration()
		{
			return Helper.GetRefExchangeRateWriterConfiguration(country).writerConfiguration;
		}

		#endregion

		#region Implement

		protected const string DateFormat = "yyyyMMdd";

		protected List<RefExchangeRateZZ> ExchangeRateList => exchangeRateList ?? (exchangeRateList = new List<RefExchangeRateZZ>());
		List<RefExchangeRateZZ> exchangeRateList;

		#endregion
	}
}
