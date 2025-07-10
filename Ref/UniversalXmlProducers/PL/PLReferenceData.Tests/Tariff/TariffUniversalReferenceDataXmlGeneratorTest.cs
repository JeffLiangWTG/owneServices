using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.PLReferenceData.Business.Helpers;
using CargoWise.RefDbRepo.PLReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.PLReferenceData.Services.Interfaces;
using Moq;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.PLReferenceData.Tests.Tariff
{
	[TestFixture]
	sealed class TariffUniversalReferenceDataXmlGeneratorTest
	{
		const string testFilesPrefix = "CargoWise.RefDbRepo.PLReferenceData.Tests.Tariff.TestFiles.";

		[Test]
		public void TestMergeImportTariffsWithEUNTariffs()
		{
			var (plTariffListForTest, eunTariffListForTest) = SetupTestTariffLists();

			var expected = new List<RefCusTariff>()
			{
				new() { ZZ1_TariffCode = "01012100", ZZ1_Description = "Test Description" },
				new() { ZZ1_TariffCode = "03011100", ZZ1_Description = "Test Description" },
				new() { ZZ1_TariffCode = "04011010", ZZ1_Description = "Test Description" },
				new() { ZZ1_TariffCode = "05010000", ZZ1_Description = "Test Description" },
				new() { ZZ1_TariffCode = "06011010", ZZ1_Description = "Test Description" },
				new() { ZZ1_TariffCode = "07019010", ZZ1_Description = "Test Description" },
				new() { ZZ1_TariffCode = "07019020", ZZ1_Description = "Test Description" }
			};

			var expander = new UniversalXMLProducers.EUNTariffDataProducer.EUNTariffExpander(null, eunTariffListForTest);
			var result  = expander.MergeExportTariffsWithEUNTarrifs(plTariffListForTest);

			Assert.AreEqual(expected.Count, result.Count);

			foreach (var item in expected)
			{
				var whereResult = result.Where(x => x.ZZ1_TariffCode == item.ZZ1_TariffCode).ToList();
				Assert.AreEqual(1, whereResult.Count);
			}
		}

		[Test]
		public void TestMergeImportTariffsWithEUNTariffsFailing()
		{
			var (plTariffListForTest, eunTariffListForTest) = SetupTestTariffLists();

			var expectedToFailData = new List<RefCusTariff>()
			{
				new() { ZZ1_TariffCode = "01012100", ZZ1_Description = "Test Description" },
				new() { ZZ1_TariffCode = "03011100", ZZ1_Description = "Test Description" },
				new() { ZZ1_TariffCode = "04011010", ZZ1_Description = "Test Description" },
				new() { ZZ1_TariffCode = "05010000", ZZ1_Description = "Test Description" },
				new() { ZZ1_TariffCode = "06011010", ZZ1_Description = "Test Description" },
				new() { ZZ1_TariffCode = "07019010", ZZ1_Description = "Test Description" },
				new() { ZZ1_TariffCode = "07019020", ZZ1_Description = "Test Description" },
				new() { ZZ1_TariffCode = "13012000", ZZ1_Description = "Test Description" },
				new() { ZZ1_TariffCode = "24011035", ZZ1_Description = "Test Description" }
			};

			var expander = new UniversalXMLProducers.EUNTariffDataProducer.EUNTariffExpander(null, eunTariffListForTest);
			var result = expander.MergeExportTariffsWithEUNTarrifs(plTariffListForTest);

			Assert.AreNotEqual(expectedToFailData.Count, result.Count);
		}

		(List<RefCusTariff>, List<RefCusTariff>) SetupTestTariffLists()
		{
			var plTariffListForTest = new List<RefCusTariff>();
			var eunTariffListForTest = new List<RefCusTariff>();

			// treat as exists in PL Data
			plTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "01010000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "01012100", ZZ1_Description = "Test Description" });

			plTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "03010000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "03011100", ZZ1_Description = "Test Description" });

			plTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "04010000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "04011010", ZZ1_Description = "Test Description" });

			plTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "05010000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "05010000", ZZ1_Description = "Test Description" });

			plTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "06010000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "06011010", ZZ1_Description = "Test Description" });

			plTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07000000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07019010", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "07019020", ZZ1_Description = "Test Description" });

			// treat as not exists in PL Data
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "13012000", ZZ1_Description = "Test Description" });
			eunTariffListForTest.Add(new RefCusTariff() { ZZ1_TariffCode = "24011035", ZZ1_Description = "Test Description" });

			return (plTariffListForTest, eunTariffListForTest);
		}

		[Test]
		public void TestGenerateUniversalTariff_DataFromTaric4()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);

			var rawInputData = XDocument.Load(TestHelper.GetManifestResourceStream($"{testFilesPrefix}Input.base.xml"));
			var parsedInputData = XmlParser.Deserialize<IsztarHistoryResponse>(rawInputData);

			var dateTimeProviderMock = Mock.Of<IDateTimeProvider>(x => x.CurrentLocalDateTime == new DateTime(2021, 1, 1));

			var resultData = PLTariffExtractor.GeneratePLTariffData(parsedInputData, dateTimeProviderMock);
			TariffUniversalReferenceDataXmlGenerator.GenerateReferenceDataXml(new DateTime(2010, 11, 11), resultData, tempOutput);

			var expected = XDocument.Load(TestHelper.GetManifestResourceStream($"{testFilesPrefix}Output.ExpectedData.xml"));
			var result = XDocument.Load(tempOutput);

			Assert.AreEqual(expected.ToString(), result.ToString());
		}

		[Test]
		public void TestGenerateUniversalTariff_RefCusRateWithRefCusCondition()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
			var dateTimeProviderMock = Mock.Of<IDateTimeProvider>(x => x.CurrentLocalDateTime == new DateTime(2021, 1, 1));
			var rawInputData = XDocument.Load(TestHelper.GetManifestResourceStream($"{testFilesPrefix}Input.singleRefCusRateWithRefCusCondition.xml"));
			var parsedInputData = XmlParser.Deserialize<IsztarHistoryResponse>(rawInputData);

			var resultData = PLTariffExtractor.GeneratePLTariffData(parsedInputData, dateTimeProviderMock);
			TariffUniversalReferenceDataXmlGenerator.GenerateReferenceDataXml(new DateTime(2010, 11, 11), resultData, tempOutput);

			var expected = XDocument.Load(TestHelper.GetManifestResourceStream($"{testFilesPrefix}Output.TestGenerateRefCusRateWithRefCusCondition.xml"));
			var result = XDocument.Load(tempOutput);

			Assert.AreEqual(expected.ToString(), result.ToString());
		}

		[Test]
		public void TestGenerateUniversalTariff_RefCusRateWithoutRefCusCondition()
		{
			CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
			var dateTimeProviderMock = Mock.Of<IDateTimeProvider>(x => x.CurrentLocalDateTime == new DateTime(2021, 1, 1));
			var rawInputData = XDocument.Load(TestHelper.GetManifestResourceStream($"{testFilesPrefix}Input.singleRefCusRateWithoutRefCusCondition.xml"));
			var parsedInputData = XmlParser.Deserialize<IsztarHistoryResponse>(rawInputData);

			var resultData = PLTariffExtractor.GeneratePLTariffData(parsedInputData, dateTimeProviderMock);
			TariffUniversalReferenceDataXmlGenerator.GenerateReferenceDataXml(new DateTime(2010, 11, 11), resultData, tempOutput);

			var expected = XDocument.Load(TestHelper.GetManifestResourceStream($"{testFilesPrefix}Output.TestGenerateRefCusRateWithoutRefCusCondition.xml"));
			var result = XDocument.Load(tempOutput);

			Assert.AreEqual(expected.ToString(), result.ToString());
		}

		[SetUp]
		public void SetUp()
		{
			tempOutput = Path.GetTempFileName();
		}

		[TearDown]
		public void TearDown()
		{
			if (File.Exists(tempOutput))
			{
				File.Delete(tempOutput);
			}
		}

		string tempOutput;
	}
}
