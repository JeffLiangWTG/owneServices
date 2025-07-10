using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class SupplyChainSecurityConfigurationUKTest : SupplyChainSecurityConfigurationTest
	{
		public void TestIsEnabledAndAllowRelevantOrganisationsWithoutAviationSecurityApproval()
		{
			Assert(!CountryCodes.EuropeanUnionAviationSecurityMembersList.Contains(CountryCodes.UnitedKingdom));
			Assert(SupplyChainSecurityConfiguration.AllowRelevantOrganisationsWithoutAviationSecurityApproval);
		}

		public override void TestShouldSetScheduledArrivalDate()
		{
			Assert(!SupplyChainSecurityConfiguration.ShouldSetScheduledArrivalDate(null));

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			Assert(!SupplyChainSecurityConfiguration.ShouldSetScheduledArrivalDate(consol));

			consol.JK_TransportMode = TransportModes.Air;
			var securityStatus = consol.AWBHeader.AWBSpecialHandlingItems.AddNew();
			securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly;
			Assert(!SupplyChainSecurityConfiguration.ShouldSetScheduledArrivalDate(consol));

			securityStatus.EP_SpecialHandling = AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
			Assert(SupplyChainSecurityConfiguration.ShouldSetScheduledArrivalDate(consol));
		}

		public override void TestUsesGenericScheme()
		{
			Assert(!SupplyChainSecurityConfiguration.UsesGenericScheme);
		}

		public override void TestAllowDefaultingCargoSecureWithHighRiskRequirements()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "GBLHR";
				consol.JK_RL_NKDischargePort = "AUBNE";

				Assert("Can't default SHR for GB", !SupplyChainSecurityConfiguration.AllowDefaultingCargoSecureForPassengerAndAllCargoAircraft(consol));

				consol.JK_RL_NKLoadPort = "DEHAM";
				Assert("But it's OK for GB user editing DE consol", SupplyChainSecurityConfiguration.AllowDefaultingCargoSecureForPassengerAndAllCargoAircraft(consol));
			}
		}

		public override void TestApprovalCodesList()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "KC", "RA", "NO", "CH" }, SupplyChainSecurityConfiguration.ApprovalCodesList.Cast<CodeDescriptionPair>().Select(x => x.Code));
		}

		public override void TestApprovalCodesWithRequiredDocumentValidation()
		{
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("AC"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("KC"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("RA"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("CH"));
		}

		public override void TestValidApprovalCodesForAviationSecurityApproval()
		{
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval("AC"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval("KC"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval("RA"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval("NO"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval("CH"));
		}

		public override void TestApprovalCodeExpiryDateValidation()
		{
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate("AC"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate("KC"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate("RA"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate("NO"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate("CH"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate("CH"));
			AssertEquals("No maximum expiry for CH", SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears("CH"), 0);
			AssertEquals("No warning for pending expiry for CH", SupplyChainSecurityConfiguration.ApprovalCodeWarnIfApprovalWillLapseInMonths("CH"), 0);
		}

		public override void TestIsApprovedToShipOnPassengerFlights()
		{
			foreach (var approvalCode in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				if (approvalCode == "NO" || approvalCode == "AC")
				{
					Assert("NO and AC are not approved for passenger flights", !SupplyChainSecurityConfiguration.ApprovalCodeIsApprovedToShipOnPassengerFlights(approvalCode));
				}
				else
				{
					Assert("Can ship on passenger flights", SupplyChainSecurityConfiguration.ApprovalCodeIsApprovedToShipOnPassengerFlights(approvalCode));
				}
			}
		}

		public override void TestApprovalNumberFormat()
		{
			var expectedFormat = "^[0-9]{5}-[0-9]{2}$";
			var expectedError = "The Approval Number must be in the following format: 'NNNNN-NN'. For example: 00002-01.";

			Assert("NO", SupplyChainSecurityConfiguration.ApprovalCodeApprovalNumberFormat("NO").IsEmpty);
			Assert("NO", SupplyChainSecurityConfiguration.ApprovalCodeErrorForApprovalNumberFormatNotMet("NO").IsEmpty);

			Assert("AC", SupplyChainSecurityConfiguration.ApprovalCodeApprovalNumberFormat("AC").IsEmpty);
			Assert("AC", SupplyChainSecurityConfiguration.ApprovalCodeErrorForApprovalNumberFormatNotMet("AC").IsEmpty);

			AssertEquals("KC", expectedFormat, SupplyChainSecurityConfiguration.ApprovalCodeApprovalNumberFormat("KC"));
			AssertEquals("KC", expectedError, SupplyChainSecurityConfiguration.ApprovalCodeErrorForApprovalNumberFormatNotMet("KC"));

			AssertEquals("RA", expectedFormat, SupplyChainSecurityConfiguration.ApprovalCodeApprovalNumberFormat("RA"));
			AssertEquals("RA", expectedError, SupplyChainSecurityConfiguration.ApprovalCodeErrorForApprovalNumberFormatNotMet("RA"));
		}

		public override void TestSpecialHandlingErrorForUnauthorizedUser()
		{
			const string expectedErrorForUK = "The Person Screening the cargo must have a valid training certification applicable to handling secured air cargo (either \"CO\" (Cargo Operative), \"COS\" (Cargo Operative Screening), \"CS\" (Cargo Supervisor) or \"CM\" (Cargo Manager)) saved against the staff profile's Human Resources > Certificates & ID Numbers grid.";

			var securityCore = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			securityCore.MaintainConsolAllowToOverrideSecurityStatus.IsAllowed = false;

			#region Only valid certification is taken into consideration for UK, not user security right

			securityCore.MaintainConsolAllowToOverrideSecurityStatus.IsAllowed = true;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = false;
				consol.JK_RL_NKLoadPort = CountryCodes.UnitedKingdom;

				AssertEquals("Uncertified user can choose NSC", ZString.Empty, SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consol, "NSC", true));

				var securityStatusCodeSpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
				securityStatusCodeSpecialHandling.JKH_JK_Consol = consol.PK;
				securityStatusCodeSpecialHandling.JKH_Code = "SPX";

				AssertEquals("Uncertified user cannot choose 'SPX' even when user has a security right", expectedErrorForUK, SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consol, "SPX", true));

				securityCore.MaintainConsolAllowToOverrideSecurityStatus.IsAllowed = false;

				var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
				certificate.XZ_Type = "CS2";
				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);

				AssertEquals("No error for valid certificate even when user doesn't have a security right", ZString.Empty, SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consol, "SPX", true));

				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(-1);

				AssertEquals("Expired Certificate", expectedErrorForUK, SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consol, "SPX", true));
				AssertEquals("No error if Special Handling has no changes", ZString.Empty, SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consol, "SPX", false));
			}

			#endregion
		}

		public void TestCheckOV_EXApprovedOrMajorExporterAdditionalValidation_AccountConsignor()
		{
			const string expectedError = "Account Consignor cannot be selected for addresses in the UK as it does not participate in the Account Consignor Scheme.";

			AssertEquals("AC is not allowed for GB", expectedError, SupplyChainSecurityConfiguration.GetErrorForApprovalCodeInvalidForCountry("AC", "GB"));
			AssertEquals("KC is allowed for GB", ZString.Empty, SupplyChainSecurityConfiguration.GetErrorForApprovalCodeInvalidForCountry("KC", "GB"));
		}

		public void TestInspectionTypeErrorForUncertifiedUser()
		{
			const string expectedError = "The Person Screening the cargo must have a valid training certification applicable to handling secured air cargo (either \"CO\" (Cargo Operative), \"COS\" (Cargo Operative Screening), \"CS\" (Cargo Supervisor) or \"CM\" (Cargo Manager)) saved against the staff profile's Human Resources > Certificates & ID Numbers grid.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "AUBNE";

				shipment.JS_InspectionTypeCode = "XRY";
				Assert("Precondition", shipment.JS_InspectionTypeCodeHasChanges);

				AssertHasError("No certificate", shipment.JS_InspectionTypeCodeInfo, expectedError);

				var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
				certificate.XZ_Type = "CS2";
				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);
				shipment.Validation.ValidateJS_InspectionTypeCode();

				AssertNoError("Valid Certificate", shipment.JS_InspectionTypeCodeInfo, expectedError);

				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(-1);
				shipment.Validation.ValidateJS_InspectionTypeCode();

				AssertHasError("Expired Certificate", shipment.JS_InspectionTypeCodeInfo, expectedError);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				reloadedShipment.Validation.ValidateJS_InspectionTypeCode();
				Assert("Precondition - no changes", !reloadedShipment.JS_InspectionTypeCodeHasChanges);
				AssertEquals("Precondition - XRY", "XRY", reloadedShipment.JS_InspectionTypeCode);

				AssertNoError("The error regarding certificate should not exist when JS_InspectionTypeCode stays unchanged", reloadedShipment.JS_InspectionTypeCodeInfo, expectedError);

				reloadedShipment.JS_InspectionTypeCode = "PHS";

				AssertHasError("User can't change from one inspection type to another", reloadedShipment.JS_InspectionTypeCodeInfo, expectedError);

				reloadedShipment.JS_InspectionTypeCode = "UNK";

				AssertNoError("OK to change to 'UNK'", reloadedShipment.JS_InspectionTypeCodeInfo, expectedError);

				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);
				reloadedShipment.JS_InspectionTypeCode = "APP";

				AssertNoError("Valid Certificate", reloadedShipment.JS_InspectionTypeCodeInfo, expectedError);
			}
		}

		public void TestAdditionalInspectionTypeErrorForUncertifiedUser()
		{
			const string expectedError = "The Person Screening the cargo must have a valid training certification applicable to handling secured air cargo (either \"CO\" (Cargo Operative), \"COS\" (Cargo Operative Screening), \"CS\" (Cargo Supervisor) or \"CM\" (Cargo Manager)) saved against the staff profile's Human Resources > Certificates & ID Numbers grid.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "AUBNE";
				shipment.JS_IsHighRisk = true;

				shipment.JS_AdditionalInspectionTypeCode = "XRY";
				Assert("Precondition", shipment.JS_AdditionalInspectionTypeCodeHasChanges);

				AssertHasError("No certificate", shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
				certificate.XZ_Type = "CS2";
				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);
				shipment.Validation.ValidateJS_AdditionalInspectionTypeCode();

				AssertNoError("Valid Certificate", shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(-1);
				shipment.Validation.ValidateJS_AdditionalInspectionTypeCode();

				AssertHasError("Expired Certificate", shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var reloadedShipment = newFactory.Load<ForwardingShipment>(shipment.PK);
				reloadedShipment.Validation.ValidateJS_AdditionalInspectionTypeCode();
				Assert("Precondition - no changes", !reloadedShipment.JS_AdditionalInspectionTypeCodeHasChanges);
				AssertEquals("Precondition - XRY", "XRY", reloadedShipment.JS_AdditionalInspectionTypeCode);

				AssertNoError("The error regarding certificate should not exist when JS_AdditionalInspectionTypeCode stays unchanged", reloadedShipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				reloadedShipment.JS_AdditionalInspectionTypeCode = "PHS";

				AssertHasError("User can't change from one inspection type to another", reloadedShipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				reloadedShipment.JS_AdditionalInspectionTypeCode = "UNK";

				AssertNoError("OK to change to 'UNK'", reloadedShipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);
				reloadedShipment.JS_InspectionTypeCode = "APP";

				AssertNoError("Valid Certificate", reloadedShipment.JS_InspectionTypeCodeInfo, expectedError);
			}
		}

		public void TestGetUncertifiedUserForScreeningError()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				GlbStaff.CurrentUser.Certificates.DeleteAll();
				var message = "The Person Screening the cargo must have a valid training certification applicable to handling secured air cargo (either \"CO\" (Cargo Operative), \"COS\" (Cargo Operative Screening), \"CS\" (Cargo Supervisor) or \"CM\" (Cargo Manager)) saved against the staff profile's Human Resources > Certificates & ID Numbers grid.";

				AssertEquals(message, SupplyChainSecurityConfiguration.GetUncertifiedUserForScreeningError(GlbStaff.CurrentUser));

				var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
				certificate.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.CO1;
				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);

				AssertEquals(ZString.Empty, SupplyChainSecurityConfiguration.GetUncertifiedUserForScreeningError(GlbStaff.CurrentUser));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				GlbStaff.CurrentUser.Certificates.DeleteAll();

				AssertEquals(ZString.Empty, SupplyChainSecurityConfiguration.GetUncertifiedUserForScreeningError(GlbStaff.CurrentUser));

				var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
				certificate.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.CO1;
				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);

				AssertEquals(ZString.Empty, SupplyChainSecurityConfiguration.GetUncertifiedUserForScreeningError(GlbStaff.CurrentUser));
			}
		}

		#region Pack Level Screening

		public override void TestIsPackLevelScreeningAvailable()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "PGPOM";

				Assert("Available for GB", SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(shipment));
			}
		}

		#endregion

		#region AllowAutomaticCalculationOfApprovedStatus

		public override void TestAllowAutomaticCalculationOfApprovedStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				var shipment = Factory.New<ForwardingShipment>();
				Assert("Automatic calculation of approved status is not allowed for the UK until approved by certificated user", !SupplyChainSecurityConfiguration.AllowAutomaticCalculationOfApprovedStatus(shipment));

				shipment.JS_InspectionTypeCode = "APP";
				Assert("Automatic calculation of approved status is allowed for the UK because shipment is approved by certificated user", SupplyChainSecurityConfiguration.AllowAutomaticCalculationOfApprovedStatus(shipment));
			}
		}

		#endregion

		#region Inspection Status

		public void TestSetApprovedShipperStatus()
		{
			var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_UK.Value;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;

			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_UK.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var consignor1 = Factory.NewWithValidTestData<OrgHeader>();
				consignor1.OH_Code = "CON";
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor1.PK;
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "SGSIN";

				var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
				certificate.XZ_Type = "CS2";
				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus("", true));
				AssertEquals("Status is set to 'UNK' for non-approved consignor", "UNK", shipment.JS_InspectionTypeCode);

				consignor1.MainAddress.OA_RN_NKCountryCode = CountryCodes.UnitedKingdom;

				var addressCountryData = consignor1.MainAddress.KnownShipperDetails.AddNew();
				addressCountryData.OV_OH_OrgHeader = consignor1.PK;
				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus("", true));
				AssertEquals("Status is set to 'APP' for Known Consignor", "APP", shipment.JS_InspectionTypeCode);

				var consignor2 = Factory.NewWithValidTestData<OrgHeader>();
				consignor2.OH_Code = "CON2";
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor2.PK;
				AssertEquals("Consinor has been changed so the Shipment's know status has been recalculated by CW1", "UNK", shipment.JS_InspectionTypeCode);
				AssertHasWarning(shipment.JS_InspectionTypeCodeInfo, "Consignor has been changed so the Shipment's known status has been recalculated by CW1.");
				AssertHasWarning(shipment.JS_InspectionTypeCodeInfo, @"The following Organizations are not Approved:
Consignor (CON2)");
			}
		}

		#endregion

		#region JS_InspectionTypeValidation

		public void TestJS_InspectionTypeValidation_Shipment_Export_UKCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "SGSIN";

				AssertEquals("Precondition", "UNK", shipment.JS_InspectionTypeCode);

				var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
				certificate.XZ_Type = "CS2";
				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);

				shipment.JS_InspectionTypeCode = "APP";
				AssertNoNotifications(
					"Can set approved status for unapproved shipper on shipment from UK when logged into UK",
					shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestJS_InspectionTypeValidation_Shipment_Export_EUCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "ITMIL";
				shipment.JS_RL_NKDestination = "SGSIN";

				AssertEquals("Precondition", "UNK", shipment.JS_InspectionTypeCode);

				var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
				certificate.XZ_Type = "CS2";
				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);

				shipment.JS_InspectionTypeCode = "APP";
				AssertHasError("Can't set approved status for unapproved shipper on shipment from Italy when logged into Italy despite having appropriate UK certificate.",
					shipment.JS_InspectionTypeCodeInfo, string.Format(@"The following Organizations are not Approved so an Inspection Type of Approved/Known Shipper is not allowed:
Consignor ({0})", consignor.OH_Code));
			}
		}

		public void TestJS_InspectionTypeCodeAdditionalValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
				certificate.XZ_Type = "CO1";
				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "DEFRA";

				shipment.JS_InspectionTypeCode = "MAI";
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "MAI exemption is not valid for shipments being uplifted by air from / within the UK.");

				shipment.JS_InspectionTypeCode = "TRN";
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "TRN exemption is not valid for shipments being uplifted by air from / within the UK.");

				shipment.JS_InspectionTypeCode = "VCK";
				AssertHasWarning(shipment.JS_InspectionTypeCodeInfo, "A visual check is only allowed in combination with other methods. Enter the secondary method used instead as that method will be used in the Security Declaration.");

				shipment.JS_IsHighRisk = true;
				AssertNoWarning(shipment.JS_InspectionTypeCodeInfo, "A visual check is only allowed in combination with other methods. Enter the secondary method used instead as that method will be used in the Security Declaration.");

				shipment.JS_IsHighRisk = false;
				shipment.JS_InspectionTypeCode = "NUC";
				AssertNoNotifications(shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_RL_NKOrigin = "IEDUB";
				AssertEquals("UNK", shipment.JS_InspectionTypeCode);
				AssertHasWarning(shipment.JS_InspectionTypeCodeInfo, "Origin has been changed so the Shipment's known status has been recalculated by CW1.");

				shipment.JS_InspectionTypeCode = "MAI";
				shipment.Validation.ValidateAll();
				AssertNoNotifications(shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "TRN";
				AssertNoNotifications(shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "VCK";
				AssertNoNotifications(shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "NUC";
				AssertNoNotifications(shipment.JS_InspectionTypeCodeInfo);
			}
		}

		#endregion

		#region High Risk Requirement

		public override void TestIsHighRiskApplicable()
		{
			Assert(SupplyChainSecurityConfiguration.IsHighRiskApplicable);
		}

		#endregion

		#region Security Declaration

		public void TestCargoSecureForPassengerAndAllCargoAircraftIsAllowed()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "DEFRA";
			consol.JK_RL_NKDischargePort = "USCHI";

			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			shipment1.JS_RL_NKOrigin = "GBLHR";
			shipment2.JS_RL_NKOrigin = "GBLON";

			Assert("SCO is allowed as consol load port is not in the UK", SupplyChainSecurityConfiguration.GetCargoSecureForAllCargoAircraftOnlyIsAllowed(consol));
			AssertEquals(ZString.Empty, SupplyChainSecurityConfiguration.GetErrorForCargoSecureForAllCargoAircraftOnlyIsNotAllowed(consol));

			consol.JK_RL_NKLoadPort = "GBLHR";
			shipment1.JS_RL_NKOrigin = "FRCAL";

			Assert("SCO is allowed as not all shipments originate in the UK", SupplyChainSecurityConfiguration.GetCargoSecureForAllCargoAircraftOnlyIsAllowed(consol));
			AssertEquals(ZString.Empty, SupplyChainSecurityConfiguration.GetErrorForCargoSecureForAllCargoAircraftOnlyIsNotAllowed(consol));

			shipment1.JS_RL_NKOrigin = "GBLHR";

			Assert("SCO is not allowed as consol and all shipments originate in the UK", !SupplyChainSecurityConfiguration.GetCargoSecureForAllCargoAircraftOnlyIsAllowed(consol));
			AssertEquals("SCO cannot be used for goods which commence their journey from the UK.", SupplyChainSecurityConfiguration.GetErrorForCargoSecureForAllCargoAircraftOnlyIsNotAllowed(consol));
		}

		#endregion

		#region UseIssuingAuthorityCountry

		public override void TestUseIssuingAuthorityCountry()
		{
			AssertEquals(true, SupplyChainSecurityConfiguration.UseIssuingAuthorityCountry);
		}

		#endregion

		#region Additional Inspection Type validation

		public void TestAdditionalValidationForAdditionalInspectionType_UKInvalidInspectionTypes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				var expectedError = "exemption is not valid for shipments being uplifted by air from / within the UK.";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "USCHI";

				shipment.JS_IsHighRisk = true;
				shipment.JS_AdditionalInspectionTypeCode = "UNK";
				AssertNoErrorContaining("UNK", shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				shipment.JS_AdditionalInspectionTypeCode = "PHS";
				AssertNoErrorContaining("PHS", shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				shipment.JS_AdditionalInspectionTypeCode = "MAI";
				AssertHasErrorContaining(shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);

				shipment.JS_AdditionalInspectionTypeCode = "TRN";
				AssertHasErrorContaining(shipment.JS_AdditionalInspectionTypeCodeInfo, expectedError);
			}
		}

		#endregion

		#region Security Declaration

		public override void TestCheckEH_AgentApprovalNumberAdditionalValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				const string expectedError = "The Regulated Agent Identifier must be in the following format: 'NNNNN-NN'. For example: 00002-01.";

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "GBLHR";
				consol.JK_RL_NKDischargePort = "CNSHA";

				var header = consol.AWBHeader;
				header.Validation.ValidateEH_AgentApprovalNumber();

				AssertEquals("Precondition", "GB", consol.AWBHeader.EH_RN_NKAgentApprovalCountryCode);
				AssertHasMessageError(header.EH_AgentApprovalNumberInfo, expectedError);

				header.EH_AgentApprovalNumber = "XYZ-AB";
				AssertHasMessageError(header.EH_AgentApprovalNumberInfo, expectedError);

				header.EH_AgentApprovalNumber = "12345-67";
				AssertNoMessageError(header.EH_AgentApprovalNumberInfo, expectedError);

				header.EH_RN_NKAgentApprovalCountryCode = "IT";
				header.EH_AgentApprovalNumber = "XYZ-AB";
				AssertNoMessageError("Approval Number Validation is not used for UK", header.EH_AgentApprovalNumberInfo, expectedError);
			}
		}

		#endregion

		#region Organisations to use

		public override void TestOrganisationsToUseForAviationSecurity()
		{
			AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.Consignor].ValidationCode);
			AssertEquals(1, SupplyChainSecurityConfiguration.OrganisationsToUse.Values.Count);

			var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_UK.Value;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPackingCFS)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolTransportCompany)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolAirline)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;

			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_UK.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
			{
				ResetSupplyChainSecurityConfiguration();
				AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.Consignor].ValidationCode);
				AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.LocalClient].ValidationCode);
				AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany].ValidationCode);
				AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.ConsolTransportCompany].ValidationCode);
				AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.ConsolAirline].ValidationCode);
				AssertEquals(5, SupplyChainSecurityConfiguration.OrganisationsToUse.Values.Count);
			}
		}

		#endregion

		#region IsExportForAviationSecurityPurposes

		public void TestIsExportForAviationSecurityPurposes_TranshipmentConsolAttached()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "PGPOM";
				Assert("Shipment is not export", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "GBLHR";
				consol.JK_RL_NKDischargePort = "JMKIN";
				Assert("Consol is export", SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol));

				shipment.Consols.Add(consol);
				Assert("Shipment is export as it is attached to an export consol", SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));

				consol.JK_RL_NKLoadPort = "AUSYD";
				Assert("Consol is no longer export", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol));
				Assert("Shipment is no longer export", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));
			}
		}

		public void TestIsExportForAviationSecurityPurposes_TranshipmentWithImportConsolAttached()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "PGPOM";
				Assert("Shipment is not export", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "GBLHR";
				Assert("Arrival consol into EU is not export", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol));

				shipment.Consols.Add(consol);
				Assert("Shipment is export as it is attached to an import consol but is not at its final destination", SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));

				consol.JK_RL_NKDischargePort = "FRCDG";
				Assert("Shipment is export as it is attached to an EU import consol but is not at its final destination", SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));

				shipment.JS_RL_NKDestination = "GBLON";
				Assert("Shipment is no longer an export, as it has arrived at destination country", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));

				shipment.JS_RL_NKDestination = "PGPOM";
				consol.JK_RL_NKDischargePort = "AUSYD";
				Assert("Consol is not export", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol));
				Assert("Shipment is no longer export as it is not attached to a consol that goes via the EU", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));
			}
		}

		#endregion

		#region Registry

		public void TestSupplyChainSecurityConfigurationIsEnabledForUK()
		{
			var actualIsEnabled = SupplyChainSecurityConfiguration.IsEnabled;

			AssertEquals(true, actualIsEnabled);
		}

		#endregion

		public override void TestValidateJS_InspectionType_GovernmentApprovedReliableOrganization()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"GBLHR";
				shipment.JS_RL_NKDestination = "JPTYO";
				shipment.JS_InspectionTypeCode = "GOV";

				AssertHasError($"JS_InspectionTypeCode of GOV should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of GOV Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (b) Government approved reliable organization(s) can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		public override void TestValidateJS_InspectionType_AdHocMovementsOfCargo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"GBLHR";
				shipment.JS_RL_NKDestination = "JPTYO";
				shipment.JS_InspectionTypeCode = "ADH";

				AssertHasError($"JS_InspectionTypeCode of ADH should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of ADH Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (c) ad hoc movements of cargo can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		public void TestSupplyChainSecurityShipmentInspectionStatusWillNotChangeWhenRelevantAttributesChangesIfItWasAlreadySetToAPPForUK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.UnitedKingdom))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_InspectionTypeCode = "APP";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_AgentType = AgentType.Agent;
				consol.JK_RL_NKLoadPort = "SGSIN";

				var shipment2 = Factory.New<ForwardingShipment>();
				shipment2.JS_TransportMode = "AIR";
				shipment2.JS_InspectionTypeCode = "GOV";

				consol.Shipments.Add(shipment);
				consol.Shipments.Add(shipment2);

				consol.JK_MasterBillIssueDate = ZDate.Today.AddDays(100);
				AssertRecalculation(shipment, shipment2);

				shipment.JS_E_DEP = ZDate.Today.AddDays(100);
				shipment2.JS_E_DEP = ZDate.Today.AddDays(100);
				AssertRecalculation(shipment, shipment2);

				shipment.JS_SystemCreateTimeUtc = ZDateTime.UtcToday.AddDays(-2).ToDateTime();
				shipment2.JS_SystemCreateTimeUtc = ZDateTime.UtcToday.AddDays(-2).ToDateTime();
				AssertRecalculation(shipment, shipment2);

				AssertEquals("No influneces on Current Date", "MBL", shipment.AviationSecurity.ShipmentDateTypeForAviationSecurity);
				AssertEquals("No influneces on Current Date", "MBL", shipment2.AviationSecurity.ShipmentDateTypeForAviationSecurity);

				shipment.JS_RL_NKDestination = "USLAX";
				shipment2.JS_RL_NKDestination = "USLAX";
				AssertRecalculation(shipment, shipment2);

				var voyage = Factory.New<JobVoyage>();
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
				voyage.GenerateSailings();

				var sailing = voyage.Sailings[0];

				AssertEquals("Precondition", false, sailing.IsReferenced());
				AssertEquals(true, voyage.CanDelete);
				AssertEquals(ZString.Empty, voyage.ReasonForNotAbleToDelete);
				shipment.JS_JX = sailing.PK;

				voyage.JV_IsCargoOnly = true;
				AssertRecalculation(shipment, shipment2);

				var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
				consol2.JK_AgentType = AgentType.Agent;
				consol2.JK_RL_NKLoadPort = "AUSYD";

				shipment.Consols.Add(consol2);
				shipment2.Consols.Add(consol2);
				AssertRecalculation(shipment, shipment2);

				shipment.Consols.Remove(consol2);
				shipment2.Consols.Remove(consol2);
				AssertRecalculation(shipment, shipment2);

				var shipment3 = AddForwardingConsolManyToManyCollection("APP");
				var shipment4 = AddForwardingConsolManyToManyCollection("GOV");
				AssertRecalculation(shipment3, shipment4);

				var shipment5 = RemoveForwardingConsolManyToManyCollection("APP");
				var shipment6 = RemoveForwardingConsolManyToManyCollection("GOV");
				AssertRecalculation(shipment, shipment2);
			}
		}

		ForwardingShipment AddForwardingConsolManyToManyCollection(ZString inspectionTypeCode)
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_InspectionTypeCode = inspectionTypeCode;
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();

			var collection = new AllMasterConsolCollection(shipment);

			return shipment;
		}

		ForwardingShipment RemoveForwardingConsolManyToManyCollection(ZString inspectionTypeCode)
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Shipment);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_InspectionTypeCode = inspectionTypeCode;
			var consol1 = shipment.Consols.AddNew();
			var consol2 = shipment.Consols.AddNew();

			var collection = new AllMasterConsolCollection(shipment);

			var consol3 = shipment.Consols.AddNew();
			shipment.Consols.Remove(consol3);

			return shipment;
		}

		static void AssertRecalculation(ForwardingShipment shipment, ForwardingShipment shipment2)
		{
			AssertEquals("APP", shipment.JS_InspectionTypeCode);
			AssertEquals("should not recaclulate", false, shipment.AviationSecurity.SupplyChainSecurityConfiguration.IsRecalculationNeeded(shipment.JS_InspectionTypeCode));

			AssertEquals("UNK", shipment2.JS_InspectionTypeCode);
			AssertEquals("should recaclulate", true, shipment2.AviationSecurity.SupplyChainSecurityConfiguration.IsRecalculationNeeded(shipment2.JS_InspectionTypeCode));
		}

		#region Implementation

		protected override SupplyChainSecurityConfiguration GetNewSupplyChainSecurityConfigurationToTest()
		{
			return new SupplyChainSecurityConfigurationUK();
		}

		#endregion
	}
}
