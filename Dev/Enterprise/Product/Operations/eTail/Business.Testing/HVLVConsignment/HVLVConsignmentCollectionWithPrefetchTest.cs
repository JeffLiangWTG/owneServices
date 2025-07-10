using CargoWise.EntityFramework.Testing;

namespace Enterprise.eTail.Business.Testing
{
	public abstract class HVLVConsignmentCollectionWithPrefetchTest : BusinessObjectCollectionTestCase
	{
		public void TestAddNew_ClusterKeyIsPopulated()
		{
			var collection = GetCollectionToTest();
			Factory.Save();

			var consignment = collection.AddNew() as HVLVConsignment;

			AssertNotEquals("Consignment cluster key should be populated", 0, consignment.HVC_ClusterKey);
		}
	}
}
