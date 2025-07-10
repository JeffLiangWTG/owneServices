using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(RailScheduleChangeEmailSender))]
	sealed class RailScheduleChangeEmailSenderTest : ScheduleChangeEmailSenderTest
	{
		[TestDate(2013, 1, 1)]
		public override void TestSendEmailIfRequired()
		{
			TestSendEmailIfRequired(
				"Rail",
				"Journey: Vessel1\r\nJourney No: Voyage1\r\nCarrier: Carrier",
				"Journey: Vessel2\r\nJourney No: Voyage2\r\nCarrier: Carrier", true);
		}

		[TestDate(2013, 1, 1)]
		public override void TestSendEmailIfRequired_NoCarrier()
		{
			TestSendEmailIfRequired(
				"Rail",
				"Journey: Vessel1\r\nJourney No: Voyage1",
				"Journey: Vessel2\r\nJourney No: Voyage2", false);
		}

		protected override string TransportMode
		{
			get { return Core.Constants.TransportModes.Rail; }
		}

		protected override string TransportModeDescription
		{
			get { return "Rail"; }
		}

		protected override GuidRegistryItem NotificationGroupRegistryItem
		{
			get { return FreightDataRegistry.Instance.RailScheduleChangeNotificationGroup; }
		}
	}
}
