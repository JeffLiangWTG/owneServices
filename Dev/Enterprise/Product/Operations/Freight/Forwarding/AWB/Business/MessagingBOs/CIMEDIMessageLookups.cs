using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	public class CIMEDIMessageLookups : EDIMessageLookups
	{
		public CIMEDIMessageLookups(CIMEDIMessage parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList MessageTypeList
		{
			get { return Factory.GetCachedValue<CargoIMPMessageTypeList>(); }
		}
	}
}
