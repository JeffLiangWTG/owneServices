using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageBuilders;
using ECBM = Enterprise.Customs.Business.MessageManagers;

namespace Enterprise.Customs.ZA.Business.MessageManagers
{
	public class CUSDECMessageManager : ECBM.EDIFACTMessageManager
	{
		public CUSDECMessageManager(MessageSendingObject source, IMessageNotificationCollector notification) : base(source.Header, source.Header?.MessageStatusCalculator, notification)
		{
			this.CUSDECMessageDataProvider = source;
		}

		ZACusPermitCusDecProcessor PermitProcessor => permitProcessor ?? (permitProcessor = new ZACusPermitCusDecProcessor(EntryHeader, CUSDECMessageDataProvider.MessageType));

		ZACusPermitCusDecProcessor permitProcessor;

		public readonly MessageSendingObject CUSDECMessageDataProvider;
		public CusEntryHeader EntryHeader => (CusEntryHeader)DataWrapper;
		public JobDeclaration Declaration => EntryHeader.Declaration;

		public MessageSubTypes ActionCode => MessageSubTypeCodes.TranslateToMessageSubType(CUSDECMessageDataProvider.MessageType);

		public bool IsTestMessage => ShouldSendMessagesInTestMode;

		#region Override Properties

		public override string MessageFriendlyName => "CUSDEC";

		protected override bool ShouldSendMessagesInTestMode
		{
			get { return Env.Registry.ZACustoms.GetIsTestMode(Declaration.Branch); }
		}

		#endregion

		#region Override Methods

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			return new CUSDECMessageBuilder(CUSDECMessageDataProvider, actionCode);
		}

		protected override void OnMessageQueuedForSending(MessageSubTypes actionCode)
		{
			base.OnMessageQueuedForSending(actionCode);
			EntryHeader.PopulateEntrySubmittedDateIfRequired();
			Declaration.LogCustomsCommencedIfNeeded();

			EntryHeader.BackPopulateInvoiceLineTargetEntryLineNumberIfNeeded();
			EntryHeader.UpdateLRNIfNeeded(CUSDECMessageDataProvider.LocalReferenceNumber, CUSDECMessageDataProvider.IsLRNEditable);
		}

		#region Pre-Send Action

		protected override bool DefineActionCodeIfUndefined(ref MessageSubTypes actionCode)
		{
			return true;
		}

		protected override bool RunRationalityCheckAndAskForConfirmation(MessageSubTypes actionCode, bool runPreSaveValidation, out bool sendWithMessageErrors)
		{
			sendWithMessageErrors = false;
			return true;
		}

		#endregion

		new IMessageNotificationCollector notification => base.notification as IMessageNotificationCollector;

		protected override void ShowMessageNotSent(MessageSubTypes actionCodeToSend, string messageText)
		{
			var message = Res.GetString("0C7BBDFE-3A83-4A0F-AE36-8A71D3BCC903", "Can not send {0} for {1} due to error:\r\n{2}", GetActionCodeDescription(actionCodeToSend), CUSDECMessageDataProvider.LocalReferenceNumber, messageText);
			notification.Notifications.AddError(message);
		}

		protected override void ShowQueuedForSending(MessageSubTypes actionCodeToSend)
		{
			var message = Res.GetString("28CEB8B9-3D8A-4903-B7E7-34E55D6019E6", "{0} {1} message for {2} queued for sending", GetActionCodeDescription(actionCodeToSend), MessageFriendlyName, CUSDECMessageDataProvider.LocalReferenceNumber);
			notification.Notifications.AddInformation(message);
		}

		#endregion

		#region Permit Transactions

		protected override void RunAdditionalEDIMessageModification(IEnumerable<EDIMessage> messages)
		{
			base.RunAdditionalEDIMessageModification(messages);

			var message = messages.FirstOrDefault();
			if (message != null)
			{
				PermitProcessor.AddPermitTransactions(message, (msg) => ZAPermitHelper.GetPermitAppIdForMessage(message));

				var submissionDate = CUSDECMessageDataProvider.SubmissionDate;
				if (submissionDate > ZDate.Today)
				{
					message.EM_HeldUntilDate = submissionDate;
				}
			}
		}

		protected override bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			base.CanSendThisMessage(actionCode, out messageText);
			messageText = PermitProcessor.PermitErrors(messageText);

			return string.IsNullOrEmpty(messageText);
		}

		#endregion

		#region Implementation

		internal bool SendMessage()
		{
			var result = false;
			try
			{
				PermitProcessor.AddPermitRecordsAndLockMutexIfNeeded();
				result = SendMessage(ActionCode);
			}
			finally
			{
				PermitProcessor.UnlockPermitMutexes();
			}
			return result;
		}

		#endregion
	}
}
