using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	public class GatePassMovementMessagingExtensions : ILBaseMessagingExtensions
	{
		public GatePassMovementMessagingExtensions(IGatePassMovementProvider provider)
			: base(provider)
		{
		}

		protected override string DocumentName => ILMessageEventParameter.GatePassMovementDocumentName;

		protected override string MessageName => Res.GetString("622FE6A7-1E2E-4A16-A8A5-1D984F4977B2", "Gatepass Movement");

		protected override ZString MessageReference => messageProvider.MessageReferenceNumber;
	}
}
