using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class ADNRecordParser : CsvDataParser<ADNRecord>
	{
		public IEnumerable<UNDGSubstanceADN> Parse(string filePath)
		{
			var parsedRecords = ParseRecords(filePath, Constants.Encodings.ADNFile, true);
			var substances = new List<UNDGSubstanceADN>();
			foreach (var record in parsedRecords.Where(x => !string.IsNullOrEmpty(x.UnId)))
			{
				substances.AddRange(ConvertCsvRecordToUndgSubstanceADN(record));
			}

			ADNParserHelper.CreateADNVariant(substances);
			return substances;
		}

		static UNDGSubstanceADN[] ConvertCsvRecordToUndgSubstanceADN(ADNRecord record)
		{
			var nameAndDescription = ADNParserHelper.ParseNameAndDescription(record.NameAndDescription);
			var substances = new UNDGSubstanceADN[nameAndDescription.Length];
			for (var i = 0; i < nameAndDescription.Length; i++)
			{
				var lqs = ParserHelper.TransformLQMaxAmts(record.LQMaxAmt);

				substances[i] = new UNDGSubstanceADN
				{
					ADN_UNNO = ParserHelper.CheckFieldLength(record.UnId, record.UnId, 4),
					ADN_PSN = ParserHelper.CheckFieldLength(nameAndDescription[i].Item1, record.UnId, 200),
					ADN_Class = ParserHelper.CheckFieldLength(record.Class, record.UnId, 4),
					ADN_ClassificationCode = ParserHelper.CheckFieldLength(ParserHelper.TransformClassificationCode(record.ClassficationCode), record.UnId, 20),
					ADN_PG = ParserHelper.CheckFieldLength(record.PG, record.UnId, 3),
					ADN_Labels = ParserHelper.CheckFieldLength(ParserHelper.SeeContentCleanUp(record.Label), record.UnId, 20),
					ADN_ExceptedQuantityCode = ParserHelper.CheckFieldLength(ParserHelper.SeeContentCleanUp(record.ExceptedQuantityCode), record.UnId, 2),
					ADN_LQMaxAmt = lqs[0].Item1,
					ADN_LQMaxAmtUQ = ParserHelper.CheckFieldLength(lqs[0].Item2, record.UnId, 2),
					ADN_LQ2MaxAmt = lqs.Length > 1 ? lqs[1].Item1 : 0,
					ADN_LQ2MaxAmtUQ = lqs.Length > 1 ? ParserHelper.CheckFieldLength(lqs[1].Item2, record.UnId, 2) : string.Empty,
					ADN_SpecialProvisions = ParserHelper.CheckFieldLength(record.SpecialProvisions, record.UnId, 40),
					ADN_OperationSpecialProv = ParserHelper.CheckFieldLength(record.OperationSpecialProv, record.UnId, 20),
					ADN_UnloadingSpecialProv = ParserHelper.CheckFieldLength(record.UnloadingSpecialProv, record.UnId, 20),
					ADN_LoadingSpecialProv = ParserHelper.CheckFieldLength(record.LoadingSpecialProv, record.UnId, 20),
					ADN_Ventilation = ParserHelper.CheckFieldLength(record.Ventilation, record.UnId, 40),
					ADN_EquipmentDetails = record.EquipmentRequired.Contains("***") ? "transport in bulk" : string.Empty,
					ADN_EquipBreathingApparatus = record.EquipmentRequired.Contains("A"),
					ADN_EquipToximeter = record.EquipmentRequired.Contains("TOX"),
					ADN_EquipGasDetector = record.EquipmentRequired.Contains("EX"),
					ADN_EquipEscapeDevice = record.EquipmentRequired.Contains("EP"),
					ADN_EquipPPE = record.EquipmentRequired.Contains("PP"),
					ADN_BlueCones = ADNParserHelper.ConvertStringToByte(record.BlueCones),
				};
				substances[i].UNDGAttributeZZs = ParserHelper.LoadAttributesZZ(nameAndDescription[i].Item2);

				ParseCarriagePermitted(record, substances[i]);
				ParseRemarks(record, substances[i]);
			}

			return substances;
		}

		static void ParseCarriagePermitted(ADNRecord record, UNDGSubstanceADN substance)
		{
			var carriagePermitted = record.CarriagePermitted;

			if (string.IsNullOrEmpty(carriagePermitted))
			{
				substance.ADN_CarriagePermittedPacks = true;
			}
			else if (carriagePermitted == "T" || carriagePermitted == "T*" || carriagePermitted == "T *")
			{
				substance.ADN_CarriagePermittedTanks = true;
			}
			else if (carriagePermitted == "B")
			{
				substance.ADN_CarriagePermittedBulk = true;
			}
			else if (carriagePermitted == "T* B**")
			{
				substance.ADN_CarriagePermittedTanks = true;
				substance.ADN_CarriagePermittedBulk = true;
			}
			else
			{
				substance.ADN_CarriagePermittedDetails = ParserHelper.CheckFieldLength(carriagePermitted, record.UnId, 35);
			}
		}

		static void ParseRemarks(ADNRecord record, UNDGSubstanceADN substance)
		{
			if (!string.IsNullOrEmpty(record.Remarks))
			{
				if (!string.IsNullOrEmpty(record.LoadingSpecialProv))
				{
					substance.ADN_LoadingSpecialProvNote = ParserHelper.CheckFieldLength(record.Remarks, record.UnId, 150);
				}

				if (!string.IsNullOrEmpty(record.UnloadingSpecialProv))
				{
					substance.ADN_UnloadingSpecialProvNote = ParserHelper.CheckFieldLength(record.Remarks, record.UnId, 150);
				}

				if (!string.IsNullOrEmpty(record.OperationSpecialProv))
				{
					substance.ADN_OperationSpecialProvNote = ParserHelper.CheckFieldLength(record.Remarks, record.UnId, 150);
				}
			}
		}

		public override IDictionary<string, int> HeaderMap { get; }

		public ADNRecordParser()
		{
			HeaderMap = new Dictionary<string, int>()
			{
				{ nameof(ADNRecord.UnId), ADNParserHelper.ConvertCSVIndexToInt("A") },
				{ nameof(ADNRecord.NameAndDescription), ADNParserHelper.ConvertCSVIndexToInt("B") },
				{ nameof(ADNRecord.Class), ADNParserHelper.ConvertCSVIndexToInt("C") },
				{ nameof(ADNRecord.ClassficationCode), ADNParserHelper.ConvertCSVIndexToInt("D") },
				{ nameof(ADNRecord.PG), ADNParserHelper.ConvertCSVIndexToInt("E") },
				{ nameof(ADNRecord.Label), ADNParserHelper.ConvertCSVIndexToInt("F") },
				{ nameof(ADNRecord.SpecialProvisions), ADNParserHelper.ConvertCSVIndexToInt("G") },
				{ nameof(ADNRecord.LQMaxAmt), ADNParserHelper.ConvertCSVIndexToInt("H") },
				{ nameof(ADNRecord.ExceptedQuantityCode), ADNParserHelper.ConvertCSVIndexToInt("I") },
				{ nameof(ADNRecord.CarriagePermitted), ADNParserHelper.ConvertCSVIndexToInt("J") },
				{ nameof(ADNRecord.EquipmentRequired), ADNParserHelper.ConvertCSVIndexToInt("K") },
				{ nameof(ADNRecord.Ventilation), ADNParserHelper.ConvertCSVIndexToInt("L") },
				{ nameof(ADNRecord.LoadingSpecialProv), ADNParserHelper.ConvertCSVIndexToInt("M") },
				{ nameof(ADNRecord.UnloadingSpecialProv), ADNParserHelper.ConvertCSVIndexToInt("N") },
				{ nameof(ADNRecord.OperationSpecialProv), ADNParserHelper.ConvertCSVIndexToInt("O") },
				{ nameof(ADNRecord.BlueCones), ADNParserHelper.ConvertCSVIndexToInt("P") },
				{ nameof(ADNRecord.Remarks), ADNParserHelper.ConvertCSVIndexToInt("Q") },
			};
		}
		public override ADNRecord ParseRecord(string[] rawDataRow)
		{
			return new ADNRecord
			{
				UnId = ParserHelper.ParseUNNO(rawDataRow[HeaderMap[nameof(ADNRecord.UnId)]]),
				NameAndDescription = rawDataRow[HeaderMap[nameof(ADNRecord.NameAndDescription)]],
				Class = rawDataRow[HeaderMap[nameof(ADNRecord.Class)]],
				ClassficationCode = rawDataRow[HeaderMap[nameof(ADNRecord.ClassficationCode)]],
				PG = rawDataRow[HeaderMap[nameof(ADNRecord.PG)]],
				Label = rawDataRow[HeaderMap[nameof(ADNRecord.Label)]],
				SpecialProvisions = rawDataRow[HeaderMap[nameof(ADNRecord.SpecialProvisions)]],
				LQMaxAmt = rawDataRow[HeaderMap[nameof(ADNRecord.LQMaxAmt)]],
				ExceptedQuantityCode = rawDataRow[HeaderMap[nameof(ADNRecord.ExceptedQuantityCode)]],
				CarriagePermitted = rawDataRow[HeaderMap[nameof(ADNRecord.CarriagePermitted)]],
				EquipmentRequired = rawDataRow[HeaderMap[nameof(ADNRecord.EquipmentRequired)]],
				Ventilation = rawDataRow[HeaderMap[nameof(ADNRecord.Ventilation)]],
				LoadingSpecialProv = rawDataRow[HeaderMap[nameof(ADNRecord.LoadingSpecialProv)]],
				UnloadingSpecialProv = rawDataRow[HeaderMap[nameof(ADNRecord.UnloadingSpecialProv)]],
				OperationSpecialProv = rawDataRow[HeaderMap[nameof(ADNRecord.OperationSpecialProv)]],
				BlueCones = rawDataRow[HeaderMap[nameof(ADNRecord.BlueCones)]],
				Remarks = rawDataRow[HeaderMap[nameof(ADNRecord.Remarks)]],
			};
		}
	}
}
