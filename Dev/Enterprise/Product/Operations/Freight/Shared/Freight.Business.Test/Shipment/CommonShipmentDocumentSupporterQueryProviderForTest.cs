namespace Enterprise.Freight.Business.Testing
{
	public class CommonShipmentDocumentSupporterQueryProviderForTest : ICommonShipmentDocumentSupporterQueryProvider
	{
		DocumentShipment documentShipment;

		public void SetDocumentOptionsOverride(DocumentShipment documentShipment)
		{
			this.documentShipment = documentShipment;
		}

		bool ICommonShipmentDocumentSupporterQueryProvider.ConfirmBOLPrinting(CommonShipment shipment)
		{
			return true;
		}

		DocumentPickupDeliveryConfirm[] ICommonShipmentDocumentSupporterQueryProvider.GetConfirmsToPrint(CommonShipment shipment, DocumentPickupDeliveryConfirmOptions documentCartageLegOptions)
		{
			return null;
		}

		ContainersToPrintOptions ICommonShipmentDocumentSupporterQueryProvider.GetContainersToPrint(ContainerToSelectFromForPrintingCollection containersToSelectFrom, bool includeUnContainerised)
		{
			return null;
		}

		DocumentShipment ICommonShipmentDocumentSupporterQueryProvider.GetDocumentOptions(DocumentShipment documentShipment)
		{
			return this.documentShipment ?? documentShipment;
		}
	}
}
