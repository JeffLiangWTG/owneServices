using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class JTTDataParserFixture
	{
		[Test]
		public void Output()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var expectedXmlFilePath = Path.Combine(binPath, @"TestFiles\UNDG JTT Expected.xml");
			var resultXmlFilePath = Path.Combine(binPath, @"TestFiles\UNDG JTT Result.xml");
			var csvFilePath = Path.Combine(binPath, @"TestFiles\UNDG JTT List Sample.csv");

			var parser = new JTTRecordParser();
			var parsedRecords = parser.Parse(Path.Combine(binPath, csvFilePath));
			var xmlWriter = JTTXmlWriterConfiguration.GetXmlWriter(new DateTime(1601, 1, 1));
			XmlWriterHelper.ExportToXml(xmlWriter, parsedRecords, resultXmlFilePath);

			var result = File.ReadAllText(resultXmlFilePath);
			var expected = File.ReadAllText(expectedXmlFilePath).TrimEnd();

			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void TestParseNonPrefixedUNNO()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var JTTFile = Path.Combine(binPath, @"TestFiles\UNDG JTT Single Short UNNO Sample.csv");

			var parser = new JTTRecordParser();
			var JTTRecords = parser.Parse(JTTFile);

			Assert.That(JTTRecords.Count(), Is.EqualTo(1));
			Assert.That(JTTRecords.First().JTT_UNNO, Is.EqualTo("0004"));
		}
	}
}
