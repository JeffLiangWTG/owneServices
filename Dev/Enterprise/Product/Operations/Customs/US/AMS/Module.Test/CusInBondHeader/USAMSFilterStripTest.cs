using System.Linq;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Module.Testing
{
	[TestedType(typeof(USAMSFilterStrip))]
	sealed class USAMSFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestLatestAMSDispositionFilter()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, "US");
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSSeaRailDispositionCode, "AMSDD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3U", "3U DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "3Z", "3Z DESC", startDate, endDate);
			Factory.Save();
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header1Bill = header1.Bills.AddNew();
			var header1BillDisposition1 = header1Bill.DispositionCodes.AddNewIfNotExist("3U", new ZDateTime(2012, 4, 1));
			var header1BillDisposition2 = header1Bill.DispositionCodes.AddNewIfNotExist("3Z", new ZDateTime(2012, 3, 31));
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header2Bill1 = header2.Bills.AddNew();
			var header2Bill1Disposition = header2Bill1.DispositionCodes.AddNewIfNotExist("3U", new ZDateTime(2012, 4, 1));
			var header2Bill2 = header2.Bills.AddNew();
			var header2Bill2Disposition = header2Bill2.DispositionCodes.AddNewIfNotExist("3Z", new ZDateTime(2012, 4, 1));
			var header3 = Factory.New<CusInBondHeader>();
			header3.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header3Bill = header3.Bills.AddNew();
			var header3Bill1Disposition = header3Bill.DispositionCodes.AddNewIfNotExist("3Z", new ZDateTime(2012, 4, 1));
			var header4 = Factory.New<CusInBondHeader>();
			header4.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header4Bill = header4.Bills.AddNew();
			var header5 = Factory.New<CusInBondHeader>();
			header5.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var header5Bill = header5.Bills.AddNew();
			header5Bill.B0_ShipmentType = CusInBondBill.OceanBillType;
			var header5BillDisposition = header5Bill.DispositionCodes.AddNewIfNotExist("3U", new ZDateTime(2012, 4, 1));
			Factory.Save();

			var filterObj = new USAMSFilterStrip();
			var filter = (ModuleNkFilter)filterObj[USAMSFilterStrip.FilterConstants.LatestAMSDisposition];
			filter.IsActive = true;
			filter.Property = "3U";
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", header1.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", header2.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", !header3.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", !header4.MatchesFilter(filterObj.Filter));
			Assert($"Except OceanBill", !header5.MatchesFilter(filterObj.Filter));

			filter.Property = "3Z";
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", !header1.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", header2.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", header3.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will be matched", !header4.MatchesFilter(filterObj.Filter));

			filter.ComparisonOperator = ModuleNkFilter.ComparisonConstants.NotEqual;
			filter.Property = "3U";
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will NOT be matched", !header1.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will NOT be matched", !header2.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will NOT be matched", header3.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will NOT be matched", header4.MatchesFilter(filterObj.Filter));
			Assert($"Except OceanBill", header5.MatchesFilter(filterObj.Filter));

			filter.Property = "3Z";
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will NOT be matched", header1.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will NOT be matched", !header2.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will NOT be matched", !header3.MatchesFilter(filterObj.Filter));
			Assert($"As long as the Latest Disposition Code of one of the bills is {filter.Property} then the header will NOT be matched", header4.MatchesFilter(filterObj.Filter));
		}

		public void TestOriginalEsitmatedTimeFilter()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_ETA = ZDateTime.Today.AddDays(1);
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_ETA = ZDateTime.Today.AddDays(100);
			var filterObj = new USAMSFilterStrip();
			var filter = (ModuleDateFilter)filterObj[USAMSFilterStrip.FilterConstants.OriginalEsitmatedTime];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = ZDateTime.Today.AddDays(7);
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestEstimatedDateofDepartureFilter()
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_FirstExportDate = ZDateTime.Today.AddDays(1);
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_FirstExportDate = ZDateTime.Today.AddDays(100);
			var filterObj = new USAMSFilterStrip();
			var filter = (ModuleDateFilter)filterObj[USAMSFilterStrip.FilterConstants.EstimatedDateofDeparture];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = ZDateTime.Today.AddDays(7);
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestTextFilters()
		{
			AssertTextFilter(USAMSFilterStrip.FilterConstants.TransportMode, CusInBondHeaderSchema.BH_ImportTransportMode);
			AssertTextFilter(USAMSFilterStrip.FilterConstants.CarrierSCAC, CusInBondHeaderSchema.BH_CarrierSCAC);
			AssertTextFilter(USAMSFilterStrip.FilterConstants.JobReference, CusInBondHeaderSchema.BH_JobReference);
			AssertTextFilter(USAMSFilterStrip.FilterConstants.ArrivalPortSchD, CusInBondHeaderSchema.BH_PortUnladingDCode);
			AssertTextFilter(USAMSFilterStrip.FilterConstants.ImportingConveyanceName, CusInBondHeaderSchema.BH_ImportConveyanceName);
			AssertTextFilter(USAMSFilterStrip.FilterConstants.VoyageNumber, CusInBondHeaderSchema.BH_VoyageNumber);
			AssertTextFilter(USAMSFilterStrip.FilterConstants.LloydsNumber, CusInBondHeaderSchema.BH_LloydsNumber);
			AssertTextFilter(USAMSFilterStrip.FilterConstants.LoadPortUNLOCO, CusInBondHeaderSchema.BH_RL_NKImportLoadPort);
		}

		void AssertTextFilter(ZString filterName, SchemaStringColumn schemaCol)
		{
			var header1 = Factory.New<CusInBondHeader>();
			header1[schemaCol] = "1";
			var header2 = Factory.New<CusInBondHeader>();
			header2[schemaCol] = "2";
			var filterObj = new USAMSFilterStrip();
			var filter = (ModuleTextFilter)filterObj[filterName];
			filter.IsActive = true;
			filter.Property = "1";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		[TestDate(2016, 07, 14)]
		public void TestActualArrivalDateFilter()
		{
			var header0 = Factory.New<CusInBondHeader>();
			header0.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var bill0 = header0.Bills.AddNew();
			bill0.B0_A_ARV = ZDateTime.Today.AddDays(-1);
			var header1 = Factory.New<CusInBondHeader>();
			header1.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var bill1 = header1.Bills.AddNew();
			bill1.B0_A_ARV = ZDateTime.Today.AddDays(-10);
			var header2 = Factory.New<CusInBondHeader>();
			header2.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			var bill2 = header2.Bills.AddNew();
			bill2.B0_A_ARV = ZDateTime.Today.AddDays(10);
			Factory.Save();
			var filterObj = new USAMSFilterStrip();
			var filter = (ModuleDateFilter)filterObj[USAMSFilterStrip.FilterConstants.ActualArrivalDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(-2);
			AssertEquals(true, header0.MatchesFilter(filterObj.Filter));
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
			filter.Property1 = ZDateTime.Today.AddDays(2);
			AssertEquals(true, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header0.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
			filter.Property2 = ZDateTime.Today.AddDays(4);
			AssertEquals(false, header2.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header0.MatchesFilter(filterObj.Filter));
			AssertEquals(false, header1.MatchesFilter(filterObj.Filter));
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
			var filterBizo = new USAMSFilterStrip();
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

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new USAMSFilterStrip();
		}
	}
}
