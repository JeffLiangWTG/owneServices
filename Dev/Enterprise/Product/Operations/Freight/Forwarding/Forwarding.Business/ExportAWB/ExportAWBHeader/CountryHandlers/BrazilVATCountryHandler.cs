using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Integration.AWB;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class BrazilVATCountryHandler : VATCountryHandler
	{
		public BrazilVATCountryHandler(IFBaseMessageDetailsProvider provider) : base(provider)
		{
		}

		public override bool ConsigneeApplicable() => IsBrazilCNPJ(Provider.ConsigneeCountryCode, Provider.ConsigneeTraderNoType);

		public override string GetConsigneeTraderCode()
		{
			return CombineTypeNo(Provider.ConsigneeTraderNoType, GetTraderCodeWithoutSpecialCharacters(Provider.ConsigneeTraderNo));
		}

		public override bool AlsoNotifyApplicable() => IsBrazilCNPJ(Provider.AlsoNotifyCountryCode, Provider.AlsoNotifyTraderNoType);

		public override string GetAlsoNotifyTraderCode()
		{
			return CombineTypeNo(Provider.AlsoNotifyTraderNoType, GetTraderCodeWithoutSpecialCharacters(Provider.AlsoNotifyTraderNo));
		}

		bool IsBrazilCNPJ(string countryCode, string type) => countryCode == CountryCodes.Brazil && type == "CNPJ";
	}
}
