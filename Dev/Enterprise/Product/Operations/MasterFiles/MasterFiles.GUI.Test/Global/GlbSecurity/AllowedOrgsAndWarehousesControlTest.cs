using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AllowedOrgsAndWarehousesControlTest : TestCaseWithFactory
	{
		#region Properties

		public void TestSetAllowedOrgsAndWarehousesControlData()
		{
			using (AllowedOrgsAndWarehousesControl control = new AllowedOrgsAndWarehousesControl())
			{
				AssertEquals("Precondition DescriptionLabel: ", string.Empty, control.DescriptionLabel.Text);
				AssertEquals("Precondition ModuleID: ", ModuleIDs.Organisation, control.ModuleID);

				control.SetAllowedOrgsAndWarehousesControlData(Environment.Env.Security.WhsAllowedClients);
				AssertEquals("DescriptionLabel", "If the above security permission to access warehouse and allocation information for all clients is denied, then access is still granted for warehouse and allocation information relating to the following clients.", control.DescriptionLabel.Text);
				AssertEquals("ModuleID", ModuleIDs.Organisation, control.ModuleID);

				control.SetAllowedOrgsAndWarehousesControlData(Environment.Env.Security.WhsAllowedWarehouses);
				AssertEquals("DescriptionLabel", "If the above security permission to access warehouse and allocation information for all warehouses is denied, then access is still granted for warehouse and allocation information relating to the following warehouses.", control.DescriptionLabel.Text);
				AssertEquals("ModuleID", ModuleIDs.WhsConfigWarehouse, control.ModuleID);

				control.SetAllowedOrgsAndWarehousesControlData(Environment.Env.Security.AgencyPrincipalAccess);
				AssertEquals("DescriptionLabel", "If the above security permission to access shipment and allocation information for all principals is denied, then access is still granted for shipment and allocation information relating to the following principals.", control.DescriptionLabel.Text);
				AssertEquals("ModuleID", ModuleIDs.Organisation, control.ModuleID);
			}
		}

		public void TestTypeOfFindBoxCollection()
		{
			using (AllowedOrgsAndWarehousesControl control = new AllowedOrgsAndWarehousesControl())
			{
				AssertEquals("Precondition: ", typeof(ShipsAgencyPrincipalCollection), control.TypeOfFindBoxCollection);
				control.TypeOfFindBoxCollection = typeof(WarehouseClientCollection);
				AssertEquals(typeof(WarehouseClientCollection), control.TypeOfFindBoxCollection);
			}
		}

		public void TestAddButtonClick()
		{
			var principal = Factory.New<OrgHeader>();
			principal.OH_Code = "MyOrg1";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();

			var staff = Factory.New<GlbStaff>();
			using (var form = new ZForm())
			using (var control = new AllowedOrgsAndWarehousesControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(staff, "");
				form.Show();

				AssertEquals("should not be authorised for any principals", 0, staff.SecurityAllowedOrgsAndWarehousesView.Count);

				((IFindBox)control).Code = "MyOrg1";
				AssertEquals("should now be authorised for one principal", 1, staff.SecurityAllowedOrgsAndWarehousesView.Count);
				AssertEquals("should be authorised for the correct principal", principal.PK, staff.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);

				var addButton = (ZButton)control.Controls.Find("AddButton", true)[0];
				addButton.PerformClick();
				System.Windows.Forms.Application.DoEvents();
				using (var popup = (EmbeddedModulePopup)ZFormModaliser.ActiveForm)
				{
					var module = (ZFilterGridModule)popup.Module_ForTest;
					((IFilterModuleInternalsForTesting)module).PerformSearch();

					var principalBussinessObject = module.GridCollection.FindByPK(principal.PK);
					AssertHasRowErrorContaining(principalBussinessObject, "This record is already selected");
					popup.Close();
				}
			}
		}

		#endregion

		#region IFindBox members

		public void TestCode()
		{
			OrgHeader principal = Factory.New<OrgHeader>();
			principal.OH_Code = "RANDOM";
			principal.OH_IsShippingProvider = true;
			principal.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();

			GlbStaff staff = Factory.New<GlbStaff>();

			using (AllowedOrgsAndWarehousesControl control = new AllowedOrgsAndWarehousesControl())
			{
				control.SetDataBinding(staff, "");

				AssertEquals("should not be authorised for any principals", 0, staff.SecurityAllowedOrgsAndWarehousesView.Count);

				((IFindBox)control).Code = "RANDOM";
				AssertEquals("should now be authorised for one principal", 1, staff.SecurityAllowedOrgsAndWarehousesView.Count);
				AssertEquals("should be authorised for the correct principal", principal.PK, staff.SecurityAllowedOrgsAndWarehousesView[0].GU_ItemGUID);
			}
		}

		public void TestListProvider()
		{
			GlbStaff staff = Factory.New<GlbStaff>();

			using (AllowedOrgsAndWarehousesControl control = new AllowedOrgsAndWarehousesControl())
			{
				control.SetDataBinding(staff, "");

				AssertEquals("Precondition: ", typeof(ShipsAgencyPrincipalCollection), ((IFindBox)control).ListProvider.GetType());
				control.TypeOfFindBoxCollection = typeof(WarehouseClientCollection);
				AssertEquals(typeof(WarehouseClientCollection), ((IFindBox)control).ListProvider.GetType());
			}
		}

		public void TestBoundGridAllowReadOnlyRowsToBeDeleted()
		{
			using (var control = new AllowedOrgsAndWarehousesControl())
			{
				var grid = control.Controls.Find("AllowedOrgsAndWarehousesSecurityPanelBoundZGrid", true)[0] as ZGrid;
				Assert(grid.AllowReadOnlyRowsToBeDeleted);
			}
		}

		#endregion
	}
}
