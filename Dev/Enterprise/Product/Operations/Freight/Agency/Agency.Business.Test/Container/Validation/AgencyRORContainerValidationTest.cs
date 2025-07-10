using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyRORContainerValidationTest : AgencyTopLevelPackValidationTest
	{
		public void TestJC_ContainerMode_ROR()
		{
			var container = Factory.New<AgencyShipmentContainer>();
			container.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;
			AssertNoErrors(container.JC_ContainerModeInfo);
			container.JC_ContainerMode = "XXX";
			AssertHasErrors(container.JC_ContainerModeInfo);
		}

		public void TestJC_ContainerNum_ROR()
		{
			var shipment = Factory.New<AgencyShipment>();
			shipment.JS_PackingMode = Constants.ContainerModes.RollOnRollOff;
			var container = shipment.RealContainers.AddNew();
			container.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;
			AssertNoErrors(container.JC_ContainerNumInfo);
			container.JC_ContainerNum = "XXX";
			AssertNoErrors(container.JC_ContainerNumInfo);
			container.JC_ContainerNum = "";
			AssertNoErrors(container.JC_ContainerNumInfo);
			container.JC_ContainerNum = "123456789012345678";
			AssertHasError(container.JC_ContainerNumInfo, "VIN/serial number can be no more than 17 characters long");
			container.JC_ContainerNum = "12345678901234567";
			AssertNoErrors(container.JC_ContainerNumInfo);
			container.JC_ContainerCount = 2;
			container.Validation.ValidateJC_ContainerNum();
			AssertHasError(container.JC_ContainerNumInfo, "You can't enter a VIN if the vehicle count is greater than one.");
			container.JC_ContainerCount = 1;
			AssertNoErrors(container.JC_ContainerNumInfo);
			var container1 = shipment.RealContainers.AddNew();
			container1.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;
			container1.JC_ContainerNum = "12345678901234567";
			AssertHasError(container1.JC_ContainerNumInfo, "Duplicate Vehicle Number is entered.");
			container1.JC_ContainerNum = "123456789012345";
			AssertNoErrors(container1.JC_ContainerNumInfo);
		}

		#region Implementation
		protected override ZString ContainerModeForTesting
		{
			get
			{
				return Constants.ContainerModes.RollOnRollOff;
			}
		}
		#endregion
	}
}
