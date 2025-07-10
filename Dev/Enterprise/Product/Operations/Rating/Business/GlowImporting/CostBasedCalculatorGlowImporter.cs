using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	internal class CostBasedCalculatorGlowImporter : RateLineItemGlowImporter<CompanyTariffOrCostBasedCalculator, RatingCalculatorColumns.CostBased>
	{
		protected override bool ImportCore(
			CompanyTariffOrCostBasedCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.CostBased, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			var success = ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,

				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CostBased>(RateLineItemsSchema.TM_Type, RatingCalculatorColumns.CostBased.Operator),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CostBased>(RateLineItemsSchema.TM_Break, RatingCalculatorColumns.CostBased.Break),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CostBased>(RateLineItemsSchema.TM_Value, RatingCalculatorColumns.CostBased.PerUnitChange),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CostBased>(RateLineItemsSchema.TM_AgentDeclaredRate, RatingCalculatorColumns.CostBased.AgentPerUnitChange),
				new RateLineItemImportDataPair<RatingCalculatorColumns.CostBased>(RateLineItemsSchema.TM_CallForPricing, RatingCalculatorColumns.CostBased.IsRestricted, RateLineItemsSchema.TM_Text, RatingCalculatorColumns.CostBased.RestrictedReason)
			);

			if (success)
			{
				SetCalculatorProperty(directValues, calculator,
					(RatingCalculatorColumns.CostBased.PercentageChange, nameof(calculator.Percent)),
					(RatingCalculatorColumns.CostBased.BasePriceChange, nameof(calculator.BaseRate)),
					(RatingCalculatorColumns.CostBased.UnitPercentageChange, nameof(calculator.PerUnitPercent)),
					(RatingCalculatorColumns.CostBased.UnitPriceChange, nameof(calculator.PerUnit)),
					(RatingCalculatorColumns.CostBased.MinimumChange, nameof(calculator.Minimum)),
					(RatingCalculatorColumns.CostBased.CalculationOrder, nameof(calculator.CalculationOrder)),
					(RatingCalculatorColumns.CostBased.Equipment, nameof(calculator.EquipmentType))
				);

				SetCalculatorPropertyForAgencyRates(directValues, calculator,
					(RatingCalculatorColumns.CostBased.AgentPercentageChange, nameof(calculator.Percent)),
					(RatingCalculatorColumns.CostBased.AgentBasePriceChange, nameof(calculator.BaseRate)),
					(RatingCalculatorColumns.CostBased.AgentUnitPercentageChange, nameof(calculator.PerUnitPercent)),
					(RatingCalculatorColumns.CostBased.AgentUnitPriceChange, nameof(calculator.PerUnit)),
					(RatingCalculatorColumns.CostBased.AgentMinimumChange, nameof(calculator.Minimum))
				);
			}

			return success;
		}
	}
}
