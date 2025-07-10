using System.Linq;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondContainerShipmentSynchroniserTest : Customs.Business.Testing.SynchroniserTestCase
	{
		public void TestSynchroniseBC_ContainerNum()
		{
			var refContainer1 = Factory.New<RefContainer>();
			refContainer1.RC_Code = "XD12";
			var refContainer2 = Factory.New<RefContainer>();
			refContainer2.RC_Code = "KD53";
			container.JC_ContainerNum = "CONT1";
			container.JC_SealNum = "SN1";
			container.JC_AdditionalSealNum = "ASN1";
			container.JC_RC = refContainer1.PK;
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container);
			packLine1.JL_HarmonisedCode = "10.10";
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, container);
			packLine2.JL_HarmonisedCode = "20.20";
			var undg = packLine1.UNDGs.FirstItemForBinding[0];
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "1010A";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg.DI_DG = subs.PK;
			undg.LinkDefault(subs);
			AssertEquals("CONT1", billContainer.BC_ContainerNum);
			AssertEquals("SN1", billContainer.BC_Seal1);
			AssertEquals("ASN1", billContainer.BC_Seal2);
			AssertEquals(refContainer1.PK, billContainer.BC_RC);
			var commodities = billContainer.Commodities.ToArray();
			AssertEquals(2, commodities.Length);
			var commodity1 = commodities.First(x => x.BY_FormattedHarmonisedTariff == "1010");
			AssertNotNull(commodity1);
			var commodity2 = commodities.First(x => x.BY_FormattedHarmonisedTariff == "2020");
			AssertNotNull(commodity2);
			AssertEquals(1, billContainer.UNDGs.Count);
			var billContainerUNDG = billContainer.UNDGs[0];
			AssertEquals("1010A", billContainerUNDG.SubstanceCode);
			container.JC_ContainerNum = "CONT2";
			container.JC_SealNum = "SN2";
			container.JC_AdditionalSealNum = "ASN2";
			container.JC_RC = refContainer2.PK;
			AssertEquals("CONT2", billContainer.BC_ContainerNum);
			AssertEquals("SN2", billContainer.BC_Seal1);
			AssertEquals("ASN2", billContainer.BC_Seal2);
			AssertEquals(refContainer2.PK, billContainer.BC_RC);
			var container2 = consol.Containers.AddNew();
			packLine1.SetContainer(consol, container2);
			commodities = billContainer.Commodities.ToArray();
			AssertEquals(1, commodities.Length);
			AssertEquals(true, commodity1.IsDeleted);
			AssertEquals(commodity2, commodities.First(x => x.BY_FormattedHarmonisedTariff == "2020"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = CreateFCLConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			container = consol.Containers.AddNew();
			header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			bill = header.Bills.AddNew();
			moveDetail = bill.MovementDetail;
			billContainer = moveDetail.Containers.AddNew();
			synchroniser = new CusInBondContainerShipmentSynchroniser(billContainer, container, shipment);
			synchroniser.Synchronise(true);
		}
		ForwardingConsol consol;
		CusInBondHeader header;
		ForwardingShipment shipment;
		ForwardingContainer container;
		CusInBondBill bill;
		CusInBondMoveDetail moveDetail;
		CusInBondContainer billContainer;
		CusInBondContainerShipmentSynchroniser synchroniser;
	}
}
