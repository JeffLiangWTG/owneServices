using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class CFRRecordParser : CsvDataParser<CFRRecord>
	{
		readonly IEnumerable<CFRQualifyingDescriptiveTextRecord> _qualifyingDescriptiveTextRecords;

		public CFRRecordParser(IEnumerable<CFRQualifyingDescriptiveTextRecord> qualifyingDescriptiveTextRecords)
		{
			_qualifyingDescriptiveTextRecords = qualifyingDescriptiveTextRecords;

			HeaderMap = new Dictionary<string, int>()
			{
				{ nameof(CFRRecord.UNNO), 0 },
				{ nameof(CFRRecord.Variant), 1 },
				{ nameof(CFRRecord.Prefix), 3 },
				{ nameof(CFRRecord.PrimaryClass), 4 },
				{ nameof(CFRRecord.SecondaryClass), 5 },
				{ nameof(CFRRecord.TertiaryClass), 6 },
				{ nameof(CFRRecord.PackingGroup), 7 },
				{ nameof(CFRRecord.PSN), 8 },
				{ nameof(CFRRecord.MarinePollutant), 9 },
				{ nameof(CFRRecord.SpecialProvisions), 10 },
				{ nameof(CFRRecord.ExceptedQuantity), 11 },
				{ nameof(CFRRecord.LimitedQuantityPermitted), 12 },
				{ nameof(CFRRecord.LimitedQuantity), 12 },
				{ nameof(CFRRecord.LimitedQuantityUnit), 12 },
				{ nameof(CFRRecord.PackingExceptions), 13 },
				{ nameof(CFRRecord.PackingInstructions), 14 },
				{ nameof(CFRRecord.PackingProvisions), 15 },
				{ nameof(CFRRecord.IBCInstructions), 16 },
				{ nameof(CFRRecord.IBCProvisions), 17 },
				{ nameof(CFRRecord.TankInstructions), 18 },
				{ nameof(CFRRecord.TankProvisions), 19 },
				{ nameof(CFRRecord.StowageCategory), 20 },
				{ nameof(CFRRecord.PoisonInhalationHazard), 21 },
				{ nameof(CFRRecord.EmergencyResponseGuide), 22 },
				{ nameof(CFRRecord.BulkPackingInstructions), 23 },
				{ nameof(CFRRecord.BulkPackingProvisions), 24 },
				{ nameof(CFRRecord.IsFixedPSN), 26 },
				{ nameof(CFRRecord.AppliesForAirTransport), 26 },
				{ nameof(CFRRecord.AppliesForDomesticTransport), 26 },
				{ nameof(CFRRecord.AppliesForVesselTransport), 26 },
				{ nameof(CFRRecord.AppliesForInternationalTransport), 26 },
				{ nameof(CFRRecord.RequiresTechnicalNameInParenthesis), 26 },
				{ nameof(CFRRecord.ReportableQuantity), 27 },
				{ nameof(CFRRecord.ReportableQuantityUnit), 27 },
				{ nameof(CFRRecord.StowageCodes), 28 },
				{ nameof(CFRRecord.GeneralStowage), 29 },
				{ nameof(CFRRecord.Variation), 30 },
				{ nameof(CFRRecord.PassengerStowage), 31 },
				{ nameof(CFRRecord.TechnicalName), 32 },
				{ nameof(CFRRecord.TreatAs), 33 },
				{ nameof(CFRRecord.State), 34 },
				{ nameof(CFRRecord.PAXAirRailLimitType), 35 },
				{ nameof(CFRRecord.PAXAirRailLimit), 35 },
				{ nameof(CFRRecord.PAXAirRailLimitUnit), 35 },
				{ nameof(CFRRecord.SecondaryPAXAirRailLimit), 35 },
				{ nameof(CFRRecord.SecondaryPAXAirRailLimitUnit), 35 },
				{ nameof(CFRRecord.CargoAirRailLimitType), 36 },
				{ nameof(CFRRecord.CargoAirRailLimit), 36 },
				{ nameof(CFRRecord.CargoAirRailLimitUnit), 36 },
				{ nameof(CFRRecord.SecondaryCargoAirRailLimit), 36 },
				{ nameof(CFRRecord.SecondaryCargoAirRailLimitUnit), 36 },
			};
		}

		public override IDictionary<string, int> HeaderMap { get; }

		public override CFRRecord ParseRecord(string[] rawDataRow)
		{
			return new CFRRecord
			{
				UNNO = ParserHelper.ParseUNNO(rawDataRow[HeaderMap[nameof(CFRRecord.UNNO)]]),
				Variant = rawDataRow[HeaderMap[nameof(CFRRecord.Variant)]],
				Prefix = rawDataRow[HeaderMap[nameof(CFRRecord.Prefix)]],
				PSN = rawDataRow[HeaderMap[nameof(CFRRecord.PSN)]],
				Variation = rawDataRow[HeaderMap[nameof(CFRRecord.Variation)]],
				PrimaryClass = CFRParserHelper.ParsePrimaryClass(rawDataRow[HeaderMap[nameof(CFRRecord.PrimaryClass)]]),
				SecondaryClass = CFRParserHelper.ParseSecondaryClass(rawDataRow[HeaderMap[nameof(CFRRecord.SecondaryClass)]]),
				TertiaryClass = CFRParserHelper.ParseTertiaryClass(rawDataRow[HeaderMap[nameof(CFRRecord.TertiaryClass)]]),
				MarinePollutant = rawDataRow[HeaderMap[nameof(CFRRecord.MarinePollutant)]],
				ExceptedQuantity = CFRParserHelper.ParseExceptedQuantityValue(rawDataRow[HeaderMap[nameof(CFRRecord.ExceptedQuantity)]]),
				LimitedQuantityPermitted = CFRParserHelper.ParseLimitedQuantityPermitted(rawDataRow[HeaderMap[nameof(CFRRecord.LimitedQuantityPermitted)]]),
				LimitedQuantity = CFRParserHelper.ParseLimitedQuantityValue(rawDataRow[HeaderMap[nameof(CFRRecord.LimitedQuantity)]]),
				LimitedQuantityUnit = CFRParserHelper.ParseLimitedQuantityUnit(rawDataRow[HeaderMap[nameof(CFRRecord.LimitedQuantityUnit)]]),
				ReportableQuantity = CFRParserHelper.ParseReportableQuantityValue(rawDataRow[HeaderMap[nameof(CFRRecord.ReportableQuantity)]]),
				ReportableQuantityUnit = "lb",
				GeneralStowage = rawDataRow[HeaderMap[nameof(CFRRecord.GeneralStowage)]],
				PassengerStowage = rawDataRow[HeaderMap[nameof(CFRRecord.PassengerStowage)]],
				StowageCategory = rawDataRow[HeaderMap[nameof(CFRRecord.StowageCategory)]],
				StowageCodes = rawDataRow[HeaderMap[nameof(CFRRecord.StowageCodes)]],
				BulkPackingInstructions = rawDataRow[HeaderMap[nameof(CFRRecord.BulkPackingInstructions)]],
				BulkPackingProvisions = rawDataRow[HeaderMap[nameof(CFRRecord.BulkPackingProvisions)]],
				IBCInstructions = rawDataRow[HeaderMap[nameof(CFRRecord.IBCInstructions)]],
				IBCProvisions = rawDataRow[HeaderMap[nameof(CFRRecord.IBCProvisions)]],
				PackingExceptions = rawDataRow[HeaderMap[nameof(CFRRecord.PackingExceptions)]],
				PackingInstructions = rawDataRow[HeaderMap[nameof(CFRRecord.PackingInstructions)]],
				PackingProvisions = rawDataRow[HeaderMap[nameof(CFRRecord.PackingProvisions)]],
				PackingGroup = rawDataRow[HeaderMap[nameof(CFRRecord.PackingGroup)]],
				SpecialProvisions = rawDataRow[HeaderMap[nameof(CFRRecord.SpecialProvisions)]],
				TankInstructions = rawDataRow[HeaderMap[nameof(CFRRecord.TankInstructions)]],
				TankProvisions = rawDataRow[HeaderMap[nameof(CFRRecord.TankProvisions)]],
				PoisonInhalationHazard = rawDataRow[HeaderMap[nameof(CFRRecord.PoisonInhalationHazard)]],
				State = rawDataRow[HeaderMap[nameof(CFRRecord.State)]],
				IsFixedPSN = CFRParserHelper.ParseIsPSNFixed(rawDataRow[HeaderMap[nameof(CFRRecord.IsFixedPSN)]]),
				AppliesForAirTransport = CFRParserHelper.ParseAppliesForAirTransport(rawDataRow[HeaderMap[nameof(CFRRecord.AppliesForDomesticTransport)]]),
				AppliesForDomesticTransport = CFRParserHelper.ParseAppliesForDomesticTransport(rawDataRow[HeaderMap[nameof(CFRRecord.AppliesForDomesticTransport)]]),
				AppliesForInternationalTransport = CFRParserHelper.ParseAppliesForInternationalTransport(rawDataRow[HeaderMap[nameof(CFRRecord.AppliesForInternationalTransport)]]),
				AppliesForVesselTransport = CFRParserHelper.ParseAppliesForVesselTransport(rawDataRow[HeaderMap[nameof(CFRRecord.AppliesForVesselTransport)]]),
				RequiresTechnicalNameInParenthesis = CFRParserHelper.ParseRequiresTechnicalNameInParenthesis(rawDataRow[HeaderMap[nameof(CFRRecord.RequiresTechnicalNameInParenthesis)]]),
				EmergencyResponseGuide = CFRParserHelper.ParseEmergencyResponseGuideline(rawDataRow[HeaderMap[nameof(CFRRecord.EmergencyResponseGuide)]]),
				TechnicalName = rawDataRow[HeaderMap[nameof(CFRRecord.TechnicalName)]],
				TreatAs = rawDataRow[HeaderMap[nameof(CFRRecord.TreatAs)]],
				PAXAirRailLimitType = CFRParserHelper.ParsePAXAirRailLimitType(rawDataRow[HeaderMap[nameof(CFRRecord.PAXAirRailLimitType)]]),
				PAXAirRailLimit = CFRParserHelper.ParsePAXAirRailLimitValue(rawDataRow[HeaderMap[nameof(CFRRecord.PAXAirRailLimit)]], 0),
				PAXAirRailLimitUnit = CFRParserHelper.ParsePAXAirRailLimitUnit(rawDataRow[HeaderMap[nameof(CFRRecord.PAXAirRailLimitUnit)]], 0),
				SecondaryPAXAirRailLimit = CFRParserHelper.ParsePAXAirRailLimitValue(rawDataRow[HeaderMap[nameof(CFRRecord.SecondaryPAXAirRailLimit)]], 1),
				SecondaryPAXAirRailLimitUnit = CFRParserHelper.ParsePAXAirRailLimitUnit(rawDataRow[HeaderMap[nameof(CFRRecord.SecondaryPAXAirRailLimitUnit)]], 1),
				CargoAirRailLimitType = CFRParserHelper.ParseCargoAirRailLimitType(rawDataRow[HeaderMap[nameof(CFRRecord.CargoAirRailLimitType)]]),
				CargoAirRailLimit = CFRParserHelper.ParseCargoAirRailLimitValue(rawDataRow[HeaderMap[nameof(CFRRecord.CargoAirRailLimit)]], 0),
				CargoAirRailLimitUnit = CFRParserHelper.ParseCargoAirRailLimitUnit(rawDataRow[HeaderMap[nameof(CFRRecord.CargoAirRailLimitUnit)]], 0),
				SecondaryCargoAirRailLimit = CFRParserHelper.ParseCargoAirRailLimitValue(rawDataRow[HeaderMap[nameof(CFRRecord.SecondaryCargoAirRailLimit)]], 1),
				SecondaryCargoAirRailLimitUnit = CFRParserHelper.ParseCargoAirRailLimitUnit(rawDataRow[HeaderMap[nameof(CFRRecord.SecondaryCargoAirRailLimitUnit)]], 1),
			};
		}

		public IEnumerable<UNDGSubstanceCFR> Parse(string filePath)
		{
			var parsedRecords = ParseRecords(filePath, Constants.Encodings.CFRZipFile, true);

			foreach (var record in parsedRecords.Where(x => string.Compare(x.Prefix, "XX", StringComparison.Ordinal) != 0
				&& string.Compare(x.Prefix, "OR", StringComparison.Ordinal) != 0))
			{
				yield return ConvertCSVRecordToUNDGSubstanceCSV(record);
			}
		}

		UNDGSubstanceCFR ConvertCSVRecordToUNDGSubstanceCSV(CFRRecord record)
		{
			var result = new UNDGSubstanceCFR
			{
				CFR_AppliesForAirTransport = record.AppliesForAirTransport,
				CFR_AppliesForDomesticTransport = record.AppliesForDomesticTransport,
				CFR_AppliesForInternationalTransport = record.AppliesForInternationalTransport,
				CFR_AppliesForVesselTransport = record.AppliesForVesselTransport,
				CFR_BulkPackingInstructions = ParserHelper.CheckFieldLength(record.BulkPackingInstructions, record.UNNO, 12),
				CFR_BulkPackingProvisions = ParserHelper.CheckFieldLength(record.BulkPackingProvisions, record.UNNO, 30),
				CFR_CargoAirRailLimit = Convert.ToDecimal(record.CargoAirRailLimit, CultureInfo.InvariantCulture),
				CFR_CargoAirRailLimitUnit = ParserHelper.CheckFieldLength(record.CargoAirRailLimitUnit, record.UNNO, 2),
				CFR_SecondaryCargoAirRailLimit = Convert.ToDecimal(record.SecondaryCargoAirRailLimit, CultureInfo.InvariantCulture),
				CFR_SecondaryCargoAirRailLimitUnit = ParserHelper.CheckFieldLength(record.SecondaryCargoAirRailLimitUnit, record.UNNO, 2),
				CFR_CargoAirRailLimitType = ParserHelper.CheckFieldLength(record.CargoAirRailLimitType, record.UNNO, 3),
				CFR_CVL = string.Empty,
				CFR_EmergencyResponseGuide = ParserHelper.CheckFieldLength(record.EmergencyResponseGuide, record.UNNO, 3),
				CFR_ExceptedQuantity = ParserHelper.CheckFieldLength(record.ExceptedQuantity, record.UNNO, 2),
				CFR_GeneralStowage = ParserHelper.CheckFieldLength(record.GeneralStowage, record.UNNO, 8),
				CFR_IBCInstructions = ParserHelper.CheckFieldLength(record.IBCInstructions, record.UNNO, 3),
				CFR_IBCProvisions = ParserHelper.CheckFieldLength(record.IBCProvisions, record.UNNO, 12),
				CFR_IsFixedPSN = record.IsFixedPSN,
				CFR_LimitedQuantityPermitted = record.LimitedQuantityPermitted,
				CFR_LQMaxAmt = Convert.ToDecimal(record.LimitedQuantity, CultureInfo.InvariantCulture),
				CFR_LQMaxAmtUQ = record.LimitedQuantityUnit,
				CFR_MarinePollutant = ParserHelper.CheckFieldLength(record.MarinePollutant, record.UNNO, 1),
				CFR_PackingExceptions = ParserHelper.CheckFieldLength(record.PackingExceptions, record.UNNO, 12),
				CFR_PackingGroup = ParserHelper.CheckFieldLength(record.PackingGroup, record.UNNO, 3),
				CFR_PackingInstructions = ParserHelper.CheckFieldLength(record.PackingInstructions, record.UNNO, 13),
				CFR_PackingProvisions = ParserHelper.CheckFieldLength(record.PackingProvisions, record.UNNO, 16),
				CFR_PassengerStowage = ParserHelper.CheckFieldLength(record.PassengerStowage, record.UNNO, 8),
				CFR_PAXAirRailLimit = Convert.ToDecimal(record.PAXAirRailLimit, CultureInfo.InvariantCulture),
				CFR_PAXAirRailLimitUnit = ParserHelper.CheckFieldLength(record.PAXAirRailLimitUnit, record.UNNO, 2),
				CFR_SecondaryPAXAirRailLimit = Convert.ToDecimal(record.SecondaryPAXAirRailLimit, CultureInfo.InvariantCulture),
				CFR_SecondaryPAXAirRailLimitUnit = ParserHelper.CheckFieldLength(record.SecondaryPAXAirRailLimitUnit, record.UNNO, 2),
				CFR_PAXAirRailLimitType = ParserHelper.CheckFieldLength(record.PAXAirRailLimitType, record.UNNO, 3),
				CFR_PoisonInhalationHazard = ParserHelper.CheckFieldLength(record.PoisonInhalationHazard, record.UNNO, 1),
				CFR_Prefix = ParserHelper.CheckFieldLength(record.Prefix, record.UNNO, 2),
				CFR_PrimaryClass = ParserHelper.CheckFieldLength(record.PrimaryClass, record.UNNO, 4),
				CFR_PSN = ParserHelper.CheckFieldLength(record.PSN, record.UNNO, 205),
				CFR_ReportableQuantity = Convert.ToDecimal(record.ReportableQuantity, CultureInfo.InvariantCulture),
				CFR_ReportableQuantityUnit = ParserHelper.CheckFieldLength(record.ReportableQuantityUnit, record.UNNO, 2),
				CFR_RequiresTechnicalNameInParenthesis = record.RequiresTechnicalNameInParenthesis,
				CFR_SecondaryClass = ParserHelper.CheckFieldLength(record.SecondaryClass, record.UNNO, 3),
				CFR_SpecialProvisions = ParserHelper.CheckFieldLength(record.SpecialProvisions, record.UNNO, 24),
				CFR_State = ParserHelper.CheckFieldLength(record.State, record.UNNO, 1),
				CFR_StowageCategory = ParserHelper.CheckFieldLength(record.StowageCategory, record.UNNO, 2),
				CFR_StowageCodes = ParserHelper.CheckFieldLength(record.StowageCodes, record.UNNO, 36),
				CFR_StowageIMDGCodes = string.Empty,
				CFR_TankInstructions = ParserHelper.CheckFieldLength(record.TankInstructions, record.UNNO, 3),
				CFR_TankProvisions = ParserHelper.CheckFieldLength(record.TankProvisions, record.UNNO, 30),
				CFR_TechnicalName = ParserHelper.CheckFieldLength(record.TechnicalName, record.UNNO, 1),
				CFR_TertiaryClass = ParserHelper.CheckFieldLength(record.TertiaryClass, record.UNNO, 1),
				CFR_TreatAs = ParserHelper.CheckFieldLength(record.TreatAs, record.UNNO, 3),
				CFR_UNNO = ParserHelper.CheckFieldLength(record.UNNO, record.UNNO, 4),
				CFR_Variant = ParserHelper.CheckFieldLength(record.Variant, record.UNNO, 2),
				CFR_Variation = ParserHelper.CheckFieldLength(record.Variation, record.UNNO, 150),
			};

			var qualifyingDescriptiveTextRecord = _qualifyingDescriptiveTextRecords.FirstOrDefault(x => (x.Unno == record.UNNO && x.Variant == record.Variant));
			if (qualifyingDescriptiveTextRecord != null)
			{
				result.UNDGAttributeZZs = ParserHelper.LoadAttributesZZ(qualifyingDescriptiveTextRecord.QualifyingDescriptiveText);
			}

			return result;
		}
	}
}
