using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	[Serializable]
	internal class RoadScheduleChangeEmailSender : ScheduleChangeEmailSender
	{
		protected override string TransportMode
		{
			get { return Core.Constants.TransportModes.Road; }
		}

		protected override string TransportModeDescription
		{
			get { return Res.GetString("3b0a39d4-0ddb-41c3-85f7-71dc93e4b20e", "Trucking"); }
		}

		protected override string VoyageLabel
		{
			get { return Res.GetString("6e6f0931-3a6d-4617-9100-6de0c01b414e", "Truck Registration No"); }
		}

		protected override GuidRegistryItem NotificationGroup
		{
			get { return FreightDataRegistry.Instance.RoadScheduleChangeNotificationGroup; }
		}
	}
}
