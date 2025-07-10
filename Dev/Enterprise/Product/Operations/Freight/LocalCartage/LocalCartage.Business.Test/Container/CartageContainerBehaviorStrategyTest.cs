using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageContainerBehaviorStrategyTest : TestCaseWithFactory
	{
		public void TestArrivalSlotDateTimeChanged()
		{
			var cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			var move1 = cartage1.ContainerBookedMoves.AddNew();
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			var move2 = cartage2.ContainerBookedMoves.AddNew();
			var now = ZDateTime.Now;
			move1.Container.JC_ArrivalSlotDateTime = now;
			move2.Container.JC_ArrivalSlotDateTime = now;
			AssertEquals("Arrival Slot requested", ZDateTime.Empty, cartage1.GetBookedMoves(move1.Container)[0].EW_RequestedPickupTimeStart);
			AssertEquals("Arrival Slot requested", ZDateTime.Empty, cartage1.GetBookedMoves(move1.Container)[0].EW_RequestedPickupTimeEnd);
			AssertEquals("Arrival Slot requested Leg", now, cartage2.GetBookedMoves(move2.Container)[0].CartageLegs[0].JU_PlannedPickupTime);
			AssertEquals("Arrival Slot requested", ZDateTime.Empty, cartage2.GetBookedMoves(move2.Container)[0].EW_RequestedPickupTimeStart);
			AssertEquals("Arrival Slot requested", ZDateTime.Empty, cartage2.GetBookedMoves(move2.Container)[0].EW_RequestedPickupTimeEnd);
		}

		public void TestDepartureSlotDateTimeChanged()
		{
			var cartage1 = Factory.New<CommonCartage>();
			cartage1.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			var move1 = cartage1.ContainerBookedMoves.AddNew();
			var cartage2 = Factory.New<CommonCartage>();
			cartage2.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			var move2 = cartage2.ContainerBookedMoves.AddNew();
			var cartage3 = Factory.New<CommonCartage>();
			cartage3.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCYDtoSHPWAITtoCTO;
			var move3 = cartage3.ContainerBookedMoves.AddNew();
			var now = ZDateTime.Now;
			move1.Container.JC_DepartureSlotDateTime = now;
			move2.Container.JC_DepartureSlotDateTime = now;
			move3.Container.JC_DepartureSlotDateTime = now;
			AssertEquals(2, move1.CartageLegs.Count);
			AssertEquals("Departure Slot to Est Delivery", ZDateTime.Empty, move1.CartageLegs[0].JU_EstimatedDeliveryTime);
			AssertEquals("Departure Slot to Est Delivery", now, move1.CartageLegs[1].JU_EstimatedDeliveryTime);
			AssertEquals(2, move2.CartageLegs.Count);
			AssertEquals("Departure Slot to Est Delivery", ZDateTime.Empty, move2.CartageLegs[0].JU_EstimatedDeliveryTime);
			AssertEquals("Departure Slot to Est Delivery", ZDateTime.Empty, move2.CartageLegs[1].JU_EstimatedDeliveryTime);
			AssertEquals(1, move3.CartageLegs.Count);
			AssertEquals("Departure Slot to Est Delivery", ZDateTime.Empty, move3.CartageLegs[0].JU_EstimatedDeliveryTime);
		}

		public void TestDepartureSlotAddsLegEvent()
		{
			var cto = Helper.CreateOrgHeader("CTOSYD", "CTO");
			cto.MainAddress.OA_City = "Botany";
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLExportToSHP;
			cartage.ThirdDocAddress.E2_OA_Address = cto.MainAddress.PK;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			container.JC_ContainerNum = "CONT123";
			var yardLeg = move.CartageLegs[0];
			var ctoLeg = move.CartageLegs[1];
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), FindEventReferences(yardLeg.PK, AutoEvents.SlotConfirmed));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), FindEventReferences(ctoLeg.PK, AutoEvents.SlotConfirmed));
			container.JC_DepartureSlotDateTime = ZDateTime.Now;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), FindEventReferences(yardLeg.PK, AutoEvents.SlotConfirmed));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), FindEventReferences(ctoLeg.PK, AutoEvents.SlotConfirmed));
			container.JC_DepartureSlotReference = "111";
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), FindEventReferences(yardLeg.PK, AutoEvents.SlotConfirmed));
			AssertContainsExactElementsInAnyOrder(new[] { "|FAC=CTO|LOC=Botany|RES=Delivery|RFN=111" }, FindEventReferences(ctoLeg.PK, AutoEvents.SlotConfirmed));
		}

		public void TestArrivalSlotAddsLegEvent()
		{
			var cto = Helper.CreateOrgHeader("CTOSYD", "CTO");
			cto.MainAddress.OA_City = "Botany";
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			cartage.FirstDocAddress.E2_OA_Address = cto.MainAddress.PK;
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			container.JC_ContainerNum = "CONT123";
			var ctoLeg = move.CartageLegs[0];
			var yardLeg = move.CartageLegs[1];
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), FindEventReferences(ctoLeg.PK, AutoEvents.SlotConfirmed));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), FindEventReferences(yardLeg.PK, AutoEvents.SlotConfirmed));
			container.JC_ArrivalSlotDateTime = ZDateTime.Now;
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), FindEventReferences(ctoLeg.PK, AutoEvents.SlotConfirmed));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), FindEventReferences(yardLeg.PK, AutoEvents.SlotConfirmed));
			container.JC_ArrivalSlotReference = "111";
			AssertContainsExactElementsInAnyOrder(new[] { "|FAC=CTO|LOC=Botany|RES=Pickup|RFN=111" }, FindEventReferences(ctoLeg.PK, AutoEvents.SlotConfirmed));
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), FindEventReferences(yardLeg.PK, AutoEvents.SlotConfirmed));
		}

		IEnumerable<string> FindEventReferences(ZGuid parentID, Event eventType)
		{
			return FindEvents(parentID, eventType).Select(e => e.SL_Reference.ToString());
		}

		IEnumerable<StmALog> FindEvents(ZGuid parentID, Event eventType)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, eventType.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, parentID);
			return Factory.Load<StmALog>(query);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
