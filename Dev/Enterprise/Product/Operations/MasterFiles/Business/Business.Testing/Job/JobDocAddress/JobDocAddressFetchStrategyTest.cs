using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobDocAddressFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			Parent = Factory.New<JobDocAddressPersistentParentForTesting>();
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			return Parent.DocAddresses;
		}

		JobDocAddressPersistentParentForTesting Parent;

		public void TestFetchForView()
		{
			TestFetchForView(Parent.DocAddresses.AddNew(), Parent.DocAddresses.AddNew());
		}
	}
}
