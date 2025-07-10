using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class IATADataParserFixture
	{
		[Test]
		public void Output()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var expectedXmlFilePath = Path.Combine(binPath, @"TestFiles\IATAUNDG Expected.xml");
			var resultXmlFilePath = Path.Combine(binPath, @"TestFiles\IATAUNDG Result.xml");
			var csvFilePath = Path.Combine(binPath, @"TestFiles\IATAUNDG Small List Sample.csv");

			var parser = new IATARecordParser();
			var parsedRecords = parser.Parse(Path.Combine(binPath, csvFilePath));
			var xmlWriter = IATAXmlWriterConfiguration.GetXmlWriter(new DateTime(1601, 1, 1));
			XmlWriterHelper.ExportToXml(xmlWriter, parsedRecords, resultXmlFilePath);

			var result = File.ReadAllText(resultXmlFilePath);
			var expected = File.ReadAllText(expectedXmlFilePath).TrimEnd();

			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void FullPopulatedRowInCsv()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var csvFilePath = Path.Combine(binPath, @"TestFiles\IATAUNDG OneRowFullPopulated.csv");

			var parsedRecords = new IATARecordParser().Parse(csvFilePath);
			Assert.That(parsedRecords.Count(), Is.EqualTo(1));

			var element = parsedRecords.ElementAt(0);
			Assert.That(element.DG_CargoMaxAmt, Is.EqualTo(30));
			Assert.That(element.DG_CargoMaxAmtUQ, Is.EqualTo("L"));
			Assert.That(element.DG_CargoPackAmtType, Is.EqualTo(Constants.AmtTypes.NetWeightLimitAmtType));
			Assert.That(element.DG_CargoPackIns, Is.EqualTo("855"));
			Assert.That(element.DG_Class, Is.EqualTo("8"));
			Assert.That(element.DG_EmergencyResponseGuide, Is.EqualTo("8F"));
			Assert.That(element.DG_ExceptedQuantityCode, Is.EqualTo("E2"));
			Assert.That(element.DG_Hazards, Is.EqualTo("Corrosive;Flammable Liquid;Package Orientation"));
			Assert.That(element.DG_IsNotOtherwiseSpecified, Is.EqualTo(false));
			Assert.That(element.DG_LQ2OrPaxMaxAmt, Is.EqualTo(1));
			Assert.That(element.DG_LQ2OrPaxMaxAmtType, Is.EqualTo(Constants.AmtTypes.NetWeightLimitAmtType));
			Assert.That(element.DG_LQ2OrPaxMaxAmtUQ, Is.EqualTo("L"));
			Assert.That(element.DG_LQMaxAmt, Is.EqualTo(0.5));
			Assert.That(element.DG_LQMaxAmtType, Is.EqualTo(Constants.AmtTypes.NetWeightLimitAmtType));
			Assert.That(element.DG_LQMaxAmtUQ, Is.EqualTo("L"));
			Assert.That(element.DG_PackIns, Is.EqualTo("Y840"));
			Assert.That(element.DG_PaxPackIns, Is.EqualTo("851"));
			Assert.That(element.DG_PG, Is.EqualTo("II"));
			Assert.That(element.DG_PSN, Is.EqualTo("Acetic acid solution"));
			Assert.That(element.DG_SubLabel1, Is.EqualTo("3"));
			Assert.That(element.DG_SubLabel2, Is.EqualTo(null));
			Assert.That(element.DG_TechName, Is.EqualTo("*"));
			Assert.That(element.DG_UniqueRecordId, Is.EqualTo("29661"));
			Assert.That(element.DG_UNNO, Is.EqualTo("2789"));
			Assert.That(element.DG_SpecialHandlingCodes, Is.EqualTo("RCM RFL"));

			Assert.That(element.UNDGAttributes, Has.Length.EqualTo(3));
			var attr1 = element.UNDGAttributes[0];
			Assert.That(attr1.DA_Descriptor, Is.EqualTo("more than 80% acid, by weight"));
			Assert.That(attr1.DA_Index, Is.EqualTo("0"));
			Assert.That(attr1.DA_Type, Is.EqualTo(Constants.AttributeTypes.QualifyingDescriptive));
			Assert.That(attr1.DA_Language, Is.EqualTo("EN"));

			var attr2 = element.UNDGAttributes[1];
			Assert.That(attr2.DA_Descriptor, Is.EqualTo("other name"));
			Assert.That(attr2.DA_Index, Is.EqualTo("0"));
			Assert.That(attr2.DA_Type, Is.EqualTo(Constants.AttributeTypes.OtherNames));
			Assert.That(attr2.DA_Language, Is.EqualTo("EN"));

			var attr3 = element.UNDGAttributes[2];
			Assert.That(attr3.DA_Descriptor, Is.EqualTo("A1 A3 A108"));
			Assert.That(attr3.DA_Index, Is.EqualTo("0"));
			Assert.That(attr3.DA_Type, Is.EqualTo(Constants.AttributeTypes.SpecialProvisions));
			Assert.That(attr3.DA_Language, Is.EqualTo("EN"));
		}

		[Test]
		public void EncodingFileSymbols()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var csvFilePath = Path.Combine(binPath, @"TestFiles\IATAUNDGEncodingForSymbols.csv");

			var parsedRecords = new IATARecordParser().Parse(csvFilePath);
			Assert.That(parsedRecords.Count(), Is.EqualTo(1));

			var element = parsedRecords.ElementAt(0);
			var attribute = element.UNDGAttributes.ElementAt(0);
			Assert.That(attribute.DA_Descriptor, Is.EqualTo("with ≥ 5.5% but ≤ 16% water"));
		}

		[Test]
		public void HeaderMap()
		{
			var headerMap = new IATARecordParser().HeaderMap;
			Assert.That(headerMap[nameof(IATARecord.UnOrIdNumber)], Is.EqualTo(0));
			Assert.That(headerMap[nameof(IATARecord.ProperShippingName)], Is.EqualTo(1));
			Assert.That(headerMap[nameof(IATARecord.NOS)], Is.EqualTo(2));
			Assert.That(headerMap[nameof(IATARecord.HasTechnicalName)], Is.EqualTo(4));
			Assert.That(headerMap[nameof(IATARecord.QualifyingDescriptiveText)], Is.EqualTo(5));
			Assert.That(headerMap[nameof(IATARecord.OtherNames)], Is.EqualTo(6));
			Assert.That(headerMap[nameof(IATARecord.ClassOrDivision)], Is.EqualTo(7));
			Assert.That(headerMap[nameof(IATARecord.SubRisks)], Is.EqualTo(8));
			Assert.That(headerMap[nameof(IATARecord.HazardAndHandlingLabels)], Is.EqualTo(9));
			Assert.That(headerMap[nameof(IATARecord.PackingGroup)], Is.EqualTo(10));
			Assert.That(headerMap[nameof(IATARecord.PassengerAndCargoLQPackInstruction)], Is.EqualTo(11));
			Assert.That(headerMap[nameof(IATARecord.PassengerAndCargoLQPackMaxAmt)], Is.EqualTo(12));
			Assert.That(headerMap[nameof(IATARecord.PassengerAndCargoPackInstruction)], Is.EqualTo(13));
			Assert.That(headerMap[nameof(IATARecord.PassengerAndCargoPackMaxAmt)], Is.EqualTo(14));
			Assert.That(headerMap[nameof(IATARecord.CargoPackInstruction)], Is.EqualTo(15));
			Assert.That(headerMap[nameof(IATARecord.CargoPackMaxAmt)], Is.EqualTo(16));
			Assert.That(headerMap[nameof(IATARecord.SpecialProvisions)], Is.EqualTo(17));
			Assert.That(headerMap[nameof(IATARecord.ERGCode)], Is.EqualTo(18));
			Assert.That(headerMap[nameof(IATARecord.ExceptedQuantity)], Is.EqualTo(22));
			Assert.That(headerMap[nameof(IATARecord.SpecialHandlingCode1)], Is.EqualTo(23));
			Assert.That(headerMap[nameof(IATARecord.SpecialHandlingCode2)], Is.EqualTo(24));
			Assert.That(headerMap[nameof(IATARecord.SpecialHandlingCode3)], Is.EqualTo(25));
			Assert.That(headerMap[nameof(IATARecord.UniqueRecordId)], Is.EqualTo(27));
		}

		[Test]
		public void TestParseNonPrefixedUNNO()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var IATAFile = Path.Combine(binPath, @"TestFiles\UNDG IATA Single Short UNNO Sample.csv");

			var parser = new IATARecordParser();
			var IATARecords = parser.Parse(IATAFile);

			Assert.That(IATARecords.Count(), Is.EqualTo(1));
			Assert.That(IATARecords.First().DG_UNNO, Is.EqualTo("0014"));
		}
	}
}
