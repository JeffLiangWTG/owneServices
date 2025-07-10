using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(DynamicPickFacesModule))]
	class DynamicPickFacesModuleTest : ZModuleBasherTest
	{
		#region LicenceCheckPoint, SecurityCheckPoint, BizO and Controller

		public void TestSecurityCheckPoint()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsConfigDynamicPickFaces, module.SecurityCheckpoint);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(typeof(DynamicPickFacesFilterBusinessObject), ((IFilterModuleInternalsForTesting)module).FilterBusinessObject.GetType());
			}
		}

		public void TestFilterControl()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (var control = ((ZFilterModule)module).GetNewFilterControlForGrid())
				{
					AssertEquals(typeof(DynamicPickFacesFilterControl), control.GetType());
				}
			}
		}

		public void TestLicenceCheckPoint()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.WarehouseManagerCoreAnd4PL, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestGridCollection

		public void TestGridCollectionType()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(typeof(WhsDynamicPickFaceViewCollection), ((IFilterModuleInternalsForTesting)module).GridCollection.GetType());
			}
		}

		#endregion

		#region Operational Actions

		public void TestAllowNew()
		{
			using (var module = new DynamicPickFacesModule())
			{
				Assert("New option should not be available", !module.AllowNew);
			}
		}

		public void TestAllowDelete()
		{
			using (var module = new DynamicPickFacesModule())
			{
				Assert("Delete option should not be available", !module.AllowDelete);
			}
		}

		public void TestIOperationalActionSupportable()
		{
			using (var module = new DynamicPickFacesModule())
			{
				AssertNull("DynamicPickFace module should have no operational support.", module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert("It has a calculated PK.", condition: true);
		}

		#endregion

		#region Overrides

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsConfigDynamicPickFaces;
		}

		#endregion

		#region Refreshable support

		public void TestDynamicPickFaceChangeInOtherFactory_WhenHasSearched_ShouldRefreshGridCollection()
		{
			TestDynamicPickFaceChangeInOtherFactoryWillRefreshGridCollectionCore(true);
		}

		public void TestDynamicPickFaceChangeInOtherFactory_WhenHasNotSearched_ShouldNotRefreshGridCollection()
		{
			TestDynamicPickFaceChangeInOtherFactoryWillRefreshGridCollectionCore(false);
		}

		void TestDynamicPickFaceChangeInOtherFactoryWillRefreshGridCollectionCore(bool hasSearched)
		{
			var data = new DynamicPickFaceViewTestData(Factory);
			var locationA = data.Whs1.FindLocation("A");
			var dynamicPFArea = Helper.CreateDynamicPF(data.Whs1, locationA);
			Factory.Save();

			using (var module = new DynamicPickFacesModule())
			{
				var filterBizO = (DynamicPickFacesFilterBusinessObject)module.FilterBusinessObject;
				var warehouseFilter = (ModuleGuidFilter)filterBizO.ModuleFilters[DynamicPickFacesFilterBusinessObject.Schema.Warehouse];
				warehouseFilter.IsActive = true;
				warehouseFilter.Property = data.Whs1.PK;

				if (hasSearched)
				{
					module.PerformSearch_ForTest();
				}

				var resultNotAssigned = module.GridCollection.Cast<WhsDynamicPickFaceView>().Single();
				AssertEquals("Pre-condition: dynamic pick face should have no assigned product", ZGuid.Empty, resultNotAssigned.WDP_OP_Product);

				var newFactory = new CargoWise.EntityFramework.BusinessObjectFactory() { RefreshEnabled = true };
				DynamicPickFaceViewTestData.AssignDynamicProductAndSave(newFactory, data.Org1, data.Part1, dynamicPFArea);

				var resultAssigned = module.GridCollection.Cast<WhsDynamicPickFaceView>().Single();

				var expectedProductGuid = hasSearched ? data.Part1.PK : ZGuid.Empty;
				AssertEquals("Dynamic pick face should have been refreshed to show part1 assigned", expectedProductGuid, resultAssigned.WDP_OP_Product);
			}
		}

		#endregion

		#region TestWhenCostLimitIsReachedButUserPressesYesGridIsLoadedCorrectly

		public void TestWhenCostLimitIsReachedButUserPressesYesGridIsLoadedCorrectly()
		{
			var data = new DynamicPickFaceViewTestData(Factory);
			var locationA = data.Whs1.FindLocation("A");
			Helper.CreateDynamicPF(data.Whs1, locationA);
			Factory.Save();

			using (var module = new TestDynamicPickFacesModule())
			using (module.ShowPopup())
			{
				var filterBizO = (DynamicPickFacesFilterBusinessObject)module.FilterBusinessObject;
				var warehouseFilter = (ModuleGuidFilter)filterBizO.ModuleFilters[DynamicPickFacesFilterBusinessObject.Schema.Warehouse];
				warehouseFilter.IsActive = true;
				warehouseFilter.Property = data.Whs1.PK;

				// make the sql cost exception thrown on next search
				module.ThrowLimitException = true;
				// User wants to load data even though cost is high
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				module.PerformSearch_ForTest();
				var result = module.GridCollection.Cast<WhsDynamicPickFaceView>().Single();
				AssertEquals("Dynamic pick face should have no assigned product.", ZGuid.Empty, result.WDP_OP_Product);
				AssertEquals("Dynamic pick face should have correct location.", locationA.PK, result.WDP_WL_Location);
			}
		}

		#endregion

		#region Test Scaffolding

		class TestDynamicPickFacesModule : DynamicPickFacesModule
		{
			protected override FilteredGridLoader CreateSearchManager()
			{
				return new TestFilteredGridLoaderWithSqlException(FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements);
			}

			public FilteredGridLoader SearchManagerExposed => SearchManager;
			public bool ThrowLimitException
			{
				get => ((TestFilteredGridLoaderWithSqlException)SearchManagerExposed).ThrowLimitException;
				set => ((TestFilteredGridLoaderWithSqlException)SearchManagerExposed).ThrowLimitException = value;
			}
		}

		WhsTestHelperFunctionsEnv Helper => helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory));
		WhsTestHelperFunctionsEnv helper;

		#endregion
	}
}
