using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	public class DeliveryOrderMessagingExtensions : ILBaseMessagingExtensions
	{
		public DeliveryOrderMessagingExtensions(IDeliveryOrderProvider provider)
			: base(provider)
		{
		}

		protected override string MessageName => Res.GetString("D829E9F5-FCDA-4EB7-AAD2-9C964AF5D0D9", "Delivery Order");

		protected override string DocumentName => ILMessageEventParameter.DeliveryOrderDocumentName;

		protected override ZString MessageReference => messageProvider.MessageReferenceNumber;
	}
}
