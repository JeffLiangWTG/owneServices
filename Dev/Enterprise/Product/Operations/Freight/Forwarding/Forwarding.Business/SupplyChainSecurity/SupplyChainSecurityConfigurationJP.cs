using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	public class SupplyChainSecurityConfigurationJP : SupplyChainSecurityConfiguration
	{
		#region Approval Code Configuration

		protected override Dictionary<ZString, ApprovalCodeConfiguration> GetApprovalCodeConfigurations()
		{
			var result = new Dictionary<ZString, ApprovalCodeConfiguration>();

			result.Add(AviationSecuritySchemeMembership.Codes.KnownConsignor, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.KnownConsignor)
			{
				IsValidForAviationSecurity = true,
				RequiresApprovalNumber = true,
				RequiredDocumentValidation = true,
				ApprovalNumberMustMatchRequiredDocumentNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				MaximumApprovalValidityInYears = 2,
				ExpiryDateMustMatchRequiredDocumentExpiry = true,
			});

			result.Add(AviationSecuritySchemeMembership.Codes.RegulatedAgent, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.RegulatedAgent)
			{
				IsValidForAviationSecurity = true,
				RequiresApprovalNumber = true,
				RequiredDocumentValidation = true,
				ApprovalNumberMustMatchRequiredDocumentNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				IsAgentType = true,
				ReadOnlyAddress = true,
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

		#region AWB

		public override bool UseAccountConsignorValidationForPassengerAircraft
		{
			get { return true; }
		}

		#endregion

		#region Shipment Inspection Types

		protected override ShipmentInspectionTypeCollection GetShipmentInspectionTypeCollection()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypes_Japan.Value.Types;
		}

		protected override ZString GetInspectionTypeDefault()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypeDefault_Japan.Value;
		}

		#endregion

		#region Transhipment

		public override ZBool UseTranshipmentAviationSecurityStatus => IsEnabled;

		#endregion

		#region Registry

		protected override T GetRegistryItemToEnable<T>()
		{
			return (T)FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP;
		}

		#endregion

		#region PermitDefaulting

		public override bool ShouldDefaultPermitToSecurityDeclaration => true;

		#endregion

		public override bool AllowManualApprovedInspectionStatus(CommonShipment shipment) => shipment?.DocManagerInfo?.AllEDocs?.OfType<IeDocBase>()?.Any(x => x.DocType == RefDocTypes.ConsignmentSecurityDeclaration) ?? false;
	}
}
