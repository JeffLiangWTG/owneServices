using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Business
{
	public class SupplyChainSecurityConfiguration : ISupplyChainSecurityConfiguration
	{
		#region Creation

		public static IEnumerable<string> SCSSupportedCountryList
		{
			get
			{
				return CountryCodes.EuropeanUnionAviationSecurityMembersList.Concat(new List<string> {
					CountryCodes.Australia,
					CountryCodes.Canada,
					CountryCodes.HongKong,
					CountryCodes.Japan,
					CountryCodes.Singapore,
					CountryCodes.SouthAfrica,
					CountryCodes.UnitedStates,
					CountryCodes.UnitedKingdom
				});
			}
		}

		public static SupplyChainSecurityConfiguration New()
		{
			return GetNewSupplyChainSecurityConfigurationForCountry(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public static SupplyChainSecurityConfiguration New(string countryCode)
		{
			return GetNewSupplyChainSecurityConfigurationForCountry(countryCode);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static SupplyChainSecurityConfiguration GetNewSupplyChainSecurityConfigurationForCountry(string countryCode)
		{
			switch (countryCode)
			{
				case CountryCodes.Australia:
					return new SupplyChainSecurityConfigurationAU();

				case CountryCodes.Canada:
					return new SupplyChainSecurityConfigurationCA();

				case CountryCodes.China:
					return new SupplyChainSecurityConfigurationCN();

				case CountryCodes.HongKong:
					return new SupplyChainSecurityConfigurationHK();

				case CountryCodes.Japan:
					return new SupplyChainSecurityConfigurationJP();

				case CountryCodes.Singapore:
					return new SupplyChainSecurityConfigurationSG();

				case CountryCodes.SouthAfrica:
					return new SupplyChainSecurityConfigurationZA();

				case CountryCodes.Taiwan:
					return new SupplyChainSecurityConfigurationTW();

				case CountryCodes.UnitedStates:
					return new SupplyChainSecurityConfigurationUS();

				case CountryCodes.UnitedKingdom:
					return new SupplyChainSecurityConfigurationUK();

				case CountryCodes.EuropeanUnion:
					return new SupplyChainSecurityConfigurationEU();
			}

			if (CountryCodes.EuropeanUnionAviationSecurityMembersList.Contains(countryCode))
			{
				return new SupplyChainSecurityConfigurationEU();
			}

			return new SupplyChainSecurityConfiguration();
		}

		#endregion

		public virtual ZString GetUncertifiedUserForScreeningError(GlbStaff staff)
		{
			return ZString.Empty;
		}

		public bool IsOnlyForDevelopers
		{
			get
			{
				var registryItem = GetRegistryItemToEnable<BooleanRegistryItem>();
				return registryItem != null && (registryItem.Options & RegistryOptions.IsOnlyForDevelopers) != 0;
			}
		}

		#region Is Enabled

		public virtual bool IsEnabled
		{
			get
			{
				var registryItem = GetRegistryItemToEnable<BooleanRegistryItem>();
				return registryItem != null && registryItem.Value;
			}
		}

		#endregion

		#region PermitDefaulting

		public virtual bool ShouldDefaultPermitToSecurityDeclaration => IsEnabled;

		#endregion

		#region Generic Scheme

		public bool UsesGenericScheme
		{
			get { return (RegistryItemKey.IsEmpty || IsOnlyForDevelopers) && !IsEnabled; }
		}

		#endregion

		#region Address Level Scheme

		public bool IsAddressLevelScheme => IsEnabled;

		#endregion

		#region Licence

		public virtual bool IsLicensedModule
		{
			get { return GetRegistryItemToEnable<BooleanRegistryItem>() != null; }
		}

		public virtual ZString LicenceEconomicGroupingCode
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Automatic Calculation Of Approved Status

		public virtual bool AllowAutomaticCalculationOfApprovedStatus(ForwardingShipment shipment) => true;

		#endregion

		#region Inspection Status Calculation

		public virtual bool IsInspectionTypeRecalculationRequired => false;

		public virtual bool AllowManualApprovedInspectionStatus(CommonShipment shipment) => false;

		internal virtual bool CheckAdditionalConditionsForApprovedInspectionType(ForwardingShipment shipment)
		{
			return true;
		}

		internal virtual void CheckJS_InspectionTypeCodeAdditionalValidation(ForwardingShipment shipment, ZPropertyInfo info, bool hasChanges)
		{
			switch (shipment.JS_InspectionTypeCode)
			{
				case ExemptionCodes.Codes.GovernmentApprovedReliableOrganization:
					info.AddError(Res.GetString("99d46b79-567d-4eb8-bf60-b83e6bee20a9", "The Inspection Type of GOV Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (b) Government approved reliable organization(s) can only be used for air shipments departing from EU, NO, CH, IS and LI", shipment.JS_InspectionTypeCode));
					break;
				case ExemptionCodes.Codes.AdHocMovementsOfCargo:
					info.AddError(Res.GetString("272bc01d-a3c5-4bcb-9df4-28907a1f0425", "The Inspection Type of ADH Exempt / Commission Implementing Decision C(2015) 8005, 6.2.3 (c) ad hoc movements of cargo can only be used for air shipments departing from EU, NO, CH, IS and LI", shipment.JS_InspectionTypeCode));
					break;
			}
		}

		internal virtual ZString GetErrorForApprovalInvalidForShipment(OrgCountryData countryData, ForwardingShipment shipment)
		{
			return ZString.Empty;
		}

		internal virtual bool GetPartyTypeChangeForcesRecalculateApproveShipperStatus(string partyType)
		{
			return false;
		}

		public virtual bool AllowRelevantOrganisationsWithoutAviationSecurityApproval => false;

		internal virtual ZString GetEditInspectionErrorForUncertifiedUser(ForwardingShipment shipment)
		{
			return ZString.Empty;
		}

		#endregion

		#region IsExportForAviationSecurityPurposes

		public bool IsExportForAviationSecurityPurposes(ISupplyChainSecurityImportExportSupporter importExportSupporter)
		{
			if (importExportSupporter != null)
			{
				if (IsExportForAviationSecurityPurposes(importExportSupporter.LoadCountryForSupplyChainSecurity, importExportSupporter.DischargeCountryForSupplyChainSecurity))
				{
					return true;
				}

				if (UseShipmentConsolsToDetermineExportForAviationSecurityPurposes && importExportSupporter is ForwardingShipment shipment && !shipment.IsImport())
				{
					if (shipment.Consols.Any(c => IsExportForAviationSecurityPurposes(c as ISupplyChainSecurityImportExportSupporter)))
					{
						return true;
					}

					if (shipment.Consols.Cast<ForwardingConsol>().Any(c => c.DischargePort?.Country != null && c.DischargePort.Country.UseEUAirCargoSecurityStandards))
					{
						return true;
					}
				}
			}

			return false;
		}

		protected virtual bool IsExportForAviationSecurityPurposes(ZString originCountryCode, ZString destinationCountryCode)
		{
			return originCountryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		protected virtual bool UseShipmentConsolsToDetermineExportForAviationSecurityPurposes => false;

		#endregion

		#region Organisations To Use

		internal Dictionary<string, OrganisationToUseForAviationSecurity> OrganisationsToUse
		{
			get
			{
				if (IsEnabled && (organisationsToUse != null || OrganisationsToUseCollection != null))
				{
					if (organisationsToUse == null)
					{
						organisationsToUse = new Dictionary<string, OrganisationToUseForAviationSecurity>();
						foreach (SupplyChainSecurityOrganisationToUse item in OrganisationsToUseCollection)
						{
							if (item.ValidationCode != SupplyChainSecurityOrganisationToUse.ValidationCodes.No)
							{
								organisationsToUse.Add(item.Code, new OrganisationToUseForAviationSecurity(item.Code, item.ValidationCode));
							}
						}
					}

					return organisationsToUse;
				}
				else
				{
					return DefaultOrganisationsToUse;
				}
			}
		}
		Dictionary<string, OrganisationToUseForAviationSecurity> organisationsToUse;

		protected virtual SupplyChainSecurityOrganisationToUseCollection OrganisationsToUseCollection => null;

		Dictionary<string, OrganisationToUseForAviationSecurity> DefaultOrganisationsToUse
		{
			get
			{
				if (defaultOrganisationsToUse == null)
				{
					defaultOrganisationsToUse = new Dictionary<string, OrganisationToUseForAviationSecurity>();
					var code = FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse.Value;
					defaultOrganisationsToUse.Add(code, new OrganisationToUseForAviationSecurity(code, SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes));
				}

				return defaultOrganisationsToUse;
			}
		}
		Dictionary<string, OrganisationToUseForAviationSecurity> defaultOrganisationsToUse;

		#endregion

		#region Approval Codes

		public CodeDescriptionPairList ApprovalCodesList
		{
			get
			{
				if (IsEnabled && enabledApprovalCodesList == null)
				{
					enabledApprovalCodesList = new CodeDescriptionPairList();
					foreach (var config in ApprovalCodeConfigurations)
					{
						enabledApprovalCodesList.AddPair(config.Key, config.Value.Description);
					}
				}
				else if (!IsEnabled && disabledApprovalCodesList == null)
				{
					disabledApprovalCodesList = new CodeDescriptionPairList();
					foreach (var config in DefaultApprovalCodeConfigurations)
					{
						disabledApprovalCodesList.AddPair(config.Key, config.Value.Description);
					}
				}

				return IsEnabled ? enabledApprovalCodesList : disabledApprovalCodesList;
			}
		}
		CodeDescriptionPairList enabledApprovalCodesList;
		CodeDescriptionPairList disabledApprovalCodesList;

		public ZString ApprovalCodeDescription(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) ? ApprovalCodeConfigurations[code].Description : ZString.Empty;
		}

		public bool ApprovalCodeIsValidForAviationSecurityApproval(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) && ApprovalCodeConfigurations[code].IsValidForAviationSecurity;
		}

		public bool ApprovalCodeHasRequiredDocumentValidation(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) && ApprovalCodeConfigurations[code].RequiredDocumentValidation;
		}

		public bool ApprovalCodeAllowsApprovalNumber(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) && ApprovalCodeConfigurations[code].AllowsApprovalNumber;
		}

		public bool ApprovalCodeRequiresApprovalNumber(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) && ApprovalCodeConfigurations[code].RequiresApprovalNumber;
		}

		public bool ApprovalCodeAllowsDuplicateApprovalNumber(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) && ApprovalCodeConfigurations[code].AllowsDuplicateApprovalNumbers;
		}

		public ZString ApprovalCodeApprovalNumberFormat(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) ? ApprovalCodeConfigurations[code].ApprovalNumberFormat : ZString.Empty;
		}

		public ZString ApprovalCodeErrorForApprovalNumberFormatNotMet(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) ? ApprovalCodeConfigurations[code].ErrorForApprovalNumberFormatNotMet : ZString.Empty;
		}

		public ZString ApprovalCodeErrorForOwnAgentApprovalNumberNotEntered(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) ? ApprovalCodeConfigurations[code].ErrorForOwnAgentApprovalNumberNotEntered : ZString.Empty;
		}

		public bool ApprovalCodeApprovalNumberMustMatchRequiredDocumentNumber(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) && ApprovalCodeConfigurations[code].ApprovalNumberMustMatchRequiredDocumentNumber;
		}

		public bool ApprovalCodeAllowsExpiryDate(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) && ApprovalCodeConfigurations[code].AllowsExpiryDate;
		}

		public bool ApprovalCodeRequiresExpiryDate(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) && ApprovalCodeConfigurations[code].RequiresExpiryDate;
		}

		public ZString ApprovalCodeWarningIfApprovalHasExpired(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) ? ApprovalCodeConfigurations[code].WarningIfApprovalHasExpired : ZString.Empty;
		}

		public ZInt ApprovalCodeMaximumValidityInYears(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) ? ApprovalCodeConfigurations[code].MaximumApprovalValidityInYears : ZInt.Zero;
		}

		public ZString ApprovalCodeErrorIfMaximumApprovalValidityExceeded(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) ? ApprovalCodeConfigurations[code].ErrorIfMaximumApprovalValidityExceeded : ZString.Empty;
		}

		public ZInt ApprovalCodeWarnIfApprovalWillLapseInMonths(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) ? ApprovalCodeConfigurations[code].WarnIfApprovalWillLapseInMonths : ZInt.Zero;
		}

		public ZString ApprovalCodeWarningIfApprovalWillLapseInMonths(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) ? ApprovalCodeConfigurations[code].WarningIfApprovalWillLapseInMonths : ZString.Empty;
		}

		public bool ApprovalCodeExpiryDateMustMatchRequiredDocumentExpiry(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) && ApprovalCodeConfigurations[code].ExpiryDateMustMatchRequiredDocumentExpiry;
		}

		public bool ApprovalCodeIsOrgLevelApproval(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) && ApprovalCodeConfigurations[code].IsOrgLevelApproval;
		}

		public IOrgAddress ApprovalCodeDefaultAddress(ZString code, IOrgHeader org)
		{
			if (ApprovalCodeConfigurations.ContainsKey(code))
			{
				var defaultAddressType = ApprovalCodeConfigurations[code].DefaultAddressType;
				if (defaultAddressType != null)
				{
					var orgHeader = (OrgHeader)org;
					return orgHeader.Addresses.DefaultAddressOfType(defaultAddressType) ?? orgHeader.MainAddress;
				}
			}

			return null;
		}

		public bool ApprovalCodeAddressIsReadOnly(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) && ApprovalCodeConfigurations[code].ReadOnlyAddress;
		}

		public bool ApprovalCodeIsAgentType(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) && ApprovalCodeConfigurations[code].IsAgentType;
		}

		public bool ApprovalCodeIsApprovedToShipOnPassengerFlights(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) && ApprovalCodeConfigurations[code].IsApprovedToShipOnPassengerFlights;
		}

		public bool ApprovalCodeNumberIsHiddenOnSecurityDeclaration(ZString code)
		{
			return ApprovalCodeConfigurations.ContainsKey(code) && ApprovalCodeConfigurations[code].HideApprovalNumberOnSecurityDeclaration;
		}

		#region Approval Code Configuration

		Dictionary<ZString, ApprovalCodeConfiguration> ApprovalCodeConfigurations
		{
			get
			{
				if (!IsEnabled)
				{
					return DefaultApprovalCodeConfigurations;
				}

				if (approvalCodeConfigurations == null)
				{
					approvalCodeConfigurations = GetApprovalCodeConfigurations();
				}

				return approvalCodeConfigurations;
			}
		}
		Dictionary<ZString, ApprovalCodeConfiguration> approvalCodeConfigurations;

		protected virtual Dictionary<ZString, ApprovalCodeConfiguration> GetApprovalCodeConfigurations()
		{
			return DefaultApprovalCodeConfigurations;
		}

		Dictionary<ZString, ApprovalCodeConfiguration> DefaultApprovalCodeConfigurations
		{
			get
			{
				if (defaultApprovalCodeConfigurations == null)
				{
					defaultApprovalCodeConfigurations = new Dictionary<ZString, ApprovalCodeConfiguration>();
					defaultApprovalCodeConfigurations.Add(AviationSecuritySchemeMembershipEx.Codes.Yes, new ApprovalCodeConfiguration(AviationSecuritySchemeMembershipEx.Descriptions.Yes)
					{
						IsValidForAviationSecurity = true,
						RequiredDocumentValidation = true
					});

					defaultApprovalCodeConfigurations.Add(AviationSecuritySchemeMembershipEx.Codes.No, new ApprovalCodeConfiguration(AviationSecuritySchemeMembershipEx.Descriptions.No)
					{
						IsApprovedToShipOnPassengerFlights = false
					});
				}

				return defaultApprovalCodeConfigurations;
			}
		}
		Dictionary<ZString, ApprovalCodeConfiguration> defaultApprovalCodeConfigurations;

		protected class ApprovalCodeConfiguration
		{
			public ApprovalCodeConfiguration(MultilingualString description)
			{
				Description = description;
			}

			public MultilingualString Description { get; internal set; }
			public bool IsValidForAviationSecurity { get; internal set; }
			public bool AllowsApprovalNumber { get; internal set; }
			public ZString ApprovalNumberFormat { get; internal set; }
			public ZString ErrorForApprovalNumberFormatNotMet { get; internal set; }
			public ZString ErrorForOwnAgentApprovalNumberNotEntered { get; internal set; }
			public bool AllowsDuplicateApprovalNumbers { get; internal set; }
			public bool RequiredDocumentValidation { get; internal set; }
			public bool AllowsExpiryDate { get; internal set; }
			public ZString ErrorIfMaximumApprovalValidityExceeded { get; internal set; }
			public ZString WarningIfApprovalWillLapseInMonths { get; internal set; }
			public bool IsOrgLevelApproval { get; internal set; }
			public OrgAddressType DefaultAddressType { get; set; }
			public bool ReadOnlyAddress { get; set; }
			public bool IsAgentType { get; internal set; }
			public bool HideApprovalNumberOnSecurityDeclaration { get; internal set; }

			public bool RequiresExpiryDate
			{
				get { return requiresExpiryDate; }
				set
				{
					requiresExpiryDate = value;
					if (value)
					{
						AllowsExpiryDate = true;
					}
				}
			}
			bool requiresExpiryDate;

			public bool RequiresApprovalNumber
			{
				get { return requiresApprovalNumber; }
				internal set
				{
					requiresApprovalNumber = value;
					if (value)
					{
						AllowsApprovalNumber = true;
					}
				}
			}
			bool requiresApprovalNumber;

			public bool ApprovalNumberMustMatchRequiredDocumentNumber
			{
				get { return approvalNumberMustMatchRequiredDocumentNumber; }
				set
				{
					approvalNumberMustMatchRequiredDocumentNumber = value;

					if (approvalNumberMustMatchRequiredDocumentNumber)
					{
						AllowsApprovalNumber = true;
						RequiredDocumentValidation = true;
					}
				}
			}
			bool approvalNumberMustMatchRequiredDocumentNumber;

			public bool ExpiryDateMustMatchRequiredDocumentExpiry
			{
				get { return expiryDateMustMatchRequiredDocumentExpiry; }
				internal set
				{
					expiryDateMustMatchRequiredDocumentExpiry = value;
					if (value)
					{
						RequiresExpiryDate = true;
						RequiredDocumentValidation = true;
					}
				}
			}
			bool expiryDateMustMatchRequiredDocumentExpiry;

			public ZInt MaximumApprovalValidityInYears
			{
				get { return maximumApprovalValidityInYears; }
				internal set
				{
					maximumApprovalValidityInYears = value;
					if (value > 0)
					{
						RequiresExpiryDate = true;
					}
				}
			}
			ZInt maximumApprovalValidityInYears;

			public ZString WarningIfApprovalHasExpired
			{
				get { return warningIfApprovalHasExpired; }
				internal set
				{
					warningIfApprovalHasExpired = value;
					if (!value.IsEmpty)
					{
						RequiresExpiryDate = true;
					}
				}
			}
			ZString warningIfApprovalHasExpired;

			public ZInt WarnIfApprovalWillLapseInMonths
			{
				get { return warnIfApprovalWillLapseInMonths; }
				internal set
				{
					warnIfApprovalWillLapseInMonths = value;
					if (value > 0)
					{
						RequiresExpiryDate = true;
					}
				}
			}
			ZInt warnIfApprovalWillLapseInMonths;

			public bool IsApprovedToShipOnPassengerFlights
			{
				get { return isApprovedToShipOnPassengerFlights; }
				internal set { isApprovedToShipOnPassengerFlights = value; }
			}
			bool isApprovedToShipOnPassengerFlights = true;
		}

		#endregion

		public virtual ZString GetErrorForApprovalCodeInvalidForCountry(ZString approvalCode, ZString countryCode)
		{
			return ZString.Empty;
		}

		public virtual ZString GetWarningForAdditionalApprovalCodeValidation(IOrgCountryData countryData)
		{
			return ZString.Empty;
		}

		public OrgCountryData GetApproval(OrgAddress address)
		{
			if (address == null)
			{
				return null;
			}

			var approval = (OrgCountryData)null;
			if (IsAddressLevelScheme
				&& address.KnownShipper != null)
			{
				approval = address.KnownShipper;
			}
			else
			{
				approval = GetApproval(address.Header);
			}

			return approval;
		}

		public OrgCountryData GetApproval(OrgHeader header)
		{
			if (IsAddressLevelScheme)
			{
				return header?.Addresses
					.Cast<OrgAddress>()
					.Select(address => address.KnownShipper)
					.FirstOrDefault(approval => approval != null
						&& ApprovalCodeIsOrgLevelApproval(approval.OV_EXApprovedOrMajorExporter));
			}
			else
			{
				return header?.CountryData != null && header.CountryData.OV_EXApprovedOrMajorExporter != AviationSecuritySchemeMembershipEx.Codes.No
					? header.CountryData
					: null;
			}
		}

		#endregion

		#region Known Shipper Filter

		public virtual ZString KnownShipperFilterText => (NoResString)"Known/Approved Status"; // Filter Constant

		public virtual MultilingualString KnownShipperFilterDescription => ResString.GetMultilingualString("5f6fc658-c10d-4ad0-8183-22a55c351db5", "Known/Approved Status");

		public CodeDescriptionPairList KnownShipperFilterList
		{
			get
			{
				if (knownShipperFilterList == null)
				{
					knownShipperFilterList = GetKnownShipperFilterList();
				}

				return knownShipperFilterList;
			}
		}
		CodeDescriptionPairList knownShipperFilterList;

		protected CodeDescriptionPairList GetKnownShipperFilterList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair("ALL", Res.GetString("48184711-81a8-49fa-b2d1-05688c0f144e", "All"));

			foreach (var code in ApprovalCodesList.GetAllCodes())
			{
				if (code == AviationSecuritySchemeMembershipEx.Codes.Yes)
				{
					result.AddPair(code, FilterTextYes);
				}
				else if (code == AviationSecuritySchemeMembershipEx.Codes.No)
				{
					result.AddPair(code, FilterTextNo);
				}
				else
				{
					result.AddPair(code, ApprovalCodesList[code, System.StringComparison.Ordinal].Description);
				}
			}

			return result;
		}

		protected virtual ZString FilterTextNo => Res.GetString("ad74dc3c-c9b6-47d0-81ad-ba75029ec868", "Not Approved Shipper/Exporter");

		protected virtual ZString FilterTextYes => Res.GetString("e20b4d57-613e-4c14-bec9-479ca6b6edb0", "Approved Shipper/Exporter");

		#endregion

		#region Approval Number Filter

		public virtual ZString ApprovalNumberFilterText => (NoResString)"Known/Approved ID Number"; // Filter Constant

		public virtual MultilingualString ApprovalNumberFilterDescription => ResString.GetMultilingualString("c0211e88-c887-4f68-a32e-60e2bc954741", "Known/Approved ID Number");

		#endregion

		#region Additional Inspection Types

		public CodeDescriptionPairList AdditionalInspectionTypeList
		{
			get
			{
				if (additionalInspectionTypeList == null)
				{
					additionalInspectionTypeList = new CodeDescriptionPairList();
					foreach (ShipmentInspectionType shipmentInspectionType in ShipmentAdditionalInspectionTypeCollection.Where(x => ((ShipmentInspectionType)x).ShowInList))
					{
						additionalInspectionTypeList.AddPair(shipmentInspectionType.Code, shipmentInspectionType.Description);
					}
				}

				return additionalInspectionTypeList;
			}
		}
		CodeDescriptionPairList additionalInspectionTypeList;

		public ShipmentInspectionTypeCollection ShipmentAdditionalInspectionTypeCollection
		{
			get
			{
				return new ShipmentInspectionTypes(ZString.Empty, new ShipmentInspectionTypeLists(RegistryFactory.Instance).SystemDefinedAdditionalInspectionalTypeList).Types;
			}
		}

		#endregion

		#region Inspection Types

		public ShipmentInspectionTypeCollection ShipmentInspectionTypeCollection
		{
			get
			{
				if (!IsEnabled)
				{
					return FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value.Types;
				}

				return GetShipmentInspectionTypeCollection();
			}
		}

		protected virtual ShipmentInspectionTypeCollection GetShipmentInspectionTypeCollection()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypeRegistryItem.Value.Types;
		}

		public CodeDescriptionPairList InspectionTypeList
		{
			get
			{
				if (inspectionTypeList == null)
				{
					inspectionTypeList = new CodeDescriptionPairList();
					foreach (ShipmentInspectionType shipmentInspectionType in ShipmentInspectionTypeCollection.Where(x => ((ShipmentInspectionType)x).ShowInList))
					{
						inspectionTypeList.AddPair(shipmentInspectionType.Code, shipmentInspectionType.Description);
					}
				}

				return inspectionTypeList;
			}
		}
		CodeDescriptionPairList inspectionTypeList;

		public bool IsInspectionTypeAllowedOnPassengerFlights(ZString code)
		{
			if (code == BaseJobShipmentLookups.InspectionType_Approved || code == FreightDataRegistry.AviationSecurity_Unknown_Code)
			{
				return true;
			}

			var shipmentInspectionType = (ShipmentInspectionType)ShipmentInspectionTypeCollection.FindByCode(code);
			return shipmentInspectionType == null || shipmentInspectionType.AllowedOnPassengerFlights && shipmentInspectionType.ShowInList;
		}

		public virtual ZString InspectionTypeDefault
		{
			get
			{
				if (!UsesGenericScheme && !IsEnabled)
				{
					return ZString.Empty;
				}
				else if (IsEnabled)
				{
					return GetInspectionTypeDefault();
				}

				return FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.Value;
			}
		}

		public virtual bool IsRecalculationNeeded(ZString inspectionTypeCode) => true;

		protected virtual ZString GetInspectionTypeDefault()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypeDefault.Value;
		}

		public virtual bool ShouldReDefaultPackLineInspectionTypeCodes(ForwardingShipment shipment) => true;

		public virtual bool IsSecuredForPackLineInspectionType => false;

		#endregion

		#region Regulated Agent

		public virtual OrgCountryData GetAgentApproval(ForwardingConsol consol)
		{
			return GetApproval(consol?.SendingForwarderAddress);
		}

		#endregion

		#region Passenger Flight Validation

		public virtual bool PassengerFlightValidationApplies => true;

		#endregion

		#region Transhipment

		public virtual ZBool UseTranshipmentAviationSecurityStatus => false;

		public bool IsTranshipment(ISupplyChainSecurityImportExportSupporter importExportSupporter)
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			return importExportSupporter != null
				&& !importExportSupporter.LoadCountryForSupplyChainSecurity.IsEmpty
				&& importExportSupporter.LoadCountryForSupplyChainSecurity != countryCode
				&& !importExportSupporter.DischargeCountryForSupplyChainSecurity.IsEmpty
				&& importExportSupporter.DischargeCountryForSupplyChainSecurity != countryCode
				&& !IsExportForAviationSecurityPurposes(importExportSupporter)
				&& importExportSupporter.SupplyChainSecurityRelatedTransports.Any(x => x.JW_RL_NKLoadPort.SubstringSafe(0, 2) == countryCode || x.JW_RL_NKDiscPort.SubstringSafe(0, 2) == countryCode);
		}

		#endregion

		#region High Risk Requirements

		public virtual bool IsHighRiskApplicable => false;

		#endregion

		#region User Certification

		public virtual ZString GetSpecialHandlingErrorForUnauthorizedUser(ForwardingConsol consol, ZString code, bool hasChanges)
		{
			if (SCSSupportedCountryList.Any(countryCode => GlbCompany.CurrentCompany.GC_RN_NKCountryCode == countryCode && consol.JK_RL_NKLoadPort.StartsWith(countryCode))
				&& code == Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft
				&& hasChanges
				&& !Env.Security.MaintainConsolAllowToOverrideSecurityStatus.IsAllowed)
			{
				return Res.GetString("06f53e76-8b6a-4743-be74-cdf1cc30eccb", "Ensure you have Security Rights 'Operate > Forwarding > Consolidations > Allow to override Security Status' granted before setting the Consol’s secured status to \"SPX\".");
			}

			return ZString.Empty;
		}

		public virtual ZString GetStaffHandlingSecuredCargoUncertificatedErrorMessages() => ZString.Empty;

		#endregion

		#region Consignment Security Declaration

		public virtual bool UseConsignmentSecurityDeclaration => true;

		public virtual bool AllowIncludeECSD
		{
			get { return true; }
		}

		public virtual bool IncludeECSDByDefault
		{
			get
			{
				return IsEnabled && AllowIncludeECSD;
			}
		}

		#endregion

		#region AWB

		public virtual bool ShowSecurityStatusOnHAWB
		{
			get { return false; }
		}

		public virtual bool ShowSecurityStatusOnMAWB
		{
			get { return false; }
		}

		public virtual ZString GetMAWBKnownConsignorCode(ForwardingConsol consol)
		{
			return ZString.Empty;
		}

		public virtual ZString GetAuthorizedSenderNumber(ForwardingConsol consol)
		{
			return ZString.Empty;
		}

		public virtual bool UseConsignorApprovalNumberAsAgentApprovedExporterNumber
		{
			get { return false; }
		}

		public virtual bool UseAccountConsignorValidationForPassengerAircraft
		{
			get { return false; }
		}

		public virtual ZString AWBRANumber
		{
			get { return ZString.Empty; }
		}

		public virtual IEnumerable<ZString> GetAdditionalSecurityInformations(ForwardingConsol consol)
		{
			if (consol == null || consol.AWBHeader.EH_AdditionalSecurityInformation.IsEmpty)
			{
				return Enumerable.Empty<ZString>();
			}
			return new List<ZString>() { consol.AWBHeader.EH_AdditionalSecurityInformation };
		}

		public virtual bool ShouldSetScheduledArrivalDate(ForwardingConsol consol)
		{
			return false;
		}

		public virtual bool ShouldSetSecurityStatement
		{
			get { return false; }
		}

		public bool IsVerifiedKnownConsignor(ForwardingShipment shipment)
		{
			if (shipment.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved)
			{
				return true;
			}

			var approvalParties = shipment.AviationSecurity.GetAviationSecurityRelevantParties(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes)
					.Concat(shipment.AviationSecurity.GetAviationSecurityRelevantParties(SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning));

			if (!approvalParties.Any())
			{
				return false;
			}

			bool HasValidApprovalStatus(string orgCode)
			{
				return approvalParties
					.Any(party => party.OrganisationCode == orgCode
								&& party.KnownShipperRecord != null
								&& GetErrorForApprovalInvalidForShipment(party.KnownShipperRecord, shipment).IsEmpty
								&& !party.ExpiryDateWillLapseBeforeShipmentDateForAviationSecurity
								&& party.IsAviationSecurityApproved);
			}

			if (approvalParties.Any(x => x.OrganisationCode == SupplyChainSecurityOrganisationTypes.Consignor))
			{
				return HasValidApprovalStatus(SupplyChainSecurityOrganisationTypes.Consignor);
			}

			return HasValidApprovalStatus(SupplyChainSecurityOrganisationTypes.LocalClient);
		}

		#endregion

		#region Required Document

		public virtual bool UseApprovedOrganisationRequiredDocTypeSpecifiedInRegistry
		{
			get { return !IsEnabled; }
		}

		#endregion

		#region Security Declaration

		public virtual void CheckEH_AgentApprovalNumberAdditionalValidation(ConsolExportAWBHeader header)
		{
			if (header == null)
			{
				return;
			}

			ValidateEH_AgentApprovalNumberFormat(header);
		}

		protected virtual void ValidateEH_AgentApprovalNumberFormat(ConsolExportAWBHeader header)
		{
			if (!header.EH_AgentApprovalNumber.IsEmpty && !Regex.IsMatch(header.EH_AgentApprovalNumber, @"^(?!RA)[A-Z0-9\-]*$"))
			{
				header.EH_AgentApprovalNumberInfo.AddWarning(Res.GetString("0c9f00a9-0f3c-4a69-8ba9-c37d332fbc3a", "Regulated Agent’s approval number should only contain capital letters, numbers or hyphens. The \"RA\" prefix of the code is not required here."));
			}
		}

		public virtual ZBool ExportAWBAgentApprovalNumberCanBeOverridden
		{
			get { return true; }
		}

		public virtual bool ShowPrefixOnAgentApprovalNumber
		{
			get { return false; }
		}

		public ZBool GetCargoSecureForAllCargoAircraftOnlyIsAllowed(ForwardingConsol consol)
		{
			return GetErrorForCargoSecureForAllCargoAircraftOnlyIsNotAllowed(consol).IsEmpty;
		}

		public ZString GetErrorForCargoSecureForAllCargoAircraftOnlyIsNotAllowed(ForwardingConsol consol)
		{
			if (!IsEnabled)
			{
				return ZString.Empty;
			}

			return GetErrorForCargoSecureForAllCargoAircraftOnlyIsNotAllowedCore(consol);
		}

		protected virtual ZString GetErrorForCargoSecureForAllCargoAircraftOnlyIsNotAllowedCore(ForwardingConsol consol)
		{
			return ZString.Empty;
		}

		public virtual bool AllowDefaultingCargoSecureForPassengerAndAllCargoAircraft(ForwardingConsol consol)
		{
			return true;
		}

		#endregion

		#region Pack Level Screening

		internal virtual bool JL_IsHighRisk_ReadOnly(ForwardingShipment shipment)
		{
			return !IsHighRiskApplicable;
		}

		internal virtual bool JL_AdditionalInspectionTypeCode_ReadOnly(ForwardingShipment shipment)
		{
			return !IsHighRiskApplicable;
		}

		internal bool IsPackLevelScreeningRequired(ISupplyChainSecurityImportExportSupporter importExportSupporter)
		{
			if (importExportSupporter == null
				|| !importExportSupporter.IsAir
				|| !(UsesGenericScheme || IsEnabled))
			{
				return false;
			}

			var isExportForAviationSecurityPurposes = IsExportForAviationSecurityPurposes(importExportSupporter);
			if (isExportForAviationSecurityPurposes && IsOriginPackLevelScreeningRequired(importExportSupporter))
			{
				return true;
			}

			if (isExportForAviationSecurityPurposes || IsTranshipment(importExportSupporter))
			{
				return GetDestinationSupplyChainSecurityCountries(importExportSupporter).Any(c => GetDestinationSupplyChainSecurity(c).IsDestinationPackLevelScreeningRequired);
			}

			return false;
		}

		public virtual bool IsPackLevelScreeningAvailable(ISupplyChainSecurityImportExportSupporter importExportSupporter)
		{
			if (importExportSupporter == null
				|| !importExportSupporter.IsAir
				|| !(UsesGenericScheme || IsEnabled))
			{
				return false;
			}

			return IsExportForAviationSecurityPurposes(importExportSupporter) || IsTranshipment(importExportSupporter);
		}

		protected virtual bool IsOriginPackLevelScreeningRequired(ISupplyChainSecurityImportExportSupporter importExportSupporter)
		{
			return false;
		}

		protected virtual bool IsDestinationPackLevelScreeningRequired => false;

		protected virtual bool IsDestinationPackLevelScreeningAvailable(ForwardingShipment shipment)
		{
			return IsDestinationPackLevelScreeningRequired;
		}

		internal virtual bool JL_InspectionTypeCode_ReadOnly(ForwardingShipment shipment)
		{
			if (IsPackLevelScreeningAvailable(shipment))
			{
				if (shipment.JS_InspectionTypeCode != BaseJobShipmentLookups.InspectionType_Approved)
				{
					return false;
				}

				foreach (var countryToCheck in GetDestinationSupplyChainSecurityCountries(shipment))
				{
					var destinationSupplyChainSecurityConfiguration = GetDestinationSupplyChainSecurity(countryToCheck);
					if (destinationSupplyChainSecurityConfiguration.IsDestinationPackLevelScreeningRequired
						&& destinationSupplyChainSecurityConfiguration.IsDestinationPackLevelScreeningAvailable(shipment))
					{
						return false;
					}
				}
			}

			return true;
		}

		internal void CheckJL_InspectionTypeCode_AdditionalValidation(ForwardingPackLine packline)
		{
			if (!packline.IsOuterPackType)
			{
				return;
			}
			var shipment = packline.Shipment;
			if (shipment != null && IsPackLevelScreeningRequired(shipment))
			{
				if (!packline.JL_InspectionTypeCodeInfo.ReadOnly)
				{
					if (shipment.IsFirstAirLegFlightDeparted() && !packline.JL_InspectionTypeCodeHasChanges)
					{
						return;
					}

					ListValidation.ErrorIfInvalidCode(packline.JL_InspectionTypeCodeInfo);
				}

				if (IsEnabled || UsesGenericScheme)
				{
					if (!packline.JL_InspectionTypeCodeInfo.ReadOnly
						&& shipment.JS_InspectionTypeCode != BaseJobShipmentLookups.InspectionType_Approved
						&& packline.JL_InspectionTypeCode.IsEmpty)
					{
						MandatoryValidation.CheckEntered(packline.JL_InspectionTypeCodeInfo);
					}

					CheckJL_InspectionTypeCode_OriginValidation(packline);
					CheckJL_InspectionTypeCode_DestinationValidation(packline);
				}
			}

			if (shipment != null
				&& IsPackLevelScreeningAvailable(shipment))
			{
				if (packline.JL_InspectionTypeCodeHasChanges
					&& packline.JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_PackLine_IsSecured_Code
					&& !packline.IsMarkingSecuredValid)
				{
					packline.JL_InspectionTypeCodeInfo.AddError(Res.GetString("a1e7aaf3-a3c6-49a3-ab64-3601f1cf770f", "'Approved' cannot be selected here. It is provided as an automatic update from the linked Transit Warehouse."));
				}

				if (shipment.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Screened
					|| shipment.OuterPackLines.Cast<ForwardingPackLine>().Any(p => !p.JL_InspectionTypeCode.IsEmpty))
				{
					MandatoryValidation.CheckEntered(packline.JL_InspectionTypeCodeInfo);
				}

				if (packline.JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code
					&& shipment.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Screened)
				{
					packline.JL_InspectionTypeCodeInfo.AddError(Res.GetString("0cf7eafd-a757-40c5-9265-9dc52acc983a", "The packline Inspection cannot be UNK as the Shipment Inspection method is SCR – Screened, which means each packline of the shipment is screened."));
				}
			}
		}

		void CheckJL_InspectionTypeCode_OriginValidation(ForwardingPackLine packline)
		{
			if ((packline.JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code || packline.JL_InspectionTypeCode.IsEmpty)
				&& (packline.JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code || packline.Shipment.JS_InspectionTypeCode != BaseJobShipmentLookups.InspectionType_Approved)
				&& IsOriginPackLevelScreeningRequired(packline.Shipment)
				&& IsExportForAviationSecurityPurposes(packline.Shipment))
			{
				packline.JL_InspectionTypeCodeInfo.AddWarning(Res.GetString("e91ea137-03a3-4f5d-9c69-c270a2dee133",
					"In line with {0}, a screening status must be recorded for all packages, unless the shipment is APP.",
					PackLevelScreeningSchemeName));
			}
		}

		void CheckJL_InspectionTypeCode_DestinationValidation(ForwardingPackLine packline)
		{
			foreach (var countryToCheck in GetDestinationSupplyChainSecurityCountries(packline.Shipment))
			{
				GetDestinationSupplyChainSecurity(countryToCheck).CheckJL_InspectionTypeCode_DestinationValidationCore(packline);
			}
		}

		protected virtual void CheckJL_InspectionTypeCode_DestinationValidationCore(ForwardingPackLine packline)
		{
		}

		internal void ValidateSendFWBPackLineInspectionTypes(ForwardingConsol consol, ZPropertyInfo info)
		{
			if (IsPackLevelScreeningRequired(consol))
			{
				CheckSendFWBOriginValidation(consol, info);

				foreach (var countryToCheck in GetDestinationSupplyChainSecurityCountries(consol))
				{
					GetDestinationSupplyChainSecurity(countryToCheck).CheckSendFWBDestinationValidation(consol, info);
				}
			}
		}

		void CheckSendFWBOriginValidation(ForwardingConsol consol, ZPropertyInfo info)
		{
			if (IsOriginPackLevelScreeningRequired(consol) && IsExportForAviationSecurityPurposes(consol))
			{
				var hasShipmentsMissingPackLevelScreening = consol.Shipments.Cast<ForwardingShipment>()
					.SelectMany(s => s.OuterPackLines).Cast<ForwardingPackLine>()
					.Any(p => p.JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code
						|| (p.Shipment.JS_InspectionTypeCode != BaseJobShipmentLookups.InspectionType_Approved && p.JL_InspectionTypeCode.IsEmpty));
				if (hasShipmentsMissingPackLevelScreening)
				{
					info.AddWarning(Res.GetString("9be1f650-5cb4-4fc1-b642-5fd5d4c056aa",
						"In line with {0}, a screening status must be recorded for all packages, unless the shipment is APP.",
						PackLevelScreeningSchemeName));
				}
			}
		}

		protected virtual void CheckSendFWBDestinationValidation(ForwardingConsol consol, ZPropertyInfo info)
		{
		}

		internal void CheckEAS_ScreeningMethod_PackLevelScreeningValidation(ExportAWBSecurityStatusLine parent, ForwardingConsol consol)
		{
			if (IsPackLevelScreeningRequired(consol))
			{
				CheckEAS_ScreeningMethod_OriginPackLevelScreeningValidation(parent, consol);
				CheckEAS_ScreeningMethod_DestinationPackLevelScreeningValidation(parent, consol);
			}
		}

		void CheckEAS_ScreeningMethod_OriginPackLevelScreeningValidation(ExportAWBSecurityStatusLine parent, ForwardingConsol consol)
		{
			if (parent.EAS_ScreeningMethod == FreightDataRegistry.AviationSecurity_Unknown_Code)
			{
				var unsecuredShipments = consol
						.Shipments
						.Cast<ForwardingShipment>()
						.Where(shipment => shipment.IsFHLShipment()
							&& shipment.OuterPackLines.Any(packLine => ((ForwardingPackLine)packLine).JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code))
						.Select(shipment => shipment.JS_UniqueConsignRef)
						.Take(3);

				if (IsOriginPackLevelScreeningRequired(consol) && IsExportForAviationSecurityPurposes(consol))
				{
					parent.EAS_ScreeningMethodInfo.AddMessageError(Res.GetString("1f369ea7-ef98-4b15-90b6-935ea30acf84", "Shipment/s {0} require a screening status to be recorded for all packages, in line with {1}, unless the shipment is APP.",
						string.Join(", ", unsecuredShipments),
						PackLevelScreeningSchemeName));
				}
			}
		}

		void CheckEAS_ScreeningMethod_DestinationPackLevelScreeningValidation(ExportAWBSecurityStatusLine parent, ForwardingConsol consol)
		{
			foreach (var countryToCheck in GetDestinationSupplyChainSecurityCountries(consol))
			{
				GetDestinationSupplyChainSecurity(countryToCheck).CheckEAS_ScreeningMethod_DestinationPackLevelScreeningValidationCore(parent, consol);
			}
		}

		protected virtual void CheckEAS_ScreeningMethod_DestinationPackLevelScreeningValidationCore(ExportAWBSecurityStatusLine parent, ForwardingConsol consol)
		{
		}

		protected virtual ZString PackLevelScreeningSchemeName => ZString.Empty;

		#endregion

		#region Prohibited Routing

		public virtual ZString GetWarningForProhibitedRouting(ISupplyChainSecurityImportExportSupporter supportedBO)
		{
			return ZString.Empty;
		}

		#endregion

		#region Related Countries For Supply Chain Security

		public IEnumerable<string> RelatedCountriesForSupplyChainSecurity => GetRelatedCountriesForSupplyChainSecurityCore();

		protected virtual IEnumerable<string> GetRelatedCountriesForSupplyChainSecurityCore() => System.Array.Empty<string>();

		#endregion

		#region Destination Supply Chain Security

		protected IEnumerable<ZString> GetDestinationSupplyChainSecurityCountries(ISupplyChainSecurityImportExportSupporter supporter)
		{
			return new List<ZString>(supporter.SupplyChainSecurityRelatedTransports
				.Where(t => ImportExportHelper.GetJobDirection(t.JW_RL_NKLoadPort, t.JW_RL_NKDiscPort) != Directions.Domestic)
				.Select(t => t.JW_RL_NKDiscPort.SubstringSafe(0, 2)))
				.Append(supporter.DischargeCountryForSupplyChainSecurity)
				.Distinct();
		}

		protected SupplyChainSecurityConfiguration GetDestinationSupplyChainSecurity(ZString countryCode)
		{
			if (!destinationSupplyChainSecurityConfigurationCache.ContainsKey(countryCode))
			{
				destinationSupplyChainSecurityConfigurationCache.Add(countryCode, SupplyChainSecurityConfiguration.New(countryCode));
			}

			return destinationSupplyChainSecurityConfigurationCache[countryCode];
		}

		readonly Dictionary<ZString, SupplyChainSecurityConfiguration> destinationSupplyChainSecurityConfigurationCache = new Dictionary<ZString, SupplyChainSecurityConfiguration>();

		#endregion

		#region Registry

		public ZString RegistryItemKey
		{
			get
			{
				var registryItem = GetRegistryItemToEnable<BooleanRegistryItem>();
				return registryItem == null ? ZString.Empty : (ZString)string.Concat(registryItem.Category, "/", registryItem.Caption);
			}
		}

		protected virtual T GetRegistryItemToEnable<T>() where T : BooleanRegistryItem
		{
			return null;
		}

		#endregion

		#region Issuing Authority Country

		public virtual bool UseIssuingAuthorityCountry => false;

		#endregion

		#region GetEditApprovalErrorForUncertifiedUser

		public virtual ZString GetEditApprovalErrorForUncertifiedUser(IOrgCountryData countryData) => ZString.Empty;

		#endregion

		#region Aviation Security Freight Movement Restricted

		public virtual bool IsAviationSecurityFreightMovementRestricted(ISupplyChainSecurityImportExportSupporter businessObject) => false;

		public virtual MultilingualString AviationSecurityFreightMovementRestrictedErrorMessage => null;

		public virtual bool IsUserCertifiedForAviationSecurity(IGlbStaff staff) => false;

		#endregion
	}
}
