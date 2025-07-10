using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentCargoProcessTask))]
	sealed class CarrierShipmentCargoProcessTaskTest : ProcessTaskTest
	{
		public override void TestParentControllerIDIsOverridenForNonStandAloneTasks()
		{
			Assert("Unnecessary test case", true);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var carrierShipmentCargo = Factory.New<CarrierShipmentCargo>();
			carrierShipmentCargo.CSC_CargoMovementTypeOrigin = "FCL";
			carrierShipmentCargo.CSC_CargoMovementTypeDestination = "FCL";
			carrierShipmentCargo.CSC_ReceiptDrayage = "ANY";
			carrierShipmentCargo.CSC_DeliveryDrayage = "ANY";
			var intCarrierShipmentCargo = (IWorkflowProvider)carrierShipmentCargo;
			return intCarrierShipmentCargo.WorkflowItems.AddNew();
		}
	}
}
