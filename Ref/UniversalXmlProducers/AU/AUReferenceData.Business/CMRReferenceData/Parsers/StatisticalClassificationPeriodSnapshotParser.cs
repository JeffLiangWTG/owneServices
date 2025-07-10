using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using static CargoWise.RefDbRepo.AUReferenceData.Business.CMRConstants;
using static CargoWise.RefDbRepo.AUReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class StatisticalClassificationPeriodSnapshotParser : BaseCMRTariffDataParser
	{
		public StatisticalClassificationPeriodSnapshotParser(IDateTimeProvider dateTimeProvider)
			: base(dateTimeProvider)
		{
		}

		protected override string FileNamePrefix => ApplicationConfig.StatisticalClassificationPeriodSnapshotFilePrefix;

		protected override string OutputXMLName => "AU Customs Tariff.xml";

		protected override string DataSource => "AU Customs Tariff";

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate)
			=> CMRXMLWriterConfigurationBuilder.BuildRefCusTariffUOMConfiguration();

		protected override void ParseCore(IXmlWriter xmlWriter, string[] args, string content)
		{
			var tariffConverter = new LineToEntityConverter<RefCusTariff>(TariffMappings, 1);
			var isOutputExpiredOnly = IsOutputExpiredOnly(args);
			var todaysDate = CurrentLocalDateTime.Date;

			using (var reader = new StringReader(content))
			{
				var line = string.Empty;
				while ((line = reader.ReadLine()) != null)
				{
					if (!string.IsNullOrWhiteSpace(line))
					{
						var tariff = tariffConverter.Convert(line);
						if (tariff.ZZ1_TariffCode != null && tariff.ZZ1_StartDate != RefData_Common.MinimumDateTime)
						{
							if (IsTariffIncludedInOutput(tariff, todaysDate, isOutputExpiredOnly))
							{
								// Remove the space between the first and second token, e.g. '01011000 25'
								tariff.ZZ1_TariffCode = tariff.ZZ1_TariffCode.Replace(" ", "");
								var hasDescription = PopulateDescriptionAndCompositeKey(tariff);
								if (!hasDescription)
								{
									Console.Error.WriteLine("Skipped line '{0}' as cannot find the description and composite key for tariff code {1}", line, tariff.ZZ1_TariffCode);
									continue;
								}
								PopulateUOMs(tariff, line);
								PopulateAttributes(tariff);
								xmlWriter.PopulateData(tariff);
							}
						}
						else
						{
							Console.Error.WriteLine(InsufficientInfoErrorMessage, line);
						}
					}
				}
			}
		}

		bool PopulateDescriptionAndCompositeKey(RefCusTariff tariff)
		{
			if (DescriptionsAndCompositeKeys == null || !DescriptionsAndCompositeKeys.TryGetValue(tariff.ZZ1_TariffCode, out var sourceTariff))
			{
				return false;
			}

			tariff.ZZ1_Description = sourceTariff.TariffDescription;
			tariff.ZZ1_CompositeKeyOnZZ5 = sourceTariff.CompositeKey;
			return true;
		}

		void PopulateUOMs(RefCusTariff tariff, string line)
		{
			var uom = UOMConverter.Convert(line);
			if (uom.ZZ8_UOM != null)
			{
				var secondUom = SecondUOMConverter.Convert(line);
				secondUom.ZZ8_Type = TariffUOMTypes.CU2;
				tariff.RefCusTariffUOMs = secondUom.ZZ8_UOM != null
					? new RefCusTariffUOM[] { uom, secondUom }
					: new RefCusTariffUOM[] { uom };
			}
		}

		void PopulateAttributes(RefCusTariff tariff)
		{
			if (!STCPCharacteristicCodes.TryGetValue(tariff.ZZ1_TariffCode, out var currentStcpCharacteristicCodes))
			{
				currentStcpCharacteristicCodes = new List<string>();
			}

			if (!TRFCCharacteristicCodes.TryGetValue(tariff.ZZ1_TariffCode.Substring(0, 8), out var currentTrfcCharacteristicCodes))
			{
				currentTrfcCharacteristicCodes = new List<string>();
			}

			var characteristicCodes = currentStcpCharacteristicCodes.Union(currentTrfcCharacteristicCodes);
			var attributes = characteristicCodes.Select(code => new RefCusTariffAttribute()
			{
				ZZ3_Name = TariffAttributes.CMRCharacteristicCode,
				ZZ3_Value = code
			});

			if (AQISCommodityStatisticalClassificationCodes.TryGetValue(tariff.ZZ1_TariffCode, out var currentAqisCommodityStatisticalClassificationCodes))
			{
				var aqisAttributes = currentAqisCommodityStatisticalClassificationCodes.Select(code =>
					new RefCusTariffAttribute()
					{
						ZZ3_Name = TariffAttributeNames.AQISCommodityCode,
						ZZ3_Value = code
					});
				attributes = attributes.Concat(aqisAttributes);
			}

			tariff.RefCusTariffAttributes = attributes.ToArray();
		}

		Dictionary<string, TariffOutput> DescriptionsAndCompositeKeys
		{
			get
			{
				if (descriptionsAndCompositeKeys == null)
				{
					if (File.Exists(TariffsSourceFilePath))
					{
						var existingJson = File.ReadAllText(TariffsSourceFilePath);
						var tariffs = JsonSerializer.Deserialize<List<TariffOutput>>(existingJson, jsonOptions);
						descriptionsAndCompositeKeys = tariffs.ToDictionary(t => t.Tariff, t => t);
					}
				}

				return descriptionsAndCompositeKeys;
			}
		}

		Dictionary<string, TariffOutput> descriptionsAndCompositeKeys;

		public virtual Dictionary<string, List<string>> STCPCharacteristicCodes => stcpCharacteristicCodes ??
			(stcpCharacteristicCodes = StatisticalClassificationPeriodCharacteristicParser.Parse(
				ApplicationConfig.StatisticalClassificationPeriodCharacteristicFilePrefix,
				ApplicationConfig.AUReferenceFilesDirectory));

		Dictionary<string, List<string>> stcpCharacteristicCodes;

		public virtual Dictionary<string, List<string>> TRFCCharacteristicCodes => trfcCharacteristicCodes ??
			(trfcCharacteristicCodes = TariffClassificationCharacteristicParser.Parse(
				ApplicationConfig.TariffClassificationCharacteristicFilePrefix,
				ApplicationConfig.AUReferenceFilesDirectory));

		Dictionary<string, List<string>> trfcCharacteristicCodes;

		public virtual Dictionary<string, List<string>> AQISCommodityStatisticalClassificationCodes => aqisCommodityStatisticalClassificationCodes ??
			(aqisCommodityStatisticalClassificationCodes = AQISCommodityStatisticalClassificationParser.Parse(
				ApplicationConfig.AQISCommodityStatisticalClassificationFilePrefix,
				ApplicationConfig.AUReferenceFilesDirectory));

		Dictionary<string, List<string>> aqisCommodityStatisticalClassificationCodes;

		LineToEntityConverter<RefCusTariffUOM> UOMConverter => uomConverter ?? (uomConverter = new LineToEntityConverter<RefCusTariffUOM>(TariffUOMMappings, 1));

		LineToEntityConverter<RefCusTariffUOM> uomConverter;

		LineToEntityConverter<RefCusTariffUOM> SecondUOMConverter => secondUomConverter ?? (secondUomConverter = new LineToEntityConverter<RefCusTariffUOM>(TariffSecondUOMMappings, 1));

		LineToEntityConverter<RefCusTariffUOM> secondUomConverter;

		readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
			PropertyNameCaseInsensitive = true,
		};

		static PropertyMapping<RefCusTariff>[] TariffMappings => new[]
		{
			new PropertyMapping<RefCusTariff>(entity => entity.ZZ1_TariffCode, 1, 11),
			new PropertyMapping<RefCusTariff>(entity => entity.ZZ1_StartDate, 39, 8, RefData_Common.MinimumDateTime),
			new PropertyMapping<RefCusTariff>(entity => entity.ZZ1_EndDate, 48, 8, RefData_Common.MaximumDateTime)
		};

		static PropertyMapping<RefCusTariffUOM>[] TariffUOMMappings => new PropertyMapping<RefCusTariffUOM>[]
		{
			new PropertyMapping<RefCusTariffUOM>(entity => entity.ZZ8_UOM, 62, 2)
		};

		static PropertyMapping<RefCusTariffUOM>[] TariffSecondUOMMappings => new PropertyMapping<RefCusTariffUOM>[]
		{
			new PropertyMapping<RefCusTariffUOM>(entity => entity.ZZ8_UOM, 65, 2)
		};
	}
}
