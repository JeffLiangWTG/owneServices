using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Business
{
	public class SupplyChainSecurityConfigurationHK : SupplyChainSecurityConfiguration
	{
		#region AWB

		public override bool ShowSecurityStatusOnHAWB
		{
			get { return true; }
		}

		public override bool ShowSecurityStatusOnMAWB
		{
			get { return true; }
		}

		public override bool UseConsignorApprovalNumberAsAgentApprovedExporterNumber
		{
			get { return true; }
		}

		public override bool UseAccountConsignorValidationForPassengerAircraft
		{
			get { return true; }
		}

		#endregion

		#region Security Declaration

		protected override void ValidateEH_AgentApprovalNumberFormat(ConsolExportAWBHeader header)
		{
			var consol = header.Consol;

			if (header == null || consol == null)
			{
				return;
			}

			if (!header.EH_AgentApprovalNumber.IsEmpty
				&& !Regex.IsMatch(header.EH_AgentApprovalNumber, ApprovalNumberFormat)
				&& IsExportForAviationSecurityPurposes(consol))
			{
				header.EH_AgentApprovalNumberInfo.AddMessageError(ErrorForApprovalNumberFormatNotMet);
			}
		}

		const string ApprovalNumberFormat = "^RA[0-9]{5}$"; // Regex pattern

		string ErrorForApprovalNumberFormatNotMet
		{
			get { return Res.GetString("2d4d4b52-da0f-402c-b0a2-217c31633164", "The Approval Number must be in the following format: 'RA' + 4 digit number + 1 check digit. For example: RA34576."); }
		}

		public override bool ShowPrefixOnAgentApprovalNumber
		{
			get { return true; }
		}

		#endregion

		#region Organisations To Use

		protected override SupplyChainSecurityOrganisationToUseCollection OrganisationsToUseCollection => FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_HongKong.Value;

		#endregion

		#region Approval Code Configuration

		protected override Dictionary<ZString, ApprovalCodeConfiguration> GetApprovalCodeConfigurations()
		{
			var result = new Dictionary<ZString, ApprovalCodeConfiguration>();

			result.Add(AviationSecuritySchemeMembership.Codes.AccountConsignor, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.AccountConsignor)
			{
				IsValidForAviationSecurity = true,
				IsApprovedToShipOnPassengerFlights = false,
				RequiresApprovalNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				RequiredDocumentValidation = true,
				ApprovalNumberMustMatchRequiredDocumentNumber = true,
				MaximumApprovalValidityInYears = 5,
				ErrorIfMaximumApprovalValidityExceeded = Res.GetString("f7d6c690-4e4d-4b95-abbf-f888b4cbcc59", "For Account Consignors, the Expiry Date cannot be more than 5 years in the future."),
				ExpiryDateMustMatchRequiredDocumentExpiry = true
			});

			result.Add(AviationSecuritySchemeMembership.Codes.KnownConsignor, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.KnownConsignor)
			{
				IsValidForAviationSecurity = true,
				RequiresApprovalNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				RequiredDocumentValidation = true,
				ApprovalNumberMustMatchRequiredDocumentNumber = true,
				AllowsExpiryDate = true
			});

			result.Add(AviationSecuritySchemeMembership.Codes.RegulatedAgent, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.RegulatedAgent)
			{
				IsValidForAviationSecurity = true,
				RequiresApprovalNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				ApprovalNumberFormat = ApprovalNumberFormat,
				ErrorForApprovalNumberFormatNotMet = ErrorForApprovalNumberFormatNotMet,
				IsAgentType = true,
				IsOrgLevelApproval = true,
				DefaultAddressType = OrgAddressType.Office
			});

			result.Add(AviationSecuritySchemeMembershipEx.Codes.No, new ApprovalCodeConfiguration(AviationSecuritySchemeMembershipEx.Descriptions.No)
			{
				IsApprovedToShipOnPassengerFlights = false
			});

			return result;
		}

		#endregion

		#region Shipment Inspection Types

		protected override ShipmentInspectionTypeCollection GetShipmentInspectionTypeCollection()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypes_HongKong.Value.Types;
		}

		protected override ZString GetInspectionTypeDefault()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypeDefault_HongKong.Value;
		}

		#endregion

		protected override bool IsExportForAviationSecurityPurposes(ZString originCountryCode, ZString destinationCountryCode)
		{
			return originCountryCode == Core.Constants.CountryCodes.HongKong
				|| (originCountryCode == Core.Constants.CountryCodes.China && destinationCountryCode != Core.Constants.CountryCodes.HongKong);
		}

		#region Transhipment

		public override ZBool UseTranshipmentAviationSecurityStatus => IsEnabled;

		#endregion

		#region Pack Level Screening

		protected override bool IsOriginPackLevelScreeningRequired(ISupplyChainSecurityImportExportSupporter importExportSupporter)
		{
			return true;
		}

		protected override ZString PackLevelScreeningSchemeName => Res.GetString("0325B51B-34CB-41B6-9C7F-2D791185D13E", "Air Cargo Piece Level Security Screening legislation");

		#endregion

		#region Registry

		protected override T GetRegistryItemToEnable<T>()
		{
			return (T)FreightDataRegistry.Instance.EnableSupplyChainSecurity_HK;
		}

		#endregion
	}
}
