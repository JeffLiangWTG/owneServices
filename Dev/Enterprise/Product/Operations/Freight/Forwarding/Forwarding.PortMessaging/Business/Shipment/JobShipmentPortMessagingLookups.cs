using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public class JobShipmentPortMessagingLookups : AutoJobShipmentPortMessagingLookups
	{
		public JobShipmentPortMessagingLookups(AutoJobShipmentPortMessaging parent)
			: base(parent)
		{
			portMessaging = parent;
		}

		readonly AutoJobShipmentPortMessaging portMessaging;

		public CodeDescriptionPairList EntryTypeList
		{
			get { return new EntryTypeList(); }
		}

		public CodeDescriptionPairList ExemptionReasonList
		{
			get { return new ExemptionReasonList(portMessaging.JSM_EntryType); }
		}

		public CodeDescriptionPairList Annex30ATypeList
		{
			get { return new Annex30ATypeList(); }
		}
	}
}
