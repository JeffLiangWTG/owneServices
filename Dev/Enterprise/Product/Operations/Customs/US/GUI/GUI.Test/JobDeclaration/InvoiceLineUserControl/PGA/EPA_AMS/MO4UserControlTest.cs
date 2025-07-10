using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class MO4UserControlTest : TestCaseWithFactory
	{
		public void TestUS_ProductNumberColumn()
		{
			using (var control = new MO4UserControl())
			{
				var grid = (ZGrid)control.Controls[0].Controls.Find("MO4Grid", true)[0];
				var column = grid.GetColumnStyle("US_ProductNumber");
				AssertEquals(typeof(ZCodeFindBoxColumnStyleInfo), column.GetType());
				AssertEquals(100, column.Width);
				AssertEquals("AddInfoLookups.ProductNumberCodes", ((ZCodeFindBoxColumnStyleInfo)column).BindToList);
			}
		}
	}
}
