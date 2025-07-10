using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using TinyCsvParser;

namespace CargoWise.RefDbRepo.IHSReferenceData.Business.Vessel
{
	public static class VesselParser
	{
		static IEnumerable<RefVessel> Parse(string vesselsCsv, string flagCodeCsv)
		{
			var csvParserOptions = new CsvParserOptions(true, ',');
			var csvReaderOptions = new CsvReaderOptions(new[] { Environment.NewLine });
			var csvMapper = new CsvVesselMapping();
			var csvParser = new CsvParser<RefVessel>(csvParserOptions, csvMapper);

			var vesselList = csvParser
				.ReadFromString(csvReaderOptions, vesselsCsv)
				.Where(x => x.IsValid)
				.Select(x => x.Result)
				.ToList();

			var flagCodelDictionary = new Dictionary<string, string>();
			if (!string.IsNullOrWhiteSpace(flagCodeCsv))
			{
				var csvFlagCodeMapper = new CsvFlagCodeMapping();
				var csvFlagCodeParser = new CsvParser<FlagCodeMap>(csvParserOptions, csvFlagCodeMapper);

				flagCodelDictionary = csvFlagCodeParser
				   .ReadFromString(csvReaderOptions, flagCodeCsv)
				   .Where(x => x.IsValid)
				   .Select(x => x.Result)
				   .ToDictionary(o => o.FlagCode, o => o.IsoTwoLetterCode);
			}

			foreach (var vessel in vesselList)
			{
				if (!string.IsNullOrWhiteSpace(vessel.RV_RN_NKCountryOfReg))
				{
					flagCodelDictionary.TryGetValue(vessel.RV_RN_NKCountryOfReg, out var twoLetterCode);
					vessel.RV_RN_NKCountryOfReg = twoLetterCode;
				}
				vessel.RV_VesselType = VesselHelper.GetCWVesselType(vessel.RV_StatCode5);
			}

			return vesselList;
		}

		public static void ExportToXMLFile(string vesselsCsv, string flagCodeCsv, string outputFolderPath, DateTime publicationDateTime)
		{
			var vessels = Parse(vesselsCsv, flagCodeCsv);
			var outputFileFullName = Path.Combine(outputFolderPath, OutputFileName);
			VesselHelper.ExportToXMLFile(outputFileFullName, DataSource, publicationDateTime, vessels);
		}

		const string DataSource = "IHS Vessels List";
		const string OutputFileName = "RefCusCodeListZZ_IHS_Vessel.xml";
	}
}
