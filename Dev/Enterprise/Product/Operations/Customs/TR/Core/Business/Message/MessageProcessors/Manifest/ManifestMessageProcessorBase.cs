using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using AsycudaManifestHeader = Enterprise.Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader;

namespace Enterprise.Customs.TR.Business
{
	public abstract class ManifestMessageProcessorBase : TRBranchCustomsApplicationTypeMessageProcessor<TRManifestMessage>
	{
		public ManifestMessageProcessorBase(LoggingInformation logger)
			: base(logger)
		{
		}

		public HtmlTableCreator TableCreator { get; protected set; }
		public bool NeedToUpdateStatus { get; protected set; } = true;
		public bool NeedToSendEmail { get; protected set; } = true;

		protected override string MessageFriendlyNameCore => Res.GetString("E844CA97-4E11-4052-B9F1-7DFE3F24AE06", "TR Manifest Message Processor");

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.TRCustoms;

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject)
		{
			var header = linkedObject as AsycudaManifestHeader;
			return header?.AMA_GB ?? ZGuid.Empty;
		}

		protected override bool ProcessMessageCore(TRManifestMessage message)
		{
			var isSuccess = false;
			var headerAttachee = message.EM_LinkedObject as IMessageAttachee;

			var defaultInnerMessageObject = message.MessageObject.InnerMessageObjects.FirstOrDefault();

			if (defaultInnerMessageObject is SOAPLevelErrorObject errorObject)
			{
				isSuccess = false;
				TableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.TableHeaderSoapMessage);
				TableCreator.WriteRow(errorObject.ErrorMessage);
				HandleErrorMessage(message, errorObject);
			}
			else if (defaultInnerMessageObject is SOAPLevelExceptionObject exceptionObject)
			{
				isSuccess = false;
				TableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.TableHeaderSoapMessage);
				TableCreator.WriteRow(exceptionObject.FaultString);
			}
			else
			{
				isSuccess = ProcessMessage(message, headerAttachee);
			}

			message.EM_MessageInterpretation = GetMessageInterpretation(headerAttachee, message, isSuccess);
			SendNotificationEmailIfNeeded(headerAttachee, message, isSuccess);
			return isSuccess;
		}

		protected abstract bool ProcessMessage(TRManifestMessage message, IMessageAttachee headerAttachee);

		protected override void UpdateStatusCore(TRManifestMessage message, bool isSuccess)
		{
			if (NeedToUpdateStatus)
			{
				var header = message.EM_LinkedObject as AsycudaManifestHeader;
				if (header != null)
				{
					header.RegistrationStatus = isSuccess ? TRMessageStatusCodeList.Codes.CLR : TRMessageStatusCodeList.Codes.Error;
					header.AMA_MessageStatus = isSuccess ? TRMessageStatusCodeList.Codes.Accepted : TRMessageStatusCodeList.Codes.Error;
				}
			}
		}

		protected virtual void HandleErrorMessage(TRManifestMessage message, SOAPLevelErrorObject errorObject) { }

		#region Interpretation & Emailing

		protected override sealed string MailSubject(IMessageAttachee messageAttacheeBO, TRManifestMessage message) => Res.GetString("E30ED0C6-E054-4A5B-A068-406F0E1FA831", "Manifest Message");

		ZString GetMessageInterpretation(IMessageAttachee messageAttacheeBO, TRManifestMessage message, bool isSuccess)
		{
			ZString result = ZString.Empty;
			if (TableCreator != null)
			{
				var title = GetInterpretationTitle(messageAttacheeBO, message, isSuccess);
				result = MessageInterpretationGenerator.FormatOutputInTemplate(title, TableCreator.ToHtml().Replace("<th>", $@"<th class=""th"">"));
			}

			return result;
		}

		protected virtual ZString GetInterpretationTitle(IMessageAttachee messageAttacheeBO, TRManifestMessage message, bool isSuccess) => Res.GetString(
				"E8AEDF14-0339-4C46-91F2-70BB2DECC6FF",
				"Manifest Message for job {0} has been {1}.",
				messageAttacheeBO.JobReference,
				isSuccess ? Res.GetString("DCAC5D85-6C24-4D7F-A4F1-7A0EB51D4AF1", "cleared") : Res.GetString("257BDFA0-BA7F-4421-9EC0-195D8F785A44", "rejected")
		);

		protected override sealed ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, TRManifestMessage message, bool isSuccess)
		{
			var formattedHtmlBody = CreateInterpretationContent(message.EM_MessageInterpretation);
			return MessageInterpretationGenerator.FormatOutputInTemplateWithSuccessFailureImage(formattedHtmlBody, isSuccess);
		}

		#endregion
	}
}
