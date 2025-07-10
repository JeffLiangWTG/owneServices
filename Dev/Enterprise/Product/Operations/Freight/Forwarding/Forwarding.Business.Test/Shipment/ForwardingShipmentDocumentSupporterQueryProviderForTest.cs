using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentDocumentSupporterQueryProviderForTest : CommonShipmentDocumentSupporterQueryProviderForTest, IForwardingShipmentDocumentSupporterQueryProvider
	{
		DebtorToSelectFromForPrinting[] IForwardingShipmentDocumentSupporterQueryProvider.GetDebtorsToPrint(DocumentShipment documentShipment)
		{
			return null;
		}

		public DocumentImportCargoLabel GetImportCargoLabelToPrint(DocumentImportCargoLabel documentImportCargoLabel)
		{
			return null;
		}

		LetterOfIndemnityOptions IForwardingShipmentDocumentSupporterQueryProvider.GetLetterOfIndemnityOptions(DocumentShipment documentShipment)
		{
			return null;
		}

		Transport IForwardingShipmentDocumentSupporterQueryProvider.GetTransportToPrint(DocumentShipment documentShipment)
		{
			return null;
		}

		ZBool IForwardingShipmentDocumentSupporterQueryProvider.PrintAWBBarcodeLabel()
		{
			return false;
		}

		ZBool IForwardingShipmentDocumentSupporterQueryProvider.PrintShiLianDan(ForwardingShipment shipment)
		{
			return true;
		}
	}
}
