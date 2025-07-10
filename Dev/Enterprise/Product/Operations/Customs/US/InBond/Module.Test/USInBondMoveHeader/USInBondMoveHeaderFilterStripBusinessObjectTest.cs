using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.Testing
{
	[TestedType(typeof(USInBondMoveHeaderFilterStripBusinessObject))]
	sealed class USInBondMoveHeaderFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestCompanyFilter()
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK;
			var currentCompanyBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyBranch1.GB_GC = currentCompanyPK;

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var company2Branch1 = Factory.NewWithValidTestData<GlbBranch>();
			company2Branch1.GB_GC = company2.PK;

			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_GB = currentCompanyBranch1.PK;
			var movement1 = header1.MovementHeader;
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_GB = company2Branch1.PK;
			var movement2 = header2.MovementHeader;
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			Assert("Movement1 matches filter", moveHeader1.MatchesFilter(filterObj.Filter));
			Assert("Movement2 does not match filter", !moveHeader2.MatchesFilter(filterObj.Filter));
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
			var movement1 = header1.MovementHeader;
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_GB = currentCompanyBranch2.PK;
			var movement2 = header2.MovementHeader;
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();

			var branchFilter = (ModuleGuidFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.Branch];
			branchFilter.IsActive = true;
			branchFilter.Property = currentCompanyBranch1.PK;

			Assert("Movement1 matches filter", moveHeader1.MatchesFilter(filterObj.Filter));
			Assert("Movement2 does not matches filter", !moveHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestInBondNumberFilter()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_JobReference = "INB3001001";
			var movement1 = header1.MovementHeaders.AddNew();
			movement1.InBondNumber = "365987412";
			var movement2 = header1.MovementHeaders.AddNew();
			movement2.InBondNumber = "968534212";
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_JobReference = "INB3001002";
			var movement3 = header2.MovementHeaders.AddNew();
			movement3.InBondNumber = "968745321";
			var movement4 = header2.MovementHeaders.AddNew();
			movement4.InBondNumber = "365988745";
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var moveHeader3 = Factory.Load<USInBondMoveHeader>(movement3.PK);
			var moveHeader4 = Factory.Load<USInBondMoveHeader>(movement4.PK);

			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondNumber];
			textFilter.IsActive = true;
			textFilter.Property = "36598";
			AssertEquals("Movement1 match filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 does not match filter", false, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 match filter", true, moveHeader4.MatchesFilter(filterObj.Filter));

			textFilter.Property = "365987";
			AssertEquals("Movement1 match filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 does not match filter", false, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 does not match filter", false, moveHeader4.MatchesFilter(filterObj.Filter));

			textFilter.Property = "968745321";
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 match filter", true, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 does not match filter", false, moveHeader4.MatchesFilter(filterObj.Filter));

			textFilter.Property = "36598,968";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 matches filter", true, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 matches filter", true, moveHeader4.MatchesFilter(filterObj.Filter));
		}

		public void TestInBondEntryTypeFilter()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var movement1 = header1.MovementHeaders.AddNew();
			movement1.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			var movement2 = header1.MovementHeaders.AddNew();
			movement2.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;

			var header2 = Factory.New<CusInBondHeader>();
			var movement3 = header2.MovementHeaders.AddNew();
			movement3.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			var movement4 = header2.MovementHeaders.AddNew();
			movement4.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var moveHeader3 = Factory.Load<USInBondMoveHeader>(movement3.PK);
			var moveHeader4 = Factory.Load<USInBondMoveHeader>(movement4.PK);

			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			var entryTypeFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.ITType];
			entryTypeFilter.IsActive = true;
			entryTypeFilter.Property = "";
			AssertEquals("Should have found all created test Inbond Movements", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbond Movements", true, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbond Movements", true, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbond Movements", true, moveHeader4.MatchesFilter(filterObj.Filter));

			entryTypeFilter.Property = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertEquals("Movement1 match filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 does not match filter", false, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 does not match filter", false, moveHeader4.MatchesFilter(filterObj.Filter));

			entryTypeFilter.Property = InbondCommonTypeList.Codes._2TransportandExport;
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 matches filter", true, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 does not match filter", false, moveHeader4.MatchesFilter(filterObj.Filter));

			entryTypeFilter.Property = InbondCommonTypeList.Codes._3ImmediateExport;
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 does not match filter", false, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 match filter", true, moveHeader4.MatchesFilter(filterObj.Filter));
		}

		public void TestJobReferenceFilter()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_JobReference = "INB3001001";
			var movement1 = header1.MovementHeader;
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_JobReference = "INB3001002";
			var movement2 = header2.MovementHeader;
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			var jobReferenceFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.JobReference];
			jobReferenceFilter.IsActive = true;
			jobReferenceFilter.Property = "INB3001001";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));

			jobReferenceFilter.Property = "INB3001002";
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));

			jobReferenceFilter.Property = "";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));

			jobReferenceFilter.Property = "INB3001001,INB3001002";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestTransportModeFilter()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			var movement1 = header1.MovementHeader;
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			var movement2 = header2.MovementHeader;
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_ImportTransportMode = TransportModeCodes.Codes.VesselNonContainer;
			var movement3 = header3.MovementHeader;
			var header4 = Factory.New<CusInBondHeader>();
			header4.BH_ImportTransportMode = TransportModeCodes.Codes.TruckNonContainer;
			var movement4 = header4.MovementHeader;
			var header5 = Factory.New<CusInBondHeader>();
			header5.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			var movement5 = header5.MovementHeader;
			var header6 = Factory.New<CusInBondHeader>();
			header6.BH_ImportTransportMode = TransportModeCodes.Codes.RailNonContainer;
			var movement6 = header6.MovementHeader;
			var header7 = Factory.New<CusInBondHeader>();
			header7.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			var movement7 = header7.MovementHeader;
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var moveHeader3 = Factory.Load<USInBondMoveHeader>(movement3.PK);
			var moveHeader4 = Factory.Load<USInBondMoveHeader>(movement4.PK);
			var moveHeader5 = Factory.Load<USInBondMoveHeader>(movement5.PK);
			var moveHeader6 = Factory.Load<USInBondMoveHeader>(movement6.PK);
			var moveHeader7 = Factory.Load<USInBondMoveHeader>(movement7.PK);
			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();

			var transportModeFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.TransportMode];
			transportModeFilter.IsActive = true;
			transportModeFilter.Property = "";
			AssertEquals("Should have found all created test Inbonds", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader5.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader6.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader7.MatchesFilter(filterObj.Filter));

			transportModeFilter.Property = TransportModeCodes.Codes.VesselContainer;
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 does not match filter", false, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 does not match filter", false, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement5 matches filter", true, moveHeader5.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement6 does not match filter", false, moveHeader6.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement7 does not match filter", false, moveHeader7.MatchesFilter(filterObj.Filter));

			transportModeFilter.Property = TransportModeCodes.Codes.VesselNonContainer;
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 matches filter", true, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 does not match filter", false, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement5 does not match filter", false, moveHeader5.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement6 does not match filter", false, moveHeader6.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement7 does not match filter", false, moveHeader7.MatchesFilter(filterObj.Filter));

			transportModeFilter.Property = TransportModeCodes.Codes.AirNonContainer;
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 does not match filter", false, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 does not match filter", false, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement5 does not match filter", false, moveHeader5.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement6 does not match filter", false, moveHeader6.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement7 matches filter", true, moveHeader7.MatchesFilter(filterObj.Filter));

			transportModeFilter.Property = TransportModeCodes.Codes.TruckNonContainer;
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 does not match filter", false, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 matches filter", true, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement5 does not match filter", false, moveHeader5.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement6 does not match filter", false, moveHeader6.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement7 does not match filter", false, moveHeader7.MatchesFilter(filterObj.Filter));

			transportModeFilter.Property = TransportModeCodes.Codes.RailNonContainer;
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 does not match filter", false, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 does not match filter", false, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement5 does not match filter", false, moveHeader5.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement6 matches filter", true, moveHeader6.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement7 does not match filter", false, moveHeader7.MatchesFilter(filterObj.Filter));
		}

		public void TestETAFilter()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_ETA = ZDateTime.Today.AddDays(-20);
			var movement1 = header1.MovementHeader;
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_ETA = ZDateTime.Today.AddDays(-10);
			var movement2 = header2.MovementHeader;
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_ETA = ZDateTime.Today;
			var movement3 = header3.MovementHeader;
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var moveHeader3 = Factory.Load<USInBondMoveHeader>(movement3.PK);
			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			var etaFilter = (ModuleDateFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.ETA];
			etaFilter.IsActive = true;
			etaFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			etaFilter.Property2 = ZDateTime.Today.AddDays(-30);
			AssertEquals("Should NOT have found all created test Inbond Movements", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should NOT have found all created test Inbond Movements", false, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should NOT have found all created test Inbond Movements", false, moveHeader3.MatchesFilter(filterObj.Filter));

			etaFilter.Property1 = ZDateTime.Today.AddDays(-20);
			etaFilter.Property2 = ZDateTime.Today;
			AssertEquals("Should have found all created test Inbond Movements", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbond Movements", true, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbond Movements", true, moveHeader3.MatchesFilter(filterObj.Filter));

			etaFilter.Property1 = ZDateTime.Today.AddDays(-12);
			etaFilter.Property2 = ZDateTime.Today.AddDays(-5);
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 does not match filter", false, moveHeader3.MatchesFilter(filterObj.Filter));
		}

		public void TestLoadingPortFilter()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_ImportLoadPortKCode = "60243";
			var movement1 = header1.MovementHeader;
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_ImportLoadPortKCode = "52120";
			var movement2 = header2.MovementHeader;
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_ImportLoadPortKCode = "60243";
			var movement3 = header3.MovementHeader;
			var header4 = Factory.New<CusInBondHeader>();
			header4.BH_ImportLoadPortKCode = "79325";
			var movement4 = header4.MovementHeader;
			var header5 = Factory.New<CusInBondHeader>();
			header5.BH_ImportLoadPortKCode = "22590";
			var movement5 = header5.MovementHeader;
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var moveHeader3 = Factory.Load<USInBondMoveHeader>(movement3.PK);
			var moveHeader4 = Factory.Load<USInBondMoveHeader>(movement4.PK);
			var moveHeader5 = Factory.Load<USInBondMoveHeader>(movement5.PK);
			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			ModuleTextFilter loadingPortFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.LoadingScheduleK];
			loadingPortFilter.IsActive = true;
			loadingPortFilter.Property = "";
			AssertEquals("Should have found all created test Inbonds", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader5.MatchesFilter(filterObj.Filter));

			loadingPortFilter.Property = "60243";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 matches filter", true, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 does not match filter", false, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement5 does not match filter", false, moveHeader5.MatchesFilter(filterObj.Filter));

			loadingPortFilter.Property = "52120";
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 does not match filter", false, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 does not match filter", false, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement5 does not match filter", false, moveHeader5.MatchesFilter(filterObj.Filter));

			loadingPortFilter.Property = "22590";
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 does not match filter", false, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 does not match filter", false, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement5 matches filter", true, moveHeader5.MatchesFilter(filterObj.Filter));

			loadingPortFilter.Property = "5,6";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 matches filter", true, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 does not match filter", false, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement5 does not match filter", false, moveHeader5.MatchesFilter(filterObj.Filter));
		}

		public void TestArrivalPortFilter()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_PortUnladingDCode = "8888";
			var movement1 = header1.MovementHeader;
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_PortUnladingDCode = "2704";
			var movement2 = header2.MovementHeader;
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_PortUnladingDCode = "8888";
			var movement3 = header3.MovementHeader;
			var header4 = Factory.New<CusInBondHeader>();
			header4.BH_PortUnladingDCode = "2724";
			var movement4 = header4.MovementHeader;
			var header5 = Factory.New<CusInBondHeader>();
			header5.BH_PortUnladingDCode = "1001";
			var movement5 = header5.MovementHeader;
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var moveHeader3 = Factory.Load<USInBondMoveHeader>(movement3.PK);
			var moveHeader4 = Factory.Load<USInBondMoveHeader>(movement4.PK);
			var moveHeader5 = Factory.Load<USInBondMoveHeader>(movement5.PK);
			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			var arrivalPortFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.ArrivalScheduleD];
			arrivalPortFilter.IsActive = true;
			arrivalPortFilter.Property = "";
			AssertEquals("Should have found all created test Inbonds", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader5.MatchesFilter(filterObj.Filter));

			arrivalPortFilter.Property = "8888";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 matches filter", true, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 does not match filter", false, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement5 does not match filter", false, moveHeader5.MatchesFilter(filterObj.Filter));

			arrivalPortFilter.Property = "2704";
			AssertEquals("Movement1 does not filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 does not match filter", false, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 does not match filter", false, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement5 does not match filter", false, moveHeader5.MatchesFilter(filterObj.Filter));

			arrivalPortFilter.Property = "1001";
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 does not match filter", false, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 does not match filter", false, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement5 matches filter", true, moveHeader5.MatchesFilter(filterObj.Filter));

			arrivalPortFilter.Property = "1,2";
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 does not match filter", false, moveHeader3.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement4 matches filter", true, moveHeader4.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement5 matches filter", true, moveHeader5.MatchesFilter(filterObj.Filter));
		}

		public void TestImporterFilter()
		{
			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "IMPCHI";

			var importer2 = Factory.NewWithValidTestData<OrgHeader>();
			importer2.OH_Code = "IMPLAX";

			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_OA_Importer = importer1.MainAddress.PK;
			var movement1 = header1.MovementHeader;
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_OA_Importer = importer2.MainAddress.PK;
			var movement2 = header2.MovementHeader;
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_OA_Importer = importer1.MainAddress.PK;
			var movement3 = header3.MovementHeader;
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var moveHeader3 = Factory.Load<USInBondMoveHeader>(movement3.PK);
			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			var importerFilter = (ModuleGuidFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.Importer];
			importerFilter.IsActive = true;
			AssertEquals("Should have found all created test Inbonds", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Should have found all created test Inbonds", true, moveHeader3.MatchesFilter(filterObj.Filter));

			importerFilter.Property = importer1.PK;
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 matches filter", true, moveHeader3.MatchesFilter(filterObj.Filter));

			importerFilter.Property = importer2.PK;
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement3 does not match filter", false, moveHeader3.MatchesFilter(filterObj.Filter));
		}

		public void TestInBondMasterBillQuery()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var movement1 = header1.MovementHeader;
			var bill1 = header1.Bills.AddNew();
			bill1.B0_MasterBillNumber = "ABMB1";

			var header2 = Factory.New<CusInBondHeader>();
			var movement2 = header2.MovementHeader;
			var bill2 = header2.Bills.AddNew();
			bill2.B0_MasterBillNumber = "ABMB2";
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			var filterMasterBill = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondMasterBill];
			filterMasterBill.IsActive = true;
			filterMasterBill.Property = "ABMB1";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));

			filterMasterBill = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondMasterBill];
			filterMasterBill.IsActive = true;
			filterMasterBill.Property = "ABMB2";
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));

			filterMasterBill = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondMasterBill];
			filterMasterBill.IsActive = true;
			filterMasterBill.Property = "";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));

			filterMasterBill = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondMasterBill];
			filterMasterBill.IsActive = true;
			filterMasterBill.Property = "ABMB1,ABMB2";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestInBondHouseBillQuery()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var movement1 = header1.MovementHeader;
			var bill1 = header1.Bills.AddNew();
			bill1.B0_HouseBillNumber = "ABHB1";

			var header2 = Factory.New<CusInBondHeader>();
			var movement2 = header2.MovementHeader;
			var bill2 = header2.Bills.AddNew();
			bill2.B0_HouseBillNumber = "ABHB2";
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			var filterHouseBill = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InbondHouseBill];
			filterHouseBill.IsActive = true;
			filterHouseBill.Property = "ABHB1";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));

			filterHouseBill = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InbondHouseBill];
			filterHouseBill.IsActive = true;
			filterHouseBill.Property = "ABHB2";
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));

			filterHouseBill = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InbondHouseBill];
			filterHouseBill.IsActive = true;
			filterHouseBill.Property = "";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));

			filterHouseBill = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InbondHouseBill];
			filterHouseBill.IsActive = true;
			filterHouseBill.Property = "ABHB1,ABHB2";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestInBondIssuerCodeQuery()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var movement1 = header1.MovementHeader;
			var bill1 = header1.Bills.AddNew();
			bill1.B0_IssuerCode = "AB1";

			var header2 = Factory.New<CusInBondHeader>();
			var movement2 = header2.MovementHeader;
			var bill2 = header2.Bills.AddNew();
			bill2.B0_IssuerCode = "AB2";
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			var filterIssuerCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.IssuerCode];
			filterIssuerCode.IsActive = true;
			filterIssuerCode.Property = "AB1";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));

			filterIssuerCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.IssuerCode];
			filterIssuerCode.IsActive = true;
			filterIssuerCode.Property = "AB2";
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));

			filterIssuerCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.IssuerCode];
			filterIssuerCode.IsActive = true;
			filterIssuerCode.Property = "";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));

			filterIssuerCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.IssuerCode];
			filterIssuerCode.IsActive = true;
			filterIssuerCode.Property = "AB1,AB2";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestInBondCarrierCodeQuery()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var movement1 = header1.MovementHeaders.AddNew();
			movement1.BM_InBondCarrierSCAC = "A1";

			var header2 = Factory.New<CusInBondHeader>();
			var movement2 = header2.MovementHeaders.AddNew();
			movement2.BM_InBondCarrierSCAC = "A2";

			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			var inBondCarrierCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondCarrierCode];
			inBondCarrierCode.IsActive = true;
			inBondCarrierCode.Property = "A1";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));

			inBondCarrierCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondCarrierCode];
			inBondCarrierCode.IsActive = true;
			inBondCarrierCode.Property = "A2";
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));

			inBondCarrierCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondCarrierCode];
			inBondCarrierCode.IsActive = true;
			inBondCarrierCode.Property = "";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));

			inBondCarrierCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondCarrierCode];
			inBondCarrierCode.IsActive = true;
			inBondCarrierCode.Property = "A1,A2";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestForeignDestPortKCode()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var movement1 = header1.MovementHeaders.AddNew();
			movement1.BM_ForeignDestPortKCode = "1101";

			var header2 = Factory.New<CusInBondHeader>();
			var movement2 = header2.MovementHeaders.AddNew();
			movement2.BM_ForeignDestPortKCode = "1139";

			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			var foreignDestPortKCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondForeignDestination];
			foreignDestPortKCode.IsActive = true;
			foreignDestPortKCode.Property = "1101";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));

			foreignDestPortKCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondForeignDestination];
			foreignDestPortKCode.IsActive = true;
			foreignDestPortKCode.Property = "1139";
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));

			foreignDestPortKCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondForeignDestination];
			foreignDestPortKCode.IsActive = true;
			foreignDestPortKCode.Property = "";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));

			foreignDestPortKCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondForeignDestination];
			foreignDestPortKCode.IsActive = true;
			foreignDestPortKCode.Property = "110,113";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestClosedDateQuery()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var movement1 = header1.MovementHeaders.AddNew();
			movement1.BM_InBondClosedDate = ZDateTime.BrettsBirthday;

			var header2 = Factory.New<CusInBondHeader>();
			var movement2 = header2.MovementHeaders.AddNew();
			movement2.BM_InBondClosedDate = ZDateTime.BrettsBirthday.AddDays(-10);
			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			var closedDateQuery = (ModuleDateFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondClosedDate];
			closedDateQuery.IsActive = true;
			closedDateQuery.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			closedDateQuery.Property1 = ZDateTime.BrettsBirthday.AddDays(-1);
			closedDateQuery.Property2 = ZDateTime.BrettsBirthday.AddDays(1);

			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestDestinationPortDCode()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var movement1 = header1.MovementHeaders.AddNew();
			movement1.BM_DestinationPortCode = "1101";

			var header2 = Factory.New<CusInBondHeader>();
			var movement2 = header2.MovementHeaders.AddNew();
			movement2.BM_DestinationPortCode = "1139";

			Factory.Save();

			var moveHeader1 = Factory.Load<USInBondMoveHeader>(movement1.PK);
			var moveHeader2 = Factory.Load<USInBondMoveHeader>(movement2.PK);
			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			var destinationPortDCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondUSDestination];
			destinationPortDCode.IsActive = true;
			destinationPortDCode.Property = "1101";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 does not match filter", false, moveHeader2.MatchesFilter(filterObj.Filter));

			destinationPortDCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondUSDestination];
			destinationPortDCode.IsActive = true;
			destinationPortDCode.Property = "1139";
			AssertEquals("Movement1 does not match filter", false, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));

			destinationPortDCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondUSDestination];
			destinationPortDCode.IsActive = true;
			destinationPortDCode.Property = "";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));

			destinationPortDCode = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.InBondUSDestination];
			destinationPortDCode.IsActive = true;
			destinationPortDCode.Property = "110,113";
			AssertEquals("Movement1 matches filter", true, moveHeader1.MatchesFilter(filterObj.Filter));
			AssertEquals("Movement2 matches filter", true, moveHeader2.MatchesFilter(filterObj.Filter));
		}

		public void TestQPStautsAndWPStatus()
		{
			var header1 = Factory.New<CusInBondHeader>();
			var movement11 = header1.MovementHeaders.AddNew();
			var movement12 = header1.MovementHeaders.AddNew();
			movement12.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDeparturePartialOriginal;
			var movement13 = header1.MovementHeaders.AddNew();
			movement13.BM_CustomsStatus = ImportMessageStatusList.Codes.ErrorDepartureOriginal;
			movement13.BM_MessageStatus = ImportMessageStatusList.Codes.ErrorExportation;
			var movement14 = header1.MovementHeaders.AddNew();
			movement14.BM_MessageStatus = ImportMessageStatusList.Codes.ClearExportation;

			var header2 = Factory.New<CusInBondHeader>();
			var movement21 = header1.MovementHeaders.AddNew();
			var movement22 = header1.MovementHeaders.AddNew();
			movement22.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDeparturePartialOriginal;
			var movement23 = header1.MovementHeaders.AddNew();
			movement23.BM_CustomsStatus = ImportMessageStatusList.Codes.ErrorDepartureOriginal;
			movement23.BM_MessageStatus = ImportMessageStatusList.Codes.ErrorExportation;
			var movement24 = header1.MovementHeaders.AddNew();
			movement24.BM_MessageStatus = ImportMessageStatusList.Codes.ClearExportation;
			Factory.Save();

			var moveHeader11 = Factory.Load<USInBondMoveHeader>(movement11.PK);
			var moveHeader12 = Factory.Load<USInBondMoveHeader>(movement12.PK);
			var moveHeader13 = Factory.Load<USInBondMoveHeader>(movement13.PK);
			var moveHeader14 = Factory.Load<USInBondMoveHeader>(movement14.PK);
			var moveHeader21 = Factory.Load<USInBondMoveHeader>(movement21.PK);
			var moveHeader22 = Factory.Load<USInBondMoveHeader>(movement22.PK);
			var moveHeader23 = Factory.Load<USInBondMoveHeader>(movement23.PK);
			var moveHeader24 = Factory.Load<USInBondMoveHeader>(movement24.PK);

			var filterObj = new USInBondMoveHeaderFilterStripBusinessObject();
			var qpStatusFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.QPMessageStatus];
			qpStatusFilter.IsActive = true;
			qpStatusFilter.Property = "";
			CombineAssertions("QP = Blank", () =>
			{
				Assert(nameof(moveHeader11), moveHeader11.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader12), moveHeader12.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader13), moveHeader13.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader14), moveHeader14.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader21), moveHeader21.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader22), moveHeader22.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader23), moveHeader23.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader24), moveHeader24.MatchesFilter(filterObj.Filter));
			});

			qpStatusFilter.Property = ImportMessageStatusList.Codes.ClearDeparturePartialOriginal;
			CombineAssertions("QP = ClearDeparturePartialOriginal", () =>
			{
				Assert(nameof(moveHeader11), !moveHeader11.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader12), moveHeader12.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader13), !moveHeader13.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader14), !moveHeader14.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader21), !moveHeader21.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader22), moveHeader22.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader23), !moveHeader23.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader24), !moveHeader24.MatchesFilter(filterObj.Filter));
			});

			qpStatusFilter.Property = ImportMessageStatusList.Codes.ErrorDepartureWithdraw;
			CombineAssertions("QP = ErrorDepartureWithdraw", () =>
			{
				Assert(nameof(moveHeader11), !moveHeader11.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader12), !moveHeader12.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader13), !moveHeader13.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader14), !moveHeader14.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader21), !moveHeader21.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader22), !moveHeader22.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader23), !moveHeader23.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader24), !moveHeader24.MatchesFilter(filterObj.Filter));
			});

			qpStatusFilter.IsActive = false;
			var wpStatusFilter = (ModuleTextFilter)filterObj[CusInBondHeaderFilterStripBusinessObject.FilterConstants.WPMessageStatus];
			wpStatusFilter.IsActive = true;
			wpStatusFilter.Property = "";
			CombineAssertions("WP = Blank", () =>
			{
				Assert(nameof(moveHeader11), moveHeader11.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader12), moveHeader12.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader13), moveHeader13.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader14), moveHeader14.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader21), moveHeader21.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader22), moveHeader22.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader23), moveHeader23.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader24), moveHeader24.MatchesFilter(filterObj.Filter));
			});

			wpStatusFilter.Property = ImportMessageStatusList.Codes.ErrorExportation;
			CombineAssertions("WP = ErrorExportation", () =>
			{
				Assert(nameof(moveHeader11), !moveHeader11.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader12), !moveHeader12.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader13), moveHeader13.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader14), !moveHeader14.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader21), !moveHeader21.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader22), !moveHeader22.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader23), moveHeader23.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader24), !moveHeader24.MatchesFilter(filterObj.Filter));
			});

			wpStatusFilter.Property = ImportMessageStatusList.Codes.ErrorArrival;
			CombineAssertions("WP = ErrorArrival", () =>
			{
				Assert(nameof(moveHeader11), !moveHeader11.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader12), !moveHeader12.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader13), !moveHeader13.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader14), !moveHeader14.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader21), !moveHeader21.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader22), !moveHeader22.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader23), !moveHeader23.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader24), !moveHeader24.MatchesFilter(filterObj.Filter));
			});

			qpStatusFilter.IsActive = true;
			qpStatusFilter.Property = ImportMessageStatusList.Codes.ClearDeparturePartialOriginal;
			wpStatusFilter.Property = ImportMessageStatusList.Codes.ErrorExportation;
			CombineAssertions("QP = ClearDeparturePartialOriginal, WP = ErrorExportation", () =>
			{
				Assert(nameof(moveHeader11), !moveHeader11.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader12), !moveHeader12.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader13), !moveHeader13.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader14), !moveHeader14.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader21), !moveHeader21.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader22), !moveHeader22.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader23), !moveHeader23.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader24), !moveHeader24.MatchesFilter(filterObj.Filter));
			});

			qpStatusFilter.Property = ImportMessageStatusList.Codes.ErrorDepartureOriginal;
			CombineAssertions("QP = ErrorDepartureOriginal, WP = ErrorExportation", () =>
			{
				Assert(nameof(moveHeader11), !moveHeader11.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader12), !moveHeader12.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader13), moveHeader13.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader14), !moveHeader14.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader21), !moveHeader21.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader22), !moveHeader22.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader23), moveHeader23.MatchesFilter(filterObj.Filter));
				Assert(nameof(moveHeader24), !moveHeader24.MatchesFilter(filterObj.Filter));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USInBondMoveHeaderFilterStripBusinessObject();
	}
}
