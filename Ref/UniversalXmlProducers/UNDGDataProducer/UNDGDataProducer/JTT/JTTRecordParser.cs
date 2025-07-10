using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class JTTRecordParser : CsvDataParser<JTTRecord>
	{
		public JTTRecordParser()
		{
			HeaderMap = new Dictionary<string, int>()
			{
				{ nameof(JTTRecord.UnId), 0 },
				{ nameof(JTTRecord.Var), 1 },
				{ nameof(JTTRecord.Name), 2 },
				{ nameof(JTTRecord.Descriptor), 3 },
				{ nameof(JTTRecord.Name_ZH), 4 },
				{ nameof(JTTRecord.Descriptor_ZH), 5 },
				{ nameof(JTTRecord.ClassificationCode), 6 },
				{ nameof(JTTRecord.Class), 7 },
				{ nameof(JTTRecord.Label), 8 },
				{ nameof(JTTRecord.PackingGroup), 9 },
				{ nameof(JTTRecord.LQMaxAmt), 10 },
				{ nameof(JTTRecord.SpecialProvisions), 11 },
				{ nameof(JTTRecord.ExceptedQuantityCode), 12 },
				{ nameof(JTTRecord.PackIns), 13 },
				{ nameof(JTTRecord.MixedPackingProv), 14 },
				{ nameof(JTTRecord.PackProv), 15 },
				{ nameof(JTTRecord.BulkTankIns), 16 },
				{ nameof(JTTRecord.TankCode), 17 },
				{ nameof(JTTRecord.BulkTankSpecProv), 18 },
				{ nameof(JTTRecord.TankSpecProv), 19 },
				{ nameof(JTTRecord.TankVehicle), 20 },
				{ nameof(JTTRecord.TransportCategory), 21 },
				{ nameof(JTTRecord.PackingSpecialProv), 22 },
				{ nameof(JTTRecord.BulkSpecialProv), 23 },
				{ nameof(JTTRecord.LoadingSpecialProv), 24 },
				{ nameof(JTTRecord.OperationSpecialProv), 25 },
				{ nameof(JTTRecord.HazardIDNumber), 26 },
			};
		}

		public override IDictionary<string, int> HeaderMap { get; }

		public IEnumerable<UNDGSubstanceJTT> Parse(string filePath)
		{
			var parsedRecords = ParseRecords(filePath, Constants.Encodings.JTTFile, true);
			foreach (var record in parsedRecords.Where(x => !string.IsNullOrEmpty(x.UnId)))
			{
				yield return ConvertCsvRecordToUndgSubstanceJTT(record);
			}
		}

		static UNDGSubstanceJTT ConvertCsvRecordToUndgSubstanceJTT(JTTRecord record)
		{
			FilterPackingInstructionsOnRecord(record);

			var lqs = ParserHelper.TransformLQMaxAmts(record.LQMaxAmt);

			var result = new UNDGSubstanceJTT
			{
				JTT_UNNO = ParserHelper.CheckFieldLength(record.UnId, record.UnId, 4),
				JTT_Variant = ParserHelper.CheckFieldLength(record.Var, record.UnId, 2),
				JTT_PSN = ParserHelper.CheckFieldLength(record.Name, record.UnId, 260),
				JTT_Class = ParserHelper.CheckFieldLength(record.Class, record.UnId, 4),
				JTT_ClassificationCode = ParserHelper.CheckFieldLength(ParserHelper.TransformClassificationCode(record.ClassificationCode), record.UnId, 20),
				JTT_PG = ParserHelper.CheckFieldLength(record.PackingGroup, record.UnId, 3),
				JTT_Labels = ParserHelper.CheckFieldLength(record.Label, record.UnId, 20),
				JTT_SpecialProvisions = ParserHelper.CheckFieldLength(record.SpecialProvisions, record.UnId, 30),
				JTT_LQMaxAmt = lqs[0].Item1,
				JTT_LQMaxAmtUQ = ParserHelper.CheckFieldLength(lqs[0].Item2, record.UnId, 2),
				JTT_ExceptedQuantityCode = ParserHelper.CheckFieldLength(record.ExceptedQuantityCode, record.UnId, 2),
				JTT_PackIns = ParserHelper.CheckFieldLength(record.PackIns, record.UnId, 50),
				JTT_PackProv = ParserHelper.CheckFieldLength(record.PackProv, record.UnId, 50),
				JTT_MixedPackingProv = ParserHelper.CheckFieldLength(record.MixedPackingProv, record.UnId, 50),
				JTT_TankSpecProv = ParserHelper.CheckFieldLength(record.BulkTankIns, record.UnId, 62),
				JTT_BulkTankSpecProv = ParserHelper.CheckFieldLength(record.BulkTankSpecProv, record.UnId, 50),
				JTT_TankCode = ParserHelper.CheckFieldLength(record.TankCode, record.UnId, 42),
				JTT_BulkTankIns = ParserHelper.CheckFieldLength(record.BulkTankIns, record.UnId, 50),
				JTT_TankVehicle = ParserHelper.CheckFieldLength(record.TankVehicle, record.UnId, 26),
				JTT_TransportCategory = ParserHelper.CheckFieldLength(ParserHelper.TransformTransportCategory(record.TransportCategory, record.UnId), record.UnId, 12),
				JTT_PackingSpecialProv = ParserHelper.CheckFieldLength(record.PackingSpecialProv, record.UnId, 18),
				JTT_BulkSpecialProv = ParserHelper.CheckFieldLength(record.BulkSpecialProv, record.UnId, 20),
				JTT_LoadingSpecialProv = ParserHelper.CheckFieldLength(record.LoadingSpecialProv, record.UnId, 48),
				JTT_OperationSpecialProv = ParserHelper.CheckFieldLength(record.OperationSpecialProv, record.UnId, 36),
				JTT_HazardIDNumber = ParserHelper.CheckFieldLength(record.HazardIDNumber, record.UnId, 8)
			};

			if (lqs.Length > 1)
			{
				result.JTT_LQ2MaxAmt = lqs[1].Item1;
				result.JTT_LQ2MaxAmtUQ = ParserHelper.CheckFieldLength(lqs[1].Item2, record.UnId, 2);
			}

			result.UNDGAttributeZZs = ParserHelper.LoadAttributesZZ(record.Descriptor);
			result.UNDGAttributeZZs = ParserHelper.AppendUNDGAttributeZZ(result.UNDGAttributeZZs, record.Descriptor_ZH, language: "ZH-CN");
			result.UNDGAttributeZZs = ParserHelper.AppendUNDGAttributeZZ(result.UNDGAttributeZZs, record.Name_ZH, Constants.AttributeTypes.ProperShippingName, "ZH-CN");

			if (record.PackIns == record.PackProv)
			{
				result.JTT_PackIns = string.Empty;
			}
			return result;
		}

		static void FilterPackingInstructionsOnRecord(JTTRecord jttRecord)
		{
			var instructionsToRemove = new HashSet<string>()
			{
				"See 1.7",
				"See 2.2.7 and 4.1.9"
			};

			if (instructionsToRemove.Contains(jttRecord.PackIns))
			{
				jttRecord.PackIns = string.Empty;
			}
		}

		public override JTTRecord ParseRecord(string[] rawDataRow)
		{
			return new JTTRecord
			{
				UnId = ParserHelper.ParseUNNO(rawDataRow[HeaderMap[nameof(JTTRecord.UnId)]]),
				Var = rawDataRow[HeaderMap[nameof(JTTRecord.Var)]],
				Name = rawDataRow[HeaderMap[nameof(JTTRecord.Name)]],
				Name_ZH = rawDataRow[HeaderMap[nameof(JTTRecord.Name_ZH)]],
				Descriptor = rawDataRow[HeaderMap[nameof(JTTRecord.Descriptor)]],
				Descriptor_ZH = rawDataRow[HeaderMap[nameof(JTTRecord.Descriptor_ZH)]],
				Class = rawDataRow[HeaderMap[nameof(JTTRecord.Class)]],
				ClassificationCode = rawDataRow[HeaderMap[nameof(JTTRecord.ClassificationCode)]],
				PackingGroup = rawDataRow[HeaderMap[nameof(JTTRecord.PackingGroup)]],
				Label = rawDataRow[HeaderMap[nameof(JTTRecord.Label)]],
				SpecialProvisions = rawDataRow[HeaderMap[nameof(JTTRecord.SpecialProvisions)]],
				LQMaxAmt = rawDataRow[HeaderMap[nameof(JTTRecord.LQMaxAmt)]],
				ExceptedQuantityCode = rawDataRow[HeaderMap[nameof(JTTRecord.ExceptedQuantityCode)]],
				PackIns = rawDataRow[HeaderMap[nameof(JTTRecord.PackIns)]],
				PackProv = rawDataRow[HeaderMap[nameof(JTTRecord.PackProv)]],
				MixedPackingProv = rawDataRow[HeaderMap[nameof(JTTRecord.MixedPackingProv)]],
				BulkTankIns = rawDataRow[HeaderMap[nameof(JTTRecord.BulkTankIns)]],
				BulkTankSpecProv = rawDataRow[HeaderMap[nameof(JTTRecord.BulkTankSpecProv)]],
				TankCode = rawDataRow[HeaderMap[nameof(JTTRecord.TankCode)]],
				TankSpecProv = rawDataRow[HeaderMap[nameof(JTTRecord.TankSpecProv)]],
				TankVehicle = rawDataRow[HeaderMap[nameof(JTTRecord.TankVehicle)]],
				TransportCategory = rawDataRow[HeaderMap[nameof(JTTRecord.TransportCategory)]],
				PackingSpecialProv = rawDataRow[HeaderMap[nameof(JTTRecord.PackingSpecialProv)]],
				BulkSpecialProv = rawDataRow[HeaderMap[nameof(JTTRecord.BulkSpecialProv)]],
				LoadingSpecialProv = rawDataRow[HeaderMap[nameof(JTTRecord.LoadingSpecialProv)]],
				OperationSpecialProv = rawDataRow[HeaderMap[nameof(JTTRecord.OperationSpecialProv)]],
				HazardIDNumber = rawDataRow[HeaderMap[nameof(JTTRecord.HazardIDNumber)]]
			};
		}
	}
}
