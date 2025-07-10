using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(UpdateExpiryNotificationPeriodMethod))]
	sealed class UpdateExpiryNotificationPeriodMethodTest : OperationalActionMethodTest<UpdateExpiryNotificationPeriodMethod>
	{
		#region TestNewApplicator_RealTest

		public void TestNewApplicator_RealTest()
		{
			AssertType<UpdateExpiryNotificationPeriodMethodApplicator>(NewMethod().NewApplicator(Factory, null));
		}

		#endregion

		#region TestNewGuiControl_RealTest

		public void TestNewGuiControl_RealTest()
		{
			var method = NewMethod();
			AssertEquals(true, method.HasControl);
			using (var control = method.NewGuiControl())
			{
				AssertType<UpdateExpiryNotificationPeriodControl>(control);
			}
		}

		#endregion

		#region TestNameAndDescription

		public void TestNameAndDescription()
		{
			AssertEquals("Update the Expiry Notification Period (Days)", NewMethod().Name);
			AssertEquals("Update the Expiry Notification Period (Days)", NewMethod().Description);
		}

		#endregion

		#region Implementation

		protected override UpdateExpiryNotificationPeriodMethod NewMethod()
		{
			return new UpdateExpiryNotificationPeriodMethod();
		}

		#endregion
	}
}
