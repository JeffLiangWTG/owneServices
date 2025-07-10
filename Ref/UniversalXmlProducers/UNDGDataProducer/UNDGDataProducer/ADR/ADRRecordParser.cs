using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class ADRRecordParser : CsvDataParser<ADRRecord>
	{
		public IEnumerable<UNDGSubstanceADR> Parse(string filePath)
		{
			var parsedRecords = ParseRecords(filePath, Constants.Encodings.ADRFile, true);
			foreach (var record in parsedRecords.Where(x => !string.IsNullOrEmpty(x.UnId)))
			{
				yield return ConvertCsvRecordToUndgSubstanceADR(record);
			}
		}

		static UNDGSubstanceADR ConvertCsvRecordToUndgSubstanceADR(ADRRecord record)
		{
			SanitiseRecord(record);

			var result = new UNDGSubstanceADR
			{
				ADR_UNNO = ParserHelper.CheckFieldLength(record.UnId, record.UnId, 4),
				ADR_Variant = ParserHelper.CheckFieldLength(record.Var, record.UnId, 2),
				ADR_PSN = ParserHelper.CheckFieldLength(record.NameADR, record.UnId, 260),
				ADR_Class = ParserHelper.CheckFieldLength(record.ClassADR, record.UnId, 4),
				ADR_ClassificationCode = ParserHelper.CheckFieldLength(ParserHelper.TransformClassificationCode(record.CCodeADR), record.UnId, 20),
				ADR_PG = ParserHelper.CheckFieldLength(record.PgADR, record.UnId, 3),
				ADR_Labels = ParserHelper.CheckFieldLength(record.LabelADR, record.UnId, 20),
				ADR_SpecialProvisions = ParserHelper.CheckFieldLength(record.SProvADR, record.UnId, 40),
				ADR_LQMaxAmt = ParserHelper.TransformLQMaxAmt(record.LqADR),
				ADR_LQMaxAmtUQ = ParserHelper.CheckFieldLength(ParserHelper.TransformLQMaxAmtUQ(record.LqADR), record.UnId, 2),
				ADR_ExceptedQuantityCode = record.EqADR,
				ADR_PackIns = ParserHelper.CheckFieldLength(record.PiADR, record.UnId, 50),
				ADR_PackProv = ParserHelper.CheckFieldLength(record.PpADR, record.UnId, 50),
				ADR_MixedPackingProv = ParserHelper.CheckFieldLength(record.MixpADR, record.UnId, 50),
				ADR_ADRTankSpecProv = ParserHelper.CheckFieldLength(record.AdrtpADR, record.UnId, 80),
				ADR_BulkTankSpecProv = ParserHelper.CheckFieldLength(record.TpADR, record.UnId, 50),
				ADR_ADRTankCode = ParserHelper.CheckFieldLength(record.AdrtADR, record.UnId, 42),
				ADR_BulkTankIns = ParserHelper.CheckFieldLength(record.TiADR, record.UnId, 62),
				ADR_TankVehicle = ParserHelper.CheckFieldLength(record.TankvADR, record.UnId, 26),
				ADR_TransportCategory = ParserHelper.CheckFieldLength(ParserHelper.TransformTransportCategory(record.TCatADR, record.UnId), record.UnId, 15),
				ADR_PackingSpecialProv = ParserHelper.CheckFieldLength(record.PProvADR, record.UnId, 18),
				ADR_BulkSpecialProv = ParserHelper.CheckFieldLength(record.BProvADR, record.UnId, 20),
				ADR_LoadingSpecialProv = ParserHelper.CheckFieldLength(record.LProvADR, record.UnId, 48),
				ADR_OperationSpecialProv = ParserHelper.CheckFieldLength(record.OProvADR, record.UnId, 36),
				ADR_HazardIDNumber = ParserHelper.CheckFieldLength(record.HinADR, record.UnId, 8),
			};
			result.UNDGAttributeZZs = ParserHelper.LoadAttributesZZ(record.QdtADR);

			if (record.PiADR == record.PpADR)
			{
				result.ADR_PackIns = string.Empty;
			}
			return result;
		}

		static void SanitiseRecord(ADRRecord adrRecord)
		{
			SanitiseExceptedQuantityCode(adrRecord);
			SanitiseADRLabels(adrRecord);
		}

		static void SanitiseExceptedQuantityCode(ADRRecord adrRecord)
		{
			adrRecord.EqADR = ParserHelper.SeeContentCleanUp(adrRecord.EqADR);

			if (adrRecord.EqADR.Length > 2)
			{
				Console.Error.WriteLine($"Field discarded because source data exceeded maximum field size. Max Length: 2 Value: {adrRecord.EqADR} UNID: {adrRecord.UnId}");
				adrRecord.EqADR = string.Empty;
			}
		}

		static void SanitiseADRLabels(ADRRecord adrRecord)
		{
			var instructionsToRemove = new HashSet<string>()
			{
				"See 5.2.2.1.12"
			};

			if (adrRecord.LabelADR.ToUpper(CultureInfo.InvariantCulture) == "NONE")
			{
				adrRecord.LabelADR = string.Empty;
			}
			else if (instructionsToRemove.Contains(adrRecord.LabelADR))
			{
				adrRecord.LabelADR = string.Empty;
			}
		}

		public ADRRecordParser()
		{
			HeaderMap = new Dictionary<string, int>()
			{
				{ nameof(ADRRecord.UnId), 0 },
				{ nameof(ADRRecord.Var), 1 },
				{ nameof(ADRRecord.NameADR), 2 },
				{ nameof(ADRRecord.QdtADR), 3 },
				{ nameof(ADRRecord.CCodeADR), 4 },
				{ nameof(ADRRecord.ClassADR), 5 },
				{ nameof(ADRRecord.LabelADR), 6 },
				{ nameof(ADRRecord.PgADR), 7 },
				{ nameof(ADRRecord.LqADR), 8 },
				{ nameof(ADRRecord.SProvADR), 9 },
				{ nameof(ADRRecord.EqADR), 10 },
				{ nameof(ADRRecord.PiADR), 11 },
				{ nameof(ADRRecord.MixpADR), 12 },
				{ nameof(ADRRecord.PpADR), 13 },
				{ nameof(ADRRecord.TiADR), 14 },
				{ nameof(ADRRecord.AdrtADR), 15 },
				{ nameof(ADRRecord.TpADR), 16 },
				{ nameof(ADRRecord.AdrtpADR), 17 },
				{ nameof(ADRRecord.TankvADR), 18 },
				{ nameof(ADRRecord.TCatADR), 19 },
				{ nameof(ADRRecord.PProvADR), 20 },
				{ nameof(ADRRecord.BProvADR), 21 },
				{ nameof(ADRRecord.LProvADR), 22 },
				{ nameof(ADRRecord.OProvADR), 23 },
				{ nameof(ADRRecord.HinADR), 24 },
			};
		}

		public override IDictionary<string, int> HeaderMap { get; }

		public override ADRRecord ParseRecord(string[] rawDataRow)
		{
			return new ADRRecord
			{
				UnId = ParserHelper.ParseUNNO(rawDataRow[HeaderMap[nameof(ADRRecord.UnId)]]),
				Var = rawDataRow[HeaderMap[nameof(ADRRecord.Var)]],
				NameADR = rawDataRow[HeaderMap[nameof(ADRRecord.NameADR)]],
				QdtADR = rawDataRow[HeaderMap[nameof(ADRRecord.QdtADR)]],
				ClassADR = rawDataRow[HeaderMap[nameof(ADRRecord.ClassADR)]],
				CCodeADR = rawDataRow[HeaderMap[nameof(ADRRecord.CCodeADR)]],
				PgADR = rawDataRow[HeaderMap[nameof(ADRRecord.PgADR)]],
				LabelADR = rawDataRow[HeaderMap[nameof(ADRRecord.LabelADR)]],
				SProvADR = rawDataRow[HeaderMap[nameof(ADRRecord.SProvADR)]],
				LqADR = rawDataRow[HeaderMap[nameof(ADRRecord.LqADR)]],
				EqADR = rawDataRow[HeaderMap[nameof(ADRRecord.EqADR)]],
				PiADR = rawDataRow[HeaderMap[nameof(ADRRecord.PiADR)]],
				PpADR = rawDataRow[HeaderMap[nameof(ADRRecord.PpADR)]],
				MixpADR = rawDataRow[HeaderMap[nameof(ADRRecord.MixpADR)]],
				TiADR = rawDataRow[HeaderMap[nameof(ADRRecord.TiADR)]],
				TpADR = rawDataRow[HeaderMap[nameof(ADRRecord.TpADR)]],
				AdrtADR = rawDataRow[HeaderMap[nameof(ADRRecord.AdrtADR)]],
				AdrtpADR = rawDataRow[HeaderMap[nameof(ADRRecord.AdrtpADR)]],
				TankvADR = rawDataRow[HeaderMap[nameof(ADRRecord.TankvADR)]],
				TCatADR = rawDataRow[HeaderMap[nameof(ADRRecord.TCatADR)]],
				PProvADR = rawDataRow[HeaderMap[nameof(ADRRecord.PProvADR)]],
				BProvADR = rawDataRow[HeaderMap[nameof(ADRRecord.BProvADR)]],
				LProvADR = rawDataRow[HeaderMap[nameof(ADRRecord.LProvADR)]],
				OProvADR = rawDataRow[HeaderMap[nameof(ADRRecord.OProvADR)]],
				HinADR = rawDataRow[HeaderMap[nameof(ADRRecord.HinADR)]],
			};
		}
	}
}
