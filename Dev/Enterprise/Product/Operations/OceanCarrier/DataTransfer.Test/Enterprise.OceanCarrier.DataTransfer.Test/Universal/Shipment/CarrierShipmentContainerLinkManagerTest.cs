using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.OceanCarrier.Business;
using UniversalContainer = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;

namespace Enterprise.OceanCarrier.DataTransfer.Universal.Testing
{
	sealed class CarrierShipmentContainerLinkManagerTest : TestCaseWithFactory
	{
		public void TestWrite()
		{
			var containerLinkManager = new CarrierShipmentContainerLinkManager();

			AssertEquals(1, containerLinkManager.GetContainerLink(container1));
			AssertEquals(2, containerLinkManager.GetContainerLink(container2));
			AssertEquals(3, containerLinkManager.GetContainerLink(container3));
		}

		public void TestRead()
		{
			var containerLinkManager = new CarrierShipmentContainerLinkManager();

			containerLinkManager.CollectContainerLink(container1, new UniversalContainer { Link = 1 });
			containerLinkManager.CollectContainerLink(container2, new UniversalContainer { Link = 2 });
			containerLinkManager.CollectContainerLink(container3, new UniversalContainer { Link = 3 });

			AssertEquals(container1, containerLinkManager.GetContainer(1));
			AssertEquals(container2, containerLinkManager.GetContainer(2));
			AssertEquals(container3, containerLinkManager.GetContainer(3));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var shipment = Factory.New<CarrierShipmentHeader>();
			shipment.CSH_CarrierShipmentReference = "CSH001";

			container1 = CreateCarrierShipmentCargoContainer(shipment, "CONT1111111");
			container2 = CreateCarrierShipmentCargoContainer(shipment, "CONT1111112");
			container3 = CreateCarrierShipmentCargoContainer(shipment, "CONT1111113");
		}

		public CarrierShipmentCargo CreateCarrierShipmentCargoContainer(CarrierShipmentHeader carrierShipmentHeader, ZString containerNumber)
		{
			var cargo = Factory.New<CarrierShipmentCargo>();
			cargo.CSC_CSH_CarrierShipment = carrierShipmentHeader.PK;
			cargo.CSC_IsTopLevel = ZBool.True;
			cargo.CSC_PieceCount = 1;
			cargo.CSC_EquipmentNo = containerNumber;
			return cargo;
		}

		CarrierShipmentCargo container1, container2, container3;
	}
}
