using CargoWiseOne.ResourceStrings;

namespace Enterprise.TransportConsignment.Business
{
	public static class RunSheetViewExtensions
	{
		public static ResourceStringData GetDescription(this RunSheetView view)
		{
			switch (view)
			{
				case RunSheetView.RunSheets:
					return AllRunSheets;
				case RunSheetView.Carriers:
					return CarrierRunSheets;
				case RunSheetView.Drivers:
					return DriverRunSheets;
				case RunSheetView.Vehicles:
					return VehicleRunSheets;
				default:
					return null;
			}
		}

		static ResourceStringData AllRunSheets { get { return Res.GetData("fabdb8f8-3b43-49b2-8492-4f7c87fce462", "All Run Sheets"); } }
		static ResourceStringData CarrierRunSheets { get { return Res.GetData("f69d0de8-6fc4-4ca0-8bf6-0c1ac309dce4", "Carrier Run Sheets"); } }
		static ResourceStringData DriverRunSheets { get { return Res.GetData("48ab700d-9877-4917-9d0a-6f288fe9f752", "Driver Run Sheets"); } }
		static ResourceStringData VehicleRunSheets { get { return Res.GetData("d1512f7d-31df-4b96-af95-2028e30f6676", "Vehicle Run Sheets"); } }
	}
}
