using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.ILReferenceData.Business.Schemas.CustomsTariff;
using CargoWise.xTMessaging.Integration;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public class ILCustomsTariffProcessor
	{
		public ILCustomsTariffProcessor(ILogger logger)
		{
			this.logger = logger;
		}

		public void GenerateFiles(
			DateTime publicationDate,
			Dictionary<string, CustomsItemComputedData> customsItemComputedData,
			Dictionary<int, PropertiesDetailsHistory> propertiesDetailsHistory,
			Dictionary<int, CustomsItemDetailsHistory> customsItemDetailsHistory)
		{
			logger.Log(LogType.Information, "Generating Customs Tariff files...");

			var result = new List<RefCusTariff>();

			foreach (var customsItemComputedDataItem in customsItemComputedData.Values.Where(x => x.IsLeaf && x.EndDate >= publicationDate))
			{
				propertiesDetailsHistory.TryGetValue(customsItemComputedDataItem.ValidPropertiesDetailsHistoryID, out var propertiesDetailsHistoryItem);
				customsItemDetailsHistory.TryGetValue(customsItemComputedDataItem.ValidCustomsItemDetailsHistoryID, out var customsItemDetailsHistoryItem);

				var tariff = new RefCusTariff
				{
					ZZ1_TariffCode = customsItemComputedDataItem.BaseFullClassification,
					ZZ1_Description = customsItemComputedDataItem.GoodsDescription,
					ZZ1_StartDate = customsItemComputedDataItem.StartDate,
					ZZ1_EndDate = customsItemComputedDataItem.EndDate,
					ZZ1_ZZI_NKTariffType = GetTariffType(customsItemComputedDataItem.CustomsBookTypeIDNum),
					ZZ1_ZZF_NKTaxOrFeeCode = propertiesDetailsHistoryItem?.VatDiscountRate == 100 ? UniversalDataHelper.Constants.VZR : UniversalDataHelper.Constants.VAT,
					RefCusTariffAttributes = new RefCusTariffAttribute[]
						{
							new RefCusTariffAttribute
							{
								ZZ3_Value = customsItemComputedDataItem.ComputedCheckDigit.HasValue ? customsItemComputedDataItem.ComputedCheckDigit.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : string.Empty
							}
						},
					RefCusTariffUOMs = new RefCusTariffUOM[]
						{
							new RefCusTariffUOM
							{
								ZZ8_UOM = UnitOfMeasurement.TryGetValue(customsItemComputedDataItem.MeasurementUnitID, out var uom) ? uom : UnitOfMeasurement[1],
							}
						},
					RefCusTariffLanguages = new RefCusTariffLanguage[]
						{
							new RefCusTariffLanguage
							{
								ZX7_Description = customsItemDetailsHistoryItem?.EnglishGoodsDescription
							}
						}
				};

				result.Add(tariff);
			}

			StoreDataCollection(publicationDate, result, logger);
		}

		static void StoreDataCollection(DateTime publicationDate, List<RefCusTariff> dataCollection, ILogger logger)
		{
			var dataSource = UniversalDataHelper.Constants.ILTariffs;
			RefDataRepoFileWriter<RefCusTariff>.StoreDataCollection(dataSource, publicationDate, Path.Combine(ApplicationConfig.Instance.OutputDirectory, $"{dataSource}.xml"), GetXmlWriterConfiguration(dataSource), dataCollection);
			logger.Log(LogType.Information, $"Processing {dataSource} done.");
		}

		static XmlWriterConfiguration GetXmlWriterConfiguration(string dataSource)
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var tariffConfiguration = new EntityTypeConfiguration<RefCusTariff>(true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_TariffCode, true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_Description);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_IAMUnique, false, 0);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_StartDate);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_EndDate);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_ZZI_NKTariffType, true);
			tariffConfiguration.IncludeColumn(x => x.ZZ1_ZZF_NKTaxOrFeeCode);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_CompositeKeyOnZZ5, false, string.Empty);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.Israel);
			tariffConfiguration.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.Israel);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffAttributes);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffUOMs);
			tariffConfiguration.IncludeColumn(x => x.RefCusTariffLanguages);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffConfiguration);

			var tariffAttributeConfiguration = new EntityTypeConfiguration<RefCusTariffAttribute>(true);
			tariffAttributeConfiguration.IncludeColumnWithConstantValue(x => x.ZZ3_Name, true, UniversalDataHelper.Constants.CheckDigit);
			tariffAttributeConfiguration.IncludeColumn(x => x.ZZ3_Value);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffAttributeConfiguration);

			var tariffUOMConfiguration = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			tariffUOMConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_Type, true, UniversalDataHelper.Constants.CU1);
			tariffUOMConfiguration.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, UniversalDataHelper.Constants.Israel);
			tariffUOMConfiguration.IncludeColumn(x => x.ZZ8_UOM);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffUOMConfiguration);

			var tariffLanguagesConfiguration = new EntityTypeConfiguration<RefCusTariffLanguage>(true);
			tariffLanguagesConfiguration.IncludeColumnWithConstantValue(x => x.ZX7_ZX6_NKLanguage, true, UniversalDataHelper.Constants.English);
			tariffLanguagesConfiguration.IncludeColumn(x => x.ZX7_Description);
			writerConfiguration.IncludeEntityTypeConfiguration(tariffLanguagesConfiguration);

			return writerConfiguration;
		}

		string GetTariffType(int customsBookTypeIDNum)
		{
			return TariffTypes.TryGetValue(customsBookTypeIDNum, out var tariffType)
				? tariffType
				: throw new ArgumentOutOfRangeException($"There is no tariff type matching customsBookTypeIDNum equal to {customsBookTypeIDNum}");
		}

		readonly Dictionary<int, string> UnitOfMeasurement = new Dictionary<int, string>
		{
			{ 1, "EA" },
			{ 2, "PR" },
			{ 3, "DZN" },
			{ 4, "GRO" },
			{ 5, "T3" },
			{ 6, "KGM" },
			{ 7, "GRM" },
			{ 8, "TNE" },
			{ 9, "LBR" },
			{ 10, "CTM" },
			{ 11, "MTQ" },
			{ 12, "LTR" },
			{ 13, "CMQ" },
			{ 14, "CLT" },
			{ 15, "INQ" },
			{ 16, "YDQ" },
			{ 17, "GLL" },
			{ 18, "PT" },
			{ 19, "MTR" },
			{ 20, "DMT" },
			{ 21, "CMT" },
			{ 22, "MMT" },
			{ 23, "KMT" },
			{ 24, "SMI" },
			{ 25, "INH" },
			{ 26, "FOT" },
			{ 27, "YRD" },
			{ 28, "MTK" },
			{ 29, "KMK" },
			{ 30, "DMK" },
			{ 31, "CMK" },
			{ 32, "MMK" },
			{ 33, "INK" },
			{ 34, "FTK" },
			{ 35, "YDK" },
			{ 36, "MIK" },
			{ 37, "BTU" },
			{ 38, "SEC" },
			{ 39, "MIN" },
			{ 40, "HUR" },
			{ 41, "DAY" },
			{ 42, "C26" },
			{ 43, "C47" },
			{ 44, "WEE" },
			{ 45, "MON" },
			{ 46, "ANN" },
			{ 47, "ILA" },
			{ 48, "CEL" },
			{ 49, "PK" },
			{ 50, "M5" },
			{ 51, "T3C" },
			{ 52, "VLT" },
			{ 53, "ONZ" },
			{ 54, "FAR" },
			{ 55, "RT" },
			{ 56, "MLT" },
			{ 57, "WTT" },
			{ 58, "HP" },
			{ 59, "Banknote" },
			{ 60, "B97" },
			{ 99, "AAA" }
		};

		readonly Dictionary<int, string> TariffTypes = new Dictionary<int, string>
		{
			{ 1, "IMP" },
			{ 2, "EXP" },
			{ 3, "AUT" },
		};

		readonly ILogger logger;
	}
}
