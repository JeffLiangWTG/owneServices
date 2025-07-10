using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using CusInBondMoveHeader = Enterprise.Customs.US.InBond.Business.CusInBondMoveHeader;

namespace Enterprise.Customs.US.InBond.Module.Testing
{
	[TestedType(typeof(CusInBondHeaderFilterStripBusinessObject))]
	sealed class CusInBondHeaderFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestBillDispostionList()
		{
			CusInBondHeaderFilterStripBusinessObject filter = new CusInBondHeaderFilterStripBusinessObject();
			var list = DispositionCodeListLoader.GetDispositionCodes(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode);
			var listAD = DispositionCodeListLoader.GetDispositionCodes(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSAirDispositionCode).ToArray();
			listAD.ForEach(x => list.AddPairIfNotExist(x.Code, x.Description));
			AssertEquals(list.Count, filter.Lookups.BillDispostionList.Count);
		}

		public void TestFilters()
		{
			CusInBondHeaderFilterStripBusinessObject filter = new CusInBondHeaderFilterStripBusinessObject();
			AssertNotNull(filter[CusInBondHeaderFilterStripBusinessObject.FilterConstants.CarrierSCAC]);
			AssertNotNull(filter[CusInBondHeaderFilterStripBusinessObject.FilterConstants.JobReference]);
			AssertNotNull(filter[CusInBondHeaderFilterStripBusinessObject.FilterConstants.Branch]);
			AssertNotNull(filter[CusInBondHeaderFilterStripBusinessObject.FilterConstants.IssuerCode]);
			AssertNotNull(filter[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondMasterBill]);
			AssertNotNull(filter[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InbondHouseBill]);
		}

		public void TestCompanyFilter()
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK;
			var currentCompanyBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyBranch1.GB_GC = currentCompanyPK;
			var currentCompanyBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyBranch2.GB_GC = currentCompanyPK;
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var company2Branch1 = Factory.NewWithValidTestData<GlbBranch>();
			company2Branch1.GB_GC = company2.PK;
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_GB = currentCompanyBranch1.PK;
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_GB = currentCompanyBranch2.PK;
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_GB = company2Branch1.PK;
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			Assert("header1 matches filter", header1.MatchesFilter(filterObj.Filter));
			Assert("header2 matches filter", header2.MatchesFilter(filterObj.Filter));
			Assert("header3 does not match filter", !header3.MatchesFilter(filterObj.Filter));
		}

		public void TestBranchFilter()
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK;
			var currentCompanyBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyBranch1.GB_GC = currentCompanyPK;
			var currentCompanyBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyBranch2.GB_GC = currentCompanyPK;
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_GB = currentCompanyBranch1.PK;
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_GB = currentCompanyBranch2.PK;
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var branchFilter = (ModuleGuidFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.Branch];
			branchFilter.IsActive = true;
			branchFilter.Property = currentCompanyBranch1.PK;
			Assert("header1 matches filter", header1.MatchesFilter(filterObj.Filter));
			Assert("header2 does not matches filter", !header2.MatchesFilter(filterObj.Filter));
		}

		public void TestInBondNumberFilter()
		{
			CusInBondHeader header1 = Factory.New<CusInBondHeader>();
			header1.BH_JobReference = "INB3001001";
			CusInBondMoveHeader moveHeader1 = header1.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "365987412";
			CusInBondMoveHeader moveHeader2 = header1.MovementHeaders.AddNew();
			moveHeader2.InBondNumber = "968534212";
			CusInBondHeader header2 = Factory.New<CusInBondHeader>();
			header2.BH_JobReference = "INB3001002";
			CusInBondMoveHeader moveHeader3 = header2.MovementHeaders.AddNew();
			moveHeader3.InBondNumber = "968745321";
			CusInBondMoveHeader moveHeader4 = header2.MovementHeaders.AddNew();
			moveHeader4.InBondNumber = "365988745";
			Factory.Save();
			CusInBondHeaderFilterStripBusinessObject filterObj = new CusInBondHeaderFilterStripBusinessObject();
			ModuleTextFilter textFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondNumber];
			textFilter.IsActive = true;
			textFilter.Property = "36598";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(filterObj.Filter));
			textFilter.Property = "365987";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(filterObj.Filter));
			textFilter.Property = "968745321";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(filterObj.Filter));
			textFilter.Property = "9685,9687";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(filterObj.Filter));
		}

		public void TestInBondNumberFilterForIsBlankQuery()
		{
			CusInBondHeader header1 = Factory.New<CusInBondHeader>(); // two movements 1 with Inbond number, 1 without
			header1.BH_JobReference = "INB3001001";
			CusInBondMoveHeader moveHeader1 = header1.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "365987412";
			CusInBondMoveHeader moveHeader2 = header1.MovementHeaders.AddNew();
			CusInBondHeader header2 = Factory.New<CusInBondHeader>(); // Has no inbond number
			header2.BH_JobReference = "INB3001002";
			CusInBondHeader header3 = Factory.New<CusInBondHeader>(); // Has no inbond number
			CusInBondMoveHeader moveHeader3 = header3.MovementHeaders.AddNew();
			CusInBondHeader header4 = Factory.New<CusInBondHeader>(); // Has inbond number
			CusInBondMoveHeader moveHeader4 = header4.MovementHeaders.AddNew();
			moveHeader4.InBondNumber = "365988745";
			CusInBondHeader header5 = Factory.New<CusInBondHeader>(); // Has no inbond number
			CusInBondMoveHeader moveHeader5 = header5.MovementHeaders.AddNew();
			CusInBondHeader header6 = Factory.New<CusInBondHeader>(); // two movements both with inbond numbers
			CusInBondMoveHeader moveHeader6 = header6.MovementHeaders.AddNew();
			moveHeader6.InBondNumber = "365987429";
			CusInBondMoveHeader moveHeader7 = header6.MovementHeaders.AddNew();
			moveHeader7.InBondNumber = "365987442";
			Factory.Save();
			CusInBondHeaderFilterStripBusinessObject bizObj = new CusInBondHeaderFilterStripBusinessObject();
			ModuleTextFilter inBondNumberFilter = (ModuleTextFilter)bizObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondNumber];
			inBondNumberFilter.IsActive = true;
			AssertEquals("Should have found all created test Inbond Movements", true, header1.MatchesFilter(bizObj.Filter));
			AssertEquals("Should have found all created test Inbond Movements", true, header2.MatchesFilter(bizObj.Filter));
			AssertEquals("Should have found all created test Inbond Movements", true, header3.MatchesFilter(bizObj.Filter));
			AssertEquals("Should have found all created test Inbond Movements", true, header4.MatchesFilter(bizObj.Filter));
			AssertEquals("Should have found all created test Inbond Movements", true, header5.MatchesFilter(bizObj.Filter));
			AssertEquals("Should have found all created test Inbond Movements", true, header6.MatchesFilter(bizObj.Filter));
			inBondNumberFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			AssertEquals("Filtering for Blank Inbond Number should find Inbond (header1) as it has 1 movement with no Inbond Number.", true, header1.MatchesFilter(bizObj.Filter));
			AssertEquals("Filtering for Blank Inbond Number should find Inbond (header2) as it has no movement therefore no Inbond Number.", true, header2.MatchesFilter(bizObj.Filter));
			AssertEquals("Filtering for Blank Inbond Number should find Inbond (header3).", true, header3.MatchesFilter(bizObj.Filter));
			AssertEquals("Filtering for Blank Inbond Number should NOT find Inbond (header4).", false, header4.MatchesFilter(bizObj.Filter));
			AssertEquals("Filtering for Blank Inbond Number should find Inbond (header5).", true, header5.MatchesFilter(bizObj.Filter));
			AssertEquals("Filtering for Blank Inbond Number should NOT find Inbond (header6).", false, header6.MatchesFilter(bizObj.Filter));
			inBondNumberFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			AssertEquals("Filtering for Not Blank Inbond Number should still find Inbond (header1) as it has 1 movement with Inbond Number.", true, header1.MatchesFilter(bizObj.Filter));
			AssertEquals("Filtering for Not Blank Inbond Number should NOT find Inbond (header2).", false, header2.MatchesFilter(bizObj.Filter));
			AssertEquals("Filtering for Not Blank Inbond Number should NOT find Inbond (header3).", false, header3.MatchesFilter(bizObj.Filter));
			AssertEquals("Filtering for Not Blank Inbond Number should find Inbond (header4).", true, header4.MatchesFilter(bizObj.Filter));
			AssertEquals("Filtering for Not Blank Inbond Number should NOT find Inbond (header5).", false, header5.MatchesFilter(bizObj.Filter));
			AssertEquals("Filtering for Not Blank Inbond Number should find Inbond (header6).", true, header6.MatchesFilter(bizObj.Filter));
		}

		public void TestJobReferenceFilter()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_JobReference = "INB3001001";
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_JobReference = "INB3001002";
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var jobReferenceFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.JobReference];
			jobReferenceFilter.IsActive = true;
			jobReferenceFilter.Property = "INB3001001";
			AssertEquals("Filtering for Job Reference INB3001001 should find Inbond (header1).", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference INB3001001 should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			jobReferenceFilter.Property = "INB3001002";
			AssertEquals("Filtering for Job Reference INB3001002 should NOT find Inbond (header1).", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference INB3001002 should find Inbond (header2).", true, header2.MatchesFilter(filterObj.Filter));
			jobReferenceFilter.Property = "";
			AssertEquals("Filtering for Job Reference INB3001001 should find Inbond (header1).", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference INB3001002 should find Inbond (header2).", true, header2.MatchesFilter(filterObj.Filter));
			jobReferenceFilter.Property = "INB3001001,INB3001002";
			AssertEquals("Filtering for Job Reference INB3001001 should find Inbond (header1).", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Job Reference INB3001002 should find Inbond (header2).", true, header2.MatchesFilter(filterObj.Filter));
		}

		public void TestInBondEntryTypeFilter()
		{
			CusInBondHeader header1 = Factory.New<CusInBondHeader>(); // two movements with mixed Entry Types (61 & 62)
			header1.BH_JobReference = "INB3001001";
			CusInBondMoveHeader moveHeader1 = header1.MovementHeaders.AddNew();
			moveHeader1.InBondNumber = "365987412";
			moveHeader1.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			CusInBondMoveHeader moveHeader2 = header1.MovementHeaders.AddNew();
			moveHeader2.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			CusInBondHeader header2 = Factory.New<CusInBondHeader>(); // Has no Entry Type
			header2.BH_JobReference = "INB3001002";
			CusInBondHeader header3 = Factory.New<CusInBondHeader>(); // Entry type 62
			CusInBondMoveHeader moveHeader3 = header3.MovementHeaders.AddNew();
			moveHeader3.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			CusInBondHeader header4 = Factory.New<CusInBondHeader>(); // Entry type 62
			CusInBondMoveHeader moveHeader4 = header4.MovementHeaders.AddNew();
			moveHeader4.InBondNumber = "365988745";
			moveHeader4.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			CusInBondHeader header5 = Factory.New<CusInBondHeader>(); // Entry type 63
			CusInBondMoveHeader moveHeader5 = header5.MovementHeaders.AddNew();
			moveHeader5.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			CusInBondHeader header6 = Factory.New<CusInBondHeader>(); // two movements with Same Entry Type (61)
			CusInBondMoveHeader moveHeader6 = header6.MovementHeaders.AddNew();
			moveHeader6.InBondNumber = "365987429";
			moveHeader6.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			CusInBondMoveHeader moveHeader7 = header6.MovementHeaders.AddNew();
			moveHeader7.InBondNumber = "365987442";
			moveHeader7.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			Factory.Save();
			CusInBondHeaderFilterStripBusinessObject filterObj = new CusInBondHeaderFilterStripBusinessObject();
			ModuleTextFilter entryTypeFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.ITType];
			entryTypeFilter.IsActive = true;
			entryTypeFilter.Property = "";
			AssertEquals("Should have found all created test Inbond Movements", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbond Movements", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbond Movements", true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbond Movements", true, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbond Movements", true, header5.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbond Movements", true, header6.MatchesFilter(filterObj.Filter));
			entryTypeFilter.Property = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertEquals("Filtering for Inbond Entry Type 61 should find Inbond (header1) as it has 1 movement with Type 61.", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Inbond Entry Type 61 should NOT find Inbond (header2) as it has no movement therefore no Entry Type.", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Inbond Entry Type 61 should NOT find Inbond (header3).", false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Inbond Entry Type 61 should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Inbond Entry Type 61 should NOT find Inbond (header5).", false, header5.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Inbond Entry Type 61 should find Inbond (header6).", true, header6.MatchesFilter(filterObj.Filter));
			entryTypeFilter.Property = InbondCommonTypeList.Codes._2TransportandExport;
			AssertEquals("Filtering for Not Blank Inbond Number should still find Inbond (header1) as it has 1 movement with Type 62.", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Not Blank Inbond Number should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Not Blank Inbond Number should find Inbond (header3).", true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Not Blank Inbond Number should find Inbond (header4).", true, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Not Blank Inbond Number should NOT find Inbond (header5).", false, header5.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Not Blank Inbond Number should find Inbond (header6).", false, header6.MatchesFilter(filterObj.Filter));
			entryTypeFilter.Property = InbondCommonTypeList.Codes._3ImmediateExport;
			AssertEquals("Filtering for Not Blank Inbond Number should NOT find Inbond (header1).", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Not Blank Inbond Number should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Not Blank Inbond Number should NOT find Inbond (header3).", false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Not Blank Inbond Number should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Not Blank Inbond Number should find Inbond (header5).", true, header5.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Not Blank Inbond Number should NOT find Inbond (header6).", false, header6.MatchesFilter(filterObj.Filter));
		}

		public void TestTransportModeFilter()
		{
			CusInBondHeader header1 = Factory.New<CusInBondHeader>();
			header1.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			CusInBondHeader header2 = Factory.New<CusInBondHeader>();
			header2.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			CusInBondHeader header3 = Factory.New<CusInBondHeader>();
			header3.BH_ImportTransportMode = TransportModeCodes.Codes.VesselNonContainer;
			CusInBondHeader header4 = Factory.New<CusInBondHeader>();
			header4.BH_ImportTransportMode = TransportModeCodes.Codes.TruckNonContainer;
			CusInBondHeader header5 = Factory.New<CusInBondHeader>();
			header5.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			CusInBondHeader header6 = Factory.New<CusInBondHeader>();
			header6.BH_ImportTransportMode = TransportModeCodes.Codes.RailNonContainer;
			CusInBondHeader header7 = Factory.New<CusInBondHeader>();
			header7.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			Factory.Save();
			CusInBondHeaderFilterStripBusinessObject filterObj = new CusInBondHeaderFilterStripBusinessObject();
			ModuleTextFilter transportModeFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.TransportMode];
			transportModeFilter.IsActive = true;
			transportModeFilter.Property = "";
			AssertEquals("Should have found all created test Inbonds", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header5.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header6.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header7.MatchesFilter(filterObj.Filter));
			transportModeFilter.Property = TransportModeCodes.Codes.VesselContainer;
			AssertEquals("Filtering for Transport Mode 11 should find Inbond (header1).", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 11 should find Inbond (header2).", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 11 should NOT find Inbond (header3).", false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 11 should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 11 should find Inbond (header5).", true, header5.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 11 hould NOT find Inbond (header6).", false, header6.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 11 hould NOT find Inbond (header7).", false, header7.MatchesFilter(filterObj.Filter));
			transportModeFilter.Property = TransportModeCodes.Codes.VesselNonContainer;
			AssertEquals("Filtering for Transport Mode 10 should NOT find Inbond (header1).", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 10 should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 10 should find Inbond (header3).", true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 10 should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 10 should NOT find Inbond (header5).", false, header5.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 10 hould NOT find Inbond (header6).", false, header6.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 10 hould NOT find Inbond (header7).", false, header7.MatchesFilter(filterObj.Filter));
			transportModeFilter.Property = TransportModeCodes.Codes.AirNonContainer;
			AssertEquals("Filtering for Transport Mode 40 should NOT find Inbond (header1).", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 40 should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 40 should NOT find Inbond (header3).", false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 40 should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 40 should NOT find Inbond (header5).", false, header5.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 40 hould NOT find Inbond (header6).", false, header6.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 40 hould find Inbond (header7).", true, header7.MatchesFilter(filterObj.Filter));
			transportModeFilter.Property = TransportModeCodes.Codes.TruckNonContainer;
			AssertEquals("Filtering for Transport Mode 30 should NOT find Inbond (header1).", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 30 should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 30 should NOT find Inbond (header3).", false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 30 should find Inbond (header4).", true, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 30 should NOT find Inbond (header5).", false, header5.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 30 hould NOT find Inbond (header6).", false, header6.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 30 hould NOT find Inbond (header7).", false, header7.MatchesFilter(filterObj.Filter));
			transportModeFilter.Property = TransportModeCodes.Codes.RailNonContainer;
			AssertEquals("Filtering for Transport Mode 20 should NOT find Inbond (header1).", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 20 should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 20 should NOT find Inbond (header3).", false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 20 should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 20 should NOT find Inbond (header5).", false, header5.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 20 should find Inbond (header6).", true, header6.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Transport Mode 20 should NOT find Inbond (header7).", false, header7.MatchesFilter(filterObj.Filter));
		}

		public void TestETAFilter()
		{
			CusInBondHeader header1 = Factory.New<CusInBondHeader>();
			header1.BH_ETA = ZDateTime.Today.AddDays(-20);
			CusInBondHeader header2 = Factory.New<CusInBondHeader>();
			header2.BH_ETA = ZDateTime.Today.AddDays(-10);
			CusInBondHeader header3 = Factory.New<CusInBondHeader>();
			header3.BH_ETA = ZDateTime.Today;
			Factory.Save();
			CusInBondHeaderFilterStripBusinessObject filterObj = new CusInBondHeaderFilterStripBusinessObject();
			ModuleDateFilter etaFilter = (ModuleDateFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.ETA];
			etaFilter.IsActive = true;
			etaFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			etaFilter.Property2 = ZDateTime.Today.AddDays(-30); //to-date
			AssertEquals("Should NOT have found all created test Inbonds", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should NOT have found all created test Inbonds", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should NOT have found all created test Inbonds", false, header3.MatchesFilter(filterObj.Filter));
			etaFilter.Property1 = ZDateTime.Today.AddDays(-20);
			etaFilter.Property2 = ZDateTime.Today; //to-date
			AssertEquals("Should have found all created test Inbonds", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header3.MatchesFilter(filterObj.Filter));
			etaFilter.Property1 = ZDateTime.Today.AddDays(-12);
			etaFilter.Property2 = ZDateTime.Today.AddDays(-5); //to-date
			AssertEquals("Should NOT have found all created test Inbonds", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should NOT have found all created test Inbonds", false, header3.MatchesFilter(filterObj.Filter));
		}

		public void TestLoadingPortFilter()
		{
			CusInBondHeader header1 = Factory.New<CusInBondHeader>();
			header1.BH_ImportLoadPortKCode = "60243";
			CusInBondHeader header2 = Factory.New<CusInBondHeader>();
			header2.BH_ImportLoadPortKCode = "52120";
			CusInBondHeader header3 = Factory.New<CusInBondHeader>();
			header3.BH_ImportLoadPortKCode = "60243";
			CusInBondHeader header4 = Factory.New<CusInBondHeader>();
			header4.BH_ImportLoadPortKCode = "79325";
			CusInBondHeader header5 = Factory.New<CusInBondHeader>();
			header5.BH_ImportLoadPortKCode = "22590";
			Factory.Save();
			CusInBondHeaderFilterStripBusinessObject filterObj = new CusInBondHeaderFilterStripBusinessObject();
			ModuleTextFilter loadingPortFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.LoadingScheduleK];
			loadingPortFilter.IsActive = true;
			loadingPortFilter.Property = "";
			AssertEquals("Should have found all created test Inbonds", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header5.MatchesFilter(filterObj.Filter));
			loadingPortFilter.Property = "60243";
			AssertEquals("Filtering for Loading Port 60243 should find Inbond (header1).", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 60243 should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 60243 should find Inbond (header3).", true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 60243 should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 60243 should NOT find Inbond (header5).", false, header5.MatchesFilter(filterObj.Filter));
			loadingPortFilter.Property = "52120";
			AssertEquals("Filtering for Loading Port 52120 should NOT find Inbond (header1).", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 52120 should find Inbond (header2).", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 52120 should NOT find Inbond (header3).", false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 52120 should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 52120 should NOT find Inbond (header5).", false, header5.MatchesFilter(filterObj.Filter));
			loadingPortFilter.Property = "22590";
			AssertEquals("Filtering for Loading Port 22590 should NOT find Inbond (header1).", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 22590 should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 22590 should NOT find Inbond (header3).", false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 22590 should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 22590 should find Inbond (header5).", true, header5.MatchesFilter(filterObj.Filter));
			loadingPortFilter.Property = "60,22";
			AssertEquals("Filtering for Loading Port 60,22 should find Inbond (header1).", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 60,22 should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 60,22 should find Inbond (header3).", true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 60,22 should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Loading Port 60,22 should find Inbond (header5).", true, header5.MatchesFilter(filterObj.Filter));
		}

		public void TestArrivalPortFilter()
		{
			CusInBondHeader header1 = Factory.New<CusInBondHeader>();
			header1.BH_PortUnladingDCode = "8888";
			CusInBondHeader header2 = Factory.New<CusInBondHeader>();
			header2.BH_PortUnladingDCode = "2704";
			CusInBondHeader header3 = Factory.New<CusInBondHeader>();
			header3.BH_PortUnladingDCode = "8888";
			CusInBondHeader header4 = Factory.New<CusInBondHeader>();
			header4.BH_PortUnladingDCode = "2724";
			CusInBondHeader header5 = Factory.New<CusInBondHeader>();
			header5.BH_PortUnladingDCode = "1001";
			Factory.Save();
			CusInBondHeaderFilterStripBusinessObject filterObj = new CusInBondHeaderFilterStripBusinessObject();
			ModuleTextFilter arrivalPortFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.ArrivalScheduleD];
			arrivalPortFilter.IsActive = true;
			arrivalPortFilter.Property = "";
			AssertEquals("Should have found all created test Inbonds", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header5.MatchesFilter(filterObj.Filter));
			arrivalPortFilter.Property = "8888";
			AssertEquals("Filtering for Arrival Port 8888 should find Inbond (header1).", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 8888 should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 8888 should find Inbond (header3).", true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 8888 should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 8888 should NOT find Inbond (header5).", false, header5.MatchesFilter(filterObj.Filter));
			arrivalPortFilter.Property = "2704";
			AssertEquals("Filtering for Arrival Port 2704 should NOT find Inbond (header1).", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 2704 should find Inbond (header2).", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 2704 should NOT find Inbond (header3).", false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 2704 should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 2704 should NOT find Inbond (header5).", false, header5.MatchesFilter(filterObj.Filter));
			arrivalPortFilter.Property = "1001";
			AssertEquals("Filtering for Arrival Port 1001 should NOT find Inbond (header1).", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 1001 should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 1001 should NOT find Inbond (header3).", false, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 1001 should NOT find Inbond (header4).", false, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 1001 should find Inbond (header5).", true, header5.MatchesFilter(filterObj.Filter));
			arrivalPortFilter.Property = "27,88";
			AssertEquals("Filtering for Arrival Port 27,88 should find Inbond (header1).", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 27,88 should find Inbond (header2).", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 27,88 should find Inbond (header3).", true, header3.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 27,88 should find Inbond (header4).", true, header4.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Arrival Port 27,88 should NOT find Inbond (header5).", false, header5.MatchesFilter(filterObj.Filter));
		}

		public void TestImporterFilter()
		{
			OrgHeader importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "IMPCHI";
			OrgHeader importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "IMPLAX";
			CusInBondHeader header1 = Factory.New<CusInBondHeader>();
			header1.BH_OA_Importer = importer1.MainAddress.PK;
			CusInBondHeader header2 = Factory.New<CusInBondHeader>();
			header2.BH_OA_Importer = importer2.MainAddress.PK;
			CusInBondHeader header3 = Factory.New<CusInBondHeader>();
			header3.BH_OA_Importer = importer1.MainAddress.PK;
			Factory.Save();
			CusInBondHeaderFilterStripBusinessObject filterObj = new CusInBondHeaderFilterStripBusinessObject();
			ModuleGuidFilter importerFilter = (ModuleGuidFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.Importer];
			importerFilter.IsActive = true;
			AssertEquals("Should have found all created test Inbonds", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, header3.MatchesFilter(filterObj.Filter));
			importerFilter.Property = importer1.PK;
			AssertEquals("Filtering for Importer 1 should find Inbond (header1).", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Importer 1 should NOT find Inbond (header2).", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Importer 1 should find Inbond (header3).", true, header3.MatchesFilter(filterObj.Filter));
			importerFilter.Property = importer2.PK;
			AssertEquals("Filtering for Importer 2 should NOT find Inbond (header1).", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Importer 2 should find Inbond (header2).", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("Filtering for Importer 2 should NOT find Inbond (header3).", false, header3.MatchesFilter(filterObj.Filter));
		}

		public void TestCurrentDispositionFilter()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var bill1 = header1.Bills.AddNew();
			bill1.B0_MasterBillNumber = "MB1";
			bill1.B0_ManifestQty = 100;
			var moveHeader1 = header1.MovementHeaders.AddNew();
			var moveDetail1 = moveHeader1.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			var data1 = moveDetail1.DispositionCodes.AddNewIfNotExist("1K", ZDateTime.BrettsBirthday);
			data1.B7_ParentID = moveDetail1.PK;
			data1.B7_ParentTableCode = moveDetail1.TablePrefix;
			var data2 = moveDetail1.DispositionCodes.AddNewIfNotExist("62", ZDateTime.BrettsBirthday);
			data2.B7_ParentID = moveDetail1.PK;
			data2.B7_ParentTableCode = moveDetail1.TablePrefix;
			var data3 = moveDetail1.DispositionCodes.AddNewIfNotExist("24", ZDateTime.BrettsBirthday);
			data3.B7_ParentID = moveDetail1.PK;
			data3.B7_ParentTableCode = moveDetail1.TablePrefix;
			var header2 = Factory.New<CusInBondHeader>();
			var bill2 = header1.Bills.AddNew();
			bill2.B0_MasterBillNumber = "081003948478";
			bill2.B0_ManifestQty = 1;
			header2.BH_JobReference = "INB3001002";
			var moveHeader2 = header2.MovementHeaders.AddNew();
			moveHeader2.InBondNumber = "968745321";
			var moveDetail2 = moveHeader2.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill2.PK;
			var move2data = moveDetail2.DispositionCodes.AddNewIfNotExist("24", ZDateTime.BrettsBirthday);
			move2data.B7_ParentID = moveDetail2.PK;
			move2data.B7_ParentTableCode = moveDetail2.TablePrefix;
			var moveHeader3 = header2.MovementHeaders.AddNew();
			moveHeader3.InBondNumber = "365988745";
			var header3 = Factory.New<CusInBondHeader>();
			var bill3 = header3.Bills.AddNew();
			bill3.B0_MasterBillNumber = "OB030483";
			bill3.B0_ManifestQty = 5000;
			var bill4 = header3.Bills.AddNew();
			bill4.B0_MasterBillNumber = "OB030392";
			bill4.B0_ManifestQty = 200;
			header3.BH_JobReference = "INB3001001";
			var moveHeader4 = header3.MovementHeaders.AddNew();
			moveHeader4.InBondNumber = "365987412";
			var moveDetail4 = moveHeader4.MovementDetails.AddNew();
			moveDetail4.B9_B0 = bill4.PK;
			var move4data = moveDetail4.DispositionCodes.AddNewIfNotExist("1J", ZDateTime.BrettsBirthday);
			move4data.B7_ParentID = moveDetail4.PK;
			move4data.B7_ParentTableCode = moveDetail4.TablePrefix;
			var moveHeader5 = header3.MovementHeaders.AddNew();
			moveHeader5.InBondNumber = "968534212";
			var moveDetail3 = moveHeader5.MovementDetails.AddNew();
			moveDetail3.B9_B0 = bill3.PK;
			var move3data = moveDetail3.DispositionCodes.AddNewIfNotExist("62", ZDateTime.BrettsBirthday);
			move3data.B7_ParentID = moveDetail3.PK;
			move3data.B7_ParentTableCode = moveDetail3.TablePrefix;
			var header6 = Factory.New<CusInBondHeader>();
			var bill6 = header6.Bills.AddNew();
			bill6.B0_MasterBillNumber = "MB2";
			bill6.B0_ManifestQty = 100;
			var moveHeader6 = header6.MovementHeaders.AddNew();
			var moveDetail6 = moveHeader6.MovementDetails.AddNew();
			moveDetail6.B9_B0 = bill6.PK;
			Factory.Save();
			AssertEquals("Pre-condition - 6 test rows established", 6, Factory.GetDatabaseCount(typeof(GenAddOnColumn)));
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.CurrentBillDisposition];
			textFilter.IsActive = true;
			textFilter.Property = "1K";
			AssertEquals("header1 matches filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 does match filter", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("header3 does match filter", false, header3.MatchesFilter(filterObj.Filter));
			textFilter.Property = "62";
			AssertEquals("header1 matches filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 does match filter", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("header3 matches filter", true, header3.MatchesFilter(filterObj.Filter));
			textFilter.Property = "24";
			AssertEquals("header1 matches filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 matches filter", true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("header3 does match filter", false, header3.MatchesFilter(filterObj.Filter));
			textFilter.Property = "1J";
			AssertEquals("header1 matches filter", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 does match filter", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("header3 matches filter", true, header3.MatchesFilter(filterObj.Filter));
			textFilter.Property = "FF";
			AssertEquals("header1 does match filter", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 does match filter", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("header3 does match filter", false, header3.MatchesFilter(filterObj.Filter));
			US.Business.IIMessageAttacheeWithDisposition dispositionData = moveDetail1;
			dispositionData.MarkPreviousDispositionsInactive(ZDateTime.Today);
			dispositionData.UpdateDispositionInformation("06", ZDateTime.Today);
			Factory.Save();
			AssertEquals("Should now be 4 dispositions in GenAddOnColumn: 1 new disposition replacing 3 deleted dispositions for Header1 and 3 dispostions for Headers 2 & 3", 4, Factory.GetDatabaseCount(typeof(GenAddOnColumn)));
			textFilter.Property = "1K";
			AssertEquals("header1 now should not match filter", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 does match filter", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("header3 does match filter", false, header3.MatchesFilter(filterObj.Filter));
			textFilter.Property = "62";
			AssertEquals("header1 now should not match filter", false, header1.MatchesFilter(filterObj.Filter));
			textFilter.Property = "24";
			AssertEquals("header1 now should not match filter", false, header1.MatchesFilter(filterObj.Filter));
			textFilter.Property = "06";
			AssertEquals("header1 matches filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 does match filter", false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals("header3 does match filter", false, header3.MatchesFilter(filterObj.Filter));
			textFilter.Property = "01";
			textFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			AssertEquals("header6 matches filter, header6 does not has any disposition data", true, header6.MatchesFilter(filterObj.Filter));
		}

		public void TestGetCustomFilterStripsHelpersCore()
		{
			var inbond1 = Factory.New<CusInBondHeader>();
			var task1 = inbond1.WorkflowItems.Tasks.AddNew();
			task1.P9_Description = "Test Task Declaration";
			var shipment = Factory.New<ForwardingShipment>();
			var task2 = shipment.WorkflowItems.Tasks.AddNew();
			task2.P9_Description = "Test Task Shipment";
			var inbond2 = Factory.New<CusInBondHeader>();
			inbond2.BH_ParentID = shipment.PK;
			var inbond3 = Factory.New<CusInBondHeader>();
			var task3 = inbond3.WorkflowItems.Tasks.AddNew();
			task3.P9_Description = "~Test Task Declaration";
			Factory.Save();
			var filterBizo = new CusInBondHeaderFilterStripBusinessObject();
			var filter = filterBizo.AddFilterStrip<TasksModuleFilter>("Tasks");
			filter.SelectedFilters.AddTextFilterStrip("Description", "Test Task");
			var subFilterResult = Factory.Load<ProcessTask>(filter.SelectedFilters.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { task1.PK, task2.PK }, subFilterResult.Select(x => x.PK));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var result = Factory.Load<CusInBondHeader>(filter.Query);
			AssertContainsExactElementsInAnyOrder("Any match: " + filter.Query.LiteralTextSqlFormatted, new[] { inbond1.PK, inbond2.PK }, result.Select(x => x.PK));
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			result = Factory.Load<CusInBondHeader>(filter.Query);
			AssertContainsExactElementsInAnyOrder("None match: " + filter.Query.LiteralTextSqlFormatted, new[] { inbond3.PK }, result.Select(x => x.PK));
		}

		public void TestInBondHouseBillQuery()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			var bill1 = header1.Bills.AddNew();
			bill1.B0_HouseBillNumber = "ABHB1";
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			var bill2 = header2.Bills.AddNew();
			bill2.B0_HouseBillNumber = "ABHB2";
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var filterMasterB = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InbondHouseBill];
			filterMasterB.IsActive = true;
			filterMasterB.Property = "ABHB1";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			filterMasterB = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InbondHouseBill];
			filterMasterB.IsActive = true;
			filterMasterB.Property = "ABHB2";
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			filterMasterB = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InbondHouseBill];
			filterMasterB.IsActive = true;
			filterMasterB.Property = "";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			filterMasterB = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InbondHouseBill];
			filterMasterB.IsActive = true;
			filterMasterB.Property = "ABHB1,ABHB2";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
		}

		public void TestInBondMasterBillQuery()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var bill1 = header1.Bills.AddNew();
			bill1.B0_MasterBillNumber = "ABMB1";
			var header2 = Factory.New<CusInBondHeader>();
			var bill2 = header2.Bills.AddNew();
			bill2.B0_MasterBillNumber = "ABMB2";
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var filterMasterB = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondMasterBill];
			filterMasterB.IsActive = true;
			filterMasterB.Property = "ABMB1";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			filterMasterB = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondMasterBill];
			filterMasterB.IsActive = true;
			filterMasterB.Property = "ABMB2";
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			filterMasterB = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondMasterBill];
			filterMasterB.IsActive = true;
			filterMasterB.Property = "";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			filterMasterB = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondMasterBill];
			filterMasterB.IsActive = true;
			filterMasterB.Property = "ABMB1,ABMB2";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
		}

		public void TestInBondIssuerCodeQuery()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var bill1 = header1.Bills.AddNew();
			bill1.B0_IssuerCode = "AB1";
			var header2 = Factory.New<CusInBondHeader>();
			var bill2 = header2.Bills.AddNew();
			bill2.B0_IssuerCode = "AB2";
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var filterIssuerCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.IssuerCode];
			filterIssuerCode.IsActive = true;
			filterIssuerCode.Property = "AB1";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			filterIssuerCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.IssuerCode];
			filterIssuerCode.IsActive = true;
			filterIssuerCode.Property = "AB2";
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			filterIssuerCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.IssuerCode];
			filterIssuerCode.IsActive = true;
			filterIssuerCode.Property = "";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			filterIssuerCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.IssuerCode];
			filterIssuerCode.IsActive = true;
			filterIssuerCode.Property = "AB1,AB2";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
		}

		public void TestInBondCarrierCodeQuery()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var inBondMoveHeader1 = header1.MovementHeaders.AddNew();
			inBondMoveHeader1.BM_InBondCarrierSCAC = "A1";
			var header2 = Factory.New<CusInBondHeader>();
			var inBondMoveHeader2 = header2.MovementHeaders.AddNew();
			inBondMoveHeader2.BM_InBondCarrierSCAC = "A2";
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var inBondCarrierCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondCarrierCode];
			inBondCarrierCode.IsActive = true;
			inBondCarrierCode.Property = "A1";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			inBondCarrierCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondCarrierCode];
			inBondCarrierCode.IsActive = true;
			inBondCarrierCode.Property = "A2";
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			inBondCarrierCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondCarrierCode];
			inBondCarrierCode.IsActive = true;
			inBondCarrierCode.Property = "";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			inBondCarrierCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondCarrierCode];
			inBondCarrierCode.IsActive = true;
			inBondCarrierCode.Property = "A1,A2";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
		}

		public void TestForeignDestPortKCode()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var inBondMoveHeader1 = header1.MovementHeaders.AddNew();
			inBondMoveHeader1.BM_ForeignDestPortKCode = "1101";
			var header2 = Factory.New<CusInBondHeader>();
			var inBondMoveHeader2 = header2.MovementHeaders.AddNew();
			inBondMoveHeader2.BM_ForeignDestPortKCode = "1139";
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var foreignDestPortKCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondForeignDestination];
			foreignDestPortKCode.IsActive = true;
			foreignDestPortKCode.Property = "1101";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			foreignDestPortKCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondForeignDestination];
			foreignDestPortKCode.IsActive = true;
			foreignDestPortKCode.Property = "1139";
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			foreignDestPortKCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondForeignDestination];
			foreignDestPortKCode.IsActive = true;
			foreignDestPortKCode.Property = "";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			foreignDestPortKCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondForeignDestination];
			foreignDestPortKCode.IsActive = true;
			foreignDestPortKCode.Property = "110,113";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
		}

		public void TesttClosedDateQuery()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var inBondMoveHeader1 = header1.MovementHeaders.AddNew();
			inBondMoveHeader1.BM_InBondClosedDate = ZDateTime.BrettsBirthday;
			var header2 = Factory.New<CusInBondHeader>();
			var inBondMoveHeader2 = header2.MovementHeaders.AddNew();
			inBondMoveHeader2.BM_InBondClosedDate = ZDateTime.BrettsBirthday.AddDays(-10);
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var closedDateQuery = (ModuleDateFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondClosedDate];
			closedDateQuery.IsActive = true;
			closedDateQuery.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			closedDateQuery.Property1 = ZDateTime.BrettsBirthday.AddDays(-1);
			closedDateQuery.Property2 = ZDateTime.BrettsBirthday.AddDays(1);
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
		}

		public void TestDestinationPortDCode()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var inBondMoveHeader1 = header1.MovementHeaders.AddNew();
			inBondMoveHeader1.BM_DestinationPortCode = "1101";
			var header2 = Factory.New<CusInBondHeader>();
			var inBondMoveHeader2 = header2.MovementHeaders.AddNew();
			inBondMoveHeader2.BM_DestinationPortCode = "1139";
			Factory.Save();
			var filterObj = new CusInBondHeaderFilterStripBusinessObject();
			var destinationPortDCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondUSDestination];
			destinationPortDCode.IsActive = true;
			destinationPortDCode.Property = "1101";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			destinationPortDCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondUSDestination];
			destinationPortDCode.IsActive = true;
			destinationPortDCode.Property = "1139";
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			destinationPortDCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondUSDestination];
			destinationPortDCode.IsActive = true;
			destinationPortDCode.Property = "";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			destinationPortDCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondUSDestination];
			destinationPortDCode.IsActive = true;
			destinationPortDCode.Property = "110,113";
			AssertEquals(true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CusInBondHeaderFilterStripBusinessObject();
	}
}
