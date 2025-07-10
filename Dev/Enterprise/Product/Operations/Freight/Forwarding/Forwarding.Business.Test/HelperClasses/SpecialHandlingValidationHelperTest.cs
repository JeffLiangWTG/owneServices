using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business.AWB.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class SpecialHandlingValidationHelperTest : TestCaseWithFactory
	{
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

			var invalidError = "The Special Handling Code of '{0} - {1}' is invalid as {2}.";

			consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			AssertHasMessageError(consol.SecurityStatusCodeInfo, string.Format(invalidError, "SCO", "Cargo Secure for All-Cargo Aircraft only", "there are shipments attached to the consol with pack lines with Inspection Code 'UNK'"));

			consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertHasMessageError(consol.SecurityStatusCodeInfo, string.Format(invalidError, "SPX", "Cargo Secure for Passenger and All-Cargo Aircraft", "there are shipments attached to the consol with pack lines with Inspection Code 'UNK'"));

			consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
			AssertNoMessageErrors("No warning for special handling status of 'not secured'", consol.SecurityStatusCodeInfo);

			transport.JW_IsCargoOnly = false;
			shipment.JS_InspectionTypeCode = "XRY";
			packline.JL_InspectionTypeCode = "XRY";
			consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			AssertHasError(consol.SecurityStatusCodeInfo, string.Format(invalidError, "SCO", "Cargo Secure for All-Cargo Aircraft only", "not all flights on this Consol are cargo flights. Tick the 'Is Cargo Only' checkbox or override this Security Status"));

			consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertNoMessageErrors("All shipments are secured", consol.SecurityStatusCodeInfo);

			consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
			AssertNoMessageErrors("Special handling status - not secured", consol.SecurityStatusCodeInfo);

			transport.JW_IsCargoOnly = true;
			consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			AssertNoMessageErrors("Special handling status - cargo only", consol.SecurityStatusCodeInfo);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				shipment.ResetAviationSecurityForTesting();
				consol.ResetSupplyChainSecurityConfigurationForTesting();

				transport.JW_RL_NKLoadPort = "JPOSA";
				shipment.JS_RL_NKOrigin = "JPOSA";
				shipment.JS_InspectionTypeCode = "UNK";
				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertHasMessageError(consol.SecurityStatusCodeInfo, string.Format(invalidError, "SPX", "Cargo Secure for Passenger and All-Cargo Aircraft", "there are shipments attached to the consol with Aviation Security Inspection Code 'UNK'"));
			}

			var inspectionTypes = FreightDataRegistry.Instance.ShipmentInspectionTypes_HongKong.Value;
			inspectionTypes.Types.Add("EXM", (NoResString)"EXM", true, false);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			using (FreightDataRegistry.Instance.ShipmentInspectionTypes_HongKong.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				shipment.ResetAviationSecurityForTesting();
				consol.ResetSupplyChainSecurityConfigurationForTesting();

				transport.JW_RL_NKLoadPort = "HKHKG";
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_InspectionTypeCode = "EXM";

				consol.SecurityStatusCode = ZString.Empty;
				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertHasMessageError(consol.SecurityStatusCodeInfo, string.Format(invalidError, "SPX", "Cargo Secure for Passenger and All-Cargo Aircraft", "there are shipments attached to the consol with pack lines with Inspection Code 'UNK'"));

				var accountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var approval = accountConsignor.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = accountConsignor.PK;
				shipment.JS_RL_NKOrigin = "HKHKG";
				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				AssertNoMessageErrors("Account Consignors can send shipments on Cargo planes", consol.SecurityStatusCodeInfo);

				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertHasMessageError(consol.SecurityStatusCodeInfo, string.Format(invalidError, "SPX", "Cargo Secure for Passenger and All-Cargo Aircraft", "there are shipments attached to the consol which have been received from an Account Consignor and are not permitted on passenger flights"));
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

				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertHasError("Expected warning not all EU destination flights are cargo only", consol.SecurityStatusCodeInfo, expectedMessage);

				transport1.JW_IsCargoOnly = true;
				consol.SecurityStatusCode = "";
				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertNoError("Expected no warning as all applicable air transports are cargo only", consol.SecurityStatusCodeInfo, expectedMessage);

				var transport2 = consol.Transports.AddNew();
				transport2.JW_TransportMode = Constants.TransportModes.Road;
				transport2.JW_RL_NKLoadPort = "JPOSA";
				transport2.JW_RL_NKDiscPort = "JPNRT";
				transport2.JW_IsCargoOnly = false;
				consol.SecurityStatusCode = "";
				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertNoError("Expected no warning as all applicable air transports are cargo only", consol.SecurityStatusCodeInfo, expectedMessage);

				var transport3 = consol.Transports.AddNew();
				transport3.JW_TransportMode = Constants.TransportModes.Air;
				transport3.JW_RL_NKLoadPort = "HKHKG";
				transport3.JW_RL_NKDiscPort = "JPOSA";
				transport3.JW_IsCargoOnly = false;
				consol.SecurityStatusCode = "";
				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				Assert("Precondition", ImportExportHelper.IsBranchCountry(transport3.JW_RL_NKLoadPort));
				AssertHasError("Expected warning a locally loaded flight is not cargo only", consol.SecurityStatusCodeInfo, expectedMessage);

				transport3.JW_IsCargoOnly = true;
				consol.SecurityStatusCode = "";
				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertNoError("Expected no warning as all flights that should be cargo only are so", consol.SecurityStatusCodeInfo, expectedMessage);
			}
		}

		public void TestSpecialHandlingValidation_CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft()
		{
			var expectedWarning = "Cargo has not been security screened so another party handling cargo will need to secure cargo for passenger or cargo aircraft.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;

			consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;

			AssertHasWarning(consol.SecurityStatusCodeInfo, expectedWarning);

			consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.Mail;

			AssertNoWarning(consol.SecurityStatusCodeInfo, expectedWarning);
		}

		public void TestSpecialHandlingValidation_CargoOnlySecurityCheckAppliesToAirOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
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

				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertNoErrors("No error as the non-cargo only leg is ROA", consol.SecurityStatusCodeInfo);

				airTransport.JW_IsCargoOnly = false;
				consol.SecurityStatusCode = "";
				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				var specialHandlingError = "The Special Handling Code of '{0} - {1}' is invalid as {2}.";

				AssertHasError("Now has a message error as the AIR leg is not cargo only", consol.SecurityStatusCodeInfo, string.Format(specialHandlingError, "SCO", "Cargo Secure for All-Cargo Aircraft only", "not all flights on this Consol are cargo flights. Tick the 'Is Cargo Only' checkbox or override this Security Status"));
			}
		}

		public void TestSpecialHandlingValidation_HighRisk()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = true;

				var transport = consol.Transports[0];
				transport.JW_RL_NKLoadPort = "DEFRA";
				transport.JW_RL_NKDiscPort = "AUBNE";
				transport.JW_IsCargoOnly = true;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_RL_NKOrigin = "DEFRA";
				shipment.JS_InspectionTypeCode = "PHS";

				const string shrDesc = "Secure for Passenger, All-Cargo and All-Mail Aircraft in Accordance with High Risk Requirements";

				var invalidError = "The Special Handling Code of '{0} - {1}' is invalid as {2}.";

				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements;
				AssertHasMessageError(consol.SecurityStatusCodeInfo, string.Format(invalidError, "SHR", shrDesc, "there are no high-risk shipments attached to the consol"));

				shipment.JS_IsHighRisk = true;
				AssertEquals("UNK", shipment.JS_AdditionalInspectionTypeCode);

				consol.SecurityStatusCode = ZString.Empty;
				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements;
				AssertHasMessageError(consol.SecurityStatusCodeInfo, string.Format(invalidError, "SHR", shrDesc, "there are shipments attached to the consol with Aviation Security Inspection Code 'UNK'"));

				shipment.JS_AdditionalInspectionTypeCode = "XRY";
				consol.SecurityStatusCode = ZString.Empty;
				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements;
				AssertNoMessageErrors("SHR status is valid", consol.SecurityStatusCodeInfo);
			}
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

			Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

			consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertHasMessageErrorContaining(consol.SecurityStatusCodeInfo, "there are shipments attached to the consol with pack lines with Inspection Code 'UNK'");

			pack11.JL_InspectionTypeCode = "PHS";
			consol.SecurityStatusCode = "";
			consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertNoMessageErrorContaining(consol.SecurityStatusCodeInfo, "there are shipments attached to the consol with pack lines with Inspection Code 'UNK'");

			pack11.JL_InspectionTypeCode = "";
			consol.SecurityStatusCode = "";
			consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertHasMessageErrorContaining(consol.SecurityStatusCodeInfo, "there are shipments attached to the consol with pack lines with no Inspection Code specified.");

			shipment1.JS_InspectionTypeCode = "APP";
			shipment2.JS_InspectionTypeCode = "APP";
			pack21.JL_InspectionTypeCode = "";
			consol.SecurityStatusCode = "";
			consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertNoMessageErrorContaining(consol.SecurityStatusCodeInfo, "there are shipments attached to the consol with pack lines with no Inspection Code specified.");
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

				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertHasMessageErrorContaining(consol.SecurityStatusCodeInfo, "there are shipments attached to the consol with pack lines with Inspection Code 'UNK'");

				pack11.JL_InspectionTypeCode = "PHS";
				consol.SecurityStatusCode = "";
				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertNoMessageErrorContaining(consol.SecurityStatusCodeInfo, "there are shipments attached to the consol with pack lines with Inspection Code 'UNK'");

				pack11.JL_InspectionTypeCode = "";
				consol.SecurityStatusCode = "";
				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertHasMessageErrorContaining(consol.SecurityStatusCodeInfo, "there are shipments attached to the consol with pack lines with no Inspection Code specified.");

				shipment1.JS_InspectionTypeCode = "APP";
				shipment2.JS_InspectionTypeCode = "APP";
				pack21.JL_InspectionTypeCode = "";
				consol.SecurityStatusCode = "";
				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertNoMessageErrorContaining(consol.SecurityStatusCodeInfo, "there are shipments attached to the consol with pack lines with no Inspection Code specified.");
			}
		}

		public void TestSpecialHandlingValidation_UK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var repository = new Mock<ISecuredFreightVerificationChecker>();
				repository.Setup(r => r.FreightIsVerifiedToBeSecure(It.IsAny<bool>())).Returns(true);

				var checker = repository.Object;
				Factory.SetValue(() => checker);

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = true;
				consol.JK_RL_NKLoadPort = "GBLHR";
				consol.JK_RL_NKDischargePort = "USLAX";

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_TransportMode = "AIR";
				shipment1.JS_RL_NKOrigin = "GBLHR";
				shipment1.JS_RL_NKDestination = "USLAX";

				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertNoError(consol.SecurityStatusCodeInfo, "SCO cannot be used for goods which commence their journey from the UK.");

				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				AssertHasError(consol.SecurityStatusCodeInfo, "SCO cannot be used for goods which commence their journey from the UK.");

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = "AIR";
				shipment2.JS_RL_NKOrigin = "IEDUB";
				shipment2.JS_RL_NKDestination = "USLAX";

				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertNoError(consol.SecurityStatusCodeInfo, "SCO cannot be used for goods which commence their journey from the UK.");

				consol.SecurityStatusCode = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				AssertNoError("Allowed as not all shipments originate in the UK", consol.SecurityStatusCodeInfo, "SCO cannot be used for goods which commence their journey from the UK.");
			}
		}
	}
}
