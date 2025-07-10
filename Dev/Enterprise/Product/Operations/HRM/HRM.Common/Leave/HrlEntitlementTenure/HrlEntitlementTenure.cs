using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class HrlEntitlementTenure : AutoHrlEntitlementTenure
	{
		public HrlEntitlementTenure(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
