using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(ReconFilterStripBusinessObject))]
	sealed class ReconFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = (ReconFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			AssertNotNull(filter[ReconFilterStripBusinessObject.Schema.DeclarationReference]);
			AssertNotNull(filter[ReconFilterStripBusinessObject.Schema.Importer]);
			AssertNotNull(filter[ReconFilterStripBusinessObject.Schema.Issue]);
			AssertNotNull(filter[ReconFilterStripBusinessObject.Schema.FilingPort]);
			AssertNotNull(filter[ReconFilterStripBusinessObject.Schema.EstReconDate]);
			AssertNotNull(filter[ReconFilterStripBusinessObject.Schema.PaymentType]);
			AssertNotNull(filter[ReconFilterStripBusinessObject.Schema.StatementPrintDate]);
			AssertNotNull(filter[ReconFilterStripBusinessObject.Schema.EntryNumber]);
			AssertNotNull(filter[ReconFilterStripBusinessObject.Schema.EntryNumberOnReconciliation]);
			AssertNotNull(filter[ReconFilterStripBusinessObject.Schema.JobNumberOnReconciliation]);
			AssertNotNull(filter[ReconFilterStripBusinessObject.Schema.ReconciliationStatus]);
			AssertNotNull(filter[ReconFilterStripBusinessObject.Schema.MessageStatus]);
			AssertNotNull(filter[ReconFilterStripBusinessObject.Schema.Preparer]);
			AssertNotNull(filter[ReconFilterStripBusinessObject.Schema.IsAggregate]);
		}

		public void TestSuretyCodeQueryWithoutGenAddOnColumn()
		{
			var reconDeclaration1 = GetReconDeclaration("B00001003", "00000048", ReconMessageStatusList.Codes.ClearReconOriginal);
			reconDeclaration1.US_SuretyCode = "12";
			DeleteAllGenAddOnColumn(reconDeclaration1);

			var reconDeclaration2 = GetReconDeclaration("B00001004", "00000051", ReconMessageStatusList.Codes.ReconOriginalAcceptedWarnings);
			reconDeclaration2.US_SuretyCode = "123";
			DeleteAllGenAddOnColumn(reconDeclaration2);

			var reconDeclaration3 = GetReconDeclaration("B00001005", "00000055", ReconMessageStatusList.Codes.ErrorReconDelete);
			reconDeclaration3.US_SuretyCode = "123";
			DeleteAllGenAddOnColumn(reconDeclaration3);

			Factory.Save();

			var filter = new ReconFilterStripBusinessObject();
			var suretyCodeFilter = (ModuleTextFilter)filter[DeclarationFilterConstants.SuretyCode];
			suretyCodeFilter.Property = "123";
			suretyCodeFilter.IsActive = true;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(2, coll.Count);
			Assert(coll.Contains(reconDeclaration2));
			Assert(coll.Contains(reconDeclaration3));
		}

		public void TestCurrentCompanyFilter()
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK;
			var currentCompanyBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyBranch1.GB_GC = currentCompanyPK;
			var currentCompanyBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyBranch2.GB_GC = currentCompanyPK;
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var company2Branch1 = Factory.NewWithValidTestData<GlbBranch>();
			company2Branch1.GB_GC = company2.PK;
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_GB = currentCompanyBranch1.PK;
			dec1.JE_DeclarationReference = "B0001";
			var recon1 = new ReconDeclaration(dec1);
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_GB = currentCompanyBranch2.PK;
			dec2.JE_DeclarationReference = "B0002";
			var recon2 = new ReconDeclaration(dec2);
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_GB = company2Branch1.PK;
			dec3.JE_DeclarationReference = "B0003";
			var recon3 = new ReconDeclaration(dec3);
			Factory.Save();
			var filterObj = GetNewFilterStripBusinessObject();
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filterObj.Filter);
			coll.Sort("JE_DeclarationReference");
			AssertEquals(2, coll.Count);
			AssertEquals(recon1.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			AssertEquals(recon2.JE_DeclarationReference, coll[1].JE_DeclarationReference);
		}

		public void TestTextFiltersQuery()
		{
			var reconDeclaration1 = GetReconDeclaration("B00001003", "00000048", ReconMessageStatusList.Codes.ClearReconOriginal);
			reconDeclaration1.US_IssueCode = ReconIssueCodeList.Codes.Class9802Recon;
			reconDeclaration1.US_SchDEntry = ReconPortsList.Codes._2304;
			reconDeclaration1.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			var reconDeclaration2 = GetReconDeclaration("B00001004", "00000051", ReconMessageStatusList.Codes.ReconOriginalAcceptedWarnings);
			reconDeclaration2.US_IssueCode = ReconIssueCodeList.Codes.Value9802Recon;
			reconDeclaration2.US_SchDEntry = ReconPortsList.Codes._0712;
			reconDeclaration2.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			var reconDeclaration3 = GetReconDeclaration("B00001005", "00000055", ReconMessageStatusList.Codes.ErrorReconDelete);
			reconDeclaration3.US_IssueCode = ReconIssueCodeList.Codes.ClassRecon;
			reconDeclaration3.US_SchDEntry = ReconPortsList.Codes._2604;
			reconDeclaration3.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			DeleteAllGenAddOnColumn(reconDeclaration1);
			DeleteAllGenAddOnColumn(reconDeclaration2);
			DeleteAllGenAddOnColumn(reconDeclaration3);
			Factory.Save();
			var filter = new ReconFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filter[ReconFilterStripBusinessObject.Schema.DeclarationReference];
			textFilter.Property = reconDeclaration1.JE_DeclarationReference;
			textFilter.IsActive = true;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(reconDeclaration1.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			filter = new ReconFilterStripBusinessObject();
			textFilter = (ModuleTextFilter)filter[ReconFilterStripBusinessObject.Schema.Issue];
			textFilter.Property = ReconIssueCodeList.Codes.Value9802Recon;
			textFilter.IsActive = true;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(reconDeclaration2.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			filter = new ReconFilterStripBusinessObject();
			textFilter = (ModuleTextFilter)filter[ReconFilterStripBusinessObject.Schema.FilingPort];
			textFilter.Property = ReconPortsList.Codes._2304;
			textFilter.IsActive = true;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(reconDeclaration1.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			filter = new ReconFilterStripBusinessObject();
			textFilter = (ModuleTextFilter)filter[ReconFilterStripBusinessObject.Schema.PaymentType];
			textFilter.Property = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			textFilter.IsActive = true;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(reconDeclaration1.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			filter = new ReconFilterStripBusinessObject();
			textFilter = (ModuleTextFilter)filter[ReconFilterStripBusinessObject.Schema.EntryNumber];
			textFilter.Property = reconDeclaration3.ReconEntry.EntryNumber;
			textFilter.IsActive = true;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(reconDeclaration3.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			filter = new ReconFilterStripBusinessObject();
			textFilter = (ModuleTextFilter)filter[ReconFilterStripBusinessObject.Schema.ReconciliationStatus];
			textFilter.Property = ReconMessageStatusList.Codes.ReconOriginalAcceptedWarnings;
			textFilter.IsActive = true;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(reconDeclaration2.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			reconDeclaration1.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
			reconDeclaration2.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconDelete;
			reconDeclaration3.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ErrorReconOriginal;
			Factory.Save();
			filter = new ReconFilterStripBusinessObject();
			textFilter = (ModuleTextFilter)filter[ReconFilterStripBusinessObject.Schema.MessageStatus];
			textFilter.Property = ReconMessageStatusList.Codes.ClearReconDelete;
			textFilter.IsActive = true;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(reconDeclaration2.JE_DeclarationReference, coll[0].JE_DeclarationReference);
		}

		public void TestJobNumberOnReconciliationFilter()
		{
			var reconDeclaration1 = GetReconDeclarationWithOriginalEntry("B00001234", "00789456", "B00001203");
			var reconDeclaration2 = GetReconDeclarationWithOriginalEntry("B00001235", "00789457", "B00001004");
			var reconDeclaration3 = GetReconDeclarationWithOriginalEntry("B00001236", "00789458", "B00001005");
			var declarationShouldBeReconciled = Factory.New<JobDeclaration>();
			declarationShouldBeReconciled.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationShouldBeReconciled.JE_DeclarationReference = "B00001301";
			declarationShouldBeReconciled.US_EntryFilerCode = "XJ5";
			var entryOnReconciliation = declarationShouldBeReconciled.ActiveEntryHeaders.AddNew();
			entryOnReconciliation.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryOnReconciliation.EntryNumber = "00789059";
			Factory.Save();
			reconDeclaration3.ImportLines(new JobDeclaration[] { declarationShouldBeReconciled });
			var reconDeclaration4 = GetReconDeclarationWithOriginalEntry("B00001237", "00789459", "B00001006");
			var reconDeclaration5 = GetReconDeclaration("B00001003", "00000048", "");
			Factory.Save();
			var filter = new ReconFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filter[ReconFilterStripBusinessObject.Schema.JobNumberOnReconciliation];
			textFilter.Property = "B00001235";
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			textFilter.IsActive = true;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(reconDeclaration2.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			textFilter.Property = "B00001236";
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(reconDeclaration3.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			textFilter.Property = "B0000123";
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(4, coll.Count);
			Assert(coll.Contains(reconDeclaration1.ReconWrappedJobDeclaration));
			Assert(coll.Contains(reconDeclaration2.ReconWrappedJobDeclaration));
			Assert(coll.Contains(reconDeclaration3.ReconWrappedJobDeclaration));
			Assert(coll.Contains(reconDeclaration4.ReconWrappedJobDeclaration));
			textFilter.Property = "B00001234";
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(3, coll.Count);
			Assert(coll.Contains(reconDeclaration2.ReconWrappedJobDeclaration));
			Assert(coll.Contains(reconDeclaration3.ReconWrappedJobDeclaration));
			Assert(coll.Contains(reconDeclaration4.ReconWrappedJobDeclaration));
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(3, coll.Count);
			Assert(coll.Contains(reconDeclaration2.ReconWrappedJobDeclaration));
			Assert(coll.Contains(reconDeclaration3.ReconWrappedJobDeclaration));
			Assert(coll.Contains(reconDeclaration4.ReconWrappedJobDeclaration));
			textFilter.Property = "B00001301";
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(4, coll.Count);
			Assert(coll.Contains(reconDeclaration1.ReconWrappedJobDeclaration));
			Assert(coll.Contains(reconDeclaration2.ReconWrappedJobDeclaration));
			Assert(coll.Contains(reconDeclaration4.ReconWrappedJobDeclaration));
			Assert(coll.Contains(reconDeclaration5.ReconWrappedJobDeclaration));
			textFilter.Property = ZString.Empty;
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(reconDeclaration5.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(4, coll.Count);
			Assert(coll.Contains(reconDeclaration1.ReconWrappedJobDeclaration));
			Assert(coll.Contains(reconDeclaration2.ReconWrappedJobDeclaration));
			Assert(coll.Contains(reconDeclaration3.ReconWrappedJobDeclaration));
			Assert(coll.Contains(reconDeclaration4.ReconWrappedJobDeclaration));
		}

		public void TestOrganizationFiltersQuery()
		{
			var reconDeclaration1 = GetReconDeclaration("B00001003", "00000048", ReconMessageStatusList.Codes.ClearReconOriginal);
			var reconDeclaration2 = GetReconDeclaration("B00001004", "00000049", ReconMessageStatusList.Codes.ClearReconOriginal);
			var reconDeclaration3 = GetReconDeclaration("B00001005", "00000050", ReconMessageStatusList.Codes.ClearReconOriginal);
			var reconDeclaration4 = GetReconDeclaration("B00001006", "00000051", ReconMessageStatusList.Codes.AwaitingReconOriginal);
			var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
			reconDeclaration3.JE_OH_Importer = organisation.PK;
			Factory.Save();
			var filter = new ReconFilterStripBusinessObject();
			var orgFilter = (ModuleGuidFilter)filter[ReconFilterStripBusinessObject.Schema.Importer];
			orgFilter.Property = organisation.PK;
			orgFilter.IsActive = true;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals("Declaration 3 should be found", reconDeclaration3.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			reconDeclaration4.JE_GB = branch.PK;
			reconDeclaration4.JE_OH_Importer = organisation.PK;
			Factory.Save();
			var branchFilter = (ModuleGuidFilter)filter[ReconFilterStripBusinessObject.Schema.Branch];
			branchFilter.Property = branch.PK;
			branchFilter.IsActive = true;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals("Declaration 4 should be found", reconDeclaration4.JE_DeclarationReference, coll[0].JE_DeclarationReference);
		}

		public void TestDateFiltersQuery()
		{
			var reconDeclaration1 = GetReconDeclaration("B00001003", "00000048", ReconMessageStatusList.Codes.ClearReconOriginal);
			reconDeclaration1.US_EstimatedEntryDate = ZDateTime.Today.AddDays(-5);
			reconDeclaration1.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(4);
			var reconDeclaration2 = GetReconDeclaration("B00001004", "00000049", ReconMessageStatusList.Codes.ClearReconOriginal);
			reconDeclaration2.US_EstimatedEntryDate = ZDateTime.Today.AddDays(-1);
			reconDeclaration2.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(1);
			var reconDeclaration3 = GetReconDeclaration("B00001005", "00000050", ReconMessageStatusList.Codes.ClearReconOriginal);
			reconDeclaration3.US_EstimatedEntryDate = ZDateTime.Today.AddDays(1);
			reconDeclaration3.US_PreliminaryStatementPrintDate = ZDateTime.Today;
			DeleteAllGenAddOnColumn(reconDeclaration1);
			DeleteAllGenAddOnColumn(reconDeclaration2);
			DeleteAllGenAddOnColumn(reconDeclaration3);
			Factory.Save();
			var filter = new ReconFilterStripBusinessObject();
			var dateFilter = (ModuleDateFilter)filter[ReconFilterStripBusinessObject.Schema.EstReconDate];
			dateFilter.Property1 = ZDateTime.Today.AddDays(-2);
			dateFilter.Property2 = ZDateTime.Today;
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals("Declaration 2 with 'US_EstimatedEntryDate - Estimated Reconciliation Date'", reconDeclaration2.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			filter = new ReconFilterStripBusinessObject();
			var dateFilter2 = (ModuleDateFilter)filter[ReconFilterStripBusinessObject.Schema.StatementPrintDate];
			dateFilter2.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter2.Property1 = ZDateTime.Today.AddDays(2);
			dateFilter2.Property2 = ZDateTime.Today.AddDays(8);
			dateFilter2.IsActive = true;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals("Declaration1 with 'US_PreliminaryStatementPrintDate - Stm Print Date'", reconDeclaration1.JE_DeclarationReference, coll[0].JE_DeclarationReference);
		}

		public void TestOtherTypesOfFilters()
		{
			var reconDeclaration1 = GetReconDeclaration("B00001003", "00000048", ReconMessageStatusList.Codes.ClearReconOriginal);
			var reconDeclaration2 = GetReconDeclaration("B00001004", "00000049", ReconMessageStatusList.Codes.ClearReconOriginal);
			reconDeclaration2.JE_GS_NKCusAgent = "SVA";
			var reconDeclaration3 = GetReconDeclaration("B00001005", "00000050", ReconMessageStatusList.Codes.ClearReconOriginal);
			reconDeclaration3.US_IsAggregate = true;
			DeleteAllGenAddOnColumn(reconDeclaration1);
			DeleteAllGenAddOnColumn(reconDeclaration2);
			DeleteAllGenAddOnColumn(reconDeclaration3);
			Factory.Save();
			var filter = new ReconFilterStripBusinessObject();
			var nkFilter = (ModuleNkFilter)filter[ReconFilterStripBusinessObject.Schema.Preparer];
			nkFilter.Property = "SVA";
			nkFilter.IsActive = true;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals("Declaration 2 should be found", reconDeclaration2.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			filter = new ReconFilterStripBusinessObject();
			var flagFilter = (ModuleFlagsFilter)filter[ReconFilterStripBusinessObject.Schema.IsAggregate];
			flagFilter.Property0 = true;
			flagFilter.IsActive = true;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			var data = Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_Name, "US_IsAggregate"));
			AssertEquals(1, coll.Count);
			AssertEquals("Declaration 3 should be found", reconDeclaration3.JE_DeclarationReference, coll[0].JE_DeclarationReference);
		}

		public void TestReconEntryNumber()
		{
			var recon1 = GetReconDeclaration("B34280", "12345678901", "");
			var recon2 = GetReconDeclaration("B34281", "12345678902", "");
			Factory.Save();
			var filterBizObj = new ReconFilterStripBusinessObject();
			var filter = (ModuleNumberFilter)filterBizObj[ReconFilterStripBusinessObject.Schema.EntryNumber];
			filter.Property = "12345678901";
			filter.IsActive = true;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filterBizObj.Filter);
			Assert(coll.Contains(recon1.JE_PK));
			Assert(!coll.Contains(recon2.JE_PK));
		}

		public void TestWorkflowFiltersPresent()
		{
			var milestoneFilter = new ReconFilterStripBusinessObject();
			AssertNotNull("Workflow filter strips should be added", milestoneFilter["Milestone Date"]);
		}

		public void TestLiquidationDateFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var liquidation1 = Factory.New<CusLiquidation>();
			liquidation1.B8_LiquidationDate = ZDateTime.Today;
			liquidation1.B8_SystemCreateDate = new ZDateTime(2012, 07, 10);
			declaration1.Liquidations.Add(liquidation1);
			var recon1 = GetReconDeclaration("B34280", "45678901", "");
			var recon1_liquidation = Factory.New<CusLiquidation>();
			recon1_liquidation.B8_LiquidationDate = ZDateTime.Today;
			recon1_liquidation.B8_SystemCreateDate = new ZDateTime(2012, 07, 10);
			recon1.Liquidations.Add(recon1_liquidation);
			var recon2 = GetReconDeclaration("B00160013", "00000012", "");
			var liquidation2 = Factory.New<CusLiquidation>();
			liquidation2.B8_LiquidationDate = new ZDateTime(2009, 11, 20);
			liquidation2.B8_SystemCreateDate = new ZDateTime(2012, 07, 10);
			recon2.Liquidations.Add(liquidation2);
			var liquidation2_1 = Factory.New<CusLiquidation>();
			liquidation2_1.B8_LiquidationDate = ZDateTime.Empty;
			liquidation2_1.B8_SystemCreateDate = new ZDateTime(2012, 07, 09);
			recon2.Liquidations.Add(liquidation2_1);
			var recon3 = GetReconDeclaration("B00160218", "00000026", "");
			var liquidation3 = Factory.New<CusLiquidation>();
			liquidation3.B8_LiquidationDate = ZDateTime.Empty;
			liquidation3.B8_SystemCreateDate = new ZDateTime(2012, 07, 10);
			recon3.Liquidations.Add(liquidation3);
			var recon4 = GetReconDeclaration("B00160219", "00000026", "");
			var recon5 = GetReconDeclaration("B00160501", "00000029", "");
			var liquid5_1 = Factory.New<CusLiquidation>();
			liquid5_1.B8_LiquidationDate = ZDateTime.Today;
			liquid5_1.B8_SystemCreateDate = new ZDateTime(2012, 07, 11);
			recon5.Liquidations.Add(liquid5_1);
			var liquid5_2 = Factory.New<CusLiquidation>();
			liquid5_2.B8_LiquidationDate = ZDateTime.Today.AddDays(-2);
			liquid5_2.B8_SystemCreateDate = ZDateTime.Today.AddDays(-2);
			recon5.Liquidations.Add(liquid5_2);
			var recon6 = GetReconDeclaration("B00160502", "00000030", "");
			var liquid6 = Factory.New<CusLiquidation>();
			liquid6.B8_LiquidationDate = ZDateTime.Today;
			liquid6.B8_SystemCreateDate = ZDateTime.Empty;
			recon6.Liquidations.Add(liquid6);
			Factory.Save();
			var bizObj = new ReconFilterStripBusinessObject();
			var liquidationDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.LiquidationDate];
			liquidationDate.IsActive = true;
			liquidationDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			liquidationDate.Property2 = ZDateTime.BrettsBirthday; //to-date
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			Assert("declaration1 should not be there", !coll.Contains(declaration1.PK));
			Assert("recon1 should not be there", !coll.Contains(recon1.JE_PK));
			Assert("recon2 should not be there", !coll.Contains(recon2.JE_PK));
			Assert("recon3 should not be there", !coll.Contains(recon3.JE_PK));
			Assert("recon4 should not be there", !coll.Contains(recon4.JE_PK));
			Assert("recon5 should not be there", !coll.Contains(recon5.JE_PK));
			liquidationDate.Property1 = ZDateTime.BrettsBirthday;
			liquidationDate.Property2 = ZDateTime.Today.AddDays(-2);
			coll.Load(bizObj.Filter);
			Assert("declaration1 should not be there", !coll.Contains(declaration1.PK));
			Assert("recon1 should not be there", !coll.Contains(recon1.JE_PK));
			Assert("recon2 should be there", coll.Contains(recon2.JE_PK));
			Assert("recon3 should not be there", !coll.Contains(recon3.JE_PK));
			Assert("recon4 should not be there", !coll.Contains(recon4.JE_PK));
			Assert("recon5 should be there", coll.Contains(recon5.JE_PK));
			liquidationDate.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			coll.RemoveAll();
			coll.Load(bizObj.Filter);
			Assert("declaration1 should not be there", !coll.Contains(declaration1.PK));
			Assert("recon1 should not be there", !coll.Contains(recon1.JE_PK));
			Assert("recon2 should not be there", !coll.Contains(recon2.JE_PK));
			Assert("recon3 should be there", coll.Contains(recon3.JE_PK));
			Assert("recon4 should be there", coll.Contains(recon4.JE_PK));
			Assert("recon5 should not be there", !coll.Contains(recon5.JE_PK));
			Assert("recon6 should not be there", !coll.Contains(recon6.JE_PK));
			liquidationDate.PropertySearch = ModuleDateFilter.HasDateEntered;
			coll.RemoveAll();
			coll.Load(bizObj.Filter);
			Assert("declaration1 should not be there", !coll.Contains(declaration1.PK));
			Assert("recon1 should be there", coll.Contains(recon1.JE_PK));
			Assert("recon2 should be there", coll.Contains(recon2.JE_PK));
			Assert("recon3 should not be there", !coll.Contains(recon3.JE_PK));
			Assert("recon4 should not be there", !coll.Contains(recon4.JE_PK));
			Assert("recon5 should be there", coll.Contains(recon5.JE_PK));
			Assert("recon6 should be there", coll.Contains(recon6.JE_PK));
			liquidationDate.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last7Days;
			coll.RemoveAll();
			coll.Load(bizObj.Filter);
			Assert("declaration1 should not be there", !coll.Contains(declaration1.PK));
			Assert("recon1 should be there", coll.Contains(recon1.JE_PK));
			Assert("recon2 should not be there", !coll.Contains(recon2.JE_PK));
			Assert("recon3 should not be there", !coll.Contains(recon3.JE_PK));
			Assert("recon4 should not be there", !coll.Contains(recon4.JE_PK));
			Assert("recon5 should be there", coll.Contains(recon5.JE_PK));
		}

		public void TestStatementNumberFilter()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_DeclarationReference = "B0098";
			dec1.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec1.US_EntryFilerCode = "XJ5";
			var entry1 = dec1.ActiveEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry1.EntryNumber = "123";
			var recon1 = GetReconDeclaration("B34280", "45678901", "");
			recon1.US_EntryFilerCode = "XJ5";
			var statement1 = Factory.New<CusStatementHeader>();
			statement1.B2_PaymentType = "1";
			statement1.B2_StatementNumber = "1111";
			var line1 = statement1.StatementLines.AddNew();
			line1.B3_EntryNum = entry1.EntryNumber;
			line1.B3_EntryFilerCode = "XJ5";
			line1.B3_Status = StatementLineStatusList.Codes.Active;
			line1.B3_BrokerReference = "B0098";
			var line2_statement1 = statement1.StatementLines.AddNew();
			line2_statement1.B3_EntryNum = "45678901";
			line2_statement1.B3_EntryFilerCode = "XJ5";
			line2_statement1.B3_Status = StatementLineStatusList.Codes.Active;
			line2_statement1.B3_BrokerReference = "B34280";
			var recon2 = GetReconDeclaration("B00160013", "00000012", "");
			recon2.US_EntryFilerCode = "XJ5";
			var statement2 = Factory.New<CusStatementHeader>();
			statement2.B2_PaymentType = "1";
			statement2.B2_StatementNumber = "2222";
			var line2 = statement2.StatementLines.AddNew();
			line2.B3_EntryNum = "00000012";
			line2.B3_EntryFilerCode = "XJ5";
			line2.B3_Status = StatementLineStatusList.Codes.Active;
			line2.B3_BrokerReference = "B00160013";
			var recon3 = GetReconDeclaration("B00160218", "00000026", "");
			recon3.US_EntryFilerCode = "XJ5";
			var statement3 = Factory.New<CusStatementHeader>();
			statement3.B2_PaymentType = "1";
			statement3.B2_StatementNumber = "3333";
			var line3 = statement3.StatementLines.AddNew();
			line3.B3_EntryNum = "00000026";
			line3.B3_EntryFilerCode = "XJ5";
			line3.B3_Status = StatementLineStatusList.Codes.Deleted;
			line3.B3_BrokerReference = "B00160219";
			var recon4 = GetReconDeclaration("B00160219", "00000026", "");
			recon4.US_EntryFilerCode = "XJ5";
			var recon5 = GetReconDeclaration("B00160501", "00000029", "");
			recon5.US_EntryFilerCode = "XJ5";
			var statement5 = Factory.New<CusStatementHeader>();
			statement5.B2_PaymentType = "1";
			statement5.B2_StatementNumber = "5555";
			var line5 = statement5.StatementLines.AddNew();
			line5.B3_EntryNum = "00000029";
			line5.B3_EntryFilerCode = "XJ5";
			line5.B3_Status = StatementLineStatusList.Codes.Active;
			line5.B3_BrokerReference = "B00160501";
			var monthlyStatement = Factory.New<CusStatementHeader>();
			monthlyStatement.B2_StatementNumber = "1234P0015";
			monthlyStatement.B2_Status = StatementHeaderStatusList.Codes.Final;
			statement5.B2_B2_PeriodicStatement = monthlyStatement.PK;
			Factory.Save();
			var bizObj = new ReconFilterStripBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.StatementNo];
			filter.IsActive = true;
			filter.Property = "1111";
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			Assert(!coll.Contains(dec1.PK));
			Assert(coll.Contains(recon1.JE_PK));
			Assert(!coll.Contains(recon2.JE_PK));
			Assert(!coll.Contains(recon3.JE_PK));
			Assert(!coll.Contains(recon4.JE_PK));
			Assert(!coll.Contains(recon5.JE_PK));
			filter.Property = "2222";
			coll.Load(bizObj.Filter);
			Assert(!coll.Contains(dec1.PK));
			Assert(!coll.Contains(recon1.JE_PK));
			Assert(coll.Contains(recon2.JE_PK));
			Assert(!coll.Contains(recon3.JE_PK));
			Assert(!coll.Contains(recon4.JE_PK));
			Assert(!coll.Contains(recon5.JE_PK));
			filter.Property = "3333";
			coll.Load(bizObj.Filter);
			Assert(!coll.Contains(dec1.PK));
			Assert(!coll.Contains(recon1.JE_PK));
			Assert(!coll.Contains(recon2.JE_PK));
			Assert(!coll.Contains(recon3.JE_PK));
			Assert(!coll.Contains(recon4.JE_PK));
			Assert(!coll.Contains(recon5.JE_PK));
			filter.Property = "5555";
			coll.Load(bizObj.Filter);
			Assert(!coll.Contains(dec1.PK));
			Assert(!coll.Contains(recon1.JE_PK));
			Assert(!coll.Contains(recon2.JE_PK));
			Assert(!coll.Contains(recon3.JE_PK));
			Assert(!coll.Contains(recon4.JE_PK));
			Assert(coll.Contains(recon5.JE_PK));
			filter.Property = "1234P0015";
			coll.Load(bizObj.Filter);
			Assert(!coll.Contains(dec1.PK));
			Assert(!coll.Contains(recon1.JE_PK));
			Assert(!coll.Contains(recon2.JE_PK));
			Assert(!coll.Contains(recon3.JE_PK));
			Assert(!coll.Contains(recon4.JE_PK));
			Assert(!coll.Contains(recon5.JE_PK));
			filter.Clear();
			coll.Load(bizObj.Filter);
			Assert(!coll.Contains(dec1.PK));
			Assert(coll.Contains(recon1.JE_PK));
			Assert(coll.Contains(recon2.JE_PK));
			Assert(coll.Contains(recon3.JE_PK));
			Assert(coll.Contains(recon4.JE_PK));
			Assert(coll.Contains(recon5.JE_PK));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			coll.Load(bizObj.Filter);
			Assert(!coll.Contains(dec1.PK));
			Assert(!coll.Contains(recon1.JE_PK));
			Assert(!coll.Contains(recon2.JE_PK));
			Assert(coll.Contains(recon3.JE_PK));
			Assert(coll.Contains(recon4.JE_PK));
			Assert(!coll.Contains(recon5.JE_PK));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			coll.Load(bizObj.Filter);
			Assert(!coll.Contains(dec1.PK));
			Assert(coll.Contains(recon1.JE_PK));
			Assert(coll.Contains(recon2.JE_PK));
			Assert(!coll.Contains(recon3.JE_PK));
			Assert(!coll.Contains(recon4.JE_PK));
			Assert(coll.Contains(recon5.JE_PK));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "2";
			coll.Load(bizObj.Filter);
			Assert(!coll.Contains(dec1.PK));
			Assert(!coll.Contains(recon1.JE_PK));
			Assert(coll.Contains(recon2.JE_PK));
			Assert(!coll.Contains(recon3.JE_PK));
			Assert(!coll.Contains(recon4.JE_PK));
			Assert(!coll.Contains(recon5.JE_PK));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			filter.Property = "3";
			coll.Load(bizObj.Filter);
			Assert(!coll.Contains(dec1.PK));
			Assert(!coll.Contains(recon1.JE_PK));
			Assert(!coll.Contains(recon2.JE_PK));
			Assert(!coll.Contains(recon3.JE_PK));
			Assert(!coll.Contains(recon4.JE_PK));
			Assert(!coll.Contains(recon5.JE_PK));
		}

		public void TestEntrySummaryActionsCompleted()
		{
			var recon1 = GetReconDeclaration("B34280", "12345678901", "");
			var recon2 = GetReconDeclaration("B34281", "12345678902", "");

			var message1 = Factory.New<MQEDIMessage>();
			message1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message1.EM_ApplicationReference = ENSStatusDispositionCodeList._1 + ":1234567";
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.SetToComplete();
			recon1.ReconEntry.Messages.Add(message1);

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message2.EM_ApplicationReference = ENSStatusDispositionCodeList._3 + ":1234567";
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			recon2.ReconEntry.Messages.Add(message2);

			recon1.ReconWrappedJobDeclaration.ReCalculateENSAction();
			recon2.ReconWrappedJobDeclaration.ReCalculateENSAction();

			Factory.Save();

			var bizObj = new ReconFilterStripBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.EntrySummaryActions];
			filter.IsActive = true;
			filter.Property = DeclarationFilterConstants.Incomplete;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			Assert("Dec1", !coll.Contains(recon1.PK));
			AssertNotEquals(EM_ActionStatusList.Codes.Incomplete, message1.EM_ActionStatus);
			Assert("Dec2", coll.Contains(recon2.PK));
			AssertEquals("Incomplete", EM_ActionStatusList.Codes.Incomplete, message2.EM_ActionStatus);
			filter.Property = DeclarationFilterConstants.ALL; // do not exclude. include all!
			coll.RemoveAll();
			coll.Load(bizObj.Filter);
			Assert("Dec1", coll.Contains(recon1.PK));
			Assert("Dec2", coll.Contains(recon2.PK));
		}

		public void TestAnticipatedLiquidationDateFilterForReconJob()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var entry1 = declaration1.ActiveEntryHeaders.AddNew();
			entry1.US_ALDate = ZDateTime.Today.AddDays(-1);
			entry1.CH_MessageType = "REC";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var entry2 = declaration2.ActiveEntryHeaders.AddNew();
			entry2.CH_MessageType = "REC";
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var entry3 = declaration3.ActiveEntryHeaders.AddNew();
			entry3.US_ALDate = ZDateTime.Today.AddDays(-45);
			entry3.CH_MessageType = "REC";
			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Recon;
			var entry4 = declaration4.ActiveEntryHeaders.AddNew();
			entry4.CH_MessageType = "REC";
			DeleteAllGenAddOnColumn(entry1);
			DeleteAllGenAddOnColumn(entry2);
			DeleteAllGenAddOnColumn(entry3);
			DeleteAllGenAddOnColumn(entry4);
			Factory.Save();
			var bizObj = new ReconFilterStripBusinessObject();
			var liqDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.AnticipLiquidationDate];
			liqDate.IsActive = true;
			liqDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			liqDate.Property2 = ZDateTime.Today;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));
			liqDate.Property2 = ZDateTime.Empty;
			liqDate.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last7Days;
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be shown", true, coll.Contains(declaration1));
			liqDate.PropertySearch = ModuleDateFilter.HasDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));
			liqDate.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration4));
		}

		public void TestCustomFieldFilter()
		{
			var declarationTemplate = CreateWorkflowTemplate(WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode);
			AddCustomField(declarationTemplate, "stringField", AddOnColumnDataType.Codes.String);
			AddCustomField(declarationTemplate, "intField", AddOnColumnDataType.Codes.Integer);
			AddCustomField(declarationTemplate, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			AddCustomField(declarationTemplate, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			var filterCollection = new ReconFilterStripBusinessObject().ModuleFilters;
			AssertNull(filterCollection["stringField"]);
			AssertNull(filterCollection["intField"]);
			AssertNull(filterCollection["dateTimeField"]);
			AssertNull(filterCollection["boolField"]);
			var reconTemplate = CreateWorkflowTemplate(WorkflowDescriptors.ReconWorkflowDescriptorCode);
			AddCustomField(reconTemplate, "stringField", AddOnColumnDataType.Codes.String);
			AddCustomField(reconTemplate, "intField", AddOnColumnDataType.Codes.Integer);
			AddCustomField(reconTemplate, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			AddCustomField(reconTemplate, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();
			filterCollection = new ReconFilterStripBusinessObject().ModuleFilters;
			AssertEquals(typeof(ModuleTextFilter), filterCollection["stringField"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["intField"].GetType());
			AssertEquals(typeof(ModuleDateFilter), filterCollection["dateTimeField"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ReconFilterStripBusinessObject();

		ProcessTaskTemplate CreateWorkflowTemplate(ZString workflowDescriptorCode)
		{
			var result = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			result.P0_ProcessType = workflowDescriptorCode;
			return result;
		}

		GenCustomColumnDefinition AddCustomField(ProcessTaskTemplate template, ZString name, ZString addOnColumnDataTypeCode)
		{
			var result = template.GenCustomColumnDefinitions.AddNew();
			result.XC_Name = name;
			result.XC_Type = addOnColumnDataTypeCode;
			return result;
		}

		ReconDeclaration GetReconDeclaration(ZString jobNumber, ZString entryNumber, ZString customsStatus)
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.ReconWrappedJobDeclaration.JE_DeclarationReference = jobNumber;
			reconDeclaration.ReconWrappedJobDeclaration.JE_EntryStatus = customsStatus;
			if (!entryNumber.IsEmpty)
			{
				reconDeclaration.ReconWrappedJobDeclaration.AllocateEntryNumber(entryNumber);
			}
			reconDeclaration.ReconWrappedJobDeclaration.JE_ClusterKey = clusterKey;
			reconDeclaration.ReconWrappedJobDeclaration.ActiveEntryHeaders.ReconciliationEntry.CH_ClusterKey = clusterKey;
			clusterKey++;

			return reconDeclaration;
		}

		int clusterKey = 1;

		ReconDeclaration GetReconDeclarationWithOriginalEntry(ZString declarationShouldBeReconciledNumber, ZString entryOnReconciliationNumber, ZString reconWrappedDeclarationJobNumber)
		{
			var declarationShouldBeReconciled = Factory.New<JobDeclaration>();
			declarationShouldBeReconciled.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationShouldBeReconciled.JE_DeclarationReference = declarationShouldBeReconciledNumber;
			declarationShouldBeReconciled.US_EntryFilerCode = "XJ5";
			var entryOnReconciliation = declarationShouldBeReconciled.ActiveEntryHeaders.AddNew();
			entryOnReconciliation.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryOnReconciliation.EntryNumber = entryOnReconciliationNumber;
			Factory.Save();
			var reconWrappedDeclaration = Factory.New<JobDeclaration>();
			reconWrappedDeclaration.JE_DeclarationReference = reconWrappedDeclarationJobNumber;
			var reconDeclaration = new ReconDeclaration(reconWrappedDeclaration);
			var reconOriginalEntry = reconWrappedDeclaration.CustomsEntryHeaders.AddNew();
			reconOriginalEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ReconOriginalEntry;
			var reconOriginalEntryFound = reconDeclaration.OriginalEntries[0];
			reconOriginalEntryFound.CH_OrigEntryReference = "XJ5" + entryOnReconciliationNumber;
			Factory.Save();
			return reconDeclaration;
		}

		void DeleteAllGenAddOnColumn(BusinessObject bo)
		{
			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, bo.PK);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, bo.TablePrefix);
			Factory.Load<GenAddOnColumn>(query).DeleteAll();
		}
	}
}
