using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using CargoWise.RefDbRepo.PLReferenceData.Business.Providers;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Quota;

public static class QuotaUniversalReferenceDataXmlGenerator
{
	static class MetaData
	{
		public const string DataSource = "PL Import Taric Quota";
	}

	public static bool GenerateQuotaUniversalReferenceData(IsztarHistoryResponse isztarHistoryResponse)
	{
		try
		{
			var data = isztarHistoryResponse ?? CommonHelper.GetBaseTaric4Data();
			if (data == null)
			{
				Console.Error.WriteLine("Empty data received.");
				return false;
			}

			var refCusQuotaData = PLQuotaExtractor.GeneratePLQuotaData(data, new DateTimeProvider());
			var saveFilePath = CommonHelper.GetOutputFilePath(Constants.QuotaUniversalReferenceDataXmlFilename);
			GenerateReferenceDataXml(data.ResultsInfo.databaseDate, refCusQuotaData, saveFilePath);
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex);
			return false;
		}

		return true;
	}

	public static void GenerateReferenceDataXml(DateTime publicationDate, IEnumerable<RefCusQuota> data, string saveFilePath)
	{
		var xmlWriter = new XmlWriter(XmlWriterConfig.GetRefCusQuotaWriterConfiguration());
		xmlWriter.SetDataSource(MetaData.DataSource);
		xmlWriter.SetPublicationTime(publicationDate);
		xmlWriter.SetUpdateType(UpdateType.Full);

		foreach (var quota in data)
		{
			xmlWriter.PopulateData(quota);
		}

		xmlWriter.SaveXml(saveFilePath);
	}
}
