using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentCargoLink))]
	sealed class CarrierShipmentCargoLinkTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var carrierShipmentHeader = Factory.New<CarrierShipmentHeader>();
			carrierShipmentHeader.CSH_CarrierShipmentReference = "CS00001";

			var cargo1 = Factory.New<CarrierShipmentCargo>();
			cargo1.CSC_CSH_CarrierShipment = carrierShipmentHeader.PK;
			cargo1.CSC_CargoID = "CRG00000001";
			cargo1.CSC_CargoMovementTypeOrigin = "FCL";
			cargo1.CSC_CargoMovementTypeDestination = "FCL";
			cargo1.CSC_ChargeableGrossWeightUnit = "KG";
			cargo1.CSC_ChargeableUnitOfDimension = "M";
			cargo1.CSC_ReceiptDrayage = "ANY";
			cargo1.CSC_DeliveryDrayage = "ANY";

			var cargo2 = Factory.New<CarrierShipmentCargo>();
			cargo2.CSC_CSH_CarrierShipment = carrierShipmentHeader.PK;
			cargo2.CSC_CargoID = "CRG00000002";
			cargo2.CSC_CargoMovementTypeOrigin = "FCL";
			cargo2.CSC_CargoMovementTypeDestination = "FCL";
			cargo2.CSC_ChargeableGrossWeightUnit = "KG";
			cargo2.CSC_ChargeableUnitOfDimension = "M";
			cargo2.CSC_ReceiptDrayage = "ANY";
			cargo2.CSC_DeliveryDrayage = "ANY";

			var link = Factory.New<CarrierShipmentCargoLink>();
			link.CCK_CSC_Parent = cargo1.PK;
			link.CCK_CSC_Child = cargo2.PK;

			return link;
		}

		#endregion
	}
}
