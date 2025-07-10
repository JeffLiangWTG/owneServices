using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentDocumentSupporterForTest : ForwardingShipmentDocumentSupporter
	{
		public ForwardingShipmentDocumentSupporterForTest(ForwardingShipment forwardingShipment)
			: base(forwardingShipment)
		{
		}

		public new IForwardingShipmentDocumentSupporterQueryProvider QueryProvider
		{
			get { return base.QueryProvider; }
		}

		public new bool PrintColoadsOnManifest(ForwardingConsol consol)
		{
			return base.PrintColoadsOnManifest(consol);
		}

		public DocumentWrapper[] GetDocumentWrappersInternal_Exposed(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
		}
	}
}
