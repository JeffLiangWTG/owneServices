using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class ReviewAllocation : AutoReviewAllocation
	{
		public ReviewAllocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
