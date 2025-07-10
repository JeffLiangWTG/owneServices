using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class SupplyChainSecurityConfigurationJPTest : SupplyChainSecurityConfigurationTest
	{
		public override void TestGetCountrySpecificShipmentInspectionTypeCollection()
		{
			var inspectionTypes = new ShipmentInspectionTypes();
			inspectionTypes.Types.Add("ABC", (NoResString)"ABC", false, false);
			inspectionTypes.Types.Add("QQQ", (NoResString)"QQQ", false, false);

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var securityEnabledList = SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection;

					AssertContainsExactElementsInAnyOrder(
						new[] { "PHS", "VCK", "XRY", "EDS", "ETD", "SMU", "DIP", "TRN", "SAF", "AVI" },
						securityEnabledList.Cast<ShipmentInspectionType>().Select(x => x.Code));
				}

				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var securityDisabledList = SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection;

					AssertContainsExactElementsInAnyOrder(
							new[] { "ABC", "QQQ" },
							securityDisabledList.Cast<ShipmentInspectionType>().Select(x => x.Code));
				}
			}
		}

		public override void TestShipmentDateForAviationSecurity()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var testDate1 = ZDateTime.Now.AddDays(1);
				var testDate2 = ZDateTime.Now.AddDays(2);
				var testDate3 = ZDateTime.Now.AddDays(3);
				var testDate4 = ZDateTime.Now.AddDays(4);

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "JPOSA";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_TransportMode = "AIR";

				AssertEquals("Unsaved shipment: date defaults to current date", ZDate.Today, shipment.AviationSecurity.ShipmentDateForAviationSecurity.Date);

				Factory.Save();

				AssertEquals("Created date", shipment.JS_SystemCreateTimeUtc.ToLocalBranchTime(), shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				shipment.JS_E_DEP = testDate1;
				AssertEquals("Departure date", testDate1, shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				var consol1 = shipment.Consols.AddNew();
				consol1.JK_RL_NKLoadPort = "JPOSA";
				consol1.JK_RL_NKDischargePort = "JPTYO";
				consol1.JK_TransportMode = "ROA";
				var transport1 = consol1.Transports.AddNew();
				transport1.JW_ETD = testDate2;

				var consol2 = shipment.Consols.AddNew();
				consol2.JK_RL_NKLoadPort = "JPTYO";
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

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault_Japan.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "XRY"))
			{
				AssertEquals("XRY", SupplyChainSecurityConfiguration.InspectionTypeDefault);
			}
		}

		public override void TestUsesGenericScheme()
		{
			Assert(!SupplyChainSecurityConfiguration.UsesGenericScheme);
		}

		#region Approval Codes Configuration

		public override void TestApprovalCodeExpiryDateValidation()
		{
			foreach (var code in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				if (code == AviationSecuritySchemeMembershipEx.Codes.No)
				{
					Assert("Expiry date does not apply", !SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(code));
				}

				if (code == AviationSecuritySchemeMembership.Codes.RegulatedAgent)
				{
					Assert("Expiry date does not apply", !SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(code));
					Assert("Expiry date is not allowed", !SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate(code));
					Assert("Expiry date does not need to match match required document", !SupplyChainSecurityConfiguration.ApprovalCodeExpiryDateMustMatchRequiredDocumentExpiry(code));
				}

				if (code == AviationSecuritySchemeMembership.Codes.KnownConsignor)
				{
					Assert("Has expiry date", SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(code));
					Assert("Expiry date must match required document", SupplyChainSecurityConfiguration.ApprovalCodeExpiryDateMustMatchRequiredDocumentExpiry(code));
					AssertEquals("The maximum expiry is not in 2 years", SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears(code), 2);
				}
				else
				{
					AssertEquals("No maximum expiry", SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears(code), 0);
				}

				AssertEquals("No warning for pending expiry", SupplyChainSecurityConfiguration.ApprovalCodeWarnIfApprovalWillLapseInMonths(code), 0);
			}
		}

		#endregion

		public override void TestValidateJS_InspectionType_GovernmentApprovedReliableOrganization()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"JPTYO";
				shipment.JS_RL_NKDestination = "SGSIN";
				shipment.JS_InspectionTypeCode = "GOV";

				AssertHasError($"JS_InspectionTypeCode of GOV should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of GOV Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (b) Government approved reliable organization(s) can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		public override void TestValidateJS_InspectionType_AdHocMovementsOfCargo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"JPTYO";
				shipment.JS_RL_NKDestination = "SGSIN";
				shipment.JS_InspectionTypeCode = "ADH";

				AssertHasError($"JS_InspectionTypeCode of ADH should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of ADH Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (c) ad hoc movements of cargo can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		public override void TestApprovalCodesList()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "KC", "RA", "NO" }, SupplyChainSecurityConfiguration.ApprovalCodesList.Cast<CodeDescriptionPair>().Select(x => x.Code));
		}

		public override void TestApprovalCodesWithRequiredDocumentValidation()
		{
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("KC"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("RA"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("NO"));
		}

		public override void TestValidApprovalCodesForAviationSecurityApproval()
		{
			Assert("KC is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.KnownConsignor));
			Assert("RA is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.RegulatedAgent));
			Assert("No is not approved code", !SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembershipEx.Codes.No));
		}

		#region Implementation

		protected override SupplyChainSecurityConfiguration GetNewSupplyChainSecurityConfigurationToTest()
		{
			return new SupplyChainSecurityConfigurationJP();
		}

		public override ShipmentInspectionTypeRegistryItem GetRegistryItem()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypes_Japan;
		}

		#endregion
	}
}
