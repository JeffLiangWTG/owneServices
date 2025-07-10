using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors
{
	abstract class ResponseMessageProcessor : Enterprise.Messaging.MessageProcessors.CustomsMessageProcessor, IErrorNotification
	{
		protected ResponseMessageProcessor(LoggingInformation logger)
			: base(logger, MessageTypes.Codes.eManifest, "e-Manifest Response") { }

		#region Implementation of IErrorNotification

		void IErrorNotification.SendError(EmailDef email)
		{
			SendErrorReport(EmailResponseLinkedObject, email);
		}

		void IErrorNotification.SendErrorToPostMaster(EmailDef email)
		{
			if (ErrorEmailGroup != Env.Registry.PostMasterGroup)
			{
				SendReport(email, EmailResponseLinkedObject, Core.Constants.EmailTo.NominatedGroup, Env.Registry.PostMasterGroup);
			}
		}

		void IErrorNotification.LogError(string errorMessage)
		{
			Logger.LogError(errorMessage);
		}

		string IErrorNotification.MessageProcessorName
		{
			get { return MessageFriendlyName; }
		}

		protected BusinessObject EmailResponseLinkedObject
		{
			get { return originalMessage ?? (linkedObject != null ? linkedObject.Messages.Master : null); }
		}

		#endregion

		#region Overrides of MessageProcessor

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return USeManifestRegistry.Instance.SendMessageAcknowledgementsToGroup.Value; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return IsHVLV ? USeManifestRegistry.Instance.SendHVLVMessageAcknowledgements.Value : USeManifestRegistry.Instance.SendMessageAcknowledgements.Value; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return USeManifestRegistry.Instance.SendMessageErrorsToGroup.Value; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return IsHVLV ? USeManifestRegistry.Instance.SendHVLVMessageErrors.Value : USeManifestRegistry.Instance.SendMessageErrors.Value; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return USeManifestRegistry.Instance.SendMessageErrorsToGroup.Value; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return IsHVLV ? USeManifestRegistry.Instance.SendHVLVMessageErrors.Value : USeManifestRegistry.Instance.SendMessageErrors.Value; }
		}

		bool IsHVLV => (originalMessage?.EM_LinkedObject as Trip)?.
						Logs?.
						GetAllLogs()?.
						Cast<StmALog>()?.
						Any(log => log.SL_SE_NKEvent == Events.TransferredCode && log.Parameters.Any(para =>
							para.Key == CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type && para.Value == "HVL")) ?? false;

		protected override GlbStaff GetUserToNotify(IBusiness parent)
		{
			GlbStaff result = null;
			var message = parent as EDIMessage;
			if (message != null)
			{
				result = parent.Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, message.EM_SystemCreateUser);
			}
			return result ?? base.GetUserToNotify(parent);
		}

		#endregion

		#region Emails

		protected EmailDef GetAcceptedResponseEmailAndSetOnMessage(EDIMessage message)
		{
			var subject = message.EM_MessageSubType == MessageSubTypeCodes.Codes.Cancellation
							? string.Format("Cancellation accepted {0} Response for {1}", statusCalculator.MessageTypeDescription, linkedObjectReference)
							: string.Format("Accepted {0} Response for {1}", statusCalculator.MessageTypeDescription, linkedObjectReference);
			var emailBuilder = new EmailDefBuilder(subject, message.EM_FormattedMessageText, EmailDefBuilder.HtmlTemplates.AcceptedResponse);
			emailBuilder.AddArgReplacementRange(GetJobLink(), linkedObjectReference, statusCalculator.MessageTypeDescription);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, MessageSender);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, GetReportedData(), true);
			message.EM_MessageInterpretation = emailBuilder.ToString();
			return emailBuilder.ToEmail();
		}

		protected EmailDef GetErrorResponseEmailAndSetOnMessage(EDIMessage message)
		{
			var subject = string.Format("Error {0} Response for {1}", statusCalculator.MessageTypeDescription, linkedObjectReference);
			var emailBuilder = new EmailDefBuilder(subject, message.EM_MessageText.Replace("'", "\r\n"), EmailDefBuilder.HtmlTemplates.ErrorResponse);
			emailBuilder.AddArgReplacementRange(GetJobLink(), linkedObjectReference, statusCalculator.MessageTypeDescription);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.FromMessageSender, MessageSender);
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, GetNotificationsText());
			emailBuilder.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, GetInvalidDataText());
			message.EM_MessageInterpretation = emailBuilder.ToString();
			return emailBuilder.ToEmail();
		}

		protected string GetJobLink()
		{
			return EmailDefBuilder.GetJobLink((IControllerIDProvider)linkedObject, linkedObject.JobIdentification);
		}

		protected virtual string GetInvalidDataText()
		{
			return string.Empty;
		}

		protected virtual string GetNotificationsText()
		{
			return string.Empty;
		}

		protected virtual StringBuilder GetReportedData()
		{
			return new StringBuilder();
		}

		#endregion

		protected EDIMessage originalMessage;
		protected IEDIFACTMessageAttachee linkedObject;
		protected ZString linkedObjectReference;
		protected eManifestStatusCalculator statusCalculator;
		protected const string MessageSender = "from CBP ";
	}
}
