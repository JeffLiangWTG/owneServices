using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public class ShipmentDocumentSupporterGuiQueryProvider : SharedGuiQueryProvider, ICommonShipmentDocumentSupporterQueryProvider
	{
		public static void Register(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				factory.SetValue<ICommonShipmentDocumentSupporterQueryProvider, ShipmentDocumentSupporterGuiQueryProvider>();
			}
		}

		bool ICommonShipmentDocumentSupporterQueryProvider.ConfirmBOLPrinting(CommonShipment shipment)
		{
			return base.ConfirmBOLPrinting(shipment);
		}

		DocumentPickupDeliveryConfirm[] ICommonShipmentDocumentSupporterQueryProvider.GetConfirmsToPrint(CommonShipment shipment, DocumentPickupDeliveryConfirmOptions documentCartageLegOptions)
		{
			DocumentPickupDeliveryConfirm[] confirmsToPrint = null;

			if (documentCartageLegOptions != null)
			{
				if (documentCartageLegOptions.Confirms.Count == 0)
				{
					Globals.Message.ShowInformation(Res.GetString("9efa7f48-07e6-439d-bcad-087c631879c4", "There are no deliveries to print."), Res.GetString("d6fd59f0-511c-4327-9091-0b8a663a6a71", "No Deliveries"));
				}
				else if (documentCartageLegOptions.Confirms.Count == 1)
				{
					confirmsToPrint = new[] { documentCartageLegOptions.Confirms[0] };
				}
				else
				{
					IConfirmationsDocumentDialog dialog = ObjectFactory.Get<IConfirmationsDocumentDialog>();
					if (dialog.ShowDialogDisposeAndContinue(documentCartageLegOptions.Confirms, shipment))
					{
						confirmsToPrint = documentCartageLegOptions.Confirms.Cast<DocumentPickupDeliveryConfirm>().Where(confirm => confirm.PrintConfirm).ToArray();
					}
				}
			}

			return confirmsToPrint;
		}

		ContainersToPrintOptions ICommonShipmentDocumentSupporterQueryProvider.GetContainersToPrint(ContainerToSelectFromForPrintingCollection containersToSelectFrom, bool includeUnContainerised)
		{
			return base.GetContainersToPrint(containersToSelectFrom, includeUnContainerised);
		}

		DocumentShipment ICommonShipmentDocumentSupporterQueryProvider.GetDocumentOptions(DocumentShipment documentShipment)
		{
			if (documentShipment.DataContext == Core.Constants.DataContext.GenericFreightJobBySelectedPackages ||
				documentShipment.DataContext == Core.Constants.DataContext.GenericFreightJobBySelectedPkgs1Doc)
			{
				return ZFormModaliser.ShowDialogAndDispose(new DocumentShipmentWithUniqueIDForm(documentShipment)) ==
					   DialogResult.Yes
					? documentShipment
					: null;
			}

			return ZFormModaliser.ShowDialogAndDispose(new DocumentShipmentForm(documentShipment)) == DialogResult.Yes ? documentShipment : null;
		}
	}
}
