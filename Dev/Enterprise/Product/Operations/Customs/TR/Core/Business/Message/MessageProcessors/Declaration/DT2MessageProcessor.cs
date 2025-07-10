using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public class DT2MessageProcessor : DeclarationMessageProcessorBase
	{
		public DT2MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		const int RegistrationType = 2;

		protected override bool ProcessMessageWithDeclaration(TRImportExportMessage message, CusEntryHeader entryHeader)
		{
			var result = false;
			MessageDetailHtmlTableCreator = new HtmlTableCreator((NoResString)"table");
			if (message.MessageObject.InnerMessageObjects.Cast<DiffGramGuidObject>().OrderByDescending(dt2Object => dt2Object.OptionTime)
														 .FirstOrDefault() is DiffGramGuidObject resultObject && resultObject.Tip == RegistrationType && !resultObject.Guid.IsEmpty)
			{
				result = true;
				var temporaryQueryGUID = resultObject.Guid;
				message.EM_ApplicationReference = temporaryQueryGUID.ToString();
				TRMessageSendingHelper.CreateCusPollingTransaction(message, TRMessageTypes.Codes.DT2, message.EM_ApplicationReference);

				MessageDetailHtmlTableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.RowHeaders_Labelvalue);
				MessageDetailHtmlTableCreator.WriteRow(Res.GetString("418A1924-A1C4-4B06-971A-D55F7E757992", "Query GUID:"), temporaryQueryGUID);
				var title = Res.GetString("418205E4-E2CC-4362-91D8-3A49318D439E", "Import-Export Message newest GUID for job {0} sent", entryHeader.Declaration.JobNumber);
				message.EM_MessageInterpretation = CreateInterpretationContent(title);
			}
			else
			{
				message.EM_MessageInterpretation = CreateInterpretationContent(SoapMessageTextHelper.Constants.OutputMessage.TextSentBackEmpty);
				SendNotificationEmailIfNeeded(message.EM_LinkedObject as IMessageAttachee, message, result);
			}

			return result;
		}

		protected override void UpdateStatusCore(TRImportExportMessage message, bool isSuccess)
		{
			if (message.EM_LinkedObject is IMessageAttachee header)
			{
				header.MessageStatus = isSuccess ? TRMessageStatusCodeList.Codes.Accepted : TRMessageStatusCodeList.Codes.Error;
				header.CustomsStatus = isSuccess ? header.GetEntryStatus(TRMessageTypes.Codes.DT2) : EntryStatusTypeList.Codes.ERR;
			}
		}

		protected override bool ShouldHandleInvalidCredentialMessage => true;
	}
}
