using System.Linq;

namespace Enterprise.Customs.Business.Testing
{
	sealed class NonContainerizedNumberSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchronise()
		{
			var consol = CreateFCLConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			var header = (CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1";
			var shipment = consol.Shipments.AddNew();
			var bill = (CusInBondBill)header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var billContainer = (CusInBondContainer)moveDetail.Containers.AddNew();
			var synchroniser = new NonContainerizedNumberSynchroniser(billContainer, shipment);
			synchroniser.Synchronise(true);

			AssertEquals(CusInBondContainer.NonContainerizedNumber, billContainer.BC_ContainerNum);
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_HarmonisedCode = "10";
			packLine1.SetContainer(consol, container);
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_HarmonisedCode = "20";
			packLine2.SetContainer(consol, null);
			AssertEquals(1, billContainer.Commodities.Count);
			var commodity1 = billContainer.Commodities[0];
			AssertEquals("200000", commodity1.BY_HarmonisedTariff);

			var packLine3 = shipment.OuterPackLines.AddNew();
			packLine3.JL_HarmonisedCode = "30";
			packLine3.SetContainer(consol, null);
			var list = billContainer.Commodities.ToArray();
			AssertEquals(2, list.Length);
			AssertEquals(commodity1, list.First(x => x.BY_FormattedHarmonisedTariff == "2000.00"));
			var commodity2 = list.First(x => x.BY_FormattedHarmonisedTariff == "3000.00");
			AssertNotNull(commodity2);

			packLine2.SetContainer(consol, container);
			list = billContainer.Commodities.ToArray();
			AssertEquals(1, list.Length);
			AssertEquals(commodity2, list.First(x => x.BY_FormattedHarmonisedTariff == "3000.00"));
			AssertEquals(true, commodity1.IsDeleted);
		}
	}
}
