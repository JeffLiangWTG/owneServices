using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public interface IMessageSenderSupporter
	{
		ISendsMessagesToCustoms MessageInitiator { get; }
		bool HasChanges { get; }
		bool HasErrors { get; }
		BusinessObjectFactory Factory { get; }
		BusinessObject BusinessObjectForNotifications { get; }
	}

	public abstract class MessageSender
	{
		public enum MessageSendingStatus
		{
			SavingJob,
			PreparingJob,
			ValidateJob,
			GeneratingMessages,
			SavingMessages,
			Done
		}

		public MessageSendingStatus SendingStatus
		{
			get; private set;
		}

		public delegate void SaveEventHandler();
		public delegate bool AllowNotificationsEventHandler(MessageSendingNotificationCollection notifications);

		protected MessageSender(IMessageSenderSupporter job)
		{
			this.Job = job;
			this.MessageInitiator = Job.MessageInitiator;
		}

		public ISendsMessagesToCustoms MessageInitiator;
		public readonly IMessageSenderSupporter Job;
		public event SaveEventHandler OnSave;
		public event AllowNotificationsEventHandler OnAllowNotifications;

		protected virtual bool SaveJob()
		{
			if (Job == null)
			{
				return false;
			}

			if (Job.HasChanges)
			{
				if (MessageInitiator.YesNoQuery(Res.GetString("98eb2bbe-d01f-41aa-b8f6-d41cb3529ffc", "The Job has not yet been saved. Do you want to save and proceed?"), Res.GetString("840654b3-4e94-4ca9-9930-0964151af04a", "Save Job")))
				{
					FireOnSave();

					if (Job.HasChanges || Job.HasErrors)
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}

			return true;
		}

		protected void FireOnSave()
		{
			if (OnSave == null)
			{
				ErrorReporter.ReportOnce(GetType().FullName + ".OnSave", "OnSave has not been set.");
				Job.Factory.Save();
			}
			else
			{
				OnSave();
			}
		}

		protected abstract bool Prepare();

		protected virtual bool ValidateJob()
		{
			if (ShouldValidateJob)
			{
				if (OnAllowNotifications != null)
				{
					return OnAllowNotifications(Notifications);
				}

				return false;
			}

			return true;
		}

		protected abstract bool GenerateMessage();

		protected virtual string SuccessfulSendNotification
		{
			get { return string.Empty; }
		}

		protected virtual bool SaveMessage()
		{
			try
			{
				Job.Factory.Save();
				if (ShouldNotifyUserOfASuccessfulSend)
				{
					MessageInitiator.NotifyUserOfASuccessfulSend(SuccessfulSendNotification);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				ZExceptionReporting.HandleSaveException(e);
				return false;
			}

			return true;
		}

		protected virtual bool ShouldNotifyUserOfASuccessfulSend
		{
			get { return true; }
		}

		protected virtual bool ShouldValidateJob
		{
			get { return false; }
		}

		protected virtual MessageSendingNotificationCollection Notifications
		{
			get
			{
				if (notifications == null)
				{
					notifications = GetMessageSendingValidation(Job.BusinessObjectForNotifications, null).CheckBusinessObjectLevelValidation();
				}

				return notifications;
			}
		}
		MessageSendingNotificationCollection notifications;

		protected virtual MessageSendingValidation GetMessageSendingValidation(BusinessObject businessObjectForNotifications, IEnumerable<INotification> msgErrors)
		{
			return MessageSendingValidation.New(businessObjectForNotifications, msgErrors);
		}

		public bool SendMessage()
		{
			SendingStatus = MessageSendingStatus.SavingJob;
			if (!SaveJob())
			{
				return false;
			}

			SendingStatus = MessageSendingStatus.PreparingJob;
			if (!Prepare())
			{
				return false;
			}

			SendingStatus = MessageSendingStatus.ValidateJob;
			if (!ValidateJob())
			{
				return false;
			}

			SendingStatus = MessageSendingStatus.GeneratingMessages;
			if (!GenerateMessage())
			{
				return false;
			}

			SendingStatus = MessageSendingStatus.SavingMessages;
			if (!SaveMessage())
			{
				return false;
			}

			SendingStatus = MessageSendingStatus.Done;
			return true;
		}
	}
}
