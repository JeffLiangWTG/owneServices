using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.Business.US.Testing;
using Enterprise.Warehouse.Environment.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.GUI.US.Testing
{
	public class LocationsEditUSControlTest : LocationsEditBaseControlTest
	{
		public void TestTSAColumnAndEditBoxWhenWarehouseIsApprovedKnown()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("WHS");
			WhsRow row = Helper.CreateRow(whs, "ROW");
			Helper.SetWarehouseTSAStatus(whs, CodeLists.US.TSAStatus.Codes.Known);

			using (ZForm testForm = new ZForm(row))
			using (LocationsEditUSControl control = new LocationsEditUSControl())
			{
				testForm.Controls.Add(control);
				testForm.Show();
				control.SetDataBinding(row, "");

				AssertEquals(true, control.LocationGrid.Columns[WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name].IsVisible);
				AssertEquals(false, control.LocationGrid.Columns[WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name].IsUnavailable);
				AssertEquals("", control.LocationGrid.Columns[WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name].ErrorMessageWhenUnavailable);
			}
		}

		public void TestTSAColumnAndEditBoxWhenWarehouseIsNotApprovedKnown()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("WHS");
			WhsRow row = Helper.CreateRow(whs, "ROW");
			Helper.SetWarehouseTSAStatus(whs, CodeLists.US.TSAStatus.Codes.Unknown);

			using (ZForm testForm = new ZForm(row))
			using (LocationsEditUSControl control = new LocationsEditUSControl())
			{
				testForm.Controls.Add(control);
				testForm.Show();
				control.SetDataBinding(row, "");

				AssertEquals(false, control.LocationGrid.Columns[WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name].IsVisible);
				AssertEquals(true, control.LocationGrid.Columns[WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name].IsUnavailable);
				AssertNotEquals("", control.LocationGrid.Columns[WhsLocationViewSchema.WLV_ApprovedKnownLocation.Name].ErrorMessageWhenUnavailable);
			}
		}

		#region Implementation

		protected new WhsTestHelperFunctionsEnvUS Helper
		{
			get { return (WhsTestHelperFunctionsEnvUS)base.Helper; }
		}

		protected override WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctionsEnvUS(Factory);
		}

		#endregion
	}
}
