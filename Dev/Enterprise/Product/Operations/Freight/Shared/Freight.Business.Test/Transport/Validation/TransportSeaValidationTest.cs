using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportSeaValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUsingCorrectValidation()
		{
			AssertEquals(typeof(TransportSeaValidation), TransportValidation.New(Transport).GetType());
			AssertEquals(typeof(TransportSeaValidation), Transport.Validation.GetType());
		}

		public void TestValidateEstimatedDepartureAndArrival()
		{
			GenericValidateDateRangeForEstimatedDate(Transport.JW_ETDInfo, Transport.JW_ETAInfo);
		}

		void GenericValidateDateRangeForEstimatedDate(ZPropertyInfo departureInfo, ZPropertyInfo arrivalInfo)
		{
			var date = ZDateTime.Now;

			departureInfo.Value = date;
			arrivalInfo.Value = ZDateTime.Empty;
			AssertNoNotifications("Arrival Date empty. No error expected on Departure", departureInfo);
			AssertHasWarning(arrivalInfo, "You have not entered an " + arrivalInfo.Description + ".");

			arrivalInfo.Value = new ZDateTime(date.AddDays(-1));
			ValidateInfo(departureInfo);
			AssertHasErrors("Error expected on Departure. Departure > Arrival.", departureInfo);
			AssertHasErrors("Error expected on Arrival. Departure > Arrival.", arrivalInfo);

			arrivalInfo.Value = new ZDateTime(date.AddDays(2));
			ValidateInfo(departureInfo);
			AssertNoNotifications("No error expected on Departure. Departure < Arrival.", departureInfo);
			AssertNoNotifications("No error expected on Arrival. Departure < Arrival.", arrivalInfo);
		}

		public void TestValidateActualDepartureAndArrival()
		{
			GenericValidateDateRangeForActualDate(Transport.JW_ATDInfo, Transport.JW_ATAInfo);
		}

		void GenericValidateDateRangeForActualDate(ZPropertyInfo departureInfo, ZPropertyInfo arrivalInfo)
		{
			ZDateTime date = ZDateTime.Now;

			departureInfo.Value = date;
			arrivalInfo.Value = ZDateTime.Empty;
			AssertNoNotifications("Arrival Date empty. No error expected on Departure", departureInfo);
			AssertNoNotifications("No error expected on Arrival. Departure > Arrival.", arrivalInfo);

			arrivalInfo.Value = new ZDateTime(date.AddHours(-1));
			ValidateInfo(departureInfo);
			AssertHasErrors("Error expected on Departure. Departure > Arrival.", departureInfo);
			AssertHasErrors("Error expected on Arrival. Departure > Arrival.", arrivalInfo);

			arrivalInfo.Value = new ZDateTime(date.AddDays(2));
			ValidateInfo(departureInfo);
			AssertNoNotifications("No error expected on Departure. Departure < Arrival.", departureInfo);
			AssertNoNotifications("No error expected on Arrival. Departure < Arrival.", arrivalInfo);
		}

		public void TestValidateJW_VoyageFlight()
		{
			Transport.JW_IsLinked = true;

			Transport.JW_VoyageFlight = "";
			Transport.Validation.ValidateJW_VoyageFlight();
			AssertHasErrors("Blank Voyage - Error expected", Transport.JW_VoyageFlightInfo);

			Transport.JW_VoyageFlight = "V1234";
			AssertHasWarning("Voyage starts with a single 'V' followed by some digits - Warning expected", Transport.JW_VoyageFlightInfo, "Voyage number should not start with a 'V' followed by numbers. The system will add this 'V' automatically.");

			Transport.JW_VoyageFlight = "VM123";
			AssertNoNotifications("Valid Voyage - No error expected", Transport.JW_VoyageFlightInfo);

			Transport.JW_VoyageFlight = "N183";
			AssertNoErrors("Valid Voyage - No error expected", Transport.JW_VoyageFlightInfo);

			Transport.JW_VoyageFlight = "98765";
			AssertNoNotifications("Valid Voyage - No error expected", Transport.JW_VoyageFlightInfo);
		}

		public void TestValidateJW_IsLinked_HasSufficientInformationButUnableToLinkToSailing()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "SSDMITRY";

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "DM1";

			var firstOrigin = voyage.Origins.AddNew();
			firstOrigin.JA_RL_NKPortOfLoading = "AUSYD";
			firstOrigin.JA_E_DEP = new ZDateTime(2014, 4, 10);

			var secondOrigin = voyage.Origins.AddNew();
			secondOrigin.JA_RL_NKPortOfLoading = "AUMEL";
			secondOrigin.JA_E_DEP = new ZDateTime(2014, 4, 20);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "NZAKL";
			destination.JB_E_ARV = new ZDateTime(2014, 4, 15);
			voyage.GenerateSailings();

			AssertEquals("Prerequisite", 1, voyage.Sailings.Count);

			Factory.Save();

			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = secondOrigin.JA_RL_NKPortOfLoading;
			consol.JK_RL_NKDischargePort = destination.JB_RL_NKPortOfDischarge;

			var transport = consol.MostInterestingTransportForBinding[0];
			transport.JW_IsLinked = false;
			transport.JW_Vessel = "SSDMITRY";
			transport.JW_VoyageFlight = "DM1";

			AssertEquals("Prerequisite", false, transport.JW_IsLinked);
			AssertEquals("Prerequisite", Core.Constants.TransportModes.Sea, transport.JW_TransportMode);
			AssertEquals("Prerequisite", consol.JK_RL_NKLoadPort, transport.JW_RL_NKLoadPort);
			AssertEquals("Prerequisite", consol.JK_RL_NKDischargePort, transport.JW_RL_NKDiscPort);

			const string errorMsg = "Sailings for this Consol could not be linked due to conflicts on the Sailing Schedule for these two ports. Check the Sailing Schedule for this Voyage.";

			AssertNoError(transport.JW_IsLinkedInfo, errorMsg);

			transport.JW_IsLinked = true;

			AssertHasError(transport.JW_IsLinkedInfo, errorMsg);

			transport.JW_IsLinked = false;

			AssertNoError(transport.JW_IsLinkedInfo, errorMsg);

			transport.JW_Vessel = "";
			transport.JW_VoyageFlight = "";
			transport.JW_IsLinked = true;

			AssertNoError(transport.JW_IsLinkedInfo, errorMsg);
		}

		public void TestValidateJWVesselWithoutSailing_ParentLinkedShouldHaveNoNotificationsOnCorrectVessel()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "V123";

			Transport.JW_Vessel = "V123";
			Transport.JW_IsLinked = true;
			transport.Validation.ValidateJW_Vessel();
			AssertNoNotifications(Transport.JW_VesselInfo);
		}

		public void TestValidateJWVesselWithoutSailing_ParentLinkedShouldErrorOnBlankVessel()
		{
			Transport.JW_Vessel = "";
			Transport.JW_IsLinked = true;
			transport.Validation.ValidateJW_Vessel();
			AssertHasError(Transport.JW_VesselInfo, "Please enter a Vessel / Journey Name.");
		}

		public void TestValidateJWVesselWithoutSailing_ParentLinkedShouldErrorOnMissingVessel()
		{
			Transport.JW_Vessel = "M123";
			Transport.JW_IsLinked = true;
			transport.Validation.ValidateJW_Vessel();
			AssertHasError(Transport.JW_VesselInfo, "Enter a valid Vessel / Journey Name.");
		}

		public void TestValidateJWVesselWithoutSailing_ParentNotLinkedRegistryNotSetShouldHaveNoNotificationsOnCorrectVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "V123";

				Transport.JW_Vessel = "V123";
				Transport.JW_IsLinked = false;
				transport.Validation.ValidateJW_Vessel();
				AssertNoNotifications(Transport.JW_VesselInfo);
			}
		}

		public void TestValidateJWVesselWithoutSailing_ParentNotLinkedRegistryNotSetShouldWarnOnMissingVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Transport.JW_Vessel = "M123";
				Transport.JW_IsLinked = false;
				transport.Validation.ValidateJW_Vessel();
				AssertHasWarning(Transport.JW_VesselInfo, "Warning: No reference file for this Vessel was found.");
			}
		}

		public void TestValidateJWVesselWithoutSailing_ParentNotLinkedRegistryNotSetShouldWarnOnInactiveVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "V123";
				vessel.RV_IsActive = false;

				Transport.JW_Vessel = "V123";
				Transport.JW_IsLinked = false;
				transport.Validation.ValidateJW_Vessel();
				AssertHasWarning(Transport.JW_VesselInfo, "Warning: This Vessel is inactive.");
			}
		}

		public void TestValidateJWVesselWithoutSailing_ParentNotLinkedRegistrySetShouldHaveNoNotificationsOnCorrectVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "V123";

				Transport.JW_Vessel = "V123";
				Transport.JW_IsLinked = false;
				transport.Validation.ValidateJW_Vessel();
				AssertNoNotifications(Transport.JW_VesselInfo);
			}
		}

		public void TestValidateJWVesselWithoutSailing_ParentNotLinkedRegistrySetShouldErrorOnMissingVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Transport.JW_Vessel = "M123";
				Transport.JW_IsLinked = false;
				transport.Validation.ValidateJW_Vessel();
				AssertHasError(Transport.JW_VesselInfo, "Please enter a valid Vessel.");
			}
		}

		public void TestValidateJWVesselWithoutSailing_ParentNotLinkedRegistrySetShouldErrorOnInactiveVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "V123";
				vessel.RV_IsActive = false;

				Transport.JW_Vessel = "V123";
				Transport.JW_IsLinked = false;
				transport.Validation.ValidateJW_Vessel();
				AssertHasError(Transport.JW_VesselInfo, "This Vessel is inactive.");
			}
		}

		public void TestCarrierValidationIsFiredWhenTransportLinked()
		{
			Transport.JW_IsLinked = false;
			Transport.CarrierPK = ZGuid.Empty;
			Transport.Validation.ValidateCarrierPK();
			AssertNoErrors(Transport.CarrierPKInfo);

			Transport.JW_IsLinked = true;
			AssertHasErrors(Transport.CarrierPKInfo);
		}

		public void TestCarrierIsMandatoryForLinkedTransports()
		{
			Transport.JW_IsLinked = true;
			Transport.CarrierPK = ZGuid.Empty;
			Transport.Validation.ValidateCarrierPK();
			AssertHasErrors(Transport.CarrierPKInfo);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			Transport.CarrierPK = carrier.PK;
			AssertNoErrors(Transport.CarrierPKInfo);

			Transport.JW_IsLinked = false;
			Transport.CarrierPK = ZGuid.Empty;
			Transport.Validation.ValidateCarrierPK();
			AssertNoErrors(Transport.CarrierPKInfo);
		}

		public void TestCarrierSecurityValidation()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;

			Env.Security.SailingScheduleCreateFromJob.IsAllowed = false;

			Transport.JW_IsLinked = true;
			Transport.JW_Vessel = "VESSEL";
			Transport.JW_VoyageFlight = "V123";
			Transport.JW_RL_NKLoadPort = "AUSYD";
			Transport.JW_RL_NKDiscPort = "NZAKL";

			Transport.CarrierPK = carrier.PK;
			Transport.Validation.ValidateCarrierPK();
			AssertHasErrorContaining(Transport.CarrierPKInfo, "No matching schedule could be found and you do not have the security rights to create one.");

			Env.Security.SailingScheduleCreateFromJob.IsAllowed = true;
			Transport.Validation.ValidateCarrierPK();
			AssertNoErrorContaining(Transport.CarrierPKInfo, "No matching schedule could be found and you do not have the security rights to create one.");
		}

		#region Implementation

		void ValidateInfo(ZPropertyInfo info)
		{
			((IBusinessObjectInternals)info.BizObj).Validate(info);
		}

		Transport Transport
		{
			get
			{
				if (transport == null)
				{
					transport = Factory.New<CommonShipment>().Transports.AddNew();
					transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
				}
				return transport;
			}
		}
		Transport transport;

		#endregion
	}
}
