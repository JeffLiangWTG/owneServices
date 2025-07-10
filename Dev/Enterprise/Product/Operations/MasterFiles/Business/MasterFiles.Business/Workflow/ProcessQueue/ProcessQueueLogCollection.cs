using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessQueueLogCollection : ProcessQueueLogCollectionBase
	{
		public ProcessQueueLogCollection(ProcessQueue processQueue, ProcessQueueType.Enum queueType) : base(processQueue)
		{
			this.QueueType = queueType;
		}

		public bool ContainsQueue(ZString queue)
		{
			foreach (ProcessQueueLog log in this)
			{
				if (log.Queue == queue)
				{
					return true;
				}
			}

			return false;
		}

		public bool ContainsQueue(ZString queue, ZString status, ZString subStatus)
		{
			foreach (ProcessQueueLog log in this)
			{
				if (log.Queue == queue && log.Status == status && log.SubStatus == subStatus)
				{
					return true;
				}
			}

			return false;
		}

		public ZDateTime GetLastQueueNameChangedEventDate()
		{
			ZDateTime result = ZDateTime.Empty;
			ProcessQueueLog[] logs = GetLogsDescendinglySortedByEventTime();

			if (logs.Length > 0)
			{
				ZString firstQueue = logs[0].Queue;
				foreach (ProcessQueueLog log in logs)
				{
					if (log.Queue != firstQueue)
					{
						break;
					}
					result = log.SL_EventTime;
				}
			}

			return result;
		}

		public ZDateTime GetLastQueueNameChangedEventDate(ZString queueName)
		{
			ZDateTime result = ZDateTime.Empty;
			ProcessQueueLog[] logs = GetLogsDescendinglySortedByEventTime();

			bool startChecking = false;
			foreach (ProcessQueueLog log in logs)
			{
				if (log.Queue == queueName)
				{
					startChecking = true;
				}

				if (startChecking && log.Queue != queueName)
				{
					break;
				}
				result = log.SL_EventTime;
			}

			return result;
		}

		public ZDateTime GetLastQueueNameAndStatusesChangedEventDate()
		{
			ZDateTime result = ZDateTime.Empty;
			ProcessQueueLog[] logs = GetLogsDescendinglySortedByEventTime();

			if (logs.Length > 0)
			{
				ZString firstQueue = logs[0].Queue;
				ZString firstStatus = logs[0].Status;
				ZString firstSubStatus = logs[0].SubStatus;
				result = logs[0].SL_EventTime;

				foreach (ProcessQueueLog log in logs)
				{
					if (log.Queue != firstQueue || log.Status != firstStatus || log.SubStatus != firstSubStatus)
					{
						break;
					}
					result = log.SL_EventTime;
				}
			}

			return result;
		}

		public ZDateTime GetLastQueueChangedEventDate()
		{
			ZQuery filter = new ZQuery();
			filter.MaximumRows = 1;
			filter.OrderBy = StmALogSchema.Constants.SL_EventTime + " DESC";
			ProcessQueueLog[] logs = (ProcessQueueLog[])Find(filter);
			return (logs.Length > 0) ? logs[0].SL_EventTime : ZDateTime.Empty;
		}

		public ProcessQueueLog AddNew(ZString queueName, ZString status, ZString subStatus, ZString reason, ZString assignedTo)
		{
			ProcessQueueLog result = AddNew();
			result.SetQueueDetails(queueName, status, subStatus, reason, assignedTo);
			return result;
		}

		public ProcessQueueLog[] GetLogsDescendinglySortedByEventTime()
		{
			ZQuery filter = new ZQuery();
			filter.OrderBy = StmALogSchema.Constants.SL_EventTime + " DESC";
			return (ProcessQueueLog[])Find(filter);
		}

		#region Implementation

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			ProcessQueueLog log = child as ProcessQueueLog;
			log.SL_Reference = QueueTypeString;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery fAdditionalFilter = base.CreateAdditionalFilter();
			fAdditionalFilter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, QueueTypeString);
			return fAdditionalFilter;
		}

		ZString QueueTypeString
		{
			get
			{
				if (fQueueTypeString.IsEmpty)
				{
					fQueueTypeString = (QueueType == ProcessQueueType.Enum.Commercial) ? ProcessQueueType.Commercial : ProcessQueueType.Customs;
				}
				return fQueueTypeString;
			}
		}

		readonly ProcessQueueType.Enum QueueType;
		ZString fQueueTypeString;

		#endregion
	}
}
