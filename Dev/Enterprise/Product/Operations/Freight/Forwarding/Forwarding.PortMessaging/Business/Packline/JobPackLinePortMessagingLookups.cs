using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.PortMessaging.Business
{
	public class JobPackLinePortMessagingLookups : AutoJobPackLinePortMessagingLookups
	{
		public JobPackLinePortMessagingLookups(AutoJobPackLinePortMessaging parent)
			: base(parent)
		{
			portMessaging = parent;
		}

		readonly AutoJobPackLinePortMessaging portMessaging;

		public CodeDescriptionPairList EntryTypeList
		{
			get { return new EntryTypeList(); }
		}

		public CodeDescriptionPairList ExemptionReasonList
		{
			get { return new ExemptionReasonList(portMessaging.JLM_EntryType); }
		}

		public CodeDescriptionPairList Annex30ATypeList
		{
			get { return new Annex30ATypeList(); }
		}
	}
}
