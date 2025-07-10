using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.eManifest.Business
{
	partial class ShipmentEntryStatusList
	{
		public static string GetStatusFromNotificationCode(BusinessObjectFactory factory, string code)
		{
			var dictionary =
				factory.GetCachedValue(
					"US.eManifest.ShipmentEntryStatusDictionary",
					() => new Dictionary<string, string>
							{
								{ "SN050", Codes.ShipmentHold },
								{ "SN051", Codes.ShipmentRemoveHold },
								{ "SN052", Codes.ShipmentSeized },
								{ "SN053", Codes.ShipmentReleasedEnteredAndReleased },
								{ "SN054", Codes.InbondAuthorizedToMove },
								{ "SN055", Codes.ArrivalOfInbondInbondNumber },
								{ "SN056", Codes.ArrivalOfInbondBillOfLading },
								{ "SN057", Codes.ArrivalOfInbondContainerequipment },
								{ "SN058", Codes.ExportOfInbondInbondNumber },
								{ "SN059", Codes.ExportOfInbondBillOfLading },
								{ "SN060", Codes.ExportOfInbondContainerequipment },
								{ "SN061", Codes.ChangeArrivalOfInbondInbondNumber },
								{ "SN062", Codes.ChangeArrivalOfInbondBillOfLading },
								{ "SN063", Codes.ChangeArrivalInbondContainerequipment },
								{ "SN064", Codes.ChangeExportOfInbondCompleteMovement },
								{ "SN065", Codes.ChangeExportOfInbondBillOfLading },
								{ "SN066", Codes.ChangeExportInbondContainerequipment },
								{ "SN067", Codes.CancelArrivalOfInbondInbondNumber },
								{ "SN068", Codes.CancelArrivalOfInbondBillOfLading },
								{ "SN069", Codes.CancelArrivalInbondContainerequipment },
								{ "SN070", Codes.CancelExportOfInbondInbondNumber },
								{ "SN071", Codes.CancelExportOfInbondBillOfLading },
								{ "SN072", Codes.CancelExportInbondContainerequipment },
								{ "SN073", Codes.Entered },
								{ "SN074", Codes.Released },
								{ "SN075", Codes.CbpHoldRemovedAtInbondDestinationPort },
								{ "SN076", Codes.CbpHoldPlacedAtInbondDestinationPort },
								{ "SN077", Codes.InbondLateIn5Days },
								{ "SN078", Codes.InbondLate },
								{ "SN079", Codes.PendingEligibleGO },
								{ "SN080", Codes.OrderedToGO },
								{ "SN081", Codes.SentToGO },
								{ "SN082", Codes.UsdaMiscellaneousHoldRemovedAtInbondDestinationPort },
								{ "SN083", Codes.UsdaMiscellaneousHoldPlacedAtInbondDestinationPort },
								{ "SN084", Codes.UsdaMiscellaneousHoldPlacedAtPortOfDischarge },
								{ "SN085", Codes.UsdaMiscellaneousHoldRemovedAtPortOfDischarge },
								{ "SN086", Codes.OtherAgencyHoldRemovedAtInbondDestinationPort },
								{ "SN087", Codes.OtherAgencyHoldPlacedAtInbondDestinationPort },
								{ "SN088", Codes.OtherAgencyHoldRemovedAtPortOfDischarge },
								{ "SN089", Codes.OtherAgencyHoldPlacedAtPortOfDischarge },
								{ "SN090", Codes.UsdaIntensiveHoldPlacedAtPortOfDischarge },
								{ "SN091", Codes.UsdaInspectiondocumentReviewHoldPlacedAtPortOfDischarge },
								{ "SN092", Codes.UsdaFumigationHoldPlacedAtPortOfDischarge },
								{ "SN093", Codes.UsdaIntensiveHoldRemovedAtPortOfDischarge },
								{ "SN094", Codes.UsdaInspectiondocumentReviewHoldRemovedAtPortOfDischarge },
								{ "SN095", Codes.UsdaFumigationHoldRemovedAtPortOfDischarge },
								{ "SN096", Codes.UsdaIntensiveHoldPlacedAtPortOfInbondDestination },
								{ "SN097", Codes.UsdaInspectiondocumentReviewPlacedAtPortOfInbondDestination },
								{ "SN098", Codes.UsdaFumigationHoldPlacedAtPortOfInbondDestination },
								{ "SN099", Codes.UsdaIntensiveHoldRemovedAtPortOfInbondDestination },
								{ "SN100", Codes.UsdaInspectiondocumentReviewRemovedAtPortOfInbondDestination },
								{ "SN101", Codes.UsdaFumigationHoldRemovedAtPortOfInbondDestination },
								{ "SN102", Codes.PnOnFile },
								{ "SN103", Codes.FdaWarning },
								{ "SN104", Codes.FdaPnRejected },
								{ "SN105", Codes.EntryNotOnFile },
								{ "SN106", Codes.PnNotOnFile },
								{ "SN107", Codes.BillNotOnFill },
								{ "SN108", Codes.Overage },
								{ "SN109", Codes.Shortage },
								{ "SN110", Codes.OverrideShipmentReleasedEnteredAndReleasedOrInbondAuthorizedToMoveToEntered },
								{ "SN111", Codes.OverrideEnteredToReleased },
								{ "SN112", Codes.EntryDeleted },
								{ "SN113", Codes.EntryProcessingHolds },
								{ "SN114", Codes.EntryProcessingHoldRemoved },
								{ "SN115", Codes.MasterInbondAdvisory },
								{ "SN116", Codes.OverdueExport },
								{ "02", Codes.EntryOnFile },
								{ "SN117", Codes.QpOnFile },
								{ "SN118", Codes.DuplicateInbondRequestViaQpCarriersInbondAccepted },
								{ "SN119", Codes.DriverAccountInformationNeedsToBeUpdatedWithCountryOfDriversCitizenship },
								{ "SN120", Codes.DriverAccountInformationNeedsToBeUpdatedWithAnApprovedWhtiDocument },
								{ "SN510", Codes.SealAddedToEquipmentByCustoms },
								{ "SN511", Codes.SealRemovedFromEquipmentByCustoms },
								{ "SN512", Codes.BillOfLadingAddedToManifestByCustoms },
								{ "SN513", Codes.BillOfLadingRemovedFromManifestByCustoms },
							});
			string result;
			if (!dictionary.TryGetValue(code, out result))
			{
				result = new ZString(code).Right(3);
			}
			return result;
		}
	}
}
