using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageSeaValidationTest : TestBaseJobVoyageValidation
	{
		#region JV_VoyageFlight

		public void TestValidateJV_VoyageFlight()
		{
			Voyage.JV_VoyageFlight = ZString.Empty;
			AssertHasErrors("Voyage flight empty, error expected", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "100";
			AssertNoNotifications("Voyage number correct, no error expected", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "V12345";
			AssertHasWarning("Voyage starts with a single 'V' - Warning expected", Voyage.JV_VoyageFlightInfo, "Voyage number should not start with a 'V' followed by numbers or punctuation characters. The system will add this 'V' automatically.");

			Voyage.JV_VoyageFlight = "V";
			AssertHasWarning("Voyage starts with a single 'V' - Warning expected", Voyage.JV_VoyageFlightInfo, "Voyage number should not start with a 'V' followed by numbers or punctuation characters. The system will add this 'V' automatically.");

			Voyage.JV_VoyageFlight = "V.105";
			AssertHasWarning("Voyage starts with a single 'V' - Warning expected", Voyage.JV_VoyageFlightInfo, "Voyage number should not start with a 'V' followed by numbers or punctuation characters. The system will add this 'V' automatically.");

			Voyage.JV_VoyageFlight = "V.105";
			AssertHasWarning("Voyage starts with a single 'V' - Warning expected", Voyage.JV_VoyageFlightInfo, "Voyage number should not start with a 'V' followed by numbers or punctuation characters. The system will add this 'V' automatically.");

			Voyage.JV_VoyageFlight = "D2345";
			AssertNoErrors("Voyage number correct, no error expected", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "VV2345";
			AssertNoErrors("Voyage number correct, no error expected", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "VVlkdfg";
			AssertNoErrors("Voyage number correct, no error expected", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "12345";
			AssertNoNotifications("Voyage number correct, no error expected", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_RV_NKVessel = Vessel1.RV_FK;
			Voyage.JV_OH_Line = Carrier.PK;

			Voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "CNSHA";
			Voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";

			var anotherVoyage = Factory.New<JobVoyage>();
			anotherVoyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			anotherVoyage.JV_RV_NKVessel = Voyage.JV_RV_NKVessel;

			anotherVoyage.JV_VoyageFlight = Voyage.JV_VoyageFlight;
			AssertHasWarning(anotherVoyage.JV_VoyageFlightInfo, "Another Sailing Schedule already exists for the given Vessel/Voyage Number combination.");

			anotherVoyage.JV_OH_Line = Voyage.JV_OH_Line;
			anotherVoyage.JV_VoyageFlight = "200";
			anotherVoyage.JV_VoyageFlight = Voyage.JV_VoyageFlight;
			AssertHasError(anotherVoyage.JV_VoyageFlightInfo, "Vessel/Voyage Number/Carrier combination must be unique and cannot be repeated for non-archived sailings.");

			anotherVoyage.JV_VoyageFlight = "200";
			AssertNoErrors(anotherVoyage.JV_VoyageFlightInfo);
		}

		#endregion

		#region JV_OH_Line

		public override void TestValidateJV_OH_Line()
		{
			Voyage.JV_VoyageFlight = "123456";
			Voyage.JV_RV_NKVessel = ZString.Empty;
			Voyage.JV_OH_Line = ZGuid.Empty;

			AssertHasErrors("Carrier is mandatory", Voyage.JV_OH_LineInfo);

			Voyage.JV_OH_Line = Factory.New<OrgHeader>().PK;
			AssertHasErrors("Not a carrier", Voyage.JV_OH_LineInfo);

			Voyage.JV_OH_Line = Carrier.PK;
			AssertNoErrors(Voyage.JV_OH_LineInfo);

			Voyage.JV_RV_NKVessel = Vessel1.RV_FK;
			Voyage.JV_OH_Line = Carrier.PK;

			Voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "CNSHA";
			Voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";

			var anotherVoyage = Factory.New<JobVoyage>();
			anotherVoyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			anotherVoyage.JV_RV_NKVessel = Voyage.JV_RV_NKVessel;
			anotherVoyage.JV_VoyageFlight = Voyage.JV_VoyageFlight;

			anotherVoyage.JV_OH_Line = Voyage.JV_OH_Line;
			AssertHasError(anotherVoyage.JV_OH_LineInfo, "Vessel/Voyage Number/Carrier combination must be unique and cannot be repeated for non-archived sailings.");

			var anotherCarrier = Factory.New<OrgHeader>();
			anotherCarrier.OH_IsShippingLine = true;
			anotherCarrier.OH_IsShippingProvider = true;

			anotherVoyage.JV_OH_Line = anotherCarrier.PK;
			AssertNoErrors(anotherVoyage.JV_VoyageFlightInfo);
		}

		#endregion

		#region JV_RV_NKVessel

		public void TestValidateJV_RV_NKVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Voyage.JV_RV_NKVessel = ZString.Empty;
				Voyage.Validation.ValidateJV_RV_NKVessel();
				AssertHasErrors("Vessel empty, error expected", Voyage.JV_RV_NKVesselInfo);

				Voyage.JV_RV_NKVessel = "alkdjlafladkj";
				AssertHasWarnings("Invalid code, warning expected", Voyage.JV_RV_NKVesselInfo);

				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "LOOSHAAADKAAAAA";

				Voyage.JV_RV_NKVessel = vessel.RV_FK;
				AssertHasWarningContaining(Voyage.JV_RV_NKVesselInfo, "The IMO number for vessel LOOSHAAADKAAAAA is empty. You will not be able to receive automatic schedule updates.");
				AssertNoErrors("Vessel not empty with correct code, no error expected", Voyage.JV_RV_NKVesselInfo);

				vessel.RV_LloydsNumber = "3334444";
				Voyage.Validation.ValidateJV_RV_NKVessel();
				AssertHasWarningContaining(Voyage.JV_RV_NKVesselInfo, "The IMO number 3334444 for vessel LOOSHAAADKAAAAA is not compliant with IMO requirements. You will not be able to receive automatic schedule updates.");
				AssertNoErrors("Vessel not empty with correct code, no error expected", Voyage.JV_RV_NKVesselInfo);

				vessel.RV_LloydsNumber = "9308390";
				Voyage.Validation.ValidateJV_RV_NKVessel();
				AssertNoWarnings(Voyage.JV_RV_NKVesselInfo);
				AssertNoErrors("Vessel not empty with correct code, no error expected", Voyage.JV_RV_NKVesselInfo);

				Voyage.JV_OH_Line = Carrier.PK;

				Voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "CNSHA";
				Voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";

				var anotherVoyage = Factory.New<JobVoyage>();
				anotherVoyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
				anotherVoyage.JV_VoyageFlight = Voyage.JV_VoyageFlight;
				anotherVoyage.JV_OH_Line = Voyage.JV_OH_Line;

				anotherVoyage.JV_RV_NKVessel = Voyage.JV_RV_NKVessel;
				AssertHasError(anotherVoyage.JV_RV_NKVesselInfo, "Vessel/Voyage Number/Carrier combination must be unique and cannot be repeated for non-archived sailings.");

				var anotherVessel = Factory.New<RefVessel>();
				anotherVessel.RV_Name = "EEJIIIIIK";

				anotherVoyage.JV_RV_NKVessel = anotherVessel.RV_FK;
				AssertNoErrors(anotherVoyage.JV_RV_NKVesselInfo);
			}
		}

		public void TestValidateJV_RV_NKVessel_RegistryNotSetShouldWarnOnMissingVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Voyage.JV_RV_NKVessel = "Eorzea";
				Voyage.Validation.ValidateJV_RV_NKVessel();
				AssertHasWarning(Voyage.JV_RV_NKVesselInfo, "Warning: No reference file for this Vessel was found.");
			}
		}

		public void TestValidateJV_RV_NKVessel_RegistryNotSetShouldWarnOnInactiveVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "Eorzea";
				vessel.RV_IsActive = false;

				Voyage.JV_RV_NKVessel = vessel.RV_FK;
				Voyage.Validation.ValidateJV_RV_NKVessel();
				AssertHasWarning(Voyage.JV_RV_NKVesselInfo, "Warning: This Vessel is inactive.");
			}
		}

		public void TestValidateJV_RV_NKVessel_RegistrySetShouldErrorOnMissingVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Voyage.JV_RV_NKVessel = "Eorzea";
				Voyage.Validation.ValidateJV_RV_NKVessel();
				AssertHasError(Voyage.JV_RV_NKVesselInfo, "Please enter a valid Vessel.");
			}
		}

		public void TestValidateJV_RV_NKVessel_RegistrySetShouldErrorOnInactiveVessel()
		{
			using (FreightDataRegistry.Instance.VesselInReferenceFileMandatoryOnShipments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var vessel = Factory.New<RefVessel>();
				vessel.RV_Name = "Eorzea";
				vessel.RV_IsActive = false;

				Voyage.JV_RV_NKVessel = vessel.RV_FK;
				Voyage.Validation.ValidateJV_RV_NKVessel();
				AssertHasError(Voyage.JV_RV_NKVesselInfo, "This Vessel is inactive.");
			}
		}

		#endregion

		#region IsArchived

		public void TestValidateIsArchived()
		{
			Voyage.JV_RV_NKVessel = Vessel1.RV_FK;
			Voyage.JV_OH_Line = Carrier.PK;

			Voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "CNSHA";
			Voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";

			var anotherVoyage = Factory.New<JobVoyage>();
			anotherVoyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			anotherVoyage.JV_RV_NKVessel = Voyage.JV_RV_NKVessel;
			anotherVoyage.JV_VoyageFlight = Voyage.JV_VoyageFlight;
			anotherVoyage.JV_OH_Line = Voyage.JV_OH_Line;

			anotherVoyage.IsArchived = true;
			AssertNoErrors(anotherVoyage.IsArchivedInfo);

			anotherVoyage.IsArchived = false;
			AssertHasError(anotherVoyage.IsArchivedInfo, "Vessel/Voyage Number/Carrier combination must be unique and cannot be repeated for non-archived sailings.");
		}

		#endregion

		#region JV_VoyageType

		public void TestValidateJV_VoyageType()
		{
			Voyage.JV_VoyageType = "BLA";
			AssertHasErrors("invalid voyage type", Voyage.JV_VoyageTypeInfo);

			Voyage.JV_VoyageType = Constants.VoyageType.MainVoyage;
			AssertNoNotifications("valid voyage Type", Voyage.JV_VoyageTypeInfo);

			Voyage.JV_RV_NKVessel = Vessel1.RV_FK;

			Voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "CNSHA";
			Voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "SGSIN";

			var anotherVoyage = Factory.New<JobVoyage>();
			anotherVoyage.JV_AirSeaRoad = Constants.TransportModes.Sea;
			anotherVoyage.JV_VoyageFlight = Voyage.JV_VoyageFlight;
			anotherVoyage.JV_RV_NKVessel = Voyage.JV_RV_NKVessel;

			anotherVoyage.JV_VoyageType = Constants.VoyageType.MainVoyage;
			AssertHasError(anotherVoyage.JV_VoyageTypeInfo, "Main Sailing Schedule already exists for the given Vessel/Voyage Number combination.");

			anotherVoyage.JV_VoyageType = Constants.VoyageType.SlotVoyage;
			AssertNoErrors(anotherVoyage.JV_VoyageTypeInfo);

			anotherVoyage.JV_VoyageFlight = "200";
			anotherVoyage.JV_VoyageType = Constants.VoyageType.MainVoyage;
			AssertNoErrors(anotherVoyage.JV_VoyageTypeInfo);
		}

		#endregion

		#region Implementation

		OrgHeader Carrier
		{
			get
			{
				if (carrier == null)
				{
					carrier = Factory.New<OrgHeader>();
					carrier.OH_IsShippingLine = true;
					carrier.OH_IsShippingProvider = true;
				}

				return carrier;
			}
		}
		OrgHeader carrier;

		protected override BaseJobVoyageValidation GetValidationObject()
		{
			return new VoyageSeaValidation(Voyage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
		}

		#endregion
	}
}
