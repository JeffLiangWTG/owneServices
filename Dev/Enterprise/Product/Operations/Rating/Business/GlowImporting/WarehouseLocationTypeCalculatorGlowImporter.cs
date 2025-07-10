using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	internal class WarehouseLocationTypeCalculatorGlowImporter : RateLineItemGlowImporter<WarehouseLocationTypeCalculator, RatingCalculatorColumns.WarehouseLocationType>
	{
		protected override bool ImportCore(
			WarehouseLocationTypeCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.WarehouseLocationType, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,

				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.WarehouseLocationType>(RateLineItemsSchema.TM_Type, RatingCalculatorColumns.WarehouseLocationType.Code),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.WarehouseLocationType>(RateLineItemsSchema.TM_Value, RatingCalculatorColumns.WarehouseLocationType.Rate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.WarehouseLocationType>(RateLineItemsSchema.TM_AgentDeclaredRate, RatingCalculatorColumns.WarehouseLocationType.AgentRate)
			);
		}
	}
}
