using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business
{
	public static class CalculatorFactory
	{
		public static Calculator GetCalculator(IRateLine line)
		{
			Argument.NotNull(line, "line");

			if (CalculatorsInConstruction.ContainsKey(line.PK))
			{
				return CalculatorsInConstruction[line.PK];
			}

			try
			{
				var result = CreateCalculator(line);
				CalculatorsInConstruction.Add(line.PK, result);
				result.PerformPostConstructionActions();        //this one is recursive
				return result;
			}
			finally
			{
				CalculatorsInConstruction.Remove(line.PK);
			}
		}

		static Calculator CreateCalculator(IRateLine line)
		{
			try
			{
				lineUnderConstruction = line;
				return NewCalculator(line);
			}
			finally
			{
				lineUnderConstruction = null;
			}
		}

		static Calculator NewCalculator(IRateLine line)
		{
			switch (line.RateCalculatorType)
			{
				case CalculatorType.Agency:
					return new AgencyCalculator(line);
				case CalculatorType.Cartage:
					return new CartageCalculator(line);
				case CalculatorType.CartageZoneDistance:
					return new CartageZoneDistanceCalculator(line);
				case CalculatorType.CostBased:
					return new CompanyTariffOrCostBasedCalculator(line);
				case CalculatorType.Combined:
					return new CombinedCalculator(line);
				case CalculatorType.CombinedWithIncrement:
					return new CombinedBreaksWithIncrementCalculator(line);
				case CalculatorType.CompanyTariffBased:
					return new CompanyTariffOrCostBasedCalculator(line);
				case CalculatorType.DisbursementInterest:
					return new DisbursementInterestCalculator(line);
				case CalculatorType.Equalization:
					return new EqualizationCalculator(line);
				case CalculatorType.EquipmentHire:
					return new EquipmentHireCalculator(line);
				case CalculatorType.ExcludeCompanyTariffs:
					return new ExcludeCompanyTariffsCalculator(line);
				case CalculatorType.FirstPlusAdditional:
					return new FirstPlusAdditionalCalculator(line);
				case CalculatorType.Flat:
					return new FlatCalculator(line);
				case CalculatorType.FlatPlusPerUnit:
					return new FlatPlusPerUnitCalculator(line);
				case CalculatorType.FreightInclusive:
					return new FreightInclusiveCalculator(line);
				case CalculatorType.HighestCharge:
					return new HighestChargeCalculator(line);
				case CalculatorType.HighestRate:
					return new HighestRateCalculator(line);
				case CalculatorType.HousebillReleaseType:
					return new HousebillReleaseTypeCalculator(line);
				case CalculatorType.Minimum:
					return new MinimumCalculator(line);
				case CalculatorType.MinimumOrPerUnit:
					return new MinimumOrPerUnitCalculator(line);
				case CalculatorType.Note:
					return new NoteCalculator(line);
				case CalculatorType.PackageCount:
					return new PackageCountCalculator(line);
				case CalculatorType.Percentage:
					return new PercentageCalculator(line);
				case CalculatorType.PercentageBreaks:
					return new PercentageBreaksCalculator(line);
				case CalculatorType.ProfitShareRebate:
					return new ProfitShareRebateCalculator(line);
				case CalculatorType.SplitMonthBilling:
					return new SplitMonthBillingCalculator(line);
				case CalculatorType.Time:
					return new TimeCalculator(line);
				case CalculatorType.Unit:
					return new UnitCalculator(line);
				case CalculatorType.ValueRange:
					return new ValueRangeCalculator(line);
				case CalculatorType.WarehousePack:
					return new WarehousePackCalculator(line);
				case CalculatorType.WarehouseLocationType:
					return new WarehouseLocationTypeCalculator(line);
#if DEBUG
				case CalculatorType.ForTest_CalculatorPropertyAttribute:
					return new CalculatorToTestCalculatorPropertyAttribute(line);
				case CalculatorType.ForTest_CartageCalculatorWithOverride:
					return new CartageCalculatorWithOverride(line);
				case CalculatorType.ForTest_CalculationLog:
					return new CalculatorForCalculationLogTest(line);
				case CalculatorType.ForTest_ConcreteCalculator:
					return new CalculatorForTest(line);
#endif
				default:
					return new NullCalculator(line);
			}
		}

		public static void EnsureIsCreatedByFactory(IRateLine rateLine)
		{
			if (!Equals(rateLine, lineUnderConstruction))
			{
				throw new InvalidOperationException("Calculator has to be created using CalculatorFactory");
			}
		}

		public static Dictionary<ZGuid, Calculator> CalculatorsInConstruction
		{
			get { return calculatorsInConstruction ?? (calculatorsInConstruction = new Dictionary<ZGuid, Calculator>()); }
		}
		[ThreadStatic]
		static Dictionary<ZGuid, Calculator> calculatorsInConstruction;

		[ThreadStatic]
		static IRateLine lineUnderConstruction;
	}
}

