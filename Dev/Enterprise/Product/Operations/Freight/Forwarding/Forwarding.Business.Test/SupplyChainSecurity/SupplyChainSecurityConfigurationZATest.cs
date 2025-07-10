using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class SupplyChainSecurityConfigurationZATest : SupplyChainSecurityConfigurationTest
	{
		#region Approval Code Configuration

		public override void TestApprovalCodesList()
		{
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_ZA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertContainsExactElementsInAnyOrder(new[] { "KC", "RA", "NO" }, SupplyChainSecurityConfiguration.ApprovalCodesList.Cast<CodeDescriptionPair>().Select(x => x.Code));
			}
		}

		public override void TestApprovalCodesWithRequiredDocumentValidation()
		{
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_ZA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("RA"));
				Assert(SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("KC"));
				Assert(!SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("NO"));
			}
		}

		public override void TestApprovalCodeExpiryDateValidation()
		{
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_ZA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert("KC: Expiry date required", SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate("KC"));
				AssertEquals("KC: Maximum expiry", SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears("KC"), 1);
				AssertEquals("No warning for pending expiry", SupplyChainSecurityConfiguration.ApprovalCodeWarnIfApprovalWillLapseInMonths("KC"), 0);

				Assert("RA: Expiry date required", SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate("RA"));
				AssertEquals("RA: Maximum expiry", SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears("RA"), 1);
				AssertEquals("RA: No warning for pending expiry", SupplyChainSecurityConfiguration.ApprovalCodeWarnIfApprovalWillLapseInMonths("RA"), 0);

				Assert("NO: Expiry date is not required", !SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate("NO"));
				Assert("NO: Expiry date is not allowed", !SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate("NO"));
				AssertEquals("NO: No maximum expiry", SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears("NO"), 0);
				AssertEquals("NO: No warning for pending expiry", SupplyChainSecurityConfiguration.ApprovalCodeWarnIfApprovalWillLapseInMonths("NO"), 0);
			}
		}

		public override void TestUsesGenericScheme()
		{
			Assert(!SupplyChainSecurityConfiguration.UsesGenericScheme);
		}

		public override void TestGetCountrySpecificShipmentInspectionTypeCollection()
		{
			var inspectionTypes = new ShipmentInspectionTypes();
			inspectionTypes.Types.Add("ABC", (NoResString)"ABC", false, false);
			inspectionTypes.Types.Add("QQQ", (NoResString)"QQQ", false, false);

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_ZA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var securityEnabledList = SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection;

					AssertContainsExactElementsInAnyOrder(
						new[] { "EDS", "ETD", "PHS", "VCK", "XRY", "EDD", "FRD", "RES", "DIP", "HMR", "LFS", "LIV", "NUC", "TRN" },
						securityEnabledList.Cast<ShipmentInspectionType>().Select(x => x.Code));
				}

				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_ZA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var securityDisabledList = SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection;

					AssertContainsExactElementsInAnyOrder(
						new[] { "ABC", "QQQ" },
						securityDisabledList.Cast<ShipmentInspectionType>().Select(x => x.Code));
				}
			}
		}

		public override void TestValidApprovalCodesForAviationSecurityApproval()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				Assert("KC is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.KnownConsignor));
				Assert("RA is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.RegulatedAgent));

				Assert("RC is not approved code", !SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.RegularCustomer));
				Assert("NO is not approved code", !SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembershipEx.Codes.No));
			}
		}

		#endregion

		#region Organisations To Use

		public override void TestOrganisationsToUseForAviationSecurity()
		{
			AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.Consignor].ValidationCode);
			AssertEquals(1, SupplyChainSecurityConfiguration.OrganisationsToUse.Values.Count);

			var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_SouthAfrica.Value;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPackingCFS)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolTransportCompany)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolAirline)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;
		}

		#endregion

		#region Shipment Dates

		public override void TestShipmentDateForAviationSecurity()
		{
			using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_ZA.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				var testDate1 = ZDateTime.Now.AddDays(1);
				var testDate2 = ZDateTime.Now.AddDays(2);
				var testDate3 = ZDateTime.Now.AddDays(3);
				var testDate4 = ZDateTime.Now.AddDays(4);

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "ZACPT";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_TransportMode = "AIR";

				AssertEquals("Unsaved shipment: date defaults to current date", ZDate.Today, shipment.AviationSecurity.ShipmentDateForAviationSecurity.Date);

				Factory.Save();

				AssertEquals("Created date", shipment.JS_SystemCreateTimeUtc.ToLocalBranchTime(), shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				shipment.JS_E_DEP = testDate1;
				AssertEquals("Departure date", testDate1, shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				var consol1 = shipment.Consols.AddNew();
				consol1.JK_RL_NKLoadPort = "ZABAL";
				consol1.JK_RL_NKDischargePort = "ZACPT";
				consol1.JK_TransportMode = "ROA";
				var transport1 = consol1.Transports.AddNew();
				transport1.JW_ETD = testDate2;

				var consol2 = shipment.Consols.AddNew();
				consol2.JK_RL_NKLoadPort = "ZACPT";
				consol2.JK_RL_NKDischargePort = "DEHAM";
				consol2.JK_TransportMode = "AIR";
				var transport2 = consol2.Transports.AddNew();
				transport2.JW_ETD = testDate4;

				AssertEquals("No MAWB issue date", testDate1, shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				consol2.JK_MasterBillIssueDate = testDate3;
				AssertEquals("MAWB issue date", testDate3, shipment.AviationSecurity.ShipmentDateForAviationSecurity);
			}
		}

		#endregion

		public override void TestValidateJS_InspectionType_GovernmentApprovedReliableOrganization()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"ZAJNB";
				shipment.JS_RL_NKDestination = "JPTYO";
				shipment.JS_InspectionTypeCode = "GOV";

				AssertHasError($"JS_InspectionTypeCode of GOV should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of GOV Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (b) Government approved reliable organization(s) can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		public override void TestValidateJS_InspectionType_AdHocMovementsOfCargo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"ZAJNB";
				shipment.JS_RL_NKDestination = "JPTYO";
				shipment.JS_InspectionTypeCode = "ADH";

				AssertHasError($"JS_InspectionTypeCode of ADH should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of ADH Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (c) ad hoc movements of cargo can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		#region Implementation

		protected override SupplyChainSecurityConfiguration GetNewSupplyChainSecurityConfigurationToTest()
		{
			return new SupplyChainSecurityConfigurationZA();
		}

		public override ShipmentInspectionTypeRegistryItem GetRegistryItem()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypes_SouthAfrica;
		}

		#endregion
	}
}
