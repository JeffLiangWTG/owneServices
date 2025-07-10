using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class SupplyChainSecurityConfigurationHKTest : SupplyChainSecurityConfigurationTest
	{
		public override void TestOrganisationsToUseForAviationSecurity()
		{
			AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.Consignor].ValidationCode);
			AssertEquals(1, SupplyChainSecurityConfiguration.OrganisationsToUse.Values.Count);

			var settings = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_HongKong.Value;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.Consignor)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.LocalClient)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPickupTransportCompany)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ShipmentPackingCFS)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolSendingAgent)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolAirline)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes;
			((SupplyChainSecurityOrganisationToUse)settings.FindByCode(SupplyChainSecurityOrganisationTypes.ConsolCoLoadWithOrganization)).ValidationCode = SupplyChainSecurityOrganisationToUse.ValidationCodes.No;

			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_HongKong.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
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

		public override void TestShipmentDateForAviationSecurity()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var testDate1 = ZDateTime.Now.AddDays(1);
				var testDate2 = ZDateTime.Now.AddDays(2);
				var testDate3 = ZDateTime.Now.AddDays(3);
				var testDate4 = ZDateTime.Now.AddDays(4);

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_TransportMode = "AIR";

				AssertEquals("Unsaved shipment: date defaults to current date", ZDate.Today, shipment.AviationSecurity.ShipmentDateForAviationSecurity.Date);

				Factory.Save();

				AssertEquals("Created date", shipment.JS_SystemCreateTimeUtc.ToLocalBranchTime(), shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				shipment.JS_E_DEP = testDate1;
				AssertEquals("Departure date", testDate1, shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				var consol1 = shipment.Consols.AddNew();
				consol1.JK_RL_NKLoadPort = "HKHKG";
				consol1.JK_RL_NKDischargePort = "DEHAM";
				consol1.JK_TransportMode = "AIR";
				var transport2 = consol1.Transports.AddNew();
				transport2.JW_ETD = testDate4;

				AssertEquals("No MAWB issue date", testDate1, shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				consol1.JK_MasterBillIssueDate = testDate3;
				AssertEquals("MAWB issue date", testDate3, shipment.AviationSecurity.ShipmentDateForAviationSecurity);
			}
		}

		#region Approval Code Configuration

		public override void TestApprovalCodesList()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "AC", "KC", "RA", "NO" }, SupplyChainSecurityConfiguration.ApprovalCodesList.Cast<CodeDescriptionPair>().Select(x => x.Code));
		}

		public override void TestApprovalCodeExpiryDateValidation()
		{
			foreach (var code in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				if (code == AviationSecuritySchemeMembership.Codes.AccountConsignor)
				{
					Assert("Has expiry date", SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(code));
					AssertEquals("Maximum expiry", SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears(code), 5);
				}
				else if (code == AviationSecuritySchemeMembership.Codes.KnownConsignor)
				{
					Assert("No expiry date", !SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(code));
					AssertEquals("No maximum expiry", SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears(code), 0);
					Assert("No ExpiryDateMustMatchRequiredDocumentExpiry", !SupplyChainSecurityConfiguration.ApprovalCodeExpiryDateMustMatchRequiredDocumentExpiry(code));
					Assert("No ErrorIfMaximumApprovalValidityExceeded", SupplyChainSecurityConfiguration.ApprovalCodeErrorIfMaximumApprovalValidityExceeded(code).IsEmpty);
					Assert("Has AllowsExpiryDate", SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate(code));
				}
				else
				{
					Assert("Expiry date does not apply", !SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(code));
					AssertEquals("No maximum expiry", SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears(code), 0);
				}

				AssertEquals("No warning for pending expiry", SupplyChainSecurityConfiguration.ApprovalCodeWarnIfApprovalWillLapseInMonths(code), 0);
			}
		}

		public void TestRegulatedAgentDefaultAddressType()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var allCodes = SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes();
			var regulatedAgentCode = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
			Assert(allCodes.Contains(regulatedAgentCode));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeIsOrgLevelApproval(regulatedAgentCode));

			var defaultAddress = SupplyChainSecurityConfiguration.ApprovalCodeDefaultAddress(regulatedAgentCode, org);
			AssertEquals(org.MainAddress.PK, defaultAddress.PK);
		}

		#endregion

		public override void TestGetCountrySpecificShipmentInspectionTypeCollection()
		{
			var inspectionTypes = new ShipmentInspectionTypes();
			inspectionTypes.Types.Add("ABC", (NoResString)"ABC", false, false);
			inspectionTypes.Types.Add("QQQ", (NoResString)"QQQ", false, false);

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_HK.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var hongKongEnabledList = SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection;
					Assert(hongKongEnabledList.ContainsCode("PHS"));
					Assert(hongKongEnabledList.ContainsCode("ETD"));
					Assert(hongKongEnabledList.ContainsCode(ScreeningMethodsHK.Codes.XRayEquipment));
					Assert(hongKongEnabledList.ContainsCode("AVI"));
					Assert(hongKongEnabledList.ContainsCode("HMR"));
					Assert(!hongKongEnabledList.ContainsCode("SAF"));
				}

				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_HK.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var hongKongDisabledList = SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection;

					AssertContainsExactElementsInAnyOrder(
						new[] { "ABC", "QQQ" },
						hongKongDisabledList.Cast<ShipmentInspectionType>().Select(x => x.Code));
				}
			}
		}

		public override void TestIsExportForAviationSecurityPurposes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "CNSHA";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var roadTransport = consol.Transports[0];
				roadTransport.JW_RL_NKLoadPort = "CNSHA";
				roadTransport.JW_RL_NKDiscPort = "HKHKG";
				roadTransport.JW_TransportMode = Core.Constants.TransportModes.Road;
				roadTransport.JW_LegOrder = 1;

				var airTransport = consol.Transports.AddNew();
				airTransport.JW_RL_NKLoadPort = "HKHKG";
				airTransport.JW_RL_NKDiscPort = "AUSYD";
				airTransport.JW_TransportMode = Core.Constants.TransportModes.Air;
				airTransport.JW_LegOrder = 2;

				Assert("Consol is export for HongKong", SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol));
				Assert("Not Transhipment", !SupplyChainSecurityConfiguration.IsTranshipment(consol));
			}
		}

		public override void TestInspectionTypeDefault()
		{
			AssertEquals("UNK", SupplyChainSecurityConfiguration.InspectionTypeDefault);

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault_HongKong.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "XRY"))
			{
				AssertEquals("XRY", SupplyChainSecurityConfiguration.InspectionTypeDefault);
			}
		}

		public override void TestUsesGenericScheme()
		{
			Assert(!SupplyChainSecurityConfiguration.UsesGenericScheme);
		}

		public override void TestApprovalCodesWithRequiredDocumentValidation()
		{
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("KC"));
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("AC"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("RA"));
		}

		public override void TestValidApprovalCodesForAviationSecurityApproval()
		{
			Assert("KC is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.KnownConsignor));
			Assert("AC is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.AccountConsignor));
			Assert("RA is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembership.Codes.RegulatedAgent));

			Assert("No is not approved code", !SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembershipEx.Codes.No));
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
			const string expectedError = "The Approval Number must be in the following format: 'RA' + 4 digit number + 1 check digit. For example: RA34576.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
				var countryData = org.MainAddress.KnownShipperDetails.AddNew();
				countryData.OV_OH_OrgHeader = org.PK;

				foreach (var code in new[] { "AC", "KC", "RA" })
				{
					countryData.OV_EXApprovedOrMajorExporter = code;
					countryData.OV_EXApprovalNumber = "1234";

					if (code == "RA")
					{
						AssertHasError("Only RA has set format", countryData.OV_EXApprovalNumberInfo, expectedError);
					}
					else
					{
						AssertNoError("No set format for KC / AC", countryData.OV_EXApprovalNumberInfo, expectedError);
					}

					countryData.OV_EXApprovalNumber = "RA98765";
					AssertNoError(countryData.OV_EXApprovalNumberInfo, expectedError);
				}
			}
		}

		public override void TestShowPrefixOnAgentApprovalNumber()
		{
			AssertEquals(true, SupplyChainSecurityConfiguration.ShowPrefixOnAgentApprovalNumber);
		}

		public void TestPopulateRAPrefixValidationsOnHKAgentApprovalNumber()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "HKHKG";
				consol.JK_RL_NKDischargePort = "AUSYD";

				var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
				sendingForwarder.OH_Code = "TESTORG1";

				var sendingForwarderAddress = Factory.New<OrgAddress>();
				sendingForwarderAddress.OA_OH = sendingForwarder.PK;

				sendingForwarder.CountryData.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.RegulatedAgent;
				sendingForwarder.CountryData.OV_EXApprovalNumber = "RA34576";
				sendingForwarder.CountryData.OV_OA_ApprovedLocation = sendingForwarderAddress.PK;

				consol.JK_OA_SendingForwarderAddress = sendingForwarderAddress.PK;

				var header = Factory.New<ConsolExportAWBHeader>();
				header.EH_ParentID = consol.PK;

				header.Populate();

				AssertEquals("RA34576", header.EH_AgentApprovalNumber);
				AssertNoMessageErrors("No message error with RA Prefix.", header.EH_AgentApprovalNumberInfo);

				sendingForwarder.CountryData.OV_EXApprovalNumber = "34576";

				header.Populate();

				AssertEquals("34576", header.EH_AgentApprovalNumber);
				AssertHasMessageError("Message error without RA prefix", header.EH_AgentApprovalNumberInfo, "The Approval Number must be in the following format: 'RA' + 4 digit number + 1 check digit. For example: RA34576.");

				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "HKHKG";
				header.EH_AgentApprovalNumber = "34576";

				AssertNoMessageError("No Message error when consol is import", header.EH_AgentApprovalNumberInfo, "The Approval Number must be in the following format: 'RA' + 4 digit number + 1 check digit. For example: RA34576.");

				using (FreightDataRegistry.Instance.EnableSupplyChainSecurity_HK.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					consol.JK_RL_NKLoadPort = "HKHKG";
					consol.JK_RL_NKDischargePort = "AUSYD";
					header.EH_AgentApprovalNumber = "34576";

					AssertHasMessageError("Message error when registry is disabled", header.EH_AgentApprovalNumberInfo, "The Approval Number must be in the following format: 'RA' + 4 digit number + 1 check digit. For example: RA34576.");
				}
			}
		}

		#region Pack Level Screening

		public override void TestIsPackLevelScreeningAvailable()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.HongKong))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "PGPOM";

				var consol1 = shipment.Consols.AddNew();
				consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol1.JK_RL_NKLoadPort = "NZAKL";
				consol1.JK_RL_NKDischargePort = "HKHKG";

				var consol2 = shipment.Consols.AddNew();
				consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol2.JK_RL_NKLoadPort = "HKHKG";
				consol2.JK_RL_NKDischargePort = "PGPOM";

				Assert(SupplyChainSecurityConfiguration.IsTranshipment(shipment));
				Assert(SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(shipment));

				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				Assert("Unavailable for non-Air shipment", !SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(shipment));
			}
		}

		public override void TestJL_InspectionTypeCode_ReadOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.HongKong))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "PGPOM";

				Assert(!SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(shipment));

				shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;
				Assert("Read-only when shipment inspection is APP", SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(shipment));
			}
		}

		#endregion

		public override void TestValidateJS_InspectionType_GovernmentApprovedReliableOrganization()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"HKHKG";
				shipment.JS_RL_NKDestination = "JPTYO";
				shipment.JS_InspectionTypeCode = "GOV";

				AssertHasError($"JS_InspectionTypeCode of GOV should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of GOV Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (b) Government approved reliable organization(s) can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		public override void TestValidateJS_InspectionType_AdHocMovementsOfCargo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"HKHKG";
				shipment.JS_RL_NKDestination = "JPTYO";
				shipment.JS_InspectionTypeCode = "ADH";

				AssertHasError($"JS_InspectionTypeCode of ADH should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of ADH Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (c) ad hoc movements of cargo can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		#region Implementation

		protected override SupplyChainSecurityConfiguration GetNewSupplyChainSecurityConfigurationToTest()
		{
			return new SupplyChainSecurityConfigurationHK();
		}

		public override ShipmentInspectionTypeRegistryItem GetRegistryItem()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypes_HongKong;
		}

		#endregion
	}
}
