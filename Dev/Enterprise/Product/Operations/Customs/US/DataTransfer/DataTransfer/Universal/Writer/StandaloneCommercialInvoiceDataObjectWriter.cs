using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class StandaloneCommercialInvoiceDataObjectWriter : Customs.DataTransfer.Universal.StandaloneCommercialInvoiceDataObjectWriter
	{
		internal protected StandaloneCommercialInvoiceDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper CreateNewUniversalDataObjectWriterHelper(Customs.Business.BaseJobComInvoiceHeader invoiceBO)
		{
			return new UniversalDataObjectWriterHelper(invoiceBO.Factory);
		}

		protected override Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper helper)
		{
			return new CommercialInvoiceHeaderDataObjectWriter(writeManager, (UniversalDataObjectWriterHelper)helper);
		}
	}
}
