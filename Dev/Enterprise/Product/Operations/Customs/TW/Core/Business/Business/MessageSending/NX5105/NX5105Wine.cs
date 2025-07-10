using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105CommodityWine : IWine
	{
		public NX5105CommodityWine(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, "invoiceLine");
		}
		protected readonly JobComInvoiceLine invoiceLine;

		ZDecimal IWine.AlcoholContentNumeric => invoiceLine.JI_AlcoholPercentage;

		#region Not Applicable

		ZInt IWine.AgeNumeric => AgeNumericCore;
		protected virtual ZInt AgeNumericCore => ZInt.Zero;

		ZDateTime IWine.BottledDate => BottledDateCore;
		protected virtual ZDateTime BottledDateCore => ZDateTime.Empty;

		ZDecimal IWine.CoverLotNumberAmount => CoverLotNumberAmountCore;
		protected virtual ZDecimal CoverLotNumberAmountCore => ZDecimal.Zero;

		ZString IWine.GeographicRegion => GeographicRegionCore;
		protected virtual ZString GeographicRegionCore => ZString.Empty;

		ZDecimal IWine.OriginalNonLotNumberAmount => OriginalNonLotNumberAmountCore;
		protected virtual ZDecimal OriginalNonLotNumberAmountCore => ZDecimal.Zero;

		ZDateTime IWine.ProductBestBeforeDateTime => ProductBestBeforeDateTimeCore;
		protected virtual ZDateTime ProductBestBeforeDateTimeCore => ZDateTime.Empty;

		ZDateTime IWine.ProductExpiryDateTime => ProductExpiryDateTimeCore;
		protected virtual ZDateTime ProductExpiryDateTimeCore => ZDateTime.Empty;

		ZDecimal IWine.RemoveLotNumberAmount => RemoveLotNumberAmountCore;
		protected virtual ZDecimal RemoveLotNumberAmountCore => ZDecimal.Zero;

		ZInt IWine.YearNumeric => YearNumericCore;
		protected virtual ZInt YearNumericCore => ZInt.Zero;

		#endregion
	}
}
