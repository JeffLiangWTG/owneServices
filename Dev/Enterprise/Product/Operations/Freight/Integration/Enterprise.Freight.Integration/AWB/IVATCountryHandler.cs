namespace Enterprise.Freight.Integration.AWB
{
	public interface IVATCountryHandler
	{
		bool ShipperApplicable();
		bool ConsigneeApplicable();
		bool AlsoNotifyApplicable();
		string GetShipperTraderCode();
		string GetConsigneeTraderCode();
		string GetAlsoNotifyTraderCode();
	}
}
