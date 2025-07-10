using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	[Serializable]
	internal class RailScheduleChangeEmailSender : ScheduleChangeEmailSender
	{
		protected override string TransportMode
		{
			get { return Core.Constants.TransportModes.Rail; }
		}

		protected override string TransportModeDescription
		{
			get { return Res.GetString("d2b59777-5493-4722-a808-7151701a35f0", "Rail"); }
		}

		protected override string VesselNameLabel
		{
			get { return Res.GetString("b823a49b-8fd8-47cc-a526-2cfdf73e0d1e", "Journey"); }
		}

		protected override string VoyageLabel
		{
			get { return Res.GetString("9c9a65a1-84f9-4d2c-8093-c035cd3a75f8", "Journey No"); }
		}

		protected override GuidRegistryItem NotificationGroup
		{
			get { return FreightDataRegistry.Instance.RailScheduleChangeNotificationGroup; }
		}
	}
}
