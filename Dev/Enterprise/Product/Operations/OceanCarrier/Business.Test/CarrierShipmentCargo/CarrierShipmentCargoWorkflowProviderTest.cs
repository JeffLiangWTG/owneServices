using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentCargo))]
	sealed class CarrierShipmentCargoWorkflowProviderTest : WorkflowProviderTest<CarrierShipmentCargo, CarrierShipmentCargoProcessTaskCollection>
	{
		int lastUsedShipmentReferenceSequence;
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CarrierShipmentCargoWorkflowDescriptorCode;

		protected override CarrierShipmentCargo GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var carrierShipmentHeader = Factory.NewWithValidTestData<CarrierShipmentHeader>();
			carrierShipmentHeader.CSH_CarrierShipmentReference = $"CS{++lastUsedShipmentReferenceSequence:D8}";
			var cargo = Factory.NewWithValidTestData<CarrierShipmentCargo>();
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			cargo.CSC_CSH_CarrierShipment = carrierShipmentHeader.PK;
			cargo.CSC_CargoMovementTypeOrigin = "FCL";
			cargo.CSC_CargoMovementTypeDestination = "FCL";
			cargo.CSC_ReceiptDrayage = "ANY";
			cargo.CSC_DeliveryDrayage = "ANY";
			cargo.CSC_RC_ChargeableEquipmentType = refContainer.PK;
			return cargo;
		}
	}
}
