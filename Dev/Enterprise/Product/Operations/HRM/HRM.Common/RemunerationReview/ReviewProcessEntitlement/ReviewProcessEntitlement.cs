using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class ReviewProcessEntitlement : AutoReviewProcessEntitlement
	{
		public ReviewProcessEntitlement(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
