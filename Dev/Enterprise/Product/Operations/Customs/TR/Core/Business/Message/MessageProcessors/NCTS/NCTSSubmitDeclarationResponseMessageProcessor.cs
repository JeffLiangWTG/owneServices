using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TR.Business
{
	public class NCTSSubmitDeclarationResponseMessageProcessor : NCTSMessageProcessor
	{
		public NCTSSubmitDeclarationResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => ResString.GetMultilingualString("35ADECCE-A54C-497D-85F3-770CDFB10777", "NCTS Submit Declaration Response Message Processor");

		protected override bool ProcessMessageCore(NCTSMessage message)
		{
			var result = false;
			if (message != null && message.EM_LinkedObject is Integration.Customs.TR.ICusInBondHeader header)
			{
				var messageText = message.EM_MessageText;
				var queryGUID = TRMessageHelper.GetNodeValue(messageText, "//x:submitdeclarationResponse/return/corrGuid", "http://ws/");
				if (!queryGUID.IsEmpty)
				{
					message.EM_ApplicationReference = queryGUID;
					TRMessageSendingHelper.CreateCusPollingTransaction(message, TRMessageTypes.Codes.TRN, queryGUID);
					result = true;
					TRInterchangeHelper.SetMessageOwnerByMainMessage(message);
					tableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.TableHeaderTRNRecieveFields);
					tableCreator.WriteRow(ResString.GetMultilingualString("DB202B33-5200-4794-9C04-AEF6C47B730D", "Query GUID:"), queryGUID);
					message.EM_MessageInterpretation = CreateInterpretationContent(message.EM_LinkedObject as IMessageAttachee);
				}
				else
				{
					header.BM_CustomsStatus = NCTSMovementHeaderCustomsStatusList.Codes.DRJ;
				}
			}

			return result;
		}

		ZString CreateInterpretationContent(IMessageAttachee header)
		{
			var title = ResString.GetMultilingualString("CAD08AE9-A530-420F-82E4-6117B2E47F72", "NCTS Query GUID for job {0} sent", header.JobReference);
			var htmlBody = string.Empty;

			if (tableCreator != null)
			{
				htmlBody = tableCreator.ToHtml().Replace("<th>", @"<th class=""th"">");
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody);
		}
	}
}

