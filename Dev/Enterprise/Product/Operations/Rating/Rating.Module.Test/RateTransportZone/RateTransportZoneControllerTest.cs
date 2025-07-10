using System;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Module.Testing
{
	[TestedType(typeof(RateTransportZoneController))]
	public class RateTransportZoneControllerTest : ZControllerBasherTest
	{
		public override void TestNewForm()
		{
			var rateTransportZoneController = new RateTransportZoneController();
			var result = rateTransportZoneController.ShowNewForm();
			AssertNull(result);

			var expectedMessage = "To add a new Transport Zone, please either create a new Transport Zone Set from the Maintain --> Locations --> Transport Zone Sets menu, or locate the existing Transport Zone Set and edit it.";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("New", UnitTestUserNotification.Instance.LastMessage.Caption);
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(RateTransportZone);
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RateTransportZone;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var provider = Factory.NewWithValidTestData<RateTransportProvider>();
			var zone = provider.Zones.AddNew();
			zone.FillWithValidTestData();
			Factory.Save();

			return zone;
		}
	}
}
