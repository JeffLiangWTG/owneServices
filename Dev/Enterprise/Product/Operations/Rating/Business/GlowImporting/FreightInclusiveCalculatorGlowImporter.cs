using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	class FreightInclusiveCalculatorGlowImporter : RateLineItemGlowImporter<FreightInclusiveCalculator, RatingCalculatorColumns.FreightInclusive>
	{
		protected override bool ImportCore(
			FreightInclusiveCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.FreightInclusive, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.FreightInclusive.Type, nameof(calculator.FreightCalcType))
			);

			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,
				new RateLineItemImportDataWithConstant(RateLineItemsSchema.TM_Type, FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType)
			);
		}
	}
}
