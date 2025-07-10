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
using Moq;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class ExportAWBSpecialHandlingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSpecialHandlingValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;

			var header = consol.AWBHeader;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUBNE";
			transport.JW_RL_NKDiscPort = "DEFRA";
			transport.JW_IsCargoOnly = true;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_InspectionTypeCode = "UNK";

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_InspectionTypeCode = "UNK";

			var specialHandling = header.AWBSpecialHandlingItems.AddNew();

			specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			AssertHasMessageError(specialHandling.EP_SpecialHandlingInfo, "The Special Handling Code of 'SCO - Cargo Secure for All-Cargo Aircraft only' is invalid as there are shipments attached to the consol with pack lines with Inspection Code 'UNK'.");

			specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertHasMessageError(specialHandling.EP_SpecialHandlingInfo, "The Special Handling Code of 'SPX - Cargo Secure for Passenger and All-Cargo Aircraft' is invalid as there are shipments attached to the consol with pack lines with Inspection Code 'UNK'.");

			specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
			AssertNoMessageErrors("No warning for special handling status of 'not secured'", specialHandling.EP_SpecialHandlingInfo);

			transport.JW_IsCargoOnly = false;
			shipment.JS_InspectionTypeCode = "XRY";
			packline.JL_InspectionTypeCode = "XRY";
			specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			AssertHasError(specialHandling.EP_SpecialHandlingInfo, "The Special Handling Code of 'SCO - Cargo Secure for All-Cargo Aircraft only' is invalid as not all flights on this Consol are cargo flights. Tick the 'Is Cargo Only' checkbox or override this Security Status.");

			specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertNoMessageErrors("All shipments are secured", specialHandling.EP_SpecialHandlingInfo);

			specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;
			AssertNoMessageErrors("Special handling status - not secured", specialHandling.EP_SpecialHandlingInfo);

			transport.JW_IsCargoOnly = true;
			specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			AssertNoMessageErrors("Special handling status - cargo only", specialHandling.EP_SpecialHandlingInfo);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				shipment.ResetAviationSecurityForTesting();
				consol.AWBHeader.ResetSupplyChainSecurityConfigurationForTesting();
				consol.ResetSupplyChainSecurityConfigurationForTesting();

				transport.JW_RL_NKLoadPort = "JPOSA";
				shipment.JS_RL_NKOrigin = "JPOSA";
				shipment.JS_InspectionTypeCode = "UNK";
				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertHasMessageError(specialHandling.EP_SpecialHandlingInfo, "The Special Handling Code of 'SPX - Cargo Secure for Passenger and All-Cargo Aircraft' is invalid as there are shipments attached to the consol with Aviation Security Inspection Code 'UNK'.");
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

				specialHandling.EP_SpecialHandling = ZString.Empty;
				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertHasMessageError(specialHandling.EP_SpecialHandlingInfo, "The Special Handling Code of 'SPX - Cargo Secure for Passenger and All-Cargo Aircraft' is invalid as there are shipments attached to the consol with pack lines with Inspection Code 'UNK'.");

				var accountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var approval = accountConsignor.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = accountConsignor.PK;
				shipment.JS_RL_NKOrigin = "HKHKG";
				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				AssertNoMessageErrors("Account Consignors can send shipments on Cargo planes", specialHandling.EP_SpecialHandlingInfo);

				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertHasMessageError(specialHandling.EP_SpecialHandlingInfo, "The Special Handling Code of 'SPX - Cargo Secure for Passenger and All-Cargo Aircraft' is invalid as there are shipments attached to the consol which have been received from an Account Consignor and are not permitted on passenger flights.");
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

				var header = consol.AWBHeader;
				var specialHandling = header.AWBSpecialHandlingItems.AddNew();
				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertHasError("Expected warning not all EU destination flights are cargo only", specialHandling.EP_SpecialHandlingInfo, expectedMessage);

				transport1.JW_IsCargoOnly = true;
				specialHandling.EP_SpecialHandling = "";
				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertNoError("Expected no warning as all applicable air transports are cargo only", specialHandling.EP_SpecialHandlingInfo, expectedMessage);

				var transport2 = consol.Transports.AddNew();
				transport2.JW_TransportMode = Constants.TransportModes.Road;
				transport2.JW_RL_NKLoadPort = "JPOSA";
				transport2.JW_RL_NKDiscPort = "JPNRT";
				transport2.JW_IsCargoOnly = false;
				specialHandling.EP_SpecialHandling = "";
				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertNoError("Expected no warning as all applicable air transports are cargo only", specialHandling.EP_SpecialHandlingInfo, expectedMessage);

				var transport3 = consol.Transports.AddNew();
				transport3.JW_TransportMode = Constants.TransportModes.Air;
				transport3.JW_RL_NKLoadPort = "HKHKG";
				transport3.JW_RL_NKDiscPort = "JPOSA";
				transport3.JW_IsCargoOnly = false;
				specialHandling.EP_SpecialHandling = "";
				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				Assert("Precondition", ImportExportHelper.IsBranchCountry(transport3.JW_RL_NKLoadPort));
				AssertHasError("Expected warning a locally loaded flight is not cargo only", specialHandling.EP_SpecialHandlingInfo, expectedMessage);

				transport3.JW_IsCargoOnly = true;
				specialHandling.EP_SpecialHandling = "";
				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertNoError("Expected no warning as all flights that should be cargo only are so", specialHandling.EP_SpecialHandlingInfo, expectedMessage);
			}
		}

		public void TestSpecialHandlingValidation_CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft()
		{
			var expectedWarning = "Cargo has not been security screened so another party handling cargo will need to secure cargo for passenger or cargo aircraft.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;
			var header = consol.AWBHeader;
			var specialHandling = header.AWBSpecialHandlingItems.AddNew();
			specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft;

			AssertHasWarning(specialHandling.EP_SpecialHandlingInfo, expectedWarning);

			specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.Mail;

			AssertNoWarning(specialHandling.EP_SpecialHandlingInfo, expectedWarning);
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

				var header = consol.AWBHeader;
				var specialHandling = header.AWBSpecialHandlingItems.AddNew();
				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertNoErrors("No error as the non-cargo only leg is ROA", specialHandling.EP_SpecialHandlingInfo);

				airTransport.JW_IsCargoOnly = false;
				specialHandling.EP_SpecialHandling = "";
				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

				AssertHasError("Now has a message error as the AIR leg is not cargo only", specialHandling.EP_SpecialHandlingInfo, "The Special Handling Code of 'SCO - Cargo Secure for All-Cargo Aircraft only' is invalid as not all flights on this Consol are cargo flights. Tick the 'Is Cargo Only' checkbox or override this Security Status.");
			}
		}

		public void TestSpecialHandlingValidation_ECC()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;

			var header = consol.AWBHeader;

			var specialHandling1 = header.AWBSpecialHandlingItems.AddNew();
			specialHandling1.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill;

			var specialHandling2 = header.AWBSpecialHandlingItems.AddNew();
			specialHandling2.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;

			var error = "ECC cannot be chosen here. It is to be used only by the airline.";
			AssertHasError("Show error for a new ECC special handling.", specialHandling1.EP_SpecialHandlingInfo, error);

			Factory.Save();

			Assert(specialHandling1.IsInDatabase);
			AssertEquals("ECC", specialHandling1.EP_SpecialHandling);

			specialHandling1.Validation.ValidateEP_SpecialHandling();
			AssertHasMessageError("Show message error when special handling is ECC and in database.", specialHandling1.EP_SpecialHandlingInfo, error);

			specialHandling2.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.ConsignmentEstablishedWithAnElectronicallyConcludedCargoContractEccWithNoAccompanyingPaperAirWaybill;
			AssertHasError("Show error when changed as ECC.", specialHandling2.EP_SpecialHandlingInfo, error);
		}

		public void TestSpecialHandlingValidation_PackLevelScreening()
		{
			Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();

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

			var header = consol.AWBHeader;

			shipment1.JS_InspectionTypeCode = "UNK";
			shipment2.JS_InspectionTypeCode = "ETD";
			pack11.JL_InspectionTypeCode = "UNK";
			pack12.JL_InspectionTypeCode = "XRY";
			pack21.JL_InspectionTypeCode = "PHS";

			var specialHandling = header.AWBSpecialHandlingItems.AddNew();
			specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertHasMessageErrorContaining(specialHandling.EP_SpecialHandlingInfo, "there are shipments attached to the consol with pack lines with Inspection Code 'UNK'");

			pack11.JL_InspectionTypeCode = "PHS";
			specialHandling.EP_SpecialHandling = "";
			specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertNoMessageErrorContaining(specialHandling.EP_SpecialHandlingInfo, "there are shipments attached to the consol with pack lines with Inspection Code 'UNK'");

			pack11.JL_InspectionTypeCode = "";
			specialHandling.EP_SpecialHandling = "";
			specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertHasMessageErrorContaining(specialHandling.EP_SpecialHandlingInfo, "there are shipments attached to the consol with pack lines with no Inspection Code specified.");

			shipment1.JS_InspectionTypeCode = "APP";
			shipment2.JS_InspectionTypeCode = "APP";
			pack21.JL_InspectionTypeCode = "";
			specialHandling.EP_SpecialHandling = "";
			specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			AssertNoMessageErrorContaining(specialHandling.EP_SpecialHandlingInfo, "there are shipments attached to the consol with pack lines with no Inspection Code specified.");
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

				var header = consol.AWBHeader;

				shipment1.JS_InspectionTypeCode = "UNK";
				shipment2.JS_InspectionTypeCode = "ETD";
				pack11.JL_InspectionTypeCode = "UNK";
				pack12.JL_InspectionTypeCode = "XRY";
				pack21.JL_InspectionTypeCode = "PHS";

				var specialHandling = header.AWBSpecialHandlingItems.AddNew();
				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertHasMessageErrorContaining(specialHandling.EP_SpecialHandlingInfo, "there are shipments attached to the consol with pack lines with Inspection Code 'UNK'");

				pack11.JL_InspectionTypeCode = "PHS";
				specialHandling.EP_SpecialHandling = "";
				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertNoMessageErrorContaining(specialHandling.EP_SpecialHandlingInfo, "there are shipments attached to the consol with pack lines with Inspection Code 'UNK'");

				pack11.JL_InspectionTypeCode = "";
				specialHandling.EP_SpecialHandling = "";
				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertHasMessageErrorContaining(specialHandling.EP_SpecialHandlingInfo, "there are shipments attached to the consol with pack lines with no Inspection Code specified.");

				shipment1.JS_InspectionTypeCode = "APP";
				shipment2.JS_InspectionTypeCode = "APP";
				pack21.JL_InspectionTypeCode = "";
				specialHandling.EP_SpecialHandling = "";
				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertNoMessageErrorContaining(specialHandling.EP_SpecialHandlingInfo, "there are shipments attached to the consol with pack lines with no Inspection Code specified.");
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

				var specialHandling = consol.AWBHeader.AWBSpecialHandlingItems.AddNew();
				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertNoError(specialHandling.EP_SpecialHandlingInfo, "SCO cannot be used for goods which commence their journey from the UK.");

				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				AssertHasError(specialHandling.EP_SpecialHandlingInfo, "SCO cannot be used for goods which commence their journey from the UK.");

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_TransportMode = "AIR";
				shipment2.JS_RL_NKOrigin = "IEDUB";
				shipment2.JS_RL_NKDestination = "USLAX";

				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
				AssertNoError(specialHandling.EP_SpecialHandlingInfo, "SCO cannot be used for goods which commence their journey from the UK.");

				specialHandling.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
				AssertNoError("Allowed as not all shipments originate in the UK", specialHandling.EP_SpecialHandlingInfo, "SCO cannot be used for goods which commence their journey from the UK.");
			}
		}

		public void TestSpecialHandlingValidation_UK_UncertifiedUser()
		{
			const string expectedError = "The Person Screening the cargo must have a valid training certification applicable to handling secured air cargo (either \"CO\" (Cargo Operative), \"COS\" (Cargo Operative Screening), \"CS\" (Cargo Supervisor) or \"CM\" (Cargo Manager)) saved against the staff profile's Human Resources > Certificates & ID Numbers grid.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.Transports[0].JW_TransportMode = "AIR";
				consol.Transports[0].JW_RL_NKLoadPort = "DEFRA";
				consol.Transports[0].JW_RL_NKDiscPort = "AUBNE";

				consol.JK_OverrideWaybillDefaults = true;

				var specialHandling = consol.AWBHeader.AWBSpecialHandlingItems.AddNew();
				specialHandling.EP_SpecialHandling = "SPX";

				AssertNoError("Not applicable for DE", specialHandling.EP_SpecialHandlingInfo, expectedError);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;
				consol.Transports[0].JW_TransportMode = "AIR";
				consol.Transports[0].JW_RL_NKLoadPort = "GBLHR";
				consol.Transports[0].JW_RL_NKDiscPort = "AUBNE";

				consol.JK_OverrideWaybillDefaults = true;

				var specialHandling = consol.AWBHeader.AWBSpecialHandlingItems.AddNew();
				specialHandling.EP_SpecialHandling = "NSC";

				AssertNoError("Uncertified user can choose NSC", specialHandling.EP_SpecialHandlingInfo, expectedError);

				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();
				specialHandling.EP_SpecialHandling = "SPX";

				AssertHasError("Uncertified user cannot chose 'SPX'", specialHandling.EP_SpecialHandlingInfo, expectedError);

				var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
				certificate.XZ_Type = "CS2";
				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);
				specialHandling.Validation.ValidateEP_SpecialHandling();

				AssertNoError("Valid Certificate", specialHandling.EP_SpecialHandlingInfo, expectedError);

				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(-1);
				specialHandling.Validation.ValidateEP_SpecialHandling();

				AssertHasError("Expired Certificate", specialHandling.EP_SpecialHandlingInfo, expectedError);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var reloadedSpecialHandling = newFactory.Load<ExportAWBSpecialHandling>(specialHandling.PK);
				AssertEquals("Precondition: SPX", "SPX", reloadedSpecialHandling.EP_SpecialHandling);

				reloadedSpecialHandling.Validation.ValidateEP_SpecialHandling();
				AssertNoError("No error as Special Handling has no changes", reloadedSpecialHandling.EP_SpecialHandlingInfo, expectedError);
			}
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

		public void TestSpecialHandlingValidation_EP_SpecialHandling()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();

			var shipment = consol.Shipments.AddNew();
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(consol, container1);
			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(consol, container2);

			var undg1ForPackLine1 = packLine1.UNDGs.AddNew();
			undg1ForPackLine1.DI_DG = CreateUNDGSubstance("0001", "RFL").PK;
			var undg2ForPackLine1 = packLine1.UNDGs.AddNew();
			undg2ForPackLine1.DI_DG = CreateUNDGSubstance("0002", "ICE MAG").PK;

			var undg1ForPackLine2 = packLine2.UNDGs.AddNew();
			undg1ForPackLine2.DI_DG = CreateUNDGSubstance("0003", "RCX").PK;
			var undg2ForPackLine2 = packLine2.UNDGs.AddNew();
			undg2ForPackLine2.DI_DG = CreateUNDGSubstance("0004", "").PK;

			consol.DefaultSpecialHandlingItems();

			Assert(consol.AWBHeader.AWBSpecialHandlingItems.Cast<ExportAWBSpecialHandling>().Select(x => x.EP_SpecialHandling).OrderBy(x => x).SequenceEqual(new ZString[] { "ICE", "MAG", "RCX", "RFL" }));

			packLine2.UNDGs.RemoveAllFromRelationship();
			consol.DefaultSpecialHandlingItems();
			Assert(consol.AWBHeader.AWBSpecialHandlingItems.Cast<ExportAWBSpecialHandling>().Select(x => x.EP_SpecialHandling).OrderBy(x => x).SequenceEqual(new ZString[] { "ICE", "MAG", "RCX", "RFL" }));
			consol.RunPreSaveValidation();
			AssertHasWarning(consol.AWBHeader.AWBSpecialHandlingItems.Cast<ExportAWBSpecialHandling>().FirstOrDefault(x => x.EP_SpecialHandling == "RCX").EP_SpecialHandlingInfo, "Check if this Special Handling Code is still relevant as it does not exist on linked Shipment's Dangerous Goods Substances.");
		}

		public void TestSpecialHandlingValidation_CheckJKHCodeIsFromAirlineSpecificAndIsNotAddedToIATA()
		{
			var refAirline = Factory.New<RefAirline>();
			refAirline.RM_AirlinePrefix = "Emk";
			refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "Emk";

			var originPort = Factory.New<RefUNLOCO>();
			originPort.Code = "O";
			var destinationPort = Factory.New<RefUNLOCO>();
			destinationPort.Code = "D";

			var refHandling = Factory.New<RefAirlineSpecialHandlingCode>();
			refHandling.RHC_Code = "MMT";
			refHandling.RHC_Description = "MMT Description";
			refHandling.RHC_OriginPortOrCountry = originPort.Code;
			refHandling.RHC_DestinationPortOrCountry = destinationPort.Code;
			refHandling.RHC_RM_Airline = refAirline.PK;

			var expectedWarning = "A non-IATA approved Special Handling Code has been selected which may not be supported or accepted by other industry recipients.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_OverrideWaybillDefaults = true;
			consol.MasterBillAirlinePrefix = refAirline.RM_AirlinePrefix;
			consol.JK_RL_NKLoadPort = originPort.Code;
			consol.JK_RL_NKDischargePort = destinationPort.Code;

			var header = consol.AWBHeader;
			var specialHandling = header.AWBSpecialHandlingItems.AddNew();
			specialHandling.EP_SpecialHandling = refHandling.RHC_Code;

			AssertHasWarning(specialHandling.EP_SpecialHandlingInfo, expectedWarning);
		}
	}
}

