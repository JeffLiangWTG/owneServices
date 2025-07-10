using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(ReconModule))]
	sealed class ReconModuleTest : ZModuleBasherWithFetchHintsTest
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
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration.US_IssueCode = ReconIssueCodeList.Codes.ValueRecon;
			_ = new ReconDeclaration(declaration);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Recon;
			declaration2.US_IssueCode = ReconIssueCodeList.Codes.ClassRecon;
			_ = new ReconDeclaration(declaration2);
			Factory.Save();
			using (var module = new ReconModule())
			{
				var filterBusinessObject = module.FilterBusinessObject;
				var issueCodeFilter = (ModuleTextFilter)filterBusinessObject[ReconFilterStripBusinessObject.Schema.Issue];
				issueCodeFilter.Property = ReconIssueCodeList.Codes.ClassRecon;
				issueCodeFilter.IsActive = true;
				var moduleTesting = (IFilterModuleInternalsForTesting)module;
				moduleTesting.PerformSearch();
				AssertEquals("It should display all recons that are created in the current company", 1, moduleTesting.GridCollection.Count);
				AssertEquals(declaration2.PK, ((BusinessObject)(moduleTesting.GridCollection[0])).PK);
				issueCodeFilter.Property = ReconIssueCodeList.Codes.ValueRecon;
				issueCodeFilter.IsActive = true;
				moduleTesting.PerformSearch();
				AssertEquals("It should display all recons that are created in the current company", 1, moduleTesting.GridCollection.Count);
				AssertEquals(declaration.PK, ((BusinessObject)(moduleTesting.GridCollection[0])).PK);
			}
		}

		public void TestReconModuleAllows()
		{
			using (var module = new ReconModule())
			{
				AssertEquals("module.AllowNew", true, module.AllowNew);
				AssertEquals("module.AllowEdit", true, module.AllowEdit);
				AssertEquals("module.AllowDelete", true, module.AllowDelete);
			}
		}

		public void TestToolBarButtons()
		{
			using (var module = new ReconModule())
			{
				AssertEquals("Module should have 5 standard buttons", 6, module.ToolBarButtons.Length);
				AssertEquals("View", "View", module.ToolBarButtons[0].Text);
				AssertEquals("New", "New", module.ToolBarButtons[1].Text);
				AssertEquals("Edit", "Edit", module.ToolBarButtons[2].Text);
				AssertEquals("Delete", "Delete", module.ToolBarButtons[3].Text);
				AssertEquals("Actions", "Actions", module.ToolBarButtons[4].Text);
				AssertEquals("Hide/Show Filters", "Hide/Show Filters", module.ToolBarButtons[5].Text);
			}
		}

		public void TestCorrectFormIsOpened()
		{
			JobDeclarationModuleTest.AssertCorrectFormIsOpened<ReconModule>();
		}

		public void TestSearchForReconCreatedInCurrentCompany()
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
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "US2";
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_GB = branch2.PK;
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Recon;
			Factory.Save();
			using (var module = new ReconModule())
			{
				var moduleTesting = (IFilterModuleInternalsForTesting)module;
				moduleTesting.PerformSearch();
				AssertEquals("It should display all recons that are created in the current company", 2, moduleTesting.GridCollection.Count);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.Reconciliation;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected override bool HasController() => true;

		protected override ZFilterModule CreateModuleForFetchHintsTest() => new ReconModule();

		protected override void SetupDataForFetchHintsTest()
		{
			var newFactory = new BusinessObjectFactory();
			for (var i = 0; i < 9; i++)
			{
				CreateReconDeclarationForFetchHintTest(newFactory, i);
			}

			newFactory.Save();
		}

		protected override List<string> FetchHintIgnoreField
		{
			get
			{
				var result = base.FetchHintIgnoreField;
				result.Add(JobDeclaration.Schema.DISStatus); // WI00076482 - Tim.Van : temporarity ignore it for now, will come back and finish it later since it needs more thoughts.
				result.Add(JobDeclaration.Schema.DISStatusDescription);
				result.Add(JobDeclaration.Schema.US_AnticipatedLiquidationDate);
				result.Add(JobDeclaration.Schema.JE_OH_Importer);
				return result;
			}
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

		void CreateReconDeclarationForFetchHintTest(BusinessObjectFactory factory, int i)
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
			var organisations = factory.GetCachedValue("OrganisationReconModuleTest", delegate
			{
				return factory.Load<OrgHeader>(new ZQuery()
				{ MaximumRows = 24 });
			});

			var importDeclaration = Factory.New<JobDeclaration>();
			importDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importEntry = importDeclaration.CustomsEntryHeaders.AddNew();
			importEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconEntry;
			importEntry.US_ALDate = ZDateTime.BrettsBirthday;
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var reconDec = new ReconDeclaration(declaration);
			var lookups = reconDec.Lookups;
			var reconIssueCodeList = lookups.US_IssueCodeList;
			var paymentTypeList = lookups.US_PaymentTypeList;
			var reconMessageStatusList = factory.GetCachedValue<ReconMessageStatusList>();
			reconDec.JE_OH_Importer = organisations[(i + 1) % organisations.Length].PK;
			reconDec.US_IssueCode = reconIssueCodeList[i % reconIssueCodeList.Count].Code;
			reconDec.US_SchDEntry = "000" + number;
			reconDec.US_EstimatedEntryDate = ZDateTime.Today.AddMinutes(i);
			reconDec.US_PaymentType = paymentTypeList[i % paymentTypeList.Count].Code;
			reconDec.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddMinutes(i);
			reconDec.US_EntryFilerCode = "EF" + number;
			reconDec.MessageStatus = reconMessageStatusList[i % reconMessageStatusList.Count].Code;
			declaration.JE_EntryStatus = reconDec.MessageStatus;
			reconDec.JE_GS_NKCusAgent = staffs[mod6].GS_Code;
			reconDec.US_SuretyCode = "SC" + number;
			var lastMilestone = declaration.WorkflowItems.Milestones.AddNew();
			lastMilestone.TriggerConditions.TriggerEventCode = "E" + number;
			lastMilestone.P9_Description = "IMP" + number;
			lastMilestone.SetMilestoneActualDateForTest(ZDateTime.Now);
			var nextMilestone = declaration.WorkflowItems.Milestones.AddNew();
			nextMilestone.P9_Description = "NXT" + number;
			nextMilestone.TriggerConditions.TriggerEventCode = "M" + number;
			nextMilestone.SetMilestoneScheduledDateForTest(ZDateTimeOffset.Today.AddDays(13));
			declaration.AllocateEntryNumber("ENTNUM" + number);
			var liquidation = declaration.Liquidations.AddNew();
			liquidation.B8_SystemCreateDate = ZDateTime.Today;
			liquidation.B8_LiquidationDate = ZDateTime.Today;
			liquidation.B8_LiquidationType = LiquidationTypeCodeList.Codes.Code04;
		}
	}
}
