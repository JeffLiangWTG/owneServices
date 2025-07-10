using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class CFRDataParserFixture
	{
		[Test]
		public void Output()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var zipFilePath = Path.Combine(binPath, @"TestFiles\CFR Sample.zip");
			var expectedXmlFilePath = Path.Combine(binPath, @"TestFiles\UNDG CFR Expected File.xml");
			var resultXmlFilePath = Path.Combine(binPath, @"TestFiles\UNDG CFR Result.xml");
			var fileDownloaderWrapper = new FileDownloaderWrapper();
			var extractedFiles = fileDownloaderWrapper.ExtractLocalZipFile(zipFilePath);
			var cfrRecordsFile = extractedFiles.FirstOrDefault(x => x.Contains(Constants.CFRFiles.List, StringComparison.OrdinalIgnoreCase));
			var qualifyingDescriptiveTextFile = extractedFiles.FirstOrDefault(x => x.Contains(Constants.CFRFiles.QualifyingDescriptiveText, StringComparison.OrdinalIgnoreCase));
			var qualifyingDescriptiveTextRecordParser = new CFRQualifyingDescriptiveTextRecordParser();
			var qualifyingDescriptiveTextRecords = qualifyingDescriptiveTextRecordParser.Parse(qualifyingDescriptiveTextFile);

			var cfrRecordParser = new CFRRecordParser(qualifyingDescriptiveTextRecords);
			var cfrRecords = cfrRecordParser.Parse(cfrRecordsFile);

			XmlWriterHelper.ExportToXml(CFRXmlWriterConfiguration.GetXmlWriter(new System.DateTime(1601, 1, 1)), cfrRecords, resultXmlFilePath);
			var result = File.ReadAllText(resultXmlFilePath);
			var expected = File.ReadAllText(expectedXmlFilePath).TrimEnd();

			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void FullPopulatedRowInCSV()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var txtQDTFilePath = Path.Combine(binPath, @"TestFiles\UNDG CFR QDT Single full Row Sample.txt");

			var parsedQDTRecords = new CFRQualifyingDescriptiveTextRecordParser().Parse(txtQDTFilePath);

			var txtFilePath = Path.Combine(binPath, @"TestFiles\UNDG CFR Single full Row Sample.txt");

			var parsedRecords = new CFRRecordParser(parsedQDTRecords).Parse(txtFilePath);
			Assert.That(parsedRecords.Count(), Is.EqualTo(1));

			var element = parsedRecords.ElementAt(0);

			Assert.That(element.CFR_PackingGroup, Is.EqualTo("I"));
			Assert.That(element.CFR_SpecialProvisions, Is.EqualTo("148"));
			Assert.That(element.CFR_TankInstructions, Is.EqualTo("T75"));
			Assert.That(element.CFR_TankProvisions, Is.EqualTo("TP5 TP22"));
			Assert.That(element.CFR_PoisonInhalationHazard, Is.EqualTo("D"));
			Assert.That(element.CFR_State, Is.EqualTo("E"));
			Assert.That(element.CFR_IsFixedPSN, Is.EqualTo(false));
			Assert.That(element.CFR_AppliesForAirTransport, Is.EqualTo(false));
			Assert.That(element.CFR_AppliesForDomesticTransport, Is.EqualTo(false));
			Assert.That(element.CFR_AppliesForInternationalTransport, Is.EqualTo(false));
			Assert.That(element.CFR_AppliesForVesselTransport, Is.EqualTo(false));
			Assert.That(element.CFR_RequiresTechnicalNameInParenthesis, Is.EqualTo(true));
			Assert.That(element.CFR_EmergencyResponseGuide, Is.EqualTo("112"));
			Assert.That(element.CFR_TechnicalName, Is.EqualTo("*"));
			Assert.That(element.CFR_TreatAs, Is.EqualTo("5.1"));
			Assert.That(element.CFR_PAXAirRailLimitType, Is.EqualTo("FOB"));
			Assert.That(element.CFR_CargoAirRailLimitType, Is.EqualTo("FOB"));
			Assert.That(element.CFR_PAXAirRailLimit, Is.EqualTo(0));
			Assert.That(element.CFR_PAXAirRailLimitUnit, Is.EqualTo("kg"));
			Assert.That(element.CFR_CargoAirRailLimit, Is.EqualTo(0));
			Assert.That(element.CFR_CargoAirRailLimitUnit, Is.EqualTo("kg"));
			Assert.That(element.CFR_SecondaryPAXAirRailLimit, Is.EqualTo(0));
			Assert.That(element.CFR_SecondaryPAXAirRailLimitUnit, Is.EqualTo("kg"));
			Assert.That(element.CFR_SecondaryCargoAirRailLimit, Is.EqualTo(0));
			Assert.That(element.CFR_SecondaryCargoAirRailLimitUnit, Is.EqualTo("kg"));
			Assert.That(element.CFR_PackingProvisions, Is.EqualTo("N77"));
			Assert.That(element.CFR_PackingInstructions, Is.EqualTo("112(b)"));
			Assert.That(element.CFR_PackingExceptions, Is.EqualTo("None"));
			Assert.That(element.CFR_IBCProvisions, Is.EqualTo("IP8"));
			Assert.That(element.CFR_UNNO, Is.EqualTo("0076"));
			Assert.That(element.CFR_Variant, Is.EqualTo("b"));
			Assert.That(element.CFR_Prefix, Is.EqualTo("UN"));
			Assert.That(element.CFR_PSN, Is.EqualTo("DINITROPHENOL"));
			Assert.That(element.CFR_Variation, Is.EqualTo("Contained in equipment. Damaged or defective in accordance with SP376, on a short international voyage."));
			Assert.That(element.CFR_PrimaryClass, Is.EqualTo("1.1D"));
			Assert.That(element.CFR_SecondaryClass, Is.EqualTo("6.1"));
			Assert.That(element.CFR_TertiaryClass, Is.EqualTo("8"));
			Assert.That(element.CFR_MarinePollutant, Is.EqualTo("Y"));
			Assert.That(element.CFR_ExceptedQuantity, Is.EqualTo("E0"));
			Assert.That(element.CFR_ReportableQuantity, Is.EqualTo(10));
			Assert.That(element.CFR_ReportableQuantityUnit, Is.EqualTo("lb"));
			Assert.That(element.CFR_PassengerStowage, Is.EqualTo("001"));
			Assert.That(element.CFR_GeneralStowage, Is.EqualTo("004 022"));
			Assert.That(element.CFR_StowageCategory, Is.EqualTo("04"));
			Assert.That(element.CFR_StowageCodes, Is.EqualTo("025 05E"));
			Assert.That(element.CFR_BulkPackingInstructions, Is.EqualTo("None"));
			Assert.That(element.CFR_BulkPackingProvisions, Is.EqualTo("B9 B14"));
			Assert.That(element.CFR_IBCInstructions, Is.EqualTo("IB2"));
			Assert.That(element.CFR_LimitedQuantityPermitted, Is.EqualTo(true));
		}

		[Test]
		public void HeaderMap()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var csvFilePath = Path.Combine(binPath, @"TestFiles\UNDG CFR QDT Single full Row Sample.txt");
			var parsedRecords = new CFRQualifyingDescriptiveTextRecordParser().Parse(csvFilePath);
			var headerMap = new CFRRecordParser(parsedRecords).HeaderMap;

			Assert.That(headerMap[nameof(CFRRecord.UNNO)], Is.EqualTo(0));
			Assert.That(headerMap[nameof(CFRRecord.Variant)], Is.EqualTo(1));
			Assert.That(headerMap[nameof(CFRRecord.Prefix)], Is.EqualTo(3));
			Assert.That(headerMap[nameof(CFRRecord.PrimaryClass)], Is.EqualTo(4));
			Assert.That(headerMap[nameof(CFRRecord.SecondaryClass)], Is.EqualTo(5));
			Assert.That(headerMap[nameof(CFRRecord.TertiaryClass)], Is.EqualTo(6));
			Assert.That(headerMap[nameof(CFRRecord.PackingGroup)], Is.EqualTo(7));
			Assert.That(headerMap[nameof(CFRRecord.PSN)], Is.EqualTo(8));
			Assert.That(headerMap[nameof(CFRRecord.MarinePollutant)], Is.EqualTo(9));
			Assert.That(headerMap[nameof(CFRRecord.SpecialProvisions)], Is.EqualTo(10));
			Assert.That(headerMap[nameof(CFRRecord.ExceptedQuantity)], Is.EqualTo(11));
			Assert.That(headerMap[nameof(CFRRecord.LimitedQuantityPermitted)], Is.EqualTo(12));
			Assert.That(headerMap[nameof(CFRRecord.LimitedQuantity)], Is.EqualTo(12));
			Assert.That(headerMap[nameof(CFRRecord.LimitedQuantityUnit)], Is.EqualTo(12));
			Assert.That(headerMap[nameof(CFRRecord.PackingExceptions)], Is.EqualTo(13));
			Assert.That(headerMap[nameof(CFRRecord.PackingInstructions)], Is.EqualTo(14));
			Assert.That(headerMap[nameof(CFRRecord.PackingProvisions)], Is.EqualTo(15));
			Assert.That(headerMap[nameof(CFRRecord.IBCInstructions)], Is.EqualTo(16));
			Assert.That(headerMap[nameof(CFRRecord.IBCProvisions)], Is.EqualTo(17));
			Assert.That(headerMap[nameof(CFRRecord.TankInstructions)], Is.EqualTo(18));
			Assert.That(headerMap[nameof(CFRRecord.TankProvisions)], Is.EqualTo(19));
			Assert.That(headerMap[nameof(CFRRecord.StowageCategory)], Is.EqualTo(20));
			Assert.That(headerMap[nameof(CFRRecord.PoisonInhalationHazard)], Is.EqualTo(21));
			Assert.That(headerMap[nameof(CFRRecord.EmergencyResponseGuide)], Is.EqualTo(22));
			Assert.That(headerMap[nameof(CFRRecord.BulkPackingInstructions)], Is.EqualTo(23));
			Assert.That(headerMap[nameof(CFRRecord.BulkPackingProvisions)], Is.EqualTo(24));
			Assert.That(headerMap[nameof(CFRRecord.IsFixedPSN)], Is.EqualTo(26));
			Assert.That(headerMap[nameof(CFRRecord.AppliesForAirTransport)], Is.EqualTo(26));
			Assert.That(headerMap[nameof(CFRRecord.AppliesForDomesticTransport)], Is.EqualTo(26));
			Assert.That(headerMap[nameof(CFRRecord.AppliesForVesselTransport)], Is.EqualTo(26));
			Assert.That(headerMap[nameof(CFRRecord.AppliesForInternationalTransport)], Is.EqualTo(26));
			Assert.That(headerMap[nameof(CFRRecord.RequiresTechnicalNameInParenthesis)], Is.EqualTo(26));
			Assert.That(headerMap[nameof(CFRRecord.ReportableQuantity)], Is.EqualTo(27));
			Assert.That(headerMap[nameof(CFRRecord.ReportableQuantityUnit)], Is.EqualTo(27));
			Assert.That(headerMap[nameof(CFRRecord.StowageCodes)], Is.EqualTo(28));
			Assert.That(headerMap[nameof(CFRRecord.GeneralStowage)], Is.EqualTo(29));
			Assert.That(headerMap[nameof(CFRRecord.Variation)], Is.EqualTo(30));
			Assert.That(headerMap[nameof(CFRRecord.PassengerStowage)], Is.EqualTo(31));
			Assert.That(headerMap[nameof(CFRRecord.TechnicalName)], Is.EqualTo(32));
			Assert.That(headerMap[nameof(CFRRecord.TreatAs)], Is.EqualTo(33));
			Assert.That(headerMap[nameof(CFRRecord.State)], Is.EqualTo(34));
			Assert.That(headerMap[nameof(CFRRecord.PAXAirRailLimitType)], Is.EqualTo(35));
			Assert.That(headerMap[nameof(CFRRecord.PAXAirRailLimit)], Is.EqualTo(35));
			Assert.That(headerMap[nameof(CFRRecord.PAXAirRailLimitUnit)], Is.EqualTo(35));
			Assert.That(headerMap[nameof(CFRRecord.SecondaryPAXAirRailLimit)], Is.EqualTo(35));
			Assert.That(headerMap[nameof(CFRRecord.SecondaryPAXAirRailLimitUnit)], Is.EqualTo(35));
			Assert.That(headerMap[nameof(CFRRecord.CargoAirRailLimitType)], Is.EqualTo(36));
			Assert.That(headerMap[nameof(CFRRecord.CargoAirRailLimit)], Is.EqualTo(36));
			Assert.That(headerMap[nameof(CFRRecord.CargoAirRailLimitUnit)], Is.EqualTo(36));
			Assert.That(headerMap[nameof(CFRRecord.SecondaryCargoAirRailLimit)], Is.EqualTo(36));
			Assert.That(headerMap[nameof(CFRRecord.SecondaryCargoAirRailLimitUnit)], Is.EqualTo(36));
		}

		[Test]
		public void TestParseNonPrefixedUNNO()
		{
			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var cfrFile = Path.Combine(binPath, @"TestFiles\UNDG CFR Single Short UNNO Sample.txt");
			var cfrRecordParser = new CFRRecordParser(Enumerable.Empty<CFRQualifyingDescriptiveTextRecord>());
			var cfrRecords = cfrRecordParser.Parse(cfrFile);

			Assert.That(cfrRecords.Count(), Is.EqualTo(1));
			Assert.That(cfrRecords.First().CFR_UNNO, Is.EqualTo("0076"));
		}
	}
}
