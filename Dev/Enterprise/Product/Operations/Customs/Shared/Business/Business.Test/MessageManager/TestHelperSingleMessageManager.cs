using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class TestHelperSingleMessageManager : SingleMessageManager
	{
		public TestHelperSingleMessageManager() : this(new BusinessObjectFactory().New<DummyBusinessObject>(), "Name")
		{
		}

		public TestHelperSingleMessageManager(DummyBusinessObject dummyBusinessObject, string messageFriendlyName)
		{
			this.DummyBusinessObject = dummyBusinessObject;
			this.messageFriendlyName = messageFriendlyName;
		}

		public override BusinessObject BusinessObject
		{
			get { return DummyBusinessObject; }
		}

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			AmendmentGenerated = true;
			EDIMessage result = BusinessObject.Factory.New<EDIMessage>();
			return new EDIMessage[] { result };
		}

		public bool RequiresAmendmentCoreExposed;
		protected override bool RequiresAmendmentCore()
		{
			return RequiresAmendmentCoreExposed || base.RequiresAmendmentCore();
		}

		protected override void OnOriginalSentCore()
		{
			OriginalSent++;
		}
		public int OriginalSent;

		protected override void OnAmendmentSentCore()
		{
			AmendmentSent++;
		}
		public int AmendmentSent;

		protected override void OnWithdrawalSentCore()
		{
			WithdrawalSent++;
		}
		public int WithdrawalSent;

		protected override MessageSendingQueryCollection GetQueriesForSendingCore()
		{
			MessageSendingQueryCollection result = base.GetQueriesForSendingCore();
			MessageSendingQuery query = new MessageSendingQuery();
			query.Question = "Do you think cuckoo squeakers are awesome?";
			query.Caption = "Do you not want to delay the cuckoo squeaker?";
			query.Delegate = new MessageSendingQueryDelegate(QueryAction);
			result.Add(query);
			return result;
		}

		void QueryAction(bool cuckooSqueakersAreAwesome)
		{
			QueryActionWasHit = cuckooSqueakersAreAwesome;
		}

		public bool QueryActionWasHit
		{
			get { return fQueryActionWasHit; }
			set { fQueryActionWasHit = value; }
		}
		bool fQueryActionWasHit;

		protected override bool GetPreventSendCore
		{
			get { return fPreventSendTestBool; }
		}

		public bool PreventSendTestBool
		{
			get { return fPreventSendTestBool; }
			set { fPreventSendTestBool = value; }
		}
		bool fPreventSendTestBool;

		protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			ArrayList result = new ArrayList();
			for (int i = 0; i < numberOfOriginalMessagesToReturn; i++)
			{
				EDIMessage message = bizo.Factory.New<EDIMessage>();
				if (ReturnDifferentOriginalMessages)
				{
					message.EM_MessageText = Guid.NewGuid().ToString();
				}

				result.Add(message);
			}
			if (ReturnDifferentNumberOfMessages)
			{
				numberOfOriginalMessagesToReturn++;
			}
			return (EDIMessage[])result.ToArray(typeof(EDIMessage));
		}

		protected override EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo)
		{
			WithdrawlGenerated = true;
			EDIMessage result = BusinessObject.Factory.New<EDIMessage>();
			return new EDIMessage[] { result };
		}

		public bool canSendOriginal;
		public override bool CanSendOriginal
		{
			get { return canSendOriginal; }
		}

		public bool canSendWithdrawal;
		public override bool CanSendWithdrawal
		{
			get
			{
				return canSendWithdrawal;
			}
		}

		public bool shouldSendOriginalOnSave;
		public override bool ShouldSendOriginalOnSave
		{
			get { return shouldSendOriginalOnSave; }
		}

		public bool shouldSendWithdrawalOnSave;
		public override bool ShouldSendWithdrawalOnSave
		{
			get { return shouldSendWithdrawalOnSave; }
		}

		public bool IsWaitingForResponseExposed;
		public override bool IsWaitingForResponse
		{
			get { return IsWaitingForResponseExposed; }
		}

		public bool shouldSendMessagesInTestMode;
		protected override bool ShouldSendMessagesInTestMode
		{
			get { return shouldSendMessagesInTestMode; }
		}

		readonly string messageFriendlyName;
		public override string MessageFriendlyName
		{
			get
			{
				return messageFriendlyName;
			}
		}

		public DummyBusinessObject DummyBusinessObject;

		public void AddError()
		{
			DummyBusinessObject.Z0_AnotherDate = ZDateTime.Invalid;
		}

		public void AddMessageError()
		{
			using (IDisposable iAmAFaker = DummyBusinessObject.SuspendValidationTesting())
			{
				DummyBusinessObject.Z0_AnotherDateInfo.AddMessageError("message");
			}
		}

		public bool ReturnDifferentNumberOfMessages;
		public bool ReturnDifferentOriginalMessages;
		int numberOfOriginalMessagesToReturn = 1;

		protected internal override MessageSendingNotificationCollection GetCommonNotificationsForSending()
		{
			MessageSendingNotificationCollection result = base.GetCommonNotificationsForSending();
			result.AddRange(AdditionalCommonNotifications);
			return result;
		}

		public override MessageSendingNotificationCollection GetNotificationsForSendingAnOriginal()
		{
			MessageSendingNotificationCollection result = base.GetNotificationsForSendingAnOriginal();
			result.AddRange(AdditionalOriginalNotifications);
			return result;
		}

		public override MessageSendingNotificationCollection GetNotificationsForSendingAReplacement()
		{
			MessageSendingNotificationCollection result = base.GetNotificationsForSendingAReplacement();
			result.AddRange(AdditionalAmendmentNotifications);
			return result;
		}

		public override MessageSendingNotificationCollection GetNotificationsForSendingAWithdrawal()
		{
			MessageSendingNotificationCollection result = base.GetNotificationsForSendingAWithdrawal();
			result.AddRange(AdditionalWithdrawalNotifications);
			return result;
		}

		protected override void ResetToOriginalCore()
		{
			ResetToOriginalCalled = true;
		}

		public bool ShouldSendSilentExceptionOverriden;
		public bool ShouldSendSilentExceptionExposed;
		protected override bool ShouldSendDeveloperExceptionForFalsePositiveCore(ZString factoryMessages, ZString databaseMessages)
		{
			if (ShouldSendSilentExceptionOverriden)
			{
				return ShouldSendSilentExceptionExposed;
			}
			return base.ShouldSendDeveloperExceptionForFalsePositiveCore(factoryMessages, databaseMessages);
		}

		public ZString DatabaseMessagesExposed;
		public override ZString DatabaseMessages
		{
			get { return DatabaseMessagesExposed.IsEmpty ? base.DatabaseMessages : DatabaseMessagesExposed; }
		}

		public ZString FactoryMessagesExposed;
		public override ZString FactoryMessages
		{
			get { return FactoryMessagesExposed.IsEmpty ? base.FactoryMessages : FactoryMessagesExposed; }
		}

		public bool ResetToOriginalCalled;
		public bool WithdrawlGenerated;
		public bool AmendmentGenerated;

		public MessageSendingNotificationCollection AdditionalOriginalNotifications = new MessageSendingNotificationCollection();
		public MessageSendingNotificationCollection AdditionalAmendmentNotifications = new MessageSendingNotificationCollection();
		public MessageSendingNotificationCollection AdditionalWithdrawalNotifications = new MessageSendingNotificationCollection();
		public MessageSendingNotificationCollection AdditionalCommonNotifications = new MessageSendingNotificationCollection();
	}
}
