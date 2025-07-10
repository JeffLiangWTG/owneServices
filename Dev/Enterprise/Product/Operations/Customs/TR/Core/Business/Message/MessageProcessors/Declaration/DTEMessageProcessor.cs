using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TR.Business
{
	public class DTEMessageProcessor : DeclarationMessageProcessorBase
	{
		public DTEMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override bool ProcessMessageWithDeclaration(TRImportExportMessage incomingMessage, CusEntryHeader entryHeader)
		{
			var isSuccess = false;
			var messageText = incomingMessage.EM_MessageText;
			var messageGuid = SoapMessageTextHelper.GetResponseGuid(messageText);
			if (!messageGuid.IsEmpty)
			{
				isSuccess = true;
				incomingMessage.EM_ApplicationReference = messageGuid;
				TRMessageSendingHelper.CreateCusPollingTransaction(incomingMessage, TRMessageTypes.Codes.DTE, messageGuid, TRMessageConstants.DT1PollingDelay);

				MessageDetailHtmlTableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.RowHeaders_Labelvalue);
				MessageDetailHtmlTableCreator.WriteRow(Res.GetString("F412F555-59A5-4DF8-8B27-9BA34245A0E8", "Query GUID:"), messageGuid);
				var title = Res.GetString("B4E11F9A-BE0B-48B6-8788-B3453FE20EE1", "Customs Declaration message for job {0} has been accepted.", entryHeader?.Declaration?.JobNumber ?? ZString.Empty);
				incomingMessage.EM_MessageInterpretation = CreateInterpretationContent(title);
				SendNotificationEmailIfNeeded(incomingMessage.EM_LinkedObject as IMessageAttachee, incomingMessage, isSuccess);
			}
			else
			{
				var errorMesaage = TRMessageHelper.GetNodeValue(messageText, "//x:Root/Error/Message", "http://schemas.microsoft.com/BizTalk/2003/Any");
				if (errorMesaage == TRMessageConstants.RegisteredOrInProcessWithThisReferenceMessage)
				{
					MessageDetailHtmlTableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.TableHeaderSoapMessage);
					MessageDetailHtmlTableCreator.WriteRow(errorMesaage);
					var title = Res.GetString("6F8025CC-F4E4-49AB-BF9A-DA676B03E054", "Import-Export message for job {0} has been rejected.", entryHeader?.Declaration?.JobNumber ?? ZString.Empty);
					incomingMessage.EM_MessageInterpretation = CreateInterpretationContent(title);

					var origialMessage = TRInterchangeHelper.GetOriginalMessageByTrackingId(incomingMessage);
					if (origialMessage == null)
					{
						Logger.LogError($"Unable to find the original outgoing message for the message, number: {incomingMessage.EM_MessageNum}");
					}
					else
					{
						TRMessageSendingHelper.SendImportExportAutoReceiveResponseMessageDT2(entryHeader, origialMessage, TRMessageTypes.Codes.DTE);
					}
				}
			}

			return isSuccess;
		}

		protected override void UpdateStatusCore(TRImportExportMessage message, bool isSuccess)
		{
			if (message.EM_LinkedObject is IMessageAttachee header)
			{
				header.MessageStatus = isSuccess ? TRMessageStatusCodeList.Codes.Accepted : TRMessageStatusCodeList.Codes.Error;
				header.CustomsStatus = isSuccess ? header.GetEntryStatus(TRMessageTypes.Codes.DTE) : EntryStatusTypeList.Codes.ERR;
			}
		}
	}
}
