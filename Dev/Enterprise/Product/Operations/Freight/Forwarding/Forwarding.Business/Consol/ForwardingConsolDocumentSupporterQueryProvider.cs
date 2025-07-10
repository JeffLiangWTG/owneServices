using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.Integration.Freight;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolDocumentSupporterQueryProvider : CommonConsolDocumentSupporterQueryProvider, IForwardingConsolDocumentSupporterQueryProvider
	{
		DeliveryAgentOrgHeader[] IForwardingConsolDocumentSupporterQueryProvider.GetDeliveryAgentsToPrint(DeliveryAgentToSelectFromForPrintingCollection deliveryAgentsToSelectFrom)
		{
			return deliveryAgentsToSelectFrom != null ? deliveryAgentsToSelectFrom.Cast<DeliveryAgentOrgHeader>().ToArray() : null;
		}

		DocumentImportCargoLabel IForwardingConsolDocumentSupporterQueryProvider.GetImportCargoLabelToPrint(DocumentImportCargoLabel documentImportCargoLabel)
		{
			return documentImportCargoLabel;
		}

		void IForwardingConsolDocumentSupporterQueryProvider.PrintAWBLabels(ForwardingConsol consol)
		{
		}

		void IForwardingConsolDocumentSupporterQueryProvider.PrintFinalMaster(ForwardingConsol consol, string menuPath)
		{
		}

		void IDocumentSupporterQueryProvider.ShowMessage(string message, string caption)
		{
		}

		bool IDocumentSupporterQueryProvider.ShowConfirmation(string message, string caption)
		{
			return true;
		}
	}
}
