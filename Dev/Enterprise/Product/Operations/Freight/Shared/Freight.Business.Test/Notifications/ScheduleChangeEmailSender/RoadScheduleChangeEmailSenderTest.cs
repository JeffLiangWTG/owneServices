using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(RoadScheduleChangeEmailSender))]
	sealed class RoadScheduleChangeEmailSenderTest : ScheduleChangeEmailSenderTest
	{
		[TestDate(2013, 1, 1)]
		public override void TestSendEmailIfRequired()
		{
			TestSendEmailIfRequired(
				"Trucking",
				"Truck Registration No: Voyage1\r\nCarrier: Carrier",
				"Truck Registration No: Voyage2\r\nCarrier: Carrier", true);
		}

		[TestDate(2013, 1, 1)]
		public override void TestSendEmailIfRequired_NoCarrier()
		{
			TestSendEmailIfRequired(
				"Trucking",
				"Truck Registration No: Voyage1",
				"Truck Registration No: Voyage2", false);
		}

		protected override string TransportMode
		{
			get { return Core.Constants.TransportModes.Road; }
		}

		protected override string TransportModeDescription
		{
			get { return "Trucking"; }
		}

		protected override GuidRegistryItem NotificationGroupRegistryItem
		{
			get { return FreightDataRegistry.Instance.RoadScheduleChangeNotificationGroup; }
		}
	}
}
