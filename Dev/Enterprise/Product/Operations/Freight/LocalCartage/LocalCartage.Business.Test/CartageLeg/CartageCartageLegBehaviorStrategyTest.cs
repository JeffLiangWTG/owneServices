using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageCartageLegBehaviorStrategyTest : TestCaseWithFactory
	{
		public void TestDefaultingOfDatesOnLegs()
		{
			var year = ZDateTime.Now.Year;
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			var containerYard = Factory.NewWithValidTestData<OrgHeader>();
			containerYard.OH_IsContainerYard = true;
			containerYard.OH_FullName = "CONTAINER YARD";
			var move1 = cartage.ContainerBookedMoves.AddNew();
			var container1 = move1.Container;
			container1.JC_JK_OA_ArrivalUnpackAddress = containerYard.MainAddress.PK;
			container1.JC_JK_OA_DeparturePackAddress = containerYard.MainAddress.PK;
			var move2 = cartage.ContainerBookedMoves.AddNew();
			var container2 = move2.Container;
			container2.JC_JK_OA_ArrivalUnpackAddress = containerYard.MainAddress.PK;
			container2.JC_JK_OA_DeparturePackAddress = containerYard.MainAddress.PK;
			cartage.JJ_EstimatedPickup = new ZDateTime(year, 1, 1);
			cartage.JJ_EstimatedDelivery = new ZDateTime(year, 1, 2);
			AssertEquals("container 1 leg 1", new ZDateTime(year, 1, 1), cartage.GetBookedMoves(container1)[0].CartageLegs[0].JU_PlannedPickupTime);
			AssertEquals("container 1 leg 1", new ZDateTime(year, 1, 2), cartage.GetBookedMoves(container1)[0].CartageLegs[0].JU_EstimatedDeliveryTime);
			AssertEquals("container 1 leg 2", ZDateTime.Empty, cartage.GetBookedMoves(container1)[0].CartageLegs[1].JU_PlannedPickupTime);
			AssertEquals("container 1 leg 2", ZDateTime.Empty, cartage.GetBookedMoves(container1)[0].CartageLegs[1].JU_EstimatedDeliveryTime);
			AssertEquals("container 2 leg 1", new ZDateTime(year, 1, 1), cartage.GetBookedMoves(container2)[0].CartageLegs[0].JU_PlannedPickupTime);
			AssertEquals("container 2 leg 1", new ZDateTime(year, 1, 2), cartage.GetBookedMoves(container2)[0].CartageLegs[0].JU_EstimatedDeliveryTime);
			AssertEquals("container 2 leg 2", ZDateTime.Empty, cartage.GetBookedMoves(container2)[0].CartageLegs[1].JU_PlannedPickupTime);
			AssertEquals("container 2 leg 2", ZDateTime.Empty, cartage.GetBookedMoves(container2)[0].CartageLegs[1].JU_EstimatedDeliveryTime);
			container1.JC_ArrivalSlotDateTime = new ZDateTime(year, 1, 3);
			container1.JC_ArrivalEstimatedDelivery = new ZDateTime(year, 1, 4);
			container1.JC_EmptyReturnedBy = new ZDateTime(year, 1, 6);
			container2.JC_ArrivalEstimatedDelivery = new ZDateTime(year, 1, 5);
			container2.JC_EmptyReturnedBy = new ZDateTime(year, 1, 7);
			var move3 = cartage.ContainerBookedMoves.AddNew();
			var container3 = move3.Container;
			var provider = new CartageCartageLegBehaviorStrategy();
			ResetLegs(cartage, provider);
			AssertEquals("container 1 leg 1", new ZDateTime(year, 1, 3), cartage.GetBookedMoves(container1)[0].CartageLegs[0].JU_PlannedPickupTime);
			AssertEquals("container 1 leg 1", new ZDateTime(year, 1, 2), cartage.GetBookedMoves(container1)[0].CartageLegs[0].JU_EstimatedDeliveryTime);
			AssertEquals("container 1 leg 2", ZDateTime.Empty, cartage.GetBookedMoves(container1)[0].CartageLegs[1].JU_PlannedPickupTime);
			AssertEquals("container 1 leg 2", new ZDateTime(year, 1, 6), cartage.GetBookedMoves(container1)[0].CartageLegs[1].JU_EstimatedDeliveryTime);
			AssertEquals("container 2 leg 1", new ZDateTime(year, 1, 5), cartage.GetBookedMoves(container2)[0].CartageLegs[0].JU_PlannedPickupTime);
			AssertEquals("container 2 leg 1", new ZDateTime(year, 1, 2), cartage.GetBookedMoves(container2)[0].CartageLegs[0].JU_EstimatedDeliveryTime);
			AssertEquals("container 2 leg 2", ZDateTime.Empty, cartage.GetBookedMoves(container2)[0].CartageLegs[1].JU_PlannedPickupTime);
			AssertEquals("container 2 leg 2", new ZDateTime(year, 1, 7), cartage.GetBookedMoves(container2)[0].CartageLegs[1].JU_EstimatedDeliveryTime);
			AssertEquals("container 3 leg 1", new ZDateTime(year, 1, 1), cartage.GetBookedMoves(container3)[0].CartageLegs[0].JU_PlannedPickupTime);
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			container1.JC_DepartureEstimatedPickup = new ZDateTime(year, 1, 9);
			container1.JC_DepartureSlotDateTime = new ZDateTime(year, 1, 11);
			container1.JC_EmptyRequired = new ZDateTime(year, 1, 12);
			container2.JC_DepartureEstimatedPickup = new ZDateTime(year, 1, 10);
			container2.JC_EmptyRequired = new ZDateTime(year, 1, 13);
			ResetLegs(cartage, provider);
			AssertEquals("container 1 leg 1", ZDateTime.Empty, cartage.GetBookedMoves(container1)[0].CartageLegs[0].JU_PlannedPickupTime);
			AssertEquals("container 1 leg 1", new ZDateTime(year, 1, 12), cartage.GetBookedMoves(container1)[0].CartageLegs[0].JU_EstimatedDeliveryTime);
			AssertEquals("container 1 leg 2", new ZDateTime(year, 1, 9), cartage.GetBookedMoves(container1)[0].CartageLegs[1].JU_PlannedPickupTime);
			AssertEquals("container 1 leg 2", new ZDateTime(year, 1, 11), cartage.GetBookedMoves(container1)[0].CartageLegs[1].JU_EstimatedDeliveryTime);
			AssertEquals("container 2 leg 1", ZDateTime.Empty, cartage.GetBookedMoves(container2)[0].CartageLegs[0].JU_PlannedPickupTime);
			AssertEquals("container 2 leg 1", new ZDateTime(year, 1, 13), cartage.GetBookedMoves(container2)[0].CartageLegs[0].JU_EstimatedDeliveryTime);
			AssertEquals("container 2 leg 2", new ZDateTime(year, 1, 10), cartage.GetBookedMoves(container2)[0].CartageLegs[1].JU_PlannedPickupTime);
			AssertEquals("container 2 leg 2", new ZDateTime(year, 1, 2), cartage.GetBookedMoves(container2)[0].CartageLegs[1].JU_EstimatedDeliveryTime);
			AssertEquals("container 3 leg 1", ZDateTime.Empty, cartage.GetBookedMoves(container3)[0].CartageLegs[0].JU_PlannedPickupTime);
			AssertEquals("container 3 leg 1", ZDateTime.Empty, cartage.GetBookedMoves(container3)[0].CartageLegs[0].JU_EstimatedDeliveryTime);
			AssertEquals("container 3 leg 2", new ZDateTime(year, 1, 1), cartage.GetBookedMoves(container3)[0].CartageLegs[1].JU_PlannedPickupTime);
			AssertEquals("container 3 leg 2", new ZDateTime(year, 1, 2), cartage.GetBookedMoves(container3)[0].CartageLegs[1].JU_EstimatedDeliveryTime);
		}

		void ResetLegs(CommonCartage cartage, CartageCartageLegBehaviorStrategy provider)
		{
			foreach (var container in cartage.Containers)
			{
				foreach (var leg in cartage.GetBookedMoves(container)[0].CartageLegs)
				{
					leg.JU_PlannedPickupTime = ZDateTime.Empty;
					leg.JU_EstimatedDeliveryTime = ZDateTime.Empty;
					provider.DefaultDates(leg);
				}
			}
		}

		public void TestCartageEstimatedPickupDefaulting()
		{
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CommonContainer container1 = cartage1.ContainerBookedMoves.AddNew().Container;
			CommonContainer container2 = cartage1.ContainerBookedMoves.AddNew().Container;
			ZDateTime now = ZDateTime.Now;
			cartage1.JJ_EstimatedPickup = now;
			AssertEquals("Should only set First Booked Moves Full leg", ZDateTime.Empty, cartage1.GetBookedMoves(container1)[0].CartageLegs[0].JU_PlannedPickupTime);
			AssertEquals("Should only set First Booked Moves Full leg", now, cartage1.GetBookedMoves(container1)[0].CartageLegs[1].JU_PlannedPickupTime);
			AssertEquals("Should only set First Booked Moves Full leg", ZDateTime.Empty, cartage1.GetBookedMoves(container2)[0].CartageLegs[0].JU_PlannedPickupTime);
			AssertEquals("Should only set First Booked Moves Full leg", now, cartage1.GetBookedMoves(container2)[0].CartageLegs[1].JU_PlannedPickupTime);
			cartage1.ContainerBookedMoves.DeleteAll();
			AssertEquals("Since all container booked moves deleted, Container1 should be deleted.", true, container1.IsDeleted);
			AssertEquals("Since all container booked moves deleted, Container2 should be deleted.", true, container2.IsDeleted);
			cartage1.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			container1 = cartage1.ContainerBookedMoves.AddNew().Container;
			container2 = cartage1.ContainerBookedMoves.AddNew().Container;
			AssertEquals("Should only set First Booked Moves leg", now, cartage1.GetBookedMoves(container1)[0].CartageLegs[0].JU_PlannedPickupTime);
			AssertEquals("Should only set First Booked Moves leg", ZDateTime.Empty, cartage1.GetBookedMoves(container1)[0].CartageLegs[1].JU_PlannedPickupTime);
			AssertEquals("Should only set First Booked Moves leg", now, cartage1.GetBookedMoves(container2)[0].CartageLegs[0].JU_PlannedPickupTime);
			AssertEquals("Should only set First Booked Moves leg", ZDateTime.Empty, cartage1.GetBookedMoves(container2)[0].CartageLegs[1].JU_PlannedPickupTime);
		}

		public void TestCartageEstimatedDeliveryDefaulting()
		{
			CommonCartage cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			CommonContainer container1 = cartage1.ContainerBookedMoves.AddNew().Container;
			CommonContainer container2 = cartage1.ContainerBookedMoves.AddNew().Container;
			ZDateTime now = ZDateTime.Now;
			cartage1.JJ_EstimatedDelivery = now;
			AssertEquals("Should only set First Booked Moves leg", now, cartage1.GetBookedMoves(container1)[0].CartageLegs[0].JU_EstimatedDeliveryTime);
			AssertEquals("Should only set First Booked Moves leg", ZDateTime.Empty, cartage1.GetBookedMoves(container1)[0].CartageLegs[1].JU_EstimatedDeliveryTime);
			AssertEquals("Should only set First Booked Moves leg", now, cartage1.GetBookedMoves(container2)[0].CartageLegs[0].JU_EstimatedDeliveryTime);
			AssertEquals("Should only set First Booked Moves leg", ZDateTime.Empty, cartage1.GetBookedMoves(container2)[0].CartageLegs[1].JU_EstimatedDeliveryTime);
			cartage1.ContainerBookedMoves.DeleteAll();
			cartage1.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			container1 = cartage1.ContainerBookedMoves.AddNew().Container;
			container2 = cartage1.ContainerBookedMoves.AddNew().Container;
			AssertEquals("Should only set First Booked Moves leg", now, cartage1.GetBookedMoves(container1)[0].CartageLegs[0].JU_EstimatedDeliveryTime);
			AssertEquals("Should only set First Booked Moves leg", ZDateTime.Empty, cartage1.GetBookedMoves(container1)[0].CartageLegs[1].JU_EstimatedDeliveryTime);
			AssertEquals("Should only set First Booked Moves leg", now, cartage1.GetBookedMoves(container2)[0].CartageLegs[0].JU_EstimatedDeliveryTime);
			AssertEquals("Should only set First Booked Moves leg", ZDateTime.Empty, cartage1.GetBookedMoves(container2)[0].CartageLegs[1].JU_EstimatedDeliveryTime);
		}

		public void TestEstimatedDeliveryDefaulting_SlotDate()
		{
			var now = ZDateTime.Now;
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCYDtoSHPWAITtoCTO;
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_DepartureSlotDateTime = now;
			AssertEquals("Should NOT set the Wait Point (Delivery) Estimated Date to the Arrival Slot time (wait point is the consignor).", ZDateTime.Empty, cartage.GetBookedMoves(container)[0].CartageLegs[0].JU_EstimatedDeliveryTime);
			cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCFStoCTO;
			container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_DepartureSlotDateTime = now;
			AssertEquals("Should set the Delivery Estimated Date to the Arrival Slot time.", now, cartage.GetBookedMoves(container)[0].CartageLegs[0].JU_EstimatedDeliveryTime);
			cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_EmptyCYDtoSHP;
			container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_DepartureSlotDateTime = now;
			AssertEquals("Should NOT set the Delivery Estimated Date to the Arrival Slot time (it's the Consignor).", ZDateTime.Empty, cartage.GetBookedMoves(container)[0].CartageLegs[0].JU_EstimatedDeliveryTime);
		}

		public void TestWorkSheetCartageLegLinkCreated()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			var containerYard = Factory.NewWithValidTestData<OrgHeader>();
			containerYard.OH_IsContainerYard = true;
			containerYard.OH_FullName = "CONTAINER YARD";
			var container = cartage.ContainerBookedMoves.AddNew().Container;
			container.JC_JK_OA_ArrivalUnpackAddress = containerYard.MainAddress.PK;
			container.JC_JK_OA_DeparturePackAddress = containerYard.MainAddress.PK;
			var cartageLeg = cartage.GetBookedMoves(container)[0].CartageLegs[0];
			var workSheet = Factory.New<CommonWorkSheet>();
			workSheet.CartageLegs.Add(cartageLeg);
			var didCartageRaise = false;
			var didWorkSheetRaise = false;
			var didCartageLegRaise = false;
			cartage.OnWorkSheetLegLinkAdded += new EventHandler<WorkSheetLegLinkEventArgs>(delegate(object sender, WorkSheetLegLinkEventArgs e)
			{
				didCartageRaise = true;
				AssertEquals("OnWorkSheetLegLinkAdded args", cartageLeg, e.CartageLeg);
			});
			workSheet.OnCartageLegAdded += new EventHandler<WorkSheetLegLinkEventArgs>(delegate(object sender, WorkSheetLegLinkEventArgs e)
			{
				didWorkSheetRaise = true;
				AssertEquals("OnCartageLegAdded args", cartageLeg, e.CartageLeg);
			});
			cartageLeg.OnWorkSheetAdded += new EventHandler<WorkSheetLegLinkEventArgs>(delegate(object sender, WorkSheetLegLinkEventArgs e)
			{
				didCartageLegRaise = true;
				AssertEquals("OnWorkSheetAdded args", cartageLeg, e.CartageLeg);
			});
			var provider = new CartageCartageLegBehaviorStrategy();
			provider.WorkSheetCartageLegLinkCreated(cartageLeg);
			AssertEquals("OnWorkSheetLegLinkAdded", true, didCartageRaise);
			AssertEquals("OnCartageLegAdded", true, didWorkSheetRaise);
			AssertEquals("OnWorkSheetAdded", true, didCartageLegRaise);
		}

		public void TestPickupAddressIDChanged_MarkAsNeededAddressReorder()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			cartage.MarkAsHavingAddressesReordered();
			AssertEquals(false, cartage.ShouldReorderAddresses);
			var address = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS);
			AssertEquals(false, cartage.ShouldReorderAddresses);
			leg.JU_E2DeliveryAddressID = address.PK;
			AssertEquals(true, cartage.ShouldReorderAddresses);
		}

		public void TestWaitPointAddressIDChanged_MarkAsNeededAddressReorder()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			cartage.MarkAsHavingAddressesReordered();
			AssertEquals(false, cartage.ShouldReorderAddresses);
			var address = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS);
			AssertEquals(false, cartage.ShouldReorderAddresses);
			leg.JU_E2WaitPointAddressID = address.PK;
			AssertEquals(true, cartage.ShouldReorderAddresses);
		}

		public void TestDeliveryAddressIDChanged_MarkAsNeededAddressReorder()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_ContainerMode = Constants.CartageContainerMode.Loose;
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			cartage.MarkAsHavingAddressesReordered();
			AssertEquals(false, cartage.ShouldReorderAddresses);
			var address = cartage.DocAddresses.AddNew(DocAddressType.LocalCartageCFS);
			AssertEquals(false, cartage.ShouldReorderAddresses);
			leg.JU_E2PickupAddressID = address.PK;
			AssertEquals(true, cartage.ShouldReorderAddresses);
		}

		protected override void SetUp()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			base.SetUp();
		}
	}
}
