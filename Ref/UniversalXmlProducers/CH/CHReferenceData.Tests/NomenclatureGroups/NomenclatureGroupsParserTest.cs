using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.RefDbRepo.CHReferenceData.Business.NomenclatureGroups;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.CHReferenceData.Services.Tariffs;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.NomenclatureGroups
{
	[TestFixture]
	class NomenclatureGroupsParserTest
	{
		[Test]
		public void TestDownloadNomenclatureGroupsAndConvert()
		{
			using (var expectedTestStream = classType.GetUnzippedTestStream("TestFiles.Output.Tarifstruktur_converted.zip"))
			using (var mockHttp = new MockHttpMessageHandler())
			{
				mockHttp.When(DownloadUrl).WithUserAgent().Respond("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", classType.GetTestStream("TestFiles.Input.Tarifstruktur.xlsx"));

				var client = mockHttp.ToHttpClient();
				var download = DownloadTariffStructure.Download(client);

				var parser = new NomenclatureGroupsParser(download);
				using (var outputFile = new TemporaryOutputFile(@"NomenclatureGroups\TestDownloadNomenclatureGroupsAndConvert.xml"))
				{
					parser.ConvertToRefXML(outputFile.FullPath, testFileTarifstrukturPublishedDate);

					using (var converterResultStream = new FileStream(outputFile.FullPath, FileMode.Open))
					{
						var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(converterResultStream));
						var expectedXml = XDocument.Load(expectedTestStream);
						Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml));
					}
				}
			}
		}

		[Test]
		public void TestNomenclatureGroupsParser()
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.Tarifstruktur.xlsx"),
			};

			using (var outputFile = new TemporaryOutputFile(@"NomenclatureGroups\TestNomenclatureGroupsParser.xml"))
			{
				new NomenclatureGroupsParser(download).ConvertToRefXML(outputFile.FullPath, testFileTarifstrukturPublishedDate);

				using (var actualStream = new FileStream(outputFile.FullPath, FileMode.Open))
				using (var expectedStream = classType.GetUnzippedTestStream("TestFiles.Output.Tarifstruktur_converted.zip"))
				{
					var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));;
					var expectedXml = XDocument.Load(expectedStream);
					Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml));
				}
			}
		}

		[Test]
		public void TestNomenclatureKeysAreUnique()
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.Tarifstruktur.xlsx"),
			};

			using (var outputFile = new TemporaryOutputFile(@"NomenclatureGroups\TestNomenclatureGroupsParser.xml"))
			{
				new NomenclatureGroupsParser(download).ConvertToRefXML(outputFile.FullPath, testFileTarifstrukturPublishedDate);

				using (var actualStream = new FileStream(outputFile.FullPath, FileMode.Open))
				{
					var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));
					var keys = actualXml.XPathSelectElements("//ZZ5_CompositeKey").Select(x => x.Value);
					Assert.That(keys, Is.Unique);
				}
			}
		}

		Type classType => GetType();
		readonly DateTime testFileTarifstrukturPublishedDate = new DateTime(2021, 4, 15);

		const string DownloadUrl = "https://www.ezv.admin.ch/dam/ezv/de/dokumente/archiv/a5/tares_datenlieferungen/Tarifstruktur.xlsx.download.xlsx/Tarifstruktur.xlsx";
	}
}
