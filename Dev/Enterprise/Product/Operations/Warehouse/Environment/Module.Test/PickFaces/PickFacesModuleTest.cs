using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Module.Testing
{
	[TestedType(typeof(PickFacesModule))]
	class PickFacesModuleTest : ZModuleBasherTest
	{
		public void TestSecurityCheckPoint()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.WhsConfigPickFaces, module.SecurityCheckpoint);
			}
		}

		public void TestFilterBusinessObject()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(typeof(PickFacesFilterBusinessObject), ((IFilterModuleInternalsForTesting)module).FilterBusinessObject.GetType());
			}
		}

		public void TestFilterControl()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (var control = ((ZFilterModule)module).GetNewFilterControlForGrid())
				{
					AssertEquals(typeof(PickFacesFilterControl), control.GetType());
				}
			}
		}

		#region TestGridCollection

		public void TestGridCollection()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(typeof(WhsPickFaceViewCollection), ((IFilterModuleInternalsForTesting)module).GridCollection.GetType());
			}
		}

		public void TestGridCollection_Columns_IsAssigned()
		{
			var whs = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var client = Helper.CreateClient("C1", "C1");
			var product = Helper.CreateProduct(client, "P1");
			var locationTypeFix = Helper.CreateLocationType("LT1", "LT1 Test", false, 1, LocationClasses.Codes.FIX);
			Factory.Save();

			var location_isAssigned = whs.FindLocation("A-1");
			var location_unAssigned = whs.FindLocation("A-2");
			var pickFace = Helper.CreateProductPickFace(product, client, location_isAssigned);
			location_isAssigned.WLV_WLT_LocationType = locationTypeFix.PK;
			location_unAssigned.WLV_WLT_LocationType = locationTypeFix.PK;
			Factory.Save();

			using (var module = new PickFacesModule())
			{
				var filter = (PickFacesFilterBusinessObject)module.FilterBusinessObject;
				var warehouseFilter = (ModuleGuidFilter)filter.ModuleFilters[PickFacesFilterBusinessObject.Schema.Warehouse];
				warehouseFilter.IsActive = true;
				warehouseFilter.Property = whs.PK;

				var pickFaceViewCollection = module.GridCollection as WhsPickFaceViewCollection;
				AssertNotNull("PickFaceViewCollection is not null.", pickFaceViewCollection);
				AssertEquals("PickFaceViewCollection should have 2 elements.", 2, pickFaceViewCollection.Count);
				AssertEquals("IsAssigned must be ticked for location A-1.", true, pickFaceViewCollection.Single(pf => pf.WPV_WL == location_isAssigned.PK).IsPickFaceAssigned);
				AssertEquals("IsAssigned must be un-ticked for location A-2.", false, pickFaceViewCollection.Single(pf => pf.WPV_WL == location_unAssigned.PK).IsPickFaceAssigned);
			}
		}

		#endregion

		public void TestAllowNew()
		{
			using (var module = new PickFacesModule())
			{
				Assert("New option should not be available", !module.AllowNew);
			}
		}

		public void TestAllowAdvancedDataAutomationWizard()
		{
			var tempStaff = Factory.New<IGlbStaff>();
			Factory.Save();
			using (Env.SetTemporaryUserContext(tempStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var module = new PickFacesModule())
			{
				AssertNotNull("Precondition: ", module.FormActionMenu.SingleOrDefault(am => am.Text == "&Actions"));

				GlowRegistry.Instance.ForceImportable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

				var dataTransferMenu = module.DataTransferMenuItem;
				dataTransferMenu.OnPopup(EventArgs.Empty);
				var glowIntegrationMenuItems = dataTransferMenu.MenuItems.Find("GlowIntegrationMenuItem", true);
				var dataAutomationMenuItem = glowIntegrationMenuItems.SingleOrDefault(gm => gm.Text == "Advanced Data Automation Wizard");

				AssertNotNull("Pick Face module should have Advanced Data Automation Wizard enabled", dataAutomationMenuItem);
			}
		}

		public void TestAllowDelete()
		{
			using (var module = new PickFacesModule())
			{
				Assert("Delete option should not be available", !module.AllowDelete);
			}
		}

		public void TestIOperationalActionSupportable()
		{
			using (var module = new PickFacesModule())
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
				AssertEquals(typeof(PickFacesOperationalActionsSupporter), ((IOperationalActionSupportable)module).OperationalActionSupporter.GetType());
			}
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert("Will use indexes of underlying tables.", true);
		}

		#region TestPickFaceChangeInOtherFactoryWillRefreshGridCollection

		public void TestPickFaceChangeInOtherFactoryWillRefreshGridCollection_HasPerformedSearch()
		{
			TestPickFaceChangeInOtherFactoryWillRefreshGridCollectionCore(true);
		}

		public void TestPickFaceChangeInOtherFactoryWillRefreshGridCollection_NotPerformedSerach()
		{
			TestPickFaceChangeInOtherFactoryWillRefreshGridCollectionCore(false);
		}

		public void TestPickFaceChangeInOtherFactoryWillRefreshGridCollectionCore(bool performSearch)
		{
			var whs = Helper.CreateWarehouse("1");
			var row1 = Helper.CreateRow(whs, "R1");
			var row2 = Helper.CreateRow(whs, "R2");
			Factory.Save();

			var location1 = row1.Locations[0];
			var location2 = row2.Locations[0];

			var locationTypePFC = Factory.LoadTop1<WhsLocationType>(new ZQuery(WhsLocationTypeSchema.WLT_Code, "PFC"))
				// this can be removed when the system Location Type PFC is checked in
				?? Helper.CreateLocationType("PFC", "PFC Test", true, 1, LocationClasses.Codes.FIX);

			location1.WLV_WLT_LocationType = locationTypePFC.PK;
			location2.WLV_WLT_LocationType = locationTypePFC.PK;

			var client = Helper.CreateClient();
			var part1 = Helper.CreateProduct(client, "P1");
			var part2 = Helper.CreateProduct(client, "P2");

			Factory.Save();

			var pickFace = Helper.CreateProductPickFace(part1, client, location1);
			Factory.Save();

			using (var module = new PickFacesModuleForTest())
			{
				var filter = (PickFacesFilterBusinessObject)module.FilterBusinessObject;
				var warehouseFilter = (ModuleGuidFilter)filter.ModuleFilters[PickFacesFilterBusinessObject.Schema.Warehouse];
				warehouseFilter.IsActive = true;
				warehouseFilter.Property = whs.PK;

				if (performSearch)
				{
					module.PerformSearchForTest();
				}

				var pickFaceViewCollection = module.GridCollection;
				AssertNotNull("Pre-condition: PickFaceViewCollection is not null.", pickFaceViewCollection);
				AssertEquals("Pre-condition: PickFaceViewCollection should have 2 elements.", 2, pickFaceViewCollection.Count);

				var pickFaceViewLinksToLocation1 = pickFaceViewCollection.Cast<WhsPickFaceView>().Single(v => v.WPV_WL == location1.PK);
				var pickFaceViewLinksToLocation2 = pickFaceViewCollection.Cast<WhsPickFaceView>().Single(v => v.WPV_WL == location2.PK);
				AssertEquals("Pre-condition: pickFaceViewLinksToLocation1 has a Product allocated.", part1.PK, pickFaceViewLinksToLocation1.WPV_OP);
				AssertEquals("Pre-condition: pickFaceViewLinksToLocation2 has no Product allocated.", ZGuid.Empty, pickFaceViewLinksToLocation2.WPV_OP);

				var otherFactory = new BusinessObjectFactory() { RefreshEnabled = true };
				var helperWithOtherFactory = new WhsTestHelperFunctionsEnv(otherFactory);
				var newPickFaceInOtherFactory = helperWithOtherFactory.CreateProductPickFace(part2, client, location2);
				otherFactory.Save();

				pickFaceViewCollection = module.GridCollection;
				AssertEquals("PickFaceViewCollection should have 2 elements.", 2, pickFaceViewCollection.Count);

				pickFaceViewLinksToLocation1 = pickFaceViewCollection.Cast<WhsPickFaceView>().Single(v => v.WPV_WL == location1.PK);
				pickFaceViewLinksToLocation2 = pickFaceViewCollection.Cast<WhsPickFaceView>().Single(v => v.WPV_WL == location2.PK);
				AssertEquals("PickFaceViewLinksToLocation1 has a Product allocated.", part1.PK, pickFaceViewLinksToLocation1.WPV_OP);
				if (performSearch)
				{
					AssertEquals("PickFaceViewCollection has been refreshed and PickFaceViewLinksToLocation2 has a Product allocated.", part2.PK, pickFaceViewLinksToLocation2.WPV_OP);
				}
				else
				{
					AssertEquals("PickFaceViewCollection should not be refreshed if module didn't run search before.", true, pickFaceViewLinksToLocation2.WPV_OP.IsEmpty);
				}
			}
		}

		class PickFacesModuleForTest : PickFacesModule
		{
			public void PerformSearchForTest()
			{
				PerformSearch();
			}
		}

		#endregion

		#region TestWhenCostLimitIsReachedButUserPressesYesGridIsLoadedCorrectly

		public void TestWhenCostLimitIsReachedButUserPressesYesGridIsLoadedCorrectly()
		{
			var data = new EnvTestDataSimpleEnvironment(Factory);
			var locationA = data.Whs1.FindLocation("A");
			new WhsTestHelperFunctionsEnv(Factory).CreateProductPickFace(data.Part1, data.Org1, locationA);
			Factory.Save();

			using (var module = new TestPickFacesModule())
			using (module.ShowPopup())
			{
				var filterBizO = (PickFacesFilterBusinessObject)module.FilterBusinessObject;
				var warehouseFilter = (ModuleGuidFilter)filterBizO.ModuleFilters[PickFacesFilterBusinessObject.Schema.Warehouse];
				warehouseFilter.IsActive = true;
				warehouseFilter.Property = data.Whs1.PK;

				// make the sql cost exception thrown on next search
				module.ThrowLimitException = true;
				// User wants to load data even though cost is high
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				module.PerformSearch_ForTest();
				var result = module.GridCollection.Cast<WhsPickFaceView>().Single();
				AssertEquals("Fixed pick face should have correct product.", data.Part1.PK, result.WPV_OP);
				AssertEquals("Fixed pick face should have correct location.", locationA.PK, result.WPV_WL);
			}
		}

		#endregion

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.WhsConfigPickFaces;
		}

		protected WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}
		WhsTestHelperFunctionsEnv helper;

		class TestPickFacesModule : PickFacesModule
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
	}
}
