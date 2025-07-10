using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class NonContainerizedNumberSynchroniserTest : SynchroniserTestCase
	{
		public void TestPieceCountAndUnitSyncWithInnerPacksForNonContainerized()
		{
			var consol = CreateFCLConsol();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;

			var header = (CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1";
			var shipment = consol.Shipments.AddNew();
			var bill = header.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var billContainer = moveDetail.Containers.AddNew();

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 10;
			packLine1.JL_F3_NKPackType = "KG";
			packLine1.SetContainer(consol, null);
			var packLine2 = shipment.InnerPackLines.AddNew();
			packLine2.JL_PackageCount = 20;
			packLine2.JL_F3_NKPackType = "PL";
			packLine2.SetContainer(consol, null);

			var packProduct1 = packLine1.Products.AddNew();
			packProduct1.D2_ProductQuantity = 20;
			packProduct1.D2_ProductUnitOfQty = "PL";

			var synchroniser = new NonContainerizedNumberSynchroniser(billContainer, shipment);
			synchroniser.Synchronise(true);

			AssertEquals(CusInBondContainer.NonContainerizedNumber, billContainer.BC_ContainerNum);
			AssertEquals(1, billContainer.Commodities.Count);

			var commodity = billContainer.Commodities[0];
			AssertEquals(10, commodity.BY_PieceCount);
			AssertEquals("KG", commodity.BY_ManifestUnitCode);
		}
	}
}
