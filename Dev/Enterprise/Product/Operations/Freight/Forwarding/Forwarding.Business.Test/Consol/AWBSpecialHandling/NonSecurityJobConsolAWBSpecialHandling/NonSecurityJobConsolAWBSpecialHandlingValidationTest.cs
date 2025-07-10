using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class NonSecurityJobConsolAWBSpecialHandlingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateJKH_Code()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var specialHandling = consol.AWBSpecialHandlingItems.AddNew();

			specialHandling.JKH_Code = "~";
			AssertHasError(specialHandling.JKH_CodeInfo, "The Special Handling Code is invalid.");

			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.FishSeafood;
			AssertNoError(specialHandling.JKH_CodeInfo, "The Special Handling Code is invalid.");

			specialHandling.JKH_Code = "";
			AssertHasErrorContaining(specialHandling.JKH_CodeInfo, MandatoryValidation.MustBeEntered);

			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.EFreightConsignmentWithNoAccompanyingPaperDocuments;
			AssertNoErrorContaining(specialHandling.JKH_CodeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestValidateJKH_CodeForAir()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;

			var container1 = Factory.NewWithValidTestData<ForwardingContainer>();
			var container2 = Factory.NewWithValidTestData<ForwardingContainer>();
			consol.Containers.Add(container1);
			consol.Containers.Add(container2);

			Factory.Save();

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.Consols.Add(consol);
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.Consols.Add(consol);

			var packLine1 = shipment1.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);
			var packLine2 = shipment2.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, container2);
			var undg1ForPackLine1 = packLine1.UNDGs.AddNew();
			undg1ForPackLine1.DI_DG = CreateUNDGSubstance("31XX", "RMD").PK;
			var undg2ForPackLine1 = packLine1.UNDGs.AddNew();
			undg2ForPackLine1.DI_DG = CreateUNDGSubstance("20XX", "RPG RFG").PK;

			var undg1ForPackLine2 = packLine2.UNDGs.AddNew();
			undg1ForPackLine2.DI_DG = CreateUNDGSubstance("34XX", "ROX RCM").PK;
			var undg2ForPackLine2 = packLine2.UNDGs.AddNew();
			undg2ForPackLine2.DI_DG = CreateUNDGSubstance("16XX", "RPB").PK;

			var shipment3 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment3.JS_TransportMode = Constants.TransportModes.Air;
			shipment3.Consols.Add(consol);

			Assert(consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().Select(x => x.JKH_Code).OrderBy(x => x).SequenceEqual(new ZString[] { "RCM", "RFG", "RMD", "ROX", "RPB", "RPG" }));
			consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().All(x => !x.JKH_CodeInfo.HasWarning("Check if this Special Handling Code is still relevant as it does not exist on linked Shipment's Dangerous Goods Substances."));

			Factory.Save();

			consol.Shipments.Remove(shipment2);
			Assert(consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().Select(x => x.JKH_Code).OrderBy(x => x).SequenceEqual(new ZString[] { "RCM", "RFG", "RMD", "ROX", "RPB", "RPG" }));
			var shipment2SpecialCodeList = new string[] { "ROX", "RCM", "RPB" };
			Assert(consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().Where(x => x.JKH_Code == "ROX" || x.JKH_Code == "RCM" || x.JKH_Code == "RPB").All(x => x.JKH_CodeInfo.HasWarning("Check if this Special Handling Code is still relevant as it does not exist on linked Shipment's Dangerous Goods Substances.")));
			Assert(consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().Where(x => x.JKH_Code != "ROX" && x.JKH_Code != "RCM" && x.JKH_Code != "RPB").All(x => !x.JKH_CodeInfo.HasWarning("Check if this Special Handling Code is still relevant as it does not exist on linked Shipment's Dangerous Goods Substances.")));
		}

		UNDGSubstance CreateUNDGSubstance(string code, string specialHandlingCodes)
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = code.Substring(0, 4);
			substance.DG_Code = code;
			substance.DG_Standard = UNDGSubstanceStandardTypes.IATA;
			substance.DG_SpecialHandlingCodes = specialHandlingCodes;
			substance.DG_UniqueRecordId = code;
			return substance;
		}

		public void TestValidateJKH_Code_Uniqueness()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			var specialHandling = consol.AWBSpecialHandlingItems.AddNew();
			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.HuntingTrophiesSkinHideAndAllArticlesMadeFromOrContainingPartsOfSpeciesListedInTheCitesConventionOnInternationalTradeInEndangeredSpeciesAppendices;

			var specialHandling2 = consol.AWBSpecialHandlingItems.AddNew();
			specialHandling2.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.HuntingTrophiesSkinHideAndAllArticlesMadeFromOrContainingPartsOfSpeciesListedInTheCitesConventionOnInternationalTradeInEndangeredSpeciesAppendices;

			AssertHasError("Show error when duplicate special handling codes entered.", specialHandling2.JKH_CodeInfo, "Special Handling codes must be unique.");

			specialHandling2.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.AircraftOnGround;
			AssertNoError(specialHandling2.JKH_CodeInfo, "Special Handling codes must be unique.");
		}

		public void TestCheckConsolHasOverriddenMAWB()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;
			var warningMessage = "The AWB tab is overridden so any changes made to this grid will not take effect there.";

			var specialHandling = consol.AWBHeader.AWBSpecialHandlingItems.AddNew();
			specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.ActiveTemperatureControlledSystem;
			AssertNoWarning("Message should not show up for changes in AWB Header", specialHandling.EP_SpecialHandlingInfo, warningMessage);

			var specialHandling2 = consol.AWBSpecialHandlingItems.AddNew();
			specialHandling2.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoAircraftOnly;
			AssertHasWarning("AWB grid has been changed, so avoid change in consol.", specialHandling2.JKH_CodeInfo, warningMessage);
		}

		public void TestSpecialHandlingValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "DEFRA";
			transport.JW_IsCargoOnly = true;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_InspectionTypeCode = "UNK";

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_InspectionTypeCode = "UNK";

			var specialHandling = consol.AWBSpecialHandlingItems.AddNew();

			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			AssertHasMessageError(specialHandling.JKH_CodeInfo, "The Special Handling Code of 'SCO - Cargo Secure for All-Cargo Aircraft only' is invalid as there are shipments attached to the consol with pack lines with Inspection Code 'UNK'.");

			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertHasMessageError(specialHandling.JKH_CodeInfo, "The Special Handling Code of 'SPX - Cargo Secure for Passenger and All-Cargo Aircraft' is invalid as there are shipments attached to the consol with pack lines with Inspection Code 'UNK'.");

			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
			AssertNoMessageErrors("No warning for special handling status of 'not secured'", specialHandling.JKH_CodeInfo);

			transport.JW_IsCargoOnly = false;
			shipment.JS_InspectionTypeCode = "XRY";
			packline.JL_InspectionTypeCode = "XRY";
			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			AssertHasError(specialHandling.JKH_CodeInfo, "The Special Handling Code of 'SCO - Cargo Secure for All-Cargo Aircraft only' is invalid as not all flights on this Consol are cargo flights. Tick the 'Is Cargo Only' checkbox or override this Security Status.");

			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertNoMessageErrors("All shipments are secured", specialHandling.JKH_CodeInfo);

			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
			AssertNoMessageErrors("Special handling status - not secured", specialHandling.JKH_CodeInfo);

			transport.JW_IsCargoOnly = true;
			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			AssertNoMessageErrors("Special handling status - cargo only", specialHandling.JKH_CodeInfo);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				shipment.ResetAviationSecurityForTesting();
				consol.AWBHeader.ResetSupplyChainSecurityConfigurationForTesting();
				consol.ResetSupplyChainSecurityConfigurationForTesting();

				transport.JW_RL_NKLoadPort = "JPOSA";
				shipment.JS_RL_NKOrigin = "JPOSA";
				shipment.JS_InspectionTypeCode = "UNK";
				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertHasMessageError(specialHandling.JKH_CodeInfo, "The Special Handling Code of 'SPX - Cargo Secure for Passenger and All-Cargo Aircraft' is invalid as there are shipments attached to the consol with Aviation Security Inspection Code 'UNK'.");
			}

			var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypes_HongKong.Value;
			inspectionTypes.Types.Add("EXM", (NoResString)"EXM", true, false);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			using (FreightDataRegistry.Instance.ShipmentInspectionTypes_HongKong.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				shipment.ResetAviationSecurityForTesting();
				consol.AWBHeader.ResetSupplyChainSecurityConfigurationForTesting();
				consol.ResetSupplyChainSecurityConfigurationForTesting();

				transport.JW_RL_NKLoadPort = "HKHKG";
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_InspectionTypeCode = "EXM";

				specialHandling.JKH_Code = ZString.Empty;
				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertHasMessageError(specialHandling.JKH_CodeInfo, "The Special Handling Code of 'SPX - Cargo Secure for Passenger and All-Cargo Aircraft' is invalid as there are shipments attached to the consol with pack lines with Inspection Code 'UNK'.");

				var accountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var approval = accountConsignor.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = accountConsignor.PK;
				shipment.JS_RL_NKOrigin = "HKHKG";
				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				AssertNoMessageErrors("Account Consignors can send shipments on Cargo planes", specialHandling.JKH_CodeInfo);

				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertHasMessageError(specialHandling.JKH_CodeInfo, "The Special Handling Code of 'SPX - Cargo Secure for Passenger and All-Cargo Aircraft' is invalid as there are shipments attached to the consol which have been received from an Account Consignor and are not permitted on passenger flights.");
			}
		}

		public void TestSpecialHandlingValidation_CargoSecureForAllCargoAircraftOnlyWarning()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var expectedMessage = "The Special Handling Code of 'SCO - Cargo Secure for All-Cargo Aircraft only' is invalid as not all flights on this Consol are cargo flights. Tick the 'Is Cargo Only' checkbox or override this Security Status.";

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = true;

				var transport1 = consol.Transports[0];
				transport1.JW_TransportMode = Constants.TransportModes.Air;
				transport1.JW_RL_NKLoadPort = "JPNRT";
				transport1.JW_RL_NKDiscPort = "DEFRA";
				transport1.JW_IsCargoOnly = false;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_InspectionTypeCode = "APP";

				var specialHandling = consol.AWBSpecialHandlingItems.AddNew();
				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertHasError("Expected warning not all EU destination flights are cargo only", specialHandling.JKH_CodeInfo, expectedMessage);

				transport1.JW_IsCargoOnly = true;
				specialHandling.JKH_Code = "";
				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertNoError("Expected no warning as all applicable air transports are cargo only", specialHandling.JKH_CodeInfo, expectedMessage);

				var transport2 = consol.Transports.AddNew();
				transport2.JW_TransportMode = Constants.TransportModes.Road;
				transport2.JW_RL_NKLoadPort = "JPOSA";
				transport2.JW_RL_NKDiscPort = "JPNRT";
				transport2.JW_IsCargoOnly = false;
				specialHandling.JKH_Code = "";
				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertNoError("Expected no warning as all applicable air transports are cargo only", specialHandling.JKH_CodeInfo, expectedMessage);

				var transport3 = consol.Transports.AddNew();
				transport3.JW_TransportMode = Constants.TransportModes.Air;
				transport3.JW_RL_NKLoadPort = "HKHKG";
				transport3.JW_RL_NKDiscPort = "JPOSA";
				transport3.JW_IsCargoOnly = false;
				specialHandling.JKH_Code = "";
				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				Assert("Precondition", ImportExportHelper.IsBranchCountry(transport3.JW_RL_NKLoadPort));
				AssertHasError("Expected warning a locally loaded flight is not cargo only", specialHandling.JKH_CodeInfo, expectedMessage);

				transport3.JW_IsCargoOnly = true;
				specialHandling.JKH_Code = "";
				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertNoError("Expected no warning as all flights that should be cargo only are so", specialHandling.JKH_CodeInfo, expectedMessage);
			}
		}

		public void TestSpecialHandlingValidation_CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft()
		{
			var expectedWarning = "Cargo has not been security screened so another party handling cargo will need to secure cargo for passenger or cargo aircraft.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;

			var specialHandling = consol.AWBSpecialHandlingItems.AddNew();
			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;

			AssertHasWarning(specialHandling.JKH_CodeInfo, expectedWarning);

			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.Mail;

			AssertNoWarning(specialHandling.JKH_CodeInfo, expectedWarning);
		}

		public void TestSpecialHandlingValidation_CargoOnlySecurityCheckAppliesToAirOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var expectedMessage = "The Special Handling Code of 'SCO - Cargo Secure for All-Cargo Aircraft only' is invalid as not all flights on this Consol are cargo flights. Tick the 'Is Cargo Only' checkbox or override this Security Status.";

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;

				var roadTransport = consol.Transports[0];
				roadTransport.JW_RL_NKLoadPort = "JPOSA";
				roadTransport.JW_RL_NKDiscPort = "JPNRT";
				roadTransport.JW_TransportMode = Constants.TransportModes.Road;
				roadTransport.JW_IsCargoOnly = false;

				var airTransport = consol.Transports.AddNew();
				airTransport.JW_RL_NKLoadPort = "JPNRT";
				airTransport.JW_RL_NKDiscPort = "HKHKG";
				airTransport.JW_TransportMode = Constants.TransportModes.Air;
				airTransport.JW_IsCargoOnly = true;

				var awbSpecialHandling = consol.AWBHeader.AWBSpecialHandlingItems.AddNew();
				awbSpecialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				var specialHandling = consol.AWBSpecialHandlingItems.AddNew();
				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertNoError("No error as the non-cargo only leg is ROA", specialHandling.JKH_CodeInfo, expectedMessage);

				airTransport.JW_IsCargoOnly = false;
				awbSpecialHandling.EP_SpecialHandling = "";
				specialHandling.JKH_Code = "";
				awbSpecialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertHasError("Now has a message error as the AIR leg is not cargo only", specialHandling.JKH_CodeInfo, expectedMessage);
			}
		}

		public void TestSpecialHandlingValidation_ECC()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;

			var awbSpecialHandling = consol.AWBHeader.AWBSpecialHandlingItems.AddNew();
			awbSpecialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill;
			var specialHandling1 = consol.AWBSpecialHandlingItems.AddNew();
			specialHandling1.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill;

			var awbSpecialHandling2 = consol.AWBHeader.AWBSpecialHandlingItems.AddNew();
			awbSpecialHandling2.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			var specialHandling2 = consol.AWBSpecialHandlingItems.AddNew();
			specialHandling2.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

			var error = "ECC cannot be chosen here. It is to be used only by the airline.";
			AssertHasError("Show error for a new ECC special handling.", specialHandling1.JKH_CodeInfo, error);

			Factory.Save();

			Assert(specialHandling1.IsInDatabase);
			AssertEquals("ECC", specialHandling1.JKH_Code);

			awbSpecialHandling.Validation.ValidateEP_SpecialHandling();
			specialHandling1.Validation.ValidateJKH_Code();
			AssertHasMessageError("Show message error when special handling is ECC and in database.", specialHandling1.JKH_CodeInfo, error);

			awbSpecialHandling2.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill;
			specialHandling2.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill;
			AssertHasError("Show error when changed as ECC.", specialHandling2.JKH_CodeInfo, error);
		}

		public void TestSpecialHandlingValidation_PackLevelScreening()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "USLAX";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = "AIR";
			shipment1.JS_RL_NKOrigin = "AUBNE";
			shipment1.JS_RL_NKDestination = "USLAX";

			var pack11 = shipment1.OuterPackLines.AddNew();
			pack11.JL_PackageCount = 2;

			var pack12 = shipment1.OuterPackLines.AddNew();
			pack12.JL_PackageCount = 3;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_TransportMode = "AIR";
			shipment2.JS_RL_NKOrigin = "AUBNE";
			shipment2.JS_RL_NKDestination = "USLAX";

			var pack21 = shipment2.OuterPackLines.AddNew();
			pack21.JL_PackageCount = 1;

			shipment1.JS_InspectionTypeCode = "UNK";
			shipment2.JS_InspectionTypeCode = "ETD";
			pack11.JL_InspectionTypeCode = "UNK";
			pack12.JL_InspectionTypeCode = "XRY";
			pack21.JL_InspectionTypeCode = "PHS";

			var specialHandling = consol.AWBSpecialHandlingItems.AddNew();
			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertHasMessageErrorContaining(specialHandling.JKH_CodeInfo, "there are shipments attached to the consol with pack lines with Inspection Code 'UNK'");

			pack11.JL_InspectionTypeCode = "PHS";
			specialHandling.JKH_Code = "";
			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertNoMessageErrorContaining(specialHandling.JKH_CodeInfo, "there are shipments attached to the consol with pack lines with Inspection Code 'UNK'");

			pack11.JL_InspectionTypeCode = "";
			specialHandling.JKH_Code = "";
			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertHasMessageErrorContaining(specialHandling.JKH_CodeInfo, "there are shipments attached to the consol with pack lines with no Inspection Code specified.");

			shipment1.JS_InspectionTypeCode = "APP";
			shipment2.JS_InspectionTypeCode = "APP";
			pack21.JL_InspectionTypeCode = "";
			specialHandling.JKH_Code = "";
			specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertNoMessageErrorContaining(specialHandling.JKH_CodeInfo, "there are shipments attached to the consol with pack lines with no Inspection Code specified.");
		}

		public void TestSpecialHandlingValidation_PackLevelScreening_USTranshipment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = true;
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "BRSAO";

				var transport1 = consol.Transports[0];
				transport1.JW_TransportMode = "AIR";
				transport1.JW_RL_NKLoadPort = "NZAKL";
				transport1.JW_RL_NKDiscPort = "USLAX";

				var transport2 = consol.Transports.AddNew();
				transport2.JW_TransportMode = "AIR";
				transport2.JW_RL_NKLoadPort = "USLAX";
				transport2.JW_RL_NKDiscPort = "BRSAO";

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = "AIR";
				shipment1.JS_RL_NKOrigin = "AUBNE";
				shipment1.JS_RL_NKDestination = "BRSAO";

				var pack11 = shipment1.OuterPackLines.AddNew();
				pack11.JL_PackageCount = 2;

				var pack12 = shipment1.OuterPackLines.AddNew();
				pack12.JL_PackageCount = 3;

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = "AIR";
				shipment2.JS_RL_NKOrigin = "AUBNE";
				shipment2.JS_RL_NKDestination = "BRSAO";

				var pack21 = shipment2.OuterPackLines.AddNew();
				pack21.JL_PackageCount = 1;

				shipment1.JS_InspectionTypeCode = "UNK";
				shipment2.JS_InspectionTypeCode = "ETD";
				pack11.JL_InspectionTypeCode = "UNK";
				pack12.JL_InspectionTypeCode = "XRY";
				pack21.JL_InspectionTypeCode = "PHS";

				var specialHandling = consol.AWBSpecialHandlingItems.AddNew();
				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertHasMessageErrorContaining(specialHandling.JKH_CodeInfo, "there are shipments attached to the consol with pack lines with Inspection Code 'UNK'");

				pack11.JL_InspectionTypeCode = "PHS";
				specialHandling.JKH_Code = "";
				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertNoMessageErrorContaining(specialHandling.JKH_CodeInfo, "there are shipments attached to the consol with pack lines with Inspection Code 'UNK'");

				pack11.JL_InspectionTypeCode = "";
				specialHandling.JKH_Code = "";
				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertHasMessageErrorContaining(specialHandling.JKH_CodeInfo, "there are shipments attached to the consol with pack lines with no Inspection Code specified.");

				shipment1.JS_InspectionTypeCode = "APP";
				shipment2.JS_InspectionTypeCode = "APP";
				pack21.JL_InspectionTypeCode = "";
				specialHandling.JKH_Code = "";
				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertNoMessageErrorContaining(specialHandling.JKH_CodeInfo, "there are shipments attached to the consol with pack lines with no Inspection Code specified.");
			}
		}

		public void TestSpecialHandlingValidation_UK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = true;
				consol.JK_RL_NKLoadPort = "GBLHR";
				consol.JK_RL_NKDischargePort = "USLAX";

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = "AIR";
				shipment1.JS_RL_NKOrigin = "GBLHR";
				shipment1.JS_RL_NKDestination = "USLAX";

				var specialHandling = consol.AWBSpecialHandlingItems.AddNew();
				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertNoError(specialHandling.JKH_CodeInfo, "SCO cannot be used for goods which commence their journey from the UK.");

				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				AssertHasError(specialHandling.JKH_CodeInfo, "SCO cannot be used for goods which commence their journey from the UK.");

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = "AIR";
				shipment2.JS_RL_NKOrigin = "IEDUB";
				shipment2.JS_RL_NKDestination = "USLAX";

				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertNoError(specialHandling.JKH_CodeInfo, "SCO cannot be used for goods which commence their journey from the UK.");

				specialHandling.JKH_Code = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				AssertNoError("Allowed as not all shipments originate in the UK", specialHandling.JKH_CodeInfo, "SCO cannot be used for goods which commence their journey from the UK.");
			}
		}

		public void TestJKHCodeIsFromAirlineSpecific()
		{
			var originPort = Factory.NewWithValidTestData<RefUNLOCO>();
			originPort.Code = "PR1";
			originPort.RL_IATA = "P1";

			var destinationPort = Factory.NewWithValidTestData<RefUNLOCO>();
			destinationPort.Code = "PR2";
			destinationPort.RL_IATA = "P2";

			var refAirline = Factory.NewWithValidTestData<RefAirline>();
			refAirline.RM_AirlinePrefix = "100";
			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "100";
			refAirline.RM_TwoCharacterCode = "FK";

			var refHandling = Factory.NewWithValidTestData<RefAirlineSpecialHandlingCode>();
			refHandling.RHC_Code = "MMT";
			refHandling.RHC_Description = "MMT Description";
			refHandling.RHC_OriginPortOrCountry = originPort.Code;
			refHandling.RHC_DestinationPortOrCountry = destinationPort.Code;
			refHandling.RHC_RM_Airline = refAirline.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.MasterBillAirlinePrefix = "100";
			consol.JK_RL_NKLoadPort = originPort.Code;
			consol.JK_RL_NKDischargePort = destinationPort.Code;

			var specialHandlingItem = consol.AWBSpecialHandlingItems.AddNew();
			specialHandlingItem.JKH_Code = "MMT";

			AssertHasWarning(consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().First().JKH_CodeInfo, "Code is from airline specific special handling codes.");
		}

		public void TestJKHCodeExistsInBothIATAAndAirLine()
		{
			var originPort = Factory.NewWithValidTestData<RefUNLOCO>();
			originPort.Code = "PR1";
			originPort.RL_IATA = "P1";

			var destinationPort = Factory.NewWithValidTestData<RefUNLOCO>();
			destinationPort.Code = "PR2";
			destinationPort.RL_IATA = "P2";

			var refAirline = Factory.NewWithValidTestData<RefAirline>();
			refAirline.RM_AirlinePrefix = "100";
			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "100";
			refAirline.RM_TwoCharacterCode = "FK";

			var refHandling = Factory.NewWithValidTestData<RefAirlineSpecialHandlingCode>();
			refHandling.RHC_Code = "RMD";
			refHandling.RHC_Description = "RMD Description";
			refHandling.RHC_OriginPortOrCountry = originPort.Code;
			refHandling.RHC_DestinationPortOrCountry = destinationPort.Code;
			refHandling.RHC_RM_Airline = refAirline.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.MasterBillAirlinePrefix = "100";
			consol.JK_RL_NKLoadPort = originPort.Code;
			consol.JK_RL_NKDischargePort = destinationPort.Code;

			var specialHandlingItem = consol.AWBSpecialHandlingItems.AddNew();
			specialHandlingItem.JKH_Code = "RMD";

			AssertHasWarning(consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().First().JKH_CodeInfo, "This code currently exists as a defined Special Handling Code in the Reference File for this Airline. Please remove this from the Airline Reference File.");
		}

		public void TestJKHCodeExistsInAirLineButNotinIATA()
		{
			var originPort = Factory.NewWithValidTestData<RefUNLOCO>();
			originPort.Code = "PR1";
			originPort.RL_IATA = "P1";

			var destinationPort = Factory.NewWithValidTestData<RefUNLOCO>();
			destinationPort.Code = "PR2";
			destinationPort.RL_IATA = "P2";

			var refAirline = Factory.NewWithValidTestData<RefAirline>();
			refAirline.RM_AirlinePrefix = "100";
			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "100";
			refAirline.RM_TwoCharacterCode = "FK";

			var refHandling = Factory.NewWithValidTestData<RefAirlineSpecialHandlingCode>();
			refHandling.RHC_Code = "MMT";
			refHandling.RHC_Description = "MMT Description";
			refHandling.RHC_OriginPortOrCountry = originPort.Code;
			refHandling.RHC_DestinationPortOrCountry = destinationPort.Code;
			refHandling.RHC_RM_Airline = refAirline.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.MasterBillAirlinePrefix = "100";
			consol.JK_RL_NKLoadPort = originPort.Code;
			consol.JK_RL_NKDischargePort = destinationPort.Code;

			var specialHandlingItem = consol.AWBSpecialHandlingItems.AddNew();
			specialHandlingItem.JKH_Code = "MMT";

			AssertHasWarning(consol.AWBSpecialHandlingItems.Cast<NonSecurityJobConsolAWBSpecialHandling>().First().JKH_CodeInfo, "A non-IATA approved Special Handling Code has been selected which may not be supported or accepted by other industry recipients.");
		}
	}
}
