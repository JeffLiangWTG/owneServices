using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.eTail.Module.Testing
{
	class HVLVBookingHeaderFilterControlTest : TestCase
	{
		public void TestIsBookingReceivedColumn()
		{
			using (var form = new ZForm())
			using (var module = new HVLVBookingHeaderModule())
			{
				var filter = (HVLVBookingHeaderFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				AssertNotNull("The column Received at Origin should be added", filter.FilteredGrid.GetColumnStyle("HVH_IsBookingReceived"));
			}
		}

		public void TestCalculatedAggregatPropertiesNotInColumns()
		{
			using (var form = new ZForm())
			using (var module = new HVLVBookingHeaderModule())
			{
				var filter = (HVLVBookingHeaderFilterControl)module.EmbeddedControl;
				form.Controls.Add(filter);
				form.Show();

				AssertNull("The column Item Count should not be in grid", filter.FilteredGrid.GetColumnStyle("HVH_ItemCount"));
				AssertNull("The column Gross Volume should not be in grid", filter.FilteredGrid.GetColumnStyle("HVH_GrossVolume"));
				AssertNull("The column Gross Weight should not be in grid", filter.FilteredGrid.GetColumnStyle("HVH_GrossWeight"));
			}
		}
	}
}
