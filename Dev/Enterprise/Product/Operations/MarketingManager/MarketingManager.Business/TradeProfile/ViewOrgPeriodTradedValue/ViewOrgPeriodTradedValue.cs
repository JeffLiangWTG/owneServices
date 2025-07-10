using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class OrgPeriodTradedValue : AutoViewOrgPeriodTradedValue
	{
		public OrgPeriodTradedValue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}
	}
}
