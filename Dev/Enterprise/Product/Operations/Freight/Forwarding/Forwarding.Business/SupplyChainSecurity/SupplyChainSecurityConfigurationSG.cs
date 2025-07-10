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
	public class SupplyChainSecurityConfigurationSG : SupplyChainSecurityConfiguration
	{
		#region Inspection Status Calculation

		internal override bool CheckAdditionalConditionsForApprovedInspectionType(ForwardingShipment shipment)
		{
			if (!GetLoginCompanyHasRAApproval(shipment))
			{
				return false;
			}

			return base.CheckAdditionalConditionsForApprovedInspectionType(shipment);
		}

		internal override void CheckJS_InspectionTypeCodeAdditionalValidation(ForwardingShipment shipment, CargoWise.EntityFramework.ZPropertyInfo info, bool hasChanges)
		{
			base.CheckJS_InspectionTypeCodeAdditionalValidation(shipment, info, hasChanges);

			if (shipment.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved
				&& !GetLoginCompanyHasRAApproval(shipment))
			{
				shipment.AviationSecurity.AddInspectionTypeErrorOrWarning(Res.GetString("ed655539-50e1-48e2-9b88-51433876821e", "Only a Regulated Cargo Agent can handle known consignor cargo, otherwise it is considered unknown. Configure your regulated cargo agent details against the login company's Organization Proxy for Singapore."));
			}
		}

		bool GetLoginCompanyHasRAApproval(ForwardingShipment shipment)
		{
			return GlbCompany.CurrentCompany.OrgProxy != null
				&& GlbCompany.CurrentCompany.OrgProxy.Addresses.Cast<OrgAddress>()
					.Any(x => x.KnownShipper != null
						&& (x.KnownShipper.OV_EXApprovalExpiryDate.IsEmpty || x.KnownShipper.OV_EXApprovalExpiryDate >= shipment.AviationSecurity.ShipmentDateForAviationSecurity.Date)
						&& x.KnownShipper.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.RegulatedAgent);
		}

		#endregion

		#region Approval Code Configuration

		protected override Dictionary<ZString, ApprovalCodeConfiguration> GetApprovalCodeConfigurations()
		{
			var result = new Dictionary<ZString, ApprovalCodeConfiguration>();

			result.Add(AviationSecuritySchemeMembership.Codes.KnownConsignor, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.KnownConsignor)
			{
				IsValidForAviationSecurity = true,
				AllowsApprovalNumber = true,
				RequiresExpiryDate = true,
				MaximumApprovalValidityInYears = 5,
				IsOrgLevelApproval = true,
				DefaultAddressType = OrgAddressType.Office,
				ReadOnlyAddress = true
			});

			result.Add(AviationSecuritySchemeMembership.Codes.RegulatedAgent, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.RegulatedAgent)
			{
				RequiresApprovalNumber = true,
				AllowsApprovalNumber = true,
				ApprovalNumberFormat = @"^RCA/[0-9]{4}/[0-9]{4}$", // Regex format string
				ErrorForApprovalNumberFormatNotMet = Res.GetString("f1e48d69-9d04-4254-9f52-c3cc3e1f0e1f", "The Approval Number must be in the following format: 'RCA/nnnn/YYYY'. For example: RCA/0001/2021."),
				AllowsDuplicateApprovalNumbers = true,
				MaximumApprovalValidityInYears = 3,
				WarningIfApprovalHasExpired = Res.GetString("50f7d75d-1d97-4b27-9259-95d1a3797f1d", "Approval has expired. Regulated Agent must engage an accredited auditor for renewal."),
				WarnIfApprovalWillLapseInMonths = 6,
				WarningIfApprovalWillLapseInMonths = Res.GetString("4a5819bd-e820-4913-b8d8-a8ec5db7cff2", "Expiry date is within 6 months. Regulated Agent must engage an accredited auditor for renewal."),
				IsAgentType = true,
				IsOrgLevelApproval = true,
				DefaultAddressType = OrgAddressType.Office,
				ReadOnlyAddress = true
			});

			result.Add(AviationSecuritySchemeMembershipEx.Codes.No, new ApprovalCodeConfiguration(AviationSecuritySchemeMembershipEx.Descriptions.No)
			{
				IsApprovedToShipOnPassengerFlights = false
			});

			return result;
		}

		#endregion

		#region Agent Approval

		public override OrgCountryData GetAgentApproval(ForwardingConsol consol)
		{
			var approval = GetApproval(GlbBranch.CurrentBranch.OrgProxy);
			if (approval == null || !ApprovalCodeIsAgentType(approval.OV_EXApprovedOrMajorExporter))
			{
				approval = GetApproval(GlbCompany.CurrentCompany.OrgProxy);
			}

			return approval != null && ApprovalCodeIsAgentType(approval.OV_EXApprovedOrMajorExporter)
				? approval
				: null;
		}

		#endregion

		#region eCSD

		public override bool AllowIncludeECSD => false;

		#endregion

		#region	AWB

		public override ZString GetMAWBKnownConsignorCode(ForwardingConsol consol)
		{
			if (IsEnabled
				&& IsExportForAviationSecurityPurposes(consol)
				&& consol.Shipments.Any())
			{
				var unknownConsignorCode = "RCAR-UC"; // Supply Chain Security Code
				var knownConsignorCode = "RCAR-KC"; // Supply Chain Security Code

				var areShipmentsKnown = !consol.Shipments.Cast<ForwardingShipment>().Any(x => x.AviationSecurity.HasUnknownInspectionTypeCode);
				return areShipmentsKnown && consol.JK_AgentType == AgentType.Direct
					|| areShipmentsKnown && consol.JK_AgentType != AgentType.Direct && (consol.SendingForwarderAddress == null || IsConsolSendingForwarderAddressKnown(consol))
					? knownConsignorCode
					: unknownConsignorCode;
			}

			return ZString.Empty;
		}

		bool IsConsolSendingForwarderAddressKnown(ForwardingConsol consol)
		{
			var addresses = consol.SendingForwarderAddress?.Header?.Addresses;
			if (addresses != null && addresses.Count > 0)
			{
				foreach (OrgAddress address in addresses)
				{
					foreach (var approval in address.KnownShipperDetails)
					{
						var approvedCodes = new string[] { AviationSecuritySchemeMembership.Codes.RegulatedAgent, AviationSecuritySchemeMembership.Codes.KnownConsignor };
						if (consol.JK_AgentType != AgentType.Direct && approvedCodes.Any(x => x == approval.OV_EXApprovedOrMajorExporter)
							&& (approval.OV_EXApprovalExpiryDate.IsEmpty || approval.OV_EXApprovalExpiryDate >= ZDate.Today))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		#endregion

		#region Shipment Inspection Types

		protected override ShipmentInspectionTypeCollection GetShipmentInspectionTypeCollection()
		{
			return new ShipmentInspectionTypeCollection();
		}

		#endregion

		#region Registry Item

		protected override T GetRegistryItemToEnable<T>()
		{
			return (T)FreightDataRegistry.Instance.EnableSupplyChainSecurity_SG;
		}

		#endregion

		#region JL_InspectionTypeCode_ReadOnly

		internal override bool JL_InspectionTypeCode_ReadOnly(ForwardingShipment shipment)
		{
			return true;
		}

		#endregion

		#region Inspection Types

		public override ZString InspectionTypeDefault => FreightDataRegistry.AviationSecurity_Unknown_Code;

		#endregion

	}
}
