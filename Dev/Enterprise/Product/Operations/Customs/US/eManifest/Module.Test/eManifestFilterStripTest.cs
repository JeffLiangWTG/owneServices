using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Module.Testing
{
	[TestedType(typeof(eManifestFilterStrip))]
	sealed class eManifestFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestModuleFilter()
		{
			var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany.GC_Code = "t1";
			glbCompany.GC_RN_NKCountryCode = "US";
			var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			glbCompany2.GC_Code = "t2";
			glbCompany2.GC_RN_NKCountryCode = "US";
			var branch = glbCompany.Branches.AddNew();
			branch.GB_GC = glbCompany.PK;
			branch.GB_Code = "Ts1";
			var branch2 = glbCompany.Branches.AddNew();
			branch2.GB_GC = glbCompany.PK;
			branch2.GB_Code = "Ts2";
			var branch3 = glbCompany.Branches.AddNew();
			branch3.GB_GC = glbCompany2.PK;
			branch3.GB_Code = "Ts3";
			var trip1 = Factory.NewWithValidTestData<Trip>();
			trip1.BH_GB = branch.PK;
			trip1.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			var trip2 = Factory.NewWithValidTestData<Trip>();
			trip2.BH_GB = branch2.PK;
			trip2.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			var trip3 = Factory.NewWithValidTestData<Trip>();
			trip3.BH_GB = branch3.PK;
			trip3.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.eManifest;
			Factory.Save();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var filter = new eManifestFilterStrip().Filter;
				var trips = Factory.Load<Trip>(filter);
				AssertEquals(2, trips.Length);
				Assert(trips.Cast<Trip>().Any(x => x.PK == trip1.PK));
				Assert(trips.Cast<Trip>().Any(x => x.PK == trip2.PK));
				AssertEquals(false, trips.Cast<Trip>().Any(x => x.PK == trip3.PK));
			}
		}

		public void TestTripReference()
		{
			var filter = (ModuleTextFilter)FilterStrip[eManifestFilterStrip.Descriptions.TripReferenceFilterId];
			AssertNotNull("Trip Reference filter", filter);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Equal, "T1", trip1);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotEqual, "T1", trip2, trip3, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.StartsWith, "T", trip1, trip2, trip3, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.DoesNotStartWith, "T");
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Contains, "2", trip2);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotContains, "2", trip1, trip3, trip4);
		}

		public void TestJobReference()
		{
			var filter = (ModuleTextFilter)FilterStrip[eManifestFilterStrip.Descriptions.JobReferenceFilterId];
			AssertNotNull("JobReference filter", filter);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Equal, "MAN0000001", trip1);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotEqual, "MAN0000001", trip2, trip3, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.StartsWith, "MAN", trip1, trip2, trip3, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.DoesNotStartWith, "MAN");
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Contains, "2", trip2, trip3);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotContains, "2", trip1, trip4);
			AssertNoBlankFilter(filter);
		}

		public void TestCarrierCode()
		{
			var filter = (ModuleTextFilter)FilterStrip[eManifestFilterStrip.Descriptions.CarrierCodeFilterId];
			AssertNotNull("Carrier Code filter", filter);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Equal, "CAR1", trip1);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotEqual, "CAR1", trip2, trip3, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.StartsWith, "CAR", trip1);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.DoesNotStartWith, "CAR", trip2, trip3, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Contains, "A2", trip2, trip3);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotContains, "A2", trip1, trip4);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsBlank, string.Empty, trip4);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsNotBlank, string.Empty, trip1, trip2, trip3);
		}

		public void TestConveyance()
		{
			var filter = (ModuleTextFilter)FilterStrip[eManifestFilterStrip.Descriptions.ConveyanceFilterId];
			AssertNotNull("Conveyance filter", filter);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Equal, "AABB32", trip1);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotEqual, "AABB32", trip2, trip3, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.StartsWith, "AA", trip1, trip2);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.DoesNotStartWith, "AA", trip3, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Contains, "XX", trip2, trip3);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotContains, "XX", trip1, trip4);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsBlank, string.Empty, trip4);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsNotBlank, string.Empty, trip1, trip2, trip3);
		}

		[TestDate(2012, 02, 27)]
		public void TestEstimatedDateOfArrival()
		{
			var filter = (ModuleDateFilter)FilterStrip[eManifestFilterStrip.Descriptions.EstimatedDateOfArrivalFilterId];
			AssertNotNull("Estimated Date Of Arrival filter", filter);
			AssertProperTripsLoaded(filter, ModuleDateFilter.HasDateEntered, ZDateTime.Empty, ZDateTime.Empty, trip1, trip2, trip3);
			AssertProperTripsLoaded(filter, ModuleDateFilter.HasNoDateEntered, ZDateTime.Empty, ZDateTime.Empty, trip4);
			AssertProperTripsLoaded(filter, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Now, ZDateTime.Now.AddDays(3), trip1, trip3);
			AssertProperTripsLoaded(filter, ModuleDateFilter.SpecifiedDateRange, ZDateTime.Now.AddDays(4), ZDateTime.Now.AddDays(6), trip2);
		}

		public void TestFirstExpectedPortOfArrival()
		{
			var filter = (ModuleTextAndNkFilter)FilterStrip[eManifestFilterStrip.Descriptions.FirstExpectedPortOfArrivalFilterId];
			AssertNotNull("First Expected Port Of Arrival filter", filter);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Equal, "USLA2", trip1);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotEqual, "USLA2", trip2, trip3, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.StartsWith, "USLA", trip1, trip2);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.DoesNotStartWith, "USLA", trip3, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Contains, "SLA", trip1, trip2);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotContains, "SLA", trip3, trip4);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsBlank, string.Empty, trip4);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsNotBlank, string.Empty, trip1, trip2, trip3);
		}

		public void TestFirstExpectedPortOfArrivalScheduleD()
		{
			var filter = (ModuleTextAndNkFilter)FilterStrip[eManifestFilterStrip.Descriptions.FirstExpectedPortOfArrivalScheduleDFilterId];
			AssertNotNull("First Expected Port Of Arrival (Schedule D) filter", filter);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Equal, "2704", trip1);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotEqual, "2704", trip2, trip3, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.StartsWith, "2", trip1, trip2);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.DoesNotStartWith, "2", trip3, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Contains, "90", trip2, trip3);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotContains, "90", trip1, trip4);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsBlank, string.Empty, trip4);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsNotBlank, string.Empty, trip1, trip2, trip3);
		}

		public void TestTransitDirection()
		{
			var filter = (ModuleTextFilter)FilterStrip[eManifestFilterStrip.Descriptions.TransitDirectionFilterId];
			AssertNotNull("Transit Direction filter", filter);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Equal, TransitDirectionCodes.Codes.Importation, trip1, trip2);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotEqual, TransitDirectionCodes.Codes.Importation, trip3, trip4);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsBlank, string.Empty, trip4);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsNotBlank, string.Empty, trip1, trip2, trip3);
			AssertEqualFilter(filter);
		}

		public void TestMessageStatus()
		{
			var filter = (ModuleTextFilter)FilterStrip[eManifestFilterStrip.Descriptions.MessageStatusFilterId];
			AssertNotNull("Message Status filter", filter);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Equal, MessageStatusList.Codes.AwaitingChange, trip1, trip3);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotEqual, MessageStatusList.Codes.AwaitingChange, trip2, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Equal, MessageStatusList.Codes.NotSent, trip2, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotEqual, MessageStatusList.Codes.NotSent, trip1, trip3);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsBlank, string.Empty, trip2, trip4);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsNotBlank, string.Empty, trip1, trip3);
			AssertEqualFilter(filter);
		}

		public void TestReleaseStatus()
		{
			var filter = (ModuleTextFilter)FilterStrip[eManifestFilterStrip.Descriptions.ReleaseStatusFilterId];
			AssertNotNull("Release Status filter", filter);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Equal, TripEntryStatusList.Codes.TripArrived, trip1, trip2);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotEqual, TripEntryStatusList.Codes.TripArrived, trip3, trip4);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsBlank, string.Empty, trip3, trip4);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsNotBlank, string.Empty, trip1, trip2);
			AssertEqualFilter(filter);
		}

		public void TestShipmentReleaseStatus()
		{
			var filter = (ModuleTextFilter)FilterStrip[eManifestFilterStrip.Descriptions.ShipmentReleaseStatusFilterId];
			AssertNotNull("Release Status filter", filter);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Equal, ShipmentEntryStatusList.Codes.Released, trip1, trip2);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotEqual, ShipmentEntryStatusList.Codes.Released, trip3, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Equal, ShipmentEntryStatusList.Codes.ShipmentHold, trip1);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotEqual, ShipmentEntryStatusList.Codes.ShipmentHold, trip2, trip3, trip4);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.Equal, ShipmentEntryStatusList.Codes.ShipmentRemoveHold, trip2, trip3);
			AssertProperTripsLoaded(filter, SQLComparisonOperator.NotEqual, ShipmentEntryStatusList.Codes.ShipmentRemoveHold, trip1, trip4);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsBlank, string.Empty, trip3, trip4);
			AssertProperTripsLoaded(filter, SpecialComparisonOperator.IsNotBlank, string.Empty, trip1, trip2, trip3);
			AssertEqualFilter(filter);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new eManifestFilterStrip();

		protected override void SetUp()
		{
			base.SetUp();
			trip1 = Factory.New<Trip>();
			trip1.BH_CarrierSCAC = "CAR1";
			trip1.BH_ImportTransportMode = TransportModes.Codes.Road;
			trip1.BH_JobReference = "MAN0000001";
			trip1.BH_VoyageNumber = "T1";
			trip1.BH_ETA = new ZDateTime(2012, 02, 27, 17, 09, 00);
			trip1.BH_RL_NKPortUnlading = "USLA2";
			trip1.BH_PortUnladingDCode = "2704";
			trip1.BH_TransitDirection = TransitDirectionCodes.Codes.Importation;
			trip1.BH_MessageStatus = MessageStatusList.Codes.AwaitingChange;
			trip1.BH_ReleaseStatus = TripEntryStatusList.Codes.TripArrived;
			trip1.Shipments.AddNew().B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Released;
			trip1.Shipments.AddNew().B0_ReleaseStatus = ShipmentEntryStatusList.Codes.ShipmentHold;
			AddRefEquipment(trip1.Conveyance, "AABB32");
			trip2 = Factory.New<Trip>();
			trip2.BH_CarrierSCAC = "CA22";
			trip2.BH_ImportTransportMode = TransportModes.Codes.Road;
			trip2.BH_JobReference = "MAN0000002";
			trip2.BH_VoyageNumber = "T2";
			trip2.BH_ETA = new ZDateTime(2012, 03, 03, 17, 09, 00);
			trip2.BH_RL_NKPortUnlading = "USLA3";
			trip2.BH_PortUnladingDCode = "2901";
			trip2.BH_TransitDirection = TransitDirectionCodes.Codes.Importation;
			trip2.BH_MessageStatus = MessageStatusList.Codes.NotSent;
			trip2.BH_ReleaseStatus = TripEntryStatusList.Codes.TripArrived;
			trip2.Shipments.AddNew().B0_ReleaseStatus = ShipmentEntryStatusList.Codes.Released;
			trip2.Shipments.AddNew().B0_ReleaseStatus = ShipmentEntryStatusList.Codes.ShipmentRemoveHold;
			AddRefEquipment(trip2.Conveyance, "AAXX73");
			trip3 = Factory.New<Trip>();
			trip3.BH_CarrierSCAC = "CA23";
			trip3.BH_ImportTransportMode = TransportModes.Codes.Road;
			trip3.BH_JobReference = "MAN0000273";
			trip3.BH_VoyageNumber = "T3";
			trip3.BH_ETA = new ZDateTime(2012, 02, 28, 17, 09, 00);
			trip3.BH_RL_NKPortUnlading = "USLI4";
			trip3.BH_PortUnladingDCode = "4908";
			trip3.BH_TransitDirection = TransitDirectionCodes.Codes.Transit;
			trip3.BH_MessageStatus = MessageStatusList.Codes.AwaitingChange;
			trip3.Shipments.AddNew();
			trip3.Shipments.AddNew().B0_ReleaseStatus = ShipmentEntryStatusList.Codes.ShipmentRemoveHold;
			AddRefEquipment(trip3.Conveyance, "XXRQ81");
			trip4 = Factory.New<Trip>();
			trip4.BH_ETA = ZDateTime.Empty;
			trip4.BH_JobReference = "MAN0000004";
			trip4.BH_VoyageNumber = "T4";
			trip4.BH_TransitDirection = ZString.Empty;
			Factory.Save();
		}

		void AssertProperTripsLoaded(ModuleTextBaseFilter filter, SQLComparisonOperator comparisonOperator, string property, params Trip[] expectedTrips)
		{
			filter.SqlComparisonOperator = comparisonOperator;
			filter.Property = property;
			AssertProperTripsLoaded(filter, expectedTrips);
		}

		void AssertProperTripsLoaded(ModuleDateFilter filter, ZString propertySearch, ZDateTime value1, ZDateTime value2, params Trip[] expectedTrips)
		{
			filter.PropertySearch = propertySearch;
			filter.Property1 = value1;
			filter.Property2 = value2;
			AssertProperTripsLoaded(filter, expectedTrips);
		}

		void AssertProperTripsLoaded(ModuleFilter filter, Trip[] expectedTrips)
		{
			var trips = Factory.Load<Trip>(filter.Query);
			AssertEquals("Proper quantity of trips loaded", expectedTrips.Length, trips.Length);
			foreach (var trip in expectedTrips)
			{
				Assert("Trip should be loaded", trips.Any(s => s.PK == trip.PK));
			}
		}

		internal static void AssertNoBlankFilter<T>(ModuleFilterWithListAndComparisonOperators<T> filter) where T : IZType
		{
			AssertNotSupportedComparisonOperators(
				filter,
				ModuleTextFilter.ComparisonConstants.IsBlank,
				ModuleTextFilter.ComparisonConstants.IsNotBlank);
		}

		internal static void AssertEqualFilter<T>(ModuleFilterWithListAndComparisonOperators<T> filter) where T : IZType
		{
			AssertNotSupportedComparisonOperators(
				filter,
				ModuleTextFilter.ComparisonConstants.Contains,
				ModuleTextFilter.ComparisonConstants.NotContain,
				ModuleTextFilter.ComparisonConstants.StartsWith,
				ModuleTextFilter.ComparisonConstants.NotStartsWith);
		}

		static void AssertNotSupportedComparisonOperators<T>(ModuleFilterWithListAndComparisonOperators<T> filter, params string[] comparisonOperators) where T : IZType
		{
			foreach (var comparisonOperator in comparisonOperators)
			{
				AssertEquals(string.Format("Comparison operator '{0}' should not be supported", comparisonOperator),
					false, filter.AllowedComparisonOperators.Contains(comparisonOperator));
			}
		}

		internal static void AddRefEquipment(Equipment conveyance, string registration)
		{
			var refEquipment = conveyance.Factory.NewWithValidTestData<RefEquipment>();
			refEquipment.RQ_Registration = registration;
			conveyance.BJ_RQ_Equipment = refEquipment.PK;
		}

		eManifestFilterStrip FilterStrip => filterStrip ?? (filterStrip = new eManifestFilterStrip());

		eManifestFilterStrip filterStrip;
		Trip trip1;
		Trip trip2;
		Trip trip3;
		Trip trip4;
	}
}
