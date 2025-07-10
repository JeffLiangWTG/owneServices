using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class IATARecordParser : CsvDataParser<IATARecord>
	{
		public IATARecordParser()
		{
			HeaderMap = new Dictionary<string, int>
			{
				{ nameof(IATARecord.UnOrIdNumber), 0 },
				{ nameof(IATARecord.ProperShippingName), 1 },
				{ nameof(IATARecord.NOS), 2 },
				{ nameof(IATARecord.HasTechnicalName), 4 },
				{ nameof(IATARecord.QualifyingDescriptiveText), 5 },
				{ nameof(IATARecord.OtherNames), 6 },
				{ nameof(IATARecord.ClassOrDivision), 7 },
				{ nameof(IATARecord.SubRisks), 8 },
				{ nameof(IATARecord.HazardAndHandlingLabels), 9 },
				{ nameof(IATARecord.PackingGroup), 10 },
				{ nameof(IATARecord.PassengerAndCargoLQPackInstruction), 11 },
				{ nameof(IATARecord.PassengerAndCargoLQPackMaxAmt), 12 },
				{ nameof(IATARecord.PassengerAndCargoPackInstruction), 13 },
				{ nameof(IATARecord.PassengerAndCargoPackMaxAmt), 14 },
				{ nameof(IATARecord.CargoPackInstruction), 15 },
				{ nameof(IATARecord.CargoPackMaxAmt), 16 },
				{ nameof(IATARecord.SpecialProvisions), 17 },
				{ nameof(IATARecord.ERGCode), 18 },
				{ nameof(IATARecord.ExceptedQuantity), 22 },
				{ nameof(IATARecord.SpecialHandlingCode1), 23 },
				{ nameof(IATARecord.SpecialHandlingCode2), 24 },
				{ nameof(IATARecord.SpecialHandlingCode3), 25 },
				{ nameof(IATARecord.UniqueRecordId), 27 },
			};
		}

		public override IDictionary<string, int> HeaderMap { get; }

		public override IATARecord ParseRecord(string[] rawDataRow)
		{
			return new IATARecord
			{
				UnOrIdNumber = rawDataRow[HeaderMap[nameof(IATARecord.UnOrIdNumber)]],
				ProperShippingName = rawDataRow[HeaderMap[nameof(IATARecord.ProperShippingName)]],
				NOS = rawDataRow[HeaderMap[nameof(IATARecord.NOS)]] == "1",
				HasTechnicalName = rawDataRow[HeaderMap[nameof(IATARecord.HasTechnicalName)]] == "1",
				QualifyingDescriptiveText = rawDataRow[HeaderMap[nameof(IATARecord.QualifyingDescriptiveText)]],
				OtherNames = rawDataRow[HeaderMap[nameof(IATARecord.OtherNames)]],
				ClassOrDivision = rawDataRow[HeaderMap[nameof(IATARecord.ClassOrDivision)]],
				SubRisks = rawDataRow[HeaderMap[nameof(IATARecord.SubRisks)]],
				HazardAndHandlingLabels = rawDataRow[HeaderMap[nameof(IATARecord.HazardAndHandlingLabels)]],
				PackingGroup = rawDataRow[HeaderMap[nameof(IATARecord.PackingGroup)]],
				PassengerAndCargoLQPackInstruction = rawDataRow[HeaderMap[nameof(IATARecord.PassengerAndCargoLQPackInstruction)]],
				PassengerAndCargoLQPackMaxAmt = rawDataRow[HeaderMap[nameof(IATARecord.PassengerAndCargoLQPackMaxAmt)]],
				PassengerAndCargoPackMaxAmt = rawDataRow[HeaderMap[nameof(IATARecord.PassengerAndCargoPackMaxAmt)]],
				CargoPackInstruction = rawDataRow[HeaderMap[nameof(IATARecord.CargoPackInstruction)]],
				CargoPackMaxAmt = rawDataRow[HeaderMap[nameof(IATARecord.CargoPackMaxAmt)]],
				PassengerAndCargoPackInstruction = rawDataRow[HeaderMap[nameof(IATARecord.PassengerAndCargoPackInstruction)]],
				SpecialProvisions = rawDataRow[HeaderMap[nameof(IATARecord.SpecialProvisions)]],
				ERGCode = rawDataRow[HeaderMap[nameof(IATARecord.ERGCode)]],
				ExceptedQuantity = rawDataRow[HeaderMap[nameof(IATARecord.ExceptedQuantity)]],
				SpecialHandlingCode1 = rawDataRow[HeaderMap[nameof(IATARecord.SpecialHandlingCode1)]],
				SpecialHandlingCode2 = rawDataRow[HeaderMap[nameof(IATARecord.SpecialHandlingCode2)]],
				SpecialHandlingCode3 = rawDataRow[HeaderMap[nameof(IATARecord.SpecialHandlingCode3)]],
				UniqueRecordId = rawDataRow[HeaderMap[nameof(IATARecord.UniqueRecordId)]],
			};
		}

		public IEnumerable<UNDGSubstance> Parse(string filePath)
		{
			var parsedRecords = ParseRecords(filePath, Constants.Encodings.IATAFile);
			var substances = new List<UNDGSubstance>();
			foreach (var record in parsedRecords)
			{
				if (string.IsNullOrEmpty(record.UnOrIdNumber))
				{
					continue;
				}
				substances.Add(ConvertCsvRecordToRealRecord(record));
			}
			IATAParserHelper.CreateIATAVariant(substances);

			return substances;
		}

		static UNDGSubstance ConvertCsvRecordToRealRecord(IATARecord dgIataRecord)
		{
			FilterPackingInstructionsOnRecord(dgIataRecord);

			var result = new UNDGSubstance
			{
				DG_UNNO = ParserHelper.ParseUNNO(dgIataRecord.UnOrIdNumber),
				DG_CargoPackIns = dgIataRecord.CargoPackInstruction,
				DG_Class = dgIataRecord.ClassOrDivision,
				DG_EmergencyResponseGuide = dgIataRecord.ERGCode,
				DG_ExceptedQuantityCode = dgIataRecord.ExceptedQuantity,
				DG_PackIns = dgIataRecord.PassengerAndCargoLQPackInstruction,
				DG_IsNotOtherwiseSpecified = dgIataRecord.NOS,
				DG_PaxPackIns = dgIataRecord.PassengerAndCargoPackInstruction,
				DG_PG = dgIataRecord.PackingGroup,
				DG_PSN = dgIataRecord.ProperShippingName,
				DG_UniqueRecordId = dgIataRecord.UniqueRecordId,
				DG_TechName = dgIataRecord.HasTechnicalName ? "*" : "",
				DG_Hazards = dgIataRecord.HazardAndHandlingLabels
			};

			var (cargoMaxAmt, cargoMaxAmtUq, cargoMaxAmtType) = ParserHelper.SeparateAmtAndUqAndType(dgIataRecord.CargoPackMaxAmt);
			var (lqMaxAmt, lqMaxAmtUq, lqMaxAmtType) = ParserHelper.SeparateAmtAndUqAndType(dgIataRecord.PassengerAndCargoLQPackMaxAmt);
			var (paxMaxAmt, paxMaxAmtUq, paxMaxAmtType) = ParserHelper.SeparateAmtAndUqAndType(dgIataRecord.PassengerAndCargoPackMaxAmt);

			result.DG_CargoMaxAmt = cargoMaxAmt;
			result.DG_CargoMaxAmtUQ = cargoMaxAmtUq;
			result.DG_CargoPackAmtType = cargoMaxAmtType;

			result.DG_LQMaxAmt = lqMaxAmt;
			result.DG_LQMaxAmtUQ = lqMaxAmtUq;
			result.DG_LQMaxAmtType = lqMaxAmtType;

			result.DG_LQ2OrPaxMaxAmt = paxMaxAmt;
			result.DG_LQ2OrPaxMaxAmtUQ = paxMaxAmtUq;
			result.DG_LQ2OrPaxMaxAmtType = paxMaxAmtType;

			var specialHandlingCodes = new[] { dgIataRecord.SpecialHandlingCode1, dgIataRecord.SpecialHandlingCode2, dgIataRecord.SpecialHandlingCode3 };
			if (specialHandlingCodes.Any(x => !string.IsNullOrEmpty(x)))
			{
				result.DG_SpecialHandlingCodes = string.Join(" ", specialHandlingCodes.Where(x => !string.IsNullOrEmpty(x)));
			}

			if (!string.IsNullOrEmpty(dgIataRecord.SubRisks))
			{
				var splitBySpace = dgIataRecord.SubRisks.Split(' ');
				result.DG_SubLabel1 = splitBySpace[0];
				if (splitBySpace.Length > 1)
				{
					result.DG_SubLabel2 = splitBySpace[1];
				}
			}

			result.UNDGAttributes = IATAParserHelper.LoadAttributes(dgIataRecord);
			return result;
		}

		static void FilterPackingInstructionsOnRecord(IATARecord dgIataRecord)
		{
			var instructionsToRemove = new HashSet<string>()
			{
				"See 10.3",
				"See A199"
			};

			switch (dgIataRecord.PassengerAndCargoPackInstruction)
			{
				case var instructionToRemove when instructionsToRemove.Contains(instructionToRemove):
					dgIataRecord.PassengerAndCargoPackInstruction = string.Empty;
					break;

				case var instructionWithSee when instructionWithSee.StartsWith("See ", StringComparison.Ordinal):
					dgIataRecord.PassengerAndCargoPackInstruction = instructionWithSee.Remove(0, "See ".Length);
					break;
			}

			switch (dgIataRecord.CargoPackInstruction)
			{
				case var instructionToRemove when instructionsToRemove.Contains(instructionToRemove):
					dgIataRecord.CargoPackInstruction = string.Empty;
					break;

				case var instructionWithSee when instructionWithSee.StartsWith("See ", StringComparison.Ordinal):
					dgIataRecord.CargoPackInstruction = instructionWithSee.Remove(0, "See ".Length);
					break;
			}
		}
	}
}
