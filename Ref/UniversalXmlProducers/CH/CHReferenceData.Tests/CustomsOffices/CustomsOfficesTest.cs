using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.RefDbRepo.CHReferenceData.Business.CustomsOffices;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.CHReferenceData.Services.CustomsOffices;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.CustomsOffices
{
	[TestFixture]
	class CustomsOfficesTest
	{
		[Test]
		public void TestDownloadCustomsOfficesTestAndConvert()
		{
			using (var expectedTestStream = classType.GetTestStream("TestFiles.Output.edecCustomsOffices_converted.xml"))
			using (var mockHttp = new MockHttpMessageHandler())
			{
				using var edecCustomsOfficesZip = classType.GetZippedTestStream("TestFiles.Input.edecCustomsOffices.xml");
				mockHttp.When(DownloadUrl).WithUserAgent().Respond("application/zip", edecCustomsOfficesZip);
				var client = mockHttp.ToHttpClient();
				var download = DownloadCustomsOffices.DownloadAndUnzip(client);
				var parser = new CustomsOfficesParser(download);
				using (var outputFile = new TemporaryOutputFile(@"CustomsOffices\RefCustomsOfficesZZ_CH.xml"))
				{
					parser.ConvertToRefXML(outputFile.FullPath, "CH CUSCH Code List", new DateTime(2021, 4, 6));
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
		public void TestCustomsOfficesParser()
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.edecCustomsOffices.xml"),
			};

			using (var outputFile = new TemporaryOutputFile(@"CustomsOffices\TestCustomsOfficesParser.xml"))
			{
				new CustomsOfficesParser(download).ConvertToRefXML(outputFile.FullPath, "CH CUSCH Code List", new DateTime(2021, 4, 6));

				using (var actualStream = new FileStream(outputFile.FullPath, FileMode.Open))
				using (var expectedStream = classType.GetTestStream("TestFiles.Output.edecCustomsOffices_converted.xml"))
				{
					var actualXml = TestHelper.RemoveIgnoreTagsFromXml(XDocument.Load(actualStream));;
					var expectedXml = XDocument.Load(expectedStream);
					Assert.IsTrue(XNode.DeepEquals(actualXml, expectedXml));
				}
			}
		}

		[Test]
		public void TestDuplicateCode()
		{
			var download = new DownloadResult
			{
				Content = classType.GetTestArray("TestFiles.Input.duplicateCode.xml"),
			};

			using (var outputFile = new TemporaryOutputFile(@"CustomsOffices\TestDuplicateCode.xml"))
			{
				new CustomsOfficesParser(download).ConvertToRefXML(outputFile.FullPath, "CH CUSCH Code List", new DateTime(2021, 4, 6));

				var actualDoc = XDocument.Load(outputFile.FullPath);
				string getActual(string code)
				{
					var elements = actualDoc.XPathSelectElements($@"//RefCusCodeList[ZZD_Code='{code}']/ZZD_Description").ToArray();
					Assert.AreEqual(1, elements.Length, "Not exactly one item converted");
					return elements[0].Value;
				}
				Assert.AreEqual("expected", getActual("TEST0001"));
				Assert.AreEqual("expected", getActual("TEST0002"));
			}
		}

		Type classType => GetType();

		const string DownloadUrl = "https://edec.douane.swiss/data/edecCustomsOffices.zip";
	}
}
