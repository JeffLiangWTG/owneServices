using CargoWise.Types;

using Enterprise.Freight.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public interface IForwardingShipmentDocumentSupporterQueryProvider : ICommonShipmentDocumentSupporterQueryProvider
	{
		DebtorToSelectFromForPrinting[] GetDebtorsToPrint(DocumentShipment documentShipment);
		DocumentImportCargoLabel GetImportCargoLabelToPrint(DocumentImportCargoLabel documentImportCargoLabel);
		LetterOfIndemnityOptions GetLetterOfIndemnityOptions(DocumentShipment documentShipment);
		Transport GetTransportToPrint(DocumentShipment documentShipment);
		ZBool PrintAWBBarcodeLabel();
		ZBool PrintShiLianDan(ForwardingShipment shipment);
	}
}
