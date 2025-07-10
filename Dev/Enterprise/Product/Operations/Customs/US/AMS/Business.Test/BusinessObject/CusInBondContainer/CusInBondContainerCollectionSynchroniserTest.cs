using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondContainerCollectionSynchroniserTest : Customs.Business.Testing.CusInBondContainerCollectionSynchroniserTest
	{
		public void TestSynchronisationViaDataRefreshBus()
		{
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "ABCDHB1";
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, container);
			packLine.JL_HarmonisedCode = "1010.10.10";
			var header = (CusInBondHeader)base.header;
			var bill = header.Bills.AddNew("ABCD", "HB1");
			var synchroniser = new CusInBondContainerCollectionSynchroniser(shipment, bill, consol, bill.ShouldSynchronise, bill.MovementDetail.Containers);
			synchroniser.Synchronise(true);
#pragma warning disable
			((IBusinessObjectState)consol).UpdatedByDataRefreshIncludingChildren += new EventHandler((x, y) => synchroniser.Synchronise());
#pragma warning restore
			var moveDetail = bill.MovementDetail;
			AssertEquals(1, moveDetail.Containers.Count);
			var billContainer = moveDetail.Containers[0];
			AssertEquals("CONT1", billContainer.BC_ContainerNum);
			AssertEquals(1, billContainer.Commodities.Count);
			var commodity = billContainer.Commodities[0];
			AssertEquals("10101010", commodity.BY_HarmonisedTariff);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var containerInOtherFactory = newFactory.Load<ForwardingContainer>(container.PK);
			AssertEquals("CONT1", containerInOtherFactory.JC_ContainerNum);
			containerInOtherFactory.JC_ContainerNum = "CONT2";
			newFactory.Save();
			AssertEquals("CONT2", container.JC_ContainerNum);
			AssertEquals(1, moveDetail.Containers.Count);
			AssertEquals(false, billContainer.IsDeleted);
			AssertEquals(billContainer, moveDetail.Containers["CONT2"]);
		}
	}
}
