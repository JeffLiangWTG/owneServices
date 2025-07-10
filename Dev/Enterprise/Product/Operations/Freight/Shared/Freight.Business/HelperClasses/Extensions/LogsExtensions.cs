using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Extensions
{
	public static class LogsExtensions
	{
		public static void AddDTCEvent(this Logs logs, bool isDetachedParentInDB, ZString eventReference, params KeyValuePair<string, string>[] parameters)
		{
			if (isDetachedParentInDB)
			{
				logs.CreateOrRecreateEventLog(Events.Detached, EstimateActual.Actual, ZDateTimeOffset.Now, eventReference, parameters);
			}
			else
			{
				var query = new ZQuery();
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.AttachedCode);
				query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, eventReference);
				var attachLogs = logs.Find(query);

				if (attachLogs != null && attachLogs.Length > 0)
				{
					foreach (var stmALog in attachLogs.Where(x => !x.IsInDatabase))
					{
						stmALog.Delete();
					}
				}
			}
		}

		public static void AddATCEvent(this Logs logs, bool isParentInDatabase, ZString eventReference, IDictionary<string, string> parameters)
		{
			var deferFiringWorkflow = !isParentInDatabase;
			var eventValue = new EventValue(Events.Attached, false, deferFiringWorkflow, ZDateTimeOffset.Now, eventReference, parameters);
			logs.AddNew(eventValue);
		}

		public static void UpdateEventReferenceNumbers(this Logs logs, Event eventNeedBeUpdated, ZString originalRef, ZString resultRef, bool deferFiringWorkflow = false)
		{
			foreach (StmALog log in logs.Find(x => x.SL_SE_NKEvent == eventNeedBeUpdated.Code).ToArray())
			{
				var reference = log.SL_Reference;
				var referenceUpdated = false;

				if (!originalRef.IsEmpty && reference.Contains(originalRef))
				{
					reference = reference.Replace(originalRef, resultRef);
					referenceUpdated = true;
				}

				if (reference.Length > log.SL_ReferenceInfo.MaxLength)
				{
					reference = reference.Substring(0, log.SL_ReferenceInfo.MaxLength);
				}

				using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
				{
					if (referenceUpdated)
					{
						log.SL_FireWorkflow = deferFiringWorkflow;
					}
					log.SL_Reference = reference;
				}
			}
		}
	}
}
