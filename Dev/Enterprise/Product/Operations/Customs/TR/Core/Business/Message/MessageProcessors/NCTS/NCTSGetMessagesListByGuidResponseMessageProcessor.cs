using System.Collections.Generic;
using System.Text;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public class NCTSGetMessagesListByGuidResponseMessageProcessor : NCTSMessageProcessor
	{
		public NCTSGetMessagesListByGuidResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		const string NoErrorCode = "000";

		protected override string MessageFriendlyNameCore => ResString.GetMultilingualString("351E1280-C712-41F3-AD8C-FB653796B877", "NCTS Get Messages List By Guid Response Message Processor");

		protected override bool ProcessMessageCore(NCTSMessage message)
		{
			var result = false;

			if (message != null && message.EM_LinkedObject is IMessageAttachee headerAttachee)
			{
				var messageText = message.EM_MessageText;
				var processingResult = TRMessageSendingHelper.ProcessCusPollingTransaction(message, messageText, TRMessageTypes.Codes.TRN);
				TRInterchangeHelper.SetMessageOwnerByMainMessage(message);
				if (processingResult == Core.Constants.Customs.CusPollingTransactionStatus.Codes.CLS)
				{
					var errorCode = TRMessageHelper.GetNodeValue(messageText, "//x:getMessagesListByGuidResponse/return/error", "http://ws/");
					if (errorCode == NoErrorCode)
					{
						var list = TRMessageHelper.GetNodeValues(messageText, new List<ZString>() { (NoResString)"Envelope", (NoResString)"Body", "getMessagesListByGuidResponse", (NoResString)"return" }, (NoResString)"list");
						foreach (var index in list)
						{
							TRMessageSendingHelper.CreateCusPollingTransaction(message, TRMessageTypes.Codes.T1N, index, TRMessageConstants.TransactionPollingDelay);
						}
						result = true;
						headerAttachee.MessageStatus = TRMessageStatusCodeList.Codes.Accepted;

						tableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.TableHeaderT1NRecieveIndexFields);
						var errorDesc = UniversalReferenceDataHelper.GetErrorDescByCode(message.Factory, errorCode);
						tableCreator.WriteRow(NoErrorCode, errorDesc, string.Join(System.Environment.NewLine, list));
						message.EM_MessageInterpretation = CreateInterpretationContent(message.EM_LinkedObject as IMessageAttachee, true);
						SendNotificationEmailIfNeeded(message.EM_LinkedObject as IMessageAttachee, message, true);
					}
					else
					{
						tableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.TableHeaderT1NRecieveMessageFields);
						var errorDesc = UniversalReferenceDataHelper.GetErrorDescByCode(message.Factory, errorCode);
						tableCreator.WriteRow(errorCode, errorDesc);
						SendNotificationEmailIfNeeded(message.EM_LinkedObject as IMessageAttachee, message, false);
						headerAttachee.MessageStatus = TRMessageStatusCodeList.Codes.Error;
						if (message != null && message.EM_LinkedObject is Integration.Customs.TR.ICusInBondHeader header)
						{
							header.BM_CustomsStatus = NCTSMovementHeaderCustomsStatusList.Codes.DRJ;
						}
					}
				}
				else if (processingResult == Core.Constants.Customs.CusPollingTransactionStatus.Codes.ERR)
				{
					tableCreator = new HtmlTableCreator();
					tableCreator.WriteRow(ResString.GetMultilingualString("9526B01E-B391-4F61-98A4-8F7E2952D644", "Error Message:"), ResString.GetMultilingualString("EBDCCA50-6814-43B3-93E6-6357AFD18F83", "The message text sent back from Customs is empty, please try to resend original message"));
					SendNotificationEmailIfNeeded(headerAttachee, message, false);
					headerAttachee.MessageStatus = TRMessageStatusCodeList.Codes.Error;
					if (message != null && message.EM_LinkedObject is Integration.Customs.TR.ICusInBondHeader header)
					{
						header.BM_CustomsStatus = NCTSMovementHeaderCustomsStatusList.Codes.DRJ;
					}
				}
				else if (processingResult == Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN)
				{
					result = true;
				}
			}

			return result;
		}

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, NCTSMessage message, bool isSuccess)
		{
			var htmlBody = new StringBuilder();
			htmlBody.Append(ResString.GetMultilingualString("CC001F0D-76A2-49F6-B9CE-B411260E24EB", "NCTS Get Messages List By Guid Response error for job {0}.", messageAttacheeBO.JobReference));
			htmlBody.Append("<br /><br />");
			htmlBody.Append(tableCreator.ToHtml());
			return htmlBody.ToString();
		}

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, NCTSMessage message) => ResString.GetMultilingualString("E5EFF4A3-A6F8-4AF6-A350-EEEDECAD1493", "NCTS Get Messages List By Guid Response");

		ZString CreateInterpretationContent(IMessageAttachee header, bool isSuccess)
		{
			ZString title;

			if (isSuccess)
			{
				title = ResString.GetMultilingualString("D99ADBEF-AB6D-4265-A68A-37DDF661B755", "NCTS Get Messages List Message for job {0} By Guid Response Message Processor", header.JobReference);
			}
			else
			{
				title = ResString.GetMultilingualString("DCAEEDCB-844B-463C-B8D7-D768DA83A0B5", "NCTS Get Messages List Message for job {0} By Guid Response Error", header.JobReference);
			}

			var htmlBody = string.Empty;
			if (tableCreator != null)
			{
				htmlBody = tableCreator.ToHtml().Replace("<th>", @"<th class=""th"">");
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody);
		}
	}
}
