using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.AIS;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.OnlineSailingSchedules.ServiceModel;
using Enterprise.Freight.SailingDataVendor.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	[TestedType(typeof(Leg))]
	public class LegTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetValues()
		{
			var serviceLeg = CreateLeg(loadPort: "AUSYD", dischargePort: "AUBNE", etd: new DateTime(2016, 8, 10), eta: new DateTime(2016, 8, 15), tradeLane: new TradeLane { Name = "AAA" });

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			AssertEquals("AUSYD", leg.OriginPortUnloco);
			AssertEquals("AUBNE", leg.DestinationPortUnloco);
			AssertEquals(new DateTime(2016, 8, 10), leg.Departure);
			AssertEquals(new DateTime(2016, 8, 15), leg.Arrival);
			AssertEquals("12345", leg.LloydsNumber);
			AssertEquals("Santa Maria", leg.VesselName);
			AssertEquals("754B", leg.VoyageCode);
			AssertEquals("AAA", leg.TradeLaneName);
			AssertEquals("CMAG", leg.CarrierSCAC);
			AssertEquals("CMA CGM", leg.OperatorName);
			AssertEquals("", leg.CarrierCode);
			AssertEquals(DepartureReference, leg.DepartureReference);
			AssertEquals("DEF", leg.DepartureReferenceProvider);
			AssertEquals(1.0m, leg.Co2eKgPerTeu);
			AssertEquals(2.0m, leg.Co2eKgPerTonne);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Org";
			var cusCodes = carrier.CustomsCodes.AddNew();
			cusCodes.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCodes.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCodes.OK_CustomsRegNo = "CMAG";

			Factory.Save();

			AssertEquals("Org", leg.CarrierCode);
		}

		public void TestSetValues_ValildateVessel_VesselRenamed()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(imoNumber: "9308390");

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "OtherName";
			vessel.RV_LloydsNumber = serviceLeg.Voyage.Vessel.ImoNumber;
			vessel.RV_IsActive = true;

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);

			AssertHasWarning(leg.VesselNameInfo, "Vessel name of the vessel found by IMO number differs from the one provided by schedule service. Use right click popup menu to create vessel with Vessel Name from schedule service.");
		}

		public void TestSetValues_ValildateVessel_VesselDefined()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(imoNumber: "9308390");

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = serviceLeg.Voyage.Vessel.VesselName;
			vessel.RV_LloydsNumber = serviceLeg.Voyage.Vessel.ImoNumber;
			vessel.RV_IsActive = true;

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);

			AssertNoErrors(leg.VesselNameInfo);
			AssertNoWarnings(leg.VesselNameInfo);
		}

		public void TestSetValues_Co2ePropertiesAreZeroIfGssApiIsNullData()
		{
			var serviceLeg = CreateLeg("AUSYD", "AUBNE", co2eKgPerTeu: null, co2eKgPerTonne: null);

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			AssertEquals(0m, leg.Co2eKgPerTeu);
			AssertEquals(0m, leg.Co2eKgPerTonne);
		}

		ServiceModel.Leg CreateLeg(string loadPort, string dischargePort,
			TradeLane tradeLane = null, string carrierSCAC = "CMAG", string voyageCode = "754B",
			string vesselName = "Santa Maria", DateTime? etd = null, DateTime? eta = null, string legType = null,
			string imoNumber = "12345", decimal? co2eKgPerTeu = 1.0m, decimal? co2eKgPerTonne = 2.0m)
		{
			return new ServiceModel.Leg
			{
				LoadPort = new ServiceModel.Port { Unloco = loadPort },
				DischargePort = new ServiceModel.Port { Unloco = dischargePort },
				Etd = etd,
				Eta = eta,
				Voyage = new Voyage
				{
					Code = voyageCode,
					Vessel = new Vessel { VesselName = vesselName, ImoNumber = imoNumber },
					TradeLane = tradeLane ?? new TradeLane { Name = "AAA" },
					Operator = new Carrier
					{
						Name = "CMA CGM",
						Code = carrierSCAC
					}
				},
				DepartureReference = DepartureReference,
				DepartureReferenceProvider = "DEF",
				Co2eKgPerTeu = co2eKgPerTeu,
				Co2eKgPerTonne = co2eKgPerTonne
			};
		}

		public void TestCreateEnterpriseVoyage_EmptyLloydsNumber()
		{
			var (voyage, serviceLeg) = CreateVoyageAndServiceLeg();

			var factory1 = new BusinessObjectFactory();
			var entCarrier = factory1.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = Scac;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			factory1.Save();

			var leg = new Leg(factory1);
			leg.SetValues(serviceLeg);
			leg.CreateEnterpriseVoyage();

			factory1.Save();

			voyage.Vessel.ImoNumber = ZString.Empty;

			var factory2 = new BusinessObjectFactory();
			var leg2 = new Leg(factory2);
			leg2.SetValues(serviceLeg);
			leg2.CreateEnterpriseVoyage();

			factory2.Save();

			Assert("Empty Vessel must result in less table hits.", factory1.GetTableHitCount(RefVesselSchema.Constants.TableName) > factory2.GetTableHitCount(RefVesselSchema.Constants.TableName));
		}

		public void TestCreateEnterpriseVoyage_VesselDoesNotExist()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg();

			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = Scac;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			leg.CreateEnterpriseVoyage();

			Factory.Save();
			AssertNotNull(RefVessel.LookupVesselByName(leg.VesselName, Factory).FirstOrDefault());
			AssertEquals(1, GetVoyages(entCarrier, leg).Length);
		}

		public void TestCreateEnterpriseVoyage_VesselExists_VoyageDoesNotExist()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(vesselName: "Santa MariaZZ");

			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = Scac;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = serviceLeg.Voyage.Vessel.VesselName;
			vessel.RV_LloydsNumber = serviceLeg.Voyage.Vessel.ImoNumber;
			vessel.RV_IsActive = true;

			Factory.Save();

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			leg.CreateEnterpriseVoyage();

			Factory.Save();

			var voyages = GetVoyages(entCarrier, leg);
			AssertEquals(1, voyages.Length);
			AssertEquals(1, voyages[0].Origins.Count);
			AssertEquals("AUSYD", voyages[0].Origins[0].JA_RL_NKPortOfLoading);
			AssertEquals(DepartureReference, voyages[0].Origins[0].JA_DepartReference);
			AssertEquals(1, voyages[0].Destinations.Count);
			AssertEquals("AUBNE", voyages[0].Destinations[0].JB_RL_NKPortOfDischarge);

			var sailingsQuery = new ZQuery(JobSailingSchema.JX_JA, voyages[0].Origins[0].PK);
			sailingsQuery.AddToFilter(JobSailingSchema.JX_JB, voyages[0].Destinations[0].PK);
			var sailings = Factory.Load<JobSailing>(sailingsQuery);
			AssertEquals(1, sailings.Length);
		}

		public void TestCreateEnterpriseVoyage_VesselExists_VoyageExists_VoyageOriginAndDestinationDontExist()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(vesselName: "Santa MariaZZ");

			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = Scac;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = serviceLeg.Voyage.Vessel.VesselName;
			vessel.RV_LloydsNumber = serviceLeg.Voyage.Vessel.ImoNumber;
			vessel.RV_IsActive = true;

			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_RV_NKVessel = vessel.RV_FK;
			jobVoyage.JV_OH_Line = entCarrier.PK;
			jobVoyage.JV_VoyageFlight = "754N";

			Factory.Save();

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			leg.CreateEnterpriseVoyage();

			var voyages = GetVoyages(entCarrier, leg);
			AssertEquals(1, voyages.Length);
			AssertEquals(voyages[0].PK, jobVoyage.PK);
			AssertEquals(1, voyages[0].Origins.Count);
			AssertEquals("AUSYD", voyages[0].Origins[0].JA_RL_NKPortOfLoading);
			AssertEquals(DepartureReference, voyages[0].Origins[0].JA_DepartReference);
			AssertEquals(1, voyages[0].Destinations.Count);
			AssertEquals("AUBNE", voyages[0].Destinations[0].JB_RL_NKPortOfDischarge);

			var sailingsQuery = new ZQuery(JobSailingSchema.JX_JA, voyages[0].Origins[0].PK);
			sailingsQuery.AddToFilter(JobSailingSchema.JX_JB, voyages[0].Destinations[0].PK);
			var sailings = Factory.Load<JobSailing>(sailingsQuery);
			AssertEquals(1, sailings.Length);
		}

		public void TestCreateEnterpriseVoyage_VesselExistsWithEmptyIMO_ValidGssImo_ShouldUpdateIMO()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(vesselName: "Santa MariaZZ");

			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = Scac;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = serviceLeg.Voyage.Vessel.VesselName;
			vessel.RV_LloydsNumber = string.Empty;
			vessel.RV_IsActive = true;

			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_RV_NKVessel = vessel.RV_FK;
			jobVoyage.JV_OH_Line = entCarrier.PK;
			jobVoyage.JV_VoyageFlight = "754N";

			Factory.Save();

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			leg.CreateEnterpriseVoyage();

			var loadedVessel = RefVessel.LookupVesselByName(leg.VesselName, Factory).FirstOrDefault();

			AssertNotNull(loadedVessel);
			AssertEquals(ImoNumber, loadedVessel.RV_LloydsNumber);
		}

		public void TestCreateEnterpriseVoyage_VesselExistsWithEmptyIMO_InvalidGssImo_Should_NOT_UpdateIMO()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(vesselName: "Santa MariaZZ", imoNumber: "123");

			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = Scac;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = serviceLeg.Voyage.Vessel.VesselName;
			vessel.RV_LloydsNumber = string.Empty;
			vessel.RV_IsActive = true;

			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_RV_NKVessel = vessel.RV_FK;
			jobVoyage.JV_OH_Line = entCarrier.PK;
			jobVoyage.JV_VoyageFlight = "754N";

			Factory.Save();

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			leg.CreateEnterpriseVoyage();

			var loadedVessel = RefVessel.LookupVesselByName(leg.VesselName, Factory).FirstOrDefault();
			AssertNotNull(loadedVessel);
			AssertEquals(string.Empty, loadedVessel.RV_LloydsNumber);
		}

		public void TestCreateEnterpriseVoyage_DakosyDataIsLoaded_OSSEstimatedDatesOverrideDakosyEstimatedDates()
		{
			GlbCompany.CurrentCompany.SetCountry("DE");

			#region Create test data

			var (_, serviceLeg) = CreateVoyageAndServiceLeg(
				vesselName: "Santa MariaZZ",
				etd: new DateTime(2017, 1, 9),
				eta: new DateTime(2017, 1, 12));

			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = Scac;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var cto = Factory.NewWithValidTestData<OrgHeader>();
			cto.OH_Code = "HLCUCTO";
			cusCode = cto.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "HLCU";
			cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.DakosyParticipantCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = serviceLeg.Voyage.Vessel.VesselName;
			vessel.RV_LloydsNumber = serviceLeg.Voyage.Vessel.ImoNumber;
			vessel.RV_IsActive = true;

			var jobVoyage = Factory.New<JobVoyage>();
			jobVoyage.JV_RV_NKVessel = vessel.RV_FK;
			jobVoyage.JV_OH_Line = entCarrier.PK;
			jobVoyage.JV_VoyageFlight = "754N";

			var jobVesselSchedule1 = Factory.New<JobVesselSchedule>();
			jobVesselSchedule1.EV_RL_NKPortCode = "AUSYD";
			jobVesselSchedule1.EV_TerminalID = "HLCU";
			jobVesselSchedule1.EV_ETD = new DateTime(2017, 1, 10);
			jobVesselSchedule1.EV_IMOLloydsNumber = "1234567";
			jobVesselSchedule1.EV_ShipName = "Santa MariaZZ";
			jobVesselSchedule1.EV_ShipOperatorVoyageIn = "754N";
			jobVesselSchedule1.EV_ActualDeparture = new DateTime(2017, 1, 10);
			jobVesselSchedule1.EV_DataProvider = "ZZZ";
			jobVesselSchedule1.EV_DataProviderReference = "ZZZ_01";
			jobVesselSchedule1.EV_LineOperator = Scac;
			jobVesselSchedule1.EV_ImportStorageCommences = new DateTime();

			var jobVesselSchedule2 = Factory.New<JobVesselSchedule>();
			jobVesselSchedule2.EV_RL_NKPortCode = "AUBNE";
			jobVesselSchedule2.EV_TerminalID = "HLCU";
			jobVesselSchedule2.EV_ETA = new DateTime(2017, 1, 13);
			jobVesselSchedule2.EV_IMOLloydsNumber = "1234567";
			jobVesselSchedule2.EV_ShipName = "Santa MariaZZ";
			jobVesselSchedule2.EV_ShipOperatorVoyageIn = "754N";
			jobVesselSchedule2.EV_ActualArrival = new DateTime(2017, 1, 13);
			jobVesselSchedule2.EV_DataProvider = "ZZZ";
			jobVesselSchedule2.EV_DataProviderReference = "ZZZ_01";
			jobVesselSchedule2.EV_LineOperator = Scac;
			jobVesselSchedule2.EV_ImportStorageCommences = new DateTime(2017, 1, 15);
			jobVesselSchedule2.EV_ImportAvailability = new DateTime(2017, 1, 14);

			Factory.Save();

			FreightDataRegistry.Instance.SailingSchedulesFeedLastReceived = ZDateTime.Today;

			#endregion

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			leg.CreateEnterpriseVoyage();

			var voyages = GetVoyages(entCarrier, leg);
			AssertEquals(1, voyages.Length);
			AssertEquals(voyages[0].PK, jobVoyage.PK);
			AssertEquals(1, voyages[0].Origins.Count);
			AssertEquals(1, voyages[0].Destinations.Count);

			var voyageOrigin = voyages[0].Origins[0];
			AssertEquals("AUSYD", voyageOrigin.JA_RL_NKPortOfLoading);
			AssertEquals("ZZZ_01", voyageOrigin.JA_DepartReference);
			AssertEquals(cto.PK, voyageOrigin.JA_Calc_DepartureCTOAddressOrg);
			AssertEquals(new DateTime(2017, 1, 9), voyageOrigin.JA_E_DEP);
			AssertEquals(ZDateTime.Empty, voyageOrigin.JA_E_ARV);
			AssertEquals(new DateTime(2017, 1, 10), voyageOrigin.JA_A_DEP);
			AssertEquals(ZDateTime.Empty, voyageOrigin.JA_A_ARV);

			var voyageDestination = voyages[0].Destinations[0];
			AssertEquals("AUBNE", voyageDestination.JB_RL_NKPortOfDischarge);
			AssertEquals(new DateTime(2017, 1, 12), voyageDestination.JB_E_ARV);
			AssertEquals(new DateTime(2017, 1, 13), voyageDestination.JB_A_ARV);
			AssertEquals(new DateTime(2017, 1, 14), voyageDestination.JB_AvailabilityDate);
			AssertEquals(new DateTime(2017, 1, 15), voyageDestination.JB_StorageDate);

			var sailingsQuery = new ZQuery(JobSailingSchema.JX_JA, voyageOrigin.PK);
			sailingsQuery.AddToFilter(JobSailingSchema.JX_JB, voyageDestination.PK);
			var sailings = Factory.Load<JobSailing>(sailingsQuery);
			AssertEquals(1, sailings.Length);
		}

		public void TestCreateEnterpriseVoyage_CarrierExists()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(vesselName: "Santa Maria");

			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = Scac;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			AssertNoExceptionThrown(leg.CreateEnterpriseVoyage);
		}

		public void TestCreateEnterpriseVoyage_CarrierDoesNotExist()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(vesselName: "Santa Maria");

			Factory.Save();

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			AssertExceptionThrown<InvalidOperationException>("Can't match Carrier SCAC CMAC to an existing carrier: there are 0 carriers with this SCAC.", leg.CreateEnterpriseVoyage);
		}

		public void TestCreateEnterpriseVoyage_MultipleCarriersExist()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(vesselName: "Santa Maria");

			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = Scac;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var entCarrier2 = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier2.OH_Code = "CMACGM2";
			var cusCode2 = entCarrier2.CustomsCodes.AddNew();
			cusCode2.OK_CustomsRegNo = Scac;
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			AssertExceptionThrown<InvalidOperationException>("Can't match Carrier SCAC CMAK to an existing carrier: there are 2 carriers with this SCAC.", leg.CreateEnterpriseVoyage);
		}

		public void TestCreateEnterpriseVoyage_PopulatesSailingServiceString()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(tradeLaneName: "myServiceString");

			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "SHMAERSK";
			entCarrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, Scac, Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			leg.CreateEnterpriseVoyage();

			var voyage = GetVoyages(entCarrier, leg)[0];
			AssertEquals("myServiceString", voyage.Sailings[0].JX_ServiceString);
		}

		public void TestCreateEnterpriseVoyage_Destinaton_LastForeignPortFirstArrivalPortPopulated()
		{
			// Arrange
			var (_, serviceLeg) = CreateVoyageAndServiceLeg();

			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = Scac;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();
			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);

			var portMatches = new PortMatches(
					lastForeignPort: new PortMatch(unloco: "AUBNE",
						arrivalTime: new DateTimeOffset(2023, 3, 4, 10, 0, 0, TimeSpan.FromHours(+8)),
						departureTime: new DateTimeOffset(2023, 3, 5, 8, 0, 0, TimeSpan.FromHours(+8))),
					firstArrivalPort: new PortMatch(unloco: "CNTAO",
						arrivalTime: new DateTimeOffset(2023, 3, 13, 19, 0, 0, TimeSpan.FromHours(+11)),
						departureTime: new DateTimeOffset(2023, 3, 14, 22, 0, 0, TimeSpan.FromHours(+11))));

			var portMatcher = new Mock<IPortMatcher>();
			portMatcher
				.Setup(matcher => matcher.MatchAsync(It.IsAny<VesselMovementsUrlModel>(), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult<IPortMatches>(portMatches));

			// Act & Assert
			using (ObjectFactory.Substitute(portMatcher.Object))
			{
				leg.CreateEnterpriseVoyage();

				Factory.Save();
				var destination = GetVoyages(entCarrier, leg)[0].Destinations.GetDestinationFromDischarge("AUBNE");
				AssertNotNull(destination);

				var expectedVesselMovementsUrlModel = new VesselMovementsUrlModel
				{
					DeparturePortUnloco = "AUSYD",
					DepartureTime = EtdConst,
					ArrivalPortUnloco = "AUBNE",
					ArrivalTime = EtaConst,
					CarrierCode = Scac,
					LloydsNumber = ImoNumber,
					VoyageNumber = "754N"
				};

				AssertNoExceptionThrown("Port Matching must be executed.",
					() => portMatcher.Verify(matcher => matcher.MatchAsync(expectedVesselMovementsUrlModel, It.IsAny<CancellationToken>()), Times.Once()));

				AssertEquals(portMatches.LastForeignPort.Unloco, destination.JB_RL_NKLastForeignPort);
				AssertEquals(portMatches.LastForeignPort.DepartureTime?.DateTime, destination.JB_LastForeignPortETD.ToDateTime());
				AssertEquals(portMatches.FirstArrivalPort.Unloco, destination.JB_RL_NKFirstDischargePort);
				AssertEquals(portMatches.FirstArrivalPort.ArrivalTime?.DateTime, destination.JB_FirstDischargePortETA.ToDateTime());
			}
		}

		public void TestFindMatchingJobSailing()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "SHMAERSK";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SHMA", Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VALHALLA SHMAERSK";
			vessel.RV_LloydsNumber = "1024";

			Factory.Save();

			var today = DateTime.Today;

			var serviceLeg = new ServiceModel.Leg
			{
				LoadPort = new ServiceModel.Port { Unloco = "AUSYD" },
				DischargePort = new ServiceModel.Port { Unloco = "NZAKL" },
				Etd = today,
				Eta = today.AddDays(5),
				Voyage = new Voyage
				{
					Code = "100",
					Vessel = new Vessel { VesselName = "VALHALLA SHMAERSK", ImoNumber = "1024" },
				}
			};

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);

			AssertNull(leg.FindMatchingJobSailing());

			var voyage = Factory.New<JobVoyage>();
			Action setupMatchingVoyage = () =>
			{
				voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
				voyage.JV_RV_NKVessel = "VALHALLA SHMAERSK";
				voyage.JV_VoyageFlight = "100";
				voyage.JV_OH_Line = carrier.PK;

				voyage.Sailings.RemoveAndDeleteAll();

				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";

				voyage.GenerateSailings();
			};

			setupMatchingVoyage();

			serviceLeg.Voyage.Operator = new Carrier { Name = "SHMA Name", Code = "SHMA" };
			leg.SetValues(serviceLeg);

			var sailing = leg.FindMatchingJobSailing();
			AssertEquals("Sailing from a matched voyage", voyage, sailing.Voyage);
			AssertEquals("Sailing with matched ports", "AUSYD-NZAKL", sailing.JX_JA_RL_NKPortOfLoading + "-" + sailing.JX_JB_RL_NKPortOfDischarge);

			Action<Action> assertVoyageWouldNotBeMatched = (changeVoyageToBecomeNonMatching) =>
			{
				setupMatchingVoyage();
				AssertNotNull("Prerequisite", leg.FindMatchingJobSailing());

				changeVoyageToBecomeNonMatching();
				AssertNull(leg.FindMatchingJobSailing());
			};

			assertVoyageWouldNotBeMatched(() => voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Rail);
			assertVoyageWouldNotBeMatched(() => voyage.JV_RV_NKVessel = "RANDOM SHIP");
			assertVoyageWouldNotBeMatched(() => voyage.JV_VoyageFlight = "1");
			assertVoyageWouldNotBeMatched(() => voyage.JV_OH_Line = Factory.New<OrgHeader>().PK);
		}

		public void TestFindMatchingJobSailing_AllowMatchOnDifferentVesselName()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "SHMAERSK";
			carrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "SHMA", Core.Constants.CountryCodes.UnitedStates);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "VALHALLA SHMAERSK";
			vessel.RV_LloydsNumber = "1024";

			Factory.Save();

			var today = DateTime.Today;

			var serviceLeg = new ServiceModel.Leg
			{
				LoadPort = new ServiceModel.Port { Unloco = "AUSYD" },
				DischargePort = new ServiceModel.Port { Unloco = "NZAKL" },
				Etd = today,
				Eta = today.AddDays(5),
				Voyage = new Voyage
				{
					Code = "100",
					Vessel = new Vessel { VesselName = "VALHALLA SHMAERSK", ImoNumber = "1024" },
				}
			};

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);

			AssertNull(leg.FindMatchingJobSailing(true));

			var voyage = Factory.New<JobVoyage>();
			Action setupMatchingVoyage = () =>
			{
				voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
				voyage.JV_RV_NKVessel = "VALHALLA SHMAERSK";
				voyage.JV_VoyageFlight = "100";
				voyage.JV_OH_Line = carrier.PK;

				voyage.Sailings.RemoveAndDeleteAll();

				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";

				voyage.GenerateSailings();
			};

			setupMatchingVoyage();

			serviceLeg.Voyage.Operator = new Carrier { Name = "SHMA Name", Code = "SHMA" };
			leg.SetValues(serviceLeg);

			Action<Action> assertVoyageWouldBeMatched = (changeVoyageToBecomeNonMatching) =>
			{
				setupMatchingVoyage();
				AssertNotNull("Prerequisite", leg.FindMatchingJobSailing());

				changeVoyageToBecomeNonMatching();
				AssertNull(leg.FindMatchingJobSailing(true));
			};

			assertVoyageWouldBeMatched(() => voyage.JV_RV_NKVessel = "RANDOM SHIP");
		}

		public void TestUpdateVesselImo()
		{
			const string vesselName = "TmpNameABC";
			const string imoNumber = "9876543";

			var (_, serviceLeg) = CreateVoyageAndServiceLeg(vesselName: vesselName, imoNumber: imoNumber);

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			Factory.Save();

			Assert(leg.ExistingVesselWithSameVesselNameButDifferentImo == null);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = vesselName;
			vessel.RV_LloydsNumber = "0";
			vessel.RV_IsActive = false;
			Factory.Save();

			Assert(leg.ExistingVesselWithSameVesselNameButDifferentImo == null);

			vessel.RV_IsActive = true;
			Factory.Save();

			Assert(leg.ExistingVesselWithSameVesselNameButDifferentImo == null);

			vessel.RV_LloydsNumber = "3334444";
			Factory.Save();

			var existingVesselImoWithSameNameButDifferentImo = leg.ExistingVesselWithSameVesselNameButDifferentImo;
			Assert(existingVesselImoWithSameNameButDifferentImo != null);

			string resultMessage;
			Assert(leg.UpdateVesselImo(existingVesselImoWithSameNameButDifferentImo, out resultMessage));
			AssertEquals("The IMO 3334444 of existing vessel with name TmpNameABC has been substituted with IMO 9876543.", resultMessage);

			Assert(vessel.RV_IsActive);
			AssertEquals(vessel.RV_LloydsNumber, imoNumber);

			Assert(leg.ExistingVesselWithSameVesselNameButDifferentImo == null);
		}

		public void TestUpdateVesselImoError()
		{
			const string vesselName = "TmpNameABC";
			const string imoNumber = "9876543";
			string resultMessage;

			var (_, serviceLeg) = CreateVoyageAndServiceLeg(vesselName: vesselName, imoNumber: imoNumber);

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = vesselName;
			vessel.RV_LloydsNumber = "3334444";
			vessel.RV_IsActive = true;

			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "TmpNameDEF";
			vessel2.RV_LloydsNumber = imoNumber;
			vessel2.RV_IsActive = true;

			Factory.Save();

			var existingVesselImoWithSameNameButDifferentImo = leg.ExistingVesselWithSameVesselNameButDifferentImo;
			Assert(existingVesselImoWithSameNameButDifferentImo != null);
			Assert(!leg.UpdateVesselImo(existingVesselImoWithSameNameButDifferentImo, out resultMessage));
			AssertEquals("This IMO number can not be updated because it belongs to existing vessel.", resultMessage);

			var vessel3 = Factory.New<RefVessel>();
			vessel3.RV_Name = "TmpNameGHI";
			vessel3.RV_LloydsNumber = imoNumber;
			vessel3.RV_IsActive = true;
			Factory.Save();

			existingVesselImoWithSameNameButDifferentImo = leg.ExistingVesselWithSameVesselNameButDifferentImo;
			Assert(existingVesselImoWithSameNameButDifferentImo != null);
			Assert(!leg.UpdateVesselImo(existingVesselImoWithSameNameButDifferentImo, out resultMessage));
			AssertEquals("This IMO number can not be updated because it belongs to existing vessel.", resultMessage);

			vessel.RV_LloydsNumber = imoNumber;
			vessel2.RV_LloydsNumber = "1111111";
			vessel3.RV_LloydsNumber = "2222222";
			Factory.Save();

			Assert(!leg.UpdateVesselImo(existingVesselImoWithSameNameButDifferentImo, out resultMessage));
			AssertEquals("Another user has already changed vessel you’re trying to update while you were working with schedules.", resultMessage);
		}

		public void TestCreateNewVesselNameError()
		{
			const string vesselName = "TmpNameABC";
			const string anotherName = "TmpNameXYZ";
			const string imoNumber = "9876543";
			string resultMessage;

			var (_, serviceLeg) = CreateVoyageAndServiceLeg(vesselName: vesselName, imoNumber: imoNumber);

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			Factory.Save();

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = anotherName;
			vessel.RV_LloydsNumber = imoNumber;
			vessel.RV_IsActive = true;
			Factory.Save();

			var existingVesselWithSameImoButDifferentVesselName = leg.ExistingVesselWithSameImoButDifferentVesselName;
			Assert(existingVesselWithSameImoButDifferentVesselName != null);

			vessel.RV_LloydsNumber = "3334444";
			Factory.Save();

			Assert(!leg.CreateNewVessel(existingVesselWithSameImoButDifferentVesselName, out resultMessage));
			AssertEquals("Another user has already changed vessel you’re trying to update while you were working with schedules.", resultMessage);

			vessel.RV_Name = vesselName;
			vessel.RV_LloydsNumber = imoNumber;
			Factory.Save();

			Assert(!leg.CreateNewVessel(existingVesselWithSameImoButDifferentVesselName, out resultMessage));
			AssertEquals("Another user has already changed vessel you’re trying to update while you were working with schedules.", resultMessage);

			vessel.RV_Name = anotherName;
			var vessel2 = Factory.New<RefVessel>();
			vessel2.RV_Name = "TmpNameDEF";
			vessel2.RV_LloydsNumber = imoNumber;
			vessel2.RV_IsActive = true;
			Factory.Save();

			Assert(!leg.CreateNewVessel(existingVesselWithSameImoButDifferentVesselName, out resultMessage));
			AssertEquals("Vessel with name TmpNameABC can’t be created/activated because more than one vessel found by IMO 9876543", resultMessage);
		}

		public void TestCreateNewVesselName()
		{
			const string vesselName = "TmpNameABC";
			const string anotherName = "TmpNameXYZ";
			const string imoNumber = "9876543";
			string resultMessage;

			var (_, serviceLeg) = CreateVoyageAndServiceLeg(vesselName: vesselName, imoNumber: imoNumber);

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			Factory.Save();

			Assert(leg.ExistingVesselWithSameImoButDifferentVesselName == null);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = vesselName;
			vessel.RV_LloydsNumber = imoNumber;
			vessel.RV_IsActive = false;
			Factory.Save();

			Assert(leg.ExistingVesselWithSameImoButDifferentVesselName == null);

			vessel.RV_IsActive = true;
			Factory.Save();

			Assert(leg.ExistingVesselWithSameImoButDifferentVesselName == null);

			vessel.RV_Name = anotherName;
			Factory.Save();

			var existingVesselWithSameImoButDifferentVesselName = leg.ExistingVesselWithSameImoButDifferentVesselName;
			Assert(existingVesselWithSameImoButDifferentVesselName != null);

			var newVessel = RefVessel.LookupVesselByName(leg.VesselName, Factory).FirstOrDefault();
			AssertEquals(null, newVessel);

			Assert(leg.CreateNewVessel(existingVesselWithSameImoButDifferentVesselName, out resultMessage));
			AssertEquals($"The existing vessel with name {vessel.RV_Name} has been deactivated, and the vessel with name {leg.VesselName} has been created.", resultMessage);
			Assert(!vessel.RV_IsActive);

			newVessel = RefVessel.LookupVesselByName(leg.VesselName, Factory).FirstOrDefault();
			Assert(newVessel != null);
			Assert(newVessel.RV_IsActive);
			AssertEquals(imoNumber, newVessel.RV_LloydsNumber);
			AssertEquals(vesselName, newVessel.RV_Name);

			vessel.RV_IsActive = true;
			newVessel.RV_IsActive = false;
			Factory.Save();

			existingVesselWithSameImoButDifferentVesselName = leg.ExistingVesselWithSameImoButDifferentVesselName;
			Assert(existingVesselWithSameImoButDifferentVesselName != null);

			Assert(leg.CreateNewVessel(existingVesselWithSameImoButDifferentVesselName, out resultMessage));
			AssertEquals($"The existing vessel with name {vessel.RV_Name} has been deactivated, and the vessel with name {leg.VesselName} has been activated.", resultMessage);

			Assert(newVessel != null);
			Assert(newVessel.RV_IsActive);
			AssertEquals(imoNumber, newVessel.RV_LloydsNumber);
			AssertEquals(vesselName, newVessel.RV_Name);
		}

		public void TestAssignScacCodeToCarrierEnabled()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(imoNumber: "9308390");

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);

			Assert(leg.ScacCodeCanBeAssginedToCarrier);

			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = Scac;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			Assert(!leg.ScacCodeCanBeAssginedToCarrier);
		}

		public void TestAssignScacCodeToCarrier_NotActiveOrgHeader()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(imoNumber: "9308390");

			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			entCarrier.OH_IsActive = false;
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = Scac;
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);

			AssertEquals(true, leg.ScacCodeCanBeAssginedToCarrier);
		}

		public void TestGetCarrier_ReturnsNull_When_ThereAreTwoCarriers_And_BothAreActive()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(imoNumber: "9308390");

			var entCarrier1 = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier1.OH_IsActive = true;
			var cusCode1 = entCarrier1.CustomsCodes.AddNew();
			cusCode1.OK_CustomsRegNo = Scac;
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var entCarrier2 = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier2.OH_IsActive = true;
			var cusCode2 = entCarrier2.CustomsCodes.AddNew();
			cusCode2.OK_CustomsRegNo = Scac;
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);

			AssertNull(leg.Carrier);
		}

		public void TestGetCarrier_ReturnsActiveCarrier_When_ThereAreTwoCarriers_And_OnlyOneIsActive()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(imoNumber: "9308390");

			var entCarrier1 = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier1.OH_IsActive = false;
			var cusCode1 = entCarrier1.CustomsCodes.AddNew();
			cusCode1.OK_CustomsRegNo = Scac;
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var entCarrier2 = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier2.OH_IsActive = true;
			var cusCode2 = entCarrier2.CustomsCodes.AddNew();
			cusCode2.OK_CustomsRegNo = Scac;
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			var carrier = leg.Carrier;

			AssertNotNull(carrier);
			AssertEquals(entCarrier2.OH_Code, carrier.OH_Code);
		}

		public void TestAssignScacCodeToCarrier()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(imoNumber: "9308390");

			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			Factory.Save();

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);

			Assert(leg.ScacCodeCanBeAssginedToCarrier);
			AssertEquals(0, entCarrier.CustomsCodes.Count);

			string resultMessage;
			Assert(leg.AssignScacCodeToCarrier(entCarrier, out resultMessage));
			AssertEquals("The SCAC code CMAC has been assigned to the carrier CMACGM.", resultMessage);

			AssertEquals(1, entCarrier.CustomsCodes.Count);
			var cusCode = entCarrier.CustomsCodes[0];
			AssertEquals(Scac, cusCode.OK_CustomsRegNo);
			AssertEquals(OrgCusCode.CodeTypes.CarrierCode, cusCode.OK_CodeType);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, cusCode.OK_RN_NKCodeCountry);

			Assert(!leg.ScacCodeCanBeAssginedToCarrier);
		}

		public void TestAssignScacCodeToCarrierError()
		{
			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			var cusCode = entCarrier.CustomsCodes.AddNew();
			cusCode.OK_CustomsRegNo = "XYZA";
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var (_, serviceLeg) = CreateVoyageAndServiceLeg(imoNumber: "9308390");

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);

			Assert(leg.ScacCodeCanBeAssginedToCarrier);

			cusCode.OK_CustomsRegNo = Scac;
			Factory.Save();

			string resultMessage;
			Assert(!leg.AssignScacCodeToCarrier(entCarrier, out resultMessage));
			AssertEquals("Another user has already changed carrier you’re trying to update while you were working with schedules.", resultMessage);
		}

		public void TestCreateNewVessel_WithConCurrency()
		{
			const string vesselName = "TmpNameABC";
			const string anotherName = "TmpNameXYZ";
			const string imoNumber = "9876543";
			string resultMessage;

			var (_, serviceLeg) = CreateVoyageAndServiceLeg(vesselName: vesselName, imoNumber: imoNumber);

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			Factory.Save();

			Assert(leg.ExistingVesselWithSameImoButDifferentVesselName == null);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = vesselName;
			vessel.RV_LloydsNumber = imoNumber;
			vessel.RV_IsActive = false;
			Factory.Save();

			Assert(leg.ExistingVesselWithSameImoButDifferentVesselName == null);

			vessel.RV_IsActive = true;
			Factory.Save();

			Assert(leg.ExistingVesselWithSameImoButDifferentVesselName == null);

			vessel.RV_Name = anotherName;
			Factory.Save();

			var existingVesselWithSameImoButDifferentVesselName = leg.ExistingVesselWithSameImoButDifferentVesselName;
			Assert(existingVesselWithSameImoButDifferentVesselName != null);

			var newVessel = RefVessel.LookupVesselByName(leg.VesselName, Factory).FirstOrDefault();
			AssertEquals(null, newVessel);

			Assert(leg.CreateNewVessel(existingVesselWithSameImoButDifferentVesselName, out resultMessage));
			AssertEquals($"The existing vessel with name {vessel.RV_Name} has been deactivated, and the vessel with name {leg.VesselName} has been created.", resultMessage);
			Assert(!vessel.RV_IsActive);

			newVessel = RefVessel.LookupVesselByName(leg.VesselName, Factory).FirstOrDefault();
			Assert(newVessel != null);
			Assert(newVessel.RV_IsActive);
			AssertEquals(imoNumber, newVessel.RV_LloydsNumber);
			AssertEquals(vesselName, newVessel.RV_Name);

			vessel.RV_IsActive = true;
			newVessel.RV_IsActive = false;
			Factory.Save();

			existingVesselWithSameImoButDifferentVesselName = leg.ExistingVesselWithSameImoButDifferentVesselName;
			Assert(existingVesselWithSameImoButDifferentVesselName != null);

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var vessel2 = RefVessel.LookupVesselByName(vesselName, factory2, true).FirstOrDefault();
			vessel2.RV_LloydsNumber = "9876577";
			factory2.Save();

			AssertNoExceptionThrown(() => leg.CreateNewVessel(existingVesselWithSameImoButDifferentVesselName, out resultMessage));
		}

		public void TestUpdateVesselImo_WithConCurrency()
		{
			const string vesselName = "TmpNameABC";
			const string imoNumber = "9876543";

			var (_, serviceLeg) = CreateVoyageAndServiceLeg(vesselName: vesselName, imoNumber: imoNumber);

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);
			Factory.Save();

			Assert(leg.ExistingVesselWithSameVesselNameButDifferentImo == null);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = vesselName;
			vessel.RV_LloydsNumber = "0";
			vessel.RV_IsActive = false;
			Factory.Save();

			Assert(leg.ExistingVesselWithSameVesselNameButDifferentImo == null);

			vessel.RV_IsActive = true;
			Factory.Save();

			Assert(leg.ExistingVesselWithSameVesselNameButDifferentImo == null);

			vessel.RV_LloydsNumber = "3334444";
			Factory.Save();

			var existingVesselImoWithSameNameButDifferentImo = leg.ExistingVesselWithSameVesselNameButDifferentImo;
			Assert(existingVesselImoWithSameNameButDifferentImo != null);

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var vessel2 = RefVessel.LookupVesselByName(vesselName, factory2).FirstOrDefault();
			vessel2.RV_LloydsNumber = "9876577";
			factory2.Save();

			AssertNoExceptionThrown(() => leg.UpdateVesselImo(existingVesselImoWithSameNameButDifferentImo, out var resultMessage));
		}

		public void TestAssignScacCodeToCarrier_WithConCurrency()
		{
			var (_, serviceLeg) = CreateVoyageAndServiceLeg(imoNumber: "9308390");

			var entCarrier = Factory.NewWithValidTestData<OrgHeader>();
			entCarrier.OH_Code = "CMACGM";
			Factory.Save();

			var leg = new Leg(Factory);
			leg.SetValues(serviceLeg);

			Assert(leg.ScacCodeCanBeAssginedToCarrier);
			AssertEquals(0, entCarrier.CustomsCodes.Count);

			var cusCode1 = entCarrier.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			cusCode1.OK_CustomsRegNo = "APLU";
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var carrier2 = factory2.Load<OrgHeader>(entCarrier.PK);
			AssertNotNull(carrier2);
			var cusCode2 = factory2.Load<OrgCusCode>(cusCode1.PK);
			AssertNotNull(cusCode2);
			cusCode2.OK_CustomsRegNo = "CDEF";
			factory2.Save();

			AssertNoExceptionThrown(() => leg.AssignScacCodeToCarrier(entCarrier, out var resultMessage));
		}

		#region Implementation

		JobVoyage[] GetVoyages(OrgHeader carrier, Leg leg)
		{
			var query = new ZQuery(JobVoyageSchema.JV_VoyageFlight, leg.VoyageCode);
			query.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, leg.VesselName);
			query.AddToFilter(JobVoyageSchema.JV_OH_Line, carrier.PK);
			query.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, Core.Constants.TransportModes.Sea);
			query.AddToFilter(JobVoyageSchema.JV_IsActive, true);

			return Factory.Load<JobVoyage>(query);
		}

		static (Voyage, ServiceModel.Leg) CreateVoyageAndServiceLeg(string vesselName = "Santa Maria", string imoNumber = ImoNumber, string carrierCode = Scac, DateTime? etd = null, DateTime? eta = null, string tradeLaneName = "AAA")
		{
			var voyage = new Voyage
			{
				Code = "754N",
				TradeLane = new TradeLane
				{
					Name = tradeLaneName
				},
				Operator = new Carrier
				{
					Code = carrierCode,
					Name = "CMA CGM"
				},
				Vessel = new Vessel
				{
					VesselName = vesselName,
					ImoNumber = imoNumber
				}
			};

			var serviceLeg = new ServiceModel.Leg
			{
				LoadPort = new ServiceModel.Port
				{
					Unloco = "AUSYD"
				},
				DischargePort = new ServiceModel.Port
				{
					Unloco = "AUBNE"
				},
				Etd = etd ?? EtdConst,
				Eta = eta ?? EtaConst,
				Voyage = voyage,
				DepartureReference = DepartureReference
			};
			return (voyage, serviceLeg);
		}

		const string Scac = "CMAC";
		const string ImoNumber = "1234567";
		const string DepartureReference = "ABC";
		static readonly DateTime EtdConst = new DateTime(2016, 8, 10);
		static readonly DateTime EtaConst = new DateTime(2016, 8, 15);
		#endregion
	}
}
