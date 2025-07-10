using System.Linq;
using Enterprise.Freight.Integration.AWB;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.AWB.Messaging;

public static class CompanyIdAndNumberUtils
{
	public static string GetShipperTraderCode(IFBaseMessageDetailsProvider awbDetailsProvider)
	{
		if (awbDetailsProvider.ShipperTraderNoType.StartsWith("EOR"))
		{
			return awbDetailsProvider.ShipperTraderNo;
		}

		return awbDetailsProvider.VATCountryHandlers.First(h => h.ShipperApplicable()).GetShipperTraderCode();
	}

	public static string GetConsigneeTraderCode(IFBaseMessageDetailsProvider awbDetailsProvider)
	{
		if (awbDetailsProvider.ConsigneeTraderNoType.StartsWith("EOR")
			|| awbDetailsProvider.ConsigneeTraderNoType == OrgCusCode.NorwayCodeTypes.MVA
			|| awbDetailsProvider.ConsigneeTraderNoType == OrgCusCode.SwissCodeTypes.UID)
		{
			return awbDetailsProvider.ConsigneeTraderNo;
		}

		return awbDetailsProvider.VATCountryHandlers.First(h => h.ConsigneeApplicable()).GetConsigneeTraderCode();
	}

	public static string GetAlsoNotifyTraderCode(IFBaseMessageDetailsProvider awbDetailsProvider)
	{
		if (awbDetailsProvider.AlsoNotifyTraderNoType.StartsWith("EOR")
			|| awbDetailsProvider.AlsoNotifyTraderNoType == OrgCusCode.NorwayCodeTypes.MVA
			|| awbDetailsProvider.AlsoNotifyTraderNoType == OrgCusCode.SwissCodeTypes.UID)
		{
			return awbDetailsProvider.AlsoNotifyTraderNo;
		}

		return awbDetailsProvider.VATCountryHandlers.First(h => h.AlsoNotifyApplicable()).GetAlsoNotifyTraderCode();
	}
}
