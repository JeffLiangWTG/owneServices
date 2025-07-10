using System;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using Moq;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.AHECC
{
	sealed class AHECCFullTestParserTest : AHECCParserAbstractTest
	{
		protected override string ManifestResourcePathBase => "CargoWise.RefDbRepo.AUReferenceData.Tests.AHECC.TestDataTestFiles";

		protected override string ManifestResourceFileName => "MainPage.html";

		protected override string DownloadFileName => "AHECCSS-Q1-EEMAIN-2204230156.txt";

		protected override string ProducedFileName => "AU Export Tariff Test (Full).xml";

		protected override string ExpectOutputFileName => "RefCusTariff_AU.xml";

		protected override BaseAHECCParser AHECCParser
		{
			get
			{
				var mockDateTimeProvider = new Mock<IDateTimeProvider>();
				mockDateTimeProvider.Setup(x => x.CurrentLocalDateTime).Returns(new DateTime(2024, 07, 28, 21, 38, 46));
				return new AHECCFullTestParser(mockDateTimeProvider.Object);
			}
		}
	}
}
