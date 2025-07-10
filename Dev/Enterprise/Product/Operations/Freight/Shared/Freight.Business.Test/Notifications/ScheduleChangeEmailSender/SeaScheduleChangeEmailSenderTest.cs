using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(SeaScheduleChangeEmailSender))]
	sealed class SeaScheduleChangeEmailSenderTest : ScheduleChangeEmailSenderTest
	{
		[TestDate(2013, 1, 1)]
		public override void TestSendEmailIfRequired()
		{
			TestSendEmailIfRequired(
				"Sailing",
				"Vessel: Vessel1\r\nVoyage: Voyage1\r\nCarrier: Carrier",
				"Vessel: Vessel2\r\nVoyage: Voyage2\r\nCarrier: Carrier", true);
		}

		public override void TestSendEmailIfRequired_NoCarrier()
		{
			Assert("Test not applicable, as Carrier is mandatory for Sea Sailing Schedules", true);
		}

		[TestDate(2013, 1, 1)]
		public void TestSendEmailIfRequired_WithDataProvider()
		{
			TestSendEmailIfRequired(
				"Sailing",
				"Vessel: Vessel1\r\nVoyage: Voyage1\r\nCarrier: Carrier",
				"Vessel: Vessel2\r\nVoyage: Voyage2\r\nCarrier: Carrier", true, FreightConstants.VesselDataProviders.OneStop);
		}

		protected override string TransportMode
		{
			get { return Core.Constants.TransportModes.Sea; }
		}

		protected override string TransportModeDescription
		{
			get { return "Sailing"; }
		}

		protected override GuidRegistryItem NotificationGroupRegistryItem
		{
			get { return FreightDataRegistry.Instance.SeaScheduleChangeNotificationGroup; }
		}
	}
}
