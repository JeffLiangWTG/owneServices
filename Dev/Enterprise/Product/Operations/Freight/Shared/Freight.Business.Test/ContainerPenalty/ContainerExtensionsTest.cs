using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ContainerExtensionsTest : TestCaseWithFactory
	{
		public void TestGetRelatedShipmentForDefaulting()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			var consignee3 = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_CarrierContractNumber = "CN0001";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();

			Factory.Save();

			AssertEquals("Precondition", false, container.GetRelatedShipmentsForPenaltyDefaulting(ContainerPenaltyProcessType.Delivery).Any());

			var shipment3 = consol.Shipments.AddNew();
			shipment3.JS_UniqueConsignRef = "JS003";
			shipment3.ConsigneePK = consignee3.PK;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "JS002";
			shipment2.ConsigneePK = consignee2.PK;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "JS001";
			shipment1.ConsigneePK = consignee1.PK;

			AssertEquals(shipment1, container.GetRelatedShipmentsForPenaltyDefaulting(ContainerPenaltyProcessType.Delivery).FirstOrDefault());

			var packLine1 = shipment1.OuterPackLines.AddNew();
			var packLine2 = shipment2.OuterPackLines.AddNew();
			var packLine3 = shipment3.OuterPackLines.AddNew();
			packLine1.Containers.RemoveAll();
			packLine2.Containers.RemoveAll();
			packLine3.Containers.RemoveAll();
			AssertEquals("Precondition", ZGuid.Empty, packLine1.JL_JC);
			AssertEquals("Precondition", ZGuid.Empty, packLine2.JL_JC);
			AssertEquals("Precondition", ZGuid.Empty, packLine3.JL_JC);

			AssertEquals(shipment1, container.GetRelatedShipmentsForPenaltyDefaulting(ContainerPenaltyProcessType.Delivery).FirstOrDefault());

			packLine2.SetContainer(consol, container);
			packLine2.JL_ContainerPackingOrder = 1;

			AssertEquals(shipment2, container.GetRelatedShipmentsForPenaltyDefaulting(ContainerPenaltyProcessType.Delivery).FirstOrDefault());

			packLine1.SetContainer(consol, container);
			packLine1.JL_ContainerPackingOrder = 2;

			packLine3.SetContainer(consol, container);
			packLine3.JL_ContainerPackingOrder = 3;

			AssertEquals(shipment2, container.GetRelatedShipmentsForPenaltyDefaulting(ContainerPenaltyProcessType.Delivery).FirstOrDefault());

			var shipmentPks = new HashSet<ZGuid>();
			var shipments = container.GetRelatedShipmentsForPenaltyDefaulting(ContainerPenaltyProcessType.Delivery);
			foreach (var shipment in shipments)
			{
				AssertEquals("Each shipment should only appear once", false, shipmentPks.Contains(shipment.PK));
				shipmentPks.Add(shipment.PK);
			}
		}
	}
}
