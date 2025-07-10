using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CloneExtensions;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class TariffMerger : ITariffMerger
	{
		readonly ITariffCodeExtractor tariffCodeExtractor;
		readonly IPreferenceMapper preferenceMapper;
		readonly IDictionary<string, RefCusTariff> tariffDictionary;
		readonly bool isExport;
		readonly bool expireTariffBasedOnMaxRateDate;

		public TariffMerger(ITariffCodeExtractor tariffCodeExtractor, IPreferenceMapper preferenceMapper, bool isExport, bool expireTariffBasedOnMaxRateDate)
		{
			Argument.NotNull(preferenceMapper, nameof(preferenceMapper));

			this.tariffCodeExtractor = tariffCodeExtractor;
			this.preferenceMapper = preferenceMapper;
			tariffDictionary = new Dictionary<string, RefCusTariff>();
			this.isExport = isExport;
			this.expireTariffBasedOnMaxRateDate = expireTariffBasedOnMaxRateDate;
		}

		public TariffMerger(ITariffCodeExtractor tariffCodeExtractor, IPreferenceMapper preferenceMapper, bool expireTariffBasedOnMaxRateDate = true)
			: this(tariffCodeExtractor, preferenceMapper, isExport: false, expireTariffBasedOnMaxRateDate)
		{
		}


		public IEnumerable<RefCusTariff> ConvertToActualTariffs(IEnumerable<RefCusTariff> rawTariffs, IEnumerable<IWebTariffHeader> applicableTariffs = null, IEnumerable<IRawRateRecord> rawUomRecords = null)
		{
			Argument.NotNull(rawTariffs, nameof(rawTariffs));
			foreach (var rawTariff in rawTariffs)
			{
				if (tariffCodeExtractor == null)
				{
					if (!tariffDictionary.ContainsKey(rawTariff.ZZ1_TariffCode))
					{
						tariffDictionary.Add(rawTariff.ZZ1_TariffCode, rawTariff);
					}
				}
				else
				{
					var actualTariffHeaders = tariffCodeExtractor.GetActualTariffHeaders(rawTariff.ZZ1_TariffCode, applicableTariffs);
					ProcessTariffGroup(rawTariff, actualTariffHeaders);
				}
			}

			FixEndDates();
			FixPreferences();

			if (isExport)
			{
				// add records for just UOM to actualTariffs
				var uomTariffs = rawUomRecords.Select(r => new RefCusTariff
				{
					ZZ1_TariffCode = EUNUtils.NormalizeExportTariffCode(r.TariffHeader),
					RefCusRates = new RefCusRate[] { },
					RefCusConditions = new RefCusCondition[] { },
					RefCusTariffUOMs = new RefCusTariffUOM[] { TariffUOMGenerator.DefaultTariffUom }
				});
				foreach (var uomTariff in uomTariffs)
				{
					if (!tariffDictionary.ContainsKey(uomTariff.ZZ1_TariffCode))
					{
						tariffDictionary.Add(uomTariff.ZZ1_TariffCode, uomTariff);
					}
				}
			}
			FixTariffUOMs(rawUomRecords);

			return tariffDictionary.Values;
		}

		void FixEndDates()
		{
			if (!expireTariffBasedOnMaxRateDate)
			{
				return;
			}

			foreach (var tariff in tariffDictionary.Values)
			{
				var maxDateTime = new DateTime(2079, 6, 6, 23, 59, 0);

				if (tariff.RefCusRates != null && tariff.RefCusRates.Length > 0)
				{
					var correctDateTime = tariff.RefCusRates?.Max(x => x.ZZ2_EndDate) ?? maxDateTime;
					if (correctDateTime != maxDateTime)
					{
						tariff.ZZ1_EndDate = correctDateTime;
					}
				}
			}
		}

		void FixPreferences()
		{
			foreach (var tariff in tariffDictionary.Values)
			{
				if (tariff?.RefCusRates != null && tariff.RefCusRates.Any())
				{
					var newRates = preferenceMapper.SetPreferenceOnRates(tariff.RefCusRates);
					tariff.RefCusRates = newRates.ToArray();
				}

				if (tariff.RefCusConditions != null && tariff.RefCusConditions.Any())
				{
					var newConditions = preferenceMapper.SetPreferenceOnConditions(tariff.RefCusConditions);
					tariff.RefCusConditions = newConditions.ToArray();
				}
			}
		}

		void ProcessTariffGroup(RefCusTariff rawTariff, IEnumerable<IWebTariffHeader> actualTariffHeaders)
		{
			Argument.NotNull(rawTariff, nameof(rawTariff));
			Argument.NotNull(actualTariffHeaders, nameof(actualTariffHeaders));
			foreach (var actualTariffHeader in actualTariffHeaders)
			{
				if (!tariffDictionary.ContainsKey(actualTariffHeader.TariffCode))
				{
					AddTariffToDictionary(actualTariffHeader, rawTariff.ZZ1_ZZI_NKTariffType);
				}

				var actualTariff = tariffDictionary[actualTariffHeader.TariffCode];

				if (rawTariff.RefCusRates != null)
				{
					actualTariff.RefCusRates = MergeRawDataToActualData(rawTariff.RefCusRates, actualTariff.RefCusRates);
				}

				if (rawTariff.RefCusConditions != null)
				{
					actualTariff.RefCusConditions = MergeRawDataToActualData(rawTariff.RefCusConditions, actualTariff.RefCusConditions);
				}

				if (rawTariff.RefCusVATApplicabilities != null)
				{
					if (actualTariff.RefCusVATApplicabilities == null)
					{
						actualTariff.RefCusVATApplicabilities = rawTariff.RefCusVATApplicabilities;
					}
					else
					{
						actualTariff.RefCusVATApplicabilities = MergeRawDataToActualData(rawTariff.RefCusVATApplicabilities, actualTariff.RefCusVATApplicabilities);
					}
				}

				if (rawTariff.RefCusTariffUOMs != null)
				{
					actualTariff.RefCusTariffUOMs = MergeRawDataToActualData(rawTariff.RefCusTariffUOMs, actualTariff.RefCusTariffUOMs);
				}
			}
		}

		static T[] MergeRawDataToActualData<T>(T[] rawValues, T[] currentActualValues)
		{
			Argument.NotNull(rawValues, nameof(rawValues));
			Argument.NotNull(currentActualValues, nameof(currentActualValues));

			var newValues = rawValues.GetClone(CloningFlags.Properties);
			var actualValues = currentActualValues.ToList();

			actualValues.AddRange(newValues);

			return actualValues.ToArray();
		}

		void AddTariffToDictionary(IWebTariffHeader webTariffHeader, string tariffType)
		{
			Argument.NotNull(webTariffHeader, nameof(webTariffHeader));
			Argument.NotNullOrEmpty(webTariffHeader.TariffCode, nameof(webTariffHeader.TariffCode));

			var tariff = new RefCusTariff
			{
				ZZ1_TariffCode = webTariffHeader.TariffCode,
				ZZ1_Description = webTariffHeader.Description,
				ZZ1_CompositeKeyOnZZ5 = webTariffHeader.CompositeKey,
				ZZ1_StartDate = webTariffHeader.StartDate,
				ZZ1_EndDate = new DateTime(2079, 06, 06, 23, 59, 00),
				ZZ1_ZZI_NKTariffType = tariffType,
				RefCusRates = new RefCusRate[] { },
				RefCusConditions = new RefCusCondition[] { },
				RefCusTariffUOMs = new RefCusTariffUOM[] { TariffUOMGenerator.DefaultTariffUom }
			};

			tariffDictionary.Add(webTariffHeader.TariffCode, tariff);
		}

		void FixTariffUOMs(IEnumerable<IRawRateRecord> rawUomRecords)
		{
			if (rawUomRecords == null)
			{
				return;
			}

			foreach (var tariffCode in tariffDictionary.Keys)
			{
				var tariffUoms = tariffDictionary[tariffCode].RefCusTariffUOMs;
				var rawTariffUoms = rawUomRecords.Where(x => tariffCode.StartsWith(Common.Utils.RemoveTrailingZeros(x.TariffHeader), StringComparison.Ordinal));
				if (rawTariffUoms != null && rawTariffUoms.Any())
				{
					var generatedTariffUoms = TariffUOMGenerator.GenerateUomRecords(rawTariffUoms, skipTradeGroupAndDataGroupingMapping: isExport);
					if (tariffUoms != null)
					{
						tariffUoms = tariffUoms.Concat(generatedTariffUoms).ToArray();
					}
					else
					{
						tariffUoms = generatedTariffUoms;
					}
				}
				if (tariffUoms != null && tariffUoms.Length > 1)
				{
					tariffUoms = tariffUoms.GroupBy(x => new { x.ZZ8_UOM, x.ZZ8_Type, x.ZZ8_ZZA_NKTradeGroup }).Select(x => x.First()).ToArray();
				}
				tariffDictionary[tariffCode].RefCusTariffUOMs = tariffUoms;
			}
		}
	}
}
