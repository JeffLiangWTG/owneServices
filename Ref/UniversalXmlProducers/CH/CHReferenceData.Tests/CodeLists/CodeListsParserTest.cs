using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists.DomainSchema;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.CHReferenceData.Services.CodeLists;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.CodeLists
{
	[TestFixture]
	class CodeListsParserTest : BaseCodeListsTest<domains>
	{
		[Test]
		public void TestDownloadCodeListsAndConvert()
		{
			var mockHttp = new MockHttpMessageHandler();
			using var edecDomainsZip = GetType().GetZippedTestStream("TestFiles.Input.edecDomains_1_0.xml");
			mockHttp.When(DownloadUrl).WithUserAgent().Respond("application/xml", edecDomainsZip);

			var client = mockHttp.ToHttpClient();
			var download = DownloadCodeLists.DownloadAndUnzip(client);

			var outputFilePath = Path.Combine(testOutputDir, "TestDownloadCodeListsAndConvert.xml");
			var parser = new CodeListsParser(download);
			var mapping = parser.DefaultListTypeMappings.Single(m => m.DomainNames[0] == "methodOfPayment");
			parser.ConvertToRefXML(outputFilePath, new DateTime(2021, 4, 7), mapping);

			CompareOutputFileContent(outputFilePath, "TestFiles.Output.edecDomains_1_0_converted_.xml", mapping.ListType);
			mockHttp.Dispose();
		}

		[TestCase("MOP")]
		[TestCase("PRMAU")]
		[TestCase("DC40I")]
		[TestCase("VEHMC")]
		[TestCase("DC44I")]
		[TestCase("NCLT")]
		[TestCase("ECFLD")]
		[TestCase("GIDNM")]
		public void TestDomainListTypeMapping(string listType)
		{
			var mappings = new CodeListsParser(null).DefaultListTypeMappings;
			Assert.That(mappings.Count, Is.EqualTo(8));
			Assert.IsTrue(mappings.Any(mapping => mapping.ListType == listType), $@"Mapping exists for ListType ""{listType}""");
		}

		[Test]
		public void TestCodeListsParser()
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.edecDomains_1_0.xml"),
			};

			var outputFile = Path.Combine(testOutputDir, "TestCodeListsParser.xml");

			var parser = new CodeListsParser(download);
			parser.ConvertToRefXML(outputFile, new DateTime(2021, 4, 7));

			Assert.Multiple(() =>
			{
				foreach (var mapping in parser.DefaultListTypeMappings)
				{
					CompareOutputFileContent(outputFile, "TestFiles.Output.edecDomains_1_0_converted_.xml", mapping.ListType);
				}
			});
		}

		[Test]
		public void TestDuplicateCodes()
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.duplicateCodes_TESTDOM.xml"),
			};

			var outputFile = Path.Combine(testOutputDir, nameof(TestDuplicateCodes) + ".xml");

			var parser = new CodeListsParser(download);
			var mapping = new MappingConfig() { DomainNames = new[] { "TESTDOM" }, OnlyForImport = false, ListType = "TST" };
			parser.ConvertToRefXML(outputFile, new DateTime(2021, 4, 7), mapping);
			outputFile = InsertListTypeIntoFilePath(outputFile, mapping.ListType);
			try
			{
				var actualDoc = XDocument.Load(outputFile);
				string getActualDescription(string code)
				{
					return (string)actualDoc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='{code}']/ZZD_Description");
				}

				Assert.AreEqual("expected", getActualDescription("TEST01"), "TEST01");
				Assert.AreEqual("expected", getActualDescription("TEST02"), "TEST02");
				Assert.AreEqual("expected", getActualDescription("TEST03"), "TEST03");

				Assert.AreEqual(DateTime.Parse("2020-01-01T00:00:00", CultureInfo.InvariantCulture), getActualDate(actualDoc, "TEST01", "ZZD_StartDate"), "ZZD_StartDate");
				Assert.AreEqual(DateTime.Parse("2021-12-31T23:59:00", CultureInfo.InvariantCulture), getActualDate(actualDoc, "TEST01", "ZZD_EndDate"), "ZZD_EndDate");
			}
			finally
			{
				DeleteTestOutputFile(outputFile);
			}
		}

		[Test]
		public void TestLowerCaseCodesTranslatedToUpperCase()
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.lowerCaseCodes_TESTDOM.xml"),
			};

			var outputFile = Path.Combine(testOutputDir, nameof(TestLowerCaseCodesTranslatedToUpperCase) + ".xml");

			var parser = new CodeListsParser(download);
			var mapping = new MappingConfig() { DomainNames = new[] { "TESTDOM" }, OnlyForImport = false, ListType = "TST" };
			parser.ConvertToRefXML(outputFile, new DateTime(2021, 4, 7), mapping);
			outputFile = InsertListTypeIntoFilePath(outputFile, mapping.ListType);
			try
			{
				var actualDoc = XDocument.Load(outputFile);
				Assert.IsNotNull(actualDoc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='TEST01']"), "Upper case code should exist");
				Assert.IsNull(actualDoc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='test01']"), "Lower case code shouldn't");
			}
			finally
			{
				DeleteTestOutputFile(outputFile);
			}
		}

		[Test]
		public void TestSkipUselessDescriptions()
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.uselessDescriptions_TESTDOM.xml"),
			};

			var outputFile = Path.Combine(testOutputDir, nameof(TestDuplicateCodes) + ".xml");

			var parser = new CodeListsParser(download);
			var mapping = new MappingConfig() { DomainNames = new[] { "TESTDOM" }, OnlyForImport = false, ListType = "TST" };
			parser.ConvertToRefXML(outputFile, new DateTime(2021, 4, 7), mapping);
			outputFile = InsertListTypeIntoFilePath(outputFile, mapping.ListType);
			try
			{
				var actualDoc = XDocument.Load(outputFile);
				(XElement frDescription, XElement itDescription, XElement enDescription) getActualDescriptions(string code)
				{
					var fr = actualDoc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='{code}']/RefCusCodeListLanguage[ZXA_ZX6_NKLanguage='FR']");
					var it = actualDoc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='{code}']/RefCusCodeListLanguage[ZXA_ZX6_NKLanguage='IT']");
					var en = actualDoc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='{code}']/RefCusCodeListLanguage[ZXA_ZX6_NKLanguage='EN']");

					return (fr, it, en);
				}
				var descriptions = getActualDescriptions("DescriptionTooShort");
				Assert.IsNull(descriptions.frDescription, "frDescription");
				Assert.IsNull(descriptions.itDescription, "itDescription");
				Assert.IsNull(descriptions.enDescription, "enDescription");

				descriptions = getActualDescriptions("Description2LetterPrefix");
				Assert.IsNull(descriptions.frDescription, "frDescription");
				Assert.IsNull(descriptions.itDescription, "itDescription");
				Assert.IsNull(descriptions.enDescription, "enDescription");

				descriptions = getActualDescriptions("Description3LetterPrefix");
				Assert.IsNull(descriptions.frDescription, "frDescription");
				Assert.IsNull(descriptions.itDescription, "itDescription");
				Assert.IsNull(descriptions.enDescription, "enDescription");
			}
			finally
			{
				DeleteTestOutputFile(outputFile);
			}
		}

		[Test]
		public void TestValidityExceedsDateLimits()
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.duplicateCodes_TESTDOM.xml"),
			};

			var outputFile = Path.Combine(testOutputDir, nameof(TestValidityExceedsDateLimits) + ".xml");

			var parser = new CodeListsParser(download);
			var mapping = new MappingConfig() { DomainNames = new[] { "TESTDOM" }, OnlyForImport = false, ListType = "TST" };
			parser.ConvertToRefXML(outputFile, new DateTime(2021, 4, 7), mapping);
			outputFile = InsertListTypeIntoFilePath(outputFile, mapping.ListType);
			try
			{
				var actualDoc = XDocument.Load(outputFile);
				Assert.AreEqual(new DateTime(1900, 01, 01, 0, 0, 0), getActualDate(actualDoc, "VALID-FROM-BELOW-MIN", "ZZD_StartDate"));
				Assert.AreEqual(new DateTime(2079, 06, 06, 0, 0, 0), getActualDate(actualDoc, "VALID-FROM-ABOVE-MAX", "ZZD_StartDate"));
				Assert.AreEqual(new DateTime(1900, 01, 01, 23, 59, 0), getActualDate(actualDoc, "VALID-TO-BELOW-MIN", "ZZD_EndDate"));
				Assert.AreEqual(new DateTime(2079, 06, 06, 23, 59, 0), getActualDate(actualDoc, "VALID-TO-ABOVE-MAX", "ZZD_EndDate"));
				Assert.AreEqual(new DateTime(1900, 01, 02, 0, 0, 0), getActualDate(actualDoc, "VALID-FROM-NEAR-MIN", "ZZD_StartDate"));
				Assert.AreEqual(new DateTime(2079, 06, 05, 0, 0, 0), getActualDate(actualDoc, "VALID-FROM-NEAR-MAX", "ZZD_StartDate"));
				Assert.AreEqual(new DateTime(1900, 01, 02, 23, 59, 0), getActualDate(actualDoc, "VALID-TO-NEAR-MIN", "ZZD_EndDate"));
				Assert.AreEqual(new DateTime(2079, 06, 05, 23, 59, 0), getActualDate(actualDoc, "VALID-TO-NEAR-MAX", "ZZD_EndDate"));
			}
			finally
			{
				DeleteTestOutputFile(outputFile);
			}
		}

		[Test]
		public void TestCodeTypeIsConstant()
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.duplicateCodes_TESTDOM.xml"),
			};

			var outputFile = Path.Combine(testOutputDir, nameof(TestCodeTypeIsConstant) + ".xml");

			var parser = new CodeListsParser(download);
			var mapping = new MappingConfig() { DomainNames = new[] { "TESTDOM" }, OnlyForImport = false, ListType = "TST" };
			parser.ConvertToRefXML(outputFile, new DateTime(2021, 4, 7), mapping);
			outputFile = InsertListTypeIntoFilePath(outputFile, mapping.ListType);
			try
			{
				var actualDoc = XDocument.Load(outputFile);
				Assert.AreEqual("TST", (string)actualDoc.XPathSelectElement($@"//Schema/EntityType[@Name='RefCusCodeList']/Property[@Name='ZZD_ZZK_NKCodeType']")?.Attribute("ConstantValue")?.Value, "ZZD_ZZK_NKCodeType");
				Assert.IsNull(actualDoc.XPathSelectElement($@"//RefCusCodeList/ZZD_ZZK_NKCodeType"), "ZZD_ZZK_NKCodeType ConstantValue");
			}
			finally
			{
				DeleteTestOutputFile(outputFile);
			}
		}

		[Test]
		public void TestFileCreatedOnlyOnChange()
		{
			var mapping = new MappingConfig() { DomainNames = new[] { "TESTDOM" }, ListType = "TST" };
			var baseOutputFile = Path.Combine(testOutputDir, nameof(TestFileCreatedOnlyOnChange) + ".xml");
			var listOutputFile = InsertListTypeIntoFilePath(baseOutputFile, mapping.ListType);

			string runParser(string input, bool delete)
			{
				File.Delete(listOutputFile);

				DownloadResult download = new DownloadResult
				{
					Content = GetType().GetTestArray(input),
				};

				var parser = new CodeListsParser(download);
				if (delete)
				{
					File.Delete(parser.LogFilePath);
				}
				parser.ConvertToRefXML(baseOutputFile, new DateTime(2021, 4, 7), mapping);
				return parser.LogFilePath;
			}

			var logFilePath = runParser("TestFiles.Input.createdDate-20210401.xml", true);
			Assert.IsTrue(File.Exists(listOutputFile), "Output should have been created if there was no log file");
			Assert.IsTrue(File.Exists(logFilePath), "Log file should have been created");

			runParser("TestFiles.Input.createdDate-20210401.xml", false);
			Assert.IsFalse(File.Exists(listOutputFile), "No output should have been created if the same input is processed");

			runParser("TestFiles.Input.createdDate-20210402.xml", false);
			Assert.IsTrue(File.Exists(listOutputFile), "New output should have been created if a new input is processed");
		}

		[Test]
		public void TestEComplaintFields()
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.eComplaintCodes.xml"),
			};

			var outputFile = Path.Combine(testOutputDir, nameof(TestEComplaintFields) + ".xml");

			var parser = new CodeListsParser(download);
			parser.ConvertToRefXML(outputFile, new DateTime(2021, 4, 7));
			outputFile = InsertListTypeIntoFilePath(outputFile, "ECFLD");
			try
			{
				var actualDoc = XDocument.Load(outputFile);
				Assert.Multiple(() =>
				{
					Assert.IsNotNull(actualDoc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='sampleHeaderField']"), "Mixed-case header field code should exist");
					Assert.IsNotNull(actualDoc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='samplePositionField']"), "Mixed-case position field code should exist");
					Assert.IsNotNull(actualDoc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='sampleField']"), "Fields are not grouped together (lower)");
					Assert.IsNotNull(actualDoc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='SampleField']"), "Fields are not grouped together (upper)");
				});
			}
			finally
			{
				DeleteTestOutputFile(outputFile);
			}
		}

		[Test]
		public void TestImportYearsOfCodes()
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.importYears_TESTDOM.xml"),
			};

			var outputFile = Path.Combine(testOutputDir, nameof(TestImportYearsOfCodes) + ".xml");

			var parser = new CodeListsParser(download);
			var mapping = new MappingConfig() { DomainNames = new[] { "TESTDOM" }, OnlyForImport = false, ListType = "TST", YearsToImport = 10 };
			parser.ConvertToRefXML(outputFile, new DateTime(2021, 4, 7), mapping);
			outputFile = InsertListTypeIntoFilePath(outputFile, mapping.ListType);
			try
			{
				var actualDoc = XDocument.Load(outputFile);
				string getActualDescription(string code)
				{
					return (string)actualDoc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='{code}']/ZZD_Description");
				}

				Assert.IsNull(getActualDescription("TEST02"), "TEST02");
				Assert.IsNull(getActualDescription("TEST05"), "TEST05");

				Assert.AreEqual("expected", getActualDescription("TEST01"), "TEST01");
				Assert.AreEqual("expected", getActualDescription("TEST03"), "TEST03");
				Assert.AreEqual("expected", getActualDescription("TEST04"), "TEST04");
				Assert.AreEqual("expected", getActualDescription("TEST06"), "TEST06");
				Assert.AreEqual("duplicate", getActualDescription("TEST07"), "TEST07");
				Assert.AreEqual("duplicate", getActualDescription("TEST08"), "TEST08");

				Assert.AreEqual(DateTime.Parse("2000-01-01T00:00:00", CultureInfo.InvariantCulture), getActualDate(actualDoc, "TEST01", "ZZD_StartDate"), "ZZD_StartDate");
				Assert.AreEqual(DateTime.Parse("2079-06-06T23:59:00", CultureInfo.InvariantCulture), getActualDate(actualDoc, "TEST01", "ZZD_EndDate"), "ZZD_EndDate");
				Assert.AreEqual(DateTime.Parse("2000-01-01T00:00:00", CultureInfo.InvariantCulture), getActualDate(actualDoc, "TEST03", "ZZD_StartDate"), "ZZD_StartDate");
				Assert.AreEqual(DateTime.Parse("2011-04-07T23:59:00", CultureInfo.InvariantCulture), getActualDate(actualDoc, "TEST03", "ZZD_EndDate"), "ZZD_EndDate");
				Assert.AreEqual(DateTime.Parse("2021-04-07T00:00:00", CultureInfo.InvariantCulture), getActualDate(actualDoc, "TEST04", "ZZD_StartDate"), "ZZD_StartDate");
				Assert.AreEqual(DateTime.Parse("2025-12-31T23:59:00", CultureInfo.InvariantCulture), getActualDate(actualDoc, "TEST04", "ZZD_EndDate"), "ZZD_EndDate");
				Assert.AreEqual(DateTime.Parse("2015-01-01T00:00:00", CultureInfo.InvariantCulture), getActualDate(actualDoc, "TEST06", "ZZD_StartDate"), "ZZD_StartDate");
				Assert.AreEqual(DateTime.Parse("2015-12-31T23:59:00", CultureInfo.InvariantCulture), getActualDate(actualDoc, "TEST06", "ZZD_EndDate"), "ZZD_EndDate");
				Assert.AreEqual(DateTime.Parse("2015-01-01T00:00:00", CultureInfo.InvariantCulture), getActualDate(actualDoc, "TEST07", "ZZD_StartDate"), "ZZD_StartDate");
				Assert.AreEqual(DateTime.Parse("2017-12-31T23:59:00", CultureInfo.InvariantCulture), getActualDate(actualDoc, "TEST07", "ZZD_EndDate"), "ZZD_EndDate");
				Assert.AreEqual(DateTime.Parse("2015-01-01T00:00:00", CultureInfo.InvariantCulture), getActualDate(actualDoc, "TEST08", "ZZD_StartDate"), "ZZD_StartDate");
				Assert.AreEqual(DateTime.Parse("2015-12-31T23:59:00", CultureInfo.InvariantCulture), getActualDate(actualDoc, "TEST08", "ZZD_EndDate"), "ZZD_EndDate");
			}
			finally
			{
				DeleteTestOutputFile(outputFile);
			}
		}

		protected override BaseCodeListsParser<domains> CreateParser() => new CodeListsParser(null);

		protected override string ExpectedLogFileName => "CargoWise.RefDbRepo.CHReferenceData.Business-CodeLists.log";

		DateTime getActualDate(XDocument actualDoc, string code, string fieldName)
		{
			var dateValue = (string)actualDoc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='{code}']/{fieldName}");
			return DateTime.Parse(dateValue, CultureInfo.InvariantCulture);
		}

		const string DownloadUrl = "https://edec.douane.swiss/data/edecDomains_1_0.zip";
	}
}
