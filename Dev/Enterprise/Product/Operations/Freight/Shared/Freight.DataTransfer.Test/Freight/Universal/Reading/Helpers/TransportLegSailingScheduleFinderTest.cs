using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Testing.Core;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	sealed class TransportLegSailingScheduleFinderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestGetTransportSailing()
		{
			var today = ZDateTime.Today;

			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAA", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var sailing = Factory.New<JobSailing>();

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			origin.JA_E_DEP = today.AddDays(1);
			origin.JA_S_DEP = today.AddDays(-1);

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "JPOSA";
			destination.JB_E_ARV = today.AddDays(2);
			destination.JB_S_ARV = today.AddDays(3);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "VESSEL1";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "A1234A";
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_OH_Line = carrier.PK;

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;

			var shipment = Factory.New<CommonShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "JPOSA";
			transport.JW_VoyageFlight = "A1234A";
			transport.JW_Vessel = "VESSEL1";

			Factory.SaveForTesting();

			var sailingLeg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			sailingLeg.TransportMode = TransportMode.Sea;
			sailingLeg.PortOfLoading = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" };
			sailingLeg.PortOfDischarge = new UNLOCO() { Code = "JPOSA", Name = "Osaka" };
			sailingLeg.VesselName = "VESSEL1";
			sailingLeg.VoyageFlightNo = "A1234A";
			sailingLeg.EstimatedArrival = today.AddDays(2);
			sailingLeg.EstimatedDeparture = today.AddDays(1);
			sailingLeg.ScheduledDeparture = today.AddDays(-1);
			sailingLeg.ScheduledArrival = today.AddDays(3);
			sailingLeg.LegType = LegType.Main;
			sailingLeg.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "Carrier"
			};
			sailingLeg.Carrier.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
				{
					new RegistrationNumber()
					{
						Type = new RegistrationNumberType { Code = OrgCusCode.CodeTypes.CarrierCode },
						Value = "AAA",
						CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.UnitedStates } }
			});

			var finder = new TransportLegSailingScheduleFinder(Factory, sailingLeg, logger, transport, shipment);
			var foundSailing = finder.GetTransportSailingFromSailingManager().Sailing;

			AssertEquals("Sailing should be found", foundSailing.PK, sailing.PK);
		}

		public void TestGetTransportSailing_NoValidSailing()
		{
			var today = ZDateTime.Today;

			var carrier = Factory.NewWithValidTestData<OrgHeader>().WithCustomsCode(OrgCusCode.CodeTypes.CarrierCode, "AAA", Core.Constants.CountryCodes.UnitedStates);
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			var sailing = Factory.New<JobSailing>();

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "NZAKL";
			origin.JA_E_DEP = today.AddDays(1);
			origin.JA_S_DEP = today.AddDays(-1);

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";
			destination.JB_E_ARV = today.AddDays(2);
			destination.JB_S_ARV = today.AddDays(3);

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = "VESSEL1";
			voyage.JV_VoyageFlight = "A1234A";
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_OH_Line = carrier.PK;

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;

			origin.JA_JV = voyage.PK;
			destination.JB_JV = voyage.PK;

			var shipment = Factory.New<CommonShipment>();
			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "JPOSA";
			transport.JW_VoyageFlight = "A1234A";
			transport.JW_Vessel = "VESSEL1";

			Factory.SaveForTesting();

			var sailingLeg = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance);
			sailingLeg.TransportMode = TransportMode.Sea;
			sailingLeg.PortOfLoading = new UNLOCO() { Code = "AUBNE", Name = "Brisbane" };
			sailingLeg.PortOfDischarge = new UNLOCO() { Code = "JPOSA", Name = "Osaka" };
			sailingLeg.VesselName = "VESSEL1";
			sailingLeg.VoyageFlightNo = "A1234A";
			sailingLeg.EstimatedArrival = today.AddDays(2);
			sailingLeg.EstimatedDeparture = today.AddDays(1);
			sailingLeg.ScheduledDeparture = today.AddDays(-1);
			sailingLeg.ScheduledArrival = today.AddDays(3);
			sailingLeg.LegType = LegType.Main;
			sailingLeg.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = "Carrier"
			};
			sailingLeg.Carrier.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
			{
				new RegistrationNumber()
				{
					Type = new RegistrationNumberType { Code = OrgCusCode.CodeTypes.CarrierCode },
					Value = "AAA",
					CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.UnitedStates } }
			});

			var finder = new TransportLegSailingScheduleFinder(Factory, sailingLeg, logger, transport, shipment);
			var foundSailing = finder.GetTransportSailingFromSailingManager().Sailing;

			AssertNull("Sailing should not be found or created", foundSailing);
		}

		protected override void SetUp()
		{
			base.SetUp();

			logger = new TestErrorLogger();
		}

		TestErrorLogger logger;
	}
}
