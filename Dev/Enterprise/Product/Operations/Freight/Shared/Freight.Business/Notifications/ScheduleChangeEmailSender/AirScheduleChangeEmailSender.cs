using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	[Serializable]
	internal class AirScheduleChangeEmailSender : ScheduleChangeEmailSender
	{
		protected override string TransportMode
		{
			get { return Core.Constants.TransportModes.Air; }
		}

		protected override string TransportModeDescription
		{
			get { return Res.GetString("4a4c6485-b81d-4ca0-a7ef-17a69ca8c30a", "Flight"); }
		}

		protected override string VoyageLabel
		{
			get { return Res.GetString("d2813ca4-aaef-41af-8a4b-f5acc9e88f37", "Flight Number"); }
		}

		protected override GuidRegistryItem NotificationGroup
		{
			get { return FreightDataRegistry.Instance.AirScheduleChangeNotificationGroup; }
		}
	}
}
