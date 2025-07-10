using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class TradeGroupProducer : BaseProducer, ITradeGroupProducer
	{
		readonly ITradeGroupParser tradeGroupParser;
		readonly IXmlProducer<RefCusTradeGroup> xmlProducer;

		public TradeGroupProducer()
		{
			tradeGroupParser = new TradeGroupParser();
			xmlProducer = new TradeGroupXmlProducer();
		}

		public override IEnumerable<string> FilesToLocate => new[] { "Geographical areas Composition", "Geographical area composition" };

		public override string EUNLibraryBasePage => ApplicationConfig.EUTariffBaseUrl;

		public void Run()
		{
			var files = LocateWebFiles().ToList();

			var errors = files.Where(x => x.Exception != null);
			if (errors.Any() && errors.Count() == FilesToLocate.Count())
			{
				var error = errors.First();
				xmlProducer.InitializeWriter(DateTime.Today);
				var errorMessage = error.Exception.Message;
				xmlProducer.ReportDataSourceError(errorMessage);
				throw error.Exception;
			}

			files = files.Where(x => x.Exception == null).ToList();
			var filesDownloadedSuccessfully = DownloadFiles(files);
			if (!filesDownloadedSuccessfully)
			{
				throw new InvalidOperationException("Failure downloading files.");
			}

			var parsedRecords = GetRawRecords(files);
			var realTradeGroupRecords = ConvertToRealTradeGroups(parsedRecords);

			xmlProducer.InitializeWriter(PublishTime);
			xmlProducer.ExportToXml(realTradeGroupRecords);
		}

		static IEnumerable<RefCusTradeGroup> ConvertToRealTradeGroups(IEnumerable<IRawTradeGroupRecord> parsedRecords)
		{
			Argument.NotNull(parsedRecords, nameof(parsedRecords));

			var result = new List<RefCusTradeGroup>();
			var groupedTradeGroupRecords = parsedRecords.GroupBy(x => x.CountryGroup).OrderBy(x => x.Key);
			IEnumerable<IRawTradeGroupRecord> tradeGroup1010 = new List<IRawTradeGroupRecord>();
			const string euCountryGroup = "1010";
			const string ergaOmnesCountryGroup = "1011";

			foreach (var tradeGroup in groupedTradeGroupRecords)
			{
				var tradeGroupRecord = new RefCusTradeGroup
				{
					ZZA_TradeGroup = tradeGroup.Key
				};

				var tradeGroupData = parsedRecords.Where(o => o.CountryGroup == tradeGroup.Key);
				if (tradeGroupData.Any())
				{
					if (tradeGroup.Key.Equals(euCountryGroup, StringComparison.Ordinal))
					{
						tradeGroup1010 = tradeGroupData;
					}

					if (tradeGroup.Key.Equals(ergaOmnesCountryGroup, StringComparison.Ordinal))
					{
						tradeGroupData = tradeGroupData
							.Union(tradeGroup1010)
							.GroupBy(x => x.MemberCountry)
							.Select(x => x.FirstOrDefault());
					}

					var firstRecordData = tradeGroupData.FirstOrDefault();

					tradeGroupRecord.ZZA_Description = firstRecordData.CountryGroupDescription;
					tradeGroupRecord.ZZA_StartDate = firstRecordData.StartDate;

					var tradeGroupCountries = new List<RefCusTradeGroupCountry>();
					foreach (var countryRecord in tradeGroupData)
					{
						tradeGroupCountries.Add(new RefCusTradeGroupCountry
						{
							ZZB_RN_NKTradeGroupCountryCode = countryRecord.MemberCountry,
							ZZB_Description = countryRecord.MemberCountryDescription,
							ZZB_StartDate = countryRecord.MemberStartDate,
							ZZB_EndDate = countryRecord.MemberEndDate
						});
					}
					tradeGroupRecord.RefCusTradeGroupCountries = tradeGroupCountries.ToArray();
					result.Add(tradeGroupRecord);
				}
			}

			return result;
		}

		IEnumerable<IRawTradeGroupRecord> GetRawRecords(IEnumerable<IWebFileInfo> webFileInfos)
		{
			Argument.NotNull(webFileInfos, nameof(webFileInfos));

			var tradeGroupRecords = new List<IRawTradeGroupRecord>();
			var downloadsFolder = ApplicationConfig.DownloadsFolder;

			foreach (var webFileInfo in webFileInfos)
			{
				var filePath = Path.Combine(downloadsFolder, webFileInfo.FileName);
				if (webFileInfo.FileName.ToUpperInvariant().Contains("GEOGRAPHICAL"))
				{
					tradeGroupRecords.AddRange(tradeGroupParser.Parse(filePath));
				}
			}

			return tradeGroupRecords;
		}
	}
}
