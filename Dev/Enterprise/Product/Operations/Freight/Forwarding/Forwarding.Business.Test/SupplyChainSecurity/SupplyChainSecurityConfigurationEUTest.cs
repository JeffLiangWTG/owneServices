using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	class SupplyChainSecurityConfigurationEUTest : SupplyChainSecurityConfigurationTest
	{
		public override void TestOrganisationsToUseForAviationSecurity()
		{
			AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.Consignor].ValidationCode);
			AssertEquals(1, SupplyChainSecurityConfiguration.OrganisationsToUse.Values.Count);

			var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_EuropeanUnion.Value;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPackingCFS)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolSendingAgent)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolAirline)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;

			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_EuropeanUnion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
			{
				ResetSupplyChainSecurityConfiguration();
				AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.Consignor].ValidationCode);
				AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.LocalClient].ValidationCode);
				AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany].ValidationCode);
				AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.ConsolSendingAgent].ValidationCode);
				AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.ConsolAirline].ValidationCode);
				AssertEquals(5, SupplyChainSecurityConfiguration.OrganisationsToUse.Values.Count);
			}
		}

		public override void TestApprovalCodesList()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "AC", "AH", "KC", "RA", "NO", "CH" }, SupplyChainSecurityConfiguration.ApprovalCodesList.Cast<CodeDescriptionPair>().Select(x => x.Code));
		}

		public override void TestApprovalCodesWithRequiredDocumentValidation()
		{
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("AC"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("KC"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("RA"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("CH"));
		}

		public override void TestValidApprovalCodesForAviationSecurityApproval()
		{
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval("AC"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval("KC"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval("RA"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval("NO"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval("CH"));
		}

		public void TestIsOrgLevelApproval()
		{
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeIsOrgLevelApproval("AC"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeIsOrgLevelApproval("KC"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeIsOrgLevelApproval("RA"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeIsOrgLevelApproval("NO"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeIsOrgLevelApproval("CH"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeIsOrgLevelApproval("AH"));
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

		public override void TestGetCountrySpecificShipmentInspectionTypeCollection()
		{
			var inspectionTypes = new ShipmentInspectionTypes();
			inspectionTypes.Types.Add("ABC", (NoResString)"ABC", false, false);
			inspectionTypes.Types.Add("QQQ", (NoResString)"QQQ", false, false);

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var securityEnabledList = SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection;

					AssertContainsExactElementsInAnyOrder(
						new[] { "PHS", "VCK", "XRY", "EDS", "AOM", "EDD", "ETD", "CMD", "EVD", "SMU", "MAI", "BIO", "DIP", "LFS", "NUC", "TRN", "GOV", "ADH" },
						securityEnabledList.Cast<ShipmentInspectionType>().Select(x => x.Code));
				}

				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var securityDisabledList = SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection;

					AssertContainsExactElementsInAnyOrder(
						new[] { "ABC", "QQQ" },
						securityDisabledList.Cast<ShipmentInspectionType>().Select(x => x.Code));
				}
			}
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

		public void TestMaximumExpiryDate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ES"))
			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(false)))
			{
				var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
				org.Addresses.RemoveAll();

				var address = org.Addresses.AddNew();
				var data = address.KnownShipperDetails.AddNew();
				data.OV_OH_OrgHeader = org.PK;
				data.OV_OA_ApprovedLocation = address.PK;
				data.OV_EXApprovalNumber = "A1234-43";

				var expiryDate = ZDate.Today.AddYears(5);

				data.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				data.OV_EXApprovalExpiryDate = expiryDate.AddDays(1);
				AssertHasErrorContaining(data.OV_EXApprovalExpiryDateInfo, string.Format("The Expiry Date cannot be more than 5 years in the future."));

				data.OV_EXApprovalExpiryDate = expiryDate.AddDays(-1);
				AssertNoErrorContaining(data.OV_EXApprovalExpiryDateInfo, string.Format("The Expiry Date cannot be more than 5 years in the future."));

				data.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				data.OV_EXApprovalExpiryDate = expiryDate.AddDays(1);
				AssertHasErrorContaining(data.OV_EXApprovalExpiryDateInfo, string.Format("The Expiry Date cannot be more than 5 years in the future."));

				data.OV_EXApprovalExpiryDate = expiryDate.AddDays(-1);
				AssertNoErrorContaining(data.OV_EXApprovalExpiryDateInfo, string.Format("The Expiry Date cannot be more than 5 years in the future."));

				data.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.ApprovedHaulier;
				data.OV_EXApprovalExpiryDate = expiryDate.AddDays(1);
				AssertHasErrorContaining(data.OV_EXApprovalExpiryDateInfo, string.Format("The Expiry Date cannot be more than 5 years in the future."));

				data.OV_EXApprovalExpiryDate = expiryDate.AddDays(-1);
				AssertNoErrorContaining(data.OV_EXApprovalExpiryDateInfo, string.Format("The Expiry Date cannot be more than 5 years in the future."));

				data.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				AssertEquals("Expiry date is blanked", ZDate.Empty, data.OV_EXApprovalExpiryDate);
				Assert("Expiry date is read-only", data.OV_EXApprovalExpiryDateInfo.ReadOnly);
				AssertNoErrors(data.OV_EXApprovalExpiryDateInfo);
			}
		}

		public void TestOrgCountryDataMandatoryValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ES"))
			{
				var validToDate = ZDate.Today.AddDays(10);

				var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var orgCountryData = organisation.MainAddress.KnownShipperDetails.AddNew();
				orgCountryData.OV_OH_OrgHeader = organisation.PK;
				orgCountryData.OV_OA_ApprovedLocation = organisation.MainAddress.PK;

				using (new RequiredDocument(organisation, "KCA", "12345-67"))
				{
					orgCountryData.OV_EXApprovedOrMajorExporter = "KC";
					orgCountryData.Validation.ValidateAll();

					AssertHasError(orgCountryData.OV_EXApprovalNumberInfo, "Please enter an Approval Number.");
					AssertHasError(orgCountryData.OV_EXApprovalExpiryDateInfo, "Please enter an Approval Expiry Date.");

					orgCountryData.OV_EXApprovalNumber = "12345-67";
					orgCountryData.OV_EXApprovalExpiryDate = validToDate;

					AssertNoErrors(orgCountryData.OV_EXApprovalNumberInfo);
					AssertNoErrors(orgCountryData.OV_EXApprovalExpiryDateInfo);

					orgCountryData.OV_EXApprovedOrMajorExporter = "AC";

					AssertNoErrors(orgCountryData.OV_EXApprovalNumberInfo);
					AssertNoErrors(orgCountryData.OV_EXApprovalExpiryDateInfo);

					orgCountryData.OV_EXApprovalNumber = "";
					orgCountryData.OV_EXApprovalExpiryDate = ZDate.Empty;

					AssertNoErrors("Approval number is not required for AC", orgCountryData.OV_EXApprovalNumberInfo);
					AssertNoErrors("Expiry date is not required for AC", orgCountryData.OV_EXApprovalExpiryDateInfo);

					orgCountryData.OV_EXApprovedOrMajorExporter = "RA";

					AssertHasError(orgCountryData.OV_EXApprovalNumberInfo, "Please enter an Approval Number.");
					AssertHasError(orgCountryData.OV_EXApprovalExpiryDateInfo, "Please enter an Approval Expiry Date.");

					orgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembershipEx.Codes.No;

					AssertEquals("", orgCountryData.OV_EXApprovalNumber);
					AssertEquals(ZDate.Empty, orgCountryData.OV_EXApprovalExpiryDate);

					AssertNoErrors(orgCountryData.OV_EXApprovalNumberInfo);
					AssertNoErrors(orgCountryData.OV_EXApprovalExpiryDateInfo);

					orgCountryData.OV_EXApprovedOrMajorExporter = "AH";
					orgCountryData.Validation.ValidateAll();

					AssertHasError(orgCountryData.OV_EXApprovalNumberInfo, "Please enter an Approval Number.");
					AssertHasError(orgCountryData.OV_EXApprovalExpiryDateInfo, "Please enter an Approval Expiry Date.");

					orgCountryData.OV_EXApprovalNumber = "12345-67";
					orgCountryData.OV_EXApprovalExpiryDate = validToDate;

					AssertNoErrors(orgCountryData.OV_EXApprovalNumberInfo);
					AssertNoErrors(orgCountryData.OV_EXApprovalExpiryDateInfo);
				}
			}
		}

		public void TestApprovalNumberValidation_AC()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(false)))
			{
				var validToDate = ZDate.Today.AddDays(10);

				var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var address = organisation.MainAddress;
				address.OA_RN_NKCountryCode = "FR";
				var orgCountryData = organisation.MainAddress.KnownShipperDetails.AddNew();
				orgCountryData.OV_OH_OrgHeader = organisation.PK;
				orgCountryData.OV_OA_ApprovedLocation = organisation.MainAddress.PK;

				orgCountryData.OV_EXApprovedOrMajorExporter = "AC";
				orgCountryData.Validation.ValidateAll();
				AssertNoErrors("Approval number is not required", orgCountryData.OV_EXApprovalNumberInfo);
				AssertHasError("Required document is required", orgCountryData.OV_EXApprovedOrMajorExporterInfo, "Before flagging this organization as approved, attach a document to eDocs using type \"KCA\" and record the details for the document within the Document Tracking grid.");

				using (new RequiredDocument(organisation, "KCA", "12345-67"))
				{
					orgCountryData.Validation.ValidateAll();
					AssertNoErrors("Approval number is not required", orgCountryData.OV_EXApprovalNumberInfo);
					AssertNoErrors("Required document is attached", orgCountryData.OV_EXApprovedOrMajorExporterInfo);

					orgCountryData.OV_EXApprovalNumber = "44444-55";
					AssertHasError("Approval number must match required document if entered.", orgCountryData.OV_EXApprovalNumberInfo, "The Approval Number must match the Document Number for the required document type KCA before marking the organization as approved.");

					orgCountryData.OV_EXApprovalNumber = "12345-67";
					AssertNoErrors("Approval number matches required document", orgCountryData.OV_EXApprovalNumberInfo);
				}
			}
		}

		public void TestAddressDefaulting()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
				org.Addresses.RemoveAndDeleteAll();

				AssertEquals("Precondition: one address is automatically created", 1, org.Addresses.Count);

				var officeAddress = org.Addresses[0];
				officeAddress.AddAddressType(OrgAddressType.Office);
				officeAddress.Address1 = "Office";

				var pickupAddress = org.Addresses.AddNew();
				pickupAddress.AddAddressType(OrgAddressType.Pickup);
				pickupAddress.Address1 = "Pickup";

				var deliveryAddress = org.Addresses.AddNew();
				deliveryAddress.AddAddressType(OrgAddressType.Delivery);
				deliveryAddress.Address1 = "Delivery";

				var countryData = org.CountryDataCollectionForThisCompany.AddNew();
				countryData.OV_EXApprovedOrMajorExporter = "AC";
				AssertEquals("Main address defaults for AC", officeAddress.PK, countryData.OV_OA_ApprovedLocation);

				countryData.OV_OA_ApprovedLocation = ZGuid.Empty;
				countryData.OV_EXApprovedOrMajorExporter = "RA";
				AssertEquals("No address defaults for RA", ZGuid.Empty, countryData.OV_OA_ApprovedLocation);

				countryData.OV_EXApprovedOrMajorExporter = "KC";
				AssertEquals("No address defaults for KC", ZGuid.Empty, countryData.OV_OA_ApprovedLocation);

				countryData.OV_EXApprovedOrMajorExporter = "CH";
				AssertEquals("Main address defaults for CH", officeAddress.PK, countryData.OV_OA_ApprovedLocation);
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

		public void TestApprovalNumberValidation()
		{
			var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("PT"))
			using (new RequiredDocument(organisation, "KCA", "ZZZ", "PER", ZDate.Today.AddDays(100)))
			using (new RequiredDocument(organisation, "KCA", "KC-H0643-13A", "PER", ZDate.Today.AddDays(100)))
			{
				var orgCountryData = organisation.CountryDataCollectionForThisCompany.AddNew();

				orgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				orgCountryData.OV_EXApprovalNumber = "ABCXYZ";
				AssertHasError(orgCountryData.OV_EXApprovalNumberInfo, "The Approval Number must be in the following format: 'NNNNN-NN'. For example: 00002-01.");

				orgCountryData.OV_EXApprovalNumber = "17744-50";
				AssertNoErrors(orgCountryData.OV_EXApprovalNumberInfo);

				orgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				orgCountryData.OV_EXApprovalNumber = "ABCXYZ";
				AssertHasError(orgCountryData.OV_EXApprovalNumberInfo, "The Approval Number must match the Document Number for the required document type KCA before marking the organization as approved.");

				orgCountryData.OV_EXApprovalNumber = "ZZZ";
				AssertNoErrors(orgCountryData.OV_EXApprovalNumberInfo);

				orgCountryData.OV_EXApprovalNumber = "";
				AssertNoErrors("Approval number is not required for AC", orgCountryData.OV_EXApprovalNumberInfo);

				orgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.CertifiedHaulier;
				orgCountryData.OV_EXApprovalNumber = "ABCXYZ";
				AssertNoErrors(orgCountryData.OV_EXApprovalNumberInfo);

				orgCountryData.OV_EXApprovalNumber = "ZZZ";
				AssertNoErrors(orgCountryData.OV_EXApprovalNumberInfo);

				orgCountryData.OV_EXApprovalNumber = "";
				AssertNoErrors("Approval number is not required for CH", orgCountryData.OV_EXApprovalNumberInfo);

				orgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.ApprovedHaulier;
				orgCountryData.OV_EXApprovalNumber = "ABCXYZ";
				AssertHasError(orgCountryData.OV_EXApprovalNumberInfo, "The Approval Number must be in the following format: 'NNNNN-NN'. For example: 00002-01.");

				orgCountryData.OV_EXApprovalNumber = "17744-50";
				AssertNoErrors(orgCountryData.OV_EXApprovalNumberInfo);
			}
		}

		public void TestIssuingCountryDefaultLoginCountryCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ES"))
			{
				var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var orgCountryData = organisation.MainAddress.KnownShipperDetails.AddNew();
				orgCountryData.OV_OH_OrgHeader = organisation.PK;
				orgCountryData.OV_OA_ApprovedLocation = organisation.MainAddress.PK;

				using (new RequiredDocument(organisation, "KCA", "12345-67"))
				{
					orgCountryData.OV_EXApprovedOrMajorExporter = "AH";
					AssertEquals("ES",orgCountryData.OV_RN_NKIssuingAuthorityCountry);
				}
			}
		}

		public void TestDuplicateApprovalNumbers()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			org.Addresses.RemoveAndDeleteAll();

			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(false)))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			using (new RequiredDocument(org, "KCA", "44444-55"))
			{
				var address1 = org.Addresses.AddNew();
				address1.Address1 = "Add1";

				var address2 = org.Addresses.AddNew();
				address2.Address1 = "Add2";

				var countryData1 = org.CountryDataCollectionForThisCompany.AddNew();
				countryData1.OV_EXApprovedOrMajorExporter = "AC";
				countryData1.OV_EXApprovalNumber = "44444-55";
				countryData1.OV_OA_ApprovedLocation = address1.PK;

				var countryData2 = org.CountryDataCollectionForThisCompany.AddNew();
				countryData2.OV_EXApprovedOrMajorExporter = "AC";
				countryData2.OV_EXApprovalNumber = "44444-55";
				countryData2.OV_OA_ApprovedLocation = address2.PK;

				AssertNoErrors("Same number can be used for multiple addresses", countryData2.OV_EXApprovalNumberInfo);

				countryData1.OV_EXApprovedOrMajorExporter = "CH";
				countryData2.OV_EXApprovedOrMajorExporter = "CH";

				AssertNoErrors("Same number can be used for multiple addresses", countryData2.OV_EXApprovalNumberInfo);
			}
		}

		public override void TestGetWarningForAdditionalApprovalCodeValidation()
		{
			var org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "DEFRA"));
			var org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "DEHAM"));
			var org3 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "FRPAR"));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var countryData1 = org1.CountryDataCollectionForThisCompany.AddNew();
				countryData1.OV_EXApprovedOrMajorExporter = "KC";
				countryData1.OV_EXApprovalNumber = "12345-00";
				countryData1.OV_OA_ApprovedLocation = org1.MainAddress.PK;
				countryData1.OV_OH_OrgHeader = org1.PK;

				Factory.Save();

				var countryData2 = org2.CountryDataCollectionForThisCompany.AddNew();
				countryData2.OV_EXApprovedOrMajorExporter = "RA";
				countryData2.OV_EXApprovalNumber = "12345-00";
				countryData2.OV_OA_ApprovedLocation = org2.MainAddress.PK;
				countryData2.OV_OH_OrgHeader = org2.PK;

				var expectedWarning = string.Format("The Approval Number must be unique for the same country/region. The following Organizations use this number already:\r\n{0}", org1.HumanReadableName);
				var warning = SupplyChainSecurityConfiguration.GetWarningForAdditionalApprovalCodeValidation(countryData2);
				AssertEquals("Approval code should not be reused for another DE company", expectedWarning, warning);

				countryData2.OV_EXApprovalNumber = "11111-00";
				warning = SupplyChainSecurityConfiguration.GetWarningForAdditionalApprovalCodeValidation(countryData2);
				AssertEquals("No warning for unique Approval Number", ZString.Empty, warning);

				var countryData3 = org3.CountryDataCollectionForThisCompany.AddNew();
				countryData3.OV_EXApprovedOrMajorExporter = "RA";
				countryData3.OV_EXApprovalNumber = "12345-00";
				countryData3.OV_OA_ApprovedLocation = org3.MainAddress.PK;
				countryData3.OV_OH_OrgHeader = org3.PK;

				warning = SupplyChainSecurityConfiguration.GetWarningForAdditionalApprovalCodeValidation(countryData3);
				AssertEquals("Approval code can be reused for a FR company", ZString.Empty, warning);
			}
		}

		public virtual void TestDocumentValidation()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var mainAddress = org.MainAddress;
			mainAddress.OA_RN_NKCountryCode = "BE";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("BE"))
			using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(false)))
			{
				var data = mainAddress.KnownShipperDetails.AddNew();
				data.OV_OH_OrgHeader = org.PK;
				data.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				data.OV_EXApprovalNumber = "KC-H0643-13A";

				data.Validation.ValidateAll();
				AssertNoErrors("Not required for KC", data.OV_EXApprovedOrMajorExporterInfo);
				AssertHasError(data.OV_EXApprovalNumberInfo, "The Approval Number must be in the following format: 'NNNNN-NN'. For example: 00002-01.");
				AssertHasError(data.OV_EXApprovalExpiryDateInfo, "Please enter an Approval Expiry Date.");

				data.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				AssertNoErrors("Document is not required for Regulated Agent", data.OV_EXApprovedOrMajorExporterInfo);

				data.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				AssertNoErrors("Document is not required for Known Consignor", data.OV_EXApprovedOrMajorExporterInfo);

				data.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;

				using (new RequiredDocument(org, "XYZ"))
				{
					data.RunPreSaveValidation();
					AssertHasError(data.OV_EXApprovedOrMajorExporterInfo, "Before flagging this organization as approved, attach a document to eDocs using type \"KCA\" and record the details for the document within the Document Tracking grid.");
					AssertHasError(data.OV_EXApprovalNumberInfo, "The Approval Number must match the Document Number for the required document type KCA before marking the organization as approved.");
					AssertNoErrors("Date is not required", data.OV_EXApprovalExpiryDateInfo);
				}

				using (var requiredDocument = new RequiredDocument(org, "KCA", "22222", "PER"))
				{
					var doc = requiredDocument.JobRequiredDocument;

					data.RunPreSaveValidation();
					AssertNoErrors("The required document is attached.", data.OV_EXApprovedOrMajorExporterInfo);
					AssertHasError(data.OV_EXApprovalNumberInfo, "The Approval Number must match the Document Number for the required document type KCA before marking the organization as approved.");
					AssertNoErrors("Date validation can't yet run", data.OV_EXApprovalExpiryDateInfo);

					doc.EQ_DocType = "KCA";
					doc.EQ_DocNumber = "KC-H0643-13A";
					doc.EQ_DocPeriod = "PER";

					data.RunPreSaveValidation();
					AssertNoErrors("The required document is attached.", data.OV_EXApprovedOrMajorExporterInfo);
					AssertNoErrors("The approval number matches", data.OV_EXApprovalNumberInfo);
					AssertNoErrors("Expiry date is not required", data.OV_EXApprovalExpiryDateInfo);

					using (var requiredDocument2 = new RequiredDocument(org, "KCA", "KC-H0643-13A", "PER", ZDate.Today.AddDays(10)))
					{
						doc = requiredDocument2.JobRequiredDocument;

						data.RunPreSaveValidation();
						AssertNoErrors("The required document is attached.", data.OV_EXApprovedOrMajorExporterInfo);
						AssertNoErrors("The approval number matches", data.OV_EXApprovalNumberInfo);
						AssertNoErrors("Expiry date is not required", data.OV_EXApprovalExpiryDateInfo);
					}
				}
			}
		}

		public override void TestUsesGenericScheme()
		{
			Assert(!SupplyChainSecurityConfiguration.UsesGenericScheme);
		}

		public override void TestIsExportForAviationSecurityPurposes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "FRCDG";
				consol.JK_RL_NKDischargePort = "JMKIN";
				Assert("Consol is export", SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol));

				consol.JK_RL_NKLoadPort = "DEFRA";
				Assert("Consol is export as loads in EU", SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol));

				consol.JK_RL_NKLoadPort = "CNSHA";
				Assert("Consol is not export", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol));

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "PGPOM";
				Assert("Shipment is export", SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));

				shipment.JS_RL_NKOrigin = "ITMIL";
				Assert("Shipment is export as loads in EU", SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));

				shipment.JS_RL_NKOrigin = "USCHI";
				Assert("Shipment is not export", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));
			}
		}

		public void TestIsExportForAviationSecurityPurposes_TranshipmentConsolAttached()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "GBLHR";
				shipment.JS_RL_NKDestination = "PGPOM";
				Assert("Shipment is not export", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "FRCDG";
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
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "PGPOM";
				Assert("Shipment is not export", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "FRCDG";
				Assert("Arrival consol is not export", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol));

				shipment.Consols.Add(consol);
				Assert("Shipment is export as it is attached to an import consol but is not at its final destination", SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));

				consol.JK_RL_NKDischargePort = "GBLHR";
				Assert("Shipment is still an export if it is attached to an import consol for another EU+ country", SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));

				shipment.JS_RL_NKDestination = "FRPAR";
				Assert("Shipment is no longer an export, as it has arrived at destination country", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));

				shipment.JS_RL_NKDestination = "PGPOM";
				consol.JK_RL_NKDischargePort = "AUSYD";
				Assert("Consol is not export", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol));
				Assert("Shipment is no longer export as it is not attached to a consol that goes via the EU", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));
			}
		}

		#region Inspection Status

		public void TestSetApprovedShipperStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "FRPAR"));
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "PGPOM";

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'UNK' for non-approved consignor", "UNK", shipment.JS_InspectionTypeCode);

				consignor.MainAddress.OA_RN_NKCountryCode = "FR";

				var addressCountryData = consignor.MainAddress.KnownShipperDetails.AddNew();
				addressCountryData.OV_OH_OrgHeader = consignor.PK;
				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'APP' for Regulated Agent", "APP", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = ZString.Empty;
				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'APP' for Account Consignor", "APP", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = ZString.Empty;
				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'APP' for Known Consignor", "APP", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = ZString.Empty;
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-1);

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'UNK' for expired approval", "UNK", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = ZString.Empty;
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today;

				Assert("Can set Approved Shipper status", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Boundary condition - Status is set to 'APP' for Known Consignor with approval expiring today", "APP", shipment.JS_InspectionTypeCode);
			}
		}

		public void TestSetApprovedShipperStatus_AccountConsignor()
		{
			var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_EuropeanUnion.Value;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;

			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_EuropeanUnion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("BE"))
			{
				var beAccountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "BE"));
				var frAccountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "FR"));
				var esKnownConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "ES"));

				var beApproval = beAccountConsignor.MainAddress.KnownShipperDetails.AddNew();
				beApproval.OV_EXApprovedOrMajorExporter = "AC";
				beApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);

				var frApproval = frAccountConsignor.MainAddress.KnownShipperDetails.AddNew();
				frApproval.OV_EXApprovedOrMajorExporter = "AC";
				frApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);

				var esApproval = esKnownConsignor.MainAddress.KnownShipperDetails.AddNew();
				esApproval.OV_EXApprovedOrMajorExporter = "KC";
				esApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);
				esApproval.OV_EXApprovalNumber = "75757-57";

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_AirSeaRoad = TransportModes.Air;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "BEBRU";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "NZAKL";

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_RL_NKLoadPort = "BEBRU";
				consol.JK_RL_NKDischargePort = "NZAKL";

				var transport = consol.Transports[0];
				transport.JW_IsLinked = true;
				transport.JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("BEBRU", "NZAKL").PK;
				transport.JW_IsCargoOnly = false;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKDestination = "NZAKL";

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = beAccountConsignor.MainAddress.PK;
				shipment.JS_RL_NKOrigin = "BEBRU";
				AssertEquals("BE Account Consignor cannot ship on Passenger flight", "UNK", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "APP";
				AssertHasErrorContaining(shipment.JS_InspectionTypeCodeInfo, "The following Organizations can only be Approved (APP) to ship on Cargo Only flights. At least one Consol attached to this Shipment is linked to a Passenger flight. Either detach the Shipment from the Consol, select a different flight, or screen your cargo with an Inspection Type approved for Passenger flights.");

				transport.JW_IsCargoOnly = true;
				AssertEquals("BE Account Consignor is approved for Cargo Only BE export", "APP", shipment.JS_InspectionTypeCode);

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = esKnownConsignor.MainAddress.PK;
				shipment.JS_RL_NKOrigin = "BEBRU";
				AssertEquals("ES Known Consignor is approved for BE export", "APP", shipment.JS_InspectionTypeCode);

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = frAccountConsignor.MainAddress.PK;
				shipment.JS_RL_NKOrigin = "BEBRU";
				AssertEquals("FR Account Consignor is not approved for BE export", "UNK", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "PHS";
				AssertNoErrors("FR Account Consignor can ship from BE as cargo is inspected", shipment.JS_InspectionTypeCodeInfo);

				consol.AWBHeader.Populate();
				AssertEquals(1, consol.AWBHeader.AWBSpecialHandlingItems.Count);
				AssertEquals("SPX", consol.AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);
				AssertNoErrors(consol.AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandlingInfo);

				var job = new JobHeader.Loader(shipment).TryCreate();
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GB = GlbBranch.CurrentBranch.PK;

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = beAccountConsignor.MainAddress.PK;
				job.JH_OA_LocalChargesAddr = frAccountConsignor.MainAddress.PK;
				AssertNoErrors("FR Account Consignor can send as local client as cargo is inspected", shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "APP";
				AssertHasErrorContaining(shipment.JS_InspectionTypeCodeInfo, "is an Account Consignor of France so their status is not recognized in Shipment's Origin country of Belgium. Apply an inspection or exemption method.");

				shipment.JS_RL_NKOrigin = "FRPAR";
				AssertEquals("Shipment origin is valid, but first air leg is from BE", "UNK", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "APP";
				AssertHasErrorContaining(shipment.JS_InspectionTypeCodeInfo, "is an Account Consignor of France so their status is not recognized in the first air leg's departure country of Belgium. Apply an inspection or exemption method.");

				shipment.JS_RL_NKOrigin = "BEBRU";
				job.JH_OA_LocalChargesAddr = beAccountConsignor.MainAddress.PK;
				AssertEquals("Shipment origin and first air leg are valid", "APP", shipment.JS_InspectionTypeCode);
				AssertNoErrorContaining(shipment.JS_InspectionTypeCodeInfo, "is an Account Consignor of France so their status is not recognized in Shipment's Origin country of Belgium. Apply an inspection or exemption method.");
				AssertNoErrorContaining(shipment.JS_InspectionTypeCodeInfo, "is an Account Consignor of France so their status is not recognized in the first air leg's departure country of Belgium. Apply an inspection or exemption method.");
			}
		}

		public void TestSetApprovedShipperStatus_ShipmentFromAnotherEuropeanCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("FR"))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				var addressCountryData = consignor.MainAddress.KnownShipperDetails.AddNew();
				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				AssertEquals("Precondition", "UNK", shipment.JS_InspectionTypeCode);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "ITMIL";
				shipment.JS_RL_NKDestination = "DEHAM";

				AssertEquals("Shipment from Italy is APP for France login country", "APP", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "UNK";
				shipment.JS_RL_NKOrigin = "CNSHA";

				AssertEquals("Shipment from China is not APP", "UNK", shipment.JS_InspectionTypeCode);
			}
		}

		public void TestSetApprovedShipperStatus_AccountConsignorShippingFromAnotherCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CH"))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "DEFRA"));

				var addressCountryData = consignor.MainAddress.KnownShipperDetails.AddNew();
				addressCountryData.OV_OH_OrgHeader = consignor.PK;
				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "DEBER";
				shipment.JS_RL_NKDestination = "AUBNE";

				AssertEquals("Shipment is approved for DE Account Consignor shipping from DE", "APP", shipment.JS_InspectionTypeCode);

				shipment.JS_RL_NKOrigin = "ITMIL";

				AssertEquals("Shipment is not approved for DE Account Consignor shipping from IT", "UNK", shipment.JS_InspectionTypeCode);
			}
		}

		public void TestSetApprovedShipperStatus_ActionCalculateInspectionStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.France))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "FRPAR"));

				consignor.MainAddress.OA_RN_NKCountryCode = "FR";

				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "DEHAM";

				AssertEquals("UNK", shipment.JS_InspectionTypeCode);

				var addressCountryData = consignor.MainAddress.KnownShipperDetails.AddNew();
				addressCountryData.OV_OH_OrgHeader = consignor.PK;
				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				AssertEquals("APP", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "XRY";
				Assert("Can calculate Inspection Status when the country of login and country of origin are the same", shipment.SetApprovedShipperStatus("", true));
				AssertEquals("Status is set to 'APP' for Regulated Agent", "APP", shipment.JS_InspectionTypeCode);

				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				shipment.JS_InspectionTypeCode = ZString.Empty;
				shipment.JS_RL_NKDestination = "AUSYD";
				Assert("Can set Approved Shipper status for Air export shipment", shipment.SetApprovedShipperStatus(""));
				AssertEquals("Status is set to 'APP' for Known Consignor", "APP", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "XRY";
				Assert("Can calculate Inspection Status for Air export shipment", shipment.SetApprovedShipperStatus("", true));
				AssertEquals("Status is set to 'APP' for Known Consignor", "APP", shipment.JS_InspectionTypeCode);
			}
		}

		public void TestJS_InspectionTypeValidation_ShipmentFromAnotherEuropeanCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ES"))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "ITMIL";
				shipment.JS_RL_NKDestination = "USCHI";

				AssertEquals("Precondition", "UNK", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "APP";
				AssertHasError("Can't set approved status for unapproved shipper on shipment from Italy when logged into Spain", shipment.JS_InspectionTypeCodeInfo, string.Format(@"The following Organizations are not Approved so an Inspection Type of Approved/Known Shipper is not allowed:
Consignor ({0})", consignor.OH_Code));
			}
		}

		public void TestJS_InspectionTypeValidation_AccountConsignorShippingFromAnotherCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CH"))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "DEFRA"));

				var addressCountryData = consignor.MainAddress.KnownShipperDetails.AddNew();
				addressCountryData.OV_OH_OrgHeader = consignor.PK;
				addressCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				addressCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "ITMIL";
				shipment.JS_RL_NKDestination = "AUBNE";

				AssertEquals("Precondition", "UNK", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "APP";

				AssertHasError("Account consignor sending from wrong country error", shipment.JS_InspectionTypeCodeInfo, string.Format("{0} is an Account Consignor of Germany so their status is not recognized in Shipment's Origin country of Italy. Apply an inspection or exemption method.", consignor.HumanReadableName));
			}
		}

		public void TestAccountConsignorPassengerFlightValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("BE"))
			{
				var beAccountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "BE"));
				var beApproval = beAccountConsignor.MainAddress.KnownShipperDetails.AddNew();
				beApproval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				beApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				var frAccountConsignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, "FR"));
				var frApproval = frAccountConsignor.MainAddress.KnownShipperDetails.AddNew();
				frApproval.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.AccountConsignor;
				frApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				var voyage = Factory.NewWithValidTestData<JobVoyage>();
				voyage.JV_AirSeaRoad = TransportModes.Air;
				voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "BEBRU";
				voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "INDEL";

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_RL_NKLoadPort = "BEBRU";
				consol.JK_RL_NKDischargePort = "INDEL";

				var transport = consol.Transports[0];
				transport.JW_IsLinked = true;
				transport.JW_JX = voyage.Sailings.GetSailingFromLoadAndDischarge("BEBRU", "INDEL").PK;
				transport.JW_IsCargoOnly = false;

				var shipment = consol.Shipments.AddNew();
				shipment.ConsignorDocumentaryAddress.E2_OA_Address = beAccountConsignor.MainAddress.PK;
				shipment.JS_RL_NKOrigin = "BEBRU";
				shipment.JS_RL_NKDestination = "INDEL";
				AssertEquals("Shipment is Unknown", "UNK", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "APP";
				AssertHasErrorContaining(shipment.JS_InspectionTypeCodeInfo, @"The following Organizations can only be Approved (APP) to ship on Cargo Only flights. At least one Consol attached to this Shipment is linked to a Passenger flight. Either detach the Shipment from the Consol, select a different flight, or screen your cargo with an Inspection Type approved for Passenger flights.
Account Consignor - Consignor");

				shipment.JS_InspectionTypeCode = "PHS";
				AssertNoErrors("Account Consignor can send on passenger flight as cargo is inspected", shipment.JS_InspectionTypeCodeInfo);

				consol.AWBHeader.Populate();
				AssertEquals(1, consol.AWBHeader.AWBSpecialHandlingItems.Count);
				AssertEquals("SPX", consol.AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);

				transport.JW_IsCargoOnly = true;
				shipment.SetApprovedShipperStatus("", true);
				shipment.Validation.ValidateJS_InspectionTypeCode();
				AssertEquals("Shipment is Approved", "APP", shipment.JS_InspectionTypeCode);
				AssertNoErrorContaining(shipment.JS_InspectionTypeCodeInfo, @"The following Organizations can only be Approved (APP) to ship on Cargo Only flights. At least one Consol attached to this Shipment is linked to a Passenger flight. Either detach the Shipment from the Consol, select a different flight, or screen your cargo with an Inspection Type approved for Passenger flights.
Account Consignor - Consignor");

				consol.AWBHeader.Populate();
				AssertEquals("SCO", consol.AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);

				consol.AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling = "SPX";
				AssertHasMessageError(consol.AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandlingInfo, "The Special Handling Code of 'SPX - Cargo Secure for Passenger and All-Cargo Aircraft' is invalid as there are shipments attached to the consol which have been received from an Account Consignor and are not permitted on passenger flights.");

				shipment.ConsignorDocumentaryAddress.E2_OA_Address = frAccountConsignor.MainAddress.PK;
				shipment.JS_RL_NKOrigin = "BEBRU";
				AssertEquals("Shipment is Unknown", "UNK", shipment.JS_InspectionTypeCode);

				consol.AWBHeader.Populate();
				AssertEquals("NSC", consol.AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling);

				consol.AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandling = "SPX";
				AssertHasMessageError(consol.AWBHeader.AWBSpecialHandlingItems[0].EP_SpecialHandlingInfo, "The Special Handling Code of 'SPX - Cargo Secure for Passenger and All-Cargo Aircraft' is invalid as there are shipments attached to the consol with Aviation Security Inspection Code 'UNK'.");
			}
		}

		public void TestGetOrgProxyHasRAStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				var shipmentDEAirExport = Factory.NewWithValidTestData<ForwardingShipment>();
				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				shipmentDEAirExport.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipmentDEAirExport.JS_TransportMode = Constants.TransportModes.Air;
				shipmentDEAirExport.JS_RL_NKOrigin = "DEHAM";
				shipmentDEAirExport.JS_RL_NKDestination = "AUSYD";

				var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
				var orgProxyApproval = orgProxy.MainAddress.KnownShipperDetails.AddNew();
				orgProxyApproval.OV_OH_OrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				orgProxyApproval.OV_EXApprovedOrMajorExporter = "KC";
				orgProxyApproval.OV_EXApprovalNumber = "12345-01";
				orgProxyApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);

				Factory.Save();

				var supplyChainSecurityConfiguration = (SupplyChainSecurityConfigurationForTestEU)SupplyChainSecurityConfiguration;

				using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(true, true)))
				{
					Assert("Should not have RA status", !supplyChainSecurityConfiguration.GetOrgProxyHasRAStatus(shipmentDEAirExport));

					orgProxyApproval.OV_EXApprovedOrMajorExporter = "RA";
					Assert("Should have RA status", supplyChainSecurityConfiguration.GetOrgProxyHasRAStatus(shipmentDEAirExport));

					orgProxyApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-100);
					Assert("Should not have RA status becasue of expiration", !supplyChainSecurityConfiguration.GetOrgProxyHasRAStatus(shipmentDEAirExport));

				orgProxyApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);

				var consolDEAirExport = shipmentDEAirExport.Consols.AddNew();
				consolDEAirExport.JK_TransportMode = TransportModes.Air;
				consolDEAirExport.JK_RL_NKLoadPort = "DEHAM";

				Assert("Should have RA status", supplyChainSecurityConfiguration.GetOrgProxyHasRAStatus(consolDEAirExport));

				orgProxyApproval.OV_EXApprovedOrMajorExporter = "KC";
				Assert("Should not have RA status", !supplyChainSecurityConfiguration.GetOrgProxyHasRAStatus(consolDEAirExport));

				orgProxyApproval.OV_EXApprovedOrMajorExporter = "RA";
				consolDEAirExport.Shipments.RemoveAll();
				Assert("Should have RA status", supplyChainSecurityConfiguration.GetOrgProxyHasRAStatus(consolDEAirExport));
				}
			}
		}

		public void TestGetEditInspectionErrorForUncertifiedUserForEuropeanUnionAviationSecurityMembers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var shipmentDEAirExport = Factory.NewWithValidTestData<ForwardingShipment>();
				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				shipmentDEAirExport.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipmentDEAirExport.JS_TransportMode = Constants.TransportModes.Air;
				shipmentDEAirExport.JS_RL_NKOrigin = "DEHAM";
				shipmentDEAirExport.JS_RL_NKDestination = "AUSYD";

				var shipmentSea = Factory.NewWithValidTestData<ForwardingShipment>();
				shipmentSea.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipmentSea.JS_TransportMode = Constants.TransportModes.Sea;

				var shipmentDEAirImport = Factory.NewWithValidTestData<ForwardingShipment>();
				shipmentDEAirImport.JS_TransportMode = Constants.TransportModes.Air;
				shipmentDEAirImport.JS_RL_NKOrigin = "AUSYD";
				shipmentDEAirImport.JS_RL_NKDestination = "DEHAM";

				var branchEU = Factory.NewWithValidTestData<GlbBranch>();
				branchEU.GB_BranchName = "EU Branch";
				branchEU.GB_RL_NKHomePort = "DEHAM";

				var branchAU = Factory.NewWithValidTestData<GlbBranch>();
				branchAU.GB_BranchName = "AU Branch";
				branchAU.GB_RL_NKHomePort = "AUSYD";

				var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
				var orgProxyApproval = orgProxy.MainAddress.KnownShipperDetails.AddNew();
				orgProxyApproval.OV_OH_OrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				orgProxyApproval.OV_EXApprovedOrMajorExporter = "RA";
				orgProxyApproval.OV_EXApprovalNumber = "12345-01";
				orgProxyApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);

				Factory.Save();

				using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(true, true)))
				{
					var expectedError = "Only users with valid BKG and DTA certificate types saved in their Staff Profile may edit this field.";

					GlbStaff.CurrentUser.GS_GB_HomeBranch = branchEU.PK;
					Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = true;

					var certificate1 = GlbStaff.CurrentUser.Certificates.AddNew();
					certificate1.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.BKG;
					certificate1.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(1);
					var certificate2 = GlbStaff.CurrentUser.Certificates.AddNew();
					certificate2.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.DTA;
					certificate2.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(1);

					var error = SupplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(shipmentSea);
					AssertEquals("Shipment is not Air Export from European Union aviation security members", ZString.Empty, error);

					error = SupplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(shipmentDEAirImport);
					AssertEquals("Shipment is not Air Export from European Union aviation security members", ZString.Empty, error);

					GlbStaff.CurrentUser.GS_GB_HomeBranch = branchAU.PK;
					error = SupplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(shipmentDEAirExport);
					AssertEquals("Current user's home port not in European Union aviation security members", ZString.Empty, error);

					GlbStaff.CurrentUser.GS_GB_HomeBranch = branchEU.PK;
					Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = false;

					error = SupplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(shipmentDEAirExport);
					AssertEquals("Current user doesn't have permission to edit", ZString.Empty, error);

					Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = true;

					error = SupplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(shipmentDEAirExport);
					AssertEquals("Current user is certified", ZString.Empty, error);

					certificate1.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(-1);
					error = SupplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(shipmentDEAirExport);
					AssertEquals("Current user's BKG certificate is expired", expectedError, error);

					certificate1.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(1);
					certificate2.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(-1);
					error = SupplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(shipmentDEAirExport);
					AssertEquals("Current user's DTA certificate is expired", expectedError, error);

					GlbStaff.CurrentUser.Certificates.Delete(certificate1);
					error = SupplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(shipmentDEAirExport);
					AssertEquals("Current user doesn't have BKG certificate", expectedError, error);

					GlbStaff.CurrentUser.Certificates.Add(certificate1);
					GlbStaff.CurrentUser.Certificates.Delete(certificate2);
					error = SupplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(shipmentDEAirExport);
					AssertEquals("Current user doesn't have DTA certificate", expectedError, error);
				}

				using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(true, false)))
				{
					GlbStaff.CurrentUser.GS_GB_HomeBranch = branchEU.PK;
					Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = true;

					var error = SupplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(shipmentDEAirExport);
					AssertEquals("Certificate restrictions are not applied", ZString.Empty, error);
				}

				using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(false)))
				{
					GlbStaff.CurrentUser.GS_GB_HomeBranch = branchEU.PK;
					Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed = true;

					var error = SupplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(shipmentDEAirExport);
					AssertEquals("Training restrictions registry is disabled", ZString.Empty, error);
				}
			}
		}

		#endregion

		#region User Authorization

		public override void TestAllowDefaultingCargoSecureWithHighRiskRequirements()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.Transports[0].JW_TransportMode = "AIR";
				consol.Transports[0].JW_RL_NKLoadPort = "DEFRA";
				consol.Transports[0].JW_RL_NKDiscPort = "AUBNE";

				Assert("Can default SHR for DE", SupplyChainSecurityConfiguration.AllowDefaultingCargoSecureForPassengerAndAllCargoAircraft(consol));
			}
		}

		public override void TestSpecialHandlingErrorForUnauthorizedUser()
		{
			const string expectedErrorForSecurityRight = "Ensure you have Security Rights 'Operate > Forwarding > Consolidations > Allow to override Security Status' granted before setting the Consol’s secured status to \"SPX\".";
			const string expectedErrorForCertificates = "Only users with valid BKG and DTA certificate types saved in their Staff Profile may edit this field.";
			const string testCode = "SPX";

			var securityCore = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			securityCore.MaintainConsolAllowToOverrideSecurityStatus.IsAllowed = false;

			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			var orgProxyApproval = orgProxy.MainAddress.KnownShipperDetails.AddNew();
			orgProxyApproval.OV_OH_OrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			orgProxyApproval.OV_EXApprovedOrMajorExporter = "RA";
			orgProxyApproval.OV_EXApprovalNumber = "12345-01";
			orgProxyApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				var branchEU = Factory.NewWithValidTestData<GlbBranch>();
				branchEU.GB_BranchName = "EU Branch";
				branchEU.GB_RL_NKHomePort = "DEHAM";

				var branchAU = Factory.NewWithValidTestData<GlbBranch>();
				branchAU.GB_BranchName = "AU Branch";
				branchAU.GB_RL_NKHomePort = "AUSYD";

				var consolGermanyAirExport = Factory.New<ForwardingConsol>();
				consolGermanyAirExport.JK_TransportMode = TransportModes.Air;
				consolGermanyAirExport.JK_RL_NKLoadPort = "DEHAM";

				var consolAustraliaAirExport = Factory.New<ForwardingConsol>();
				consolAustraliaAirExport.JK_TransportMode = TransportModes.Air;
				consolAustraliaAirExport.JK_RL_NKLoadPort = "AUSYD";
				consolAustraliaAirExport.JK_RL_NKDischargePort = "DEHAM";

				var consolGermanySeaExport = Factory.New<ForwardingConsol>();
				consolGermanySeaExport.JK_TransportMode = TransportModes.Sea;
				consolGermanySeaExport.JK_RL_NKLoadPort = "DEHAM";

				Factory.Save();

				#region European Union aviation security members are Supply Chain Security supported country

				using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(true, true)))
				{
					GlbStaff.CurrentUser.GS_GB_HomeBranch = branchEU.PK;

					var certificate1 = GlbStaff.CurrentUser.Certificates.AddNew();
					certificate1.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.BKG;
					certificate1.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(1);
					var certificate2 = GlbStaff.CurrentUser.Certificates.AddNew();
					certificate2.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.DTA;
					certificate2.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(1);

					var error = SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consolGermanyAirExport, testCode, true);
					AssertEquals("User without security right cannot choose SPX", expectedErrorForSecurityRight, error);

					error = SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consolAustraliaAirExport, testCode, true);
					AssertEquals("No error for different login country and consol's load port", ZString.Empty, error);

					securityCore.MaintainConsolAllowToOverrideSecurityStatus.IsAllowed = true;
					error = SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consolGermanyAirExport, testCode, true);
					AssertEquals("No error for export shipment with a valid security right and staff certified", ZString.Empty, error);

					certificate1.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(-1);
					error = SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consolGermanyAirExport, testCode, true);
					AssertEquals("Current user's BKG certificate is expired", expectedErrorForCertificates, error);

					certificate1.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(1);
					certificate2.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(-1);
					error = SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consolGermanyAirExport, testCode, true);
					AssertEquals("Current user's DTA certificate is expired", expectedErrorForCertificates, error);

					certificate2.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(1);
					GlbStaff.CurrentUser.Certificates.Delete(certificate1);
					error = SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consolGermanyAirExport, testCode, true);
					AssertEquals("Current user doesn't have BKG certificate", expectedErrorForCertificates, error);

					GlbStaff.CurrentUser.Certificates.Add(certificate1);
					GlbStaff.CurrentUser.Certificates.Delete(certificate2);
					error = SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consolGermanyAirExport, testCode, true);
					AssertEquals("Current user doesn't have DTA certificate", expectedErrorForCertificates, error);

					GlbStaff.CurrentUser.Certificates.Add(certificate2);
					error = SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consolGermanyAirExport, testCode, false);
					AssertEquals("No error for code does not has changed", ZString.Empty, error);

					error = SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consolGermanyAirExport, "Code", true);
					AssertEquals("No error for code does not SPX", ZString.Empty, error);

					error = SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consolGermanySeaExport, testCode, true);
					AssertEquals("No error for transport mode is not air", ZString.Empty, error);

					GlbStaff.CurrentUser.GS_GB_HomeBranch = branchAU.PK;
					error = SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consolGermanyAirExport, testCode, true);
					AssertEquals("No error for current user's home port not in EU, CH,IS,LI, or NO", ZString.Empty, error);
				}

				using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(true, false)))
				{
					GlbStaff.CurrentUser.GS_GB_HomeBranch = branchEU.PK;
					securityCore.MaintainConsolAllowToOverrideSecurityStatus.IsAllowed = true;
					var error = SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consolGermanyAirExport, testCode, true);
					AssertEquals("Certificate restrictions are not applied", ZString.Empty, error);
				}

				using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(false)))
				{
					GlbStaff.CurrentUser.GS_GB_HomeBranch = branchEU.PK;
					securityCore.MaintainConsolAllowToOverrideSecurityStatus.IsAllowed = true;
					var error = SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consolGermanyAirExport, testCode, true);
					AssertEquals("Training restrictions registry is disabled", ZString.Empty, error);
				}
				#endregion
			}
		}

		#endregion

		#region High Risk Requirement

		public override void TestIsHighRiskApplicable()
		{
			Assert(SupplyChainSecurityConfiguration.IsHighRiskApplicable);

			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert(!SupplyChainSecurityConfiguration.IsHighRiskApplicable);
			}
		}

		#endregion

		#region Pack Level Screening

		public override void TestIsPackLevelScreeningAvailable()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.France))
			{
				var shipment = SetupTranShipment("NZAKL", "PGPOM", "FRCDG");
				Assert("Available for FR", SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(shipment));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				var shipment = SetupTranShipment("NZAKL", "PGPOM", "DEFRA");
				Assert("Available for DE", SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(shipment));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "PGPOM";
				Assert("Unavailable for NZ", !SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(shipment));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.France))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "FRCDG";
				shipment.JS_RL_NKDestination = "PGPOM";

				Assert("Available for FR", SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(shipment));
			}
		}

		public override void TestJL_InspectionTypeCode_ReadOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.France))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "FRCDG";
				shipment.JS_RL_NKDestination = "PGPOM";

				Assert("Read-only for SEA", SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(shipment));

				shipment.JS_TransportMode = TransportModes.Air;

				Assert("Read/Write for AIR", !SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(shipment));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.PapuaNewGuinea))
			{
				var shipment = SetupTranShipment("DEFRA", "USCHI", "PGPOM");
				Assert("Read/Write for DE-PG-US", !SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(shipment));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.PapuaNewGuinea))
			{
				var shipment = SetupTranShipment("NZAKL", "GBLHR", "PGPOM");
				Assert("Read/Write for NZ-PG-GB", !SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(shipment));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.PapuaNewGuinea))
			{
				var shipment = SetupTranShipment("NZAKL", "FRCDG", "PGPOM");
				Assert("Read/Write for NZ-PG-FR", !SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(shipment));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.PapuaNewGuinea))
			{
				var shipment = SetupTranShipment("DEFRA", "FRCDG", "PGPOM");
				Assert("Read/Write for NZ-PG-FR", !SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(shipment));
			}
		}

		ForwardingShipment SetupTranShipment(ZString origin, ZString destination, ZString tranPort)
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol1.JK_RL_NKLoadPort = origin;
			consol1.JK_RL_NKDischargePort = tranPort;

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol2.JK_RL_NKLoadPort = tranPort;
			consol2.JK_RL_NKDischargePort = destination;

			return shipment;
		}

		#endregion

		public void TestCheckOV_EXApprovedOrMajorExporterAdditionalValidation_AccountConsignor()
		{
			const string expectedError = "Account Consignor cannot be selected for addresses in Germany as it does not participate in the Account Consignor Scheme.";

			AssertEquals("AC is not allowed for DE", expectedError, SupplyChainSecurityConfiguration.GetErrorForApprovalCodeInvalidForCountry("AC", "DE"));
			AssertEquals("AC is allowed for IT", ZString.Empty, SupplyChainSecurityConfiguration.GetErrorForApprovalCodeInvalidForCountry("AC", "IT"));
			AssertEquals("KC is allowed for DE", ZString.Empty, SupplyChainSecurityConfiguration.GetErrorForApprovalCodeInvalidForCountry("KC", "DE"));
		}

		#region AllowAutomaticCalculationOfApprovedStatus

		public override void TestAllowAutomaticCalculationOfApprovedStatus()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.France))
			{
				var shipment = Factory.New<ForwardingShipment>();
				AssertEquals("AllowAutomaticCalculationOfApprovedStatus defaults to true", true, SupplyChainSecurityConfiguration.AllowAutomaticCalculationOfApprovedStatus(shipment));
			}
		}

		#endregion

		#region UseIssuingAuthorityCountry

		public override void TestUseIssuingAuthorityCountry()
		{
			AssertEquals(true, SupplyChainSecurityConfiguration.UseIssuingAuthorityCountry);
		}

		#endregion

		#region GetEditApprovalErrorForUncertifiedUser

		public override void TestGetEditApprovalErrorForUncertifiedUser()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				var org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "DEFRA"));
				var org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "USLAX"));
				var org3 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "AUSYD"));

				var countryData1 = org1.CountryDataCollectionForThisCompany.AddNew();
				countryData1.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				countryData1.OV_EXApprovalNumber = "12345-00";
				countryData1.OV_OA_ApprovedLocation = org1.MainAddress.PK;
				countryData1.OV_OH_OrgHeader = org1.PK;

				var countryData2 = org2.CountryDataCollectionForThisCompany.AddNew();
				countryData2.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				countryData2.OV_EXApprovalNumber = "12345-00";
				countryData2.OV_OA_ApprovedLocation = org2.MainAddress.PK;
				countryData2.OV_OH_OrgHeader = org2.PK;

				Factory.Save();

				using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(true)))
				{
					const string expectedError = "Only users with valid BKG and DTA certificate types saved in their Staff Profile may edit this approval.";

					var error = SupplyChainSecurityConfiguration.GetEditApprovalErrorForUncertifiedUser(countryData1);
					AssertEquals("Data has not changed", ZString.Empty, error);

					countryData1.OV_OA_ApprovedLocation = org2.MainAddress.PK;
					error = SupplyChainSecurityConfiguration.GetEditApprovalErrorForUncertifiedUser(countryData1);
					AssertEquals("ApprovedLocation has changed, and the original location is in European Union aviation security member", expectedError, error);

					countryData2.OV_OA_ApprovedLocation = org3.MainAddress.PK;
					error = SupplyChainSecurityConfiguration.GetEditApprovalErrorForUncertifiedUser(countryData2);
					AssertEquals("ApprovedLocation has changed, and the original location is not in European Union aviation security member", ZString.Empty, error);

					countryData2.OV_OA_ApprovedLocation = org1.MainAddress.PK;
					error = SupplyChainSecurityConfiguration.GetEditApprovalErrorForUncertifiedUser(countryData2);
					AssertEquals("The original location is not in European Union aviation security member, but ApprovedLocation has changed to a location in European Union aviation security member", expectedError, error);

					countryData1.Reload();
					countryData1.OV_EXApprovalNumber = "12345-11";
					error = SupplyChainSecurityConfiguration.GetEditApprovalErrorForUncertifiedUser(countryData1);
					AssertEquals("ApprovalNumber has changed, and the location is in European Union aviation security member", expectedError, error);

					countryData2.Reload();
					countryData2.OV_EXApprovalNumber = "12345-11";
					error = SupplyChainSecurityConfiguration.GetEditApprovalErrorForUncertifiedUser(countryData2);
					AssertEquals("ApprovalNumber has changed, and the location is not in European Union aviation security member", ZString.Empty, error);

					var certificate1 = GlbStaff.CurrentUser.Certificates.AddNew();
					certificate1.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.BKG;
					var certificate2 = GlbStaff.CurrentUser.Certificates.AddNew();
					certificate2.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.DTA;
					error = SupplyChainSecurityConfiguration.GetEditApprovalErrorForUncertifiedUser(countryData1);
					AssertEquals("Current user is certified", ZString.Empty, error);

					GlbStaff.CurrentUser.Certificates.Delete(certificate1);
					GlbStaff.CurrentUser.Certificates.Delete(certificate2);
				}

				using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(false)))
				{
					var error = SupplyChainSecurityConfiguration.GetEditApprovalErrorForUncertifiedUser(countryData1);
					AssertEquals("Training restrictions registry is disabled", ZString.Empty, error);
				}
			}
		}

		#endregion

		public void TestEuropeanUnionAviationSecurityMembers()
		{
			var ports = new List<string>()
			{
				"ATVIE",
				"BEBRU",
				"BGVAR",
				"HR2HZ",
				"CYAKT",
				"CZPRG",
				"DK2DH",
				"EEAAR",
				"FIAAI",
				"FRPAR",
				"DEHAM",
				"GRAGM",
				"HUAND",
				"ISHEA",
				"IEANT",
				"ITROM",
				"LV6LV",
				"LITES",
				"LTBOT",
				"LUBET",
				"MTGRB",
				"NLSAS",
				"NOAAA",
				"PLADC",
				"PT8DA",
				"RODBA",
				"SK2BA",
				"SI9SI",
				"ESBCN",
				"SEABS",
				"CHARF"
			};

			var address = Factory.NewWithValidTestData<OrgAddress>();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "AIR";

			foreach (var port in ports)
			{
				address.ClosestPort = port;
				Assert(((SupplyChainSecurityConfigurationForTestEU)SupplyChainSecurityConfiguration).IsAddressInEuropeanUnionAviationSecurityScheme(address));

				shipment.JS_RL_NKOrigin = port;
				shipment.JS_RL_NKDestination = "AUSYD";
				Assert("Should be true when is air export from an European Union aviation security member", SupplyChainSecurityConfigurationForTestEU.IsAirExportFromEuropeanUnionAviationSecurityMembers(shipment));
			}

			shipment.JS_RL_NKDestination = "DEHAM";
			shipment.JS_RL_NKDestination = "FRPAR";
			Assert("Should not be true when discharge country is an European Union aviation security member", !SupplyChainSecurityConfigurationForTestEU.IsAirExportFromEuropeanUnionAviationSecurityMembers(shipment));

			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			Assert("Should not be true when load country is not an European Union aviation security member", !SupplyChainSecurityConfigurationForTestEU.IsAirExportFromEuropeanUnionAviationSecurityMembers(shipment));

			shipment.JS_RL_NKOrigin = "DEHAM";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = "SEA";
			Assert("Should not be true when is not Air freight", !SupplyChainSecurityConfigurationForTestEU.IsAirExportFromEuropeanUnionAviationSecurityMembers(shipment));
		}

		public override void TestIsAviationSecurityFreightMovementRestricted()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.France))
			{
				var shipmentDEAirExport = Factory.NewWithValidTestData<ForwardingShipment>();
				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());

				shipmentDEAirExport.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipmentDEAirExport.JS_TransportMode = Constants.TransportModes.Air;
				shipmentDEAirExport.JS_RL_NKOrigin = "DEHAM";
				shipmentDEAirExport.JS_RL_NKDestination = "AUSYD";

				var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
				var orgProxyApproval = orgProxy.MainAddress.KnownShipperDetails.AddNew();
				orgProxyApproval.OV_OH_OrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				orgProxyApproval.OV_EXApprovedOrMajorExporter = "RA";
				orgProxyApproval.OV_EXApprovalNumber = "12345-01";
				orgProxyApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);

				var homeBranch = Factory.NewWithValidTestData<GlbBranch>();
				homeBranch.GB_RL_NKHomePort = "FRPAR";
				GlbStaff.CurrentUser.GS_GB_HomeBranch = homeBranch.PK;

				GlbStaff.CurrentUser.Certificates.DeleteAll();

				Factory.Save();

				bool actualResult;

				CombineAssertions(() =>
				{
					using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(true)))
					{
						actualResult = SupplyChainSecurityConfiguration.IsAviationSecurityFreightMovementRestricted(shipmentDEAirExport);
						AssertEquals("User w/o valid BKG and DTA certificates must not be able to run documents when feature is ON for Air Export from European Union aviation security member.", true, actualResult);

						var bkgCertificate = GlbStaff.CurrentUser.Certificates.AddNew();
						bkgCertificate.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.BKG;
						actualResult = SupplyChainSecurityConfiguration.IsAviationSecurityFreightMovementRestricted(shipmentDEAirExport);
						AssertEquals("User w/o valid DTA certificate must not be able to run documents when feature is ON for Air Export from European Union aviation security member.", true, actualResult);

						homeBranch.GB_RL_NKHomePort = "AUSYD";
						Factory.Save();

						actualResult = SupplyChainSecurityConfiguration.IsAviationSecurityFreightMovementRestricted(shipmentDEAirExport);
						AssertEquals("User w/o valid BKG certificate but with home port not within European Union aviation security member must be able to run documents when feature is ON for Air Export from European Union aviation security member.", false, actualResult);

						homeBranch.GB_RL_NKHomePort = "FRPAR";
						Factory.Save();

						GlbStaff.CurrentUser.Certificates.Delete(bkgCertificate);
						var dtaCertificate = GlbStaff.CurrentUser.Certificates.AddNew();
						dtaCertificate.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.DTA;
						actualResult = SupplyChainSecurityConfiguration.IsAviationSecurityFreightMovementRestricted(shipmentDEAirExport);
						AssertEquals("User w/o valid BKG certificate must not be able to run documents when feature is ON for Air Export from European Union aviation security member.", true, actualResult);

						bkgCertificate = GlbStaff.CurrentUser.Certificates.AddNew();
						bkgCertificate.XZ_Type = StaffDefaultCertificateIDAndTrainingTypes.BKG;
						actualResult = SupplyChainSecurityConfiguration.IsAviationSecurityFreightMovementRestricted(shipmentDEAirExport);
						AssertEquals("User w/ valid BKG and DTA certificates must be able to run documents when feature is ON for Air Export from European Union aviation security member.", false, actualResult);

						shipmentDEAirExport.JS_RL_NKDestination = "DEFRA";
						GlbStaff.CurrentUser.Certificates.DeleteAll();
						actualResult = SupplyChainSecurityConfiguration.IsAviationSecurityFreightMovementRestricted(shipmentDEAirExport);
						AssertEquals("User w/o valid BKG and/or DTA certificates must be able to run documents when feature is ON since there is no Air Export from European Union aviation security member.", false, actualResult);
					}

					using (FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AviationSecurityTrainingRestriction(false)))
					{
						GlbStaff.CurrentUser.Certificates.DeleteAll();
						actualResult = SupplyChainSecurityConfiguration.IsAviationSecurityFreightMovementRestricted(shipmentDEAirExport);
						AssertEquals("User w/o valid BKG and/or DTA certificates must be able to run documents when feature is OFF.", false, actualResult);
					}
				});
			}
		}

		public override void TestValidateJS_InspectionType_GovernmentApprovedReliableOrganization()
		{
			foreach (var countryCode in CountryCodes.EuropeanUnionAviationSecurityMembersList)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_TransportMode = "AIR";
					shipment.JS_RL_NKOrigin = $"{countryCode}ABC";
					shipment.JS_RL_NKDestination = "AUBNE";
					shipment.JS_InspectionTypeCode = "GOV";

					AssertNoErrors($"JS_InspectionTypeCode of GOV should be allowed for country {countryCode}", shipment.JS_InspectionTypeCodeInfo);
				}
			}
		}

		public override void TestValidateJS_InspectionType_AdHocMovementsOfCargo()
		{
			foreach (var countryCode in CountryCodes.EuropeanUnionAviationSecurityMembersList)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var shipment = Factory.New<ForwardingShipment>();
					shipment.JS_TransportMode = "AIR";
					shipment.JS_RL_NKOrigin = $"{countryCode}ABC";
					shipment.JS_RL_NKDestination = "AUBNE";
					shipment.JS_InspectionTypeCode = "ADH";

					AssertNoErrors($"JS_InspectionTypeCode of ADH should be allowed for country {countryCode}", shipment.JS_InspectionTypeCodeInfo);
				}
			}
		}

		public void TestValidateJS_InspectionType_WhenOverrideToAppWithNoInvalidCHOrAHSecurity()
		{
			var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_EuropeanUnion.Value;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = "YES";
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany)).ValidationCode = "YES";

			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_EuropeanUnion.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.France))
			{
				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "GLOKWN"));
				var consignorKnownShipperDetails = consignor.MainAddress.KnownShipperDetails.AddNew();
				consignorKnownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.CertifiedHaulier;
				consignorKnownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddYears(2);
				consignorKnownShipperDetails.OV_OH_OrgHeader = consignor.PK;

				var organization = Factory.New<OrgHeader>();
				organization.OH_Code = "FRATRUFRA";

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "FRMRS";
				shipment.JS_RL_NKDestination = "USSEA";
				shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
				shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr  = organization.MainAddress.PK;
				shipment.JS_InspectionTypeCode = "UNK";
				AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "APP";
				AssertEquals(@"The following Organizations are not Approved so an Inspection Type of Approved/Known Shipper is not allowed:
Shipment Pickup Transport Company (FRATRUFRA)", shipment.AviationSecurity.GetErrorsForRelevantOrganisationsWithoutAviationSecurityApproval().First());

				var knownShipperDetails = organization.MainAddress.KnownShipperDetails.AddNew();
				knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.CertifiedHaulier;
				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddYears(2);
				knownShipperDetails.OV_OH_OrgHeader = organization.PK;
				AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);

				knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-5);
				AssertEquals(@"The following Organizations are not Approved so an Inspection Type of Approved/Known Shipper is not allowed:
Shipment Pickup Transport Company (FRATRUFRA)", shipment.AviationSecurity.GetErrorsForRelevantOrganisationsWithoutAviationSecurityApproval().First());
			}
		}

		#region Implementation

		protected override SupplyChainSecurityConfiguration GetNewSupplyChainSecurityConfigurationToTest()
		{
			return new SupplyChainSecurityConfigurationForTestEU();
		}

		class SupplyChainSecurityConfigurationForTestEU : SupplyChainSecurityConfigurationEU
		{
			public new bool IsAddressInEuropeanUnionAviationSecurityScheme(OrgAddress address) => base.IsAddressInEuropeanUnionAviationSecurityScheme(address);

			public new static bool IsAirExportFromEuropeanUnionAviationSecurityMembers(ISupplyChainSecurityImportExportSupporter businessObject) => SupplyChainSecurityConfigurationEU.IsAirExportFromEuropeanUnionAviationSecurityMembers(businessObject);

			public new bool GetOrgProxyHasRAStatus(ForwardingShipment shipment) => base.GetOrgProxyHasRAStatus(shipment);

			public new bool GetOrgProxyHasRAStatus(ForwardingConsol consol) => base.GetOrgProxyHasRAStatus(consol);
		}

		public override ShipmentInspectionTypeRegistryItem GetRegistryItem()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypes_EU;
		}

		#endregion
	}
}
