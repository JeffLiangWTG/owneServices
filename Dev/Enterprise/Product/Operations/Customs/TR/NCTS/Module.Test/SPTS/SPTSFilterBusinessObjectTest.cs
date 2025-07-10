using Enterprise.Customs.Common;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Module.Testing
{
	[TestedType(typeof(SPTSFilterBusinessObject))]
	class SPTSFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void MaxLengthsOfTheFilters()
		{
			TestJobNumberFilterMaxLength();
			TestTransportModeFilterMaxLength();
			TestRegistrationNumberFilterMaxLength();
			TestRegistrationDateFilterMaxLength();
			TestDepartureCustomsOfficeFilterMaxLength();
			TestArrivalCustomsOfficeFilterMaxLength();
			TestVoyageDateFilterMaxLength();
			TestVoyageNoFilterMaxLength();
			TestCarrierFilterMaxLength();
		}

		public void TestToMatchTheFilters()
		{
			TestJobNumberFilter();
			TestTransportModeFilter();
			TestRegistrationNumberFilter();
			TestRegistrationDateFilter();
			TestDepartureCustomsOfficeFilter();
			TestArrivalCustomsOfficeFilter();
			TestVoyageDateFilter();
			TestVoyageNoFilter();
			TestCarrierFilter();
		}

		#region TestToMatchTheFilters

		public void TestJobNumberFilter()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.JobNumber];

			filter.IsActive = true;
			filter.Property = "XXX";

			CombineAssertions("Asserted to Match the Filters for JobNumber", () =>
			{
				Assert(sptsHeader1.MatchesFilter(filterObj.Filter));
				Assert(!sptsHeader2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestTransportModeFilter()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.TransportMode];

			filter.IsActive = true;
			filter.Property = "SEA";

			CombineAssertions("Asserted to Match the Filters for TransportMode", () =>
			{
				Assert(sptsHeader1.MatchesFilter(filterObj.Filter));
				Assert(!sptsHeader2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestRegistrationNumberFilter()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.RegistrationNumber];

			filter.IsActive = true;
			filter.Property = "123123";

			CombineAssertions("Asserted to Match the Filters for RegistrationNumber", () =>
			{
				Assert(sptsHeader1.MatchesFilter(filterObj.Filter));
				Assert(!sptsHeader2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestRegistrationDateFilter()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.RegistrationDate];

			filter.IsActive = true;
			filter.Property1 = new CargoWise.Types.ZDateTime(2022, 03, 5);
			filter.Property2 = new CargoWise.Types.ZDateTime(2022, 03, 20);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			CombineAssertions("Asserted to Match the Filters for RegistrationDate", () =>
			{
				Assert(sptsHeader1.MatchesFilter(filterObj.Filter));
				Assert(!sptsHeader2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestDepartureCustomsOfficeFilter()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.DepartureCustomsOffice];

			filter.IsActive = true;
			filter.Property = "TR060000";

			CombineAssertions("Asserted to Match the Filters for DepartureCustomsOffice", () =>
			{
				Assert(sptsHeader1.MatchesFilter(filterObj.Filter));
				Assert(!sptsHeader2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestArrivalCustomsOfficeFilter()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.ArrivalCustomsOffice];

			filter.IsActive = true;
			filter.Property = "TR041700";

			CombineAssertions("Asserted to Match the Filters for ArrivalCustomsOffice", () =>
			{
				Assert(sptsHeader1.MatchesFilter(filterObj.Filter));
				Assert(!sptsHeader2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestVoyageDateFilter()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.VoyageDate];

			filter.IsActive = true;
			filter.Property1 = new CargoWise.Types.ZDateTime(2022, 07, 10);
			filter.Property2 = new CargoWise.Types.ZDateTime(2022, 07, 30);
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			CombineAssertions("Asserted to Match the Filters for VoyageDate", () =>
			{
				Assert(sptsHeader1.MatchesFilter(filterObj.Filter));
				Assert(!sptsHeader2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestVoyageNoFilter()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.VoyageNo];

			filter.IsActive = true;
			filter.Property = "235";

			CombineAssertions("Asserted to Match the Filters for VoyageNo", () =>
			{
				Assert(sptsHeader1.MatchesFilter(filterObj.Filter));
				Assert(!sptsHeader2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestCarrierFilter()
		{
			OrgHeader carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_Code = "TEDEXPADL";

			OrgHeader carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_Code = "REDEXPADL";

			sptsHeader1 = Factory.New<SPTSHeader>();
			var movementHeader1 = sptsHeader1.MovementHeaders.AddNew();
			movementHeader1.BM_SubApplicationCode = "D";
			movementHeader1.BM_OA_InBondCarrier = carrier1.MainAddress.PK;

			sptsHeader2 = Factory.New<SPTSHeader>();
			var movementHeader2 = sptsHeader2.MovementHeaders.AddNew();
			movementHeader2.BM_SubApplicationCode = "D";
			movementHeader2.BM_OA_InBondCarrier = carrier2.MainAddress.PK;

			sptsHeader3 = Factory.New<SPTSHeader>();
			var movementHeader3 = sptsHeader3.MovementHeaders.AddNew();
			movementHeader3.BM_SubApplicationCode = "D";
			movementHeader3.BM_OA_InBondCarrier = carrier1.MainAddress.PK;

			Factory.Save();

			SPTSFilterBusinessObject filterObj = new SPTSFilterBusinessObject();
			ModuleGuidFilter carrierFilter = (ModuleGuidFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.Carrier];

			carrierFilter.IsActive = true;

			CombineAssertions("Asserted to Match the Filters As Test Cases", () =>
			{
				AssertEquals("Carrier Should Be Created As Test Case", true, sptsHeader1.MatchesFilter(filterObj.Filter));
				AssertEquals("Carrier Should Be Created As Test Case", true, sptsHeader2.MatchesFilter(filterObj.Filter));
				AssertEquals("Carrier Should Be Created As Test Case", true, sptsHeader3.MatchesFilter(filterObj.Filter));
			});

			carrierFilter.Property = carrier1.PK;

			CombineAssertions("Asserted to Match the Filters As Test Cases", () =>
			{
				AssertEquals("Carrier Should Be Find As Case for Carrier 1 & Header 1", true, sptsHeader1.MatchesFilter(filterObj.Filter));
				AssertEquals("Carrier Should Not Be Find As Case for Carrier 1 & Header 2", false, sptsHeader2.MatchesFilter(filterObj.Filter));
				AssertEquals("Carrier Should Be Find As Case for Carrier 1 & Header 3", true, sptsHeader3.MatchesFilter(filterObj.Filter));
			});

			carrierFilter.Property = carrier2.PK;

			CombineAssertions("Asserted to Match the Filters As Test Cases", () =>
			{
				AssertEquals("Carrier Should Not Be Find As Case for Carrier 2 & Header 1", false, sptsHeader1.MatchesFilter(filterObj.Filter));
				AssertEquals("Carrier Should Be Find As Case for Carrier 2 & Header 1", true, sptsHeader2.MatchesFilter(filterObj.Filter));
				AssertEquals("Carrier Should Not Be Find As Case for Carrier 2 & Header 1", false, sptsHeader3.MatchesFilter(filterObj.Filter));
			});
		}

		#endregion

		#region MaxLengthsOfTheFilters

		public void TestJobNumberFilterMaxLength()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.JobNumber];

			AssertEquals("SPTS Filter Should Contain MaxLength for BH_JobReference", CusInBondHeaderSchema.BH_JobReference.MaxLength, filter.MaxLength);
		}

		public void TestTransportModeFilterMaxLength()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.TransportMode];

			AssertEquals("SPTS Filter Should Contain MaxLength for BM_InlandTransportMode", CusInBondMoveHeaderSchema.BM_InlandTransportMode.MaxLength, filter.MaxLength);
		}

		public void TestRegistrationNumberFilterMaxLength()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.RegistrationNumber];

			AssertEquals("SPTS Filter Should Contain MaxLength for CE_EntryNum", CusEntryNumSchema.CE_EntryNum.MaxLength, filter.MaxLength);
		}

		public void TestRegistrationDateFilterMaxLength()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.RegistrationDate];

			AssertEquals("SPTS Filter Should Contain MaxLength for CE_IssueDate", CusEntryNumSchema.CE_IssueDate.MaxLength, filter.MaxLength);
		}

		public void TestDepartureCustomsOfficeFilterMaxLength()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.DepartureCustomsOffice];

			AssertEquals("SPTS Filter Should Contain MaxLength for BM_PortOfPresentationCode", CusInBondMoveHeaderSchema.BM_PortOfPresentationCode.MaxLength, filter.MaxLength);
		}

		public void TestArrivalCustomsOfficeFilterMaxLength()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.ArrivalCustomsOffice];

			AssertEquals("SPTS Filter Should Contain MaxLength for BM_DestinationPortCode", CusInBondMoveHeaderSchema.BM_DestinationPortCode.MaxLength, filter.MaxLength);
		}

		public void TestVoyageDateFilterMaxLength()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.VoyageDate];

			AssertEquals("SPTS Filter Should Contain MaxLength for BH_SailingDate", CusInBondHeaderSchema.BH_SailingDate.MaxLength, filter.MaxLength);
		}

		public void TestVoyageNoFilterMaxLength()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.VoyageNo];

			AssertEquals("SPTS Filter Should Contain MaxLength for BH_VoyageNumber", CusInBondHeaderSchema.BH_VoyageNumber.MaxLength, filter.MaxLength);
		}

		public void TestCarrierFilterMaxLength()
		{
			var filterObj = new SPTSFilterBusinessObject();
			var filter = (ModuleGuidFilter)filterObj[SPTSFilterBusinessObject.FilterConstants.Carrier];

			AssertEquals("SPTS Filter Should Contain MaxLength for BM_OA_InBondCarrier", CusInBondMoveHeaderSchema.BM_OA_InBondCarrier.MaxLength, filter.MaxLength);
		}

		#endregion

		public void TestForExistanceOfTheFilters()
		{
			var filter = new SPTSFilterBusinessObject();

			CombineAssertions("Asserted to Check the Constants Existance", () =>
			{
				AssertNotNull(filter[SPTSFilterBusinessObject.FilterConstants.JobNumber]);
				AssertNotNull(filter[SPTSFilterBusinessObject.FilterConstants.TransportMode]);
				AssertNotNull(filter[SPTSFilterBusinessObject.FilterConstants.RegistrationNumber]);
				AssertNotNull(filter[SPTSFilterBusinessObject.FilterConstants.RegistrationDate]);
				AssertNotNull(filter[SPTSFilterBusinessObject.FilterConstants.DepartureCustomsOffice]);
				AssertNotNull(filter[SPTSFilterBusinessObject.FilterConstants.ArrivalCustomsOffice]);
				AssertNotNull(filter[SPTSFilterBusinessObject.FilterConstants.VoyageDate]);
				AssertNotNull(filter[SPTSFilterBusinessObject.FilterConstants.VoyageNo]);
				AssertNotNull(filter[SPTSFilterBusinessObject.FilterConstants.Carrier]);
			});
		}

		SPTSHeader sptsHeader1;
		SPTSHeader sptsHeader2;
		SPTSHeader sptsHeader3;

		SPTSDepartureMovementHeader sptsDepartureMovementHeader1;
		SPTSDepartureMovementHeader sptsDepartureMovementHeader2;

		void SetData()
		{
			sptsHeader1 = Factory.New<SPTSHeader>();

			sptsHeader1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.TRSPTS;
			sptsHeader1.BH_JobReference = "XXX";
			sptsHeader1.RegistrationNumber = "123123";
			sptsHeader1.RegistrationDate = new CargoWise.Types.ZDateTime(2022, 03, 15);
			sptsHeader1.BH_SailingDate = new CargoWise.Types.ZDateTime(2022, 07, 13);
			sptsHeader1.BH_VoyageNumber = "235";

			sptsDepartureMovementHeader1 = (SPTSDepartureMovementHeader)sptsHeader1.MovementHeaders.AddNew();
			sptsDepartureMovementHeader1.BM_SubApplicationCode = "D";
			sptsDepartureMovementHeader1.BM_InlandTransportMode = SPTSTransportModeList.Codes.SEA;
			sptsDepartureMovementHeader1.BM_PortOfPresentationCode = "TR060000";
			sptsDepartureMovementHeader1.BM_DestinationPortCode = "TR041700";

			sptsHeader2 = Factory.New<SPTSHeader>();
			sptsHeader2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.TRSPTS;
			sptsHeader2.BH_JobReference = "XXY";
			sptsHeader2.RegistrationNumber = "123122";
			sptsHeader2.RegistrationDate = new CargoWise.Types.ZDateTime(2022, 01, 15);
			sptsHeader2.BH_SailingDate = new CargoWise.Types.ZDateTime(2022, 06, 13);
			sptsHeader2.BH_VoyageNumber = "233";

			sptsDepartureMovementHeader2 = (SPTSDepartureMovementHeader)sptsHeader2.MovementHeaders.AddNew();
			sptsDepartureMovementHeader2.BM_InlandTransportMode = SPTSTransportModeList.Codes.AIR;
			sptsDepartureMovementHeader2.BM_PortOfPresentationCode = "TR050000";
			sptsDepartureMovementHeader2.BM_DestinationPortCode = "TR041600";

			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetData();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new SPTSFilterBusinessObject();
		}
	}
}
