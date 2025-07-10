using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	internal class AgencyContainerReaderTest : ContainerDataObjectReaderTest
	{
		public void TestPopulateBusinessObject_AgencyBooking()
		{
			var booking = Factory.New<AgencyBooking>();
			booking.FillWithValidTestData();
			AssertPopulateBusinessObject(booking, true);
			AssertPopulateBusinessObject(booking, false);
		}

		public void TestPopulateBusinessObject_BillOfLading()
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.FillWithValidTestData();
			AssertPopulateBusinessObject(billOfLading, true);
			AssertPopulateBusinessObject(billOfLading, false);
		}

		public void TestNewContainer_AgencyBooking()
		{
			AssertContainer(Factory.New<AgencyBooking>(), null, false);
			AssertContainer(Factory.New<AgencyBooking>(), null, true);
		}

		public void TestNewContainer_BillOfLading()
		{
			AssertContainer(Factory.New<BillOfLading>(), null, false);
			AssertContainer(Factory.New<BillOfLading>(), null, true);
		}

		public void TestExistingContainer_AgencyBooking()
		{
			var booking = Factory.New<AgencyBooking>();
			var container = booking.BookedContainers.AddNew();
			AssertContainer(booking, container, false);
			booking = Factory.New<AgencyBooking>();
			container = booking.RealContainers.AddNew();
			AssertContainer(booking, container, true);
		}

		public void TestExistingContainer_BillOfLading()
		{
			var billOfLading = Factory.New<BillOfLading>();
			var container = billOfLading.ShippingContainers.AddNew();
			AssertContainer(billOfLading, container, false);
			AssertContainer(billOfLading, container, true);
		}

		void AssertContainer(AgencyShipment agencyShipment, AgencyShipmentContainer container, bool isVgm)
		{
			var dataObject = new Container { ContainerNumber = "AAA", ContainerType = new ContainerType { Code = "20GP", ISOCode = "22G0" }, Seal = "SEAL1", SealPartyType = new CodeDescriptionPair { Code = "CAR", Description = "CAR Carrier/Shipping Line" }, SecondSealPartyType = new CodeDescriptionPair { Code = "CRD", Description = "Consignor/Shipper" }, ThirdSealPartyType = new CodeDescriptionPair { Code = "CUS", Description = "Terminal" } };
			ReadIntoContainer(dataObject, agencyShipment, container, isVgm);
			var shipmentContainers = isVgm && !agencyShipment.IsBillOfLadingStage ? agencyShipment.RealContainers : agencyShipment.ShippingContainers;
			AssertEquals("container found or created", 1, shipmentContainers.Count);
			AssertEquals("JC_ContainerNum", "AAA", shipmentContainers[0].JC_ContainerNum);
			AssertEquals("JC_SealNum", "SEAL1", shipmentContainers[0].JC_SealNum);
			AssertEquals("JC_SealParty", "CAR", shipmentContainers[0].JC_SealParty);
			AssertEquals("JC_AdditionalSealParty", "CRD", shipmentContainers[0].JC_AdditionalSealParty);
			AssertEquals("JC_Additional2SealParty", "CUS", shipmentContainers[0].JC_Additional2SealParty);
		}

		void AssertPopulateBusinessObject(AgencyShipment shipment, bool isVgm)
		{
			shipment.RealContainers.RemoveAndDeleteAll();
			shipment.BookedContainers.RemoveAndDeleteAll();
			var dataObject = new Container { ContainerNumber = "AAA", Seal = "1111" };
			var container = shipment.RealContainers.AddNew();
			container.JC_ContainerNum = "BBB";
			container.JC_SealNum = "0000";
			Factory.Save();
			ReadIntoContainer(dataObject, shipment, container, isVgm);
			AssertEquals(1, shipment.RealContainers.Count);
			AssertEquals(0, shipment.BookedContainers.Count);
			AssertEquals("AAA", shipment.RealContainers[0].JC_ContainerNum);
			AssertEquals("1111", shipment.RealContainers[0].JC_SealNum);
			shipment.RealContainers.RemoveAndDeleteAll();
			container = shipment.BookedContainers.AddNew();
			container.JC_ContainerNum = "CCC";
			container.JC_SealNum = "2222";
			Factory.Save();
			ReadIntoContainer(dataObject, shipment, container, isVgm);
			AssertEquals(1, shipment.BookedContainers.Count);
			AssertEquals("AAA", shipment.BookedContainers[0].JC_ContainerNum);
			AssertEquals("1111", shipment.BookedContainers[0].JC_SealNum);
			AssertEquals(0, shipment.RealContainers.Count);
		}

		void ReadIntoContainer(Container dataObject, AgencyShipment shipment, AgencyShipmentContainer container, bool isVgm)
		{
			var containerInfo = new AgencyContainersInfo(shipment, isVgm);
			var containerReader = new AgencyContainerReader(dataObject, container, containerInfo, new TestErrorLogger(), new UniversalObjectFactory());
			containerReader.ReadIntoBusinessObject();
		}
	}
}
