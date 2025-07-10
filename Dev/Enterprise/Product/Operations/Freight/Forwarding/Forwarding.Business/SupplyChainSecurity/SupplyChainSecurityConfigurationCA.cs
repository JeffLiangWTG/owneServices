using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public class SupplyChainSecurityConfigurationCA : SupplyChainSecurityConfiguration
	{
		#region Approval Codes Configuration

		protected override Dictionary<ZString, ApprovalCodeConfiguration> GetApprovalCodeConfigurations()
		{
			var result = new Dictionary<ZString, ApprovalCodeConfiguration>();
			result.Add(AviationSecuritySchemeMembership.Codes.RegulatedAgent, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.RegulatedAgent)
			{
				IsValidForAviationSecurity = true,
				IsAgentType = true,
				AllowsApprovalNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				AllowsExpiryDate = true,
				DefaultAddressType = OrgAddressType.Office
			});

			result.Add(AviationSecuritySchemeMembership.Codes.CertifiedAgent, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.CertifiedAgent)
			{
				IsValidForAviationSecurity = true,
				IsAgentType = true,
				AllowsApprovalNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				AllowsExpiryDate = true,
				DefaultAddressType = OrgAddressType.Office
			});

			result.Add(AviationSecuritySchemeMembership.Codes.KnownConsignor, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.KnownConsignor)
			{
				IsValidForAviationSecurity = true,
				AllowsApprovalNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				AllowsExpiryDate = true,
				DefaultAddressType = OrgAddressType.Office,
			});

			result.Add(AviationSecuritySchemeMembership.Codes.AccountConsignor, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.AccountConsignor)
			{
				IsValidForAviationSecurity = true,
				IsApprovedToShipOnPassengerFlights = false,
				AllowsApprovalNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				AllowsExpiryDate = true,
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
			return FreightDataRegistry.Instance.ShipmentInspectionTypes_Canada.Value.Types;
		}

		#endregion

		#region Registry

		protected override T GetRegistryItemToEnable<T>()
		{
			return (T)FreightDataRegistry.Instance.EnableSupplyChainSecurity_CA;
		}

		protected override SupplyChainSecurityOrganisationToUseCollection OrganisationsToUseCollection => FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_Canada.Value;

		#endregion
	}
}
