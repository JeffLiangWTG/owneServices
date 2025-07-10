using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.MessageBuilders;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	class ILGPMSendMessageCommand : ILSendCustomCommandBase
	{
		public ILGPMSendMessageCommand(IGatePassMovementProvider gatePassMessageProvider)
			: base(gatePassMessageProvider, false)
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

		protected override string MessageGeneratedCore() => Enterprise.Freight.Forwarding.Documents.DataObjects.Res.GetString("9D8E4CFC-3D97-474F-ACE8-AF495B7DB6E4", "Gatepass Movement Message has been generated.");
	}
}
