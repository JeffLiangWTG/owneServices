using System;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration.Schedule;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	public class TransportValidationTest : BaseFreightTest
	{
		public void TestCarrierBookingReference()
		{
			var carrier1 = Factory.New<OrgHeader>();
			var carrier2 = Factory.New<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_IsShippingProvider = true;
			var consol = Factory.New<CommonConsol>();

			Transport route = consol.Transports.AddNew();
			route.JW_Status = Constants.TransportStatus.Confirmed;
			AssertNoWarnings(route.JW_CarrierBookingReferenceInfo);

			route.CarrierPK = carrier1.PK;
			consol.SetDefaultShippingLineAddress(carrier1);
			AssertNoWarnings(route.JW_CarrierBookingReferenceInfo);

			route.CarrierPK = carrier2.PK;
			AssertHasWarning(route.JW_CarrierBookingReferenceInfo, "You have not entered a " + route.JW_CarrierBookingReferenceInfo.Description + ".");

			route.JW_CarrierBookingReference = "ICCC556";
			AssertNoWarnings(route.JW_CarrierBookingReferenceInfo);
		}

		[ExpectNoExceptions]
		public void TestEmptyTransportParentDoesNotThowException()
		{
			var route = Factory.NewWithValidTestData<Transport>();
			route.ParentType = typeof(CommonConsol);
			route.JW_Status = Constants.TransportStatus.Confirmed;
			route.CarrierPK = Factory.New<OrgHeader>().PK;
			route.Validation.ValidateJW_CarrierBookingReference();
		}

		public void TestCarrierPK()
		{
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			Transport transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			transport.CarrierPK = carrier.PK;

			AssertNoErrors("The Carrier is a valid Carrier organization", transport.CarrierPKInfo);

			OrgHeader creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsShippingLine = false;
			creditor.OH_IsCreditor = true;
			creditor.OH_IsAirLine = false;
			creditor.OH_FullName = "Creditor";
			creditor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			transport.CarrierPK = creditor.PK;

			AssertHasErrors("The Creditor is not a valid Carrier organization", transport.CarrierPKInfo);
		}

		public void TestCreditorPK()
		{
			const string expectedError = "The selected organization is not valid. Please choose a new organization or amend the organization using F3.";
			CommonConsol consol = Factory.NewWithValidTestData<CommonConsol>();
			Transport transport = consol.Transports.AddNew();

			OrgHeader creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			creditor.OH_FullName = "Creditor";

			Factory.Save();

			transport.CreditorPK = creditor.PK;
			transport.JW_OA_CreditorAddress = creditor.MainAddress.PK;

			AssertNoErrors(expectedError, transport.CreditorPKInfo);

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			transport.CreditorPK = carrier.PK;

			AssertHasError("Has the expected error", transport.CreditorPKInfo, expectedError);
		}

		public void TestValidateCarrierServiceLevels()
		{
			Transport transport = Factory.New<CommonConsol>().Transports.AddNew();

			OrgHeader carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			transport.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			OrgCarrierServiceLevel serviceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "XXX";
			serviceLevel.PL_CarrierServiceLevelDescription = "XXX";

			transport.JW_PL_NKCarrierServiceLevel = "XXX";
			AssertNoErrors("The Carrier Service Level is from carrier organization", transport.JW_PL_NKCarrierServiceLevelInfo);

			OrgHeader carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			carrier1.OH_IsShippingLine = true;
			carrier1.OH_FullName = "Carrier";
			carrier1.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			transport.JW_OA_CarrierAddress = carrier1.MainAddress.PK;

			OrgCarrierServiceLevel serviceLevel1 = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel1.PL_Code = "YYY";
			serviceLevel1.PL_CarrierServiceLevelDescription = "YYY";

			transport.JW_PL_NKCarrierServiceLevel = "XXX";
			AssertHasErrors("The Carrier Service Level is not from the carrier1 organization", transport.JW_PL_NKCarrierServiceLevelInfo);

			carrier1.Delete();

			transport.JW_PL_NKCarrierServiceLevel = "XXX";
			AssertHasErrors("The Carrier Service Level is not from the carrier organization", transport.JW_PL_NKCarrierServiceLevelInfo);
		}

		public void TestAllowEditValidation()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "QF1";
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUBNE";
			voyage.JV_FlightDate = origin.JA_E_DEP = new ZDateTime(2010, 7, 25, 11, 0, 0);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = new ZDateTime(2010, 7, 26, 9, 0, 0);
			voyage.GenerateSailings();

			AssertEquals("Prerequisite", 1, voyage.Sailings.Count);

			var sailing = voyage.Sailings[0];
			sailing.JX_IsPublished = true;

			Factory.Save();

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = origin.JA_RL_NKPortOfLoading;
			consol.JK_RL_NKDischargePort = destination.JB_RL_NKPortOfDischarge;

			var transport = consol.MostInterestingTransportForBinding[0];

			AssertEquals("Prerequisite", true, transport.JW_IsLinked);
			AssertEquals("Prerequisite", Core.Constants.TransportModes.Air, transport.JW_TransportMode);
			AssertEquals("Prerequisite", consol.JK_RL_NKLoadPort, transport.JW_RL_NKLoadPort);
			AssertEquals("Prerequisite", consol.JK_RL_NKDischargePort, transport.JW_RL_NKDiscPort);

			transport.JW_VoyageFlight = voyage.JV_VoyageFlight;
			transport.JW_ETD = origin.JA_E_DEP; // to find sailing
			AssertEquals("Prerequisite; should have found sailing", sailing.PK, transport.JW_JX);

			Env.Security.FlightScheduleEdit.IsAllowed = false;

			try
			{
				const string errorMsg = "You do not have security rights to edit this value.\r\nIf you believe you should have rights then ask your administrator for the Operate -> Schedules -> Flight Schedule -> Edit security right.";

				ZPropertyInfo[] propertiesToValidate = new ZPropertyInfo[]
				{
					transport.JW_DocumentaryCutOffInfo,
					transport.JW_DepotReceivalCommencesInfo,
					transport.JW_TerminalReceivalCommencesInfo,
					transport.JW_DepotCutOffInfo,
					transport.JW_TerminalCutOffInfo,
					transport.JW_TerminalAvailabilityDateInfo,
					transport.JW_DepotAvailabilityDateInfo,
					transport.JW_TerminalStorageDateInfo,
					transport.JW_DepotStorageDateInfo,
					transport.JW_JX_IsPublishedInfo,
					transport.JW_ETAInfo,
					transport.JW_ETDInfo,
					transport.JW_ATDInfo,
					transport.JW_ATAInfo,
					transport.JW_JX_Load_ETAInfo,
					transport.JW_JX_Load_ATAInfo
				};

				foreach (ZPropertyInfo info in propertiesToValidate)
				{
					IZType oldValue = info.Value;

					AssertNoError(String.Format("{0} should have no error", info.Name), info, errorMsg);

					MakeDirty(info);

					AssertHasError(String.Format("{0} should have error", info.Name), info, errorMsg);

					info.Value = oldValue;

					AssertNoError(String.Format("Resetting; {0} should have no error", info.Name), info, errorMsg);
				}
			}
			finally
			{
				Env.Security.FlightScheduleEdit.IsAllowed = true;
			}
		}

		void MakeDirty(ZPropertyInfo info)
		{
			if (info.PropertyType == typeof(ZDateTime))
			{
				info.Value = info.Value.IsEmpty ? ZDateTime.Today : ((ZDateTime)info.Value).AddHours(1);
			}
			else if (info.PropertyType == typeof(ZBool))
			{
				info.Value = (ZBool)info.Value ? ZBool.False : ZBool.True;
			}
			else
			{
				throw new NotImplementedException(String.Format("{0} type is not supported by MakeDirty(ZPropertyInfo info) method", info.PropertyType.Name));
			}
		}

		public void TestSecurityValidation()
		{
			var vessel = RefVessel.LookupVesselByName("CONDOR", Factory).First();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "012";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NLAMS";
			voyage.GenerateSailings();

			Factory.Save();

			Transport.JW_IsLinked = true;
			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			Transport.JW_Vessel = "CONDOR";
			Transport.JW_VoyageFlight = "F111";
			Transport.JW_RL_NKLoadPort = "AUBNE";
			Transport.JW_RL_NKDiscPort = "NLAMS";

			ZPropertyInfo[] infos = new ZPropertyInfo[]
			{
				Transport.JW_IsLinkedInfo,
				Transport.JW_TransportModeInfo,
				Transport.JW_VesselInfo,
				Transport.JW_VoyageFlightInfo,
				Transport.JW_RL_NKLoadPortInfo,
				Transport.JW_RL_NKDiscPortInfo,
				Transport.JW_JX_Load_ETAInfo,
				Transport.JW_JX_Load_ATAInfo
			};

			const string error = "No matching schedule could be found and you do not have the security rights to create one. If you believe you should have rights then ask your administrator for the Operate -> Schedules -> Sailing Schedule -> Create From Job security right.";

			Env.Security.SailingScheduleCreateFromJob.IsAllowed = false;
			Transport.Validation.ValidateAll();
			AssertHasError(false, error, infos);

			Transport.JW_RL_NKLoadPort = "AUSYD";
			Transport.Validation.ValidateAll();
			AssertHasError(true, error, infos);

			Transport.JW_IsLinked = false;
			Transport.Validation.ValidateAll();
			AssertHasError(false, error, infos);

			Transport.JW_IsLinked = true;
			AssertHasError(string.Format("{0} should have the error", Transport.JW_IsLinkedInfo.Name), Transport.JW_IsLinkedInfo, error);

			Env.Security.SailingScheduleCreateFromJob.IsAllowed = true;
			Transport.Validation.ValidateAll();
			AssertHasError(false, error, infos);
		}

		void AssertHasError(bool expectedError, string errorMessage, ZPropertyInfo[] infos)
		{
			CombineAssertions(delegate
			{
				if (expectedError)
				{
					foreach (var info in infos)
					{
						AssertHasError(string.Format("{0} should have the error", info.Name), info, errorMessage);
					}
				}
				else
				{
					foreach (var info in infos)
					{
						AssertNoError(string.Format("{0} should not have the error", info.Name), info, errorMessage);
					}
				}
			});
		}

		public void TestJW_Status()
		{
			string missingValue = "Please enter a " + Transport.JW_StatusInfo.Description + ".";
			string invalidValue = "Enter a valid " + Transport.JW_StatusInfo.Description + ".";

			Transport.JW_Status = "";
			Transport.Validation.ValidateJW_Status();
			AssertHasError(Transport.JW_StatusInfo, missingValue);

			Transport.JW_Status = Constants.TransportStatus.Confirmed;
			AssertNoNotifications(Transport.JW_StatusInfo);

			Transport.JW_Status = "XXX";
			AssertHasError(Transport.JW_StatusInfo, invalidValue);
		}

		public void TestJW_DistanceUnit()
		{
			AssertNoErrors("Precondition", Transport.JW_DistanceUnitInfo);

			Transport.JW_Distance = 10m;
			Transport.Validation.ValidateJW_DistanceUnit();
			AssertHasErrors("Distance units mandatory if the distance entered", Transport.JW_DistanceUnitInfo);

			Transport.JW_DistanceUnit = Core.Constants.Length.Kilometres;
			Transport.Validation.ValidateJW_DistanceUnit();
			AssertNoErrors("No errors", Transport.JW_DistanceUnitInfo);

			Transport.JW_DistanceUnit = "XXX";
			Transport.Validation.ValidateJW_DistanceUnit();
			AssertHasErrors("Wrong distance unit", Transport.JW_DistanceUnitInfo);
		}

		public void TestManditoryDates()
		{
			foreach (ZString loadPort in new ZString[] { HomePort, OverseasPort })
			{
				foreach (ZString dischargePort in new ZString[] { AlternateHomePort, OverseasPort2 })
				{
					Transport.JW_RL_NKLoadPort = loadPort;
					Transport.JW_RL_NKDiscPort = dischargePort;

					string label = " (" + loadPort + " -> " + dischargePort + ")";

					Transport.JW_IsLinked = true;

					if (ImportExportHelper.IsImport(loadPort, dischargePort))
					{
						AssertMandatory(label, Transport.JW_ETAInfo, Transport.JW_ETDInfo);
					}
					else
					{
						AssertMandatory(label, Transport.JW_ETDInfo, Transport.JW_ETAInfo);
					}

					Transport.JW_IsLinked = false;
					AssertNotMandatory(label, Transport.JW_ETDInfo, Transport.JW_ETAInfo);
				}
			}
		}

		public void TestJW_ETDNotMandatoryOnTemplateRecords()
		{
			var dummy = Factory.New<DummyTemplateRecordProviderWithTransports>();
			var transport = dummy.Transports.AddNew();
			var label = $"({HomePort} -> {OverseasPort})";
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = OverseasPort;
			transport.JW_IsLinked = true;

			CombineAssertions("Pre-Condition: SupportETD and IsExport and Mandatory because template null", () =>
			{
				Assert(ImportExportHelper.IsExport(transport.JW_RL_NKLoadPort, transport.JW_RL_NKDiscPort));
				AssertMandatory(label, transport.JW_ETDInfo, transport.JW_ETAInfo);
			});

			dummy.TemplateRecord = Factory.New<DummyTemplateRecord>();

			AssertNotMandatory(label, transport.JW_ETDInfo, transport.JW_ETAInfo);

			var parent = Factory.New<DummyTransportParent>();
			transport = parent.Transports.AddNew();
			label = $"({HomePort} -> {OverseasPort})";
			transport.JW_RL_NKLoadPort = HomePort;
			transport.JW_RL_NKDiscPort = OverseasPort;
			transport.JW_IsLinked = true;
			CombineAssertions("Pre-Condition: not SupportETD and IsExport and Mandatory because template null", () =>
			{
				Assert("When not Support ETD", ImportExportHelper.IsExport(transport.JW_RL_NKLoadPort, transport.JW_RL_NKDiscPort));
				AssertNotMandatory(label, transport.JW_ETDInfo, transport.JW_ETAInfo);
			});
		}

		public void TestJW_ETANotMandatoryOnTemplateRecords()
		{
			var dummy = Factory.New<DummyTemplateRecordProviderWithTransports>();
			var transport = dummy.Transports.AddNew();
			var label = $"({OverseasPort} -> {HomePort})";
			transport.JW_RL_NKLoadPort = OverseasPort;
			transport.JW_RL_NKDiscPort = HomePort;
			transport.JW_IsLinked = true;

			CombineAssertions("Pre-Condition: IsImport and Mandatory because template null", () =>
			{
				Assert(ImportExportHelper.IsImport(transport.JW_RL_NKLoadPort, transport.JW_RL_NKDiscPort));
				AssertMandatory(label, transport.JW_ETAInfo, transport.JW_ETDInfo);
			});

			dummy.TemplateRecord = Factory.New<DummyTemplateRecord>();

			AssertNotMandatory(label, transport.JW_ETAInfo, transport.JW_ETDInfo);
		}

		public void TestErrorOnProxiedSailingFieldsWhenNotLinked()
		{
			const string error = "This value is only saved for linked transports.";
			int count = 1;
			ZPropertyInfo[] fields = GetProxiedSailingFields(Transport);

			foreach (ZPropertyInfo info in fields)
			{
				Transport.JW_IsLinked = true;

				Populate(info, count++);
				AssertNoError(info, error);

				Transport.JW_IsLinked = false;
				Populate(info, count++);
				((IBusinessObjectInternals)info.BizObj).Validate(info);
				AssertHasError(info, error);

				info.Value = GetEmptyValue(info.PropertyType);
				AssertNoError(info, error);
			}
		}

		public void TestDatesValidatedEvenIfLinked()
		{
			AssertDateTimeFieldsHaveErrorsForInvalidDates(Transport);

			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();
			JobSailing sailing = Factory.New<JobSailing>();
			Transport.JW_IsLinked = true;
			Transport.JW_JX = sailing.PK;
			AssertDateTimeFieldsHaveErrorsForInvalidDates(Transport);

			sailing.JX_JA = origin.PK;
			sailing.JX_JB = destination.PK;
			AssertDateTimeFieldsHaveErrorsForInvalidDates(Transport);
		}

		public void TestValidateJW_TransportType()
		{
			Transport.JW_TransportMode = Constants.TransportModes.Sea;
			Transport.JW_TransportType = "";
			AssertHasErrors(Transport.JW_TransportTypeInfo);

			Transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			AssertNoNotifications(Transport.JW_TransportTypeInfo);

			Transport.JW_TransportType = Constants.TransportPlanningType.Flight1;
			AssertHasErrors(Transport.JW_TransportTypeInfo);

			Transport.JW_TransportMode = Constants.TransportModes.Air;
			Transport.JW_TransportType = Constants.TransportPlanningType.Flight1;
			Transport.Validation.ValidateJW_TransportType();
			AssertNoNotifications(Transport.JW_TransportTypeInfo);

			Transport.JW_TransportType = "";
			AssertHasErrors(Transport.JW_TransportTypeInfo);
		}

		public void TestValidateJW_RL_NKLoadPort_MustBeUnique()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			Transport transport1 = shipment.Transports.AddNew();
			Transport transport2 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			AssertHasError(transport2.JW_RL_NKLoadPortInfo, "You can't have more than one leg leaving the same port.");
		}

		public void TestPortsValidations()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			Transport transport1 = shipment.Transports.AddNew();
			Transport transport2 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKLoadPort = "AUSYD";

			shipment.RunPreSaveValidation();
			AssertHasError(transport1.JW_RL_NKLoadPortInfo, "You can't have more than one leg leaving the same port.");
			AssertHasError(transport2.JW_RL_NKLoadPortInfo, "You can't have more than one leg leaving the same port.");

			var location11 = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var location12 = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var location21 = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var location22 = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var location31 = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var location32 = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_OA_DepartureLocation = location11;
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_OA_ArrivalLocation = location12;
			transport1.JW_TransportMode = "ROA";

			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_TransportMode = "AIR";

			shipment.RunPreSaveValidation();
			AssertNoErrors(transport1.JW_RL_NKLoadPortInfo);
			AssertNoErrors(transport1.JW_RL_NKDiscPortInfo);
			AssertNoErrors(transport2.JW_RL_NKLoadPortInfo);
			AssertNoErrors(transport2.JW_RL_NKDiscPortInfo);

			transport1.JW_TransportMode = "SEA";
			transport1.JW_OA_ArrivalLocation = location11;

			shipment.RunPreSaveValidation();
			AssertHasError(transport1.JW_RL_NKLoadPortInfo, "The Load and Discharge cannot be the same.");
			AssertHasError(transport1.JW_RL_NKDiscPortInfo, "The Load and Discharge cannot be the same.");

			transport1.JW_TransportMode = "RAI";

			shipment.RunPreSaveValidation();
			AssertNoErrors(transport1.JW_RL_NKLoadPortInfo);
			AssertNoErrors(transport1.JW_RL_NKDiscPortInfo);
			AssertNoErrors(transport2.JW_RL_NKLoadPortInfo);
			AssertNoErrors(transport2.JW_RL_NKDiscPortInfo);

			Transport transport3 = shipment.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "USLAX";
			transport3.JW_OA_DepartureLocation = location31;
			transport3.JW_RL_NKDiscPort = "USLAX";
			transport3.JW_OA_ArrivalLocation = location32;
			transport3.JW_TransportMode = "AIR";

			shipment.RunPreSaveValidation();
			AssertHasError(transport3.JW_RL_NKDiscPortInfo, "The Load and Discharge cannot be the same.");

			transport3.JW_TransportMode = "ROA";

			shipment.RunPreSaveValidation();
			AssertNoErrors(transport2.JW_RL_NKDiscPortInfo);
			AssertNoErrors(transport3.JW_RL_NKDiscPortInfo);
		}

		public void TestValidateJW_RL_NKDiscPort_MustBeUnique()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			Transport transport1 = shipment.Transports.AddNew();
			Transport transport2 = shipment.Transports.AddNew();
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "AUSYD";
			AssertHasError(transport2.JW_RL_NKDiscPortInfo, "You can't have more than one leg arriving at the same port.");
		}

		public void TestValidatePorts_ProhibitedRouting()
		{
			var expectedWarning = "The Australian Government has imposed prohibitions on air cargo that has originated from, or transited through, Turkey. However, this prohibition applies only to electromechanical devices that weigh over 1 kilogram. You are required to meet with government requirements and/or consider a change of transport mode.";

			var consol = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();
			var transport3 = consol.Transports.AddNew();

			transport1.JW_TransportMode = "AIR";
			transport2.JW_TransportMode = "AIR";
			transport3.JW_TransportMode = "AIR";

			Action<string, string, string, string> setupRouting = (origin, transit1, transit2, destination) =>
			{
				transport1.JW_RL_NKLoadPort = origin;
				transport1.JW_RL_NKDiscPort = transit1;
				transport2.JW_RL_NKLoadPort = transit1;
				transport2.JW_RL_NKDiscPort = transit2;
				transport3.JW_RL_NKLoadPort = transit2;
				transport3.JW_RL_NKDiscPort = destination;
			};

			setupRouting("TRADA", "CHGVA", "SGSIN", "AUBNE");
			AssertHasWarning(transport1.JW_RL_NKLoadPortInfo, expectedWarning);
			AssertNoWarning(transport2.JW_RL_NKLoadPortInfo, expectedWarning);
			AssertNoWarning(transport3.JW_RL_NKLoadPortInfo, expectedWarning);

			transport1.JW_TransportMode = "ROA";
			AssertNoWarning("No warning as not air", transport1.JW_RL_NKLoadPortInfo, expectedWarning);
			AssertNoWarning(transport2.JW_RL_NKLoadPortInfo, expectedWarning);
			AssertNoWarning(transport3.JW_RL_NKLoadPortInfo, expectedWarning);

			transport1.JW_TransportMode = "AIR";
			setupRouting("TRADA", "CHGVA", "AUBNE", "NZAKL");
			AssertHasWarning("Has warning as transits through Australia", transport1.JW_RL_NKLoadPortInfo, expectedWarning);
			AssertNoWarning(transport2.JW_RL_NKLoadPortInfo, expectedWarning);
			AssertNoWarning(transport3.JW_RL_NKLoadPortInfo, expectedWarning);

			setupRouting("TRADA", "CHGVA", "SGSIN", "MYKUL");
			AssertNoWarning("No warning as no AU discharge", transport1.JW_RL_NKLoadPortInfo, expectedWarning);
			AssertNoWarning(transport2.JW_RL_NKLoadPortInfo, expectedWarning);
			AssertNoWarning(transport3.JW_RL_NKLoadPortInfo, expectedWarning);

			setupRouting("NZAKL", "AUBNE", "TRADA", "CHGVA");
			AssertNoWarning("No warning as AU discharge is before prohibited load country", transport1.JW_RL_NKLoadPortInfo, expectedWarning);
			AssertNoWarning(transport2.JW_RL_NKLoadPortInfo, expectedWarning);
			AssertNoWarning(transport3.JW_RL_NKLoadPortInfo, expectedWarning);

			setupRouting("CHGVA", "TRADA", "AUSYD", "NZAKL");
			AssertNoWarning(transport1.JW_RL_NKLoadPortInfo, expectedWarning);
			AssertHasWarning("Has warning as transits through prohibited country before Australia", transport2.JW_RL_NKLoadPortInfo, expectedWarning);
			AssertNoWarning(transport3.JW_RL_NKLoadPortInfo, expectedWarning);
		}

		public void TestValidatePorts_WarningForSameLoadPortWhenModeIsROAOrRAI()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			Transport transport1 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_TransportMode = "ROA";

			shipment.RunPreSaveValidation();
			AssertHasWarning(transport1.JW_RL_NKLoadPortInfo, "If this leg is both loading and discharging in the same location, use the Departure From and Arrival At fields to record the actual locations.");

			transport1.JW_TransportMode = "RAI";
			shipment.RunPreSaveValidation();
			AssertHasWarning(transport1.JW_RL_NKLoadPortInfo, "If this leg is both loading and discharging in the same location, use the Departure From and Arrival At fields to record the actual locations.");
		}

		public void TestValidatePorts_ErrorForSameLoadPortFor2LegsWhenTheyAreAllROAOrRAI()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			Transport transport1 = shipment.Transports.AddNew();
			Transport transport2 = shipment.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_TransportMode = "ROA";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_TransportMode = "ROA";

			shipment.RunPreSaveValidation();
			AssertHasWarning(transport1.JW_RL_NKLoadPortInfo, "If this leg is both loading and discharging in the same location, use the Departure From and Arrival At fields to record the actual locations.");
			AssertHasWarning(transport2.JW_RL_NKLoadPortInfo, "If this leg is both loading and discharging in the same location, use the Departure From and Arrival At fields to record the actual locations.");

			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_TransportMode = "RAI";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_TransportMode = "RAI";

			shipment.RunPreSaveValidation();
			AssertHasWarning(transport1.JW_RL_NKLoadPortInfo, "If this leg is both loading and discharging in the same location, use the Departure From and Arrival At fields to record the actual locations.");
			AssertHasWarning(transport2.JW_RL_NKLoadPortInfo, "If this leg is both loading and discharging in the same location, use the Departure From and Arrival At fields to record the actual locations.");

			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_TransportMode = "RAI";
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "AUSYD";
			transport2.JW_TransportMode = "ROA";

			shipment.RunPreSaveValidation();
			AssertHasWarning(transport1.JW_RL_NKLoadPortInfo, "If this leg is both loading and discharging in the same location, use the Departure From and Arrival At fields to record the actual locations.");
			AssertHasWarning(transport2.JW_RL_NKLoadPortInfo, "If this leg is both loading and discharging in the same location, use the Departure From and Arrival At fields to record the actual locations.");
		}

		public void TestValidateIsDomestic()
		{
			Transport.JW_IsLinked = true;
			Transport.JW_RL_NKLoadPort = OverseasPort;
			Transport.JW_RL_NKDiscPort = "";
			Transport.IsDomestic = false;
			AssertEquals(false, Transport.IsDomesticInfo.HasErrors());
			Transport.IsDomestic = true;
			AssertEquals(false, Transport.IsDomesticInfo.HasErrors());
			Transport.JW_RL_NKDiscPort = "AAAAA";
			Transport.IsDomestic = false;
			AssertEquals(false, Transport.IsDomesticInfo.HasErrors());
			Transport.IsDomestic = true;
			AssertEquals(false, Transport.IsDomesticInfo.HasErrors());

			Transport.JW_RL_NKLoadPort = "";
			Transport.JW_RL_NKDiscPort = OverseasPort;
			Transport.IsDomestic = false;
			AssertEquals(false, Transport.IsDomesticInfo.HasErrors());
			Transport.IsDomestic = true;
			AssertEquals(false, Transport.IsDomesticInfo.HasErrors());
			Transport.JW_RL_NKLoadPort = "AAAAA";
			Transport.IsDomestic = false;
			AssertEquals(false, Transport.IsDomesticInfo.HasErrors());
			Transport.IsDomestic = true;
			AssertEquals(false, Transport.IsDomesticInfo.HasErrors());

			Transport.JW_RL_NKLoadPort = HomePort;
			Transport.JW_RL_NKDiscPort = OverseasPort;
			Transport.IsDomestic = false;
			AssertEquals(false, Transport.IsDomesticInfo.HasErrors());
			Transport.IsDomestic = true;
			AssertHasError(Transport.IsDomesticInfo, "You have marked this transport as domestic but the load and discharge ports are not in the same country/region.");

			Transport.JW_RL_NKLoadPort = HomePort;
			Transport.JW_RL_NKDiscPort = AlternateHomePort;
			Transport.IsDomestic = true;
			AssertEquals(false, Transport.IsDomesticInfo.HasErrors());
			Transport.IsDomestic = false;
			AssertHasError(Transport.IsDomesticInfo, "You have marked this transport as international but the load and discharge ports are for a domestic movement.");
		}

		public void TestJW_ETAHasWarningWhenNotEntered()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports.AddNew();
			transport.JW_IsLinked = false;
			AssertHasWarning(transport.JW_ETAInfo, "You have not entered an " + transport.JW_ETAInfo.Description + ".");
		}

		public void TestJW_ETDHasWarningWhenNotEntered()
		{
			var factory = Factory;
			CombineAssertions("when Support ETD", () =>
			{
				CommonConsol consol = factory.New<CommonConsol>();
				Transport transport = consol.Transports.AddNew();
				transport.JW_IsLinked = false;
				AssertHasWarning(transport.JW_ETDInfo, "You have not entered an " + transport.JW_ETDInfo.Description + ".");
			});

			CombineAssertions("when not Support ETD", () =>
			{
				var dummyTransportParent = factory.New<DummyTransportParent>();
				Transport transport = dummyTransportParent.Transports.AddNew();
				transport.JW_IsLinked = false;
				AssertNoWarning(transport.JW_ETDInfo, "You have not entered an " + transport.JW_ETDInfo.Description + ".");
			});
		}

		public void TestJW_VesselValidationIsFiredWhenTransportLinked()
		{
			AssertNoErrors("Precondition: No errors on JW_Vessel property.", Transport.JW_VesselInfo);

			Transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			Transport.JW_Vessel = "InvalidVessel";
			Transport.JW_IsLinked = false;

			Transport.Validation.ValidateAll();
			Assert(!Transport.JW_IsLinked);
			AssertNoErrors("No errors: transport is NOT linked.", Transport.JW_VesselInfo);

			Transport.JW_IsLinked = true;

			Transport.Validation.ValidateAll();
			Assert(Transport.JW_IsLinked);
			AssertHasErrors("Error: transport is linked but vessel is NOT a valid vessel.", Transport.JW_VesselInfo);
		}

		public void TestValidateJW_TerminalAvailabilityDateIsTriggeredWhenJW_ETAAndJW_ATAIsUpdated()
		{
			var vessel = RefVessel.LookupVesselByName("CONDOR", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "01245678";
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			voyage.GenerateSailings();

			CommonConsol consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports.AddNew();
			transport.JW_IsLinked = true;
			transport.JW_JX = voyage.Sailings[0].PK;

			transport.JW_ETA = new ZDateTime(2017, 1, 4);
			transport.JW_TerminalAvailabilityDate = new ZDateTime(2017, 1, 3);
			AssertHasErrors("Precondition: Availability date set to being before JW_ETA causes an error", transport.JW_TerminalAvailabilityDateInfo);

			transport.JW_ATA = new ZDateTime(2017, 1, 2);
			AssertNoErrors("Availability date should now be valid when an ATA date before Availability date exists", transport.JW_TerminalAvailabilityDateInfo);

			transport.JW_ATA = ZDateTime.Empty;
			AssertHasErrors("Removing ATA date should notify Availability date to be invalid again", transport.JW_TerminalAvailabilityDateInfo);

			transport.JW_ETA = new ZDateTime(2017, 1, 2);
			AssertNoErrors("Availability date should now be valid when the ETA date is changed to be before Availability", transport.JW_TerminalAvailabilityDateInfo);
		}

		public void TestDuplicatePortsAllowedOnLegWhenAddressesSpecified()
		{
			var consol = Factory.New<CommonConsol>();

			var transport1 = consol.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "USLAX";
			transport1.JW_RL_NKDiscPort = "AUSYD";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "AUSYD";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "AUSYD";
			transport3.JW_RL_NKDiscPort = "SGSIN";

			var duplicateLoadPortMessage = "You can't have more than one leg leaving the same port. Please specify arrival and departure locations if you need this port setup.";
			var duplicateDiscPortMessage = "You can't have more than one leg arriving at the same port. Please specify arrival and departure locations if you need this port setup.";
			var duplicatePortsOnLegMessage = "The Load and Discharge cannot be the same.";

			transport1.Validation.ValidateAll();
			AssertNoErrors(transport1.JW_RL_NKLoadPortInfo);
			AssertNoErrors(transport1.JW_RL_NKDiscPortInfo);

			transport2.Validation.ValidateAll();
			AssertHasError(transport2.JW_RL_NKLoadPortInfo, duplicateLoadPortMessage);
			AssertHasError(transport2.JW_RL_NKLoadPortInfo, duplicatePortsOnLegMessage);
			AssertHasError(transport2.JW_RL_NKDiscPortInfo, duplicateDiscPortMessage);
			AssertHasError(transport2.JW_RL_NKDiscPortInfo, duplicatePortsOnLegMessage);

			transport3.Validation.ValidateAll();
			AssertNoErrors(transport3.JW_RL_NKLoadPortInfo);
			AssertNoErrors(transport3.JW_RL_NKDiscPortInfo);

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var departureLocation = organisation.Addresses.AddNew();
			var arrivalLocation = organisation.Addresses.AddNew();

			transport2.JW_OA_DepartureLocation = departureLocation.PK;
			transport2.Validation.ValidateJW_RL_NKLoadPort();
			AssertNoError(transport2.JW_RL_NKLoadPortInfo, duplicateLoadPortMessage);
			AssertHasError(transport2.JW_RL_NKLoadPortInfo, duplicatePortsOnLegMessage);
			AssertHasError(transport2.JW_RL_NKDiscPortInfo, duplicateDiscPortMessage);
			AssertHasError(transport2.JW_RL_NKDiscPortInfo, duplicatePortsOnLegMessage);

			transport2.JW_OA_ArrivalLocation = arrivalLocation.PK;
			transport2.Validation.ValidateJW_RL_NKDiscPort();
			AssertNoError(transport2.JW_RL_NKLoadPortInfo, duplicateLoadPortMessage);
			AssertNoError(transport2.JW_RL_NKLoadPortInfo, duplicatePortsOnLegMessage);
			AssertNoError(transport2.JW_RL_NKDiscPortInfo, duplicateDiscPortMessage);
			AssertNoError(transport2.JW_RL_NKDiscPortInfo, duplicatePortsOnLegMessage);
		}

		public void TestCheckTotalCO2eForSorting()
		{
			var transport = GetNewTransport();
			transport.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			transport.Validation.ValidateTotalCO2eForSorting();
			AssertHasWarning(transport.TotalCO2eForSortingInfo, CO2eTestHelper.CO2eStaleWarning);

			transport.SetCO2eStatus(CO2eStatusList.Codes.Rejected);
			transport.Validation.ValidateTotalCO2eForSorting();
			AssertHasWarning(transport.TotalCO2eForSortingInfo, "The greenhouse gas emissions value could not be calculated.");

			var loadPort = Factory.New<RefUNLOCO>();
			var disPort = Factory.New<RefUNLOCO>();
			loadPort.RL_Code = "AU";
			disPort.RL_Code = "IR";

			transport.SetCO2ePerTonneInKg(0);
			transport.JW_RL_NKLoadPort = loadPort.RL_Code;
			transport.JW_RL_NKDiscPort = disPort.RL_Code;
			transport.SetCO2eStatus(CO2eStatusList.Codes.Current);
			transport.Validation.ValidateTotalCO2eForSorting();

			AssertHasWarning(transport.TotalCO2eForSortingInfo, "The greenhouse gas emissions for the input data on routing leg cannot be calculated.");
		}

		#region Check Actual Dates are not Future Dates

		[TestDate(2013, 05, 27)]
		[TestUtcOffset(10, 0, 0)]
		public void TestActualDatesAreNotFutureDates()
		{
			var atdError = "The Actual Time of Departure cannot be set in the future. The date 30-May-13 00:00 is in the future for AUSYD (UTC+10).";
			var ataError = "The Actual Time of Arrival cannot be set in the future. The date 01-Jun-13 00:00 is in the future for GBLON (UTC+0).";

			var consol = Factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = ZString.Empty;
			transport.JW_RL_NKDiscPort = "ZZZZZ";
			transport.JW_IsLinked = true;

			transport.JW_JX_Load_ATA = ZDateTime.Today.AddDays(2);
			transport.JW_ATD = ZDateTime.Today.AddDays(3);
			transport.JW_ATA = ZDateTime.Today.AddDays(5);

			AssertNoErrors("No error expected as future date validation only occurs on JW_ATD if load port has a valid time zone info.", transport.JW_ATDInfo);
			AssertNoErrors("No error expected as future date validation only occurs on JW_ATA if disc port has a valid time zone info.", transport.JW_ATAInfo);

			TestUtcOffsetAttribute.Time = new TimeSpan(10, 0, 0);
			transport.JW_RL_NKLoadPort = "AUSYD";

			AssertHasError("Error expected on ATD as load port is valid and the date is in the future", transport.JW_ATDInfo, atdError);

			TestUtcOffsetAttribute.Time = new TimeSpan(0, 0, 0);
			transport.JW_RL_NKDiscPort = "GBLON";

			AssertHasError("Error expected on ATA as load port is valid and the date is in the future", transport.JW_ATAInfo, ataError);

			transport.JW_RL_NKDiscPort = "XXXXX";

			AssertNoErrors("No error expected on ATA, as the discharge port UNLOCO does not have time zone info", transport.JW_ATAInfo);

			var portWithoutTimeZone = Factory.New<RefUNLOCO>();
			portWithoutTimeZone.RL_Code = "PANDA";
			portWithoutTimeZone.RL_PortName = "New New Port";
			portWithoutTimeZone.Factory.Save();

			transport.JW_RL_NKDiscPort = "PANDA";

			AssertNoErrors("No error expected on ATA, as the newly added UNLOCO port does not have time zone info", transport.JW_ATAInfo);

			TestUtcOffsetAttribute.Time = new TimeSpan(10, 0, 0);
			transport.JW_IsLinked = false;
			transport.Validation.ValidateJW_ATD();

			AssertHasError("Error expected on ATD, regardless of being linked or unlinked", transport.JW_ATDInfo, atdError);
		}

		#endregion

		#region ReceivalCommences and CutOff

		public void TestValidateJW_EmptyReceivalCommences_And_JW_EmptyCutOff()
		{
			var message = "Empty Receival Start date must be before the Empty Cut Off date.";

			Transport.JW_EmptyReceivalCommences = ZDateTime.Today;
			Transport.Validation.ValidateJW_EmptyCutOff();
			AssertNoErrors(Transport.JW_EmptyReceivalCommencesInfo);
			AssertNoErrors(Transport.JW_EmptyCutOffInfo);

			Transport.JW_EmptyCutOff = ZDateTime.Today.AddDays(-1);
			Transport.Validation.ValidateJW_EmptyReceivalCommences();
			AssertHasError(Transport.JW_EmptyReceivalCommencesInfo, message);
			AssertHasError(Transport.JW_EmptyCutOffInfo, message);

			Transport.JW_EmptyCutOff = ZDateTime.Today.AddDays(1);
			Transport.Validation.ValidateJW_EmptyReceivalCommences();
			AssertNoErrors(Transport.JW_EmptyReceivalCommencesInfo);
			AssertNoErrors(Transport.JW_EmptyCutOffInfo);
		}

		public void TestValidateJW_ReeferReceivalCommences_And_JW_ReeferCutOff()
		{
			var message = "Reefer Receival Start date must be before the Reefer Cut Off date.";

			Transport.JW_ReeferReceivalCommences = ZDateTime.Today;
			Transport.Validation.ValidateJW_ReeferCutOff();
			AssertNoErrors(Transport.JW_ReeferReceivalCommencesInfo);
			AssertNoErrors(Transport.JW_ReeferCutOffInfo);

			Transport.JW_ReeferCutOff = ZDateTime.Today.AddDays(-1);
			Transport.Validation.ValidateJW_ReeferReceivalCommences();
			AssertHasError(Transport.JW_ReeferReceivalCommencesInfo, message);
			AssertHasError(Transport.JW_ReeferCutOffInfo, message);

			Transport.JW_ReeferCutOff = ZDateTime.Today.AddDays(1);
			Transport.Validation.ValidateJW_ReeferReceivalCommences();
			AssertNoErrors(Transport.JW_ReeferReceivalCommencesInfo);
			AssertNoErrors(Transport.JW_ReeferCutOffInfo);
		}

		public void TestValidateJW_DGReceivalCommences_And_JW_DGCutOff()
		{
			var message = "HAZ Receival Start date must be before the HAZ Cut Off date.";

			Transport.JW_DGReceivalCommences = ZDateTime.Today;
			Transport.Validation.ValidateJW_DGCutOff();
			AssertNoErrors(Transport.JW_DGReceivalCommencesInfo);
			AssertNoErrors(Transport.JW_DGCutOffInfo);

			Transport.JW_DGCutOff = ZDateTime.Today.AddDays(-1);
			Transport.Validation.ValidateJW_DGReceivalCommences();
			AssertHasError(Transport.JW_DGReceivalCommencesInfo, message);
			AssertHasError(Transport.JW_DGCutOffInfo, message);

			Transport.JW_DGCutOff = ZDateTime.Today.AddDays(1);
			Transport.Validation.ValidateJW_DGReceivalCommences();
			AssertNoErrors(Transport.JW_DGReceivalCommencesInfo);
			AssertNoErrors(Transport.JW_DGCutOffInfo);
		}

		#endregion

		#region Implementation

		void AssertDateTimeFieldsHaveErrorsForInvalidDates(Transport transport)
		{
			ZPropertyInfo[] fields = GetProxiedSailingFields(transport);
			foreach (ZPropertyInfo info in fields)
			{
				if (info is ZPropertyInfoDateTime)
				{
					info.Value = ZDateTime.Invalid;
					AssertHasErrors(info);
				}
			}
		}

		ZPropertyInfo[] GetProxiedSailingFields(Transport transport)
		{
			return new ZPropertyInfo[]
			{
				transport.JW_JX_IsPublishedInfo,
				transport.JW_JX_Load_ATAInfo,
				transport.JW_JX_Load_ETAInfo
			};
		}

		void AssertMandatory(ZString label, ZPropertyInfo mandatoryDate, ZPropertyInfo alternateDate)
		{
			mandatoryDate.Value = ZDateTime.Empty;
			alternateDate.Value = ZDateTime.Empty;

			((IBusinessObjectInternals)mandatoryDate.BizObj).Validate(mandatoryDate);
			((IBusinessObjectInternals)alternateDate.BizObj).Validate(alternateDate);

			AssertHasErrors(mandatoryDate.Name + " should be manditory" + label, mandatoryDate);
			AssertNoErrors(alternateDate.Name + " should not be manditory" + label, alternateDate);

			mandatoryDate.Value = ZDateTime.Today;
			AssertNoErrors(mandatoryDate.Name + " now has a value" + label, mandatoryDate);
		}

		void AssertNotMandatory(ZString label, ZPropertyInfo date1, ZPropertyInfo date2)
		{
			date1.Value = ZDateTime.Empty;
			date2.Value = ZDateTime.Empty;

			((IBusinessObjectInternals)date1.BizObj).Validate(date1);
			((IBusinessObjectInternals)date2.BizObj).Validate(date2);

			AssertNoErrors(date1.Name + " should not be manditory" + label, date1);
			AssertNoErrors(date2.Name + " should not be manditory" + label, date2);
		}

		void Populate(ZPropertyInfo info, int seed)
		{
			info.Value = GetValueForPopulation(info, seed);
		}

		IZType GetValueForPopulation(ZPropertyInfo info, int seed)
		{
			if (info.PropertyType == typeof(ZString))
			{
				return (ZString)seed.ToString().PadLeft(info.MaxLength >> 1, '<').PadRight(info.MaxLength, '>');
			}
			else if (info.PropertyType == typeof(ZDateTime))
			{
				return ZDateTime.Today.AddDays(seed);
			}
			else if (info.PropertyType == typeof(ZBool))
			{
				return ZBool.True;
			}
			else
			{
				throw new ApplicationException("dont know how to populate " + info.PropertyType.Name);
			}
		}

		IZType GetEmptyValue(Type zType)
		{
			if (!typeof(IZType).IsAssignableFrom(zType))
			{
				throw new ApplicationException(zType.Name + " does not implement IZType");
			}

			if (!zType.IsValueType)
			{
				throw new ApplicationException(zType.Name + " is not a value type");
			}

			return (IZType)Activator.CreateInstance(zType, Array.Empty<object>());
		}

		Transport Transport
		{
			get
			{
				if (transport == null)
				{
					transport = GetNewTransport();
				}
				return transport;
			}
		}
		Transport transport;

		protected virtual Transport GetNewTransport()
		{
			return Factory.New<CommonShipment>().Transports.AddNew();
		}

		class DummyTemplateRecordProviderWithTransports : DummyBusinessObject, ITemplateRecordProvider, ITransportParentCommon
		{
			public DummyTemplateRecordProviderWithTransports(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString TypeCode => "ZZZ";

			public TransportCollection Transports
			{
				get
				{
					if (transports == null)
					{
						transports = GetNewTransportCollection();
						transports.Load();
					}

					return transports;
				}
			}

			TransportCollection transports;

			protected virtual TransportCollection GetNewTransportCollection()
			{
				return new TransportCollection(this);
			}

			public ITemplateRecord TemplateRecord { get; set; }

			public bool IsTemplateRecord { get; set; } = true;

			void ITemplateRecordProvider.SaveToTemplateRecord()
			{
			}

			void ITemplateRecordProvider.LoadFromTemplateRecord(ITemplateRecord templateRecord)
			{
			}

			BusinessObject ITemplateRecordProvider.InstantiateFromTemplateRecord(BusinessObjectFactory factory, Type elementType, ITemplateRecord templateRecord)
			{
				throw new NotImplementedException();
			}
		}

		class DummyTemplateRecord : DummyBusinessObject, ITemplateRecord
		{
			public DummyTemplateRecord(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		#endregion
	}
}
