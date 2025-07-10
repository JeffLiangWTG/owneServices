using Enterprise.Freight.Integration.AWB;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class ChinaContactNumberCountryHandler : IContactNumberCountryHandler
	{
		public ChinaContactNumberCountryHandler(IFBaseMessageDetailsProvider provider)
		{
			Provider = provider;
		}

		public ChinaContactNumberCountryHandler(IFBaseMessageDetailsProvider provider, ExportAWBHeader exportAWBHeader) : this(provider)
		{
			Header = exportAWBHeader;
		}

		protected IFBaseMessageDetailsProvider Provider { get; }
		protected ExportAWBHeader Header { get; }

		public bool UseAirlineIdentifierForShipper => true;
		public bool UseAirlineIdentifierForConsignee => true;
		public bool UseAirlineIdentifierForAlsoNotify => true;

		public string ShipperCountryCodeForContactNumber() => CountryCodes.China;
		public string ConsigneeCountryCodeForContactNumber() => CountryCodes.China;
		public string AlsoNotifyCountryCodeForContactNumber() => CountryCodes.China;

		public bool ShipperApplicable() => Header.IsTransitingThroughChina;
		public bool ConsigneeApplicable() => Header.IsImportToChina || Header.IsTransitingThroughChina;
		public bool AlsoNotifyApplicable() => Header.IsImportToChina || Header.IsTransitingThroughChina;
	}
}
