using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Protest;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using ProtestClass = Enterprise.Customs.US.Business.Protest.Protest;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(ProtestFilterStripBusinessObject))]
	sealed class ProtestFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = (ProtestFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.DeclarationReference]);
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.Protestant]);
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.TariffActCitation]);
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.PeriodBaseDate]);
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.MessageStatus]);
			AssertNotNull(filter["Job Status"]);
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.AddressTeam]);
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.InternalAdviceNumber]);
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.LeadProtestNumber]);
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.TestSummonsNumber]);
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.SubstituteDistrictPort]);
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.SubstituteFilerCode]);
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.RefundParty]);
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.FurtherReview]);
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.AcceleratedDisposition]);
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.Broker]);
			AssertNotNull(filter[ProtestFilterStripBusinessObject.Schema.Branch]);
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
			var protest1 = new ProtestClass(dec1);
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_GB = currentCompanyBranch2.PK;
			var protest2 = new ProtestClass(dec2);
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_GB = company2Branch1.PK;
			var protest3 = new ProtestClass(dec3);
			Factory.Save();
			var filterObj = GetNewFilterStripBusinessObject();
			Assert("wrapper1 matches filter", protest1.MatchesFilter(filterObj.Filter));
			Assert("wrapper2 matches filter", protest2.MatchesFilter(filterObj.Filter));
			Assert("wrapper3 does not match filter", !protest3.MatchesFilter(filterObj.Filter));
		}

		public void TestGetProtestantQuery()
		{
			var importDec = Factory.NewWithValidTestData<JobDeclaration>();
			importDec.JE_MessageType = "IMP";
			var exportDec = Factory.NewWithValidTestData<JobDeclaration>();
			importDec.JE_MessageType = "EXP";
			var declaration1 = Factory.New<JobDeclaration>();
			var protest1 = new ProtestClass(declaration1);
			var organisation = Factory.New<OrgHeader>();
			organisation.FillWithValidTestData();
			protest1.Protestant.OrganisationPK = organisation.PK;
			importDec.JE_OH_Importer = organisation.PK;
			importDec.JE_OH_Supplier = organisation.PK;
			Factory.Save();
			var filter = new ProtestFilterStripBusinessObject();
			var protestantFilter = (ModuleGuidFilter)filter[ProtestFilterStripBusinessObject.Schema.Protestant];
			protestantFilter.Property = organisation.PK;
			protestantFilter.IsActive = true;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration1, coll[0]);
		}

		public void TestBlankFilterOperators()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var protest1 = new ProtestClass(declaration1);
			protest1.US_P_Assoc514ProtestNo = "1234";
			protest1.US_P_Assoc520PetitionNo = "7896";
			protest1.US_P_ProtestantType = ProtestantTypeList.Codes.ForeignExporterProducer;
			var declaration2 = Factory.New<JobDeclaration>();
			var protest2 = new ProtestClass(declaration2);
			protest2.US_P_FilingDDPP = "3901";
			Factory.Save();
			var filterObject = new ProtestFilterStripBusinessObject();
			//US_P_Assoc514ProtestNo
			var filter = (ModuleTextFilter)filterObject[ProtestFilterStripBusinessObject.Schema.ProtestNumber514];
			filter.IsActive = true;
			filter.Property = "1234";
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration1, coll[0]);
			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration2, coll[0]);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration1, coll[0]);
			//US_P_Assoc520PetitionNo
			filterObject = new ProtestFilterStripBusinessObject();
			filter = (ModuleTextFilter)filterObject[ProtestFilterStripBusinessObject.Schema.ProtestNumber520];
			filter.IsActive = true;
			filter.Property = "7896";
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration1, coll[0]);
			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration2, coll[0]);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration1, coll[0]);
			//US_P_ProtestantType
			filterObject = new ProtestFilterStripBusinessObject();
			filter = (ModuleTextFilter)filterObject[ProtestFilterStripBusinessObject.Schema.ProtestantType];
			filter.IsActive = true;
			filter.Property = ProtestantTypeList.Codes.ForeignExporterProducer;
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration1, coll[0]);
			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration2, coll[0]);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration1, coll[0]);
			//US_P_FilingDDPP
			filterObject = new ProtestFilterStripBusinessObject();
			filter = (ModuleTextFilter)filterObject[ProtestFilterStripBusinessObject.Schema.FilingDistrictPort];
			filter.IsActive = true;
			filter.Property = "3901";
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration2, coll[0]);
			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration1, coll[0]);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration2, coll[0]);
		}

		public void TestProtestNumberQuery()
		{
			var protest1 = new ProtestClass(Factory.New<JobDeclaration>());
			protest1.US_P_CBPAssignedProtestNumber = "1234567890";
			var protest2 = new ProtestClass(Factory.New<JobDeclaration>());
			protest2.US_P_CBPAssignedProtestNumber = "1234567891";
			Factory.Save();
			var filterObject = new ProtestFilterStripBusinessObject();
			var filter = (ModuleNumberFilter)filterObject[ProtestClass.Schema.ProtestNumber];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "1234";
			Assert(protest1.Declaration.MatchesFilter(filterObject.Filter));
			Assert(protest2.Declaration.MatchesFilter(filterObject.Filter));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			Assert(!protest1.Declaration.MatchesFilter(filterObject.Filter));
			Assert(!protest2.Declaration.MatchesFilter(filterObject.Filter));
			filter.Property = "1234567890";
			Assert(protest1.Declaration.MatchesFilter(filterObject.Filter));
			Assert(!protest2.Declaration.MatchesFilter(filterObject.Filter));
		}

		public void TestProtestNumberQueryWithNegativeFilters()
		{
			var protest1 = new ProtestClass(Factory.New<JobDeclaration>());
			protest1.US_P_CBPAssignedProtestNumber = "1234567890";
			var protest2 = new ProtestClass(Factory.New<JobDeclaration>());
			protest2.US_P_CBPAssignedProtestNumber = "2234567891";
			Factory.Save();
			var filterObject = new ProtestFilterStripBusinessObject();
			var filter = (ModuleNumberFilter)filterObject[ProtestClass.Schema.ProtestNumber];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			filter.Property = "90";
			Assert(!protest1.Declaration.MatchesFilter(filterObject.Filter));
			Assert(protest2.Declaration.MatchesFilter(filterObject.Filter));
			filter.Property = "1234567890";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			Assert(!protest1.Declaration.MatchesFilter(filterObject.Filter));
			Assert(protest2.Declaration.MatchesFilter(filterObject.Filter));
			filter.Property = "12345678";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			Assert(!protest1.Declaration.MatchesFilter(filterObject.Filter));
			Assert(protest2.Declaration.MatchesFilter(filterObject.Filter));
		}

		public void TestLinkedEntryNumberFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var protest1 = new ProtestClass(declaration1);
			var entry1 = protest1.LinkedEntries.AddNew();
			entry1.US_LE_EntryNumber = "XJ5000006";
			entry1.US_LE_PortCode = "3905";
			var declaration2 = Factory.New<JobDeclaration>();
			var protest2 = new ProtestClass(declaration2);
			var entry2 = protest2.LinkedEntries.AddNew();
			entry2.US_LE_EntryNumber = "SV9000091";
			entry2.US_LE_PortCode = "3901";
			var declaration3 = Factory.New<JobDeclaration>();
			var protest3 = new ProtestClass(declaration3);
			var entry3 = protest3.LinkedEntries.AddNew();
			entry3.US_LE_EntryNumber = "XJ5000091";
			entry3.US_LE_PortCode = "3905";
			Factory.Save();
			var filterObject = new ProtestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObject[ProtestFilterStripBusinessObject.Schema.LinkedEntryNumber];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = "SV9000091";
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			AssertEquals(declaration2, coll[0]);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "XJ5";
			coll.Load(filterObject.Filter);
			AssertEquals(2, coll.Count);
			Assert(coll.Contains(declaration1));
			Assert(coll.Contains(declaration3));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotStartsWith;
			filter.Property = "XJ5";
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			Assert(coll.Contains(declaration2));
		}

		public void TestJobInvoicingStatusFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var protest1 = new ProtestClass(declaration1);
			var job1 = new JobHeader.Loader(protest1).TryLoadOrCreate();
			AssertNotNull(job1);
			AssertEquals(JobHeaderStatus.Working.Code, job1.JH_Status);
			Factory.Save();
			var filterBo = new ProtestFilterStripBusinessObject();
			var filterCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filter = (ModuleTextFilter)filterBo["Job Status"];
			filter.Property = JobHeaderStatus.Working.Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filterCollection.Load(filterBo.Filter);
			AssertCollectionContains(declaration1, filterCollection);
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filterCollection.Load(filterBo.Filter);
			AssertCollectionNotContains(declaration1, filterCollection);
		}

		public void TestProtestStatusFilters()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var protest1 = new ProtestClass(declaration1);
			protest1.US_P_StatusDate = ZDateTime.Now.AddMonths(-2);
			declaration1.JE_EntryStatus = ProtestStatusCodesList.Codes.A;
			var declaration2 = Factory.New<JobDeclaration>();
			var protest2 = new ProtestClass(declaration2);
			protest2.US_P_StatusDate = ZDateTime.Now.AddDays(-1);
			declaration2.JE_EntryStatus = ProtestStatusCodesList.Codes.O;
			var declaration3 = Factory.New<JobDeclaration>();
			var protest3 = new ProtestClass(declaration3);
			protest3.US_P_StatusDate = ZDateTime.Now.AddDays(-20);
			declaration3.JE_EntryStatus = ProtestStatusCodesList.Codes.P;
			var declaration4 = Factory.New<JobDeclaration>();
			var protest4 = new ProtestClass(declaration4);
			protest4.US_P_StatusDate = ZDateTime.Today;
			declaration4.JE_EntryStatus = ProtestStatusCodesList.Codes.O;
			var declaration5 = Factory.New<JobDeclaration>();
			var protest5 = new ProtestClass(declaration5);
			Factory.Save();
			var filterObject = new ProtestFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObject[ProtestFilterStripBusinessObject.Schema.Status];
			filter.IsActive = true;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			filter.Property = ProtestStatusCodesList.Codes.O;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filterObject.Filter);
			AssertEquals(2, coll.Count);
			Assert(coll.Contains(declaration2));
			Assert(coll.Contains(declaration4));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			Assert(coll.Contains(declaration5));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			coll.Load(filterObject.Filter);
			AssertEquals(4, coll.Count);
			Assert(coll.Contains(declaration1));
			Assert(coll.Contains(declaration2));
			Assert(coll.Contains(declaration3));
			Assert(coll.Contains(declaration4));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			filter.Property = ProtestStatusCodesList.Codes.O;
			coll.Load(filterObject.Filter);
			AssertEquals(3, coll.Count);
			Assert(coll.Contains(declaration1));
			Assert(coll.Contains(declaration3));
			Assert(coll.Contains(declaration5));
			filterObject = new ProtestFilterStripBusinessObject();
			var datefilter = (ModuleDateFilter)filterObject[ProtestFilterStripBusinessObject.Schema.StatusDate];
			datefilter.IsActive = true;
			datefilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			datefilter.Property1 = ZDateTime.Now.AddDays(-21);
			datefilter.Property2 = ZDateTime.Now.AddDays(1);
			coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filterObject.Filter);
			AssertEquals(3, coll.Count);
			Assert(coll.Contains(declaration2));
			Assert(coll.Contains(declaration3));
			Assert(coll.Contains(declaration4));
			datefilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			Assert(coll.Contains(declaration5));
			datefilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			datefilter.Property1 = ZDateTime.Empty;
			datefilter.Property2 = ZDateTime.Empty;
			coll.Load(filterObject.Filter);
			AssertEquals(1, coll.Count);
			Assert(coll.Contains(declaration4));
			datefilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last3Mths;
			coll.Load(filterObject.Filter);
			AssertEquals(4, coll.Count);
			Assert(coll.Contains(declaration1));
			Assert(coll.Contains(declaration2));
			Assert(coll.Contains(declaration3));
			Assert(coll.Contains(declaration4));
			datefilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Last7Days;
			coll.Load(filterObject.Filter);
			AssertEquals(2, coll.Count);
			Assert(coll.Contains(declaration2));
			Assert(coll.Contains(declaration4));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ProtestFilterStripBusinessObject();
	}
}
