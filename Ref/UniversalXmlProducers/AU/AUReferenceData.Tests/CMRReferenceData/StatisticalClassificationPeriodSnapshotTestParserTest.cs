using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	abstract class BaseStatisticalClassificationPeriodSnapshotTestParserTest : CMRTariffDataParserAbstractTest
	{
		protected override string TestFileFolderName => "StatisticalClassificationPeriodSnapshotTest";

		protected override string TextFileName => "STCPSNAP-Q1-EDMAIN-1906050022.txt";

		protected override ITariffDataParser Parser
		{
			get
			{
				var mockDateTimeProvider = new Mock<IDateTimeProvider>();
				mockDateTimeProvider.Setup(x => x.CurrentLocalDateTime).Returns(TodaysDate);

				var mockParser = new Mock<StatisticalClassificationPeriodSnapshotTestParser>(mockDateTimeProvider.Object) { CallBase = true };
				mockParser.Setup(x => x.TariffsSourceFilePath).Returns(TestTariffsSourceFilePath);
				mockParser.Setup(x => x.STCPCharacteristicCodes).Returns(STCPCharacteristicCodes);
				mockParser.Setup(x => x.TRFCCharacteristicCodes).Returns(TRFCCharacteristicCodes);
				mockParser.Setup(x => x.AQISCommodityStatisticalClassificationCodes).Returns(AQISCommodityStatisticalClassificationCodes);

				return mockParser.Object;
			}
		}
	}

	[TestFixture]
	sealed class StatisticalClassificationPeriodSnapshotTestParserTest : BaseStatisticalClassificationPeriodSnapshotTestParserTest
	{
		protected override string[] StringArgs => new string[] { };

		protected override string XMLFileName => "AU Customs Tariff Test.xml";

		protected override DateTime TodaysDate => new DateTime(2001, 01, 01, 00, 22, 00);
	}

	[TestFixture]
	sealed class StatisticalClassificationPeriodSnapshotTestParserExpiredOnlyTest : BaseStatisticalClassificationPeriodSnapshotTestParserTest
	{
		protected override string[] StringArgs => new[] { "Some random arg", "--OUTPUT-EXPIRED-DATA-ONLY", "A different random arg" };

		protected override string XMLFileName => "(EXPIRED)AU Customs Tariff Test.xml";

		protected override DateTime TodaysDate => new DateTime(2001, 01, 01, 00, 22, 00);
	}
}
