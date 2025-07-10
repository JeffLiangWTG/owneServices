namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class TG_PreventUnpickedPickLinesOnFinalisedDocketLinesTest : WhsTestCaseWithFactory
	{
		#region TestTrigger_FinaliseWhsDocketLineWithPickedPickline

		public abstract void TestTrigger_FinaliseWhsDocketLineWithPickedPickline();

		#endregion

		#region TestTrigger_FinaliseWhsDocketLineWithUnpickedPickline

		public abstract void TestTrigger_FinaliseWhsDocketLineWithUnpickedPickline();

		#endregion
	}
}
