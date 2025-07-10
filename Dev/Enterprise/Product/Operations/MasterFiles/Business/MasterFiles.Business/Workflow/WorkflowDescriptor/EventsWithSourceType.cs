using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Store Event codes from a source that were triggered
	/// </summary>
	public class EventsWithSourceType
	{
		public enum SourceType { None, Shipment, Consol, Order, ShipmentPrePlanning, Declaration, CustomsStatement, ImporterSecurityFiling, WhsReceipt, WhsOrder, WhsAdjustment }

		protected EventsWithSourceType()
		{
			sourceType = SourceType.None;
			triggeredByEvents = Array.Empty<Event>();
		}

		public EventsWithSourceType(SourceType sourceType, ProcessTaskNotification triggerAction, BusinessObject triggerParent)
		{
			this.sourceType = sourceType;
			this.triggerAction = triggerAction;
			this.triggerParent = triggerParent;
		}

		readonly SourceType sourceType;
		Event[] triggeredByEvents;
		HashSet<ZGuid> triggeringLogPKs;

		#region Empty

		public static EventsWithSourceType Empty
		{
			get { return empty ?? (empty = new EventsWithSourceType()); }
		}
		[ThreadSafe]
		static EventsWithSourceType empty;

		#endregion

		#region Trigger Action

		public ProcessTaskNotification TriggerAction
		{
			get { return triggerAction; }
		}

		readonly ProcessTaskNotification triggerAction;

		public BusinessObject TriggerParent => triggerParent;
		readonly BusinessObject triggerParent;

		#endregion

		#region Get Events

		public Event[] GetEvents()
		{
			return triggeredByEvents ?? (triggeredByEvents = ExtractTriggeredByEvents(triggerAction));
		}

		public Event[] GetEvents(SourceType sourceType)
		{
			if (sourceType == this.sourceType)
			{
				return GetEvents();
			}
			else
			{
				return Array.Empty<Event>();
			}
		}

		static Event[] ExtractTriggeredByEvents(ProcessTaskNotification triggerAction)
		{
			List<Event> eventsList = new List<Event>();

			if (triggerAction != null && triggerAction.Parent != null)
			{
				EventCollectionBase allEvents = Events.All;
				Event eventFound = allEvents[triggerAction.Parent.TriggerEventCode];
				eventsList.Add(eventFound);
			}

			return eventsList.ToArray();
		}

		#endregion

		#region Get Triggerring Logs

		public bool HasLog(ZGuid guid)
		{
			return GetTriggerringLogPKs().Contains(guid);
		}

		public HashSet<ZGuid> GetTriggerringLogPKs()
		{
			return triggeringLogPKs ?? (triggeringLogPKs = ExtractTriggeringLogPKs(triggerAction));
		}

		static HashSet<ZGuid> ExtractTriggeringLogPKs(ProcessTaskNotification triggerAction)
		{
			var guids = new HashSet<ZGuid>();
			if (triggerAction != null)
			{
				var logsParent = (IStmALogParent)triggerAction.Parent;
				if (logsParent != null)
				{
					var query = new ZQuery(StmALogSchema.SL_Parent, logsParent.LogsParentPK);
					query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);

					foreach (var log in logsParent.Factory.Load<StmALog>(query))
					{
						var guid = new WorkflowTriggerEventData(log).TriggeringLogPK;
						if (guid.IsValid)
						{
							guids.Add(guid);
						}
					}
				}
			}

			return guids;
		}

		#endregion
	}
}
