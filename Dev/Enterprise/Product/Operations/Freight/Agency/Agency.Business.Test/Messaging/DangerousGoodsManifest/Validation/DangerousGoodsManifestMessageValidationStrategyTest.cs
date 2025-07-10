using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business.Test
{
	internal class DangerousGoodsManifestMessageValidationStrategyTest : TestCaseWithFactory
	{
		public void TestRegistration()
		{
			AssertEquals(false, DangerousGoodsManifestMessageValidationStrategy.IsRegisteredInFactory(Factory));

			DangerousGoodsManifestMessageValidationStrategy.RegisterForFactory(Factory);
			AssertEquals(true, DangerousGoodsManifestMessageValidationStrategy.IsRegisteredInFactory(Factory));

			DangerousGoodsManifestMessageValidationStrategy.UnregisterForFactory(Factory);
			AssertEquals(false, DangerousGoodsManifestMessageValidationStrategy.IsRegisteredInFactory(Factory));
		}
	}
}
