using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Freight.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Business
{
	public class SupplyChainSecurityConfigurationEU : SupplyChainSecurityConfiguration
	{
		#region Licence

		public override ZString LicenceEconomicGroupingCode
		{
			get { return CountryCodes.EuropeanUnion; }
		}

		#endregion

		#region Organisations To Use

		protected override SupplyChainSecurityOrganisationToUseCollection OrganisationsToUseCollection => FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_EuropeanUnion.Value;

		#endregion

		#region Known Shipper Types

		protected override ShipmentInspectionTypeCollection GetShipmentInspectionTypeCollection()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypes_EU.Value.Types;
		}

		#endregion

		#region Approval Code Configuration

		protected override Dictionary<ZString, ApprovalCodeConfiguration> GetApprovalCodeConfigurations()
		{
			var result = new Dictionary<ZString, ApprovalCodeConfiguration>();

			const string approvalNumberFormat = @"^[0-9]{5}-[0-9]{2}$"; // Regex pattern
			var errorForApprovalNumberFormatNotMet = Res.GetString("36cb3885-3a34-4339-a6ea-66715fde8bb2", "The Approval Number must be in the following format: 'NNNNN-NN'. For example: 00002-01.");

			result.Add(AviationSecuritySchemeMembership.Codes.AccountConsignor, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.AccountConsignor)
			{
				IsValidForAviationSecurity = true,
				IsApprovedToShipOnPassengerFlights = false,
				AllowsApprovalNumber = true,
				ApprovalNumberMustMatchRequiredDocumentNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				IsOrgLevelApproval = true,
				DefaultAddressType = OrgAddressType.Office
			});

			result.Add(AviationSecuritySchemeMembership.Codes.ApprovedHaulier, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.ApprovedHaulier)
			{
				IsValidForAviationSecurity = true,
				RequiresApprovalNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				ApprovalNumberFormat = approvalNumberFormat,
				ErrorForApprovalNumberFormatNotMet = errorForApprovalNumberFormatNotMet,
				DefaultAddressType = OrgAddressType.Office,
				MaximumApprovalValidityInYears = 5,
				IsOrgLevelApproval = true,
				RequiredDocumentValidation = true
			});

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

		#region Approval Code Validation

		public override ZString GetErrorForApprovalCodeInvalidForCountry(ZString approvalCode, ZString countryCode)
		{
			if (approvalCode == AviationSecuritySchemeMembership.Codes.AccountConsignor
				&& countryCode == CountryCodes.Germany)
			{
				return Res.GetString("1ad4a3f4-f1f9-41ec-8635-1c0d89e3d7a0", "Account Consignor cannot be selected for addresses in Germany as it does not participate in the Account Consignor Scheme.");
			}

			return ZString.Empty;
		}

		public override ZString GetWarningForAdditionalApprovalCodeValidation(IOrgCountryData countryData)
		{
			if (IsEnabled
				&& (countryData.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.RegulatedAgent || countryData.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.KnownConsignor)
				&& !countryData.OV_EXApprovalNumber.IsEmpty)
			{
				var approval = countryData as OrgCountryData;
				if (approval != null)
				{
					var query = new ZDBOnlyQuery(typeof(OrgCountryData));
					query.AddToFilter(OrgCountryDataSchema.OV_OH_OrgHeader, SQLComparisonOperator.NotEqual, approval.OV_OH_OrgHeader);
					query.AddToFilter(OrgCountryDataSchema.OV_RN_NKClientCountryRelation, approval.OV_RN_NKClientCountryRelation);
					query.AddToFilter(OrgCountryDataSchema.OV_EXApprovalNumber, approval.OV_EXApprovalNumber);

					var approvalTypeQuery = new ZQuery(OrgCountryDataSchema.OV_EXApprovedOrMajorExporter, AviationSecuritySchemeMembership.Codes.RegulatedAgent);
					approvalTypeQuery.AddToFilter(JoinCondition.Or, OrgCountryDataSchema.OV_EXApprovedOrMajorExporter, AviationSecuritySchemeMembership.Codes.KnownConsignor);
					query.AddToFilter(approvalTypeQuery);

					var addressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgCountryDataSchema.OV_OA_ApprovedLocation);
					addressSubQuery.AddToFilter(OrgAddressSchema.OA_RN_NKCountryCode, approval.ApprovedLocation == null ? approval.OrgHeader.OH_RL_NKClosestPort.SubstringSafe(0, 2) : approval.ApprovedLocation.OA_RN_NKCountryCode);
					query.AddSubQuery(addressSubQuery, JoinCondition.And);

					var approvalsWithSameApprovalNumber = approval.Factory.Load<OrgCountryData>(query);
					if (approvalsWithSameApprovalNumber.Any())
					{
						var sb = new ZStringBuilder(approvalsWithSameApprovalNumber.Take(20).Select(x => x.OrgHeader.HumanReadableName));
						return Res.GetString("b3d538b4-2652-4968-a6a7-7c2afc584d37", "The Approval Number must be unique for the same country/region. The following Organizations use this number already:\r\n{0}", sb.ToStringWithNewLineBetweenAppends());
					}
				}
			}

			return ZString.Empty;
		}

		#endregion

		#region Inspection Status

		internal override ZString GetErrorForApprovalInvalidForShipment(OrgCountryData countryData, ForwardingShipment shipment)
		{
			if (countryData.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.AccountConsignor
				&& countryData.ApprovedLocation != null)
			{
				var approvedAddressCountry = countryData.ApprovedLocation.OA_RN_NKCountryCode;

				var originCountry = shipment.JS_RL_NKOrigin.SubstringSafe(0, 2);
				var invalidOrigin = originCountry != approvedAddressCountry;

				var firstAirLeg = shipment.TransportsIncludingRelated
					.FirstLegMatching(x => x.IsAir);
				var upliftCountry = firstAirLeg?.JW_RL_NKLoadPort.SubstringSafe(0, 2);
				var invalidUplift = upliftCountry.HasValue && upliftCountry.Value != approvedAddressCountry;

				if (invalidOrigin || invalidUplift)
				{
					var org = countryData.OrgHeader ?? countryData.ApprovedLocation.Header;
					if (org != null)
					{
						if (invalidOrigin)
						{
							return Res.GetString("b727bcdb-72fc-412b-b3d8-10653d2b8357", "{0} is an Account Consignor of {1} so their status is not recognized in Shipment's Origin country of {2}. Apply an inspection or exemption method.",
								org.HumanReadableName,
								countryData.ApprovedLocation.Country?.RN_Desc ?? approvedAddressCountry,
								shipment.Origin?.Country?.RN_Desc ?? originCountry);
						}
						else if (invalidUplift)
						{
							return Res.GetString("11f93ddb-6207-4c69-a133-e22cad3ef3e4", "{0} is an Account Consignor of {1} so their status is not recognized in the first air leg's departure country of {2}. Apply an inspection or exemption method.",
								org.HumanReadableName,
								countryData.ApprovedLocation.Country?.RN_Desc ?? approvedAddressCountry,
								firstAirLeg?.LoadPort?.Country?.RN_Desc ?? upliftCountry);
						}
					}
				}
			}

			return ZString.Empty;
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

		internal override ZString GetEditInspectionErrorForUncertifiedUser(ForwardingShipment shipment)
		{
			var staffHomePortCountryCode = GlbStaff.CurrentUser.HomeBranch?.Country.Code.ToString() ?? string.Empty;
			return FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.Value.Enabled
					&& FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.Value.ApplyCertificationRestriction
					&& GetOrgProxyHasRAStatus(shipment)
					&& !IsUserCertifiedForAviationSecurity(GlbStaff.CurrentUser)
					&& Env.Security.MaintainShipmentAllowToOverrideInspection.IsAllowed
					&& CountryCodes.EuropeanUnionAviationSecurityMembersList.Contains(staffHomePortCountryCode)
					&& shipment.Origin != null
					&& CountryCodes.EuropeanUnionAviationSecurityMembersList.Contains(shipment.Origin.Country.Code.ToString())
					&& shipment.IsAir
				? (ZString)Res.GetString("71cda1b0-edd0-4367-aa39-c4947beba665", "Only users with valid BKG and DTA certificate types saved in their Staff Profile may edit this field.")
				: ZString.Empty;
		}

		public override ZString GetSpecialHandlingErrorForUnauthorizedUser(ForwardingConsol consol, ZString code, bool hasChanges)
		{
			var error = base.GetSpecialHandlingErrorForUnauthorizedUser(consol, code, hasChanges);
			if (error.IsEmpty)
			{
				var staffHomePortCountryCode = GlbStaff.CurrentUser.HomeBranch?.Country.Code.ToString() ?? string.Empty;
				error = FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.Value.Enabled
						&& FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.Value.ApplyCertificationRestriction
						&& GetOrgProxyHasRAStatus(consol) 
						&& !IsUserCertifiedForAviationSecurity(GlbStaff.CurrentUser)
						&& Env.Security.MaintainConsolAllowToOverrideSecurityStatus.IsAllowed
						&& CountryCodes.EuropeanUnionAviationSecurityMembersList.Contains(staffHomePortCountryCode)
						&& consol.LoadPort != null
						&& CountryCodes.EuropeanUnionAviationSecurityMembersList.Contains(consol.LoadPort.Country.Code.ToString())
						&& consol.IsAir
						&& code == Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft
						&& hasChanges
					? (ZString)Res.GetString("71cda1b0-edd0-4367-aa39-c4947beba665", "Only users with valid BKG and DTA certificate types saved in their Staff Profile may edit this field.")
					: ZString.Empty;
			}
			return error;
		}

		#endregion

		#region IsExportForAviationSecurityPurposes

		protected override bool IsExportForAviationSecurityPurposes(ZString originCountryCode, ZString destinationCountryCode)
		{
			return CountryCodes.EuropeanUnionAviationSecurityMembersList.Contains(originCountryCode.ToString());
		}

		protected override bool UseShipmentConsolsToDetermineExportForAviationSecurityPurposes => true;

		#endregion

		#region High Risk Requirement

		public override bool IsHighRiskApplicable => IsEnabled;

		#endregion

		#region Pack Level Screening

		internal override bool JL_IsHighRisk_ReadOnly(ForwardingShipment shipment)
		{
			if (!shipment.IsAir || !IsExportForAviationSecurityPurposes(shipment))
			{
				return true;
			}
			return base.JL_IsHighRisk_ReadOnly(shipment);
		}

		internal override bool JL_AdditionalInspectionTypeCode_ReadOnly(ForwardingShipment shipment)
		{
			if (!shipment.IsAir || !IsExportForAviationSecurityPurposes(shipment))
			{
				return true;
			}
			return base.JL_AdditionalInspectionTypeCode_ReadOnly(shipment);
		}

		public override bool IsPackLevelScreeningAvailable(ISupplyChainSecurityImportExportSupporter importExportSupporter)
		{
			if (importExportSupporter == null || !importExportSupporter.IsAir)
			{
				return false;
			}

			return IsExportForAviationSecurityPurposes(importExportSupporter) || IsTranshipmentWithExceptions(importExportSupporter);
		}

		bool IsTranshipmentWithExceptions(ISupplyChainSecurityImportExportSupporter importExportSupporter)
		{
			return IsTranshipment(importExportSupporter)
				&& (importExportSupporter.DischargeCountryForSupplyChainSecurity != CountryCodes.UnitedStates
				|| !IsLoadCountryForSupplyChainSecurityWithExceptions(importExportSupporter));
		}

		bool IsLoadCountryForSupplyChainSecurityWithExceptions(ISupplyChainSecurityImportExportSupporter importExportSupporter)
		{
			var loadCountryForSupplyChainSecurity = importExportSupporter.LoadCountryForSupplyChainSecurity;
			return CountryCodes.EuropeanUnionAviationSecurityMembersList.Contains(loadCountryForSupplyChainSecurity.ToString())
				|| loadCountryForSupplyChainSecurity == CountryCodes.Canada
				|| loadCountryForSupplyChainSecurity == CountryCodes.Israel
				|| loadCountryForSupplyChainSecurity == CountryCodes.Australia
				|| loadCountryForSupplyChainSecurity == CountryCodes.NewZealand
				|| loadCountryForSupplyChainSecurity == CountryCodes.Japan
				|| loadCountryForSupplyChainSecurity == CountryCodes.KoreaSouth
				|| loadCountryForSupplyChainSecurity == CountryCodes.SouthAfrica;
		}

		public override bool IsSecuredForPackLineInspectionType => IsEnabled;

		#endregion

		#region Registry

		protected override T GetRegistryItemToEnable<T>()
		{
			return (T)FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU;
		}

		#endregion

		#region AWB

		public override ZString GetAuthorizedSenderNumber(ForwardingConsol consol)
		{
			var authorizedSenderCusCodes = GlbCompany.CurrentCompany.OrgProxy?.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.SwissCodeTypes.ASN, CountryCodes.Switzerland);

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != CountryCodes.Switzerland
				|| authorizedSenderCusCodes.IsNullOrEmpty()
				|| !consol.IsAir
				|| !consol.IsExport()
				|| consol.JK_RL_NKLoadPort.SubstringSafe(0, 2) != CountryCodes.Switzerland
				|| !GlbCompany.CurrentCompany.Branches.Any(b => b.GB_OH_OrgProxy == consol.SendingForwarderPK)
				|| consol.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_CommunityTransitStatus == ExportCommunityTransitStatusList.Codes.TD))
			{
				return ZString.Empty;
			}

			if (consol.JK_RL_NKLoadPort != "CHGVA" && consol.JK_RL_NKLoadPort != "CHZRH")
			{
				return ZString.Empty;
			}

			if (authorizedSenderCusCodes.Any(c => !c.OK_OA_PremisesAddress.IsEmpty))
			{
				return Res.GetString("45a2a8b2-0dc3-4dc9-a23c-6d5ea24e7679", "LA FILED");
			}

			if (consol.JK_RL_NKLoadPort == "CHGVA")
			{
				return Res.GetString("e718a1f7-e517-419b-9473-696a45d5d154", "EA FILED");
			}
			else if (consol.JK_RL_NKLoadPort == "CHZRH")
			{
				return Res.GetString("a79f48ca-731b-4e47-a1d0-726bb01b5b0a", "ZV FILED");
			}

			return ZString.Empty;
		}

		#endregion

		#region Issuing Authority Country

		public override bool UseIssuingAuthorityCountry => true;

		#endregion

		#region Check RA Status

		protected bool GetOrgProxyHasRAStatus(ForwardingConsol consol)
		{
			if(consol.ShipmentCount == 0)
			{
				return GetOrgProxyHasRAStatus((ForwardingShipment)null);
			}

			return consol.Shipments.Any(x => GetOrgProxyHasRAStatus((ForwardingShipment)x));
		}

		protected bool GetOrgProxyHasRAStatus(ForwardingShipment shipment)
		{
			var shipmentDate = shipment != null ? shipment.AviationSecurity.ShipmentDateForAviationSecurity : ZDateTime.Now;

			bool branchOrgProxyHasRAStatus = GlbBranch.CurrentBranch.OrgProxy != null
				&& GetAddressesHasRAStatus(GlbBranch.CurrentBranch.OrgProxy.Addresses.Cast<OrgAddress>(), shipmentDate);

			bool companyOrgProxyHasRAStatus = GlbCompany.CurrentCompany.OrgProxy != null
				&& GetAddressesHasRAStatus(GlbCompany.CurrentCompany.OrgProxy.Addresses.Cast<OrgAddress>(), shipmentDate);

			return branchOrgProxyHasRAStatus || companyOrgProxyHasRAStatus;
		}

		bool GetAddressesHasRAStatus(IEnumerable<OrgAddress> addresses, ZDateTime dateTime)
		{
			return addresses
					.Any(x => x.KnownShipper != null
						&& (x.KnownShipper.OV_EXApprovalExpiryDate.IsEmpty || x.KnownShipper.OV_EXApprovalExpiryDate >= dateTime.Date)
						&& x.KnownShipper.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.RegulatedAgent);
		}

		#endregion

		#region GetEditApprovalErrorForUncertifiedUser

		public override ZString GetEditApprovalErrorForUncertifiedUser(IOrgCountryData countryData)
		{
			return !FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.Value.Enabled || IsUserCertifiedForAviationSecurity(GlbStaff.CurrentUser) || CanEditApproval(countryData)
				? ZString.Empty
				: (ZString)Res.GetString("488c96b6-ee31-4374-9c8c-a263672e11be", "Only users with valid BKG and DTA certificate types saved in their Staff Profile may edit this approval.");
		}

		protected bool IsAddressInEuropeanUnionAviationSecurityScheme(OrgAddress address) => address != null && CountryCodes.IsInEuropeanUnionAviationSecurityScheme(address.OA_RN_NKCountryCode);

		bool CanEditApproval(IOrgCountryData countryData)
		{
			var approval = countryData as OrgCountryData;
			if (approval == null || !approval.HasChanges)
			{
				return true;
			}

			var addressesToCheck = new List<OrgAddress>();
			addressesToCheck.Add(approval.ApprovedLocation);

			if (approval.OV_OA_ApprovedLocationInfo.HasChanges)
			{
				addressesToCheck.Add(approval.Factory.Load<OrgAddress>((ZGuid)approval.OV_OA_ApprovedLocationInfo.OriginalValue));
			}

			return !addressesToCheck.Any(IsAddressInEuropeanUnionAviationSecurityScheme);
		}

		#endregion

		#region Aviation Security Freight Movement Restricted

		public override bool IsAviationSecurityFreightMovementRestricted(ISupplyChainSecurityImportExportSupporter businessObject)
		{
			bool orgProxyHasRAStatus = false;
			if(businessObject is ForwardingConsol consol)
			{
				orgProxyHasRAStatus = GetOrgProxyHasRAStatus(consol);
			}
			else if (businessObject is ForwardingShipment shipment)
			{
				orgProxyHasRAStatus = GetOrgProxyHasRAStatus(shipment);
			}

			return FreightDataRegistry.Instance.AviationSecurityTrainingRestrictions_EU.Value.Enabled
				&& orgProxyHasRAStatus
				&& CountryCodes.EuropeanUnionAviationSecurityMembersList.Contains(GlbStaff.CurrentUser.HomeBranch?.Country.Code.ToString() ?? string.Empty)
				&& !IsUserCertifiedForAviationSecurity(GlbStaff.CurrentUser)
				&& IsAirExportFromEuropeanUnionAviationSecurityMembers(businessObject);
		}

		public override bool IsUserCertifiedForAviationSecurity(IGlbStaff staff)
		{
			if (staff is GlbStaff glbStaff)
			{
				var certificates = glbStaff.Certificates;
				var today = ZDate.Today;
				return certificates.GetFirstCertificate(StaffDefaultCertificateIDAndTrainingTypes.BKG, today) != null
					&& certificates.GetFirstCertificate(StaffDefaultCertificateIDAndTrainingTypes.DTA, today) != null;
			}
			else
			{
				return false;
			}
		}

		public override MultilingualString AviationSecurityFreightMovementRestrictedErrorMessage =>
			ResString.GetMultilingualString("3464a6fc-a051-4758-8bab-ea6c0922ac65", @"Only users with valid BKG and DTA certificate types saved in their Staff Profile may issue documents for air export jobs from the EU, Iceland, Switzerland, Norway or Liechtenstein.");

		protected static bool IsAirExportFromEuropeanUnionAviationSecurityMembers(ISupplyChainSecurityImportExportSupporter businessObject)
		{
			return businessObject.IsAir && CountryCodes.IsInEuropeanUnionAviationSecurityScheme(businessObject.LoadCountryForSupplyChainSecurity)
				&& !CountryCodes.IsInEuropeanUnionAviationSecurityScheme(businessObject.DischargeCountryForSupplyChainSecurity);
		}

		#endregion

		internal override void CheckJS_InspectionTypeCodeAdditionalValidation(ForwardingShipment shipment, ZPropertyInfo info, bool hasChanges)
		{
		}
	}
}
