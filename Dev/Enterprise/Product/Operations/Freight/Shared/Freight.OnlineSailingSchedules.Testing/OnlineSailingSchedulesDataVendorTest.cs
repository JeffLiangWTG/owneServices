using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	public class OnlineSailingSchedulesDataVendorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestObjectFactoryIsConfigured()
		{
			AssertNotNull(ObjectFactory.Get<Integration.SailingDataVendor.IOnlineSailingSchedulesDataVendor>());
		}

		public void TestIsEnabled_IsControlledByRegistry()
		{
			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(expected: false, new OnlineSailingSchedulesDataVendor().IsEnabled);
			}

			using (FreightDataRegistry.Instance.EnableScheduleFeedService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(expected: true, new OnlineSailingSchedulesDataVendor().IsEnabled);
			}
		}

		public void TestIsVendorDataCurrent()
		{
			AssertEquals(expected: true, new OnlineSailingSchedulesDataVendor().IsVendorDataCurrent);
		}

		public void TestStatus()
		{
			AssertEquals(string.Empty, new OnlineSailingSchedulesDataVendor().Status);
		}

		public void TestUpdateVoyageOrigin_CheckedToContainAllRequiredData()
		{
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);

			var vendor = new OnlineSailingSchedulesDataVendorForTesting(routesProvider.Object);

			VoyageOrigin origin = null;

			void AssertOriginIsCheckedForRequiredData(Action makeOriginRequiredDataMissing)
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "OLGA MAERSK";

				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = "SEA";
				voyage.JV_VoyageFlight = "100";
				voyage.JV_RV_NKVessel = vessel.RV_FK;

				origin = voyage.Origins.AddNew();
				origin.JA_RL_NKPortOfLoading = "AUSYD";

				makeOriginRequiredDataMissing();

				vendor.UpdateVoyageOrigin(origin);
				AssertEquals("Origin's departure date should not be affected", ZDateTime.Empty, origin.JA_E_DEP);
			}

			AssertOriginIsCheckedForRequiredData(() => origin.JA_RL_NKPortOfLoading = "");
			AssertOriginIsCheckedForRequiredData(() => origin.JA_JV = ZGuid.Empty);
			AssertOriginIsCheckedForRequiredData(() => origin.Voyage.JV_AirSeaRoad = "AIR");
			AssertOriginIsCheckedForRequiredData(() => origin.Voyage.JV_VoyageFlight = "");
			AssertOriginIsCheckedForRequiredData(() => origin.Voyage.JV_RV_NKVessel = "");
		}

		public void TestUpdateVoyageOrigin()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "MAERSK";

			var cusCode = carrierOrganisation.CustomsCodes.AddNew(
				OrgCusCode.CodeTypes.CarrierCode,
				"MAER",
				Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "1024567";
			vessel.RV_Name = "OLGA MAERSK";

			Factory.Save();

			var today = DateTime.Today;

			var voyageBizo = Factory.New<JobVoyage>();
			voyageBizo.JV_AirSeaRoad = "SEA";
			voyageBizo.JV_VoyageFlight = "100";
			voyageBizo.JV_RV_NKVessel = vessel.RV_FK;
			voyageBizo.JV_OH_Line = carrierOrganisation.PK;

			var origin = voyageBizo.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var route1 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(10),
				"NZAKL", today.AddDays(15));

			var route2 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(1),
				"NZAKL", today.AddDays(5));

			var route3 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(20),
				"NZAKL", today.AddDays(25));

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "100",
				VesselName = "OLGA MAERSK",
				ImoNumber = "1024567",
				CarrierCode = "MAER",
				LegsCount = "1",
				LoadPort = "AUSYD",
				IncludeRelatedPorts = "True"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProvider.Setup(r => r.GetRoutes(urlParams, It.IsAny<AutoInitiatedServiceRequestManager>()))
				.Returns(new[] { route1, route2, route3 });

			var vendor = new OnlineSailingSchedulesDataVendorForTesting(routesProvider.Object);
			vendor.UpdateVoyageOrigin(origin);

			Assert("prerequisite", route1.Departure > route2.Departure);
			Assert("prerequisite", route3.Departure > route2.Departure);

			AssertEquals("ETD should have been updated from the route with earliest departure", route2.Departure, origin.JA_E_DEP);

			// Should be using local cache for further identical requests
			origin.JA_E_DEP = ZDateTime.Empty;
			vendor.UpdateVoyageOrigin(origin);

			AssertEquals("ETD should have been updated", route2.Departure, origin.JA_E_DEP);
			routesProvider.VerifyAll();

			// Should still access service for non-cached requests
			routesProvider.Setup(r => r.GetRoutes(It.IsAny<string>(), It.IsAny<IServiceRequestManager>()))
				.Returns(Array.Empty<Route>());

			voyageBizo.JV_VoyageFlight = "200";

			origin.JA_E_DEP = ZDateTime.Empty;
			vendor.UpdateVoyageOrigin(origin);
			routesProvider.VerifyAll();
		}

		public void TestUpdateVoyageOrigin_DoesNotUpdate_When_SCACIsEmpty()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "MAERSK";

			var cusCode = carrierOrganisation.CustomsCodes.AddNew(
				OrgCusCode.CodeTypes.CarrierCode,
				ZString.Empty,
				Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "1024567";
			vessel.RV_Code = "OLGA MAERSK";

			Factory.Save();

			var today = DateTime.Today;

			var voyageBizo = Factory.New<JobVoyage>();
			voyageBizo.JV_AirSeaRoad = "SEA";
			voyageBizo.JV_VoyageFlight = "100";
			voyageBizo.JV_RV_NKVessel = vessel.RV_FK;
			voyageBizo.JV_OH_Line = carrierOrganisation.PK;

			var origin = voyageBizo.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = today;

			var route = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(1),
				"NZAKL", today.AddDays(5));

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "100",
				VesselName = "OLGA MAERSK",
				ImoNumber = "1024567",
				CarrierCode = "MAER",
				LegsCount = "1",
				LoadPort = "AUSYD",
				IncludeRelatedPorts = "True"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProvider.Setup(r => r.GetRoutes(urlParams, It.IsAny<AutoInitiatedServiceRequestManager>()))
				.Returns(new[] { route });

			var vendor = new OnlineSailingSchedulesDataVendorForTesting(routesProvider.Object);
			vendor.UpdateVoyageOrigin(origin);

			AssertEquals("ETD should not have been updated from the route when SCAC is empty", today, origin.JA_E_DEP);
			AssertNotEquals("ETD should not have been updated from the route when SCAC is empty", route.Departure, origin.JA_E_DEP);
		}

		public void TestUpdateVoyageOrigin_WithRelatedPortRoutes()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "MAERSK";

			var cusCode = carrierOrganisation.CustomsCodes.AddNew(
				OrgCusCode.CodeTypes.CarrierCode,
				"MAER",
				Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "1024567";
			vessel.RV_Name = "OLGA MAERSK";

			Factory.Save();

			var today = DateTime.Today;

			var voyageBizo = Factory.New<JobVoyage>();
			voyageBizo.JV_AirSeaRoad = "SEA";
			voyageBizo.JV_VoyageFlight = "100";
			voyageBizo.JV_RV_NKVessel = vessel.RV_FK;
			voyageBizo.JV_OH_Line = carrierOrganisation.PK;

			var route = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUMEL", today.AddDays(1),
				"NZAKL", today.AddDays(5));

			var origin = voyageBizo.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUNTL";

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "100",
				VesselName = "OLGA MAERSK",
				ImoNumber = "1024567",
				CarrierCode = "MAER",
				LegsCount = "1",
				LoadPort = "AUNTL",
				IncludeRelatedPorts = "True"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProvider.Setup(r => r.GetRoutes(urlParams, It.IsAny<AutoInitiatedServiceRequestManager>()))
				.Returns(new[] { route });

			var vendor = new OnlineSailingSchedulesDataVendorForTesting(routesProvider.Object);
			vendor.UpdateVoyageOrigin(origin);

			AssertEquals("ETD should not be updated from the route with the non-matched origin port", ZDateTime.Empty, origin.JA_E_DEP);
			AssertEquals("Origin port should not be updated from the route with the non-matched origin port", "AUNTL", origin.JA_RL_NKPortOfLoading);
		}

		public void TestUpdateVoyageOrigin_HandleException()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "MAERSK";

			var cusCode = carrierOrganisation.CustomsCodes.AddNew(
				OrgCusCode.CodeTypes.CarrierCode,
				"MAER",
				Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "1024567";
			vessel.RV_Name = "OLGA MAERSK";

			Factory.Save();

			var today = DateTime.Today;

			var voyageBizo = Factory.New<JobVoyage>();
			voyageBizo.JV_AirSeaRoad = "SEA";
			voyageBizo.JV_VoyageFlight = "100";
			voyageBizo.JV_RV_NKVessel = vessel.RV_FK;
			voyageBizo.JV_OH_Line = carrierOrganisation.PK;

			var origin = voyageBizo.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";

			var route1 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(10),
				"NZAKL", today.AddDays(15));

			var route2 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(1),
				"NZAKL", today.AddDays(5));

			var route3 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(20),
				"NZAKL", today.AddDays(25));

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "100",
				VesselName = "OLGA MAERSK",
				ImoNumber = "1024",
				CarrierCode = "MAER",
				LegsCount = "1",
				LoadPort = "AUSYD"
			};

			var routesProviderForTesting = new RoutesProviderForTesting(Factory);
			routesProviderForTesting.MockResponseGetter = (httpClient, requestUri) =>
			{
				throw new Exception("Unknown exception");
			};

			var vendor = new OnlineSailingSchedulesDataVendorForTesting(routesProviderForTesting);
			vendor.UpdateVoyageOrigin(origin);

			var notification = "Error occurred while loading routes from Global Schedules service.";
			AssertContains(notification, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestUpdateVoyageOrigin_Suppression()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "MAERSK";

			carrierOrganisation.CustomsCodes.AddNew(
				OrgCusCode.CodeTypes.CarrierCode,
				"MAER",
				Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "1024567";
			vessel.RV_Name = "OLGA MAERSK";

			Factory.Save();

			var today = DateTime.Today;

			var voyageBizo = Factory.New<JobVoyage>();
			voyageBizo.JV_AirSeaRoad = "SEA";
			voyageBizo.JV_VoyageFlight = "100";
			voyageBizo.JV_RV_NKVessel = vessel.RV_FK;
			voyageBizo.JV_OH_Line = carrierOrganisation.PK;

			var origin = voyageBizo.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = today;

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var vendor = new OnlineSailingSchedulesDataVendorForTesting(routesProvider.Object);

			AssertNoExceptionThrown("GSS should not be called because ETD is already set", () => vendor.UpdateVoyageOrigin(origin));
			Assert("Origin should NOT have been saved to the database", !origin.IsInDatabase);

			origin.JA_E_DEP = ZDateTime.Empty;
			Factory.Save();

			Assert("Precondition: origin should have been saved to the database", origin.IsInDatabase);
			AssertNoExceptionThrown("GSS should not be called because origin is already saved to the db", () => vendor.UpdateVoyageOrigin(origin));
		}

		public void TestUpdateVoyageDestination_CheckedToContainAllRequiredData()
		{
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);

			var vendor = new OnlineSailingSchedulesDataVendorForTesting(routesProvider.Object);

			VoyageDestination destination = null;

			void AssertDestinationIsCheckedForRequiredData(Action makeDestinationRequiredDataMissing)
			{
				var vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "OLGA MAERSK";

				var voyage = Factory.New<JobVoyage>();
				voyage.JV_AirSeaRoad = "SEA";
				voyage.JV_VoyageFlight = "100";
				voyage.JV_RV_NKVessel = vessel.RV_FK;

				destination = voyage.Destinations.AddNew();
				destination.JB_RL_NKPortOfDischarge = "NZAKL";

				makeDestinationRequiredDataMissing();

				vendor.UpdateVoyageDestination(destination);
				AssertEquals("Destination's arrival date should not be affected", ZDateTime.Empty, destination.JB_E_ARV);
			}

			AssertDestinationIsCheckedForRequiredData(() => destination.JB_RL_NKPortOfDischarge = "");
			AssertDestinationIsCheckedForRequiredData(() => destination.JB_JV = ZGuid.Empty);
			AssertDestinationIsCheckedForRequiredData(() => destination.Voyage.JV_AirSeaRoad = "AIR");
			AssertDestinationIsCheckedForRequiredData(() => destination.Voyage.JV_VoyageFlight = "");
			AssertDestinationIsCheckedForRequiredData(() => destination.Voyage.JV_RV_NKVessel = "");
		}

		public void TestUpdateVoyageDestination()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "MAERSK";

			var cusCode = carrierOrganisation.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "MAER";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "1024567";
			vessel.RV_Name = "OLGA MAERSK";

			Factory.Save();

			var today = DateTime.Today;

			var voyageBizo = Factory.New<JobVoyage>();
			voyageBizo.JV_AirSeaRoad = "SEA";
			voyageBizo.JV_VoyageFlight = "100";
			voyageBizo.JV_RV_NKVessel = vessel.RV_FK;
			voyageBizo.JV_OH_Line = carrierOrganisation.PK;

			var destination = voyageBizo.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";

			var route1 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(10),
				"NZAKL", today.AddDays(15));

			var route2 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(1),
				"NZAKL", today.AddDays(5));

			var route3 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(20),
				"NZAKL", today.AddDays(25));

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "100",
				VesselName = "OLGA MAERSK",
				ImoNumber = "1024567",
				CarrierCode = "MAER",
				LegsCount = "1",
				DischargePort = "NZAKL",
				IncludeRelatedPorts = "True"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProvider.Setup(r => r.GetRoutes(urlParams, It.IsAny<AutoInitiatedServiceRequestManager>()))
				.Returns(new[] { route1, route2, route3 });

			var vendor = new OnlineSailingSchedulesDataVendorForTesting(routesProvider.Object);
			vendor.UpdateVoyageDestination(destination);

			Assert("prerequisite", route1.Arrival > route2.Arrival);
			Assert("prerequisite", route3.Arrival > route2.Arrival);

			AssertEquals("ETA should have been updated from the route with earliest arrival", route2.Arrival, destination.JB_E_ARV);

			// Should be using local cache for further identical requests

			destination.JB_E_ARV = ZDateTime.Empty;
			vendor.UpdateVoyageDestination(destination);

			AssertEquals("ETA should have been updated", route2.Arrival, destination.JB_E_ARV);
			routesProvider.VerifyAll();

			// Should still access service for non-cached requests

			routesProvider.Setup(r => r.GetRoutes(It.IsAny<string>(), It.IsAny<IServiceRequestManager>()))
				.Returns(Array.Empty<Route>());

			voyageBizo.JV_VoyageFlight = "200";

			destination.JB_E_ARV = ZDateTime.Empty;
			vendor.UpdateVoyageDestination(destination);
			routesProvider.VerifyAll();
		}

		public void TestUpdateVoyageDestination_DoesNotUpdate_When_SCACIsEmpty()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "MAERSK";

			var cusCode = carrierOrganisation.CustomsCodes.AddNew(
				OrgCusCode.CodeTypes.CarrierCode,
				ZString.Empty,
				Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "1024567";
			vessel.RV_Code = "OLGA MAERSK";

			Factory.Save();

			var today = DateTime.Today;

			var voyageBizo = Factory.New<JobVoyage>();
			voyageBizo.JV_AirSeaRoad = "SEA";
			voyageBizo.JV_VoyageFlight = "100";
			voyageBizo.JV_RV_NKVessel = vessel.RV_FK;
			voyageBizo.JV_OH_Line = carrierOrganisation.PK;

			var destination = voyageBizo.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = today;

			var route = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(1),
				"NZAKL", today.AddDays(5));

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "100",
				VesselName = "OLGA MAERSK",
				ImoNumber = "1024567",
				CarrierCode = "MAER",
				LegsCount = "1",
				DischargePort = "NZAKL",
				IncludeRelatedPorts = "True"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);
			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProvider.Setup(r => r.GetRoutes(urlParams, It.IsAny<AutoInitiatedServiceRequestManager>()))
				.Returns(new[] { route });

			var vendor = new OnlineSailingSchedulesDataVendorForTesting(routesProvider.Object);
			vendor.UpdateVoyageDestination(destination);

			AssertEquals("ETD should not have been updated from the route when SCAC is empty", today, destination.JB_E_ARV);
			AssertNotEquals("ETD should not have been updated from the route when no SCAC is empty", route.Arrival, destination.JB_E_ARV);
		}

		public void TestUpdateVoyageDestination_WithRelatedPortRoutes()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "MAERSK";

			var cusCode = carrierOrganisation.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "MAER";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "1024567";
			vessel.RV_Name = "OLGA MAERSK";

			Factory.Save();

			var today = DateTime.Today;

			var voyageBizo = Factory.New<JobVoyage>();
			voyageBizo.JV_AirSeaRoad = "SEA";
			voyageBizo.JV_VoyageFlight = "100";
			voyageBizo.JV_RV_NKVessel = vessel.RV_FK;
			voyageBizo.JV_OH_Line = carrierOrganisation.PK;

			var route = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(1),
				"AUMEL", today.AddDays(5));

			var destination = voyageBizo.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAMZ";

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "100",
				VesselName = "OLGA MAERSK",
				ImoNumber = "1024567",
				CarrierCode = "MAER",
				LegsCount = "1",
				DischargePort = "NZAMZ",
				IncludeRelatedPorts = "True"
			};

			var urlParams = UrlHelper.ConvertToParams(filterRequest);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			routesProvider.Setup(r => r.GetRoutes(urlParams, It.IsAny<AutoInitiatedServiceRequestManager>()))
				.Returns(new[] { route });

			var vendor = new OnlineSailingSchedulesDataVendorForTesting(routesProvider.Object);
			vendor.UpdateVoyageDestination(destination);

			AssertEquals("ETA should not be updated from the route with the non-matched destination port", ZDateTime.Empty, destination.JB_E_ARV);
			AssertEquals("Destination port should not be updated from the route with the non-matched destination port", "NZAMZ", destination.JB_RL_NKPortOfDischarge);
		}

		public void TestUpdateVoyageDestination_HandleException()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "MAERSK";

			var cusCode = carrierOrganisation.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "MAER";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "1024567";
			vessel.RV_Name = "OLGA MAERSK";

			Factory.Save();

			var today = DateTime.Today;

			var voyageBizo = Factory.New<JobVoyage>();
			voyageBizo.JV_AirSeaRoad = "SEA";
			voyageBizo.JV_VoyageFlight = "100";
			voyageBizo.JV_RV_NKVessel = "OLGA MAERSK";
			voyageBizo.JV_OH_Line = carrierOrganisation.PK;

			var destination = voyageBizo.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";

			var route1 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(10),
				"NZAKL", today.AddDays(15));

			var route2 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(1),
				"NZAKL", today.AddDays(5));

			var route3 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(20),
				"NZAKL", today.AddDays(25));

			var filterRequest = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "100",
				VesselName = "OLGA MAERSK",
				ImoNumber = "1024567",
				CarrierCode = "MAER",
				LegsCount = "1",
				DischargePort = "NZAKL"
			};

			var routesProviderForTesting = new RoutesProviderForTesting(Factory);
			routesProviderForTesting.MockResponseGetter = (httpClient, requestUri) =>
			{
				throw new Exception("Unknown exception");
			};

			var vendor = new OnlineSailingSchedulesDataVendorForTesting(routesProviderForTesting);
			vendor.UpdateVoyageDestination(destination);

			var notification = "Error occurred while loading routes from Global Schedules service.";
			AssertContains(notification, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestUpdateVoyageDestination_Suppression()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "MAERSK";

			carrierOrganisation.CustomsCodes.AddNew(
				OrgCusCode.CodeTypes.CarrierCode,
				"MAER",
				Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "1024567";
			vessel.RV_Name = "OLGA MAERSK";

			Factory.Save();

			var today = DateTime.Today;

			var voyageBizo = Factory.New<JobVoyage>();
			voyageBizo.JV_AirSeaRoad = "SEA";
			voyageBizo.JV_VoyageFlight = "100";
			voyageBizo.JV_RV_NKVessel = vessel.RV_FK;
			voyageBizo.JV_OH_Line = carrierOrganisation.PK;

			var destination = voyageBizo.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = today;

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var vendor = new OnlineSailingSchedulesDataVendorForTesting(routesProvider.Object);

			AssertNoExceptionThrown("GSS should not be called because ETA is already set", () => vendor.UpdateVoyageDestination(destination));
			Assert("Destination should NOT have been saved to the database", !destination.IsInDatabase);

			destination.JB_E_ARV = ZDateTime.Empty;
			Factory.Save();

			Assert("Precondition: destination should have been saved to the database", destination.IsInDatabase);
			AssertNoExceptionThrown("GSS should not be called because destination is already saved to the db", () => vendor.UpdateVoyageDestination(destination));
		}

		SailingInformation TryFindSailingIncludingRelatedPortsForTest(OrgHeader carrierOrganisation, Route[] routes1, Route[] routes2)
		{
			var today = DateTime.Today;

			var vessel = RefVessel.LookupVesselByName("OLGA MAERSK", Factory).FirstOrDefault();
			if (vessel == null)
			{
				vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "OLGA MAERSK";
			}

			var voyageBizo = Factory.New<JobVoyage>();
			voyageBizo.JV_AirSeaRoad = "SEA";
			voyageBizo.JV_VoyageFlight = "100";
			voyageBizo.JV_RV_NKVessel = vessel.RV_FK;
			voyageBizo.JV_OH_Line = carrierOrganisation.PK;

			var origin = voyageBizo.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUNTL";
			origin.JA_E_DEP = today;

			var destination = voyageBizo.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAMZ";
			destination.JB_E_ARV = today;

			var filterRequest1 = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "100",
				VesselName = "OLGA MAERSK",
				ImoNumber = "1024567",
				CarrierCode = "MAER",
				LegsCount = "1",
				LoadPort = "AUNTL",
				IncludeRelatedPorts = "True"
			};

			var urlParams1 = UrlHelper.ConvertToParams(filterRequest1);

			var filterRequest2 = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "100",
				VesselName = "OLGA MAERSK",
				ImoNumber = "1024567",
				CarrierCode = "MAER",
				LegsCount = "1",
				DischargePort = "NZAMZ",
				IncludeRelatedPorts = "True"
			};

			var urlParams2 = UrlHelper.ConvertToParams(filterRequest2);

			var routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var vendor = new OnlineSailingSchedulesDataVendorForTesting(routesProvider.Object);

			ClearCachedValue(Factory, urlParams1);
			ClearCachedValue(Factory, urlParams2);

			routesProvider.Setup(r => r.GetRoutes(urlParams1, It.IsAny<AutoInitiatedServiceRequestManager>()))
				.Returns(routes1);

			routesProvider.Setup(r => r.GetRoutes(urlParams2, It.IsAny<AutoInitiatedServiceRequestManager>()))
				.Returns(routes2);

			var result = vendor.TryFindSailingIncludingRelatedPorts(origin, destination);
			var resultRepeat = vendor.TryFindSailingIncludingRelatedPorts(origin, destination);

			if (result != null)
			{
				AssertEquals(resultRepeat.Load, result.Load);
				AssertEquals(resultRepeat.ETD, result.ETD);
				AssertEquals(resultRepeat.Discharge, result.Discharge);
				AssertEquals(resultRepeat.ETA, result.ETA);
			}
			else
			{
				AssertEquals(resultRepeat, result);
			}

			return result;
		}

		public void TestTryFindSailingIncludingRelatedPorts()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "MAERSK";

			var cusCode = carrierOrganisation.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "MAER";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "1024567";
			vessel.RV_Name = "OLGA MAERSK";

			Factory.Save();

			var today = DateTime.Today;

			var route1 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUSYD", today.AddDays(10),
				"NZAKL", today.AddDays(15));

			var route2 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUMEL", today.AddDays(1),
				"NZAKL", today.AddDays(5));

			var route3 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUNTL", today,
				"NZAMZ", today);

			var route4 = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				"AUNTL", today.AddDays(1),
				"NZAMZ", today.AddDays(2));

			// cannot find loading port
			{
				var updatedSailingInfo = TryFindSailingIncludingRelatedPortsForTest(carrierOrganisation, Array.Empty<Route>(), new[] { route2 });
				AssertEquals(null, updatedSailingInfo);
			}

			// cannot find discharging port
			{
				var updatedSailingInfo = TryFindSailingIncludingRelatedPortsForTest(carrierOrganisation, new[] { route1 }, Array.Empty<Route>());
				AssertEquals(null, updatedSailingInfo);
			}

			// find a new schedules with the same loading port and discharging port
			{
				var updatedSailingInfo = TryFindSailingIncludingRelatedPortsForTest(carrierOrganisation, new[] { route4 }, new[] { route4 });
				AssertEquals("AUNTL", updatedSailingInfo.Load);
				AssertEquals(today.AddDays(1), updatedSailingInfo.ETD);
				AssertEquals("NZAMZ", updatedSailingInfo.Discharge);
				AssertEquals(today.AddDays(2), updatedSailingInfo.ETA);
			}
		}

		public void TestTryFindSailingIncludingRelatedPortsWithIllegalPortNames()
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "MAERSK";

			var cusCode = carrierOrganisation.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "MAER";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var vessel = Factory.New<RefVessel>();
			vessel.RV_LloydsNumber = "1024567";
			vessel.RV_Code = "OLGA MAERSK";

			Factory.Save();
			// legal port name
			var loadPortName = "AUNTL";
			var dischargePortName = "NZAMZ";
			var result = TryFindSailingIncludingRelatedPortsWithPortName(carrierOrganisation, loadPortName, dischargePortName, out var mockedProvider1, isExpectedToGetRoutes: true);
			Assert(result != null);
			AssertEquals(loadPortName, result.Load);
			AssertEquals(dischargePortName, result.Discharge);
			mockedProvider1.Verify(m => m.GetRoutes(null, null), Times.Never);

			// illegal port name
			loadPortName = "AU";
			dischargePortName = "CN";
			result = TryFindSailingIncludingRelatedPortsWithPortName(carrierOrganisation, loadPortName, dischargePortName, out var mockedProvider2, isExpectedToGetRoutes: false);
			Assert(result == null);
			mockedProvider2.Verify(m => m.GetRoutes(null, null), Times.Never);
		}

		SailingInformation TryFindSailingIncludingRelatedPortsWithPortName(
			OrgHeader carrierOrganisation,
			string loadPortName,
			string dischargePortName,
			out Mock<IRoutesProvider> routesProvider,
			bool isExpectedToGetRoutes = true)
		{
			var today = DateTime.Today;

			var vessel = RefVessel.LookupVesselByName("OLGA MAERSK", Factory).FirstOrDefault();
			if (vessel == null)
			{
				vessel = Factory.NewWithValidTestData<RefVessel>();
				vessel.RV_Name = "OLGA MAERSK";
			}

			var voyageBizo = Factory.New<JobVoyage>();
			voyageBizo.JV_AirSeaRoad = "SEA";
			voyageBizo.JV_VoyageFlight = "100";
			voyageBizo.JV_RV_NKVessel = vessel.RV_FK;
			voyageBizo.JV_OH_Line = carrierOrganisation.PK;

			var origin = voyageBizo.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = loadPortName;
			origin.JA_E_DEP = today;

			var destination = voyageBizo.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = dischargePortName;
			destination.JB_E_ARV = today;

			var route = new RouteTestHelper(Factory).CreateRoute(
				"100",
				"OLGA MAERSK",
				"MAERSK",
				loadPortName, today.AddDays(1),
				dischargePortName, today.AddDays(2));

			var routesForLoadPort = new Route[] { route };
			var routesForDischargePort = new Route[] { route };

			var filterRequestForLoadPort = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "100",
				VesselName = "OLGA MAERSK",
				ImoNumber = "1024567",
				CarrierCode = "MAER",
				LegsCount = "1",
				LoadPort = loadPortName,
				IncludeRelatedPorts = "True"
			};
			var urlParamsForLoadPort = UrlHelper.ConvertToParams(filterRequestForLoadPort);

			var filterRequestForDischargePort = new OnlineSchedulesFilterRequest
			{
				VoyageNumber = "100",
				VesselName = "OLGA MAERSK",
				ImoNumber = "1024567",
				CarrierCode = "MAER",
				LegsCount = "1",
				DischargePort = dischargePortName,
				IncludeRelatedPorts = "True"
			};

			var urlParamsForDischargePort = UrlHelper.ConvertToParams(filterRequestForDischargePort);
			routesProvider = new Mock<IRoutesProvider>(MockBehavior.Strict);
			var vendor = new OnlineSailingSchedulesDataVendorForTesting(routesProvider.Object);

			ClearCachedValue(Factory, urlParamsForLoadPort);
			ClearCachedValue(Factory, urlParamsForDischargePort);

			if (isExpectedToGetRoutes)
			{
				routesProvider.Setup(r => r.GetRoutes(urlParamsForLoadPort, It.IsAny<AutoInitiatedServiceRequestManager>()))
					.Returns(routesForLoadPort);

				routesProvider.Setup(r => r.GetRoutes(urlParamsForDischargePort, It.IsAny<AutoInitiatedServiceRequestManager>()))
					.Returns(routesForDischargePort);
			}

			var result = vendor.TryFindSailingIncludingRelatedPorts(origin, destination);
			routesProvider.VerifyAll();
			return result;
		}

		void ClearCachedValue(BusinessObjectFactory factory, string searchParams)
		{
			var dict = factory.GetCachedValue("OnlineSailingSchedulesDataVendorRouteCache", () => new Dictionary<string, Route[]>());
			dict[searchParams] = null;
		}

		#region Implementation

		class OnlineSailingSchedulesDataVendorForTesting : OnlineSailingSchedulesDataVendor
		{
			public OnlineSailingSchedulesDataVendorForTesting(IRoutesProvider routesProvider) : base()
			{
				this.routesProvider = routesProvider;
			}

			readonly IRoutesProvider routesProvider;

			protected override IRoutesProvider GetRoutesProvider(BusinessObjectFactory factory)
			{
				return routesProvider;
			}
		}

		#endregion
	}
}
