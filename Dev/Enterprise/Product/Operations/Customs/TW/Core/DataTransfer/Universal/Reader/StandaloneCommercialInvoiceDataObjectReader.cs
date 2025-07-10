using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.TW.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.TW.DataTransfer.Universal
{
	public class StandaloneCommercialInvoiceDataObjectReader : StandaloneCommercialInvoiceDataObjectReader<JobComInvoiceHeader, JobComInvoiceGroupHeader>
	{
		public StandaloneCommercialInvoiceDataObjectReader(Shipment shipmentDataObject, CommercialInvoiceHeader invoiceHeaderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(shipmentDataObject, invoiceHeaderDataObject, logger, factory)
		{
		}

		protected new UniversalDataObjectReaderHelper Helper => (TWDataObjectReaderHelper)base.Helper;

		protected override CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData)
		{
			return new TWInvoiceHeaderDataObjectReader(groupHeader, invoiceData, logger, Helper);
		}

		protected override UniversalDataObjectReaderHelper CreateNewUniversalDataObjectReaderHelper()
		{
			return new TWDataObjectReaderHelper(factory, dataObject.GetTargetCountryCode());
		}
	}
}
