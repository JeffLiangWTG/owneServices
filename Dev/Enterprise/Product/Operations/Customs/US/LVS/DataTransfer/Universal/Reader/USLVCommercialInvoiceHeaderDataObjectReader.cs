using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.LVS.DataTransfer.Universal
{
	public class USLVCommercialInvoiceHeaderDataObjectReader : CommercialInvoiceHeaderDataObjectReader
	{ 
		protected internal USLVCommercialInvoiceHeaderDataObjectReader(CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, JobComInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null)
			: base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader)
		{
		}

		protected override bool ShouldFireDataImportedToBusinessObject => true;
	}
}
