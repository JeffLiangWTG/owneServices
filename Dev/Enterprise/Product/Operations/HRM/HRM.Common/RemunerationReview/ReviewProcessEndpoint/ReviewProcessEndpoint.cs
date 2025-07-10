using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.HRM.Common
{
	public class ReviewProcessEndpoint : AutoReviewProcessEndpoint
	{
		public ReviewProcessEndpoint(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}
}
