using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.TR.Business
{
	public class DT3MessageProcessor : DeclarationMessageProcessorBase
	{
		public DT3MessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override bool ProcessMessageWithDeclaration(TRImportExportMessage incomingMessage, CusEntryHeader entryHeader)
		{
			var isSuccess = false;
			var defaultMessageObject = incomingMessage.MessageObject.InnerMessageObjects.FirstOrDefault();
			if (defaultMessageObject != null)
			{
				if (defaultMessageObject is InnerXmlRegisterAnswerObject registerAnswerObject)
				{
					if (!registerAnswerObject.RegistrationNumber.IsEmpty)
					{
						isSuccess = true;
						var registrationNumber = registerAnswerObject.RegistrationNumber;
						var registrationDate = registerAnswerObject.RegistrationDate;

						entryHeader.CH_EntrySubmittedDate = registrationDate;
						entryHeader.RegistrationSetter(registrationNumber, registrationDate);

						MessageDetailHtmlTableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.RowHeaders_Labelvalue);
						MessageDetailHtmlTableCreator.WriteRow(Res.GetString("7B992FD1-E73E-44CA-A5F3-6713FC0F344F", "Registration Number:"), registrationNumber);
						MessageDetailHtmlTableCreator.WriteRow(Res.GetString("924B6F98-2E91-4018-B19B-B4D0E346FA01", "Registration Date:"), registrationDate.ToString(CusEntryMessageConstants.DateFormat.DayMonthYear));
						var title = Res.GetString("AC628840-E70C-48AB-81CB-76C55FFFFE10", "Import-Export message for job {0} has been accepted.", entryHeader?.Declaration?.JobNumber ?? ZString.Empty);
						incomingMessage.EM_MessageInterpretation = CreateInterpretationContent(title);
					}
				}

				if (!isSuccess)
				{
					incomingMessage.EM_MessageInterpretation = CreateInterpretationContent(SoapMessageTextHelper.Constants.OutputMessage.NoRegistrationInfo);
				}
				SendNotificationEmailIfNeeded(incomingMessage.EM_LinkedObject as IMessageAttachee, incomingMessage, isSuccess);
			}
			return isSuccess;
		}

		protected override void UpdateStatusCore(TRImportExportMessage message, bool isSuccess)
		{
			if (message.EM_LinkedObject is IMessageAttachee header)
			{
				header.MessageStatus = isSuccess ? TRMessageStatusCodeList.Codes.Accepted : TRMessageStatusCodeList.Codes.Error;
				header.CustomsStatus = isSuccess ? header.GetEntryStatus(TRMessageTypes.Codes.DT3) : EntryStatusTypeList.Codes.ERR;
			}
		}
	}
}
