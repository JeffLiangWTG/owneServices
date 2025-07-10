using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	internal class WarehousePackCalculatorGlowImporter : RateLineItemGlowImporter<WarehousePackCalculator, RatingCalculatorColumns.WarehousePack>
	{
		protected override bool ImportCore(
			WarehousePackCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.WarehousePack, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,

				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.WarehousePack>(RateLineItemsSchema.TM_Type, RatingCalculatorColumns.WarehousePack.Code),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.WarehousePack>(RateLineItemsSchema.TM_Value, RatingCalculatorColumns.WarehousePack.Rate),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.WarehousePack>(RateLineItemsSchema.TM_AgentDeclaredRate, RatingCalculatorColumns.WarehousePack.AgentRate)
			);
		}
	}
}
