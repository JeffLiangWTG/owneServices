using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Protest;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ProtestClass = Enterprise.Customs.US.Business.Protest.Protest;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(ProtestModule))]
	sealed class ProtestModuleTest : ZModuleBasherWithFetchHintsTest
	{
		public void TestClearCollectionBeforeNewSearch()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			company.GC_Code = "US1";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "US1";
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_GB = GlbBranch.CurrentBranch.PK;
			var protest1 = new ProtestClass(declaration1);
			protest1.US_P_ProtestantType = ProtestantTypeList.Codes.ImporterConsignee;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_GB = GlbBranch.CurrentBranch.PK;
			var protest2 = new ProtestClass(declaration2);
			protest2.US_P_ProtestantType = ProtestantTypeList.Codes.DrawbackClaimant;
			Factory.Save();
			using (var module = new ProtestModule())
			{
				var filterBusinessObject = module.FilterBusinessObject;
				var protestantTypeFilter = (ModuleTextFilter)filterBusinessObject[ProtestFilterStripBusinessObject.Schema.ProtestantType];
				protestantTypeFilter.Property = ProtestantTypeList.Codes.DrawbackClaimant;
				protestantTypeFilter.IsActive = true;
				var moduleTesting = (IFilterModuleInternalsForTesting)module;
				moduleTesting.PerformSearch();
				AssertEquals("It should display all recons that are created in the current company", 1, moduleTesting.GridCollection.Count);
				AssertEquals(protest2.PK, ((BusinessObject)(moduleTesting.GridCollection[0])).PK);
				protestantTypeFilter.Property = ProtestantTypeList.Codes.ImporterConsignee;
				protestantTypeFilter.IsActive = true;
				moduleTesting.PerformSearch();
				AssertEquals("It should display all recons that are created in the current company", 1, moduleTesting.GridCollection.Count);
				AssertEquals(protest1.PK, ((BusinessObject)(moduleTesting.GridCollection[0])).PK);
			}
		}

		public void TestProtestModuleAllows()
		{
			using (var module = new ProtestModule())
			{
				AssertEquals("module.AllowNew", true, module.AllowNew);
				AssertEquals("module.AllowEdit", true, module.AllowEdit);
				AssertEquals("module.AllowDelete", false, module.AllowDelete);
			}
		}

		public void TestToolBarButtons()
		{
			using (var module = new ProtestModule())
			{
				AssertEquals("Module should have 4 standard buttons", 5, module.ToolBarButtons.Length);
				AssertEquals("View", "View", module.ToolBarButtons[0].Text);
				AssertEquals("New", "New", module.ToolBarButtons[1].Text);
				AssertEquals("Edit", "Edit", module.ToolBarButtons[2].Text);
				AssertEquals("Actions", "Actions", module.ToolBarButtons[3].Text);
				AssertEquals("Hide/Show Filters", "Hide/Show Filters", module.ToolBarButtons[4].Text);
			}
		}

		public void TestCorrectFormIsOpened()
		{
			JobDeclarationModuleTest.AssertCorrectFormIsOpened<ProtestModule>();
		}

		public void TestSearchForProtestCreatedInCurrentCompany()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			company.GC_Code = "US1";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "US1";
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = branch.PK;
			declaration.JE_MessageType = JobMessageTypeList.MoreCodes.Protest;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration2.JE_MessageType = JobMessageTypeList.MoreCodes.Protest;
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "US2";
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_GB = branch2.PK;
			declaration3.JE_MessageType = JobMessageTypeList.MoreCodes.Protest;
			Factory.Save();
			using (var module = new ProtestModule())
			{
				var moduleTesting = (IFilterModuleInternalsForTesting)module;
				moduleTesting.PerformSearch();
				AssertEquals("It should display all protests that are created in the current company", 2, moduleTesting.GridCollection.Count);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.Protest;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override bool HasController() => true;

		protected override ZFilterModule CreateModuleForFetchHintsTest() => new ProtestModule();

		protected override void SetupDataForFetchHintsTest()
		{
			var newFactory = new BusinessObjectFactory();
			for (var i = 0; i < 9; i++)
			{
				CreateProtestForFetchHintTest(newFactory, i);
			}

			newFactory.Save();
		}

		GlbStaff CreateStaff(BusinessObjectFactory factory, ZString code)
		{
			var staff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, code);
			if (staff == null)
			{
				staff = factory.New<GlbStaff>();
				staff.GS_FullName = code + " name";
				staff.GS_LoginName = code;
				staff.GS_Code = code;
			}

			return staff;
		}

		void CreateProtestForFetchHintTest(BusinessObjectFactory factory, int i)
		{
			var number = i.ToString();
			var mod6 = i % 6;

			var staffs = factory.GetCachedValue("GlbStaffTest", delegate
			{
				return new[]
				{
					CreateStaff(factory, "SA1"),
					CreateStaff(factory, "SA2"),
					CreateStaff(factory, "SA3"),
					CreateStaff(factory, "SA4"),
					CreateStaff(factory, "SA5"),
					CreateStaff(factory, "SA6")
				};
			});

			var jobStatuses = factory.GetCachedValue("JobHeaderStatusTest", delegate
			{
				return new[]
					{
						JobHeaderStatus.Closed,
						JobHeaderStatus.Complete,
						JobHeaderStatus.CustomsProcessActive,
						JobHeaderStatus.InvoiceOnHold,
						JobHeaderStatus.JobInvoiced,
						JobHeaderStatus.JobReadyForCostPosting,
						JobHeaderStatus.JobReadyForDelivery,
						JobHeaderStatus.JobReadyForRevenueAndCostPosting,
						JobHeaderStatus.JobReadyForRevenuePosting,
						JobHeaderStatus.ScheduledForArchive,
						JobHeaderStatus.Working,
						JobHeaderStatus.WorkOnHold,
					};
			});

			var organisations = factory.GetCachedValue("OrganisationReconModuleTest", delegate
			{
				return factory.Load<OrgHeader>(new ZQuery() { MaximumRows = 24 });
			});

			var scheduleDs = factory.GetCachedValue("USCRegionDistrictPortReconModuleTest", delegate
			{
				var helper = new UniversalReferenceTestDataHelper(factory);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
				var list = new ZZRefCusCodeListCombinedCollection(factory);

				for (var index = 0; index < 11; index++)
				{
					list.Add(helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "000" + index, "Test PR Port", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6)));
				}
				factory.Save();
				return list;
			});

			var declaration = factory.New<JobDeclaration>();
			var protest = new ProtestClass(declaration);
			var lookups = protest.Lookups;
			var protestantTypes = lookups.ProtestantTypes;
			var messageStatusList = declaration.Lookups.MessageStatusList;
			var protestStatusCodesList = factory.GetCachedValue<ProtestStatusCodesList>();
			protest.US_P_CBPAssignedProtestNumber = "P" + number;
			protest.US_P_MerchandiseDesc = "MDesc " + number;
			protest.Protestant.E2_AddressOverride = ZBool.True;
			protest.Protestant.E2_CompanyName = "Protestant " + number;
			protest.US_P_ProtestantType = protestantTypes[i % protestantTypes.Count].Code;
			protest.US_P_FilingDDPP = "000" + number;
			protest.US_P_Assoc514ProtestNo = "A" + number;
			protest.US_P_Assoc520PetitionNo = "B" + number;
			declaration.JE_EntryStatus = protestStatusCodesList[i % protestStatusCodesList.Count].Code;
			protest.US_P_StatusDate = ZDateTime.Today.AddMinutes(i);
			declaration.JE_MessageStatus = messageStatusList[i % messageStatusList.Count].Code;
			protest.US_P_AddressTeam = "AT" + number;
			protest.US_P_InternalAdviceNo = "IA" + number;
			protest.US_P_LeadProtestNo = "LPN" + number;
			protest.US_P_TestSummonsNo = "TS" + number;
			protest.US_P_SubstituteDDPP = "000" + number;
			protest.US_P_SubstituteFilerCode = "SF" + number;
			protest.RefundPartyAddress.OrganisationPK = organisations[(i + 1) % organisations.Length].PK;
			var linkEntry1 = protest.LinkedEntries.AddNew();
			linkEntry1.US_LE_EntryNumber = "LE" + number;
			protest.JE_GS_NKCusAgent = staffs[mod6].GS_Code;
			protest.US_P_PeriodBaseDate = ZDateTime.Today.AddMinutes(i + 1);
			new JobHeader.Loader(declaration).TryLoadOrCreate();
			declaration.Job.JH_Status = jobStatuses[i % jobStatuses.Length].Code;
		}
	}
}
