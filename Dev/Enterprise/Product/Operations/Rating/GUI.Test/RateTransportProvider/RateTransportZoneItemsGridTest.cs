using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Rating.GUI.Testing
{
	public class RateTransportZoneItemsGridTest : TestCaseWithFactory
	{
		public void TestIsAbleToImportData()
		{
			var provider = Factory.New<RateTransportProvider>();
			var zone = Factory.New<RateTransportZone>();
			zone.TZ_TP = provider.PK;

			using (var grid = new RateTransportZoneItemsGridForTest())
			{
				AssertEquals(true, grid.IsAbleToImportDataExposed);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

				grid.TransportProvider = provider;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals(false, grid.IsAbleToImportDataExposed);
				AssertEquals("Please enter a valid Zone Country/Region before importing data.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				provider.TP_RN_NKCountry = "1";
				AssertEquals(false, grid.IsAbleToImportDataExposed);
				AssertEquals("Please enter a valid Zone Country/Region before importing data.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				provider.TP_RN_NKCountry = Core.Constants.CountryCodes.Australia;
				AssertEquals(true, grid.IsAbleToImportDataExposed);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		class RateTransportZoneItemsGridForTest : RateTransportZoneItemsGrid
		{
			public bool IsAbleToImportDataExposed => IsAbleToImportData;
		}
	}
}
