using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	class ILDLOResetToOriginalMessageCommand : ILResetToOriginalMessageCommandBase
	{
		public ILDLOResetToOriginalMessageCommand(IDeliveryOrderProvider deliveryOrderMessageProvider)
			: base(deliveryOrderMessageProvider)
		{
		}

		public override bool IsVisible => true;

		protected override void ResetToOriginal()
		{
			messageProvider.MessageReferenceNumber = ZString.Empty;
		}

		protected override string Warning =>
			Enterprise.Freight.Forwarding.Documents.DataObjects.Res.GetString("CFC5BA9B-F64A-496E-886A-4F6EB80721B8", @"WARNING: Using this option without checking with Customs first might result in duplicate messages being processed by Customs. 
Resetting to Original should only be required when previous delivery order was canceled or there is a serious messaging failure at the Customs end.
In the normal course of events, every message you send should be responded to, so the system knows what kind of message to send automatically. 
Before using this option, you should always check with Customs to make sure they have not already processed the message.");

		protected new IDeliveryOrderProvider messageProvider => (IDeliveryOrderProvider)base.messageProvider;

		protected override string DocumentName => ILMessageEventParameter.DeliveryOrderDocumentName;

		protected override ZString MessageReference => messageProvider.MessageReferenceNumber;
	}
}
