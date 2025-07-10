using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(EntryLineFilterBusinessObject))]
	sealed class EntryLineFilterBusinessObjectTest : Customs.Module.Testing.EntryLineFilterBusinessObjectTest
	{
		public void TestEntryTypeFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_GB = declaration1.JE_GB;
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			var entryLine1 = entryHeader1.MergedLines.AddNew();
			entryLine1.CL_AdValoremTariff = "12332123";
			declaration1.ImportEntryNumber = "32352123";
			Factory.Save();
			var entryNumberQuery = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.EntryNumber];
			entryNumberQuery.IsActive = true;
			entryNumberQuery.Property = "32352123";
			entryNumberQuery.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_GB = declaration1.JE_GB;
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entryLine2 = entryHeader2.MergedLines.AddNew();
			entryLine2.CL_AdValoremTariff = "12332123";
			declaration2.ImportEntryNumber = "32352123";
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);
			AssertEquals(entryLine2.PK, filterCollection[0].PK);
			entryHeader1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals("Pre-Condition", 2, filterCollection.Count);
			entryLine1.CL_AddInfo = "xxx*SupLine=Y*xxx";
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);
			entryLine2.CL_AddInfo = "SupLine=Y";
			var entryLine3 = entryHeader2.MergedLines.AddNew();
			entryLine3.CL_AdValoremTariff = "12332123";
			entryLine3.CL_AddInfo = "ChildLineNum=1*CL_ParentLine=xxx";
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);
			AssertEquals(entryLine3.PK, filterCollection[0].PK);
			entryLine2.CL_AddInfo = "";
			entryLine3.CL_AddInfo = "ChildLineNum=2*CL_ParentLine=xxx";
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);
			AssertEquals(entryLine2.PK, filterCollection[0].PK);
			entryLine2.CL_AddInfo = "SupLine=Y";
			entryLine3.CL_AddInfo = "CL_ParentLine=xxx";
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);
			AssertEquals(entryLine3.PK, filterCollection[0].PK);
			var entryLine4 = entryHeader2.MergedLines.AddNew();
			entryLine4.CL_AdValoremTariff = "12332123";
			entryLine4.CL_AddInfo = "ChildLineNum=3*CL_ParentLine=xxx";
			var entryLine5 = entryHeader2.MergedLines.AddNew();
			entryLine5.CL_AddInfo = "xxx*SupLine=Y*xxx";
			entryLine2.CL_AdValoremTariff = "";
			entryLine2.CL_AddInfo = "ChildLineNum=1*CL_ParentLine=xxx";
			entryLine3.CL_AddInfo = "ChildLineNum=2*CL_ParentLine=xxx*SupLine=Y";
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(1, filterCollection.Count);
			AssertEquals(entryLine4.PK, filterCollection[0].PK);
		}

		public void TestEntryDateFilter()
		{
			var testCreatedDateFilter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.EntryDate];
			testCreatedDateFilter.IsActive = true;
			testCreatedDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			testCreatedDateFilter.Property1 = ZDateTime.Now.AddDays(-11);
			testCreatedDateFilter.Property2 = ZDateTime.Now.AddDays(-9);
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(2, filterCollection.Count);
			testCreatedDateFilter.Property1 = ZDateTime.Now.AddDays(-13);
			filterCollection.Load(filterBO.Filter);
			AssertEquals(4, filterCollection.Count);
		}

		public void TestGetPortOfEntryQuery()
		{
			Declaration1.US_SchDEntry = "8888";
			Declaration2.US_SchDEntry = "8889";
			var testPortOfEntryFilter = (ModuleNkFilter)filterBO[DeclarationFilterConstants.PortOfEntry];
			testPortOfEntryFilter.IsActive = true;
			testPortOfEntryFilter.Property = "8888";
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(2, filterCollection.Count);
			testPortOfEntryFilter.Property = "8887";
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
		}

		public void TestGetDischargeSchedDKQuery()
		{
			Declaration1.US_SchDArrival = "8888";
			Declaration2.US_SchDArrival = "8889";
			var testPortOfDischargeFilter = (ModuleNkFilter)filterBO[DeclarationFilterConstants.DischargeSchedDK];
			testPortOfDischargeFilter.IsActive = true;
			testPortOfDischargeFilter.Property = "8888";
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(2, filterCollection.Count);
			testPortOfDischargeFilter.Property = "8887";
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
		}

		public void TestGetImporterOfRecordQuery()
		{
			var importerOfRecord = Factory.New<OrgHeader>();
			importerOfRecord.FillWithValidTestData();
			var importerOfRecord2 = Factory.New<OrgHeader>();
			importerOfRecord2.FillWithValidTestData();
			var importerOfRecord3 = Factory.New<OrgHeader>();
			importerOfRecord3.FillWithValidTestData();
			Declaration1.IOROrgPK = importerOfRecord.PK;
			Declaration2.IOROrgPK = importerOfRecord2.PK;
			var testImporterOfRecordFilter = (ModuleGuidFilter)filterBO[DeclarationFilterConstants.ImporterOfRecord];
			testImporterOfRecordFilter.IsActive = true;
			testImporterOfRecordFilter.Property = importerOfRecord.PK;
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(2, filterCollection.Count);
			testImporterOfRecordFilter.Property = importerOfRecord3.PK;
			Factory.Save();
			filterCollection.Load(filterBO.Filter);
			AssertEquals(0, filterCollection.Count);
		}

		public override void TestFirstArrivalDateFilter()
		{
			Assert(true);
		}

		public override void TestFirstArrivalDateFilterAdded()
		{
			AssertNull(filterBO[DeclarationFilterConstants.DateFilterTypes.FirstArrival]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryLineFilterBusinessObject(filterCollection);

		CusEntryHeader entryHeader3;
		CusEntryNumber entryNumber3;
		JobComInvoiceHeader invHeader3;
		CusEntryLine line5;
		JobComInvoiceLine invLine5;
		JobDeclaration Declaration1 => (JobDeclaration)declaration1;

		JobDeclaration Declaration2 => (JobDeclaration)declaration2;

		protected override void SetUp()
		{
			base.SetUp();
			Declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration1.US_EntryDate = ZDateTime.Now.AddDays(-10);
			Declaration1.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration2.US_EntryDate = ZDateTime.Now.AddDays(-12);
			Declaration2.MergeManager.DisablePreSaveMergeRequirementForTesting();
			entryNumber1.Parent = Declaration1;
			entryNumber2.Parent = Declaration2;
			entryHeader1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryNumber1.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryNumber2.CE_EntryType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entryHeader3 = Declaration1.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			entryNumber3 = CusEntryNumber.New(Declaration1, CusEntryHeaderMessageTypeList.Codes.InBond, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			entryNumber3.CE_EntryNum = "ENTRY3";
			invHeader3 = Declaration1.Invoices.AddNew();
			invHeader3.JZ_InvoiceDate = ZDateTime.Now;
			line5 = entryHeader3.MergedLines.AddNew();
			line5.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.Active;
			line5.CL_AdValoremTariff = "1111.11.11";
			line5.CL_LineNumber = 1;
			invLine5 = invHeader3.JobComInvoiceLines.AddNew();
			invLine5.JI_CL = line5.PK;
			invLine5.JI_CC = class2.PK;
		}
	}
}
