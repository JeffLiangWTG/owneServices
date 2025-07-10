using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.NZReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public sealed class ConcessionParser
	{
		public ConcessionParser(ILogger logger)
		{
			DataRepo = new ConcessionDataRepo();
			Logger = logger;
		}

		const string DataSource = "NZ Tariff Concessions";

		internal ITopLevelDataRepo<RefCusTariff> DataRepo { get; }

		ILogger Logger { get; }

		public bool Parse(BuildersFilePath[] filePaths, IDateProvider dateProvider, NZConcessionProcessingData processingData)
		{
			var result = true;
			
			var concesstionPublicationTimeBuilder = new TariffPublicationTimeBuilder<NZConcessionProcessingData>(
				(NZConcessionProcessingData processingData) => processingData.LastRunDateConcession,
				(NZConcessionProcessingData processingData, DateTime dateTime) => processingData.LastRunDateConcession = dateTime);
			result = concesstionPublicationTimeBuilder.Build(DataRepo, new BuildersFilePath[] { filePaths.FirstOrDefault(x => x.Symbol == BuilderFilePathSymbol.PublicationTime) }, processingData);

			if (result)
			{
				var builder = new ConcessionBuilder(dateProvider, Logger);
				result = builder.Build(DataRepo, filePaths, processingData);
			}

			DataRepo.RemoveInvalidData();
			return result;
		}

		public void ExportToXMLFile(string outputFolderPath)
		{
			var list = DataRepo.Get();
			var writerConfiguration = GetWriterConfiguration();

			var chapterToFile = new Dictionary<string, int>();
			for (var c = 1; c < 100; c++)
			{
				chapterToFile[c.ToString("00", CultureInfo.InvariantCulture)] = (c - 1) / 10;
			}
			var tariffsByChapters = list.ToLookup(x => chapterToFile[x.ZZ1_TariffCode.Substring(0, 2)]);

			foreach (var tariffsByChapter in tariffsByChapters)
			{
				var chapterFrom = (tariffsByChapter.Key * 10 + 1).ToString("00", CultureInfo.InvariantCulture);
				var chapterTo = ((tariffsByChapter.Key + 1) * 10).ToString("00", CultureInfo.InvariantCulture);
				var saveList = tariffsByChapter;
				if (saveList.Any())
				{
					var outputFileFullName = Path.Combine(outputFolderPath, $"RefCusTariffConcessions_NZ_{chapterFrom}-{chapterTo}.xml");
					var writer = Helper.GenerateXmlWriter(writerConfiguration, UpdateType.Full, $"{DataSource} {chapterFrom}-{chapterTo}", DataRepo.PublicationTime, saveList);
					Helper.ExportToXMLFile(writer, outputFileFullName);
				}

				GC.Collect();
			}
		}

		static XmlWriterConfiguration GetWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var tariffConfig = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffConfig.IncludeColumn(x => x.RefCusRates);
			tariffConfig.IncludeColumn(x => x.RefCusTariffRelationships);
			tariffConfig.IncludeColumn(x => x.ZZ1_Description);
			tariffConfig.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, Constants.DefaultEndDateTime);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_IAMUnique, true, 0);
			tariffConfig.IncludeColumn(x => x.ZZ1_StartDate);
			tariffConfig.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZF_NKTaxOrFeeCode, true, Constants.TaxOrFeeCodes.GST);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffTypes.CON);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.NewZealand);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.NewZealand);

			var rateConfig = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfig.IncludeColumn(x => x.RefCusApplicabilities, true);
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_StartDate, false, MinSmallDateTime);
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_EndDate, false, MaxSmallDateTime);
			rateConfig.IncludeColumnWithDefaultValue(x => x.ZZ2_RateFormula, false, 0);
			rateConfig.IncludeColumnWithDefaultValue(x => x.ZZ2_ZZS_NKPreference, true, TariffTradeGroups.NML);
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.NewZealand);
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_NKRateCode, false, TariffRateCodes.DTY);
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_NKRateType, false, TariffRateCodes.DTY);

			var rateApplicability = new EntityTypeConfiguration<RefCusApplicability>(true);
			rateApplicability.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.NewZealand);
			rateApplicability.IncludeColumn(x => x.ZZT_StartDate);
			rateApplicability.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, false, MaxSmallDateTime);
			rateApplicability.IncludeColumnWithDefaultValue(x => x.ZZT_ZZA_NKTradeGroup, true, TariffTradeGroups.NML);
			rateApplicability.IncludeColumnWithDefaultValue(x => x.ZZT_AdditionalCode, false, string.Empty);
			rateApplicability.IncludeColumn(x => x.ZZT_OrderNumber, true);

			var tariffRelationshipConfig = new EntityTypeConfiguration<RefCusTariffRelationship>(true);
			tariffRelationshipConfig.IncludeColumn(x => x.ZZH_TariffCode, true);
			tariffRelationshipConfig.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_NKTariffType, true, Constants.TariffTypes.HSN);
			tariffRelationshipConfig.IncludeColumnWithConstantValue(x => x.ZZH_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.NewZealand);

			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(rateConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(rateApplicability);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffRelationshipConfig);

			return writerConfiguration;
		}
	}
}
