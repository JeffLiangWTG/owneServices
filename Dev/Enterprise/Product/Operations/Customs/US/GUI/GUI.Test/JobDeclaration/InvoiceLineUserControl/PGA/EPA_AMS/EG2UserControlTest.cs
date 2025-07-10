using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class EG2UserControlTest : TestCaseWithFactory
	{
		public void TestUS_ProductNumberColumn()
		{
			using (var control = new EG2UserControl())
			{
				var grid = (ZGrid)control.Controls[0].Controls.Find("EG2Grid", true)[0];
				var column = grid.GetColumnStyle("US_ProductNumber");
				AssertEquals(typeof(ZDropEditColumnStyleInfo), column.GetType());
				AssertEquals(160, column.Width);
			}
		}
	}
}
