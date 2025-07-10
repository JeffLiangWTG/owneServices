using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	public partial class AviationSecuritySupport : CommonAviationSecuritySupport, ILicensedComponent
	{
		public AviationSecuritySupport(ForwardingShipment shipmentBO, string countryCode = null)
			: base(shipmentBO, countryCode)
		{
		}

		protected new ForwardingShipment ShipmentBO => (ForwardingShipment)base.ShipmentBO;

		#region Aviation Security Helper methods

		internal bool SetApprovedShipperStatus(ZString reason, bool overrideUserEnteredValue)
		{
			var result = false;

			if (!overrideUserEnteredValue && ShipmentBO.IsFirstAirLegFlightDeparted())
			{
				return false;
			}

			if (!ShipmentBO.IsSettingDefaultOrImportingData && !ShipmentBO.IsDeleted && !IsHighRiskShipment)
			{
				var previousValue = ShipmentBO.JS_InspectionTypeCode;

				if (IsAviationSecurityApplicableForTransportMode
					&& (SupplyChainSecurityConfiguration.IsEnabled || SupplyChainSecurityConfiguration.UsesGenericScheme)
					&& (overrideUserEnteredValue || ShipmentInspectionTypeCodeCanBeOverwritten))
				{
					if (!overrideUserEnteredValue
						&& ShipmentInspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved
						&& SupplyChainSecurityConfiguration.AllowManualApprovedInspectionStatus(ShipmentBO)
						&& !SupplyChainSecurityConfiguration.IsInspectionTypeRecalculationRequired)
					{
						result = true;
					}
					else if (SupplyChainSecurityConfiguration.UseTranshipmentAviationSecurityStatus && SupplyChainSecurityConfiguration.IsTranshipment(ShipmentBO))
					{
						ShipmentInspectionTypeCode = BaseJobShipmentLookups.InspectionType_Transshipment;
						result = true;
					}
					else if (SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(ShipmentBO))
					{
						ShipmentInspectionTypeCode = SupplyChainSecurityConfiguration.CheckAdditionalConditionsForApprovedInspectionType(ShipmentBO)
								&& AreAllRelevantOrganisationsApprovedForAviationSecurity
								&& (!PassengerFlightValidationApplies || RelevantOrganisationsAreApprovedForShippingOnPassengerFlights)
								&& (overrideUserEnteredValue || SupplyChainSecurityConfiguration.AllowAutomaticCalculationOfApprovedStatus(ShipmentBO))
							? (ZString)BaseJobShipmentLookups.InspectionType_Approved
							: SupplyChainSecurityConfiguration.InspectionTypeDefault;

						result = true;
					}
				}
				else if (!IsAviationSecurityApplicableForTransportMode
					|| (!SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(ShipmentBO) && !SupplyChainSecurityConfiguration.IsTranshipment(ShipmentBO)))
				{
					ShipmentInspectionTypeCode = SupplyChainSecurityConfiguration.InspectionTypeDefault;
				}

				if (ShipmentBO.JS_InspectionTypeCode != previousValue)
				{
					ShipmentBO.MostRecentInspectionTypeChangeReason = reason;
					ShipmentBO.JS_InspectionTypeCodeOriginalValue = ShipmentBO.JS_InspectionTypeCode;
				}

				ShipmentBO.Validation.ValidateJS_InspectionTypeCode();
			}

			return result;
		}

		bool ShipmentInspectionTypeCodeCanBeOverwritten
		{
			get
			{
				return ShipmentInspectionTypeCode.IsEmpty
					|| ShipmentInspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved
					|| (ShipmentInspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code && !ShipmentBO.HasUserSelectedUnknownInspectionType)
					|| (ShipmentInspectionTypeCode == BaseJobShipmentLookups.InspectionType_Transshipment && SupplyChainSecurityConfiguration.UseTranshipmentAviationSecurityStatus);
			}
		}

		#region Aviation Security Approval

		public bool AreAllRelevantOrganisationsApprovedForAviationSecurity
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				var approvalParties = GetAviationSecurityRelevantParties(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes).ToArray();
				if (approvalParties.Any())
				{
					return approvalParties.All(x => x.IsAviationSecurityApproved);
				}

				var warningParties = GetAviationSecurityRelevantParties(SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning);
				return warningParties.Any(x => x.IsAviationSecurityApproved);
			}
		}

		public List<ZString> GetErrorsForRelevantOrganisationsWithoutAviationSecurityApproval()
		{
			var result = new List<ZString>();
			var approvalParties = GetAviationSecurityRelevantParties(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes).ToArray();
			var partiesWithInvalidApprovalForShipment = approvalParties.Where(party => party.KnownShipperRecord != null && !SupplyChainSecurityConfiguration.GetErrorForApprovalInvalidForShipment(party.KnownShipperRecord, ShipmentBO).IsEmpty).ToArray();

			result.AddRange(partiesWithInvalidApprovalForShipment.Select(party => SupplyChainSecurityConfiguration.GetErrorForApprovalInvalidForShipment(party.KnownShipperRecord, ShipmentBO)));

			var partiesWithExpiryDatesThatWillHaveLapsed = approvalParties.Where(o => o.ExpiryDateWillLapseBeforeShipmentDateForAviationSecurity).ToArray();

			if (partiesWithExpiryDatesThatWillHaveLapsed.Any())
			{
				result.Add(Res.GetString("3c0ecd0e-8cdb-4894-abc3-00ee49e697d7", "The following Organizations' known/approved status will lapse between the current date and the {0} of {1}, so an Inspection Type of Approved/Known shipper is not allowed:\r\n{2}",
					AviationSecurityDateTypes.GetAviationSecurityDateTypeDescription(ShipmentDateTypeForAviationSecurity),
					ShipmentDateForAviationSecurity.ToShortDateString(),
					new ZStringBuilder(partiesWithExpiryDatesThatWillHaveLapsed.Select(x => x.HumanReadableName)).ToStringWithNewLineBetweenAppends()));
			}

			var otherUnapprovedParties = approvalParties
				.Where(x => !partiesWithInvalidApprovalForShipment.Union(partiesWithExpiryDatesThatWillHaveLapsed).Select(party => party.OrganisationCode).Contains(x.OrganisationCode)
						&& !x.IsAviationSecurityApproved)
				.ToArray();

			if (otherUnapprovedParties.Any())
			{
				result.Add(Res.GetString("614b3fee-d133-40e9-8305-70f463071da2",
					"The following Organizations are not Approved so an Inspection Type of Approved/Known Shipper is not allowed:\r\n{0}",
					new ZStringBuilder(otherUnapprovedParties.Select(party => party.HumanReadableName)).ToStringWithNewLineBetweenAppends()));
			}

			return result;
		}

		public ZString GetWarningForRelevantOrganisationsWithoutAviationSecurityApproval()
		{
			var unapprovedOrganisations = GetAviationSecurityRelevantParties(SupplyChainSecurityOrganisationToUse.ValidationCodes.Warning)
				.Where(x => !x.IsAviationSecurityApproved)
				.Select(x => x.HumanReadableName)
				.ToArray();

			return unapprovedOrganisations.Any()
				? Res.GetString("5bc4ad9f-392b-48de-8a53-ea9b21e89a29", "The following Organizations are not Approved:\r\n{0}", new ZStringBuilder(unapprovedOrganisations).ToStringWithNewLineBetweenAppends())
				: string.Empty;
		}

		public override bool RelevantOrganisationsAreApprovedForShippingOnPassengerFlights
		{
			get { return GetErrorForUnapprovedOrganisationsShippingOnPassengerFlights().IsEmpty; }
		}

		internal ZString GetErrorForUnapprovedOrganisationsShippingOnPassengerFlights()
		{
			var approvalParties = GetAviationSecurityRelevantParties(SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes);
			var unapprovedOrgs = approvalParties
				.Where(x => !x.IsApprovedToShipOnPassengerFlights)
				.Select(x => Res.GetString("bdb8a32e-533b-44ef-8ac9-e8471366c392", "{0} - {1}", SupplyChainSecurityConfiguration.ApprovalCodeDescription(x.KnownShipperRecord.OV_EXApprovedOrMajorExporter), x.HumanReadableName))
				.ToArray();

			if (unapprovedOrgs.Any())
			{
				var builder = new ZStringBuilder(unapprovedOrgs);
				builder.Prepend(Res.GetString("068fd887-8dbe-4b67-86dd-c2f40acf0c7d", "The following Organizations can only be Approved (APP) to ship on Cargo Only flights. At least one Consol attached to this Shipment is linked to a Passenger flight. Either detach the Shipment from the Consol, select a different flight, or screen your cargo with an Inspection Type approved for Passenger flights."));
				return builder.ToStringWithNewLineBetweenAppends();
			}

			return ZString.Empty;
		}

		public IEnumerable<ZString> OrganisationsTypesToUseForAviationSecurity
		{
			get
			{
				return SupplyChainSecurityConfiguration.OrganisationsToUse.Values.Where(x => x.ValidationCode == SupplyChainSecurityOrganisationToUse.ValidationCodes.Yes)
					.Select(x => x.OrganisationDescription);
			}
		}

		public IEnumerable<AviationSecurityRelevantParty> GetAviationSecurityRelevantParties(string validationCode)
		{
			foreach (var requiredOrganisationType in SupplyChainSecurityConfiguration.OrganisationsToUse.Values
				.Where(x => x.ValidationCode == validationCode))
			{
				var relevantParty = AviationSecurityRelevantParty.New(ShipmentBO, requiredOrganisationType.OrganisationCode, CountryCode);
				if (relevantParty != null)
				{
					yield return relevantParty;
				}
			}
		}

		internal bool PassengerFlightValidationApplies
		{
			get
			{
				return SupplyChainSecurityConfiguration.IsEnabled
					&& ShipmentBO.Consols.Cast<ForwardingConsol>().Any(consol => SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol))
					&& ShipmentBO.Consols.Cast<ForwardingConsol>().Select(consol => consol.Transports.FirstTransportWithTransportMode(TransportModes.Air))
						.Any(x => x != null && !x.JW_IsCargoOnly);
			}
		}

		#endregion

		public ZString ErrorMessageForCantSetApprovedShipperStatus
		{
			get
			{
				var reason = ReasonForAviationSecurityNotBeingAvailable;
				if (!reason.IsEmpty)
				{
					return reason;
				}

				if (IsHighRiskShipment)
				{
					return Res.GetString("efe55000-1b5c-45d8-91a4-ef57584fc476", "Inspection Status cannot be calculated for High Risk shipments.");
				}

				return SupplyChainSecurityConfiguration.UseTranshipmentAviationSecurityStatus
					? Res.GetString("353a71e3-28c9-46a6-a441-618c5002f801", "Inspection Status can only be calculated for Air Exports or Transhipments.")
					: Res.GetString("33a07e20-83db-467b-9d27-945861e12084", "Inspection Status can only be calculated for Air shipments where the Origin Country/Region matches the current Login Country/Region.");
			}
		}

		internal ZString GetErrorForApprovalInvalidForShipment(OrgCountryData countryData)
		{
			return SupplyChainSecurityConfiguration.GetErrorForApprovalInvalidForShipment(countryData, ShipmentBO);
		}

		#region Shipment Inspection Type Code

		ZString ShipmentInspectionTypeCode
		{
			get => ShipmentBO.GetInspectionTypeCodeForCountry(CountryCode);
			set => ShipmentBO.SetInspectionTypeCodeForCountry(CountryCode, value);
		}

		internal void AddInspectionTypeErrorOrWarning(ZString notification, ZPropertyInfo info = null)
		{
			if (info == null)
			{
				info = ShipmentBO.JS_InspectionTypeCodeInfo;
			}

			if (ActualDepartureDate.IsInThePast() && !ShipmentBO.JS_InspectionTypeCodeHasChanges)
			{
				info.AddWarning(notification);
			}
			else
			{
				info.AddError(notification);
			}
		}

		#endregion

		#region Shipment Date

		internal ZDateTime ShipmentDateForAviationSecurity
		{
			get
			{
				if (SupplyChainSecurityConfiguration.IsEnabled)
				{
					var dateType = ShipmentDateTypeForAviationSecurity;
					return dateType == AviationSecurityDateTypes.MasterBillIssueDate ? MasterBillIssueDate
						: dateType == AviationSecurityDateTypes.ConsolETD ? ConsolETD
						: dateType == AviationSecurityDateTypes.ShipmentETD ? ShipmentBO.JS_E_DEP
						: dateType == AviationSecurityDateTypes.ShipmentCreatedDate ? ShipmentBO.JS_SystemCreateTimeUtc.ToLocalBranchTime()
						: ZDateTime.Now;
				}

				return ZDateTime.Now;
			}
		}

		internal ZString ShipmentDateTypeForAviationSecurity
		{
			get
			{
				if (SupplyChainSecurityConfiguration.IsEnabled)
				{
					return MasterBillIssueDate.IsValid ? AviationSecurityDateTypes.MasterBillIssueDate
						: ConsolETD.IsValid ? AviationSecurityDateTypes.ConsolETD
						: ShipmentBO.JS_E_DEP.IsValid ? AviationSecurityDateTypes.ShipmentETD
						: ShipmentBO.JS_SystemCreateTimeUtc.IsValid ? AviationSecurityDateTypes.ShipmentCreatedDate
						: AviationSecurityDateTypes.CurrentDate;
				}

				return AviationSecurityDateTypes.CurrentDate;
			}
		}

		ZDateTime MasterBillIssueDate => FirstAirConsol?.JK_MasterBillIssueDate ?? ZDateTime.Empty;

		ZDateTime ConsolETD => FirstAirConsol?.Transports.FirstTransportWithTransportMode(TransportModes.Air)?.JW_ETD ?? ZDateTime.Empty;

		internal ZDateTime ActualDepartureDate
		{
			get
			{
				if (ShipmentBO.Consols.Any())
				{
					return FirstAirConsol?.Transports.FirstTransportWithTransportMode(TransportModes.Air)?.JW_ATD ?? ZDateTime.Empty;
				}

				ShipmentBO.Transports.Sort(MovementLegComparer.PortsAndDatesBased(ShipmentBO.Transports));
				return ShipmentBO.Transports.Cast<Transport>().FirstOrDefault(t => t.JW_TransportMode == TransportModes.Air)?.JW_ATD ?? ZDateTime.Empty;
			}
		}

		ForwardingConsol FirstAirConsol => MovementLegComparer.FirstOrDefaultLegForTransportMode(ShipmentBO.Consols.Cast<ForwardingConsol>(), TransportModes.Air);

		public static class AviationSecurityDateTypes
		{
			public const string MasterBillIssueDate = "MBL";
			public const string ConsolETD = "CON";
			public const string ShipmentETD = "SHP";
			public const string ShipmentCreatedDate = "CRE";
			public const string CurrentDate = "NOW";

			public static ZString GetAviationSecurityDateTypeDescription(string aviationSecurityDateType)
			{
				switch (aviationSecurityDateType)
				{
					case MasterBillIssueDate:
						return Res.GetString("0add4e10-ec77-4078-a2c8-e9861c0ac5b9", "Master Bill Issue Date");

					case ConsolETD:
						return Res.GetString("518838dd-638a-4e1a-a23b-cc8c7c27f688", "Consol ETD");

					case ShipmentETD:
						return Res.GetString("9724a815-f1bd-4de1-88e2-f25c9234c7db", "Shipment ETD");

					case ShipmentCreatedDate:
						return Res.GetString("c6ab9b6e-8e2a-4015-a32d-61219feeb3c1", "Shipment creation date");

					case CurrentDate:
						return Res.GetString("126da910-4c74-49f0-9fe3-b808495cfed5", "Current date");
				}

				return ZString.Empty;
			}
		}

		#endregion

		#endregion

		#region ILicensedComponent Members

		IDisposable ILicensedComponent.LicensedComponentManager
		{
			get { return this.licensedComponentManager ?? (this.licensedComponentManager = new LicensedComponentManager(this)); }
		}
		LicensedComponentManager licensedComponentManager;

		#endregion

		#region SupplyChainSecurityConfiguration

		public SupplyChainSecurityConfiguration SupplyChainSecurityConfiguration
		{
			get { return supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = SupplyChainSecurityConfiguration.New(CountryCode)); }
		}
		SupplyChainSecurityConfiguration supplyChainSecurityConfiguration;

		internal SupplyChainSecurityConfiguration DestinationSupplyChainSecurityConfiguration
		{
			get
			{
				var countryCode = ShipmentBO.JS_RL_NKDestination.SubstringSafe(0, 2);
				var cacheKey = "AviationSecuritySupport.DestinationSupplyChainSecurityConfiguration." + countryCode;
				return ShipmentBO.Factory.GetCachedValue(cacheKey, delegate
				{
					return SupplyChainSecurityConfiguration.New(countryCode);
				});
			}
		}

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.Freight.Forwarding.Business
{
	public partial class AviationSecuritySupport
	{
		internal void ResetSupplyChainSecurityConfigurationForTesting()
		{
			supplyChainSecurityConfiguration = null;
		}
	}
}

#endif
#endregion
