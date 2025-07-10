using System.ComponentModel;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(AreaModule))]
	class AreaModuleTest : ZModuleBasherTest
	{
		#region TestModuleID

		public void TestModuleID()
		{
			using (var module = (AreaModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.WhsConfigArea, module.ID);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = (AreaModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerOperationsAnd3PL, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (var module = (AreaModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsConfigArea, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region TestControllersDefinedForAllCountriesModuleDefinedOn

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		#endregion

		#region TestShowNewForm_DeniedBySecurityCheckPoint

		public void TestShowNewForm_DeniedBySecurityCheckPoint()
		{
			Env.Security.WhsConfigAreaNew.IsAllowed = false;

			using (var module = new TestAreaModule())
			{
				const string expected =
@"Error You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Warehouse -> Areas -> New
";
				AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () =>
				{
					module.ShowNewForm();
					AssertMultilineASCIIEquals("Should have shown the security dialog", expected, UnitTestUserNotification.Instance.LastMessage.ToString());
				});
			}
		}

		#endregion

		#region TestShowNewFormNullRef

		[ExpectNoExceptions]
		public void TestShowNewFormNullRef()
		{
			using (var module = new AreaModuleForNullRefTest())
			{
				using (module.ShowNewFormExposed())
				{
				}
			}
		}

		class AreaModuleForNullRefTest : AreaModule
		{
			public IZForm ShowNewFormExposed()
			{
				return ShowNewForm();
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsConfigArea;
		}

		class TestAreaModule : AreaModule
		{
			public new IZForm ShowNewForm()
			{
				return base.ShowNewForm();
			}
		}

		#endregion
	}

	[TestedType(typeof(WhsAreaTypedBusinessObjectCollection))]
	class WhsAreaTypedBusinessObjectCollectionTest : BusinessObjectCollectionTestCase
	{
		#region TestCompare

		public void TestCompare()
		{
			var areas = new WhsAreaTypedBusinessObjectCollection(Factory);

			var transitWarehouse = Helper.CreateWarehouse("W1", "A", 2, 1);
			transitWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Transit;

			var productWarehouse = Helper.CreateWarehouse("W2", "B", 2, 1);
			productWarehouse.WW_WarehouseType = WarehouseTypes.Codes.Product;
			var org1 = Helper.CreateClient("O1", "A Client");
			var org2 = Helper.CreateClient("O2", "X Client");

			CreateArea("Area1", areas, productWarehouse, org2);
			CreateArea("Area2", areas, productWarehouse, org2);
			CreateArea("Area3", areas, productWarehouse, org2);
			CreateArea("Area4", areas, transitWarehouse, org1);
			CreateArea("Area5", areas, transitWarehouse, org2);
			CreateArea("Area6", areas, productWarehouse, org1);
			CreateArea("Area7", areas, productWarehouse, org2);
			CreateArea("Area8", areas, productWarehouse, org2);

			const string expectedText =
				"A Client\n" +
				"X Client\n" +
				"N/A\n" +
				"N/A\n" +
				"N/A\n" +
				"N/A\n" +
				"N/A\n" +
				"N/A\n" +
				"";

			const string expectedTextInverted =
				"X Client\n" +
				"A Client\n" +
				"N/A\n" +
				"N/A\n" +
				"N/A\n" +
				"N/A\n" +
				"N/A\n" +
				"N/A\n" +
				"";

			areas.Sort(WhsArea.Schema.WA_CalcTransitClientDescription, ListSortDirection.Ascending);
			AssertMultilineASCIIEquals("Ascending", expectedText, OrderAsString(areas));

			areas.Sort(WhsArea.Schema.WA_CalcTransitClientDescription, ListSortDirection.Descending);
			AssertMultilineASCIIEquals("Descending", expectedTextInverted, OrderAsString(areas));
		}

		string OrderAsString(WhsAreaTypedBusinessObjectCollection areas)
		{
			var builder = new StringBuilder();

			foreach (WhsArea area in areas)
			{
				builder.AppendFormat("{0}\n", area.WA_CalcTransitClientDescription);
			}

			return builder.ToString();
		}

		void CreateArea(ZString areaName, WhsAreaTypedBusinessObjectCollection areas, WhsWarehouse transitWarehouse, MasterFiles.Business.OrgHeader org1)
		{
			var area1 = areas.AddNew();
			area1.WA_WW_Whs = transitWarehouse.PK;
			area1.WA_OH_TransitClient = org1.PK;
			area1.WA_Name = areaName;
			area1.WA_AreaType = "FRE";
		}

		#endregion

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsAreaTypedBusinessObjectCollection(Factory);
		}

		WhsTestHelperFunctionsEnv Helper => helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory));
		WhsTestHelperFunctionsEnv helper;

		#endregion
	}
}
