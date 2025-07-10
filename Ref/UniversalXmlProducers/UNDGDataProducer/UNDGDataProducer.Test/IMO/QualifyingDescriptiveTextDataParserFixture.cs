using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class QualifyingDescriptiveTextDataParserFixture
	{
		[Test]
		public void HeaderMap()
		{
			var headerMap = new QualifyingDescriptiveTextRecordParser().HeaderMap;
			Assert.That(headerMap[nameof(QualifyingDescriptiveTextRecord.UnId)], Is.EqualTo(0));
			Assert.That(headerMap[nameof(QualifyingDescriptiveTextRecord.QualifyingDescriptiveText)], Is.EqualTo(1));
		}

		[Test]
		public void TestParseNonPrefixedUNNO()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var qdtFile = Path.Combine(binPath, @"TestFiles\UNDG IMO QDT Single Short UNNO Sample.txt");
			var parser = new QualifyingDescriptiveTextRecordParser();
			var qdtRecords = parser.Parse(qdtFile);
			Assert.That(qdtRecords.Count(), Is.EqualTo(1));
			Assert.That(qdtRecords.First().UnId, Is.EqualTo("0005a"));
		}
	}
}
