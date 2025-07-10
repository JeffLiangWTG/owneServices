using System.Linq;
using System.Text;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using AsycudaManifestHeader = Enterprise.Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader;

namespace Enterprise.Customs.TR.Business
{
	public class ETradeQueryInspectionClerkMessageProcessor : ETradeMessageProcessor
	{
		public ETradeQueryInspectionClerkMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}
		HtmlTableCreator tableCreator;

		protected override string MessageFriendlyNameCore => Res.GetString("8BF92F3F-BA94-4428-B755-530BA5C6433B", "E-Trade Inspection Clerk Message");

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message, bool isSuccess)
		{
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();
			if (isSuccess)
			{
				title = Res.GetString("D816A38E-16BD-4167-89B0-5500087BC39C", "E-Trade Inspection Clerk Message for job {0} has been cleared. For details please follow the Link to the E-Trade", messageAttacheeBO.JobReference);
			}
			else
			{
				title = Res.GetString("C6A6E6F9-287A-4EFF-B8BB-3F2D24266C2A", "E-Trade Inspection Clerk Message for job {0} has been rejected. For details please follow the Link to the E-Trade", messageAttacheeBO.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, ETradeEDIMessage message) => Res.GetString("EA8152E5-ED37-4205-8B09-AE0D79795C61", "E-Trade Inspection Clerk Message Status");

		protected override bool ProcessMessageCore(ETradeEDIMessage message)
		{
			var isSuccess = false;
			var defaultMessageObject = message.MessageObject.InnerMessageObjects.FirstOrDefault();

			if (message != null && message.EM_LinkedObject is AsycudaManifestHeader header)
			{
				if (defaultMessageObject is TRIMessageResponseObject answerObject && answerObject.Errors.Count > 0)
				{
					tableCreator = new HtmlTableCreator((NoResString)"table", new string[] { SoapMessageTextHelper.TableHeaderWithErrorMessage });
					foreach (var error in answerObject.Errors)
					{
						tableCreator.WriteRow(new string[] { error.Description });
					}
				}
				else
				{
					tableCreator = new HtmlTableCreator((NoResString)"table", SoapMessageTextHelper.RowHeaders_Labelvalue);

					if (defaultMessageObject != null && !defaultMessageObject.MessageText.IsEmpty())
					{
						header.InspectionClerk = defaultMessageObject.MessageText;
						tableCreator.WriteRow(Res.GetString("C8EE6AEC-2BDF-4B10-B080-1B39CE1BC8F9", "Inspection Clerk: "), header.InspectionClerk);
						isSuccess = true;
					}
					else
					{
						tableCreator.WriteRow(Res.GetString("ABBE9547-2361-4E7E-91FC-E9CF2DC0A4FD", "Error Message: "), Res.GetString("28D3076E-ACEA-4AE9-8E9F-A711561D6A07", "The message missed the Inspection Clerk"));
					}
				}

				message.EM_MessageInterpretation = CreateInterpretationContent(message.EM_LinkedObject as IMessageAttachee, isSuccess);
				SendNotificationEmailIfNeeded((IMessageAttachee)header, message, isSuccess);
			}
			return isSuccess;
		}

		ZString CreateInterpretationContent(IMessageAttachee header, bool isSuccess)
		{
			var title = ZString.Empty;
			var htmlBody = new StringBuilder();

			if (isSuccess)
			{
				title = Res.GetString("5E400AEA-CAE2-4D45-9A1E-14FB82D07816", "E-Trade Inspection Clerk Message for job {0} has been cleared.", header.JobReference);
			}
			else
			{
				title = Res.GetString("8F4C07FE-DFCF-4E83-9CF6-2B8377E1F8E3", "E-Trade Inspection Clerk Message for job {0} has been rejected.", header.JobReference);
			}

			if (tableCreator != null)
			{
				htmlBody.Append(tableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}
			return MessageInterpretationGenerator.FormatOutputInTemplate(title, htmlBody.ToString());
		}

		protected override ZString OriginalMessageType => TRMessageTypes.Codes.TRI;
		protected override ControllerID ControllerIDForemail => ControllerIDs.Customs.TR.ETrade;
	}
}
