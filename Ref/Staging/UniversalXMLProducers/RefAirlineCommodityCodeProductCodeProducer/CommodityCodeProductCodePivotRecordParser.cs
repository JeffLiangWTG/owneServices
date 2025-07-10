using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Microsoft.VisualBasic.FileIO;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineCommodityCodeProductCodeProducer
{
	public class CommodityCodeProductCodePivotRecordParser
	{
		readonly IDictionary<string, int> _headerMap = new Dictionary<string, int>()
		{
			{ nameof(CommodityCodeProductCodePivotRecord.AirlineID), 0 },
			{ nameof(CommodityCodeProductCodePivotRecord.ProductCode), 1 },
			{ nameof(CommodityCodeProductCodePivotRecord.ProductDescription), 2 },
			{ nameof(CommodityCodeProductCodePivotRecord.CommodityCode), 3 },
			{ nameof(CommodityCodeProductCodePivotRecord.CommodityDescription), 4 },
			{ nameof(CommodityCodeProductCodePivotRecord.SpecialHandlingCodes), 5 }
		};

		public (IEnumerable<RefAirlineProductCode>, IEnumerable<RefAirlineCommodityCode>, string) Parse(string filePath)
		{
			var records = new List<CommodityCodeProductCodePivotRecord>();
			var airlineId = string.Empty;

			using (var csvParser = new TextFieldParser(filePath, Encoding.UTF8))
			{
				csvParser.SetDelimiters(",");
				csvParser.HasFieldsEnclosedInQuotes = true;
				var recordSplit = csvParser.ReadFields();

				while (!csvParser.EndOfData)
				{
					recordSplit = csvParser.ReadFields();
					
					var parsedRecord = ParseRecord(recordSplit);
					if (!string.IsNullOrEmpty(parsedRecord.ProductCode) &&
						!string.IsNullOrEmpty(parsedRecord.CommodityCode))
					{
						records.Add(parsedRecord);
						if (string.IsNullOrEmpty(airlineId))
						{
							airlineId = parsedRecord.AirlineID;
						}
						else if (!airlineId.Equals(parsedRecord.AirlineID, StringComparison.OrdinalIgnoreCase))
						{
							Console.WriteLine(
								$"AirlineID '{parsedRecord.AirlineID}' is different from that('{airlineId}') in the first row. Skip this row.");
						}
					}
				}
			}

			var productCodeToPivotsDictionary = CreateProductCodeToPivotsDictionary(records);
			var productCodeEntities = ConvertProductCodeToPivotsDictionaryToProductCodeEntities(productCodeToPivotsDictionary);

			var commodityCodeEntities = CreateCommodityCodeEntities(records);

			return (productCodeEntities, commodityCodeEntities, airlineId);
		}

		IEnumerable<RefAirlineCommodityCode> CreateCommodityCodeEntities(List<CommodityCodeProductCodePivotRecord> records)
		{
			var commodityCodeSet = new HashSet<CommodityCodeRecord>();

			foreach (var pivot in records)
			{
				var commodityCodeRecord = new CommodityCodeRecord(pivot.AirlineID, pivot.CommodityCode, pivot.CommodityDescription, pivot.SpecialHandlingCodes);

				var foundCommodityCode =
					commodityCodeSet.FirstOrDefault(c => c.AirlineID.Equals(commodityCodeRecord.AirlineID, StringComparison.Ordinal) &&
														 c.CommodityCode.Equals(commodityCodeRecord.CommodityCode, StringComparison.Ordinal));
				if (foundCommodityCode != null)
				{
					foundCommodityCode.MergeSpecialHandlingCodes(commodityCodeRecord);
				}
				else
				{
					commodityCodeSet.Add(commodityCodeRecord);
				}
			}

			return commodityCodeSet.Select(ConvertCommodityCodeRecordToCommodityCodeEntity);
		}

		IEnumerable<RefAirlineProductCode> ConvertProductCodeToPivotsDictionaryToProductCodeEntities(IDictionary<ProductCodeRecord, IList<CommodityCodeProductCodePivotRecord>> productCodeToPivotsDictionary)
		{
			return productCodeToPivotsDictionary.Select(keyValuePair => ConvertProductCodeRecordWithPivotsToProductCodeEntity(keyValuePair.Key, keyValuePair.Value));
		}

		static IDictionary<ProductCodeRecord, IList<CommodityCodeProductCodePivotRecord>> CreateProductCodeToPivotsDictionary(List<CommodityCodeProductCodePivotRecord> records)
		{
			var productCodeToPivotsDictionary = new Dictionary<ProductCodeRecord, IList<CommodityCodeProductCodePivotRecord>>();

			foreach (var pivot in records)
			{
				var productCode = new ProductCodeRecord(pivot.AirlineID, pivot.ProductCode, pivot.ProductDescription);

				if (productCodeToPivotsDictionary.TryGetValue(productCode, out var pivots))
				{
					pivots.Add(pivot);
				}
				else
				{
					productCodeToPivotsDictionary.Add(productCode, new List<CommodityCodeProductCodePivotRecord> { pivot });
				}
			}

			return productCodeToPivotsDictionary;
		}

		RefAirlineCommodityCode ConvertCommodityCodeRecordToCommodityCodeEntity(CommodityCodeRecord record)
		{
			return new RefAirlineCommodityCode
			{
				RAC_Code = record.CommodityCode,
				RAC_Description = record.CommodityDescription,
				RAC_SpecialHandlingCodes = record.SpecialHandlingCodes,
				RAC_AirlineID = record.AirlineID
			};
		}

		RefAirlineProductCode ConvertProductCodeRecordWithPivotsToProductCodeEntity(ProductCodeRecord productCode, IList<CommodityCodeProductCodePivotRecord> pivots)
		{
			return new RefAirlineProductCode
			{
				RAR_Code = productCode.ProductCode,
				RAR_Description = productCode.ProductDescription,
				RAR_AirlineID = productCode.AirlineID,
				RefAirlineProductCodeCommodityCodePivots = pivots.Select(ConvertPivotRecordToPivotEntity).ToArray()
			};
		}

		RefAirlineProductCodeCommodityCodePivot ConvertPivotRecordToPivotEntity(CommodityCodeProductCodePivotRecord record)
		{
			return new RefAirlineProductCodeCommodityCodePivot
			{
				RPC_AirlineID = record.AirlineID,
				RPC_RAC_NKCode = record.CommodityCode,
				RPC_RAC_NKAirlineID = record.AirlineID
			};
		}

		CommodityCodeProductCodePivotRecord ParseRecord(string[] recordRow)
		{
			var record = new CommodityCodeProductCodePivotRecord();

			record.AirlineID = recordRow.ElementAtOrDefault(_headerMap[nameof(CommodityCodeProductCodePivotRecord.AirlineID)]) != null ?
				recordRow[_headerMap[nameof(CommodityCodeProductCodePivotRecord.AirlineID)]] : string.Empty;

			record.ProductCode = recordRow.ElementAtOrDefault(_headerMap[nameof(CommodityCodeProductCodePivotRecord.ProductCode)]) != null ?
				recordRow[_headerMap[nameof(CommodityCodeProductCodePivotRecord.ProductCode)]] : string.Empty;

			record.ProductDescription = recordRow.ElementAtOrDefault(_headerMap[nameof(CommodityCodeProductCodePivotRecord.ProductDescription)]) != null ?
				recordRow[_headerMap[nameof(CommodityCodeProductCodePivotRecord.ProductDescription)]] : string.Empty;

			record.CommodityCode = recordRow.ElementAtOrDefault(_headerMap[nameof(CommodityCodeProductCodePivotRecord.CommodityCode)]) != null ?
				recordRow[_headerMap[nameof(CommodityCodeProductCodePivotRecord.CommodityCode)]] : string.Empty;

			record.CommodityDescription = recordRow.ElementAtOrDefault(_headerMap[nameof(CommodityCodeProductCodePivotRecord.CommodityDescription)]) != null ?
				recordRow[_headerMap[nameof(CommodityCodeProductCodePivotRecord.CommodityDescription)]] : string.Empty;

			record.SpecialHandlingCodes = recordRow.ElementAtOrDefault(_headerMap[nameof(CommodityCodeProductCodePivotRecord.SpecialHandlingCodes)]) != null ?
				recordRow[_headerMap[nameof(CommodityCodeProductCodePivotRecord.SpecialHandlingCodes)]] : string.Empty;

			return record;
		}
	}
}
