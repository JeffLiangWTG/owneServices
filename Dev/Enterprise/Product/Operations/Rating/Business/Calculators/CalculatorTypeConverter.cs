using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Convert between enum and string code.
	/// </summary>
	public sealed class CalculatorTypeConverter
	{
		public static CalculatorType CodeToEnum(string code)
			=> Instance.GetType(code);

		public static string EnumToCode(CalculatorType calcType)
			=> Instance.GetCode(calcType);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly CalculatorTypeConverter Instance = new CalculatorTypeConverter();

		string GetCode(CalculatorType calcType)
			=> (int)calcType >= 0 && (int)calcType < allCodes.Length
				? allCodes[(int)calcType]
				: null;

		CalculatorType GetType(string code)
			=> code != null && codeToType.TryGetValue(code, out var calcType) ? calcType : CalculatorType.None;

		CalculatorTypeConverter()
		{
			var values = Enum.GetValues(typeof(CalculatorType));
			int maxValue = values.Cast<int>().Max();
			allCodes = new string[maxValue + 1];
			allCodes[(int)CalculatorType.Flat] = FlatCalculator.Code;
			allCodes[(int)CalculatorType.Agency] = AgencyCalculator.Code;
			allCodes[(int)CalculatorType.Cartage] = CartageCalculator.Code;
			allCodes[(int)CalculatorType.CartageZoneDistance] = CartageZoneDistanceCalculator.Code;
			allCodes[(int)CalculatorType.Combined] = CombinedCalculator.Code;
			allCodes[(int)CalculatorType.CombinedWithIncrement] = CombinedBreaksWithIncrementCalculator.Code;
			allCodes[(int)CalculatorType.CompanyTariffBased] = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			allCodes[(int)CalculatorType.CostBased] = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			allCodes[(int)CalculatorType.DisbursementInterest] = DisbursementInterestCalculator.Code;
			allCodes[(int)CalculatorType.Equalization] = EqualizationCalculator.Code;
			allCodes[(int)CalculatorType.EquipmentHire] = EquipmentHireCalculator.Code;
			allCodes[(int)CalculatorType.ExcludeCompanyTariffs] = ExcludeCompanyTariffsCalculator.Code;
			allCodes[(int)CalculatorType.FirstPlusAdditional] = FirstPlusAdditionalCalculator.Code;
			allCodes[(int)CalculatorType.FlatPlusPerUnit] = FlatPlusPerUnitCalculator.Code;
			allCodes[(int)CalculatorType.FreightInclusive] = FreightInclusiveCalculator.Code;
			allCodes[(int)CalculatorType.HighestCharge] = HighestChargeCalculator.Code;
			allCodes[(int)CalculatorType.HighestRate] = HighestRateCalculator.Code;
			allCodes[(int)CalculatorType.HousebillReleaseType] = HousebillReleaseTypeCalculator.Code;
			allCodes[(int)CalculatorType.Minimum] = MinimumCalculator.Code;
			allCodes[(int)CalculatorType.MinimumOrPerUnit] = MinimumOrPerUnitCalculator.Code;
			allCodes[(int)CalculatorType.Note] = NoteCalculator.Code;
			allCodes[(int)CalculatorType.PackageCount] = PackageCountCalculator.Code;
			allCodes[(int)CalculatorType.Percentage] = PercentageCalculator.Code;
			allCodes[(int)CalculatorType.PercentageBreaks] = PercentageBreaksCalculator.Code;
			allCodes[(int)CalculatorType.ProfitShareRebate] = ProfitShareRebateCalculator.Code;
			allCodes[(int)CalculatorType.SplitMonthBilling] = SplitMonthBillingCalculator.Code;
			allCodes[(int)CalculatorType.Time] = TimeCalculator.Code;
			allCodes[(int)CalculatorType.Unit] = UnitCalculator.Code;
			allCodes[(int)CalculatorType.ValueRange] = ValueRangeCalculator.Code;
			allCodes[(int)CalculatorType.WarehouseLocationType] = WarehouseLocationTypeCalculator.Code;
			allCodes[(int)CalculatorType.WarehousePack] = WarehousePackCalculator.Code;

#if DEBUG
			allCodes[(int)CalculatorType.ForTest_CalculatorPropertyAttribute] = CalculatorToTestCalculatorPropertyAttribute.Code;
			allCodes[(int)CalculatorType.ForTest_CartageCalculatorWithOverride] = CartageCalculatorWithOverride.Code;
			allCodes[(int)CalculatorType.ForTest_CalculationLog] = CalculatorForCalculationLogTest.Code;
			allCodes[(int)CalculatorType.ForTest_ConcreteCalculator] = CalculatorForTest.Code;
#endif

			codeToType = new Dictionary<string, CalculatorType>(allCodes.Length);
			for (int i = 0; i < allCodes.Length; ++i)
			{
				if (allCodes[i] != null)
				{
					codeToType.Add(allCodes[i], (CalculatorType)i);
				}
			}
		}

		readonly string[] allCodes;
		readonly Dictionary<string, CalculatorType> codeToType;
	}
}
