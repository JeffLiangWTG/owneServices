using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.DataTransfer.Universal.Testing
{
	class TransportLegDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPopulateTransportDataObject()
		{
			var transport = GetTransport();
			SetupTransportLeg(transport);

			var writer = new TransportLegDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, transport)));
			var transportDataObject = writer.GetDataObject(transport);
			CombineAssertions(delegate
			{
				AssertBasicContents(transportDataObject);
				AssertContentsSailing(transportDataObject);
				AssertContentsArrivalSchedule(transportDataObject);
				AssertContentsDepartureSchedule(transportDataObject);
			});
		}

		public void TestPopulateTransportDataObjectWhenTransportIsNotLinked()
		{
			var transportBO = GetTransport();
			SetupTransportLeg(transportBO, false);
			AssertEquals("Precondition", ZGuid.Empty, transportBO.JW_JX);
			var writer = new TransportLegDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, transportBO)));
			var transportDataObject = writer.GetDataObject(transportBO);

			AssertCTODateWhenTransportIsNotLinked(transportDataObject);
			AssertEquals("transportDataObject.IsCargoOnly", true, transportDataObject.IsCargoOnly);
		}

		public void TestGreenhouseGasEmission()
		{
			var transportBO = GetTransport();
			SetupTransportLeg(transportBO, false);
			transportBO.SetCO2ePerTonneInKg(1);
			transportBO.SetCO2eDistanceInKM(2);
			transportBO.SetCO2ePerTEUInKg(3);
			transportBO.SetCO2eStatus(CO2eStatusList.Codes.Current);
			consol.JK_TotalShipmentActWeightCheck = 1000m;

			var writer = new TransportLegDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, transportBO)), consol);
			var transportLeg = writer.GetDataObject(transportBO);

			AssertNotNull("transportLeg.GreenhouseGasEmission", transportLeg.GreenhouseGasEmission);
			AssertEquals(1m, transportLeg.GreenhouseGasEmission.CO2e);
			CombineAssertions("GreenhouseGasEmission.CO2eUnit", () =>
			{
				AssertEquals("Code", "KG", transportLeg.GreenhouseGasEmission.CO2eUnit.Code);
				AssertEquals("Description", "Kilograms", transportLeg.GreenhouseGasEmission.CO2eUnit.Description);
			});
			AssertNull("Should not write into GreenhouseGasEmission.CO2eStatus", transportLeg.GreenhouseGasEmission.CO2eStatus);
			CombineAssertions("GreenhouseGasEmission.CO2eDescriptiveStatus", () =>
			{
				AssertEquals("Code", CO2eStatusList.Codes.Current, transportLeg.GreenhouseGasEmission.CO2eDescriptiveStatus.Code);
				AssertEquals("Description", CO2eHelper.GetCO2eStatusShortDescription(CO2eStatusList.Codes.Current), transportLeg.GreenhouseGasEmission.CO2eDescriptiveStatus.Description);
			});
		}

		public void TestAdditionalTransportModes()
		{
			var transport = GetTransport();
			SetupTransportLeg(transport);
			transport.JW_TransportMode = Core.Constants.TransportModes.Rail;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.OnForwarding;
			transport.JW_AdditionalTransportMode = Core.Constants.TransportModes.Road;

			var writer = new TransportLegDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, transport)));
			var transportDataObject = writer.GetDataObject(transport);
			AssertEquals("transportDataObject.AdditionalTransportModeCollection", 1, transportDataObject.AdditionalTransportModeCollection.Count);
			AssertEquals("transportDataObject.AdditionalTransportModeCollection[0].TransportMode", TransportMode.Road, transportDataObject.AdditionalTransportModeCollection[0].TransportMode);
		}

		CommonConsol consol;

		protected virtual Transport GetTransport()
		{
			consol = Factory.New<IForwardingConsol>() as CommonConsol;
			return consol.Transports.AddNew();
		}

		protected void SetupTransportLeg(Transport transportBO, bool isLinked = true)
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "ANRO ASIA ZZ";
			vessel.RV_LloydsNumber = "9174622";

			transportBO.JW_IsCargoOnly = true;
			transportBO.JW_LegOrder = 1;
			transportBO.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transportBO.JW_TransportType = Core.Constants.TransportPlanningType.MainVessel;
			transportBO.JW_LegNotes = "Test Leg Notes";
			transportBO.JW_Vessel = vessel.RV_FK;
			transportBO.JW_VoyageFlight = "121A";

			transportBO.JW_RL_NKLoadPort = "NZAKL";
			transportBO.JW_ETD = new ZDateTime(2012, 08, 14);
			transportBO.JW_ATD = new ZDateTime(2012, 08, 15);
			transportBO.JW_RL_NKDiscPort = "AUSYD";
			transportBO.JW_ETA = new ZDateTime(2012, 08, 18);
			transportBO.JW_ATA = new ZDateTime(2012, 08, 19);

			transportBO.JW_DepotReceivalCommences = new ZDateTime(2012, 8, 2);
			transportBO.JW_DepotCutOff = new ZDateTime(2012, 8, 1);
			transportBO.JW_DepotAvailabilityDate = new ZDateTime(2012, 8, 23);
			transportBO.JW_DepotStorageDate = new ZDateTime(2012, 8, 24);
			transportBO.JW_TerminalReceivalCommences = new ZDateTime(2012, 8, 6);
			transportBO.JW_TerminalCutOff = new ZDateTime(2012, 8, 5);
			transportBO.JW_TerminalAvailabilityDate = new ZDateTime(2012, 8, 21);
			transportBO.JW_TerminalStorageDate = new ZDateTime(2012, 8, 22);
			transportBO.JW_DocumentaryCutOff = new ZDateTime(2012, 8, 7);
			transportBO.JW_VGMCutOff = new ZDateTime(2012, 8, 10);

			transportBO.JW_CarrierBookingReference = "XX12345";

			var departureFrom = Factory.New<OrgHeader>();
			departureFrom.OH_Code = "DEPFROM";
			departureFrom.MainAddress.OA_Address1 = "32 SOMETHING ST";
			departureFrom.MainAddress.OA_City = "NEW NEW YORK";
			transportBO.JW_OA_DepartureLocation = departureFrom.MainAddress.PK;

			var arrivalAt = Factory.New<OrgHeader>();
			arrivalAt.OH_Code = "ARVAT";
			arrivalAt.MainAddress.OA_Address1 = "92 TEST AVE";
			arrivalAt.MainAddress.OA_City = "ZANZIBAR";
			transportBO.JW_OA_ArrivalLocation = arrivalAt.MainAddress.PK;

			var carrier = transportBO.Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsShippingProvider = true;
			carrier.OH_Code = "CARORG";
			carrier.MainAddress.OA_Address1 = "ADDRESS 1";
			carrier.MainAddress.OA_City = "MASCOT";
			transportBO.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			transportBO.JW_PL_NKCarrierServiceLevel = "STD";

			var creditor = transportBO.Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;
			creditor.OH_Code = "CRDSYD";
			creditor.MainAddress.OA_Address1 = "ADDRESS 2";
			creditor.MainAddress.OA_City = "ALEXANDRIA";
			transportBO.JW_OA_CreditorAddress = creditor.MainAddress.PK;

			transportBO.JW_IsLinked = isLinked;
			transportBO.JW_AircraftType = "E90";

			transportBO.JW_Status = "QUE";

			transportBO.JW_STD = new ZDateTime(2012, 08, 17);
			transportBO.JW_STA = new ZDateTime(2012, 08, 20);

			transportBO.JW_DGReceivalCommences = new ZDateTime(2012, 8, 4);
			transportBO.JW_DGCutOff = new ZDateTime(2012, 8, 3);
			transportBO.JW_EmptyReceivalCommences = new ZDateTime(2012, 8, 25);
			transportBO.JW_EmptyCutOff = new ZDateTime(2012, 8, 26);
			transportBO.JW_ReeferReceivalCommences = new ZDateTime(2012, 8, 27);
			transportBO.JW_ReeferCutOff = new ZDateTime(2012, 8, 28);

			if (transportBO.JW_IsLinked)
			{
				var sailing = transportBO.Sailing;
				AssertNotNull("Precondition:transportBO.Sailing", sailing);

				var depCTO = transportBO.Factory.New<OrgHeader>();
				depCTO.OH_IsSeaCTO = true;
				depCTO.OH_Code = "DEPCTO";

				var departure = sailing.Origin;
				departure.JA_OA_DepartureCTOAddress = depCTO.MainAddress.PK;
				departure.JA_Berth = "DEP_BERTH";
				departure.JA_DepartReference = "DEP_REF";
				departure.JA_E_ARV = new ZDateTime(2012, 8, 9);
				departure.JA_A_ARV = new ZDateTime(2012, 8, 8);
				departure.JA_S_ARV = new ZDate(2012, 8, 10);

				var arrCTO = transportBO.Factory.New<OrgHeader>();
				arrCTO.OH_IsSeaCTO = true;
				arrCTO.OH_Code = "ARRCTO";

				var arrival = sailing.Destination;
				arrival.JB_OA_ArrivalCTOAddress = arrCTO.MainAddress.PK;
				arrival.JB_Berth = "ARR_BERTH";
				arrival.JB_ArrivalReference = "ARR_REF";
			}
		}

		static void AssertBasicContents(TransportLeg transportDataObject)
		{
			AssertEquals("transportDataObject.LegOrder.Value", (ZByte)1, transportDataObject.LegOrder);
			AssertEquals("transportDataObject.IsCargoOnly", true, transportDataObject.IsCargoOnly);
			AssertEquals("transportDataObject.TransportMode", new TransportModeConverter().ToEnumValue(Core.Constants.TransportModes.Sea), transportDataObject.TransportMode);
			AssertEquals("transportDataObject.LegType", new LegTypeConverter().ToEnumValue(Core.Constants.TransportPlanningType.MainVessel), transportDataObject.LegType);
			AssertEquals("transportDataObject.LegNotes", "Test Leg Notes", transportDataObject.LegNotes);
			AssertEquals("transportDataObject.VesselName", "ANRO ASIA ZZ", transportDataObject.VesselName);
			AssertEquals("transportDataObject.VoyageFlightNo", "121A", transportDataObject.VoyageFlightNo);
			AssertEquals("transportDataObject.AircraftType.Code", "E90", transportDataObject.AircraftType.Code);
			AssertNull("transportDataObject.AircraftType.Description", transportDataObject.AircraftType.Description);
			AssertEquals("transportDataObject.VesselLloydsNumber", "9174622", transportDataObject.VesselLloydsIMO);
			AssertEquals("transportDataObject.PortOfLoading.Code", "NZAKL", transportDataObject.PortOfLoading.Code);
			AssertEquals("transportDataObject.EstimatedDeparture", new ZDateTime(2012, 8, 14), transportDataObject.EstimatedDeparture);
			AssertEquals("transportDataObject.ActualDeparture", new ZDateTime(2012, 8, 15), transportDataObject.ActualDeparture);
			AssertEquals("transportDataObject.PortOfDischarge.Code", "AUSYD", transportDataObject.PortOfDischarge.Code);
			AssertEquals("transportDataObject.EstimatedArrival", new ZDateTime(2012, 8, 18), transportDataObject.EstimatedArrival);
			AssertEquals("transportDataObject.ActualArrival", new ZDateTime(2012, 8, 19), transportDataObject.ActualArrival);
			AssertEquals("transportDataObject.CarrierBookingReference", "XX12345", transportDataObject.CarrierBookingReference);
			AssertEquals("transportDataObject.CarrierServiceLevel.Code", "STD", transportDataObject.CarrierServiceLevel.Code);
			AssertEquals("transportDataObject.Carrier.OrganizationCode", "CARORG", transportDataObject.Carrier.OrganizationCode);
			AssertEquals("transportDataObject.Creditor.OrganizationCode", "CRDSYD", transportDataObject.Creditor.OrganizationCode);
			AssertEquals("transportDataObject.BookingStatus.Code", "QUE", transportDataObject.BookingStatus.Code);
			AssertNull("transportDataObject.AdditionalTransportModeCollection", transportDataObject.AdditionalTransportModeCollection);
		}

		static void AssertContentsSailing(TransportLeg transportData)
		{
			AssertEquals("transportData.LCLReceivalCommences", new ZDateTime(2012, 8, 2), transportData.LCLReceivalCommences);
			AssertEquals("transportData.LCLCutOff", new ZDateTime(2012, 8, 1), transportData.LCLCutOff);
			AssertEquals("transportData.LCLAvailability", new ZDateTime(2012, 8, 23), transportData.LCLAvailability);
			AssertEquals("transportData.LCLStorageDate", new ZDateTime(2012, 8, 24), transportData.LCLStorageDate);
		}

		static void AssertContentsDepartureSchedule(TransportLeg transportData)
		{
			AssertEquals("transportData.DepartureCTO", "DEPCTO", transportData.DepartureCTO.OrganizationCode);
			AssertEquals("transportData.DepartureBerth", "DEP_BERTH", transportData.DepartureBerth);
			AssertEquals("transportData.DepartureReference", "DEP_REF", transportData.DepartureReference);

			AssertEquals("transportData.EstimatedArrivalInPortOfLoading", new ZDateTime(2012, 8, 9), transportData.EstimatedArrivalInPortOfLoading);
			AssertEquals("transportData.ActualArrivalInPortOfLoading", new ZDateTime(2012, 8, 8), transportData.ActualArrivalInPortOfLoading);
			AssertEquals("transportData.DocumentCutOff", new ZDateTime(2012, 8, 7), transportData.DocumentCutOff);
			AssertEquals("transportData.FCLReceivalCommences", new ZDateTime(2012, 8, 6), transportData.FCLReceivalCommences);
			AssertEquals("transportData.FCLCutOff", new ZDateTime(2012, 8, 5), transportData.FCLCutOff);
			AssertEquals("transportData.HazzardReceivalCommences", new ZDateTime(2012, 8, 4), transportData.HazzardReceivalCommences);
			AssertEquals("transportData.HazzardCutOffDate", new ZDateTime(2012, 8, 3), transportData.HazzardCutOffDate);
			AssertEquals("transportData.VGMCutOff", new ZDateTime(2012, 8, 10), transportData.VGMCutOff);

			AssertEquals("transportData.EmptyReceivalCommences", new ZDateTime(2012, 8, 25), transportData.EmptyReceivalCommences);
			AssertEquals("transportData.EmptyCutOff", new ZDateTime(2012, 8, 26), transportData.EmptyCutOff);
			AssertEquals("transportData.ReeferReceivalCommences", new ZDateTime(2012, 8, 27), transportData.ReeferReceivalCommences);
			AssertEquals("transportData.ReeferCutOff", new ZDateTime(2012, 8, 28), transportData.ReeferCutOff);
			AssertEquals("transportData.ScheduledDeparture", new ZDateTime(2012, 8, 17), transportData.ScheduledDeparture);

			AssertEquals("transportData.DepartureFrom.AddressType", "PickUpAddress", transportData.DepartureFrom.AddressType);
			AssertEquals("transportData.DepartureFrom.OrganizationCode", "DEPFROM", transportData.DepartureFrom.OrganizationCode);
			AssertEquals("transportData.DepartureFrom.Address1", "32 SOMETHING ST", transportData.DepartureFrom.Address1);
			AssertEquals("transportData.DepartureFrom.City", "NEW NEW YORK", transportData.DepartureFrom.City);
		}

		static void AssertContentsArrivalSchedule(TransportLeg transportData)
		{
			AssertEquals("transportData.ArrivalCTO", "ARRCTO", transportData.ArrivalCTO.OrganizationCode);
			AssertEquals("transportData.ArrivalBerth", "ARR_BERTH", transportData.ArrivalBerth);
			AssertEquals("transportData.ArrivalReference", "ARR_REF", transportData.ArrivalReference);

			AssertEquals("transportData.FCLAvailability", new ZDateTime(2012, 8, 21), transportData.FCLAvailability);
			AssertEquals("transportData.FCLStorage", new ZDateTime(2012, 8, 22), transportData.FCLStorage);
			AssertEquals("transportData.ScheduledArrival", new ZDateTime(2012, 8, 20), transportData.ScheduledArrival);
			AssertEquals("transportData.ScheduledArrival", new ZDateTime(2012, 8, 10), transportData.ScheduledArrivalInPortOfLoading);

			AssertEquals("transportData.ArrivalAt.AddressType", "DropOffAddress", transportData.ArrivalAt.AddressType);
			AssertEquals("transportData.ArrivalAt.OrganizationCode", "ARVAT", transportData.ArrivalAt.OrganizationCode);
			AssertEquals("transportData.ArrivalAt.Address1", "92 TEST AVE", transportData.ArrivalAt.Address1);
			AssertEquals("transportData.ArrivalAt.City", "ZANZIBAR", transportData.ArrivalAt.City);
		}

		static void AssertCTODateWhenTransportIsNotLinked(TransportLeg transportData)
		{
			AssertEquals("transportData.LCLReceivalCommences", new ZDateTime(2012, 8, 2), transportData.LCLReceivalCommences);
			AssertEquals("transportData.LCLCutOff", new ZDateTime(2012, 8, 1), transportData.LCLCutOff);
			AssertEquals("transportData.LCLAvailability", new ZDateTime(2012, 8, 23), transportData.LCLAvailability);
			AssertEquals("transportData.LCLStorageDate", new ZDateTime(2012, 8, 24), transportData.LCLStorageDate);

			AssertEquals("transportData.FCLReceivalCommences", new ZDateTime(2012, 8, 6), transportData.FCLReceivalCommences);
			AssertEquals("transportData.FCLCutOff", new ZDateTime(2012, 8, 5), transportData.FCLCutOff);
			AssertEquals("transportData.FCLAvailability", new ZDateTime(2012, 8, 21), transportData.FCLAvailability);
			AssertEquals("transportData.FCLStorage", new ZDateTime(2012, 8, 22), transportData.FCLStorage);

			AssertEquals("transportData.DocumentCutOff", new ZDateTime(2012, 8, 7), transportData.DocumentCutOff);
			AssertEquals("transportData.VGMCutOff", new ZDateTime(2012, 8, 10), transportData.VGMCutOff);
		}
	}
}
