using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.General;
using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbGroupModule))]
	sealed class GlbGroupModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbGroup;
		}

		[RequiresSTA]
		public void TestLoadCollection_ShouldHideORGTypeGroups()
		{
			var group1 = NewGroup();
			group1.GG_Type = GlbGroupTypeList.Codes.Organisation;
			var group2 = NewGroup();
			group2.GG_Type = GlbGroupTypeList.Codes.Staff;
			var group3 = NewGroup();
			group3.GG_Type = GlbGroupTypeList.Codes.Staff;
			Factory.Save();

			using (var module = (GlbGroupModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var moduleInternals = (IFilterGridModuleInternalsForTesting)module;
				var filterBusinessObject = (GlbGroupFilterBusinessObject)moduleInternals.FilterBusinessObject;

				filterBusinessObject.ResetToDefaultValues();
				moduleInternals.PerformSearch();
				AssertEquals("Group1 is not in the result collection", false, moduleInternals.GridCollection.Contains(group1.PK));
				AssertEquals("Group2 is in the result collection", true, moduleInternals.GridCollection.Contains(group2.PK));
				AssertEquals("Group3 is in the result collection", true, moduleInternals.GridCollection.Contains(group3.PK));
			}
		}

		public void TestBusinessContexts()
		{
			using (GlbGroupModule module = new GlbGroupModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("GlbStaff business context should be returned", BusinessContext.GlbGroup, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		[RequiresSTA]
		public void TestFilterBySecurityRight()
		{
			DeactivateAllNonDefaultGroups();

			var differentBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK));
			var differentDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, GlbDepartment.CurrentDepartment.PK));

			var group1 = NewGroup();
			AddSecurityRight(group1, null, null, null, Env.Security.QuotationView.Code);

			var group2 = NewGroup();
			AddSecurityRight(group2, null, differentBranch, differentDepartment, Env.Security.QuotationView.Code);

			var group3 = NewGroup();
			AddSecurityRight(group3, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.QuotationView.Code);

			var group4 = NewGroup();
			AddSecurityRight(group4, null, null, null, Env.Security.Forwarding.Code);
			AddSecurityRight(group4, null, null, null, "RegASSFTPConnectionTimeout");

			var group5 = NewGroup();
			AddSecurityRight(group5, null, null, null, Env.Security.Forwarding.Code);
			AddSecurityRight(group5, null, null, null, Env.Security.MaintainShipment.Code, false);

			Factory.Save();

			using (var module = (GlbGroupModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var moduleInternals = (IFilterGridModuleInternalsForTesting)module;
				var filterBusinessObject = (GlbGroupFilterBusinessObject)moduleInternals.FilterBusinessObject;
				filterBusinessObject.ResetToDefaultValues();

				moduleInternals.PerformSearch();
				AssertEquals(7, moduleInternals.GridCollection.Count); //5 named groups + ALL + PMG

				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = Env.Security.QuotationView.LookupKey;
				filterBusinessObject.SecurityRightsFilter.IsActive = true;
				moduleInternals.PerformSearch();
				AssertEquals(2, moduleInternals.GridCollection.Count);
				Assert(moduleInternals.GridCollection.FindByPK(group1.PK) != null);
				Assert(moduleInternals.GridCollection.FindByPK(group3.PK) != null);

				filterBusinessObject.SecurityRightsFilter.Branch = differentBranch.PK;
				filterBusinessObject.SecurityRightsFilter.Department = differentDepartment.PK;
				moduleInternals.PerformSearch();
				AssertEquals(2, moduleInternals.GridCollection.Count);
				Assert(moduleInternals.GridCollection.FindByPK(group1.PK) != null);
				Assert(moduleInternals.GridCollection.FindByPK(group2.PK) != null);

				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("MaintainShipmentWorkflow");
				moduleInternals.PerformSearch();
				AssertEquals(1, moduleInternals.GridCollection.Count);
				Assert(moduleInternals.GridCollection.FindByPK(group4.PK) != null);

				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("RegASSFTPConnectionTimeout");
				moduleInternals.PerformSearch();
				AssertEquals(1, moduleInternals.GridCollection.Count);
				Assert(moduleInternals.GridCollection.FindByPK(group4.PK) != null);
			}
		}

		[RequiresSTA]
		public void TestFilterBySecurityRight_Login()
		{
			DeactivateAllNonDefaultGroups();

			var nonCurrentCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			var nonCurrentBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.Equal, nonCurrentCompany.PK));
			var groupAll = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, SQLComparisonOperator.Equal, "ALL"));
			var groupPmg = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, SQLComparisonOperator.Equal, "PMG"));

			var group1 = NewGroup();
			AddSecurityRight(group1, GlbCompany.CurrentCompany, null, null, Env.Security.Login.Code);

			var group2 = NewGroup();
			AddSecurityRight(group2, null, GlbBranch.CurrentBranch, null, Env.Security.Login.Code);

			var group3 = NewGroup();
			AddSecurityRight(group3, nonCurrentCompany, null, null, Env.Security.Login.Code);

			var group4 = NewGroup();
			AddSecurityRight(group4, null, nonCurrentBranch, null, Env.Security.Login.Code);

			Factory.Save();

			using (var module = (GlbGroupModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var moduleInternals = (IFilterGridModuleInternalsForTesting)module;
				var filterBusinessObject = (GlbGroupFilterBusinessObject)moduleInternals.FilterBusinessObject;
				filterBusinessObject.ResetToDefaultValues();

				moduleInternals.PerformSearch();
				AssertEquals(6, moduleInternals.GridCollection.Count); //4 named groups + ALL + PMG

				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = Env.Security.Login.LookupKey;
				filterBusinessObject.SecurityRightsFilter.IsActive = true;
				moduleInternals.PerformSearch();
				AssertEquals(4, moduleInternals.GridCollection.Count);
				AssertEquals(group1.PK, moduleInternals.GridCollection.FindByPK(group1.PK).PK);
				AssertEquals(group2.PK, moduleInternals.GridCollection.FindByPK(group2.PK).PK);
				AssertEquals(groupAll.PK, moduleInternals.GridCollection.FindByPK(groupAll.PK).PK);
				AssertEquals(groupPmg.PK, moduleInternals.GridCollection.FindByPK(groupPmg.PK).PK);

				filterBusinessObject.SecurityRightsFilter.Branch = GlbBranch.CurrentBranch.PK;
				moduleInternals.PerformSearch();
				AssertEquals(4, moduleInternals.GridCollection.Count);
				AssertEquals(group1.PK, moduleInternals.GridCollection.FindByPK(group1.PK).PK);
				AssertEquals(group2.PK, moduleInternals.GridCollection.FindByPK(group2.PK).PK);
				AssertEquals(groupAll.PK, moduleInternals.GridCollection.FindByPK(groupAll.PK).PK);
				AssertEquals(groupPmg.PK, moduleInternals.GridCollection.FindByPK(groupPmg.PK).PK);

				filterBusinessObject.SecurityRightsFilter.Branch = nonCurrentBranch.PK;
				moduleInternals.PerformSearch();
				AssertEquals(4, moduleInternals.GridCollection.Count);
				AssertEquals(group3.PK, moduleInternals.GridCollection.FindByPK(group3.PK).PK);
				AssertEquals(group4.PK, moduleInternals.GridCollection.FindByPK(group4.PK).PK);
				AssertEquals(groupAll.PK, moduleInternals.GridCollection.FindByPK(groupAll.PK).PK);
				AssertEquals(groupPmg.PK, moduleInternals.GridCollection.FindByPK(groupPmg.PK).PK);
			}
		}

		[RequiresSTA]
		public void TestFilterBySecurityRight_SupportOrCategory()
		{
			DeactivateAllNonDefaultGroups();

			var group1 = NewGroup();
			AddSecurityRight(group1, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.Operations.Code);

			var group2 = NewGroup();
			AddSecurityRight(group2, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.Operations.Code);

			var group3 = NewGroup();
			AddSecurityRight(group3, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.Operations.Code);

			var group4 = NewGroup();
			AddSecurityRight(group4, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.MaintainShipmentNew.Code);

			var group5 = NewGroup();
			AddSecurityRight(group5, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.MaintainShipmentNew.Code);

			var group6 = NewGroup();
			AddSecurityRight(group6, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.MaintainShipmentNew.Code);

			var group7 = NewGroup();
			AddSecurityRight(group7, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.MaintainShipmentNew.Code);

			Factory.Save();

			using (var module = (GlbGroupModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var moduleInternals = (IFilterGridModuleInternalsForTesting)module;
				var filterBusinessObject = (GlbGroupFilterBusinessObject)moduleInternals.FilterBusinessObject;
				filterBusinessObject.ResetToDefaultValues();

				moduleInternals.PerformSearch();
				AssertEquals(9, moduleInternals.GridCollection.Count); //7 named groups + ALL + PMG

				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = Env.Security.SailingScheduleEdit.LookupKey;
				filterBusinessObject.SecurityRightsFilter.IsActive = true;
				moduleInternals.PerformSearch();
				AssertEquals(3, moduleInternals.GridCollection.Count);

				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("MaintainShipmentNew");
				moduleInternals.PerformSearch();
				AssertEquals(7, moduleInternals.GridCollection.Count);

				var orFilter1 = filterBusinessObject.SecurityRightsFilter;
				orFilter1.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("MAINTAINSHIPMENTNEW");
				var orFilter2 = filterBusinessObject.AddFilterStrip<GroupSecurityModuleFilter>("Security Rights");
				orFilter2.IsActive = true;
				orFilter2.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("SAILINGSCHEDULEEDIT");

				orFilter1.OrCategory = FilterOrCategory.Red;
				orFilter2.OrCategory = FilterOrCategory.Red;

				moduleInternals.PerformSearch();
				AssertEquals(7, moduleInternals.GridCollection.Count);
			}
		}

		[RequiresSTA]
		public void TestFilterBySecurityRight_NoExceptionThrown()
		{
			using (var module = (GlbGroupModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				module.LimitedColumns = new ZLimitedColumnsProvider(typeof(GlbGroup));

				AssertNoExceptionThrown(() => (module as IFilterGridModuleInternalsForTesting).PerformSearch());
			}
		}

		[RequiresSTA]
		public void TestFilterBySecurityRight_SysAdminAllowedCheckpoint()
		{
			DeactivateAllNonDefaultGroups();

			var group1 = NewGroup();
			AddSecurityRight(group1, null, null, null, Env.Security.StaffActivityLog.Code);
			var group2 = NewGroup();

			Factory.Save();

			using (var module = (GlbGroupModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var moduleInternals = (IFilterGridModuleInternalsForTesting)module;
				var filterBusinessObject = (GlbGroupFilterBusinessObject)moduleInternals.FilterBusinessObject;

				// no filter, should return group11 + group2 + ALL + PMG
				filterBusinessObject.ResetToDefaultValues();
				moduleInternals.PerformSearch();
				AssertEquals(4, moduleInternals.GridCollection.Count);
				Assert(moduleInternals.GridCollection.Contains(group1.PK));

				// filter by StaffActivityLog, should return group1 only
				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("StaffActivityLog");
				filterBusinessObject.SecurityRightsFilter.IsActive = true;
				moduleInternals.PerformSearch();
				AssertEquals(1, moduleInternals.GridCollection.Count);
				Assert(moduleInternals.GridCollection.FindByPK(group1.PK) != null);
			}
		}

		[StressTest]
		[RequiresSTA]
		public void TestFilterBySecurityRight_MinimizesDBHits()
		{
			DeactivateAllNonDefaultGroups();

			for (int i = 0; i < 100; ++i)
			{
				var group = Factory.NewWithValidTestData<GlbGroup>();
				AddSecurityRight(group, null, null, null, Env.Security.QuotationView.Code);
			}

			Factory.Save();

			using (var module = (GlbGroupModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var moduleInternals = (IFilterGridModuleInternalsForTesting)module;
				var filterBusinessObject = (GlbGroupFilterBusinessObject)moduleInternals.FilterBusinessObject;

				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = Env.Security.QuotationView.LookupKey;
				filterBusinessObject.SecurityRightsFilter.IsActive = true;
				moduleInternals.PerformSearch();
				AssertEquals(100, moduleInternals.GridCollection.Count);

				AssertEquals(2, GlbStaffGroupHelper.securityFactory_ExposedForTest.GetTableHitCount("GlbGroup"));
				AssertEquals(1, GlbStaffGroupHelper.securityFactory_ExposedForTest.GetTableHitCount("GlbGroupLink"));
				AssertEquals(0, GlbStaffGroupHelper.securityFactory_ExposedForTest.GetTableHitCount("GlbStaff"));
			}
		}

		[StressTest]
		[RequiresSTA]
		public void TestFilterBySecurityRight_WithOverMaxRowsReturned()
		{
			DeactivateAllNonDefaultGroups();

			SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);

			for (int i = 0; i < 50; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					var group1 = Factory.NewWithValidTestData<GlbGroup>();
				}

				var group2 = Factory.NewWithValidTestData<GlbGroup>();

				AddSecurityRight(group2, null, null, null, Env.Security.StaffActivityLog.Code);
			}

			Factory.Save();

			using (var module = (GlbGroupModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var moduleInternals = (IFilterGridModuleInternalsForTesting)module;
				var filterBusinessObject = (GlbGroupFilterBusinessObject)moduleInternals.FilterBusinessObject;

				// filter by StaffActivityLog, should return exactly 50
				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("StaffActivityLog");
				filterBusinessObject.SecurityRightsFilter.IsActive = true;
				moduleInternals.PerformSearch();
				AssertEquals(50, moduleInternals.GridCollection.Count);
			}
		}

		[RequiresSTA]
		public void TestFilterBySecurityRight_DisposesOfUserFactory()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()) as GlbGroupModule)
			{
				var moduleInternals = module as IFilterGridModuleInternalsForTesting;
				var filterBusinessObject = moduleInternals.FilterBusinessObject as GlbGroupFilterBusinessObject;

				User.Factory = null;

				filterBusinessObject.ResetToDefaultValues();
				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("StaffActivityLog");
				filterBusinessObject.SecurityRightsFilter.IsActive = true;
				moduleInternals.PerformSearch();
				AssertNull("Should have set User.Factory to null after searching by Security Right.", User.Factory);
			}
		}

		[RequiresSTA]
		public void TestADMenuItem_ShouldAlwaysShow()
		{
			SetIntegrationModeAndAssertADMenuItemEnabled(false, true);
			SetIntegrationModeAndAssertADMenuItemEnabled(true, true);
		}

		void SetIntegrationModeAndAssertADMenuItemEnabled(bool adEnabled, bool shouldHaveADItem)
		{
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = adEnabled;
			using (var module = new GlbGroupModule())
			{
				AssertNotNull(module.EmbeddedControl); // to build the context menu
				if (shouldHaveADItem)
				{
					AssertNotNull(module.ActionsMenuItem.MenuItems.FindByText("Active Directory"));
				}
				else
				{
					AssertNull(module.ActionsMenuItem.MenuItems.FindByText("Active Directory"));
				}
			}
		}

		public override void TestExceptionsFilter()
		{
			Assert("Not available on Group module", true);
		}

		public override void TestMilestonesFilter()
		{
			Assert("Not available on Group module", true);
		}

		public override void TestAutoAddedMilestoneDateFilter()
		{
			Assert("Not available on Group module", true);
		}

		public override void TestAutoAddedTaskStatusFilter()
		{
			Assert("Not available on Group module", true);
		}

		public override void TestTasksFilter()
		{
			Assert("Not available on Group module", true);
		}

		public override void TestTriggersFilter()
		{
			Assert("Not available on Group module", true);
		}

		public void TestHasOperationalActionsPlugin()
		{
			using (var module = new GlbGroupModule())
			{
				AssertNotNull(module.Plugins.GetPlugin(ControllerIDs.OperationalActions));
			}
		}

		public void TestHasOperationalActions()
		{
			using (var module = new GlbGroupModule())
			{
				var supportable = module as IOperationalActionSupportable;
				AssertNotNull(supportable);
				AssertNotNull(supportable.OperationalActionSupporter);
			}
		}

		#region Implementation

		GlbGroup NewGroup()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "TS" + (++groupCounter).ToString();
			group.GG_Desc = "Test" + groupCounter.ToString();

			return group;
		}

		int groupCounter;

		void AddSecurityRight(GlbGroup group, GlbCompany company, GlbBranch branch, GlbDepartment department, string securityRight, bool isAllowed = true)
		{
			GlbSecurity security = Factory.New<GlbSecurity>();
			security.GU_SecurityItemIsAllowed = isAllowed;
			security.GU_SecurityRight = securityRight;
			if (company != null)
			{
				security.GU_GC = company.PK;
			}

			if (branch != null)
			{
				security.GU_GB = branch.PK;
			}

			if (department != null)
			{
				security.GU_GE = department.PK;
			}

			security.GU_GG = group.PK;
		}

		void DeactivateAllNonDefaultGroups()
		{
			ZQuery query = new ZQuery(GlbGroupSchema.GG_IsActive, ZBool.True);
			var existingGroup = Factory.Load<GlbGroup>(query);
			foreach (var group in existingGroup)
			{
				if (group.GG_Code != "ALL" && group.GG_Code != "PMG")
				{
					group.GG_IsActive = ZBool.False;
				}
			}
		}

		#endregion
	}
}
