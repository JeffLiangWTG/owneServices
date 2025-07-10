namespace Enterprise.Freight.Business
{
	public interface ICommonShipmentDocumentSupporterQueryProvider
	{
		bool ConfirmBOLPrinting(CommonShipment shipment);
		DocumentPickupDeliveryConfirm[] GetConfirmsToPrint(CommonShipment shipment, DocumentPickupDeliveryConfirmOptions documentCartageLegOptions);
		ContainersToPrintOptions GetContainersToPrint(ContainerToSelectFromForPrintingCollection containersToSelectFrom, bool includeUnContainerised);
		DocumentShipment GetDocumentOptions(DocumentShipment documentShipment);
	}
}
