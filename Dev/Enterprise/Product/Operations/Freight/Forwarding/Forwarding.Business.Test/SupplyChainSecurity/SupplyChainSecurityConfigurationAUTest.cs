using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	class SupplyChainSecurityConfigurationAUTest : SupplyChainSecurityConfigurationTest
	{
		public override void TestOrganisationsToUseForAviationSecurity()
		{
			AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.Consignor].ValidationCode);
			AssertEquals(1, SupplyChainSecurityConfiguration.OrganisationsToUse.Values.Count);

			var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_Australia.Value;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPackingCFS)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolTransportCompany)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolAirline)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;

			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_Australia.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
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

		public override void TestShipmentDateForAviationSecurity()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var testDate1 = ZDateTime.Now.AddDays(1);
				var testDate2 = ZDateTime.Now.AddDays(2);
				var testDate3 = ZDateTime.Now.AddDays(3);
				var testDate4 = ZDateTime.Now.AddDays(4);

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_TransportMode = "AIR";

				AssertEquals("Unsaved shipment: date defaults to current date", ZDate.Today, shipment.AviationSecurity.ShipmentDateForAviationSecurity.Date);

				Factory.Save();

				AssertEquals("Created date", shipment.JS_SystemCreateTimeUtc.ToLocalBranchTime(), shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				shipment.JS_E_DEP = testDate1;
				AssertEquals("Departure date", testDate1, shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				var consol1 = shipment.Consols.AddNew();
				consol1.JK_RL_NKLoadPort = "AUSYD";
				consol1.JK_RL_NKDischargePort = "AUBNE";
				consol1.JK_TransportMode = "ROA";
				var transport1 = consol1.Transports.AddNew();
				transport1.JW_ETD = testDate2;

				var consol2 = shipment.Consols.AddNew();
				consol2.JK_RL_NKLoadPort = "AUBNE";
				consol2.JK_RL_NKDischargePort = "DEHAM";
				consol2.JK_TransportMode = "AIR";
				var transport2 = consol2.Transports.AddNew();
				transport2.JW_ETD = testDate4;

				AssertEquals("No MAWB issue date", testDate1, shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				consol2.JK_MasterBillIssueDate = testDate3;
				AssertEquals("MAWB issue date", testDate3, shipment.AviationSecurity.ShipmentDateForAviationSecurity);
			}
		}

		public override void TestInspectionTypeDefault()
		{
			AssertEquals("UNK", SupplyChainSecurityConfiguration.InspectionTypeDefault);

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault_Australia.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			{
				AssertEquals("", SupplyChainSecurityConfiguration.InspectionTypeDefault);
			}
		}

		#region Approval Code Configuration

		public override void TestApprovalCodesList()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "RC", "KC", "RA", "RE", "AA", "NO" }, SupplyChainSecurityConfiguration.ApprovalCodesList.Cast<CodeDescriptionPair>().Select(x => x.Code));

			AssertEquals("RACA - Regulated Air Cargo Agent (EACE Notice)", SupplyChainSecurityConfiguration.ApprovalCodesList["RE"].Description);
			AssertEquals("RACA - Regulated Air Cargo Agent (ACE Notice)", SupplyChainSecurityConfiguration.ApprovalCodesList["RA"].Description);
			AssertEquals("AACA - Accredited Air Cargo Agent", SupplyChainSecurityConfiguration.ApprovalCodesList["AA"].Description);
			AssertEquals("Known Consignor", SupplyChainSecurityConfiguration.ApprovalCodesList["KC"].Description);
			AssertEquals("Regular Customer", SupplyChainSecurityConfiguration.ApprovalCodesList["RC"].Description);
		}

		public override void TestApprovalCodesWithRequiredDocumentValidation()
		{
			foreach (var code in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				Assert(!SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation(code));
			}
		}

		public override void TestApprovalCodeExpiryDateValidation()
		{
			foreach (var code in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				if (code == AviationSecuritySchemeMembership.Codes.RegulatedAgent
					|| code == AviationSecuritySchemeMembership.Codes.RegulatedAgentEACE
					|| code == AviationSecuritySchemeMembership.Codes.AccreditedAirCargoAgent
					|| code == AviationSecuritySchemeMembership.Codes.KnownConsignor)
				{
					Assert("Has expiry date", SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(code));
					AssertEquals("Maximum expiry", SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears(code), 5);
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

		#endregion

		public override void TestGetCountrySpecificShipmentInspectionTypeCollection()
		{
			var inspectionTypes = new ShipmentInspectionTypes();
			inspectionTypes.Types.Add("ABC", (NoResString)"ABC", false, false);
			inspectionTypes.Types.Add("QQQ", (NoResString)"QQQ", false, false);

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_AU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var securityEnabledList = SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection;

					AssertContainsExactElementsInAnyOrder(
						new[] { "XRY", "CMD", "ETD", "PHS", "BIO", "DIP", "LFS", "MAI", "NUC" },
						securityEnabledList.Cast<ShipmentInspectionType>().Select(x => x.Code));
				}

				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_AU.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var securityDisabledList = SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection;

					AssertContainsExactElementsInAnyOrder(
						new[] { "ABC", "QQQ" },
						securityDisabledList.Cast<ShipmentInspectionType>().Select(x => x.Code));
				}
			}
		}

		public void TestValidateJS_InspectionType_RA_ImpliedByCurrentCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
				var orgProxyApproval = orgProxy.MainAddress.KnownShipperDetails.AddNew();
				orgProxyApproval.OV_OH_OrgHeader = orgProxy.PK;
				orgProxyApproval.OV_EXApprovedOrMajorExporter = "RA";
				orgProxyApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(10);
				orgProxyApproval.Factory.Save();

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "JPTYO";

				shipment.JS_InspectionTypeCode = "XRY";
				AssertHasWarning("XRY", shipment.JS_InspectionTypeCodeInfo, "Your login branch/company is a RACA so you can apply a screened status. When a Consolidation is added to this Shipment, the Sending Agent's address will be checked to ensure it is an approved facility. If not, this warning will become an error.");

				shipment.JS_InspectionTypeCode = "APP";
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "This Consignor is not Approved so an Inspection Type of Approved/Known Shipper is not allowed.");

				orgProxyApproval.OV_EXApprovedOrMajorExporter = "RE";
				orgProxyApproval.Factory.Save();

				shipment.JS_InspectionTypeCode = "PHS";
				AssertHasWarning("PHS", shipment.JS_InspectionTypeCodeInfo, "Your login branch/company is a RACA so you can apply a screened status. When a Consolidation is added to this Shipment, the Sending Agent's address will be checked to ensure it is an approved facility. If not, this warning will become an error.");

				shipment.JS_InspectionTypeCode = "DIP";
				AssertHasWarning("DIP", shipment.JS_InspectionTypeCodeInfo, "Your login branch/company is a RACA so you can apply a screened status. When a Consolidation is added to this Shipment, the Sending Agent's address will be checked to ensure it is an approved facility. If not, this warning will become an error.");

				shipment.JS_InspectionTypeCode = "UNK";
				AssertNoNotifications("UNK", shipment.JS_InspectionTypeCodeInfo);

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, orgProxy.PK));
				var consignorApproval = consignor.MainAddress.KnownShipperDetails.AddNew();
				consignorApproval.OV_OH_OrgHeader = consignor.PK;
				consignorApproval.OV_EXApprovedOrMajorExporter = "KC";
				consignorApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(10);

				shipment.ConsignorPK = consignor.PK;
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_InspectionTypeCode = "APP";

				AssertNoErrors("APP is allowed", shipment.JS_InspectionTypeCodeInfo);
				AssertHasWarning("APP", shipment.JS_InspectionTypeCodeInfo, "Your login branch/company is a RACA so you can apply a screened status. When a Consolidation is added to this Shipment, the Sending Agent's address will be checked to ensure it is an approved facility. If not, this warning will become an error.");
			}
		}

		public void TestValidateJS_InspectionType_RA_FromCurrentBranch()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;

				AssertNull("Precondition", GlbBranch.CurrentBranch.OrgProxy);
				AssertNull("Precondition", GlbBranch.CurrentBranch.AddressProxy);

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "JPTYO";

				shipment.JS_InspectionTypeCode = "MAI";
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of MAI cannot be used as the login Branch and Company Organization Proxy do not have a valid RACA Known/Approval status within their Organization > Supply Chain Security tab.");

				GlbBranch.CurrentBranch.GB_OH_OrgProxy = orgProxy.PK;
				var orgProxyMainAddress = orgProxy.MainAddress;

				shipment.JS_InspectionTypeCode = "DIP";
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of DIP cannot be used as the login Branch and Company Organization Proxy do not have a valid RACA Known/Approval status within their Organization > Supply Chain Security tab.");

				var orgProxyMainAddressApproval = orgProxyMainAddress.KnownShipperDetails.AddNew();
				orgProxyMainAddressApproval.OV_OH_OrgHeader = orgProxyMainAddress.OA_OH;
				orgProxyMainAddressApproval.OV_EXApprovedOrMajorExporter = "RA";
				orgProxyMainAddressApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(10);
				orgProxyMainAddressApproval.Factory.Save();

				shipment.JS_InspectionTypeCode = "XRY";
				AssertHasWarning("XRY", shipment.JS_InspectionTypeCodeInfo, "Your login branch/company is a RACA so you can apply a screened status. When a Consolidation is added to this Shipment, the Sending Agent's address will be checked to ensure it is an approved facility. If not, this warning will become an error.");

				orgProxyMainAddressApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-10);
				shipment.JS_InspectionTypeCode = "DIP";
				AssertHasError(shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of DIP cannot be used as the login Branch and Company Organization Proxy do not have a valid RACA Known/Approval status within their Organization > Supply Chain Security tab.");

				var anotherOrgProxyAddress = Factory.NewWithValidTestData<OrgAddress>();
				anotherOrgProxyAddress.OA_OH = orgProxy.PK;
				var anotherOrgProxyAddressApproval = anotherOrgProxyAddress.KnownShipperDetails.AddNew();
				anotherOrgProxyAddressApproval.OV_OH_OrgHeader = orgProxy.PK;
				anotherOrgProxyAddressApproval.OV_EXApprovedOrMajorExporter = "RA";
				anotherOrgProxyAddressApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(10);
				anotherOrgProxyAddressApproval.Factory.Save();

				shipment.JS_InspectionTypeCode = "XRY";
				AssertHasWarning("XRY", shipment.JS_InspectionTypeCodeInfo, "Your login branch/company is a RACA so you can apply a screened status. When a Consolidation is added to this Shipment, the Sending Agent's address will be checked to ensure it is an approved facility. If not, this warning will become an error.");

				var consol = shipment.Consols.AddNew();
				shipment.JS_InspectionTypeCode = "PHS";
				AssertHasError("XRY", shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of PHS cannot be used as the Consol Sending Agent does not have a RACA Known/Approved status. Ensure the Sending Agent's Organization > Supply Chain Security tab has valid RACA details.");

				consol.JK_OA_SendingForwarderAddress = orgProxyMainAddress.PK;
				shipment.JS_InspectionTypeCode = "NUC";
				AssertNoNotifications("Branch Address has valid approval", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestValidateJS_InspectionType_RA_FromSendingAgent()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var sendingAgent = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var sendingAgentApproval = sendingAgent.MainAddress.KnownShipperDetails.AddNew();
				sendingAgentApproval.OV_OH_OrgHeader = sendingAgent.PK;
				sendingAgentApproval.OV_EXApprovedOrMajorExporter = "RA";
				sendingAgentApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(10);
				sendingAgentApproval.Factory.Save();

				var shipment = Factory.New<ForwardingShipment>();
				var consol = shipment.Consols.AddNew();

				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "JPTYO";

				shipment.JS_InspectionTypeCode = "UNK";
				AssertNoErrors("Precondition", shipment.JS_InspectionTypeCodeInfo);

				shipment.JS_InspectionTypeCode = "XRY";
				AssertHasError("Consol Sending Agent not specified", shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of XRY cannot be used as the Consol Sending Agent does not have a RACA Known/Approved status. Ensure the Sending Agent's Organization > Supply Chain Security tab has valid RACA details.");

				consol.JK_OA_SendingForwarderAddress = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, SQLComparisonOperator.NotEqual, sendingAgent.PK)).PK;
				shipment.Validation.ValidateJS_InspectionTypeCode();
				AssertHasError("Sending agent is not approved", shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of XRY cannot be used as the Consol Sending Agent does not have a RACA Known/Approved status. Ensure the Sending Agent's Organization > Supply Chain Security tab has valid RACA details.");

				var anotherAddress = sendingAgent.Addresses.AddNew();
				anotherAddress.FillWithValidTestData();
				anotherAddress.Factory.Save();

				consol.JK_OA_SendingForwarderAddress = anotherAddress.PK;
				shipment.Validation.ValidateJS_InspectionTypeCode();
				AssertHasError("The address selected is not the one with RACA approval", shipment.JS_InspectionTypeCodeInfo, "The Consol Sending Agent's address does not match the address of the RACA approval.");

				consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
				shipment.Validation.ValidateJS_InspectionTypeCode();
				AssertNoErrors("Addresss has approval", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestValidateJS_InspectionType_AACA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
				var orgProxyApproval = orgProxy.MainAddress.KnownShipperDetails.AddNew();
				orgProxyApproval.OV_OH_OrgHeader = orgProxy.PK;
				orgProxyApproval.OV_EXApprovedOrMajorExporter = "AA";
				orgProxyApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(10);
				orgProxyApproval.Factory.Save();

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "JPTYO";

				shipment.JS_InspectionTypeCode = "XRY";
				AssertHasError("XRY", shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of XRY cannot be used as the Consol Sending Agent does not have a RACA Known/Approved status. Ensure the Sending Agent's Organization > Supply Chain Security tab has valid RACA details.");

				shipment.JS_InspectionTypeCode = "MAI";
				AssertNoErrors("Exemption codes are allowed", shipment.JS_InspectionTypeCodeInfo);

				var consol = shipment.Consols.AddNew();
				consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

				shipment.JS_InspectionTypeCode = "PHS";
				AssertHasError("Inspection is still not allowed", shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of PHS cannot be used as the Consol Sending Agent does not have a RACA Known/Approved status. Ensure the Sending Agent's Organization > Supply Chain Security tab has valid RACA details.");

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, orgProxy.PK));
				var consignorApproval = consignor.MainAddress.KnownShipperDetails.AddNew();
				consignorApproval.OV_OH_OrgHeader = consignor.PK;
				consignorApproval.OV_EXApprovedOrMajorExporter = "KC";
				consignorApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(10);

				shipment.ConsignorPK = consignor.PK;
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_InspectionTypeCode = "APP";
				AssertNoErrors("APP is allowed", shipment.JS_InspectionTypeCodeInfo);
			}
		}

		public void TestValidateJS_InspectionType_NotApproved()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_RL_NKDestination = "JPTYO";

				shipment.JS_InspectionTypeCode = "XRY";
				AssertHasError("Inspection codes are not allowed", shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of XRY cannot be used as the login Branch and Company Organization Proxy do not have a valid RACA Known/Approval status within their Organization > Supply Chain Security tab.");

				shipment.JS_InspectionTypeCode = "MAI";
				AssertHasError("Exemption codes are not allowed", shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of MAI cannot be used as the login Branch and Company Organization Proxy do not have a valid RACA Known/Approval status within their Organization > Supply Chain Security tab.");

				var consol = shipment.Consols.AddNew();
				consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

				shipment.JS_InspectionTypeCode = "PHS";
				AssertHasError("Inspection is still not allowed", shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of PHS cannot be used as the Consol Sending Agent does not have a RACA Known/Approved status. Ensure the Sending Agent's Organization > Supply Chain Security tab has valid RACA details.");

				var consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var consignorApproval = consignor.MainAddress.KnownShipperDetails.AddNew();
				consignorApproval.OV_OH_OrgHeader = consignor.PK;
				consignorApproval.OV_EXApprovedOrMajorExporter = "KC";
				consignorApproval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(10);

				shipment.ConsignorPK = consignor.PK;
				shipment.JS_RL_NKOrigin = "AUBNE";
				shipment.JS_InspectionTypeCode = "APP";
				AssertHasError("APP is not allowed - even for Known Consignor", shipment.JS_InspectionTypeCodeInfo, "The Inspection Type of APP cannot be used as the Consol Sending Agent does not have a RACA Known/Approved status. Ensure the Sending Agent's Organization > Supply Chain Security tab has valid RACA details.");
			}
		}

		public void TestIsLicensedModule()
		{
			Assert(SupplyChainSecurityConfiguration.IsLicensedModule);
		}

		public override void TestUsesGenericScheme()
		{
			Assert(!SupplyChainSecurityConfiguration.UsesGenericScheme);
		}

		public override void TestValidApprovalCodesForAviationSecurityApproval()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				Assert("KC is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.KnownConsignor));
				Assert("RA is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.RegulatedAgent));
				Assert("RE is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.RegulatedAgentEACE));
				Assert("AA is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.AccreditedAirCargoAgent));

				Assert("RC is not approved code", !SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.RegularCustomer));
				Assert("No is not approved code", !SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembershipEx.Codes.No));
			}
		}

		public override void TestCheckEH_AgentApprovalNumberAdditionalValidation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = "AUBNE";
				consol.JK_RL_NKDischargePort = "CNSHA";

				var header = consol.AWBHeader;
				header.EH_AgentApprovalNumber = "88888-88";
				AssertHasMessageError(header.EH_AgentApprovalNumberInfo, "A Security Declaration cannot be issued for this consignment because the Sending Agent does not match the login (issuing) company.");

				var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
				consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

				AssertHasMessageError(header.EH_AgentApprovalNumberInfo, "Issuing Sending Agent does not have a regulated status for air cargo security.");

				var approval = sendingAgent.MainAddress.KnownShipperDetails.AddNew();
				approval.OV_OH_OrgHeader = sendingAgent.PK;
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				approval.OV_EXApprovedOrMajorExporter = "AA";
				approval.Factory.Save();

				header.Validation.ValidateEH_AgentApprovalNumber();
				AssertHasMessageError(header.EH_AgentApprovalNumberInfo, "The Sending Agent is an AACA so cannot clear cargo.");

				approval.OV_EXApprovedOrMajorExporter = "RE";
				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(-1);
				approval.Factory.Save();

				header.Validation.ValidateEH_AgentApprovalNumber();
				AssertHasMessageError(header.EH_AgentApprovalNumberInfo, "Issuing Sending Agent does not have a regulated status for air cargo security.");

				approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
				approval.Factory.Save();

				header.Validation.ValidateEH_AgentApprovalNumber();
				AssertNoMessageErrors(header.EH_AgentApprovalNumberInfo);
			}
		}

		public override void TestApprovalNumberFormat()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var countryData = org.MainAddress.KnownShipperDetails.AddNew();
				countryData.OV_OH_OrgHeader = org.PK;

				foreach (var code in new[] { "AA", "RA", "RE" })
				{
					countryData.OV_EXApprovedOrMajorExporter = code;
					countryData.OV_EXApprovalNumber = "1234";
					AssertHasError(countryData.OV_EXApprovalNumberInfo, "The Approval Number must be in the following format: 'NNNNN-NN'. For example: 00002-01.");

					countryData.OV_EXApprovalNumber = "98765-11";
					AssertNoError(countryData.OV_EXApprovalNumberInfo, "The Approval Number must be in the following format: 'NNNNN-NN'. For example: 00002-01.");
				}
			}
		}

		public void TestAddressDefaulting()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
				org.Addresses.RemoveAndDeleteAll();

				var officeAddress = org.Addresses.AddNew();
				officeAddress.AddAddressType(OrgAddressType.Office);
				officeAddress.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);
				officeAddress.Address1 = "Office";

				var pickupAddress = org.Addresses.AddNew();
				pickupAddress.AddAddressType(OrgAddressType.Pickup);
				pickupAddress.Address1 = "Pickup";

				var deliveryAddress = org.Addresses.AddNew();
				deliveryAddress.AddAddressType(OrgAddressType.Delivery);
				deliveryAddress.Address1 = "Delivery";

				var countryData = org.CountryDataCollectionForThisCompany.AddNew();
				countryData.OV_EXApprovedOrMajorExporter = "AA";
				AssertEquals("Main address defaults for AA", officeAddress.PK, countryData.OV_OA_ApprovedLocation);

				countryData.OV_OA_ApprovedLocation = ZGuid.Empty;
				countryData.OV_EXApprovedOrMajorExporter = "RA";
				AssertEquals("Main address defaults for RA", officeAddress.PK, countryData.OV_OA_ApprovedLocation);

				countryData.OV_OA_ApprovedLocation = ZGuid.Empty;
				countryData.OV_EXApprovedOrMajorExporter = "RE";
				AssertEquals("Main address defaults for RE", officeAddress.PK, countryData.OV_OA_ApprovedLocation);

				countryData.OV_OA_ApprovedLocation = ZGuid.Empty;
				countryData.OV_EXApprovedOrMajorExporter = "KC";
				AssertEquals("No address defaults for KC", ZGuid.Empty, countryData.OV_OA_ApprovedLocation);

				countryData.OV_EXApprovedOrMajorExporter = "RC";
				AssertEquals("No address defaults for RC", ZGuid.Empty, countryData.OV_OA_ApprovedLocation);
			}
		}

		#region Prohibited Routing

		public override void TestGetWarningForProhibitedRouting()
		{
			var expectedWarningForBangladesh = @"The Australian Government has imposed prohibitions on the carriage of air cargo that has originated from, or transited through, Bangladesh, unless it has undergone security examination at an approved last port of call before traveling to Australia, or is otherwise exempt from examination under Australian regulations. You are required to meet with government requirements and/or consider a change of transport mode.

Approved last ports of call are Abu Dhabi; Bangkok; Doha; Dubai; Guangzhou; Hong Kong; Kuala Lumpur or Singapore.

Approved examination methods are X-ray; explosive trace detection; or physical examination.";

			var expectedWarningForEgypt = "The Australian Government has imposed prohibitions on the carriage of air cargo that has originated from, or transited through Egypt, except for items that are currently exempt from screening under Australian Regulations, such as diplomatic bags and smaller items of international mail. You are required to meet with government requirements and/or consider a change of transport mode.";

			var expectedWarningForSomaliaSyriaAndYemen = "The Australian Government has imposed prohibitions on the carriage of air cargo that has originated from, or transited through, Syria, Yemen, and Somalia. You are required to meet with government requirements and/or consider a change of transport mode.";

			var expectedWarningForTurkey = "The Australian Government has imposed prohibitions on air cargo that has originated from, or transited through, Turkey. However, this prohibition applies only to electromechanical devices that weigh over 1 kilogram. You are required to meet with government requirements and/or consider a change of transport mode.";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "BDCGP";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_TransportMode = "AIR";
			AssertHasWarning(shipment.JS_RL_NKOriginInfo, expectedWarningForBangladesh);

			shipment.JS_RL_NKOrigin = "EGCAI";
			AssertHasWarning(shipment.JS_RL_NKOriginInfo, expectedWarningForEgypt);

			shipment.JS_RL_NKOrigin = "SOGSR";
			AssertHasWarning(shipment.JS_RL_NKOriginInfo, expectedWarningForSomaliaSyriaAndYemen);

			shipment.JS_RL_NKOrigin = "SYPMS";
			AssertHasWarning(shipment.JS_RL_NKOriginInfo, expectedWarningForSomaliaSyriaAndYemen);

			shipment.JS_RL_NKOrigin = "YEHOD";
			AssertHasWarning(shipment.JS_RL_NKOriginInfo, expectedWarningForSomaliaSyriaAndYemen);

			shipment.JS_RL_NKOrigin = "TREDO";
			AssertHasWarning(shipment.JS_RL_NKOriginInfo, expectedWarningForTurkey);

			shipment.JS_RL_NKOrigin = "USCHI";
			AssertNoWarning(shipment.JS_RL_NKOriginInfo, expectedWarningForBangladesh);
			AssertNoWarning(shipment.JS_RL_NKOriginInfo, expectedWarningForEgypt);
			AssertNoWarning(shipment.JS_RL_NKOriginInfo, expectedWarningForSomaliaSyriaAndYemen);
			AssertNoWarning(shipment.JS_RL_NKOriginInfo, expectedWarningForTurkey);
		}

		#endregion

		public override void TestValidateJS_InspectionType_GovernmentApprovedReliableOrganization()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"AUMEL";
				shipment.JS_RL_NKDestination = "JPTYO";
				shipment.JS_InspectionTypeCode = "GOV";

				AssertHasError($"JS_InspectionTypeCode of GOV should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of GOV Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (b) Government approved reliable organization(s) can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		public override void TestValidateJS_InspectionType_AdHocMovementsOfCargo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"AUMEL";
				shipment.JS_RL_NKDestination = "JPTYO";
				shipment.JS_InspectionTypeCode = "ADH";

				AssertHasError($"JS_InspectionTypeCode of ADH should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of ADH Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (c) ad hoc movements of cargo can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		#region Implementation

		protected override SupplyChainSecurityConfiguration GetNewSupplyChainSecurityConfigurationToTest()
		{
			return new SupplyChainSecurityConfigurationAU();
		}

		public override ShipmentInspectionTypeRegistryItem GetRegistryItem()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypes_Australia;
		}

		#endregion
	}
}
