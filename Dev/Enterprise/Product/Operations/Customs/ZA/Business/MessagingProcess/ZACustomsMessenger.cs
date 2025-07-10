using CargoWise.Types;
using Enterprise.Customs.Business.BusinessObjects.CusPermit;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.MessagingProcess
{
	public sealed class ZACustomsMessenger : ICustomsMessenger, ISupportPermitProcessing
	{
		public static ICustomsMessenger New(MessageSendingObject msgSendingObject)
		{
			return new ZACustomsMessenger(msgSendingObject, CreateMessageGenerator(msgSendingObject));
		}

		static ICustomsMessageGenerator CreateMessageGenerator(MessageSendingObject msgSendingObject)
		{
			return new CUSDECMessageBuilder(msgSendingObject, MessageSubTypeCodes.TranslateToMessageSubType(msgSendingObject.MessageType));
		}
		ZACustomsMessenger(MessageSendingObject msgSendingObject, ICustomsMessageGenerator messageGenerator)
		{
			this.msgSendingObject = msgSendingObject;
			this.owner = msgSendingObject.Header;
			this.messageGenerator = messageGenerator;
			this.permitProcessor = new ZACusPermitCusDecProcessor(msgSendingObject.Header, msgSendingObject.MessageType);
		}
		readonly MessageSendingObject msgSendingObject;
		readonly IEDIMessageCollectionOwner owner;
		readonly ICustomsMessageGenerator messageGenerator;
		readonly ZACusPermitCusDecProcessor permitProcessor;

		ICustomsMessageGenerator ICustomsMessenger.MessageGenerator => messageGenerator;
		bool ICustomsMessenger.ShouldCreateMessage(ActionResult previousResult) => msgSendingObject.ShouldSend;

		bool ICustomsMessenger.ProcessUpdates(ActionResult previousResult)
		{
			if (previousResult.Success)
			{
				var header = msgSendingObject.Header;
				header.PopulateEntrySubmittedDateIfRequired();
				header.Declaration.LogCustomsCommencedIfNeeded();

				header.BackPopulateInvoiceLineTargetEntryLineNumberIfNeeded();
				header.UpdateLRNIfNeeded(msgSendingObject.LocalReferenceNumber, msgSendingObject.IsLRNEditable);

				header.MessageStatus = ZAMessageStatusList.Codes.AwaitingResponse;
			}

			return true;
		}

		IEDIMessageCollectionOwner ICustomsMessenger.Owner => owner;

		ICusPermitCusDecProcessor<EDIMessage> ISupportPermitProcessing.PermitProcessor => permitProcessor;
		ZString ISupportPermitProcessing.GetPermitAppIdForMessage(EDIMessage message) => ZAPermitHelper.GetPermitAppIdForMessage(message);
	}
}
