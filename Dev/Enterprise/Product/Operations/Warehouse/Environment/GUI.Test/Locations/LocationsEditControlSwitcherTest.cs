using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
#if DEBUG
	using Enterprise.Warehouse.Environment.Business.US.Testing;

	public class LocationsEditControlTest : WhsGuiTestCaseWithFactory
	{
		#region General

		public void TestLocationEditControlType()
		{
			WhsWarehouse whs = Helper.CreateWarehouse("WHS");
			WhsRow row = Helper.CreateRow(whs, "ROW");
			using (LocationsEditControlSwitcher control = new LocationsEditControlSwitcher())
			{
				control.SetDataBinding(row, null);
				AssertEquals(typeof(LocationsEditBaseControl), control.RowLocationEditControl.GetType());
			}
		}

		#endregion

		#region Country Specific

		public void TestLocationEditControlTypeForUS()
		{
			WhsTestHelperFunctionsEnvUS helper = new WhsTestHelperFunctionsEnvUS(Factory);
			WhsWarehouse whs = helper.CreateWarehouse("WHS");
			WhsRow row = helper.CreateRow(whs, "ROW");
			using (LocationsEditControlSwitcher control = new LocationsEditControlSwitcher())
			{
				control.SetDataBinding(row, null);
				AssertEquals(typeof(US.LocationsEditUSControl), control.RowLocationEditControl.GetType());
			}
		}

		#endregion

	}
#endif
}
