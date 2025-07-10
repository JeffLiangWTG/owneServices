
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business
{
	public static class ServiceEventUtils
	{
		#region IsServiceCompleted

		public static bool IsServiceCompleted(BusinessObject bizO, ZBool isServiceRequired, Event requiredEvent, Event completedEvent)
		{
			ZBool result = false;
			if (isServiceRequired)
			{
				StmALog requiredLog = bizO.GetLogs().MostRecentLogByEventTime(requiredEvent);
				if (requiredLog != null)
				{
					StmALog completedLog = bizO.GetLogs().MostRecentLogByEventTime(completedEvent);
					if (completedLog != null && !completedLog.SL_IsCancelled && completedLog.SL_EventTime > requiredLog.SL_EventTime)
					{
						result = true;
					}
				}
			}
			return result;
		}

		#endregion

		#region SetupEvent

		public static void SetupEvent(BusinessObject bizO, ZBool isChecked, Event eventToDeal)
		{
			if (isChecked)
			{
				StmALog log = bizO.GetLogs().MostRecentLogByEventTime(eventToDeal);
				if (log != null && !log.IsInDatabase)
				{
					log.Delete();
				}
				bizO.GetLogs().AddNew(eventToDeal);
			}
			else
			{
				StmALog log = bizO.GetLogs().MostRecentLogByEventTime(eventToDeal);
				if (log != null)
				{
					if (log.IsInDatabase)
					{
						log.Cancel();
					}
					else
					{
						log.Delete();
					}
				}
			}
		}

		#endregion

		#region ExcludeEventsFromAdd

		public static void ExcludeEventsFromAdd(BusinessObject bizO, Event[] eventsToExlude)
		{
			foreach (Event item in eventsToExlude)
			{
				bizO.GetLogs().EventsThatCannotBeAdded.Add(item);
			}
		}

		#endregion

		#region ExcludeEventsFromCancel

		public static void ExcludeEventsFromCancel(BusinessObject bizO, Event[] eventsToExlude)
		{
			foreach (Event item in eventsToExlude)
			{
				bizO.GetLogs().EventsThatCannotBeCancelled.Add(item);
			}
		}

		#endregion
	}
}
