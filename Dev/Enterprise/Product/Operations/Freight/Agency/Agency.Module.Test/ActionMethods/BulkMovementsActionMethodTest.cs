using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI.BulkMovements;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(BulkMovementsActionMethod))]
	internal class BulkMovementsActionMethodTest : OperationalActionMethodTest<BulkMovementsActionMethod>
	{
		public void TestGuiControl()
		{
			AssertEquals(true, Method.HasControl);
			using (Control control = (Control)Method.NewGuiControl())
			{
				AssertType(typeof(BulkMovementsApplicatorControl), control);
			}
		}

		public void TestNewApplicatorIsOfTheCorrectType()
		{
			OperationalActionMethodApplicator applicator = Method.NewApplicator(Factory, null);
			AssertEquals(typeof(BulkMovementsApplicator), applicator == null ? null : applicator.GetType());
		}

		public void TestRequiredLicences()
		{
			AssertContainsExactElementsInAnyOrder("Should require the shipping manager container control licence", (l) => l.DisplayName, new LicenceCheckpoint[] { Env.Licence.ShippingManagerContainerControl }, Method.GetRequiredLicenceCheckpoints());
		}

		public void TestRequiredSecurity()
		{
			AssertContainsExactElementsInAnyOrder("Should require the shipping manager container control edit security checkpoint", (c) => c.DisplayText, new SecurityCheckpoint[] { Env.Security.AgencyContainerManagerEdit }, Method.GetRequiredSecurityCheckpoints());
		}

		#region Implementation
		protected override BulkMovementsActionMethod NewMethod()
		{
			return new BulkMovementsActionMethod();
		}
		#endregion
	}
}
