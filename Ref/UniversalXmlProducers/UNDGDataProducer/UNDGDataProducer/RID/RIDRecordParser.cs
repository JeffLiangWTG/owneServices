using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class RIDRecordParser : CsvDataParser<RIDRecord>
	{
		public IEnumerable<UNDGSubstanceRID> Parse(string filePath)
		{
			var parsedRecords = ParseRecords(filePath, Constants.Encodings.RIDFile, true);
			foreach (var record in parsedRecords.Where(x => !string.IsNullOrEmpty(x.UnId)))
			{
				yield return ConvertCsvRecordToUndgSubstanceRID(record);
			}
		}

		static UNDGSubstanceRID ConvertCsvRecordToUndgSubstanceRID(RIDRecord record)
		{
			var result = new UNDGSubstanceRID
			{
				RID_UNNO = ParserHelper.CheckFieldLength(record.UnId, record.UnId, 4),
				RID_Variant = ParserHelper.CheckFieldLength(record.Var, record.UnId, 2),
				RID_PSN = ParserHelper.CheckFieldLength(record.NameRID, record.UnId, 200),
				RID_Class = ParserHelper.CheckFieldLength(record.ClassRID, record.UnId, 4),
				RID_ClassificationCode = ParserHelper.CheckFieldLength(ParserHelper.TransformClassificationCode(record.CCodeRID), record.UnId, 20),
				RID_PG = ParserHelper.CheckFieldLength(record.PgRID, record.UnId, 3),
				RID_Labels = ParserHelper.CheckFieldLength(ParserHelper.TransformLabels(record.LabRID), record.UnId, 20),
				RID_SpecialProvisions = ParserHelper.CheckFieldLength(record.SpRID, record.UnId, 46),
				RID_LQMaxAmt = ParserHelper.TransformLQMaxAmt(record.LqRID),
				RID_LQMaxAmtUQ = ParserHelper.CheckFieldLength(ParserHelper.TransformLQMaxAmtUQ(record.LqRID), record.UnId, 2),
				RID_ExceptedQuantityCode = ParserHelper.CheckFieldLength(ParserHelper.SeeContentCleanUp(record.EqRID), record.UnId, 2),
				RID_PackIns = ParserHelper.CheckFieldLength(ParserHelper.TransformPackIns(ParserHelper.SeeContentCleanUp(record.PiRID)), record.UnId, 50),
				RID_IBCIns = ParserHelper.CheckFieldLength(ParserHelper.TransformIBCIns(ParserHelper.SeeContentCleanUp(record.PiRID)), record.UnId, 46),
				RID_PackProv = ParserHelper.CheckFieldLength(ParserHelper.SeeContentCleanUp(record.PpRID), record.UnId, 48),
				RID_MixedPackProv = ParserHelper.CheckFieldLength(record.MixRID, record.UnId, 18),
				RID_BulkContainerTankIns = ParserHelper.CheckFieldLength(record.TiRID, record.UnId, 20),
				RID_BulkContainerTankProv = ParserHelper.CheckFieldLength(record.TpRID, record.UnId, 34),
				RID_TankCode = ParserHelper.CheckFieldLength(record.TrRID, record.UnId, 35),
				RID_TankSpecProv = ParserHelper.CheckFieldLength(record.SptRID, record.UnId, 60),
				RID_TransportCategory = ParserHelper.CheckFieldLength(record.CatRID, record.UnId, 2),
				RID_CarriagePackagesSpecialProv = ParserHelper.CheckFieldLength(record.SppRID, record.UnId, 18),
				RID_CarriageBulkSpecialProv = ParserHelper.CheckFieldLength(ParserHelper.SeeContentCleanUp(record.SpbRID), record.UnId, 38),
				RID_CarriageLoadingSpecialProv = ParserHelper.CheckFieldLength(ParserHelper.SeeContentCleanUp(record.SplRID), record.UnId, 40),
				RID_ColisExpressCode = ParserHelper.CheckFieldLength(record.ColisRID, record.UnId, 36),
				RID_HazardIDNumber = ParserHelper.CheckFieldLength(record.HinRID, record.UnId, 8)
			};
			result.UNDGAttributeZZs = ParserHelper.LoadAttributesZZ(record.Qdt);

			if (record.LqRID.Contains("or"))
			{
				var limitedQuantity = record.LqRID.Substring(record.LqRID.IndexOf("or", StringComparison.InvariantCulture) + 3);
				result.RID_LQ2MaxAmt = ParserHelper.TransformLQMaxAmt(limitedQuantity);
				result.RID_LQ2MaxAmtUQ = ParserHelper.CheckFieldLength(ParserHelper.TransformLQMaxAmtUQ(limitedQuantity), record.UnId, 2);
			}

			return result;
		}
		public override IDictionary<string, int> HeaderMap { get; }
		public RIDRecordParser()
		{
			HeaderMap = new Dictionary<string, int>()
			{
				{ nameof(RIDRecord.UnId), 0 },
				{ nameof(RIDRecord.Var), 1 },
				{ nameof(RIDRecord.NameRID), 2 },
				{ nameof(RIDRecord.ClassRID), 3 },
				{ nameof(RIDRecord.CCodeRID), 4 },
				{ nameof(RIDRecord.PgRID), 5 },
				{ nameof(RIDRecord.LabRID), 6 },
				{ nameof(RIDRecord.SpRID), 7 },
				{ nameof(RIDRecord.LqRID), 8 },
				{ nameof(RIDRecord.EqRID), 9 },
				{ nameof(RIDRecord.PiRID), 10 },
				{ nameof(RIDRecord.PpRID), 11 },
				{ nameof(RIDRecord.MixRID), 12 },
				{ nameof(RIDRecord.TiRID), 13 },
				{ nameof(RIDRecord.TpRID), 14 },
				{ nameof(RIDRecord.TrRID), 15 },
				{ nameof(RIDRecord.SptRID), 16 },
				{ nameof(RIDRecord.CatRID), 17 },
				{ nameof(RIDRecord.SppRID), 18 },
				{ nameof(RIDRecord.SpbRID), 19 },
				{ nameof(RIDRecord.SplRID), 20 },
				{ nameof(RIDRecord.ColisRID), 21 },
				{ nameof(RIDRecord.HinRID), 22 },
				{ nameof(RIDRecord.Qdt), 23 }
			};
		}
		public override RIDRecord ParseRecord(string[] rawDataRow)
		{
			return new RIDRecord
			{
				UnId = ParserHelper.ParseUNNO(rawDataRow[HeaderMap[nameof(RIDRecord.UnId)]]),
				Var = rawDataRow[HeaderMap[nameof(RIDRecord.Var)]],
				NameRID = rawDataRow[HeaderMap[nameof(RIDRecord.NameRID)]],
				ClassRID = rawDataRow[HeaderMap[nameof(RIDRecord.ClassRID)]],
				CCodeRID = rawDataRow[HeaderMap[nameof(RIDRecord.CCodeRID)]],
				PgRID = rawDataRow[HeaderMap[nameof(RIDRecord.PgRID)]],
				LabRID = rawDataRow[HeaderMap[nameof(RIDRecord.LabRID)]],
				SpRID = rawDataRow[HeaderMap[nameof(RIDRecord.SpRID)]],
				LqRID = rawDataRow[HeaderMap[nameof(RIDRecord.LqRID)]],
				EqRID = rawDataRow[HeaderMap[nameof(RIDRecord.EqRID)]],
				PiRID = rawDataRow[HeaderMap[nameof(RIDRecord.PiRID)]],
				PpRID = rawDataRow[HeaderMap[nameof(RIDRecord.PpRID)]],
				MixRID = rawDataRow[HeaderMap[nameof(RIDRecord.MixRID)]],
				TiRID = rawDataRow[HeaderMap[nameof(RIDRecord.TiRID)]],
				TpRID = rawDataRow[HeaderMap[nameof(RIDRecord.TpRID)]],
				TrRID = rawDataRow[HeaderMap[nameof(RIDRecord.TrRID)]],
				SptRID = rawDataRow[HeaderMap[nameof(RIDRecord.SptRID)]],
				CatRID = rawDataRow[HeaderMap[nameof(RIDRecord.CatRID)]],
				SppRID = rawDataRow[HeaderMap[nameof(RIDRecord.SppRID)]],
				SpbRID = rawDataRow[HeaderMap[nameof(RIDRecord.SpbRID)]],
				SplRID = rawDataRow[HeaderMap[nameof(RIDRecord.SplRID)]],
				ColisRID = rawDataRow[HeaderMap[nameof(RIDRecord.ColisRID)]],
				HinRID = rawDataRow[HeaderMap[nameof(RIDRecord.HinRID)]],
				Qdt = rawDataRow[HeaderMap[nameof(RIDRecord.Qdt)]]
			};
		}
	}
}
