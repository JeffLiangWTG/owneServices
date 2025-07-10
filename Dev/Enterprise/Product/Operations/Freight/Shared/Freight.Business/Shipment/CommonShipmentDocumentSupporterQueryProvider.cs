using System.Linq;
using Enterprise.Environment;

namespace Enterprise.Freight.Business
{
	public class CommonShipmentDocumentSupporterQueryProvider : ICommonShipmentDocumentSupporterQueryProvider
	{
		bool ICommonShipmentDocumentSupporterQueryProvider.ConfirmBOLPrinting(CommonShipment shipment)
		{
			return Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed;
		}

		DocumentPickupDeliveryConfirm[] ICommonShipmentDocumentSupporterQueryProvider.GetConfirmsToPrint(CommonShipment shipment, DocumentPickupDeliveryConfirmOptions documentCartageLegOptions)
		{
			return documentCartageLegOptions.Confirms.Cast<DocumentPickupDeliveryConfirm>().ToArray();
		}

		ContainersToPrintOptions ICommonShipmentDocumentSupporterQueryProvider.GetContainersToPrint(ContainerToSelectFromForPrintingCollection containersToSelectFrom, bool includeUnContainerised)
		{
			return new ContainersToPrintOptions { ContainersToPrint = containersToSelectFrom.Cast<ContainerToSelectFromForPrinting>().Select(c => c.Container).ToArray(), IncludeUnContainerised = includeUnContainerised };
		}

		DocumentShipment ICommonShipmentDocumentSupporterQueryProvider.GetDocumentOptions(DocumentShipment documentShipment)
		{
			return documentShipment;
		}
	}
}
