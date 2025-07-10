using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class AccQueryClaimCollectionTest : BusinessObjectCollectionTestCase
	{
		public abstract AccQueryClaimCollection CallConstructorWithCompany();

		public void TestGetConstructorHandlesCompany()
		{
			AccQueryClaimCollection collection = CallConstructorWithCompany();
			AssertNotNull(collection.fCompany);
		}
	}
}
