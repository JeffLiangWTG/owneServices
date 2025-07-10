using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public class CusInBondContainerSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchroniseBC_ContainerNum()
		{
			container.JC_ContainerNum = "CONT1";
			AssertEquals("CONT1", billContainer.BC_ContainerNum);

			container.JC_ContainerNum = ZString.Empty;
			AssertEquals(ZString.Empty, billContainer.BC_ContainerNum);

			container.JC_ContainerNum = "CONT2";
			AssertEquals("CONT2", billContainer.BC_ContainerNum);
		}

		public void TestSynchroniseBC_Seal1()
		{
			container.JC_SealNum = "CONT1";
			AssertEquals("CONT1", billContainer.BC_Seal1);

			container.JC_SealNum = ZString.Empty;
			AssertEquals(ZString.Empty, billContainer.BC_Seal1);

			container.JC_SealNum = "CONT2";
			AssertEquals("CONT2", billContainer.BC_Seal1);
		}

		public void TestSynchroniseBC_Seal2()
		{
			container.JC_AdditionalSealNum = "CONT1";
			AssertEquals("CONT1", billContainer.BC_Seal2);

			container.JC_AdditionalSealNum = ZString.Empty;
			AssertEquals(ZString.Empty, billContainer.BC_Seal2);

			container.JC_AdditionalSealNum = "CONT2";
			AssertEquals("CONT2", billContainer.BC_Seal2);
		}

		public void TestSynchroniseCommodities()
		{
			container.JC_ContainerNum = "CONT1";
			AssertEquals(0, billContainer.Commodities.Count);

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container);
			packLine1.JL_HarmonisedCode = "10.10";

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, container);
			packLine2.JL_HarmonisedCode = "20.20";

			var list = billContainer.Commodities.ToArray();
			AssertEquals(2, list.Length);
			var commodity1 = list.First(x => x.BY_FormattedHarmonisedTariff == "1010.00");
			AssertNotNull(commodity1);

			var commodity2 = list.First(x => x.BY_FormattedHarmonisedTariff == "2020.00");
			AssertNotNull(commodity2);

			var container2 = consol.Containers.AddNew();
			list = billContainer.Commodities.ToArray();
			AssertEquals(2, list.Length);
			AssertEquals(commodity1, list.First(x => x.BY_FormattedHarmonisedTariff == "1010.00"));
			AssertEquals(commodity2, list.First(x => x.BY_FormattedHarmonisedTariff == "2020.00"));

			packLine1.SetContainer(consol, container2);
			list = billContainer.Commodities.ToArray();
			AssertEquals(1, list.Length);
			AssertEquals(true, commodity1.IsDeleted);
			AssertEquals(commodity2, list.First(x => x.BY_FormattedHarmonisedTariff == "2020.00"));

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_HarmonisedCode = "30.30";
			packLine3.SetContainer(consol, container);
			list = billContainer.Commodities.ToArray();
			AssertEquals(2, list.Length);
			var commodity3 = list.First(x => x.BY_FormattedHarmonisedTariff == "3030.00");
			AssertNotNull(commodity3);
			AssertEquals(commodity2, list.First(x => x.BY_FormattedHarmonisedTariff == "2020.00"));

			packLine2.SetContainer(consol, null);
			list = billContainer.Commodities.ToArray();
			AssertEquals(1, list.Length);
			AssertEquals(true, commodity2.IsDeleted);
			AssertEquals(commodity3, list.First(x => x.BY_FormattedHarmonisedTariff == "3030.00"));
		}

		public void TestSynchroniseUNDGs()
		{
			AssertEquals(0, billContainer.UNDGs.Count);
			var packLine = shipment.OuterPackLines.AddNew();
			var undg = packLine.UNDGs.FirstItemForBinding[0];
			AssertEquals(0, billContainer.UNDGs.Count);

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "2010";
			subs.DG_Variant = "b";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_Code = "1010a";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var subs3 = Factory.New<UNDGSubstance>();
			subs3.DG_Code = "1010b";
			subs3.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			undg.LinkDefault(subs2);
			undg.DI_DG = subs2.PK;
			AssertEquals(1, billContainer.UNDGs.Count);
			var billContainerUNDG = billContainer.UNDGs[0];
			AssertEquals("1010a", billContainerUNDG.UNDGSubstance.DG_Code);

			var undg1 = packLine.UNDGs.AddNew();
			undg1.DI_DG = subs.PK;
			undg1.LinkDefault(subs);
			AssertEquals(2, billContainer.UNDGs.Count);
			AssertEquals(billContainerUNDG, billContainer.UNDGs.TryGetOrCreate("1010a", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO));
			var billContainerUNDG2 = billContainer.UNDGs[0] == billContainerUNDG ? billContainer.UNDGs[1] : billContainer.UNDGs[0];
			AssertEquals(billContainerUNDG2, billContainer.UNDGs.TryGetOrCreate("2010b", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO));

			undg1.DI_DG = subs2.PK;
			undg1.LinkDefault(subs2);
			AssertEquals(1, billContainer.UNDGs.Count);
			AssertEquals(billContainerUNDG, billContainer.UNDGs.TryGetOrCreate("1010a", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO));
			AssertEquals(true, billContainerUNDG2.IsDeleted);

			undg1.DI_DG = subs3.PK;
			undg1.LinkDefault(subs3);
			AssertEquals(2, billContainer.UNDGs.Count);
			AssertEquals(billContainerUNDG, billContainer.UNDGs.TryGetOrCreate("1010a", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO));
			billContainerUNDG2 = billContainer.UNDGs[0] == billContainerUNDG ? billContainer.UNDGs[1] : billContainer.UNDGs[0];
			AssertEquals(billContainerUNDG2, billContainer.UNDGs.TryGetOrCreate("1010b", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO));
		}

		protected override void SetUp()
		{
			base.SetUp();

			consol = CreateFCLConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			shipment = consol.Shipments.AddNew();
			container = consol.Containers.AddNew();

			var header = (CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;

			bill = (CusInBondBill)header.Bills.AddNew();
			moveDetail = bill.MovementDetail;
			billContainer = (CusInBondContainer)moveDetail.Containers.AddNew();

			synchroniser = new CusInBondContainerSynchroniser(billContainer, container, shipment);
			synchroniser.Synchronise(true);
		}
		ForwardingConsol consol;
		protected ForwardingShipment shipment;
		protected ForwardingContainer container;

		CusInBondBill bill;
		CusInBondMoveDetail moveDetail;
		protected CusInBondContainer billContainer;

		protected CusInBondContainerSynchroniser synchroniser;
	}
}
