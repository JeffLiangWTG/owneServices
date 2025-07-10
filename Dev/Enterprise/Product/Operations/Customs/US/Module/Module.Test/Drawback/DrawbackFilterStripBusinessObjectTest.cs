using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
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
	[TestedType(typeof(DrawbackFilterStripBusinessObject))]
	sealed class DrawbackFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = (DrawbackFilterStripBusinessObject)GetNewFilterStripBusinessObject();
			AssertNotNull(filter["Drawback Claimant"]);
			AssertNotNull(filter["Drawback Status"]);
			AssertNotNull(filter["Nafta Drawback Country Code"]);
			AssertNotNull(filter["Message Status"]);
			AssertNotNull(filter["Owners Reference Number"]);
			AssertNotNull(filter["Estimated Claim Date"]);
			AssertNotNull(filter["Earliest Export Date"]);
			AssertNotNull(filter["Period Covered From Date"]);
			AssertNotNull(filter["Period Covered To Date"]);
			AssertNotNull(filter["License Port"]);
			AssertNotNull(filter["Indicators"]);
			AssertNotNull(filter["Invoice Line Product Code"]);
			AssertNotNull(filter["Invoice #"]);
			AssertNotNull(filter["Total Duties & Fees"]);
			AssertNotNull(filter["Total Outstanding"]);
			AssertNotNull(filter["Total Billed"]);
		}

		public void TestFilerCodeQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_EntryFilerCode = "AB";
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_EntryFilerCode = "ABC";
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.FilerCode];
			filter.IsActive = true;

			filter.Property = "AB";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(declaration2.MatchesFilter(bizObj.Filter));

			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(!declaration1.MatchesFilter(bizObj.Filter));
			Assert(!declaration2.MatchesFilter(bizObj.Filter));
			Assert(declaration3.MatchesFilter(bizObj.Filter));
		}

		public void TestClaimTypeQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_EntryType = "A";
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_EntryType = "AB";
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.ClaimType];
			filter.IsActive = true;

			filter.Property = "A";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(declaration2.MatchesFilter(bizObj.Filter));

			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(!declaration1.MatchesFilter(bizObj.Filter));
			Assert(!declaration2.MatchesFilter(bizObj.Filter));
			Assert(declaration3.MatchesFilter(bizObj.Filter));
		}

		public void TestDrawbackTeamQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_TeamNo = "AB";
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_TeamNo = "ABC";
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.Team];
			filter.IsActive = true;

			filter.Property = "AB";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(declaration2.MatchesFilter(bizObj.Filter));

			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(!declaration1.MatchesFilter(bizObj.Filter));
			Assert(!declaration2.MatchesFilter(bizObj.Filter));
			Assert(declaration3.MatchesFilter(bizObj.Filter));
		}

		public void TestDrawbackNaftaCountryQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_NAFTADrawbackCountry = "A";
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_NAFTADrawbackCountry = "AB";
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.NaftaDrawbackCountry];
			filter.IsActive = true;

			filter.Property = "A";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(declaration2.MatchesFilter(bizObj.Filter));

			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(!declaration1.MatchesFilter(bizObj.Filter));
			Assert(!declaration2.MatchesFilter(bizObj.Filter));
			Assert(declaration3.MatchesFilter(bizObj.Filter));
		}

		public void TestLicensePortQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_PreparerDistrictPort = "AB";
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_PreparerDistrictPort = "ABC";
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleNkFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.LicensePort];
			filter.IsActive = true;

			filter.Property = "AB";
			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.Exact;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(!declaration2.MatchesFilter(bizObj.Filter));

			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.IsBlank;
			Assert(!declaration1.MatchesFilter(bizObj.Filter));
			Assert(!declaration2.MatchesFilter(bizObj.Filter));
			Assert(declaration3.MatchesFilter(bizObj.Filter));
		}

		public void TestClaimPortQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_ClaimPort = "AB";
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_ClaimPort = "ABC";
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.ClaimPort];
			filter.IsActive = true;

			filter.Property = "AB";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(declaration2.MatchesFilter(bizObj.Filter));

			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			Assert(!declaration1.MatchesFilter(bizObj.Filter));
			Assert(!declaration2.MatchesFilter(bizObj.Filter));
			Assert(declaration3.MatchesFilter(bizObj.Filter));
		}

		public void TestExporterSummaryIndQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_ExporterSummaryInd = true;
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_ExporterSummaryInd = false;
			DeleteAllGenAddOnColumn(declaration2);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.Indicators];
			filter.IsActive = true;

			filter.Property0 = true;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(!declaration2.MatchesFilter(bizObj.Filter));

			// if unticked, return all
			filter.Property0 = false;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestPreInspectionIndQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_PreInspectionInd = true;
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_PreInspectionInd = false;
			DeleteAllGenAddOnColumn(declaration2);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.Indicators];
			filter.IsActive = true;

			filter.Property1 = true;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(!declaration2.MatchesFilter(bizObj.Filter));

			// if unticked, return all
			filter.Property1 = false;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestNAFTAClaimIndQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_NAFTAClaimInd = true;
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_NAFTAClaimInd = false;
			DeleteAllGenAddOnColumn(declaration2);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.Indicators];
			filter.IsActive = true;

			filter.Property2 = true;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(!declaration2.MatchesFilter(bizObj.Filter));

			// if unticked, return all
			filter.Property2 = false;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestAcceleratedClaimIndQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_AcceleratedClaimInd = true;
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_AcceleratedClaimInd = false;
			DeleteAllGenAddOnColumn(declaration2);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.Indicators];
			filter.IsActive = true;

			filter.Property3 = true;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(!declaration2.MatchesFilter(bizObj.Filter));

			// if unticked, return all
			filter.Property3 = false;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestWaiverNoticeIndQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_WaiverNoticeInd = true;
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_WaiverNoticeInd = false;
			DeleteAllGenAddOnColumn(declaration2);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.Indicators];
			filter.IsActive = true;

			filter.Property4 = true;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(!declaration2.MatchesFilter(bizObj.Filter));

			// if unticked, return all
			filter.Property4 = false;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestPetroleumClaimIndQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_PetroleumClaimInd = true;
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_PetroleumClaimInd = false;
			DeleteAllGenAddOnColumn(declaration2);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.Indicators];
			filter.IsActive = true;

			filter.Property5 = true;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(!declaration2.MatchesFilter(bizObj.Filter));

			// if unticked, return all
			filter.Property5 = false;
			Assert(declaration1.MatchesFilter(bizObj.Filter));
			Assert(declaration2.MatchesFilter(bizObj.Filter));
		}

		public void TestClaimDateQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_EstimatedEntryDate = new ZDateTime(2013, 8, 30);
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_EstimatedEntryDate = new ZDateTime(2013, 11, 29);
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleDateFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.ClaimDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = new ZDateTime(2013, 8, 30);

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));

			filter.Property2 = new ZDateTime(2013, 11, 30);
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.Property1 = new ZDateTime(2013, 09, 27);
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be found", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 3, coll.Count);
			AssertEquals("declaration3 should now be shown as well", true, coll.Contains(declaration3));

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("One declarations should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be found", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));
		}

		public void TestEarliestExportDateQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_EarliestExportDate = new ZDateTime(2013, 8, 30);
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_EarliestExportDate = new ZDateTime(2013, 11, 29);
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleDateFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.EarliestExportDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = new ZDateTime(2013, 8, 30);

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));

			filter.Property2 = new ZDateTime(2013, 11, 30);
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.Property1 = new ZDateTime(2013, 09, 27);
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be found", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 3, coll.Count);
			AssertEquals("declaration3 should now be shown as well", true, coll.Contains(declaration3));

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("One declarations should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be found", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));
		}

		public void TestCoveredFromDateQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_DRWDatePeriodFrom = new ZDateTime(2013, 8, 30);
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_DRWDatePeriodFrom = new ZDateTime(2013, 11, 29);
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleDateFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.CoveredFromDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = new ZDateTime(2013, 8, 30);

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));

			filter.Property2 = new ZDateTime(2013, 11, 30);
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.Property1 = new ZDateTime(2013, 09, 27);
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be found", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 3, coll.Count);
			AssertEquals("declaration3 should now be shown as well", true, coll.Contains(declaration3));

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("One declarations should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be found", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));
		}

		public void TestCoveredToDateQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_DRWDatePeriodTo = new ZDateTime(2013, 8, 30);
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_DRWDatePeriodTo = new ZDateTime(2013, 11, 29);
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleDateFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.CoveredToDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = new ZDateTime(2013, 8, 30);

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));

			filter.Property2 = new ZDateTime(2013, 11, 30);
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.Property1 = new ZDateTime(2013, 09, 27);
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be found", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 3, coll.Count);
			AssertEquals("declaration3 should now be shown as well", true, coll.Contains(declaration3));

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("One declarations should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be found", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));
		}

		public void TestAnticipLiquidationDateQueryWithoutGenAddOnColumn()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration1.US_ALDate = new ZDateTime(2013, 8, 30);
			DeleteAllGenAddOnColumn(declaration1);

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_ALDate = new ZDateTime(2013, 11, 29);
			DeleteAllGenAddOnColumn(declaration2);

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			DeleteAllGenAddOnColumn(declaration3);

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleDateFilter)bizObj[DrawbackFilterStripBusinessObject.Constants.AnticipLiquidationDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = new ZDateTime(2013, 8, 30);

			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));

			filter.Property2 = new ZDateTime(2013, 11, 30);
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.Property1 = new ZDateTime(2013, 09, 27);
			coll.Load(bizObj.Filter);
			AssertEquals("One declaration should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be found", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = ZDateTime.Empty;
			coll.Load(bizObj.Filter);
			AssertEquals("All declarations should be found", 3, coll.Count);
			AssertEquals("declaration3 should now be shown as well", true, coll.Contains(declaration3));

			filter.PropertySearch = ModuleDateFilter.HasDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("Two declarations should be found", 2, coll.Count);
			AssertEquals("declaration1 should be found", true, coll.Contains(declaration1));
			AssertEquals("declaration2 should be found", true, coll.Contains(declaration2));

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			coll.Load(bizObj.Filter);
			AssertEquals("One declarations should be found", 1, coll.Count);
			AssertEquals("declaration1 should not be found", false, coll.Contains(declaration1));
			AssertEquals("declaration2 should not be found", false, coll.Contains(declaration2));
			AssertEquals("declaration3 should be found", true, coll.Contains(declaration3));
		}

		public void TestGetContractNumberQuery()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var contract1 = dec1.ContractNumbers.AddNew();
			contract1.CY_Data = "CN1";
			Factory.Save();
			var filteredDecs = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filter = new DrawbackFilterStripBusinessObject();
			var contractNumberFilter = (ModuleTextFilter)filter[DrawbackFilterStripBusinessObject.Constants.ContractNumber];
			contractNumberFilter.IsActive = true;
			contractNumberFilter.Property = "2";
			filteredDecs.Load(filter.Filter);
			AssertEquals(0, filteredDecs.Count);
			contractNumberFilter.Property = "CN1";
			filteredDecs.Load(filter.Filter);
			AssertEquals(1, filteredDecs.Count);
			AssertEquals(dec1, filteredDecs[0]);
		}

		public void TestBlankFilters()
		{
			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_ClaimPort = "3901";
			declaration2.US_EntryType = "42";
			declaration2.US_TeamNo = "12";
			declaration2.US_EntryFilerCode = "SC9";
			Factory.Save();
			AssertBlankFilters(DrawbackFilterStripBusinessObject.Constants.ClaimPort);
			AssertBlankFilters(DrawbackFilterStripBusinessObject.Constants.ClaimType);
			AssertBlankFilters(DrawbackFilterStripBusinessObject.Constants.Team);
			AssertBlankFilters(DrawbackFilterStripBusinessObject.Constants.FilerCode);
		}

		public void TestClaimTypeFilterList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "Drawback Provision Codes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "99", "1313(A) - Direct Identification Manufacturing Drawback (Articles made from imported merchandise)", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.DrawbackProvisionCodes, "55", "TFTEA 5062(C) - TFTEA Distilled spirits, wines, or beer which are unmerchantable or do not conform to sample or specifications", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();
			var filter = new DrawbackFilterStripBusinessObject();
			AssertEquals(2, filter.DrawbackSummaryEntryTypeList.Count);
			AssertEquals("55, 99", filter.DrawbackSummaryEntryTypeList.CodesAsString);
		}

		public void TestAnticipatedLiquidationDateFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration2.US_ALDate = new ZDateTime(2019, 07, 10);
			Factory.Save();
			var bizObj = new DrawbackFilterStripBusinessObject();
			var liquidationDate = (ModuleDateFilter)bizObj[DeclarationFilterConstants.AnticipLiquidationDate];
			liquidationDate.IsActive = true;
			liquidationDate.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			liquidationDate.Property1 = new ZDateTime(2019, 07, 01);
			liquidationDate.Property2 = new ZDateTime(2019, 07, 30);
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(bizObj.Filter);
			AssertEquals("declaration1 should not be there", false, collection.Contains(declaration1));
			AssertEquals("declaration2 should be there", true, collection.Contains(declaration2));
			liquidationDate.Property1 = new ZDateTime(2019, 07, 11);
			liquidationDate.Property2 = new ZDateTime(2019, 07, 30);
			collection.Load(bizObj.Filter);
			AssertEquals("declaration1 should not be there", false, collection.Contains(declaration1));
			AssertEquals("declaration2 should not be there", false, collection.Contains(declaration2));
		}

		public void TestDrawbackEntrySummaryActionsCompleted()
		{
			var drawback1 = Factory.New<JobDeclaration>();
			drawback1.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback1.JE_DeclarationReference = "B11111";

			var drawback2 = Factory.New<JobDeclaration>();
			drawback2.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			drawback2.JE_DeclarationReference = "B22222";

			var message1 = Factory.New<MQEDIMessage>();
			message1.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message1.EM_ApplicationReference = ENSStatusDispositionCodeList._1 + ":1234567";
			message1.EM_LinkUniqueID = drawback1.PK;
			message1.EM_LinkTable = drawback1.TableName;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.SetToComplete();
			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.EntrySummaryNotification;
			message2.EM_ApplicationReference = ENSStatusDispositionCodeList._3 + ":1234567";
			message2.EM_LinkUniqueID = drawback2.PK;
			message2.EM_LinkTable = drawback2.TableName;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			drawback2.Messages.Add(message2);

			drawback1.ReCalculateENSAction();
			drawback2.ReCalculateENSAction();

			Factory.Save();

			var bizObj = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleTextFilter)bizObj[DeclarationFilterConstants.EntrySummaryActions];
			filter.IsActive = true;
			filter.Property = DeclarationFilterConstants.Incomplete;
			var coll = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(bizObj.Filter);
			Assert("drawback1", !coll.Contains(drawback1.PK));
			AssertNotEquals(EM_ActionStatusList.Codes.Incomplete, message1.EM_ActionStatus);
			Assert("drawback2", coll.Contains(drawback2.PK));
			AssertEquals("Incomplete", EM_ActionStatusList.Codes.Incomplete, message2.EM_ActionStatus);
			filter.Property = DeclarationFilterConstants.ALL; // do not exclude. include all!
			coll.RemoveAll();
			coll.Load(bizObj.Filter);
			Assert("Drawback1", coll.Contains(drawback1.PK));
			Assert("Drawback2", coll.Contains(drawback2.PK));
		}

		public void TestCustomFieldFilter()
		{
			var declarationTemplate = CreateWorkflowTemplate(WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode);
			AddCustomField(declarationTemplate, "stringField", AddOnColumnDataType.Codes.String);
			AddCustomField(declarationTemplate, "intField", AddOnColumnDataType.Codes.Integer);
			AddCustomField(declarationTemplate, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			AddCustomField(declarationTemplate, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			var filterCollection = new DrawbackFilterStripBusinessObject().ModuleFilters;
			AssertNull(filterCollection["stringField"]);
			AssertNull(filterCollection["intField"]);
			AssertNull(filterCollection["dateTimeField"]);
			AssertNull(filterCollection["boolField"]);
			var drawbackTemplate = CreateWorkflowTemplate(WorkflowDescriptors.DrawBackWorkflowDescriptorCode);
			AddCustomField(drawbackTemplate, "stringField", AddOnColumnDataType.Codes.String);
			AddCustomField(drawbackTemplate, "intField", AddOnColumnDataType.Codes.Integer);
			AddCustomField(drawbackTemplate, "dateTimeField", AddOnColumnDataType.Codes.Datetime);
			AddCustomField(drawbackTemplate, "boolField", AddOnColumnDataType.Codes.Boolean);
			Factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();
			filterCollection = new DrawbackFilterStripBusinessObject().ModuleFilters;
			AssertEquals(typeof(ModuleTextFilter), filterCollection["stringField"].GetType());
			AssertEquals(typeof(ModuleNumberRangeFilter), filterCollection["intField"].GetType());
			AssertEquals(typeof(ModuleDateFilter), filterCollection["dateTimeField"].GetType());
			AssertEquals(typeof(ModuleFlagsFilter), filterCollection["Workflow Flags"].GetType());
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new DrawbackFilterStripBusinessObject();

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

		void DeleteAllGenAddOnColumn(JobDeclaration declaration)
		{
			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, declaration.PK);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, declaration.TablePrefix);
			Factory.Load<GenAddOnColumn>(query).DeleteAll();
		}

		void AssertBlankFilters(ZString filterName)
		{
			var filteredDecs = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new DrawbackFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBO[filterName];
			filter.IsActive = true;
			filter.Property = ZString.Empty;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;
			filteredDecs.Load(filterBO.Filter);
			AssertEquals(1, filteredDecs.Count);
			AssertEquals(declaration1, filteredDecs[0]);
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;
			filteredDecs.Load(filterBO.Filter);
			AssertEquals(1, filteredDecs.Count);
			AssertEquals(declaration2, filteredDecs[0]);
		}

		JobDeclaration declaration1;
		JobDeclaration declaration2;
	}
}
