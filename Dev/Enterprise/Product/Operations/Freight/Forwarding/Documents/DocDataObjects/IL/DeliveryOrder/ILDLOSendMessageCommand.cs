using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.MessageBuilders;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	class ILDLOSendMessageCommand : ILSendCustomCommandBase
	{
		public ILDLOSendMessageCommand(IDeliveryOrderProvider deliveryOrderMessageProvider)
			: base(deliveryOrderMessageProvider, false)
		{
		}

		protected new IDeliveryOrderProvider messageProvider => (IDeliveryOrderProvider)base.messageProvider;

		protected override string DocumentName => ILMessageEventParameter.DeliveryOrderDocumentName;

		protected override ZString MessageReference => messageProvider.MessageReferenceNumber;

		protected override IMessageBuilder GetMessageBuilder(DocumentVisualizer.DocDataObjects.DocDataObject docDataObject, bool isCancelActionTypeCode)
		{
			var deliveryOrderDocDataSendingObject = new DeliveryOrderDocDataSendingObject((DeliveryOrderDocDataObject)docDataObject, isCancelActionTypeCode);
			var messageBuilder = ObjectFactory.Get<Enterprise.Integration.Customs.IL.IILDLOMessageBuilder>("IL.IILDLOMessageBuilder", new object[] { messageProvider, deliveryOrderDocDataSendingObject }) as IMessageBuilder;
			return messageBuilder;
		}

		protected override string MessageGeneratedCore() => Enterprise.Freight.Forwarding.Documents.DataObjects.Res.GetString("4B18A430-A781-4806-A86D-5E7B939E06C4", "Delivery Order Message has been generated.");
	}
}
