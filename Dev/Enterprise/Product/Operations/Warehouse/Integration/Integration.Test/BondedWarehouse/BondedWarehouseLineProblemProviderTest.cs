using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Integration.BondedWarehouse.Testing
{
	public class BondedWarehouseLineProblemProviderTest : TestCaseWithFactory
	{
		public void TestClearAllNotifications()
		{
			var line = new BondedWarehouseLineProblemProvider();
			AssertEquals(false, line.HasErrors);

			line.WarehouseProblems.ErrorList.Add("E1");
			line.QuantityProblems.ErrorList.Add("E1");
			line.EntryKeyProblems.ErrorList.Add("E1");
			line.PartAttrib1Problems.ErrorList.Add("E1");
			line.PartAttrib2Problems.ErrorList.Add("E1");
			line.PartAttrib3Problems.ErrorList.Add("E1");
			line.SerialNumberProblems.ErrorList.Add("E1");

			line.WarehouseProblems.WarningList.Add("W1");
			line.QuantityProblems.WarningList.Add("W1");
			line.EntryKeyProblems.WarningList.Add("W1");
			line.PartAttrib1Problems.WarningList.Add("W1");
			line.PartAttrib2Problems.WarningList.Add("W1");
			line.PartAttrib3Problems.WarningList.Add("W1");
			line.SerialNumberProblems.WarningList.Add("W1");

			AssertEquals(true, line.HasErrors);
			line.ClearAllNotifications();
			AssertEquals(false, line.HasErrors);

			AssertEquals(0, line.WarehouseProblems.ErrorList.Count);
			AssertEquals(0, line.QuantityProblems.ErrorList.Count);
			AssertEquals(0, line.EntryKeyProblems.ErrorList.Count);
			AssertEquals(0, line.PartAttrib1Problems.ErrorList.Count);
			AssertEquals(0, line.PartAttrib2Problems.ErrorList.Count);
			AssertEquals(0, line.PartAttrib3Problems.ErrorList.Count);
			AssertEquals(0, line.SerialNumberProblems.ErrorList.Count);

			AssertEquals(0, line.WarehouseProblems.WarningList.Count);
			AssertEquals(0, line.QuantityProblems.WarningList.Count);
			AssertEquals(0, line.EntryKeyProblems.WarningList.Count);
			AssertEquals(0, line.PartAttrib1Problems.WarningList.Count);
			AssertEquals(0, line.PartAttrib2Problems.WarningList.Count);
			AssertEquals(0, line.PartAttrib3Problems.WarningList.Count);
			AssertEquals(0, line.SerialNumberProblems.WarningList.Count);
		}

		public void TestHasErrors()
		{
			var line = new BondedWarehouseLineProblemProvider();
			AssertEquals(false, line.HasErrors);

			line.WarehouseProblems.ErrorList.Add("1");
			AssertEquals(true, line.HasErrors);
			line.WarehouseProblems.ErrorList.Clear();
			AssertEquals(false, line.HasErrors);

			line.QuantityProblems.ErrorList.Add("1");
			AssertEquals(true, line.HasErrors);
			line.QuantityProblems.ErrorList.Clear();
			AssertEquals(false, line.HasErrors);

			line.EntryKeyProblems.ErrorList.Add("1");
			AssertEquals(true, line.HasErrors);
			line.EntryKeyProblems.ErrorList.Clear();
			AssertEquals(false, line.HasErrors);

			line.PartAttrib1Problems.ErrorList.Add("1");
			AssertEquals(true, line.HasErrors);
			line.PartAttrib1Problems.ErrorList.Clear();
			AssertEquals(false, line.HasErrors);

			line.PartAttrib2Problems.ErrorList.Add("1");
			AssertEquals(true, line.HasErrors);
			line.PartAttrib2Problems.ErrorList.Clear();
			AssertEquals(false, line.HasErrors);

			line.PartAttrib3Problems.ErrorList.Add("1");
			AssertEquals(true, line.HasErrors);
			line.PartAttrib3Problems.ErrorList.Clear();
			AssertEquals(false, line.HasErrors);

			line.SerialNumberProblems.ErrorList.Add("1");
			AssertEquals(true, line.HasErrors);
			line.SerialNumberProblems.ErrorList.Clear();
			AssertEquals(false, line.HasErrors);

			line.WarehouseProblems.ErrorList.Add("1");
			line.QuantityProblems.ErrorList.Add("1");
			line.EntryKeyProblems.ErrorList.Add("1");
			line.PartAttrib1Problems.ErrorList.Add("1");
			line.PartAttrib2Problems.ErrorList.Add("1");
			line.PartAttrib3Problems.ErrorList.Add("1");
			line.SerialNumberProblems.ErrorList.Add("1");
			AssertEquals(true, line.HasErrors);
		}

		public void TestHasWarnings()
		{
			var line = new BondedWarehouseLineProblemProvider();
			AssertEquals(false, line.HasWarnings);

			line.WarehouseProblems.WarningList.Add("1");
			AssertEquals(true, line.HasWarnings);
			line.WarehouseProblems.WarningList.Clear();
			AssertEquals(false, line.HasWarnings);

			line.QuantityProblems.WarningList.Add("1");
			AssertEquals(true, line.HasWarnings);
			line.QuantityProblems.WarningList.Clear();
			AssertEquals(false, line.HasWarnings);

			line.EntryKeyProblems.WarningList.Add("1");
			AssertEquals(true, line.HasWarnings);
			line.EntryKeyProblems.WarningList.Clear();
			AssertEquals(false, line.HasWarnings);

			line.PartAttrib1Problems.WarningList.Add("1");
			AssertEquals(true, line.HasWarnings);
			line.PartAttrib1Problems.WarningList.Clear();
			AssertEquals(false, line.HasWarnings);

			line.PartAttrib2Problems.WarningList.Add("1");
			AssertEquals(true, line.HasWarnings);
			line.PartAttrib2Problems.WarningList.Clear();
			AssertEquals(false, line.HasWarnings);

			line.PartAttrib3Problems.WarningList.Add("1");
			AssertEquals(true, line.HasWarnings);
			line.PartAttrib3Problems.WarningList.Clear();
			AssertEquals(false, line.HasWarnings);

			line.SerialNumberProblems.WarningList.Add("1");
			AssertEquals(true, line.HasWarnings);
			line.SerialNumberProblems.WarningList.Clear();
			AssertEquals(false, line.HasWarnings);

			line.WarehouseProblems.WarningList.Add("1");
			line.QuantityProblems.WarningList.Add("1");
			line.EntryKeyProblems.WarningList.Add("1");
			line.PartAttrib1Problems.WarningList.Add("1");
			line.PartAttrib2Problems.WarningList.Add("1");
			line.PartAttrib3Problems.WarningList.Add("1");
			line.SerialNumberProblems.WarningList.Add("1");
			AssertEquals(true, line.HasWarnings);
		}

		public void TestClearAllNotificationsIncludingBonded()
		{
			var line = new BondedWarehouseLineProblemProvider();
			AssertEquals(false, line.HasErrors);

			line.QuantityProblems.ErrorList.Add("E1");
			line.BondedWarehouseQuantityProblems.ErrorList.Add("E1");

			line.QuantityProblems.WarningList.Add("W1");
			line.BondedWarehouseQuantityProblems.WarningList.Add("W1");

			AssertEquals(true, line.HasErrors);
			line.ClearAllNotifications();
			AssertEquals(false, line.HasErrors);

			line.BondedWarehouseQuantityProblems.ErrorList.Add("E1");
			AssertEquals(true, line.HasErrors);
			line.ClearAllNotifications();

			AssertEquals(0, line.BondedWarehouseQuantityProblems.ErrorList.Count);
			AssertEquals(0, line.BondedWarehouseQuantityProblems.WarningList.Count);
		}

		public void TestHasErrorsIncludingBonded()
		{
			var line = new BondedWarehouseLineProblemProvider();
			AssertEquals(false, line.HasErrors);

			line.BondedWarehouseQuantityProblems.ErrorList.Add("1");
			AssertEquals(true, line.HasErrors);
			line.BondedWarehouseQuantityProblems.ErrorList.Clear();
			AssertEquals(false, line.HasErrors);

			line.BondedWarehouseQuantityProblems.ErrorList.Add("1");
			AssertEquals(true, line.HasErrors);
		}

		public void TestHasWarningsIncludingBonded()
		{
			var line = new BondedWarehouseLineProblemProvider();
			AssertEquals(false, line.HasWarnings);

			line.BondedWarehouseQuantityProblems.WarningList.Add("1");
			AssertEquals(true, line.HasWarnings);
			line.BondedWarehouseQuantityProblems.WarningList.Clear();
			AssertEquals(false, line.HasWarnings);

			line.BondedWarehouseQuantityProblems.WarningList.Add("1");
			AssertEquals(true, line.HasWarnings);
		}
	}
}
