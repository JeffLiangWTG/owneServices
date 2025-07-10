using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class ReviewProcessBudget : AutoReviewProcessBudget
	{
		public ReviewProcessBudget(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
