using System.Collections.Generic;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.ZA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.DataTransfer.Universal
{
	public class JobDeclarationDataObjectReaderForWOT : JobDeclarationDataObjectReader
	{
		internal JobDeclarationDataObjectReaderForWOT(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment = null)
			: base(declarationDataObject, logger, factory, shipment)
		{
		}

		protected override void PopulateApplicationCode(Shipment dataObject, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			if (dataObject.MessagingApplicationCode?.Code?.ToString() == ApplicationCodeList.Codes.ZATransactionOrders)
			{
				SetValue(declaration, JobDeclarationSchema.JE_ApplicationCode, "BLT", delaySetters);
			}
			else
			{
				base.PopulateApplicationCode(dataObject, declaration, delaySetters);
			}
		}

		protected override CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return new CommercialInvoiceHeaderDataObjectReaderForWOT(invoiceData, logger, Helper, groupHeader, dataObject, landedCostDataReader);
		}
	}
}
