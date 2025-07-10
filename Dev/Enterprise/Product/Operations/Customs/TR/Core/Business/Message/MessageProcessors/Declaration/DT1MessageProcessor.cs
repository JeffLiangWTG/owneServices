using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TR.Business
{
	public class DT1MessageProcessor : DeclarationMessageProcessorBase
	{
		public DT1MessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		bool needToUpdateStatus = true;
		protected override bool ProcessMessageWithDeclaration(TRImportExportMessage incomingMessage, CusEntryHeader entryHeader)
		{
			var isSuccess = false;
			var defaultMessageObject = incomingMessage.MessageObject.InnerMessageObjects.FirstOrDefault();
			if (defaultMessageObject == null)
			{
				MessageDetailHtmlTableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass);
				MessageDetailHtmlTableCreator.WriteRow(SoapMessageTextHelper.Row_EmptyResponse);
				var title = Res.GetString("6F8025CC-F4E4-49AB-BF9A-DA676B03E054", "Import-Export message for job {0} has been rejected.", entryHeader?.Declaration?.JobNumber ?? ZString.Empty);
				incomingMessage.EM_MessageInterpretation = CreateInterpretationContent(title);

				var originalMessage = TRInterchangeHelper.GetOriginalMessageByTrackingId(incomingMessage);
				if (originalMessage == null)
				{
					Logger.LogError($"Unable to find the original outgoing message for the message, number: {incomingMessage.EM_MessageNum}");
				}
				else
				{
					TRMessageSendingHelper.SendImportExportAutoReceiveResponseMessageDT2(entryHeader, originalMessage, TRMessageTypes.Codes.DT1);
					needToUpdateStatus = false;
				}
			}
			else
			{
				if (defaultMessageObject is InnerXmlRegisterAnswerObject registerAnswerObject)
				{
					if (!registerAnswerObject.RegistrationNumber.IsEmpty)
					{
						var registrationNumber = registerAnswerObject.RegistrationNumber;
						var registrationDate = registerAnswerObject.RegistrationDate;

						entryHeader.CH_EntrySubmittedDate = registrationDate;
						entryHeader.RegistrationSetter(registrationNumber, registrationDate);

						MessageDetailHtmlTableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.RowHeaders_Labelvalue);
						MessageDetailHtmlTableCreator.WriteRow(Res.GetString("7B992FD1-E73E-44CA-A5F3-6713FC0F344F", "Registration Number:"), registrationNumber);
						MessageDetailHtmlTableCreator.WriteRow(Res.GetString("924B6F98-2E91-4018-B19B-B4D0E346FA01", "Registration Date:"), registrationDate.ToString(CusEntryMessageConstants.DateFormat.DayMonthYear));
					}

					var readResult = DeclarationMessageHelper.ReadAnswerAndUpdateEntry(entryHeader, registerAnswerObject.ControlAnswerObject);
					MessageDetailHtmlTableCreators = readResult.htmlTables;
					var title = readResult.title.FallbackTo(Res.GetString("AC628840-E70C-48AB-81CB-76C55FFFFE10", "Import-Export message for job {0} has been accepted.", entryHeader.Declaration.JobNumber));
					isSuccess = readResult.isSucceed;

					incomingMessage.EM_MessageInterpretation = CreateInterpretationContent(title);
				}
				else
				{
					incomingMessage.EM_MessageInterpretation = CreateInterpretationContent(SoapMessageTextHelper.Constants.OutputMessage.TextSentBackEmpty);
				}
				SendNotificationEmailIfNeeded(incomingMessage.EM_LinkedObject as IMessageAttachee, incomingMessage, isSuccess);
			}
			return isSuccess;
		}

		protected override void UpdateStatusCore(TRImportExportMessage message, bool isSuccess)
		{
			if (needToUpdateStatus && message.EM_LinkedObject is IMessageAttachee header)
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
						header.CustomsStatus = header.GetEntryStatus(TRMessageTypes.Codes.DT1);
					}
				}
			}
		}
	}
}
