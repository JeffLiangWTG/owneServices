using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Integration.AWB;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class BangladeshVATCountryHandler : VATCountryHandler
	{
		public BangladeshVATCountryHandler(IFBaseMessageDetailsProvider provider) : base(provider)
		{
		}

		public override bool ConsigneeApplicable() => Provider.ConsigneeCountryCode == CountryCodes.Bangladesh && Provider.ConsigneeTraderNoType == OrgCusCode.BangladeshCodeTypes.BIN;

		public override string GetConsigneeTraderCode()
		{
			return Provider.ConsigneeTraderNo.ToString();
		}

		public override bool AlsoNotifyApplicable() => Provider.AlsoNotifyCountryCode == CountryCodes.Bangladesh && Provider.AlsoNotifyTraderNoType == OrgCusCode.BangladeshCodeTypes.BIN;

		public override string GetAlsoNotifyTraderCode()
		{
			return Provider.AlsoNotifyTraderNo.ToString();
		}
	}
}
