using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	class InternalCartageManagerHelperTest : TestCaseWithFactory
	{
		public void TestPopulateCartage()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLImportToCNE, null);
			cartageType.SetDropMode(Constants.FCLEquipmentNeeded.Trailer);
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals(Constants.FCLEquipmentNeeded.Trailer, cartage.JJ_DropMode);
		}

		public void TestUNDGs_Containerised()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLImportToCNE, null);
			RefContainer refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			DummyCartageContainer container1 = new DummyCartageContainer(Factory, "C01", "FCL", refContainer20GP.PK);
			DummyCartageLooseCargo looseGoods1 = new DummyCartageLooseCargo(Factory, 3);
			ICartageContainer[] containers = new ICartageContainer[] { container1 };
			ICartageLooseCargo[] looseGoods = new ICartageLooseCargo[] { looseGoods1 };
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			var move = cartage.ContainerBookedMoves[0];
			AssertEquals(0, move.UNDGs.Count);
		}

		public void TestPopulateContainers()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLImportToCNE, null);
			RefContainer refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			DummyCartageContainer container1 = new DummyCartageContainer(Factory, "C01", "FCL", refContainer20GP.PK, 123.45, "SealNum");
			DummyCartageLooseCargo looseGoods1 = new DummyCartageLooseCargo(Factory, 3);
			ICartageContainer[] containers = new ICartageContainer[] { container1 };
			ICartageLooseCargo[] looseGoods = new ICartageLooseCargo[] { looseGoods1 };
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals("SealNum", cartage.ContainerBookedMoves[0].Container.JC_SealNum);
			AssertEquals(123.45m, cartage.ContainerBookedMoves[0].Container.JC_Calc_NetWeight);
		}

		public void TestPopulateContainers_UpdatesTotalPackages()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLImportToCNE, null);
			CommonContainer container = Factory.New<CommonContainer>();
			PackLine packLine = Factory.New<PackLine>();
			packLine.JL_FreightMode = "XXX";
			packLine.JL_F3_NKPackType = "CTN";
			packLine.JL_PackageCount = 3;
			container.AddPackLine(packLine);
			DummyCartageContainer dummyContainer = new DummyCartageContainer(Factory, "C01", "FCL", ZGuid.Empty, 123.45, "SealNum", container.PK);
			ICartageContainer[] containers = new ICartageContainer[] { dummyContainer };
			cartageType.SetContainers(containers);
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals(3, cartage.GetBookedMoves(container)[0].EW_BookedPackCount);
			AssertEquals("CTN", cartage.GetBookedMoves(container)[0].EW_F3_NKPackType);
		}

		public void TestUNDGs_Loose()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_LCLImport, null);
			RefContainer refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			DummyCartageContainer container1 = new DummyCartageContainer(Factory, "C01", "FCL", refContainer20GP.PK);
			DummyCartageLooseCargo looseGoods1 = new DummyCartageLooseCargo(Factory, 3);
			ICartageContainer[] containers = new ICartageContainer[] { container1 };
			ICartageLooseCargo[] looseGoods = new ICartageLooseCargo[] { looseGoods1 };
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals(0, cartage.LooseBookedMoves[0].UNDGs.Count);
		}

		public void TestCartageDoesntGetValidationError()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_LCLImport, null);
			RefContainer refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			DummyCartageContainer container1 = new DummyCartageContainer(Factory, "C01", "FCL", refContainer20GP.PK);
			DummyCartageLooseCargo looseGoods1 = new DummyCartageLooseCargo(Factory, 3);
			looseGoods1.SetBooked(1, "PT", 12, "KG", 14, "LT");
			looseGoods1.SetDimensions(0, 0, 0, "");
			ICartageContainer[] containers = new ICartageContainer[] { container1 };
			ICartageLooseCargo[] looseGoods = new ICartageLooseCargo[] { looseGoods1 };
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			dummyParent.ICartageParentUseJobTotals = true;
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertNoWarnings(cartage.JJ_WeightInfo);
			AssertNoWarnings(cartage.JJ_VolumeInfo);
			AssertNoWarnings(cartage.LooseBookedMoves[0].EW_DimUnitInfo);
			AssertNoErrors(cartage.LooseBookedMoves[0].EW_DimUnitInfo);
		}

		public void TestLogCartageDeactivationOnParent()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLImportToCNE, null);
			// to instantiate LocalTransportProviderAddress
			var dummyTransportProviderAddress = cartageType.LocalTransportProviderAddress;
			cartageType.LocalTransportProviderAddress.Header.OH_FullName = "A Local Transport Provider Org";
			CommonCartage cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			cartage.JJ_ConsignmentID = "XX1234";
			cartage.JJ_Direction = "EXP";
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.LogCartageDeactivationOnParent(cartage);
			var mostRecentLog = dummyParent.Logs.MostRecentLog;
			AssertEquals("Most recent log should be ICX - Internal Cartage Deactivated", Events.InternalCartageJobDeactivatedCode, mostRecentLog.SL_SE_NKEvent);
			AssertEquals("Most recent log should have reference in correct format and with correct values for parameters JOB (JobID), NAM (Transport Provider Org Name) and TYP (Direction)", "|JOB=XX1234|NAM=A Local Transport Provider Org|TYP=EXP", mostRecentLog.SL_Reference);
		}

		protected override void SetUp()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			base.SetUp();
		}
	}
}
