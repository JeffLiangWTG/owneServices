using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.NumberFountain;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentValidation : CommonShipmentValidation
	{
		public ForwardingShipmentValidation(ForwardingShipment parent)
			: base(parent)
		{
		}

		#region Implementation

		#region Parent

		public new ForwardingShipment Parent
		{
			get { return (ForwardingShipment)base.Parent; }
		}

		#endregion

		#region ManualShipmentNumberValidation

		ManualShipmentNumberValidation ManualShipmentNumberValidation
		{
			get
			{
				if (fManualShipmentNumberValidation == null)
				{
					fManualShipmentNumberValidation = new ManualShipmentNumberValidation();
				}

				return fManualShipmentNumberValidation;
			}
		}

		ManualShipmentNumberValidation fManualShipmentNumberValidation;

		#endregion

		internal void ValidateJS_UniqueConsignRefAfterConstraintFailure()
		{
			ConstraintErrorOccured = true;
			try
			{
				ValidateJS_UniqueConsignRef();
			}
			finally
			{
				ConstraintErrorOccured = false;
			}
		}

		bool ConstraintErrorOccured;

		#endregion

		#region ShouldValidateFKToCancelledRecord

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			return info.Name != JobShipmentSchema.JS_OH_HandledOnBehalfOfForwarder.Name && base.ShouldValidateFKToCancelledRecord(info);
		}

		#endregion

		#region Validate All

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateJS_Calc_EstimatedExportClearanceDate();
			ValidateTotalCO2eForBinding();
		}

		#endregion

		#region JS_RS_NKServiceLevel

		protected override void CheckJS_RS_NKServiceLevel()
		{
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsTransportModeActive(Parent?.JS_TransportMode ?? ZString.Empty))
			{
				ListValidation.ErrorIfInvalidCode(Parent.JS_RS_NKServiceLevelInfo, Parent.Lookups.RefServiceLevel_List);

				var deliveryDueDateCanBeCalculated = DeliveryDueDateCalculator.SupportedDeliveryModes.Contains(Parent.JS_HBLContainerPackModeOverride.ToString());
				if (deliveryDueDateCanBeCalculated)
				{
					ListValidation.ErrorIfInvalidCode(Parent.JS_RS_NKServiceLevelInfo);
				}

				var servicelevel = Parent.Factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, Parent.JS_RS_NKServiceLevel);
				if (servicelevel == null || servicelevel.RS_DefaultTransitHours <= 0)
				{
					ListValidation.WarnIfInvalidCode(Parent.JS_RS_NKServiceLevelInfo);
				}
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.JS_RS_NKServiceLevelInfo);
				if (Parent.IsHighVolumeLowValue)
				{
					MandatoryValidation.CheckEntered(Parent.JS_RS_NKServiceLevelInfo, Res.GetString("A9856399-47DD-4EBB-A2B5-68B36BBC4726", "Service Level for HVLV data"));
				}
			}
		}

		#endregion

		#region JS_UniqueConsignRef

		protected override void CheckJS_UniqueConsignRef()
		{
			base.CheckJS_UniqueConsignRef();

			if (Env.Registry.AllowManualShipmentEntry && !Parent.SuppressShipmentNumberValidation)
			{
				ManualShipmentNumberValidation.ValidateManualShipmentNumber(Parent, false);
				if (!Parent.JS_UniqueConsignRefInfo.HasErrors() && ConstraintErrorOccured)
				{
					Parent.JS_UniqueConsignRefInfo.AddError(Res.GetString("e7317a12-6136-4a7a-856a-839255c673cc", "Another Shipment with this number already exists. Please change the number."));
				}
			}
		}

		#endregion

		#region CheckJS_HouseBill

		protected override void CheckJS_HouseBill()
		{
			base.CheckJS_HouseBill();
			RunHouseBillCheckDigitValidation();
			CheckHouseBillLengthForBrazilAirTransport();
		}

		#region House Bill Check Digit validation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Non-translatable literal constant")]
		void RunHouseBillCheckDigitValidation()
		{
			if (Parent.IsPendingAllocationSetBySystem && Parent.JS_HouseBill.ToUpper() == "PENDING ALLOCATION..")
			{
				return;
			}

			if (Parent.HouseBillCheckDigitValidationCanBeApplied
				&& !Parent.JS_TransportMode.IsEmpty
				&& !Parent.JS_RL_NKOrigin.IsEmpty
				&& !Parent.JS_RL_NKDestination.IsEmpty
				&& !Parent.JS_HouseBill.IsEmpty)
			{
				var allRules = ForwardingConfigurationRegistry.Instance.HouseBillsNumberValidation.Value;

				var applicableRules = allRules.Cast<HouseBillsNumberValidation>()
							.OrderByDescending(x => HBLRuleMatchEvaluation(x))
							.Take(2)
							.Where(x => HBLRuleMatchEvaluation(x) > 0)
							.ToList();

				if (applicableRules.Count > 1)
				{
					if (HBLRuleMatchEvaluation(applicableRules[0]) > HBLRuleMatchEvaluation(applicableRules[1]))
					{
						RunRule(applicableRules[0]);
					}
					else
					{
						Parent.JS_HouseBillInfo.AddWarning(Res.GetString("2da61c95-2b35-4c6d-8b00-9562c0656c8a", "Check digit validation could not be run because more than one validation rule applies"));
					}
				}
				else if (applicableRules.Count == 1)
				{
					RunRule(applicableRules[0]);
				}
			}
		}

		void RunRule(HouseBillsNumberValidation rule)
		{
			if (rule.HBLLength > 0 && !HBLLengthMatches(Parent.JS_HouseBill, rule.HBLPrefix, HBLSuffixMatch(Parent.JS_HouseBill, rule.HBLSuffix), rule.HBLLength))
			{
				Parent.JS_HouseBillInfo.AddError(Res.GetString("82d9230d-35fd-4b91-b973-b60f816b78ef", "The length (excluding prefix and suffix) should be {0} characters.", rule.HBLLength));
			}
			else
			{
				ValidateCheckDigit(rule);
			}
		}

		bool HBLLengthMatches(ZString hbl, ZString matchedPrefix, Match suffixMatch, int expectedLength)
		{
			int actualLength = hbl.Length - matchedPrefix.Length;

			if (!suffixMatch.Success)
			{
				return false;
			}

			actualLength -= suffixMatch.Length;

			return actualLength == expectedLength;
		}

		Match HBLSuffixMatch(ZString hbl, ZString suffix)
		{
			StringBuilder sb = new StringBuilder(Regex.Escape(suffix));
			sb.Replace(@"\*", ".*"); // * matches zero or more characters
			sb.Replace(@"\?", "."); // ? matches one character exactly
			sb.Append("$");

			return Regex.Match(hbl, sb.ToString(), RegexOptions.IgnoreCase);
		}

		bool HBLUserDefinedConditionMatch(ZString macro)
		{
			if (macro.IsEmpty)
			{
				return true;
			}

			using (Parent.Factory.GetDocWrapperContextManager().SetContextValuesTemporarily(DocumentDirection.ANY, ContactType.All))
			{
				var genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(Constants.DataContext.GenericFreightJobFromShipment, Parent);

				try
				{
					return ZExpressionEvaluator.Evaluate(macro, Parent, genericWrappers);
				}
				catch (ExpressionEvaluationException)
				{
					return false;
				}
			}
		}

#if DEBUG
		internal
#endif
		int HBLRuleMatchEvaluation(HouseBillsNumberValidation rule)
		{
			int result = 0;

			if (rule.TransportMode == Parent.JS_TransportMode)
			{
				result += HBLMatchValues.TransportModeExact;
			}
			else if (rule.TransportMode == Constants.TransportModes.All)
			{
				result += HBLMatchValues.TransportModeAll;
			}
			else
			{
				return 0;
			}

			if (rule.Origin == Parent.JS_RL_NKOrigin)
			{
				result += HBLMatchValues.OriginPort;
			}
			else if (rule.Origin == Parent.JS_RL_NKOrigin.Left(2))
			{
				result += HBLMatchValues.OriginCountry;
			}
			else if (!rule.Origin.IsEmpty)
			{
				return 0;
			}

			if (rule.Destination == Parent.JS_RL_NKDestination)
			{
				result += HBLMatchValues.DestinationPort;
			}
			else if (rule.Destination == Parent.JS_RL_NKDestination.Left(2))
			{
				result += HBLMatchValues.DestinationCountry;
			}
			else if (!rule.Destination.IsEmpty)
			{
				return 0;
			}

			if (!rule.HBLPrefix.IsEmpty && Parent.JS_HouseBill.StartsWith(rule.HBLPrefix, StringComparison.OrdinalIgnoreCase))
			{
				result += HBLMatchValues.PrefixMatch;
			}
			else if (rule.HBLPrefix.IsEmpty)
			{
				result += HBLMatchValues.PrefixBlank;
			}
			else
			{
				return 0;
			}

			if (!rule.HBLSuffix.IsEmpty && HBLSuffixMatch(Parent.JS_HouseBill, rule.HBLSuffix).Success)
			{
				result += HBLMatchValues.SuffixMatch;
			}
			else if (rule.HBLSuffix.IsEmpty)
			{
				result += HBLMatchValues.SuffixBlank;
			}
			else
			{
				return 0;
			}

			if (rule.UserDefinedCondition.IsEmpty || HBLUserDefinedConditionMatch(rule.UserDefinedCondition))
			{
				result += HBLMatchValues.MacroMatch;
			}
			else
			{
				return 0;
			}

			return result;
		}

		class HBLMatchValues
		{
			internal const int TransportModeExact = 0x2000;
			internal const int TransportModeAll = 0x1000;
			internal const int OriginPort = 0x0200;
			internal const int DestinationPort = 0x0200;
			internal const int OriginCountry = 0x0100;
			internal const int DestinationCountry = 0x0100;
			internal const int PrefixMatch = 0x0020;
			internal const int SuffixMatch = 0x0020;
			internal const int PrefixBlank = 0x0010;
			internal const int SuffixBlank = 0x0010;
			internal const int MacroMatch = 0x0001;
		}

		void ValidateCheckDigit(HouseBillsNumberValidation rule)
		{
			if (!Parent.JS_HouseBill.IsEmpty && !rule.CheckDigitAlgorithm.IsEmpty && rule.CheckDigitAlgorithm != CheckDigitAlgorithmList.Codes.None)
			{
				int matchedSuffixLength = HBLSuffixMatch(Parent.JS_HouseBill, rule.HBLSuffix).Length;
				ZString suppliedCheck = Parent.JS_HouseBill.Substring(Parent.JS_HouseBill.Length - matchedSuffixLength - 1, 1);
				ZString numberBase = Parent.JS_HouseBill.Substring(0, Parent.JS_HouseBill.Length - matchedSuffixLength - 1);

				if (!rule.IncludeHBLPrefix && !rule.HBLPrefix.IsEmpty)
				{
					numberBase = numberBase.Substring(rule.HBLPrefix.Length);
				}

				var expectedCheck = CheckDigitHelper.CalculateCheckDigit(rule.CheckDigitAlgorithm, numberBase);
				if (expectedCheck != suppliedCheck)
				{
					Parent.JS_HouseBillInfo.AddError(Res.GetString("31561a13-102f-4645-9b35-f32679860806", "Incorrect Check Digit. The check digit should be '{0}'.", expectedCheck));
				}
			}
		}

		#endregion

		#region Brazil Air Transport Housebill max 11 chars long

		protected void CheckHouseBillLengthForBrazilAirTransport()
		{
			if (Parent.JS_TransportMode == Constants.TransportModes.Air
				&& Parent.JS_RL_NKDestination.SubstringSafe(0, 2) == Constants.CountryCodes.Brazil
				&& Parent.JS_HouseBill.Length > 11)
			{
				Parent.JS_HouseBillInfo.AddWarning(Res.GetString("8094AAF5-E7D0-4E20-B696-821A2CFC4A0F", "HAWB number should be 11 characters or less when destined to Brazil to comply with Cargo Control and Transit (CCT) system requirements."));
			}
		}

		#endregion

		#endregion

		#region JS_HouseBillType

		protected override void CheckJS_HouseBillOfLadingType()
		{
			base.CheckJS_HouseBillOfLadingType();
			if (!Parent.IsAir && !Parent.JS_IsBooking)
			{
				ListValidation.ErrorIfInvalidCode(Parent.JS_HouseBillOfLadingTypeInfo, Parent.Lookups.JS_HouseBillOfLadingType_List);
			}
		}

		#endregion

		#region JS_Phase

		protected override void CheckJS_Phase()
		{
			base.CheckJS_Phase();
			ListValidation.ErrorIfInvalidCode(Parent.JS_PhaseInfo, Parent.Lookups.Phases);
		}

		#endregion

		#region JS_InspectionTypeCode

		protected override void CheckJS_InspectionTypeCode()
		{
			if (Parent.JS_InspectionTypeCodeOriginalValue == BaseJobShipmentLookups.InspectionType_Approved && Parent.JS_InspectionTypeCodeHasChanges)
			{
				var errorMessages = Parent.AviationSecurity.SupplyChainSecurityConfiguration.GetStaffHandlingSecuredCargoUncertificatedErrorMessages();
				if (!errorMessages.IsEmpty)
				{
					Parent.JS_InspectionTypeCodeInfo.AddError(errorMessages);
				}
			}

			if (Parent.JS_InspectionTypeCodeHasChanges && !Parent.JS_InspectionTypeCode.Equals(Parent.JS_InspectionTypeCodeOriginalValue))
			{
				var error = Parent.AviationSecurity.SupplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(Parent);
				if (!error.IsEmpty)
				{
					Parent.JS_InspectionTypeCodeInfo.AddError(error);
				}
			}

			if (Parent.JS_InspectionTypeCode != BaseJobShipmentLookups.InspectionType_Web && !Parent.IsMasterInSubShipmentContext)
			{
				if (BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(Parent.Factory) || Parent.IsFirstAirLegFlightDeparted() && !Parent.JS_InspectionTypeCodeHasChanges)
				{
					return;
				}
				base.CheckJS_InspectionTypeCode();

				if (Parent.JS_InspectionTypeCode == "EXM")
				{
					if (!Parent.IsInDatabase || Parent.JS_SystemCreateTimeUtc > FreightDataRegistry.Instance.EXMExemptionCodeRemovalDate.Value)
					{
						ListValidation.ErrorIfInvalidCode(Parent.JS_InspectionTypeCodeInfo, Parent.Lookups.InspectionTypes);
					}
				}
				else if (Parent.JS_InspectionTypeCodeHasChanges || !Parent.IsInDatabase)
				{
					ListValidation.ErrorIfInvalidCode(Parent.JS_InspectionTypeCodeInfo, Parent.Lookups.InspectionTypes);
				}

				if (Parent.AviationSecurity.IsAviationSecurityApplicableForTransportMode && Parent.AviationSecurity.SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(Parent))
				{
					var reason = Parent.AviationSecurity.ReasonForAviationSecurityNotBeingAvailable;
					if (!reason.IsEmpty)
					{
						Parent.JS_InspectionTypeCodeInfo.AddWarning(reason);
					}
					else
					{
						MandatoryValidation.CheckEntered(Parent.JS_InspectionTypeCodeInfo);

						ValidateInspectionAndAdditionalInspectionCannotBeTheSame(Parent.JS_InspectionTypeCodeInfo);
						ValidateSecuredPackLinesFromWarehouseIfNeeded();

						if (Parent.AviationSecurity.SupplyChainSecurityConfiguration.IsEnabled)
						{
							Parent.AviationSecurity.SupplyChainSecurityConfiguration.CheckJS_InspectionTypeCodeAdditionalValidation(Parent, Parent.JS_InspectionTypeCodeInfo, Parent.JS_InspectionTypeCodeHasChanges);
						}

						if (!Parent.JS_InspectionTypeCodeInfo.HasErrors())
						{
							ValidateUnknownInspectionTypeCode(Parent.JS_InspectionTypeCodeInfo, Parent.JS_InspectionTypeCodeHasChanges);
							ValidateApprovedInspectionTypeCode();
							ValidateInspectionTypeAllowedForPassengerFlights(Parent.JS_InspectionTypeCodeInfo);
							ValidateScreenedInspectionTypeCode();
						}
					}

					var shipmentInspectionType = Parent.AviationSecurity.SupplyChainSecurityConfiguration.ShipmentInspectionTypeCollection.FindByCode(Parent.JS_InspectionTypeCode) as ShipmentInspectionType;
					if (shipmentInspectionType != null && !shipmentInspectionType.SystemDefined)
					{
						Parent.JS_InspectionTypeCodeInfo.AddWarning(Res.GetString("8e358e5a-9f5f-4a24-ab57-8481edcc8e60", "This is a custom inspection type. Custom inspection types, if not authorized by LGA, will cause rejection by airlines and may result in cargo being re-screened, delayed or not uplifted."));
					}
				}

				if (!Parent.MostRecentInspectionTypeChangeReason.IsEmpty)
				{
					Parent.JS_InspectionTypeCodeInfo.AddWarning(Res.GetString("bcc09716-f240-406a-bee5-e0e31e9006f0", "{0} so the Shipment's known status has been recalculated by CW1.", Parent.MostRecentInspectionTypeChangeReason));
				}
				if (Parent.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Screened && !Parent.AviationSecurity.SupplyChainSecurityConfiguration.IsPackLevelScreeningAvailable(Parent))
				{
					Parent.JS_InspectionTypeCodeInfo.AddError(Res.GetString("70814284-885f-471e-92f9-8f0ae2aedb4f", "SCR - Screened cannot be chosen here as it applies when all Packing Inspections are entered. Packing Inspections are not available on this Shipment."));
				}
			}
		}

		void ValidateSecuredPackLinesFromWarehouseIfNeeded()
		{
			if (Parent.RequiresSecuredCargoFromWarehouse && Parent.JS_IsForwardRegistered)
			{
				var inspectionTypeCode = Parent.JS_InspectionTypeCode;
				if ((inspectionTypeCode == BaseJobShipmentLookups.InspectionType_Screened || inspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved)
					&& Parent.JS_InspectionTypeCodeHasChanges
					&& Parent.RequiresSecuredCargoFromWarehouse
					&& Parent.OuterPackLines.Cast<ForwardingPackLine>().Any(p => p.JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code))
				{
					Parent.JS_InspectionTypeCodeInfo.AddError(Res.GetString("19d569ba-a600-4378-bfe7-8a1a592797f9", "At least one pack line’s Inspection status is 'UNK - Unknown' so either it is pending receipt at the warehouse or its secured status has not yet been advised by the Warehouse."));
				}
			}
		}

		void ValidateInspectionTypeAllowedForPassengerFlights(ZPropertyInfo info)
		{
			if (Parent.AviationSecurity.PassengerFlightValidationApplies)
			{
				ZString code = info.Value.ToString();

				if (!Parent.AviationSecurity.IsAllowedOnPassengerFlights(code))
				{
					info.AddError(Res.GetString("7c654e2b-6876-4722-86f6-9db4c262e0d1", "Inspection Type '{0}' is not allowed for passenger flights.", code));
				}

				if (code == BaseJobShipmentLookups.InspectionType_Approved)
				{
					var errorForUnapprovedOrganisationShippingOnPassengerFlights = Parent.AviationSecurity.GetErrorForUnapprovedOrganisationsShippingOnPassengerFlights();
					if (!errorForUnapprovedOrganisationShippingOnPassengerFlights.IsEmpty)
					{
						info.AddError(errorForUnapprovedOrganisationShippingOnPassengerFlights);
					}
				}
			}
		}

		void ValidateApprovedInspectionTypeCode()
		{
			if ((Parent.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved
				|| Parent.JS_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code)
				&& !Parent.IsTemplate)
			{
				if (Parent.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved
					&& !Parent.AviationSecurity.AreAllRelevantOrganisationsApprovedForAviationSecurity
					&& !Parent.AviationSecurity.SupplyChainSecurityConfiguration.AllowManualApprovedInspectionStatus(Parent))
				{
					var errorsForRelevantOrganisationsWithoutAviationSecurityApproval = Parent.AviationSecurity.GetErrorsForRelevantOrganisationsWithoutAviationSecurityApproval();
					foreach (var error in errorsForRelevantOrganisationsWithoutAviationSecurityApproval)
					{
						if (Parent.AviationSecurity.SupplyChainSecurityConfiguration.AllowRelevantOrganisationsWithoutAviationSecurityApproval)
						{
							Parent.JS_InspectionTypeCodeInfo.AddWarning(error);
						}
						else
						{
							Parent.AviationSecurity.AddInspectionTypeErrorOrWarning(error);
						}
					}

					if (!errorsForRelevantOrganisationsWithoutAviationSecurityApproval.Any() && Parent.AviationSecurity.OrganisationsTypesToUseForAviationSecurity.Any())
					{
						Parent.AviationSecurity.AddInspectionTypeErrorOrWarning(Res.GetString("c7fe513b-da63-4a0e-a49e-97477897853d", "This {0} is not Approved so an Inspection Type of Approved/Known Shipper is not allowed.", ZString.Join(", ", Parent.AviationSecurity.OrganisationsTypesToUseForAviationSecurity.ToArray())));
					}
				}

				var warning = Parent.AviationSecurity.GetWarningForRelevantOrganisationsWithoutAviationSecurityApproval();
				if (!warning.IsEmpty)
				{
					Parent.JS_InspectionTypeCodeInfo.AddWarning(warning);
				}
			}

			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.HongKong &&
					Parent.AviationSecurity.SupplyChainSecurityConfiguration.OrganisationsToUse.Values.All(x => x.ValidationCode == SupplyChainSecurityOrganisationToUse.ValidationCodes.No) &&
					Parent.JS_InspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code)
			{
				var warningText = Res.GetString("5c99a8de-396d-433c-a13b-c2fb99440b19", @"Inspection Status - This Shipment has not been checked for Known/Unknown Organizations. Review and ensure this Shipment should be changed to Approved.

No Organizations have been configured to be checked in this Registry setting:

Freight > Supply Chain Security > Hong Kong > Organization to Use for Supply Chain Security");

				Parent.JS_InspectionTypeCodeInfo.AddWarning(warningText);
			}

			ValidateApprovedInspectionTypeIsNotUsedForHighRiskShipment();
		}

		void ValidateApprovedInspectionTypeIsNotUsedForHighRiskShipment()
		{
			if (Parent.AviationSecurity.IsHighRiskShipment && Parent.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved)
			{
				Parent.JS_InspectionTypeCodeInfo.AddError(Res.GetString("a1167e70-2a7b-46fe-82bc-e58e5a8f9014", "{0} is not allowed for shipments identified as high risk.", BaseJobShipmentLookups.InspectionType_Approved));
			}
		}

		void ValidateScreenedInspectionTypeCode()
		{
			var shipmentPacklines = Parent.OuterPackLines.Cast<ForwardingPackLine>();

			if (Parent.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Screened && !shipmentPacklines.Any())
			{
				Parent.JS_InspectionTypeCodeInfo.AddError(Res.GetString("d4b1ab5f-39a8-49c7-bd69-f5f3f9b74fa4", "SCR - Screened status cannot be used as there are no packlines."));
			}

			if (Parent.JS_IsForwardRegistered)
			{
				if (Parent.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Screened
					&& shipmentPacklines.Any(p => !p.JL_InspectionTypeCodeInfo.ReadOnly && p.JL_InspectionTypeCode.IsEmpty || p.JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code))
				{
					Parent.JS_InspectionTypeCodeInfo.AddError(Res.GetString("b8e1920f-310e-4db8-b92f-12207f4016cb", "Inspection method SCR – Screened means each packline of the shipment is screened. You have packlines with UNK status. Either ensure each packline has an additional inspection entered that is not UNK - Unknown, or change shipment Inspection to UNK until all packs are screened."));
				}
				else if (Parent.JS_InspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code
					&& shipmentPacklines.Any(p => p.JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code))
				{
					Parent.JS_InspectionTypeCodeInfo.AddError(Res.GetString("54fd506e-b77b-4a63-9536-f91ab2cc4dce", "Shipment inspection states {0}, however some packlines are UNK. Either add inspection value to each packline, or change Shipment Inspection to UNK until all packs are screened.",
						Parent.JS_InspectionTypeCode));
				}
				else if (shipmentPacklines.All(p => !p.JL_InspectionTypeCode.IsEmpty && p.JL_InspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code))
				{
					var distinctInspectionTypes = shipmentPacklines.Select(p => p.JL_InspectionTypeCode).Distinct().Count();
					if (distinctInspectionTypes == 1 && Parent.JS_InspectionTypeCode != shipmentPacklines.First().JL_InspectionTypeCode)
					{
						Parent.JS_InspectionTypeCodeInfo.AddError(Res.GetString("ebb5552b-8606-4991-81a5-3fbc91c194f3", "Shipment inspection states {0}, however all packlines are screened by {1}. Change Shipment Inspection to match packline inspection type.",
							Parent.JS_InspectionTypeCode,
							shipmentPacklines.First().JL_InspectionTypeCode));
					}
					else if (distinctInspectionTypes > 1 && Parent.JS_InspectionTypeCode != BaseJobShipmentLookups.InspectionType_Screened)
					{
						Parent.JS_InspectionTypeCodeInfo.AddError(Res.GetString("ce3565f3-f42c-40b3-87c3-cd352c4d749e", "Shipment inspection states {0}, however packlines are screened using different methods. Change Shipment Inspection to SCR – Screened instead.",
							Parent.JS_InspectionTypeCode));
					}
				}
			}
		}

		void ValidateUnknownInspectionTypeCode(ZPropertyInfo info, bool hasChanges)
		{
			if (info.Value.ToString() == FreightDataRegistry.AviationSecurity_Unknown_Code && hasChanges && Parent.AviationSecurity.SupplyChainSecurityConfiguration.IsEnabled)
			{
				var firstAirLeg = Parent.DepartureConsol?.Transports.FirstTransportWithTransportMode(Constants.TransportModes.Air);
				if (firstAirLeg != null && !firstAirLeg.JW_ATD.IsEmpty && firstAirLeg.JW_ATD < ZDateTime.Now)
				{
					info.AddError(Res.GetString("a692d7a5-688f-48c8-b8e0-949b196d309b", "'Unknown' cannot be manually chosen as the first routing air leg’s ATD is in the past."));
				}
				else
				{
					var firstAirConsol = MovementLegComparer.FirstOrDefaultLegForTransportMode(Parent.Consols.Cast<ForwardingConsol>(), Core.Constants.TransportModes.Air);
					if (firstAirConsol != null && firstAirConsol.JK_MasterBillIssueDate.IsValid && firstAirConsol.JK_MasterBillIssueDate.Date < ZDate.Today)
					{
						info.AddError(Res.GetString("e7856c78-a4cb-4154-a6c6-cc1d83b6f785", "'Unknown' cannot be manually chosen as the departure Consol's MAWB issue date has passed."));
					}
				}
			}
		}

		void ValidateInspectionAndAdditionalInspectionCannotBeTheSame(ZPropertyInfo info)
		{
			if (Parent.AviationSecurity.IsHighRiskShipment
				&& Parent.JS_AdditionalInspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code
				&& Parent.JS_AdditionalInspectionTypeCode != BaseJobShipmentLookups.InspectionType_Screened
				&& Parent.JS_AdditionalInspectionTypeCode == Parent.JS_InspectionTypeCode)
			{
				info.AddError(Res.GetString("9eca8f23-4593-4078-8fa7-8918bafaf8f9", "{0} and {1} cannot be the same.",
					Parent.JS_InspectionTypeCodeInfo.HumanReadableName, Parent.JS_AdditionalInspectionTypeCodeInfo.HumanReadableName));
			}
		}

		#endregion

		#region JS_AdditionalInspectionTypeCode

		protected override void CheckJS_AdditionalInspectionTypeCode()
		{
			base.CheckJS_AdditionalInspectionTypeCode();

			if (Parent.AviationSecurity.IsHighRiskShipment)
			{
				if (Parent.JS_AdditionalInspectionTypeCodeHasChanges && !Parent.JS_AdditionalInspectionTypeCode.Equals(Parent.JS_AdditionalInspectionTypeCodeOriginalValue))
				{
					var error = Parent.AviationSecurity.SupplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(Parent);
					if (!error.IsEmpty)
					{
						Parent.JS_AdditionalInspectionTypeCodeInfo.AddError(error);
					}
				}

				if (Parent.JS_AdditionalInspectionTypeCodeHasChanges || !Parent.IsInDatabase)
				{
					ListValidation.ErrorIfInvalidCode(Parent.JS_AdditionalInspectionTypeCodeInfo, Parent.Lookups.AdditionalInspectionTypes);
				}

				if (Parent.AviationSecurity.IsAviationSecurityApplicableForTransportMode && Parent.AviationSecurity.SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(Parent))
				{
					MandatoryValidation.CheckEntered(Parent.JS_AdditionalInspectionTypeCodeInfo);

					ValidateInspectionAndAdditionalInspectionCannotBeTheSame(Parent.JS_AdditionalInspectionTypeCodeInfo);

					Parent.AviationSecurity.SupplyChainSecurityConfiguration.CheckJS_InspectionTypeCodeAdditionalValidation(Parent, Parent.JS_AdditionalInspectionTypeCodeInfo, Parent.JS_AdditionalInspectionTypeCodeHasChanges);

					if (!Parent.JS_InspectionTypeCodeInfo.HasErrors())
					{
						ValidateUnknownInspectionTypeCode(Parent.JS_AdditionalInspectionTypeCodeInfo, Parent.JS_AdditionalInspectionTypeCodeHasChanges);
						ValidateInspectionTypeAllowedForPassengerFlights(Parent.JS_AdditionalInspectionTypeCodeInfo);
						ValidateScreenedAdditionalInspectionTypeCode();
					}
				}

				if (!Parent.ApplyShipmentAdditionalInspectionType && Parent.OuterPackLines.Cast<ForwardingPackLine>().Where(p => !p.JL_AdditionalInspectionTypeCodeInfo.ReadOnly).Any(p => p.JL_AdditionalInspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code))
				{
					Parent.JS_AdditionalInspectionTypeCodeInfo.AddWarning(Res.GetString("b89a83b1-3b43-4073-a760-ac8096069e54", "Shipment > Basic Registration > Additional Inspection will be overridden once all high-risk packlines have Additional Inspection."));
				}
			}
		}

		void ValidateScreenedAdditionalInspectionTypeCode()
		{
			var shipmentHighRiskPacklines = Parent.OuterPackLines.Cast<ForwardingPackLine>().Where(p => !p.JL_AdditionalInspectionTypeCodeInfo.ReadOnly);
			if (Parent.JS_AdditionalInspectionTypeCode == BaseJobShipmentLookups.InspectionType_Screened
				&& shipmentHighRiskPacklines.Any(p => p.JL_AdditionalInspectionTypeCode.IsEmpty || p.JL_AdditionalInspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code))
			{
				Parent.JS_AdditionalInspectionTypeCodeInfo.AddError(Res.GetString("72cee938-fdfc-4f92-b845-260b8a5f79eb", "Additional Inspection method SCR – Screened means each high-risk packline of the shipment is screened. You have high-risk packlines with UNK status. Either ensure each high-risk packline has an additional inspection entered that is not UNK - Unknown, or change shipment Additional Inspection to UNK until all packs are screened."));
			}
			else if (Parent.JS_IsForwardRegistered && Parent.JS_AdditionalInspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code
				&& shipmentHighRiskPacklines.Any(p => p.JL_AdditionalInspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code))
			{
				Parent.JS_AdditionalInspectionTypeCodeInfo.AddWarning(Res.GetString("ba322580-ea86-4637-9ddf-c939b10766e7", "Shipment additional inspection states {0}, however some high-risk packlines are UNK. Either add additional inspection value to each high-risk packline, or change Shipment Inspection to UNK until all packs are screened.",
					Parent.JS_AdditionalInspectionTypeCode));
			}
			else if (shipmentHighRiskPacklines.All(p => !p.JL_AdditionalInspectionTypeCode.IsEmpty && p.JL_AdditionalInspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code))
			{
				var distinctAdditionalInspectionTypes = shipmentHighRiskPacklines.Select(p => p.JL_AdditionalInspectionTypeCode).Distinct().Count();
				if (distinctAdditionalInspectionTypes == 1 && Parent.JS_AdditionalInspectionTypeCode != shipmentHighRiskPacklines.First().JL_AdditionalInspectionTypeCode)
				{
					Parent.JS_AdditionalInspectionTypeCodeInfo.AddError(Res.GetString("99277eab-0db3-400b-a3f4-560618420c52", "Shipment additional inspection states {0}, however all high-risk packlines are screened by {1}. Change Shipment Additional Inspection to match high-risk packline additional inspection type.",
						Parent.JS_AdditionalInspectionTypeCode,
						shipmentHighRiskPacklines.First().JL_AdditionalInspectionTypeCode));
				}
				else if (Parent.JS_AdditionalInspectionTypeCode == BaseJobShipmentLookups.InspectionType_Screened && !shipmentHighRiskPacklines.Any())
				{
					Parent.JS_AdditionalInspectionTypeCodeInfo.AddError(Res.GetString("962f450f-f78e-486a-8c73-9001c74e529c", "SCR - Screened status cannot be used as there are no high-risk packlines."));
				}
			}
		}

		#endregion

		#region JS_IsHighRisk

		protected override void CheckJS_IsHighRisk()
		{
			if (Parent.JS_IsHighRiskHasChanges && !Parent.JS_IsHighRisk.Equals(Parent.JS_IsHighRiskOriginalValue))
			{
				var error = Parent.AviationSecurity.SupplyChainSecurityConfiguration.GetEditInspectionErrorForUncertifiedUser(Parent);
				if (!error.IsEmpty)
				{
					Parent.JS_IsHighRiskInfo.AddError(error);
				}
			}
			base.CheckJS_IsHighRisk();
			if (Parent.AviationSecurity.IsHighRiskShipment && !Parent.ApplyShipmentIsHighRisk && Parent.OuterPackLines.Where(x => x.JL_IsHighRisk == false).Any())
			{
				Parent.JS_IsHighRiskInfo.AddWarning(Res.GetString("82bfdd31-3991-445f-bbea-6b95874d9483", "Shipment has been flagged as ‘High Risk’ but packline is not."));
			}
			else if (!Parent.AviationSecurity.IsHighRiskShipment && !Parent.ApplyShipmentIsHighRisk && Parent.OuterPackLines.Where(x => x.JL_IsHighRisk == true).Any())
			{
				Parent.JS_IsHighRiskInfo.AddWarning(Res.GetString("49073486-E395-406C-83EF-85909E38C020", "Shipment has not been flagged as 'High Risk', but packline is."));
			}
		}

		#endregion

		#region Weight / Volume

		protected override void CheckJS_ActualWeight()
		{
			base.CheckJS_ActualWeight();

			AddNotificationForWeightOrVolumeChangedAfterConsolCutOffDate(Parent.JS_ActualWeightInfo, Parent.JS_ActualWeightModifiedDateUtc);
		}

		protected override void CheckJS_UnitOfWeight()
		{
			base.CheckJS_UnitOfWeight();

			AddNotificationForWeightOrVolumeChangedAfterConsolCutOffDate(Parent.JS_UnitOfWeightInfo, Parent.JS_UnitOfWeightModifiedDateUtc);
		}

		protected override void CheckJS_ActualVolume()
		{
			base.CheckJS_ActualVolume();

			AddNotificationForWeightOrVolumeChangedAfterConsolCutOffDate(Parent.JS_ActualVolumeInfo, Parent.JS_ActualVolumeModifiedDateUtc);
		}

		protected override void CheckJS_UnitOfVolume()
		{
			base.CheckJS_UnitOfVolume();

			AddNotificationForWeightOrVolumeChangedAfterConsolCutOffDate(Parent.JS_UnitOfVolumeInfo, Parent.JS_UnitOfVolumeModifiedDateUtc);
		}

		void AddNotificationForWeightOrVolumeChangedAfterConsolCutOffDate(ZPropertyInfo info, ZDateTime lastModificationDateUtc)
		{
			if (!lastModificationDateUtc.IsValid)
			{
				return;
			}

			if (!Parent.IsValidationSuspended
				&& Parent.IsInDatabase
				&& info.HasChanges
				&& Parent.Consols.Cast<ForwardingConsol>().Any(x => x.JK_ConsolCutOffDate.IsValid && x.JK_ConsolCutOffDate <= lastModificationDateUtc))
			{
				if (Env.Security.ConsolChangeWeightOrVolumeAfterCutOffDate.IsAllowed)
				{
					info.AddWarning(Res.GetString("5173bc46-f3da-44ac-b738-ed970a42803a", "You have modified shipment's weight or volume after the Consol Cut Off Date."));
				}
				else
				{
					info.AddError(Res.GetString("0db5d6cb-2d44-4f89-8857-b067f7f0aa95",
						"You cannot modify Shipment's weight or volume after the Consol Cut Off Date. Supervisor access is required to save the changes at this time.\r\n\r\n{0}",
						Env.Security.GetErrorMessageForNotAllowed(Env.Security.ConsolChangeWeightOrVolumeAfterCutOffDate)));
				}
			}
		}

		#endregion

		#region Validate JS_RL_NKOrigin JS_RL_NKDestination

		protected override void CheckJS_RL_NKOrigin()
		{
			base.CheckJS_RL_NKOrigin();

			if (ShouldRunAdditionalValidationOnOrigin)
			{
				ValidateJS_RL_NKDestination();
			}

			if (!Parent.IsValidationSuspended)
			{
				ValidateJS_InspectionTypeCode();

				if (!Parent.JS_RL_NKOriginInfo.HasErrors())
				{
					var warningForProhibitedRouting = Parent.AviationSecurity.DestinationSupplyChainSecurityConfiguration.GetWarningForProhibitedRouting(Parent);
					if (!warningForProhibitedRouting.IsEmpty)
					{
						Parent.JS_RL_NKOriginInfo.AddWarning(warningForProhibitedRouting);
					}
				}
			}
		}

		protected override void CheckJS_RL_NKDestination()
		{
			base.CheckJS_RL_NKDestination();

			if (ShouldRunAdditionalValidationOnDestination)
			{
				ValidateJS_RL_NKOrigin();
			}

			if (!Parent.IsValidationSuspended)
			{
				ValidateJS_InspectionTypeCode();
			}
		}

		#endregion

		protected override void CheckIsDomesticFreight()
		{
			base.CheckIsDomesticFreight();

			if (!Parent.IsValidationSuspended)
			{
				ValidateJS_InspectionTypeCode();
			}
		}

		#region ValidateCheckJS_A_RCV

		protected override void CheckJS_A_RCV()
		{
			base.CheckJS_A_RCV();

			if (Parent == null || !Parent.JS_IsForwardRegistered)
			{
				return;
			}

			RefUNLOCO unloco = null;
			var portCode = Parent.ExportReceivingDepot?.OA_RL_NKRelatedPortCode;

			if (!string.IsNullOrEmpty(portCode))
			{
				unloco = Parent.Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.Equal, portCode));
			}

			if (unloco == null)
			{
				var pickupAddress = Parent.ConsignorPickupAddress;
				unloco = ((ILocation)pickupAddress)?.UNLOCO ?? Parent.Origin;
			}

			if (unloco != null && Parent.JS_A_RCV > (unloco.TimeZoneSet?.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime()) ?? Env.Time.CurrentLocalDateTime))
			{
				Parent.JS_A_RCVInfo.AddError(Res.GetString("33f04d87-b832-4079-a07b-01de5ea9ba1f", "Interim Receipt Date cannot be in the future."));
			}
			else if (unloco == null && Parent.JS_A_RCV > (GlbStaff.CurrentUser.HomeBranch?.HomePort.TimeZoneSet?.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime()) ?? Env.Time.CurrentLocalDateTime))
			{
				Parent.JS_A_RCVInfo.AddWarning(Res.GetString("2257d986-2345-44e4-ad2d-a71c1d29c411", "The Interim Receipt Date is in the future."));
			}
		}

		#endregion

		protected override void CheckJS_ReleaseType()
		{
			base.CheckJS_ReleaseType();

			if (Parent.JS_TransportMode == Constants.TransportModes.Sea && Parent.JS_IsForwardRegistered)
			{
				MandatoryValidation.CheckEntered(Parent.JS_ReleaseTypeInfo);
			}

			if (Parent.IsInDatabase && Parent.JS_ReleaseTypeInfo.HasChanges && !Env.Security.MaintainShipmentEditReleaseType.IsAllowed)
			{
				string notification = Res.GetString("de1536eb-f267-4fa9-a30b-6bfe6176353f", "Security right '{0}' doesn't allow you to change {1} from {2}.",
					Env.Security.MaintainShipmentEditReleaseType.DisplayTextPathToSecurityRight,
					Parent.JS_ReleaseTypeInfo.Description, Parent.JS_ReleaseTypeInfo.OriginalValue);
				Parent.JS_ReleaseTypeInfo.AddError(notification);
			}
		}

		protected override void CheckJS_TransportMode()
		{
			base.CheckJS_TransportMode();
			ValidateJS_ReleaseType();
		}

		protected override void CheckJS_INCO()
		{
			base.CheckJS_INCO();

			if (Parent.IsExport() && FreightConfigurationRegistry.Instance.MandatoryIncoTerm.Value && Parent.JS_INCO.IsEmpty)
			{
				Parent.JS_INCOInfo.AddError(Res.GetString("9593d07f-cdce-4783-48d4-829fe98ded59", "Please enter a value. This field has been defined as mandatory and can be changed in the system registry at Freight -> Shipment -> Incoterm Mandatory."));
			}
		}

		protected override void CheckJS_OH_ExportBroker()
		{
			base.CheckJS_OH_ExportBroker();
			if (!Parent.JS_OH_ExportBroker.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(Parent.JS_OH_ExportBrokerInfo);
				if (!Parent.JS_OH_ImportBroker.IsEmpty)
				{
					CompareValidation.CheckNotEqual(Parent.JS_OH_ExportBrokerInfo, Parent.JS_OH_ImportBrokerInfo);
				}
			}
		}

		protected override void CheckJS_CompanyTariffLevelOverride()
		{
			if (!Parent.JS_CompanyTariffLevelOverrideInfo.ReadOnly) // We don't need to validate JS_CompanyTariffLevelOverride when it is disabled.
			{
				base.CheckJS_CompanyTariffLevelOverride();
				ListValidation.ErrorIfInvalidCode(Parent.JS_CompanyTariffLevelOverrideInfo, Parent.Lookups.CompanyTariffLevelOverrideList);
			}
		}

		protected override void CheckJS_OH_ImportBroker()
		{
			base.CheckJS_OH_ImportBroker();
			if (!Parent.JS_OH_ImportBroker.IsEmpty)
			{
				ListValidation.ErrorIfInvalidPK(Parent.JS_OH_ImportBrokerInfo);
				if (!Parent.JS_OH_ExportBroker.IsEmpty)
				{
					CompareValidation.CheckNotEqual(Parent.JS_OH_ImportBrokerInfo, Parent.JS_OH_ExportBrokerInfo);
				}
			}
			else if (Parent.JS_IsForwardRegistered && Parent.IsImport() && FreightConfigurationRegistry.Instance.MandatoryImportBroker.Value)
			{
				Parent.JS_OH_ImportBrokerInfo.AddError(Res.GetString("04f4076c-b0e8-4cab-9d7a-91128070b496", "Please enter a value. This field has been defined as mandatory and can be changed in the system registry at Freight -> Shipment -> Import Broker Mandatory."));
			}
		}

		protected override void CheckJS_F3_NKTotalCountPackType()
		{
			base.CheckJS_F3_NKTotalCountPackType();
			ListValidation.ErrorIfInvalidCode(Parent.JS_F3_NKTotalCountPackTypeInfo, Parent.Lookups.JS_PackType_List);
			MandatoryValidation.CheckUnitEntered(Parent.JS_F3_NKTotalCountPackTypeInfo, Parent.JS_TotalPackageCountInfo);
		}

		#region JS_ElectronicBillOfLadingType

		protected override void CheckJS_ElectronicBillOfLadingType()
		{
			base.CheckJS_ElectronicBillOfLadingType();

			if (Parent.EnabledElectronicBOL && Parent.JS_ElectronicBillOfLadingType.IsEmpty)
			{
				Parent.JS_ElectronicBillOfLadingTypeInfo.AddMessageError(Res.GetString("fde1e0a9-2c35-4a6d-8dba-e1ce7a53f63d", "Please enter Bill Type."));
			}
		}

		#endregion

		#region JS_ElectronicBillOfLadingTerms

		protected override void CheckJS_ElectronicBillOfLadingTerms()
		{
			base.CheckJS_ElectronicBillOfLadingTerms();

			if (Parent.EnabledElectronicBOL && Parent.JS_ElectronicBillOfLadingTerms.IsEmpty)
			{
				Parent.JS_ElectronicBillOfLadingTermsInfo.AddMessageError(Res.GetString("062eb9bf-0db5-4b82-ba7d-e808dd64eddc", "Please enter Bill Terms."));
			}
		}

		#endregion

		#region JS_ShipmentType

		protected override void CheckJS_ShipmentType()
		{
			base.CheckJS_ShipmentType();

			CheckHighVolumeLowValueShipmentType();
			CheckMasterShipmentType();

			if (!Parent.IsAssemblyMaster
				&& Parent.CoLoadShipments.Cast<ForwardingShipment>().Any(s => !s.IsStandardHouse && !s.IsHighVolumeLowValueLegacy & !s.IsHighVolumeLowValue))
			{
				Parent.JS_ShipmentTypeInfo.AddError(Res.GetString("83b4fe5c-9099-4a2c-94c6-a384039672f4",
					"Only {0} shipments can have sub-shipments that are neither {1} nor {2} shipments. Either update the sub-shipments or change this shipment type to {0}.",
					Constants.ShipmentTypeDescriptions.AssemblyMaster,
					Constants.ShipmentTypeDescriptions.StandardHouse,
					Constants.ShipmentTypeDescriptions.HighVolumeLowValueLegacy));
			}

			var isSentConsolidationAdviceShipmentNumbers = Parent.CoLoadShipments.OfType<ForwardingShipment>().Where(x => x.IsStandardHouse && x.IsSentConsolidationAdvice).Select(x => x.JS_UniqueConsignRef).ToArray();
			if (Parent.JS_ShipmentType == Core.Constants.ShipmentTypes.CoLoadMaster && isSentConsolidationAdviceShipmentNumbers.Length > 0)
			{
				Parent.JS_ShipmentTypeInfo.AddWarning(Res.GetString("7c840d04-2ac4-46e0-a173-50f1f80cf0d3", "A Consolidation Advice was already sent from the Shipment(s) {0}. Please arrange for the cancellation of Consolidation with the Forwarder.", string.Join(", ", isSentConsolidationAdviceShipmentNumbers)));
			}
		}

		protected override void CheckJS_PackingMode()
		{
			base.CheckJS_PackingMode();

			if (Parent.IsBuyersConsolLead && Parent.JS_PackingMode != Constants.ContainerModes.BuyersConsol)
			{
				Parent.JS_PackingModeInfo.AddError(Res.GetString("eff46ad0-efeb-4461-815b-ace44fa64bdf", "Container mode must be set to {0} to mark this Shipment as Buyer's Consol Lead.", Constants.ContainerModes.BuyersConsol));
			}

			if (Parent.IsShippersConsolLead && Parent.JS_PackingMode != Constants.ContainerModes.ShippersConsol)
			{
				Parent.JS_PackingModeInfo.AddError(Res.GetString("cf473a94-da0f-4387-9d70-87dc9218163c", "Container mode must be set to {0} to mark this Shipment as Shipper's Consol Lead.", Constants.ContainerModes.ShippersConsol));
			}

			if (Parent.JS_PackingMode == Constants.ContainerModes.BuyersConsol)
			{
				if (!IsOnBCNConsol && Parent.Consols.Count != 0)
				{
					Parent.JS_PackingModeInfo.AddWarning(Res.GetString("c9821425-d39b-445c-97dd-c73058a2e67b", "A Shipment that is a Buyers Consol must belong to a consol of type BCN or OTH"));
				}
			}

			if (Parent.JS_PackingMode == Constants.ContainerModes.ShippersConsol)
			{
				if (!IsOnSCNConsol && Parent.Consols.Count != 0)
				{
					Parent.JS_PackingModeInfo.AddWarning(Res.GetString("7121d491-6c1a-4975-b366-b22415396193", "A Shipment that is a Shippers Consol must belong to a consol type of SCN"));
				}
			}
		}

		void CheckMasterShipmentType()
		{
			if (Parent.CoLoadMasterShipment != null
				&& !Parent.CoLoadMasterShipment.IsAssemblyMaster
				&& !Parent.IsStandardHouse
				&& !Parent.IsHighVolumeLowValueLegacy
				&& !Parent.IsHighVolumeLowValue)
			{
				Parent.JS_ShipmentTypeInfo.AddError(Res.GetString("72aaac89-6377-44b9-8755-49af11e985cb",
					"Only {0} shipments can have sub-shipments that are neither {1} nor {2} shipments.",
					Constants.ShipmentTypeDescriptions.AssemblyMaster,
					Constants.ShipmentTypeDescriptions.StandardHouse,
					Constants.ShipmentTypeDescriptions.HighVolumeLowValueLegacy));
			}
		}

		void CheckHighVolumeLowValueShipmentType()
		{
			if (Parent.JS_ShipmentType != Constants.ShipmentTypes.HighVolumeLowValue && Parent.HasHVLVItems)
			{
				Parent.JS_ShipmentTypeInfo.AddError(ErrorMessageHVLVItemsAttachedMustBeHVLShipmentType);
			}
		}

		internal static string ErrorMessageHVLVItemsAttachedMustBeHVLShipmentType
		{
			get { return Res.GetString("3abed39c-5825-48dd-b311-75185b0dee42", "HVLV Items can only be attached to 'HVL' shipment type."); }
		}

		internal static string ErrorMessageHLSShipmentTypeMustHaveEManifest
		{
			get { return Res.GetString("be8b582f-a7ea-434d-8da0-8431557ae1de", "'HLS' shipment type should have eManifest attached."); }
		}

		internal static string ErrorMessageEManifestAttachedMustBeHLSShipmentType
		{
			get { return Res.GetString("be8b582f-a7ea-434d-8da0-8431557ae1dd", "Can attach eManifest only to 'HLS' shipment type."); }
		}

		internal static string ErrorMessageMustHaveHVLVEnabledToUseHLSShipmentType
		{
			get { return Res.GetString("03159c25-7f6b-4ba5-89e8-544610495e21", "You must have a 'HVLV Clearance' enabled to use the 'HLS' Shipment Type."); }
		}

		#endregion

		#region JS_ShipmentStatus

		protected override void CheckJS_ShipmentStatus()
		{
			if (!Parent.IsSea && Parent.JS_IsForwardRegistered)
			{
				return;
			}

			if (Parent.JS_ShipmentStatus == ShipmentStatusList.Codes.WebBooking && Parent.IsInDatabase && !Parent.JS_ShipmentStatusInfo.HasChanges)
			{
				return;
			}

			base.CheckJS_ShipmentStatus();

			MandatoryValidation.CheckEntered(Parent.JS_ShipmentStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JS_ShipmentStatusInfo);
		}

		#endregion

		#region CheckJS_JS_ColoadMasterShipment

		protected override void CheckJS_JS_ColoadMasterShipment()
		{
			CheckJS_JS_ColoadMasterShipment_ACAS();

			var master = Parent.CoLoadMasterShipment;

			if (master != null && master.IsCoLoadMaster && Parent.IsStandardHouse && Parent.IsSentConsolidationAdvice)
			{
				Parent.JS_JS_ColoadMasterShipmentInfo.AddWarning(Res.GetString("ce93e457-df53-4aab-8ec9-d706de4bf21e", "A Consolidation Advice was already sent from the Shipment(s) {0}. Please arrange for the cancellation of Consolidation with the Forwarder.", Parent.JS_UniqueConsignRef));
			}

			base.CheckJS_JS_ColoadMasterShipment();
		}

		void CheckJS_JS_ColoadMasterShipment_ACAS()
		{
			if (Parent.JS_JS_ColoadMasterShipment.IsValid
				&& Parent.JS_TransportMode == Constants.TransportModes.Air
				&& Parent.JS_RL_NKDestination.SubstringSafe(0, 2) == Constants.CountryCodes.Brazil
				&& Parent.TransportsIncludingRelated.Cast<Transport>().Any(t => t.JW_TransportMode == Constants.TransportModes.Air && t.JW_RL_NKDiscPort.SubstringSafe(0, 2) == Constants.CountryCodes.Brazil)
				&& Parent.CoLoadMasterShipment != null
				&& Parent.HasSentAdvancedCargoReport)
			{
				if (Parent.CoLoadMasterShipment.JS_ShipmentType == Constants.ShipmentTypes.CoLoadMaster)
				{
					Parent.JS_JS_ColoadMasterShipmentInfo.AddError(Res.GetString("9e8daf05-58f7-4095-a379-e342ad7f2ce0", "Advanced Air Cargo Reporting has been sent for this Shipment. In co-load scenario, Advance Air Cargo Reporting should be done from the Co-Load Master (CLD) shipment. Please withdraw the message sent from this shipment prior to attaching it to CLD master."));
				}
				if (Parent.CoLoadMasterShipment.JS_ShipmentType == Constants.ShipmentTypes.AssemblyMaster && Parent.CoLoadMasterShipment.HasSentAdvancedCargoReport)
				{
					Parent.JS_JS_ColoadMasterShipmentInfo.AddError(Res.GetString("586F6FFF-BB1C-496A-981A-5CC979E781EB", "Advanced Air Cargo Reporting has been sent from this shipment. In Assembly master scenario, Advanced Air Cargo Reporting can be done from master or subs not from both. Please withdraw the message sent from this shipment prior to attaching it to Assembly master."));
				}
			}
		}

		protected override string CheckConsolFromMasterShipment(CommonShipment master, CommonConsol consol)
		{
			Argument.NotNull(master, nameof(master));
			Argument.NotNull(consol, nameof(consol));

			var errorMessage = string.Empty;
			var forwardingConsol = consol as ForwardingConsol;

			if (forwardingConsol != null)
			{
				if (!Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster.IsAllowed && forwardingConsol.FinalMAWBPrintedDate.IsValid)
				{
					errorMessage = Res.GetString(
								"ea627efb-5566-47df-9942-4337d4368ccd",
								"Consol {0} cannot be attached to the Shipment {1} from its proposed Master/Lead {2} as the Master Bill for {0} has already been printed on {3}.{5}{4}",
								consol.JK_UniqueConsignRef,
								Parent.JS_UniqueConsignRef,
								master.JS_UniqueConsignRef,
								forwardingConsol.FinalMAWBPrintedDate,
								Env.Security.GetErrorMessageForNotAllowed(Env.Security.AllowAttachDetachShipmentsAfterPrintingFinalMaster),
								System.Environment.NewLine);
				}
				else if (Parent.AviationSecurity.SupplyChainSecurityConfiguration.IsEnabled
					&& Parent.JS_InspectionTypeCode != FreightDataRegistry.AviationSecurity_Unknown_Code
					&& !Parent.AviationSecurity.IsAllowedOnPassengerFlights()
					&& Parent.AviationSecurity.SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(forwardingConsol)
					&& !forwardingConsol.AllowsShipmentsWithApprovedForCargoOnlyInspectionType)
				{
					errorMessage = Res.GetString("9e9a0be3-8b54-40b6-8903-005a66d4414b", "For a voyage that is not Cargo Only, all Shipments must be Aviation Security Approved or Exempt.");
				}
			}

			return errorMessage;
		}

		#endregion

		#region Validate JS_Calc_EstimatedExportClearanceDate

		public void ValidateJS_Calc_EstimatedExportClearanceDate()
		{
			ValidateCalculatedProperty(Parent.JS_Calc_EstimatedExportClearanceDateInfo);
		}

		protected void CheckJS_Calc_EstimatedExportClearanceDate()
		{
			TypeValidation.CheckValidZDateTimeWithoutRange(Parent.JS_Calc_EstimatedExportClearanceDateInfo);
			new DateRangeValidation().ValidateDateValueHasChanged(Parent.JS_Calc_EstimatedExportClearanceDateInfo);

			if (!IsNonForwardingBooking && Parent.IsExport() && FreightDataRegistry.Instance.EstimatedExportCustomsClearanceDateMandatory.Value && Parent.JS_Calc_EstimatedExportClearanceDate.IsEmpty)
			{
				var errorMessage = Res.GetString(
					"b7da9378-04da-4c59-8a0a-cd9e175effd8",
					"date. This field has been defined as mandatory. This setting is found in the system registry at {0}/{1}",
					FreightDataRegistry.Instance.EstimatedExportCustomsClearanceDateMandatory.Category,
					FreightDataRegistry.Instance.EstimatedExportCustomsClearanceDateMandatory.Caption);

				MandatoryValidation.CheckEntered(Parent.JS_Calc_EstimatedExportClearanceDateInfo, errorMessage);
			}
		}

		#endregion

		#region Validate Total CO2e

		public void ValidateTotalCO2eForBinding()
		{
			ValidateCalculatedProperty(Parent.TotalCO2eForBindingInfo);
		}

		protected void CheckTotalCO2eForBinding()
		{
			Parent.CheckCO2eStatus(Parent.TotalCO2eForBindingInfo);
		}

		#endregion

		#region Validate Total CO2e Sorting

		public void ValidateTotalCO2eForSorting()
		{
			ValidateCalculatedProperty(Parent.TotalCO2eForSortingInfo);
		}

		protected void CheckTotalCO2eForSorting()
		{
			Parent.CheckCO2eStatus(Parent.TotalCO2eForSortingInfo);
		}

		#endregion

		#region JS_OA_BookedShippingLineAddress

		protected override void CheckJS_OA_BookedShippingLineAddress()
		{
			base.CheckJS_OA_BookedShippingLineAddress();
			if (Parent.JS_IsBooking && Parent.IsSea && !Parent.JS_OA_BookedShippingLineAddressInfo.HasErrors())
			{
				var sailing = Parent.Factory.Load<JobSailing>(Parent.JS_JX);
				if (sailing != null
					&& sailing.Voyage != null
					&& !sailing.Voyage.JV_OH_Line.IsEmpty
					&& Parent.BookedShippingLinePK != sailing.Voyage.JV_OH_Line)
				{
					Parent.JS_OA_BookedShippingLineAddressInfo.AddWarning(Res.GetString("54698c55-9a43-49a2-8a89-4d3dacd9ccc8", "Attached Sailing has different Carrier."));
				}
			}
		}

		#endregion

		protected override bool IsAgreedCharge
		{
			get
			{
				return Parent.IsAir
					? IsAgreedChargeCodesForAir.Contains(Parent.JS_HBLAWBChargesDisplay)
					: base.IsAgreedCharge;
			}
		}

		#region JS_PaymentTermAutoratingOverride
		protected override void CheckJS_PaymentTermAutoratingOverride()
		{
			base.CheckJS_PaymentTermAutoratingOverride();
			if (!Parent.JS_PaymentTermAutoratingOverride.IsEmpty && !Parent.Lookups.JS_PaymentTermAutoratingOverride_List.ContainsCode(Parent.JS_PaymentTermAutoratingOverride))
			{
				Parent.JS_PaymentTermAutoratingOverrideInfo.AddError(Res.GetString("a695974a-6254-4598-82bf-e94fa1894c58", "Please enter a valid payment term from the available list."));
			}

			ValidateJS_PaymentTermAutoratingOverride();
		}
		#endregion

		#region JS_DeliveryDueDate

		protected override void CheckJS_DeliveryDueDate()
		{
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsTransportModeActive(Parent?.JS_TransportMode ?? ZString.Empty) && Parent.JS_DeliveryDueDate.IsEmpty)
			{
				if (!Parent.ReasonForNotBeingAbleToCalculateDeliveryDueDate.IsEmpty)
				{
					Parent.JS_DeliveryDueDateInfo.AddWarning(Parent.ReasonForNotBeingAbleToCalculateDeliveryDueDate);
				}
				else
				{
					if (!DeliveryDueDateCalculator.SupportedDeliveryModes.Contains(Parent.JS_HBLContainerPackModeOverride.ToString()))
					{
						Parent.JS_DeliveryDueDateInfo.AddWarning(Res.GetString("25024868-3c8a-42e4-ba44-9ca84d644c45", "Only HBL Delivery Modes DOOR/DOOR, DOOR/CFS, CFS/DOOR, CFS/CFS, ARPT/ARPT, DOOR/ARPT, CFS/ARPT, ARPT/DOOR or ARPT/CFS are used in DDD (Delivery Due Date) calculation."));
					}
					else
					{
						AddWarningIfMandatoryFieldForDDDCalculationNotEntered(Parent.JS_RS_NKServiceLevelInfo, Res.GetString("ffb9f8d6-f10f-4a21-b6c8-6dde2e706326", "Service Level"));

						var pickupCFSValidity = Parent.PickupCFSValidity;
						if (!pickupCFSValidity.IsValid)
						{
							Parent.JS_DeliveryDueDateInfo.AddWarning(GetMandatoryFieldForDDDCalculationNotEnteredWarning(pickupCFSValidity.InvalidFieldName));
						}

						var deliveryCFSValidity = Parent.DeliveryCFSValidity;
						if (!deliveryCFSValidity.IsValid)
						{
							Parent.JS_DeliveryDueDateInfo.AddWarning(GetMandatoryFieldForDDDCalculationNotEnteredWarning(deliveryCFSValidity.InvalidFieldName));
						}

						var readyDateValidity = Parent.ReadyDateValidity;
						if (!readyDateValidity.IsValid)
						{
							Parent.JS_DeliveryDueDateInfo.AddWarning(GetMandatoryFieldForDDDCalculationNotEnteredWarning(readyDateValidity.InvalidFieldName));
						}

						if (Parent.ConsignorPickupAddress.IsEmpty && Parent.IsHBLContainerPackModeDOOR_X)
						{
							Parent.JS_DeliveryDueDateInfo.AddWarning(GetMandatoryFieldForDDDCalculationNotEnteredWarning(Res.GetString("282df07f-1037-4ae7-b42d-56fd6ee38852", "Pickup From address")));
						}

						if (Parent.ConsigneeDeliveryAddress.IsEmpty && Parent.IsHBLContainerPackModeX_DOOR)
						{
							Parent.JS_DeliveryDueDateInfo.AddWarning(GetMandatoryFieldForDDDCalculationNotEnteredWarning(Res.GetString("0cd16e95-f21e-4567-af1e-178460e1fa61", "Deliver To address")));
						}
					}
				}
			}
		}

		void AddWarningIfMandatoryFieldForDDDCalculationNotEntered(ZPropertyInfo property, string propertyDescription)
		{
			if (property.Value.IsEmpty)
			{
				Parent.JS_DeliveryDueDateInfo.AddWarning(GetMandatoryFieldForDDDCalculationNotEnteredWarning(propertyDescription));
			}
		}

		string GetMandatoryFieldForDDDCalculationNotEnteredWarning(string fieldName)
		{
			return Res.GetString(
				"e76d9f0a-17b4-4425-8742-58a52f4aa110",
				"{0} is mandatory for DDD (Delivery Due Date) calculation.",
				fieldName);
		}

		#endregion

		protected override void CheckJS_OA_ImportReleaseDepot()
		{
			var errorMessage = ZString.Empty;
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsTransportModeActive(Parent.JS_TransportMode))
			{
				var deliveryAddress = Parent.ConsigneeDeliveryAddress?.E2_AddressOverride == true ? Parent.ConsigneeDeliveryAddress : (IDocAddress)Parent.ConsigneeDeliveryAddress?.Address;
				var cfsDeliveryAddress = Parent.JS_OA_ImportReleaseDepot_ZAddress?.OrgAddress as OrgAddress;

				errorMessage = CheckIfTransportZoneMatch(deliveryAddress, cfsDeliveryAddress);
			}

			if (!errorMessage.IsEmpty)
			{
				Parent.JS_OA_ImportReleaseDepotInfo.AddWarning(errorMessage);
			}
		}

		protected override void CheckJS_OA_ExportReceivingDepot()
		{
			var errorMessage = ZString.Empty;
			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsTransportModeActive(Parent.JS_TransportMode))
			{
				var pickupAddress = Parent.ConsignorPickupAddress?.E2_AddressOverride == true ? Parent.ConsignorPickupAddress : (IDocAddress)Parent.ConsignorPickupAddress?.Address;
				var cfsPickupAddress = Parent.JS_OA_ExportReceivingDepot_ZAddress?.OrgAddress as OrgAddress;

				errorMessage = CheckIfTransportZoneMatch(pickupAddress, cfsPickupAddress);
			}

			if (!errorMessage.IsEmpty)
			{
				Parent.JS_OA_ExportReceivingDepotInfo.AddWarning(errorMessage);
			}
		}

		ZString CheckIfTransportZoneMatch(IDocAddress nonCFSAddress, OrgAddress cfsAddress)
		{
			if (nonCFSAddress != null && cfsAddress != null)
			{
				var matchedZoneItem = DeliveryDueDateCalculationHelper.GetZoneItem(nonCFSAddress, cfsAddress, Parent.Factory);
				if (matchedZoneItem == null)
				{
					return Res.GetString("aa6f4c64-7f15-4be1-afa6-c49797ede440",
						"Pickup or Delivery CFS is not a Zone Owner. Zone Owner is configured in the Transport Zone Set applicable to the pickup/delivery postcode.");
				}
			}
			return ZString.Empty;
		}

		#region ValidateHolderPK

		public void ValidateHolderPK(JobDocAddressValidation validation)
		{
			if (validation.Parent.OrganisationPK.IsEmpty)
			{
				validation.Parent.OrganisationPKInfo.AddMessageError(Res.GetString("562bb9dc-3178-4a02-8bbd-a78c92b7b41b", "First Holder cannot be blank. Please select a First Holder party from the organization lookup."));
			}
			else
			{
				if (validation.Parent.Organisation == null)
				{
					validation.Parent.OrganisationPKInfo.AddMessageError(ValidateTriMessage);
				}
				else
				{
					if (TRIRecordHelper.GetTRIRecord(validation.Parent.Organisation).IsEmpty)
					{
						validation.Parent.OrganisationPKInfo.AddMessageError(ValidateTriMessage);
					}

					if (validation.Parent.Organisation.OH_IsConsignor && ((!validation.Parent.Organisation.MiscServ?.OM_FWRequiresElectronicBOLForDirectConsol) ?? true))
					{
						validation.Parent.OrganisationPKInfo.AddMessageError(Res.GetString("11edda1a-e5d2-404b-970f-b8e81c56d690", "First Holder must have 'Requires electronic Bill of Lading' flagged against Organization > Consignor > Details > Exporter/Consignor Defaults."));
					}

					if (validation.Parent.Organisation.OH_IsForwarder && ((!validation.Parent.Organisation.MiscServ?.OM_FWRequiresElectronicBOLForNonDirectConsol) ?? true))
					{
						validation.Parent.OrganisationPKInfo.AddMessageError(Res.GetString("b41191fd-2d0c-4c59-b6b3-bfa3bad3a401", "First Holder must have 'Requires electronic Bill of Lading' flagged against Organization > FWD/Agent > Details > Forwarder Details."));
					}
				}
			}
		}

		#endregion

		#region ValidateTri

		public string ValidateTriMessage => Res.GetString("0dd825fe-2276-4e6a-84ac-723d7002dcd8", "An Organization selected from here must have a valid Bolero Entity Identifier (TRI) recorded.");

		#endregion

		#region ValidateShipperPK

		public void ValidateShipperPK(JobDocAddressValidation validation)
		{
			if (!validation.Parent.E2_AddressOverride && validation.Parent.OrganisationPK.IsEmpty)
			{
				validation.Parent.OrganisationPKInfo.AddMessageError(Res.GetString("3165F732-9602-4A74-B6C1-7A8EED1340B9", "Shipper cannot be blank. Please enter a Shipper in Basic Registration>Consignor"));
			}
			else if (TRIRecordHelper.GetTRIRecord(validation.Parent.Organisation).IsEmpty)
			{
				validation.Parent.OrganisationPKInfo.AddMessageError(Res.GetString("Enterprise.Freight|2e38aec7-2dc4-402f-a547-9fd77e19b620", $"Shipper must have a valid Bolero Entity Identifier (TRI){System.Environment.NewLine}to proceed with publishing an Electronic Bill of Lading."));
			}
		}

		#endregion

		#region ValidateSurrenderPartyPK

		public void ValidateSurrenderPartyPK(JobDocAddressValidation validation)
		{
			if (validation.Parent.OrganisationPK.IsEmpty)
			{
				return;
			}

			if (validation.Parent.Organisation == null
				|| TRIRecordHelper.GetTRIRecord(validation.Parent.Organisation).IsEmpty)
			{
				validation.Parent.OrganisationPKInfo.AddMessageError(ValidateTriMessage);
			}
		}

		#endregion

		#region ValidateElectronicBillOfLadingConsigneePK

		public void ValidateElectronicBillOfLadingConsigneePK(JobDocAddressValidation validation)
		{
			if (Parent.JS_ElectronicBillOfLadingType == Constants.BillOfLadingBillType.Codes.Straight)
			{
				if (validation.Parent.OrganisationPK.IsEmpty)
				{
					validation.Parent.OrganisationPKInfo.AddMessageError(Res.GetString("cf68182e-1756-4690-9a70-cfc601088c47", "Consignee cannot be blank. Please enter a Consignee in Basic Registration>Consignee"));
				}
				else if (TRIRecordHelper.GetTRIRecord(validation.Parent.Organisation).IsEmpty)
				{
					validation.Parent.OrganisationPKInfo.AddMessageError(Res.GetString("Enterprise.Freight|3c192bf0-0151-498a-baf0-2c507e0d3ef2", $"The consignee must have a valid Bolero Entity Identifier (TRI){System.Environment.NewLine}to proceed with publishing a 'Straight Bill'."));
				}
			}
		}

		#endregion

		#region ValidateElectronicBillOfLadingToOrderPK

		public void ValidateElectronicBillOfLadingToOrderPK(JobDocAddressValidation validation)
		{
			if (Parent.JS_ElectronicBillOfLadingType == Constants.BillOfLadingBillType.Codes.ToOrder)
			{
				if (validation.Parent.OrganisationPK.IsEmpty)
				{
					validation.Parent.OrganisationPKInfo.AddMessageError(Res.GetString("21fd9422-4359-473b-9e1d-fb9b52f7c245", "An organization must be selected when the Bill Type is set to 'To Order.' If the 'To Order' party is unknown, you may opt for the Bill Type 'BLE - Blank Endorse'."));
				}
				else if (TRIRecordHelper.GetTRIRecord(validation.Parent.Organisation).IsEmpty)
				{
					validation.Parent.OrganisationPKInfo.AddMessageError(Res.GetString("Enterprise.Freight|c055fbbb-d5d3-4a9b-b4ae-64335fce2825", $"The 'To Order' party must have a valid Bolero Entity Identifier (TRI){System.Environment.NewLine}to proceed with publishing a 'To Order Bill'."));
				}
			}
		}

		#endregion

		#region ValidateAutoratingDate

		/// <summary>
		///		Validate calculated field RevenueAutoratingDate on a shipment
		/// </summary>
		public void ValidateAutoratingDate()
		{
			ValidateCalculatedProperty(Parent.RevenueAutoratingDateInfo);
		}

		// Note: CheckXXX method is called automatically by validation framework when ValidateXXX is called.
		protected void CheckAutoratingDate()
		{
			TypeValidation.CheckValidSmallDateTime(Parent.RevenueAutoratingDateInfo);
			TypeValidation.CheckValidZDateTimeRange(Parent.RevenueAutoratingDateInfo);
		}

		#endregion

		protected override bool EnforceUniqueHouseBills =>
				Parent.JS_IsForwardRegistered && FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.Value && Parent.IsAir &&
					(
					!Parent.IsInDatabase
					|| Parent.JS_HouseBillInfo.HasChanges
					|| Parent.JS_SystemCreateTimeUtc >= FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.SystemLastEditTimeUtc
					|| (Parent.JS_IsBooking && Parent.JS_IsForwardRegisteredInfo.HasChanges)
					);

		protected override ZQuery GetHouseBillDuplicateCheckAdditionalConditions()
		{
			var query = new ZQuery(JobShipmentSchema.JS_IsForwardRegistered, true);
			if (FreightDataRegistry.Instance.EnforceUniqueHAWBNumbers.Value && Parent.JS_TransportMode == Constants.TransportModes.Air)
			{
				query.AddToFilter(JobShipmentSchema.JS_TransportMode, Parent.JS_TransportMode);
			}

			return query;
		}

		HashSet<ZString> IsAgreedChargeCodesForAir => isAgreedChargeCodesForAir ?? (isAgreedChargeCodesForAir = new HashSet<ZString>()
		{
			ChargesApplyHelper.ChargesApplyConstants.ALL,
			ChargesApplyHelper.ChargesApplyConstants.ANO,
			ChargesApplyHelper.ChargesApplyConstants.APP,
			ChargesApplyHelper.ChargesApplyConstants.CAL,
			ChargesApplyHelper.ChargesApplyConstants.CNO,
			ChargesApplyHelper.ChargesApplyConstants.CPD,
			ChargesApplyHelper.ChargesApplyConstants.NAL,
			ChargesApplyHelper.ChargesApplyConstants.NON,
			ChargesApplyHelper.ChargesApplyConstants.NPP
		});

		HashSet<ZString> isAgreedChargeCodesForAir;
	}
}
