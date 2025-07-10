using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(RelatedTransportLegsFilterBusinessObject))]
	public class TransportLegsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestVoyageOrFlightFilter()
		{
			var consol1 = NewConsol("mwhc1", false, "USNYC", "SGSIN", "AUSYD", "NZAKA");
			var consol2 = NewConsol("mwhc2", false, "USNYC", "SGSIN", "AUSYD");

			var transports1 = consol1.Transports[0];
			var transports2 = consol1.Transports[1];
			var transports3 = consol1.Transports[2];

			var transports4 = consol2.Transports[0];
			var transports5 = consol2.Transports[1];

			transports1.JW_VoyageFlight = "aaa";
			transports2.JW_VoyageFlight = "bbb";
			transports3.JW_VoyageFlight = "ccc";

			transports4.JW_VoyageFlight = "aaa";
			transports5.JW_VoyageFlight = "ddd";
			Factory.Save();

			var transportFilter = (ModuleTextFilter)FilterStripBizO["Voyage / Flight"];

			var results = new TransportNonDependentCollection(Factory);

			transportFilter.IsActive = true;

			transportFilter.Property = "";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports1, transports2, transports3, transports4, transports5 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = "aaa";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports1, transports4 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = "ddd";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports5 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = "eee";
			results.Load(FilterStripBizO.Filter);
			AssertEquals(0, results.Count);
		}

		public void TestVesselOrJourneyNameFilter()
		{
			var consol1 = NewConsol("mwhc1", false, "USNYC", "SGSIN", "AUSYD", "NZAKA");
			var consol2 = NewConsol("mwhc2", false, "USNYC", "SGSIN", "AUSYD");

			var transports1 = consol1.Transports[0];
			var transports2 = consol1.Transports[1];
			var transports3 = consol1.Transports[2];

			var transports4 = consol2.Transports[0];
			var transports5 = consol2.Transports[1];

			transports1.JW_Vessel = "aaa";
			transports2.JW_Vessel = "bbb";
			transports3.JW_Vessel = "ccc";

			transports4.JW_Vessel = "aaa";
			transports5.JW_Vessel = "ddd";
			Factory.Save();

			var transportFilter = (ModuleNkFilter)FilterStripBizO["Vessel / Journey Name"];

			var results = new TransportNonDependentCollection(Factory);

			transportFilter.IsActive = true;

			transportFilter.Property = "";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports1, transports2, transports3, transports4, transports5 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = "aaa";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports1, transports4 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = "ddd";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports5 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = "eee";
			results.Load(FilterStripBizO.Filter);
			AssertEquals(0, results.Count);
		}

		[TestDate(2016, 12, 2)]
		public void TestATAFilter()
		{
			TestDateTimeFilter(JobConsolTransportSchema.JW_ATA.Name, "ATA");
		}

		[TestDate(2016, 12, 2)]
		public void TestATDFilter()
		{
			TestDateTimeFilter(JobConsolTransportSchema.JW_ATD.Name, "ATD");
		}

		[TestDate(2016, 12, 2)]
		public void TestETAFilter()
		{
			TestDateTimeFilter(JobConsolTransportSchema.JW_ETA.Name, "ETA");
		}

		[TestDate(2016, 12, 2)]
		public void TestETDFilter()
		{
			TestDateTimeFilter(JobConsolTransportSchema.JW_ETD.Name, "ETD");
		}

		[TestDate(2016, 12, 2)]
		public void TestCTOCutOffFilter()
		{
			TestCommonDateTimeFilter(JobConsolTransportSchema.JW_TerminalCutOff.Name, "CTO Cut Off");
		}

		[TestDate(2016, 12, 2)]
		public void TestCFSCutOffFilter()
		{
			TestCommonDateTimeFilter(JobConsolTransportSchema.JW_DepotCutOff.Name, "CFS Cut Off");
		}

		[TestDate(2016, 12, 2)]
		public void TestDocsDueFilter()
		{
			TestCommonDateTimeFilter(JobConsolTransportSchema.JW_DocumentaryCutOff.Name, "Docs Due");
		}

		[TestDate(2016, 12, 2)]
		public void TestVGMCutOffFilter()
		{
			TestCommonDateTimeFilter(JobConsolTransportSchema.JW_VGMCutOff.Name, "VGM Cut Off");
		}

		[TestDate(2016, 12, 2)]
		public void TestCTOReceivalFilter()
		{
			TestCommonDateTimeFilter(JobConsolTransportSchema.JW_TerminalReceivalCommences.Name, "CTO Receival");
		}

		[TestDate(2016, 12, 2)]
		public void TestCFSReceivalFilter()
		{
			TestCommonDateTimeFilter(JobConsolTransportSchema.JW_DepotReceivalCommences.Name, "CFS Receival");
		}

		[TestDate(2016, 12, 2)]
		public void TestCTOAvailableFilter()
		{
			TestCommonDateTimeFilter(JobConsolTransportSchema.JW_TerminalAvailabilityDate.Name, "CTO Available");
		}

		[TestDate(2016, 12, 2)]
		public void TestCFSAvailableFilter()
		{
			TestCommonDateTimeFilter(JobConsolTransportSchema.JW_DepotAvailabilityDate.Name, "CFS Available");
		}

		[TestDate(2016, 12, 2)]
		public void TestCTOStorageFilter()
		{
			TestCommonDateTimeFilter(JobConsolTransportSchema.JW_TerminalStorageDate.Name, "CTO Storage");
		}

		[TestDate(2016, 12, 2)]
		public void TestCFSStorageFilter()
		{
			TestCommonDateTimeFilter(JobConsolTransportSchema.JW_DepotStorageDate.Name, "CFS Storage");
		}

		void TestCommonDateTimeFilter(ZString dateProperty, ZString filterProperty)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var transports1 = consol.Transports.AddNew();
			transports1[dateProperty] = new ZDateTime(2007, 1, 1, 10, 0, 0);
			var transports2 = consol.Transports.AddNew();
			transports2[dateProperty] = new ZDateTime(2016, 12, 1, 23, 59, 59);
			var transports3 = consol.Transports.AddNew();
			transports3[dateProperty] = new ZDateTime(2016, 12, 3, 0, 0, 0);
			var transports4 = consol.Transports.AddNew();
			transports4[dateProperty] = new ZDateTime(2017, 1, 1, 11, 0, 0);

			Factory.Save();

			var transportFilter = (ModuleDateFilter)FilterStripBizO[filterProperty];
			var results = new TransportNonDependentCollection(Factory);
			transportFilter.IsActive = true;
			transportFilter.PropertySearch = ModuleDateFilter.Past;
			results.Load(FilterStripBizO.Filter);
			Assert(results.Contains(transports1));

			AssertContainsExactElementsInAnyOrder(new[] { transports1, transports2 }.Select(x => x.PK), results.Select(x => x.PK));
		}

		public void TestloadDischargePortsFilter()
		{
			var consol1 = NewConsol("mwhc1", false, "USNYC", "SGSIN", "AUSYD", "NZAKA");
			var consol2 = NewConsol("mwhc2", false, "USNYC", "SGSIN", "AUSYD");

			var transports1 = consol1.Transports[0];
			var transports2 = consol1.Transports[1];
			var transports3 = consol1.Transports[2];

			var transports4 = consol2.Transports[0];
			var transports5 = consol2.Transports[1];
			Factory.Save();

			var transportFilter = (ModuleLocationFilter)FilterStripBizO["Load / Discharge"];
			var results = new TransportNonDependentCollection(Factory);

			transportFilter.IsActive = true;

			transportFilter.Property1 = "";
			transportFilter.Property2 = "";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports1, transports2, transports3, transports4, transports5 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property1 = "SGSIN";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports2, transports5 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property1 = "";
			transportFilter.Property2 = "AUSYD";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports2, transports5 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property1 = "AUSYD";
			transportFilter.Property2 = "NZAKA";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports3 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property1 = "NZAKA";
			transportFilter.Property2 = "SGSIN";
			results.Load(FilterStripBizO.Filter);
			AssertEquals(0, results.Count);
		}

		public void TestCarrierOrProviderFilter()
		{
			var consol1 = NewConsol("mwhc1", false, "USNYC", "SGSIN", "AUSYD", "NZAKA");
			var org1 = Factory.NewWithValidTestData<OrgHeader>();

			var consol2 = NewConsol("mwhc2", false, "USNYC", "SGSIN", "AUSYD");
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_IsCargoOnly = true;
			voyage.JV_VoyageFlight = "DI56";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "USNYC";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";
			voyage.GenerateSailings();
			voyage.JV_OH_Line = org1.PK;

			var transport1 = consol1.Transports[0];
			transport1.JW_JX = voyage.Sailings[0].PK;
			transport1.JW_IsLinked = true;

			var transport2 = consol1.Transports[1];
			transport2.JW_OA_CarrierAddress = org2.MainAddress.PK;
			var transport3 = consol1.Transports[2];
			transport3.JW_OA_CarrierAddress = ZGuid.Empty;

			var transports4 = consol2.Transports[0];
			transports4.JW_OA_CarrierAddress = org1.MainAddress.PK;

			var transports5 = consol2.Transports[1];
			transports5.JW_OA_CarrierAddress = ZGuid.Empty;

			Factory.Save();

			var transportFilter = (ModuleGuidFilter)FilterStripBizO["Carrier / Provider"];
			var results = new TransportNonDependentCollection(Factory);

			transportFilter.IsActive = true;

			transportFilter.Property = ZGuid.Empty;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transport1, transport2, transport3, transports4, transports5 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = org1.PK;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transport1, transports4 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = org2.PK;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transport2 }.Select(x => x.PK), results.Select(x => x.PK));
		}

		public void TestTransportModeFilter()
		{
			var consol1 = NewConsol("mwhc1", false, "USNYC", "SGSIN", "AUSYD", "NZAKA");
			var consol2 = NewConsol("mwhc2", false, "USNYC", "SGSIN", "AUSYD");

			var transports1 = consol1.Transports[0];
			var transports2 = consol1.Transports[1];
			var transports3 = consol1.Transports[2];

			var transports4 = consol2.Transports[0];
			var transports5 = consol2.Transports[1];

			transports1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transports2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transports3.JW_TransportMode = "";

			transports4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transports5.JW_TransportMode = Core.Constants.TransportModes.Road;
			Factory.Save();

			var transportFilter = (ModuleTextFilter)FilterStripBizO["Transport Mode"];

			var results = new TransportNonDependentCollection(Factory);

			transportFilter.IsActive = true;

			transportFilter.Property = "";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports1, transports2, transports3, transports4, transports5 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = Core.Constants.TransportModes.Sea;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports1, transports4 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = Core.Constants.TransportModes.Air;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports2 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = Core.Constants.TransportModes.Rail;
			results.Load(FilterStripBizO.Filter);
			AssertEquals(0, results.Count);
		}

		public void TestAdditionalTransportModeFilter()
		{
			var consol1 = NewConsol("mwhc1", false, "USNYC", "SGSIN", "AUSYD", "NZAKA");
			var consol2 = NewConsol("mwhc2", false, "USNYC", "SGSIN", "AUSYD", "AUMEL");
			var consol3 = NewConsol("mwhc3", false, "USNYC", "SGSIN", "AUSYD", "AUBNE");

			var transports1 = consol1.Transports[0];
			var transports2 = consol1.Transports[1];
			var transports3 = consol1.Transports[2];

			var transports4 = consol2.Transports[0];
			var transports5 = consol2.Transports[1];
			var transports6 = consol2.Transports[2];

			var transports7 = consol3.Transports[0];
			var transports8 = consol3.Transports[1];
			var transports9 = consol3.Transports[2];

			transports1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transports2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transports3.JW_TransportMode = ZString.Empty;

			transports4.JW_TransportMode = Core.Constants.TransportModes.Rail;
			transports4.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			transports4.JW_AdditionalTransportMode = Core.Constants.TransportModes.Road;
			transports5.JW_TransportMode = Core.Constants.TransportModes.Road;
			transports6.JW_TransportMode = Core.Constants.TransportModes.Storage;

			transports7.JW_TransportMode = Core.Constants.TransportModes.Rail;
			transports7.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			transports7.JW_AdditionalTransportMode = ZString.Empty;
			transports8.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			transports8.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			transports8.JW_AdditionalTransportMode = Core.Constants.TransportModes.Rail;
			transports9.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			transports9.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			transports9.JW_AdditionalTransportMode = Core.Constants.TransportModes.Road;

			Factory.Save();

			var transportFilter = (ModuleTextFilter)FilterStripBizO["Additional Transport Mode"];

			var results = new TransportNonDependentCollection(Factory);

			transportFilter.IsActive = true;

			transportFilter.Property = "";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				transports1, transports2, transports3,
				transports4, transports5, transports6,
				transports7, transports8, transports9
			}.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = Core.Constants.TransportModes.Rail;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports8 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = Core.Constants.TransportModes.Road;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports4, transports9 }.Select(x => x.PK), results.Select(x => x.PK));
		}

		public void TestTransportTypeFilter()
		{
			var consol1 = NewConsol("mwhc1", false, "USNYC", "SGSIN", "AUSYD", "NZAKA");
			var consol2 = NewConsol("mwhc2", false, "USNYC", "SGSIN", "AUSYD");

			var transports1 = consol1.Transports[0];
			var transports2 = consol1.Transports[1];
			var transports3 = consol1.Transports[2];

			var transports4 = consol2.Transports[0];
			var transports5 = consol2.Transports[1];

			transports1.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transports2.JW_TransportType = Core.Constants.TransportPlanningType.Other;
			transports3.JW_TransportType = "";

			transports4.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transports5.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			Factory.Save();

			var transportFilter = (ModuleTextFilter)FilterStripBizO["Transport Type"];

			var results = new TransportNonDependentCollection(Factory);

			transportFilter.IsActive = true;

			transportFilter.Property = "";
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports1, transports2, transports3, transports4, transports5 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = Core.Constants.TransportPlanningType.MainVessel;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports1, transports4 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = Core.Constants.TransportPlanningType.Other;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports2 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property = Core.Constants.TransportPlanningType.OnForwarding;
			results.Load(FilterStripBizO.Filter);
			AssertEquals(0, results.Count);
		}

		public void TestIsLinkedFilter()
		{
			var consol1 = NewConsol("mwhc1", false, "USNYC", "SGSIN", "AUSYD");
			var consol2 = NewConsol("mwhc2", false, "USNYC", "SGSIN");

			var transports1 = consol1.Transports[0];
			var transports2 = consol1.Transports[1];

			var transports3 = consol2.Transports[0];

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_IsCargoOnly = true;
			voyage.JV_VoyageFlight = "DI56";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "NLAMS";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();
			Factory.Save();

			transports1.JW_JX = voyage.Sailings[0].PK;
			transports1.JW_IsLinked = true;

			var transportFilter = (ModuleFlagsFilter)FilterStripBizO["Is Linked"];

			var results = new TransportNonDependentCollection(Factory);

			transportFilter.IsActive = true;
			transportFilter.Property0 = true;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports1 }.Select(x => x.PK), results.Select(x => x.PK));

			transportFilter.Property0 = false;
			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports2, transports3 }.Select(x => x.PK), results.Select(x => x.PK));
		}

		void TestDateTimeFilter(ZString dateProperty, ZString filterProperty)
		{
			var consol1 = NewConsol("mwhc1", false, "USNYC", "SGSIN", "AUSYD", "NZAKA");

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_IsCargoOnly = true;
			voyage.JV_VoyageFlight = "DI56";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "NLAMS";
			origin.JA_E_DEP = new ZDateTime(2007, 1, 1, 9, 0, 0);
			origin.JA_A_DEP = new ZDateTime(2007, 1, 2, 9, 0, 0);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			destination.JB_E_ARV = new DateTime(2007, 3, 2, 9, 0, 0);
			destination.JB_A_ARV = new DateTime(2007, 4, 2, 9, 0, 0);

			voyage.GenerateSailings();

			var transports1 = consol1.Transports[0];
			transports1.JW_IsLinked = true;
			transports1.JW_JX = voyage.Sailings[0].PK;
			transports1.JW_IsLinked = true;

			var transports2 = consol1.Transports[1];
			transports2[dateProperty] = new ZDateTime(2007, 1, 1, 10, 0, 0);

			var transports3 = consol1.Transports[2];
			transports3[dateProperty] = new ZDateTime(2017, 1, 1, 11, 0, 0);

			Factory.Save();

			var transportFilter = (ModuleDateFilter)FilterStripBizO[filterProperty];
			var results = new TransportNonDependentCollection(Factory);

			transportFilter.IsActive = true;
			transportFilter.PropertySearch = ModuleDateFilter.Past;

			results.Load(FilterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { transports1, transports2 }.Select(x => x.PK), results.Select(x => x.PK));
		}

		#region Test Flight Status Filter

		public void TestFlightStatusFilter()
		{
			var consol1 = NewConsol("mwhc1", false, "USNYC", "SGSIN", "AUSYD");
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			var transport1 = consol1.Transports.AddNew();
			transport1.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.Matched;

			var consol2 = NewConsol("mwhc2", false, "USNYC", "SGSIN", "AUSYD");
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			var transport2 = consol2.Transports.AddNew();
			transport2.JW_OnlineScheduleStatus = Constants.FlightScheduleStatus.PartiallyMatched;

			Factory.Save();

			var consolFilter = (ModuleTextFilter)FilterStripBizO["Flight Status"];
			var results = new TransportNonDependentCollection(Factory);

			consolFilter.IsActive = true;
			consolFilter.Property = Constants.FlightScheduleStatus.Matched;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering transports for 'Flight Status' = MTD", () =>
			{
				Assert("Transport1 should be in collection as it has a Matched ('MTD') Flight Status.", results.Contains(transport1.PK));
				Assert("Transport2 should not be in collection as it does not have a Matched ('MTD') Flight Status.", !results.Contains(transport2.PK));
			});

			consolFilter.IsActive = true;
			consolFilter.Property = Constants.FlightScheduleStatus.PartiallyMatched;
			results.Load(FilterStripBizO.Filter);

			CombineAssertions("Filtering transports for 'Flight Status' = PMD", () =>
			{
				Assert("Transport1 should not be in collection as it has a PartiallyMatched ('PMD') Flight Status.", !results.Contains(transport1.PK));
				Assert("Transport2 should be in collection as it has a PartiallyMatched ('PMD') Flight Status.", results.Contains(transport2.PK));
			});
		}

		#endregion

		ForwardingConsol NewConsol(string name, bool isLinked, string port1, string port2, params string[] otherports)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = name;
			consol.JK_RL_NKLoadPort = port1;
			consol.JK_RL_NKDischargePort = (otherports.Length == 0 ? port2 : otherports[otherports.Length - 1]);

			var lastTransport = consol.Transports[0];
			lastTransport.JW_RL_NKLoadPort = port1;
			lastTransport.JW_RL_NKDiscPort = port2;
			lastTransport.JW_IsLinked = isLinked;

			foreach (var nextPort in otherports)
			{
				var lastPort = lastTransport.JW_RL_NKDiscPort;
				lastTransport = consol.Transports.AddNew();
				lastTransport.JW_IsLinked = isLinked;
				lastTransport.JW_RL_NKLoadPort = lastPort;
				lastTransport.JW_RL_NKDiscPort = nextPort;
			}

			return consol;
		}

		protected FilterStripBusinessObject FilterStripBizO
		{
			get { return filterStripBizO ?? (filterStripBizO = GetNewFilterStripBusinessObject()); }
		}
		FilterStripBusinessObject filterStripBizO;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RelatedTransportLegsFilterBusinessObject();
		}
	}
}
