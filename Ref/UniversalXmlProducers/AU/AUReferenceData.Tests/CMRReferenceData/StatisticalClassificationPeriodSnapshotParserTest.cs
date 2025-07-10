using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.CMRReferenceData
{
	abstract class BaseStatisticalClassificationPeriodSnapshotParserTest : CMRTariffDataParserAbstractTest
	{
		protected override string TestFileFolderName => "StatisticalClassificationPeriodSnapshot";

		protected override string TextFileName => "STCPSNAP-P1-EDMAIN-2208050143.txt";

		protected override ITariffDataParser Parser
		{
			get
			{
				var mockDateTimeProvider = new Mock<IDateTimeProvider>();
				mockDateTimeProvider.Setup(x => x.CurrentLocalDateTime).Returns(TodaysDate);

				var mockParser = new Mock<StatisticalClassificationPeriodSnapshotParser>(mockDateTimeProvider.Object) { CallBase = true };
				mockParser.Setup(x => x.TariffsSourceFilePath).Returns(TestTariffsSourceFilePath);
				mockParser.Setup(x => x.STCPCharacteristicCodes).Returns(STCPCharacteristicCodes);
				mockParser.Setup(x => x.TRFCCharacteristicCodes).Returns(TRFCCharacteristicCodes);
				mockParser.Setup(x => x.AQISCommodityStatisticalClassificationCodes).Returns(AQISCommodityStatisticalClassificationCodes);

				return mockParser.Object;
			}
		}
	}

	[TestFixture]
	sealed class StatisticalClassificationPeriodSnapshotParserTest : BaseStatisticalClassificationPeriodSnapshotParserTest
	{
		protected override string[] StringArgs => new string[] { };

		protected override string XMLFileName => "AU Customs Tariff.xml";

		protected override DateTime TodaysDate => new DateTime(2002, 08, 05, 01, 43, 00);
	}

	[TestFixture]
	sealed class StatisticalClassificationPeriodSnapshotParserExpiredOnlyTest : BaseStatisticalClassificationPeriodSnapshotParserTest
	{
		protected override string[] StringArgs => new[] { "Some other arg", "--OUTPUT-EXPIRED-DATA-ONLY", "Another random arg" };

		protected override string XMLFileName => "(EXPIRED)AU Customs Tariff.xml";

		protected override DateTime TodaysDate => new DateTime(2002, 08, 05, 01, 43, 00);
	}
}
