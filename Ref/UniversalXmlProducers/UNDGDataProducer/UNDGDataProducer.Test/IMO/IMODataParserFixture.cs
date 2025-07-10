using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class IMODataParserFixture
	{
		[Test]
		public void Output()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var expectedXmlFilePath = Path.Combine(binPath, @"TestFiles\IMOUNDG Expected.xml");
			var resultXmlFilePath = Path.Combine(binPath, @"TestFiles\IMOUNDG Result.xml");
			var zipFilePath = Path.Combine(binPath, @"TestFiles\IMO Sample.zip");

			ExportToXml(zipFilePath, resultXmlFilePath);
			AssertXmlContent(resultXmlFilePath, expectedXmlFilePath);
		}

		[Test]
		public void EmptyVariantsForSingleIMORecords()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var expectedXmlFilePath = Path.Combine(binPath, @"TestFiles\IMO VariantFix Expected.xml");
			var resultXmlFilePath = Path.Combine(binPath, @"TestFiles\IMO VariantFix Result.xml");
			var zipFilePath = Path.Combine(binPath, @"TestFiles\IMO VariantFix.zip");

			ExportToXml(zipFilePath, resultXmlFilePath);
			AssertXmlContent(resultXmlFilePath, expectedXmlFilePath);
		}

		void ExportToXml(string zipFilePath, string resultXmlFilePath)
		{
			var fileDownloaderWrapper = new FileDownloaderWrapper();
			var extractedFiles = fileDownloaderWrapper.ExtractLocalZipFile(zipFilePath);
			var seaRecordsFile = extractedFiles.FirstOrDefault(x => x.Contains(Constants.IMOFiles.List, StringComparison.OrdinalIgnoreCase));

			var parser = new IMORecordParser(Enumerable.Empty<StowSegRecord>(), Enumerable.Empty<SpecProvRecord>(), Enumerable.Empty<PropertyRecord>(), Enumerable.Empty<QualifyingDescriptiveTextRecord>());
			var seaRecords = parser.Parse(seaRecordsFile);
			XmlWriterHelper.ExportToXml(IMOXmlWriterConfiguration.GetXmlWriter(new DateTime(1601, 1, 1)), seaRecords, resultXmlFilePath);
		}

		void AssertXmlContent(string resultXmlFilePath, string expectedXmlFilePath)
		{
			var result = File.ReadAllText(resultXmlFilePath);
			var expected = File.ReadAllText(expectedXmlFilePath).TrimEnd();
			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void HeaderMap()
		{
			var headerMap = new IMORecordParser(null, null, null, null).HeaderMap;
			Assert.That(headerMap[nameof(IMORecord.UNNO)], Is.EqualTo(0));
			Assert.That(headerMap[nameof(IMORecord.Variant)], Is.EqualTo(1));
			Assert.That(headerMap[nameof(IMORecord.CVL)], Is.EqualTo(2));
			Assert.That(headerMap[nameof(IMORecord.Variation)], Is.EqualTo(3));
			Assert.That(headerMap[nameof(IMORecord.Class)], Is.EqualTo(4));
			Assert.That(headerMap[nameof(IMORecord.SubLabel1)], Is.EqualTo(5));
			Assert.That(headerMap[nameof(IMORecord.SubLabel2)], Is.EqualTo(6));
			Assert.That(headerMap[nameof(IMORecord.PSN)], Is.EqualTo(7));
			Assert.That(headerMap[nameof(IMORecord.PG)], Is.EqualTo(8));
			Assert.That(headerMap[nameof(IMORecord.EMS)], Is.EqualTo(9));
			Assert.That(headerMap[nameof(IMORecord.MP)], Is.EqualTo(10));
			Assert.That(headerMap[nameof(IMORecord.FP)], Is.EqualTo(11));
			Assert.That(headerMap[nameof(IMORecord.LQ)], Is.EqualTo(12));
			Assert.That(headerMap[nameof(IMORecord.EQ)], Is.EqualTo(13));
			Assert.That(headerMap[nameof(IMORecord.TechName)], Is.EqualTo(14));
			Assert.That(headerMap[nameof(IMORecord.TreatAs)], Is.EqualTo(15));
			Assert.That(headerMap[nameof(IMORecord.DGLPhrase)], Is.EqualTo(16));
			Assert.That(headerMap[nameof(IMORecord.Stow)], Is.EqualTo(17));
			Assert.That(headerMap[nameof(IMORecord.Seg)], Is.EqualTo(18));
			Assert.That(headerMap[nameof(IMORecord.SpecProv)], Is.EqualTo(19));
			Assert.That(headerMap[nameof(IMORecord.PackIns)], Is.EqualTo(20));
			Assert.That(headerMap[nameof(IMORecord.PackProv)], Is.EqualTo(21));
			Assert.That(headerMap[nameof(IMORecord.IbcIns)], Is.EqualTo(22));
			Assert.That(headerMap[nameof(IMORecord.IbcProv)], Is.EqualTo(23));
			Assert.That(headerMap[nameof(IMORecord.UNTankIns)], Is.EqualTo(24));
			Assert.That(headerMap[nameof(IMORecord.TankProv)], Is.EqualTo(25));
			Assert.That(headerMap[nameof(IMORecord.StowCat)], Is.EqualTo(26));
			Assert.That(headerMap[nameof(IMORecord.ExpLim)], Is.EqualTo(27));
			Assert.That(headerMap[nameof(IMORecord.UlineEMS)], Is.EqualTo(28));
		}

		[Test]
		public void TestParseNonPrefixedUNNO()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var imoFile = Path.Combine(binPath, @"TestFiles\UNDG IMO Single Short UNNO Sample.txt");

			var parser = new IMORecordParser(Enumerable.Empty<StowSegRecord>(), Enumerable.Empty<SpecProvRecord>(), Enumerable.Empty<PropertyRecord>(), Enumerable.Empty<QualifyingDescriptiveTextRecord>());
			var imoRecords = parser.Parse(imoFile);

			Assert.That(imoRecords.Count(), Is.EqualTo(1));
			Assert.That(imoRecords.First().DG_UNNO, Is.EqualTo("0004"));
		}
	}
}
