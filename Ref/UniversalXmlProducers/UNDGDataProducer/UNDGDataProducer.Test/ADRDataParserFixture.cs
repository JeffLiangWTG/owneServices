using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class ADRDataParserFixture
	{
		[Test]
		public void Output()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var expectedXmlFilePath = Path.Combine(binPath, @"TestFiles\UNDG ADR Expected.xml");
			var resultXmlFilePath = Path.Combine(binPath, @"TestFiles\UNDG ADR Result.xml");
			var csvFilePath = Path.Combine(binPath, @"TestFiles\UNDG ADR List Sample.csv");

			var parser = new ADRRecordParser();
			var parsedRecords = parser.Parse(Path.Combine(binPath, csvFilePath));
			var xmlWriter = ADRXmlWriterConfiguration.GetXmlWriter(new DateTime(1601, 1, 1));
			XmlWriterHelper.ExportToXml(xmlWriter, parsedRecords, resultXmlFilePath);

			var result = File.ReadAllText(resultXmlFilePath);
			var expected = File.ReadAllText(expectedXmlFilePath).TrimEnd();

			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void FullPopulatedRowInCsv()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var csvFilePath = Path.Combine(binPath, @"TestFiles\UNDG ADR Single Full Row Sample.csv");

			var parsedRecords = new ADRRecordParser().Parse(csvFilePath);
			Assert.That(parsedRecords.Count(), Is.EqualTo(1));

			var element = parsedRecords.ElementAt(0);
			Assert.That(element.ADR_UNNO, Is.EqualTo("0331"));
			Assert.That(element.ADR_Variant, Is.EqualTo("a"));
			Assert.That(element.ADR_PSN, Is.EqualTo("EXPLOSIVE, BLASTING, TYPE B"));
			Assert.That(element.ADR_Class, Is.EqualTo("1"));
			Assert.That(element.ADR_ClassificationCode, Is.EqualTo("1.5D"));
			Assert.That(element.ADR_PG, Is.EqualTo(""));
			Assert.That(element.ADR_Labels, Is.EqualTo("1.5"));
			Assert.That(element.ADR_SpecialProvisions, Is.EqualTo("617"));
			Assert.That(element.ADR_LQMaxAmt, Is.EqualTo(5));
			Assert.That(element.ADR_LQMaxAmtUQ, Is.EqualTo("kg"));
			Assert.That(element.ADR_ExceptedQuantityCode, Is.Empty);
			Assert.That(element.ADR_PackIns, Is.EqualTo("P116 IBC100"));
			Assert.That(element.ADR_PackProv, Is.EqualTo("PP61 PP62 PP64"));
			Assert.That(element.ADR_MixedPackingProv, Is.EqualTo("MP20"));
			Assert.That(element.ADR_ADRTankSpecProv, Is.EqualTo("TU3 TU12 TU41 TC8 TA1 TA5"));
			Assert.That(element.ADR_BulkTankSpecProv, Is.EqualTo("TP1 TP17 TP32"));
			Assert.That(element.ADR_ADRTankCode, Is.EqualTo("S2.65AN(+)"));
			Assert.That(element.ADR_BulkTankIns, Is.EqualTo("T1"));
			Assert.That(element.ADR_TankVehicle, Is.EqualTo("EX/III"));
			Assert.That(element.ADR_TransportCategory, Is.EqualTo("1 (A)"));
			Assert.That(element.ADR_PackingSpecialProv, Is.EqualTo("V2 V12"));
			Assert.That(element.ADR_BulkSpecialProv, Is.EqualTo(""));
			Assert.That(element.ADR_LoadingSpecialProv, Is.EqualTo("CV1 CV2 CV3"));
			Assert.That(element.ADR_OperationSpecialProv, Is.EqualTo("S1"));
			Assert.That(element.ADR_HazardIDNumber, Is.EqualTo("1.5D"));
		}

		[Test]
		public void CsvRowWithHyphenTransportCategory()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var csvFilePath = Path.Combine(binPath, @"TestFiles\UNDG ADR Single Row Sample With Hyphen Transport Category.csv");

			var parsedRecords = new ADRRecordParser().Parse(csvFilePath);
			Assert.That(parsedRecords.Count(), Is.EqualTo(1));

			var element = parsedRecords.ElementAt(0);
			Assert.That(element.ADR_TransportCategory, Is.EqualTo("1 (-)"));
		}

		[Test]
		public void CsvRowWithSeeContent()
		{
			var originalErrorWriter = Console.Error;
			using (var tempOutputWriter = new StringWriter())
			{
				Console.SetError(tempOutputWriter);
				var output = tempOutputWriter.GetStringBuilder();

				var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				var csvFilePath = Path.Combine(binPath, @"TestFiles\UNDG ADR Single Row Sample With See Content.csv");

				var parsedRecords = new ADRRecordParser().Parse(csvFilePath);
				Assert.That(parsedRecords.Count(), Is.EqualTo(1));

				var errorString = output.ToString();
				Assert.That(errorString, Is.Null.Or.Empty);
				output.Clear();

				var element = parsedRecords.ElementAt(0);
				Assert.That(element.ADR_ExceptedQuantityCode, Is.EqualTo(""));

				Console.SetError(originalErrorWriter);
			}
		}

		[Test]
		public void HeaderMap()
		{
			var headerMap = new ADRRecordParser().HeaderMap;
			Assert.That(headerMap[nameof(ADRRecord.UnId)], Is.EqualTo(0));
			Assert.That(headerMap[nameof(ADRRecord.Var)], Is.EqualTo(1));
			Assert.That(headerMap[nameof(ADRRecord.NameADR)], Is.EqualTo(2));
			Assert.That(headerMap[nameof(ADRRecord.QdtADR)], Is.EqualTo(3));
			Assert.That(headerMap[nameof(ADRRecord.CCodeADR)], Is.EqualTo(4));
			Assert.That(headerMap[nameof(ADRRecord.ClassADR)], Is.EqualTo(5));
			Assert.That(headerMap[nameof(ADRRecord.LabelADR)], Is.EqualTo(6));
			Assert.That(headerMap[nameof(ADRRecord.PgADR)], Is.EqualTo(7));
			Assert.That(headerMap[nameof(ADRRecord.LqADR)], Is.EqualTo(8));
			Assert.That(headerMap[nameof(ADRRecord.SProvADR)], Is.EqualTo(9));
			Assert.That(headerMap[nameof(ADRRecord.EqADR)], Is.EqualTo(10));
			Assert.That(headerMap[nameof(ADRRecord.PiADR)], Is.EqualTo(11));
			Assert.That(headerMap[nameof(ADRRecord.MixpADR)], Is.EqualTo(12));
			Assert.That(headerMap[nameof(ADRRecord.PpADR)], Is.EqualTo(13));
			Assert.That(headerMap[nameof(ADRRecord.TiADR)], Is.EqualTo(14));
			Assert.That(headerMap[nameof(ADRRecord.AdrtADR)], Is.EqualTo(15));
			Assert.That(headerMap[nameof(ADRRecord.TpADR)], Is.EqualTo(16));
			Assert.That(headerMap[nameof(ADRRecord.AdrtpADR)], Is.EqualTo(17));
			Assert.That(headerMap[nameof(ADRRecord.TankvADR)], Is.EqualTo(18));
			Assert.That(headerMap[nameof(ADRRecord.TCatADR)], Is.EqualTo(19));
			Assert.That(headerMap[nameof(ADRRecord.PProvADR)], Is.EqualTo(20));
			Assert.That(headerMap[nameof(ADRRecord.BProvADR)], Is.EqualTo(21));
			Assert.That(headerMap[nameof(ADRRecord.LProvADR)], Is.EqualTo(22));
			Assert.That(headerMap[nameof(ADRRecord.OProvADR)], Is.EqualTo(23));
			Assert.That(headerMap[nameof(ADRRecord.HinADR)], Is.EqualTo(24));
		}

		[Test]
		public void TestParseNonPrefixedUNNO()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var ADRFile = Path.Combine(binPath, @"TestFiles\UNDG ADR Single Row Sample With See Content.csv");

			var parser = new ADRRecordParser();
			var ADRRecords = parser.Parse(ADRFile);

			Assert.That(ADRRecords.Count(), Is.EqualTo(1));
			Assert.That(ADRRecords.First().ADR_UNNO, Is.EqualTo("0331"));
		}
	}
}
