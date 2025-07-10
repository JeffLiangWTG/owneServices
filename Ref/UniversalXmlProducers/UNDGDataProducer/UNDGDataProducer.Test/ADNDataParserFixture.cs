using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class ADNDataParserFixture
	{
		[Test]
		public void Output()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var expectedXmlFilePath = Path.Combine(binPath, @"TestFiles\UNDG ADN Expected.xml");
			var resultXmlFilePath = Path.Combine(binPath, @"TestFiles\UNDG ADN Result.xml");
			var csvFilePath = Path.Combine(binPath, @"TestFiles\UNDG ADN List Sample.csv");

			var parser = new ADNRecordParser();
			var parsedRecords = parser.Parse(Path.Combine(binPath, csvFilePath));
			var xmlWriter = ADNXmlWriterConfiguration.GetXmlWriter(new DateTime(1601, 1, 1));
			XmlWriterHelper.ExportToXml(xmlWriter, parsedRecords, resultXmlFilePath, false); //TODO: set validation to true on WI00433229

			var result = File.ReadAllText(resultXmlFilePath);
			var expected = File.ReadAllText(expectedXmlFilePath).TrimEnd();

			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void FullPopulatedRowInCsv()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var csvFilePath = Path.Combine(binPath, @"TestFiles\UNDG ADN Single Full Row Sample.csv");

			var parsedRecords = new ADNRecordParser().Parse(csvFilePath);
			Assert.That(parsedRecords.Count(), Is.EqualTo(1));

			var element = parsedRecords.ElementAt(0);
			Assert.That(element.ADN_UNNO, Is.EqualTo("0271"));
			Assert.That(element.ADN_Variant, Is.EqualTo(""));
			Assert.That(element.ADN_PSN, Is.EqualTo("CHARGES, PROPELLING"));
			Assert.That(element.ADN_Class, Is.EqualTo("1"));
			Assert.That(element.ADN_ClassificationCode, Is.EqualTo("1.1C"));
			Assert.That(element.ADN_PG, Is.EqualTo("II"));
			Assert.That(element.ADN_Labels, Is.EqualTo("1"));
			Assert.That(element.ADN_ExceptedQuantityCode, Is.EqualTo("E0"));
			Assert.That(element.ADN_LQMaxAmt, Is.EqualTo(0));
			Assert.That(element.ADN_LQMaxAmtUQ, Is.EqualTo("kg"));
			Assert.That(element.ADN_LQ2MaxAmt, Is.EqualTo(500));
			Assert.That(element.ADN_LQ2MaxAmtUQ, Is.EqualTo("gr"));
			Assert.That(element.ADN_CarriagePermittedPacks, Is.False);
			Assert.That(element.ADN_CarriagePermittedBulk, Is.False);
			Assert.That(element.ADN_CarriagePermittedTanks, Is.False);
			Assert.That(element.ADN_CarriagePermittedDetails, Is.EqualTo("NOT SUBJECT TO AND"));
			Assert.That(element.ADN_Ventilation, Is.EqualTo("Test Ventilation"));
			Assert.That(element.ADN_LoadingSpecialProv, Is.EqualTo("LO01"));
			Assert.That(element.ADN_LoadingSpecialProvNote, Is.EqualTo("Dangerous only in bulk or without packaging."));
			Assert.That(element.ADN_UnloadingSpecialProv, Is.EqualTo("HA01 HA02 HA03"));
			Assert.That(element.ADN_UnloadingSpecialProvNote, Is.EqualTo("Dangerous only in bulk or without packaging."));
			Assert.That(element.ADN_OperationSpecialProv, Is.EqualTo("IN01 IN03"));
			Assert.That(element.ADN_OperationSpecialProvNote, Is.EqualTo("Dangerous only in bulk or without packaging."));
			Assert.That(element.ADN_BlueCones, Is.EqualTo(3));
		}

		[Test]
		public void HeaderMap()
		{
			var headerMap = new ADNRecordParser().HeaderMap;
			Assert.That(headerMap[nameof(ADNRecord.UnId)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("A")));
			Assert.That(headerMap[nameof(ADNRecord.NameAndDescription)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("B")));
			Assert.That(headerMap[nameof(ADNRecord.Class)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("C")));
			Assert.That(headerMap[nameof(ADNRecord.ClassficationCode)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("D")));
			Assert.That(headerMap[nameof(ADNRecord.PG)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("E")));
			Assert.That(headerMap[nameof(ADNRecord.Label)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("F")));
			Assert.That(headerMap[nameof(ADNRecord.SpecialProvisions)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("G")));
			Assert.That(headerMap[nameof(ADNRecord.LQMaxAmt)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("H")));
			Assert.That(headerMap[nameof(ADNRecord.ExceptedQuantityCode)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("I")));
			Assert.That(headerMap[nameof(ADNRecord.CarriagePermitted)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("J")));
			Assert.That(headerMap[nameof(ADNRecord.EquipmentRequired)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("K")));
			Assert.That(headerMap[nameof(ADNRecord.Ventilation)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("L")));
			Assert.That(headerMap[nameof(ADNRecord.LoadingSpecialProv)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("M")));
			Assert.That(headerMap[nameof(ADNRecord.UnloadingSpecialProv)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("N")));
			Assert.That(headerMap[nameof(ADNRecord.OperationSpecialProv)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("O")));
			Assert.That(headerMap[nameof(ADNRecord.BlueCones)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("P")));
			Assert.That(headerMap[nameof(ADNRecord.Remarks)], Is.EqualTo(ADNParserHelper.ConvertCSVIndexToInt("Q")));
		}

		[Test]
		public void TestParseNonPrefixedUNNO()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var ADNFile = Path.Combine(binPath, @"TestFiles\UNDG ADN Single Short UNNO Sample.csv");

			var parser = new ADNRecordParser();
			var ADNRecords = parser.Parse(ADNFile);

			Assert.That(ADNRecords.Count(), Is.EqualTo(1));
			Assert.That(ADNRecords.First().ADN_UNNO, Is.EqualTo("0271"));
		}
	}
}
