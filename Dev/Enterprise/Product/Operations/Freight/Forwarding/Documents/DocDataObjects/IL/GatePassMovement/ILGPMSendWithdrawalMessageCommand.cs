using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.MessageBuilders;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	class ILGPMSendWithdrawalMessageCommand : ILSendWithdrawalCustomCommandBase
	{
		public ILGPMSendWithdrawalMessageCommand(IGatePassMovementProvider gatePassMessageProvider)
			: base(gatePassMessageProvider)
		{
		}

		protected new IGatePassMovementProvider messageProvider => (IGatePassMovementProvider)base.messageProvider;

		protected override string DocumentName => ILMessageEventParameter.GatePassMovementDocumentName;

		protected override ZString MessageReference => messageProvider.MessageReferenceNumber;

		protected override IMessageBuilder GetMessageBuilder(DocumentVisualizer.DocDataObjects.DocDataObject docDataObject, bool isCancelActionTypeCode)
		{
			var gatePassMovementDocDataSendingObject = new GatePassMovementDocDataSendingObject((GatePassMovementDocDataObject)docDataObject, isCancelActionTypeCode);
			var messageBuilder = ObjectFactory.Get<Enterprise.Integration.Customs.IL.IILGPMMessageBuilder>("IL.IILGPMMessageBuilder", new object[] { messageProvider, gatePassMovementDocDataSendingObject }) as IMessageBuilder;
			return messageBuilder;
		}

		protected override string MessageGeneratedCore() => Enterprise.Freight.Forwarding.Documents.DataObjects.Res.GetString("5311FCC2-1B8B-4616-99F9-DE0A717E7D73", "Gatepass Movement Message withdrawal has been generated.");
	}
}
