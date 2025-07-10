using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.Integration.Freight;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolDocumentSupporterQueryProviderForTest : CommonConsolDocumentSupporterQueryProviderForTest, IForwardingConsolDocumentSupporterQueryProvider
	{
		DeliveryAgentOrgHeader[] IForwardingConsolDocumentSupporterQueryProvider.GetDeliveryAgentsToPrint(DeliveryAgentToSelectFromForPrintingCollection deliveryAgentsToSelectFrom)
		{
			return null;
		}

		DocumentImportCargoLabel IForwardingConsolDocumentSupporterQueryProvider.GetImportCargoLabelToPrint(DocumentImportCargoLabel documentImportCargoLabel)
		{
			return null;
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
			return ZBool.False;
		}
	}
}
