using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.TaiwanReferenceData.TurnkeyPlugInUpdateService;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public class ExchangeRateUpdateInfo : RefDataUpdateInfo
	{
		public ExchangeRateUpdateInfo(updateInfoBean info, string filename) : base(info, filename) { }

		protected virtual IWebClient GetWebClientWrapper()
		{
			return new WebClientWrapper();
		}

		List<IExchangeRateData> DataRows
		{
			get
			{
				if (rows == null)
				{
					rows = new List<IExchangeRateData>();
				}
				return rows;
			}
		}
		List<IExchangeRateData> rows;

		delegate bool GetDataRowsDelegate(string url);

		bool TryGetDataRowsFromTextFile(string url)
		{
			try
			{
				using (var webClient = GetWebClientWrapper())
				using (var txtFile = new MemoryStream(webClient.DownloadData(url)))
				{
					using (var reader = new StreamReader(txtFile, Encoding.UTF8))
					{
						reader.ReadLine();
						while (!reader.EndOfStream)
						{
							DataRows.Add(new ExchangeRateDataTXT(reader.ReadLine()));
						}
					}
				}
			}
			catch (System.Exception e)
			{
				Console.WriteLine("Fail to get exchange rate data through txt file, will fallback to use data from JSON file. Fail message: ");
				Console.WriteLine(e.Message);
				return false;
			}
			return true;
		}

		bool TryGetDataRowsFromJSONFile(string url)
		{
			try
			{
				using (var webClient = GetWebClientWrapper())
				using (var txtFile = new MemoryStream(webClient.DownloadData(url)))
				{
					using (var reader = new StreamReader(txtFile, Encoding.UTF8))
					{
						var text = reader.ReadToEnd();
						var obj = JsonConvert.DeserializeObject<ExchangeRateJSON>(text);

						foreach (var item in obj.items)
						{
							DataRows.Add(new ExchangeRateDataJSON(item.code, obj.start, new DateTime(obj.end.Year, obj.end.Month, obj.end.Day, 23, 59, 00), item.buyValue, item.sellValue));
						}
					}
				}
			}
			catch (System.Exception e)
			{
				Console.WriteLine("Fail to get exchange rate data through JSON file. Fail message: ");
				Console.WriteLine(e.Message);
				Console.WriteLine("Exchange rate update failed.");
				return false;
			}
			return true;
		}

		class ExchangeRateJSON
		{
			public DateTime start;
			public DateTime end;
			public ExchageRateItem[] items;

			public ExchangeRateJSON()
			{
				start = DateTime.MinValue;
				end = DateTime.MaxValue;
				items = null;
			}
		}

		class ExchageRateItem
		{
			public string code;
			public decimal buyValue;
			public decimal sellValue;

			public ExchageRateItem()
			{
				code = string.Empty;
				buyValue = 0m;
				sellValue = 0m;
			}
		}

		protected override void Execute()
		{
			string[] urlArray = Info.downloadURL.Split(';');
			GetDataRowsDelegate[] getDataRowsDelegates = { TryGetDataRowsFromTextFile, TryGetDataRowsFromJSONFile };

			for (int i = 0; i < getDataRowsDelegates.Length; i++)
			{
				if (getDataRowsDelegates[i](urlArray[i]))
				{
					break;
				}
			}

			if (DataRows.Count > 0)
			{
				var xmlWriterConfig = GetWriterConfiguration();
				var refExchangeRateZZs = new List<RefExchangeRateZZ>();
				var cueRateType = "CUE";
				var cusRateType = "CUS";
				foreach (var rate in DataRows)
				{
					if (!refExchangeRateZZs.Any(exchangeRate =>
					exchangeRate.ZZN_RX_NKExCurrency == rate.Currency &&
					exchangeRate.ZZN_ExRateType == cueRateType &&
					exchangeRate.ZZN_StartDate == rate.StartDate))
					{
						refExchangeRateZZs.Add(new RefExchangeRateZZ()
						{
							ZZN_RX_NKExCurrency = rate.Currency,
							ZZN_Rate = rate.InRate,
							ZZN_ExRateType = cueRateType,
							ZZN_StartDate = rate.StartDate,
							ZZN_EndDate = rate.EndDate,
						});
					}
					if (!refExchangeRateZZs.Any(exchangeRate =>
						exchangeRate.ZZN_RX_NKExCurrency == rate.Currency &&
						exchangeRate.ZZN_ExRateType == cusRateType &&
						exchangeRate.ZZN_StartDate == rate.StartDate))
					{
						refExchangeRateZZs.Add(new RefExchangeRateZZ()
						{
							ZZN_RX_NKExCurrency = rate.Currency,
							ZZN_Rate = rate.ExRate,
							ZZN_ExRateType = cusRateType,
							ZZN_StartDate = rate.StartDate,
							ZZN_EndDate = rate.EndDate,
						});
					}
				}
				WriteXmlHelper.ExportToXMLFile(Filename, "TW Exchange Rate", SystemContext.Now(), refExchangeRateZZs, xmlWriterConfig);
			}
		}

		XmlWriterConfiguration GetWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var exchangeRateConfiguration = new EntityTypeConfiguration<RefExchangeRateZZ>(true);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_RX_NKExCurrency, true);
			exchangeRateConfiguration.IncludeColumnWithConstantValue(x => x.ZZN_RN_NKCountry, true, Constants.DataGroupings.Taiwan);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_ExRateType, true);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_StartDate, true, IsDataValue.DefaultValueFromEntityType, XmlOperations.None, 0, "RefExchangeRate could have overlapped boundary dates, so it's eligible to have ZZN_StartDate as key property");
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_EndDate, false);
			exchangeRateConfiguration.IncludeColumn(x => x.ZZN_Rate, false);
			writerConfiguration.IncludeEntityTypeConfiguration(exchangeRateConfiguration);
			return writerConfiguration;
		}
	}
}
