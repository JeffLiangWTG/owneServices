using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	public static class CommandEnablerHelper
	{
		public static bool IsSendCustomCommandEnabled(IILElectronicMessageProvider messageProvider, string documentName)
		{
			var (reloadedBizObject, messageReference) = ReloadAndCreateProvider(messageProvider);

			return messageReference.IsEmpty
			|| AreEventsInOrder(reloadedBizObject, documentName, messageReference,
				(shouldBeTheLatestEvent: Events.MessageSent, previousEvent: Events.MessageWithdrawCancelRequest),
				(shouldBeTheLatestEvent: Events.MessageRejected, previousEvent: Events.MessageSent));
		}

		public static bool IsSendWithdrawalCustomCommandEnabled(IILElectronicMessageProvider messageProvider, string documentName)
		{
			var (reloadedBizObject, messageReference) = ReloadAndCreateProvider(messageProvider);

			return !messageReference.IsEmpty
			&& (IsLatestEvent(reloadedBizObject, documentName, messageReference, Events.MessageAccepted, Events.MessageSent)
			|| AreEventsInOrder(reloadedBizObject, documentName, messageReference,
				(shouldBeTheLatestEvent: Events.MessageWithdrawCancelRequest, previousEvent: Events.MessageSent),
				(shouldBeTheLatestEvent: Events.MessageRejected, previousEvent: Events.MessageWithdrawCancelRequest))
			|| IsLatestEvent(reloadedBizObject, documentName, messageReference, Events.MessageWithdrawCancelRequest, Events.MessageRejected, Events.MessageWithdrawCancelAccepted));
		}

		public static bool IsResetToOriginalCustomCommandEnabled(IILElectronicMessageProvider messageProvider, string documentName)
		{
			var (reloadedBizObject, messageReference) = ReloadAndCreateProvider(messageProvider);

			return !messageReference.IsEmpty
			&& IsLatestEvent(reloadedBizObject, documentName, messageReference, Events.MessageSent, Events.MessageAccepted, Events.MessageRejected);
		}

		public static StmALog GetMostRecentMessageSentEvent(this EnterpriseBusinessObject businessObject, Event @event, string documentName, string messageReference)
		{
			var reloadedBizObject = ReloadBizObject(businessObject.TablePrefix, businessObject.PK);
			return GetMostRecentMessageSentEventCore(reloadedBizObject, documentName, messageReference, @event);
		}

		static (EnterpriseBusinessObject reloadedBizObject, ZString messageReference) ReloadAndCreateProvider(IILElectronicMessageProvider messageProvider)
		{
			var reloadedBizObject = ReloadBizObject(messageProvider.BusinessObject.TablePrefix, messageProvider.BusinessObject.PK);
			var electronicMessageProvider = ILElectronicMessageProviderFactory.CreateILElectronicMessageProvider(messageProvider, reloadedBizObject);
			return (reloadedBizObject, electronicMessageProvider.MessageReferenceNumber);
		}

		static bool IsLatestEvent(EnterpriseBusinessObject businessObject, string documentName, string messageReference, Event shouldBeTheLatestEvent, Event firstOldEvent, params Event[] additionalOldEvents)
		{
			return IsLatestEventCore(businessObject, documentName, messageReference, shouldBeTheLatestEvent, firstOldEvent, additionalOldEvents);
		}

		static bool AreEventsInOrder(EnterpriseBusinessObject businessObject, string documentName, string messageReference, params (Event shouldBeTheLatestEvent, Event previousEvent)[] eventPairs)
		{
			foreach (var (shouldBeTheLatestEvent, previousEvent) in eventPairs)
			{
				if (!IsLatestEventCore(businessObject, documentName, messageReference, shouldBeTheLatestEvent, previousEvent))
				{
					return false;
				}
			}
			return true;
		}

		static bool IsLatestEventCore(EnterpriseBusinessObject bizObject, string documentName, string messageReference, Event shouldBeTheLatestEvent, Event firstOldEvent, params Event[] additionalOldEvents)
		{
			var shouldBeTheLatestEventLog = GetMostRecentMessageSentEventCore(bizObject, documentName, messageReference, shouldBeTheLatestEvent);
			if (shouldBeTheLatestEventLog == null)
			{
				return false;
			}

			if (IsOldEventLogNewer(bizObject, documentName, messageReference, firstOldEvent, shouldBeTheLatestEventLog))
			{
				return false;
			}

			foreach (var oldEvent in additionalOldEvents)
			{
				if (IsOldEventLogNewer(bizObject, documentName, messageReference, oldEvent, shouldBeTheLatestEventLog))
				{
					return false;
				}
			}

			return true;
		}

		static bool IsOldEventLogNewer(EnterpriseBusinessObject bizObject, string documentName, string messageReference, Event oldEvent, StmALog shouldBeTheLatestEventLog)
		{
			var oldEventLog = GetMostRecentMessageSentEventCore(bizObject, documentName, messageReference, oldEvent);
			return oldEventLog != null && oldEventLog.EventTimeOffset > shouldBeTheLatestEventLog.EventTimeOffset;
		}

		static StmALog GetMostRecentMessageSentEventCore(IStmALogParent reloadShipment, string documentName, string messageReference, Event @event)
		{
			var reference = $"MST={documentName}";
			if (!messageReference.IsNullOrEmpty() && @event != Events.MessageSent)
			{
				reference += $"|RFN={messageReference}";
			}

			var extraQuery = new ZQuery(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, reference);
			return reloadShipment.Logs.MostRecentLogByEventTime(@event, extraQuery);
		}

		static EnterpriseBusinessObject ReloadBizObject(string tablePrefix, ZGuid pK)
		{
			var factory = new BusinessObjectFactory();
			return (EnterpriseBusinessObject)factory.Load(tablePrefix, pK);
		}
	}
}
