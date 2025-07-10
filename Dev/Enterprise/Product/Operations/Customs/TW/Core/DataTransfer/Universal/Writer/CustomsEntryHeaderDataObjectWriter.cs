using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.TW.DataTransfer.Universal
{
	public class CustomsEntryHeaderDataObjectWriter : Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectWriter
	{
		public CustomsEntryHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper)
		  : base(manager, helper)
		{
		}

		protected override Customs.DataTransfer.Universal.CustomsEntryNumberDataObjectWriter GetNewCustomsEntryNumberDataObjectWriter()
		{
			return new CustomsEntryNumberDataObjectWriter(writeManager, helper);
		}

		protected override bool ShouldPopulatePaymentInformationData => true;

		protected override void PopulateAddInfo(CusEntryHeader entryHeaderBO, EntryHeader entryHeaderData)
		{
			base.PopulateAddInfo(entryHeaderBO, entryHeaderData);
			if (entryHeaderBO is Business.CusEntryHeader entryHeader)
			{
				var collection = entryHeaderData.AddInfoCollection;
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.TotalEXPDisbursedAmountInInvoiceCurrency, entryHeader.CH_TotalEXPDisbursedAmountInInvoiceCurrency);
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.TotalIMPFOBAmountInInvoiceCurrency, entryHeader.CH_TotalIMPFOBAmountInInvoiceCurrency);
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.TotalInternationalFreightAmountInInvoiceCurrency, entryHeader.CH_TotalInternationalFreightAmountInInvoiceCurrency);
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.TotalInternationalInsuranceAmountInInvoiceCurrency, entryHeader.CH_TotalInternationalInsuranceAmountInInvoiceCurrency);
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.TotalAdditionsInInvoiceCurrency, entryHeader.CH_TotalAdditionsInInvoiceCurrency);
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.TotalDeductionsInInvoiceCurrency, entryHeader.CH_TotalDeductionsInInvoiceCurrency);
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.TotalCustomsValueInInvoiceCurrency, entryHeader.CH_TotalCustomsValueInInvoiceCurrency);
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.TotalCustomsValueInLocalCurrency, entryHeader.CH_TotalCustomsValueInLocalCurrency);
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.BusinessTaxBaseAmount, entryHeader.BusinessTaxBaseAmount);
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.TotalCashTaxAmount, entryHeader.TotalCashTaxAmount);
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.TotalNonCashTaxAmount, entryHeader.TotalNonCashTaxAmount);
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.ConfirmedBusinessTaxBaseAmount, entryHeader.CH_ConfirmedBusinessTaxBase);
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.ConfirmedTotalCashTaxAmount, entryHeader.CH_ConfirmedTotalDutyTaxFee);
				helper.Update(collection, Constants.AddInfoKeys.CusEntryHeader.ConfirmedTotalNonCashTaxAmount, entryHeader.CH_ConfirmedTotalDutyTaxFeeDeferred);
			}
		}
	}
}
