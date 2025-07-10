using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Business
{
	public class SupplyChainSecurityConfigurationUS : SupplyChainSecurityConfiguration
	{
		#region IsEnabled

		public override bool IsEnabled
		{
			get { return true; }
		}

		#endregion

		#region Approval Code Configuration

		protected override Dictionary<ZString, ApprovalCodeConfiguration> GetApprovalCodeConfigurations()
		{
			var result = new Dictionary<ZString, ApprovalCodeConfiguration>();

			result.Add(AviationSecuritySchemeMembershipEx.Codes.Yes, new ApprovalCodeConfiguration(AviationSecuritySchemeMembershipEx.Descriptions.Yes)
			{
				IsValidForAviationSecurity = true,
				AllowsApprovalNumber = true,
				RequiredDocumentValidation = true
			});

			result.Add(AviationSecuritySchemeMembershipEx.Codes.No, new ApprovalCodeConfiguration(AviationSecuritySchemeMembershipEx.Descriptions.No)
			{
				AllowsApprovalNumber = true,
				IsApprovedToShipOnPassengerFlights = false
			});

			return result;
		}

		#endregion

		#region Consignment Security Declaration

		public override bool UseConsignmentSecurityDeclaration => false;

		public override bool AllowIncludeECSD => false;

		#endregion

		#region Known Shipper Filter

		public override ZString KnownShipperFilterText => (NoResString)"TSA Known Shipper Status"; // Filter Constant

		public override MultilingualString KnownShipperFilterDescription => ResString.GetMultilingualString("db6b6261-784d-4a77-b705-27afa4ccd4fe", "TSA Known Shipper Status");

		protected override ZString FilterTextYes => Res.GetString("03fcef39-15ad-476e-8530-30a072d362f5", "Known Shipper");

		protected override ZString FilterTextNo => Res.GetString("3fb28375-cdb4-4ea4-b8b4-54c48273b5cd", "Unknown Shipper");

		#region Approval Number Filter

		public override ZString ApprovalNumberFilterText => (NoResString)"TSA ID Number"; // Filter Constant

		public override MultilingualString ApprovalNumberFilterDescription => ResString.GetMultilingualString("5111fd71-f211-4ee1-bce2-6d68ec9300dc", "TSA ID Number");

		#endregion

		#endregion

		#region Required Document

		public override bool UseApprovedOrganisationRequiredDocTypeSpecifiedInRegistry
		{
			get { return true; }
		}

		#endregion

		#region Pack Level Screening

		protected override bool IsDestinationPackLevelScreeningRequired => true;

		protected override ZString PackLevelScreeningSchemeName => Res.GetString("573e16b2-3534-4efc-a227-2a482d5b4329", "TSA's National Security Program (NSCP)");

		protected override bool IsDestinationPackLevelScreeningAvailable(ForwardingShipment shipment)
		{
			if (shipment.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved
				&& (IsExportForAviationSecurityPurposes(shipment) || IsTranshipment(shipment)))
			{
				return !HasMutualRecognitionAgreement(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			}

			return true;
		}

		bool HasMutualRecognitionAgreement(ZString countryCode)
		{
			switch (countryCode)
			{
				case CountryCodes.Australia:
				case CountryCodes.Canada:
				case CountryCodes.EuropeanUnion:
				case CountryCodes.Israel:
				case CountryCodes.Japan:
				case CountryCodes.KoreaSouth:
				case CountryCodes.NewZealand:
				case CountryCodes.SouthAfrica:
					return true;

				default:
					if (CountryCodes.EuropeanUnionAviationSecurityMembersList.Contains(countryCode.ToString()))
					{
						return true;
					}

					return false;
			}
		}

		protected override void CheckSendFWBDestinationValidation(ForwardingConsol consol, ZPropertyInfo info)
		{
			var importExportSupporter = consol as ISupplyChainSecurityImportExportSupporter;
			if (IsExportForAviationSecurityPurposes(importExportSupporter) || IsTranshipment(importExportSupporter))
			{
				var hasShipmentsMissingPackLevelScreening = consol.Shipments.Cast<ForwardingShipment>()
					.SelectMany(s => s.OuterPackLines).Cast<ForwardingPackLine>()
					.Any(p => p.JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code
						|| (p.Shipment.JS_InspectionTypeCode != BaseJobShipmentLookups.InspectionType_Approved && p.JL_InspectionTypeCode.IsEmpty));
				if (hasShipmentsMissingPackLevelScreening)
				{
					info.AddWarning(Res.GetString("c47ebca3-3bb3-4cf5-91c5-71bda20b3a95",
						"This Consol is destined for {0}, and as part of {1}, MRAs are currently in place between {0} and your country, so in line with 100% screening legislation, a screening status must be recorded for all packages.",
						consol.DischargePort.Country.RN_Desc,
						PackLevelScreeningSchemeName));
				}
			}
		}

		protected override void CheckJL_InspectionTypeCode_DestinationValidationCore(ForwardingPackLine packline)
		{
			if (!packline.JL_InspectionTypeCodeInfo.ReadOnly)
			{
				if (HasMutualRecognitionAgreement(GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
				{
					if (packline.JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code)
					{
						packline.JL_InspectionTypeCodeInfo.AddWarning(Res.GetString("9fa1ed4a-360a-4118-8986-2607d43159d5",
							"This Shipment is destined for the United States, and as part of {0}, MRAs are currently in place between the United States and your country, so in line with 100% screening legislation, a screening status must be recorded for all packages.",
							PackLevelScreeningSchemeName));
					}
				}
				else if (packline.JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code || packline.JL_InspectionTypeCode.IsEmpty)
				{
					packline.JL_InspectionTypeCodeInfo.AddWarning(Res.GetString("b155efc3-6bff-49d6-856e-eb35a20cebed", "This Shipment is destined for the United States so in line with 100% screening legislation, a screening status must be recorded for all packages."));
				}
			}
		}

		protected override void CheckEAS_ScreeningMethod_DestinationPackLevelScreeningValidationCore(ExportAWBSecurityStatusLine parent, ForwardingConsol consol)
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

				parent.EAS_ScreeningMethodInfo.AddMessageError(Res.GetString("44797d27-3d9d-439e-8532-c7c055c83159", "Shipment/s {0} are destined for the United States so in line with 100% screening legislation, a screening status must be recorded for all packages, unless the shipment is APP.",
					string.Join(", ", unsecuredShipments)));
			}
		}

		#endregion

		#region PermitDefaulting

		public override bool ShouldDefaultPermitToSecurityDeclaration => false;

		#endregion

		#region Registry

		protected override T GetRegistryItemToEnable<T>()
		{
			return (T)FreightDataRegistry.Instance.EnableSupplyChainSecurity_US;
		}

		#endregion
	}
}
