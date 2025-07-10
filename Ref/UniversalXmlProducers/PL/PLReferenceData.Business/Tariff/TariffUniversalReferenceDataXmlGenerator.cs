using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using CargoWise.RefDbRepo.PLReferenceData.Business.Providers;
using CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer;

namespace CargoWise.RefDbRepo.PLReferenceData.Business.Tariff
{
	public static class TariffUniversalReferenceDataXmlGenerator
	{
		static class MetaData
		{
			public const string DataSource = "PL Import National Measures";
		}

		public static bool GenerateTariffUniversalReferenceData(IsztarHistoryResponse isztarHistoryResponse)
		{
			try
			{
				var data = isztarHistoryResponse ?? CommonHelper.GetBaseTaric4Data();
				if (data == null)
				{
					Console.Error.Write("Empty data received.");
					return false;
				}

				List<RefCusTariff> refCusTariffData = new List<RefCusTariff>();

				refCusTariffData.AddRange(PLTariffExtractor.GeneratePLTariffData(data, new DateTimeProvider()));

				refCusTariffData = MergeImportTariffsWithEUNTariffs(refCusTariffData);

				var saveFilePath = CommonHelper.GetOutputFilePath(Constants.TariffUniversalReferenceDataXmlFilename);
				GenerateReferenceDataXml(data.ResultsInfo.databaseDate, refCusTariffData, saveFilePath);
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
				return false;
			}

			return true;
		}

		static List<RefCusTariff> MergeImportTariffsWithEUNTariffs(List<RefCusTariff> refCusTariffData, EUNTariffExpander expander = null)
		{
			var tariffExpander = expander ?? new EUNTariffExpander();
			return tariffExpander.MergeImportTariffsWithEUNTarrifs(refCusTariffData);
		}

		public static void GenerateReferenceDataXml(DateTime publicationDate, List<RefCusTariff> data, string saveFilePath)
		{
			var xmlWriter = new XmlWriter(XmlWriterConfig.GetRefCusTariffWriterConfiguration());
			xmlWriter.SetDataSource(MetaData.DataSource);
			xmlWriter.SetPublicationTime(publicationDate);
			xmlWriter.SetUpdateType(UpdateType.Full);

			foreach (var tariff in data)
			{
				xmlWriter.PopulateData(tariff);
			}

			xmlWriter.SaveXml(saveFilePath);
		}
	}
}
