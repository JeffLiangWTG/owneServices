using System;
using System.IO;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using Moq;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests.AHECC
{
	sealed class AHECCPartialParserTest : AHECCParserAbstractTest
	{
		[Test, Timeout(10000),]
		public void TestPartialUpdateRunsOnce()
		{
			using (var stringWriter = new StringWriter())
			using (var stringWriter2 = new StringWriter())
			using (var stringWriter3 = new StringWriter())
			{
				var originalOut = Console.Out;
				try
				{
					Console.SetOut(stringWriter);

					var parser = CreateAHECCPartialParser(new DateTime(2024, 07, 28, 01, 00, 00));
					parser.HttpClientHelper = CreateHttpClientHelper();
					parser.Parse();
					var output = stringWriter.ToString().Trim();
					StringAssert.DoesNotContain("Partial update already ran today", output);
					StringAssert.Contains("AHECC Parser completed.", output);

					Console.SetOut(stringWriter2);

					var parser2 = CreateAHECCPartialParser(new DateTime(2024, 07, 28, 04, 00, 00));
					parser2.HttpClientHelper = CreateHttpClientHelper();
					parser2.Parse();
					var output2 = stringWriter2.ToString().Trim();
					StringAssert.Contains("Partial update already ran today", output2);
					StringAssert.Contains("Skipping partial update.", output2);
					StringAssert.DoesNotContain("AHECC Parser completed.", output2);

					Console.SetOut(stringWriter3);

					var parser3 = CreateAHECCPartialParser(new DateTime(2024, 07, 29, 01, 00, 00));
					parser3.HttpClientHelper = CreateHttpClientHelper();
					parser3.Parse();
					var output3 = stringWriter3.ToString().Trim();
					StringAssert.DoesNotContain("Partial update already ran today", output3);
					StringAssert.Contains("AHECC Parser completed.", output3);
				}
				finally
				{
					Console.SetOut(originalOut);
				}
			}
		}

		protected override string ManifestResourcePathBase => "CargoWise.RefDbRepo.AUReferenceData.Tests.AHECC.ProductionDataTestFiles";

		protected override string ManifestResourceFileName => "ChangeSetPage.html";

		protected override string DownloadFileName => "AHECCSS-P1-EDCHNG-2204230156.txt";

		protected override string ProducedFileName => "AU Export Tariff (Partial).xml";

		protected override string ExpectOutputFileName => "RefCusTariff_AU_Partial.xml";

		protected override BaseAHECCParser AHECCParser => CreateAHECCPartialParser(new DateTime(2024, 07, 28, 21, 38, 46));

		AHECCPartialParser CreateAHECCPartialParser(DateTime currentLocalDateTime)
		{
			var mockDateTimeProvider = new Mock<IDateTimeProvider>();
			mockDateTimeProvider.Setup(x => x.CurrentLocalDateTime).Returns(currentLocalDateTime);
			return new AHECCPartialParser(mockDateTimeProvider.Object);
		}

		public void EraseProcessingData()
		{
			var processingDataJsonFilePath = Path.Combine(ApplicationConfig.OutputPath, "AUCustomsProcessingData", "AUExportTariffPartialProcessingData.json");
			if (File.Exists(processingDataJsonFilePath))
			{
				File.Delete(processingDataJsonFilePath);
			}
		}

		[SetUp]
		public void SetUp()
		{
			EraseProcessingData();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			EraseProcessingData();
		}
	}
}
