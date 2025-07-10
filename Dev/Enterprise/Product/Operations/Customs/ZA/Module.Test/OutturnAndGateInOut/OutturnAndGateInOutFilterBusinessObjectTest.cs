using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Customs.ZA.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(OutturnAndGateInOutFilterBusinessObject))]
	sealed class OutturnAndGateInOutFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestJobNumberFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VWG";
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "VIC";
			Factory.Save();
			var filterObj = new OutturnAndGateInOutFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[OutturnAndGateInOutFilterBusinessObject.FilterConstants.JobNumber];
			filter.IsActive = true;
			filter.Property = "VWG";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestBillNumberFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VVV";
			header1.MasterBill.ABL_BillNumber = "VWG";
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "VV2";
			header2.MasterBill.ABL_BillNumber = "VIC";
			Factory.Save();
			var filterObj = new OutturnAndGateInOutFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[OutturnAndGateInOutFilterBusinessObject.FilterConstants.BillNumber];
			filter.IsActive = true;
			filter.Property = "VWG";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestCustomsOfficeFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VVV";
			header1.AMA_CustomsOffice = "VWG";
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "VV2";
			header2.AMA_CustomsOffice = "VIC";
			Factory.Save();
			var filterObj = new OutturnAndGateInOutFilterBusinessObject();
			var filter = (ModuleNkFilter)filterObj[OutturnAndGateInOutFilterBusinessObject.FilterConstants.CustomsOffice];
			filter.IsActive = true;
			filter.Property = "VWG";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestTransportModeFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VV1";
			header1.AMA_TransportMode = "VWG";
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "VV2";
			header2.AMA_TransportMode = "VIC";
			Factory.Save();
			var filterObj = new OutturnAndGateInOutFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[OutturnAndGateInOutFilterBusinessObject.FilterConstants.TransportMode];
			filter.IsActive = true;
			filter.Property = "VWG";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestVoyageCodeFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VV1";
			header1.AMA_Voyage = "VWG";
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_Voyage = "VIC";
			header2.AMA_JobReference = "VV2";
			Factory.Save();
			var filterObj = new OutturnAndGateInOutFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[OutturnAndGateInOutFilterBusinessObject.FilterConstants.VoyageCode];
			filter.IsActive = true;
			filter.Property = "VWG";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		[TestDate(2017, 6, 16)]
		public void TestDateFilters()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VV1";
			header1.MasterBill.ABL_E_DEP = ZDateTime.Today.AddDays(1);
			header1.MasterBill.ABL_E_ARV = ZDateTime.Today.AddDays(100);
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "VV2";
			header2.MasterBill.ABL_E_DEP = ZDateTime.Today.AddDays(100);
			header2.MasterBill.ABL_E_ARV = ZDateTime.Today.AddDays(1);
			Factory.Save();
			var filterObj = new OutturnAndGateInOutFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[OutturnAndGateInOutFilterBusinessObject.FilterConstants.EstimatedDepartureDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property2 = ZDateTime.Today.AddDays(7);
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
			var filterObjArv = new OutturnAndGateInOutFilterBusinessObject();
			var filterArv = (ModuleDateFilter)filterObjArv[OutturnAndGateInOutFilterBusinessObject.FilterConstants.EstimatedArrivalDate];
			filterArv.IsActive = true;
			filterArv.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filterArv.Property2 = ZDateTime.Today.AddDays(7);
			Assert(!header1.MatchesFilter(filterObjArv.Filter));
			Assert(header2.MatchesFilter(filterObjArv.Filter));
		}

		public void TestLoadDischargeFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VV1";
			header1.MasterBill.ABL_RL_NKPortOfLoading = "AUSYD";
			header1.MasterBill.ABL_RL_NKPortOfDischarge = "JPTKY";
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "VV2";
			header2.MasterBill.ABL_RL_NKPortOfLoading = "AUMEL";
			header2.MasterBill.ABL_RL_NKPortOfDischarge = "SGSIN";
			Factory.Save();
			var filterObj = new OutturnAndGateInOutFilterBusinessObject();
			var filter = (ModuleLocationFilter)filterObj[OutturnAndGateInOutFilterBusinessObject.FilterConstants.PortOfLoadingDischarge];
			AssertEquals("Loading", filter.ItemDescription1.Caption);
			AssertEquals("Discharge", filter.ItemDescription2.Caption);
			filter.IsActive = true;
			filter.Property1 = "AU";
			filter.Property2 = ZString.Empty;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(filterObj.Filter));
			filter.Property2 = "SGSIN";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", true, header2.MatchesFilter(filterObj.Filter));
			filter.Property2 = "USLAX";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(filterObj.Filter));
			filter.Property2 = "JP";
			AssertEquals("header1 match filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(filterObj.Filter));
			filter.Property1 = "AUMEL";
			AssertEquals("header1 match filter", false, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(filterObj.Filter));
			filter.Property1 = ZString.Empty;
			AssertEquals("header1 match filter", true, header1.MatchesFilter(filterObj.Filter));
			AssertEquals("header2 match filter", false, header2.MatchesFilter(filterObj.Filter));
		}

		public void TestOutturnStatusFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VV1";
			header1.RegistrationStatus = "VWG";
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "VV2";
			Factory.Save();
			var filterObj = new OutturnAndGateInOutFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[OutturnAndGateInOutFilterBusinessObject.FilterConstants.OutturnStatus];
			filter.IsActive = true;
			filter.Property = "VWG";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestGateInOutCustomsStatusFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VV1";
			header1.GateInOutCustomsStatus = "VWG";
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "VV2";
			Factory.Save();
			var filterObj = new OutturnAndGateInOutFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[OutturnAndGateInOutFilterBusinessObject.FilterConstants.GateInOutCustomsStatus];
			filter.IsActive = true;
			filter.Property = "VWG";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestOutturnTypeFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VV1";
			header1.AMA_ManifestType = "VWG";
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "VV2";
			Factory.Save();
			var filterObj = new OutturnAndGateInOutFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[OutturnAndGateInOutFilterBusinessObject.FilterConstants.OutturnType];
			filter.IsActive = true;
			filter.Property = "VWG";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestGateInOutTypeFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VV1";
			header1.GateInOutMessageType = "VWG";
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "VV2";
			Factory.Save();
			var filterObj = new OutturnAndGateInOutFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[OutturnAndGateInOutFilterBusinessObject.FilterConstants.GateInOutMessageType];
			filter.IsActive = true;
			filter.Property = "VWG";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestNatureFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VV1";
			header1.AMA_Nature = "VWG";
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "VV2";
			Factory.Save();
			var filterObj = new OutturnAndGateInOutFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[OutturnAndGateInOutFilterBusinessObject.FilterConstants.Nature];
			filter.IsActive = true;
			filter.Property = "VWG";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestGateInOutDateFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VV1";
			header1.GateInOutDate = ZDateTime.Today.AddDays(-2);
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "VV2";
			header2.GateInOutDate = ZDateTime.Today;
			Factory.Save();
			var filterObj = new OutturnAndGateInOutFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[OutturnAndGateInOutFilterBusinessObject.FilterConstants.GateInOutDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(-3);
			filter.Property2 = ZDateTime.Today.AddDays(-1);
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestContainerModeFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VV1";
			header1.AMA_ContainerMode = "BBK";
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "VV2";
			header2.AMA_ContainerMode = "OTH";
			Factory.Save();
			var filterObj = new OutturnAndGateInOutFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[OutturnAndGateInOutFilterBusinessObject.FilterConstants.ContainerMode];
			filter.IsActive = true;
			filter.Property = "BBK";
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestUnpackedDateFilter()
		{
			var header1 = Factory.New<AsycudaManifestHeader>();
			header1.AMA_JobReference = "VV1";
			header1.UnpackedDate = ZDateTime.Today.AddDays(-2);
			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_JobReference = "VV2";
			header2.UnpackedDate = ZDateTime.Today;
			Factory.Save();
			var filterObj = new OutturnAndGateInOutFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[OutturnAndGateInOutFilterBusinessObject.FilterConstants.UnpackedDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDateTime.Today.AddDays(-3);
			filter.Property2 = ZDateTime.Today.AddDays(-1);
			Assert(header1.MatchesFilter(filterObj.Filter));
			Assert(!header2.MatchesFilter(filterObj.Filter));
		}

		public void TestFilters()
		{
			var filter = new OutturnAndGateInOutFilterBusinessObject();
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.JobNumber]);
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.BillNumber]);
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.CustomsOffice]);
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.EstimatedArrivalDate]);
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.EstimatedDepartureDate]);
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.TransportMode]);
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.VoyageCode]);
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.PortOfLoadingDischarge]);
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.OutturnStatus]);
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.OutturnType]);
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.GateInOutMessageType]);
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.Nature]);
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.GateInOutDate]);
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.ContainerMode]);
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.UnpackedDate]);
			AssertNotNull(filter[OutturnAndGateInOutFilterBusinessObject.FilterConstants.GateInOutCustomsStatus]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new OutturnAndGateInOutFilterBusinessObject();
	}
}
