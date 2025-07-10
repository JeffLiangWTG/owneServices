using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.OnlineSailingSchedules.PortCall;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(PortCallRequestFilterStripBusinessObject))]
	sealed class PortCallRequestFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestDefaultFilters()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TOM";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "JON";
			vessel.RV_LloydsNumber = "1234567";
			vessel.RV_RadioCallSign = "123";

			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "VOYAGE";
			voyage.JV_OH_Line = carrier.PK;

			Factory.Save();

			var etd = new ZDateTime(2017, 8, 12);
			var requestManger = new PortCallManager(Factory);
			requestManger.SetupRequest("AUSYD", etd, voyage, PortCallRequestType.Load);

			var filterStripBO = new PortCallRequestFilterStripBusinessObject(requestManger);

			var originFilter = (ModuleNkFilter)filterStripBO["Port"];
			AssertEquals("AUSYD", originFilter.Property);

			var carrierFilter = (ModuleGuidFilter)filterStripBO["Carrier"];
			AssertEquals(carrier.PK, carrierFilter.Property);

			var vesselFilter = (ModuleGuidFilter)filterStripBO["Vessel"];
			AssertEquals(vessel.PK, vesselFilter.Property);
			vesselFilter.IsActive = true;
			vesselFilter.Property = vessel.PK;

			var voyageFilter = (ModuleTextFilter)filterStripBO["Voyage"];
			AssertEquals("VOYAGE", voyageFilter.Property);

			var imoFilter = (ModuleTextFilter)filterStripBO["IMO"];
			AssertEquals("1234567", imoFilter.Property);

			var callSignFilter = (ModuleTextFilter)filterStripBO["CallSign"];
			AssertEquals("123", callSignFilter.Property);

			var dateRangeFilter = (ModuleDateFilter)filterStripBO["DateRange"];
			AssertEquals(etd, dateRangeFilter.Property1);
			AssertEquals(etd, dateRangeFilter.Property2);
		}

		public void TestBuildRequest()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "TOM";

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "JON";

			Factory.Save();

			var requestManger = new PortCallManager(Factory);
			requestManger.Request.RequestType = PortCallRequestType.Load;
			var filterStripBO = new PortCallRequestFilterStripBusinessObject(requestManger);

			var originFilter = (ModuleNkFilter)filterStripBO["Port"];
			AssertEquals(FilterVisibility.AlwaysVisible, originFilter.Visibility);
			originFilter.IsActive = true;
			originFilter.Property = "AUSYD";

			var carrierFilter = (ModuleGuidFilter)filterStripBO["Carrier"];
			AssertEquals(FilterVisibility.AlwaysVisible, carrierFilter.Visibility);
			carrierFilter.IsActive = true;
			carrierFilter.Property = carrier.PK;

			var vesselFilter = (ModuleGuidFilter)filterStripBO["Vessel"];
			AssertEquals(FilterVisibility.AlwaysVisible, vesselFilter.Visibility);
			vesselFilter.IsActive = true;
			vesselFilter.Property = vessel.PK;

			var voyageFilter = (ModuleTextFilter)filterStripBO["Voyage"];
			AssertEquals(FilterVisibility.AlwaysVisible, voyageFilter.Visibility);
			voyageFilter.IsActive = true;
			voyageFilter.Property = "ABC";

			var imoFilter = (ModuleTextFilter)filterStripBO["IMO"];
			AssertEquals(FilterVisibility.AlwaysVisible, imoFilter.Visibility);
			imoFilter.IsActive = true;
			imoFilter.Property = "12345";

			var callSignFilter = (ModuleTextFilter)filterStripBO["CallSign"];
			AssertEquals(FilterVisibility.AlwaysVisible, callSignFilter.Visibility);
			callSignFilter.IsActive = true;
			callSignFilter.Property = "CAL66";

			var dateRangeFilter = (ModuleDateFilter)filterStripBO["DateRange"];
			AssertEquals(FilterVisibility.AlwaysVisible, dateRangeFilter.Visibility);
			dateRangeFilter.IsActive = true;
			dateRangeFilter.Property1 = new ZDateTime(2016, 9, 10);
			dateRangeFilter.Property2 = new ZDateTime(2016, 9, 15);

			filterStripBO.ReBuildRequest();
			var request = requestManger.Request;

			AssertEquals(vessel.PK, request.VesselPK);
			AssertEquals("12345", request.IMO);
			AssertEquals("CAL66", request.CallSign);
			AssertEquals("AUSYD", request.Port);
			AssertEquals(carrier.PK, request.CarrierPK);
			AssertEquals("ABC", request.Voyage);
			AssertEquals(PortCallRequestType.Load, request.RequestType);
			AssertEquals(new ZDateTime(2016, 9, 10), request.StartEstimatedTime);
			AssertEquals(new ZDateTime(2016, 9, 15), request.EndEstimatedTime);
		}

		public void TestValidateIMOCallsignAndVesselFilters_ShouldHaveOneFilled()
		{
			var error = "One of Vessel, IMO, Call Sign should be filled in.";
			var requestManger = new PortCallManager(Factory);
			requestManger.Request.RequestType = PortCallRequestType.Load;
			var filterStripBO = new PortCallRequestFilterStripBusinessObject(requestManger);

			var vesselFilter = (ModuleGuidFilter)filterStripBO["Vessel"];
			vesselFilter.IsActive = true;

			var imoFilter = (ModuleTextFilter)filterStripBO["IMO"];
			imoFilter.IsActive = true;

			var callSignFilter = (ModuleTextFilter)filterStripBO["CallSign"];
			callSignFilter.IsActive = true;

			vesselFilter.Validation.ValidateProperty();
			AssertHasError(vesselFilter.PropertyInfo, error);
			AssertHasError(imoFilter.PropertyInfo, error);
			AssertHasError(callSignFilter.PropertyInfo, error);

			imoFilter.Property = "1234567";
			AssertNoErrors(vesselFilter.PropertyInfo);
			AssertNoErrors(imoFilter.PropertyInfo);
			AssertNoErrors(callSignFilter.PropertyInfo);
		}

		public void TestValidateIMOFilter()
		{
			var requestManger = new PortCallManager(Factory);
			requestManger.Request.RequestType = PortCallRequestType.Load;
			var filterStripBO = new PortCallRequestFilterStripBusinessObject(requestManger);

			var imoFilter = (ModuleTextFilter)filterStripBO["IMO"];
			imoFilter.IsActive = true;

			imoFilter.Property = "123";
			AssertHasError(imoFilter.PropertyInfo, "International Maritime Organization (IMO) number must consist of 7 characters.");

			imoFilter.Property = "1234567";
			AssertNoErrors(imoFilter.PropertyInfo);
		}

		public void TestValidateCallSignFilter()
		{
			var error = "Radio call sign of the vessel must consist of 3-7 characters.";
			var requestManger = new PortCallManager(Factory);
			requestManger.Request.RequestType = PortCallRequestType.Load;
			var filterStripBO = new PortCallRequestFilterStripBusinessObject(requestManger);

			var callSignFilter = (ModuleTextFilter)filterStripBO["CallSign"];
			callSignFilter.IsActive = true;

			callSignFilter.Property = "12";
			AssertHasError(callSignFilter.PropertyInfo, error);

			callSignFilter.Property = "12345678";
			AssertHasError(callSignFilter.PropertyInfo, error);

			callSignFilter.Property = "123";
			AssertNoErrors(callSignFilter.PropertyInfo);

			callSignFilter.Property = "1234567";
			AssertNoErrors(callSignFilter.PropertyInfo);
		}

		public void TestValidateVoyageFilter()
		{
			var error = "Voyage number should consist of 0-20 characters.";
			var requestManger = new PortCallManager(Factory);
			requestManger.Request.RequestType = PortCallRequestType.Load;
			var filterStripBO = new PortCallRequestFilterStripBusinessObject(requestManger);

			var voyageFilter = (ModuleTextFilter)filterStripBO["Voyage"];
			voyageFilter.IsActive = true;

			voyageFilter.Property = "1234567890ABCDEFGHIJK";
			AssertHasError(voyageFilter.PropertyInfo, error);

			voyageFilter.Property = "1234567890ABCDEFGHIJ";
			AssertNoErrors(voyageFilter.PropertyInfo);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			var requestManger = new PortCallManager(Factory);
			requestManger.Request.RequestType = PortCallRequestType.Load;
			return new PortCallRequestFilterStripBusinessObject(requestManger);
		}
	}
}
