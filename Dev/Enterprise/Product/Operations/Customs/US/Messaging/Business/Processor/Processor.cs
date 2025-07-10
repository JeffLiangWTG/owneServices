using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
#if NETFRAMEWORK
using System.Web.UI;
#endif
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.Messaging.Business
{
	public interface IProcessor
	{
		bool CanAcceptBlock(MessageBlock messageBlock);
		void AddMessageBlock(MessageBlock messageBlock);
		void SetBAndYBlock(IControlMessageBlockB b, IControlMessageBlockY y);
		void SetLogger(LoggingInformation logger);
		void Process();
		CBPEDIMessage Message { get; set; }
		BusinessObjectFactory Factory { get; set; }
	}

	public interface IDatabaseLockProcessor : IProcessor
	{
		string GetLockType();
	}

	[WTG.StaticAnalysis.Annotation.CodeAlive("Instantiated through reflection")]
	public abstract class Processor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : IProcessor
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		#region Constants

		public static class Constants
		{
			public const string TransactionDataRejected = "524";
			public const string BrokerNotAuthorized = "ABX";
			public const string BlockRejectedDueToErrors = "565";

			public const string AcceptedWithWarnings = "584";
			public const string AcceptedWithWarningsPaperReqd = "58L";
			public const string AcceptedWithWarningsNoPaperReqd = "58M";
			public const string AcceptedWithWarningsTSUSAEffectIn3Weeks = "41D";

			public const string RequestFullAII = "Full Electronic Invoice data is being requested by Customs.";
			public const string AII48Hours = "Electronic Invoice must be sent within 48 hours of the request.";
			public const string RequestAII = "Customs may request Electronic Invoices.";
			public const string RequestAIIForLegacyEntry = "Disposition message 'Override To Intensive' has been received for an entry that may have been lodged from a legacy system. Please check if Cargo Release Processing Results with a disposition message 'Paperless Entry' has been received in a legacy system. If such entry has been received, Customs may request Electronic Invoices.";

			public static class MessageStatusChangeLogReference
			{
				public const string BIRD_JR = "JR:BR";
			}

			public static class EntrySummaryQuery
			{
				public const string BillDataStatus = "BILLING DATA NOT ON FILE";
				public const string CollectionDataStatus = "COLLECTION DATA NOT ON FILE";
			}
		}

		#endregion

		protected Processor()
		{
			object[] topLevelAttributes = GetType().GetCustomAttributes(typeof(TopLevelAttribute), true);
			if (topLevelAttributes.Length > 0)
			{
				topLevelAttribute = (TopLevelAttribute)topLevelAttributes[0];
			}

			messageBlocks = new List<MessageBlock>();
			if (RequiresSeparateFactory)
			{
				Factory = new BusinessObjectFactory();
				Factory.RefreshEnabled = false;
			}
		}

		public CBPEDIMessage Message
		{
			get { return message; }
			set
			{
				message = value;
				if (message != null)
				{
					if (message.OriginalMessage is CBPEDIMessage originalMessage)
					{
						if (message.EM_GB != originalMessage.EM_GB)
						{
							message.EM_GB = originalMessage.EM_GB;
						}
						if (originalMessage.EM_LinkedObject is BusinessObject linkedObject && message.EM_LinkedObject != linkedObject)
						{
							message.EM_LinkedObject = linkedObject;
						}
						message.EM_MessageSubType = originalMessage.EM_MessageSubType;
					}

					if (!RequiresSeparateFactory)
					{
						Factory = message.Factory;
					}
				}
			}
		}

		public virtual bool RequiresSeparateFactory
		{
			get { return false; }
		}

		public virtual int MaximumTopLevelMessageBlocksPerProcessor
		{
			get { return 0; }
		}

		public abstract void Process();

		int topLevelBlockCount;
		public bool CanAcceptBlock(MessageBlock messageBlock)
		{
			if (messageBlock == null)
			{
				throw new ArgumentNullException(nameof(messageBlock));
			}

			bool result;

			if (topLevelAttribute == null)
			{
				result = true;
			}
			else
			{
				result = topLevelAttribute.MessageBlockType == messageBlock.GetType();
				if (result)
				{
					topLevelBlockCount++;
					if (topLevelBlockCount >= MaximumTopLevelMessageBlocksPerProcessor && MaximumTopLevelMessageBlocksPerProcessor > 0)
					{
						result = false;
					}
				}
				else
				{
					result = ((IList)topLevelAttribute.PossibleChildren).IndexOf(messageBlock.GetType()) > -1;
				}
			}

			return result;
		}

		#region Send emails to the original sender

		protected void SendEmailToOriginalSenderOrGroupIfSenderInvalid(EmailDef email, bool shouldAttachMessageText, IGlbBranch branch, bool isFailure)
		{
			SendEmailToOriginalSenderOrGroupIfSenderInvalid(email, shouldAttachMessageText, branch, GetEmailAddressToSendTo(), isFailure);
		}

		protected void SendEmailToOriginalSenderOrGroupIfSenderInvalid(EmailDef email, bool shouldAttachMessageText, IGlbBranch branch, ZString emailAddressToSendTo, bool isFailure)
		{
			if (email != null)
			{
				if (shouldAttachMessageText)
				{
					if (Message != null)
					{
						email.Attachments.Add(new AttachmentDef("IncomingMessage.edi", Encoding.ASCII.GetBytes(Message.EM_FormattedMessageText)));
					}

					if (Message.OriginalMessage != null)
					{
						email.Attachments.Add(new AttachmentDef("OutgoingMessage.edi", Encoding.ASCII.GetBytes(Message.OriginalMessage.EM_FormattedMessageText)));
					}
				}

				if (branch == null)
				{
					branch = GlbBranch.CurrentBranch;
				}

				var emailRepCal = new EmailRecipientCalculator(
					GetEmailSendMode(branch), GetEmailGroupPK(GetEmailGroupRegistryItem(), branch),
					emailAddressToSendTo, GetEmailGroupPK(GetAlternativeEmailGroupRegistryItemWhenNoRecipientFound(), branch));

				var registryItem = !emailRepCal.EmailRedirected ? GetEmailGroupRegistryItem() : GetAlternativeEmailGroupRegistryItemWhenNoRecipientFound();
				var supportMessageSuppressRegistry = registryItem as ISupportMessageSuppressRegistry;

				var shouldSendErrorEmailsOnly = supportMessageSuppressRegistry?.ShouldSEndErrorsOnly(branch.GB_GC, branch.PK, ZGuid.Empty) ?? false;
				if (!shouldSendErrorEmailsOnly || (shouldSendErrorEmailsOnly && isFailure))
				{
					emailRepCal.SendNotifications(Factory, email, registryItem);
				}
			}
		}

		protected virtual ZString GetEmailAddressToSendTo()
		{
			ZString result = ZString.Empty;
			if (Message != null)
			{
				CBPEDIMessage originalMessage = Message.OriginalMessage;
				GlbStaff originalSender = originalMessage != null ? originalMessage.UserWhoQueuedThisRecord : null;
				if (originalSender != null)
				{
					result = originalSender.GS_EmailAddress;
				}

				if (result.IsEmpty)
				{
					IMessageResponseNotificator notificator = Message.EM_LinkedObject as IMessageResponseNotificator;
					if (notificator != null)
					{
						result = notificator.GetFallbackEmailAddressRecipient();
					}
				}
			}

			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1021: AvoidOutParameters")]
		protected bool GenerateHtmlEmail(string subject, string header, string description, string bodyDetails, string footerDetails, out EmailDef email, IGlbBranch branchForEmailLogo)
		{
			return new HtmlResponseEmailGenerator().TryGenerateEmail(subject, header, description, bodyDetails, footerDetails, out email, branchForEmailLogo);
		}

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "We are using URL as string on the rest of the implementation.")]
		protected virtual void GenerateHtmlEmailAndSendToOriginalOrGroup(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo, BusinessObject sourceBusinessObject, bool hasWarning = false, string warningMessage = "")
		{
			EmailDef email;
			new HtmlResponseEmailGenerator().TryGenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, out email, branchForEmailLogo, hasWarning, warningMessage);
			email.SetupBusinessEntityInfo(sourceBusinessObject);
			SendEmailToOriginalSenderOrGroupIfSenderInvalid(email, false, branchForEmailLogo, (isFailure | hasWarning));
		}

		protected ZGuid GetEmailGroupPK(Integration.IRegistryItem registryItem, IGlbBranch branch)
		{
			var result = ZGuid.Empty;
			if (registryItem != null)
			{
				var emailGroup = registryItem.GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty);
				if (emailGroup is GroupNotification groupNotification)
				{
					result = groupNotification.SendGroupPK;
				}
				else if (emailGroup is Guid)
				{
					result = (Guid)emailGroup;
				}
			}
			return result;
		}

		protected abstract Integration.IRegistryItem GetEmailGroupRegistryItem();

		protected virtual ZString GetEmailSendMode(IGlbBranch branch)
		{
			var result = GroupNotification.StaffMemberOrNominatedGroup;
			var groupNotificationRegistry = GetEmailGroupRegistryItem().GetFallBackValueAtAllLevels(branch.GB_GC.ToGuid(), branch.PK.ToGuid(), Guid.Empty) as GroupNotification;
			if (groupNotificationRegistry != null)
			{
				result = groupNotificationRegistry.SendMode;
			}
			return result;
		}

		protected virtual Integration.IRegistryItem GetAlternativeEmailGroupRegistryItemWhenNoRecipientFound()
		{
			return null;
		}

		#endregion

		#region Implementation

		protected EmailDef CreateEmail(ZString subject, ZString htmlBody)
		{
			HtmlNotificationEmailSender emailSender = new HtmlNotificationEmailSender();
			EmailDef email = null;
			try
			{
				email = emailSender.CreateEmail(subject, htmlBody);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ErrorReporter.ReportOnce(subject, "Error creating email", e);
			}
			return email;
		}

		protected ControlMessageBlockA A
		{
			get { return a ?? (a = GetControlMessageBlockAFromMessage()); }
		}
		ControlMessageBlockA a;

		ControlMessageBlockA GetControlMessageBlockAFromMessage()
		{
			ControlMessageBlockA result = null;
			CBPEDIInterchange interchange = Message.Interchange as CBPEDIInterchange;
			if (interchange != null)
			{
				result = new ControlMessageBlockA();
				result.Deserialise(interchange.EI_HeaderText.PadRight(80));
			}

			return result;
		}

		protected void LogMessageStatusChangeEventAgainstTopLevelBusinessObject(IMessageAttachee messageAttachee)
		{
			LogMessageStatusChangeEventAgainstTopLevelBusinessObject(messageAttachee, Message.EM_MessageType);
		}

		protected void LogMessageStatusChangeEventAgainstTopLevelBusinessObject(IMessageAttachee messageAttachee, string reference)
		{
			if (messageAttachee != null && Message != null)
			{
				Logs logs = messageAttachee.TopLevelBusinessObjectLogs;
				if (logs != null)
				{
					logs.AddNew(Events.MessageStatusChange, reference, DateTimeParser.GetFromJobBranchCurrentTime(messageAttachee.Branch), null);
				}
			}
		}

		protected void ReportAbnormalityInResponseMessageIfNeeded(IMessageAttachee messageAttachee, CBPEDIMessage message, bool isFailure, List<ZString> errorCodes)
		{
			if (messageAttachee != null)
			{
				if (isFailure)
				{
					if (errorCodes == null || errorCodes.Count == 0 || ContainsANonExcludedCode(errorCodes))
					{
						MessageCalculator.ReportAbnormalityInResponseMessageIfNeeded(messageAttachee, message, true);
					}
				}
				else
				{
					MessageCalculator.ReportAbnormalityInResponseMessageIfNeeded(messageAttachee, message, false);
				}
			}
		}

		bool ContainsANonExcludedCode(List<ZString> errorCodes)
		{
			foreach (ZString errorCode in errorCodes)
			{
				if (!errorCode.IsEmpty && !ExcludedErrorCodes.Contains(errorCode))
				{
					return true;
				}
			}

			return false;
		}

		protected virtual List<ZString> ExcludedErrorCodes
		{
			get
			{
				if (excludedErrorCodes == null)
				{
					excludedErrorCodes = new List<ZString>();
					excludedErrorCodes.Add(Constants.TransactionDataRejected);
					excludedErrorCodes.Add("79I");
					excludedErrorCodes.Add("484");// Ultimate Consignee Not on file (Can never validate this)
					excludedErrorCodes.Add("4DA");// Importer Not On File
					excludedErrorCodes.Add("34D");// Cannot delete an invoice while on a final statement(This wont be a normal validation as system validates only when Delete Invoice is selected)
					excludedErrorCodes.Add("AGU");// Cannot replace an invoice after cargo is certified
					excludedErrorCodes.Add("EJQ");// Cannot replace an invoice after appears on a statement
				}

				return excludedErrorCodes;
			}
		}
		List<ZString> excludedErrorCodes;

		protected void ReportUnexpected(MessageBlock block)
		{
			string processorFullname = GetType().FullName;
			string blockFullname = block.GetType().FullName;
			ErrorReporter.ReportOnce(processorFullname + blockFullname, string.Format("Unexpected message block '{0}' found in message processor '{1}'.", blockFullname, processorFullname));
		}

		protected MessageErrorCalculator MessageCalculator
		{
			get { return messageCalculator ?? (messageCalculator = GetNewMessageCalculator()); }
		}
		MessageErrorCalculator messageCalculator;

		protected virtual MessageErrorCalculator GetNewMessageCalculator()
		{
			return new MessageErrorCalculator(Factory);
		}

		protected virtual bool IsRejectedCode(string code)
		{
			return code == Constants.TransactionDataRejected
				|| code == Constants.BrokerNotAuthorized
				|| code == Constants.BlockRejectedDueToErrors;
		}

		protected virtual bool IsWarningCode(string code)
		{
			return code == Constants.AcceptedWithWarnings
				|| code == Constants.AcceptedWithWarningsNoPaperReqd
				|| code == Constants.AcceptedWithWarningsPaperReqd
				|| code == Constants.AcceptedWithWarningsTSUSAEffectIn3Weeks;
		}

		protected void CheckNot(Type type, MessageBlock messageBlock)
		{
			if (messageBlock.GetType() == type)
			{
				throw new InvalidMessageFormatException("Duplicate segment found : " + type.Name);
			}
		}

		protected List<MessageBlock> messageBlocks;
		CBPEDIMessage message;

		protected ControlMessageBlockB B
		{
			get { return b ?? (b = new ControlMessageBlockB()); }
		}
		ControlMessageBlockB b;

		protected ControlMessageBlockY Y
		{
			get { return y ?? (y = new ControlMessageBlockY()); }
		}
		ControlMessageBlockY y;

		readonly TopLevelAttribute topLevelAttribute;

		public BusinessObjectFactory Factory
		{
			get { return factory; }
			set { factory = value; }
		}
		BusinessObjectFactory factory;

		public void SetBAndYBlock(IControlMessageBlockB b, IControlMessageBlockY y)
		{
			this.b = (ControlMessageBlockB)b;
			this.y = (ControlMessageBlockY)y;
		}

		public void SetLogger(LoggingInformation logger)
		{
			this.logger = logger;
		}

		public LoggingInformation Logger
		{
			get { return logger; }
		}
		LoggingInformation logger;

		public void AddMessageBlock(MessageBlock messageBlock)
		{
			messageBlocks.Add(messageBlock);
		}

		/// <summary>
		/// Get the first message block matching the T type
		/// Keep in mind that there might be multiple blocks with the same T type
		/// </summary>
		protected T GetFirstMessageBlock<T>()
			where T : MessageBlock
		{
			foreach (var block in messageBlocks)
			{
				if (block is T)
				{
					return (T)block;
				}
			}

			return null;
		}

		protected T[] GetMessageBlocks<T>(int maxNumberAllowed)
			where T : MessageBlock
		{
			var result = new List<T>();
			foreach (var block in messageBlocks)
			{
				if (block is T)
				{
					result.Add((T)block);

					if (result.Count == maxNumberAllowed)
					{
						break;
					}
				}
			}

			return result.ToArray();
		}

		protected T GetLastMessageBlock<T>()
			where T : MessageBlock
		{
			T result = null;
			foreach (MessageBlock block in messageBlocks)
			{
				if (block is T)
				{
					result = (T)block;
				}
			}

			return result;
		}

#if NETFRAMEWORK
		protected void WriteRow(HtmlTextWriter htmlWriter, string description)
		{
			htmlWriter.WriteBeginTag("TR");
			htmlWriter.Write(HtmlTextWriter.TagRightChar);

			WriteTD(htmlWriter, description);

			htmlWriter.WriteEndTag("TR");
		}

		protected void WriteTD(HtmlTextWriter htmlWriter, string text)
		{
			htmlWriter.WriteBeginTag("TD");

			int throwawayResult;
			if (int.TryParse(text, out throwawayResult))
			{
				htmlWriter.WriteAttribute("style", "text-align: right");
			}

			htmlWriter.Write(HtmlTextWriter.TagRightChar);

			if (string.IsNullOrEmpty(text))
			{
				htmlWriter.Write("&nbsp;");
			}
			else
			{
				htmlWriter.WriteEncodedText(text);
			}

			htmlWriter.WriteEndTag("TD");
		}
#endif
#endregion
	}
}

#if DEBUG
namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.DummyForTesting1, CBPEDIInterchange.ApplicationCodeForTesting)]
	sealed class ProcessorTestClass : ProcessorTestClass<ZZZA, ZZZB, ZZZY>
	{
		public override void Process()
		{
			base.Process();

			Message.Factory.Saved -= SendEmail;
			Message.Factory.Saved += SendEmail;
		}

		void SendEmail(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (Message.EM_MessageText.Contains("JOEY"))
			{
				var registryItem = GetEmailGroupRegistryItem() as ManifestGroupNotificationRegistryItem;
				var groupNotification = registryItem.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
				var recipientCalculator = new EmailRecipientCalculator(groupNotification.SendMode, groupNotification.SendGroupPK, new ZString[] { "a@a.com" }, ZGuid.Empty);
				var email = new EmailDef();
				email.Subject = "Joey's Test";
				recipientCalculator.SendNotifications(Factory, email, registryItem);
			}
		}

		protected override Integration.IRegistryItem GetEmailGroupRegistryItem() => ObjectFactory.Get<Integration.Customs.US.IUSCustomsDataRegistry>().ABIMessagesGroup;
	}

	[ApplicationIdentifier(ApplicationIdentifierCodeList.DummyForTesting2, CBPEDIInterchange.ApplicationCodeForTesting)]
	[TopLevel(typeof(ZZZDForAll), typeof(ZZZZ))]
	sealed class ProcessorTestClass2 : ProcessorTestClass<ZZZA, ZZZB2, ZZZY2>
	{
	}

	abstract class ProcessorTestClass<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY> : Processor<ControlMessageBlockA, ControlMessageBlockB, ControlMessageBlockY>
		where ControlMessageBlockA : MessageBlock, IControlMessageBlockA, new()
		where ControlMessageBlockB : MessageBlock, IControlMessageBlockB, new()
		where ControlMessageBlockY : MessageBlock, IControlMessageBlockY, new()
	{
		public override void Process()
		{
			LastMessageBlocks.AddRange(messageBlocks);
		}

		public static List<MessageBlock> LastMessageBlocks => lastMessageBlocks ?? (lastMessageBlocks = new List<MessageBlock>());

		[ThreadStatic]
		static List<MessageBlock> lastMessageBlocks;

		protected override Integration.IRegistryItem GetEmailGroupRegistryItem() => null;
	}
}
#endif
