using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Common.SafeDataClient
{
	public class NonPersistentBusinessObjectComparison : INonPersistentBusinessObjectComparison
	{
		public static DateTimeOffset MinStartDate => new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
		public static DateTimeOffset MaxEndDate => new DateTimeOffset(2079, 06, 06, 0, 0, 0, TimeSpan.Zero);

		public bool IsIdentical(INonPersistentBusinessObject nonPersistentObject, object persistentObject)
		{
			Argument.Argument.NotNull(nonPersistentObject, nameof(nonPersistentObject));
			Argument.Argument.NotNull(persistentObject, nameof(persistentObject));

			var key = (nonPersistentObject.GetType(), persistentObject.GetType());
			if (FunctionMap.ContainsKey(key))
			{
				return FunctionMap[key](nonPersistentObject, persistentObject);
			}
			throw new ArgumentException($"Not support type group: nonPersistentObject is {nonPersistentObject.GetType().Name}, persistentObject is {persistentObject.GetType().Name}");
		}

		static bool IsIdentical(RefCusRateApplicability rateApp, RefCusRate rate)
		{
			// We should compare uoms here as well but we can short-circut it if rateFormula is equal
			return (rateApp != null && rate != null)
				&& CompareHelper.IsEquals(rateApp.S01_ZZ1_Tariff, rate.ZZ2_ZZ1_Tariff)
				&& CompareHelper.IsEquals(rateApp.S01_ZZW_TariffNationalCode, rate.ZZ2_ZZW_TariffNationalCode)
				&& CompareHelper.IsEquals(rateApp.S01_ZY1_RateCode, rate.ZZ2_ZY1_RateCode)
				&& CompareHelper.IsEquals(rateApp.S01_RateFormula, rate.ZZ2_RateFormula)
				&& CompareHelper.IsEquals(rateApp.S01_ZZS_Preference, rate.ZZ2_ZZS_Preference)
				&& CompareHelper.IsEquals(rateApp.S01_SelectorFormula, rate.ZZ2_SelectorFormula)
				&& CompareHelper.IsEquals(rateApp.S01_ZZZ_NKDataGrouping, rate.ZZ2_ZZZ_NKDataGrouping)
				&& CompareHelper.IsEquals(rate.ZZ2_StartDate, MinStartDate)
				&& CompareHelper.IsEquals(rate.ZZ2_EndDate, MaxEndDate)
				&& CompareHelper.IsEquals(rateApp.S01_RateFormulaDerivedFrom, rate.ZZ2_RateFormulaDerivedFrom)
				&& CompareHelper.IsEquals(rateApp.S01_RX_NKCurrencyOverride, rate.ZZ2_RX_NKCurrencyOverride);
		}

		static bool IsIdentical(RefCusRateApplicability rateApp, RefCusApplicability app)
		{
			return (rateApp != null && app != null)
				&& CompareHelper.IsEquals(rateApp.S01_AdditionalCode, app.ZZT_AdditionalCode)
				&& CompareHelper.IsEquals(rateApp.S01_OrderNumber, app.ZZT_OrderNumber)
				&& CompareHelper.IsEquals(rateApp.S01_ZZA_TradeGroup, app.ZZT_ZZA_TradeGroup)
				&& CompareHelper.IsEquals(rateApp.S01_ZZA_SecondTradeGroup, app.ZZT_ZZA_SecondTradeGroup)
				&& CompareHelper.IsEquals(rateApp.S01_StartDate, app.ZZT_StartDate)
				&& CompareHelper.IsEquals(rateApp.S01_EndDate, app.ZZT_EndDate)
				&& CompareHelper.IsEquals(rateApp.S01_ZY2_AdditionalCode, app.ZZT_ZY2_AdditionalCode);
		}

		static bool IsIdentical(RefCusRateApplicabilityUOM rateAppUOM, RefCusRateUOM uom)
		{
			return (rateAppUOM != null && uom != null)
				&& CompareHelper.IsEquals(rateAppUOM.S02_UOM, uom.ZXG_UOM);
		}

		static bool IsIdentical(RefCusExcludedTradeGroupNew exNew, RefCusExcludedTradeGroup ex)
		{
			return (exNew != null && ex != null)
				&& CompareHelper.IsEquals(exNew.S03_ZZA_TradeGroup, ex.ZZC_ZZA_TradeGroup);
		}

		static bool IsIdentical(RefCusConditionApplicability condApp, RefCusCondition cond)
		{
			return (condApp != null && cond != null)
				&& CompareHelper.IsEquals(condApp.S07_ZZ1_Tariff, cond.ZX1_ZZ1_Tariff)
				&& CompareHelper.IsEquals(condApp.S07_AdditionalComment, cond.ZX1_AdditionalComment)
				&& CompareHelper.IsEquals(condApp.S07_Comment, cond.ZX1_Comment)
				&& CompareHelper.IsEquals(condApp.S07_IsImport, cond.ZX1_IsImport)
				&& CompareHelper.IsEquals(condApp.S07_IsExport, cond.ZX1_IsExport)
				&& CompareHelper.IsEquals(condApp.S07_ConditionValueTrueMeansStop, cond.ZX1_ConditionValueTrueMeansStop)
				&& CompareHelper.IsEquals(condApp.S07_LogicalANDWithinGroup, cond.ZX1_LogicalANDWithinGroup)
				&& CompareHelper.IsEquals(condApp.S07_Source, cond.ZX1_Source)
				&& CompareHelper.IsEquals(cond.ZX1_StartDate, MinStartDate)
				&& CompareHelper.IsEquals(cond.ZX1_EndDate, MaxEndDate)
				&& CompareHelper.IsEquals(condApp.S07_Severity, cond.ZX1_Severity)
				&& CompareHelper.IsEquals(condApp.S07_ZX2_ConditionType, cond.ZX1_ZX2_ConditionType)
				&& CompareHelper.IsEquals(condApp.S07_ZY7_NKConditionCode, cond.ZX1_ZY7_NKConditionCode)
				&& CompareHelper.IsEquals(condApp.S07_ZZ5_Nomenclature, cond.ZX1_ZZ5_Nomenclature)
				&& CompareHelper.IsEquals(condApp.S07_ZZS_Preference, cond.ZX1_ZZS_Preference)
				&& CompareHelper.IsEquals(condApp.S07_ZZZ_NKDataGrouping, cond.ZX1_ZZZ_NKDataGrouping)
				&& CompareHelper.IsEquals(condApp.RefCusConditionApplicabilityValues.Count, cond.RefCusConditionValues.Count)
				&& condApp.RefCusConditionApplicabilityValues
				.Zip(cond.RefCusConditionValues, (a, b) => IsIdentical(a, b)).All(result => result);
		}

		static bool IsIdentical(RefCusConditionApplicability condApp, RefCusApplicability app)
		{
			return (condApp != null && app != null)
				&& CompareHelper.IsEquals(condApp.S07_AdditionalCode, app.ZZT_AdditionalCode)
				&& CompareHelper.IsEquals(condApp.S07_OrderNumber, app.ZZT_OrderNumber)
				&& CompareHelper.IsEquals(condApp.S07_ZZA_TradeGroup, app.ZZT_ZZA_TradeGroup)
				&& CompareHelper.IsEquals(condApp.S07_ZZA_SecondTradeGroup, app.ZZT_ZZA_SecondTradeGroup)
				&& CompareHelper.IsEquals(condApp.S07_StartDate, app.ZZT_StartDate)
				&& CompareHelper.IsEquals(condApp.S07_EndDate, app.ZZT_EndDate)
				&& CompareHelper.IsEquals(condApp.S07_ZY2_AdditionalCode, app.ZZT_ZY2_AdditionalCode);
		}

		static bool IsIdentical(RefCusConditionApplicabilityValue condAppVal, RefCusConditionValue condVal)
		{
			return (condAppVal != null && condVal != null)
				&& CompareHelper.IsEquals(condAppVal.S08_Value, condVal.ZX3_Value)
				&& CompareHelper.IsEquals(condAppVal.S08_ZX4_ValueType, condVal.ZX3_ZX4_ValueType)
				&& CompareHelper.IsEquals(condAppVal.S08_LogicalORWithinGroup, condVal.ZX3_LogicalORWithinGroup);
		}

		static bool IsIdentical(RefCusConditionApplicabilityLanguage condAppLang, RefCusConditionLanguage condLang)
		{
			return (condAppLang != null && condLang != null)
				&& CompareHelper.IsEquals(condAppLang.S09_ZX6_NKLanguage, condLang.ZXJ_ZX6_NKLanguage);
		}

		static Dictionary<(Type, Type), Func<object, object, bool>> FunctionMap = new Dictionary<(Type, Type), Func<object, object, bool>>
		{
			{(typeof(RefCusRateApplicability),typeof(RefCusRate)), (x,y)=> IsIdentical((RefCusRateApplicability)x,(RefCusRate)y)},
			{(typeof(RefCusRateApplicability),typeof(RefCusApplicability)), (x,y)=> IsIdentical((RefCusRateApplicability)x,(RefCusApplicability)y)},
			{(typeof(RefCusRateApplicabilityUOM),typeof(RefCusRateUOM)), (x,y)=> IsIdentical((RefCusRateApplicabilityUOM)x,(RefCusRateUOM)y)},
			{(typeof(RefCusExcludedTradeGroupNew),typeof(RefCusExcludedTradeGroup)), (x,y)=> IsIdentical((RefCusExcludedTradeGroupNew)x,(RefCusExcludedTradeGroup)y)},
			{(typeof(RefCusConditionApplicability),typeof(RefCusCondition)), (x,y)=> IsIdentical((RefCusConditionApplicability)x,(RefCusCondition)y)},
			{(typeof(RefCusConditionApplicability),typeof(RefCusApplicability)), (x,y)=> IsIdentical((RefCusConditionApplicability)x,(RefCusApplicability)y)},
			{(typeof(RefCusConditionApplicabilityValue),typeof(RefCusConditionValue)), (x,y)=> IsIdentical((RefCusConditionApplicabilityValue)x,(RefCusConditionValue)y)},
			{(typeof(RefCusConditionApplicabilityLanguage),typeof(RefCusConditionLanguage)), (x,y)=> IsIdentical((RefCusConditionApplicabilityLanguage)x,(RefCusConditionLanguage)y)}
		};
	}
}
