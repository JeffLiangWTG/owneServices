using Enterprise.Freight.Business;
using Enterprise.Integration.Freight;

namespace Enterprise.Freight.Forwarding.Business
{
	public interface IForwardingConsolDocumentSupporterQueryProvider : ICommonConsolDocumentSupporterQueryProvider, IDocumentSupporterQueryProvider
	{
		DeliveryAgentOrgHeader[] GetDeliveryAgentsToPrint(DeliveryAgentToSelectFromForPrintingCollection deliveryAgentsToSelectFrom);
		DocumentImportCargoLabel GetImportCargoLabelToPrint(DocumentImportCargoLabel documentImportCargoLabel);
		void PrintAWBLabels(ForwardingConsol consol);
		void PrintFinalMaster(ForwardingConsol consol, string menuPath);
	}
}
