using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageAirValidationTest : TestBaseJobVoyageValidation
	{
		public void TestCountriesWhereCargoValidationApplies()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			VoyageOrigin origin = voyage.Origins.AddNew();
			VoyageDestination destination = voyage.Destinations.AddNew();

			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2004, 04, 14);
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = new ZDateTime(2004, 04, 16);
			voyage.GenerateSailings();
			Assert("Sailings.Count > 0", voyage.Sailings.Count > 0);

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Transport transport = consol.Transports[0];
			transport.JW_JX = voyage.Sailings[0].PK;
			consol.Voyage.ParentConsol = consol;
			VoyageAirValidation validation = (VoyageAirValidation)voyage.Validation;
			AssertEquals("there is only one country Where cargo validation applied", validation.CountriesWhereCargoValidationApplies.Length, 1);
		}

		public override void TestValidateJV_OH_Line()
		{
			RefAirline airLine1 = Factory.New<RefAirline>();
			airLine1.RM_EagleAddedAirlinePrefixOrAccountingCode = "GLM";
			airLine1.RM_ThreeLetterCode = "GLM";
			airLine1.RM_TwoCharacterCode = "GG";

			RefAirline airLine2 = Factory.New<RefAirline>();
			airLine2.RM_EagleAddedAirlinePrefixOrAccountingCode = "MLG";
			airLine2.RM_ThreeLetterCode = "MLG";
			airLine2.RM_TwoCharacterCode = "LM";

			OrgHeader carrier1 = Factory.New<OrgHeader>();
			carrier1.OH_FullName = "Gabriela";
			carrier1.OH_IsShippingProvider = true;
			carrier1.OH_IsAirLine = true;
			carrier1.MainAddress.OA_Address1 = "184 Bourke Rd";
			carrier1.OH_RL_NKClosestPort = "AUSYD";

			OrgMiscServ carrier1Info = carrier1.MiscServ;
			carrier1Info.OM_RM_Airline = airLine1.PK;

			OrgHeader carrier2 = Factory.New<OrgHeader>();
			carrier2.OH_FullName = "Marcionetti";
			carrier2.OH_IsShippingProvider = true;
			carrier2.OH_IsAirLine = true;
			carrier2.MainAddress.OA_Address1 = "184 Bourke Rd";
			carrier2.OH_RL_NKClosestPort = "AUSYD";

			OrgMiscServ carrier2Info = carrier2.MiscServ;
			carrier2Info.OM_RM_Airline = airLine2.PK;

			Factory.Save();

			Voyage.JV_VoyageFlight = "GG123";
			AssertEquals("Expecting JV_OH_Line to default to carrier 1.", carrier1.PK, Voyage.JV_OH_Line);
			AssertNoWarnings("Not expecting Line to have warnings.", Voyage.JV_OH_LineInfo);

			Voyage.JV_VoyageFlight = "LM123";
			AssertEquals("Expecting JV_OH_Line to update to carrier 2.", carrier2.PK, Voyage.JV_OH_Line);
			AssertNoWarnings("Not expecting Line to have warnings.", Voyage.JV_OH_LineInfo);

			Voyage.JV_OH_Line = carrier1.PK;
			AssertHasWarnings("Expecting Line to have warnings because it does not match the flight code.", Voyage.JV_OH_LineInfo);

			Voyage.JV_OH_Line = carrier2.PK;
			AssertNoWarnings("Not expecting Line to have warnings.", Voyage.JV_OH_LineInfo);

			RefAirline airLine3 = Factory.New<RefAirline>();
			airLine3.RM_EagleAddedAirlinePrefixOrAccountingCode = "GL1";
			airLine3.RM_ThreeLetterCode = "GL1";
			airLine3.RM_TwoCharacterCode = "XZ";

			RefAirline airLine4 = Factory.New<RefAirline>();
			airLine4.RM_EagleAddedAirlinePrefixOrAccountingCode = "GL2";
			airLine4.RM_ThreeLetterCode = "GL2";
			airLine4.RM_TwoCharacterCode = "XZ";

			OrgHeader carrier3 = Factory.New<OrgHeader>();
			carrier3.OH_FullName = "Gabriela";
			carrier3.OH_IsShippingProvider = true;
			carrier3.OH_IsAirLine = true;
			carrier3.MainAddress.OA_Address1 = "384 Bourke Rd";
			carrier3.OH_RL_NKClosestPort = "AUSYD";

			OrgMiscServ carrier3Info = carrier3.MiscServ;
			carrier3Info.OM_RM_Airline = airLine3.PK;

			OrgHeader carrier4 = Factory.New<OrgHeader>();
			carrier4.OH_FullName = "Marcionetti";
			carrier4.OH_IsShippingProvider = true;
			carrier4.OH_IsAirLine = true;
			carrier4.MainAddress.OA_Address1 = "384 Bourke Rd";
			carrier4.OH_RL_NKClosestPort = "AUSYD";

			OrgMiscServ carrier4Info = carrier4.MiscServ;
			carrier4Info.OM_RM_Airline = airLine4.PK;

			Factory.Save();

			Voyage.JV_OH_Line = ZGuid.Empty;

			Voyage.JV_VoyageFlight = "XZ123";
			Assert("Expecting JV_OH_Line to be empty.", Voyage.JV_OH_Line.IsEmpty);
			AssertHasWarnings("Expecting Line to have warnings.", Voyage.JV_OH_LineInfo);

			Voyage.JV_OH_Line = carrier4.PK;

			Assert("Not expecting JV_OH_Line to be empty.", !Voyage.JV_OH_Line.IsEmpty);
			AssertNoWarnings("Not expecting Line to have warnings.", Voyage.JV_OH_LineInfo);

			Voyage.JV_OH_Line = carrier3.PK;

			Assert("Not expecting JV_OH_Line to be empty.", !Voyage.JV_OH_Line.IsEmpty);
			AssertNoWarnings("Not expecting Line to have warnings.", Voyage.JV_OH_LineInfo);

			Voyage.JV_VoyageFlight = "XZ234";

			AssertEquals("Not expecting JV_OH_Line to have changed, should be carrier3.", Voyage.JV_OH_Line, carrier3.PK);
			AssertNoWarnings("Not expecting Line to have warnings.", Voyage.JV_OH_LineInfo);
		}

		public void TestValidateJV_VoyageFlight()
		{
			Voyage.JV_VoyageFlight = ZString.Empty;
			AssertHasErrors("Flight number empty, error expected", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "12345678";
			AssertHasNotifications("Flight number has more than 7 characters, error expected.", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "123456";
			AssertHasNotifications("Either or both of the first two characters of the Flight number are not letters, error expected.", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "AAA456";
			AssertHasNotifications("One of the characters 3-6 of the Flight number are letters, error expected.", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "A2A456";
			AssertHasNotifications("One of the characters 3-6 of the Flight number are letters, error expected.", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "A234!6";
			AssertHasNotifications("Flight number are contains non-alphanumeric character, error expected.", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "QF43";
			AssertNoNotifications("Valid flight number, not expecting errors.", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "F43";
			AssertNoNotifications("Valid flight number, not expecting errors.", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "1F1";
			AssertNoNotifications("Valid flight number, not expecting errors.", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "QF1234";
			AssertNoNotifications("Valid flight number, not expecting errors.", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "QF1234A";
			AssertNoNotifications("Valid flight number, not expecting errors.", Voyage.JV_VoyageFlightInfo);
		}

		public void TestValidateJV_VoyageFlight_Charter()
		{
			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			Voyage.JV_IsChartered = true;
			Voyage.JV_VoyageFlight = ZString.Empty;
			AssertHasErrors("Aircraft registration empty, error expected", Voyage.JV_VoyageFlightInfo);

			Voyage.JV_VoyageFlight = "W40983";
			AssertNoErrors("Not expecting errors, aircraft reg. is not empty.", Voyage.JV_VoyageFlightInfo);
		}

		public void TestValidateJV_VoyageFlight_DuplicateFlightNo()
		{
			JobVoyage flight = GetPopulatedVoyage();
			flight.JV_VoyageFlight = "QF7";

			foreach (VoyageOrigin origin in flight.Origins)
			{
				if (origin.JA_RL_NKPortOfLoading == "AUSYD")
				{
					origin.JA_E_DEP = ZDateTime.Now.AddHours(6);
				}
				else if (origin.JA_RL_NKPortOfLoading == "AUMEL")
				{
					origin.JA_E_DEP = ZDateTime.Now.AddHours(9);
				}
				else if (origin.JA_RL_NKPortOfLoading == "AUBNE")
				{
					origin.JA_E_DEP = ZDateTime.Now.AddHours(12);
				}
			}

			JobVoyage flight2 = GetPopulatedVoyage();
			flight2.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			flight2.JV_VoyageFlight = "QF7";

			foreach (VoyageOrigin origin in flight2.Origins)
			{
				if (origin.JA_RL_NKPortOfLoading == "AUSYD")
				{
					origin.JA_E_DEP = ZDateTime.Now.AddDays(2).AddHours(6);
				}
				else if (origin.JA_RL_NKPortOfLoading == "AUMEL")
				{
					origin.JA_E_DEP = ZDateTime.Now.AddDays(2).AddHours(9);
				}
				else if (origin.JA_RL_NKPortOfLoading == "AUBNE")
				{
					origin.JA_E_DEP = ZDateTime.Now.AddDays(2).AddHours(12);
				}
			}

			JobVoyage flight3 = GetPopulatedVoyage();
			flight3.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			flight3.JV_VoyageFlight = "QF8";

			foreach (VoyageOrigin origin in flight2.Origins)
			{
				if (origin.JA_RL_NKPortOfLoading == "AUSYD")
				{
					origin.JA_E_DEP = ZDateTime.Now.AddDays(1).AddHours(8);
				}
				else if (origin.JA_RL_NKPortOfLoading == "AUMEL")
				{
					origin.JA_E_DEP = ZDateTime.Now.AddDays(1).AddHours(11);
				}
				else if (origin.JA_RL_NKPortOfLoading == "AUBNE")
				{
					origin.JA_E_DEP = ZDateTime.Now.AddDays(1).AddHours(13);
				}
			}

			JobVoyage currentFlight = GetPopulatedVoyage();
			currentFlight.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			currentFlight.JV_VoyageFlight = "QF7";

			AssertNoErrors("Expecting no errors on the flight number, as no dates have been set yet.", currentFlight.JV_VoyageFlightInfo);
		}

		public void TestValidateJV_RegistrationNo()
		{
			string error = "Please enter an aircraft registration number.";

			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			Voyage.Validation.ValidateJV_RegistrationNo();
			AssertNoError(Voyage.JV_RegistrationNoInfo, error);

			Voyage.JV_IsChartered = true;
			Voyage.Validation.ValidateJV_RegistrationNo();
			AssertHasError(Voyage.JV_RegistrationNoInfo, error);

			Voyage.JV_RegistrationNo = "snth";
			AssertNoError(Voyage.JV_RegistrationNoInfo, error);
		}

		public void TestJV_IsCargoOnly()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "USNYC";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "UAIEV";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "DEHAM";
			voyage.GenerateSailings();

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";
			consol1.JK_RL_NKLoadPort = "USNYC";
			var shipment1 = consol1.Shipments.AddNew();
			consol1.Transports[0].JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("USNYC", "DEHAM").PK;

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_UniqueConsignRef = "Consol2";
			consol2.JK_RL_NKLoadPort = "UAIEV";
			var shipment2 = consol2.Shipments.AddNew();
			consol2.Transports[0].JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("UAIEV", "DEHAM").PK;

			var consol3 = Factory.New<CommonConsol>();
			consol3.JK_UniqueConsignRef = "Consol3";
			consol3.JK_RL_NKLoadPort = "USNYC";
			var shipment3 = consol3.Shipments.AddNew();
			consol3.Transports[0].JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("USNYC", "DEHAM").PK;

			var consol4 = Factory.New<CommonConsol>();
			consol4.JK_UniqueConsignRef = "Consol4";
			consol4.JK_RL_NKLoadPort = "UAIEV";
			var shipment4 = consol4.Shipments.AddNew();
			consol4.Transports.AddNew();
			consol4.Transports[0].JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("UAIEV", "DEHAM").PK;

			Factory.Save();

			var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value;
			inspectionTypes.Types.Add("XXX", (NoResString)"XXX IS TOO DANGEROUS", false, false);
			using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
				{
					// reload shipments and voyage to ensure we are working with the current company
					var newFactory = new BusinessObjectFactory();
					var shipment1BO = newFactory.Load<CommonShipment>(shipment1.PK);
					var shipment2BO = newFactory.Load<CommonShipment>(shipment2.PK);
					var shipment3BO = newFactory.Load<CommonShipment>(shipment3.PK);
					var shipment4BO = newFactory.Load<CommonShipment>(shipment4.PK);
					shipment1BO.JS_InspectionTypeCode = "XXX";
					shipment2BO.JS_InspectionTypeCode = "XXX";
					shipment3BO.JS_InspectionTypeCode = "APP";
					shipment4BO.JS_InspectionTypeCode = "APP";

					var voyageBO = newFactory.Load<JobVoyage>(voyage.PK);
					voyageBO.JV_IsCargoOnly = false;
					AssertHasError(voyageBO.JV_IsCargoOnlyInfo, "For a voyage that is not Cargo Only, all Shipments must be Aviation Security approved or Exempt, Consols containing such shipments are\r\nConsol1");

					voyageBO.JV_IsCargoOnly = true;
					AssertNoErrors(voyageBO.JV_IsCargoOnlyInfo);
					AssertNoWarnings(voyageBO.JV_IsCargoOnlyInfo);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry("UA"))
				{
					// reload shipments, consols and voyage to ensure we are working with the current company
					var newFactory = new BusinessObjectFactory();
					var shipment1Bo = newFactory.Load<CommonShipment>(shipment1.PK);
					var shipment2Bo = newFactory.Load<CommonShipment>(shipment2.PK);
					var shipment3Bo = newFactory.Load<CommonShipment>(shipment3.PK);
					var shipment4Bo = newFactory.Load<CommonShipment>(shipment4.PK);
					shipment1Bo.JS_InspectionTypeCode = "XXX";
					shipment2Bo.JS_InspectionTypeCode = "XXX";
					shipment3Bo.JS_InspectionTypeCode = "APP";
					shipment4Bo.JS_InspectionTypeCode = "APP";

					var voyageBo = newFactory.Load<JobVoyage>(voyage.PK);
					voyageBo.JV_AirSeaRoad = Constants.TransportModes.Sea;
					voyageBo.JV_IsCargoOnly = false;
					voyageBo.Validation.ValidateJV_IsCargoOnly();
					AssertNoWarnings(voyageBo.JV_IsCargoOnlyInfo);

					voyageBo.JV_AirSeaRoad = Constants.TransportModes.Air;
					voyageBo.Validation.ValidateJV_IsCargoOnly();
					AssertHasWarning(voyageBo.JV_IsCargoOnlyInfo, "For a voyage that is not Cargo Only, all Shipments should be Aviation Security approved or Exempt, Consols containing such shipments are\r\nConsol2");

					var consol1Bo = newFactory.Load<CommonConsol>(consol1.PK);
					var consol2Bo = newFactory.Load<CommonConsol>(consol2.PK);
					consol1Bo.Shipments.RemoveAll();
					consol2Bo.Shipments.RemoveAll();

					voyageBo.Validation.ValidateJV_IsCargoOnly();
					AssertNoWarnings(voyageBo.JV_IsCargoOnlyInfo);
				}
			}
		}

		public void TestJV_IsCargoOnly_FlightsNotOriginatingInLoginCountry()
		{
			var voyage1 = Factory.New<JobVoyage>();
			voyage1.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage1.Origins.AddNew().JA_RL_NKPortOfLoading = "JPOSA";
			voyage1.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USNYC";
			voyage1.GenerateSailings();

			var voyage2 = Factory.New<JobVoyage>();
			voyage2.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage2.Origins.AddNew().JA_RL_NKPortOfLoading = "USNYC";
			voyage2.Destinations.AddNew().JB_RL_NKPortOfDischarge = "FRPAR";
			voyage2.GenerateSailings();

			var voyage3 = Factory.New<JobVoyage>();
			voyage3.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage3.Origins.AddNew().JA_RL_NKPortOfLoading = "BRRIO";
			voyage3.Destinations.AddNew().JB_RL_NKPortOfDischarge = "CUHAV";
			voyage3.GenerateSailings();

			var voyage4 = Factory.New<JobVoyage>();
			voyage4.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage4.Origins.AddNew().JA_RL_NKPortOfLoading = "CUHAV";
			voyage4.Destinations.AddNew().JB_RL_NKPortOfDischarge = "JMKIN";

			var consol1 = Factory.New<CommonConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";
			consol1.JK_RL_NKLoadPort = "JPOSA";
			var shipment1 = consol1.Shipments.AddNew();
			consol1.Transports[0].JW_JX = voyage1.Sailings.GetSailingFromLoadAndDischarge("JPOSA", "USNYC").PK;
			consol1.Transports.AddNew();
			consol1.Transports[1].JW_JX = voyage2.Sailings.GetSailingFromLoadAndDischarge("USNYC", "FRPAR").PK;

			var consol2 = Factory.New<CommonConsol>();
			consol2.JK_UniqueConsignRef = "Consol2";
			consol2.JK_RL_NKLoadPort = "BRRIO";
			var shipment2 = consol2.Shipments.AddNew();
			consol2.Transports[0].JW_JX = voyage3.Sailings.GetSailingFromLoadAndDischarge("BRRIO", "CUHAV").PK;
			consol2.Transports.AddNew();
			consol2.Transports[1].JW_JX = voyage4.Sailings.GetSailingFromLoadAndDischarge("CUHAV", "JMKIN").PK;

			Factory.Save();

			var inspectionTypesJapan = FreightDataRegistry.Instance.ShipmentInspectionTypes_Japan.Value;
			inspectionTypesJapan.Types.Add("XXX", (NoResString)"XXX", false, false);

			var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value;
			inspectionTypes.Types.Add("XXX", (NoResString)"XXX", false, false);

			using (FreightDataRegistry.Instance.ShipmentInspectionTypes_Japan.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypesJapan))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				// reload shipments and voyage to ensure we are working with the current company
				var newFactory = new BusinessObjectFactory();
				var shipment1Bo = newFactory.Load<CommonShipment>(shipment1.PK);
				shipment1Bo.JS_InspectionTypeCode = "XXX";

				var voyage1Bo = newFactory.Load<JobVoyage>(voyage1.PK);
				var voyage2Bo = newFactory.Load<JobVoyage>(voyage2.PK);
				voyage1Bo.JV_IsCargoOnly = false;
				voyage2Bo.JV_IsCargoOnly = true;
				AssertHasError(voyage1Bo.JV_IsCargoOnlyInfo, "For a voyage that is not Cargo Only, all Shipments must be Aviation Security approved or Exempt, Consols containing such shipments are\r\nConsol1");
				AssertNoNotifications("Voyage 2 is cargo only and does not depart in JP", voyage2Bo.JV_IsCargoOnlyInfo);

				voyage1Bo.JV_IsCargoOnly = true;
				voyage2Bo.JV_IsCargoOnly = false;
				AssertNoNotifications("Voyage 1 is cargo only", voyage1Bo.JV_IsCargoOnlyInfo);
				AssertNoNotifications("Voyage 2 does not depart from the JP", voyage2Bo.JV_IsCargoOnlyInfo);
			}

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("BR"))
			{
				// reload shipments and voyage to ensure we are working with the current company
				var newFactory = new BusinessObjectFactory();
				var shipment2Bo = newFactory.Load<CommonShipment>(shipment2.PK);
				shipment2Bo.JS_InspectionTypeCode = "XXX";

				var voyage3Bo = newFactory.Load<JobVoyage>(voyage3.PK);
				var voyage4Bo = newFactory.Load<JobVoyage>(voyage4.PK);
				voyage3Bo.JV_IsCargoOnly = false;
				voyage4Bo.JV_IsCargoOnly = true;
				AssertHasWarning(voyage3Bo.JV_IsCargoOnlyInfo, "For a voyage that is not Cargo Only, all Shipments should be Aviation Security approved or Exempt, Consols containing such shipments are\r\nConsol2");
				AssertNoNotifications("Voyage 4 is cargo only and does not depart from BR", voyage4Bo.JV_IsCargoOnlyInfo);

				voyage3Bo.JV_IsCargoOnly = true;
				voyage4Bo.JV_IsCargoOnly = false;
				AssertNoNotifications("Voyage 3 is cargo only", voyage3Bo.JV_IsCargoOnlyInfo);
				AssertNoNotifications("Voyage 4 does not depart from BR", voyage4Bo.JV_IsCargoOnlyInfo);
			}
		}

		public void TestJV_IsCargoOnly_AccountConsignor()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "HKHKG";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "DEHAM";
			voyage.GenerateSailings();

			CommonConsol consol1 = (CommonConsol)Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			consol1.JK_UniqueConsignRef = "Consol1";
			consol1.JK_RL_NKLoadPort = "HKHKG";
			consol1.JK_TransportMode = "AIR";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var shipment1 = consol1.Shipments.AddNew();
				shipment1.JS_TransportMode = "AIR";
				consol1.Transports[0].JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("HKHKG", "DEHAM").PK;

				Factory.Save();

				shipment1.JS_InspectionTypeCode = "TRN";

				voyage.JV_IsCargoOnly = false;
				AssertNoErrors(voyage.JV_IsCargoOnlyInfo);

				voyage.JV_IsCargoOnly = true;
				AssertNoErrors(voyage.JV_IsCargoOnlyInfo);

				var accountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var knownShipperDetails = accountConsignor.MainAddress.KnownShipperDetails.AddNew();
				knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				shipment1.ConsignorDocumentaryAddress.OrganisationPK = accountConsignor.PK;
				shipment1.JS_InspectionTypeCode = "APP";

				voyage.JV_IsCargoOnly = false;

				ZString expectedError = @"One or more Consols linked to this Voyage have Shipments that have been received from an Account Consignor so can only be sent on 'Is Cargo Only' aircraft even though tendered as known cargo. Please select an 'Is Cargo Only' flight, change the Shipment's Inspection type or remove the Shipment(s) from the affected Consols. Consols containing such Shipments are:
Consol1";
				AssertHasError(voyage.JV_IsCargoOnlyInfo, expectedError);
			}
		}

		#region Implementation

		protected override BaseJobVoyageValidation GetValidationObject()
		{
			return new VoyageAirValidation(Voyage);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
		}

		#endregion
	}
}
