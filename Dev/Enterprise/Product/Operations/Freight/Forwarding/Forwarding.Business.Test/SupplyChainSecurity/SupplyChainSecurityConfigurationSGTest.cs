using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class SupplyChainSecurityConfigurationSGTest : SupplyChainSecurityConfigurationTest
	{
		#region Pack Level Screening

		public override void TestJL_InspectionTypeCode_ReadOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Singapore))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "SGSIN";
				shipment.JS_RL_NKDestination = "USPHL";
				Assert("JL_InspectionTypeCode should always be read-only when login company is SG", SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(shipment));
			}
		}

		#endregion

		public override void TestInspectionTypeDefault()
		{
			AssertEquals("UNK", SupplyChainSecurityConfiguration.InspectionTypeDefault);

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "PHS"))
			{
				AssertEquals("When login company = SG, we should not default the generic registry value to the Shipment > Inspection field or Packing > Inspection field for SG", "UNK", SupplyChainSecurityConfiguration.InspectionTypeDefault);
			}
		}

		public void TestIsAddressLevelScheme()
		{
			Assert(SupplyChainSecurityConfiguration.IsAddressLevelScheme);
		}

		public override void TestApprovalNumberFormat()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var countryData = org.MainAddress.KnownShipperDetails.AddNew();
				countryData.OV_OH_OrgHeader = org.PK;
				countryData.OV_EXApprovedOrMajorExporter = "RA";
				countryData.OV_EXApprovalNumber = "1234";
				AssertHasError(countryData.OV_EXApprovalNumberInfo, "The Approval Number must be in the following format: 'RCA/nnnn/YYYY'. For example: RCA/0001/2021.");

				countryData.OV_EXApprovalNumber = "RCA/0001/2021";
				AssertNoError(countryData.OV_EXApprovalNumberInfo, "The Approval Number must be in the following format: 'RCA/nnnn/YYYY'. For example: RCA/0001/2021.");
			}
		}

		public override void TestShipmentDateForAviationSecurity()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var testDate1 = ZDateTime.Now.AddDays(1);
				var testDate2 = ZDateTime.Now.AddDays(2);
				var testDate3 = ZDateTime.Now.AddDays(3);
				var testDate4 = ZDateTime.Now.AddDays(4);

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "SGSIN";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_TransportMode = "AIR";

				AssertEquals("Unsaved shipment: date defaults to current date", ZDate.Today, shipment.AviationSecurity.ShipmentDateForAviationSecurity.Date);

				Factory.Save();

				AssertEquals("Created date", shipment.JS_SystemCreateTimeUtc.ToLocalBranchTime(), shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				shipment.JS_E_DEP = testDate1;
				AssertEquals("Departure date", testDate1, shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				var consol1 = shipment.Consols.AddNew();
				consol1.JK_RL_NKLoadPort = "SGSIN";
				consol1.JK_RL_NKDischargePort = "DEHAM";
				consol1.JK_TransportMode = "AIR";
				var transport2 = consol1.Transports.AddNew();
				transport2.JW_ETD = testDate4;

				AssertEquals("No MAWB issue date", testDate1, shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				consol1.JK_MasterBillIssueDate = testDate3;
				AssertEquals("MAWB issue date", testDate3, shipment.AviationSecurity.ShipmentDateForAviationSecurity);
			}
		}

		#region Inspection Status

		public void TestValidateApprovedInspectionTypeRequiresRA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var consignorApproval = consignor.MainAddress.KnownShipperDetails.AddNew();
				consignorApproval.OV_OH_OrgHeader = consignor.PK;
				consignorApproval.OV_EXApprovedOrMajorExporter = "KC";
				consignorApproval.OV_EXApprovalNumber = "1234";
				consignorApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);

				shipment.ConsignorPK = consignor.PK;
				shipment.JS_RL_NKOrigin = "SGSIN";
				shipment.JS_RL_NKDestination = "JPTYO";

				shipment.AviationSecurity.SetApprovedShipperStatus("Test", true);

				AssertEquals("Unknown because login company is not an RA", "UNK", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "APP";
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "Only a Regulated Cargo Agent can handle known consignor cargo, otherwise it is considered unknown. Configure your regulated cargo agent details against the login company's Organization Proxy for Singapore.");

				var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
				var agentApproval = orgProxy.MainAddress.KnownShipperDetails.AddNew();
				agentApproval.OV_OH_OrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				agentApproval.OV_EXApprovedOrMajorExporter = "KC";
				agentApproval.OV_EXApprovalNumber = "5678";
				agentApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);

				shipment.AviationSecurity.SetApprovedShipperStatus("Test", true);

				AssertEquals("Unknown because login company is KC, not RA", "UNK", shipment.JS_InspectionTypeCode);

				shipment.JS_InspectionTypeCode = "APP";
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "Only a Regulated Cargo Agent can handle known consignor cargo, otherwise it is considered unknown. Configure your regulated cargo agent details against the login company's Organization Proxy for Singapore.");

				agentApproval.OV_EXApprovedOrMajorExporter = "RA";

				shipment.AviationSecurity.SetApprovedShipperStatus("Test", true);

				AssertEquals("APP", shipment.JS_InspectionTypeCode);
				AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public override void TestGetAgentApproval()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var forwarder = Factory.NewWithValidTestData<OrgHeader>();
				consol.JK_OA_SendingForwarderAddress = forwarder.MainAddress.PK;

				var approval1 = forwarder.MainAddress.KnownShipperDetails.AddNew();
				approval1.OV_RN_NKClientCountryRelation = "SG";
				approval1.OV_OH_OrgHeader = forwarder.PK;
				approval1.OV_EXApprovedOrMajorExporter = "RA";
				approval1.OV_EXApprovalNumber = "12345";
				approval1.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

				AssertNull("Sending forwarder is not considered to be the agent for SG", SupplyChainSecurityConfiguration.GetAgentApproval(consol));

				var approval2 = GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipperDetails.AddNew();
				approval2.OV_RN_NKClientCountryRelation = "SG";
				approval2.OV_OH_OrgHeader = GlbCompany.CurrentCompany.OrgProxy.PK;
				approval2.OV_EXApprovedOrMajorExporter = "RA";
				approval2.OV_EXApprovalNumber = "55555";

				AssertEquals("Approval number from OrgProxy", "55555", SupplyChainSecurityConfiguration.GetAgentApproval(consol).OV_EXApprovalNumber);

				var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();

				var additionalAddress = branchOrgProxy.Addresses.AddNew();
				additionalAddress.FillWithValidTestData();

				var approval3 = additionalAddress.KnownShipperDetails.AddNew();
				approval3.OV_RN_NKClientCountryRelation = "SG";
				approval3.OV_OH_OrgHeader = branchOrgProxy.PK;
				approval3.OV_EXApprovedOrMajorExporter = "RA";
				approval3.OV_EXApprovalNumber = "56789";

				Factory.Save();

				GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrgProxy.PK;

				AssertEquals("Approval number from Branch OrgProxy - even though it is not setup for the main address", "56789", SupplyChainSecurityConfiguration.GetAgentApproval(consol).OV_EXApprovalNumber);
			}
		}

		public void TestKnownConsignorApprovalIsOrgLevel()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
				var agentApproval = orgProxy.MainAddress.KnownShipperDetails.AddNew();
				agentApproval.OV_OH_OrgHeader = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				agentApproval.OV_EXApprovedOrMajorExporter = "RA";
				agentApproval.OV_EXApprovalNumber = "5678";
				agentApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				shipment.ConsignorPK = consignor.PK;

				shipment.JS_RL_NKOrigin = "SGSIN";
				shipment.JS_RL_NKDestination = "JPTYO";

				shipment.AviationSecurity.SetApprovedShipperStatus("Test", true);

				AssertEquals("Consignor is not approved", "UNK", shipment.JS_InspectionTypeCode);

				var approvedAddress = consignor.Addresses.AddNew();
				approvedAddress.AddAddressType(OrgAddressType.Miscellaneous);

				var consignorApproval = approvedAddress.KnownShipperDetails.AddNew();
				consignorApproval.OV_OH_OrgHeader = consignor.PK;
				consignorApproval.OV_EXApprovedOrMajorExporter = "KC";
				consignorApproval.OV_EXApprovalNumber = "1234";
				consignorApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(100);

				AssertNotEquals("Shipment consignor address is not the approved address", approvedAddress.PK, shipment.ConsignorDocumentaryAddress.E2_OA_Address);
				AssertNotEquals("Shipment pickup address is not the approved address", approvedAddress.PK, shipment.ConsignorPickupAddress.E2_OA_Address);

				shipment.AviationSecurity.SetApprovedShipperStatus("Test", true);

				AssertEquals("Consignor is approved as it has another address with an org level approval", "APP", shipment.JS_InspectionTypeCode);
				AssertNoErrors(shipment.JS_InspectionTypeCodeInfo);
			}
		}

		#endregion

		#region Approval Code Configuration

		public override void TestApprovalCodesList()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "KC", "RA", "NO" }, SupplyChainSecurityConfiguration.ApprovalCodesList.Cast<CodeDescriptionPair>().Select(x => x.Code));
		}

		public override void TestApprovalCodeExpiryDateValidation()
		{
			foreach (var code in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				if (code == AviationSecuritySchemeMembership.Codes.RegulatedAgent)
				{
					Assert("Has expiry date", SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(code));
					AssertEquals("Maximum expiry 3 years", SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears(code), 3);
					AssertEquals(SupplyChainSecurityConfiguration.ApprovalCodeWarningIfApprovalHasExpired(code), "Approval has expired. Regulated Agent must engage an accredited auditor for renewal.");
					AssertEquals(SupplyChainSecurityConfiguration.ApprovalCodeWarningIfApprovalWillLapseInMonths(code), "Expiry date is within 6 months. Regulated Agent must engage an accredited auditor for renewal.");
				}
				else if (code == AviationSecuritySchemeMembership.Codes.KnownConsignor)
				{
					Assert("Has expiry date", SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(code));
					AssertEquals("Maximum expiry 5 years", SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears(code), 5);
					AssertEquals("No warning for pending expiry", SupplyChainSecurityConfiguration.ApprovalCodeWarnIfApprovalWillLapseInMonths(code), 0);
				}
				else
				{
					Assert("Expiry date does not apply", !SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(code));
					AssertEquals("No maximum expiry", SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears(code), 0);
					AssertEquals("No warning for pending expiry", SupplyChainSecurityConfiguration.ApprovalCodeWarnIfApprovalWillLapseInMonths(code), 0);
				}
			}
		}

		public void TestApprovalCodeAddressIsReadOnly()
		{
			Assert("KC address is read-only", SupplyChainSecurityConfiguration.ApprovalCodeAddressIsReadOnly("KC"));
			Assert("RA address is read-only", SupplyChainSecurityConfiguration.ApprovalCodeAddressIsReadOnly("RA"));
		}

		#endregion

		public override void TestGetCountrySpecificShipmentInspectionTypeCollection()
		{
			AssertEquals("Inspection cannot be carried out by forwarders in SG", 0, SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection.Count);
		}

		public override void TestUsesGenericScheme()
		{
			Assert(!SupplyChainSecurityConfiguration.UsesGenericScheme);
		}

		public override void TestValidApprovalCodesForAviationSecurityApproval()
		{
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_SG.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Assert("Yes is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembershipEx.Codes.Yes));
				Assert("No is not approved code", !SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembershipEx.Codes.No));
				Assert("KC is not approved when registry is disabled", !SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.KnownConsignor));
				Assert("RA is not approved when registry is disabled", !SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.RegulatedAgent));
			}

			Assert("Yes is not approved when registry enabled", !SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembershipEx.Codes.Yes));
			Assert("No is not approved code", !SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembershipEx.Codes.No));
			Assert("KC is approved when registry is enabled", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.KnownConsignor));
			Assert("RA is not approved when registry is enabled", !SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.RegulatedAgent));
		}

		public override void TestApprovalCodesWithRequiredDocumentValidation()
		{
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation(AviationSecuritySchemeMembership.Codes.KnownConsignor));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation(AviationSecuritySchemeMembership.Codes.RegulatedAgent));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation(AviationSecuritySchemeMembershipEx.Codes.No));
		}

		[TestDate]
		public void TestExpiryDate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var orgCountryData = GlbCompany.CurrentCompany.OrgProxy.CountryData;

				orgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				orgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-1);
				AssertHasError(orgCountryData.OV_EXApprovalExpiryDateInfo, "The Expiry Date must be in the future.");

				orgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				AssertNoErrors(orgCountryData.OV_EXApprovalExpiryDateInfo);

				orgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddYears(3).AddDays(1);
				AssertHasError(orgCountryData.OV_EXApprovalExpiryDateInfo, "The Expiry Date cannot be more than 3 years in the future.");

				orgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1).AddMonths(6);
				AssertNoErrors(orgCountryData.OV_EXApprovalExpiryDateInfo);
				AssertNoWarnings(orgCountryData.OV_EXApprovalExpiryDateInfo);

				orgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddMonths(6).AddDays(-1);
				AssertHasWarning(orgCountryData.OV_EXApprovalExpiryDateInfo, "Expiry date is within 6 months. Regulated Agent must engage an accredited auditor for renewal.");

				orgCountryData.Factory.Save();

				TestDateAttribute.Date = DateTime.UtcNow.AddMonths(7);

				orgCountryData.Validation.ValidateAll();
				AssertNoErrors(orgCountryData.OV_EXApprovalExpiryDateInfo);
				AssertHasWarning(orgCountryData.OV_EXApprovalExpiryDateInfo, "Approval has expired. Regulated Agent must engage an accredited auditor for renewal.");
			}
		}

		public void TestExpiryDate_KnownConsignor()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var orgCountryData = GlbCompany.CurrentCompany.OrgProxy.CountryData;

				orgCountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
				orgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-1);
				AssertHasError(orgCountryData.OV_EXApprovalExpiryDateInfo, "The Expiry Date must be in the future.");

				orgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddYears(5).AddDays(1);
				AssertHasError(orgCountryData.OV_EXApprovalExpiryDateInfo, "The Expiry Date cannot be more than 5 years in the future.");

				orgCountryData.OV_EXApprovalExpiryDate = ZDate.Today.AddYears(5).AddDays(-1);
				AssertNoErrors(orgCountryData.OV_EXApprovalExpiryDateInfo);

				orgCountryData.OV_EXApprovalExpiryDate = ZDate.Empty;
				AssertHasError(orgCountryData.OV_EXApprovalExpiryDateInfo, "Please enter an Approval Expiry Date.");
			}
		}

		public void TestSendingAgent_KnownConsignor_WhenAnyOfRevalantAddressIsKnow()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_AgentType = AgentType.Agent;
				consol.JK_RL_NKLoadPort = "SGSIN";
				consol.Shipments.AddNew().JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;
				var forwarder = Factory.NewWithValidTestData<OrgHeader>();
				consol.JK_OA_SendingForwarderAddress = forwarder.MainAddress.PK;

				var approval1 = forwarder.MainAddress.KnownShipperDetails.AddNew();
				approval1.OV_RN_NKClientCountryRelation = "SG";
				approval1.OV_EXApprovedOrMajorExporter = "RA";
				approval1.OV_EXApprovalNumber = "12345";
				approval1.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				AssertEquals("RCAR-KC", SupplyChainSecurityConfiguration.GetMAWBKnownConsignorCode(consol));

				approval1.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-1);
				AssertEquals("RCAR-UC", SupplyChainSecurityConfiguration.GetMAWBKnownConsignorCode(consol));

				var approval2 = forwarder.Addresses.AddNew().KnownShipperDetails.AddNew();
				approval2.OV_RN_NKClientCountryRelation = "SG";
				approval2.OV_EXApprovedOrMajorExporter = "RA";
				approval2.OV_EXApprovalNumber = "23456";
				approval2.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				AssertEquals("RCAR-KC", SupplyChainSecurityConfiguration.GetMAWBKnownConsignorCode(consol));
			}
		}

		#region Consignment Security Declaration

		public override void TestAllowIncludeECSD()
		{
			AssertEquals("Should not allow include eCSD", false, SupplyChainSecurityConfiguration.AllowIncludeECSD);
		}

		#endregion

		public override void TestValidateJS_InspectionType_GovernmentApprovedReliableOrganization()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"SGSIN";
				shipment.JS_RL_NKDestination = "JPTYO";
				shipment.JS_InspectionTypeCode = "GOV";

				AssertHasError($"JS_InspectionTypeCode of GOV should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of GOV Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (b) Government approved reliable organization(s) can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		public override void TestValidateJS_InspectionType_AdHocMovementsOfCargo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("SG"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"SGSIN";
				shipment.JS_RL_NKDestination = "JPTYO";
				shipment.JS_InspectionTypeCode = "ADH";

				AssertHasError($"JS_InspectionTypeCode of ADH should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of ADH Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (c) ad hoc movements of cargo can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		#region Implementation

		protected override SupplyChainSecurityConfiguration GetNewSupplyChainSecurityConfigurationToTest()
		{
			return new SupplyChainSecurityConfigurationSG();
		}

		#endregion
	}
}
