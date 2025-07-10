using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(RollBookingActionMethod))]
	internal class RollBookingActionMethodTest : OperationalActionMethodTest<RollBookingActionMethod>
	{
		public void TestGuiControl()
		{
			AssertEquals(true, Method.HasControl);
			using (var control = Method.NewGuiControl())
			{
				AssertType(typeof(RollBookingApplicatorControl), control);
			}
		}

		public void TestNewApplicatorIsOfTheCorrectType()
		{
			var applicator = Method.NewApplicator(Factory, null);
			AssertEquals(typeof(RollBookingApplicator), applicator?.GetType());
		}

		public void TestRequiredLicences()
		{
			AssertContainsExactElementsInAnyOrder((l) => l.DisplayName, new LicenceCheckpoint[] { Env.Licence.ShippingManagerBookings }, Method.GetRequiredLicenceCheckpoints());
		}

		public void TestRequiredSecurity()
		{
			AssertContainsExactElementsInAnyOrder((c) => c.DisplayText, new SecurityCheckpoint[] { Env.Security.AgencyBookingEdit }, Method.GetRequiredSecurityCheckpoints());
		}

		#region Implementation
		protected override RollBookingActionMethod NewMethod()
		{
			return new RollBookingActionMethod();
		}
		#endregion
	}
}
