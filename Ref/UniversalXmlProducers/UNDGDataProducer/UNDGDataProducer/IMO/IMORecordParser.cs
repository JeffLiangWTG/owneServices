using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class IMORecordParser : CsvDataParser<IMORecord>
	{
		readonly IEnumerable<StowSegRecord> _stowSegRecords;
		readonly IEnumerable<SpecProvRecord> _specProvRecords;
		readonly IEnumerable<PropertyRecord> _propertyRecords;
		readonly IEnumerable<QualifyingDescriptiveTextRecord> _qualifyingDescriptiveTextRecords;

		public IEnumerable<UNDGSubstance> Parse(string filePath)
		{
			var parsedRecords = ParseRecords(filePath, Constants.Encodings.IMOZipFile, true);
			var results = new List<UNDGSubstance>();
			foreach (var record in parsedRecords)
			{
				results.Add(ConvertCsvRecordToUndgSubstance(record));
			}

			results.ForEach(result =>
			{
				if (parsedRecords.Count(x => x.UNNO == result.DG_UNNO && !string.IsNullOrEmpty(result.DG_Variant)) == 1)
				{
					result.DG_Variant = "";
				}
			});

			return results;
		}

		public IEnumerable<IMORecord> GetRawIMORecords(string filePath)
		{
			return ParseRecords(filePath, Constants.Encodings.IMOZipFile, true);
		}

		UNDGSubstance ConvertCsvRecordToUndgSubstance(IMORecord record)
		{
			FilterPackingInstructionsOnRecord(record);

			var result = new UNDGSubstance
			{
				DG_UNNO = record.UNNO,
				DG_Variant = record.Variant,
				DG_Variation = record.Variation,
				DG_Class = record.Class,
				DG_SubLabel1 = record.SubLabel1.Replace("See", "").TrimStart(),
				DG_SubLabel2 = record.SubLabel2,
				DG_PSN = record.PSN,
				DG_PG = record.PG,
				DG_EMS = record.EMS,
				DG_MP = record.MP == "P" ? "Y" : "",
				DG_FlashPoint = record.FP,
				DG_TechName = record.TechName,
				DG_TreatAs = record.TreatAs,
				DG_DglPhrase = record.DGLPhrase,
				DG_PackIns = record.PackIns,
				DG_PackProv = record.PackProv,
				DG_IBCIns = record.IbcIns,
				DG_IBCProv = record.IbcProv,
				DG_UNTankIns = record.UNTankIns,
				DG_TankProv = record.TankProv,
				DG_StowCat = record.StowCat,
				DG_ExpLim = record.ExpLim,
				DG_UlineEMS = record.UlineEMS,
			};

			if (record.PackIns == record.PackProv)
			{
				result.DG_PackIns = string.Empty;
			}
			if (record.IbcIns == record.IbcProv)
			{
				result.DG_IBCIns = string.Empty;
			}

			var attributes = new List<UNDGAttribute>();

			result.SetLQData(record.LQ, attributes);
			result.SetEQData(record.EQ, attributes);

			IMOParserHelper.CreateAttributesStowSegSpecProv(_stowSegRecords, _specProvRecords, record.Stow, record.Seg, record.SpecProv, attributes);
			IMOParserHelper.CreateAttributesProperty(_propertyRecords, record.UNNO, record.Variant, attributes);
			IMOParserHelper.CreateAttributesQualifyingDescriptiveText(_qualifyingDescriptiveTextRecords, record.Code, attributes);

			result.UNDGAttributes = attributes.ToArray();

			return result;
		}

		static void FilterPackingInstructionsOnRecord(IMORecord imoRecord)
		{
			var instructionsToRemove = new HashSet<string>()
			{
				"See 4.1.9",
				"See SP963"
			};

			if (instructionsToRemove.Contains(imoRecord.PackIns))
			{
				imoRecord.PackIns = string.Empty;
			}
		}

		public IMORecordParser(IEnumerable<StowSegRecord> stowSegRecords, IEnumerable<SpecProvRecord> specProvRecords, IEnumerable<PropertyRecord> propertyRecords, IEnumerable<QualifyingDescriptiveTextRecord> qualifyingDescriptiveTextRecords)
		{
			_stowSegRecords = stowSegRecords;
			_specProvRecords = specProvRecords;
			_propertyRecords = propertyRecords;
			_qualifyingDescriptiveTextRecords = qualifyingDescriptiveTextRecords;

			HeaderMap = new Dictionary<string, int>()
			{
				{ nameof(IMORecord.UNNO), 0 },
				{ nameof(IMORecord.Variant), 1 },
				{ nameof(IMORecord.CVL), 2 },
				{ nameof(IMORecord.Variation), 3 },
				{ nameof(IMORecord.Class), 4 },
				{ nameof(IMORecord.SubLabel1), 5 },
				{ nameof(IMORecord.SubLabel2), 6 },
				{ nameof(IMORecord.PSN), 7 },
				{ nameof(IMORecord.PG), 8 },
				{ nameof(IMORecord.EMS), 9 },
				{ nameof(IMORecord.MP), 10 },
				{ nameof(IMORecord.FP), 11 },
				{ nameof(IMORecord.LQ), 12 },
				{ nameof(IMORecord.EQ), 13 },
				{ nameof(IMORecord.TechName), 14 },
				{ nameof(IMORecord.TreatAs), 15 },
				{ nameof(IMORecord.DGLPhrase), 16 },
				{ nameof(IMORecord.Stow), 17 },
				{ nameof(IMORecord.Seg), 18 },
				{ nameof(IMORecord.SpecProv), 19 },
				{ nameof(IMORecord.PackIns), 20 },
				{ nameof(IMORecord.PackProv), 21 },
				{ nameof(IMORecord.IbcIns), 22 },
				{ nameof(IMORecord.IbcProv), 23 },
				{ nameof(IMORecord.UNTankIns), 24 },
				{ nameof(IMORecord.TankProv), 25 },
				{ nameof(IMORecord.StowCat), 26 },
				{ nameof(IMORecord.ExpLim), 27 },
				{ nameof(IMORecord.UlineEMS), 28 },
			};
		}

		public override IDictionary<string, int> HeaderMap { get; }

		public override IMORecord ParseRecord(string[] rawDataRow)
		{
			return new IMORecord
			{
				Class = rawDataRow[HeaderMap[nameof(IMORecord.Class)]],
				CVL = rawDataRow[HeaderMap[nameof(IMORecord.CVL)]],
				DGLPhrase = rawDataRow[HeaderMap[nameof(IMORecord.DGLPhrase)]],
				EMS = rawDataRow[HeaderMap[nameof(IMORecord.EMS)]],
				EQ = rawDataRow[HeaderMap[nameof(IMORecord.EQ)]],
				ExpLim = rawDataRow[HeaderMap[nameof(IMORecord.ExpLim)]],
				FP = rawDataRow[HeaderMap[nameof(IMORecord.FP)]],
				IbcIns = rawDataRow[HeaderMap[nameof(IMORecord.IbcIns)]],
				IbcProv = rawDataRow[HeaderMap[nameof(IMORecord.IbcProv)]],
				LQ = rawDataRow[HeaderMap[nameof(IMORecord.LQ)]],
				MP = rawDataRow[HeaderMap[nameof(IMORecord.MP)]],
				PackIns = rawDataRow[HeaderMap[nameof(IMORecord.PackIns)]],
				PackProv = rawDataRow[HeaderMap[nameof(IMORecord.PackProv)]],
				PG = rawDataRow[HeaderMap[nameof(IMORecord.PG)]],
				PSN = rawDataRow[HeaderMap[nameof(IMORecord.PSN)]],
				Seg = rawDataRow[HeaderMap[nameof(IMORecord.Seg)]],
				SpecProv = rawDataRow[HeaderMap[nameof(IMORecord.SpecProv)]],
				Stow = rawDataRow[HeaderMap[nameof(IMORecord.Stow)]],
				StowCat = rawDataRow[HeaderMap[nameof(IMORecord.StowCat)]],
				SubLabel1 = rawDataRow[HeaderMap[nameof(IMORecord.SubLabel1)]],
				SubLabel2 = rawDataRow[HeaderMap[nameof(IMORecord.SubLabel2)]],
				TankProv = rawDataRow[HeaderMap[nameof(IMORecord.TankProv)]],
				TechName = rawDataRow[HeaderMap[nameof(IMORecord.TechName)]],
				TreatAs = rawDataRow[HeaderMap[nameof(IMORecord.TreatAs)]],
				UlineEMS = rawDataRow[HeaderMap[nameof(IMORecord.UlineEMS)]],
				UNNO = ParserHelper.ParseUNNO(rawDataRow[HeaderMap[nameof(IMORecord.UNNO)]]),
				UNTankIns = rawDataRow[HeaderMap[nameof(IMORecord.UNTankIns)]],
				Variant = rawDataRow[HeaderMap[nameof(IMORecord.Variant)]],
				Variation = rawDataRow[HeaderMap[nameof(IMORecord.Variation)]],
			};
		}
	}
}
