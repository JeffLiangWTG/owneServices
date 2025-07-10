using System.IO;
using CargoWise.RefDbRepo.NOReferenceData.Services;
using CargoWise.RefDbRepo.NOReferenceData.Services.ReferenceCodes;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.ReferenceCodes.Tests
{
	sealed class ReferenceCodesTests
	{
		[Test]
		public void TestDC44IOutputXml()
		{
			var xmlData = XmlHelper.ReadDeserializedManifestResourceContent<referanserListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.ReferenceCodes.Testfiles.Input.innfoerselsreferanse.xml");
			ReferenceCodesParser.ConvertToXmlFile(xmlData, "04/21/2022 23:01:56", "DC44I", outputTempFileForTest);

			var expectedUniversalXml = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.ReferenceCodes.Testfiles.Output.RefCusCodeListZZ_NO_DC44I.xml");
			var actualUniversalXml = File.ReadAllText(outputTempFileForTest);

			Assert.That(actualUniversalXml, Is.EqualTo(expectedUniversalXml).NoClip);
		}

		[Test]
		public void TestDC44EOutputXml()
		{
			var xmlData = XmlHelper.ReadDeserializedManifestResourceContent<referanserListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.ReferenceCodes.Testfiles.Input.utfoerselsreferanse.xml");
			ReferenceCodesParser.ConvertToXmlFile(xmlData, "05/19/2022 12:21:08", "DC44E", outputTempFileForTest);

			var expectedUniversalXml = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.ReferenceCodes.Testfiles.Output.RefCusCodeListZZ_NO_DC44E.xml");
			var actualUniversalXml = File.ReadAllText(outputTempFileForTest);

			Assert.That(actualUniversalXml, Is.EqualTo(expectedUniversalXml).NoClip);
		}

		[Test]
		public void TestEmptyCode()
		{
			AssertReferenceCodeError(string.Empty, "Avgiftsfri bruk innførsel", "2019-04-01", "2022-02-06");
		}

		[Test]
		public void TestNullCode()
		{
			AssertReferenceCodeError(null, "Avgiftsfri bruk innførsel", "2019-04-01", "2022-02-06");
		}

		[Test]
		public void TestEmptyDescription()
		{
			AssertReferenceCodeError("AFB", string.Empty, "2019-04-01", "2022-02-06");
		}

		[Test]
		public void TestNullDescription()
		{
			AssertReferenceCodeError("AFB", null, "2019-04-01", "2022-02-06");
		}

		[Test]
		public void TestInvalidStartDate()
		{
			AssertReferenceCodeError("AFB", "Avgiftsfri bruk innførsel", "01-04-2019", "2022-02-06");
		}

		[Test]
		public void TestInvalidEndDate()
		{
			AssertReferenceCodeError("AFB", "Avgiftsfri bruk innførsel", "2019-04-01", "2022/02/06");
		}

		[Test]
		public void TestInvalidModifiedDateTime()
		{
			var referenceCode = SetupReferenceCodeRecord("AFB", "Avgiftsfri bruk innførsel", "2019-04-01", "2022-02-06");
			var errors = ReferenceCodesParser.ConvertToXmlFile(referenceCode, "32/13/2022 00:87:36", "DC44I", outputTempFileForTest);
			Assert.That(errors, Does.StartWith("Failed to parse lastUpdated DateTime 32/13/2022 00:87:36"));
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

		void AssertReferenceCodeError(string code, string description, string startDate, string endDate)
		{
			var expectedErrorMessage = $@"Unable to parse Reference code due to empty code, empty description or invalid Dates.
DETAILS:
Code Type: DC44I
Code: {code}
Description: {description}
Start Date: {startDate}
End Date: {endDate}
";
			var referenceCode = SetupReferenceCodeRecord(code, description, startDate, endDate);
			var errors = ReferenceCodesParser.ConvertToXmlFile(referenceCode, "05/19/2022 12:21:55", "DC44I", outputTempFileForTest);
			Assert.That(errors, Is.EqualTo(expectedErrorMessage).NoClip);
		}

		static referanserListe SetupReferenceCodeRecord(string code, string description, string startDate, string endDate)
		{
			return new referanserListe()
			{
				Reference = new Referanse[]
				{
					new Referanse()
					{
						Code = code,
						Description = description,
						DateStart = startDate,
						DateEnd = endDate
					}
				}
			};
		}
	}
}
