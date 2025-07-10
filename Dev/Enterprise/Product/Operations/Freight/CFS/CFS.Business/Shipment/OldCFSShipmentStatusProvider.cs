using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.CFS.Business
{
	public class OldCFSShipmentStatusProvider : CFSShipmentStatusProvider
	{
		public OldCFSShipmentStatusProvider(CFSShipment shipment)
			: base(shipment)
		{
		}

		protected override StatusClass StatusClassCore()
		{
			return StatusClass.None;
		}

		protected override ZString ShortStatusCore()
		{
			ZString result = "";
			ZString longDescription = Status.ToUpper();

			if (longDescription.StartsWith(CMRGatePassStatuses.ClearHrm, StringComparison.Ordinal))
			{
				result = CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement;
			}
			else if (longDescription.StartsWith(CMRGatePassStatuses.Clear, StringComparison.Ordinal))
			{
				result = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			}
			else if (longDescription.StartsWith(CMRGatePassStatuses.Detained, StringComparison.Ordinal))
			{
				result = CMRGatePassStatuses.Detained;
			}
			else if (longDescription.StartsWith(CMRGatePassStatuses.ConditionalClear, StringComparison.Ordinal))
			{
				result = CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation;
			}
			else if (longDescription.StartsWith(CMRGatePassStatuses.AcsSeized, StringComparison.Ordinal))
			{
				result = CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms;
			}
			else if (longDescription.StartsWith(CMRGatePassStatuses.AqisSeized, StringComparison.Ordinal))
			{
				result = CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine;
			}
			else if (longDescription.StartsWith(CMRGatePassStatuses.Held, StringComparison.Ordinal))
			{
				result = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			}
			else if (longDescription.StartsWith(CMRGatePassStatuses.SubUBMov, StringComparison.Ordinal))
			{
				result = CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed;
			}
			else if (longDescription.StartsWith(CMRGatePassStatuses.Tranship, StringComparison.Ordinal))
			{
				result = CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus;
			}
			else if (longDescription.StartsWith(CMRGatePassStatuses.TranshipHrm, StringComparison.Ordinal))
			{
				result = CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement;
			}
			else if (longDescription.StartsWith(CMRGatePassStatuses.Transit, StringComparison.Ordinal))
			{
				result = CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction;
			}
			return result;
		}

		protected override ZString StatusCore()
		{
			var seaCargoLog = Factory.LoadTop1<StmALog>(JS_GatePassMostRecentClearOrDetainedLogFilter);
			return (seaCargoLog != null) ? seaCargoLog.SL_Reference : ZString.Empty;
		}

		internal static class CMRGatePassStatuses
		{
			public const string Clear = "CLEAR";
			public const string Detained = "DETAINED";
			public const string ConditionalClear = "CONDCLEAR";
			public const string AcsSeized = "ACSSEIZED";
			public const string AqisSeized = "AQISSEIZED";
			public const string ClearHrm = "CLEARHRM";
			public const string Held = "HELD";
			public const string SubUBMov = "SUBUBMOV";
			public const string Tranship = "TRANSHIP";
			public const string TranshipHrm = "TRANSHPHRM";
			public const string Transit = "TRANSIT";
		}

		public static ZQuery CMRSL_ReferenceFilter
		{
			get
			{
				ZQuery result = new ZQuery();
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CMRGatePassStatuses.Clear);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CMRGatePassStatuses.Detained);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CMRGatePassStatuses.ConditionalClear);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CMRGatePassStatuses.AcsSeized);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CMRGatePassStatuses.AqisSeized);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CMRGatePassStatuses.ClearHrm);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CMRGatePassStatuses.Held);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CMRGatePassStatuses.SubUBMov);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CMRGatePassStatuses.Tranship);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CMRGatePassStatuses.TranshipHrm);
				result.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, CMRGatePassStatuses.Transit);
				return result;
			}
		}

		ZQuery JS_GatePassMostRecentClearOrDetainedLogFilter
		{
			get
			{
				ZQuery result = new ZQuery();

				result.AddToFilter(StmALogSchema.SL_Parent, shipment.PK);
				result.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.SeaCargoDepotEvent.Code);
				result.AddToFilter(CMRSL_ReferenceFilter);
				result.OrderBy = StmALogSchema.Constants.SL_EventTime + " " + OrderByClause.Descending;

				result.FetchOnlyFromLocalCache = shipment.LogsNeededToDetermineSeaCargoStatusWerePrefetched;

				return result;
			}
		}

		BusinessObjectFactory Factory
		{
			get { return shipment.Factory; }
		}

		protected override bool CanSaveAndPrintCore(ISaveAndPrintUI ui)
		{
			if (shipment.HasErrors)
			{
				ui.ShowError(Res.GetString("eb005f8b-1c06-4cac-91a4-2e7c0ae13108", "Please clear all errors before saving."));
			}
			else
			{
				bool continueWithUnclearCargo = false;

				switch (ShortStatus)
				{
					case CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased:
					case CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus:
					case CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction:
						continueWithUnclearCargo = true;
						break;
					case CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation:
						continueWithUnclearCargo = ui.Ask(Res.GetString("ae068af8-cff2-44ef-a048-f10d62389d98", "Confirm conditional clearance actions have been completed.\r\nHave the conditional clearance requirements been met?"));
						break;
					case CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms:
						ui.ShowError(Res.GetString("2d826a60-9eba-4134-91a7-5dca00a2d158", "This shipment has been seized by customs and may not be gate passed."));
						break;
					case CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine:
						ui.ShowError(Res.GetString("2b4a5c93-99fa-4064-bbc0-ea6aa9e1d648", "This shipment has been seized by Quarantine and may not be gate passed"));
						break;
					case CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement:
					case CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement:
						ui.ShowWarning(Res.GetString("0c545e4e-6205-48b5-a1cc-967519a799f4", "This shipment is marked as high risk"));
						continueWithUnclearCargo = true;
						break;
					case CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed:
						ui.ShowWarning(Res.GetString("17ece812-91b8-4cec-9510-3671c6e9a581", "This shipment is clear to be moved underbond but may not be delivered for home consumption"));
						continueWithUnclearCargo = true;
						break;
					default:
						continueWithUnclearCargo = ui.Ask(Res.GetString("cba7ba81-be7a-4d89-9638-ab4aa1b60ecb", "This shipment has not been cleared by customs.  Continue with Contingency Release?"));
						break;
				}

				if (continueWithUnclearCargo || ShortStatus == CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased)
				{
					return true;
				}
			}

			return false;
		}

		protected override ZString DetailsFromMessagesCore
		{
			get { return ZString.Empty; }
		}
	}
}
