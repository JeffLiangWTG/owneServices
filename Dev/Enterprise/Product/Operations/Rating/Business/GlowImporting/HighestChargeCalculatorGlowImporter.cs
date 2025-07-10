using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	class HighestChargeCalculatorGlowImporter : RateLineItemGlowImporter<HighestChargeCalculator, RatingCalculatorColumns.HighestCharge>
	{
		protected override bool ImportCore(
			HighestChargeCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.HighestCharge, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			// The mapping for RatingCalculatorColumns.HighestCharge.MustMapToSomethingAnythingWillDo
			// is ignored. It will show up in the ADAW GLOW mapping page but during
			// importing it is discarded. It exists only as a dummy value.

			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,
				new RateLineItemImportDataWithConstant(RateLineItemsSchema.TM_Type, CalculatorConstants.Type.ApplyTo),
				new RateLineItemImportDataWithConstant(RateLineItemsSchema.TM_Text, CalculatorConstants.Text.ApplyTo.Charges.ChargeCode)
			);
		}
	}
}
