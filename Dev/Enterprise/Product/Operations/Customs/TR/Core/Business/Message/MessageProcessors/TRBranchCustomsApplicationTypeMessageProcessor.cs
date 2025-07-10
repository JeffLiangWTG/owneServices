using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TR.Business
{
	enum ProcessInvalidCredentialResult
	{
		NoneInvalidMessage,
		ProcessFail,
		ProcessSuccess
	}

	public abstract class TRBranchCustomsApplicationTypeMessageProcessor<TEDIMessage> : BranchCustomsApplicationTypeMessageProcessor
		where TEDIMessage : EDIMessage
	{
		protected TRBranchCustomsApplicationTypeMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected sealed override void PreProcessMessageCore(EDIMessage baseMessage)
		{
			var success = false;
			var message = (TEDIMessage)baseMessage;

			var originalMessage = TRInterchangeHelper.GetOriginalMessageByTrackingId(message);
			if (originalMessage != null)
			{
				message.EM_LinkUniqueID = originalMessage.EM_LinkUniqueID;
				message.EM_LinkTable = originalMessage.EM_LinkTable;

				var linkedObject = message.EM_LinkedObject;
				if (linkedObject == null)
				{
					Logger.LogWarning(Res.GetString("9AF3A06A-0E97-4A60-875D-C8B33F398B30", "Unable to find business object for message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to ERROR.", baseMessage.EM_MessageNum, baseMessage.EM_MessageType, baseMessage.EM_MessageSubType, baseMessage.EM_ApplicationReference));
				}
				else
				{
					var branchPK = GetCorrectBranchPK(linkedObject);
					if (branchPK.IsValid)
					{
						message.EM_GB = branchPK;
						success = true;
					}
				}
			}

			message.EM_Status = success ? EDIMessage.Status.PreProcessedOK : EDIMessage.Status.Failed;
		}

		protected abstract ZGuid GetCorrectBranchPK(BusinessObject linkedObject);

		protected sealed override void ProcessMessageCore(EDIMessage baseMessage)
		{
			var message = (TEDIMessage)baseMessage;
			var success = true;

			var hasInvalidCredentialMessage = ShouldHandleInvalidCredentialMessage ? ProcessInvalidCredentialMessage(baseMessage) : ProcessInvalidCredentialResult.NoneInvalidMessage;
			if (hasInvalidCredentialMessage == ProcessInvalidCredentialResult.NoneInvalidMessage)
			{
				success = ProcessMessageCore(message);
			}
			else
			{
				success = false;
			}

			baseMessage.EM_Status = EDIMessage.Status.ProcessedOK;

			UpdateStatus(message, success);
		}

		protected abstract bool ProcessMessageCore(TEDIMessage message);
		protected virtual bool ShouldHandleInvalidCredentialMessage => false;

		void UpdateStatus(TEDIMessage message, bool isSuccess) => UpdateStatusCore(message, isSuccess);

		protected virtual void UpdateStatusCore(TEDIMessage message, bool isSuccess) { }

		protected override Integration.IRegistryItem GetEmailGroupRegistryItem() => TRCustomsDataRegistry.Instance.TRMANGroupNotification;

		#region Process Invalid Credential Message
		ProcessInvalidCredentialResult ProcessInvalidCredentialMessage(EDIMessage message)
		{
			var result = ProcessInvalidCredentialResult.NoneInvalidMessage;

			var credentialErrorMessage = ZString.Empty;
			var headerAttachee = message.EM_LinkedObject as IMessageAttachee;
			if (headerAttachee != null && TRMessageHelper.TryGetCredentialErrorMessage(message.EM_MessageText, out credentialErrorMessage))
			{
				result = UpdateCredentialData(message, credentialErrorMessage);

				var tableCreator = new HtmlTableCreator();
				tableCreator.WriteRow(Res.GetString("17B08A03-4BFA-49D4-A5E4-F973A410A69D", "Error Message:"), credentialErrorMessage);
				var htmlBody = new StringBuilder();
				if (result == ProcessInvalidCredentialResult.ProcessSuccess)
				{
					htmlBody.Append(Res.GetString("2543DDF8-D8AB-4C6B-ABF2-E0C7CEAC66C9", "The Message for job {0} has been rejected.", headerAttachee.JobReference));
				}
				else
				{
					htmlBody.Append(Res.GetString("72BCEAF0-F944-488C-8731-F811CD9F25B0", "The Message for job {0} has been rejected. please update credential information of the message sender.", headerAttachee.JobReference));
				}
				htmlBody.Append("<br /><br />");
				htmlBody.Append(tableCreator.ToHtml());

				SendNotificationEmailIfNeeded(headerAttachee, (TEDIMessage)message, false, htmlBody.ToString());
			}

			return result;
		}

		ProcessInvalidCredentialResult UpdateCredentialData(EDIMessage message, ZString credentialErrorMessage)
		{
			var result = ProcessInvalidCredentialResult.ProcessFail;
			var originalMessage = TRInterchangeHelper.GetOriginalMessageByTrackingId(message);
			if (originalMessage != null)
			{
				var glbStaff = originalMessage.UserWhoQueuedThisRecord;
				if (glbStaff != null)
				{
					var staffWrapper = TRGlbStaffWrapper.Get(glbStaff);
					var password = staffWrapper.GetGlbExternalPassword<GlbExternalPassword>(PasswordTypesList.Codes.TRK, message.Branch.Company.PK);
					password.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
					result = ProcessInvalidCredentialResult.ProcessSuccess;
				}
			}
			return result;
		}

		#endregion

		#region Send Notification Email

		protected void SendNotificationEmailIfNeeded(IMessageAttachee messageAttacheeBO, TEDIMessage message, bool isSuccess, string emailBody = "")
		{
			if (messageAttacheeBO != null)
			{
				var branchPK = messageAttacheeBO.GlobalBranchPK;
				if (branchPK.IsValid)
				{
					messageBranch = message.Factory.Load<IGlbBranch>(branchPK);
				}
				else
				{
					messageBranch = Enterprise.MasterFiles.Business.GlbBranch.CurrentBranch;
				}

				var emailGroupRegistryItem = GetEmailGroupRegistryItem();
				var supportMessageSuppressRegistry = emailGroupRegistryItem as ISupportMessageSuppressRegistry;
				var shouldSendErrorEmailsOnly = supportMessageSuppressRegistry?.ShouldSEndErrorsOnly(MessageBranch.GB_GC, MessageBranch.PK, ZGuid.Empty) ?? false;
				var url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDForemail, messageAttacheeBO.PK.ToGuid());

				if (!shouldSendErrorEmailsOnly || (shouldSendErrorEmailsOnly && !isSuccess))
				{
					GenerateHtmlEmailAndSendToOriginalOrGroup(
						message.Factory,
						url,
						messageAttacheeBO.JobReference,
						MailSubject(messageAttacheeBO, message),
						string.IsNullOrEmpty(emailBody) ? GetEmailBody(messageAttacheeBO, message, isSuccess) : (ZString)emailBody,
						!isSuccess,
						message.Branch,
						messageAttacheeBO as BusinessObject,
						() => GetEmailAddressToSendTo(messageAttacheeBO, message));
				}
			}
		}

		protected virtual ControllerID ControllerIDForemail
		{
			get { return ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest; }
		}

		ZString GetEmailAddressToSendTo(IMessageAttachee messageAttacheeBO, EDIMessage baseMessage)
		{
			var result = ZString.Empty;
			if (messageAttacheeBO != null)
			{
				var originalMessage = GetOriginalMessage(messageAttacheeBO, baseMessage);
				var originalSender = (IUser)originalMessage?.UserWhoQueuedThisRecord;

				if (originalSender == null || originalSender.IsBatchProcessor)
				{
					originalMessage = GetLastTransmitMessage(messageAttacheeBO);
					originalSender = originalMessage?.UserWhoQueuedThisRecord;
				}
				result = originalSender?.EmailAddress ?? string.Empty;
			}
			return result;
		}

		EDIMessage GetOriginalMessage(IMessageAttachee messageAttacheeBO, EDIMessage baseMessage)
		{
			EDIMessage result = null;
			if (messageAttacheeBO != null)
			{
				if (originalMessageCached == null || originalMessageCached.EM_LinkUniqueID != messageAttacheeBO.PK)
				{
					var messageNumber = baseMessage.EM_MessageNum;
					if (!messageNumber.IsEmpty)
					{
						var originalMessageType = OriginalMessageType.IsEmpty ? baseMessage.EM_MessageType : OriginalMessageType;
						originalMessageCached = messageAttacheeBO.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageNum == messageNumber && x.PK != baseMessage.PK && x.EM_MessageType == originalMessageType);
					}
				}
				result = originalMessageCached;
			}
			return result;
		}
		EDIMessage originalMessageCached;
		protected virtual ZString OriginalMessageType => ZString.Empty;

		EDIMessage GetLastTransmitMessage(IMessageAttachee messageAttacheeBO)
		{
			EDIMessage result = null;
			if (messageAttacheeBO != null)
			{
				if (lastTransmitMessageCached == null || lastTransmitMessageCached.EM_LinkUniqueID != messageAttacheeBO.PK)
				{
					lastTransmitMessageCached = messageAttacheeBO.Messages.OfType<EDIMessage>().Where(x => x.IsTransmitMessage && x.EM_SystemCreateUser != User.ServiceUserCode && (LastTransmitMessageType.IsEmpty || x.EM_MessageType == LastTransmitMessageType)).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
				}
				result = lastTransmitMessageCached;
			}
			return result;
		}
		EDIMessage lastTransmitMessageCached;
		protected virtual ZString LastTransmitMessageType => ZString.Empty;

		protected abstract string MailSubject(IMessageAttachee messageAttacheeBO, TEDIMessage message);

		ZString GetEmailBody(IMessageAttachee messageAttacheeBO, TEDIMessage message, bool isSuccess)
		{
			return CreateEmailBodyCore(messageAttacheeBO, message, isSuccess);
		}
		protected abstract ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, TEDIMessage message, bool isSuccess);

		IGlbBranch MessageBranch
		{
			get { return messageBranch; }
			set { messageBranch = value; }
		}
		IGlbBranch messageBranch;

		protected HtmlTableCreator MessageDetailHtmlTableCreator { get; set; }

		protected IEnumerable<HtmlTableCreator> MessageDetailHtmlTableCreators { get; set; } = Enumerable.Empty<HtmlTableCreator>();

		protected string CreateInterpretationContent(string messageTitle)
		{
			var htmlBody = GetMessageDetailHtmlTableText();
			return MessageInterpretationGenerator.FormatOutputInTemplate(messageTitle, htmlBody, true);
		}

		ZString GetMessageDetailHtmlTableText()
		{
			var builder = new ZStringBuilder(
				MessageDetailHtmlTableCreators
				.Union(new[] { MessageDetailHtmlTableCreator })
				.WhereNotNull()
				.Distinct()
				.Select(table => table.ToHtml())
			);

			return builder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		#endregion
	}
}
