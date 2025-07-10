using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CartageInternalCartageBehaviorStrategyTest : TestCaseWithFactory
	{
		public void TestJobTypeChanged_OK()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLImportToCNE, null);
			RefContainer refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			DummyCartageContainer container1 = new DummyCartageContainer(Factory, "C01", "FCL", refContainer20GP.PK);
			DummyCartageContainer container2 = new DummyCartageContainer(Factory, "C02", "FCL", refContainer20GP.PK);
			DummyCartageContainer container3 = new DummyCartageContainer(Factory, "C03", "FCL", refContainer20GP.PK);
			DummyCartageLooseCargo looseGoods1 = new DummyCartageLooseCargo(Factory, 3);
			DummyCartageLooseCargo looseGoods2 = new DummyCartageLooseCargo(Factory, 4);
			DummyCartageLooseCargo looseGoods3 = new DummyCartageLooseCargo(Factory, 5);
			ICartageContainer[] containers = new ICartageContainer[] { container1, container2, container3 };
			ICartageLooseCargo[] looseGoods = new ICartageLooseCargo[] { looseGoods1, looseGoods2, looseGoods3 };
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			//Setup Cartage
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals(Core.Constants.CartageJobType.NEW_FCLImportToCNE, cartage.JJ_E3_NKJobType);
			AssertEquals(3, cartage.Containers.Count());
			AssertEquals(0, cartage.LooseBookedMoves.Count);
			//Remove a Container from the Cartage to cause a discrepancy with Parent
			BusinessObject c1 = cartage.Containers.ElementAt(0);
			BusinessObject c2 = cartage.Containers.ElementAt(1);
			BusinessObject c3 = cartage.Containers.ElementAt(2);
			c3.Delete();
			AssertEquals(2, cartage.Containers.Count());
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_FCLImportUnpack;
			AssertEquals(Core.Constants.CartageJobType.NEW_FCLImportUnpack, cartage.JJ_E3_NKJobType);
			AssertEquals(3, cartage.Containers.Count()); //repopulated
			AssertEquals(0, cartage.LooseBookedMoves.Count);
		}

		public void TestJobTypeChanged_OKContainerToLoose()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_FCLImportToCNE, null);
			RefContainer refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			DummyCartageContainer container1 = new DummyCartageContainer(Factory, "C01", "FCL", refContainer20GP.PK);
			DummyCartageContainer container2 = new DummyCartageContainer(Factory, "C02", "FCL", refContainer20GP.PK);
			DummyCartageContainer container3 = new DummyCartageContainer(Factory, "C03", "FCL", refContainer20GP.PK);
			DummyCartageLooseCargo looseGoods1 = new DummyCartageLooseCargo(Factory, 3);
			DummyCartageLooseCargo looseGoods2 = new DummyCartageLooseCargo(Factory, 4);
			DummyCartageLooseCargo looseGoods3 = new DummyCartageLooseCargo(Factory, 5);
			ICartageContainer[] containers = new ICartageContainer[] { container1, container2, container3 };
			ICartageLooseCargo[] looseGoods = new ICartageLooseCargo[] { looseGoods1, looseGoods2, looseGoods3 };
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			//Setup Cartage
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals(Core.Constants.CartageJobType.NEW_FCLImportToCNE, cartage.JJ_E3_NKJobType);
			AssertEquals(3, cartage.Containers.Count());
			AssertEquals(0, cartage.LooseBookedMoves.Count);
			//Remove a Container from the Cartage to cause a discrepancy with Parent
			BusinessObject c1 = cartage.Containers.ElementAt(0);
			BusinessObject c2 = cartage.Containers.ElementAt(1);
			BusinessObject c3 = cartage.Containers.ElementAt(2);
			c3.Delete();
			AssertEquals(2, cartage.Containers.Count());
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_FCLUnpackLooseToCNE;
			AssertEquals(Core.Constants.CartageJobType.NEW_FCLUnpackLooseToCNE, cartage.JJ_E3_NKJobType);
			AssertEquals(3, cartage.Containers.Count()); //repopulated
			AssertEquals(3, cartage.LooseBookedMoves.Count); //populated
		}

		public void TestJobTypeChanged_OKLoose()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_LCLImport, null);
			RefContainer refContainer20GP = new RefContainer.Loader(Factory).LoadFromCode("20GP");
			DummyCartageContainer container1 = new DummyCartageContainer(Factory, "C01", "FCL", refContainer20GP.PK);
			DummyCartageContainer container2 = new DummyCartageContainer(Factory, "C02", "FCL", refContainer20GP.PK);
			DummyCartageContainer container3 = new DummyCartageContainer(Factory, "C03", "FCL", refContainer20GP.PK);
			DummyCartageLooseCargo looseGoods1 = new DummyCartageLooseCargo(Factory, 3);
			DummyCartageLooseCargo looseGoods2 = new DummyCartageLooseCargo(Factory, 4);
			DummyCartageLooseCargo looseGoods3 = new DummyCartageLooseCargo(Factory, 5);
			ICartageContainer[] containers = new ICartageContainer[] { container1, container2, container3 };
			ICartageLooseCargo[] looseGoods = new ICartageLooseCargo[] { looseGoods1, looseGoods2, looseGoods3 };
			cartageType.SetContainers(containers);
			cartageType.SetLoose(looseGoods);
			//Setup Cartage
			CartageForTest cartage = Factory.New<CartageForTest>();
			cartage.SetParent(dummyParent);
			dummyParent.SetCartageType(cartageType);
			InternalCartageManagerHelper.PopulateCartage(cartage, cartageType);
			AssertEquals(Core.Constants.CartageJobType.NEW_LCLImport, cartage.JJ_E3_NKJobType);
			AssertEquals(0, cartage.Containers.Count());
			AssertEquals(3, cartage.LooseBookedMoves.Count);
			//Remove a Container from the Cartage to cause a discrepancy with Parent
			CommonBookedCtgMove m1 = cartage.LooseBookedMoves[0];
			CommonBookedCtgMove m2 = cartage.LooseBookedMoves[1];
			CommonBookedCtgMove m3 = cartage.LooseBookedMoves[2];
			m3.Delete();
			AssertEquals(2, cartage.LooseBookedMoves.Count);
			cartage.JJ_E3_NKJobType = Enterprise.Core.Constants.CartageJobType.NEW_AirImport;
			AssertEquals(Core.Constants.CartageJobType.NEW_AirImport, cartage.JJ_E3_NKJobType);
			AssertEquals(0, cartage.Containers.Count());
			AssertEquals(3, cartage.LooseBookedMoves.Count); //repopulated
		}

		protected override void SetUp()
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(Factory, new CartageBehaviorStrategyProvider());
			base.SetUp();
		}
	}
}
