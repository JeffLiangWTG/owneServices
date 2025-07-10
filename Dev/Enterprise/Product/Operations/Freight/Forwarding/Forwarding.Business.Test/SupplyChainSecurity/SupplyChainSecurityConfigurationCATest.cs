using System;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class SupplyChainSecurityConfigurationCATest : SupplyChainSecurityConfigurationTest
	{
		public override void TestUsesGenericScheme()
		{
			Assert(!SupplyChainSecurityConfiguration.UsesGenericScheme);
		}

		#region Approval Code Configuration

		public override void TestApprovalCodesList()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "RA", "CA", "KC", "AC", "NO" }, SupplyChainSecurityConfiguration.ApprovalCodesList.Cast<CodeDescriptionPair>().Select(x => x.Code));

			AssertEquals("Regulated Agent", SupplyChainSecurityConfiguration.ApprovalCodesList["RA"].Description);
			AssertEquals("Certified Agent", SupplyChainSecurityConfiguration.ApprovalCodesList["CA"].Description);
			AssertEquals("Known Consignor", SupplyChainSecurityConfiguration.ApprovalCodesList["KC"].Description);
			AssertEquals("Account Consignor", SupplyChainSecurityConfiguration.ApprovalCodesList["AC"].Description);
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
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate("AC"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate("KC"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate("RA"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate("RA"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate("CA"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate("NO"));

			Assert(SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate("AC"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate("KC"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate("RA"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate("RA"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate("CA"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate("NO"));
		}

		public override void TestValidApprovalCodesForAviationSecurityApproval()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CA"))
			{
				Assert("KC is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.KnownConsignor));
				Assert("RA is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.RegulatedAgent));
				Assert("CA is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.CertifiedAgent));
				Assert("AC is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.AccountConsignor));
				Assert("No is not approved code", !SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembershipEx.Codes.No));
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

		#endregion

		#region Organisations to use

		public override void TestOrganisationsToUseForAviationSecurity()
		{
			AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.Consignor].ValidationCode);
			AssertEquals(1, SupplyChainSecurityConfiguration.OrganisationsToUse.Values.Count);

			var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_Canada.Value;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPackingCFS)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolTransportCompany)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolAirline)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;

			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_Canada.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
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

		#region Shipper Type Collection

		public override void TestGetCountrySpecificShipmentInspectionTypeCollection()
		{
			var inspectionTypes = new ShipmentInspectionTypes();
			inspectionTypes.Types.Add("ABC", (NoResString)"ABC", false, false);
			inspectionTypes.Types.Add("QQQ", (NoResString)"QQQ", false, false);

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_CA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var enabledList = SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection;

					AssertContainsExactElementsInAnyOrder(new[] { "PHS", "XRY", "ETD", "EDD", "BIO", "DIP", "TRN" },
						enabledList.Cast<ShipmentInspectionType>().Select(x => x.Code));
				}

				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_CA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var disabledList = SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection;

					AssertContainsExactElementsInAnyOrder(
						new[] { "ABC", "QQQ" },
						disabledList.Cast<ShipmentInspectionType>().Select(x => x.Code));
				}
			}
		}

		#endregion

		public override void TestValidateJS_InspectionType_GovernmentApprovedReliableOrganization()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CA"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"CATOR";
				shipment.JS_RL_NKDestination = "JPTYO";
				shipment.JS_InspectionTypeCode = "GOV";

				AssertHasError($"JS_InspectionTypeCode of GOV should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of GOV Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (b) Government approved reliable organization(s) can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		public override void TestValidateJS_InspectionType_AdHocMovementsOfCargo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CA"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"CATOR";
				shipment.JS_RL_NKDestination = "JPTYO";
				shipment.JS_InspectionTypeCode = "ADH";

				AssertHasError($"JS_InspectionTypeCode of ADH should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of ADH Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (c) ad hoc movements of cargo can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		#region Implementation

		protected override SupplyChainSecurityConfiguration GetNewSupplyChainSecurityConfigurationToTest()
		{
			return new SupplyChainSecurityConfigurationCA();
		}

		public override ShipmentInspectionTypeRegistryItem GetRegistryItem()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypes_Canada;
		}

		#endregion
	}
}
