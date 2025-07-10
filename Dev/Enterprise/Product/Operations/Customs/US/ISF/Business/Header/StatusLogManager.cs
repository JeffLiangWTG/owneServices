using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.ISF.Business
{
	public class StatusLogManager
	{
		public StatusLogManager(Logs logs)
		{
			if (logs == null)
			{
				throw new ArgumentNullException(nameof(logs));
			}

			this.logs = logs;
		}

		readonly Logs logs;

		#region Add a log

		public StmALog AddALogIfNecessary(ZString oldStatus, ZString newStatus)
		{
			StmALog result = null;

			if (!newStatus.IsEmpty && newStatus != MessageStatusList.Codes.NotSentISF && oldStatus != newStatus)
			{
				if (IsStatusChangingFromNotClearToClear(oldStatus, newStatus))
				{
					result = AddAClearLogIfNecessary(oldStatus, newStatus);
				}
				else
				{
					result = MessageStatusChangeEvent.AddNew(newStatus);
				}
			}

			return result;
		}

		public StmALog AddAClearLogIfNecessary(ZString oldStatus, ZString newStatus)
		{
			StmALog result = null;
			if (IsStatusChangingFromNotClearToClear(oldStatus, newStatus))
			{
				if (MessageStatusList.IsDeleteStatus(newStatus))
				{
					MessageStatusChangeEvent.CancelAll();

					result = MessageCancelHeaderEvent.AddNew(newStatus);
				}
				else
				{
					MessageCancelHeaderEvent.CancelAll();

					result = MessageStatusChangeEvent.AddNew(newStatus);
				}
			}
			return result;
		}

		#endregion

		public StmALog GetLatestLog()
		{
			StmALog result = null;
			if (MessageStatusChangeEvent.Count > 0)
			{
				result = MessageStatusChangeEvent[0];
			}
			if (MessageCancelHeaderEvent.Count > 0)
			{
				if (result == null || MessageCancelHeaderEvent[0].SL_EventTime > result.SL_EventTime)
				{
					result = MessageCancelHeaderEvent[0];
				}
			}
			return result;
		}

		public void CancelAll()
		{
			MessageStatusChangeEvent.CancelAll();
			MessageCancelHeaderEvent.CancelAll();
		}

		#region Has a certain log

		public bool HasAClearLog()
		{
			return MessageStatusChangeEvent.DescriptionExists(MessageStatusList.Codes.ClearISFAdd);
		}

		public bool HasAWithdrawnLog
		{
			get { return MessageCancelHeaderEvent.Count > 0; }
		}

		#endregion

		LogsForNominatedEvent MessageStatusChangeEvent
		{
			get { return fMessageStatusChange ?? (fMessageStatusChange = new LogsForNominatedEvent(logs, Events.MessageStatusChange)); }
		}
		LogsForNominatedEvent fMessageStatusChange;

		LogsForNominatedEvent MessageCancelHeaderEvent
		{
			get { return fMessageCancelHeader ?? (fMessageCancelHeader = new LogsForNominatedEvent(logs, Events.MessageCancelHeader)); }
		}
		LogsForNominatedEvent fMessageCancelHeader;

		bool IsStatusChangingFromNotClearToClear(ZString oldStatus, ZString newStatus)
		{
			return !MessageStatusList.IsStatusClear(oldStatus) && MessageStatusList.IsStatusClear(newStatus);
		}
	}
}
