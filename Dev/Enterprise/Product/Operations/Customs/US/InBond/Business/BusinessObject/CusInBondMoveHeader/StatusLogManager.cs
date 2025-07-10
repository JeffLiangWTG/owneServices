using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.InBond.Business
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

			if (!newStatus.IsEmpty && newStatus != ImportMessageStatusList.Codes.NotSent && oldStatus != newStatus)
			{
				if (IsStatusChangingFromNotClearToClear(oldStatus, newStatus))
				{
					result = AddAClearLogIfNecessary(oldStatus, newStatus);
				}
				else if (!IsAnAwaitingStatus(newStatus))
				{
					result = MessageStatusChangeEvents.AddNew(newStatus);
				}
			}

			return result;
		}

		public StmALog AddAClearLogIfNecessary(ZString oldStatus, ZString newStatus)
		{
			StmALog result = null;
			if (IsStatusChangingFromNotClearToClear(oldStatus, newStatus))
			{
				if (StatusList.IsWithdrawnStatus(newStatus))
				{
					MessageStatusChangeEvents.CancelAll();

					result = MessageCancelHeaderEvents.AddNew(newStatus);
				}
				else
				{
					MessageCancelHeaderEvents.CancelAll();

					result = MessageStatusChangeEvents.AddNew(newStatus);
				}
			}
			return result;
		}

		#endregion

		public StmALog GetLatestLog()
		{
			StmALog result = null;
			if (MessageStatusChangeEvents.Count > 0)
			{
				result = MessageStatusChangeEvents[0];
			}
			if (MessageCancelHeaderEvents.Count > 0)
			{
				if (result == null || MessageCancelHeaderEvents[0].SL_EventTime > result.SL_EventTime)
				{
					result = MessageCancelHeaderEvents[0];
				}
			}
			return result;
		}

		public void CancelAll()
		{
			MessageStatusChangeEvents.CancelAll();
			MessageCancelHeaderEvents.CancelAll();
		}

		public bool IsAnAwaitingStatus(params string[] statusCollect) => statusCollect.Any(StatusList.IsWaitingForResponse);

		public bool IsAcceptedByCustoms(params string[] statusCollect) => statusCollect.Any(StatusList.IsStatusClear);

		public bool IsWithdrawn(params string[] statusCollect) => statusCollect.Any(StatusList.IsWithdrawnStatus);

		#region Has a certain log

		public bool HasAClearLog
		{
			get
			{
				return MessageStatusChangeEvents.DescriptionExists(ImportMessageStatusList.Codes.ClearDepartureOriginal) ||
					MessageStatusChangeEvents.DescriptionExists(ImportMessageStatusList.Codes.ClearDeparturePartialOriginal);
			}
		}

		public bool HasAWithdrawnLog
		{
			get { return MessageCancelHeaderEvents.Count > 0; }
		}

		#endregion

		LogsForNominatedEvent MessageStatusChangeEvents
		{
			get { return fMessageStatusChange ?? (fMessageStatusChange = new LogsForNominatedEvent(logs, Events.MessageStatusChange)); }
		}
		LogsForNominatedEvent fMessageStatusChange;

		LogsForNominatedEvent MessageCancelHeaderEvents
		{
			get { return fMessageCancelHeader ?? (fMessageCancelHeader = new LogsForNominatedEvent(logs, Events.MessageCancelHeader)); }
		}
		LogsForNominatedEvent fMessageCancelHeader;

		bool IsStatusChangingFromNotClearToClear(ZString oldStatus, ZString newStatus)
		{
			return !StatusList.IsStatusClear(oldStatus) && StatusList.IsStatusClear(newStatus);
		}

		internal ImportMessageStatusList StatusList
		{
			get { return statusList ?? (statusList = logs.Factory.GetCachedValue<ImportMessageStatusList>()); }
		}
		ImportMessageStatusList statusList;
	}
}
