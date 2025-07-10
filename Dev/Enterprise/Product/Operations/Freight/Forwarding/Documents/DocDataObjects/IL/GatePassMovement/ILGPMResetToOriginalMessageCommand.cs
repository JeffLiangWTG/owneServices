using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	class ILGPMResetToOriginalMessageCommand : ILResetToOriginalMessageCommandBase
	{
		public ILGPMResetToOriginalMessageCommand(IGatePassMovementProvider gatePassMessageProvider) : base(gatePassMessageProvider)
		{
		}

		public override bool IsVisible => true;

		protected override void ResetToOriginal()
		{
			messageProvider.MessageReferenceNumber = ZString.Empty;
		}

		protected override string Warning =>
			Enterprise.Freight.Forwarding.Documents.DataObjects.Res.GetString("A6AA2C95-7403-42D0-BB27-28E668CCACC1", @"WARNING: Using this option without checking with Customs first might result in duplicate messages being processed by Customs. 
Resetting to Original should only be required when previous Cargo Movement was canceled or there is a serious messaging failure at the Customs end.
In the normal course of events, every message you send should be responded to, so the system knows what kind of message to send automatically. 
Before using this option, you should always check with Customs to make sure they have not already processed the message.");

		protected new IGatePassMovementProvider messageProvider => (IGatePassMovementProvider)base.messageProvider;

		protected override string DocumentName => ILMessageEventParameter.GatePassMovementDocumentName;

		protected override ZString MessageReference => messageProvider.MessageReferenceNumber;
	}
}

