namespace Enterprise.Freight.Integration.AWB
{
	public interface IContactNumberCountryHandler
	{
		bool ShipperApplicable();
		bool ConsigneeApplicable();
		bool AlsoNotifyApplicable();
		string ShipperCountryCodeForContactNumber();
		string ConsigneeCountryCodeForContactNumber();
		string AlsoNotifyCountryCodeForContactNumber();
		bool UseAirlineIdentifierForShipper { get; }
		bool UseAirlineIdentifierForConsignee { get; }
		bool UseAirlineIdentifierForAlsoNotify { get; }
	}
}
