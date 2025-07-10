using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public class SupplyChainSecurityConfigurationTest : TestCaseWithFactory
	{
		public void TestGetNewSupplyChainSecurityConfigurationTypeForCountry()
		{
			AssertEquals(typeof(SupplyChainSecurityConfigurationAU), SupplyChainSecurityConfiguration.New("AU").GetType());
			AssertEquals(typeof(SupplyChainSecurityConfigurationCA), SupplyChainSecurityConfiguration.New("CA").GetType());
			AssertEquals(typeof(SupplyChainSecurityConfigurationEU), SupplyChainSecurityConfiguration.New("FR").GetType());
			AssertEquals(typeof(SupplyChainSecurityConfigurationHK), SupplyChainSecurityConfiguration.New("HK").GetType());
			AssertEquals(typeof(SupplyChainSecurityConfigurationJP), SupplyChainSecurityConfiguration.New("JP").GetType());
			AssertEquals(typeof(SupplyChainSecurityConfigurationSG), SupplyChainSecurityConfiguration.New("SG").GetType());
			AssertEquals(typeof(SupplyChainSecurityConfigurationTW), SupplyChainSecurityConfiguration.New("TW").GetType());
			AssertEquals(typeof(SupplyChainSecurityConfigurationUS), SupplyChainSecurityConfiguration.New("US").GetType());
			AssertEquals(typeof(SupplyChainSecurityConfigurationZA), SupplyChainSecurityConfiguration.New("ZA").GetType());
			AssertEquals(typeof(SupplyChainSecurityConfigurationUK), SupplyChainSecurityConfiguration.New("GB").GetType());
			AssertEquals(typeof(SupplyChainSecurityConfiguration), SupplyChainSecurityConfiguration.New("IN").GetType());
		}

		public virtual void TestShouldSetScheduledArrivalDate()
		{
			Assert(!SupplyChainSecurityConfiguration.ShouldSetScheduledArrivalDate(null));
		}

		public virtual void TestIsOnlyForDevelopers()
		{
			var configuration = GetNewSupplyChainSecurityConfigurationToTest();
			AssertEquals("Development is complete and it is available for customers to use", false, configuration.IsOnlyForDevelopers);
		}

		public virtual void TestUsesGenericScheme()
		{
			Assert(SupplyChainSecurityConfiguration.UsesGenericScheme);
		}

		public virtual void TestOrganisationsToUseForAviationSecurity()
		{
			AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.Consignor].ValidationCode);
			AssertEquals(1, SupplyChainSecurityConfiguration.OrganisationsToUse.Values.Count);

			using (FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, SupplyChainSecurityOrganisationTypes.LocalClient))
			{
				ResetSupplyChainSecurityConfiguration();
				AssertEquals(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes, SupplyChainSecurityConfiguration.OrganisationsToUse[SupplyChainSecurityOrganisationTypes.LocalClient].ValidationCode);
				AssertEquals(1, SupplyChainSecurityConfiguration.OrganisationsToUse.Values.Count);
			}
		}

		public virtual void TestApprovalCodesList()
		{
			AssertContainsExactElementsInAnyOrder(new[] { "YES", "NO" }, SupplyChainSecurityConfiguration.ApprovalCodesList.Cast<CodeDescriptionPair>().Select(x => x.Code));
		}

		public virtual void TestGetCountrySpecificShipmentInspectionTypeCollection()
		{
			var inspectionTypes = new ShipmentInspectionTypes();
			inspectionTypes.Types.Add("ABC", (NoResString)"ABC", false, false);
			inspectionTypes.Types.Add("QQQ", (NoResString)"QQQ", false, false);

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, inspectionTypes))
			{
				var list = SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection;

				AssertContainsExactElementsInAnyOrder(
					new[] { "ABC", "QQQ" },
					list.Cast<ShipmentInspectionType>().Select(x => x.Code));
			}
		}

		public void TestCountrySpecificShipmentInspectionTypeListMatchesRegistry()
		{
			if (SupplyChainSecurityConfiguration.IsEnabled)
			{
				var registryItem = GetRegistryItem();
				using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItem.defaultValue))
				{
					var shipmentInspectionTypeCodes = SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection.Cast<ShipmentInspectionType>().Select(x => x.Code);
					var name = SupplyChainSecurityConfiguration.GetType().Name;
					var countryCode = name.Substring(name.Length - 2, 2);
					var countrySpecificSystemDefinedCodes = new ShipmentInspectionTypeLists(Factory).GetCountrySpecificSystemDefinedList(countryCode).Cast<ShipmentInspectionType>().Select(x => x.Code);

					AssertContainsExactElementsInAnyOrder("Ensure FreightDataRegistry.ShipmentInspectionTypeLists.GetCountrySpecificSystemDefinedList / GetCountrySpecificScreeningMethods / GetCountrySpecificExemptionTypeCodes have been updated", shipmentInspectionTypeCodes, countrySpecificSystemDefinedCodes);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public virtual ShipmentInspectionTypeRegistryItem GetRegistryItem()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem;
		}

		public virtual void TestInspectionTypeDefault()
		{
			AssertEquals("UNK", SupplyChainSecurityConfiguration.InspectionTypeDefault);

			using (FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "XRY"))
			{
				AssertEquals("XRY", SupplyChainSecurityConfiguration.InspectionTypeDefault);
			}
		}

		public virtual void TestUseApprovedOrganisationRequiredDocTypeSpecifiedInRegistry()
		{
			AssertEquals(!SupplyChainSecurityConfiguration.IsEnabled, SupplyChainSecurityConfiguration.UseApprovedOrganisationRequiredDocTypeSpecifiedInRegistry);
		}

		#region Approval Codes

		public virtual void TestApprovalCodesWithRequiredDocumentValidation()
		{
			Assert(SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("YES"));
			Assert(!SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation("NO"));
		}

		public virtual void TestValidApprovalCodesForAviationSecurityApproval()
		{
			Assert("Yes is approved code", SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembershipEx.Codes.Yes));
			Assert("No is not approved code", !SupplyChainSecurityConfiguration.ApprovalCodeIsValidForAviationSecurityApproval(AviationSecuritySchemeMembershipEx.Codes.No));
		}

		public void TestApprovalCodesWithRequiredFormatMustHaveAnErrorMessage()
		{
			foreach (var code in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				if (!SupplyChainSecurityConfiguration.ApprovalCodeApprovalNumberFormat(code).IsEmpty)
				{
					Assert("Error must not be empty", !SupplyChainSecurityConfiguration.ApprovalCodeErrorForApprovalNumberFormatNotMet(code).IsEmpty);
				}
				else
				{
					Assert("Error must be empty", SupplyChainSecurityConfiguration.ApprovalCodeErrorForApprovalNumberFormatNotMet(code).IsEmpty);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestApprovalCodesRequiredDocumentOptionCombinations()
		{
			foreach (var code in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				if (SupplyChainSecurityConfiguration.ApprovalCodeApprovalNumberMustMatchRequiredDocumentNumber(code))
				{
					Assert("Must have Required Document Validation", SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation(code));
					Assert("Must allow Approval Number", SupplyChainSecurityConfiguration.ApprovalCodeAllowsApprovalNumber(code));
				}
			}
		}

		public virtual void TestApprovalCodeExpiryDateValidation()
		{
			foreach (var code in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				Assert("Expiry date is not required", !SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(code));
				Assert("Expiry date is not allowed", !SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate(code));
				AssertEquals("No maximum expiry", SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears(code), 0);
				AssertEquals("No warning for pending expiry", SupplyChainSecurityConfiguration.ApprovalCodeWarnIfApprovalWillLapseInMonths(code), 0);
			}
		}

		public void TestExpiryDateConfiguration()
		{
			foreach (var approvalCode in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				if (SupplyChainSecurityConfiguration.ApprovalCodeExpiryDateMustMatchRequiredDocumentExpiry(approvalCode))
				{
					Assert("HasExpiryDate must be true if ExpiryDateMustMatchRequiredDocumentExpiry is true", SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(approvalCode));
					Assert("RequiredDocumentValidation must be true if ExpiryDateMustMatchRequiredDocumentExpiry is true", SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation(approvalCode));
				}

				if (SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears(approvalCode) > 0)
				{
					Assert("HasExpiryDate must be true if MaximumValidityInYears > 0", SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(approvalCode));
				}

				if (!SupplyChainSecurityConfiguration.ApprovalCodeErrorIfMaximumApprovalValidityExceeded(approvalCode).IsEmpty)
				{
					Assert("MaximumValidityInYears must be > 0 if !ErrorIfMaximumApprovalValidityExceeded.IsEmpty", SupplyChainSecurityConfiguration.ApprovalCodeMaximumValidityInYears(approvalCode) > 0);
				}

				if (SupplyChainSecurityConfiguration.ApprovalCodeWarnIfApprovalWillLapseInMonths(approvalCode) > 0)
				{
					Assert("HasExpiryDate must be true if WarnIfApprovalWillLapseInMonths > 0", SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(approvalCode));
				}

				if (!SupplyChainSecurityConfiguration.ApprovalCodeWarningIfApprovalHasExpired(approvalCode).IsEmpty)
				{
					Assert("HasExpiryDate must be true if !ErrorIfApprovalHasExpired.IsEmpty", SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(approvalCode));
				}

				if (!SupplyChainSecurityConfiguration.ApprovalCodeWarningIfApprovalWillLapseInMonths(approvalCode).IsEmpty)
				{
					Assert("WarnIfApprovalWillLapseInMonths must be > 0 if !WarningIfApprovalWillLapseInMonths.IsEmpty", SupplyChainSecurityConfiguration.ApprovalCodeWarnIfApprovalWillLapseInMonths(approvalCode) > 0);
				}

				if (SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(approvalCode))
				{
					Assert("AllowsExpiryDate must be true if RequiresExpiryDate is true", SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate(approvalCode));
				}

				if (!SupplyChainSecurityConfiguration.ApprovalCodeAllowsExpiryDate(approvalCode))
				{
					Assert("RequiresExpiryDate must be false if AllowsExpiryDate is false", !SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(approvalCode));
				}
			}

			Assert("Passed", true);
		}

		public void TestDefaultAddressTypeConfiguration()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());

			foreach (var approvalCode in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				if (SupplyChainSecurityConfiguration.ApprovalCodeDefaultAddress(approvalCode, org) != null)
				{
					Assert("Must be an address level scheme", SupplyChainSecurityConfiguration.IsAddressLevelScheme);
				}
			}

			Assert("Passed", true);
		}

		public void TestReadOnlyAddressConfiguration()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());

			foreach (var approvalCode in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				if (SupplyChainSecurityConfiguration.ApprovalCodeAddressIsReadOnly(approvalCode))
				{
					Assert("Must be Org level approval", SupplyChainSecurityConfiguration.ApprovalCodeIsOrgLevelApproval(approvalCode));
					Assert("Must have a default address", SupplyChainSecurityConfiguration.ApprovalCodeDefaultAddress(approvalCode, org) != null);
				}
			}

			Assert("Passed", true);
		}

		public void TestRequiredDocumentConfiguration()
		{
			foreach (var approvalCode in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				if (SupplyChainSecurityConfiguration.ApprovalCodeApprovalNumberMustMatchRequiredDocumentNumber(approvalCode))
				{
					Assert("Must allow approval number", SupplyChainSecurityConfiguration.ApprovalCodeAllowsApprovalNumber(approvalCode));
					Assert("Must have required document validation", SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation(approvalCode));
				}

				if (SupplyChainSecurityConfiguration.ApprovalCodeExpiryDateMustMatchRequiredDocumentExpiry(approvalCode))
				{
					Assert("Must have expiry date", SupplyChainSecurityConfiguration.ApprovalCodeRequiresExpiryDate(approvalCode));
					Assert("Must have required document validation", SupplyChainSecurityConfiguration.ApprovalCodeHasRequiredDocumentValidation(approvalCode));
				}
			}

			Assert("Passed", true);
		}

		[ExpectNoExceptions]
		public void TestApprovalCodesWithError()
		{
			foreach (var code in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				if (!SupplyChainSecurityConfiguration.ApprovalCodeErrorForOwnAgentApprovalNumberNotEntered(code).IsEmpty)
				{
					Assert("Must be mandatory", SupplyChainSecurityConfiguration.ApprovalCodeRequiresApprovalNumber(code));
				}
			}
		}

		public virtual void TestGetWarningForAdditionalApprovalCodeValidation()
		{
			AssertEquals(ZString.Empty, SupplyChainSecurityConfiguration.GetWarningForAdditionalApprovalCodeValidation(Factory.NewWithValidTestData<OrgCountryData>()));
		}

		public virtual void TestGetAgentApproval()
		{
			AssertNull("No consol", SupplyChainSecurityConfiguration.GetAgentApproval(null));

			var consol = Factory.New<ForwardingConsol>();
			AssertNull("No Sending Forwarder", SupplyChainSecurityConfiguration.GetAgentApproval(consol));

			var forwarder = Factory.New<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = forwarder.MainAddress.PK;
			AssertNull("No approval", SupplyChainSecurityConfiguration.GetAgentApproval(consol));

			var approval = forwarder.MainAddress.KnownShipperDetails.AddNew();
			approval.OV_RN_NKClientCountryRelation = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			approval.OV_OH_OrgHeader = forwarder.PK;
			approval.OV_EXApprovedOrMajorExporter = "RA";
			approval.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);

			if (SupplyChainSecurityConfiguration.IsEnabled)
			{
				AssertEquals("RA", SupplyChainSecurityConfiguration.GetAgentApproval(consol).OV_EXApprovedOrMajorExporter);
			}
			else
			{
				AssertNull("Not enabled", SupplyChainSecurityConfiguration.GetAgentApproval(consol));
			}
		}

		public void TestGetApproval()
		{
			AssertNoExceptionThrown(() => { SupplyChainSecurityConfiguration.GetApproval((OrgHeader)null); });
		}

		#endregion

		#region Known Shipper Filter

		public virtual void TestKnownShipperFilter()
		{
			AssertEquals("Known/Approved Status", SupplyChainSecurityConfiguration.KnownShipperFilterText);
			AssertEquals("Known/Approved Status", SupplyChainSecurityConfiguration.KnownShipperFilterDescription);

			AssertEquals("All", SupplyChainSecurityConfiguration.KnownShipperFilterList["ALL"].Description);

			foreach (var code in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				if (code == "YES")
				{
					AssertEquals("Approved Shipper/Exporter", SupplyChainSecurityConfiguration.KnownShipperFilterList["YES"].Description);
				}
				else if (code == "NO")
				{
					AssertEquals("Not Approved Shipper/Exporter", SupplyChainSecurityConfiguration.KnownShipperFilterList["NO"].Description);
				}
				else
				{
					AssertEquals(SupplyChainSecurityConfiguration.ApprovalCodesList[code].Description, SupplyChainSecurityConfiguration.KnownShipperFilterList[code].Description);
				}
			}

			AssertEquals("Every code + All should be in the list", SupplyChainSecurityConfiguration.ApprovalCodesList.Count + 1, SupplyChainSecurityConfiguration.KnownShipperFilterList.Count);
		}

		public virtual void TestIsApprovedToShipOnPassengerFlights()
		{
			foreach (var approvalCode in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				if (approvalCode == "NO")
				{
					Assert("NO cannot be approved for passenger flights", !SupplyChainSecurityConfiguration.ApprovalCodeIsApprovedToShipOnPassengerFlights(approvalCode));
				}
				else
				{
					Assert("Can ship on passenger flights", SupplyChainSecurityConfiguration.ApprovalCodeIsApprovedToShipOnPassengerFlights(approvalCode));
				}
			}
		}

		#endregion

		#region Approval Number Filter

		public virtual void TestApprovalNumberFilter()
		{
			AssertEquals("Known/Approved ID Number", SupplyChainSecurityConfiguration.ApprovalNumberFilterText);
			AssertEquals("Known/Approved ID Number", SupplyChainSecurityConfiguration.ApprovalNumberFilterDescription);
		}

		#endregion

		[ExpectNoExceptions]
		public virtual void TestCheckEH_AgentApprovalNumberAdditionalValidation()
		{ }

		public virtual void TestApprovalNumberFormat()
		{
			foreach (var code in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				Assert(SupplyChainSecurityConfiguration.ApprovalCodeApprovalNumberFormat(code).IsEmpty);
				Assert(SupplyChainSecurityConfiguration.ApprovalCodeErrorForApprovalNumberFormatNotMet(code).IsEmpty);
			}
		}

		public virtual void TestShowPrefixOnAgentApprovalNumber()
		{
			AssertEquals(false, SupplyChainSecurityConfiguration.ShowPrefixOnAgentApprovalNumber);
		}

		[TestDate(2017, 01, 01)]
		public virtual void TestShipmentDateForAviationSecurity()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "JMKIN";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_TransportMode = "AIR";

				AssertEquals("Unsaved shipment: date defaults to current date", ZDate.Today, shipment.AviationSecurity.ShipmentDateForAviationSecurity.Date);

				Factory.Save();

				AssertEquals("Created date", shipment.JS_SystemCreateTimeUtc, shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				shipment.JS_E_DEP = ZDate.Today.AddDays(1);
				AssertEquals("Doesn't use departure date", ZDate.Today, shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				var consol = shipment.Consols.AddNew();
				consol.JK_RL_NKLoadPort = "JMKIN";
				consol.JK_RL_NKDischargePort = "DEHAM";
				consol.JK_TransportMode = "AIR";

				var transport = consol.Transports.AddNew();
				transport.JW_ETD = ZDate.Today.AddDays(2);
				AssertEquals("Doesn't use transport date", ZDate.Today, shipment.AviationSecurity.ShipmentDateForAviationSecurity);

				consol.JK_MasterBillIssueDate = ZDate.Today.AddDays(-1);
				AssertEquals("Doesn't use MAWB issue date", ZDate.Today, shipment.AviationSecurity.ShipmentDateForAviationSecurity);
			}
		}

		public void TestNoApprovalNumberRAWarningShownForUK()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("HK"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_RL_NKLoadPort = "GB2AB";
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
				AssertNoWarnings("No message error with RA Prefix.", header.EH_AgentApprovalNumberInfo);
			}
		}

		public void TestAllowsApprovalNumberMustBeTrueIfRequiresApprovalNumberIsTrue()
		{
			foreach (var approvalCode in SupplyChainSecurityConfiguration.ApprovalCodesList.GetAllCodes())
			{
				if (SupplyChainSecurityConfiguration.ApprovalCodeRequiresApprovalNumber(approvalCode))
				{
					Assert("AllowsApprovalCode must be true if RequiresApprovalCode is true", SupplyChainSecurityConfiguration.ApprovalCodeAllowsApprovalNumber(approvalCode));
				}
			}

			Assert("Passed", true);
		}

		#region AllowAutomaticCalculationOfApprovedStatus

		public virtual void TestAllowAutomaticCalculationOfApprovedStatus()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AssertEquals("AllowAutomaticCalculationOfApprovedStatus defaults to true", true, SupplyChainSecurityConfiguration.AllowAutomaticCalculationOfApprovedStatus(shipment));
		}

		#endregion

		#region IsExportForAviationSecurityPurposes

		public virtual void TestIsExportForAviationSecurityPurposes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "JMKIN";
				consol.JK_RL_NKDischargePort = "FRCDG";
				Assert("Consol is export", SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol));

				consol.JK_RL_NKLoadPort = "FRCDG";
				consol.JK_RL_NKDischargePort = "JMKIN";
				Assert("Consol is not export", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol));

				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "JMKIN";
				shipment.JS_RL_NKDestination = "FRPAR";
				Assert("Shipment is export", SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));

				shipment.JS_RL_NKOrigin = "FRPAR";
				shipment.JS_RL_NKDestination = "JMKIN";
				Assert("Shipment is not export", !SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));
			}

			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "FSA";

				var seaConsol = shipment.Consols.AddNew();
				seaConsol.JK_TransportMode = "SEA";
				seaConsol.JK_RL_NKLoadPort = "ZACPT";
				seaConsol.JK_RL_NKDischargePort = "DEHAM";

				var airConsol = shipment.Consols.AddNew();
				airConsol.JK_TransportMode = "AIR";
				airConsol.JK_RL_NKLoadPort = "DEHAM";
				airConsol.JK_RL_NKDischargePort = "SGSIN";

				shipment.JS_RL_NKOrigin = "ZACPT";
				shipment.JS_RL_NKDestination = "SGSIN";

				using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
				{
					Assert("Shipment is export", SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry("ZA"))
				{
					Assert("Shipment is not export", SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(shipment));
				}
			}
		}

		#endregion

		#region User Authorization

		public virtual void TestAllowDefaultingCargoSecureWithHighRiskRequirements()
		{
			var consol = Factory.New<ForwardingConsol>();
			Assert(SupplyChainSecurityConfiguration.AllowDefaultingCargoSecureForPassengerAndAllCargoAircraft(consol));
		}

		public virtual void TestSpecialHandlingErrorForUnauthorizedUser()
		{
			var securityCore = new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			securityCore.MaintainConsolAllowToOverrideSecurityStatus.IsAllowed = false;

			const string expectedError = "Ensure you have Security Rights 'Operate > Forwarding > Consolidations > Allow to override Security Status' granted before setting the Consol’s secured status to \"SPX\".";

			#region Canada is a Supply Chain Security supported country

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CA"))
			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_OverrideWaybillDefaults = false;
				consol.JK_RL_NKLoadPort = CountryCodes.Canada;

				var securityStatusCodeSpecialHandling = Factory.New<SecurityJobConsolAWBSpecialHandling>();
				securityStatusCodeSpecialHandling.JKH_JK_Consol = consol.PK;
				securityStatusCodeSpecialHandling.JKH_Code = "SPX";

				AssertEquals("User without security right cannot choose SPX", expectedError, SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consol, "SPX", true));

				consol.JK_RL_NKLoadPort = CountryCodes.Japan;
				AssertEquals("No error for different login country and consol's load port", ZString.Empty, SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consol, "SPX", true));

				consol.JK_RL_NKLoadPort = CountryCodes.Canada;
				securityCore.MaintainConsolAllowToOverrideSecurityStatus.IsAllowed = true;
				AssertEquals("No error for export shipment with a valid security right", ZString.Empty, SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consol, "SPX", true));
			}

			#endregion

			#region Bangladesh is NOT a Supply Chain Security supported country

			securityCore.MaintainConsolAllowToOverrideSecurityStatus.IsAllowed = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("BD"))
			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				var consol = Factory.New<ForwardingConsol>();
				AssertEquals("No error for not SCS supported country with no security right", ZString.Empty, SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consol, "SPX", true));
			}

			#endregion
		}

		#endregion

		#region Consignment Security Declaration

		public virtual void TestUseConsignmentSecurityDeclaration()
		{
			AssertEquals("Should use ConsignmentSecurityDeclaration", true, SupplyChainSecurityConfiguration.UseConsignmentSecurityDeclaration);
		}

		public virtual void TestAllowIncludeECSD()
		{
			AssertEquals("Should allow include eCSD", true, SupplyChainSecurityConfiguration.AllowIncludeECSD);
		}

		#endregion

		#region High Risk Requirement

		public virtual void TestIsHighRiskApplicable()
		{
			Assert(!SupplyChainSecurityConfiguration.IsHighRiskApplicable);
		}

		#endregion

		#region IsTranshipment

		public void TestIsTranshipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUBNE";
			shipment.JS_RL_NKDestination = "DEHAM";

			var consol = shipment.Consols.AddNew();
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();
			var transport3 = consol.Transports.AddNew();

			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "USLAX";
			transport2.JW_RL_NKLoadPort = "USLAX";
			transport2.JW_RL_NKDiscPort = "USNYC";
			transport3.JW_RL_NKLoadPort = "USNYC";
			transport3.JW_RL_NKDiscPort = "DEHAM";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				AssertEquals("Not a transhipment if at origin", false, SupplyChainSecurityConfiguration.New().IsTranshipment(shipment));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				AssertEquals("Not a transhipment if at destination", false, SupplyChainSecurityConfiguration.New().IsTranshipment(shipment));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				AssertEquals("Not a transhipment if at unrelated country", false, SupplyChainSecurityConfiguration.New().IsTranshipment(shipment));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				AssertEquals("Is transhipped in the US", true, SupplyChainSecurityConfiguration.New().IsTranshipment(shipment));
			}
		}

		#endregion

		#region Pack Level Screening

		public virtual void TestIsPackLevelScreeningAvailable()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "SGSIN";

				Assert("Unavailable for non-Air shipment", !SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(shipment));

				shipment.JS_TransportMode = TransportModes.Air;

				Assert("Available for Air shipment", SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(shipment));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "HKHKG";
				shipment.JS_RL_NKDestination = "PGPOM";

				var consol1 = shipment.Consols.AddNew();
				consol1.JK_TransportMode = TransportModes.Air;
				consol1.JK_RL_NKLoadPort = "HKHKG";
				consol1.JK_RL_NKDischargePort = "NZAKL";

				var consol2 = shipment.Consols.AddNew();
				consol2.JK_TransportMode = TransportModes.Air;
				consol2.JK_RL_NKLoadPort = "NZAKL";
				consol2.JK_RL_NKDischargePort = "PGPOM";

				Assert(SupplyChainSecurityConfiguration.IsTranshipment(shipment));
				Assert(SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(shipment));
			}
		}

		public virtual void TestJL_InspectionTypeCode_ReadOnly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "SGSIN";

				Assert("Read-only for SEA", SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(shipment));

				shipment.JS_TransportMode = TransportModes.Air;

				Assert("Read/Write for AIR", !SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(shipment));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "PGPOM";

				Assert(!SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(shipment));

				shipment.JS_InspectionTypeCode = BaseJobShipmentLookups.InspectionType_Approved;
				Assert("Read-only when shipment inspection is APP", SupplyChainSecurityConfiguration.JL_InspectionTypeCode_ReadOnly(shipment));
			}
		}

		#endregion

		#region Prohibited Routing

		public virtual void TestGetWarningForProhibitedRouting()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "SYPMS";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.JS_TransportMode = "AIR";
			Assert("No warning for prohibited routing", SupplyChainSecurityConfiguration.GetWarningForProhibitedRouting(shipment).IsEmpty);
		}

		#endregion

		#region UseIssuingAuthorityCountry

		public virtual void TestUseIssuingAuthorityCountry()
		{
			AssertEquals(false, SupplyChainSecurityConfiguration.UseIssuingAuthorityCountry);
		}

		#endregion

		#region GetEditApprovalErrorForUncertifiedUser

		public virtual void TestGetEditApprovalErrorForUncertifiedUser()
		{
			AssertEquals(ZString.Empty, SupplyChainSecurityConfiguration.GetEditApprovalErrorForUncertifiedUser(Factory.NewWithValidTestData<OrgCountryData>()));
		}

		#endregion

		public virtual void TestIsAviationSecurityFreightMovementRestricted()
		{
			Assert("IsAviationSecurityFreightMovementRestricted should be false by default", !SupplyChainSecurityConfiguration.IsAviationSecurityFreightMovementRestricted(null));
		}

		public virtual void TestValidateJS_InspectionType_GovernmentApprovedReliableOrganization()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"JPTYO";
				shipment.JS_RL_NKDestination = "AUBNE";
				shipment.JS_InspectionTypeCode = "GOV";

				AssertHasError($"JS_InspectionTypeCode of GOV should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of GOV Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (b) Government approved reliable organization(s) can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		public virtual void TestValidateJS_InspectionType_AdHocMovementsOfCargo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JP"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = "AIR";
				shipment.JS_RL_NKOrigin = $"JPTYO";
				shipment.JS_RL_NKDestination = "AUBNE";
				shipment.JS_InspectionTypeCode = "ADH";

				AssertHasError($"JS_InspectionTypeCode of ADH should not be allowed", shipment.JS_InspectionTypeCodeInfo,
					"The Inspection Type of ADH Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (c) ad hoc movements of cargo can only be used for air shipments departing from EU, NO, CH, IS and LI");
			}
		}

		#region Implementation

		protected virtual SupplyChainSecurityConfiguration GetNewSupplyChainSecurityConfigurationToTest()
		{
			return new SupplyChainSecurityConfiguration();
		}

		internal void ResetSupplyChainSecurityConfiguration()
		{
			supplyChainSecurityConfiguration = null;
		}

		protected SupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get { return supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = GetNewSupplyChainSecurityConfigurationToTest()); }
		}
		SupplyChainSecurityConfiguration supplyChainSecurityConfiguration;

		#region Required Document

		protected class RequiredDocument : IDisposable
		{
			public RequiredDocument(OrgHeader organisation, string requiredDocumentType, string docNumber = "", string docPeriod = "", ZDate? docValidToDate = null)
			{
				tempFile = TempFile.New();
				System.IO.File.WriteAllBytes(tempFile.Filename, new byte[] { 1, 2, 3 });
				this.organisation = organisation;

				((IDocManagerSupport)organisation).DocManagerInfo.AddFileOrDocument(tempFile.Filename, requiredDocumentType);

				JobRequiredDocument = organisation.RequiredDocuments.AddNew();
				JobRequiredDocument.EQ_DocType = requiredDocumentType;
				JobRequiredDocument.EQ_DocNumber = docNumber;
				JobRequiredDocument.EQ_DocPeriod = docPeriod;

				if (docValidToDate != null)
				{
					JobRequiredDocument.EQ_ValidToDate = (ZDate)docValidToDate;
				}
			}

			public void Dispose()
			{
				organisation.RequiredDocuments.RemoveAndDelete(JobRequiredDocument);
				tempFile.Dispose();
				tempFile = null;
			}

			public JobRequiredDocument JobRequiredDocument { get; private set; }

			readonly OrgHeader organisation;

			TempFile tempFile;
		}

		#endregion

		#endregion
	}
}
