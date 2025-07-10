using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.RefDbRepo.CHReferenceData.Business.RateCodes;
using CargoWise.RefDbRepo.CHReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CHReferenceData.Tests.RateCodes
{
	internal abstract class RateCodesParserBaseTest
	{
		protected abstract RateCodesParser GetRateCodesParser(DownloadResult masterdataDownload);

		protected abstract string ExpectedConfiguration { get; }

		protected abstract string SampleInput { get; }

		protected abstract string RateType {  get; }

		[TestCase]
		public void TestGetCusRateTypeConfiguration()
		{
			using (var outputFile = new TemporaryOutputFile($@"RateCodes\{GetType().Name}-TestGetCusRateTypeConfiguration.xml"))
			{
				var parser = GetRateCodesParser((DownloadResult)null);
				var writerConfiguration = parser.GetXmlWriterConfiguration();
				var writer = new XmlWriter(writerConfiguration);
				writer.SetDataSource("TEST");
				writer.SetPublicationTime(DateTime.Parse("2021-12-31", CultureInfo.InvariantCulture));
				writer.SetUpdateType(UpdateType.Full);
				writer.SaveXml(outputFile.FullPath);
				var expectedXML = TestExtensions.ReadManifestResourceContent($"{typeof(RateCodesParserBaseTest).Namespace}.TestFiles.Output.{ExpectedConfiguration}");
				Assert.That(TestHelper.RemoveIgnoreTagsFromXml(File.ReadAllText(outputFile.FullPath)), Is.EqualTo(expectedXML));
			}
		}

		[TestCase]
		public void TestSample()
		{
			using (var outputFile = new TemporaryOutputFile($@"RateCodes\{GetType().Name}-TestSample.xml"))
			{
				GetRateCodesParser($"TestFiles.Input.{SampleInput}").ConvertToRefXML(outputFile.FullPath);

				var outputDoc = XDocument.Load(outputFile.FullPath);
				Assert.That(outputDoc.XPathSelectElements("//RefCusRateCode").Count(), Is.AtLeast(1));
			}
		}

		RateCodesParser GetRateCodesParser(string inputResource)
		{
			DownloadResult masterDataDownload = new DownloadResult
			{
				Content = typeof(RateCodesParserBaseOnlyTest).GetTestArray(inputResource),
			};

			return GetRateCodesParser(masterDataDownload);
		}

		[SetUp]
		public void SetUp()
		{
			TestHelper.DeleteLogFiles();
		}

		[TearDown]
		public void TearDown()
		{
			TestHelper.DeleteLogFiles();
		}
	}
}
