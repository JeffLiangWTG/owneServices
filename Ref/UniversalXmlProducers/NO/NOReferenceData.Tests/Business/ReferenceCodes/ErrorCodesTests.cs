using System.IO;
using CargoWise.RefDbRepo.NOReferenceData.Services;
using CargoWise.RefDbRepo.NOReferenceData.Services.ErrorCodes;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.ReferenceCodes.Tests
{
	sealed class ErrorCodesTests
	{
		[Test]
		public void TestERRCDOutputXml()
		{
			var xmlData = XmlHelper.ReadDeserializedManifestResourceContent<FeilmeldingListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.ReferenceCodes.Testfiles.Input.feilmelding.xml");
			ErrorCodesParser.ConvertToXmlFile(xmlData, "04/21/2022 23:01:56", "ERRCD", outputTempFileForTest);

			var expectedUniversalXml = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.ReferenceCodes.Testfiles.Output.RefCusCodeListZZ_NO_ERRCD.xml");
			var actualUniversalXml = File.ReadAllText(outputTempFileForTest);
			Assert.That(actualUniversalXml, Is.EqualTo(expectedUniversalXml).NoClip);
		}

		[Test]
		public void TestEmptyCode()
		{
			AssertReferenceCodeError(string.Empty, "VOEC-nummer må oppgis på alle varelinjer");
		}

		[Test]
		public void TestNullCode()
		{
			AssertReferenceCodeError(null, "VOEC-nummer må oppgis på alle varelinjer");
		}

		[Test]
		public void TestEmptyDescription()
		{
			AssertReferenceCodeError("9072", string.Empty);
		}

		[Test]
		public void TestNullDescription()
		{
			AssertReferenceCodeError("9072", null);
		}

		[Test]
		public void TestInvalidModifiedDateTime()
		{
			var errorCode = SetupErrorCodeRecord("9072", "VOEC-nummer må oppgis på alle varelinjer");
			var errors = ErrorCodesParser.ConvertToXmlFile(errorCode, "32/13/2022 00:87:36", "ERRCD", outputTempFileForTest);
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

		void AssertReferenceCodeError(string errNo, string description)
		{
			var expectedErrorMessage = $@"Unable to parse Error Code due to empty code or empty description.
DETAILS:
Code Type: ERRCD
Error No: {errNo}
Description: {description}
";
			var referenceCode = SetupErrorCodeRecord(errNo, description);
			var errors = ErrorCodesParser.ConvertToXmlFile(referenceCode, "05/19/2022 12:21:55", "ERRCD", outputTempFileForTest);
			Assert.That(errors, Is.EqualTo(expectedErrorMessage).NoClip);
		}

		static FeilmeldingListe SetupErrorCodeRecord(string errNo, string description)
		{
			return new FeilmeldingListe()
			{
				ErrorMessage = new Feilmelding[]
				{
					new Feilmelding()
					{
						MessageNumber = errNo,
						MessageText = description
					}
				}
			};
		}
	}
}
