using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;

namespace Enterprise.Freight.QuotedBookings.GUI.Test
{
	class ForwardingBookingServicesControlTest : TestCaseWithFactory
	{
		public void TestServicesColumns()
		{
			using (var control = new ForwardingBookingServicesControl())
			{
				control.Show();
				var expectedVisible = new string[] { "ES_ServiceCode", "ES_Calc_Description", "ES_OH_Contractor", "ES_Booked", "ES_Completed" };
				var expectedInvisible = new string[] { "ES_ServiceCount", "ES_Calc_LocationCode", "ES_Duration", "ES_ServiceNote", "ES_References", "ES_ServiceId", "ES_ExternalServiceId", "ServiceProviderPK" };
				var actualVisible = control.ServicesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(column => column.IsVisible).Select(column => column.ColumnName);
				var actualInvisible = control.ServicesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(column => !column.IsVisible).Select(column => column.ColumnName);
				AssertContainsExactElementsInAnyOrder(expectedVisible, actualVisible);
				AssertContainsExactElementsInAnyOrder(expectedInvisible, actualInvisible);
			}
		}
	}
}
