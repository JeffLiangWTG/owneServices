using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class SupplyChainSecurityConfigurationTW : SupplyChainSecurityConfiguration
	{
		public override ZString AWBRANumber => FreightDataRegistry.Instance.RegulatedAgentNumber_TW.Value;

		protected override T GetRegistryItemToEnable<T>() => (T)FreightDataRegistry.Instance.EnableSupplyChainSecurity_TW;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Format string")]
		protected override Dictionary<ZString, ApprovalCodeConfiguration> GetApprovalCodeConfigurations()
		{
			const string approvalNumberFormat = @"^\d{5}$";
			var errorForOwnAgentApprovalNumberNotEntered = Res.GetString(
				"F8D6982E-1661-4C78-B604-1C53886A952E",
				"Please enter an Approval Number.");
			var errorForApprovalNumberFormatNotMet = Res.GetString(
				"A89F4FC8-ABEA-4006-87AC-9529782B68FA",
				"The Approval Number must be in the following format: “NNNNN”, for example 00069.");

			var result = new Dictionary<ZString, ApprovalCodeConfiguration>
			{
				{
					AviationSecuritySchemeMembershipEx.Codes.No,
					new ApprovalCodeConfiguration(AviationSecuritySchemeMembershipEx.Descriptions.No)
					{
						IsApprovedToShipOnPassengerFlights = false
					}
				},
				{
					AviationSecuritySchemeMembership.Codes.KnownConsignor,
					new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.KnownConsignor)
					{
						IsValidForAviationSecurity = true,
						IsApprovedToShipOnPassengerFlights = true,
						AllowsApprovalNumber = true,
						RequiresApprovalNumber = true,
						ApprovalNumberFormat = approvalNumberFormat,
						ErrorForOwnAgentApprovalNumberNotEntered = errorForOwnAgentApprovalNumberNotEntered,
						ErrorForApprovalNumberFormatNotMet = errorForApprovalNumberFormatNotMet,
						RequiresExpiryDate = true,
						MaximumApprovalValidityInYears = 2,
						ErrorIfMaximumApprovalValidityExceeded = Res.GetString("24A31270-C45A-4C05-AFCA-A630D2E64B30",
							"The Expiry Date cannot be more than 2 years in the future."),
						IsOrgLevelApproval = true,
						DefaultAddressType = OrgAddressType.Office,
					}
				},
				{
					AviationSecuritySchemeMembership.Codes.RegulatedAgent,
					new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.RegulatedAgent)
					{
						IsValidForAviationSecurity = true,
						IsApprovedToShipOnPassengerFlights = true,
						AllowsApprovalNumber = true,
						RequiresApprovalNumber = true,
						ApprovalNumberFormat = approvalNumberFormat,
						ErrorForOwnAgentApprovalNumberNotEntered = errorForOwnAgentApprovalNumberNotEntered,
						ErrorForApprovalNumberFormatNotMet = errorForApprovalNumberFormatNotMet,
						RequiresExpiryDate = true,
						MaximumApprovalValidityInYears = 3,
						ErrorIfMaximumApprovalValidityExceeded = Res.GetString("A2CF6934-1EBE-4DF9-8631-A6F202788F16",
							"The Expiry Date cannot be more than 3 years in the future."),
						IsOrgLevelApproval = true,
						DefaultAddressType = OrgAddressType.Office
					}
				},
			};

			return result;
		}
	}
}
