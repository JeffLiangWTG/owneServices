using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class RIDDataParserFixture
	{
		[Test]
		public void Output()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var expectedXmlFilePath = Path.Combine(binPath, @"TestFiles\UNDG RID Expected.xml");
			var resultXmlFilePath = Path.Combine(binPath, @"TestFiles\UNDG RID Result.xml");
			var csvFilePath = Path.Combine(binPath, @"TestFiles\UNDG RID List Sample.csv");

			var parser = new RIDRecordParser();
			var parsedRecords = parser.Parse(Path.Combine(binPath, csvFilePath));
			var xmlWriter = RIDXmlWriterConfiguration.GetXmlWriter(new DateTime(1601, 1, 1));
			XmlWriterHelper.ExportToXml(xmlWriter, parsedRecords, resultXmlFilePath);

			var result = File.ReadAllText(resultXmlFilePath);
			var expected = File.ReadAllText(expectedXmlFilePath).TrimEnd();

			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void HeaderMap()
		{
			var headerMap = new RIDRecordParser().HeaderMap;
			Assert.That(headerMap[nameof(RIDRecord.UnId)], Is.EqualTo(0));
			Assert.That(headerMap[nameof(RIDRecord.Var)], Is.EqualTo(1));
			Assert.That(headerMap[nameof(RIDRecord.NameRID)], Is.EqualTo(2));
			Assert.That(headerMap[nameof(RIDRecord.ClassRID)], Is.EqualTo(3));
			Assert.That(headerMap[nameof(RIDRecord.CCodeRID)], Is.EqualTo(4));
			Assert.That(headerMap[nameof(RIDRecord.PgRID)], Is.EqualTo(5));
			Assert.That(headerMap[nameof(RIDRecord.LabRID)], Is.EqualTo(6));
			Assert.That(headerMap[nameof(RIDRecord.SpRID)], Is.EqualTo(7));
			Assert.That(headerMap[nameof(RIDRecord.LqRID)], Is.EqualTo(8));
			Assert.That(headerMap[nameof(RIDRecord.EqRID)], Is.EqualTo(9));
			Assert.That(headerMap[nameof(RIDRecord.PiRID)], Is.EqualTo(10));
			Assert.That(headerMap[nameof(RIDRecord.PpRID)], Is.EqualTo(11));
			Assert.That(headerMap[nameof(RIDRecord.MixRID)], Is.EqualTo(12));
			Assert.That(headerMap[nameof(RIDRecord.TiRID)], Is.EqualTo(13));
			Assert.That(headerMap[nameof(RIDRecord.TpRID)], Is.EqualTo(14));
			Assert.That(headerMap[nameof(RIDRecord.TrRID)], Is.EqualTo(15));
			Assert.That(headerMap[nameof(RIDRecord.SptRID)], Is.EqualTo(16));
			Assert.That(headerMap[nameof(RIDRecord.CatRID)], Is.EqualTo(17));
			Assert.That(headerMap[nameof(RIDRecord.SppRID)], Is.EqualTo(18));
			Assert.That(headerMap[nameof(RIDRecord.SpbRID)], Is.EqualTo(19));
			Assert.That(headerMap[nameof(RIDRecord.SplRID)], Is.EqualTo(20));
			Assert.That(headerMap[nameof(RIDRecord.ColisRID)], Is.EqualTo(21));
			Assert.That(headerMap[nameof(RIDRecord.HinRID)], Is.EqualTo(22));
			Assert.That(headerMap[nameof(RIDRecord.Qdt)], Is.EqualTo(23));
		}

		[Test]
		public void TestParseNonPrefixedUNNO()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var RIDFile = Path.Combine(binPath, @"TestFiles\UNDG RID Single Short UNNO Sample.csv");

			var parser = new RIDRecordParser();
			var RIDRecords = parser.Parse(RIDFile);

			Assert.That(RIDRecords.Count(), Is.EqualTo(1));
			Assert.That(RIDRecords.First().RID_UNNO, Is.EqualTo("0004"));
		}
	}
}
