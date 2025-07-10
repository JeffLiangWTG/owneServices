using System;

namespace Enterprise.MarketingManager.Integration
{
	[Flags]
	public enum OrgSalesProductAssociationTarget
	{
		None = 0,
		OrgSales = 1 << 0,
		OrgTradeDetail = 1 << 1
	}
}
