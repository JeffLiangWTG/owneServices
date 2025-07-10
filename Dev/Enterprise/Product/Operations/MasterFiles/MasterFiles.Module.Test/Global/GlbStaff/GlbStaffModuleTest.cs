using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.General;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbStaffModule))]
	sealed class GlbStaffModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbStaff;
		}

		public void TestBusinessContexts()
		{
			using (GlbStaffModule module = new GlbStaffModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("GlbStaff business context should be returned", BusinessContext.GlbStaff, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		[RequiresSTA]
		public void TestFilterBySecurityRight()
		{
			// At least one active staff should be Controller and/or NonOperational
			// Set all the other staff to be not active - so that below test runs correctly. 
			// Could not ClearTable as there are too many other dependencies on staff.
			DeactivateAllStaff();

			var factory2 = new BusinessObjectFactory();
			var controllerStaff = factory2.NewWithValidTestData<GlbStaff>();
			controllerStaff.GS_LoginName = "First Controller Staff";
			controllerStaff.GS_Code = "AAA";
			controllerStaff.GS_IsActive = true;
			controllerStaff.GS_IsController = true;
			factory2.Save();

			GlbBranch differentBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK));
			GlbDepartment differentDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, GlbDepartment.CurrentDepartment.PK));

			GlbStaff staff1 = NewStaff();
			AddSecurityRight(staff1, null, null, null, Env.Security.QuotationView.Code);

			GlbStaff staff2 = NewStaff();
			AddSecurityRight(staff2, null, differentBranch, differentDepartment, Env.Security.QuotationView.Code);

			GlbStaff staff3 = NewStaff();
			AddSecurityRight(staff3, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.QuotationView.Code);

			GlbStaff staff4 = NewStaff();
			AddSecurityRight(staff4, null, null, null, Env.Security.Forwarding.Code);
			AddSecurityRight(staff4, null, null, null, "RegASSFTPConnectionTimeout");

			GlbStaff staff5 = NewStaff();
			AddSecurityRight(staff5, null, null, null, Env.Security.Forwarding.Code);
			AddSecurityRight(staff5, null, null, null, Env.Security.MaintainShipment.Code, false);

			Factory.Save();

			using (GlbStaffModule module = (GlbStaffModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				IFilterGridModuleInternalsForTesting moduleInternals = module;
				GlbStaffFilterBusinessObject filterBusinessObject = (GlbStaffFilterBusinessObject)moduleInternals.FilterBusinessObject;
				filterBusinessObject.ResetToDefaultValues();

				moduleInternals.PerformSearch();
				AssertEquals(6, moduleInternals.GridCollection.Count);

				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = Env.Security.QuotationView.LookupKey;
				filterBusinessObject.SecurityRightsFilter.IsActive = true;
				moduleInternals.PerformSearch();
				AssertEquals(3, moduleInternals.GridCollection.Count);
				Assert(moduleInternals.GridCollection.FindByPK(staff1.PK) != null);
				Assert(moduleInternals.GridCollection.FindByPK(staff3.PK) != null);
				Assert(moduleInternals.GridCollection.FindByPK(controllerStaff.PK) != null);

				filterBusinessObject.SecurityRightsFilter.Branch = differentBranch.PK;
				filterBusinessObject.SecurityRightsFilter.Department = differentDepartment.PK;
				moduleInternals.PerformSearch();
				AssertEquals(3, moduleInternals.GridCollection.Count);
				Assert(moduleInternals.GridCollection.FindByPK(staff1.PK) != null);
				Assert(moduleInternals.GridCollection.FindByPK(staff2.PK) != null);
				Assert(moduleInternals.GridCollection.FindByPK(controllerStaff.PK) != null);

				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("MaintainShipmentWorkflow");
				moduleInternals.PerformSearch();
				AssertEquals(2, moduleInternals.GridCollection.Count);
				Assert(moduleInternals.GridCollection.FindByPK(staff4.PK) != null);
				Assert(moduleInternals.GridCollection.FindByPK(controllerStaff.PK) != null);
				Assert(moduleInternals.GridCollection.FindByPK(staff5.PK) == null);

				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("RegASSFTPConnectionTimeout");
				moduleInternals.PerformSearch();
				AssertEquals(2, moduleInternals.GridCollection.Count);
				Assert(moduleInternals.GridCollection.FindByPK(staff4.PK) != null);
				Assert(moduleInternals.GridCollection.FindByPK(controllerStaff.PK) != null);
			}
		}

		[RequiresSTA]
		public void TestFilterBySecurityRight_GlbStaffCollection()
		{
			DeactivateAllStaff();
			CreateNewFactoryAndController();

			var staff1 = NewStaff();
			AddSecurityRight(staff1, null, null, null, Env.Security.QuotationView.Code);

			var staff2 = NewStaff();
			AddSecurityRight(staff2, null, null, null, Env.Security.MaintainShipmentNew.Code);

			Factory.Save();

			using (var module = (GlbStaffModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				module.OverrideModuleDecisionProvider(new DummyDecisionProvider(module));

				var moduleInternals = (IFilterGridModuleInternalsForTesting)module;
				var filterBusinessObject = (GlbStaffFilterBusinessObject)moduleInternals.FilterBusinessObject;
				filterBusinessObject.ResetToDefaultValues();

				moduleInternals.PerformSearch();
				AssertEquals(3, moduleInternals.GridCollection.Count);

				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = Env.Security.QuotationView.LookupKey;
				filterBusinessObject.SecurityRightsFilter.IsActive = true;
				moduleInternals.PerformSearch();
				AssertEquals(2, moduleInternals.GridCollection.Count);
				AssertNotNull(moduleInternals.GridCollection.FindByPK(staff1.PK));
				AssertNull(moduleInternals.GridCollection.FindByPK(staff2.PK));

				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = Env.Security.MaintainShipmentNew.LookupKey;
				filterBusinessObject.SecurityRightsFilter.IsActive = true;
				moduleInternals.PerformSearch();
				AssertEquals(2, moduleInternals.GridCollection.Count);
				AssertNull(moduleInternals.GridCollection.FindByPK(staff1.PK));
				AssertNotNull(moduleInternals.GridCollection.FindByPK(staff2.PK));
			}
		}

		public class DummyDecisionProvider : ZFilterModule.DefaultModuleDecisionProvider
		{
			public DummyDecisionProvider(ZFilterModule module) : base(module) { }

			public override IBusinessObjectCollection List
			{
				get
				{
					return new GlbStaffCollection(new BusinessObjectFactory());
				}
			}
		}

		[RequiresSTA]
		public void TestFilterBySecurityRight_SupportOrCategory()
		{
			DeactivateAllStaff();
			CreateNewFactoryAndController();

			var staff1 = NewStaff();
			AddSecurityRight(staff1, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.Operations.Code);

			var staff2 = NewStaff();
			AddSecurityRight(staff2, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.Operations.Code);

			var staff3 = NewStaff();
			AddSecurityRight(staff3, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.Operations.Code);

			var staff4 = NewStaff();
			AddSecurityRight(staff4, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.MaintainShipmentNew.Code);

			var staff5 = NewStaff();
			AddSecurityRight(staff5, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.MaintainShipmentNew.Code);

			var staff6 = NewStaff();
			AddSecurityRight(staff6, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.MaintainShipmentNew.Code);

			var staff7 = NewStaff();
			AddSecurityRight(staff7, null, GlbBranch.CurrentBranch, GlbDepartment.CurrentDepartment, Env.Security.MaintainShipmentNew.Code);

			Factory.Save();

			using (GlbStaffModule module = (GlbStaffModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var moduleInternals = (IFilterGridModuleInternalsForTesting)module;
				var filterBusinessObject = (GlbStaffFilterBusinessObject)moduleInternals.FilterBusinessObject;
				filterBusinessObject.ResetToDefaultValues();

				moduleInternals.PerformSearch();
				AssertEquals(8, moduleInternals.GridCollection.Count);

				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = Env.Security.SailingScheduleEdit.LookupKey;
				filterBusinessObject.SecurityRightsFilter.IsActive = true;
				moduleInternals.PerformSearch();
				AssertEquals(4, moduleInternals.GridCollection.Count); //controllerStaff + 3

				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("MaintainShipmentNew");
				moduleInternals.PerformSearch();
				AssertEquals(8, moduleInternals.GridCollection.Count);

				var orFilter1 = filterBusinessObject.SecurityRightsFilter;
				orFilter1.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("MAINTAINSHIPMENTNEW");
				var orFilter2 = filterBusinessObject.AddFilterStrip<StaffSecurityModuleFilter>("Security Rights");
				orFilter2.IsActive = true;
				orFilter2.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("SAILINGSCHEDULEEDIT");

				orFilter1.OrCategory = FilterOrCategory.Red;
				orFilter2.OrCategory = FilterOrCategory.Red;

				moduleInternals.PerformSearch();
				AssertEquals(8, moduleInternals.GridCollection.Count);
			}
		}

		[RequiresSTA]
		public void TestMultipleSecurityRightsWork()
		{
			DeactivateAllStaff();
			CreateNewFactoryAndController();

			GlbStaff staff1 = NewStaffWithSecurityRights("ST1", Env.Security.QuotationView);
			GlbStaff staff2 = NewStaffWithSecurityRights("ST2", Env.Security.QuotationView);
			GlbStaff staff3 = NewStaffWithSecurityRights("ST3", Env.Security.TimeZoneSetView);
			GlbStaff staff4 = NewStaffWithSecurityRights("ST4", Env.Security.TimeZoneSetView);
			GlbStaff staff5 = NewStaffWithSecurityRights("ST5", Env.Security.MaintainShipment);
			GlbStaff staff6 = NewStaffWithSecurityRights("ST6", Env.Security.DataTransferPayablesTransaction, Env.Security.QuotationView);

			Factory.Save();

			using (GlbStaffModule module = (GlbStaffModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				IFilterGridModuleInternalsForTesting moduleInternals = module;
				GlbStaffFilterBusinessObject filterBusinessObject = (GlbStaffFilterBusinessObject)moduleInternals.FilterBusinessObject;
				filterBusinessObject.ResetToDefaultValues();

				moduleInternals.PerformSearch();
				AssertEquals(7, moduleInternals.GridCollection.Count);

				AssertEquals(2, SetLookupKey(filterBusinessObject.SecurityRightsFilter, Env.Security.MaintainShipment.LookupKey, moduleInternals, true));

				AssertEquals(4, SetLookupKey(filterBusinessObject.SecurityRightsFilter, Env.Security.QuotationView.LookupKey, moduleInternals, true));
				AssertEquals(3, SetLookupKey(filterBusinessObject.SecurityRightsFilter, Env.Security.TimeZoneSetView.LookupKey, moduleInternals, true));

				SetLookupKey(filterBusinessObject.SecurityRightsFilter, CheckpointLookupKey.Empty, moduleInternals, false);

				SetLookupKey(AddNewSecurityModuleFilter(filterBusinessObject, "secondFilter", true), Env.Security.DataTransferPayablesTransaction.LookupKey, moduleInternals, false);
				SetLookupKey(AddNewSecurityModuleFilter(filterBusinessObject, "thirdFilter", true), Env.Security.QuotationView.LookupKey, moduleInternals, false);

				moduleInternals.PerformSearch();
				AssertEquals(2, moduleInternals.GridCollection.Count);
			}
		}

		[RequiresSTA]
		public void TestFilterBySecurityRight_SysAdminAllowedCheckpoint()
		{
			DeactivateAllStaff();

			var factory2 = new BusinessObjectFactory();
			var controllerStaff = factory2.NewWithValidTestData<GlbStaff>();
			controllerStaff.GS_LoginName = "First Controller Staff";
			controllerStaff.GS_Code = "AAA";
			controllerStaff.GS_IsActive = true;
			controllerStaff.GS_IsController = true;
			factory2.Save();

			var staffWithActivityLogPrivileges = Factory.NewWithValidTestData<GlbStaff>();
			staffWithActivityLogPrivileges.GS_IsOperational = true;
			staffWithActivityLogPrivileges.GS_IsController = false;

			AddSecurityRight(staffWithActivityLogPrivileges, null, null, null, Env.Security.StaffActivityLog.Code);

			var staffWithoutPrivileges = Factory.NewWithValidTestData<GlbStaff>();
			staffWithoutPrivileges.GS_IsOperational = true;
			staffWithoutPrivileges.GS_IsController = false;

			Factory.Save();

			Assert("Test precondition", Env.CurrentUser.PK != staffWithActivityLogPrivileges.PK);
			Assert("Test precondition", Env.CurrentUser.PK != staffWithoutPrivileges.PK);

			using (var module = (GlbStaffModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var moduleInternals = (IFilterGridModuleInternalsForTesting)module;
				var filterBusinessObject = (GlbStaffFilterBusinessObject)moduleInternals.FilterBusinessObject;

				// no filter, should return both plus controller staff
				filterBusinessObject.ResetToDefaultValues();
				moduleInternals.PerformSearch();
				AssertEquals(3, moduleInternals.GridCollection.Count);
				Assert(moduleInternals.GridCollection.Contains(staffWithActivityLogPrivileges.PK));
				Assert(moduleInternals.GridCollection.Contains(staffWithoutPrivileges.PK));

				// filter by StaffActivityLog, should return only one plus controller staff
				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("StaffActivityLog");
				filterBusinessObject.SecurityRightsFilter.IsActive = true;
				moduleInternals.PerformSearch();
				AssertEquals(2, moduleInternals.GridCollection.Count);
				Assert(moduleInternals.GridCollection.FindByPK(staffWithActivityLogPrivileges.PK) != null);
			}
		}

		[StressTest]
		[RequiresSTA]
		public void TestFilterBySecurityRight_MinimizesDBHits()
		{
			DeactivateAllStaff();

			var factory2 = new BusinessObjectFactory();
			var controllerStaff = factory2.NewWithValidTestData<GlbStaff>();
			controllerStaff.GS_LoginName = "First Controller Staff";
			controllerStaff.GS_Code = "AAA";
			controllerStaff.GS_IsActive = true;
			controllerStaff.GS_IsController = true;
			factory2.Save();

			factory2.RefreshEnabled = false;
			Factory.RefreshEnabled = false;

			for (int i = 0; i < 100; ++i)
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_IsOperational = true;
				staff.GS_IsController = false;
				var group = Factory.NewWithValidTestData<GlbGroup>();
				Assert("Staff wasn't in group", !staff.Groups.Contains(group));
				staff.Groups.Add(group);
				Assert("Staff is now in group", staff.Groups.Contains(group));
				AddSecurityRight(group, null, null, Env.Security.QuotationView.Code);
			}

			Factory.Save();

			using (var module = (GlbStaffModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var moduleInternals = (IFilterGridModuleInternalsForTesting)module;
				var filterBusinessObject = (GlbStaffFilterBusinessObject)moduleInternals.FilterBusinessObject;

				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = Env.Security.QuotationView.LookupKey;
				filterBusinessObject.SecurityRightsFilter.IsActive = true;
				moduleInternals.PerformSearch();
				AssertEquals(101, moduleInternals.GridCollection.Count);

				AssertEquals(2, GlbStaffGroupHelper.securityFactory_ExposedForTest.GetTableHitCount("GlbGroup"));
				AssertEquals(1, GlbStaffGroupHelper.securityFactory_ExposedForTest.GetTableHitCount("GlbGroupLink"));
			}
		}

		[RequiresSTA]
		public void TestFilterBySecurityRight_WithOverMaxRowsReturned()
		{
			DeactivateAllStaff();

			var factory2 = new BusinessObjectFactory();
			var controllerStaff = factory2.NewWithValidTestData<GlbStaff>();
			controllerStaff.GS_LoginName = "First Controller Staff";
			controllerStaff.GS_Code = "AAA";
			controllerStaff.GS_IsActive = true;
			controllerStaff.GS_IsController = true;
			factory2.Save();

			SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);

			for (int i = 0; i < 50; ++i)
			{
				for (int j = 0; j < 3; ++j)
				{
					var staffWithoutPrivileges = Factory.NewWithValidTestData<GlbStaff>();
					staffWithoutPrivileges.GS_IsOperational = true;
					staffWithoutPrivileges.GS_IsController = false;
				}

				var staffWithActivityLogPrivileges = Factory.NewWithValidTestData<GlbStaff>();
				staffWithActivityLogPrivileges.GS_IsOperational = true;
				staffWithActivityLogPrivileges.GS_IsController = false;

				AddSecurityRight(staffWithActivityLogPrivileges, null, null, null, Env.Security.StaffActivityLog.Code);
			}

			Factory.Save();

			using (var module = (GlbStaffModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var moduleInternals = (IFilterGridModuleInternalsForTesting)module;
				var filterBusinessObject = (GlbStaffFilterBusinessObject)moduleInternals.FilterBusinessObject;

				// filter by StaffActivityLog, should return exactly 50 plus controller staff
				filterBusinessObject.SecurityRightsFilter.SecurityFilterContainer.LookupKey = new CheckpointLookupKey("StaffActivityLog");
				filterBusinessObject.SecurityRightsFilter.IsActive = true;
				moduleInternals.PerformSearch();
				AssertEquals(51, moduleInternals.GridCollection.Count);
			}
		}

		[RequiresSTA]
		public void TestGridCollectionFilterUpdatesAfterMaxRowsReturned()
		{
			DeactivateAllStaff();

			var factory2 = new BusinessObjectFactory();
			var controllerStaff = factory2.NewWithValidTestData<GlbStaff>();
			controllerStaff.GS_LoginName = "First Controller Staff";
			controllerStaff.GS_Code = "AAA";
			controllerStaff.GS_IsActive = true;
			controllerStaff.GS_IsController = true;
			factory2.Save();

			var staff1 = NewStaff();
			AddSecurityRight(staff1, null, null, null, Env.Security.QuotationView.Code);
			var staff2 = NewStaff();
			AddSecurityRight(staff2, null, null, null, Env.Security.QuotationView.Code);

			Factory.Save();

			SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			using var module = (GlbStaffModule)ZModuleFactory.Instance.Create(GetModuleID());
			var moduleInternals = (IFilterGridModuleInternalsForTesting)module;
			var filterBusinessObject = (GlbStaffFilterBusinessObject)moduleInternals.FilterBusinessObject;
			var initalRelationshipFilter = moduleInternals.GridCollection.RelationshipFilter;
			moduleInternals.PerformSearch();
			var relationshipFilterAfterInitialMaxRowsException = moduleInternals.GridCollection.RelationshipFilter;
			AssertNotEquals(initalRelationshipFilter, relationshipFilterAfterInitialMaxRowsException);
			AssertEquals(0, moduleInternals.GridCollection.Count);

			filterBusinessObject.AddTextFilterStrip("Login Name", "First Controller Staff");
			moduleInternals.PerformSearch();
			AssertEquals(1, moduleInternals.GridCollection.Count);
			AssertNotEquals(relationshipFilterAfterInitialMaxRowsException, moduleInternals.GridCollection.RelationshipFilter);

			filterBusinessObject.ResetModuleFilters();
			moduleInternals.PerformSearch();
			AssertEquals(relationshipFilterAfterInitialMaxRowsException, moduleInternals.GridCollection.RelationshipFilter);
			AssertEquals(0, moduleInternals.GridCollection.Count);
		}

		[RequiresSTA]
		public void TestGridCollectionIsUpdatedOnSuccessfulSearch()
		{
			DeactivateAllStaff();

			var factory2 = new BusinessObjectFactory();
			var controllerStaff = factory2.NewWithValidTestData<GlbStaff>();
			controllerStaff.GS_LoginName = "First Controller Staff";
			controllerStaff.GS_Code = "AAA";
			controllerStaff.GS_IsActive = true;
			controllerStaff.GS_IsController = true;
			factory2.Save();

			var staff1 = NewStaff();
			AddSecurityRight(staff1, null, null, null, Env.Security.QuotationView.Code);
			var staff2 = NewStaff();
			AddSecurityRight(staff2, null, null, null, Env.Security.QuotationView.Code);

			Factory.Save();

			SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);

			using var module = (GlbStaffModule)ZModuleFactory.Instance.Create(GetModuleID());
			var moduleInternals = (IFilterGridModuleInternalsForTesting)module;
			var filterBusinessObject = (GlbStaffFilterBusinessObject)moduleInternals.FilterBusinessObject;
			var initalRelationshipFilter = moduleInternals.GridCollection.RelationshipFilter;
			moduleInternals.PerformSearch();
			AssertNotEquals(initalRelationshipFilter, moduleInternals.GridCollection.RelationshipFilter);
			AssertEquals(3, moduleInternals.GridCollection.Count);
		}

		[RequiresSTA]
		public void TestFilterBySecurityRight_DisposesOfUserFactory()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()) as GlbStaffModule)
			{
				var moduleInternals = module as IFilterGridModuleInternalsForTesting;
				var filterBusinessObject = moduleInternals.FilterBusinessObject as GlbStaffFilterBusinessObject;

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
			ActiveDirectoryRegistry.IsIntegrationEnabled = adEnabled;
			using (var module = new GlbStaffModule())
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

		public void TestUpperCasingForStaffImport()
		{
			using (GlbStaffModule module = (GlbStaffModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var properties = module.ImportCollectionInfo.Properties;
				AssertEquals("Upper case properties should be added", 9, properties.Count(x => x.CharacterCasing == ZCharacterCasing.Upper));
			}
		}

		class GlbStaffModuleForTesting : GlbStaffModule
		{
			public MenuItem[] GetNewStandardMenuItems_Exposed()
			{
				return GetNewStandardMenuItems();
			}
		}

		public void TestGetActionMenu()
		{
			using (var module = new GlbStaffModuleForTesting())
			{
				MenuItem[] standardMenuItems = module.GetNewStandardMenuItems_Exposed();

				var newMenuItem = standardMenuItems.FindByText("New");

				AssertNotNull("There should be a 'Staff' menu item", newMenuItem.MenuItems.FindByText("New Staff"));
				AssertNotNull("There should be a 'Resource' menu item", newMenuItem.MenuItems.FindByText("New Resource"));
			}
		}

		public void TestMappingFieldsForStaffImport()
		{
			using (GlbStaffModule module = (GlbStaffModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var properties = module.ImportCollectionInfo.Properties.ToList();
				var expected = new[] {
					("GS_LoginName", "Login Name"),
					("GS_Code", "Code"),
					("GS_Title", "Job Title"),
					("GS_FullName", "Preferred Full Name"),
					("GS_Birthdate", "Date of Birth"),
					("GS_Gender", "Gender"),
					("GS_EmploymentDate", "Employment Date"),
					("GS_EmploymentBasis", "Employment Basis"),
					("GS_Pager", "Other References"),
					("GS_UserAddress1", "Address Line 1"),
					("GS_UserAddress2", "Address Line 2"),
					("GS_RN_NKCountryCode", "Country/Region"),
					("GS_City", "City"),
					("GS_State", "State"),
					("GS_Postcode", "Postcode"),
					("GS_WorkingLanguage", "Language"),
					("GS_GB_HomeBranch", "Staff Member's Home Branch"),
					("GS_GE_HomeDepartment", "Staff Member's Home Department"),
					("GS_WorkPhone", "Work Phone"),
					("GS_WorkExtension", "Work Extension"),
					("GS_FaxNum", "Fax Number"),
					("GS_HomePhone", "Home Phone"),
					("GS_EmailAddress", "Email Address"),
					("StaffPlainTextPasswordForImportOnly", "Password"),
					("GS_MobilePhone", "Mobile Phone"),
					("GS_ChangePasswordAtNextLogin", "Change Password at Next Login"),
					("GS_CanLogin", "Can Login"),
					("GS_NameTitle", "Title"),
					("GS_GivenName", "Given Name(s)"),
					("GS_MiddleName", "Middle Name(s)"),
					("GS_Surname", "Surname(s)"),
					("GS_FriendlyName", "Preferred Given Name"),
					("GS_PreferredSurname", "Preferred Surname"),
					("GS_FullNameInMotherLanguage", "Full Name in Mother Language"),
					("GS_NextOfKin", "Next of Kin Name"),
					("GS_NextOfKinHomePhone", "Next of Kin Home Phone"),
					("GS_NextOfKinEmail", "Next of Kin Email"),
					("GS_NextOfKinRelationship", "Next of Kin Relation"),
					("GS_EmergencyContactName", "Emergency Contact Name"),
					("GS_EmergencyHomePhone", "Emergency Home Phone"),
					("GS_EmergencyContactEmail", "Emergency Contact Email"),
					("GS_EmergencyContactRelationship", "Emergency Contact Relation"),
					("GS_RN_NKNationalityCode", "Nationality Code"),
					("GS_ResidencyStatus", "Residency Status"),
					("GS_ResidencyExpiry", "Residency Expiry"),
					("GS_DepartureDate", "Departure Date"),
					("GS_IsSalesRep", "Is Sales Rep"),
					("GS_SavePersonalDataToActiveDirectory", "Save Personal Data To Active Directory"),
				};

				var actual = properties.Select(p => (p.MappingName, p.HeaderText));
				AssertContainsExactElementsInAnyOrder("Mapping fields", expected, actual);
			}
		}

		public void TestSupportsWorkflow()
		{
			using (GlbStaffModule module = (GlbStaffModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Assert("Should support Worflow", module.SupportsWorkflow);
			}
		}

		#region Implementation

		GlbStaff NewStaff()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TS" + (++staffCounter).ToString();
			staff.GS_LoginName = "Test" + staffCounter.ToString();

			return staff;
		}

		int staffCounter;

		void AddSecurityRight(GlbStaff staff, GlbCompany company, GlbBranch branch, GlbDepartment department, string securityRight, bool isAllowed = true)
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

			security.GU_GS = staff.PK;
		}

		void AddSecurityRight(GlbGroup group, GlbBranch branch, GlbDepartment department, string securityRight, bool isAllowed = true)
		{
			GlbSecurity security = Factory.New<GlbSecurity>();
			security.GU_SecurityItemIsAllowed = isAllowed;
			security.GU_SecurityRight = securityRight;
			security.GU_GC = GlbCompany.CurrentCompany.PK;
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

		GlbStaff NewStaffWithSecurityRights(string code, params SecurityCheckpoint[] securityRightsToAllow)
		{
			GlbStaff staff = NewStaff();
			foreach (SecurityCheckpoint securityRightToAllow in securityRightsToAllow)
			{
				AddSecurityRight(staff, null, null, null, securityRightToAllow.Code);
				staff.GS_Code = code;
			}

			return staff;
		}

		IADRegistry ActiveDirectoryRegistry
		{
			get { return ObjectFactory.Get<IADRegistry>(); }
		}

		void DeactivateAllStaff()
		{
			ZQuery query = new ZQuery(GlbStaffSchema.GS_IsActive, ZBool.True);
			GlbStaff[] existingStaff = Factory.Load<GlbStaff>(query);
			foreach (GlbStaff staff in existingStaff)
			{
				staff.GS_IsActive = ZBool.False;
			}
		}

		void CreateNewFactoryAndController()
		{
			var factory2 = new BusinessObjectFactory();
			var controllerStaff = factory2.NewWithValidTestData<GlbStaff>();
			controllerStaff.GS_LoginName = "First Controller Staff";
			controllerStaff.GS_Code = "AAA";
			controllerStaff.GS_IsActive = true;
			controllerStaff.GS_IsController = true;
			factory2.Save();
		}

		int SetLookupKey(StaffSecurityModuleFilter filter, CheckpointLookupKey lookupKey, IFilterGridModuleInternalsForTesting moduleInternals, bool performSearch = false)
		{
			filter.SecurityFilterContainer.LookupKey = lookupKey;
			filter.IsActive = true;
			if (performSearch)
			{
				moduleInternals.PerformSearch();
				return moduleInternals.GridCollection.Count;
			}
			return 0;
		}

		StaffSecurityModuleFilter AddNewSecurityModuleFilter(GlbStaffFilterBusinessObject filter, string name, bool isActive)
		{
			var newFilter = filter.FilterStrips.AddNew(filter.SecurityRightsFilter.Description);
			newFilter.CurrentModuleFilter.IsActive = isActive;

			return (StaffSecurityModuleFilter)newFilter.CurrentModuleFilter;
		}
		#endregion
	}
}
