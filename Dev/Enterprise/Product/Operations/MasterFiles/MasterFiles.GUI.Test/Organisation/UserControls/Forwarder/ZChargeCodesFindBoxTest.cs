using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class ZChargeCodesFindBoxTest : TestCaseWithDummy
	{
		public void TestColumnTextAtRow_WhenNoCurrentPosition()
		{
			SuperDummyBusinessObject superDummy = Factory.New<SuperDummyBusinessObject>();

			using (ZChargeCodesFindBoxColumnStyleTestForm form = new ZChargeCodesFindBoxColumnStyleTestForm(superDummy, false))
			{
				form.Show();
				ZChargeCodesFindBoxColumnStyle columnStyle = (ZChargeCodesFindBoxColumnStyle)form.zGrid1.Columns[1].ColumnStyle;
				form.zGrid1.ListManager.Position = -1;
				string columnText = columnStyle.ColumnTextAtRow(form.zGrid1.ListManager, -1);
				AssertEquals("ColumnTextAtRow value when no current position", "", columnText);
			}
		}
	}
}
