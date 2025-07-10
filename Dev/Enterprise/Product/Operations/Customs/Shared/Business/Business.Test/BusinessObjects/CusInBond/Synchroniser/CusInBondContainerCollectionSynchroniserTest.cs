using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class CusInBondContainerCollectionSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchronisation()
		{
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";
			var shipment = consol.Shipments.AddNew();
			var consol2 = shipment.Consols.AddNew();
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2";

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = "10.01 01.10 A";
			packLine1.SetContainer(consol, container1);
			packLine1.SetContainer(consol2, container2);

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "20.02 02.20 B";
			packLine2.SetContainer(consol, container1);
			packLine2.SetContainer(consol2, null);

			var bill = (CusInBondBill)header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			AssertEquals(0, moveDetail.Containers.Count);

			var synchroniser = new CusInBondContainerCollectionSynchroniser(shipment, bill, consol, bill.ShouldSynchronise, bill.MovementDetail.Containers);
			synchroniser.Synchronise();
			AssertEquals(1, moveDetail.Containers.Count);
			var billContainer1 = moveDetail.Containers["CONT1"];
			AssertNotNull(billContainer1);

			Factory.Save();
			// changes to packing details should be done on a different factory as packing details is not exposed on the Consol Screen.

			var newFactory = new BusinessObjectFactory();
			var shipmentInOtherFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			var consolInOtherFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			var consol2InOtherFactory = newFactory.Load<ForwardingConsol>(consol2.PK);
			var packLine2InOtherFactory = newFactory.Load<PackLine>(packLine2.PK);
			packLine2InOtherFactory.SetContainer(consolInOtherFactory, null);

			newFactory.Save();
			AssertEquals(2, moveDetail.Containers.Count);
			AssertEquals(billContainer1, moveDetail.Containers["CONT1"]);

			var billContainer2 = moveDetail.Containers[CusInBondContainer.NonContainerizedNumber];
			AssertNotNull(billContainer2);

			var packLine3InOtherFactory = shipmentInOtherFactory.OuterPackLines.AddNew();
			packLine3InOtherFactory.JL_HarmonisedCode = "60.605.30";
			packLine3InOtherFactory.SetContainer(consolInOtherFactory, null);
			packLine3InOtherFactory.SetContainer(consol2InOtherFactory, null);

			newFactory.Save();
			AssertEquals(3, shipment.OuterPackLines.Count);
			var packLine3 = (PackLine)shipment.OuterPackLines.FindByPK(packLine3InOtherFactory.PK);
			packLine3.SetContainer(consol, null);
			packLine3.SetContainer(consol2, null);
			Factory.Save();

			AssertEquals(ZGuid.Empty, packLine3.JL_JC);
			AssertEquals(2, moveDetail.Containers.Count);
			AssertEquals(billContainer1, moveDetail.Containers["CONT1"]);
			AssertEquals(billContainer2, moveDetail.Containers[CusInBondContainer.NonContainerizedNumber]);

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "CONT3";
			Factory.Save();

			AssertEquals(2, moveDetail.Containers.Count);
			AssertEquals(billContainer1, moveDetail.Containers["CONT1"]);
			AssertEquals(billContainer2, moveDetail.Containers[CusInBondContainer.NonContainerizedNumber]);

			var container3InOtherFactory = newFactory.Load<ForwardingContainer>(container3.PK);
			packLine3InOtherFactory.SetContainer(consolInOtherFactory, container3InOtherFactory);
			newFactory.Save();

			AssertEquals(2, consol.Containers.Count);
			AssertEquals(3, moveDetail.Containers.Count);
			AssertEquals(billContainer1, moveDetail.Containers["CONT1"]);
			AssertEquals(billContainer2, moveDetail.Containers[CusInBondContainer.NonContainerizedNumber]);
			var billContainer3 = moveDetail.Containers["CONT3"];
			AssertNotNull(billContainer3);

			container1.Delete();
			AssertEquals(2, moveDetail.Containers.Count);
			AssertEquals(true, billContainer1.IsDeleted);
			AssertEquals(billContainer2, moveDetail.Containers[CusInBondContainer.NonContainerizedNumber]);
			AssertEquals(billContainer3, moveDetail.Containers["CONT3"]);
		}

		public void TestSynchronisation_BreakBulkContainer()
		{
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			var shipment = consol.Shipments.AddNew();

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = "1001";
			packLine1.SetContainer(consol, container1);

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "1002";
			packLine2.SetContainer(consol, null);

			var bill = (CusInBondBill)header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			AssertEquals(0, moveDetail.Containers.Count);

			var synchroniser = new CusInBondContainerCollectionSynchroniser(shipment, bill, consol, bill.ShouldSynchronise, bill.MovementDetail.Containers);
			synchroniser.Synchronise();
			AssertEquals(1, moveDetail.Containers.Count);
			var billContainer1 = (CusInBondContainer)moveDetail.Containers[CusInBondContainer.NonContainerizedNumber];
			AssertNotNull(billContainer1);
			var list = billContainer1.Commodities.ToArray();
			AssertEquals(2, list.Length);
			AssertNotNull(list.First(x => x.BY_FormattedHarmonisedTariff == "1001.00"));
			AssertNotNull(list.First(x => x.BY_FormattedHarmonisedTariff == "1002.00"));

			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(2, moveDetail.Containers.Count);
			AssertEquals(false, billContainer1.IsDeleted);
			AssertEquals(billContainer1, moveDetail.Containers[CusInBondContainer.NonContainerizedNumber]);
			list = billContainer1.Commodities.ToArray();
			AssertEquals(1, list.Length);
			AssertNotNull(list.First(x => x.BY_FormattedHarmonisedTariff == "1002.00"));
			var billContainer2 = (CusInBondContainer)moveDetail.Containers["CONT1"];
			AssertNotNull(billContainer2);
			list = billContainer2.Commodities.ToArray();
			AssertEquals(1, list.Length);
			AssertNotNull(list.First(x => x.BY_FormattedHarmonisedTariff == "1001.00"));

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2";
			container2.JC_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals(2, moveDetail.Containers.Count);
			AssertEquals(false, billContainer1.IsDeleted);
			AssertEquals(billContainer1, (CusInBondContainer)moveDetail.Containers[CusInBondContainer.NonContainerizedNumber]);
			list = billContainer1.Commodities.ToArray();
			AssertEquals(1, list.Length);
			AssertNotNull(list.First(x => x.BY_FormattedHarmonisedTariff == "1002.00"));
			AssertEquals(false, billContainer2.IsDeleted);
			AssertEquals(billContainer2, moveDetail.Containers["CONT1"]);
			AssertNotNull(billContainer2);
			list = billContainer2.Commodities.ToArray();
			AssertEquals(1, list.Length);
			AssertNotNull(list.First(x => x.BY_FormattedHarmonisedTariff == "1001.00"));

			Factory.Save();
			// changes to packing details should be done on a different factory as packing details is not exposed on the Consol Screen.

			var newFactory = new BusinessObjectFactory();
			var shipmentInOtherFactory = newFactory.Load<ForwardingShipment>(shipment.PK);
			var consolInOtherFactory = newFactory.Load<ForwardingConsol>(consol.PK);
			var packLine2InOtherFactory = newFactory.Load<PackLine>(packLine2.PK);
			var container2InOtherFactory = newFactory.Load<CommonContainer>(container2.PK);
			packLine2InOtherFactory.SetContainer(consolInOtherFactory, container2InOtherFactory);

			newFactory.Save();
			AssertEquals(2, moveDetail.Containers.Count);
			AssertEquals(false, billContainer1.IsDeleted);
			AssertEquals(billContainer1, moveDetail.Containers[CusInBondContainer.NonContainerizedNumber]);
			list = billContainer1.Commodities.ToArray();
			AssertEquals(1, list.Length);
			AssertNotNull(list.First(x => x.BY_FormattedHarmonisedTariff == "1002.00"));
			AssertEquals(false, billContainer2.IsDeleted);
			AssertEquals(billContainer2, moveDetail.Containers["CONT1"]);
			AssertNotNull(billContainer2);
			list = billContainer2.Commodities.ToArray();
			AssertEquals(1, list.Length);
			AssertNotNull(list.First(x => x.BY_FormattedHarmonisedTariff == "1001.00"));

			var packLine1InOtherFactory = newFactory.Load<PackLine>(packLine1.PK);
			packLine1.SetContainer(consolInOtherFactory, null);
			Factory.Save();
			AssertEquals(1, moveDetail.Containers.Count);
			AssertEquals(false, billContainer1.IsDeleted);
			AssertEquals(billContainer1, moveDetail.Containers[CusInBondContainer.NonContainerizedNumber]);
			list = billContainer1.Commodities.ToArray();
			AssertEquals(2, list.Length);
			AssertNotNull(list.First(x => x.BY_FormattedHarmonisedTariff == "1001.00"));
			AssertNotNull(list.First(x => x.BY_FormattedHarmonisedTariff == "1002.00"));
			AssertEquals(true, billContainer2.IsDeleted);
		}

		public void TestSynchronisation_DuplicatedContainers()
		{
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";
			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT1";
			var shipment = consol.Shipments.AddNew();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = "1001";
			packLine1.SetContainer(consol, container1);
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "1002";
			packLine2.SetContainer(consol, container2);

			var bill = (CusInBondBill)header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			AssertEquals(0, moveDetail.Containers.Count);

			var synchroniser = new CusInBondContainerCollectionSynchroniser(shipment, bill, consol, bill.ShouldSynchronise, bill.MovementDetail.Containers);
			synchroniser.Synchronise();
			AssertEquals(2, moveDetail.Containers.Count);
			var billContainer1 = (CusInBondContainer)moveDetail.Containers[0];
			var billContainer2 = (CusInBondContainer)moveDetail.Containers[1];
			AssertEquals("CONT1", billContainer1.BC_ContainerNum);
			AssertNotNull(billContainer1.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1001.00"));
			AssertEquals("CONT1", billContainer2.BC_ContainerNum);
			AssertNotNull(billContainer2.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1002.00"));

			container2.JC_ContainerNum = "CONT2";
			AssertEquals(2, moveDetail.Containers.Count);
			AssertEquals("CONT1", billContainer1.BC_ContainerNum);
			AssertNotNull(billContainer1.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1001.00"));
			AssertEquals("CONT2", billContainer2.BC_ContainerNum);
			AssertNotNull(billContainer2.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1002.00"));

			container2.JC_ContainerNum = "CONT1";
			AssertEquals(2, moveDetail.Containers.Count);
			AssertEquals("CONT1", billContainer1.BC_ContainerNum);
			AssertNotNull(billContainer1.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1001.00"));
			AssertEquals("CONT1", billContainer2.BC_ContainerNum);
			AssertNotNull(billContainer2.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1002.00"));

			synchroniser.SetEnabled(false, false);
			container1.Delete();

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = "CONT1";
			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_HarmonisedCode = "1003";
			packLine3.SetContainer(consol, container3);

			synchroniser.SetEnabled(true, false);
			AssertEquals(2, moveDetail.Containers.Count);
			AssertEquals("CONT1", billContainer1.BC_ContainerNum);
			AssertNotNull(billContainer1.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1001.00"));
			AssertEquals("CONT1", billContainer2.BC_ContainerNum);
			AssertNotNull(billContainer2.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1002.00"));

			synchroniser.Synchronise(true);
			AssertEquals("Is deleted as container1 is deleted", true, billContainer1.IsDeleted);
			var list = moveDetail.Containers.ToArray<CusInBondContainer>();
			AssertEquals(3, list.Length);
			var billContainer3 = (CusInBondContainer)moveDetail.Containers[CusInBondContainer.NonContainerizedNumber];
			AssertNotNull(billContainer3);
			AssertEquals(billContainer2, list.First(x => x.PK == billContainer2.PK));
			var billContainer4 = list.First(x => x.PK != billContainer3.PK && x.PK != billContainer2.PK);
			AssertEquals("CONT1", billContainer2.BC_ContainerNum);
			AssertNotNull(billContainer2.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1002.00"));
			AssertEquals(CusInBondContainer.NonContainerizedNumber, billContainer3.BC_ContainerNum);
			AssertNotNull(billContainer3.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1001.00"));
			AssertEquals("CONT1", billContainer4.BC_ContainerNum);
			AssertNotNull(billContainer4.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1003.00"));

			synchroniser.SetEnabled(false, false);
			packLine1.Delete();

			synchroniser.SetEnabled(true, false);
			AssertEquals(3, moveDetail.Containers.Count);
			AssertEquals("CONT1", billContainer2.BC_ContainerNum);
			AssertNotNull(billContainer2.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1002.00"));
			AssertEquals(CusInBondContainer.NonContainerizedNumber, billContainer3.BC_ContainerNum);
			AssertNotNull(billContainer3.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1001.00"));
			AssertEquals("CONT1", billContainer4.BC_ContainerNum);
			AssertNotNull(billContainer4.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1003.00"));

			synchroniser.Synchronise(true);
			AssertEquals("Is deleted as packLine1 is deleted", true, billContainer3.IsDeleted);
			list = moveDetail.Containers.ToArray<CusInBondContainer>();
			AssertEquals(2, list.Length);
			AssertEquals(billContainer2, list.First(x => x.PK == billContainer2.PK));
			AssertEquals(billContainer4, list.First(x => x.PK == billContainer4.PK));
			AssertEquals("CONT1", billContainer2.BC_ContainerNum);
			AssertNotNull(billContainer2.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1002.00"));
			AssertEquals("CONT1", billContainer4.BC_ContainerNum);
			AssertNotNull(billContainer4.Commodities.ToArray().First(x => x.BY_FormattedHarmonisedTariff == "1003.00"));
		}

		public void TestSynchronisation_NullConsolInput()
		{
			var shipment = consol.Shipments.AddNew();

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONTAINER";

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_HarmonisedCode = "1001";
			packline.SetContainer(consol, container);

			var bill = (CusInBondBill)header.Bills.AddNew();

			Factory.Save();

			var synchroniser = new CusInBondContainerCollectionSynchroniser(shipment, bill, null, bill.ShouldSynchronise, bill.MovementDetail.Containers);
			AssertNoExceptionThrown(() => synchroniser.Synchronise());
		}

		protected override void SetUp()
		{
			base.SetUp();

			consol = CreateFCLConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			header = (CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
		}
		protected ForwardingConsol consol;
		protected CusInBondHeader header;
	}
}
