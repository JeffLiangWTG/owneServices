using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class PropertyDataParserFixture
	{
		[Test]
		public void HeaderMap()
		{
			var headerMap = new PropertyRecordParser().HeaderMap;
			Assert.That(headerMap[nameof(PropertyRecord.UNNO)], Is.EqualTo(0));
			Assert.That(headerMap[nameof(PropertyRecord.Variant)], Is.EqualTo(1));
			Assert.That(headerMap[nameof(PropertyRecord.PropObs)], Is.EqualTo(2));
		}

		[Test]
		public void TestParseNonPrefixedUNNO()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var propertyFile = Path.Combine(binPath, @"TestFiles\UNDG IMO Property Single Short UNNO Sample.txt");

			var parser = new PropertyRecordParser();
			var propertyRecords = parser.Parse(propertyFile);

			Assert.That(propertyRecords.Count(), Is.EqualTo(1));
			Assert.That(propertyRecords.First().UNNO, Is.EqualTo("0004"));
		}
	}
}
