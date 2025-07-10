using System.Linq;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal abstract class ReleaseOrderActionMethodTest<T> : OperationalActionMethodTest<T> where T : ReleaseOrderActionMethod
	{
		public void TestRequiredLicenceCheckpoints()
		{
			AssertContainsExactElementsInAnyOrder((l) => l.DisplayName, new LicenceCheckpoint[] { Env.Licence.ShippingManagerBillOfLading }, Method.GetRequiredLicenceCheckpoints());
		}

		public void TestRequiredSecurityCheckpoints()
		{
			AssertContainsExactElementsInAnyOrder((s) => s.DisplayTextPathToSecurityRight, new SecurityCheckpoint[] { Env.Security.AgencyBillOfLadingReleaseDeliverOrderMessaging }, Method.GetRequiredSecurityCheckpoints());
		}

		public void TestHasControl()
		{
			AssertEquals("Should have control enabled", true, Method.HasControl);
		}

		public void TestHasSettings()
		{
			AssertEquals("Should have settings enabled", true, Method.HasSettings);
			AssertEquals("Expected Settings Type", typeof(ReleaseImportOrderSettings), Method.NewSetting(Factory).GetType());
		}

		public void TestRequirements()
		{
			var requirements = Method.GetFilterRequirements();

			AssertContainsExactElementsInAnyOrder("Constraints", new[] { FilterConstants.IsImportReleaseOrderEnabled }, requirements.Select(r => r.ConstraintName));
			AssertContainsExactElementsInAnyOrder("Values", new string[] { "Y" }, requirements[FilterConstants.IsImportReleaseOrderEnabled]);
		}
	}
}
