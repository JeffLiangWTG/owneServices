using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Forwarding.Routing.S8.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.GUI.Test
{
	[TestedType(typeof(RoutingRequestFilterStripBusinessObject))]
	public class RoutingRequestFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Validation

		public void TestMandatoryFilters()
		{
			RoutingRequestFilterStripBusinessObject filter = new RoutingRequestFilterStripBusinessObject(new RoutingManager(Factory));

			((ModuleNkFilter)filter["Origin"]).Property = "AUSYD";
			((ModuleNkFilter)filter["Destination"]).Property = "USLAX";
			((ModuleSingleDateFilter)filter["Departure Date"]).Property1 = ZDateTime.Now;
			AssertNoErrors(((ModuleNkFilter)filter["Origin"]).PropertyInfo);
			AssertNoErrors(((ModuleNkFilter)filter["Destination"]).PropertyInfo);
			AssertNoErrors(((ModuleSingleDateFilter)filter["Departure Date"]).Property1Info);

			((ModuleNkFilter)filter["Origin"]).Property = "";
			((ModuleNkFilter)filter["Destination"]).Property = "";
			((ModuleSingleDateFilter)filter["Departure Date"]).Property1 = ZDateTime.Empty;
			AssertHasErrors(((ModuleNkFilter)filter["Origin"]).PropertyInfo);
			AssertHasErrors(((ModuleNkFilter)filter["Destination"]).PropertyInfo);
			AssertHasErrors(((ModuleSingleDateFilter)filter["Departure Date"]).Property1Info);
		}

		public void TestFilterVisibility()
		{
			RoutingRequestFilterStripBusinessObject filter = new RoutingRequestFilterStripBusinessObject(new RoutingManager(Factory));

			AssertEquals(FilterVisibility.AlwaysVisible, filter["Origin"].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filter["Destination"].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filter["Airline"].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filter["Departure Date"].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filter["Max. Connections"].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filter["Code Share/Interline"].Visibility);
			AssertEquals(FilterVisibility.AlwaysVisible, filter["Equipment Type"].Visibility);
		}

		public void TestOriginDestinationIATAValidation()
		{
			RefUNLOCO unlocoWithoutIATA = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			unlocoWithoutIATA.RL_Code = "VNXXX";
			unlocoWithoutIATA.RL_IATA = "";

			var schedulesZone1 = Factory.NewWithValidTestData<RefZoneHeader>();
			schedulesZone1.FZ_Code = "VNV1";
			schedulesZone1.FZ_ZoneType = "SCH";
			schedulesZone1.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"));
			schedulesZone1.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL"));

			var schedulesZone2 = Factory.NewWithValidTestData<RefZoneHeader>();
			schedulesZone2.FZ_Code = "VNV2";
			schedulesZone2.FZ_ZoneType = "SCH";
			schedulesZone2.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"));
			schedulesZone2.UNLOCOs.Add(unlocoWithoutIATA);

			Factory.Save();

			var filters = new RoutingRequestFilterStripBusinessObject(new RoutingManager(Factory));

			ModuleNkFilter originFilter = (ModuleNkFilter)filters["Origin"];
			originFilter.Property = "AUSYD";
			AssertNoErrors(originFilter.PropertyInfo);

			originFilter.Property = unlocoWithoutIATA.RL_Code;
			AssertHasErrors(originFilter.PropertyInfo);

			originFilter.Property = schedulesZone1.FZ_Code;
			AssertNoErrors(originFilter.PropertyInfo);

			originFilter.Property = schedulesZone2.FZ_Code;
			AssertHasErrors(originFilter.PropertyInfo);

			ModuleNkFilter destinationFilter = (ModuleNkFilter)filters["Destination"];
			destinationFilter.Property = "AUSYD";
			AssertNoErrors(destinationFilter.PropertyInfo);

			destinationFilter.Property = unlocoWithoutIATA.RL_Code;
			AssertHasErrors(destinationFilter.PropertyInfo);

			destinationFilter.Property = schedulesZone1.FZ_Code;
			AssertNoErrors(destinationFilter.PropertyInfo);

			destinationFilter.Property = schedulesZone2.FZ_Code;
			AssertHasErrors(destinationFilter.PropertyInfo);
		}

		public void TestOriginDestinationZoneValidation()
		{
			var errorMessage = "International Zone type must be SCH – Schedules.";

			var allZone = Factory.NewWithValidTestData<RefZoneHeader>();
			allZone.FZ_Code = "AUAL";
			allZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.All;
			var schZone = Factory.NewWithValidTestData<RefZoneHeader>();
			schZone.FZ_Code = "AUSC";
			schZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Schedules;
			Factory.Save();

			var filters = new RoutingRequestFilterStripBusinessObject(new RoutingManager(Factory));

			ModuleNkFilter originFilter = (ModuleNkFilter)filters["Origin"];
			originFilter.Property = "AUSC";
			AssertNoErrors(originFilter.PropertyInfo);

			originFilter.Property = "AUAL";
			AssertHasError(originFilter.PropertyInfo, errorMessage);

			ModuleNkFilter destinationFilter = (ModuleNkFilter)filters["Destination"];
			destinationFilter.Property = "AUSC";
			AssertNoErrors(destinationFilter.PropertyInfo);

			destinationFilter.Property = "AUAL";
			AssertHasError(destinationFilter.PropertyInfo, errorMessage);
		}

		public void TestConnectionsCountValidation()
		{
			var filters = new RoutingRequestFilterStripBusinessObject(new RoutingManager(Factory));
			filters.SetFilterDefaults("AAA", "BBB", new ZDateTime(2011, 11, 11), "CCC");

			var connectionsCountFilter = (RealTimeRoutingModuleNumberFilter)filters["Max. Connections"];
			connectionsCountFilter.Property = "10";
			AssertHasError(connectionsCountFilter.PropertyInfo, "Connections Count must be a number between 0 and 3.");
		}

		#endregion

		public void TestSetFilterDefaults()
		{
			var filters = new RoutingRequestFilterStripBusinessObject(new RoutingManager(Factory));
			filters.SetFilterDefaults("AAA", "BBB", new ZDateTime(2011, 11, 11), "CCC");

			AssertEquals("AAA", ((ModuleNkFilter)filters["Origin"]).Property);
			AssertEquals("BBB", ((ModuleNkFilter)filters["Destination"]).Property);
			AssertEquals("CCC", ((ModuleNkFilter)filters["Airline"]).Property);
			AssertEquals(new ZDateTime(2011, 11, 11), ((ModuleSingleDateFilter)filters["Departure Date"]).Property1);
			AssertEquals("", ((RealTimeRoutingModuleNumberFilter)filters["Max. Connections"]).Property);
		}

		#region Build Request

		public void TestBuildRequest_MandatoryFields()
		{
			RoutingRequestFilterStripBusinessObject filter = new RoutingRequestFilterStripBusinessObject(new RoutingManager(Factory));
			filter.Manager.IncludeWeeklyTimetable = false;

			ModuleNkFilter originFilter = (ModuleNkFilter)filter["Origin"];
			originFilter.IsActive = true;
			originFilter.Property = "AUSYD";

			ModuleNkFilter destFilter = (ModuleNkFilter)filter["Destination"];
			destFilter.IsActive = true;
			destFilter.Property = "INBOM";

			ModuleSingleDateFilter departureDateFilter = (ModuleSingleDateFilter)filter["Departure Date"];
			departureDateFilter.IsActive = true;
			departureDateFilter.Property1 = new ZDateTime(2008, 3, 5);

			RoutingRequest request = filter.BuildRequests().First();
			AssertEquals("AUSYD", request.OriginUNLOCOCode);
			AssertEquals("INBOM", request.DestinationUNLOCOCode);
			AssertEquals("", request.AirlineCode);
			AssertEquals(new ZDateTime(2008, 3, 5), request.DepartureDate);
			AssertEquals(false, request.IncludeWeeklyTimetable);
			AssertEquals("", request.ConnectionsCount);
		}

		public void TestBuildRequest_CodeShareInterLine()
		{
			var filter = GetDefaultFilterStripBusinessObjectForTest();

			RoutingRequest request = filter.BuildRequests().First();
			AssertEquals("", request.CodeShareInterlineOption);

			var codeShareFilter = (ModuleTextFilter)filter["Code Share/Interline"];
			codeShareFilter.IsActive = true;

			AssertEquals("Default is Single Line Only", codeShareFilter.Property, RoutingRequestFilterStripBusinessObject.CodeShareInterLineConstants.SingleLineOnly);

			codeShareFilter.Property = RoutingRequestFilterStripBusinessObject.CodeShareInterLineConstants.SingleLineAndCodeShare;
			request = filter.BuildRequests().First();
			AssertEquals(".", request.CodeShareInterlineOption);

			codeShareFilter.Property = RoutingRequestFilterStripBusinessObject.CodeShareInterLineConstants.SingleLineOnly;
			request = filter.BuildRequests().First();
			AssertEquals("C", request.CodeShareInterlineOption);

			codeShareFilter.Property = RoutingRequestFilterStripBusinessObject.CodeShareInterLineConstants.InterLineAndCodeShare;
			request = filter.BuildRequests().First();
			AssertEquals("X", request.CodeShareInterlineOption);

			codeShareFilter.Property = RoutingRequestFilterStripBusinessObject.CodeShareInterLineConstants.InterLineOnly;
			request = filter.BuildRequests().First();
			AssertEquals("Y", request.CodeShareInterlineOption);
		}

		public void TestBuildRequest_CargoPassengerFlight()
		{
			var filter = GetDefaultFilterStripBusinessObjectForTest();

			RoutingRequest request = filter.BuildRequests().First();
			AssertEquals("", request.CargoPassengerFlightOption);

			ModuleTextFilter flightTypeFilter = (ModuleTextFilter)filter["Flight Type"];
			flightTypeFilter.IsActive = true;

			flightTypeFilter.Property = FlightTypeConstants.AllFlightTypes;
			request = filter.BuildRequests().First();
			AssertEquals("", request.CargoPassengerFlightOption);

			flightTypeFilter.Property = FlightTypeConstants.CargoOnly;
			request = filter.BuildRequests().First();
			AssertEquals("C", request.CargoPassengerFlightOption);

			flightTypeFilter.Property = FlightTypeConstants.PassengerOnly;
			request = filter.BuildRequests().First();
			AssertEquals("P", request.CargoPassengerFlightOption);
		}

		public void TestBuildRequest_EqupmentType()
		{
			var filter = GetDefaultFilterStripBusinessObjectForTest();

			RoutingRequest request = filter.BuildRequests().First();
			AssertEquals("", request.EquipmentType);

			ModuleTextFilter equipmentTypeFilter = (ModuleTextFilter)filter["Equipment Type"];
			equipmentTypeFilter.IsActive = true;

			AssertEquals("Default is WDF", equipmentTypeFilter.Property, RoutingRequestFilterStripBusinessObject.EquipmentTypeConstants.WideBodyAndFreighter);

			equipmentTypeFilter.Property = RoutingRequestFilterStripBusinessObject.EquipmentTypeConstants.AllEquipmentTypes;
			request = filter.BuildRequests().First();
			AssertEquals("", request.EquipmentType);

			equipmentTypeFilter.Property = RoutingRequestFilterStripBusinessObject.EquipmentTypeConstants.WideBody;
			request = filter.BuildRequests().First();
			AssertEquals("W", request.EquipmentType);

			equipmentTypeFilter.Property = RoutingRequestFilterStripBusinessObject.EquipmentTypeConstants.Freighter;
			request = filter.BuildRequests().First();
			AssertEquals("F", request.EquipmentType);

			equipmentTypeFilter.Property = RoutingRequestFilterStripBusinessObject.EquipmentTypeConstants.WideBodyAndFreighter;
			request = filter.BuildRequests().First();
			AssertEquals("B", request.EquipmentType);
		}

		public void TestBuildRequest_MinimumConnectionTime()
		{
			var filter = GetDefaultFilterStripBusinessObjectForTest();

			var request = filter.BuildRequests().First();
			AssertEquals(0, request.MinimumConnectionTime);

			var minimumConnectionTimeFilter = (ModuleTextFilter)filter["Minimum Connection Time (hours)"];
			minimumConnectionTimeFilter.IsActive = true;

			minimumConnectionTimeFilter.Property = "3";
			request = filter.BuildRequests().First();
			AssertEquals(3, request.MinimumConnectionTime);

			minimumConnectionTimeFilter.Property = "Hello";
			request = filter.BuildRequests().First();
			AssertEquals(0, request.MinimumConnectionTime);
		}

		public void TestBuildRequest_IncludeWeeklyTimetable()
		{
			var filter = GetDefaultFilterStripBusinessObjectForTest();
			filter.Manager.IncludeWeeklyTimetable = false;

			var request = filter.BuildRequests().First();
			AssertEquals(false, request.IncludeWeeklyTimetable);

			filter.Manager.IncludeWeeklyTimetable = true;

			request = filter.BuildRequests().First();
			AssertEquals(true, request.IncludeWeeklyTimetable);
		}

		public void TestBuildRequest_IncludeCO2EmissionValue()
		{
			var filter = GetDefaultFilterStripBusinessObjectForTest();

			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				using (FreightDataRegistry.Instance.EnableGlobalFlightSchedulesCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals(expected: false, filter.BuildRequests().First().IncludeCO2EmissionValue);
				}

				using (FreightDataRegistry.Instance.EnableGlobalFlightSchedulesCalculation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals(expected: true, filter.BuildRequests().First().IncludeCO2EmissionValue);
				}
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			{
				AssertEquals(expected: false, filter.BuildRequests().First().IncludeCO2EmissionValue);
			}
		}

		public void TestBuildRequest_ConnectionsCount()
		{
			var filter = GetDefaultFilterStripBusinessObjectForTest();

			var request = filter.BuildRequests().First();
			AssertEquals("", request.ConnectionsCount);

			var connectionsCountFilter = (RealTimeRoutingModuleNumberFilter)filter["Max. Connections"];
			connectionsCountFilter.IsActive = true;

			connectionsCountFilter.Property = "1";
			request = filter.BuildRequests().First();
			AssertEquals("1", request.ConnectionsCount);
		}

		public void TestBuildRequest_InternationalZone()
		{
			var zone = Factory.NewWithValidTestData<RefZoneHeader>();
			zone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Schedules;
			zone.FZ_Code = "AUAU";
			zone.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD"));
			zone.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL"));
			zone.UNLOCOs.Add(Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE"));
			Factory.Save();

			var filter = GetDefaultFilterStripBusinessObjectForTest();
			ModuleNkFilter originFilter = (ModuleNkFilter)filter["Origin"];
			ModuleNkFilter destFilter = (ModuleNkFilter)filter["Destination"];

			originFilter.Property = "AUAU";
			destFilter.Property = "VNVIN";

			var requests = filter.BuildRequests();
			CombineAssertions(() =>
			{
				AssertEquals(3, requests.Count);
				Assert(requests.All(r => r.DestinationUNLOCOCode == "VNVIN"));
				AssertContainsExactElementsInAnyOrder(new[] { "AUSYD", "AUMEL", "AUBNE" }, requests.Select(r => r.OriginUNLOCOCode));
			});

			originFilter.Property = "VNVIN";
			destFilter.Property = "AUAU";

			requests = filter.BuildRequests();
			CombineAssertions(() =>
			{
				AssertEquals(3, requests.Count);
				Assert(requests.All(r => r.OriginUNLOCOCode == "VNVIN"));
				AssertContainsExactElementsInAnyOrder(new[] { "AUSYD", "AUMEL", "AUBNE" }, requests.Select(r => r.DestinationUNLOCOCode));
			});
		}

		public void TestAreOriginDestinationBothZone()
		{
			var filter = new RoutingRequestFilterStripBusinessObject(new RoutingManager(Factory));
			var originFilter = (ModuleNkFilter)filter["Origin"];
			originFilter.IsActive = true;
			var destFilter = (ModuleNkFilter)filter["Destination"];
			destFilter.IsActive = true;

			originFilter.Property = "AUSYD";
			destFilter.Property = "INBOM";
			AssertEquals(false, filter.AreOriginDestinationBothZone());

			originFilter.Property = "AUAU";
			destFilter.Property = "INBOM";
			AssertEquals(false, filter.AreOriginDestinationBothZone());

			originFilter.Property = "AUSYD";
			destFilter.Property = "AUAU";
			AssertEquals(false, filter.AreOriginDestinationBothZone());

			originFilter.Property = "AUAU";
			destFilter.Property = "AUSC";
			AssertEquals(true, filter.AreOriginDestinationBothZone());
		}

		#endregion

		#region Implementation

		RoutingRequestFilterStripBusinessObject GetDefaultFilterStripBusinessObjectForTest()
		{
			var filter = new RoutingRequestFilterStripBusinessObject(new RoutingManager(Factory));
			var originFilter = (ModuleNkFilter)filter["Origin"];
			originFilter.IsActive = true;
			originFilter.Property = "AUSYD";
			var destFilter = (ModuleNkFilter)filter["Destination"];
			destFilter.IsActive = true;
			destFilter.Property = "VNVIN";

			return filter;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RoutingRequestFilterStripBusinessObject(new RoutingManager(Factory));
		}

		#endregion
	}
}
