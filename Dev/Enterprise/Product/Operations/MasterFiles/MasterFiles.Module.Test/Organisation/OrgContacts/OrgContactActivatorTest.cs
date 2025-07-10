using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class OrgContactActivatorTest : BusinessObjectActivatorTest
	{
		public void TestAdditionalActivateDeactivateAction()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.SupersedeWebAccess();
			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_IsActive = false;
			Factory.Save();

			var activator = new OrgContactActivator();
			activator.Deactivate(new BusinessObject[] { contact1, contact2, contact3 }, EnvProxy.Instance.Security.None);

			AssertEquals(false, contact1.OC_IsActive);
			AssertEquals(false, contact1.WebAccessSuperseded);
			AssertEquals(false, contact2.OC_IsActive);
			AssertEquals(false, contact2.WebAccessSuperseded);
			AssertEquals(false, contact3.OC_IsActive);
			AssertEquals(false, contact3.WebAccessSuperseded);
		}
	}
}
