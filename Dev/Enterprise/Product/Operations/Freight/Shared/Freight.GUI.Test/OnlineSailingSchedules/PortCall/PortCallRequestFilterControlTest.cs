using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.OnlineSailingSchedules.PortCall;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class PortCallRequestFilterControlTest : TestCaseWithFactory
	{
		public void TestFilterdGridColumns()
		{
			var manager = new PortCallManager(Factory);
			using (var form = new PortCallRequestForm(manager))
			{
				var filterControl = form.FilterControl;
				form.Show();

				TestGridColumns(filterControl.FilteredGrid, "VesselName", "Vessel Name", true);
				TestGridColumns(filterControl.FilteredGrid, "IMO", "IMO", true);
				TestGridColumns(filterControl.FilteredGrid, "CallSign", "Call Sign", true);
				TestGridColumns(filterControl.FilteredGrid, "CarrierCode", "Carrier Code", true);
				TestGridColumns(filterControl.FilteredGrid, "VoyageNumber", "Voyage Number", true);
				TestGridColumns(filterControl.FilteredGrid, "ReferenceNumber", "Reference Number", true);
				TestGridColumns(filterControl.FilteredGrid, "EstimatedTime", "ETAETD", true);
			}
		}

		void TestGridColumns(ZGrid grid, string columnName, string caption, bool isVisible)
		{
			var columnStyle = grid.GetColumnStyle(columnName);
			AssertNotNull(columnStyle);
			AssertNotNull(columnStyle.CaptionResourceString);
			AssertEquals(caption, columnStyle.CaptionResourceString.Caption);
			AssertEquals(isVisible, columnStyle.IsVisible);
		}
	}
}
