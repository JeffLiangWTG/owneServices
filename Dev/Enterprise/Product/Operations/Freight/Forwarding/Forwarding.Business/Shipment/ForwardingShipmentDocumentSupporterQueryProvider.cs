using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentDocumentSupporterQueryProvider : CommonShipmentDocumentSupporterQueryProvider, IForwardingShipmentDocumentSupporterQueryProvider
	{
		DebtorToSelectFromForPrinting[] IForwardingShipmentDocumentSupporterQueryProvider.GetDebtorsToPrint(DocumentShipment documentShipment)
		{
			return documentShipment != null ? documentShipment.DebtorsToPrint.Cast<DebtorToSelectFromForPrinting>().ToArray() : null;
		}

		public DocumentImportCargoLabel GetImportCargoLabelToPrint(DocumentImportCargoLabel documentImportCargoLabel)
		{
			return documentImportCargoLabel;
		}

		LetterOfIndemnityOptions IForwardingShipmentDocumentSupporterQueryProvider.GetLetterOfIndemnityOptions(DocumentShipment documentShipment)
		{
			return new LetterOfIndemnityOptions();
		}

		Transport IForwardingShipmentDocumentSupporterQueryProvider.GetTransportToPrint(DocumentShipment documentShipment)
		{
			return documentShipment != null && documentShipment.Shipment.TransportsIncludingRelated.Count > 0 ? documentShipment.Shipment.TransportsIncludingRelated[0] : null;
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
