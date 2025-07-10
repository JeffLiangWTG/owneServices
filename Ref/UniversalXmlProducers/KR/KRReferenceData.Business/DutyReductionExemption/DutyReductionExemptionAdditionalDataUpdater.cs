using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.Xml;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;
namespace CargoWise.RefDbRepo.KRReferenceData.Business.DutyReductionExemption
{
	public class DutyReductionExemptionAdditionalDataUpdater : IAdditionalDataUpdater<RefCusTariff>
	{
		public DutyReductionExemptionAdditionalDataUpdater()
		{
		}

		const string rateFormulaDerivedFrom100 = "100";
		const string cellValueY = "Y";
		const string rateFormulaFormat = "VFD * {0}";

		public static bool IsDataRowValid(IRow row, EntityConfiguration configuration)
		{
			var result = false;
			var columnIndex = configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties.Where(y => y.Name == nameof(RefCusRate.ZZ2_RateFormulaDerivedFrom))).FirstOrDefault()?.ExcelColumn ?? -1;
			if (columnIndex != -1)
			{
				var cell = row.GetCell(columnIndex);
				cell?.SetCellType(CellType.String);
				var cellValue = cell == null ? 0 : int.Parse(cell.StringCellValue, CultureInfo.CurrentCulture);
				result = cellValue > 0;
			}
			return result;
		}

		public static void UpdateAdditionally(RefCusTariff dataEntity, IRow row, EntityConfiguration configuration)
		{
			var entityType = configuration.EntityTypeExcelColumnMapping.EntityTypes.FirstOrDefault(x => x.Name == nameof(RefCusTariffAttribute));
			List<RefCusTariffAttribute> tariffAttributes = new List<RefCusTariffAttribute>();
			if (dataEntity.RefCusRates.FirstOrDefault()?.ZZ2_RateFormulaDerivedFrom == rateFormulaDerivedFrom100)
			{
				tariffAttributes.Add(new RefCusTariffAttribute { ZZ3_Name = NonPersistentNames.IsDutyExempt, ZZ3_Value = cellValueY });
			}
			foreach (var property in entityType.Properties)
			{
				if (property.IsNonPersistent)
				{
					var cell = row.GetCell(property.ExcelColumn);
					cell?.SetCellType(CellType.String);
					var cellValue = cell?.StringCellValue ?? string.Empty;
					bool shouldAdd = false;
					switch (property.Name)
					{
						case NonPersistentNames.PostClearanceProcedure:
							shouldAdd = cellValue == cellValueY;
							break;
						case NonPersistentNames.AgricultureTaxAApplies:
							shouldAdd = cellValue == cellValueY;
							break;
					}
					if (shouldAdd)
					{
						tariffAttributes.Add(new RefCusTariffAttribute { ZZ3_Name = property.Name, ZZ3_Value = cellValueY });
					}
				}
			}
			dataEntity.RefCusTariffAttributes = tariffAttributes.Count > 0 ? tariffAttributes.ToArray() : null;

			if (dataEntity.RefCusRates == null)
			{
				return;
			}
			foreach (var rate in dataEntity.RefCusRates)
			{
				var reductionValue = 0.0m;
				if (decimal.TryParse(rate.ZZ2_RateFormulaDerivedFrom, out reductionValue))
				{
					rate.ZZ2_RateFormula = string.Format(CultureInfo.CurrentCulture, rateFormulaFormat, reductionValue / 100);
				}
			}
		}

		public static void UpdateRule(RefCusTariff dataEntity, Rule rule)
		{
			if (rule.Name == RuleID.FOR_REIMPORT_OF_PREVIOUSLY_EXPORTED_GOODS)
			{
				var tariffCode = dataEntity.ZZ1_TariffCode;
				var tariffAttributes = dataEntity.RefCusTariffAttributes == null ? new List<RefCusTariffAttribute>() : new List<RefCusTariffAttribute>(dataEntity.RefCusTariffAttributes);
				foreach (var ruleValue in rule.RuleValues)
				{
					if (tariffCode == ruleValue.Value)
					{
						tariffAttributes.Add(new RefCusTariffAttribute
						{
							ZZ3_Name = rule.Name,
							ZZ3_Value = cellValueY
						});
						break;
					}
				}
				dataEntity.RefCusTariffAttributes = tariffAttributes.Count > 0 ? tariffAttributes.ToArray() : null;
			}
		}

		static class NonPersistentNames
		{
			public const string IsDutyExempt = "IsDutyExempt";
			public const string PostClearanceProcedure = "PostClearanceProcedure";
			public const string AgricultureTaxAApplies = "AgricultureTaxAApplies";
		}

		bool IAdditionalDataUpdater<RefCusTariff>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusTariff>.UpdateAdditionally(RefCusTariff dataEntity, IRow row, EntityConfiguration configuration) => UpdateAdditionally(dataEntity, row, configuration);
		void IAdditionalDataUpdater<RefCusTariff>.UpdateRule(RefCusTariff dataEntity, Rule rule) => UpdateRule(dataEntity, rule);
		void IAdditionalDataUpdater<RefCusTariff>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }
	}
	class RuleID
	{
		public const string FOR_REIMPORT_OF_PREVIOUSLY_EXPORTED_GOODS = "For Re-Import of Previously Exported Goods";
	}
}
