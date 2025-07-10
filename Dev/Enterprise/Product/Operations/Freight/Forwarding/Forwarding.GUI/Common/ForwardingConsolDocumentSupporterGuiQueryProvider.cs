using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.GUI.AWB;
using Enterprise.Freight.GUI;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public class ForwardingConsolDocumentSupporterGuiQueryProvider : ConsolDocumentSupporterGuiQueryProvider, IForwardingConsolDocumentSupporterQueryProvider
	{
		public static new void Register(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				ConsolDocumentSupporterGuiQueryProvider.Register(factory);
				factory.SetValue<IForwardingConsolDocumentSupporterQueryProvider, ForwardingConsolDocumentSupporterGuiQueryProvider>();
			}
		}

		DeliveryAgentOrgHeader[] IForwardingConsolDocumentSupporterQueryProvider.GetDeliveryAgentsToPrint(DeliveryAgentToSelectFromForPrintingCollection deliveryAgentsToSelectFrom)
		{
			DeliveryAgentOrgHeader[] deliveryAgentsToPrint = null;

			if (deliveryAgentsToSelectFrom != null)
			{
				if (deliveryAgentsToSelectFrom.Count == 0)
				{
					Globals.Message.ShowInformation(Res.GetString("ad636f5f-fabb-4b96-9348-bc20234548b0", "There are no Delivery Agents for this Consol."), Res.GetString("fd2b08d0-b4a4-490e-86cf-285e65af35f1", "No Delivery Agents"));
				}
				else if (deliveryAgentsToSelectFrom.Count == 1)
				{
					DeliveryAgentOrgHeader deliveryAgent = deliveryAgentsToSelectFrom.Factory.Load<DeliveryAgentOrgHeader>(deliveryAgentsToSelectFrom[0].PK);
					deliveryAgentsToPrint = new[] { deliveryAgent };
				}
				else
				{
					DocumentDeliveryAgents docDeliveryAgentsBizObject = new DocumentDeliveryAgents(deliveryAgentsToSelectFrom);
					if (ZFormModaliser.ShowDialogAndDispose(new DocumentDeliveryAgentsForm(docDeliveryAgentsBizObject)) == DialogResult.Yes)
					{
						deliveryAgentsToPrint = docDeliveryAgentsBizObject.DeliveryAgentsToPrint.ToArray<DeliveryAgentOrgHeader>();
					}
				}
			}

			return deliveryAgentsToPrint;
		}

		DocumentImportCargoLabel IForwardingConsolDocumentSupporterQueryProvider.GetImportCargoLabelToPrint(DocumentImportCargoLabel documentImportCargoLabel)
		{
			return documentImportCargoLabel != null &&
				ZFormModaliser.ShowDialogAndDispose(new DocumentImportCargoForm(documentImportCargoLabel)) == DialogResult.Yes ? documentImportCargoLabel : null;
		}

		void IForwardingConsolDocumentSupporterQueryProvider.PrintAWBLabels(ForwardingConsol consol)
		{
			if (consol != null)
			{
				AWBActions actions = new ConsolAWBActions(consol, AWBActions.ActionsModeType.LabelsOnly);
				ZFormModaliser.ShowDialogAndDispose(new LabelRangeForm(actions));
			}
		}

		void IForwardingConsolDocumentSupporterQueryProvider.PrintFinalMaster(ForwardingConsol consol, string menuPath)
		{
			if (consol != null)
			{
				ConsolAWBActions actions = new ConsolAWBActions(consol, AWBActions.ActionsModeType.All, menuPath);
				if (ZFormModaliser.ShowDialogAndDispose(new AWBPrintForm(actions)) == DialogResult.OK && actions.PrintMasterAirWaybill)
				{
					try
					{
						consol.UpdateAWBPrinted();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}

		void IDocumentSupporterQueryProvider.ShowMessage(string message, string title)
		{
			Globals.Message.ShowInformation(message, title);
		}

		bool IDocumentSupporterQueryProvider.ShowConfirmation(string message, string title)
		{
			return Globals.Message.Show(message, title, MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes;
		}
	}
}
