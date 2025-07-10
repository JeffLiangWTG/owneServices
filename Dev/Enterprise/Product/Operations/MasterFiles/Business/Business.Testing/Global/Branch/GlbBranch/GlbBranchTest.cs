using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.ServiceManager;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbBranch))]
	sealed class GlbBranchTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadCompanyWillReportError_GlbBranchCompany_CannotLoadCompany_PersistedBranchIsNotInDatabase()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory();

			var newCompany = factory1.NewWithValidTestData<GlbCompany>();
			newCompany.GC_IsActive = true;
			var newBranch = factory1.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = newCompany.PK;
			newBranch.GB_IsActive = true;
			factory1.Save();

			var branchInFactory2 = factory2.Load<GlbBranch>(newBranch.PK); //to cache newBranch in Factory2
			newBranch.Delete();
			newCompany.Delete();
			factory1.Save();

			AssertNull("newCompany should not be cached in Factory2", factory2.Load<GlbCompany>(newCompany.PK));
			AssertNotNull("newBranch should be cached in Factory2", factory2.Load<GlbBranch>(newBranch.PK));

			AssertNull(branchInFactory2.Company);
			AssertEquals("GlbBranchCompany_CannotLoadCompany_PersistedBranchIsNotInDatabase", ErrorReporter.LastKeyReported);
			AssertContains(@$"Cannot load Company for PK '{newCompany.PK}' for branch '{newBranch.PK}'.
	PK = {newBranch.PK}
	Type = GlbBranch
	Types around row = GlbBranch
	Factory Instance = {factory2._Instance}
	IsDeleted = False
	IsInDb = True
	IsSavedByFactory = False
	HasChanges = False
	HasErrors = False
	IsDeleting = False

Business Contexts = None


Properties:", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestLoadCompanyWillReportError_GlbBranchCompany_CannotLoadCompany_PersistedBranch()
		{
			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_IsActive = true;
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = newCompany.PK;
			newBranch.GB_IsActive = true;
			Factory.Save();

			newBranch.GB_GC = ZGuid.NewZGuid();

			AssertNull(newBranch.Company);
			AssertEquals("GlbBranchCompany_CannotLoadCompany_PersistedBranch", ErrorReporter.LastKeyReported);
			AssertContains(@$"Branch in Database:
	PK = {newBranch.PK}
	Type = GlbBranch
	Types around row = GlbBranch
", ErrorReporter.LastMessageReported);
			AssertContains(@$"Factory information: Is this factory an Uber Factory itself: False
Does key exist if loaded with this factory: False
Does key exist with a reload: False
Row Factory Information:
Table name: GlbCompany
Is the table a Cached Table: True
Is the data row in the UberFactory: False, ({newBranch.GB_GC})
Is the filter cached in the UberFactory: True", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestLoadCompanyWillReportError_GlbBranchCompany_CannotLoadCompany_NewBranch()
		{
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_GC = ZGuid.NewZGuid();
			newBranch.GB_IsActive = true;

			AssertNull(newBranch.Company);
			AssertEquals("GlbBranchCompany_CannotLoadCompany_NewBranch", ErrorReporter.LastKeyReported);
			AssertContains(@$"Cannot load Company for PK '{newBranch.GB_GC}' for branch '{newBranch.PK}'.
	PK = {newBranch.PK}
	Type = GlbBranch
	Types around row = GlbBranch
	Factory Instance = {Factory._Instance}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = None


Properties:", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestLoadCompanyWillNotReportError()
		{
			var factory = new Mock<BusinessObjectFactory>();
			//fails on load by pk
			factory.Setup(f => f.Load(ObjectFactory.GetType("IGlbCompany"), It.IsAny<ZGuid>())).Returns(() => null);
			//succeeds on load by query
			factory.Setup(f => f.Load(ObjectFactory.GetType("IGlbCompany"), It.IsAny<ZQuery>())).Returns((Type bizO, ZQuery sqlQ) => Factory.Load(bizO, sqlQ));

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var branch = factory2.NewWithValidTestData(typeof(GlbBranch));
			var newBranch = new GlbBranch(factory.Object, ((INeedRow)branch).Row);
			newBranch.GB_GC = newCompany.PK;

			AssertNoExceptionThrown(() => _ = newBranch.Company);
		}

		public void TestGetOneActiveBranchPerCompany()
		{
			var allCompanies = Factory.Load<GlbCompany>(new ZQuery());
			allCompanies.ForEach(c => c.GC_IsActive = false); // Hack to filter out existing data
			Factory.Save();

			var noBranchCompany = Factory.New<GlbCompany>();
			noBranchCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			noBranchCompany.GC_Code = "NBC";

			var inactiveCompany = Factory.New<GlbCompany>();
			inactiveCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			inactiveCompany.GC_Code = "INA";
			inactiveCompany.GC_IsActive = false;
			var inactiveCompanyBranch = inactiveCompany.Branches.AddNew();
			inactiveCompanyBranch.FillWithValidTestData();

			var youngs = Factory.New<GlbCompany>();
			youngs.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			youngs.GC_Code = "YNG";

			var youngsBranchInactive = youngs.Branches.AddNew();
			youngsBranchInactive.FillWithValidTestData();
			youngsBranchInactive.GB_IsActive = false;

			var youngsBranch1 = youngs.Branches.AddNew();
			youngsBranch1.FillWithValidTestData();

			var youngsBranch2 = youngs.Branches.AddNew();
			youngsBranch2.FillWithValidTestData();

			var charlesWells = Factory.New<GlbCompany>();
			charlesWells.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			charlesWells.GC_Code = "CHW";
			var charlesWellsBranch = charlesWells.Branches.AddNew();
			charlesWellsBranch.FillWithValidTestData();

			var carltonAndUnitedFosters = Factory.New<GlbCompany>();
			carltonAndUnitedFosters.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			carltonAndUnitedFosters.GC_Code = "CUF";
			var carltonAndUnitedFostersBranch = carltonAndUnitedFosters.Branches.AddNew();
			carltonAndUnitedFostersBranch.FillWithValidTestData();

			AssertContainsExactElementsInAnyOrder("Should show all active branches per company in memory.", new[] { youngsBranch1, charlesWellsBranch, carltonAndUnitedFostersBranch }, GlbBranch.GetOneActiveBranchPerCompany(factory: Factory));
			AssertContainsExactElementsInAnyOrder("Should show all active branches per company in memory for the UK.", new[] { youngsBranch1, charlesWellsBranch }, GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.UnitedKingdom, Factory));

			AssertEquals("Should use a different factory.", 0, GlbBranch.GetOneActiveBranchPerCompany().Length);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Should show all active branches per company.", new[] { youngsBranch1.PK, charlesWellsBranch.PK, carltonAndUnitedFostersBranch.PK }, GlbBranch.GetOneActiveBranchPerCompany().Select(c => c.PK));
		}

		public void TestGetFirstActiveBranch()
		{
			var allCompanies = Factory.Load<GlbCompany>(new ZQuery());
			allCompanies.ForEach(c => c.GC_IsActive = false); // Hack to filter out existing data
			Factory.Save();

			var noBranchCompany = Factory.New<GlbCompany>();
			noBranchCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			noBranchCompany.GC_Code = "NBC";

			var inactiveCompany = Factory.New<GlbCompany>();
			inactiveCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			inactiveCompany.GC_Code = "INA";
			inactiveCompany.GC_IsActive = false;
			var inactiveCompanyBranch = inactiveCompany.Branches.AddNew();
			inactiveCompanyBranch.FillWithValidTestData();

			var youngs = Factory.New<GlbCompany>();
			youngs.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedKingdom;
			youngs.GC_Code = "YNG";

			var youngsBranchInactive = youngs.Branches.AddNew();
			youngsBranchInactive.FillWithValidTestData();
			youngsBranchInactive.GB_IsActive = false;

			var youngsBranch1 = youngs.Branches.AddNew();
			youngsBranch1.FillWithValidTestData();

			var youngsBranch2 = youngs.Branches.AddNew();
			youngsBranch2.FillWithValidTestData();

			var carltonAndUnitedFosters = Factory.New<GlbCompany>();
			carltonAndUnitedFosters.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Australia;
			carltonAndUnitedFosters.GC_Code = "CUF";
			var carltonAndUnitedFostersBranch = carltonAndUnitedFosters.Branches.AddNew();
			carltonAndUnitedFostersBranch.FillWithValidTestData();

			AssertEquals("Should get the first active branch.", true, new[] { youngsBranch1, youngsBranch2, carltonAndUnitedFostersBranch }.Contains(GlbBranch.GetFirstActiveBranch(factory: Factory)));
			AssertEquals("Should get the first active branch in Australia.", carltonAndUnitedFostersBranch, GlbBranch.GetFirstActiveBranch(Core.Constants.CountryCodes.Australia, factory: Factory));
			AssertEquals("Should get the first active branch in the UK.", youngsBranch1, GlbBranch.GetFirstActiveBranch(Core.Constants.CountryCodes.UnitedKingdom, factory: Factory));

			AssertNull("Should use a different factory.", GlbBranch.GetFirstActiveBranch());
			Factory.Save();
			AssertEquals("Should get the first active branch.", true, new[] { youngsBranch1.PK, youngsBranch2.PK, carltonAndUnitedFostersBranch.PK }.Contains(GlbBranch.GetFirstActiveBranch().PK));
		}

		public void TestDeleteWhenReferencedByStaff()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			var staffa = Factory.NewWithValidTestData<GlbStaff>();
			staffa.GS_GB_HomeBranch = branch.PK;
			var staffb = Factory.NewWithValidTestData<GlbStaff>();
			staffb.GS_GB_LastLogonBranch = branch.PK;
			Factory.Save();
			branch.Delete();
			Factory.Save();
			Assert(branch.IsDeleted);
			AssertEquals(ZGuid.Empty, staffa.GS_GB_HomeBranch);
			AssertEquals(ZGuid.Empty, staffb.GS_GB_LastLogonBranch);
		}

		public void TestDeleteReferencingStaff_SameHomeBranchAndLastLogonBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			var staffa = Factory.NewWithValidTestData<GlbStaff>();
			staffa.GS_GB_HomeBranch = branch.PK;
			staffa.GS_GB_LastLogonBranch = branch.PK;
			Factory.Save();

			branch.Delete();

			CombineAssertions(() =>
			{
				Assert("Branch should have been deleted", branch.IsDeleted);
				AssertEquals("staffc home branch should be set to empty", ZGuid.Empty, staffa.GS_GB_LastLogonBranch);
				AssertEquals("staffc last logon branch should also be set to empty", ZGuid.Empty, staffa.GS_GB_LastLogonBranch);
			});
		}

		public void TestBusinessObjectsWithRelatedEventsCore()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			company.AccCFXConfigurations.SetUplifts(JobInvoicingConsumerTypes.Shipment.Code, FreightShipmentDirection.Code.All, TransportModes.All, CurrencyCodes.Afghanistan, 5m, 0.1m);
			branch.AccCFXConfigurations.SetUplifts(JobInvoicingConsumerTypes.Shipment.Code, FreightShipmentDirection.Code.Import, TransportModes.Air, CurrencyCodes.Afghanistan, 5m, 0.1m);

			var companyLevelCFX = branch.AccCFXConfigurations.Cast<AccCFXUpliftConfiguration>().FirstOrDefault(x => x.Level == AccCFXConfigurationLevelEnum.Company);
			AssertNotNull("Company level CFX uplift", companyLevelCFX);
			var branchLevelCFX = branch.AccCFXConfigurations.Cast<AccCFXUpliftConfiguration>().FirstOrDefault(x => x.Level == AccCFXConfigurationLevelEnum.Branch);
			AssertNotNull("Branch level CFX uplift", branchLevelCFX);

			var bizObjsWithRelatedEvents = branch.BusinessObjectsWithRelatedEvents;

			AssertCollectionNotContains("BusinessObjectsWithRelatedEvents must not contain company level CFX.", companyLevelCFX, bizObjsWithRelatedEvents);
			AssertCollectionContains("BusinessObjectsWithRelatedEvents must contain branch level CFX.", branchLevelCFX, bizObjsWithRelatedEvents);
		}

		public void TestEditableChild()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			AssertEquals("Should be ChildEditable", true, branch.IsRegisteredEditableChildObject(branch.AccTaxConfigurations));
		}

		public void TestHomePortWithoutSecurity()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			branch.GB_RL_NKHomePort = "ADALV";
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			Env.Security.BranchModify.IsAllowed = false;
			branch.GB_RL_NKHomePort = "AUSYD";
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
			Env.Security.BranchModify.IsAllowed = true;
			branch.GB_RL_NKHomePort = "ADALV";
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}

		public void TestFillWithValidTestData()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			AssertEquals("Test Branch", branch.GB_BranchName);
		}

		public void TestSetCountry() => CombineAssertions(() =>
		{
			const string fakeCountryCode = "=)";
			const string fakeUnlocoCode = "('-')";
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = fakeUnlocoCode;
			unloco.RL_RN_NKCountryCode = fakeCountryCode;
			Factory.Save();

			Branch.SetCountry(fakeCountryCode);
			AssertEquals("When RefUNLOCO is active", fakeUnlocoCode, Branch.GB_RL_NKHomePort);

			unloco.RL_IsActive = false;
			Factory.Save();
			Branch.GB_RL_NKHomePort = ZString.Empty;
			Branch.SetCountry(fakeCountryCode);
			AssertEquals("When RefUNLOCO is NOT active", expected: ZString.Empty, Branch.GB_RL_NKHomePort);
		});

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestRelatedScheduleTasks()
		{
			// Arrange
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var branch = Factory.NewWithValidTestData<GlbBranch>();

				// Act
				var relatedActive = (IStmScheduleTask)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmScheduleTask>());
				relatedActive.S5_GB = branch.PK;

				var relatedInactive = (IStmScheduleTask)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmScheduleTask>());
				relatedInactive.S5_GB = branch.PK;
				relatedInactive.S5_IsActive = false;

				var notRelated = Factory.NewWithValidTestData(ObjectFactory.GetType<IStmScheduleTask>());

				// Assert
				AssertContainsExactElementsInAnyOrder(new[] { relatedActive, relatedInactive }, branch.RelatedScheduleTasks);
				AssertContainsExactElementsInAnyOrder(new[] { relatedActive }, branch.RelatedActiveScheduleTasks);
			}
		}

		public void TestRelatedStmServiceTasks()
		{
			// Arrange
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.Description == "some description" &&
				a.Category == "some category" &&
				a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
				a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes"));

			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
				.Returns(hostedServiceMock);

			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
			{
				// Act
				var relatedActiveStmServiceTask = (IStmScheduleTask)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmServiceTaskBranchValidationAdapter>());
				relatedActiveStmServiceTask.S5_GB = branch.PK;

				var relatedInactiveStmServiceTask = (IStmScheduleTask)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmServiceTaskBranchValidationAdapter>());
				relatedInactiveStmServiceTask.S5_GB = branch.PK;
				relatedInactiveStmServiceTask.S5_IsActive = false;

				var relatedScheduleServiceTaskShouldBeIgnored = (IStmScheduleTask)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmScheduleTask>());
				relatedScheduleServiceTaskShouldBeIgnored.S5_GB = branch.PK;
				relatedScheduleServiceTaskShouldBeIgnored.S5_ParentTableCode = ServiceTask.ParentTableCode;

				var relatedScheduleTask = (IStmScheduleTask)Factory.NewWithValidTestData(ObjectFactory.GetType<IStmScheduleTask>());
				relatedScheduleTask.S5_GB = branch.PK;
				relatedScheduleTask.S5_ParentTableCode = "ZZ";

				var notRelated = Factory.NewWithValidTestData(ObjectFactory.GetType<IStmServiceTaskBranchValidationAdapter>());

				// Assert
				AssertContainsExactElementsInAnyOrder(new[] { relatedActiveStmServiceTask, relatedInactiveStmServiceTask, relatedScheduleTask },
					branch.RelatedScheduleTasks);
				AssertContainsExactElementsInAnyOrder(new[] { relatedActiveStmServiceTask, relatedScheduleTask }, branch.RelatedActiveScheduleTasks);
			}
		}

		public void TestAccCFXUpliftConfigurationsNotNullAndCorrectLevel()
		{
			var glbBranch = Factory.NewWithValidTestData<GlbBranch>();

			AssertNotNull(glbBranch.AccCFXConfigurations);
			AssertEquals(AccCFXConfigurationLevelEnum.Branch, glbBranch.AccCFXConfigurations.Level);
		}

		#region Current branch

		public void TestCurrentBranch()
		{
			AssertEquals("CurrentBranch", Env.CurrentBranch.PK, GlbBranch.CurrentBranch.PK);
		}

		public void TestGetCurrentBranch_InSameFactory()
		{
			AssertEquals("CurrentBranch", Env.CurrentBranch, GlbBranch.GetCurrentBranch(GlbBranch.CurrentBranch.Factory));
		}

		public void TestGetCurrentBranch_InDifferentFactory()
		{
			var newFactory = Factory.CreateNewFactory();
			var currentBranch = GlbBranch.GetCurrentBranch(newFactory);

			AssertNotEquals("CurrentBranch", Env.CurrentBranch, currentBranch);
			AssertEquals("CurrentBranch", Env.CurrentBranch.PK, currentBranch.PK);
		}

		public void TestGetCurrentBranch_WhenCurrentBranchIsNull()
		{
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNull(GlbBranch.GetCurrentBranch(Factory));
			}
		}

		public void TestOrganisation()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertNull(GlbBranch.CurrentBranch.OrgProxy);
			OrgHeader org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = org.PK;
			AssertEquals(org.PK, GlbBranch.CurrentBranch.OrgProxy.PK);
		}

		public void TestCurrentBranchLoadsNewRecords()
		{
			using (RowFactory.SetCachedTables())
			{
				var allBranches = new BusinessObjectFactory().Load<GlbBranch>(new ZQuery());
				Assert("Precondition", allBranches.Length > 0);

				var companyPK = ZGuid.NewZGuid();
				var branchPK = ZGuid.NewZGuid();
				using (var command = TestConnection.Command(
					"insert into dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) values('" + companyPK.ToString() + "', 'NCO', 'New company', 'AU', 'AUD')" +
					"insert into dbo.GlbBranch(GB_PK, GB_Code, GB_BranchName, GB_GC) values('" + branchPK.ToString() + "', 'NBR', 'New branch', '" + companyPK.ToString() + "')"))
				{
					command.ExecuteNonQuery();
				}

				using (DisposableEnvironment.ForBranch(branchPK.ToGuid()))
				{
					var filter = new ZQuery(GlbBranchSchema.PK, branchPK);

					AssertEquals("New factory loads new record", 1, new BusinessObjectFactory().Load<GlbBranch>(filter).Length);

					AssertNotNull("CurrentBranch should find and load new branch", GlbBranch.CurrentBranch);
					AssertEquals(branchPK, GlbBranch.CurrentBranch.PK);
					AssertNotNull("Should find and load new company", GlbBranch.CurrentBranch.Company);
					AssertEquals(companyPK, GlbBranch.CurrentBranch.Company.PK);
				}
			}
		}

		#endregion

		#region Test Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(typeof(GlbBranch));
		}

		public override BusinessObject GetNewBusinessObjectSafeSaving()
		{
			var factory = new BusinessObjectFactory();
			var address = factory.NewWithValidTestData<GlbBranch>();
			address.Address1 = "A1";
			address.Address2 = "A2";
			address.Postcode = "1234";
			address.City = "Syd";
			address.State = "NSW";
			address.GB_RN_NKCountryCode = "CN";
			address.GB_RN_NKCountryCode = "AU";
			factory.Save();

			return address;
		}

		#endregion

		#region Related Business Objects

		public void TestPhoneNumbers()
		{
			Branch.GB_RN_NKCountryCode = "AU";
			Branch.GB_Phone_Formatted = "0426 829 924";
			Branch.GB_Fax_Formatted = "+86-156-0113-1981";

			AssertEquals("+61 426 829 924", Branch.GB_Phone_Wrapper.FormattedForBinding);
			AssertEquals("+86 156 0113 1981", Branch.GB_Fax_Wrapper.FormattedForBinding);

			AssertEquals("0426 829 924", Branch.GB_Phone_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);
			AssertEquals(string.Empty, Branch.GB_Fax_Wrapper.FormattedLocalNumberIfLoggedInSameCountryForBinding);

			Branch.GB_Fax_IsManuallyVerified = true;
			AssertEquals(1, GenCustomAddOnRuleAckCount);
			Branch.Delete();
			AssertEquals(0, GenCustomAddOnRuleAckCount);
		}

		public void TestMaster()
		{
			GlbBranch branch = (GlbBranch)GetNewBusinessObject();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			AssertEquals(GlbCompany.CurrentCompany.PK, branch.Company.PK);

			branch.GB_GC = ZGuid.Empty;
			AssertNull(branch.Company);
		}

		public void TestHolidays()
		{
			AssertEquals("Newly added Holidays count", 0, Branch.GlbHolidays.Count);
		}

		public void TestAllowedDepartments()
		{
			AssertEquals("Newly added Allowed Departments count", 0, Branch.AllowedDepartments.Count);
		}

		#endregion

		#region Overrides

		#region Onload

		public void TestCompanyReadOnlyForExisting()
		{
			AssertEquals("GB_GC should be readonly.", true, GlbBranch.CurrentBranch.GB_GCInfo.ReadOnly);
		}

		public void TestCompanyEditableForNew()
		{
			GlbBranch newBranch = Factory.New<GlbBranch>();
			AssertEquals("GB_GC should NOT be readonly.", false, newBranch.GB_GCInfo.ReadOnly);
		}

		#endregion

		#region Delete

		public void TestDelete()
		{
			GlbBranch.CurrentBranch.Delete();
			AssertEquals("Current Branch could not be deleted.", false, GlbBranch.CurrentBranch.IsDeleted);

			GlbBranch branch = Factory.LoadTop1<GlbBranch>(new ZQuery());

			branch.GlbHolidays.AddNew();
			AssertEquals("A GlbHoliday should have been added", 1, branch.GlbHolidays.Count);

			branch.GlbHolidays.DeleteAll();
			AssertEquals("GlbHolidays should have been deleted", 0, branch.GlbHolidays.Count);

			branch.AllowedDepartments.AddNew();
			AssertEquals("An Allowed Department should have been added", 1, branch.AllowedDepartments.Count);

			branch.AllowedDepartments.DeleteAll();
			AssertEquals("All Allowed Departments should have been deleted", 0, branch.AllowedDepartments.Count);

			branch.ExtraPorts.AddNew();
			AssertEquals("An Extra Port should have been added", 1, branch.ExtraPorts.Count);

			branch.ExtraPorts.RemoveAndDeleteAll();
			AssertEquals("Extra Ports should have been deleted", 0, branch.ExtraPorts.Count);

			branch.DefaultPorts.AddNew();
			AssertEquals("A Default Port should have been added", 1, branch.DefaultPorts.Count);

			branch.DefaultPorts.RemoveAndDeleteAll();
			AssertEquals("Default Ports should have been deleted", 0, branch.DefaultPorts.Count);
		}

		public void TestDelete_DeletesAssociatedTaxConfigs() 
		{
			// -> Arrange
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var taxConfiguration1 = AccountingTestObjectCreator.CreateTaxConfiguration(branch);
			var taxConfiguration2 = AccountingTestObjectCreator.CreateTaxConfiguration(branch);
			var taxConfiguration3 = AccountingTestObjectCreator.CreateTaxConfiguration(branch);

			// -> Act
			branch.Delete();

			// -> Assert
			Assert("Is first TaxConfiguration flagged to be deleted?", taxConfiguration1.IsDeleted);
			Assert("Is second TaxConfiguration flagged to be deleted?", taxConfiguration2.IsDeleted);
			Assert("Is third TaxConfiguration flagged to be deleted?", taxConfiguration3.IsDeleted);
		}

		#endregion

		#region BranchCredentials

		public void TestCertificateCredentialsTaxCore()
		{
			Branch.Factory.Save();

			AssertNotNull(nameof(Branch.CertificateCredentialsTaxCore), Branch.CertificateCredentialsTaxCore);

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), Branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("Branch.CertificateCredentialsTaxCore.IsLoaded is expected to be true prior to test", true, Branch.CertificateCredentialsTaxCore.IsLoaded);
				AssertEquals("Branch.CertificateCredentialsTaxCore.Count is expected to be 0 prior to test", 0, Branch.CertificateCredentialsTaxCore.Count);

				var branchCredential = Branch.CertificateCredentialsTaxCore.AddNew();
				AssertEquals(nameof(branchCredential.GP_GC), Branch.GB_GC, branchCredential.GP_GC);

				AssertEquals("Branch.CertificateCredentialsTaxCore.Count is expected to be 1 after an EInvoicingCertificate has been added for the branch", 1, Branch.CertificateCredentialsTaxCore.Count);
				Branch.Factory.Save();

				var newFactory = Factory.CreateNewFactory();
				var loadedBranch = newFactory.Load<GlbBranch>(Branch.PK);
				AssertEquals("Branch.CertificateCredentialsTaxCore.IsLoaded is expected to be true after reloading a branch with credentials already added", true, loadedBranch.CertificateCredentialsTaxCore.IsLoaded);
				AssertEquals("Branch.CertificateCredentialsTaxCore.Count is expected to be 1 after a reloading a branch with credentials already added", 1, loadedBranch.CertificateCredentialsTaxCore.Count);
			}
		}

		public void TestBranchCredentialsIndia()
		{
			Branch.Factory.Save();

			using (Branch.Company.TemporarilySetCountry(CountryCodes.Australia))
			{
				AssertNull("BranchCredentialsIndia should be null when not in India branch", Branch.BranchCredentialsIndia);
			}

			using (Branch.Company.TemporarilySetCountry(CountryCodes.India))
			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), Branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertNotNull("BranchCredentialsIndia should be lazy loaded / created when in India branch", Branch.BranchCredentialsIndia);

				AssertEquals("UserCredentialPasswordStatus should be empty when created", ZString.Empty, Branch.BranchCredentialsIndia.UserCredentialPasswordStatus);
				AssertEquals("ClientCredentialPasswordStatus should be empty when created", ZString.Empty, Branch.BranchCredentialsIndia.ClientCredentialPasswordStatus);

				Branch.BranchCredentialsIndia.Username = "user";
				Branch.BranchCredentialsIndia.Password = "pass";
				Branch.BranchCredentialsIndia.PasswordConfirmation = "pass";

				Branch.BranchCredentialsIndia.ClientId = "id";
				Branch.BranchCredentialsIndia.ClientSecret = "secret";

				AssertEquals("UserCredentialPasswordStatus should be empty before save", ZString.Empty, Branch.BranchCredentialsIndia.UserCredentialPasswordStatus);
				AssertEquals("ClientCredentialPasswordStatus should be empty before save", ZString.Empty, Branch.BranchCredentialsIndia.ClientCredentialPasswordStatus);

				Branch.Factory.Save();

				var newFactory = Factory.CreateNewFactory();
				var loadedBranch = newFactory.Load<GlbBranch>(Branch.PK);
				AssertEquals("UserCredentialPasswordStatus should be 'Saved' after save", "Saved", Branch.BranchCredentialsIndia.UserCredentialPasswordStatus);
				AssertEquals("ClientCredentialPasswordStatus should be 'Saved' after save", "Saved", Branch.BranchCredentialsIndia.ClientCredentialPasswordStatus);
			}
		}

		public void TestEInvoicingCredentials()
		{
			Branch.Factory.Save();

			AssertNotNull(nameof(Branch.EInvoicingCertificateCredentials), Branch.EInvoicingCertificateCredentials);
			AssertNotNull(nameof(Branch.EInvoicingPasswordCredentials), Branch.EInvoicingPasswordCredentials);

			AssertEquals("EInvoicingCertificateCredentials is will not load unless configured", false, Branch.EInvoicingCertificateCredentials.IsLoaded);
			AssertEquals("EInvoicingCertificateCredentials should be empty for a new branch", 0, Branch.EInvoicingCertificateCredentials.Count);

			AssertEquals("EInvoicingPasswordCredentials is will not load unless configured", false, Branch.EInvoicingPasswordCredentials.IsLoaded);
			AssertEquals("EInvoicingPasswordCredentials should be empty for a new branch", 0, Branch.EInvoicingPasswordCredentials.Count);
		}

		#endregion

		#region TestDeleteLastActiveBranch

		public void TestDeleteLastActiveBranch()
		{
			using (RowFactory.SetCachedTables())
			{
				var newFactory = new BusinessObjectFactory(); // Using Factory will make this unit test fail because of the Branch that is created/added in the SetUp() method

				GlbBranch.CurrentBranch.Delete();
				AssertEquals("Current Branch could not be deleted.", false, GlbBranch.CurrentBranch.IsDeleted);

				GlbBranch[] branches = newFactory.Load<GlbBranch>(new ZQuery());
				branches.ToList().ForEach(branch => branch.GB_IsActive = false);

				GlbCompany company1 = newFactory.New<GlbCompany>();

				GlbBranch branch1 = company1.Branches.AddNew();
				branch1.GB_IsActive = true;
				branch1.GB_Code = "UT1";
				branch1.GB_BranchName = "Test1";

				GlbBranch branch2 = company1.Branches.AddNew();
				branch2.GB_IsActive = true;
				branch2.GB_Code = "UT2";
				branch2.GB_BranchName = "Test2";

				newFactory.Save();

				AssertEquals("No Error expected", true, branch1.CanDelete);
				branch1.Delete();

				AssertEquals("Error expected - Last active branch cannot be deleted", false, branch2.CanDelete);
			}
		}

		public void TestLastActiveBranch()
		{
			using (RowFactory.SetCachedTables())
			{
				Branch.Delete();
				foreach (var branch in Factory.Load<GlbBranch>(new ZQuery()))
				{
					branch.GB_IsActive = false;
				}
				Factory.Save();

				var factory1 = new BusinessObjectFactory();

				GlbCompany company1 = factory1.NewWithValidTestData<GlbCompany>();
				GlbCompany company2 = factory1.NewWithValidTestData<GlbCompany>();

				GlbBranch branch1A = company1.Branches.AddNew();
				branch1A.GB_IsActive = true;
				branch1A.GB_Code = "U1A";
				branch1A.GB_BranchName = "Test1A";
				GlbBranch branch1B = company1.Branches.AddNew();
				branch1B.GB_IsActive = false;
				branch1B.GB_Code = "U1B";
				branch1B.GB_BranchName = "Test1B";
				GlbBranch branch1C = company1.Branches.AddNew();
				branch1C.GB_IsActive = false;
				branch1C.GB_Code = "U1C";
				branch1C.GB_BranchName = "Test1C";

				GlbBranch branch2A = company2.Branches.AddNew();
				branch2A.GB_IsActive = true;
				branch2A.GB_Code = "U2A";
				branch2A.GB_BranchName = "Test2A";
				GlbBranch branch2B = company2.Branches.AddNew();
				branch2B.GB_IsActive = true;
				branch2B.GB_Code = "U2B";
				branch2B.GB_BranchName = "Test2B";
				GlbBranch branch2C = company2.Branches.AddNew();
				branch2C.GB_IsActive = true;
				branch2C.GB_Code = "U2C";
				branch2C.GB_BranchName = "Test2C";

				factory1.Save();

				var factory2 = new BusinessObjectFactory();
				GlbCompany company2InFactory2 = factory2.Load<GlbCompany>(company2.PK);
				GlbBranch branch2AInFactory2 = factory2.Load<GlbBranch>(branch2A.PK);
				GlbBranch branch2BInFactory2 = factory2.Load<GlbBranch>(branch2B.PK);
				GlbBranch branch2CInFactory2 = factory2.Load<GlbBranch>(branch2C.PK);

				branch2AInFactory2.GB_IsActive = false;
				branch2BInFactory2.GB_IsActive = false;
				branch2CInFactory2.GB_IsActive = false;

				AssertEquals(false, branch2CInFactory2.IsLastActiveBranch);

				branch1B.GB_IsActive = true;
				branch1C.GB_IsActive = true;
				branch2B.GB_IsActive = false;
				branch2C.GB_IsActive = false;
				factory1.Save();

				var factory3 = new BusinessObjectFactory();
				GlbCompany company1InFactory3 = factory3.Load<GlbCompany>(company1.PK);
				GlbBranch branch1AInFactory3 = factory3.Load<GlbBranch>(branch1A.PK);
				GlbBranch branch1BInFactory3 = factory3.Load<GlbBranch>(branch1B.PK);
				GlbBranch branch1CInFactory3 = factory3.Load<GlbBranch>(branch1C.PK);

				branch1AInFactory3.GB_IsActive = false;
				branch1BInFactory3.GB_IsActive = false;
				branch1CInFactory3.GB_IsActive = false;

				AssertEquals(false, branch1CInFactory3.IsLastActiveBranch);
			}
		}

		#endregion

		#endregion

		#region Property Overrides

		#region GB_GC

		public void TestGB_GC()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			GlbCompany company = Factory.New<GlbCompany>();
			branch.GB_GC = company.PK;
			AssertEquals("GB_OH_OrgProxy should be empty", ZGuid.Empty, branch.GB_OH_OrgProxy);

			company.GC_OH_OrgProxy = ZGuid.NewZGuid();
			branch.GB_GC = ZGuid.Empty;
			branch.GB_GC = company.PK;
			AssertEquals("GB_OH_OrgProxy should be defaulted from company's GC_OH", company.GC_OH_OrgProxy, branch.GB_OH_OrgProxy);
		}

		#endregion

		#region GB_AccountingGroupCode

		public void TestGB_AccountingGroupCode()
		{
			AssertEquals("GB_AccountingGroupCode not assigned yet", true, Branch.GB_AccountingGroupCode.IsEmpty);

			var newCodeCollection = new BranchManagementCodeDescriptionBoolCollection();
			newCodeCollection.Add("BRA");
			AccountingMasterFilesRegistry.Instance.BranchManagementCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newCodeCollection);
			Branch.GB_AccountingGroupCode = "BRA";

			AssertEquals("GB_AccountingGroupCode is assigned", "BRA", Branch.GB_AccountingGroupCode);
		}

		#endregion

		#endregion

		#region ICancellable

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(typeof(GlbBranch)));
		}

		#endregion

		#region IAddressDetails

		public void TestIAddressDetails()
		{
			Branch.GB_Address1 = "ADDRESS1";
			Branch.GB_Address2 = "ADDRESS2";
			Branch.GB_City = "CITY";
			Branch.GB_State = "STATE";
			Branch.GB_PostCode = "1234";
			Branch.GB_RL_NKHomePort = "USLAX";
			Branch.GB_Email = "ZUBIN.APPOO@CARGOWISE.COM";
			Branch.GB_Phone = "123456";
			Branch.GB_Fax = "98764";
			Branch.GB_OH_OrgProxy = Factory.New<OrgHeader>().PK;
			Branch.OrgProxy.OH_FullName = "TEST";

			AssertEquals("ADDRESS1", AddressDetails.AddressLine1);
			AssertEquals("ADDRESS2", AddressDetails.AddressLine2);
			AssertEquals("CITY", AddressDetails.City);
			AssertEquals("STATE", AddressDetails.State);
			AssertEquals("1234", AddressDetails.PostCode);
			AssertEquals("US", AddressDetails.Country);
			AssertEquals("ZUBIN.APPOO@CARGOWISE.COM", AddressDetails.Email);
			AssertEquals("123456", AddressDetails.Phone);
			AssertEquals("98764", AddressDetails.Fax);
			AssertEquals("TEST", AddressDetails.CompanyName);
		}

		IAddressDetails AddressDetails
		{
			get { return Branch; }
		}

		#endregion

		#region ILocationReference

		public void TestILocationReferenceIsLocalInRelationTo()
		{
			ILocationReference locationReference = Branch;

			Branch.GB_RL_NKHomePort = ZString.Empty;
			AssertEquals("home port not specified", false, locationReference.IsLocalInRelationTo("AUSYD"));

			Branch.GB_RL_NKHomePort = "AUSYD";
			AssertEquals("AUSYD local to AUSYD", true, locationReference.IsLocalInRelationTo("AUSYD"));

			AssertEquals("AUMEL local to AUSYD", true, locationReference.IsLocalInRelationTo("AUMEL"));

			var extraPort = Branch.ExtraPorts.AddNew();
			extraPort.GY_RL_NKAdditionalBranchRelatedPort = "DEHAM";

			AssertEquals("local to extra port", true, locationReference.IsLocalInRelationTo("DEHAM"));
			AssertEquals("not local to unloco in the same country as extra port", false, locationReference.IsLocalInRelationTo("DEBER"));
		}

		#endregion

		#region IsDepartmentAllowed
		public void TestIsDepartmentAllowed()
		{
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			AssertEquals(0, Branch.AllowedDepartments.Count);
			AssertEquals(true, Branch.IsDepartmentAllowed(department1.PK));
			AssertEquals(true, Branch.IsDepartmentAllowed(department2.PK));

			var combo1 = Branch.AllowedDepartments.AddNew();
			combo1.AAB_GE_Department = department1.PK;

			AssertEquals(1, Branch.AllowedDepartments.Count);
			AssertEquals(true, Branch.IsDepartmentAllowed(department1.PK));
			AssertEquals(false, Branch.IsDepartmentAllowed(department2.PK));
		}
		#endregion

		#region Address Validation

		public void TestValidationStatus_WhenChangingAddressFieldWhileValueIsCna_ShouldKeepItAsCna()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_ValidationStatus = AddressValidationStatus.CountryNotAvailable;

			// Act & Assert.

			branch.GB_Address1 = "[_MOCK_ADDRESS_1_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, branch.GB_ValidationStatus);

			branch.GB_Address2 = "[_MOCK_ADDRESS_2_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, branch.GB_ValidationStatus);

			branch.GB_City = "[_MOCK_CITY_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, branch.GB_ValidationStatus);

			branch.GB_PostCode = "0000";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, branch.GB_ValidationStatus);

			branch.GB_State = "[_MOCK_STATE_]";
			AssertEquals(AddressValidationStatus.CountryNotAvailable, branch.GB_ValidationStatus);

			branch.GB_RN_NKCountryCode = "XY";
			AssertEquals(AddressValidationStatus.ToBeVerified, branch.GB_ValidationStatus);
		}

		public void TestChangingAddressResetsValidationStatus()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			var address = Factory.NewWithValidTestData<GlbBranch>();
			address.GB_RN_NKCountryCode = "AU";
			address.ValidationStatus = AddressValidationStatus.Verified;

			AssertValidationStatusIsReset(address, () => address.Address1 += "A");
			AssertValidationStatusIsReset(address, () => address.Address2 += "A");
			AssertValidationStatusIsReset(address, () => address.City += "A");
			AssertValidationStatusIsReset(address, () => address.Postcode += "A");
			AssertValidationStatusIsReset(address, () => address.State += "A");
		}

		void AssertValidationStatusIsReset(ISupportWebAddressValidation address, Action action)
		{
			address.ValidationStatus = AddressValidationStatus.Verified;
			action.Invoke();
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);
		}

		public void TestRaiseAddressValidationStatusChanged()
		{
			var address = Factory.NewWithValidTestData<GlbBranch>() as ISupportWebAddressValidation;
			address.ValidationStatus = AddressValidationStatus.ToBeVerified;
			address.Address2 = "";

			address.AddressValidationStatusChanged += address_AddressValidationStatusChanged;
			address.ValidationStatus = AddressValidationStatus.Verified;
			AssertEquals("It happened", address.Addressee);
		}

		void address_AddressValidationStatusChanged(object sender, EventArgs e)
		{
			((GlbBranch)sender).GB_BranchName = "It happened";
		}

		public void TestValidationStatus()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery());
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<GlbBranch>();
			address.GB_RN_NKCountryCode = country.Code;
			address.GB_ValidationStatus = AddressValidationStatus.ToBeVerified;
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);

			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.NotAvailable;
			AssertEquals(AddressValidationStatus.ToBeVerified, address.ValidationStatus);
		}

		public void TestState()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			country.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;

			var address = Factory.NewWithValidTestData<GlbBranch>();
			address.GB_RN_NKCountryCode = country.Code;
			address.GB_State = "NSW";
			AssertEquals("New South Wales", address.State);

			address.State = "Victoria";
			AssertEquals("VIC", address.GB_State);
		}

		public void TestNeedValidation()
		{
			var factory = new BusinessObjectFactory();

			var australia = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			australia.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.Street;
			var china = factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "CN");
			china.RN_ValidationStatus = ExternalAddressValidationRulesList.Codes.NotAvailable;

			var address = factory.NewWithValidTestData<GlbBranch>();
			address.Address1 = "A1";
			address.Address2 = "A2";
			address.Postcode = "1234";
			address.City = "Syd";
			address.State = "NSW";
			address.GB_RN_NKCountryCode = "CN";
			Assert(address.NeedValidation);

			address.GB_RN_NKCountryCode = "AU";
			Assert(address.NeedValidation);

			factory.Save();
			Assert(address.IsInDatabase);
			Assert(!address.NeedValidation);

			address.Address1 += "A";
			Assert(address.NeedValidation);

			address.Address2 = "";
			Assert(address.NeedValidation);

			address.Address1 = "";
			Assert(!address.NeedValidation);
		}

		public void TestResetAddressMap()
		{
			Env.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			var address = Factory.NewWithValidTestData<GlbBranch>();
			address.GB_RN_NKCountryCode = "AU";
			AssertAddressMap(address, address.GB_Address1Info);
			AssertAddressMap(address, address.GB_Address2Info);
			AssertAddressMap(address, address.GB_CityInfo);
			AssertAddressMap(address, address.GB_PostCodeInfo);
			AssertAddressMap(address, address.GB_StateInfo);
			AssertAddressMap(address, address.GB_RN_NKCountryCodeInfo);

			address.GB_RN_NKCountryCode = "AU";
			var usPK = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "US")).PK.ToGuid();
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(usPK, disabledForBranch: true));
			AssertAddressMap(address, address.GB_Address1Info);
			AssertAddressMap(address, address.GB_Address2Info);
			AssertAddressMap(address, address.GB_CityInfo);
			AssertAddressMap(address, address.GB_PostCodeInfo);
			AssertAddressMap(address, address.GB_StateInfo);
			AssertAddressMap(address, address.GB_RN_NKCountryCodeInfo);
		}

		void AssertAddressMap(GlbBranch address, ZPropertyInfo propertyInfo)
		{
			address.AddressMap = "ABCDE";
			propertyInfo.Value = (ZString)(propertyInfo.Name == nameof(GlbBranch.GB_RN_NKCountryCode) ? "US" : (ZString)propertyInfo.Value + "1");
			Assert(string.IsNullOrEmpty(address.AddressMap));
		}

		public void TestValidationSection()
		{
			AssertEquals(AddressValidationSection.Branch, Factory.New<GlbBranch>().ValidationSection);
		}

		#endregion

		public void TestShowCodeAtCompanyAndBranchName()
		{
			var factory = new BusinessObjectFactory();
			var company = factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();

			branch.GB_GC = company.PK;
			branch.GB_Code = "TBR";
			branch.GB_BranchName = "Test Branch";

			Env.Registry.ShowCodeAtCompanyAndBranchName = true;
			AssertEquals(branch.HumanReadableNameForRegistry, branch.HumanReadableShortcutName);

			Env.Registry.ShowCodeAtCompanyAndBranchName = false;
			AssertEquals(branch.HumanReadableNameForRegistry, branch.GB_BranchName);
		}

		#region GB_ValidationStatus

		public void TestValidationStatus_WhenSetToManuallyVerifiedFromOtherValue_ShouldStayAsIsUntilBranchIsReloaded()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();

			branch.GB_Address1 = "42 FOOBAR STREET";
			branch.GB_Address2 = "FUNPLACE";
			branch.GB_PostCode = "0000";
			branch.GB_City = "WHITERUN";
			branch.GB_State = "TAMRIEL";
			branch.GB_RN_NKCountryCode = "ID";

			// Act.

			branch.GB_ValidationStatus = AddressValidationStatus.ManuallyVerified;

			branch.GB_Address1 = "72 O'RIORDAN STREET";
			branch.GB_Address2 = "WISETECH GLOBAL";
			branch.GB_PostCode = "2015";
			branch.GB_City = "ALEXANDRIA";
			branch.GB_State = "NSW";
			branch.GB_RN_NKCountryCode = "AU";

			// Assert.

			AssertEquals(AddressValidationStatus.ManuallyVerified, branch.GB_ValidationStatus);
		}

		public void TestValidationStatus_WhenLoadedAsManuallyVerifiedFromDatabase_ShouldResetValueAfterChangingAddressField()
		{
			// Arrange.

			Env.Registry.EnableAddressValidationWebService = true;

			var factory = new BusinessObjectFactory();
			var company = factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_GC = company.PK;

			branch.GB_Address1 = "42 FOOBAR STREET";
			branch.GB_Address2 = "FUNPLACE";
			branch.GB_PostCode = "0000";
			branch.GB_City = "WHITERUN";
			branch.GB_State = "TAMRIEL";
			branch.GB_RN_NKCountryCode = "ID";

			branch.GB_ValidationStatus = AddressValidationStatus.ManuallyVerified;
			factory.Save();

			var reloadedBranch = new BusinessObjectFactory().Load<GlbBranch>(branch.PK);

			// Act.

			reloadedBranch.GB_Address1 = "72 O'RIORDAN STREET";
			reloadedBranch.GB_Address2 = "WISETECH GLOBAL";
			reloadedBranch.GB_PostCode = "2015";
			reloadedBranch.GB_City = "ALEXANDRIA";
			reloadedBranch.GB_State = "NSW";
			reloadedBranch.GB_RN_NKCountryCode = "AU";

			// Assert.

			AssertEquals(AddressValidationStatus.ToBeVerified, reloadedBranch.GB_ValidationStatus);
		}

		#endregion

		#region GeoLocation

		public void TestConstructor_WhenCreatingWithDataRow_ShouldInitializeGeoLocationWithNonNullValue()
		{
			// Arrange.

			// Act.

			var branch = Factory.New<GlbBranch>();

			// Assert.

			var row = ((INeedRow)branch).Row;

			AssertEquals(ZGeography.Empty, row[GlbBranchSchema.Constants.GB_GeoLocation]);
		}

		public void TestGeoLocation_WhenGettingEmptyValue_ShouldSetItToPointZero()
		{
			// Arrange.

			var branch = Factory.New<GlbBranch>();

			// Act.

			branch.GB_GeoLocation = ZGeography.Empty;

			// Assert.

			AssertEquals(ZGeography.Empty, branch.GB_GeoLocation);
		}

		#endregion
		#region Implementation

		GlbBranch Branch;
		ZGuid InitialProxyOrgPK;

		int GenCustomAddOnRuleAckCount
		{
			get
			{
				var factory = Factory.Load<GenCustomAddOnRuleAck>(new ZQuery(GenCustomAddOnRuleAckSchema.XK_RuleID, Core.Constants.CargoWiseOneGenCustomAddOnRuleIDs.PhoneNumberFormatValidation));
				return factory.Length;
			}
		}

		protected override void SetUp()
		{
			InitialProxyOrgPK = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			base.SetUp();
			Branch = Factory.New<GlbBranch>();
			Branch.GB_GC = Env.CurrentCompany.PK;       // Required to save to database successfully.
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = InitialProxyOrgPK;
		}

		AccountingTestObjectCreator AccountingTestObjectCreator => accTestObjectCreator ?? (accTestObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator accTestObjectCreator;

		#endregion
	}
}
