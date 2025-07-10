using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.TW.DataTransfer.Universal
{
	public class StandaloneCommercialInvoiceDataObjectWriter : Customs.DataTransfer.Universal.StandaloneCommercialInvoiceDataObjectWriter
	{
		protected internal StandaloneCommercialInvoiceDataObjectWriter(IDataWritingManager manager) : base(manager)
		{
		}

		protected override CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(UniversalDataObjectWriterHelper helper)
		{
			return new TWInvoiceHeaderDataObjectWriter(writeManager, helper);
		}
	}
}
