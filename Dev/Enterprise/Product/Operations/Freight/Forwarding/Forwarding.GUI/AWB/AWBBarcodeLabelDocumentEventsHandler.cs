using System.Linq;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public class AWBBarcodeLabelDocumentEventsHandler : IDocumentEventsHandler
	{
		#region IDocumentEventsHandler Members

		public DocumentSupporter DocumentSupporter { get; set; }

		public bool CanHandleMenuItem(IStmMenuItem menuItem)
		{
			return menuItem.SU_MenuName == AWBActions.DocumentNames.AWBBarcodeLabels;
		}

		public void HandleDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			ForwardingShipmentDocumentSupporter forwardingShipmentDocumentSupporter = DocumentSupporter as ForwardingShipmentDocumentSupporter;

			if (forwardingShipmentDocumentSupporter != null)
			{
				e.Cancel = true;

				if (Globals.CanShowDialogs)
				{
					var shipment = forwardingShipmentDocumentSupporter.Shipment;

					if (shipment.JS_OuterPacks == 0)
					{
						string message = Res.GetString("612d565b-9a5d-4025-be90-aa4fe7829684", "No labels will be printed.\r\nLabels will only be printed if this Shipment has Outer Packs.");
						Globals.Message.ShowInformation(message, Res.GetString("ec15db00-ae5c-4e3c-91a4-afe9e495ea97", "AWB Barcode Label Printing"));
					}
					else if (!shipment.Consols.Cast<ForwardingConsol>().ToList().Exists(consol => consol.IsAir))
					{
						string message = Res.GetString("a09014ad-ca8b-4928-87a9-6b0150d06aac", "No labels will be printed.\r\nLabels will only be printed if this Shipment has departure Consolidation attached with 'Air' transport mode.");
						Globals.Message.ShowInformation(message, Res.GetString("ec15db00-ae5c-4e3c-91a4-afe9e495ea97", "AWB Barcode Label Printing"));
					}
					else
					{
						ShipmentAWBActions actions = new ShipmentAWBActions(shipment, AWBActions.ActionsModeType.LabelsOnly);
						ZFormModaliser.ShowDialogAndDispose(new ShipmentLabelRangeForm(actions));
					}
				}
			}
		}

		public void HandleDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
		{
		}

		public void HandleDocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
		{
		}

		public void HandleDocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
		}

		#endregion
	}
}
