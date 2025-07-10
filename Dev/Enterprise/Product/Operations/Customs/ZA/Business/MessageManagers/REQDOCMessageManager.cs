using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.ZA.Business.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageBuilders;
using ECBM = Enterprise.Customs.Business.MessageManagers;

namespace Enterprise.Customs.ZA.Business.MessageManagers
{
	public class REQDOCMessageManager : ECBM.EDIFACTMessageManager
	{
		public REQDOCMessageManager(MessageSendingObjectForREQDOC source, ECBM.IUserNotification notification) : base(source.Header, null, notification)
		{
			this.rEQDOCMessageDataProvider = source;
			this.messageCollectionProvider = source.Header;
			this.header = source.Header;
			this.factory = source.Factory;
		}

		public REQDOCMessageManager(STATACREQDOCSendingObject source, IMessageNotificationCollector notification) : this(source.Factory, source, source, notification)
		{
		}

		public REQDOCMessageManager(BusinessObjectFactory factory, IREQDOCMessageDataProvider dataProvider, IEDIFACTMessageAttachee edifactMessageProvider, IMessageNotificationCollector notification) : base(edifactMessageProvider, new EDIFACTStatusCalculator(SARSEDIMessage.MessageTypeNames.REQDOC), notification)
		{
			this.rEQDOCMessageDataProvider = dataProvider;
			this.factory = factory;
		}

		readonly IREQDOCMessageDataProvider rEQDOCMessageDataProvider;
		readonly BusinessObjectFactory factory;
		readonly IEDIMessageCollectionProvider messageCollectionProvider;
		readonly CusEntryHeader header;

		#region New Properties

		public bool IsTestMessage => ShouldSendMessagesInTestMode;

		public bool HasParentNotification => notification != null;

		#endregion

		#region Override Properties

		public override string MessageFriendlyName => rEQDOCMessageDataProvider.MessageType == MessageTypeList.Codes.StatementOfAccount ? "Customs Statement Request (REQDOC)" : SARSEDIMessage.MessageTypeNames.REQDOC;

		protected override bool ShouldSendMessagesInTestMode
		{
			get
			{
				var branch = rEQDOCMessageDataProvider.Branch;
				return branch == null || Env.Registry.ZACustoms.GetIsTestMode(branch);
			}
		}

		#endregion

		#region Override Methods

		protected override IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode)
		{
			return new REQDOCMessageBuilder(factory, messageCollectionProvider, rEQDOCMessageDataProvider, actionCode);
		}

		#region Pre-Send Action

		protected override bool DefineActionCodeIfUndefined(ref MessageSubTypes actionCode)
		{
			return true;
		}

		protected override bool RunRationalityCheckAndAskForConfirmation(MessageSubTypes actionCode, bool runPreSaveValidation, out bool sendWithMessageErrors)
		{
			var result = false;
			if (HasParentNotification)
			{
				sendWithMessageErrors = false;
				result = true;
			}
			else
			{
				result = base.RunRationalityCheckAndAskForConfirmation(actionCode, runPreSaveValidation, out sendWithMessageErrors);
			}
			return result;
		}

		protected override bool ShouldJobBeSavedBeforeSendingMessage
		{
			get { return rEQDOCMessageDataProvider.MessageType != MessageTypeList.Codes.StatementOfAccount; }
		}

		#endregion

		new IMessageNotificationCollector notification => base.notification as IMessageNotificationCollector;

		protected override void ShowMessageNotSent(MessageSubTypes actionCodeToSend, string messageText)
		{
			if (HasParentNotification)
			{
				var message = Res.GetString("00E68A79-92B6-4C4F-A89C-BF0E57061BB6", "Can not send {0} for {1} due to error:\r\n{2}", MessageFriendlyName, rEQDOCMessageDataProvider.FinancialAccountNumber + rEQDOCMessageDataProvider.FinalMRN, messageText);
				notification.Notifications.AddError(message);
			}
			else
			{
				base.ShowMessageNotSent(actionCodeToSend, messageText);
			}
		}

		protected override void ShowQueuedForSending(MessageSubTypes actionCodeToSend)
		{
			if (HasParentNotification)
			{
				var message = Res.GetString("A48CBC68-1C84-4067-8345-C36AF3C6FD5B", "{0} message for {1} queued for sending", MessageFriendlyName, rEQDOCMessageDataProvider.FinancialAccountNumber + rEQDOCMessageDataProvider.FinalMRN);
				notification.Notifications.AddInformation(message);
			}
			else
			{
				base.ShowQueuedForSending(actionCodeToSend);
			}
		}

		public override bool IsWaitingForResponse
		{
			get
			{
				var result = false;
				if (rEQDOCMessageDataProvider.DocumentMessageSource.IsEmpty)
				{
					var allMessages = header?.Messages;
					var lastCUSDECMessage = allMessages?.GetLastMessage(EDIMessage.ApplicationCodes.SouthAfricanCustoms, SARSEDIMessage.MessageTypes.CUSDEC, EDIMessage.Direction.Transmit) as CUSDECEDIMessage;
					if (lastCUSDECMessage != null)
					{
						var msgSentTime = lastCUSDECMessage.EM_SystemCreateTimeUtc;
						var msgConversationGroup = lastCUSDECMessage.EM_MessageNum;
						var msgConversationClosed = allMessages.OfType<ZAMessage>().Where(msg =>
						{
							return msg.EM_SystemCreateTimeUtc > msgSentTime && (msg is CUSRESEDIMessage || msg is CONTRLEDIMessage) && msg.ParentMessageNumber == msgConversationGroup;
						}).Any(msg =>
						{
							return msg is CUSRESEDIMessage || (msg as CONTRLEDIMessage).IsRejectionMessage;
						});
						result = !msgConversationClosed;
					}
				}
				return result;
			}
		}

		protected override string AwaitingCustomsResponseMessage
		{
			get
			{
				return Res.GetString("EC9DE465-6975-45FD-85B2-0286B8137E7F", @"This job is waiting for a response from Customs.
Would you like to request Customs to resend the latest response that they have on their system for this entry?
If Customs have not issued a response on their system then this request will not receive a response from Customs.");
			}
		}

		#endregion

		#region Implementation

		public bool Send()
		{
			return SendMessage(MessageSubTypeCodes.TranslateToMessageSubType(MessageSubTypeCodes.Codes.Original), false);
		}

		#endregion
	}
}
