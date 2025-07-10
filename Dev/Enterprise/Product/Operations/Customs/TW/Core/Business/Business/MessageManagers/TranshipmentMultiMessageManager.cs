using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business.N5301;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TW.Business.MessageManagers
{
	public class TranshipmentMultiMessageManager : MultiMessageManager
	{
		#region Constructor

		public TranshipmentMultiMessageManager(TranshipmentMessageSendingObjectParent transhipmentWrapper, ZString messageType, IMessageNotificationCollector notification) : base()
		{
			this.transhipmentWrapper = Argument.NotNull(transhipmentWrapper, "transhipmentWrapper");
			this.notification = Argument.NotNull(notification, "notification");
			this.messageType = messageType;
		}

		readonly ZString messageType;

		readonly TranshipmentMessageSendingObjectParent transhipmentWrapper;

		readonly IMessageNotificationCollector notification;

		protected override bool SendWheneverPossibleOnceMessagingActive => true;

		public override IMessageManageableBizObj TopLevelBizObjToManage => transhipmentWrapper.Header;
		#endregion

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			var result = new List<SingleMessageManager>();
			switch (messageType)
			{
				case MessageTypeList.Codes.TRA:
					foreach (var sendingObject in transhipmentWrapper.SendingObjectsCollection.Cast<N5301MessageSendingObject>().Where(x => x.ShouldSend))
					{
						result.Add(new TransferApplicationMessageManager(sendingObject));
					}
					break;
			}
			return result.ToArray();
		}

		#region Implementation
		public bool SendMessages(ISendsMessagesToCustoms sender)
		{
			if (CanSendMessages())
			{
				var messages = SendMessagesWithoutSaving(sender);
				if (messages.Any())
				{
					using (transhipmentWrapper.Header.GetGenerateEntryNumberExceptionSupporter())
					{
						try
						{
							try
							{
								transhipmentWrapper.Factory.Save();
							}
							catch (Exception ex) when (!ex.IsCriticalException())
							{
								messages.ForEach(f => f.Delete());
								throw;
							}
						}
						catch (GenerateEntryNumberException ex)
						{
							AllMessageManagers.Select(x => x.BusinessObject as N5301MessageSendingObject).Where(x => x.ShouldSend).ForEach(x => x.Header.ReloadSafe());
							notification.ShowError(ex.Message, ResString.GetMultilingualString("4AD5381F-C2A1-462C-A725-B226C185FB96", "Cannot Send Message"));
							return false;
						}
						catch (ZSaveException ex)
						{
							ZExceptionReporting.HandleSaveException(ex);
						}
					}
				}
			}
			return true;
		}

		internal IList<EDIMessage> SendMessagesWithoutSaving(ISendsMessagesToCustoms sender)
		{
			Initialise();
			var singleMessageManagers = AllMessageManagers;
			return singleMessageManagers.Any() ? SendOriginal(sender, singleMessageManagers) : Array.Empty<EDIMessage>();
		}

		bool CanSendMessages()
		{
			var result = true;
			var notifications = notification?.Notifications;
			if (notifications != null)
			{
				CollectNotificationsFromMessageManagers(notifications);
				if (notifications.ContainsWarning())
				{
					result = notification.ShowConfirmation(Res.GetString("41D7A554-5B32-4003-8AB6-27CDC3732FA8", "{0}\r\nAre you sure you want to continue with sending?", notifications.WarningNotificationsAsString()),
						Res.GetString("3EFF3BB2-0E76-4CC8-A601-195CA2BC3E2E", "Continue send with warning?"));
				}

				notifications.Clear();
			}

			return result;
		}

		void CollectNotificationsFromMessageManagers(MessageSendingNotificationCollection notifications)
		{
			var allmanagers = GetAllMessageManagers();
			if (allmanagers.Any(x => x.IsWaitingForResponse))
			{
				notifications.AddWarning(Res.GetString("0FBE06F0-C63C-44D0-82D9-CD64491DAA2B", "This transhipment is waiting for a Customs response."));
			}
		}
		#endregion
	}
}
