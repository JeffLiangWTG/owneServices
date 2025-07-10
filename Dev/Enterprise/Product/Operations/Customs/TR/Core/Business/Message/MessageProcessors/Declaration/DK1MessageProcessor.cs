using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TR.Business
{
	public class DK1MessageProcessor : DeclarationMessageProcessorBase
	{
		public DK1MessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override bool ProcessMessageWithDeclaration(TRImportExportMessage incomingMessage, CusEntryHeader entryHeader)
		{
			var isSuccess = false;
			var defaultMessageObject = incomingMessage.MessageObject.InnerMessageObjects.FirstOrDefault();
			if (defaultMessageObject is InnerXmlControlAnswerObject answerObject)
			{
				var result = DeclarationMessageHelper.ReadAnswerAndUpdateEntry(entryHeader, answerObject);
				isSuccess = result.isSucceed;
				MessageDetailHtmlTableCreators = result.htmlTables;
				incomingMessage.EM_MessageInterpretation = CreateInterpretationContent(result.title);
			}
			else
			{
				if (defaultMessageObject is SOAPLevelExceptionObject exceptionObject)
				{
					var title = Res.GetString("67026635-5ee6-4729-b9d4-1ea0721d3dc3", "Declaration Message Type DK1 received successfully.");
					MessageDetailHtmlTableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.RowHeaders_Labelvalue);
					MessageDetailHtmlTableCreator.WriteRow("soapenv:Server", exceptionObject.FaultString);
					incomingMessage.EM_MessageInterpretation = CreateInterpretationContent(title);
				}
				else
				{
					incomingMessage.EM_MessageInterpretation = CreateInterpretationContent(SoapMessageTextHelper.Constants.OutputMessage.TextSentBackEmpty);
				}
			}
			SendNotificationEmailIfNeeded(incomingMessage.EM_LinkedObject as IMessageAttachee, incomingMessage, isSuccess);

			return isSuccess;
		}

		protected override void UpdateStatusCore(TRImportExportMessage message, bool isSuccess)
		{
			if (message.EM_LinkedObject is IMessageAttachee header)
			{
				header.MessageStatus = isSuccess ? TRMessageStatusCodeList.Codes.Accepted : TRMessageStatusCodeList.Codes.Error;
				if (!isSuccess)
				{
					header.CustomsStatus = EntryStatusTypeList.Codes.ERR;
				}
				else
				{
					var isNotWRNorSDOrQUE = header.GetIsNotWRNorSDOrQUE(header.CustomsStatus);
					if (isNotWRNorSDOrQUE)
					{
						header.CustomsStatus = header.GetEntryStatus(TRMessageTypes.Codes.DK1);
					}
				}
			}
		}
	}
}
