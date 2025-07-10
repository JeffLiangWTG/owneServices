using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class RunSheetViewExtensionsTest : TestCase
	{
		#region TestGetDescription

		public void TestGetDescription()
		{
			AssertEquals("All Run Sheets", RunSheetView.RunSheets.GetDescription().Caption);
			AssertEquals("Carrier Run Sheets", RunSheetView.Carriers.GetDescription().Caption);
			AssertEquals("Driver Run Sheets", RunSheetView.Drivers.GetDescription().Caption);
			AssertEquals("Vehicle Run Sheets", RunSheetView.Vehicles.GetDescription().Caption);
		}

		#endregion
	}
}
