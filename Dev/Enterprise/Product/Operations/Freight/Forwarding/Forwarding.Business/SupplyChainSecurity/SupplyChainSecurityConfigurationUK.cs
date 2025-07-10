using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Business
{
	public class SupplyChainSecurityConfigurationUK : SupplyChainSecurityConfiguration
	{
		public override bool AllowRelevantOrganisationsWithoutAviationSecurityApproval => true;

		#region Approval Code Configuration

		protected override Dictionary<ZString, ApprovalCodeConfiguration> GetApprovalCodeConfigurations()
		{
			var result = new Dictionary<ZString, ApprovalCodeConfiguration>();

			const string approvalNumberFormat = @"^[0-9]{5}-[0-9]{2}$"; // Regex pattern
			var errorForApprovalNumberFormatNotMet = Res.GetString("772b0eea-7ce4-40b1-9916-c1218b1b040e", "The Approval Number must be in the following format: 'NNNNN-NN'. For example: 00002-01.");

			result.Add(AviationSecuritySchemeMembership.Codes.CertifiedHaulier, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.CertifiedHaulier)
			{
				IsValidForAviationSecurity = true,
				AllowsApprovalNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				IsOrgLevelApproval = true,
				DefaultAddressType = OrgAddressType.Office,
				AllowsExpiryDate = true,
				RequiredDocumentValidation = true
			});

			result.Add(AviationSecuritySchemeMembership.Codes.KnownConsignor, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.KnownConsignor)
			{
				IsValidForAviationSecurity = true,
				RequiresApprovalNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				ApprovalNumberFormat = approvalNumberFormat,
				ErrorForApprovalNumberFormatNotMet = errorForApprovalNumberFormatNotMet,
				MaximumApprovalValidityInYears = 5,
			});

			result.Add(AviationSecuritySchemeMembership.Codes.RegulatedAgent, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.RegulatedAgent)
			{
				IsValidForAviationSecurity = true,
				RequiresApprovalNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				ApprovalNumberFormat = approvalNumberFormat,
				ErrorForApprovalNumberFormatNotMet = errorForApprovalNumberFormatNotMet,
				MaximumApprovalValidityInYears = 5,
				IsAgentType = true
			});

			result.Add(AviationSecuritySchemeMembershipEx.Codes.No, new ApprovalCodeConfiguration(AviationSecuritySchemeMembershipEx.Descriptions.No)
			{
				IsApprovedToShipOnPassengerFlights = false
			});

			return result;
		}

		#endregion

		#region High Risk Requirement

		public override bool IsHighRiskApplicable => IsEnabled;

		#endregion

		#region Automatic Calculation Of Approved Status

		public override bool AllowAutomaticCalculationOfApprovedStatus(ForwardingShipment shipment)
		{
			return shipment.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved;
		}

		#endregion

		#region Approval Code Validation

		public override ZString GetErrorForApprovalCodeInvalidForCountry(ZString approvalCode, ZString countryCode)
		{
			if (approvalCode == AviationSecuritySchemeMembership.Codes.AccountConsignor
				&& countryCode == CountryCodes.UnitedKingdom)
			{
				return Res.GetString("5d90c958-bf48-4e63-b803-f0b6da62fde5", "Account Consignor cannot be selected for addresses in the UK as it does not participate in the Account Consignor Scheme.");
			}

			return ZString.Empty;
		}

		#endregion

		#region Inspection Status

		public override bool IsInspectionTypeRecalculationRequired => OrganisationsToUse.Values.Any(x => x.ValidationCode == SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes || x.ValidationCode == SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning);

		public override bool AllowManualApprovedInspectionStatus(CommonShipment shipment)
		{
			return IsEnabled
				? GlbCompany.CurrentCompany.Country.IsUnitedKingdom && HasUKStaffHandlingSecuredCargoCertification()
				: base.AllowManualApprovedInspectionStatus(shipment);
		}

		internal override void CheckJS_InspectionTypeCodeAdditionalValidation(ForwardingShipment shipment, ZPropertyInfo info, bool hasChanges)
		{
			base.CheckJS_InspectionTypeCodeAdditionalValidation(shipment, info, hasChanges);

			if (IsEnabled)
			{
				var code = info.Value.ToString();

				if (shipment.JS_RL_NKOrigin.SubstringSafe(0, 2) == CountryCodes.UnitedKingdom)
				{
					switch (code)
					{
						case ExemptionCodes.Codes.TransferOrTransshipment:
						case ExemptionCodes.Codes.Mail:
							info.AddError(Res.GetString("b5fcdc7b-b38e-40f2-93b7-0edf95292e0a", "{0} exemption is not valid for shipments being uplifted by air from / within the UK.", code));
							break;

						case ScreeningMethods.Codes.VisualCheck:
							if (!shipment.AviationSecurity.IsHighRiskShipment)
							{
								info.AddWarning(Res.GetString("db72e39f-5630-472f-b589-a13b44c670ee", "A visual check is only allowed in combination with other methods. Enter the secondary method used instead as that method will be used in the Security Declaration."));
							}

							break;
					}
				}

				var certificationError = GetInspectionTypeErrorForUncertifiedUser(info);
				if (!certificationError.IsEmpty && hasChanges)
				{
					info.AddError(certificationError);
				}
			}
		}

		internal override bool CheckAdditionalConditionsForApprovedInspectionType(ForwardingShipment shipment)
		{
			if (IsEnabled)
			{
				var approvalParties = shipment.AviationSecurity.GetAviationSecurityRelevantParties(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes).Cast<AviationSecurityRelevantParty>();
				return approvalParties.All(x => x.KnownShipperRecord != null && GetErrorForApprovalInvalidForShipment(x.KnownShipperRecord, shipment).IsEmpty);
			}

			return base.CheckAdditionalConditionsForApprovedInspectionType(shipment);
		}

		public override bool ShouldReDefaultPackLineInspectionTypeCodes(ForwardingShipment shipment)
			=> !IsEnabled || GetInspectionTypeErrorForUncertifiedUser(shipment.JS_InspectionTypeCodeInfo).IsEmpty;

		public override bool IsSecuredForPackLineInspectionType => IsEnabled;

		public override bool IsRecalculationNeeded(ZString inspectionTypeCode) => inspectionTypeCode != BaseJobShipmentLookups.InspectionType_Approved;

		#endregion

		#region User Authorization

		public override ZString GetSpecialHandlingErrorForUnauthorizedUser(ForwardingConsol consol, ZString code, bool hasChanges)
		{
			if (consol.JK_RL_NKLoadPort.StartsWith(CountryCodes.UnitedKingdom)
				&& code == AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft
				&& hasChanges
				&& !HasUKStaffHandlingSecuredCargoCertification())
			{
				return NoValidCertificationError;
			}

			return ZString.Empty;
		}

		ZString GetInspectionTypeErrorForUncertifiedUser(ZPropertyInfo info)
		{
			if (info.Value.ToString() != FreightDataRegistry.AviationSecurity_Unknown_Code && !HasUKStaffHandlingSecuredCargoCertification())
			{
				return NoValidCertificationError;
			}

			return ZString.Empty;
		}

		bool HasUKStaffHandlingSecuredCargoCertification()
		{
			return GlbStaff.CurrentUser.Certificates.Any(c => c.IsUKStaffHandlingSecureCargoCertificate && c.XZ_ExpiryOrDueDate >= ZDate.Today);
		}

		public override ZString GetUncertifiedUserForScreeningError(GlbStaff staff)
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == CountryCodes.UnitedKingdom && !HasUKStaffHandlingSecuredCargoCertification(staff))
			{
				return NoValidCertificationError;
			}
			return ZString.Empty;
		}

		bool HasUKStaffHandlingSecuredCargoCertification(GlbStaff staff)
		{
			return staff.Certificates.Any(c => c.IsUKStaffHandlingSecureCargoCertificate && c.XZ_ExpiryOrDueDate >= ZDate.Today);
		}

		public override ZString GetStaffHandlingSecuredCargoUncertificatedErrorMessages()
		{
			if (!HasUKStaffHandlingSecuredCargoCertification())
			{
				return NoValidCertificationError;
			}
			return ZString.Empty;
		}

		string NoValidCertificationError => Res.GetString("c2163c41-d7e6-44a8-ba61-63cfd284b491",
					"The Person Screening the cargo must have a valid training certification applicable to handling secured air cargo (either \"CO\" (Cargo Operative), \"COS\" (Cargo Operative Screening), \"CS\" (Cargo Supervisor) or \"CM\" (Cargo Manager)) saved against the staff profile's Human Resources > Certificates & ID Numbers grid.");

		#endregion

		#region Security Declaration

		protected override void ValidateEH_AgentApprovalNumberFormat(ConsolExportAWBHeader header)
		{
			const string agentApprovalNumberFormat = @"^[0-9]{5}-[0-9]{2}$"; // Regex pattern

			var regex = new Regex(agentApprovalNumberFormat);
			if (!regex.IsMatch(header.EH_AgentApprovalNumber))
			{
				header.EH_AgentApprovalNumberInfo.AddMessageError(Res.GetString("4e3fe5f6-82bf-4541-822f-28ee553941eb", "The Regulated Agent Identifier must be in the following format: 'NNNNN-NN'. For example: 00002-01."));
			}
		}

		protected override ZString GetErrorForCargoSecureForAllCargoAircraftOnlyIsNotAllowedCore(ForwardingConsol consol)
		{
			if (consol.JK_RL_NKLoadPort.SubstringSafe(0, 2) == CountryCodes.UnitedKingdom
				&& consol.Shipments.Cast<CommonShipment>().All(x => x.JS_RL_NKOrigin.SubstringSafe(0, 2) == CountryCodes.UnitedKingdom))
			{
				return Res.GetString("774bf725-9478-4733-bf98-ccbcd85fcfad", "{0} cannot be used for goods which commence their journey from the UK.", AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForAllCargoAircraftOnly);
			}

			return base.GetErrorForCargoSecureForAllCargoAircraftOnlyIsNotAllowedCore(consol);
		}

		public override bool AllowDefaultingCargoSecureForPassengerAndAllCargoAircraft(ForwardingConsol consol)
		{
			if (consol.JK_RL_NKLoadPort.SubstringSafe(0, 2) == CountryCodes.UnitedKingdom)
			{
				return false;
			}

			return base.AllowDefaultingCargoSecureForPassengerAndAllCargoAircraft(consol);
		}

		#endregion

		#region Pack Level Screening

		protected override bool IsOriginPackLevelScreeningRequired(ISupplyChainSecurityImportExportSupporter importExportSupporter)
		{
			return true;
		}

		protected override ZString PackLevelScreeningSchemeName
		{
			get
			{
				return Res.GetString("F03B990F-FB8C-421E-925A-9FF54B5E67A6", "Air Cargo Piece Level Security Screening legislation");
			}
		}

		#endregion

		#region Issuing Authority Country

		public override bool UseIssuingAuthorityCountry => true;

		#endregion

		#region Registry

		protected override T GetRegistryItemToEnable<T>()
		{
			return (T)FreightDataRegistry.Instance.EnableSupplyChainSecurity_UK;
		}

		protected override SupplyChainSecurityOrganisationToUseCollection OrganisationsToUseCollection => FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_UK.Value;

		#endregion

		#region AWB

		public override bool ShouldSetScheduledArrivalDate(ForwardingConsol consol)
		{
			return (consol?.AWBHeader?.EH_SecurityStatus).GetValueOrDefault() == AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft;
		}

		public override IEnumerable<ZString> GetAdditionalSecurityInformations(ForwardingConsol consol)
		{
			var result = new List<ZString>();
			if (!consol.AWBHeader.EH_ScheduledArrivalDate.IsEmpty)
			{
				result.Add("DOC" + consol.AWBHeader.EH_ScheduledArrivalDate.ToString("ddMMMyy", CultureInfo.InvariantCulture)); // Scheduled Arrival Date
			}
			result.AddRange(base.GetAdditionalSecurityInformations(consol));
			return result;
		}

		#endregion

		#region IsExportForAviationSecurityPurposes

		protected override bool UseShipmentConsolsToDetermineExportForAviationSecurityPurposes => true;

		#endregion
	}
}
