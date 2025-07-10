using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public class TariffParser
	{
		public TariffParser(ILogger logger)
		{
			DataRepo = new TariffDataRepo();
			Logger = logger;
		}

		ILogger Logger { get; }

		const string DataSource = "NZ Tariff Rates";
		const string OutputFileName = "NZRefCusTariff_{0}.xml";

		internal ITopLevelDataRepo<RefCusTariff> DataRepo { get; }

		public bool Parse(string dir, IDateProvider dateProvider, NZTariffProcessingData processingData)
		{
			var result = true;
			var builderInfos = BuildersProvider.GetBuilderInfos(dir, dateProvider, Logger);

			foreach (var builderInfo in builderInfos)
			{
				if (!builderInfo.Builder.Build(DataRepo, builderInfo.FilePaths, processingData))
				{
					result = false;
					break;
				}
			}

			DataRepo.RemoveInvalidData();
			DataRepo.Sort();
			return result;
		}

		public void ExportToXMLFile(string outputFolderPath)
		{
			var list = DataRepo.Get();
			var writerConfiguration = GetWriterConfiguration();
			var tariffSets = list.ToLookup(x => x.ZZ1_TariffCode.Substring(0, 1));

			foreach (var tariffSet in tariffSets)
			{
				if (tariffSet.Any())
				{
					var rangeKey = $"{tariffSet.Key}0-{tariffSet.Key}9";
					var outputFileFullName = Path.Combine(outputFolderPath, string.Format(CultureInfo.InvariantCulture, OutputFileName, rangeKey));
					var writer = Helper.GenerateXmlWriter(writerConfiguration, UpdateType.Full, $"{DataSource} {rangeKey}", DataRepo.PublicationTime, tariffSet);
					Helper.ExportToXMLFile(writer, outputFileFullName);
				}
			}
		}

		protected virtual IBuildersProvider BuildersProvider { get; } = new BuildersProvider();

		static XmlWriterConfiguration GetWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var tariffConfig = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffConfig.IncludeColumn(x => x.RefCusTariffUOMs);
			tariffConfig.IncludeColumn(x => x.RefCusRates);
			tariffConfig.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfig.IncludeColumn(x => x.ZZ1_StartDate);
			tariffConfig.IncludeColumn(x => x.ZZ1_Description);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_IAMUnique, true, 0);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_NKTariffType, true, Constants.TariffTypes.HSN);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.NewZealand);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.NewZealand);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZF_NKTaxOrFeeCode, true, Constants.TaxOrFeeCodes.GST);
			tariffConfig.IncludeColumnWithDefaultValue(x => x.ZZ1_EndDate, false, Constants.MaxSmallDateTime);

			var rateApplicability = new EntityTypeConfiguration<RefCusApplicability>(true);
			rateApplicability.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.NewZealand);
			rateApplicability.IncludeColumnWithDefaultValue(x => x.ZZT_AdditionalCode, false, string.Empty);
			rateApplicability.IncludeColumnWithDefaultValue(x => x.ZZT_EndDate, false, Constants.MaxSmallDateTime);
			rateApplicability.IncludeColumn(x => x.ZZT_StartDate);
			rateApplicability.IncludeColumnWithDefaultValue(x => x.ZZT_ZZA_NKTradeGroup, true, Constants.TariffTradeGroups.NML);

			var rateUOMConfig = new EntityTypeConfiguration<RefCusRateUOM>(true);
			rateUOMConfig.IncludeColumnWithDefaultValue(x => x.ZXG_UOM, true, Constants.TariffUOMs.NMB);

			var tariffUOMConfig = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			tariffUOMConfig.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.NewZealand);
			tariffUOMConfig.IncludeColumnWithDefaultValue(x => x.ZZ8_Type, true, Constants.TariffUOMTypes.CU1);
			tariffUOMConfig.IncludeColumnWithDefaultValue(x => x.ZZ8_UOM, false, Constants.TariffUOMs.NMB);

			var rateConfig = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfig.IncludeColumn(x => x.RefCusRateUOMs);
			rateConfig.IncludeColumn(x => x.RefCusApplicabilities, true);
			rateConfig.IncludeColumnWithDefaultValue(x => x.ZZ2_RateFormula, false, "0");
			rateConfig.IncludeColumnWithDefaultValue(x => x.ZZ2_ZY1_NKRateCode, true, Constants.TariffRateCodes.DTY);
			rateConfig.IncludeColumnWithDefaultValue(x => x.ZZ2_ZY1_ZZR_NKRateType, true, Constants.TariffRateCodes.DTY);
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.NewZealand);
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, Constants.DataGroupingCodes.NewZealand);
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_StartDate, false, Constants.MinSmallDateTime);
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_EndDate, false, Constants.MaxSmallDateTime);
			rateConfig.IncludeColumnWithDefaultValue(x => x.ZZ2_ZZS_NKPreference, true, Constants.TariffTradeGroups.NML);

			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffUOMConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(rateConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(rateUOMConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(rateApplicability);

			return writerConfiguration;
		}
	}
}
