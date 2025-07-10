using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TR.Business
{
	public class DKOMessageProcessor : DeclarationMessageProcessorBase
	{
		public DKOMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override bool ProcessMessageWithDeclaration(TRImportExportMessage incomingMessage, CusEntryHeader entryHeader)
		{
			var isSuccess = false;
			var messageText = incomingMessage.EM_MessageText;
			var messageGuid = SoapMessageTextHelper.GetResponseGuid(messageText);
			if (!messageGuid.IsEmpty)
			{
				isSuccess = true;
				incomingMessage.EM_ApplicationReference = messageGuid;
				TRMessageSendingHelper.CreateCusPollingTransaction(incomingMessage, TRMessageTypes.Codes.DKO, messageGuid);

				MessageDetailHtmlTableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.RowHeaders_Labelvalue);
				MessageDetailHtmlTableCreator.WriteRow(Res.GetString("189CBEB2-E2EA-4CC5-AAA4-716B3358179B", "Query GUID:"), messageGuid);
				var title = Res.GetString("CE2EADC6-CEC3-42D1-8A3D-FB55874478A9", "Customs Declaration message for job {0} has been accepted.", entryHeader?.Declaration?.JobNumber ?? ZString.Empty);
				incomingMessage.EM_MessageInterpretation = CreateInterpretationContent(title);
				SendNotificationEmailIfNeeded(incomingMessage.EM_LinkedObject as IMessageAttachee, incomingMessage, isSuccess);
			}

			return isSuccess;
		}

		protected override void UpdateStatusCore(TRImportExportMessage message, bool isSuccess)
		{
			if (message.EM_LinkedObject is IMessageAttachee header)
			{
				header.MessageStatus = isSuccess ? TRMessageStatusCodeList.Codes.Accepted : TRMessageStatusCodeList.Codes.Error;
				header.CustomsStatus = isSuccess ? header.GetEntryStatus(TRMessageTypes.Codes.DKO) : EntryStatusTypeList.Codes.ERR;
			}
		}
	}
}
