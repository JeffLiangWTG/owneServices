using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class CustomsValuesFetchHintSuspenderHelperTest : TestCaseWithFactory
	{
		public void TestCustomsValuesFetchHintSuspender()
		{
			var factory1 = new BusinessObjectFactory();
			var factory2 = new BusinessObjectFactory();
			var type = typeof(DummyBusinessObject);
			using (factory1.SuspendCustomsValuesFetchHint(type))
			{
				Assert("Fetch hint for TestBusinessObject is suspended in factory1", factory1.IsCustomsValuesFetchHintSuspended(type));
				Assert("Fetch hint for TestBusinessObject is NOT suspended in factory2", !factory2.IsCustomsValuesFetchHintSuspended(type));
				Assert("Fetch hint for TestBusinessObject2 is NOT suspended in factory1", !factory1.IsCustomsValuesFetchHintSuspended(typeof(OrgHeader)));
			}

			Assert("Fetch hint for TestBusinessObject is NOT suspended", !factory1.IsCustomsValuesFetchHintSuspended(type));
		}
	}
}
