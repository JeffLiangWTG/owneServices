using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;
using static System.FormattableString;
using static CargoWise.RefDbRepo.SharedReferenceData.Services.Common.CommonHelper;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff
{
	public abstract class TariffBuilderBase : BuilderBase<RefCusTariff>
	{
		protected TariffBuilderBase(IDateTimeProvider dateTimeProvider, StringBuilder errorCollector) : base(dateTimeProvider, errorCollector)
		{
		}

		protected override IEnumerable<RefCusTariff> ConvertToRefModels(List<ITariffModel> data)
		{
			return ConvertMeasuresToRefModels(data.Cast<Measure>().ToList());
		}

		protected override void DuplicateError(RefCusTariff refModel, string uniqueId, string chapterFilter)
		{
			var msg = Invariant($"RefCusTariff duplicate exists. Key: '{uniqueId}' Filter: '{chapterFilter}'");
			ErrorCollector.AppendLine(msg);
		}

		protected override bool IsExpired(RefCusTariff refModel) => IsDateExpired(refModel.ZZ1_EndDate.Date);

		bool IsDateExpired(DateTime date) => date < dateTimeProvider.UTCHistoricalDate;

		protected override bool IsValid(RefCusTariff refModel, string chapterFilter)
		{
			bool valid = true;

			var validationErrors = new StringBuilder();

			if (string.IsNullOrWhiteSpace(refModel.ZZ1_TariffCode))
			{
				validationErrors.Append("ZZ1_TariffCode is required. ");
				valid = false;
			}

			if (string.IsNullOrWhiteSpace(refModel.ZZ1_Description))
			{
				validationErrors.Append("ZZ1_Description is required. ");
				valid = false;
			}

			if (refModel.RefCusRates != null)
			{
				foreach (var rate in refModel.RefCusRates)
				{
					if (rate.ZZ2_RateFormula.Length > 500)
					{
						validationErrors.Append(CultureInfo.InvariantCulture, $"ZZ2_RateFormula maxiumum length of 500 exceeded (actual: {rate.ZZ2_RateFormula.Length}). ");
						valid = false;
						break;
					}
				}
			}

			if (!valid)
			{
				var msg = Invariant($"RefCusTariff validation error. Key: '{refModel.ZZ1_TariffCode}' Errors: '{validationErrors}' Filter: '{chapterFilter}'");
				ErrorCollector.AppendLine(msg);
			}

			return valid;
		}

		protected override string UniqueId(RefCusTariff refModel) => Invariant($"{refModel.ZZ1_TariffCode}_{refModel.ZZ1_ZZI_NKTariffType}_{refModel.ZZ1_IAMUnique}_{refModel.ZZ1_StartDate:yyyyMMddhhmmss}_{refModel.ZZ1_EndDate:yyyyMMddhhmmss}");

		protected virtual bool IsEntityTypeConfigurationRefCusTariffEnableNullOrEmptyKeyMatching => true;

		protected override XmlWriterConfiguration XmlWriterConfiguration()
		{
			var writerConfig = new XmlWriterConfiguration();
			var tariffConfig = new EntityTypeConfiguration<RefCusTariff>(IsEntityTypeConfigurationRefCusTariffEnableNullOrEmptyKeyMatching);

			tariffConfig.IncludeColumn(x => x.ZZ1_TariffCode, true);
			if (DefaultTariffType != null)
			{
				tariffConfig.IncludeColumnWithDefaultValue(x => x.ZZ1_ZZI_NKTariffType, true, DefaultTariffType);
			}
			else
			{
				tariffConfig.IncludeColumn(x => x.ZZ1_ZZI_NKTariffType, true);
			}
			tariffConfig.IncludeColumn(x => x.ZZ1_IAMUnique, true);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZI_ZZZ_NKDataGrouping, true, ParentDataGrouping);
			tariffConfig.IncludeColumnWithConstantValue(x => x.ZZ1_ZZZ_NKDataGrouping, true, ParentDataGrouping);
			tariffConfig.IncludeColumn(x => x.ZZ1_Description);
			tariffConfig.IncludeColumn(x => x.ZZ1_StartDate);
			tariffConfig.IncludeColumn(x => x.ZZ1_EndDate);
			tariffConfig.IncludeColumn(x => x.ZZ1_CompositeKeyOnZZ5);
			tariffConfig.IncludeColumn(x => x.RefCusConditions);
			tariffConfig.IncludeColumn(x => x.RefCusRates);
			tariffConfig.IncludeColumn(x => x.RefCusTariffUOMs);
			tariffConfig.IncludeColumn(x => x.RefCusVATApplicabilities);

			writerConfig.IncludeEntityTypeConfiguration(tariffConfig);

			var uomConfig = new EntityTypeConfiguration<RefCusTariffUOM>(true);
			uomConfig.IncludeColumn(x => x.ZZ8_Type, true);
			uomConfig.IncludeColumn(x => x.ZZ8_ZZA_NKTradeGroup, true);
			uomConfig.IncludeColumnWithConstantValue(x => x.ZZ8_ZZZ_NKDataGrouping, true, DataGrouping);
			uomConfig.IncludeColumn(x => x.ZZ8_ZZA_ZZZ_NKDataGrouping, true);
			uomConfig.IncludeColumn(x => x.ZZ8_UOM);

			writerConfig.IncludeEntityTypeConfiguration(uomConfig);

			var rateConfig = new EntityTypeConfiguration<RefCusRate>(true);
			rateConfig.IncludeColumn(x => x.ZZ2_ZY1_NKRateCode, true);
			rateConfig.IncludeColumn(x => x.ZZ2_ZY1_ZZR_NKRateType, true);
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping, true, DataGrouping);
			rateConfig.IncludeColumnWithConstantValue(x => x.ZZ2_ZZZ_NKDataGrouping, true, DataGrouping);
			rateConfig.IncludeColumn(x => x.RefCusApplicabilities, true);
			rateConfig.IncludeColumn(x => x.ZZ2_StartDate);
			rateConfig.IncludeColumn(x => x.ZZ2_EndDate);
			rateConfig.IncludeColumn(x => x.ZZ2_RateFormula);
			rateConfig.IncludeColumn(x => x.ZZ2_ZZS_NKPreference, true);
			rateConfig.IncludeColumn(x => x.ZZ2_ZZS_ZZZ_NKDataGrouping, true);
			rateConfig.IncludeColumn(x => x.RefCusRateUOMs);

			writerConfig.IncludeEntityTypeConfiguration(rateConfig);

			var appConfig = new EntityTypeConfiguration<RefCusApplicability>(true);
			appConfig.IncludeColumn(x => x.ZZT_AdditionalCode, true);
			appConfig.IncludeColumn(x => x.ZZT_OrderNumber, true);
			appConfig.IncludeColumn(x => x.ZZT_ZZA_NKTradeGroup, true);
			appConfig.IncludeColumnWithConstantValue(x => x.ZZT_ZZA_ZZZ_NKDataGrouping, true, ParentDataGrouping);
			appConfig.IncludeColumn(x => x.ZZT_StartDate);
			appConfig.IncludeColumn(x => x.ZZT_EndDate);
			appConfig.IncludeColumn(x => x.RefCusExcludedTradeGroups);

			writerConfig.IncludeEntityTypeConfiguration(appConfig);

			var rateUOMConfig = new EntityTypeConfiguration<RefCusRateUOM>(true);
			rateUOMConfig.IncludeColumn(x => x.ZXG_UOM, true);

			writerConfig.IncludeEntityTypeConfiguration(rateUOMConfig);

			var exclConfig = new EntityTypeConfiguration<RefCusExcludedTradeGroup>(true);
			exclConfig.IncludeColumn(x => x.ZZC_ZZA_NKTradeGroup, true);
			exclConfig.IncludeColumnWithConstantValue(x => x.ZZC_ZZA_ZZZ_NKDataGrouping, true, DataGrouping);

			writerConfig.IncludeEntityTypeConfiguration(exclConfig);

			var conditionConfig = new EntityTypeConfiguration<RefCusCondition>(true);
			conditionConfig.IncludeColumn(x => x.RefCusApplicabilities, true);
			conditionConfig.IncludeColumn(x => x.ZX1_Comment, true);
			conditionConfig.IncludeColumn(x => x.ZX1_ZX2_NKConditionType, true);
			conditionConfig.IncludeColumnWithConstantValue(x => x.ZX1_ZX2_ZZZ_NKDataGrouping, true, DataGrouping);
			conditionConfig.IncludeColumnWithConstantValue(x => x.ZX1_ZZZ_NKDataGrouping, true, DataGrouping);
			conditionConfig.IncludeColumnWithConstantValue(x => x.ZX1_ConditionValueTrueMeansStop, false, "False");
			conditionConfig.IncludeColumn(x => x.ZX1_StartDate);
			conditionConfig.IncludeColumn(x => x.ZX1_EndDate);
			conditionConfig.IncludeColumn(x => x.ZX1_IsImport);
			conditionConfig.IncludeColumn(x => x.ZX1_IsExport);
			conditionConfig.IncludeColumn(x => x.ZX1_ZZS_NKPreference, true);
			conditionConfig.IncludeColumn(x => x.ZX1_ZZS_ZZZ_NKDataGrouping, true);
			conditionConfig.IncludeColumnWithDefaultValue(x => x.ZX1_Source, false, $"{DataGrouping} Tariff");
			conditionConfig.IncludeColumn(x => x.RefCusConditionValues, true);

			writerConfig.IncludeEntityTypeConfiguration(conditionConfig);

			var conValConfig = new EntityTypeConfiguration<RefCusConditionValue>(true);
			conValConfig.IncludeColumn(x => x.ZX3_Value, true);
			conValConfig.IncludeColumn(x => x.ZX3_ZX4_NKValueType, true);
			conValConfig.IncludeColumnWithConstantValue(x => x.ZX3_ZX4_ZZZ_NKDataGrouping, true, ParentDataGrouping);

			writerConfig.IncludeEntityTypeConfiguration(conValConfig);

			var vatApplConfig = new EntityTypeConfiguration<RefCusVATApplicability>(true);
			vatApplConfig.IncludeColumnWithConstantValue(x => x.ZX5_ZZZ_NKDataGrouping, true, DataGrouping);
			vatApplConfig.IncludeColumn(x => x.ZX5_ZZF_NKTaxOrFeeCode, true);
			vatApplConfig.IncludeColumn(x => x.ZX5_AdditionalCode, true);
			vatApplConfig.IncludeColumn(x => x.ZX5_StartDate);
			vatApplConfig.IncludeColumn(x => x.ZX5_EndDate);

			writerConfig.IncludeEntityTypeConfiguration(vatApplConfig);

			return writerConfig;
		}

		IEnumerable<RefCusTariff> ConvertMeasuresToRefModels(List<Measure> measures)
		{
			var models = new List<RefCusTariff>();
			var tariffTypeList = new List<string> { MeasureHelper.TariffTypeImport, MeasureHelper.TariffTypeExport };

			foreach (var measure in measures.OrderBy(x => x.ItemId).ThenBy(x => x.IsAdditionalInfo).ThenBy(x => x.CalcNomenclatureStartDate).ThenBy(x => x.CalcNomenclatureEndDate))
			{
				var startDate = measure.CalcNomenclatureStartDate;
				var endDate = measure.CalcNomenclatureEndDate;

				if (!IsDateExpired(endDate))
				{
					var actualTariffTypes = measure.TariffTypes.ToList();

					tariffTypeList.ForEach((tariffType) =>
					{
						var tariffCode = tariffType == MeasureHelper.TariffTypeImport ? measure.ItemId : measure.ItemId.Substring(0, 8);

						var similiarTariffs = models.Where(x => x.ZZ1_ZZI_NKTariffType == tariffType && x.ZZ1_TariffCode == tariffCode);
						RefCusTariff refTariff = similiarTariffs?.FirstOrDefault(x => x.ZZ1_CompositeKeyOnZZ5 == measure.CompositeKey);
						if (refTariff == null)
						{
							var uniqueId = (short)(similiarTariffs?.Count() ?? 0);

							refTariff = CreateRefCusTariff(measure, tariffType, tariffCode, uniqueId);
							models.Add(refTariff);
						}
						else
						{
							if (startDate < refTariff.ZZ1_StartDate)
							{
								refTariff.ZZ1_StartDate = startDate;
							}

							if (endDate > refTariff.ZZ1_EndDate)
							{
								refTariff.ZZ1_EndDate = endDate;
							}

							if (!measure.IsAdditionalInfo)
							{
								refTariff.ZZ1_Description = measure.Description;
							}
						}

						var isActualMeasure = actualTariffTypes.Contains(tariffType);
						if (isActualMeasure)
						{
							var newRefCusApplicability = CreateRefCusApplicability(measure);

							if (!measure.IsAdditionalInfo)
							{
								if (measure.ConditionClass == MeasureHelper.ConditionClass.Rate)
								{
									AddRefCusRate(refTariff, measure, newRefCusApplicability);
								}

								if (measure.ConditionClass != MeasureHelper.ConditionClass.Vat)
								{
									AddRefCusConditions(refTariff, tariffType, measure, newRefCusApplicability);
								}
								else
								{
									AddRefCusVATApplicability(refTariff, measure);
								}
							}
							else if (measure.ConditionClass == MeasureHelper.ConditionClass.Vat)
							{
								AddRefCusVATApplicability(refTariff, measure);
							}
							else if (!measure.IsSupplementaryUnit)
							{
								ApplyAdditonalRefCusApplicability(refTariff, newRefCusApplicability);
							}
						}

						AddRefCusTariffUOMs(refTariff, measure, isActualMeasure);
					});
				}
			}

			return FilterData(models);
		}

		protected List<RefCusTariff> FilterData(List<RefCusTariff> data) => data.FindAll(TariffFilter);

		protected virtual Predicate<RefCusTariff> TariffFilter => (tariff) => true;

		protected virtual void AddRefCusRate(RefCusTariff tariff, Measure measure, RefCusApplicability newRefCusApplicability)
		{
			if (!IsDateExpired(measure.CalcEndDate) && !string.IsNullOrEmpty(measure.Formula))
			{
				foreach (var pref in measure.Preferences)
				{
					var preference = string.IsNullOrEmpty(pref) ? null : pref;

					var existingRate = tariff.RefCusRates?.FirstOrDefault(x => x.ZZ2_ZZS_NKPreference == preference
																			&& x.ZZ2_ZY1_NKRateCode == measure.RateCode
																			&& x.ZZ2_RateFormula == measure.Formula
																			&& x.ZZ2_ZY1_ZZR_NKRateType == measure.RateType);

					if (existingRate != null)
					{
						existingRate.RefCusApplicabilities = AddRefCusApplicability(existingRate.RefCusApplicabilities, newRefCusApplicability);
					}
					else
					{
						var rates = tariff.RefCusRates ?? new RefCusRate[] { };
						Array.Resize(ref rates, rates.Length + 1);
						rates[rates.Length - 1] = CreateRefCusRate(measure, preference, newRefCusApplicability);
						tariff.RefCusRates = rates;
					}
				}
			}
		}

		protected virtual void AddRefCusTariffUOMs(RefCusTariff tariff, Measure measure, bool isActualMeasure)
		{
			var uoms = CreateRefCusTariffUOMs(measure, isActualMeasure);

			foreach (var uom in uoms)
			{
				if (uom != null && !tariff.RefCusTariffUOMs.Any(x => x.ZZ8_UOM == uom.ZZ8_UOM && x.ZZ8_ZZA_NKTradeGroup == uom.ZZ8_ZZA_NKTradeGroup && x.ZZ8_Type == uom.ZZ8_Type))
				{
					var uomArr = tariff.RefCusTariffUOMs;
					Array.Resize(ref uomArr, uomArr.Length + 1);
					uomArr[uomArr.Length - 1] = uom;
					tariff.RefCusTariffUOMs = uomArr;
				}
			}
		}

		protected virtual void AddRefCusConditions(RefCusTariff tariff, string tariffType, Measure measure, RefCusApplicability newRefCusApplicability)
		{
			if (!IsDateExpired(measure.CalcEndDate) && (measure.Conditions?.Any() ?? false))
			{
				var isImp = tariffType == MeasureHelper.TariffTypeImport;
				var isExp = tariffType == MeasureHelper.TariffTypeExport;

				foreach (var pref in measure.Preferences)
				{
					var preference = string.IsNullOrEmpty(pref) ? null : pref;

					foreach (var conditionGrp in measure.Conditions.GroupBy(x => x.ConditionCode))
					{
						var mc = conditionGrp.First();

						var values = CreateRefCusConditionValues(measure, conditionGrp.ToList());
						var valuesKey = GetConditionValuesKey(values);

						var existingCondition = tariff.RefCusConditions?.FirstOrDefault(x => x.ZX1_ZZS_NKPreference == preference
																						&& x.ZX1_ZX2_NKConditionType == measure.MeasureType
																						&& x.ZX1_Comment == $"Condition {mc.ConditionCode}: {mc.ConditionCodeDescription}"
																						&& x.ZX1_IsExport == isExp
																						&& x.ZX1_IsImport == isImp
																						&& GetConditionValuesKey(x.RefCusConditionValues) == valuesKey);

						if (existingCondition != null)
						{
							if (measure.CalcStartDate < existingCondition.ZX1_StartDate)
							{
								existingCondition.ZX1_StartDate = measure.CalcStartDate;
							}

							if (measure.CalcEndDate > existingCondition.ZX1_EndDate)
							{
								existingCondition.ZX1_EndDate = measure.CalcEndDate;
							}
							existingCondition.RefCusApplicabilities = AddRefCusApplicability(existingCondition.RefCusApplicabilities, newRefCusApplicability);
						}
						else
						{
							var condition = CreateRefCusCondition(measure, tariff.ZZ1_ZZI_NKTariffType, preference, conditionGrp.ToList(), newRefCusApplicability, values);
							if (condition != null)
							{
								var conditions = tariff.RefCusConditions ?? new RefCusCondition[] { };
								Array.Resize(ref conditions, conditions.Length + 1);
								conditions[conditions.Length - 1] = condition;
								tariff.RefCusConditions = conditions;
							}
						}
					}
				}
			}
		}

		static string GetConditionValuesKey(RefCusConditionValue[] values)
		{
			return string.Join(",", (values ?? new RefCusConditionValue[] { })
						.Select(x => $"{x.ZX3_ZX4_NKValueType}:{x.ZX3_Value}")
						.OrderBy(x => x));
		}

		protected virtual void AddRefCusVATApplicability(RefCusTariff tariff, Measure measure)
		{
			if (!string.IsNullOrEmpty(measure.VatCode))
			{
				var startDate = measure.CalcStartDate;
				var endDate = measure.CalcEndDate;
				var addCode = $"{measure.AdditionalCodeType}{measure.AdditionalCode}";
				var existing = tariff.RefCusVATApplicabilities?.FirstOrDefault(x => x.ZX5_ZZF_NKTaxOrFeeCode == measure.VatCode
																					&& x.ZX5_AdditionalCode == addCode
																					&& x.ZX5_StartDate <= endDate && x.ZX5_EndDate >= startDate);

				if (existing == null)
				{
					var vatAppl = CreateRefCusVATApplicability(measure);
					var vatAppls = tariff.RefCusVATApplicabilities ?? new RefCusVATApplicability[] { };
					Array.Resize(ref vatAppls, vatAppls.Length + 1);
					vatAppls[vatAppls.Length - 1] = vatAppl;
					tariff.RefCusVATApplicabilities = vatAppls;
				}
				else
				{
					if (startDate < existing.ZX5_StartDate)
					{
						existing.ZX5_StartDate = startDate;
					}

					if (endDate > existing.ZX5_EndDate)
					{
						existing.ZX5_EndDate = endDate;
					}
				}
			}
		}

		RefCusTariff CreateRefCusTariff(Measure measure, string tariffType, string tariffCode, short uniqueId)
		{
			return new RefCusTariff
			{
				ZZ1_TariffCode = tariffCode,
				ZZ1_Description = measure.Description,
				ZZ1_StartDate = measure.CalcNomenclatureStartDate,
				ZZ1_EndDate = measure.CalcNomenclatureEndDate,
				ZZ1_CompositeKeyOnZZ5 = measure.CompositeKey,
				ZZ1_ZZZ_NKDataGrouping = DataGrouping,
				ZZ1_ZZI_NKTariffType = tariffType,
				ZZ1_IAMUnique = uniqueId,

				RefCusRates = new RefCusRate[] { },
				RefCusTariffUOMs = new RefCusTariffUOM[] { }
			};
		}

		RefCusRate CreateRefCusRate(Measure measure, string preference, RefCusApplicability newRefCusApplicability)
		{
			return string.IsNullOrEmpty(measure.Formula) ? null : new RefCusRate
			{
				RefCusApplicabilities = new[] { newRefCusApplicability },
				ZZ2_ZY1_NKRateCode = measure.RateCode,
				ZZ2_ZY1_ZZR_NKRateType = measure.RateType,
				ZZ2_ZZS_NKPreference = preference,
				ZZ2_ZZS_ZZZ_NKDataGrouping = string.IsNullOrEmpty(preference) ? null : DataGrouping,
				ZZ2_StartDate = DefaultValues.MinimumDateTime,
				ZZ2_EndDate = DefaultValues.MaximumDateTime,
				ZZ2_RateFormula = measure.Formula,

				RefCusRateUOMs = GetRefCusRateUOM(measure)
			};
		}

		static RefCusRateUOM[] GetRefCusRateUOM(Measure measure)
		{
			var uoms = measure.Components?.Select(x => $"{x.MeasurementUnit}{x.MeasurementUnitQualifier}").ToList() ?? new List<string>();

			uoms.AddRange(measure.Conditions?.Select(x => $"{x.MeasurementUnit}{x.MeasurementUnitQualifier}").ToList() ?? new List<string>());
			uoms.AddRange(measure.Conditions?.SelectMany(x => x.Components ?? new List<MeasureComponent>())?.Select(y => $"{y.MeasurementUnit}{y.MeasurementUnitQualifier}")?.ToList() ?? new List<string>());

			return uoms.Distinct().Where(x => !string.IsNullOrEmpty(x)).Select(u => new RefCusRateUOM { ZXG_UOM = u }).ToArray();
		}

		static RefCusApplicability CreateRefCusApplicability(Measure measure)
		{
			return new RefCusApplicability
			{
				ZZT_AdditionalCode = $"{measure.AdditionalCodeType}{measure.AdditionalCode}",
				ZZT_OrderNumber = measure.OrderNumber,
				ZZT_ZZA_NKTradeGroup = measure.GeographicalArea,
				ZZT_StartDate = measure.CalcStartDate,
				ZZT_EndDate = measure.CalcEndDate,
				RefCusExcludedTradeGroups = measure.ExcludedGeographicalAreas?.Select(x => new RefCusExcludedTradeGroup { ZZC_ZZA_NKTradeGroup = x }).ToArray() ?? new RefCusExcludedTradeGroup[] { }
			};
		}

		IEnumerable<RefCusTariffUOM> CreateRefCusTariffUOMs(Measure measure, bool isActualMeasure)
		{
			yield return new RefCusTariffUOM
			{
				ZZ8_Type = "CU1",
				ZZ8_UOM = "KGM"
			};

			if (isActualMeasure)
			{
				var supUnit = measure.Components?.FirstOrDefault(x => x.DutyExpression == "99");
				if (supUnit != null)
				{
					yield return new RefCusTariffUOM
					{
						ZZ8_Type = "CU2",
						ZZ8_UOM = $"{supUnit.MeasurementUnit}{supUnit.MeasurementUnitQualifier}",
						ZZ8_ZZA_NKTradeGroup = measure.GeographicalArea,
						ZZ8_ZZA_ZZZ_NKDataGrouping = DataGrouping
					};
				}
			}
		}

		RefCusCondition CreateRefCusCondition(Measure measure, string tariffType, string preference, List<MeasureCondition> measureConditions, RefCusApplicability newRefCusApplicability, RefCusConditionValue[] values)
		{
			var mc = measureConditions.First();

			if (!mc.IsRateFormulaCondition || mc.IsCertificate)
			{
				if (values.Any())
				{
					return new RefCusCondition
					{
						RefCusApplicabilities = new[] { newRefCusApplicability },

						ZX1_Comment = $"Condition {mc.ConditionCode}: {mc.ConditionCodeDescription}",
						ZX1_ZX2_NKConditionType = measure.MeasureType,
						ZX1_ZZS_NKPreference = preference,
						ZX1_ZZS_ZZZ_NKDataGrouping = string.IsNullOrEmpty(preference) ? null : DataGrouping,
						ZX1_StartDate = DefaultValues.MinimumDateTime,
						ZX1_EndDate = DefaultValues.MaximumDateTime,
						ZX1_IsExport = tariffType == MeasureHelper.TariffTypeExport,
						ZX1_IsImport = tariffType == MeasureHelper.TariffTypeImport,
						RefCusConditionValues = values
					};
				}
			}

			return null;
		}

		static RefCusConditionValue[] CreateRefCusConditionValues(Measure measure, List<MeasureCondition> measureConditions)
		{
			if (measure.ConditionClass == MeasureHelper.ConditionClass.Class)
			{
				return new RefCusConditionValue[] { CreateRefCusConditionValue(measure.Formula, "SNR") };
			}
			else
			{
				var values = new List<RefCusConditionValue>();

				var conditions = measureConditions.Where(x => !MeasureHelper.IsNegativeAction(x.MeasureAction));

				foreach (var mc in conditions)
				{
					if (!string.IsNullOrEmpty(mc.CertificateCode))
					{
						values.Add(CreateRefCusConditionValue($"{mc.CertificateTypeCode}{mc.CertificateCode}", "SUP"));
					}
					else if (!string.IsNullOrEmpty(mc.Formula))
					{
						var valueType = conditions.Any(x => !string.IsNullOrEmpty(x.CertificateCode)) ? "FRM" : "SNR";
						values.Add(CreateRefCusConditionValue(mc.Formula, valueType));
					}
				}

				return values.ToArray();
			}
		}

		static RefCusConditionValue CreateRefCusConditionValue(string value, string valueType)
		{
			return new RefCusConditionValue
			{
				ZX3_ZX4_NKValueType = valueType,
				ZX3_Value = value
			};
		}

		static RefCusVATApplicability CreateRefCusVATApplicability(Measure measure)
		{
			return new RefCusVATApplicability
			{
				ZX5_AdditionalCode = $"{measure.AdditionalCodeType}{measure.AdditionalCode}",
				ZX5_ZZF_NKTaxOrFeeCode = measure.VatCode,
				ZX5_StartDate = measure.CalcStartDate,
				ZX5_EndDate = measure.CalcEndDate
			};
		}

		static RefCusApplicability[] AddRefCusApplicability(RefCusApplicability[] refCusApplicabilities, RefCusApplicability newRefCusApplicability)
		{
			if (refCusApplicabilities == null)
			{
				refCusApplicabilities = new RefCusApplicability[] { };
			}
			var exclKey = GetExcludedAreasKey(newRefCusApplicability);
			var existing = refCusApplicabilities.FirstOrDefault(x => x.ZZT_AdditionalCode == newRefCusApplicability.ZZT_AdditionalCode
																	&& x.ZZT_OrderNumber == newRefCusApplicability.ZZT_OrderNumber
																	&& x.ZZT_ZZA_NKTradeGroup == newRefCusApplicability.ZZT_ZZA_NKTradeGroup
																	&& x.ZZT_StartDate == newRefCusApplicability.ZZT_StartDate
																	&& x.ZZT_EndDate == newRefCusApplicability.ZZT_EndDate
																	&& GetExcludedAreasKey(x) == exclKey);
			if (existing == null)
			{
				Array.Resize(ref refCusApplicabilities, refCusApplicabilities.Length + 1);
				refCusApplicabilities[refCusApplicabilities.Length - 1] = newRefCusApplicability;
			}

			return refCusApplicabilities;
		}

		static string GetExcludedAreasKey(RefCusApplicability refCusApplicability)
		{
			return string.Join(",", (refCusApplicability.RefCusExcludedTradeGroups ?? new RefCusExcludedTradeGroup[] { })
						.Select(x => x.ZZC_ZZA_NKTradeGroup)
						.OrderBy(x => x));
		}

		static void ApplyAdditonalRefCusApplicability(RefCusTariff tariff, RefCusApplicability newRefCusApplicability)
		{
			if (tariff.RefCusRates?.Any() ?? false)
			{
				foreach (var rate in tariff.RefCusRates)
				{
					rate.RefCusApplicabilities = AddApplicabilityIfMatches(newRefCusApplicability, rate.RefCusApplicabilities, true);
				}
			}

			if (tariff.RefCusConditions?.Any() ?? false)
			{
				foreach (var condition in tariff.RefCusConditions)
				{
					condition.RefCusApplicabilities = AddApplicabilityIfMatches(newRefCusApplicability, condition.RefCusApplicabilities, false);
				}
			}
		}

		static RefCusApplicability[] AddApplicabilityIfMatches(RefCusApplicability newRefCusApplicability, RefCusApplicability[] existingAppliacabilities, bool matchByDate)
		{
			var exclKey = GetExcludedAreasKey(newRefCusApplicability);
			var matches = existingAppliacabilities?.Where(x => x.ZZT_ZZA_NKTradeGroup == newRefCusApplicability.ZZT_ZZA_NKTradeGroup
															&& (!matchByDate || (x.ZZT_StartDate == newRefCusApplicability.ZZT_StartDate
																			  && x.ZZT_EndDate == newRefCusApplicability.ZZT_EndDate))
															&& GetExcludedAreasKey(x) == exclKey);
			if (matches?.Any() ?? false)
			{
				if (!matches.Any(x => x.ZZT_AdditionalCode == newRefCusApplicability.ZZT_AdditionalCode && x.ZZT_OrderNumber == newRefCusApplicability.ZZT_OrderNumber))
				{
					Array.Resize(ref existingAppliacabilities, existingAppliacabilities.Length + 1);
					existingAppliacabilities[existingAppliacabilities.Length - 1] = newRefCusApplicability;
				}
			}

			return existingAppliacabilities;
		}

		protected abstract string DataGrouping { get; }

		protected virtual string ParentDataGrouping => DataGrouping;

		protected virtual string DefaultTariffType => null;
	}
}
