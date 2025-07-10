using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(AirScheduleChangeEmailSender))]
	sealed class AirScheduleChangeEmailSenderTest : ScheduleChangeEmailSenderTest
	{
		[TestDate(2013, 1, 1)]
		public override void TestSendEmailIfRequired()
		{
			TestSendEmailIfRequired(
				"Flight",
				"Flight Number: Voyage1\r\nCarrier: Carrier",
				"Flight Number: Voyage2\r\nCarrier: Carrier", true);
		}

		[TestDate(2013, 1, 1)]
		public override void TestSendEmailIfRequired_NoCarrier()
		{
			TestSendEmailIfRequired(
				"Flight",
				"Flight Number: Voyage1",
				"Flight Number: Voyage2", false);
		}

		protected override string TransportMode
		{
			get { return Core.Constants.TransportModes.Air; }
		}

		protected override string TransportModeDescription
		{
			get { return "Flight"; }
		}

		protected override GuidRegistryItem NotificationGroupRegistryItem
		{
			get { return FreightDataRegistry.Instance.AirScheduleChangeNotificationGroup; }
		}
	}
}
