using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.Business.MessageManagers
{
	public class SupportingDocEHubDelivery : EServicesDelivery
	{
		public SupportingDocEHubDelivery(UniversalEvent universalEvent)
		{
			universalEventCore = universalEvent;
		}

		readonly UniversalEvent universalEventCore;

		protected override string GetRecipientID(IEDICommunicationsMode mode)
		{
			return mode.EK_Destination;
		}

		protected override string GetInterchangeTo(IEDICommunicationsMode mode)
		{
			return mode.EK_Destination.SubstringSafe(0, mode.EK_Destination.IndexOf(':'));
		}

		protected override string InterchangeQueuedStatus
		{
			get { return EDIInterchangeStatusList.Codes.eHubQueued; }
		}

		protected override string TransportType
		{
			get { return EDIInterchangeTransportTypeList.Codes.eHub; }
		}

		protected override MessageDataLogLinker GetMessageDataLogLinker(BusinessObjectFactory factory, DeliveryContext context)
		{
			return new MessageDataLogLinker(Events.DocumentSent, factory, universalEventCore?.EventReference ?? ZString.Empty);
		}
	}
}
