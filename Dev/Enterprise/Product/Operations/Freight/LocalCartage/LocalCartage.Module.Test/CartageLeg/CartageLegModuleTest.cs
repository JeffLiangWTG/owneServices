using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageLegModule))]
	public class CartageLegModuleTest : ZModuleBasherTest
	{
		public void TestModuleFilterPerformance_ActiveBizo()
		{
			for (var c = 0; c < 5; c++)
			{
				var cartage = Factory.New<CommonCartage>();
				cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
				cartage.JJ_ConsignmentID = "T000" + c;
				for (var l = 0; l < 10; l++)
				{
					var move = cartage.LooseBookedMoves.AddNew();
					var leg = move.CartageLegs.AddNew();
				}
			}

			Factory.Save();
			using (var module = new CartageLegModule())
			{
				var iModule = (IFilterModuleInternalsForTesting)module;
				AssertEquals(0, iModule.GridCollection.Count);
				using (var form = module.ShowPopup())
				{
					var filters = (CartageLegFilterStripBusinessObject)iModule.FilterBusinessObject;
					var filter = ((ModuleTextFilter)filters["Port Transport #"]);
					filter.Property = "T";
					filter.IsActive = true;
					var factory_Initial = iModule.GridCollection.Factory;
					factory_Initial.ResetDatabaseLoadCount();
					iModule.PerformSearch();
					AssertEquals(50, iModule.GridCollection.Count);
					var factory_1stSearch = iModule.GridCollection.Factory;
					// GenCustomColumnDefinition: 1
					// Hits: 1/0
					AssertMaxDbHits(1, factory_Initial);
					// JobBookedCtgMove: 1
					// JobCartage: 1
					// JobContainerLegs: 1
					// Hits: 3/0
					AssertMaxDbHits(3, factory_1stSearch);
					factory_Initial.ResetDatabaseLoadCount();
					factory_1stSearch.ResetDatabaseLoadCount();
					filter.Property = "BLA";
					iModule.PerformSearch();
					AssertEquals(0, iModule.GridCollection.Count);
					var factory_2ndSearch = iModule.GridCollection.Factory;
					AssertMaxDbHits(0, factory_Initial);
					AssertMaxDbHits(0, factory_1stSearch);
					// JobContainerLegs: 1
					// Hits: 1/0
					AssertMaxDbHits("Should not be 54", 1, factory_2ndSearch);
				}
			}
		}

		public void TestStrategyProviderSetForInModule()
		{
			using (CartageLegModule module = new CartageLegModule())
			{
				Assert(CommonCartageBehaviorStrategyProvider.GetProvider(module.GridCollection.Factory).GetType().IsAssignableFrom(typeof(CartageBehaviorStrategyProvider)));
			}
		}

		public void TestRunSheetDashboardMenu()
		{
			using (CartageLegModule module = new CartageLegModule())
			{
				ZDateTime now = ZDateTime.Now;
				MenuItem openJobMenu = module.FormActionMenu.FindByText("Open Transport Job");
				AssertNotNull(openJobMenu);
			}
		}

		public void TestOpenTransportJob()
		{
			using (CartageLegModule module = new CartageLegModule())
			{
				Assert(CommonCartageBehaviorStrategyProvider.GetProvider(module.GridCollection.Factory).GetType().IsAssignableFrom(typeof(CartageBehaviorStrategyProvider)));
			}
		}

		public void TestCartageLegModuleID_WorkflowExceptionReport()
		{
			var branchPK = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 GB_PK FROM dbo.GlbBranch");
			var jobContainerPK = new JobContainer().InsertAndReturnObject(TestConnection).PK;
			var jobCartagePK = new JobCartage()
			{
				JJ_GB = branchPK,
				JJ_ConsignmentID = "T1"
			}.InsertAndReturnObject(TestConnection).PK;
			var jobBookedCtgMovePK = new JobBookedCtgMove()
			{
				EW_JJ = jobCartagePK,
				EW_JC_Container = jobContainerPK
			}.InsertAndReturnObject(TestConnection).PK;
			var jobContainerLegsPK = new JobContainerLegs()
			{
				JU_EW = jobBookedCtgMovePK
			}.InsertAndReturnObject(TestConnection).PK;
			var processTasksPK = new ProcessTasks(jobContainerLegsPK, "JJ")
			{
				P9_Type = "EXC",
				P9_ActualDate = new DateTime(2016, 1, 1)
			}.InsertAndReturnObject(TestConnection).PK;
			using (var module = new CartageLegModule())
			{
				var report = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT JobModuleID FROM Report_WorkflowExceptions((SELECT TOP 1 GC_PK FROM dbo.GlbCompany),'2016-01-01 00:00:00','2016-01-02 00:00:00')");
				AssertEquals("Result should have rows", 1, report.Rows.Count);
				AssertEquals("row['JobModuleID']", module.ID.Name, report.Rows[0]["JobModuleID"]);
			}
		}

#if !WINZOR
		public void TestWhenOverGuiResourceThresholdOnNumberOfWindowHandlesDoesNotThrow()
		{
			const int numberOfCartageJobs = 50;
			const string expectedMaxWindowsWarningMessage = "There are too many windows and/or graphical elements open by the application. Please close some unused windows and repeat this operation again.";
			var cartageLegPKs = new ZGuid[numberOfCartageJobs];
			for (var recordNo = 0; recordNo < numberOfCartageJobs; recordNo++)
			{
				// Create job cartage
				var cartage = Factory.New<CommonCartage>();
				cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_AirExport;
				cartage.JJ_ConsignmentID = "T000" + recordNo.ToString();
				// Create leg on job cartage
				var move = cartage.LooseBookedMoves.AddNew();
				var leg = move.CartageLegs.AddNew();
				cartageLegPKs[recordNo] = leg.PK;
			}

			Factory.Save();
			using (var module = new CartageLegModule())
			{
				var iModule = (IFilterModuleInternalsForTesting)module;
				AssertEquals(0, iModule.GridCollection.Count);
				using (var form = module.ShowPopup())
				{
					var filters = (CartageLegFilterStripBusinessObject)iModule.FilterBusinessObject;
					var filter = ((ModuleTextFilter)filters["Port Transport #"]);
					filter.Property = "T";
					filter.IsActive = true;
					var factory_Initial = iModule.GridCollection.Factory;
					factory_Initial.ResetDatabaseLoadCount();
					iModule.PerformSearch();
					AssertEquals("Precondition: created records have loaded", numberOfCartageJobs, iModule.GridCollection.Count);
					var instancesOfTooManyWindowsWarning = 0;
					for (var recordNo = 0; recordNo < numberOfCartageJobs; recordNo++)
					{
						module.DisplayGrid.SelectSingleElementByPK(cartageLegPKs[recordNo]);
						var menuItem = module.DisplayGrid.ContextMenu.MenuItems.FindByText("Open Transport Job");
						AssertNotNull("Precondition: should have created new additional menu item 'Open Transport Job'", menuItem);
						AssertNoExceptionThrown("Should not throw exception when form is not created because too many forms are open already", () => menuItem.PerformClick());
						instancesOfTooManyWindowsWarning = UnitTestUserNotification.Instance.PreviousMessages.Count(msg => msg.Text == expectedMaxWindowsWarningMessage);
						if (instancesOfTooManyWindowsWarning >= 2)
						{
							break;
						}
					}

					AssertEquals("Should have encountered the 'too many windows open' message twice at this point", 2, instancesOfTooManyWindowsWarning);
				}
			}
		}
#endif

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CartageLeg;
		}

		protected override void TearDown()
		{
			CloseAllOpenCartageForms();
			base.TearDown();
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var cartage = factory.NewWithValidTestData<CommonCartage>();
			var move = factory.NewWithValidTestData<CommonBookedCtgMove>();
			var leg = factory.NewWithValidTestData<CommonCartageLeg>();
			move.EW_JJ = cartage.PK;
			leg.JU_EW = move.PK;
			return leg;
		}

		void CloseAllOpenCartageForms()
		{
			foreach (var form in Application.OpenForms.OfType<CartageForm>().ToArray())
			{
				form.Close();
			}
		}
	}
}
