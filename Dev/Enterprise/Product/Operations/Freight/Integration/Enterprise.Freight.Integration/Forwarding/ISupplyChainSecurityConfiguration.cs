using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Integration
{
	public static partial class Forwarding
	{
		public interface ISupplyChainSecurityConfiguration
		{
			bool IsEnabled { get; }
			bool UsesGenericScheme { get; }
			bool IsLicensedModule { get; }
			bool IsAddressLevelScheme { get; }
			bool IsOnlyForDevelopers { get; }
			bool IsHighRiskApplicable { get; }
			bool PassengerFlightValidationApplies { get; }
			bool UseApprovedOrganisationRequiredDocTypeSpecifiedInRegistry { get; }
			bool IsInspectionTypeAllowedOnPassengerFlights(ZString code);
			bool IsExportForAviationSecurityPurposes(ISupplyChainSecurityImportExportSupporter importExportSupporter);
			bool UseIssuingAuthorityCountry { get; }
			bool IsRecalculationNeeded(ZString inspectionTypeCode);

			ZString RegistryItemKey { get; }
			ZString InspectionTypeDefault { get; }
			ZString LicenceEconomicGroupingCode { get; }

			CodeDescriptionPairList ApprovalCodesList { get; }
			CodeDescriptionPairList InspectionTypeList { get; }
			CodeDescriptionPairList AdditionalInspectionTypeList { get; }

			ZString KnownShipperFilterText { get; }
			MultilingualString KnownShipperFilterDescription { get; }
			CodeDescriptionPairList KnownShipperFilterList { get; }

			ZString ApprovalNumberFilterText { get; }
			MultilingualString ApprovalNumberFilterDescription { get; }

			#region Approval Codes

			bool ApprovalCodeHasRequiredDocumentValidation(ZString approvalCode);
			bool ApprovalCodeApprovalNumberMustMatchRequiredDocumentNumber(ZString approvalCode);
			bool ApprovalCodeAllowsApprovalNumber(ZString approvalCode);
			bool ApprovalCodeRequiresApprovalNumber(ZString approvalCode);
			ZString ApprovalCodeApprovalNumberFormat(ZString approvalCode);
			ZString ApprovalCodeErrorForApprovalNumberFormatNotMet(ZString approvalCode);
			ZString ApprovalCodeErrorForOwnAgentApprovalNumberNotEntered(ZString approvalCode);
			bool ApprovalCodeAllowsDuplicateApprovalNumber(ZString approvalCode);
			bool ApprovalCodeIsOrgLevelApproval(ZString approvalCode);
			IOrgAddress ApprovalCodeDefaultAddress(ZString approvalCode, IOrgHeader org);
			bool ApprovalCodeAddressIsReadOnly(ZString approvalCode);
			ZString GetErrorForApprovalCodeInvalidForCountry(ZString approvalCode, ZString countryCode);
			ZString GetWarningForAdditionalApprovalCodeValidation(IOrgCountryData countryData);
			#endregion

			#region Expiry Date

			bool ApprovalCodeAllowsExpiryDate(ZString approvalCode);
			bool ApprovalCodeRequiresExpiryDate(ZString approvalCode);
			ZString ApprovalCodeWarningIfApprovalHasExpired(ZString approvalCode);
			ZInt ApprovalCodeMaximumValidityInYears(ZString approvalCode);
			ZString ApprovalCodeErrorIfMaximumApprovalValidityExceeded(ZString code);
			ZInt ApprovalCodeWarnIfApprovalWillLapseInMonths(ZString approvalCode);
			ZString ApprovalCodeWarningIfApprovalWillLapseInMonths(ZString approvalCode);
			bool ApprovalCodeExpiryDateMustMatchRequiredDocumentExpiry(ZString code);

			#endregion

			#region Prohibited Routing

			ZString GetWarningForProhibitedRouting(ISupplyChainSecurityImportExportSupporter supportedBO);

			#endregion

			#region GetEditApprovalErrorForUncertifiedUser

			ZString GetEditApprovalErrorForUncertifiedUser(IOrgCountryData countryData);

			#endregion

			#region AviationSecurityFreightMovementRestricted

			bool IsAviationSecurityFreightMovementRestricted(ISupplyChainSecurityImportExportSupporter businessObject);

			MultilingualString AviationSecurityFreightMovementRestrictedErrorMessage { get; }

			bool IsUserCertifiedForAviationSecurity(IGlbStaff staff);

			#endregion
		}
	}
}
