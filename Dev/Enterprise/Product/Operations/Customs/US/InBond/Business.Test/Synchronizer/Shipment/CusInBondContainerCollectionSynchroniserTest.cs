using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondContainerCollectionSynchroniserTest : Customs.Business.Testing.SynchroniserTestCase
	{
		public void TestSynchronisation()
		{
			var refContainer1 = Factory.New<RefContainer>();
			refContainer1.RC_Code = "XD12";
			var refContainer2 = Factory.New<RefContainer>();
			refContainer2.RC_Code = "KD53";
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";
			container1.JC_RC = refContainer1.PK;
			var shipment = consol.Shipments.AddNew();
			var consol2 = shipment.Consols.AddNew();
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2";
			container2.JC_RC = refContainer2.PK;
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = "10.01 01.10 A";
			packLine1.SetContainer(consol, container1);
			packLine1.SetContainer(consol2, container2);
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "20.02 02.20 B";
			packLine2.SetContainer(consol, container1);
			packLine2.SetContainer(consol2, null);
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			AssertEquals(0, moveDetail.Containers.Count);
			var synchroniser = new CusInBondContainerCollectionShipmentSynchroniser(shipment, moveDetail, consol, bill.ShouldSynchronise, moveDetail.Containers);
			synchroniser.Synchronise();
			AssertEquals(1, moveDetail.Containers.Count);
			var billContainer1 = moveDetail.Containers["CONT1"];
			AssertNotNull(billContainer1);
			AssertEquals(refContainer1.PK, billContainer1.BC_RC);
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

		protected override void SetUp()
		{
			base.SetUp();
			consol = CreateFCLConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
		}
		ForwardingConsol consol;
		CusInBondHeader header;
	}
}
