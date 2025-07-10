using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class StatisticalClassificationPeriodSnapshotTestParser : StatisticalClassificationPeriodSnapshotParser
	{
		public StatisticalClassificationPeriodSnapshotTestParser(IDateTimeProvider dateTimeProvider)
			: base(dateTimeProvider)
		{
		}

		protected override string FileNamePrefix => ApplicationConfig.StatisticalClassificationPeriodSnapshotTestFilePrefix;

		protected override string OutputXMLName => "AU Customs Tariff Test.xml";

		protected override string DataSource => "AU Customs Tariff Test";

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate) => CMRXMLWriterConfigurationBuilder.BuildRefCusTariffUOMTestConfiguration();

		public override Dictionary<string, List<string>> STCPCharacteristicCodes => stcpCharacteristicCodes ??
			(stcpCharacteristicCodes = StatisticalClassificationPeriodCharacteristicParser.Parse(
				ApplicationConfig.StatisticalClassificationPeriodCharacteristicTestFilePrefix,
				ApplicationConfig.AUReferenceTestFilesDirectory));

		Dictionary<string, List<string>> stcpCharacteristicCodes;

		public override Dictionary<string, List<string>> TRFCCharacteristicCodes => trfcCharacteristicCodes ??
			(trfcCharacteristicCodes = TariffClassificationCharacteristicParser.Parse(
				ApplicationConfig.TariffClassificationCharacteristicTestFilePrefix,
				ApplicationConfig.AUReferenceTestFilesDirectory));

		Dictionary<string, List<string>> trfcCharacteristicCodes;

		public override Dictionary<string, List<string>> AQISCommodityStatisticalClassificationCodes => aqisCommodityStatisticalClassificationCodes ??
			(aqisCommodityStatisticalClassificationCodes = AQISCommodityStatisticalClassificationParser.Parse(
				ApplicationConfig.AQISCommodityStatisticalClassificationTestFilePrefix,
				ApplicationConfig.AUReferenceTestFilesDirectory));

		Dictionary<string, List<string>> aqisCommodityStatisticalClassificationCodes;
	}
}
