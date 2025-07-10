using System.Collections.Generic;
using CargoWise.DataTransfer.Ratings;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	class NoteCalculatorGlowImporter : RateLineItemGlowImporter<NoteCalculator, RatingCalculatorColumns.Note>
	{
		protected override bool ImportCore(
			NoteCalculator calculator,
			ValueObjectImportContext context,
			Dictionary<RatingCalculatorColumns.Note, string> directValues,
			Dictionary<string, string> relationshipValues = null)
		{
			SetCalculatorProperty(directValues, calculator,
				(RatingCalculatorColumns.Note.ShowOnBillingWithoutPrefix, nameof(calculator.ShowOnBillingWithoutPrefix))
			);

			return ImportRateLineItem(
				calculator.RateLineBizO,
				context,
				directValues,
				relationshipValues,

				new RateLineItemImportDataWithConstant(RateLineItemsSchema.TM_Type, string.Empty),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Note>(RateLineItemsSchema.TM_Value, RatingCalculatorColumns.Note.Amount),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Note>(RateLineItemsSchema.TM_Text, RatingCalculatorColumns.Note.ItemDescription),
				new RateLineItemImportDataWithColumn<RatingCalculatorColumns.Note>(RateLineItemsSchema.TM_AgentDeclaredRate, RatingCalculatorColumns.Note.AgentAmount)
			);
		}
	}
}
