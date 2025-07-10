using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.US.LVS.DataTransfer.Universal
{
	public class USLVConsolidatedDeclarationReader : US.DataTransfer.Universal.JobDeclarationDataObjectReader
	{
		public USLVConsolidatedDeclarationReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(declarationDataObject, logger, factory, null)
		{
		}

		protected override JobDeclaration GetExistingBusinessObjectUsingModuleSpecificBusinessRules() => null;

		protected override CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return new USLVCommercialInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, groupHeader, dataObject, landedCostDataReader);
		}
	}
}
