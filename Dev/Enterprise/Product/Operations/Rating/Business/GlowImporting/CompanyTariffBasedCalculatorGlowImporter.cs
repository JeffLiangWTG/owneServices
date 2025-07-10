using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	internal class CompanyTariffBasedCalculatorGlowImporter : RateLineItemGlowImporter<CompanyTariffOrCostBasedCalculator, RatingCalculatorColumns.CompanyTariffBased>
	{
		protected override bool ImportCore(
			CompanyTariffOrCostBasedCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.CompanyTariffBased, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			var success = ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,

				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CompanyTariffBased>(RateLineItemsSchema.TM_Type, RatingCalculatorColumns.CompanyTariffBased.Operator),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CompanyTariffBased>(RateLineItemsSchema.TM_Break, RatingCalculatorColumns.CompanyTariffBased.Break),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CompanyTariffBased>(RateLineItemsSchema.TM_Value, RatingCalculatorColumns.CompanyTariffBased.PerUnitChange),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.CompanyTariffBased>(RateLineItemsSchema.TM_AgentDeclaredRate, RatingCalculatorColumns.CompanyTariffBased.AgentPerUnitChange),
				new RateLineItemImportDataPair<RatingCalculatorColumns.CompanyTariffBased>(RateLineItemsSchema.TM_CallForPricing, RatingCalculatorColumns.CompanyTariffBased.IsRestricted, RateLineItemsSchema.TM_Text, RatingCalculatorColumns.CompanyTariffBased.RestrictedReason)
			);

			if (success)
			{
				SetCalculatorProperty(directValues, calculator,
					(RatingCalculatorColumns.CompanyTariffBased.PercentageChange, nameof(calculator.Percent)),
					(RatingCalculatorColumns.CompanyTariffBased.BasePriceChange, nameof(calculator.BaseRate)),
					(RatingCalculatorColumns.CompanyTariffBased.UnitPercentageChange, nameof(calculator.PerUnitPercent)),
					(RatingCalculatorColumns.CompanyTariffBased.UnitPriceChange, nameof(calculator.PerUnit)),
					(RatingCalculatorColumns.CompanyTariffBased.MinimumChange, nameof(calculator.Minimum)),
					(RatingCalculatorColumns.CompanyTariffBased.Equipment, nameof(calculator.EquipmentType)),
					(RatingCalculatorColumns.CompanyTariffBased.CalculationOrder, nameof(calculator.CalculationOrder))
				);

				SetCalculatorPropertyForAgencyRates(directValues, calculator,
					(RatingCalculatorColumns.CompanyTariffBased.AgentPercentageChange, nameof(calculator.Percent)),
					(RatingCalculatorColumns.CompanyTariffBased.AgentBasePriceChange, nameof(calculator.BaseRate)),
					(RatingCalculatorColumns.CompanyTariffBased.AgentUnitPercentageChange, nameof(calculator.PerUnitPercent)),
					(RatingCalculatorColumns.CompanyTariffBased.AgentUnitPriceChange, nameof(calculator.PerUnit)),
					(RatingCalculatorColumns.CompanyTariffBased.AgentMinimumChange, nameof(calculator.Minimum))
				);
			}

			return success;
		}
	}
}
