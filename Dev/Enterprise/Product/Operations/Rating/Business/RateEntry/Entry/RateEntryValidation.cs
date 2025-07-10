using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	public class RateEntryValidation : AutoRateEntryValidation
	{
		public RateEntryValidation(AutoRateEntry parent)
			: base(parent)
		{
		}

		new RateEntry Parent
		{
			get { return (RateEntry)base.Parent; }
		}

		#region TI_OriginLRC

		protected override void CheckTI_OriginLRC()
		{
			base.CheckTI_OriginLRC();
			var location = Parent.Origin();
			if (!Parent.IsWHS() && !Parent.IsTRW() && !Parent.IsTWU())
			{
				if (!Parent.TI_OriginLRC.IsEmpty && location == null)
				{
					ListValidation.ErrorIfInvalidCode(Parent.TI_OriginLRCInfo, Parent.Lookups.Locations);
				}
				else
				{
					OriginValidator.CheckRateEntryLocations(location);
				}

				OriginValidator.ValidateInternationalZone();
				OriginValidator.ValidateIATACityCode(location);
			}
		}

		LocationValidator fOriginValidator;
		LocationValidator OriginValidator
		{
			get { return fOriginValidator ?? (fOriginValidator = new LocationValidator(Parent, Parent.TI_OriginLRCInfo)); }
		}

		#endregion

		#region TI_TZ_OriginZone

		protected override void CheckTI_TZ_OriginZone()
		{
			base.CheckTI_TZ_OriginZone();
			ValidateTI_OriginLRC();
			ValidateTI_DestinationLRC();

			var zoneSet = Parent.OriginZone?.TransportProvider;
			if (zoneSet != null)
			{
				CheckValidTransportProvider(zoneSet, Parent.TI_TZ_OriginZoneInfo);
			}
		}

		#endregion

		#region TI_YardUnitType

		protected override void CheckTI_YardUnitType()
		{
			base.CheckTI_YardUnitType();
			ListValidation.ErrorIfInvalidCode(Parent.TI_YardUnitTypeInfo, Parent.Lookups.YardUnitTypes);
		}

		#endregion

		#region TI_YardUnitLoad

		protected override void CheckTI_YardUnitLoad()
		{
			base.CheckTI_YardUnitLoad();
			ListValidation.ErrorIfInvalidCode(Parent.TI_YardUnitLoadInfo, Parent.Lookups.YardUnitLoads);
		}

		#endregion

		#region TI_ContainerUnitSection

		protected override void CheckTI_ContainerUnitSection()
		{
			base.CheckTI_YardUnitType();
			ListValidation.ErrorIfInvalidCode(Parent.TI_ContainerUnitSectionInfo, Parent.Lookups.ContainerUnitSections);
		}

		#endregion

		#region TI_TZ_DestinationZone

		protected override void CheckTI_TZ_DestinationZone()
		{
			base.CheckTI_TZ_DestinationZone();
			ValidateTI_OriginLRC();
			ValidateTI_DestinationLRC();

			var zoneSet = Parent.DestinationZone?.TransportProvider;
			if (zoneSet != null)
			{
				CheckValidTransportProvider(zoneSet, Parent.TI_TZ_DestinationZoneInfo);
			}
		}

		void CheckValidTransportProvider(RateTransportProvider zoneSet, ZPropertyInfo info)
		{
			if (!IsZoneModeCompatibleWithRateMode(zoneSet.TP_ZoneType, zoneSet.TP_ZoneMode))
			{
				var errorMessage = Res.GetString("f580651b-3a25-451f-a813-1d75502e691e", "This {0} cannot be chosen as the Transport Zone Set's Mode is incompatible with this Rate Mode.", info.HumanReadableName);
				info.AddError(errorMessage);
			}
			var transportZoneOwnerPKs = Parent.Lookups.TransportZoneOwnerPKs;
			var zoneOwnerPK = zoneSet.TP_OH_RelatedParty;
			if (!zoneOwnerPK.IsEmpty && !transportZoneOwnerPKs.Contains(zoneOwnerPK))
			{
				info.AddError(Res.GetString("6760d7be-7282-4175-b894-52c6c5299804", "This {0} cannot be chosen as the Transport Zone Set's Zone Owner is different to the Service Provider / Client of this Rate Entry.", info.HumanReadableName));
			}

			var country = Parent.Origin()?.Country;
			if (Parent.Origin()?.Country != null && zoneSet.Country != country)
			{
				info.AddError(Res.GetString("1d3372a4-9593-42f9-bb77-4b4004d0475b", "This {0} cannot be chosen as it belongs to a Transport Zone Set is not in the same country/region as the Location of this Rate Entry.", info.HumanReadableName));
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		bool IsZoneModeCompatibleWithRateMode(string zoneType, string zoneMode)
		{
			if (zoneType != RatingConstants.RatingZoneTypes.All && zoneType != RatingConstants.RatingZoneTypes.Rating)
			{
				return false;
			}

			if (zoneMode == Parent.TI_Mode || zoneMode == Core.Constants.RateMode.ALL)
			{
				return true;
			}

			switch (Parent.TI_Mode)
			{
				case Core.Constants.RateMode.LSE:
				case Core.Constants.RateMode.ULD:
					return zoneMode == Core.Constants.RateMode.AIR;

				case Core.Constants.RateMode.LCL:
				case Core.Constants.RateMode.FCL:
					return zoneMode == Core.Constants.RateMode.SEA;

				case Core.Constants.RateMode.FRO:
				case Core.Constants.RateMode.FTL:
				case Core.Constants.RateMode.LRO:
					return zoneMode == Core.Constants.RateMode.ROA;

				case Core.Constants.RateMode.LRA:
				case Core.Constants.RateMode.FRA:
				case Core.Constants.RateMode.FWL:
					return zoneMode == Core.Constants.RateMode.RAI;
			}

			return false;
		}

		#endregion

		#region TI_DestinationLRC

		protected override void CheckTI_DestinationLRC()
		{
			base.CheckTI_DestinationLRC();
			var location = Parent.Destination();
			if (!Parent.TI_DestinationLRC.IsEmpty && location == null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TI_DestinationLRCInfo, Parent.Lookups.Locations);
			}
			else
			{
				DestinationValidator.CheckRateEntryLocations(location);
			}

			DestinationValidator.ValidateInternationalZone();
			DestinationValidator.ValidateIATACityCode(location);
		}

		LocationValidator DestinationValidator
		{
			get
			{
				if (fDestinationValidator == null)
				{
					fDestinationValidator = new LocationValidator(Parent, Parent.TI_DestinationLRCInfo);
				}
				return fDestinationValidator;
			}
		}

		LocationValidator fDestinationValidator;

		#endregion

		#region TI_RC

		protected override void CheckTI_RC()
		{
			base.CheckTI_RC();
			if (Parent.IsULDFreight() ||
				Parent.TI_RateCategory == RatingConstants.RateCategory.SCO ||
				Parent.IsShippingImportDetention() ||
				Parent.IsShippingExportDetention())
			{
				MandatoryValidation.CheckEntered(Parent.TI_RCInfo);
			}

			if (Parent.Container != null)
			{
				if (Parent.IsAir() && !Parent.Container.IsAirContainer)
				{
					Parent.TI_RCInfo.AddError(Res.GetString("edf8bfa0-9948-42ba-83be-a5dcf50b4512", "This is an Air Freight entry - please choose an Air Freight ULD Container."));
				}
				else if (Parent.IsSea() && !Parent.Container.IsSeaContainer)
				{
					Parent.TI_RCInfo.AddError(Res.GetString("04a1bfd9-30b9-480a-950b-dee8c34b573e", "This is a Sea Freight entry - please choose a Sea Freight FCL Container."));
				}
				else if (Parent.IsYardUnitTypeEmpty())
				{
					Parent.TI_RCInfo.AddError(Res.GetString("cac8db8d-480b-46c8-99e4-a41277d2a75b", "The type size is not allowed when Unit Type is left blank."));
				}
			}

			var matchingContract = GetMatchingLinkedCarrierContract();

			var hasBlank = string.IsNullOrEmpty(matchingContract?.RCT_ContainerType) || string.IsNullOrEmpty(Parent?.Container?.RC_ContainerType);
			if (!hasBlank && matchingContract.RCT_ContainerType != Parent.Container.RC_ContainerType)
			{
				var message = Res.GetString(
					"ed3fdb30-3cc2-4dba-b844-d196ac245c02",
					"Container Type ({0}) of Container/Equipment Type ({1}) does not match the Container Type ({2}) of the linked Carrier Contract {3}.",
					Parent.Container.RC_ContainerType, Parent.Container.RC_Code, matchingContract.RCT_ContainerType, matchingContract.RCT_ContractNumber
				);
				Parent.TI_RCInfo.AddError(message);
			}
		}

		#endregion

		#region TI_ContractNumber

		protected override void CheckTI_ContractNumberIsWesternEuropean()
		{
			// Should not force Western European only
		}

		protected override void CheckTI_ContractNumber()
		{
			base.CheckTI_ContractNumber();

			// The Equalization Calculator requires a TI_ContractNumber
			if (Parent.TI_ContractNumber.IsEmpty && (Parent.TI_ContractNumberInfo.HasChangesOrIsNew() || Parent.IsRateLinesLoaded))
			{
				if (Parent.RateLines.Cast<RateLine>().Any(x => x.Uses(CalculatorType.Equalization)))
				{
					MandatoryValidation.CheckEntered(Parent.TI_ContractNumberInfo);
				}
			}
			else if (!Parent.TI_ContractNumber.IsEmpty && IsEligibleForCarrierOrClientContracts && ObjectFactory.Get<IContractPermissions>().IsCarrierAndClientContractModulesEnabled())
			{
				CheckTI_ContractNumberForContractNumberLookup();
			}
		}

		void CheckTI_ContractNumberForContractNumberLookup()
		{
			var result = GetActiveRatingContract();
			if (result.MatchingContract == null)
			{
				Parent.TI_ContractNumberInfo.AddWarning(GetContractNumberNotFoundErrorMessage(result.OthersWithSameContractNumber));
			}
		}

		string GetContractNumberNotFoundErrorMessage(IEnumerable<IRatingContract> othersWithSameContractNumber)
		{
			var contractType = GetContractNumberType();
			if (contractType == RatingContractTypes.Client)
			{
				return Res.GetString("2eecea30-ab38-4c46-acdf-edd4018e6cf1", "'{0}' does NOT have a corresponding Client Contract & Allocations record.", Parent.TI_ContractNumber);
			}
			else // if (contractType == RatingContractTypes.Provider)
			{
				if (othersWithSameContractNumber.Any())
				{
					return Res.GetString("8180a142-5d0a-41b9-aa18-4673e3b620c1", "No Carrier Contract {0} found under Carrier {1}. However, it is a valid Carrier Contract under other carriers.", Parent.TI_ContractNumber, Parent.Organisation);
				}
				else
				{
					return Res.GetString("d1e80a74-02bc-4a7c-9dcf-9d2b60bd46fe", "No Carrier Contract {0} found under Carrier {1}.", Parent.TI_ContractNumber, Parent.Organisation);
				}
			}
		}

		string GetContractNumberType()
		{
			if (Parent.HasCarrierContractNumberLookup())
			{
				return RatingContractTypes.Provider;
			}
			else if (Parent.HasClientContractNumberLookup())
			{
				return RatingContractTypes.Client;
			}
			else
			{
				return String.Empty;
			}
		}

		bool IsEligibleForCarrierOrClientContracts => !GetContractNumberType().IsNullOrEmpty();

		(IRatingContract MatchingContract, IEnumerable<IRatingContract> OthersWithSameContractNumber) GetActiveRatingContract()
		{
			var baseQuery = new ZQuery(RatingContractSchema.RCT_ContractType, GetContractNumberType());
			baseQuery.AddToFilter(RatingContractSchema.RCT_ContractNumber, Parent.TI_ContractNumber);
			baseQuery.AddToFilter(RatingContractSchema.RCT_IsActive, true);

			var queryLocalCompany = baseQuery.ShallowClone();
			queryLocalCompany.AddToFilter(RatingContractSchema.RCT_GC, Env.CurrentCompanyPK);

			var results = Parent.Factory.Load<IRatingContract>(queryLocalCompany);
			var resultsByCarrierMatch = results.Split(r => r.RCT_OH == Parent.Parent.TH_OH);
			var singleCarrierMatch = resultsByCarrierMatch.MatchingSet.SingleOrDefault();

			if (singleCarrierMatch == null)
			{
				var queryGlobalCompany = baseQuery.ShallowClone();
				queryGlobalCompany.AddToFilter(RatingContractSchema.RCT_GC, null);

				var resultsGlobal = Parent.Factory.Load<IRatingContract>(queryGlobalCompany);
				var resultsByCarrierMatchGlobal = resultsGlobal.Split(r => r.RCT_OH == Parent.Parent.TH_OH);
				var singleCarrierMatchGlobal = resultsByCarrierMatchGlobal.MatchingSet.SingleOrDefault();

				return (singleCarrierMatchGlobal, resultsByCarrierMatch.NonMatchingSet.Union(resultsByCarrierMatchGlobal.NonMatchingSet));
			}

			return (singleCarrierMatch, resultsByCarrierMatch.NonMatchingSet);
		}

		IRatingContract GetMatchingLinkedCarrierContract()
		{
			// Only Carrier contracts may be linked.
			if (Parent.TI_ContractNumberLinked
				&& !Parent.TI_ContractNumber.IsEmpty
				&& ObjectFactory.Get<IContractPermissions>().IsCarrierAndClientContractModulesEnabled())
			{
				return GetActiveRatingContract().MatchingContract;
			}
			return null;
		}

		#endregion

		#region TI_ContractNumberLinked

		protected override void CheckTI_ContractNumberLinked()
		{
			if (Parent.TI_ContractNumberLinked && ObjectFactory.Get<IContractPermissions>().IsCarrierAndClientContractModulesEnabled())
			{
				var contractType = GetContractNumberType();
				if (contractType != RatingContractTypes.Provider)
				{
					Parent.TI_ContractNumberLinkedInfo.AddError(Res.GetString("bb18da06-a0fe-46a3-9bc9-0937d5f87f94", "Only Carrier Contract Numbers may be linked"));
					return;
				}

				if (Parent.TI_ContractNumber.IsEmpty)
				{
					var message = Res.GetString("a0ec1fc1-cb6b-456a-8eed-12797f4fee18", "The Carrier Contract Number cannot be blank. It must have a corresponding Carrier Contract & Allocations record.");
					Parent.TI_ContractNumberLinkedInfo.AddError(message);
					return;
				}

				var result = GetActiveRatingContract();
				if (result.MatchingContract == null)
				{
					Parent.TI_ContractNumberLinkedInfo.AddError(GetContractNumberNotFoundErrorMessage(result.OthersWithSameContractNumber));
					return;
				}
			}

			// Needs to be outside the condition so that it can
			// re-validate once the contract number is unlinked
			ValidateTI_RH_NKCommodityCode();
			ValidateTI_RateStartDate();
			ValidateTI_RateEndDate();
			ValidateTI_Mode();
			ValidateTI_RC();
		}

		#endregion

		#region TI_RateStartDate

		protected override void CheckTI_RateStartDate()
		{
			base.CheckTI_RateStartDate();
			CheckRateStartDateForContractLinked();
		}

		void CheckRateStartDateForContractLinked()
		{
			var contract = GetMatchingLinkedCarrierContract();
			if (contract != null)
			{
				if (Parent.TI_RateStartDate < contract.RCT_StartDate)
				{
					var startMessage = Res.GetString("38fdcddf-5c6f-497e-bef2-84b4a7eb4d8f",
						"Start Date ({0}) of Rates for the Carrier Contract {1} is earlier than the Contract’s Start Date ({2}). Relevant rates validity period should be within the Contract’s validity period.",
						Parent.TI_RateStartDate.ToShortDateString(),
						contract.RCT_ContractNumber,
						contract.RCT_StartDate.ToShortDateString());

					Parent.TI_RateStartDateInfo.AddError(startMessage);
				}
			}
		}

		#endregion

		#region TI_RateEndDate

		protected override void CheckTI_RateEndDate()
		{
			base.CheckTI_RateEndDate();

			if (Parent.TI_RateEndDate.IsEmpty && (Parent.TI_RateEndDateInfo.HasChangesOrIsNew() || Parent.IsRateLinesLoaded))
			{
				if (Parent.RateLines.Cast<RateLine>().Any(x => x.Uses(CalculatorType.Equalization)))
				{
					MandatoryValidation.CheckEntered(Parent.TI_RateEndDateInfo);
				}
			}

			CheckRateEndDateForContractLinked();
		}

		void CheckRateEndDateForContractLinked()
		{
			var contract = GetMatchingLinkedCarrierContract();
			if (contract != null)
			{
				// End date validation logic:
				//  | RCT_EndDate  | TI_RateEndDate | Result
				//  | Blank        | Blank          | Valid
				//  | Blank        | Any value      | Valid
				//  | T            | Before T       | Valid
				//  | T            | Blank          | Invalid
				//  | T            | After T        | Invalid
				if (!contract.RCT_EndDate.IsEmpty &&
					(Parent.TI_RateEndDate.IsEmpty || contract.RCT_EndDate < Parent.TI_RateEndDate))
				{
					var endMessage = Res.GetString("db15e4d5-3e92-492d-8a3c-9804744c0ac3",
						"Expiry Date ({0}) of Rates for the Carrier Contract {1} is later than the Contract’s Expiry Date ({2}). Relevant rates validity period should be within the Contract’s validity period.",
						Parent.TI_RateEndDate.IsEmpty ? (NoResString)"<blank>" : Parent.TI_RateEndDate.ToShortDateString(),
						contract.RCT_ContractNumber,
						contract.RCT_EndDate.ToShortDateString());

					Parent.TI_RateEndDateInfo.AddError(endMessage);
				}
			}
		}

		#endregion

		#region TI_Mode

		protected override void CheckTI_Mode()
		{
			base.CheckTI_Mode();
			MandatoryValidation.CheckEntered(Parent.TI_ModeInfo);
			if (!Parent.TI_Mode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TI_ModeInfo, Parent.Lookups.TransportModes);
				ValidateTI_OH_TransportProvider();
				CheckModeForContractLinked();
			}
		}

		void CheckModeForContractLinked()
		{
			var contract = GetMatchingLinkedCarrierContract();
			if (contract != null)
			{
				var entryTransportMode = GetTransportMode();
				if (entryTransportMode != contract.RCT_TransportMode)
				{
					var message = Res.GetString("1102ad68-4f75-45ee-8e67-6088f3d4a1fd", "Transport Mode ({0}) of the Carrier Contract {1} does not match the Transport Mode ({2}) of this Rate. Rate and relevant Carrier Contract should have aligned Transport Mode.",
						contract.RCT_TransportMode,
						contract.RCT_ContractNumber,
						entryTransportMode);

					Parent.TI_ModeInfo.AddError(message);
				}
			}
		}

		string GetTransportMode()
		{
			if (Parent.IsSea())
			{
				return TransportModes.Sea;
			}
			else if (Parent.IsAir())
			{
				return TransportModes.Air;
			}

			return Parent.TI_Mode;
		}

		#endregion

		#region TI_OH_TransportProvider

		protected override void CheckTI_OH_TransportProvider()
		{
			base.CheckTI_OH_TransportProvider();
			if (!Parent.TI_Mode.IsEmpty && !Parent.TI_OH_TransportProvider.IsEmpty)
			{
				var provider = Parent.TransportProvider;
				var info = Parent.TI_OH_TransportProviderInfo;
				var providerList = Parent.Lookups.ShippingProviders;
				var msg = provider != null
					? (string)providerList.GetAllNotificationsWhenAdditionalFilterNotMet(provider)
					: ListValidation.GetNotificationMessage(info).ToString();
				ListValidation.ErrorIfInvalidPK(info, providerList, (NoResString)msg);
			}
		}

		#endregion

		#region TI_ViaLRC

		protected override void CheckTI_ViaLRC()
		{
			base.CheckTI_ViaLRC();
			var location = Parent.Via();
			if (!Parent.TI_ViaLRC.IsEmpty && location == null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TI_ViaLRCInfo, Parent.Lookups.Locations);
			}
			else if (!Parent.TI_ViaLRC.IsEmpty && (Parent.TI_ViaLRC == Parent.TI_OriginLRC || Parent.TI_ViaLRC == Parent.TI_DestinationLRC))
			{
				Parent.TI_ViaLRCInfo.AddError(ErrorMessages.ViaError);
			}
			else if (!Parent.IsIntercompanyTariff())
			{
				ViaValidator.CheckRateEntryLocations(location);
			}

			ViaValidator.ValidateInternationalZone();
			ViaValidator.ValidateIATACityCode(location);
		}

		LocationValidator ViaValidator => fViaValidator ?? (fViaValidator = new LocationValidator(Parent, Parent.TI_ViaLRCInfo));

		LocationValidator fViaValidator;

		#endregion

		#region TI_GatewayAgentType

		protected override void CheckTI_GatewayAgentType()
		{
			base.CheckTI_GatewayAgentType();

			if (Parent.IsIntercompanyTariff())
			{
				if (!Parent.TI_GatewayAgentType.IsEmpty)
				{
					if (Parent.TI_GatewayAgentType == Core.Constants.GatewayAgentType.Codes.SucceedingSendingAgent || Parent.TI_GatewayAgentType == Core.Constants.GatewayAgentType.Codes.FirstSendingAgent)
					{
						if (Parent.IsInDatabase && !Parent.HasChanges)
						{
							Parent.TI_GatewayAgentTypeInfo.AddWarning(ErrorMessages.ObsoleteGatewayAgentTypesErrorMessage);
						}
						else
						{
							Parent.TI_GatewayAgentTypeInfo.AddError(ErrorMessages.ObsoleteGatewayAgentTypesErrorMessage);
						}
					}
					else
					{
						ListValidation.ErrorIfInvalidCode(Parent.TI_GatewayAgentTypeInfo, Parent.Lookups.GatewayAgentTypes);
					}
				}
			}
			else
			{
				MandatoryValidation.CheckNotEntered(Parent.TI_GatewayAgentTypeInfo);
			}
		}
		#endregion

		#region TI_PlannedLoadLRC

		protected override void CheckTI_PlannedLoadLRC()
		{
			base.CheckTI_PlannedLoadLRC();
			var location = Parent.PlannedLoad();
			if (!Parent.TI_PlannedLoadLRC.IsEmpty && location == null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TI_PlannedLoadLRCInfo, Parent.Lookups.Locations);
			}
			else if (Parent.IsIntercompanyTariff())
			{
				PlannedLoadValidator.CheckRateEntryLocations(location);
			}

			PlannedLoadValidator.ValidateInternationalZone();
			PlannedLoadValidator.ValidateIATACityCode(location);
		}

		LocationValidator fPlannedLoadValidator;
		LocationValidator PlannedLoadValidator
		{
			get { return fPlannedLoadValidator ?? (fPlannedLoadValidator = new LocationValidator(Parent, Parent.TI_PlannedLoadLRCInfo)); }
		}

		#endregion

		#region TI_PlannedDischargeLRC

		protected override void CheckTI_PlannedDischargeLRC()
		{
			base.CheckTI_PlannedDischargeLRC();
			var location = Parent.PlannedDischarge();
			if (!Parent.TI_PlannedDischargeLRC.IsEmpty && location == null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TI_PlannedDischargeLRCInfo, Parent.Lookups.Locations);
			}
			else if (Parent.IsIntercompanyTariff())
			{
				PlannedDischargeValidator.CheckRateEntryLocations(location);
			}

			PlannedDischargeValidator.ValidateInternationalZone();
			PlannedDischargeValidator.ValidateIATACityCode(location);
		}

		LocationValidator fPlannedDischargeValidator;
		LocationValidator PlannedDischargeValidator
		{
			get { return fPlannedDischargeValidator ?? (fPlannedDischargeValidator = new LocationValidator(Parent, Parent.TI_PlannedDischargeLRCInfo)); }
		}

		#endregion

		#region RateEntryLocations

		void ValidateRateEntryLocations()
		{
			Parent.RateEntryLocations.RunPreSaveValidation();
		}

		#endregion

		#region TI_FirstLoadLRC

		protected override void CheckTI_FirstLoadLRC()
		{
			base.CheckTI_FirstLoadLRC();

			var location = Parent.FirstLoad();
			if (!Parent.TI_FirstLoadLRC.IsEmpty && location == null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TI_FirstLoadLRCInfo, Parent.Lookups.Locations);
			}
		}

		#endregion

		#region TI_LastDischargeLRC

		protected override void CheckTI_LastDischargeLRC()
		{
			base.CheckTI_LastDischargeLRC();

			var location = Parent.LastDischarge();
			if (!Parent.TI_LastDischargeLRC.IsEmpty && location == null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TI_LastDischargeLRCInfo, Parent.Lookups.Locations);
			}
		}

		#endregion

		#region TI_FirstRouteSetLoadPortLRC

		protected override void CheckTI_FirstRouteSetLoadPortLRC()
		{
			base.CheckTI_FirstRouteSetLoadPortLRC();

			var location = Parent.FirstRouteSetLoad();
			if (!Parent.TI_FirstRouteSetLoadPortLRC.IsEmpty && location == null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TI_FirstRouteSetLoadPortLRCInfo, Parent.Lookups.Locations);
			}
		}

		#endregion

		#region TI_LastRouteSetDischargePortLRC

		protected override void CheckTI_LastRouteSetDischargePortLRC()
		{
			base.CheckTI_LastRouteSetDischargePortLRC();

			var location = Parent.LastRouteSetDischarge();
			if (!Parent.TI_LastRouteSetDischargePortLRC.IsEmpty && location == null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TI_LastRouteSetDischargePortLRCInfo, Parent.Lookups.Locations);
			}
		}

		#endregion

		#region TI_RateOrigin

		protected override void CheckTI_RateOrigin()
		{
			base.CheckTI_RateOrigin();
			var location = Parent.RateOrigin();
			if (!Parent.TI_RateOrigin.IsEmpty && location == null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TI_RateOriginInfo, Parent.Lookups.Locations);
			}
			else if (!Parent.IsCosting())
			{
				RateOriginValidator.CheckRateEntryLocations(location);
			}

			RateOriginValidator.ValidateInternationalZone();
			RateOriginValidator.ValidateIATACityCode(location);
		}

		LocationValidator fRateOriginValidator;
		LocationValidator RateOriginValidator
		{
			get { return fRateOriginValidator ?? (fRateOriginValidator = new LocationValidator(Parent, Parent.TI_RateOriginInfo)); }
		}

		#endregion

		#region TI_RateDestination

		protected override void CheckTI_RateDestination()
		{
			base.CheckTI_RateDestination();
			var location = Parent.RateDestination();
			if (!Parent.TI_RateDestination.IsEmpty && location == null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TI_RateDestinationInfo, Parent.Lookups.Locations);
			}
			else if (!Parent.IsCosting())
			{
				RateDestinationValidator.CheckRateEntryLocations(location);
			}

			RateDestinationValidator.ValidateInternationalZone();
			RateDestinationValidator.ValidateIATACityCode(location);
		}

		LocationValidator fRateDestinationValidator;
		LocationValidator RateDestinationValidator
		{
			get { return fRateDestinationValidator ?? (fRateDestinationValidator = new LocationValidator(Parent, Parent.TI_RateDestinationInfo)); }
		}

		#endregion

		#region TI_RX_NKCurrency

		protected override void CheckTI_RX_NKCurrency()
		{
			base.CheckTI_RX_NKCurrency();
			if (Parent.TI_RX_NKCurrency.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.TI_RX_NKCurrencyInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.TI_RX_NKCurrencyInfo, Parent.Lookups.Currencies);
			}
		}

		#endregion

		#region TI_Frequency

		protected override void CheckTI_Frequency()
		{
			base.CheckTI_Frequency();
			if (Parent.TI_Frequency < 0)
			{
				Parent.TI_FrequencyInfo.AddError(ErrorMessages.FrequencyGreaterThanZero);
			}
			else if (Parent.TI_Frequency == 0 && !Parent.TI_FrequencyUnit.IsEmpty)
			{
				Parent.TI_FrequencyInfo.AddError(ErrorMessages.NoFrequency);
			}
			else if (Parent.IsFreightEntry() && RequiredFieldsFromRegistry.RequireFrequency && Parent.TI_Frequency == 0)
			{
				MandatoryValidation.CheckEntered(Parent.TI_FrequencyInfo);
			}

			ValidateTI_FrequencyUnit();
		}

		#endregion

		#region TI_FrequencyUnit

		protected override void CheckTI_FrequencyUnit()
		{
			base.CheckTI_FrequencyUnit();
			if (!Parent.Lookups.FrequencyUnits.ContainsCode(Parent.TI_FrequencyUnitInfo.Value))
			{
				Parent.TI_FrequencyUnitInfo.AddError(ErrorMessages.InvalidFrequencyUnit);
			}
			else if (Parent.TI_FrequencyUnit.IsEmpty && Parent.TI_Frequency > 0)
			{
				Parent.TI_FrequencyUnitInfo.AddError(ErrorMessages.NoFrequencyUnit);
			}

			ValidateTI_Frequency();
		}

		#endregion

		#region TI_TransitTime

		protected override void CheckTI_TransitTime()
		{
			base.CheckTI_TransitTime();

			if (Parent.IsAirFreight())
			{
				if (!Parent.Lookups.AirTransitTimes.ContainsCode(Parent.TI_TransitTimeInfo.Value) && !Parent.TI_TransitTime.IsEmpty)
				{
					Parent.TI_TransitTimeInfo.AddError(ErrorMessages.InvalidTransitTime);
				}
			}
			else if (Parent.IsLCLFreight() || Parent.TI_RateCategory == RatingConstants.RateCategory.FCL)
			{
				if (!Parent.Lookups.SeaTransitTimes.ContainsCode(Parent.TI_TransitTimeInfo.Value) && !Parent.TI_TransitTime.IsEmpty)
				{
					Parent.TI_TransitTimeInfo.AddError(ErrorMessages.InvalidTransitTime);
				}
			}

			if (!Parent.TI_TransitTimeInfo.HasErrors() && Parent.IsFreightEntry() && RequiredFieldsFromRegistry.RequireTransitTime)
			{
				MandatoryValidation.CheckEntered(Parent.TI_TransitTimeInfo);
			}
		}

		#endregion

		#region TI_MatchContainerRateClass

		protected override void CheckTI_MatchContainerRateClass()
		{
			base.CheckTI_MatchContainerRateClass();

			if (Parent.TI_MatchContainerRateClass)
			{
				if (Parent.Container == null)
				{
					Parent.TI_MatchContainerRateClassInfo.AddError(Res.GetString("00385ad5-4fc6-4dfb-bb86-842d4d95c4ee", "You must specify a container type before you can choose to match the container class."));
					return;
				}
				else
				{
					if (Parent.ContainerClass.IsEmpty)
					{
						Parent.TI_MatchContainerRateClassInfo.AddError(Res.GetString("e3cc8bf3-09f1-4317-862e-6636fcf2ec50", "The {0} container type does not have a Rate Class specified.", Parent.Container.RC_Code));
						return;
					}
				}
			}
		}

		#endregion

		#region TI_RH_NKCommodityCode

		protected override void CheckTI_RH_NKCommodityCode()
		{
			base.CheckTI_RH_NKCommodityCode();
			ListValidation.ErrorIfInvalidCode(Parent.TI_RH_NKCommodityCodeInfo, Parent.Lookups.CommodityCodes);

			var hasCommodityCode = Parent.TI_RateCategory != RatingConstants.RateCategory.CST
				&& Parent.TI_RateCategory != RatingConstants.RateCategory.SID
				&& Parent.TI_RateCategory != RatingConstants.RateCategory.SED;
			if (hasCommodityCode && RequiredFieldsFromRegistry.RequireCommodityCode)
			{
				MandatoryValidation.CheckEntered(Parent.TI_RH_NKCommodityCodeInfo);
			}

			CheckCommodityForContractLinked();
		}

		void CheckCommodityForContractLinked()
		{
			var contract = GetMatchingLinkedCarrierContract();
			if (contract != null)
			{
				var contractAllowsHazardous = contract.RCT_AllowHazardousCommodities;
				var commodity = Parent.Factory.LoadFromNaturalKey<RefCommodityCode>(RefCommodityCodeSchema.RH_Code, Parent.TI_RH_NKCommodityCode);
				var commodityIsHazardous = commodity?.RH_IsHazardous ?? false;

				var success = contractAllowsHazardous || !commodityIsHazardous;
				if (!success)
				{
					var message = Res.GetString("528a4fba-9fd7-491e-9366-92bb2df32ef7", "Commodity ({0}) is a hazardous commodity but the linked Carrier Contract {1} does not allow Hazardous Commodities.",
						commodity.RH_Code,
						contract.RCT_ContractNumber);

					Parent.TI_RH_NKCommodityCodeInfo.AddError(message);
				}
			}
		}

		#endregion

		#region TI_RS_NKServiceLevel_NI

		protected override void CheckTI_RS_NKServiceLevel_NI()
		{
			base.CheckTI_RS_NKServiceLevel_NI();
			ListValidation.ErrorIfInvalidCode(Parent.TI_RS_NKServiceLevel_NIInfo, Parent.Lookups.ServiceLevel_NIs);

			if (!Parent.IsCFS() && !Parent.IsShippingExportDetention() && !Parent.IsShippingImportDetention() && RequiredFieldsFromRegistry.RequireServiceLevel)
			{
				MandatoryValidation.CheckEntered(Parent.TI_RS_NKServiceLevel_NIInfo);
			}
		}

		#endregion

		#region TI_PL_NKCarrierServiceLevel

		protected override void CheckTI_PL_NKCarrierServiceLevel()
		{
			base.CheckTI_PL_NKCarrierServiceLevel();
			ListValidation.ErrorIfInvalidCode(Parent.TI_PL_NKCarrierServiceLevelInfo);

			if (!Parent.IsCFS() && Parent.IsCosting() && Parent.IsAir() && RequiredFieldsFromRegistry.RequireServiceLevel)
			{
				MandatoryValidation.CheckEntered(Parent.TI_PL_NKCarrierServiceLevelInfo);
			}
		}

		#endregion

		#region TI_RS_NKGatewayServiceLevel

		protected override void CheckTI_RS_NKGatewayServiceLevel()
		{
			base.CheckTI_RS_NKGatewayServiceLevel();

			if (!Parent.TI_RS_NKGatewayServiceLevel.IsEmpty)
			{
				if (!Parent.IsIntercompanyTariff())
				{
					Parent.TI_RS_NKGatewayServiceLevelInfo.AddError(ErrorMessages.OnlyApplicableForIntercompanyTariffErrorMessage);
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(Parent.TI_RS_NKGatewayServiceLevelInfo);
				}
			}
		}

		#endregion

		#region TI_RS_NKShipmentGatewayServiceLevel

		protected override void CheckTI_RS_NKShipmentGatewayServiceLevel()
		{
			base.CheckTI_RS_NKShipmentGatewayServiceLevel();

			if (!Parent.TI_RS_NKShipmentGatewayServiceLevel.IsEmpty)
			{
				if (!Parent.IsIntercompanyTariff())
				{
					Parent.TI_RS_NKShipmentGatewayServiceLevelInfo.AddError(ErrorMessages.OnlyApplicableForIntercompanyTariffErrorMessage);
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(Parent.TI_RS_NKShipmentGatewayServiceLevelInfo);
				}
			}
		}

		#endregion

		#region TI_PaymentTerm
		protected override void CheckTI_PaymentTerm()
		{
			base.CheckTI_PaymentTerm();

			if (!Parent.TI_PaymentTerm.IsEmpty && !Parent.Lookups.PaymentTerms.ContainsCode(Parent.TI_PaymentTerm))
			{
				Parent.TI_PaymentTermInfo.AddError(ErrorMessages.InvalidPaymentTerm);
			}

			ValidateTI_PaymentTerm();
		}
		#endregion

		#region TI_AircraftType

		protected override void CheckTI_AircraftType()
		{
			base.CheckTI_AircraftType();

			ListValidation.ErrorIfInvalidCode(Parent.TI_AircraftTypeInfo, Parent.Lookups.AircraftTypes);

			if (!Parent.IsAir() && !Parent.TI_AircraftType.IsEmpty)
			{
				Parent.TI_AircraftTypeInfo.AddError(Res.GetString("8f333192-cd9c-4a3b-8c65-19989f29d0a1", "Aircraft Type can only be selected with an Air entry."));
			}
		}

		#endregion

		#region TI_ShipmentConsolidationStatus

		protected override void CheckTI_ShipmentConsolidationStatus()
		{
			base.CheckTI_ShipmentConsolidationStatus();
			ListValidation.ErrorIfInvalidCode(Parent.TI_ShipmentConsolidationStatusInfo, Parent.Lookups.ShipmentConsolidationStatusList);
		}

		#endregion

		#region Rating Validation Required Fields

		IAutoRatingRequiredFields RequiredFieldsFromRegistry
		{
			get { return RatingValidationRequiredFields.TypedValue; }
		}

		AutoRatingRequiredFieldsRegistryItem RatingValidationRequiredFields
		{
			get
			{
				AutoRatingRequiredFieldsRegistryItem result;

				if (Parent.IsClientRate())
				{
					result = RatingDataRegistry.Instance.ClientRatesRequiredFields;
				}
				else if (Parent.IsQuote())
				{
					result = RatingDataRegistry.Instance.QuotationsRequiredFields;
				}
				else if (Parent.IsCosting())
				{
					result = RatingDataRegistry.Instance.CostsRequiredFields;
				}
				else if (Parent.IsCompanyTariff())
				{
					result = RatingDataRegistry.Instance.CompanyTariffsRequiredFields;
				}
				else
				{
					result = new AutoRatingRequiredFieldsRegistryItem((NoResString)"Other", (NoResString)"Other", null, null, null, RegistryStorageFlags.All, false); // Hard-coded constant
				}

				return result;
			}
		}

		#endregion

		internal void ValidateLocation(ZPropertyInfo info = null)
		{
			if (info == null)
			{
				ValidateCalculatedProperty(Parent.OriginSuburbPKInfo);
				ValidateCalculatedProperty(Parent.DestinationSuburbPKInfo);
			}
			else
			{
				ValidateCalculatedProperty(info);
			}
		}

		protected void CheckOriginSuburbPK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.OriginSuburbPKInfo);
		}

		protected void CheckDestinationSuburbPK()
		{
			ListValidation.ErrorIfInvalidPK(Parent.DestinationSuburbPKInfo);
		}

		public void ValidateTI_WW_Warehouse()
		{
			ValidateCalculatedProperty(Parent.TI_WW_WarehouseInfo);
		}

		protected void CheckTI_WW_Warehouse()
		{
			if (Parent.IsWHS() || Parent.IsTRW() || Parent.IsTWU())
			{
				if (!Parent.AllWarehouses)
				{
					MandatoryValidation.CheckEntered(Parent.TI_WW_WarehouseInfo);
				}
				else if (Parent.IsCosting() && Parent.Warehouse() == null)
				{
					Parent.TI_WW_WarehouseInfo.AddError(Res.GetString("bbb74c2d-32c5-4ce9-b7a4-dbc1dfb09720", "For Warehouse Costings, you must nominate a specific Warehouse. You cannot create Warehouse costs for 'All Warehouses'."));
				}
			}
		}

		public void ValidateTI_CYC_WW_Facility()
		{
			ValidateCalculatedProperty(Parent.TI_CYC_WW_FacilityInfo);
		}

		protected void CheckTI_CYC_WW_Facility()
		{
			if (Parent.IsContainerYard())
			{
				ListValidation.ErrorIfInvalidPK(Parent.TI_CYC_WW_FacilityInfo);

				if (Parent.IsCosting() && Parent.Yard() == null)
				{
					Parent.TI_CYC_WW_FacilityInfo.AddError(Res.GetString("051031e7-88a9-41e2-8639-1b1fd65a42fb", "For Container Yard Costings, you must nominate a specific Yard. You cannot create Container Yard costs for 'All Yards'."));
				}
			}
		}

		protected override void CheckTI_QuotePageIncoTerm()
		{
			base.CheckTI_QuotePageIncoTerm();

			IncotermValidation.Instance.ErrorIfExpired(Parent.TI_QuotePageIncoTermInfo);
		}

		protected override void CheckTI_HBLDeliveryMode()
		{
			base.CheckTI_HBLDeliveryMode();

			if (!Parent.TI_HBLDeliveryMode.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.TI_HBLDeliveryModeInfo, Parent.Lookups.HBLDeliveryModeList);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateTI_WW_Warehouse();
			ValidateTI_CYC_WW_Facility();

			ValidateRateEntryLocations();
#if DEBUG
			Parent.ValidationCountForTest++;
#endif
		}
	}
}

