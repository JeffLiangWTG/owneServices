using System;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer
{
	public class CountryReferenceParser : CsvDataParser<CountryReferenceRecord>
	{
		public override IDictionary<string, int> HeaderMap => headerMap ?? (headerMap = new Dictionary<string, int>()
		{
			{ nameof(CountryReferenceRecord.Type), 0 },
			{ nameof(CountryReferenceRecord.Country), 1 },
			{ nameof(CountryReferenceRecord.Code), 2 },
			{ nameof(CountryReferenceRecord.Description), 3 },
			{ nameof(CountryReferenceRecord.HasFlashPointLower), 4 },
			{ nameof(CountryReferenceRecord.FlashPointLowerCentigrade), 5 },
			{ nameof(CountryReferenceRecord.HasFlashPointUpper), 6 },
			{ nameof(CountryReferenceRecord.FlashPointUpperCentigrade), 7 },
		});

		IDictionary<string, int> headerMap;

		public override CountryReferenceRecord ParseRecord(string[] rawDataRow)
		{
			return new CountryReferenceRecord
			{
				Type = rawDataRow[HeaderMap[nameof(CountryReferenceRecord.Type)]],
				Country = rawDataRow[HeaderMap[nameof(CountryReferenceRecord.Country)]],
				Code = rawDataRow[HeaderMap[nameof(CountryReferenceRecord.Code)]],
				Description = rawDataRow[HeaderMap[nameof(CountryReferenceRecord.Description)]],
				HasFlashPointLower = rawDataRow[HeaderMap[nameof(CountryReferenceRecord.HasFlashPointLower)]] == "1",
				FlashPointLowerCentigrade = rawDataRow[HeaderMap[nameof(CountryReferenceRecord.FlashPointLowerCentigrade)]],
				HasFlashPointUpper = rawDataRow[HeaderMap[nameof(CountryReferenceRecord.HasFlashPointUpper)]] == "1",
				FlashPointUpperCentigrade = rawDataRow[HeaderMap[nameof(CountryReferenceRecord.FlashPointUpperCentigrade)]],
			};
		}

		public IEnumerable<UNDGCountryReference> Parse(string referencePath)
		{
			var parsedRecords = ParseRecords(referencePath, Constants.Encodings.UNDGCountryReferenceFile, true);

			foreach (var record in parsedRecords)
			{
				yield return ConvertCSVRecordToUNDGCountryReference(record);
			}
		}

		static UNDGCountryReference ConvertCSVRecordToUNDGCountryReference(CountryReferenceRecord record)
		{
			return new UNDGCountryReference
			{
				DCR_Type = record.Type,
				DCR_RN_NKCountry = record.Country,
				DCR_Code = record.Code,
				DCR_Description = record.Description,
				DCR_HasFlashPointLower = record.HasFlashPointLower,
				DCR_FlashPointLowerCentigrade = Convert.ToDecimal(record.FlashPointLowerCentigrade, CultureInfo.InvariantCulture),
				DCR_HasFlashPointUpper = record.HasFlashPointUpper,
				DCR_FlashPointUpperCentigrade = Convert.ToDecimal(record.FlashPointUpperCentigrade, CultureInfo.InvariantCulture)
			};
		}
	}
}
