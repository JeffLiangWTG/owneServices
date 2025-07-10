using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
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
	public class SupplyChainSecurityConfigurationAU : SupplyChainSecurityConfiguration
	{
		#region Inspection Status

		internal override bool CheckAdditionalConditionsForApprovedInspectionType(ForwardingShipment shipment)
		{
			if (!IsEnabled)
			{
				return base.CheckAdditionalConditionsForApprovedInspectionType(shipment);
			}

			var approvalCode = GetAgentTypeApprovalCode(shipment);
			return ApprovalCodeIsAgentType(approvalCode);
		}

		internal override void CheckJS_InspectionTypeCodeAdditionalValidation(ForwardingShipment shipment, ZPropertyInfo info, bool hasChanges)
		{
			base.CheckJS_InspectionTypeCodeAdditionalValidation(shipment, info, hasChanges);

			if (info.HasErrors())
			{
				return;
			}

			var inspectionTypeCode = info.Value.ToString();
			var approvalType = GetAgentTypeApprovalCode(shipment);

			var errorForScreeningIsNotAllowedWithConsol = Res.GetString("ca35aed3-5ce1-4b93-931a-7ff2977aaec4",
				"The Inspection Type of {0} cannot be used as the Consol Sending Agent does not have a RACA Known/Approved status. Ensure the Sending Agent's Organization > Supply Chain Security tab has valid RACA details.", shipment.JS_InspectionTypeCode);
			var errorForScreeningIsNotAllowedWithoutConsol = Res.GetString("ee976e73-46e0-4e0b-9fa4-75d4fabfae40",
				"The Inspection Type of {0} cannot be used as the login Branch and Company Organization Proxy do not have a valid RACA Known/Approval status within their Organization > Supply Chain Security tab.", shipment.JS_InspectionTypeCode);

			if (inspectionTypeCode != InspectionTypeDefault
				&& inspectionTypeCode != ShipmentInspectionType.AviationSecurity_Unknown_Code
				&& IsRACAWithoutConsol(shipment, approvalType))
			{
				info.AddWarning(Res.GetString("598cd5f1-93e8-4c0d-befa-fae969fc31bc", "Your login branch/company is a RACA so you can apply a screened status. When a Consolidation is added to this Shipment, the Sending Agent's address will be checked to ensure it is an approved facility. If not, this warning will become an error."));
			}
			else if (approvalType == AviationSecuritySchemeMembership.Codes.AccreditedAirCargoAgent
				&& inspectionTypeCode != InspectionTypeDefault
				&& inspectionTypeCode != ShipmentInspectionType.AviationSecurity_Unknown_Code
				&& inspectionTypeCode != BaseJobShipmentLookups.InspectionType_Approved
				&& !ExemptionCodesList.ContainsCode(inspectionTypeCode))
			{
				info.AddError(errorForScreeningIsNotAllowedWithConsol);
			}
			else if (inspectionTypeCode != InspectionTypeDefault
				&& inspectionTypeCode != ShipmentInspectionType.AviationSecurity_Unknown_Code
				&& IsRACAApprovalHeldAgainstAnotherConsolSendingAgentAddress(shipment, approvalType))
			{
				info.AddError(Res.GetString("031679d1-7a00-41c8-a1fd-3faf955c4cab", "The Consol Sending Agent's address does not match the address of the RACA approval."));
			}
			else if (approvalType.IsEmpty
				&& inspectionTypeCode != ShipmentInspectionType.AviationSecurity_Unknown_Code
				&& inspectionTypeCode != InspectionTypeDefault)
			{
				info.AddError(shipment.DepartureConsol == null ? errorForScreeningIsNotAllowedWithoutConsol : errorForScreeningIsNotAllowedWithConsol);
			}
		}

		bool IsRACAWithoutConsol(ForwardingShipment shipment, ZString approvalType)
		{
			return (approvalType == AviationSecuritySchemeMembership.Codes.RegulatedAgent || approvalType == AviationSecuritySchemeMembership.Codes.RegulatedAgentEACE)
				&& shipment.DepartureConsol == null;
		}

		bool IsRACAApprovalHeldAgainstAnotherConsolSendingAgentAddress(ForwardingShipment shipment, ZString approvalType)
		{
			return approvalType.IsEmpty
				&& shipment.DepartureConsol?.SendingForwarder != null
				&& shipment.DepartureConsol.SendingForwarder.Addresses.Cast<OrgAddress>()
					.Any(x => x.KnownShipper != null
						&& (x.KnownShipper.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.RegulatedAgent || x.KnownShipper.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.RegulatedAgentEACE)
						&& x.KnownShipper.OV_EXApprovalExpiryDate > shipment.AviationSecurity.ShipmentDateForAviationSecurity.Date);
		}

		ZString GetAgentTypeApprovalCode(ForwardingShipment shipment)
		{
			var code = GetRACATypeApprovalCode(shipment);
			if (code.IsEmpty)
			{
				code = GetAACATypeApprovalCode(shipment);
			}

			return code;
		}

		ZString GetRACATypeApprovalCode(ForwardingShipment shipment)
		{
			var shipmentDate = shipment.AviationSecurity.ShipmentDateForAviationSecurity;
			if (TryGetRACAApprovalCodeFromAddresses(shipmentDate, new[] { shipment.DepartureConsol?.SendingForwarderAddress }, out var approvalCode))
			{
				return approvalCode;
			}

			if (shipment.DepartureConsol == null)
			{
				if (TryGetRACAApprovalCodeFromAddresses(shipmentDate, GlbBranch.CurrentBranch.OrgProxy?.Addresses.Cast<OrgAddress>(), out approvalCode))
				{
					return approvalCode;
				}

				if (TryGetRACAApprovalCodeFromAddresses(shipmentDate, GlbCompany.CurrentCompany.OrgProxy?.Addresses.Cast<OrgAddress>(), out approvalCode))
				{
					return approvalCode;
				}
			}

			return ZString.Empty;
		}

		bool TryGetRACAApprovalCodeFromAddresses(ZDateTime dateTime, IEnumerable<OrgAddress> addresses, out string approvalCode)
		{
			approvalCode = ZString.Empty;
			var approvalAddress = addresses?.FirstOrDefault(x => x?.KnownShipper != null
				&& x.KnownShipper.OV_EXApprovalExpiryDate > dateTime.Date
				&& (x.KnownShipper.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.RegulatedAgent || x.KnownShipper.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.RegulatedAgentEACE));

			if (approvalAddress == null)
			{
				return false;
			}

			approvalCode = approvalAddress.KnownShipper.OV_EXApprovedOrMajorExporter;
			return true;
		}

		ZString GetAACATypeApprovalCode(ForwardingShipment shipment)
		{
			var shipmentDate = shipment.AviationSecurity.ShipmentDateForAviationSecurity.Date;

			if (GlbBranch.CurrentBranch.OrgProxy?.MainAddress.KnownShipper != null
				&& GlbBranch.CurrentBranch.OrgProxy.MainAddress.KnownShipper.OV_EXApprovalExpiryDate >= shipmentDate
				&& GlbBranch.CurrentBranch.OrgProxy.MainAddress.KnownShipper.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.AccreditedAirCargoAgent)
			{
				return GlbBranch.CurrentBranch.OrgProxy.MainAddress.KnownShipper.OV_EXApprovedOrMajorExporter;
			}

			if (GlbCompany.CurrentCompany.OrgProxy?.MainAddress?.KnownShipper != null
				&& GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipper.OV_EXApprovalExpiryDate >= shipmentDate
				&& GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipper.OV_EXApprovedOrMajorExporter == AviationSecuritySchemeMembership.Codes.AccreditedAirCargoAgent)
			{
				return GlbCompany.CurrentCompany.OrgProxy.MainAddress.KnownShipper.OV_EXApprovedOrMajorExporter;
			}

			return ZString.Empty;
		}

		internal override bool GetPartyTypeChangeForcesRecalculateApproveShipperStatus(string partyType)
		{
			return partyType == SupplyChainSecurityOrganisationTypes.ConsolSendingAgent;
		}

		#endregion

		#region Organisations To Use

		protected override SupplyChainSecurityOrganisationToUseCollection OrganisationsToUseCollection => FreightDataRegistry.Instance.ShipmentInspectionOrganisationToUse_Australia.Value;

		#endregion

		#region Approval Codes Configuration

		protected override Dictionary<ZString, ApprovalCodeConfiguration> GetApprovalCodeConfigurations()
		{
			var approvalNumberFormat = @"^[0-9]{5}-[0-9]{2}$";
			var errorForApprovalNumberFormatNotMet = Res.GetString("5759fc74-32eb-41c5-a835-4f5d825f530a", "The Approval Number must be in the following format: 'NNNNN-NN'. For example: 00002-01.");
			var errorForApprovalNumberNotEntered = Res.GetString("42299362-26c6-4286-8bde-4bfe6b139148", @"Enter the number from the Transport Security Programme (TSP) provided by OTS that proves regulated status of this Organization.  
This is a five-digit number given to each regulated agent, followed by a two-digit suffix.

Note that the number in the TSP may be preceded by a letter but ignore that letter – only enter the five digit number and a two digit suffix, for example 12345-00. 

The two-digit suffix indicates the address which is regulated (e.g. 01 or 02, etc.). If this is only one address use 00.");

			var result = new Dictionary<ZString, ApprovalCodeConfiguration>();
			result.Add(AviationSecuritySchemeMembership.Codes.RegulatedAgentEACE, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.RegulatedAgentEACE)
			{
				IsValidForAviationSecurity = true,
				RequiresApprovalNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				ApprovalNumberFormat = approvalNumberFormat,
				ErrorForApprovalNumberFormatNotMet = errorForApprovalNumberFormatNotMet,
				ErrorForOwnAgentApprovalNumberNotEntered = errorForApprovalNumberNotEntered,
				MaximumApprovalValidityInYears = 5,
				IsAgentType = true,
				DefaultAddressType = OrgAddressType.Office
			});

			result.Add(AviationSecuritySchemeMembership.Codes.RegulatedAgent, new ApprovalCodeConfiguration(ResString.GetMultilingualString("a5b05cc8-9db2-4096-81b7-7463981404bc", "RACA - Regulated Air Cargo Agent (ACE Notice)"))
			{
				IsValidForAviationSecurity = true,
				RequiresApprovalNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				ApprovalNumberFormat = approvalNumberFormat,
				ErrorForApprovalNumberFormatNotMet = errorForApprovalNumberFormatNotMet,
				ErrorForOwnAgentApprovalNumberNotEntered = errorForApprovalNumberNotEntered,
				MaximumApprovalValidityInYears = 5,
				IsAgentType = true,
				DefaultAddressType = OrgAddressType.Office
			});

			result.Add(AviationSecuritySchemeMembership.Codes.AccreditedAirCargoAgent, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.AccreditedAirCargoAgent)
			{
				IsValidForAviationSecurity = true,
				RequiresApprovalNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				ApprovalNumberFormat = approvalNumberFormat,
				ErrorForApprovalNumberFormatNotMet = errorForApprovalNumberFormatNotMet,
				ErrorForOwnAgentApprovalNumberNotEntered = errorForApprovalNumberNotEntered,
				MaximumApprovalValidityInYears = 5,
				IsAgentType = true,
				DefaultAddressType = OrgAddressType.Office,
				HideApprovalNumberOnSecurityDeclaration = true
			});

			result.Add(AviationSecuritySchemeMembership.Codes.KnownConsignor, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.KnownConsignor)
			{
				IsValidForAviationSecurity = true,
				RequiresApprovalNumber = true,
				AllowsDuplicateApprovalNumbers = true,
				ApprovalNumberFormat = approvalNumberFormat,
				ErrorForApprovalNumberFormatNotMet = errorForApprovalNumberFormatNotMet,
				ErrorForOwnAgentApprovalNumberNotEntered = errorForApprovalNumberNotEntered,
				MaximumApprovalValidityInYears = 5
			});

			result.Add(AviationSecuritySchemeMembership.Codes.RegularCustomer, new ApprovalCodeConfiguration(AviationSecuritySchemeMembership.Descriptions.RegularCustomer));

			result.Add(AviationSecuritySchemeMembershipEx.Codes.No, new ApprovalCodeConfiguration(AviationSecuritySchemeMembershipEx.Descriptions.No)
			{
				IsApprovedToShipOnPassengerFlights = false
			});

			return result;
		}

		#endregion

		#region Passenger Flight Validation

		public override bool PassengerFlightValidationApplies => false;

		#endregion

		#region Known Shipper Types

		protected override ShipmentInspectionTypeCollection GetShipmentInspectionTypeCollection()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypes_Australia.Value.Types;
		}

		protected override ZString GetInspectionTypeDefault()
		{
			return FreightDataRegistry.Instance.ShipmentInspectionTypeDefault_Australia.Value;
		}

		#endregion

		#region Exemption Codes

		CodeDescriptionPairList ExemptionCodesList
		{
			get { return exemptionCodesList ?? (exemptionCodesList = new ExemptionCodesAU()); }
		}
		CodeDescriptionPairList exemptionCodesList;

		#endregion

		#region Security Declaration

		public override void CheckEH_AgentApprovalNumberAdditionalValidation(ConsolExportAWBHeader header)
		{
			base.CheckEH_AgentApprovalNumberAdditionalValidation(header);

			if (!IsEnabled || header.Consol == null)
			{
				return;
			}

			var consol = header.Consol;

			if (consol.SendingForwarder == null
				|| (consol.SendingForwarder.PK != GlbCompany.CurrentCompany.GC_OH_OrgProxy && consol.SendingForwarderPK != GlbBranch.CurrentBranch.GB_OH_OrgProxy))
			{
				header.EH_AgentApprovalNumberInfo.AddMessageError(Res.GetString("91ec3ea5-d7be-4ae7-9e26-18f90a46137f", "A Security Declaration cannot be issued for this consignment because the Sending Agent does not match the login (issuing) company."));
			}

			var consolETD = consol.Transports.DepartureTransport != null && consol.Transports.DepartureTransport.JW_ETD.IsValid
				? consol.Transports.DepartureTransport.JW_ETD.Date
				: ZDate.Today;

			var approvalCode = consol.SendingForwarderAddress?.KnownShipper != null && consol.SendingForwarderAddress.KnownShipper.OV_EXApprovalExpiryDate >= consolETD
				? consol.SendingForwarderAddress.KnownShipper.OV_EXApprovedOrMajorExporter
				: ZString.Empty;

			if (approvalCode == AviationSecuritySchemeMembership.Codes.RegulatedAgent || approvalCode == AviationSecuritySchemeMembership.Codes.RegulatedAgentEACE)
			{
				return;
			}
			else if (approvalCode == AviationSecuritySchemeMembership.Codes.AccreditedAirCargoAgent)
			{
				header.EH_AgentApprovalNumberInfo.AddMessageError(Res.GetString("9b00eb5a-d073-4a5b-8f4d-07e0168f5ff0", "The Sending Agent is an AACA so cannot clear cargo."));
			}
			else
			{
				header.EH_AgentApprovalNumberInfo.AddMessageError(Res.GetString("2b1507e1-a381-4183-bbb3-78adcbcea4aa", "Issuing Sending Agent does not have a regulated status for air cargo security."));
			}
		}

		public override ZBool ExportAWBAgentApprovalNumberCanBeOverridden
		{
			get { return false; }
		}

		#endregion

		#region Prohibited Routing

		public override ZString GetWarningForProhibitedRouting(ISupplyChainSecurityImportExportSupporter supportedBO)
		{
			if (supportedBO == null)
			{
				return ZString.Empty;
			}

			var warning = ZString.Empty;

			switch (supportedBO.LoadCountryForSupplyChainSecurity)
			{
				case CountryCodes.Bangladesh:
					warning = Res.GetString("a2cdb74e-8118-4e0c-a798-6c6d0c0fd0c4",
@"The Australian Government has imposed prohibitions on the carriage of air cargo that has originated from, or transited through, Bangladesh, unless it has undergone security examination at an approved last port of call before traveling to Australia, or is otherwise exempt from examination under Australian regulations. You are required to meet with government requirements and/or consider a change of transport mode.

Approved last ports of call are Abu Dhabi; Bangkok; Doha; Dubai; Guangzhou; Hong Kong; Kuala Lumpur or Singapore.

Approved examination methods are X-ray; explosive trace detection; or physical examination.");
					break;

				case CountryCodes.Egypt:
					warning = Res.GetString("383f68fb-b2d7-4c9f-8503-dd23697a6a0e", "The Australian Government has imposed prohibitions on the carriage of air cargo that has originated from, or transited through Egypt, except for items that are currently exempt from screening under Australian Regulations, such as diplomatic bags and smaller items of international mail. You are required to meet with government requirements and/or consider a change of transport mode.");
					break;

				case CountryCodes.Somalia:
				case CountryCodes.SyrianArabRepublic:
				case CountryCodes.Yemen:
					warning = Res.GetString("94ece212-7040-4052-8f31-0fb65917ea3b", "The Australian Government has imposed prohibitions on the carriage of air cargo that has originated from, or transited through, Syria, Yemen, and Somalia. You are required to meet with government requirements and/or consider a change of transport mode.");
					break;

				case CountryCodes.Turkey:
					warning = Res.GetString("e793b4e5-0069-446c-9777-004ac1bd4537", "The Australian Government has imposed prohibitions on air cargo that has originated from, or transited through, Turkey. However, this prohibition applies only to electromechanical devices that weigh over 1 kilogram. You are required to meet with government requirements and/or consider a change of transport mode.");
					break;
			}

			if (!warning.IsEmpty && HasProhibitedRouting(supportedBO))
			{
				return warning;
			}

			return ZString.Empty;
		}

		bool HasProhibitedRouting(ISupplyChainSecurityImportExportSupporter supportedBO)
		{
			if (IsProhibitedByLoadDischarge(supportedBO)
				|| IsProhibitedByTransportLegs(supportedBO))
			{
				return true;
			}

			return false;
		}

		bool IsProhibitedByLoadDischarge(ISupplyChainSecurityImportExportSupporter supportedBO)
		{
			return !supportedBO.SupplyChainSecurityRelatedTransports.Any()
				&& supportedBO.IsAir
				&& supportedBO.DischargeCountryForSupplyChainSecurity == CountryCodes.Australia;
		}

		bool IsProhibitedByTransportLegs(ISupplyChainSecurityImportExportSupporter supportedBO)
		{
			var orderedTransports = new TransportOrderHelper(supportedBO.SupplyChainSecurityRelatedTransports.Cast<Transport>().ToArray());
			var airTransports = orderedTransports.Where(t => t.IsAir);
			var firstAUDischargeTransport = airTransports.FirstOrDefault(t => t.JW_RL_NKDiscPort.SubstringSafe(0, 2) == CountryCodes.Australia);
			if (firstAUDischargeTransport != null)
			{
				foreach (var transport in airTransports)
				{
					if (transport.JW_RL_NKLoadPort.SubstringSafe(0, 2) == supportedBO.LoadCountryForSupplyChainSecurity)
					{
						return true;
					}

					if (transport.PK == firstAUDischargeTransport.PK)
					{
						break;
					}
				}
			}

			return false;
		}

		#endregion

		#region Pack Level Screening

		protected override bool IsOriginPackLevelScreeningRequired(ISupplyChainSecurityImportExportSupporter importExportSupporter)
		{
			return true;
		}

		protected override ZString PackLevelScreeningSchemeName => Res.GetString("79af41a3-6e7e-4ab2-afc8-e881d7e71ae8", "Air Cargo Piece Level Security Screening legislation");

		#endregion

		#region Registry

		protected override T GetRegistryItemToEnable<T>()
		{
			return (T)FreightDataRegistry.Instance.EnableSupplyChainSecurity_AU;
		}

		#endregion

		#region AWB

		public override bool ShouldSetSecurityStatement
		{
			get { return true; }
		}

		public override IEnumerable<ZString> GetAdditionalSecurityInformations(ForwardingConsol consol)
		{
			var result = new List<ZString>();
			if (!consol.AWBHeader.EH_AdditionalSecurityInformationStatement.IsEmpty)
			{
				result.Add(consol.AWBHeader.EH_AdditionalSecurityInformationStatement);
			}
			result.AddRange(base.GetAdditionalSecurityInformations(consol));
			return result;
		}

		#endregion
	}
}
