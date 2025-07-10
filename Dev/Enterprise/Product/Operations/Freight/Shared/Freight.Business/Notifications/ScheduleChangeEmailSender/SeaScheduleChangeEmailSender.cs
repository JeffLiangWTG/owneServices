using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	[Serializable]
	internal class SeaScheduleChangeEmailSender : ScheduleChangeEmailSender
	{
		protected override string TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		protected override string TransportModeDescription
		{
			get { return Res.GetString("68cbf23c-e591-4550-85ec-97d02510597b", "Sailing"); }
		}

		protected override string VesselNameLabel
		{
			get { return Res.GetString("d0b976fd-e07a-4c3d-8566-bf050e631e95", "Vessel"); }
		}

		protected override string VoyageLabel
		{
			get { return Res.GetString("6cac34a6-81f9-4d3c-b9ca-f8b4ed2cd177", "Voyage"); }
		}

		protected override GuidRegistryItem NotificationGroup
		{
			get { return FreightDataRegistry.Instance.SeaScheduleChangeNotificationGroup; }
		}
	}
}
