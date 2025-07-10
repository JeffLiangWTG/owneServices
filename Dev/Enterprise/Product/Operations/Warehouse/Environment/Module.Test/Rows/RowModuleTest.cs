using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(RowModule))]
	internal class RowModuleTest : ZModuleBasherTest
	{
		public void TestModuleID()
		{
			using (RowModule module = (RowModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsConfigRow, module.ID);
			}
		}

		public void TestLicenseCheckPoint()
		{
			using (RowModule module = (RowModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerCoreAnd4PL, module.LicenceCheckPoint);
			}
		}

		public void TestSecurityCheckPoint()
		{
			using (RowModule module = (RowModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsConfigLocation, module.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsConfigRow;
		}

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		#region TestShowNewForm_DeniedBySecurityCheckPoint

		public void TestShowNewForm_DeniedBySecurityCheckPoint()
		{
			Env.Security.WhsConfigLocationNew.IsAllowed = false;

			using (var module = new TestRowModule())
			{
				const string expected =
@"Error You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Warehouse -> Locations -> New
";
				AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () =>
					{
						module.ShowNewForm();
						AssertMultilineASCIIEquals("Should have shown the security dialog", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
					});
			}
		}

		class TestRowModule : RowModule
		{
			public new IZForm ShowNewForm()
			{
				return base.ShowNewForm();
			}
		}

		#endregion
	}

	[TestedType(typeof(WhsRowTypedBusinessObjectCollection))]
	class WhsRowTypedBusinessObjectCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsRowTypedBusinessObjectCollection(Factory);
		}
	}
}
