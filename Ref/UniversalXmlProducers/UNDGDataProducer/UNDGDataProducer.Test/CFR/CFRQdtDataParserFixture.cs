using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class CFRQdtDataParserFixture
	{
		[Test]
		public void FullPopulatedRowInCSV()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var txtFilePath = Path.Combine(binPath, @"TestFiles\UNDG CFR QDT Single full Row Sample.txt");

			var parsedRecords = new CFRQualifyingDescriptiveTextRecordParser().Parse(txtFilePath);
			Assert.That(parsedRecords.Count(), Is.EqualTo(1));

			var element = parsedRecords.ElementAt(0);

			Assert.That(element.Unno, Is.EqualTo("0004"));
			Assert.That(element.Variant, Is.EqualTo("a"));
			Assert.That(element.QualifyingDescriptiveText, Is.EqualTo("dry or wetted with less than 10% water, by mass"));
		}

		[Test]
		public void HeaderMap()
		{
			var headerMap = new CFRQualifyingDescriptiveTextRecordParser().HeaderMap;
			Assert.That(headerMap[nameof(CFRQualifyingDescriptiveTextRecord.Unno)], Is.EqualTo(0));
			Assert.That(headerMap[nameof(CFRQualifyingDescriptiveTextRecord.Variant)], Is.EqualTo(1));
			Assert.That(headerMap[nameof(CFRQualifyingDescriptiveTextRecord.QualifyingDescriptiveText)], Is.EqualTo(3));
		}

		[Test]
		public void TestParseNonPrefixedUNNO()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var cfrQdtFile = Path.Combine(binPath, @"TestFiles\UNDG CFR QDT Single Short UNNO Sample.txt");
			var cfrQdtRecordParser = new CFRQualifyingDescriptiveTextRecordParser();
			var cfrQdtRecords = cfrQdtRecordParser.Parse(cfrQdtFile);
			Assert.That(cfrQdtRecords.Count(), Is.EqualTo(1));
			Assert.That(cfrQdtRecords.First().Unno, Is.EqualTo("0005"));
		}
	}
}
