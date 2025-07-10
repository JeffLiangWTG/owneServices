using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.USReferenceData.Services;

namespace CargoWise.RefDbRepo.USReferenceData.Business
{
	public abstract class TariffParser
	{
		protected TariffParser(IDownLoadService serviceClient, DateTime processDate, IReadOnlyDictionary<string, Tariff4PGA[]> tariff4PGAMap, List<EV1> ev1List)
		{
			Tariff4PGAMap = tariff4PGAMap;
			ServiceClient = serviceClient;
			ProcessDate = processDate;
			EV1List = ev1List;
		}

		readonly IDownLoadService ServiceClient;
		readonly DateTime ProcessDate;
		readonly IReadOnlyDictionary<string, Tariff4PGA[]> Tariff4PGAMap;
		readonly List<EV1> EV1List;

		public string DownloadAndConvertCodesToXMLFile(string outPutFilePath)
		{
			ErrorBuilder.Clear();
			var htmlNodeWithDate = ServiceClient.FindNode(ApplicationConfig.Instance.CensusTradeDocumentReferenceLibraryEndPoint, node => node.Name == Constants.HtmlNodeNames.H3 && node.InnerText.Contains(UpdateInfoNodeKeyword));
			var (dateSuccessfullyParsed, publishedDateTime) = ServiceClient.GetDateTimeFromHtmlNode(htmlNodeWithDate);
			if (dateSuccessfullyParsed)
			{
				var htmlNodeWithFile = ServiceClient.FindNode(ApplicationConfig.Instance.CensusTradeDocumentReferenceLibraryEndPoint, node => node.Name == Constants.HtmlNodeNames.A && node.InnerText.Contains(UpdateInfoNodeKeyword));
				var fileDownLoaded = ServiceClient.DownloadFile(htmlNodeWithFile?.OuterHtml ?? string.Empty, ApplicationConfig.Instance.CensusEndPoint, DownloadFilePath);
				if (fileDownLoaded)
				{
					XmlWriterHelper.ExportToXMLFile(XMLWriterDataSource, Path.Combine(outPutFilePath, OutPutFileName), XmlWriterHelper.GetRefCusTariffWriterConfiguration(TariffType), ProcessDate, GetRefCusTariff(publishedDateTime));
				}
				else
				{
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to download file from the website. Processing has failed for Tariff Type: {TariffType}");
				}
			}
			else
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to get file update date from the website. Processing has failed for Tariff Type: {TariffType}");
			}

			return ErrorBuilder.ToString();
		}

		protected abstract string OutPutFileName { get; }

		protected abstract string DownloadFileName { get; }

		protected abstract string TariffType { get; }

		protected abstract string UpdateInfoNodeKeyword { get; }

		protected abstract string XMLWriterDataSource { get; }

		static bool CheckDataIsValid(TariffKeyValues keyValues) => !string.IsNullOrWhiteSpace(keyValues.Code) && !string.IsNullOrWhiteSpace(keyValues.Description);

		string DownloadFilePath => Path.Combine(Path.GetTempPath(), DownloadFileName);

		List<RefCusTariff> GetRefCusTariff(DateTime startDateTime)
		{
			var result = new List<RefCusTariff>();
			foreach (var keyValues in GetTariffKeyValuesList())
			{
				if (CheckDataIsValid(keyValues))
				{
					var refCusTariff = new RefCusTariff()
					{
						ZZ1_TariffCode = keyValues.Code,
						ZZ1_Description = keyValues.Description,
						ZZ1_StartDate = startDateTime,
					};

					var refCusTariffUOMs = new List<RefCusTariffUOM>();
					AddTariffUOM(refCusTariffUOMs, Constants.TariffUOMTypes.CU1, keyValues.UOM1);
					AddTariffUOM(refCusTariffUOMs, Constants.TariffUOMTypes.CU2, keyValues.UOM2);
					refCusTariff.RefCusTariffUOMs = refCusTariffUOMs.ToArray();

					PopulateRefCusConditions(refCusTariff);
					PopulateRefCusTariffAttribute(refCusTariff);
					result.Add(refCusTariff);
				}
				else
				{
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Tariff Type: {TariffType}");
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {keyValues.Code}");
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {keyValues.Description}");
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"First UOM: {keyValues.UOM1}");
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Second UOM: {keyValues.UOM2}");
				}
			}
			return result;
		}

		List<TariffKeyValues> GetTariffKeyValuesList()
		{
			var result = new List<TariffKeyValues>();
			using (var streamReader = new StreamReader(DownloadFilePath))
			{
				string line;
				while ((line = streamReader.ReadLine()) != null)
				{
					result.Add(new TariffKeyValues
					{
						Code = line.Substring(0, 10),
						Description = line.Substring(15, 155).TrimEnd(),
						UOM1 = line.Substring(170, 8).TrimEnd(),
						UOM2 = line.Substring(178).TrimEnd()
					});
				}
			}
			return result;
		}

		static void AddTariffUOM(List<RefCusTariffUOM> refCusTariffUOMs, string type, string value)
		{
			if (!string.IsNullOrWhiteSpace(value))
			{
				refCusTariffUOMs.Add(new RefCusTariffUOM()
				{
					ZZ8_Type = type,
					ZZ8_UOM = value
				});
			}
		}

		void PopulateRefCusConditions(RefCusTariff refCusTariff)
		{
			if (Tariff4PGAMap != null)
			{
				Tariff4PGA[] tariff4PGAArray = null;
				if (!Tariff4PGAMap.TryGetValue(refCusTariff.ZZ1_TariffCode, out tariff4PGAArray))
				{
					var tariffPrefix = Tariff4PGAMap.Keys.FirstOrDefault(f => refCusTariff.ZZ1_TariffCode.StartsWith(f, StringComparison.OrdinalIgnoreCase));
					if (tariffPrefix != null)
					{
						Tariff4PGAMap.TryGetValue(tariffPrefix, out tariff4PGAArray);
					}
				}
				if (tariff4PGAArray != null)
				{
					var condition = new RefCusCondition();
					condition.RefCusConditionValues = tariff4PGAArray.Select(s => new RefCusConditionValue()
					{
						ZX3_Value = s.IsMandatory ? "M" : "O",
						ZX3_ZX4_NKValueType = s.PGACode
					}).ToArray();
					if (refCusTariff.RefCusConditions == null)
					{
						refCusTariff.RefCusConditions = new RefCusCondition[] { condition };
					}
					else
					{
						var tariffList = refCusTariff.RefCusConditions.ToList();
						tariffList.Add(condition);
						refCusTariff.RefCusConditions = tariffList.ToArray();
					}
				}
			}
		}

		void PopulateRefCusTariffAttribute(RefCusTariff refCusTariff)
		{
			if (EV1List?.Any() ?? false)
			{
				var ev1 = EV1List.FirstOrDefault(x => x.TariffCode == refCusTariff.ZZ1_TariffCode);
				if (ev1 != null)
				{
					refCusTariff.RefCusTariffAttributes = new[] { new RefCusTariffAttribute()
					{
						ZZ3_Name = Constants.AttributeNames.EV1,
						ZZ3_Value = ev1.IsMandatory ? "M" : "O",
					}};
				}
			}
		}

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;
	}
}
