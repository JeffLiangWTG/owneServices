using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists;
using CargoWise.RefDbRepo.CHReferenceData.Business.CodeLists.Schema.PermitItemDetails;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.CHReferenceData.Services.CodeLists;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.CodeLists
{
	[TestFixture]
	class PermitItemDetailsTest : BaseCodeListsTest<permitItemDetails>
	{
		protected override BaseCodeListsParser<permitItemDetails> CreateParser() => new PermitItemDetailsParser(null);

		protected override string ExpectedLogFileName => "CargoWise.RefDbRepo.CHReferenceData.Business-PermitItemDetails.log";

		[Test]
		public void TestDownload()
		{
			using var mockHttp = new MockHttpMessageHandler();
			using var edecDomainsPermitItemDetailsZip = GetType().GetZippedTestStream("TestFiles.Input.edecDomainsPermitItemDetails_1_0.xml");
			mockHttp.When(DownloadUrl).WithUserAgent().Respond("application/zip", edecDomainsPermitItemDetailsZip);
			var client = mockHttp.ToHttpClient();
			var download = DownloadPermitItemDetails.DownloadAndUnzip(client);
			var parser = new PermitItemDetailsParser(download);

			foreach (var mapping in parser.DefaultListTypeMappings)
			{
				using (var expectedTestStream = GetType().GetTestStream($"TestFiles.Output.edecDomainsPermitItemDetails_1_0_{mapping.ListType}_converted.xml"))
				{
					var outputFile = Path.Combine(testOutputDir, nameof(TestDownload) + ".xml");
					File.Delete(parser.LogFilePath);
					parser.ConvertToRefXML(outputFile, new DateTime(2021, 4, 6), mapping);
					using (var converterResultStream = new FileStream(InsertListTypeIntoFilePath(outputFile, mapping.ListType), FileMode.Open))
					{
						var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(converterResultStream));	
						var expectedXml = XDocument.Load(expectedTestStream);
						Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml));
					}
				}
			}

			mockHttp.Dispose();
		}

		[Test]
		public void TestMissingLanguges()
		{
			var download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.DescriptionPlaceholders_WarenartBVET.xml"),
			};

			var outputFile = Path.Combine(testOutputDir, @"PermitItemDetails\TestMissingLanguges-.xml");
			var parser = new PermitItemDetailsParser(download);
			var mapping = parser.DefaultListTypeMappings.Single(m => m.DomainNames[0] == "WarenartBVET");
			parser.ConvertToRefXML(outputFile, actualTestDate, mapping);

			var actualDoc = XDocument.Load(InsertListTypeIntoFilePath(outputFile, mapping.ListType));

			void AssertLanguage(string code, string language, string expected)
			{
				var description = actualDoc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='{code}']/RefCusCodeListLanguage[ZXA_ZX6_NKLanguage='{language}']/ZXA_Description");
				Assert.That(description?.Value, Is.EqualTo(expected), $"Code={code} Language={language}");
			}

			AssertLanguage("10", "FR", "Chaussures");
			AssertLanguage("10", "IT", "Calzature");
			AssertLanguage("10", "EN", "Shoes");
			AssertLanguage("11", "FR", null);
			AssertLanguage("12", "IT", null);
			AssertLanguage("13", "EN", null);
		}

		[Test]
		public void TestLargeValuesTruncated()
		{
			var download = new DownloadResult
			{
				Content = GetType().GetTestArray("TestFiles.Input.LargeValues_WarenartBVET.xml"),
			};

			var parser = new PermitItemDetailsParser(download);
			var outputFile = Path.Combine(testOutputDir, @"PermitItemDetails\TestLargeValuesTruncated.xml");
			var mapping = parser.DefaultListTypeMappings.Single(m => m.DomainNames[0] == "WarenartBVET");
			parser.ConvertToRefXML(outputFile, actualTestDate, mapping);

			var actualDoc = XDocument.Load(InsertListTypeIntoFilePath(outputFile, mapping.ListType));

			string GetDefaultDescription(string code)
			{
				return actualDoc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='{code}']/ZZD_Description")?.Value;
			}

			string GetDescription(string code, string language)
			{
				return actualDoc.XPathSelectElement($@"//RefCusCodeList[ZZD_Code='{code}']/RefCusCodeListLanguage[ZXA_ZX6_NKLanguage='{language}']/ZXA_Description")?.Value;
			}

			Assert.That(GetDefaultDescription("10").EndsWith("+-2000"));
			Assert.That(GetDescription("10", "FR").EndsWith("+-2000"));
			Assert.That(GetDescription("10", "IT").EndsWith("+-2000"));
			Assert.That(GetDescription("10", "EN").EndsWith("+-2000"));
		}

		const string DownloadUrl = "https://edec.douane.swiss/data/edecDomainsPermitItemDetails_1_0.zip";
		readonly DateTime actualTestDate = new DateTime(2021, 4, 6);
	}
}
