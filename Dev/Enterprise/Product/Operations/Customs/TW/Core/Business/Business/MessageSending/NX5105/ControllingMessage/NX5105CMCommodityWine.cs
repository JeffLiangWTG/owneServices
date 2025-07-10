using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	class NX5105CMCommodityWine : NX5105CommodityWine
	{
		public NX5105CMCommodityWine(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
		}

		protected override ZInt AgeNumericCore => invoiceLine.JI_AlcoholAge;

		protected override ZDateTime BottledDateCore => invoiceLine.JI_BottledDate;

		protected override ZDecimal CoverLotNumberAmountCore => invoiceLine.JI_AlteredLotNoAmt;

		protected override ZString GeographicRegionCore => invoiceLine.JI_AlcoholCountryRegion;

		protected override ZDecimal OriginalNonLotNumberAmountCore => invoiceLine.JI_NoOriginalLotNoAmt;

		protected override ZDateTime ProductBestBeforeDateTimeCore => invoiceLine.JI_AlcoholEndOfShelfLife;

		protected override ZDateTime ProductExpiryDateTimeCore => invoiceLine.JI_ExpirationDate;

		protected override ZDecimal RemoveLotNumberAmountCore => invoiceLine.JI_RemovedLotNoAmt;

		protected override ZInt YearNumericCore => invoiceLine.JI_AlcoholYear;
	}
}
