using Enterprise.Freight.Integration.AWB;

namespace Enterprise.Freight.Forwarding.AWB.Messaging
{
	class DefaultVATCountryHandler : VATCountryHandler
	{
		public DefaultVATCountryHandler(IFBaseMessageDetailsProvider provider) : base(provider)
		{
		}

		public override bool ShipperApplicable() => true;
		public override bool ConsigneeApplicable() => true;
		public override bool AlsoNotifyApplicable() => true;

		public override string GetShipperTraderCode() => CombineTypeNo(Provider.ShipperTraderNoType, Provider.ShipperTraderNo);
		public override string GetConsigneeTraderCode() => CombineTypeNo(Provider.ConsigneeTraderNoType, Provider.ConsigneeTraderNo);
		public override string GetAlsoNotifyTraderCode() => CombineTypeNo(Provider.AlsoNotifyTraderNoType, Provider.AlsoNotifyTraderNo);
	}
}
