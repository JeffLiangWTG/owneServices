using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.SG.V4.DataTransfer.Universal
{
	public class StandaloneCommercialInvoiceDataObjectReader : Customs.DataTransfer.Universal.StandaloneCommercialInvoiceDataObjectReader
	{
		public StandaloneCommercialInvoiceDataObjectReader(UniversalShipment shipmentDataObject, CommercialInvoiceHeader invoiceHeaderDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(shipmentDataObject, invoiceHeaderDataObject, logger, factory)
		{
		}

		protected override CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(BaseJobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData)
		{
			return new CommercialInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, groupHeader);
		}
	}
}
