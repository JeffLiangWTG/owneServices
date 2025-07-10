namespace Enterprise.Warehouse.Invoicing.Module.Test
{
	using Enterprise.Licensing;
	using Enterprise.Security;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.Testing;
	using NUnit.Framework;

	[TestedType(typeof(PeriodicInvoicingModuleBasherTest))]
	public abstract class PeriodicInvoicingModuleBasherTest : ZModuleBasherTest
	{
		#region TestModuleID

		public void TestModuleID()
		{
			using (var module = (PeriodicInvoicingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(GetModuleID(), module.ID);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = (PeriodicInvoicingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(GetLicenceCheckpoint(), module.LicenceCheckPoint);
			}
		}

		protected abstract LicenceCheckpoint GetLicenceCheckpoint();

		#endregion

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (var module = (PeriodicInvoicingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(GetSecurityCheckpoint(), module.SecurityCheckpoint);
			}
		}

		protected abstract SecurityCheckpoint GetSecurityCheckpoint();

		#endregion

		#region TestSecurityCheckPoint_Message

		public void TestSecurityCheckPoint_Message()
		{
			const string expected =
@"Error You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Warehouse -> Periodic Billing -> New
";

			AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () =>
			{
				ShowNewFormForTest();
				AssertMultilineASCIIEquals("Should have shown the security dialog", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
			});
		}

		#endregion

		#region TestAllowDelete

		public void TestAllowDelete()
		{
			using (var module = (PeriodicInvoicingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Assert("Delete option should be available", module.AllowDelete);
			}
		}

		#endregion

		#region Implementation

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", condition: true);
		}

		protected abstract void ShowNewFormForTest();

		#endregion
	}
}
