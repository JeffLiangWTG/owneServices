using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.Module.Testing
{
	sealed class NctsMovementFilterControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			using (var userControl = new NctsMovementFilterControl(new NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject()))
			{
				var grid = userControl.Grid;
				CombineAssertions("Assert columns", () =>
				{
					AssertColumn("Lrn Registration Number", "LrnRegistrationNumber", grid, "LRN");
					AssertColumn("Lrn Registration Date", "LrnRegistrationDate", grid, "LRN Date");
				});
			}
		}

		void AssertColumn(string humanColumnName, string columnName, ZFilterGrid grid, string expectedCaption)
		{
			var columnStyle = grid.GetColumnStyle(columnName);

			AssertNotNull($"{humanColumnName} column", columnStyle);
			if (columnStyle != null)
			{
				AssertEquals($"{columnName} is visible", true, columnStyle.IsVisible);
				AssertEquals($"{columnName} caption", expectedCaption, columnStyle.CaptionResourceString != null ? columnStyle.CaptionResourceString.GetCaptions()[0] : columnStyle.Caption);
			}
		}
	}
}
