using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business
{
	[TestedType(typeof(DocumentCartageLegCollection))]
	public class DocumentCartageLegCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentCartageLegCollection>
	{
		protected override DocumentCartageLegCollection GetCollectionToTest()
		{
			var cartageLegs = new CommonCartageLegCollection(Factory);
			return new DocumentCartageLegCollection(cartageLegs);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cartageLeg = Factory.New<CommonCartageLeg>();
			return new DocumentCartageLeg(cartageLeg);
		}

		public void TestCartageLegsGroupsWithSameInfo()
		{
			var now = ZDateTime.Now;
			var commonCartage = Factory.NewWithValidTestData<CommonCartage>();
			CommonBookedCtgMove bookedMove1 = commonCartage.BookedMovesCollection.AddNew();
			CommonBookedCtgMove bookedMove2 = commonCartage.BookedMovesCollection.AddNew();
			CommonCartageLeg cartageLeg1 = bookedMove1.CartageLegs.AddNew();
			CommonCartageLeg cartageLeg2 = bookedMove2.CartageLegs.AddNew();
			var pickUpOrg1 = commonCartage.DocAddresses.AddNew();
			var pickUpOrg2 = commonCartage.DocAddresses.AddNew();
			var waitPointOrg1 = commonCartage.DocAddresses.AddNew();
			var waitPointOrg2 = commonCartage.DocAddresses.AddNew();
			var deliveryOrg1 = commonCartage.DocAddresses.AddNew();
			var deliveryOrg2 = commonCartage.DocAddresses.AddNew();
			cartageLeg1.JU_E2PickupAddressID = pickUpOrg1.PK;
			cartageLeg1.JU_E2WaitPointAddressID = waitPointOrg1.PK;
			cartageLeg1.JU_E2DeliveryAddressID = deliveryOrg1.PK;
			cartageLeg1.JU_PlannedPickupTime = now.AddDays(1);
			cartageLeg1.JU_EstimatedDeliveryTime = now.AddDays(2);
			bookedMove1.EW_DropMode = "PSL";
			cartageLeg2.JU_E2PickupAddressID = pickUpOrg1.PK;
			cartageLeg2.JU_E2WaitPointAddressID = waitPointOrg1.PK;
			cartageLeg2.JU_E2DeliveryAddressID = deliveryOrg1.PK;
			cartageLeg2.JU_PlannedPickupTime = now.AddDays(1);
			cartageLeg2.JU_EstimatedDeliveryTime = now.AddDays(2);
			bookedMove2.EW_DropMode = "PSL";
			var legCollection = new DocumentCartageLegCollection(commonCartage.CartageLegs);
			AssertEquals("Since we are not grouping cartage legs, leg collection should have two legs.", 2, legCollection.Count);
			AssertEquals("Leg collection should have cartage leg 1.", cartageLeg1, legCollection[0].CartageLeg);
			AssertEquals("Leg collection should have cartage leg 2.", cartageLeg2, legCollection[1].CartageLeg);
			legCollection = new DocumentCartageLegCollection(commonCartage.CartageLegs, true);
			AssertEquals("Since the two legs have the same information, legCollection should have one leg.", 1, legCollection.Count);
			cartageLeg2.JU_E2PickupAddressID = pickUpOrg2.PK;
			legCollection = new DocumentCartageLegCollection(commonCartage.CartageLegs, true);
			AssertEquals("Since two legs have different pickup Addresses legCollection should have two legs.", 2, legCollection.Count);
			AssertEquals("Leg collection should have cartage leg 1.", cartageLeg1, legCollection[0].CartageLeg);
			AssertEquals("Leg collection should have cartage leg 2.", cartageLeg2, legCollection[1].CartageLeg);
			cartageLeg2.JU_E2PickupAddressID = pickUpOrg1.PK;
			cartageLeg2.JU_E2WaitPointAddressID = waitPointOrg2.PK;
			legCollection = new DocumentCartageLegCollection(commonCartage.CartageLegs, true);
			AssertEquals("Since two legs have different delivery Addresses legCollection should have two legs.", 2, legCollection.Count);
			AssertEquals("Leg collection should have cartage leg 1.", cartageLeg1, legCollection[0].CartageLeg);
			AssertEquals("Leg collection should have cartage leg 2.", cartageLeg2, legCollection[1].CartageLeg);
			cartageLeg2.JU_E2WaitPointAddressID = waitPointOrg1.PK;
			cartageLeg2.JU_E2DeliveryAddressID = deliveryOrg2.PK;
			legCollection = new DocumentCartageLegCollection(commonCartage.CartageLegs, true);
			AssertEquals("Since two legs have different delivery Addresses legCollection should have two legs.", 2, legCollection.Count);
			AssertEquals("Leg collection should have cartage leg 1.", cartageLeg1, legCollection[0].CartageLeg);
			AssertEquals("Leg collection should have cartage leg 2.", cartageLeg2, legCollection[1].CartageLeg);
			cartageLeg2.JU_E2DeliveryAddressID = deliveryOrg1.PK;
			cartageLeg2.JU_PlannedPickupTime = now.AddDays(4);
			legCollection = new DocumentCartageLegCollection(commonCartage.CartageLegs, true);
			AssertEquals("Since two legs have different planned pickup times legCollection should have two legs.", 2, legCollection.Count);
			AssertEquals("Leg collection should have cartage leg 1.", cartageLeg1, legCollection[0].CartageLeg);
			AssertEquals("Leg collection should have cartage leg 2.", cartageLeg2, legCollection[1].CartageLeg);
			cartageLeg2.JU_PlannedPickupTime = now.AddDays(1);
			cartageLeg2.JU_EstimatedDeliveryTime = now.AddDays(4);
			legCollection = new DocumentCartageLegCollection(commonCartage.CartageLegs, true);
			AssertEquals("Since two legs have different estimated delivery times legCollection should have two legs.", 2, legCollection.Count);
			AssertEquals("Leg collection should have cartage leg 1.", cartageLeg1, legCollection[0].CartageLeg);
			AssertEquals("Leg collection should have cartage leg 2.", cartageLeg2, legCollection[1].CartageLeg);
			cartageLeg2.JU_EstimatedDeliveryTime = now.AddDays(2);
			bookedMove2.EW_DropMode = "HSL";
			legCollection = new DocumentCartageLegCollection(commonCartage.CartageLegs, true);
			AssertEquals("Since two legs have different drop modes legCollection should have two legs.", 2, legCollection.Count);
			AssertEquals("Leg collection should have cartage leg 1.", cartageLeg1, legCollection[0].CartageLeg);
			AssertEquals("Leg collection should have cartage leg 2.", cartageLeg2, legCollection[1].CartageLeg);
			bookedMove2.EW_DropMode = "PSL";
			legCollection = new DocumentCartageLegCollection(commonCartage.CartageLegs, true);
			AssertEquals("Since two legs have same information legCollection should have one leg.", 1, legCollection.Count);
		}
	}
}
