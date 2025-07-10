using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	class WhsItemPackageStateOverpackHUAndChildPackagesHaveSameRCNandDCNDeferTriggerStrategyTest : TestCaseWithFactory
	{
		public void TestRunType()
		{
			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IWhsItemPackageStateOverpackHUAndChildPackagesHaveSameRCNandDCNDeferTriggerStrategy));
			AssertEquals(strategy.RunType, TriggerRunType.InsertOrUpdate);
		}

		public void TestShouldDeferTrigger()
		{
			var now = DateTimeOffset.Now;
			var data = new TransitTestDataSimpleEnvironment(Factory, 2, 1);
			var strategy = ObjectFactory.Get<IDeferTriggerConditionStrategy>(nameof(IWhsItemPackageStateOverpackHUAndChildPackagesHaveSameRCNandDCNDeferTriggerStrategy));
			var warehouse = Helper.CreateTRWWarehouse();
			Factory.Save();
			var rcn = Helper.CreateReceiveConsignment("RC1", warehouse.PK);
			var packageState = Helper.CreatePackageState(rcn, 1, "PKG", "PKGDEP1", TransitWarehouseStatuses.Codes.Booked);

			AssertEquals("Trigger should be deferred.", true, strategy.ShouldDeferTrigger(packageState));
		}

		WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;
	}
}
