using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	sealed class JobDeclarationFilterBusinessObjectTest_ForCoreFunctionality : JobDeclarationFilterBusinessObjectTest
	{
		public void TestBaseDefaultColourGridIsLoaded()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (var module = (JobDeclarationModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.JobDeclaration, "AU"))
			using (var form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				var auBranch = Factory.NewWithValidTestData<GlbBranch>();
				auBranch.GB_GC = GlbCompany.CurrentCompany.PK;
				auBranch.GB_Code = "ALT";
				auBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
				auBranch.GB_RL_NKHomePort = "AUSYD";
				var dec = Factory.New<BaseJobDeclaration>();
				dec.JE_GB = auBranch.PK;
				dec.JE_DeclarationReference = "X0000";
				dec.DontReAssignReferenceNoForUnitTest = true;
				Factory.Save();
				((IFilterModuleInternalsForTesting)module).PerformSearch();
				var mananger = new SchemeManagerForTest(module.DisplayGrid);
				var filterBusinessObject = new GridFilterStripBusinessObject(module.DisplayGrid);
				var auDecType = ObjectFactory.GetType<Integration.Customs.AU.IJobDeclaration>().FullName;
				var systemScheme = Factory.New<GridColourScheme>();
				systemScheme.S9_FilterName = "[duduk]";
				systemScheme.S9_ModuleID = typeof(BaseJobDeclaration).FullName + "|_CS";
				systemScheme.S9_IsSystem = true;
				systemScheme.S9_GC = ZGuid.Empty;
				var colorStrip = new GridColourStripBusinessObject(filterBusinessObject, systemScheme, null);
				systemScheme.ColourStrips.Add(colorStrip);
				var publishedScheme = Factory.New<GridColourScheme>();
				publishedScheme.S9_FilterName = "user scheme";
				publishedScheme.S9_ModuleID = auDecType + "|_CS";
				publishedScheme.S9_IsPublished = true;
				publishedScheme.S9_GC = GlbCompany.CurrentCompany.PK;
				publishedScheme.S9_RelatedEntityID = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbStaff")).PK;
				colorStrip = new GridColourStripBusinessObject(filterBusinessObject, publishedScheme, null);
				publishedScheme.ColourStrips.Add(colorStrip);
				var userScheme = Factory.New<GridColourScheme>();
				userScheme.S9_FilterName = "user scheme";
				userScheme.S9_ModuleID = auDecType + "|_CS";
				userScheme.S9_IsPublished = false;
				userScheme.S9_GC = GlbCompany.CurrentCompany.PK;
				userScheme.S9_RelatedEntityID = GlbStaff.CurrentUser.PK;
				colorStrip = new GridColourStripBusinessObject(filterBusinessObject, userScheme, null);
				userScheme.ColourStrips.Add(colorStrip);
				Factory.Save();
				mananger.GridColoursParentMenuItemForTest.PerformSelect();
				AssertEquals(3, mananger.GridColourManageMenuItemForTest.MenuItems.Count);
				AssertEquals(5, mananger.GridColourSelectMenuItemForTest.MenuItems.Count);
				AssertEquals("Standard*", mananger.GridColourSelectMenuItemForTest.MenuItems[0].Text);
				AssertEquals("-", mananger.GridColourSelectMenuItemForTest.MenuItems[1].Text);
				var userSchemeMenuItem1 = mananger.GridColourSelectMenuItemForTest.MenuItems[3];
				var userSchemeMenuItem2 = mananger.GridColourSelectMenuItemForTest.MenuItems[4];
				if (userSchemeMenuItem2.Text == "user scheme")
				{
					userSchemeMenuItem1 = mananger.GridColourSelectMenuItemForTest.MenuItems[4];
					userSchemeMenuItem2 = mananger.GridColourSelectMenuItemForTest.MenuItems[3];
				}

				AssertEquals("user scheme", userSchemeMenuItem1.Text);
				AssertEquals("user scheme*", userSchemeMenuItem2.Text);
				AssertEquals("[duduk]", mananger.GridColourSelectMenuItemForTest.MenuItems[2].Text);
			}
		}

		public void TestConsolidatedDeclarationFilterReady()
		{
			filterBO.ParentModuleID = ModuleIDs.Customs.ConsolidatedDeclaration;
			var entryStatusFilter = (EntryStatusFilter)filterBO[filterBO.EntryStatusText];
			AssertEquals("RFC is the filtered status for declarations ready for consolidation", ConsolidatedEntryStatusList.Codes.ReadyForConsolidation, entryStatusFilter.Property);
			AssertEquals("RFC is always effective", FilterVisibility.AlwaysAppliedAndHidden, entryStatusFilter.Visibility);
			AssertEquals("Importer is always visible for declarations ready for consolidation", FilterVisibility.AlwaysVisible, filterBO[DeclarationFilterConstants.OrgFilterTypes.ImporterSupplier].Visibility);
		}

		public void TestEntryStatusFilter_AnyAllEntriesOptions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var declaration1 = Factory.New<BaseJobDeclaration>();
				var entryHeader0 = declaration1.CustomsEntryHeaders.AddNew();
				entryHeader0.CH_EntryStatus = "6";

				var declaration2 = Factory.New<BaseJobDeclaration>();
				var entryHeader1 = declaration2.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_EntryStatus = "6";
				var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
				entryHeader2.CH_EntryStatus = "1";

				var declaration3 = Factory.New<BaseJobDeclaration>();
				var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
				entryHeader3.CH_EntryStatus = "6";
				var entryHeader4 = declaration3.CustomsEntryHeaders.AddNew();
				entryHeader4.CH_EntryStatus = "6";

				var declaration4 = Factory.New<BaseJobDeclaration>();
				var entryHeader5 = declaration4.CustomsEntryHeaders.AddNew();
				entryHeader5.CH_EntryStatus = ZString.Empty;
				var entryHeader6 = declaration4.CustomsEntryHeaders.AddNew();
				entryHeader6.CH_EntryStatus = ZString.Empty;

				var declaration5 = Factory.New<BaseJobDeclaration>();
				var entryHeader7 = declaration5.CustomsEntryHeaders.AddNew();
				entryHeader7.CH_EntryStatus = "6";

				var declaration6 = Factory.New<BaseJobDeclaration>();
				var entryHeader8 = declaration6.CustomsEntryHeaders.AddNew();
				entryHeader8.CH_EntryStatus = "1";

				Factory.Save();
				filterBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
				var filter = (EntryStatusFilter)filterBO[filterBO.EntryStatusText];

				filter.IsActive = true;
				filter.Property = "6";
				filter.FilterType = EntryStatusFilterTypeList.Codes.All;

				var filteredDecs = Factory.Load<BaseJobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { declaration1.PK, declaration3.PK, declaration5.PK }, filteredDecs.Select(x => x.PK));

				filter.Property = "6";
				filter.FilterType = EntryStatusFilterTypeList.Codes.Any;
				filteredDecs = Factory.Load<BaseJobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { declaration1.PK, declaration2.PK, declaration3.PK, declaration5.PK }, filteredDecs.Select(x => x.PK));

				filter.FilterType = EntryStatusFilterTypeList.Codes.Any;
				filter.Property = DeclarationFilterConstants.EntryStatus.NotSentForFilter;
				filteredDecs = Factory.Load<BaseJobDeclaration>(filterBO.Filter);
				AssertContainsExactElementsInAnyOrder(new[] { declaration4.PK }, filteredDecs.Select(x => x.PK));
			}
		}

		public void TestEntryReleaseDateFilter()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var releaseDate = new ZDateTime(2021, 08, 27, 00, 00, 00);
			var filterObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[DeclarationFilterConstants.DateFilterTypes.EntryReleaseDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_EntryReleaseDate = releaseDate;
			Factory.Save();

			filter.Property1 = releaseDate.AddDays(1);
			filter.Property2 = ZDateTime.Empty;
			AssertEquals("declaration should not match because its Release Date is outside of the filter boundaries.", false, declaration.MatchesFilter(filterObj.Filter));

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = releaseDate.AddDays(-1);
			AssertEquals("declaration should not match because its Release Date is outside of the filter boundaries.", false, declaration.MatchesFilter(filterObj.Filter));

			filter.Property1 = releaseDate.AddDays(-1);
			filter.Property2 = releaseDate.AddDays(1);
			AssertEquals("Declaration should match because its Release Date is within the filter boundaries.", true, declaration.MatchesFilter(filterObj.Filter));

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertEquals("declaration should match because its Release Date is not empty.", true, declaration.MatchesFilter(filterObj.Filter));
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals("declaration should not match because its Release Date is empty.", false, declaration.MatchesFilter(filterObj.Filter));

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_EntryReleaseDate = ZDateTime.Empty;
			Factory.Save();

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertEquals("Declaration should match because it has an Entry Header with Release Date Entered.", true, declaration.MatchesFilter(filterObj.Filter));
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals("Declaration should match because it has an Entry Header with no Release Date Entered.", true, declaration.MatchesFilter(filterObj.Filter));

			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			entryHeader2.CH_EntryReleaseDate = releaseDate.AddDays(-100);
			Factory.Save();

			filter.Property1 = releaseDate.AddDays(-101);
			filter.Property2 = releaseDate.AddDays(-10);
			AssertEquals("Declaration should match because the Release Date on the second entry header matches the filter.", true, declaration.MatchesFilter(filterObj.Filter));
		}

		public void TestEntryReleaseDateFilterProperties()
		{
			var filterObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[DeclarationFilterConstants.DateFilterTypes.EntryReleaseDate];

			AssertEquals(FilterCategories.Dates, filter.Category);
			AssertEquals("Entry Release Date", filter.Description);
		}

		public void TestEntryReleaseDateFilter_Availability()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var filterObj = new JobDeclarationFilterBusinessObject();
				var filter = (ModuleDateFilter)filterObj[DeclarationFilterConstants.DateFilterTypes.EntryReleaseDate];

				AssertNull("Filter for CH_EntryReleaseDate should not be added for US as CH_EntryReleaseDate should not be used in US Customs", filter);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var filterObj = new JobDeclarationFilterBusinessObject();
				var filter = (ModuleDateFilter)filterObj[DeclarationFilterConstants.DateFilterTypes.EntryReleaseDate];

				AssertNotNull("Filter added for non-US country", filter);
			}
		}

		public void TestJobDeclarationRelatedShipmentSecurityFilter_IgnoreOSMG()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var jobHeaderStandAlone = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderStandAlone.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeaderStandAlone.JH_OA_LocalChargesAddr = org.Addresses[0].PK;

			var baseJobDeclarationStandAlone = Factory.NewWithValidTestData<BaseJobDeclaration>();
			jobHeaderStandAlone.JH_ParentID = baseJobDeclarationStandAlone.PK;
			jobHeaderStandAlone.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var jobHeaderLinkedToShipment = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderLinkedToShipment.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;
			jobHeaderLinkedToShipment.JH_OA_LocalChargesAddr = org.Addresses[0].PK;

			var baseJobDeclarationLinkedToShipment = Factory.NewWithValidTestData<BaseJobDeclaration>();
			baseJobDeclarationLinkedToShipment.JE_JS = shipment.PK;
			jobHeaderLinkedToShipment.JH_ParentID = shipment.PK;
			jobHeaderLinkedToShipment.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();

			Env.Security.MaintainShipmentCRMSecurity.IgnoreOSMG.IsAllowed = false;

			var filterStripBizo = new JobDeclarationFilterBusinessObject();
			var filter = filterStripBizo[DeclarationFilterConstants.RelatedShipmentSecurity];
			AssertNotNull(filter);

			var jobDeclarations = new BaseJobDeclarationCollection(Factory);
			jobDeclarations.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder(new[] { baseJobDeclarationStandAlone }, jobDeclarations);

			Env.Security.MaintainShipmentCRMSecurity.IgnoreOSMG.IsAllowed = true;

			var filterStripBizoIsAllowed = new JobDeclarationFilterBusinessObject();
			var filterIsAllowed = filterStripBizoIsAllowed[DeclarationFilterConstants.RelatedShipmentSecurity];
			AssertNull(filterIsAllowed);

			var jobDeclarationsIsAllowed = new BaseJobDeclarationCollection(Factory);
			jobDeclarationsIsAllowed.Load(filterStripBizoIsAllowed.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { baseJobDeclarationStandAlone, baseJobDeclarationLinkedToShipment }, jobDeclarationsIsAllowed);
		}

		public void TestJobDeclarationRelatedShipmentSecurityFilter_ViewByStaffNotAssigned()
		{
			var jobHeaderStandAlone = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderStandAlone.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;

			var baseJobDeclarationStandAlone = Factory.NewWithValidTestData<BaseJobDeclaration>();
			jobHeaderStandAlone.JH_ParentID = baseJobDeclarationStandAlone.PK;
			jobHeaderStandAlone.JH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			var shipmentCurrentUser = Factory.NewWithValidTestData<ForwardingShipment>();
			var jobHeaderLinkedToShipmentCurrentUser = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderLinkedToShipmentCurrentUser.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;

			var baseJobDeclarationLinkedToShipmentCurrentUser = Factory.NewWithValidTestData<BaseJobDeclaration>();
			baseJobDeclarationLinkedToShipmentCurrentUser.JE_JS = shipmentCurrentUser.PK;
			jobHeaderLinkedToShipmentCurrentUser.JH_ParentID = shipmentCurrentUser.PK;
			jobHeaderLinkedToShipmentCurrentUser.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			var shipmentSalesRep = Factory.NewWithValidTestData<ForwardingShipment>();
			var jobHeaderLinkedToShipmentSalesRep = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeaderLinkedToShipmentSalesRep.JH_GS_NKRepSales = salesRep.GS_Code;

			var baseJobDeclarationLinkedToShipmentSalesRep = Factory.NewWithValidTestData<BaseJobDeclaration>();
			baseJobDeclarationLinkedToShipmentSalesRep.JE_JS = shipmentSalesRep.PK;
			jobHeaderLinkedToShipmentSalesRep.JH_ParentID = shipmentSalesRep.PK;
			jobHeaderLinkedToShipmentSalesRep.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			Factory.Save();

			Env.Security.MaintainShipmentCRMSecurity.ViewByStaffNotAssigned.IsAllowed = false;

			var filterStripBizo = new JobDeclarationFilterBusinessObject();
			var filter = filterStripBizo[DeclarationFilterConstants.RelatedShipmentSecurity];
			AssertNotNull(filter);

			var jobDeclarations = new BaseJobDeclarationCollection(Factory);
			jobDeclarations.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder(new[] { baseJobDeclarationStandAlone, baseJobDeclarationLinkedToShipmentCurrentUser }, jobDeclarations);

			Env.Security.MaintainShipmentCRMSecurity.ViewByStaffNotAssigned.IsAllowed = true;

			var filterStripBizoIsAllowed = new JobDeclarationFilterBusinessObject();
			var filterIsAllowed = filterStripBizoIsAllowed[DeclarationFilterConstants.RelatedShipmentSecurity];
			AssertNull(filterIsAllowed);

			var jobDeclarationsIsAllowed = new BaseJobDeclarationCollection(Factory);
			jobDeclarationsIsAllowed.Load(filterStripBizoIsAllowed.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { baseJobDeclarationStandAlone, baseJobDeclarationLinkedToShipmentCurrentUser, baseJobDeclarationLinkedToShipmentSalesRep }, jobDeclarationsIsAllowed);
		}

		public void TestJobDeclarationRelatedShipmentSecurityFilter_IgnoreTaskAssignment()
		{
			var baseJobDeclarationStandAlone = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var workflowStandAlone = baseJobDeclarationStandAlone.WorkflowItems.AddNew();
			workflowStandAlone.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var shipmentCurrentUser = Factory.NewWithValidTestData<ForwardingShipment>();
			var workflowShipmentCurrentUser = shipmentCurrentUser.WorkflowItems.AddNew();
			workflowShipmentCurrentUser.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			var baseJobDeclarationLinkedToShipmentCurrentUser = Factory.NewWithValidTestData<BaseJobDeclaration>();
			baseJobDeclarationLinkedToShipmentCurrentUser.JE_JS = shipmentCurrentUser.PK;

			var salesRep = Factory.NewWithValidTestData<GlbStaff>();
			var shipmentSalesRep = Factory.NewWithValidTestData<ForwardingShipment>();
			var workflowShipmentSalesRep = shipmentSalesRep.WorkflowItems.AddNew();
			workflowShipmentSalesRep.P9_GS_NKAssignedStaffMember = salesRep.GS_Code;

			var baseJobDeclarationLinkedToShipmentSalesRep = Factory.NewWithValidTestData<BaseJobDeclaration>();
			baseJobDeclarationLinkedToShipmentSalesRep.JE_JS = shipmentSalesRep.PK;
			Factory.Save();

			Env.Security.MaintainShipmentCRMSecurity.IgnoreTaskAssignment.IsAllowed = false;

			var filterStripBizo = new JobDeclarationFilterBusinessObject();
			var filter = filterStripBizo[DeclarationFilterConstants.RelatedShipmentSecurity];
			AssertNotNull(filter);

			var jobDeclarations = new BaseJobDeclarationCollection(Factory);
			jobDeclarations.Load(filter.Query);
			AssertContainsExactElementsInAnyOrder(new[] { baseJobDeclarationStandAlone, baseJobDeclarationLinkedToShipmentCurrentUser }, jobDeclarations);

			Env.Security.MaintainShipmentCRMSecurity.IgnoreTaskAssignment.IsAllowed = true;

			var filterStripBizoIsAllowed = new JobDeclarationFilterBusinessObject();
			var filterIsAllowed = filterStripBizoIsAllowed[DeclarationFilterConstants.RelatedShipmentSecurity];
			AssertNull(filterIsAllowed);

			var jobDeclarationsIsAllowed = new BaseJobDeclarationCollection(Factory);
			jobDeclarationsIsAllowed.Load(filterStripBizoIsAllowed.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { baseJobDeclarationStandAlone, baseJobDeclarationLinkedToShipmentCurrentUser, baseJobDeclarationLinkedToShipmentSalesRep }, jobDeclarationsIsAllowed);
		}

		public void TestMessageStatusFilter_ShouldExcludeComparisonOperatorsFromMessageStatusFilterIsTrue()
		{
			var filterBizObj = new JobDeclarationFilterBusinessObject();
			var messageStatusFilter = (ModuleTextFilter)filterBizObj[DeclarationFilterConstants.MessageStatusText];
			AssertEquals(ModuleTextFilter.ComparisonConstants.Exact, messageStatusFilter.ComparisonOperator_List.CodesAsString);
		}

		public void TestMessageStatusFilter_ShouldExcludeComparisonOperatorsFromMessageStatusFilterIsFalse()
		{
			var filterBizObj = new JobDeclarationFilterBusinessObjectForTest();
			var messageStatusFilter = (ModuleTextFilter)filterBizObj[DeclarationFilterConstants.MessageStatusText];
			AssertEquals(true, messageStatusFilter.ComparisonOperator_List.Count > 1);
		}

		sealed class SchemeManagerForTest : GridColourSchemeManager
		{
			public SchemeManagerForTest(ZGrid grid) : base(grid)
			{
			}

			public MenuItem GridColoursParentMenuItemForTest => GridColoursParentMenuItem;

			public MenuItem GridColourManageMenuItemForTest => GridColourManageMenuItem;

			public MenuItem GridColourSelectMenuItemForTest => GridColourSelectMenuItem;
		}

		sealed class JobDeclarationFilterBusinessObjectForTest : JobDeclarationFilterBusinessObject
		{
			public JobDeclarationFilterBusinessObjectForTest() : base()
			{
			}

			protected override bool ShouldExcludeComparisonOperatorsFromMessageStatusFilter => false;
		}
	}
}
