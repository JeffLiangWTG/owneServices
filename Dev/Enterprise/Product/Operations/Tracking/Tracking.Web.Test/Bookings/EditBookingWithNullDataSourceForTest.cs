using CargoWise.EntityFramework;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class EditBookingWithNullDataSourceForTest : EditBookingForTest
	{
		protected override BusinessObject GetNewDataSource() => null;
	}
}
