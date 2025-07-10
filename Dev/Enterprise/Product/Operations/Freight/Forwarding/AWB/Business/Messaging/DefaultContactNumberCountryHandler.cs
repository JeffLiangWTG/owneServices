using Enterprise.Freight.Integration.AWB;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	public class DefaultContactNumberCountryHandler : IContactNumberCountryHandler
	{
		public DefaultContactNumberCountryHandler(IFBaseMessageDetailsProvider provider)
		{
			Provider = provider;
		}

		protected IFBaseMessageDetailsProvider Provider { get; }

		public bool UseAirlineIdentifierForShipper => false;
		public bool UseAirlineIdentifierForConsignee => false;
		public bool UseAirlineIdentifierForAlsoNotify => false;

		public string ShipperCountryCodeForContactNumber() => Provider.ShipperCountryCode;
		public string ConsigneeCountryCodeForContactNumber() => Provider.ConsigneeCountryCode;
		public string AlsoNotifyCountryCodeForContactNumber() => Provider.AlsoNotifyCountryCode;

		public bool ShipperApplicable() => true;
		public bool ConsigneeApplicable() => true;
		public bool AlsoNotifyApplicable() => true;
	}
}
