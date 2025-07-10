using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class PN1UserControlTest : TestCaseWithFactory
	{
		public void TestUS_ProductNumberColumn()
		{
			using (var control = new PN1UserControl())
			{
				var grid = (ZGrid)control.Controls[0].Controls.Find("PN1Grid", true)[0];
				var column = grid.GetColumnStyle("US_ProductNumber");
				AssertEquals(typeof(ZDropEditColumnStyleInfo), column.GetType());
				AssertEquals(100, column.Width);
				AssertEquals("AddInfoLookups.ProductNumberCodes", ((ZDropEditColumnStyleInfo)column).BindToList);
			}
		}
	}
}
