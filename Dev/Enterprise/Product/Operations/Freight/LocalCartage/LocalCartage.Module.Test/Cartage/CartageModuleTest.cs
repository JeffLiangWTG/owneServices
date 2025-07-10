using System;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageModule))]
	public class CartageModuleTest : ZModuleBasherTest
	{
		public void TestModuleFilterPerformance_ActiveBizo()
		{
			for (var c = 0; c < 50; c++)
			{
				var cartage = Factory.New<CommonCartage>();
				cartage.JJ_E3_NKJobType = Constants.CartageJobType.NEW_FCLCFStoCTO;
				cartage.JJ_ConsignmentID = "T000" + c;
				var container = cartage.ContainerBookedMoves.AddNew().Container;
				container.JC_ContainerNum = "C00" + c;
			}

			Factory.Save();
			using (var module = new CartageModuleForTest())
			{
				var iModule = (IFilterModuleInternalsForTesting)module;
				AssertEquals(0, iModule.GridCollection.Count);
				using (var form = module.ShowPopup())
				{
					var filters = (CartageFilterBusinessObject)iModule.FilterBusinessObject;
					var filter = ((ModuleTextFilter)filters["Container #"]);
					filter.Property = "C";
					filter.IsActive = true;
					var factory_Initial = iModule.GridCollection.Factory;
					factory_Initial.ResetDatabaseLoadCount();
					iModule.PerformSearch();
					AssertEquals(50, iModule.GridCollection.Count);
					var factory_1stSearch = iModule.GridCollection.Factory;
					// GenCustomColumnDefinition: 1
					// Hits: 1/0
					AssertMaxDbHits(1, factory_Initial);
					// JobCartage: 1
					// JobContainer: 1
					// Hits: 2/0
					AssertMaxDbHits(2, factory_1stSearch);
					factory_Initial.ResetDatabaseLoadCount();
					factory_1stSearch.ResetDatabaseLoadCount();
					filter.Property = "BLA";
					iModule.PerformSearch();
					AssertEquals(0, iModule.GridCollection.Count);
					var factory_2ndSearch = iModule.GridCollection.Factory;
					AssertMaxDbHits(0, factory_Initial);
					AssertMaxDbHits(0, factory_1stSearch);
					// JobCartage: 1
					// Hits: 1/0
					AssertMaxDbHits("Should not be 53", 1, factory_2ndSearch);
				}
			}
		}

		public void TestCartageModuleID_WorkflowExceptionReport()
		{
			var branchPK = (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 GB_PK FROM dbo.GlbBranch");
			var jobCartagePK = new JobCartage()
			{ JJ_ConsignmentID = "CARTAGE1", JJ_GB = branchPK }.InsertAndReturnObject(TestConnection).PK;
			var processTasksPK = new CargoWise.Database.TestFramework.ObjectModel.ProcessTasks(jobCartagePK, "JJ")
			{ P9_Type = "EXC", P9_ActualDate = new DateTime(2016, 1, 1) }.InsertAndReturnObject(TestConnection).PK;
			using (var module = new CartageModuleForTest())
			{
				var report = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT JobModuleID FROM Report_WorkflowExceptions((SELECT TOP 1 GC_PK FROM dbo.GlbCompany),'2016-01-01 00:00:00','2016-01-02 00:00:00')");
				AssertEquals("Result should have rows", 1, report.Rows.Count);
				AssertEquals("row['JobModuleID']", module.ID.Name, report.Rows[0]["JobModuleID"]);
			}
		}

		public void TestOperationalActionPlugin()
		{
			using (CartageModule module = (CartageModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		public void TestOperationalActionSupporter()
		{
			using (CartageModule module = (CartageModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(typeof(CartageOperationalActionSupporter), ((IOperationalActionSupportable)module).OperationalActionSupporter.GetType());
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Cartage;
		}
	}

	public class CartageModuleForTest : CartageModule, IBulkPostingModuleInternalsForTesting
	{
		JobInvoicingPostingOption IBulkPostingModuleInternalsForTesting.LastUsedPostingOptionForTest => fLastUsedPostingOptionForTest;

		IMenuItem IBulkPostingModuleInternalsForTesting.PostMenuItem
		{
			get
			{
				var menuItem = GetNewActionMenuItems().FindByText("&Post");
				var iMenuItem = menuItem as IMenuItem;
				if (menuItem != null && iMenuItem == null)
				{
					throw new InvalidOperationException("Post menu item should have type ZMenuItem.");
				}
				return iMenuItem;
			}
		}

		bool IBulkPostingModuleInternalsForTesting.DontDoActualPosting
		{
			get;
			set;
		}
	}
}
