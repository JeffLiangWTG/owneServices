using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	class WarehouseCustomsLineDetailsProvider : WarehouseCustomsLineDetailsProviderWithEntryInstruction<JobDeclaration, JobComInvoiceHeader>
	{
		public WarehouseCustomsLineDetailsProvider(Shipment shipment)
			: base(shipment)
		{
		}

		protected override WarehouseCustomsLineDetailsWithEntryInstruction GetNewLineDetail(BusinessObjectFactory factory, CommercialInvoiceLine invoiceLine, WarehouseCustomsFallbackDetailWithEntryInstruction fallbackDetail)
		{
			return new WarehouseCustomsLineDetails(factory, invoiceLine, fallbackDetail, shipment);
		}

		protected override IEnumerable<string> GetInvoiceLineAddInfosApplicableForInwardWarehousing() => Factory.GetInvoiceLineAddInfosApplicableForInwardWarehousing();

		protected override ITableSchema GetDeclarationAddInfoSchema() => ZAJobDeclarationSchema.Instance;

		protected override ITableSchema GetInvoiceAddInfoSchema() => ZAJobComInvoiceLineSchema.Instance;
	}
}
