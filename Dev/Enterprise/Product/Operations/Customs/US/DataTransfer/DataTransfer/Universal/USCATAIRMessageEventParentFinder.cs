using CargoWise.EntityFramework;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Business;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class USCATAIRMessageEventParentFinder : Customs.DataTransfer.Universal.IUSCATAIRMessageEventParentFinder
	{
		BusinessObject[] Customs.DataTransfer.Universal.IUSCATAIRMessageEventParentFinder.GetLogParentsForEventUsingContext(Event eventDataObject, BusinessObjectFactory factory)
		{
			BusinessObject[] result = null;
			var isBondStatusNotificationMessageEvent = IsBondStatusNotificationMessageEvent(eventDataObject);
			if (isBondStatusNotificationMessageEvent)
			{
				var eBondProcessor = new eBondEventProcessor(eventDataObject, factory);
				result = eBondProcessor.Process();
			}

			return result == null && isBondStatusNotificationMessageEvent ? System.Array.Empty<BusinessObject>() : result;
		}

		#region Filter Constructors

		public static bool IsBondStatusNotificationMessageEvent(Event eventDataObject)
		{
			var result = false;
			if (eventDataObject != null && eventDataObject.EventParameters != null)
			{
				result = eventDataObject.EventType.GetValueOrDefault() == Events.MessageReceivedCode
						 && eventDataObject.EventParameters.MessageType.GetValueOrDefault() == ACEApplicationIdentifierCodeList.Codes.eBondStatusNotification
						 && eventDataObject.EventParameters.Department.GetValueOrDefault() == "CBP";
			}
			return result;
		}

		#endregion
	}
}
