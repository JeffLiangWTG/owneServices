using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Integration.AWB;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class ArgentinaVATCountryHandler : VATCountryHandler
	{
		public ArgentinaVATCountryHandler(IFBaseMessageDetailsProvider provider) : base(provider)
		{
		}

		public override bool ConsigneeApplicable() => Provider.ConsigneeCountryCode == CountryCodes.Argentina;

		public override string GetConsigneeTraderCode()
		{
			return CombineTypeNo(Provider.ConsigneeTraderNoType, GetTraderCodeWithoutSpecialCharacters(Provider.ConsigneeTraderNo));
		}
	}
}
