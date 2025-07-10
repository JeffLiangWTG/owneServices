using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists.PassarCodeListsSchema;
using CargoWise.RefDbRepo.CHReferenceData.CmdLine;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.CHReferenceData.Services.CodeLists;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.CodeLists
{
	sealed class PassarCodeListsParserTest : BaseCodeListsTest<codeLists>
	{
		protected override BaseCodeListsParser<codeLists> CreateParser() => new PassarCodeListsParser(null);

		protected override string ExpectedLogFileName => "CargoWise.RefDbRepo.CHReferenceData.Business-PassarCodeLists.log";

		[Test]
		public void TestDownload()
		{
			var mockHttp = new MockHttpMessageHandler();
			using var passarCodelistsZip = GetType().GetZippedTestStream("TestFiles.Input.PassarCodelists_v1.xml");
			mockHttp.When(DownloadUrl).WithUserAgent().WithUserAgent().Respond("application/zip", passarCodelistsZip);
			var client = mockHttp.ToHttpClient();
			var download = DownloadPassarCodeLists.DownloadAndUnzip(client);
			var parser = new PassarCodeListsParser(download);

			Assert.Multiple(() =>
			{
				foreach (var mapping in parser.DefaultListTypeMappings)
				{
					var testFileName = $"TestFiles.Output.PassarCodelists_v1_converted_{mapping.ListType}.xml";
					using (var expectedTestStream = GetType().GetTestStream(testFileName))
					{
						var outputFile = Path.Combine(testOutputDir, nameof(TestDownload) + ".xml");
						File.Delete(parser.LogFilePath);
						parser.ConvertToRefXML(outputFile, TestDate, mapping);
						using (var converterResultStream = new FileStream(InsertListTypeIntoFilePath(outputFile, mapping.ListType), FileMode.Open))
						{
							var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(converterResultStream));
							var expectedXml = XDocument.Load(expectedTestStream);
							Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml), testFileName);
						}
					}
				}
			});

			mockHttp.Dispose();
		}

		[Test]
		public void TestParser() => Assert.Multiple(() =>
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.PassarCodelists_v1.xml"),
			};

			var outputFile = Path.Combine(testOutputDir, $"{nameof(TestParser)}.xml");

			var parser = new PassarCodeListsParser(download);
			parser.ConvertToRefXML(outputFile, TestDate);

			foreach (var mapping in parser.DefaultListTypeMappings)
			{
				CompareOutputFileContent(outputFile, "TestFiles.Output.PassarCodelists_v1_converted_.xml", mapping.ListType);
			}
		});

		[TestCase("AI44E")]
		[TestCase("AI44N")]
		[TestCase("AR44N")]
		[TestCase("DC40N")]
		[TestCase("DC40E")]
		[TestCase("DC44E")]
		[TestCase("DC44N")]
		[TestCase("N0231")]
		[TestCase("N0251")]
		[TestCase("N0252")]
		[TestCase("N0296")]
		[TestCase("N1053")]
		[TestCase("N1054")]
		[TestCase("N1057")]
		[TestCase("N1119")]
		[TestCase("N1121")]
		[TestCase("N1123")]
		[TestCase("N1141")]
		[TestCase("N1150")]
		[TestCase("N2000")]
		[TestCase("N3000")]
		[TestCase("N5001")]
		[TestCase("N5002")]
		[TestCase("N5003")]
		[TestCase("N5004")]
		[TestCase("N5005")]
		[TestCase("N5010")]
		[TestCase("PRMAP")]
		[TestCase("RFNDT")]
		[TestCase("TD44N")]
		public void TestDomainListTypeMapping(string listType)
		{
			const int totalNumberOfLists = 30;

			var mappings = new PassarCodeListsParser(null).DefaultListTypeMappings;

			Assert.Multiple(() =>
			{
				Assert.That(mappings.Count(), Is.EqualTo(totalNumberOfLists), "DefaultListTypeMappings Count");
				Assert.IsTrue(mappings.Any(mapping => mapping.ListType == listType), $@"Mapping exists for ListType ""{listType}""");
			});
		}

		[Test]
		public void TestOriginDocument()
		{
			DownloadResult download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.Passar_NCL1213.xml"),
			};

			var outputFile = Path.Combine(testOutputDir, nameof(TestOriginDocument) + ".xml");

			var parser = new PassarCodeListsParser(download);
			parser.ConvertToRefXML(outputFile, TestDate);
			outputFile = InsertListTypeIntoFilePath(outputFile, "DC44E");
			try
			{
				var doc = XDocument.Load(outputFile);
				Assert.Multiple(() =>
				{
					AssertAttributeValue(doc, "9541", "OriginDocument", "Y");
					AssertAttributeValue(doc, "9542", "OriginDocument", "Y");
					AssertAttributeValue(doc, "9543", "OriginDocument", "Y");
					AssertAttributeValue(doc, "9544", "OriginDocument", "Y");
					AssertAttributeValue(doc, "9545", "OriginDocument", null);
				});
			}
			finally
			{
				DeleteTestOutputFile(outputFile);
			}

		}

		void AssertAttributeValue(XDocument doc, string code, string name, string value)
		{
			Assert.That(doc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='{code}']/RefCusCodeListAttribute[ZZE_ZXE_NKName='{name}']/ZZE_Value")?.Value, Is.EqualTo(value), $"Code={code} {name}='{value}'");
		}

		const string DownloadUrl = "https://datahub.bazg.admin.ch/PassarCodelists_v1.zip";

		DateTime TestDate => new DateTime(2024, 4, 1);
	}
}
