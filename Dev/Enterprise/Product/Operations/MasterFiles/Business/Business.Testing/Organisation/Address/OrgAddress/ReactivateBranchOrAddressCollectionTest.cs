using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ReactivateBranchOrAddressCollection))]
	public class ReactivateBranchOrAddressCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReactivateBranchOrAddressCollection>
	{
		public void TestOverride()
		{
			var collection = GetCollectionToTest();
			AssertEquals(false, collection.AllowNew);
			AssertEquals(false, collection.AllowRemove);
		}

		protected override ReactivateBranchOrAddressCollection GetCollectionToTest()
		{
			return new ReactivateBranchOrAddressCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ReactivateBranchOrAddressItem();
		}
	}
}

