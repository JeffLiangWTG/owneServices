using System;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business.MessageManagers
{
	public class DeclarationMessageManagerWrapper
	{
		public DeclarationMessageManagerWrapper(JobDeclaration declaration, JobDeclarationMessageSendingObjectParent wrapper, IMessageNotificationCollector notificationCollector, ISendsMessagesToCustoms sender, ZString messageType)
		{
			Wrapper = wrapper;
			NotificationCollector = notificationCollector;
			Sender = sender;
			MessageType = messageType;
			Declaration = declaration;
		}
		protected JobDeclarationMessageSendingObjectParent Wrapper { get; }
		protected IMessageNotificationCollector NotificationCollector { get; }
		protected ISendsMessagesToCustoms Sender { get; }
		protected ZString MessageType { get; }

		protected JobDeclaration Declaration { get; }

		protected bool ShouldCheckDaysDelayedDeclaration => MessageType != MessageTypeList.Codes.IEA && MessageType != MessageTypeList.Codes.ADM;

		#region MessageManager
		protected MessageManager MessageManager => messageManager ?? (messageManager = CreateMessageManagerCore());
		MessageManager messageManager;
		#endregion

		public void PerformFunctionOperationalAction(bool useDaysOfDelayed = true, Func<bool> showDialogFunc = null)
		{
			var result = BeforeSendMessage();
			if (result)
			{
				result = RunPreSendValidation();
			}
			if (result && showDialogFunc != null)
			{
				result = showDialogFunc.Invoke();
			}
			if (result && ShouldCheckDaysDelayedDeclaration)
			{
				result = MessageManager.CheckDaysDelayedDeclaration(useDaysOfDelayed ? DaysOfDelayAnswersList.Codes.Update : DaysOfDelayAnswersList.Codes.Continue);
			}
			if (result)
			{
				result = MessageManager.SendMessages(Sender);
			}
			AfterSendMessage(result);
		}

		protected virtual MessageManager CreateMessageManagerCore() => new(Wrapper, NotificationCollector);

		protected virtual bool RunPreSendValidation() => MessageManager.CheckEntries(Declaration, NotificationCollector) && MessageManager.CheckDeclarationNumber();

		protected virtual bool BeforeSendMessage() => true;

		protected virtual void AfterSendMessage(bool result)
		{
		}
	}
}
