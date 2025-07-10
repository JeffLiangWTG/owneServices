using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.GUI.Testing
{
	public class QuickAddressHelperStrategyTest : TestCaseWithFactory
	{
		public void TestPickupLegQuickAddressHelperStrategy()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var container = cartage.Containers.First();
			var move = cartage.GetBookedMoves(container)[0];
			var leg = move.CartageLegs[0];
			var strategy = new PickupLegQuickAddressHelperStrategy(leg);
			AssertStrategy(strategy, leg.Lookups.GetCartageAddressElements(), cartage, leg.JU_E2PickupAddressIDInfo, leg.PickupDocAddressType, leg.Factory);
		}

		public void TestWaitPointLegQuickAddressHelperStrategy()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var container = cartage.Containers.First();
			var move = cartage.GetBookedMoves(container)[0];
			var leg = move.CartageLegs[0];
			var strategy = new WaitPointLegQuickAddressHelperStrategy(leg);
			AssertStrategy(strategy, leg.Lookups.GetCartageAddressElements(), cartage, leg.JU_E2WaitPointAddressIDInfo, leg.WaitPointDocAddressType, leg.Factory);
		}

		public void TestDeliveryLegQuickAddressHelperStrategy()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var container = cartage.Containers.First();
			var move = cartage.GetBookedMoves(container)[0];
			var leg = move.CartageLegs[0];
			var strategy = new DeliveryLegQuickAddressHelperStrategy(leg);
			AssertStrategy(strategy, leg.Lookups.GetCartageAddressElements(), cartage, leg.JU_E2DeliveryAddressIDInfo, leg.DeliverToDocAddressType, leg.Factory);
		}

		public void TestFirstBookingQuickAddressHelperStrategy()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var container = cartage.Containers.First();
			var move = cartage.GetBookedMoves(container)[0];
			var strategy = new FirstBookingQuickAddressHelperStrategy(move);
			AssertStrategy(strategy, move.Lookups.GetCartageAddressElements(), cartage, move.EW_E2PickupAddressIDInfo, move.PickupDocAddressType, move.Factory);
		}

		public void TestSecondBookingQuickAddressHelperStrategy()
		{
			var cartage = Helper.CreateCartage(Constants.CartageJobType.NEW_FCLImportToCNE, 1);
			var container = cartage.Containers.First();
			var move = cartage.GetBookedMoves(container)[0];
			var strategy = new SecondBookingQuickAddressHelperStrategy(move);
			AssertStrategy(strategy, move.Lookups.GetCartageAddressElements(), cartage, move.EW_E2WaitPointAddressIDInfo, move.WaitPointDocAddressType, move.Factory);
		}

		void AssertStrategy(QuickAddressHelperStrategy strategy, CartageBindToLists.AddressSelectionElement[] addressElements, IDocAddresses parent, ZPropertyInfo info, DocAddressType defaultAddressType, BusinessObjectFactory factory)
		{
			AssertEquals(addressElements, strategy.GetAddressElements());
			AssertEquals(parent, strategy.Parent);
			AssertEquals(info, strategy.Info);
			AssertEquals(defaultAddressType, strategy.DefaultAddressType);
			AssertEquals(factory, strategy.Factory);
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
