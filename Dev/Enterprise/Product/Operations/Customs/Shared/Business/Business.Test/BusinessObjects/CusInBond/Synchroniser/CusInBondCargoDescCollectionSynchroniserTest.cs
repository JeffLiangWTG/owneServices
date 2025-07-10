using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class CusInBondCargoDescCollectionSynchroniserTest : SynchroniserTestCase
	{
		public virtual void TestSynchronisation()
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

			var bill = (CusInBondBill)header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var billContainer = (CusInBondContainer)moveDetail.Containers.AddNew();
			billContainer.BC_ContainerNum = "CONT1";
			AssertEquals(0, billContainer.Commodities.Count);

			var synchroniser = new CusInBondCargoDescCollectionSynchroniser(shipment, billContainer);
			synchroniser.Synchronise();
			var list = billContainer.Commodities.ToArray();
			AssertEquals(2, list.Length);
			var commodity1 = list.First(x => x.BY_FormattedHarmonisedTariff == "2002.02.20");
			AssertNotNull(commodity1);
			var commodity2 = list.First(x => x.BY_FormattedHarmonisedTariff == "3003.03.30");
			AssertNotNull(commodity2);

			var packLine6 = shipment.OuterPackLines.AddNew();
			packLine6.JL_HarmonisedCode = "60.605.30";
			packLine6.SetContainer(consol, container1);
			packLine6.SetContainer(consol2, null);

			list = billContainer.Commodities.ToArray();
			AssertEquals(3, list.Length);
			AssertCollectionContains(commodity1, list);
			AssertCollectionContains(commodity2, list);
			var commodity3 = list.First(x => x.BY_FormattedHarmonisedTariff == "6060.53.0");
			AssertNotNull(commodity3);
		}

		public void TestSynchronisation_DuplicatedCommodities()
		{
			var shipment = consol.Shipments.AddNew();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = "1001";
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "1001";

			var bill = (CusInBondBill)header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var billContainer = (CusInBondContainer)moveDetail.Containers.AddNew();
			billContainer.BC_ContainerNum = CusInBondContainer.NonContainerizedNumber;
			AssertEquals(0, billContainer.Commodities.Count);

			var synchroniser = new CusInBondCargoDescCollectionSynchroniser(shipment, billContainer);
			synchroniser.Synchronise();
			AssertEquals(2, billContainer.Commodities.Count);
			var commodity1 = billContainer.Commodities[0];
			var commodity2 = billContainer.Commodities[1];
			AssertEquals("100100", commodity1.BY_HarmonisedTariff);
			AssertEquals("100100", commodity2.BY_HarmonisedTariff);

			packLine2.JL_HarmonisedCode = "2002";
			AssertEquals(2, billContainer.Commodities.Count);
			AssertEquals("100100", commodity1.BY_HarmonisedTariff);
			AssertEquals("200200", commodity2.BY_HarmonisedTariff);

			packLine2.JL_HarmonisedCode = "1001";
			AssertEquals(2, billContainer.Commodities.Count);
			AssertEquals("100100", commodity1.BY_HarmonisedTariff);
			AssertEquals("100100", commodity2.BY_HarmonisedTariff);

			synchroniser.SetEnabled(false, false);
			packLine1.Delete();

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_HarmonisedCode = "1001";

			synchroniser.SetEnabled(true, false);
			AssertEquals(2, billContainer.Commodities.Count);
			AssertEquals("100100", commodity1.BY_HarmonisedTariff);
			AssertEquals("100100", commodity2.BY_HarmonisedTariff);

			synchroniser.Synchronise(true);
			AssertEquals("Is deleted as packLine1 is deleted", true, commodity1.IsDeleted);
			var list = billContainer.Commodities.ToArray();
			AssertEquals(2, list.Length);
			var commodity3 = list[1];
			if (commodity3 == commodity2)
			{
				commodity3 = list[0];
			}
			else
			{
				AssertEquals(commodity2, list[0]);
			}
			AssertEquals("100100", commodity2.BY_HarmonisedTariff);
			AssertEquals("100100", commodity3.BY_HarmonisedTariff);
			AssertEquals("1001.00", commodity2.BY_FormattedHarmonisedTariff);
			AssertEquals("1001.00", commodity3.BY_FormattedHarmonisedTariff);
		}

		protected override void SetUp()
		{
			base.SetUp();

			consol = CreateFCLConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			header = (CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.AMS;
		}
		ForwardingConsol consol;
		CusInBondHeader header;
	}
}
