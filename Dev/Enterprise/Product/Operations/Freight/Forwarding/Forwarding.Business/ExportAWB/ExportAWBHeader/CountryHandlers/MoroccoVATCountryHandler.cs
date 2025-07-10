using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Integration.AWB;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class MoroccoVATCountryHandler : VATCountryHandler
	{
		public MoroccoVATCountryHandler(IFBaseMessageDetailsProvider provider) : base(provider)
		{
		}

		public override bool ConsigneeApplicable() => Provider.ConsigneeCountryCode == CountryCodes.Morocco && Provider.ConsigneeTraderNoType == OrgCusCode.MoroccoCodeTypes.ICE;

		public override string GetConsigneeTraderCode()
		{
			return Provider.ConsigneeTraderNo.IsEmpty ? "000000000000000" : Provider.ConsigneeTraderNo.ToString();
		}

		public override bool AlsoNotifyApplicable() => Provider.AlsoNotifyCountryCode == CountryCodes.Morocco && Provider.AlsoNotifyTraderNoType == OrgCusCode.MoroccoCodeTypes.ICE;

		public override string GetAlsoNotifyTraderCode()
		{
			return Provider.AlsoNotifyTraderNo.IsEmpty ? "000000000000000" : Provider.AlsoNotifyTraderNo.ToString() ;
		}
	}
}
