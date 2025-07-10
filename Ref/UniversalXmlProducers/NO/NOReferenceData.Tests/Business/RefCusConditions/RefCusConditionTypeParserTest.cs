using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.NOReferenceData.Services;
using CargoWise.RefDbRepo.NOReferenceData.Services.RefCusConditions;
using CargoWise.RefDbRepo.NOReferenceData.Services.Tariff.Avgiftliste;
using CargoWise.RefDbRepo.NOReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.RefCusConditions.Tests
{
	sealed class RefCusConditionTypeParserTest
	{
		[Test]
		public void TestCusConditionsXml()
		{
			var modified = DateTime.Parse("01/01/2023 00:00:00", CultureInfo.InvariantCulture);
			var errors = RefCusConditionTypeParser.ConvertToXMLFile("NO RefCusConditionType", refCusConditions, innfoerselsavgiftData, modified, outputTempFileForTest);

			var expectedUniversalXml = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.RefCusConditions.Testfiles.Output.RefCusConditionType_NO.xml");
			var actualUniversalXml = File.ReadAllText(outputTempFileForTest);

			Assert.Multiple(() =>
			{
				Assert.That(errors, Is.EqualTo(string.Empty));
				Assert.That(actualUniversalXml, Is.EqualTo(expectedUniversalXml).NoClip);
			});
		}

		[Test]
		public void TestCusConditionsXmlWhenNull()
		{
			refCusConditions = new RefCusCodeConditionItems();

			var modified = DateTime.Parse("01/01/2023 00:00:00", CultureInfo.InvariantCulture);
			var errors = RefCusConditionTypeParser.ConvertToXMLFile("NO RefCusConditionType", refCusConditions, innfoerselsavgiftData, modified, outputTempFileForTest);

			var expectedUniversalXml = EmbeddedResourceHelper.ReadManifestResourceContent("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.RefCusConditions.Testfiles.Output.RefCusConditionType_NO.xml");
			Assert.Multiple(() =>
			{
				Assert.That(errors, Is.EqualTo($"Failed to parse CusConditionTypeParser, no conditions in file{Environment.NewLine}"));
			});
		}

		[SetUp]
		public void Setup()
		{
			refCusConditions = CusConditionCodes.GetCusConditionData();
			innfoerselsavgiftData = XmlHelper.ReadDeserializedManifestResourceContent<AvgiftListe>("CargoWise.RefDbRepo.NOReferenceData.Tests.Business.Tariff.Testfiles.Input.innfoerselsavgift.xml");
			outputTempFileForTest = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		}

		[TearDown]
		public void TearDown()
		{
			outputTempFileForTest.DeleteTestOutput();
		}
		string outputTempFileForTest;
		AvgiftListe innfoerselsavgiftData;
		RefCusCodeConditionItems refCusConditions;
	}
}
