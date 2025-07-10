using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.Web;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class SingaporePSAPivotParser : CsvDataParser<SingaporePSACountryReferencePivotRecord>
	{
		readonly IFileDownloaderWrapper _fileDownloaderWrapper;
		readonly string _imoZipFilePath;

		public SingaporePSAPivotParser(IFileDownloaderWrapper fileDownloaderWrapper, string imoZipFilePath)
		{
			_fileDownloaderWrapper = fileDownloaderWrapper;
			_imoZipFilePath = imoZipFilePath;
		}

		public override IDictionary<string, int> HeaderMap => headerMap ?? (headerMap = new Dictionary<string, int>()
		{
			{ nameof(SingaporePSACountryReferencePivotRecord.UNNO), 0 },
			{ nameof(SingaporePSACountryReferencePivotRecord.Variant), 1 },
			{ nameof(SingaporePSACountryReferencePivotRecord.Standard), 2 },
			{ nameof(SingaporePSACountryReferencePivotRecord.Type), 3 },
			{ nameof(SingaporePSACountryReferencePivotRecord.Country), 4 },
			{ nameof(SingaporePSACountryReferencePivotRecord.Code), 5 },
			{ nameof(SingaporePSACountryReferencePivotRecord.HasFlashPointLower), 6 },
			{ nameof(SingaporePSACountryReferencePivotRecord.FlashPointLowerCentigrade), 7 },
		});

		IDictionary<string, int> headerMap;

		public override SingaporePSACountryReferencePivotRecord ParseRecord(string[] rawDataRow)
		{
			return new SingaporePSACountryReferencePivotRecord
			{
				UNNO = ParserHelper.ParseUNNO(rawDataRow[HeaderMap[nameof(SingaporePSACountryReferencePivotRecord.UNNO)]]),
				Variant = rawDataRow[HeaderMap[nameof(SingaporePSACountryReferencePivotRecord.Variant)]],
				Standard = rawDataRow[HeaderMap[nameof(SingaporePSACountryReferencePivotRecord.Standard)]],
				Type = rawDataRow[HeaderMap[nameof(SingaporePSACountryReferencePivotRecord.Type)]],
				Country = rawDataRow[HeaderMap[nameof(SingaporePSACountryReferencePivotRecord.Country)]],
				Code = rawDataRow[HeaderMap[nameof(SingaporePSACountryReferencePivotRecord.Code)]],
				HasFlashPointLower = rawDataRow[HeaderMap[nameof(SingaporePSACountryReferencePivotRecord.HasFlashPointLower)]] == "1",
				FlashPointLowerCentigrade = rawDataRow[HeaderMap[nameof(SingaporePSACountryReferencePivotRecord.FlashPointLowerCentigrade)]],
			};
		}

		public static void ValidatePivotRecords(IEnumerable<UNDGSubstance> substances, IEnumerable<SingaporePSACountryReferencePivotRecord> pivotRecords)
		{
			var invalidPivotRecord = pivotRecords.FirstOrDefault(pivotRecord => !substances.Any(substance => substance.DG_UNNO.Equals(pivotRecord.UNNO, StringComparison.OrdinalIgnoreCase) && substance.DG_Variant.Equals(pivotRecord.Variant, StringComparison.OrdinalIgnoreCase)));
			if (invalidPivotRecord != null)
			{
				throw new InvalidOperationException($"PivotRecord with UNNO {invalidPivotRecord.UNNO} and Variant '{invalidPivotRecord.Variant}' has no matching IMO Record.");
			}
		}

		public IEnumerable<UNDGCountryReference> ParseAndAddPivotsToReferences(string referencePath, IEnumerable<UNDGCountryReference> referencesWithoutPivots)
		{
			var imoRecords = new IMOParser(_fileDownloaderWrapper, _imoZipFilePath).ParseZipAndExtractUNDGSbustances();
			var parsedRecords = ParseRecords(referencePath, Constants.Encodings.UNDGCountryReferenceFile, true);
			ValidatePivotRecords(imoRecords, parsedRecords);

			var referenceMap = CreateReferenceToPivotDictionary(parsedRecords);
			var countryReferences = referencesWithoutPivots.ToList();

			foreach (var record in countryReferences)
			{
				if (referenceMap.TryGetValue(new UniquePSAReference(
					record.DCR_Code,
					record.DCR_HasFlashPointLower,
					record.DCR_FlashPointLowerCentigrade), out var pivots))
				{
					record.UNDGCountryReferencePivots = pivots.ToArray();
				}
			}

			return countryReferences;
		}

		static IDictionary<UniquePSAReference, IList<UNDGCountryReferencePivot>> CreateReferenceToPivotDictionary(IEnumerable<SingaporePSACountryReferencePivotRecord> pivotRecords)
		{
			var referenceToPivotDictionary = new Dictionary<UniquePSAReference, IList<UNDGCountryReferencePivot>>();

			foreach (var pivotRecord in pivotRecords)
			{
				var reference = new UniquePSAReference(
					pivotRecord.Code,
					pivotRecord.HasFlashPointLower,
					Convert.ToDecimal(pivotRecord.FlashPointLowerCentigrade, CultureInfo.InvariantCulture));

				var undgCountryReferencePivot = ConvertPivotCSVRecordToUNDGCountryReferencePivot(pivotRecord);

				if (referenceToPivotDictionary.TryGetValue(reference, out var pivots))
				{
					pivots.Add(undgCountryReferencePivot);
				}
				else
				{
					referenceToPivotDictionary.Add(reference, new List<UNDGCountryReferencePivot> { undgCountryReferencePivot });
				}
			}

			return referenceToPivotDictionary;
		}

		static UNDGCountryReferencePivot ConvertPivotCSVRecordToUNDGCountryReferencePivot(SingaporePSACountryReferencePivotRecord pivotRecord)
		{
			return new UNDGCountryReferencePivot
			{
				DCP_UNNO = pivotRecord.UNNO,
				DCP_Variant = pivotRecord.Variant,
				DCP_Standard = pivotRecord.Standard
			};
		}
	}
}
