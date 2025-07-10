using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.NOReferenceData.Services;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Avgiftliste;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.Tariff.Tests
{
	sealed class TariffCodeParserTest
	{
		[Test]
		public void TestTariffRefCurRateXml()
		{
			var innfoerselsavgiftData = XmlHelper.ReadDeserializedManifestResourceContent<AvgiftListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.innfoerselsavgift.xml");
			var utfoerselsavgiftData = XmlHelper.ReadDeserializedManifestResourceContent<AvgiftListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.utfoerselsavgift.xml");

			var modified = DateTime.Parse("01/01/2023 00:00:00", CultureInfo.InvariantCulture);
			var errors = TariffCodeParser.ConvertToXMLFile("NO RateCodes", innfoerselsavgiftData, utfoerselsavgiftData, modified, outputTempFileForTest);

			var expectedUniversalXml = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Output.TariffRateCodes_NO.xml");
			var actualUniversalXml = File.ReadAllText(outputTempFileForTest);

			Assert.Multiple(() =>
			{
				Assert.That(errors, Is.Empty);
				Assert.That(actualUniversalXml, Is.EqualTo(expectedUniversalXml).NoClip);
			});
		}

		[Test]
		public void TestRefCusRateCode()
		{
			Assert.Multiple(() =>
			{
				AssertTestRefCusRateCode(true, "12", "ABC Description");
				AssertTestRefCusRateCode(false, "12", string.Empty);
			});
		}

		void AssertTestRefCusRateCode(bool isValid, string feeType, string feeDescription)
		{
			TariffCodeParser.ErrorBuilder.Clear();
			var errMsg = $@"Unable to parse CusRateCode due to empty code, or empty description.
DETAILS:
FeeType    : {feeType}
Description: {feeDescription}
";

			var refCusRateType = TariffCodeParser.ConvertRefCusRateCode(feeType, feeDescription);
			Assert.AreEqual(isValid, (refCusRateType != null));
			if (isValid)
			{
				Assert.That(refCusRateType.ZY1_RateCode, Is.EqualTo(feeType));
				Assert.That(refCusRateType.ZY1_Description, Is.EqualTo(feeDescription));
			}
			else
			{
				Assert.That(refCusRateType, Is.EqualTo(null));
				Assert.That(TariffCodeParser.ErrorBuilder.ToString(), Is.EqualTo(errMsg).NoClip);
			}
		}

		[SetUp]
		public void Setup()
		{
			outputTempFileForTest = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		}

		[TearDown]
		public void TearDown()
		{
			outputTempFileForTest.DeleteTestOutput();
		}
		string outputTempFileForTest;
	}
}
