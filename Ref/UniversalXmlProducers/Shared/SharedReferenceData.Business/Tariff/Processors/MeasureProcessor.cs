using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Models;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors
{
	public class MeasureProcessor : ProcessorBase<Measure>
	{
		public MeasureProcessor(IDateTimeProvider dateTimeProvider, IMeasureMappingProvider measureMappingProvider, IRefXmlBuilder[] builders) : base(dateTimeProvider, builders, new MeasureLoader())
		{
			this.measureMappingProvider = measureMappingProvider;
			measureTypeHelper = new MeasureTypeHelper(measureMappingProvider);
		}
		readonly IMeasureMappingProvider measureMappingProvider;
		readonly MeasureTypeHelper measureTypeHelper;

		bool IsDateExpired(DateTime date) => date < DateTimeProvider.UTCHistoricalDate;

		internal MeasureHelper MeasureHelper => measureHelper ?? (measureHelper = GetNewMeasureHelper());
		MeasureHelper measureHelper;
		protected virtual MeasureHelper GetNewMeasureHelper() => new MeasureHelper();

		protected virtual bool ProcessConfiguredMeasureTypesOnly => false;

		protected override void UpdateModelsCore(string chapterFilter, List<ITariffModel> referenceData, StringBuilder errorCollector)
		{
			var regulationHelper = new RegulationHelper(referenceData, DateTimeProvider, errorCollector);
			var nomenclatureList = referenceData.OfType<GoodsNomenclature>();
			var measureTypeList = referenceData.OfType<MeasureType>();
			var conditionCodeList = referenceData.OfType<MeasureConditionCode>();

			var measureModels = Models.Cast<Measure>().ToList();

			ModelTracking("Measures Loaded", measureModels);

			var models = measureModels
				.Where(x =>
					!IsDateExpired(x.CalcEndDate)
					&& measureTypeHelper.ShouldProcessMeasureType(x.MeasureType, ProcessConfiguredMeasureTypesOnly)
					&& regulationHelper.IsRegulationActive(RegulationHelper.GetUniqueKey(x)))
				.ToList();

			ModelTracking("Measures Filtered", models);

			foreach (var model in models)
			{
				model.CleanId = CommonHelper.CleanCommodityCode(model.ItemId);
				model.ConditionClass = measureTypeHelper.GetConditionClass(model.MeasureType);

				UpdateMeasureTypeInfo(model, measureTypeList);

				if (model.Conditions != null && model.Conditions.Any())
				{
					foreach (var grp in model.Conditions.GroupBy(x => x.ConditionCode))
					{
						UpdateConditions(model, grp.ToList(), grp.Key, conditionCodeList);
					}
				}

				model.Formula = MeasureHelper.GenerateFormula(model, errorCollector);
			}

			ModelTracking("Measures Setup", models);

			models = BuildFromNomenclature(models, nomenclatureList);

			ModelTracking("Measures Inheritted", models);

			AddAuthorisedUsePreferences(models);

			Models = models.Cast<ITariffModel>().ToList();
		}

		protected virtual void ModelTracking(string action, List<Measure> models) { } // Used to assist logging/debugging 

		void UpdateMeasureTypeInfo(Measure model, IEnumerable<MeasureType> measureTypes)
		{
			var mt = measureTypes.FirstOrDefault(x => x.Id == model.MeasureType);
			model.MeasureTypeDescription = mt?.Description ?? string.Empty;
			model.TariffTypes = MeasureHelper.GetTariffTypes(mt?.TradeMovementCode ?? string.Empty);
			model.MeasureTypeSeries = mt?.MeasureTypeSeries ?? string.Empty;
			model.RateType = measureTypeHelper.GetRateType(model.MeasureType);
			model.RateCode = measureMappingProvider.ConvertRateCode(measureTypeHelper.GetRateCode(model.MeasureType), model);
			model.Preferences = measureMappingProvider.ConvertPreferences(measureTypeHelper.GetPreferences(model.MeasureType), model.GeographicalArea, model.Footnotes);
			model.VatCode = measureMappingProvider.GetVatCode(model);
			model.IsSupplementaryUnit = measureTypeHelper.IsSupplementaryUnit(model.MeasureType);
		}

		static void UpdateConditions(Measure measure, List<MeasureCondition> measureConditions, string conditionGroup, IEnumerable<MeasureConditionCode> conditionCodes)
		{
			bool isRateFormula = MeasureHelper.IsRateFormula(measureConditions, conditionGroup);
			bool isCertificate = MeasureHelper.IsCertificateCondition(conditionGroup);

			var cc = conditionCodes.FirstOrDefault(x => x.Id == conditionGroup);
			var description = cc?.Description ?? string.Empty;

			measureConditions.ForEach(condition =>
			{
				condition.ConditionCodeDescription = description;
				condition.IsRateFormulaCondition = isRateFormula;
				condition.IsCertificate = isCertificate;
				condition.Formula = MeasureHelper.GenerateConditionFormula(measure, condition);
			});
		}

		static string GetLatestDescription(string itemId, IEnumerable<GoodsNomenclature> nomenclatureList)
		{
			var nom = nomenclatureList.Where(x => x.ItemId == itemId && x.ProductLineSuffix == "80")
						.OrderByDescending(x => x.CalcStartDate)
						.ThenByDescending(x => x.CalcEndDate)
						.FirstOrDefault();

			return nom?.Description ?? string.Empty;
		}

		List<Measure> BuildFromNomenclature(IEnumerable<Measure> measureList, IEnumerable<GoodsNomenclature> nomenclatureList)
		{
			var models = new List<Measure>();
			var dateTimeProvider = DateTimeProvider;

			foreach (var child in nomenclatureList.Where(x => x.IsForMeasure ?? false))
			{
				var latestDescription = GetLatestDescription(child.ItemId, nomenclatureList);
				var childMeasures = measureList.Where(x => x.ItemId == child.ItemId).OrderBy(x => x.CalcStartDate).ToList();

				childMeasures.ForEach(x =>
				{
					x.Description = latestDescription;
					x.ExportDescription = latestDescription;
					x.CompositeKey = child.Key;
					x.NomenclatureStartDate = child.CalcStartDate;
					x.NomenclatureEndDate = child.CalcEndDate;
				});

				AddParentMeasures(child, ref childMeasures, measureList, nomenclatureList, child.ParentId, latestDescription);
				RemoveDateOverlapping(childMeasures);
				ConvertSupplementaryUnits(childMeasures);

				models.AddRange(childMeasures);
			}

			return models;
		}

		static void AddParentMeasures(GoodsNomenclature child, ref List<Measure> childMeasures, IEnumerable<Measure> measureList, IEnumerable<GoodsNomenclature> nomenclatureList, int? parentId, string latestDescription)
		{
			var parent = parentId ?? 0;

			if (parent > 0)
			{
				var parentNomenclature = nomenclatureList.FirstOrDefault(x => x.Id == parent);
				if (parentNomenclature != null && !string.IsNullOrEmpty(parentNomenclature.ItemId))
				{
					var parentMeasures = measureList.Where(x => x.ItemId == parentNomenclature.ItemId && x.Suffix == parentNomenclature.ProductLineSuffix)
											.OrderByDescending(x => x.CalcStartDate)
											.ThenByDescending(x => x.CalcEndDate)
											.ThenByDescending(x => x.HJID);

					foreach (var pm in parentMeasures)
					{
						var startDate = pm.CalcStartDate;
						var endDate = pm.CalcEndDate;

						if (startDate < endDate &&
							!childMeasures.Any(x => x.MeasureType == pm.MeasureType
											&& x.GeographicalArea == pm.GeographicalArea
											&& x.AdditionalCode == pm.AdditionalCode
											&& x.AdditionalCodeType == pm.AdditionalCodeType
											&& x.OrderNumber == pm.OrderNumber
											&& x.CalcStartDate <= endDate
											&& x.CalcEndDate >= startDate))
						{
							var newMeasure = pm.Copy();
							newMeasure.ItemId = child.ItemId;
							newMeasure.CleanId = child.CleanId;
							newMeasure.ExportItemId = child.ItemId;
							newMeasure.CompositeKey = child.Key;
							newMeasure.Description = latestDescription;
							newMeasure.ExportDescription = latestDescription;

							newMeasure.StartDate = startDate;
							newMeasure.EndDate = endDate;
							newMeasure.NomenclatureStartDate = child.CalcStartDate;
							newMeasure.NomenclatureEndDate = child.CalcEndDate;

							childMeasures.Add(newMeasure);
						}
					}

					AddParentMeasures(child, ref childMeasures, measureList, nomenclatureList, parentNomenclature.ParentId, latestDescription);
				}
			}
		}

		static void RemoveDateOverlapping(List<Measure> childMeasures)
		{
			foreach (var measureTypeGeoArea in childMeasures.GroupBy(x => new { x.MeasureType, x.GeographicalArea, x.OrderNumber, x.AdditionalCode, x.AdditionalCodeType }))
			{
				var measures = measureTypeGeoArea
								.OrderByDescending(x => x.CalcStartDate)
								.ThenByDescending(x => x.CalcEndDate)
								.ToList();

				for (int i = measures.Count - 1; i > 0; i--)
				{
					if (measures[i].CalcEndDate == measures[i - 1].CalcEndDate && measures[i].CalcStartDate < measures[i - 1].CalcStartDate)
					{
						measures[i].EndDate = measures[i - 1].CalcStartDate.AddSeconds(-1);
					}
				}
			}
		}

		static void ConvertSupplementaryUnits(List<Measure> childMeasures)
		{
			var supMeasure = childMeasures.FirstOrDefault(x => x.IsSupplementaryUnit);
			var supComponent = supMeasure?.Components?.FirstOrDefault(x => x.DutyExpression == "99");
			var supUnit = supComponent == null ? MeasureHelper.DefaultSupplementaryUnit : $"{supComponent.MeasurementUnit}{supComponent.MeasurementUnitQualifier}";

			childMeasures.ForEach((m) =>
			{
				m.Formula = m.Formula.Replace(MeasureHelper.SupplementaryUnitPlaceHolder, supUnit);

				if (m.Conditions?.Any() ?? false)
				{
					foreach (var c in m.Conditions)
					{
						c.Formula = c.Formula.Replace(MeasureHelper.SupplementaryUnitPlaceHolder, supUnit);
					}
				}
			});
		}

		void AddAuthorisedUsePreferences(List<Measure> models)
		{
			const string measureTypeForAuthorisedUse = "464";

			var tariffsForAuth = models.Where(x => x.MeasureType == measureTypeForAuthorisedUse).Select(x => new { x.ItemId, x.GeographicalArea }).Distinct().ToList();

			foreach (var measure in models.Where(x => x.MeasureType != measureTypeForAuthorisedUse && tariffsForAuth.Any(t => t.ItemId == x.ItemId && t.GeographicalArea == x.GeographicalArea)))
			{
				var authPrefs = measureTypeHelper.GetAuthorisedUsePreferences(measure.MeasureType);
				if (authPrefs.Any())
				{
					var existingPrefs = measure.Preferences.ToList();
					var added = false;

					foreach (var ap in authPrefs)
					{
						if (!existingPrefs.Contains(ap))
						{
							existingPrefs.Add(ap);
							added = true;
						}
					}

					if (added)
					{
						measure.Preferences = existingPrefs;
					}
				}
			}
		}
	}
}
