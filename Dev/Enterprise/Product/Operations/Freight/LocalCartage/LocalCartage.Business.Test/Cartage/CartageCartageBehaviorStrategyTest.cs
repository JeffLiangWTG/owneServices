using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageCartageBehaviorStrategyTest : TestCaseWithFactory
	{
		public void TestJobTypeChanged()
		{
			var cartage = Factory.New<CommonCartage>();
			cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLImportToCNE;
			var container1 = cartage.ContainerBookedMoves.AddNew().Container;
			var container2 = cartage.ContainerBookedMoves.AddNew().Container;
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_FCLImportUnpack;
			AssertEquals(Core.Constants.CartageJobType.NEW_FCLImportUnpack, cartage.JJ_E3_NKJobType);
			AssertEquals(2, cartage.Containers.Count());
			AssertEquals(2, cartage.GetBookedMoves(container1)[0].CartageLegs.Count);
			AssertEquals(2, cartage.GetBookedMoves(container2)[0].CartageLegs.Count);
			AssertEquals(0, cartage.LooseBookedMoves.Count);
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			AssertEquals(Core.Constants.CartageJobType.NEW_FCLUnpackLooseToCNE, cartage.JJ_E3_NKJobType);
			AssertEquals(2, cartage.Containers.Count());
			AssertEquals(2, cartage.GetBookedMoves(container1)[0].CartageLegs.Count);
			AssertEquals(2, cartage.GetBookedMoves(container2)[0].CartageLegs.Count);
			AssertEquals(1, cartage.LooseBookedMoves.Count);
			AssertEquals(1, cartage.LooseBookedMoves[0].CartageLegs.Count);
			cartage.LooseBookedMoves.AddNew();
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_AirImport;
			AssertEquals(Core.Constants.CartageJobType.NEW_AirImport, cartage.JJ_E3_NKJobType);
			AssertEquals(0, cartage.Containers.Count());
			AssertEquals(2, cartage.LooseBookedMoves.Count);
			AssertEquals(1, cartage.LooseBookedMoves[0].CartageLegs.Count);
			AssertEquals(1, cartage.LooseBookedMoves[1].CartageLegs.Count);
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			AssertEquals(Core.Constants.CartageJobType.NEW_FCLUnpackLooseToCNE, cartage.JJ_E3_NKJobType);
			AssertEquals(0, cartage.Containers.Count());
			AssertEquals(2, cartage.LooseBookedMoves.Count);
			AssertEquals(1, cartage.LooseBookedMoves[0].CartageLegs.Count);
			AssertEquals(1, cartage.LooseBookedMoves[1].CartageLegs.Count);
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_FCLImportUnpack;
			AssertEquals(Core.Constants.CartageJobType.NEW_FCLImportUnpack, cartage.JJ_E3_NKJobType);
			AssertEquals(0, cartage.Containers.Count());
			AssertEquals(0, cartage.LooseBookedMoves.Count);
		}

		public void TestCreateDefaultContainerisedLegs()
		{
			var iSMY = Helper.CreateCartageType("ISM1");
			var cTO = Helper.CreateCartageTypeOrg(iSMY, "CTO");
			var cFS = Helper.CreateCartageTypeOrg(iSMY, "CFS");
			var cNE = Helper.CreateCartageTypeOrg(iSMY, "CNE");
			var cYD = Helper.CreateCartageTypeOrg(iSMY, "CYD");
			var containerMove = Helper.CreateCartageMoveType(iSMY, "CNT", cTO, cFS);
			var containerLeg1 = Helper.CreateCartageLegType(iSMY, "CNT", cTO, null, cFS);
			var containerLeg2 = Helper.CreateCartageLegType(iSMY, "CNT", cFS, null, cYD);
			var looseMove = Helper.CreateCartageMoveType(iSMY, "LSE", cFS, cNE);
			var looseLeg = Helper.CreateCartageLegType(iSMY, "LSE", cFS, null, cNE);
			var cartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLCTOtoCNE, 1);
			var container = cartage.Containers.First();
			var move = cartage.GetBookedMoves(container)[0];
			cartage.JJ_E3_NKJobType = "ISM1";
			AssertEquals(2, move.CartageLegs.Count);
			AssertEquals(cartage.FirstDocAddress.PK, move.EW_E2PickupAddressID);
			AssertEquals(cartage.SecondDocAddress.PK, move.EW_E2WaitPointAddressID);
			AssertEquals(ZGuid.Empty, move.EW_E2DeliveryAddressID);
			var leg1 = move.CartageLegs[0];
			AssertEquals(cartage.FirstDocAddress.PK, leg1.JU_E2PickupAddressID);
			AssertEquals(ZGuid.Empty, leg1.JU_E2WaitPointAddressID);
			AssertEquals(cartage.SecondDocAddress.PK, leg1.JU_E2DeliveryAddressID);
			var leg2 = move.CartageLegs[1];
			AssertEquals(cartage.SecondDocAddress.PK, leg2.JU_E2PickupAddressID);
			AssertEquals(ZGuid.Empty, leg2.JU_E2WaitPointAddressID);
			AssertEquals(cartage.ThirdDocAddress.PK, leg2.JU_E2DeliveryAddressID);
		}

		public void TestCreateDefaultLooseLegs()
		{
			var iSMY = Helper.CreateCartageType("ISM1");
			var cTO = Helper.CreateCartageTypeOrg(iSMY, "CTO");
			var cFS = Helper.CreateCartageTypeOrg(iSMY, "CFS");
			var cNE = Helper.CreateCartageTypeOrg(iSMY, "CNE");
			var cYD = Helper.CreateCartageTypeOrg(iSMY, "CYD");
			var containerMove = Helper.CreateCartageMoveType(iSMY, "CNT", cTO, cFS);
			var containerLeg1 = Helper.CreateCartageLegType(iSMY, "CNT", cTO, null, cFS);
			var containerLeg2 = Helper.CreateCartageLegType(iSMY, "CNT", cFS, null, cYD);
			var looseMove = Helper.CreateCartageMoveType(iSMY, "LSE", cFS, cNE);
			var looseLeg = Helper.CreateCartageLegType(iSMY, "LSE", cFS, null, cNE);
			var cartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_AirExport, 1);
			cartage.JJ_E3_NKJobType = "ISM1";
			var move = cartage.LooseBookedMoves[0];
			AssertEquals(1, move.CartageLegs.Count);
			AssertEquals(cartage.SecondDocAddress.PK, move.EW_E2PickupAddressID);
			AssertEquals(cartage.FourthDocAddress.PK, move.EW_E2WaitPointAddressID);
			AssertEquals(ZGuid.Empty, move.EW_E2DeliveryAddressID);
			var leg1 = move.CartageLegs[0];
			AssertEquals(cartage.SecondDocAddress.PK, leg1.JU_E2PickupAddressID);
			AssertEquals(ZGuid.Empty, leg1.JU_E2WaitPointAddressID);
			AssertEquals(cartage.FourthDocAddress.PK, leg1.JU_E2DeliveryAddressID);
		}

		public void TesttDefaultAddresses_Containerised()
		{
			var iSMY = Helper.CreateCartageType("ISM1");
			var cTO = Helper.CreateCartageTypeOrg(iSMY, "CTO");
			var cFS = Helper.CreateCartageTypeOrg(iSMY, "CFS");
			var cNE = Helper.CreateCartageTypeOrg(iSMY, "CNE");
			var cYD = Helper.CreateCartageTypeOrg(iSMY, "CYD");
			var containerMove = Helper.CreateCartageMoveType(iSMY, "CNT", cTO, cFS);
			var containerLeg1 = Helper.CreateCartageLegType(iSMY, "CNT", cTO, null, cFS);
			var containerLeg2 = Helper.CreateCartageLegType(iSMY, "CNT", cFS, null, cYD);
			var looseMove = Helper.CreateCartageMoveType(iSMY, "LSE", cFS, cNE);
			var looseLeg = Helper.CreateCartageLegType(iSMY, "LSE", cFS, null, cNE);
			var cartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_FCLCTOtoCNE, 1);
			cartage.JJ_E3_NKJobType = "ISM1";
			var move = cartage.ContainerBookedMoves.AddNew();
			var container = move.Container;
			AssertEquals(cartage.FirstDocAddress.PK, move.EW_E2PickupAddressID);
			AssertEquals(cartage.SecondDocAddress.PK, move.EW_E2WaitPointAddressID);
			AssertEquals(ZGuid.Empty, move.EW_E2DeliveryAddressID);
		}

		public void TestDefaultAddresses_Loose()
		{
			var iSMY = Helper.CreateCartageType("ISM1");
			var cTO = Helper.CreateCartageTypeOrg(iSMY, "CTO");
			var cFS = Helper.CreateCartageTypeOrg(iSMY, "CFS");
			var cNE = Helper.CreateCartageTypeOrg(iSMY, "CNE");
			var cYD = Helper.CreateCartageTypeOrg(iSMY, "CYD");
			var containerMove = Helper.CreateCartageMoveType(iSMY, "CNT", cTO, cFS);
			var containerLeg1 = Helper.CreateCartageLegType(iSMY, "CNT", cTO, null, cFS);
			var containerLeg2 = Helper.CreateCartageLegType(iSMY, "CNT", cFS, null, cYD);
			var looseMove = Helper.CreateCartageMoveType(iSMY, "LSE", cFS, cNE);
			var looseLeg = Helper.CreateCartageLegType(iSMY, "LSE", cFS, null, cNE);
			var cartage = Helper.CreateCartage(Core.Constants.CartageJobType.NEW_AirExport, 1);
			cartage.JJ_E3_NKJobType = "ISM1";
			var move = cartage.LooseBookedMoves[0];
			AssertEquals(1, move.CartageLegs.Count);
			AssertEquals(cartage.SecondDocAddress.PK, move.EW_E2PickupAddressID);
			AssertEquals(cartage.FourthDocAddress.PK, move.EW_E2WaitPointAddressID);
			AssertEquals(ZGuid.Empty, move.EW_E2DeliveryAddressID);
		}

		protected override void SetUp()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			base.SetUp();
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
