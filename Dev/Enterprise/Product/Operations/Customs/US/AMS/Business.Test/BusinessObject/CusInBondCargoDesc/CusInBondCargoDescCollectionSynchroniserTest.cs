using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondCargoDescCollectionSynchroniserTest : Customs.Business.Testing.CusInBondCargoDescCollectionSynchroniserTest
	{
		public override void TestSynchronisation()
		{
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";
			var shipment = consol.Shipments.AddNew();
			var consol2 = shipment.Consols.AddNew();
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = "10.01 01.10 A";
			packLine1.SetContainer(consol, null);
			packLine1.SetContainer(consol2, container2);

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "20.02 02.20 B";
			packLine2.SetContainer(consol, container1);
			packLine2.SetContainer(consol2, null);

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_HarmonisedCode = "30.03 03.30 C";
			packLine3.SetContainer(consol, container1);
			packLine3.SetContainer(consol2, null);

			var packLine4 = shipment.OuterPackLines.AddNew();
			packLine4.JL_HarmonisedCode = "40.02.234B";
			packLine4.SetContainer(consol, null);
			packLine4.SetContainer(consol2, null);

			var packLine5 = shipment.OuterPackLines.AddNew();
			packLine5.JL_HarmonisedCode = "50.105.30";
			packLine5.SetContainer(consol, null);
			packLine5.SetContainer(consol2, null);

			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var billContainer = moveDetail.Containers.AddNew();
			billContainer.BC_ContainerNum = "CONT1";
			AssertEquals(0, billContainer.Commodities.Count);

			var synchroniser = new Customs.Business.CusInBondCargoDescCollectionSynchroniser(shipment, billContainer);
			synchroniser.Synchronise();
			AssertEquals(2, billContainer.Commodities.Count);
			var commodity1 = billContainer.Commodities["20.020220"];
			AssertNotNull(commodity1);
			var commodity2 = billContainer.Commodities["30.03 0330"];
			AssertNotNull(commodity2);

			var packLine6 = shipment.OuterPackLines.AddNew();
			packLine6.JL_HarmonisedCode = "60.605.30";
			packLine6.SetContainer(consol, container1);
			packLine6.SetContainer(consol2, null);

			AssertEquals(3, billContainer.Commodities.Count);
			AssertCollectionContains(commodity1, billContainer.Commodities);
			AssertCollectionContains(commodity2, billContainer.Commodities);
			var commodity3 = billContainer.Commodities["6060530"];
			AssertNotNull(commodity3);
		}

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

			var bill = header.Bills.AddNew("ABCD", "HB1");
			var moveDetail = bill.MovementDetail;
			var billContainer = moveDetail.Containers.AddNew();
			billContainer.BC_ContainerNum = "CONT1";
			var synchroniser = new Customs.Business.CusInBondCargoDescCollectionSynchroniser(shipment, billContainer);
			synchroniser.Synchronise(true);
#pragma warning disable
			((IBusinessObjectState)consol).UpdatedByDataRefreshIncludingChildren += new EventHandler((x, y) => synchroniser.Synchronise());
#pragma warning restore
			AssertEquals(1, billContainer.Commodities.Count);
			var commodity = billContainer.Commodities[0];
			AssertEquals("10101010", commodity.BY_HarmonisedTariff);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var packLineInOtherFactory = newFactory.Load<ForwardingPackLine>(packLine.PK);
			AssertEquals("1010.10.10", packLineInOtherFactory.JL_HarmonisedCode);
			packLineInOtherFactory.JL_HarmonisedCode = "2020.20.10";

			newFactory.Save();
			AssertEquals("2020.20.10", packLine.JL_HarmonisedCode);
			AssertEquals(1, billContainer.Commodities.Count);
			AssertEquals(false, commodity.IsDeleted);
			AssertEquals(commodity, billContainer.Commodities["20202010"]);
		}

		public void TestSynchronisation_InnerPackLineLinkedAndOuterPackLineStandAlone_SyncFromBothPackLines()
		{
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1";
			var shipment = consol.Shipments.AddNew();

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container);
			packLine1.JL_PackageCount = 1;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, container);
			packLine2.JL_PackageCount = 2;
			Factory.Save();

			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var billContainer = moveDetail.Containers.AddNew();
			billContainer.BC_ContainerNum = "CONT1";
			AssertEquals("No commidties, not sync yet", 0, billContainer.Commodities.Count);

			var synchroniser = new CusInBondCargoDescCollectionSynchroniser(shipment, billContainer);
			synchroniser.Synchronise();
			AssertEquals("Sync commodities from outer packs", 2, billContainer.Commodities.Count);
			AssertNotNull("1 is from the outer", billContainer.Commodities.First(commodity => commodity.BY_PieceCount == 1));
			AssertNotNull("2 is from the outer", billContainer.Commodities.First(commodity => commodity.BY_PieceCount == 2));

			var packLine3 = shipment.InnerPackLines.AddNew();
			packLine3.JL_PackageCount = 3;
			packLine3.JL_JL_OuterPackLine = packLine1.PK;
			synchroniser.Synchronise();
			AssertEquals("Sync commodities from inner packs and outer packs", 2, billContainer.Commodities.Count);
			AssertNotNull("3 is from the inner", billContainer.Commodities.First(commodity => commodity.BY_PieceCount == 3));
			AssertNotNull("2 is from the outer", billContainer.Commodities.First(commodity => commodity.BY_PieceCount == 2));

			var packLine4 = shipment.InnerPackLines.AddNew();
			packLine4.JL_PackageCount = 4;
			packLine4.JL_JL_OuterPackLine = packLine1.PK;
			synchroniser.Synchronise();
			AssertEquals("Sync commodities from inner packs and outer packs", 3, billContainer.Commodities.Count);
			AssertNotNull("3 is from the inner", billContainer.Commodities.First(commodity => commodity.BY_PieceCount == 3));
			AssertNotNull("4 is from the inner", billContainer.Commodities.First(commodity => commodity.BY_PieceCount == 4));
			AssertNotNull("2 is from the outer", billContainer.Commodities.First(commodity => commodity.BY_PieceCount == 2));

			var packLine5 = shipment.InnerPackLines.AddNew();
			packLine5.JL_PackageCount = 5;
			packLine5.JL_JL_OuterPackLine = packLine2.PK;
			synchroniser.Synchronise();
			AssertEquals("Sync commodities from inner packs", 3, billContainer.Commodities.Count);
			AssertNotNull("3 is from the inner", billContainer.Commodities.First(commodity => commodity.BY_PieceCount == 3));
			AssertNotNull("4 is from the inner", billContainer.Commodities.First(commodity => commodity.BY_PieceCount == 4));
			AssertNotNull("5 is from the inner", billContainer.Commodities.First(commodity => commodity.BY_PieceCount == 5));
		}

		protected override void SetUp()
		{
			base.SetUp();

			consol = CreateFCLConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.Synchroniser.SetEnabled(false, false);
			header.BH_ImportTransportMode = TransportTypeList.Codes.VesselNonContainer;
		}
		protected ForwardingConsol consol;
		protected CusInBondHeader header;
	}
}
