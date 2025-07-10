using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using Moq;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.AHECC
{
	sealed class AHECCFullProductionParserTest : AHECCParserAbstractTest
	{
		protected override string ManifestResourcePathBase => "CargoWise.RefDbRepo.AUReferenceData.Tests.AHECC.ProductionDataTestFiles";

		protected override string ManifestResourceFileName => "MainPage.html";

		protected override string DownloadFileName => "AHECCSS-P1-EEMAIN-2204230156.txt";

		protected override string ProducedFileName => "AU Export Tariff (Full).xml";

		protected override string ExpectOutputFileName => "RefCusTariff_AU.xml";

		protected override BaseAHECCParser AHECCParser
		{
			get
			{
				var mockDateTimeProvider = new Mock<IDateTimeProvider>();
				mockDateTimeProvider.Setup(x => x.CurrentLocalDateTime).Returns(new DateTime(2024, 07, 28, 21, 40, 38));
				return new AHECCFullParser(mockDateTimeProvider.Object);
			}
		}
	}
}
