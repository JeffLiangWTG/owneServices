using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.CFS.Business.Rating.Testing
{
	public class CFSLoadListConsolRatingAdapterTest : TestCaseWithFactory
	{
		public void TestNullJobDoesntCauseErrors()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			var adapter = loadList.RatingAdapter;

			AssertNull(adapter.DeliveryAddress);
			AssertNull(adapter.PickupAddress);
			AssertNull(adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE]);
			AssertNull(adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR]);
		}
	}
}
