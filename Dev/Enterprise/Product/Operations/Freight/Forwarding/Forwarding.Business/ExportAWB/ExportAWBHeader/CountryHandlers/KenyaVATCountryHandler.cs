using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Freight.Integration.AWB;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	class KenyaVATCountryHandler : VATCountryHandler
	{
		public KenyaVATCountryHandler(IFBaseMessageDetailsProvider provider) : base(provider)
		{
		}

		public override bool ConsigneeApplicable() => Provider.ConsigneeCountryCode == CountryCodes.Kenya && Provider.ConsigneeTraderNoType == OrgCusCode.KenyaCodeTypes.PIN;

		public override string GetConsigneeTraderCode()
		{
			return IsFWB ? Provider.ConsigneeTraderNo.ToString() : OrgCusCode.KenyaCodeTypes.PIN.Substring(0, 1) + Provider.ConsigneeTraderNo;
		}
	}
}
