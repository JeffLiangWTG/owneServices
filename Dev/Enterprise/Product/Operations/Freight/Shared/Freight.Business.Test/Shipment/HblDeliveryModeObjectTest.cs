using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class HBLDeliveryModeObjectTest : TestCaseWithFactory
	{
		public void TestInvalid()
		{
			var hblDeliveryMode = HBLDeliveryModeObject.Invalid;
			AssertEquals("is empty", false, hblDeliveryMode.IsEmpty);
			AssertEquals("LogString mismatch", "-Invalid-", hblDeliveryMode.LogString);

			CheckInvalidDeliveryMode(hblDeliveryMode);
		}

		public void TestGetInvalid()
		{
			var hblDeliveryMode = HBLDeliveryModeObject.Get("not a valid value");
			AssertEquals("Does not equal Invalid", HBLDeliveryModeObject.Invalid, hblDeliveryMode);
			AssertEquals("Is empty", false, hblDeliveryMode.IsEmpty);

			CheckInvalidDeliveryMode(hblDeliveryMode);
		}

		public void TestEmpty()
		{
			var hblDeliveryMode = HBLDeliveryModeObject.Empty;
			AssertEquals("Is not empty", true, hblDeliveryMode.IsEmpty);
			AssertEquals("LogString mismatch", "-", hblDeliveryMode.LogString);

			CheckInvalidDeliveryMode(hblDeliveryMode);
		}

		public void TestGetEmpty()
		{
			var hblDeliveryMode = HBLDeliveryModeObject.Get("");
			AssertEquals("Does not equal Empty", HBLDeliveryModeObject.Empty, hblDeliveryMode);
			AssertEquals("Is not empty", true, hblDeliveryMode.IsEmpty);

			CheckInvalidDeliveryMode(hblDeliveryMode);
		}

		public void TestGetNull()
		{
			var hblDeliveryMode = HBLDeliveryModeObject.Get(null);
			AssertEquals(HBLDeliveryModeObject.Empty, hblDeliveryMode);

			CheckInvalidDeliveryMode(hblDeliveryMode);
		}

		void CheckInvalidDeliveryMode(HBLDeliveryModeObject hblDeliveryMode)
		{
			AssertEquals("Is valid", false, hblDeliveryMode.IsValid);

			AssertEquals("Has pickup endpoint", EndpointType.None, hblDeliveryMode.PickupEndpointType);
			AssertEquals("Has delivery endpoint", EndpointType.None, hblDeliveryMode.DeliveryEndpointType);

			AssertEquals("Supports DDD", false, hblDeliveryMode.SupportsDDD);
			AssertEquals("Supports DTC",  false,hblDeliveryMode.SupportsDTC);

			AssertEquals("Does require Service Level", false, hblDeliveryMode.RequiresServiceLevel);
			AssertEquals("Does require Pickup CFS", false, hblDeliveryMode.RequiresPickupCFS);
			AssertEquals("Does require Delivery CFS", false, hblDeliveryMode.RequiresDeliveryCFS);

			AssertEquals("Is Airport pickup", false, hblDeliveryMode.IsAirportPickup);
			AssertEquals("Is CFS pickup", false, hblDeliveryMode.IsCFSPickup);
			AssertEquals("Is CY pickup", false, hblDeliveryMode.IsCYPickup);
			AssertEquals("Is Door pickup", false, hblDeliveryMode.IsDoorPickup);
			AssertEquals("Is Port pickup", false, hblDeliveryMode.IsPortPickup);
			AssertEquals("Is Airport delivery", false, hblDeliveryMode.IsAirportDelivery);
			AssertEquals("Is CFS delivery", false, hblDeliveryMode.IsCFSDelivery);
			AssertEquals("Is CY delivery", false, hblDeliveryMode.IsCYDelivery);
			AssertEquals("Is Door delivery", false, hblDeliveryMode.IsDoorDelivery);
			AssertEquals("Is Port delivery", false, hblDeliveryMode.IsPortDelivery);
		}

		public void TestDoorToDoor()
		{
			var hblDeliveryMode = HBLDeliveryModeObject.Get(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR);

			AssertNotNull(hblDeliveryMode);
			AssertEquals(false, hblDeliveryMode.IsEmpty);
			AssertEquals(true, hblDeliveryMode.IsValid);
			AssertEquals(Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR, hblDeliveryMode.Name);
			AssertEquals(Core.Constants.HBLDeliveryModes.Descriptions.DOOR_DOOR, hblDeliveryMode.Description);

			AssertEquals("Does not support DDD", true, hblDeliveryMode.SupportsDDD);
			AssertEquals("Does not support DTC", true, hblDeliveryMode.SupportsDTC);

			AssertEquals("Does not require Service Level", true, hblDeliveryMode.RequiresServiceLevel);
			AssertEquals("Does not require Pickup CFS", true, hblDeliveryMode.RequiresPickupCFS);
			AssertEquals("Does not require Delivery CFS", true, hblDeliveryMode.RequiresDeliveryCFS);

			AssertEquals("Incorrect pickup endpoint type", hblDeliveryMode.PickupEndpointType, EndpointType.Door);
			AssertEquals("Incorrect delivery endpoint type", hblDeliveryMode.DeliveryEndpointType, EndpointType.Door);

			AssertEquals("Is Airport pickup", false, hblDeliveryMode.IsAirportPickup);
			AssertEquals("Is CFS pickup", false, hblDeliveryMode.IsCFSPickup);
			AssertEquals("Is CY pickup", false, hblDeliveryMode.IsCYPickup);
			AssertEquals("Is not Door pickup", true, hblDeliveryMode.IsDoorPickup);
			AssertEquals("Is Port pickup", false, hblDeliveryMode.IsPortPickup);
			AssertEquals("Is Airport delivery", false, hblDeliveryMode.IsAirportDelivery);
			AssertEquals("Is CFS delivery", false, hblDeliveryMode.IsCFSDelivery);
			AssertEquals("Is CY delivery", false, hblDeliveryMode.IsCYDelivery);
			AssertEquals("Is not Door delivery", true, hblDeliveryMode.IsDoorDelivery);
			AssertEquals("Is Port delivery", false, hblDeliveryMode.IsPortDelivery);
		}

		public void TestCFSToCY()
		{
			var hblDeliveryMode = HBLDeliveryModeObject.Get(Core.Constants.HBLDeliveryModes.Codes.CFS_CY);

			AssertNotNull(hblDeliveryMode);
			AssertEquals(false, hblDeliveryMode.IsEmpty);
			AssertEquals(true, hblDeliveryMode.IsValid);
			AssertEquals(Core.Constants.HBLDeliveryModes.Codes.CFS_CY, hblDeliveryMode.Name);
			AssertEquals(Core.Constants.HBLDeliveryModes.Descriptions.CFS_CY, hblDeliveryMode.Description);

			AssertEquals("Supports DDD", false, hblDeliveryMode.SupportsDDD);
			AssertEquals("Supports DTC", false, hblDeliveryMode.SupportsDTC);

			AssertEquals("Requires Service Level", false, hblDeliveryMode.RequiresServiceLevel);
			AssertEquals("Requires Pickup CFS", false, hblDeliveryMode.RequiresPickupCFS);
			AssertEquals("Requires Delivery CFS", false, hblDeliveryMode.RequiresDeliveryCFS);

			AssertEquals("Incorrect pickup endpoint type", hblDeliveryMode.PickupEndpointType, EndpointType.CFS);
			AssertEquals("Incorrect delivery endpoint type", hblDeliveryMode.DeliveryEndpointType, EndpointType.CY);

			AssertEquals("Is Airport pickup", false, hblDeliveryMode.IsAirportPickup);
			AssertEquals("Is not CFS pickup", true, hblDeliveryMode.IsCFSPickup);
			AssertEquals("Is CY pickup", false, hblDeliveryMode.IsCYPickup);
			AssertEquals("Is Door pickup", false, hblDeliveryMode.IsDoorPickup);
			AssertEquals("Is Port pickup", false, hblDeliveryMode.IsPortPickup);
			AssertEquals("Is Airport delivery", false, hblDeliveryMode.IsAirportDelivery);
			AssertEquals("Is CFS delivery", false, hblDeliveryMode.IsCFSDelivery);
			AssertEquals("Is not CY delivery", true, hblDeliveryMode.IsCYDelivery);
			AssertEquals("Is Door delivery", false, hblDeliveryMode.IsDoorDelivery);
			AssertEquals("Is Port delivery", false, hblDeliveryMode.IsPortDelivery);
		}

		public void TestAirportToAirport()
		{
			var hblDeliveryMode = HBLDeliveryModeObject.Get(Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT);

			AssertNotNull(hblDeliveryMode);
			AssertEquals(false, hblDeliveryMode.IsEmpty);
			AssertEquals(true, hblDeliveryMode.IsValid);
			AssertEquals(Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT, hblDeliveryMode.Name);
			AssertEquals(Core.Constants.HBLDeliveryModes.Descriptions.ARPT_ARPT, hblDeliveryMode.Description);

			AssertEquals("Does not support DDD", true, hblDeliveryMode.SupportsDDD);
			AssertEquals("Supports DTC", false, hblDeliveryMode.SupportsDTC);

			AssertEquals("Requires Service Level", false, hblDeliveryMode.RequiresServiceLevel);
			AssertEquals("Requires Pickup CFS", false, hblDeliveryMode.RequiresPickupCFS);
			AssertEquals("Requires Delivery CFS", false, hblDeliveryMode.RequiresDeliveryCFS);

			AssertEquals("Incorrect pickup endpoint type", hblDeliveryMode.PickupEndpointType, EndpointType.Airport);
			AssertEquals("Incorrect delivery endpoint type", hblDeliveryMode.DeliveryEndpointType, EndpointType.Airport);

			AssertEquals("Is not Airport pickup", true, hblDeliveryMode.IsAirportPickup);
			AssertEquals("Is CFS pickup", false, hblDeliveryMode.IsCFSPickup);
			AssertEquals("Is CY pickup", false, hblDeliveryMode.IsCYPickup);
			AssertEquals("Is Door pickup", false, hblDeliveryMode.IsDoorPickup);
			AssertEquals("Is Port pickup", false, hblDeliveryMode.IsPortPickup);
			AssertEquals("Is not Airport delivery", true, hblDeliveryMode.IsAirportDelivery);
			AssertEquals("Is CFS delivery", false, hblDeliveryMode.IsCFSDelivery);
			AssertEquals("Is CY delivery", false, hblDeliveryMode.IsCYDelivery);
			AssertEquals("Is Door delivery", false, hblDeliveryMode.IsDoorDelivery);
			AssertEquals("Is Port delivery", false, hblDeliveryMode.IsPortDelivery);
		}

		public void TestAirportToCFS()
		{
			var hblDeliveryMode = HBLDeliveryModeObject.Get(Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS);

			AssertNotNull(hblDeliveryMode);
			AssertEquals(false, hblDeliveryMode.IsEmpty);
			AssertEquals(true, hblDeliveryMode.IsValid);
			AssertEquals(Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS, hblDeliveryMode.Name);
			AssertEquals(Core.Constants.HBLDeliveryModes.Descriptions.ARPT_CFS, hblDeliveryMode.Description);

			AssertEquals("Does not support DDD", true, hblDeliveryMode.SupportsDDD);
			AssertEquals("Supports DTC", false, hblDeliveryMode.SupportsDTC);

			AssertEquals("Does not require Service Level", true, hblDeliveryMode.RequiresServiceLevel);
			AssertEquals("Requires Pickup CFS", false, hblDeliveryMode.RequiresPickupCFS);
			AssertEquals("Does not require Delivery CFS", true, hblDeliveryMode.RequiresDeliveryCFS);

			AssertEquals("Incorrect pickup endpoint type", hblDeliveryMode.PickupEndpointType, EndpointType.Airport);
			AssertEquals("Incorrect delivery endpoint type", hblDeliveryMode.DeliveryEndpointType, EndpointType.CFS);

			AssertEquals("Is not Airport pickup", true, hblDeliveryMode.IsAirportPickup);
			AssertEquals("Is CFS pickup", false, hblDeliveryMode.IsCFSPickup);
			AssertEquals("Is CY pickup", false, hblDeliveryMode.IsCYPickup);
			AssertEquals("Is Door pickup", false, hblDeliveryMode.IsDoorPickup);
			AssertEquals("Is Port pickup", false, hblDeliveryMode.IsPortPickup);
			AssertEquals("Is Airport delivery", false, hblDeliveryMode.IsAirportDelivery);
			AssertEquals("Is not CFS delivery", true, hblDeliveryMode.IsCFSDelivery);
			AssertEquals("Is CY delivery", false, hblDeliveryMode.IsCYDelivery);
			AssertEquals("Is Door delivery", false, hblDeliveryMode.IsDoorDelivery);
			AssertEquals("Is Port delivery", false, hblDeliveryMode.IsPortDelivery);
		}

		public void TestListAllModeNames()
		{
			var modeNames = HBLDeliveryModeObject.ListAllModeNames().ToArray();

			AssertEquals("Not all mode names are distinct", modeNames.Length, modeNames.Distinct().Count());

			foreach (var modeName in modeNames)
			{
				var mode = HBLDeliveryModeObject.Get(modeName);
				AssertEquals("Mode is not valid", true, mode.IsValid);
				AssertEquals("Name does not match", mode.Name, modeName);
			}

			var allModes = HBLDeliveryModeObject.ListAllModes().ToArray();
			AssertEquals("All modes length", allModes.Length, modeNames.Length);

			AssertEquals("Not all mode have distinct names", allModes.Length, allModes.Select(mode => mode.Name).Distinct().Count());
		}

		public void TestListAllModes()
		{
			var allModes = HBLDeliveryModeObject.ListAllModes().ToArray();

			foreach (var mode in allModes)
			{
				AssertEquals("LogString mismatch", mode.Name, mode.LogString);
				AssertNotEquals($"No Pickup endpoint {mode}", EndpointType.None, mode.PickupEndpointType);
				AssertNotEquals($"No Delivery endpoint {mode}", EndpointType.None, mode.DeliveryEndpointType);
			}
		}
	}
}
