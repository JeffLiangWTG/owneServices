using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class ShipmentEntryStatusListTest : TestCaseWithFactory
	{
		public void TestGetStatusFromNotificationCode()
		{
			AssertEquals(ShipmentEntryStatusList.Codes.ShipmentHold, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN050"));
			AssertEquals(ShipmentEntryStatusList.Codes.ShipmentRemoveHold, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN051"));
			AssertEquals(ShipmentEntryStatusList.Codes.ShipmentSeized, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN052"));
			AssertEquals(ShipmentEntryStatusList.Codes.ShipmentReleasedEnteredAndReleased, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN053"));
			AssertEquals(ShipmentEntryStatusList.Codes.InbondAuthorizedToMove, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN054"));
			AssertEquals(ShipmentEntryStatusList.Codes.ArrivalOfInbondInbondNumber, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN055"));
			AssertEquals(ShipmentEntryStatusList.Codes.ArrivalOfInbondBillOfLading, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN056"));
			AssertEquals(ShipmentEntryStatusList.Codes.ArrivalOfInbondContainerequipment, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN057"));
			AssertEquals(ShipmentEntryStatusList.Codes.ExportOfInbondInbondNumber, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN058"));
			AssertEquals(ShipmentEntryStatusList.Codes.ExportOfInbondBillOfLading, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN059"));
			AssertEquals(ShipmentEntryStatusList.Codes.ExportOfInbondContainerequipment, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN060"));
			AssertEquals(ShipmentEntryStatusList.Codes.ChangeArrivalOfInbondInbondNumber, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN061"));
			AssertEquals(ShipmentEntryStatusList.Codes.ChangeArrivalOfInbondBillOfLading, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN062"));
			AssertEquals(ShipmentEntryStatusList.Codes.ChangeArrivalInbondContainerequipment, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN063"));
			AssertEquals(ShipmentEntryStatusList.Codes.ChangeExportOfInbondCompleteMovement, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN064"));
			AssertEquals(ShipmentEntryStatusList.Codes.ChangeExportOfInbondBillOfLading, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN065"));
			AssertEquals(ShipmentEntryStatusList.Codes.ChangeExportInbondContainerequipment, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN066"));
			AssertEquals(ShipmentEntryStatusList.Codes.CancelArrivalOfInbondInbondNumber, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN067"));
			AssertEquals(ShipmentEntryStatusList.Codes.CancelArrivalOfInbondBillOfLading, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN068"));
			AssertEquals(ShipmentEntryStatusList.Codes.CancelArrivalInbondContainerequipment, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN069"));
			AssertEquals(ShipmentEntryStatusList.Codes.CancelExportOfInbondInbondNumber, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN070"));
			AssertEquals(ShipmentEntryStatusList.Codes.CancelExportOfInbondBillOfLading, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN071"));
			AssertEquals(ShipmentEntryStatusList.Codes.CancelExportInbondContainerequipment, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN072"));
			AssertEquals(ShipmentEntryStatusList.Codes.Entered, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN073"));
			AssertEquals(ShipmentEntryStatusList.Codes.Released, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN074"));
			AssertEquals(ShipmentEntryStatusList.Codes.CbpHoldRemovedAtInbondDestinationPort, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN075"));
			AssertEquals(ShipmentEntryStatusList.Codes.CbpHoldPlacedAtInbondDestinationPort, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN076"));
			AssertEquals(ShipmentEntryStatusList.Codes.InbondLateIn5Days, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN077"));
			AssertEquals(ShipmentEntryStatusList.Codes.InbondLate, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN078"));
			AssertEquals(ShipmentEntryStatusList.Codes.PendingEligibleGO, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN079"));
			AssertEquals(ShipmentEntryStatusList.Codes.OrderedToGO, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN080"));
			AssertEquals(ShipmentEntryStatusList.Codes.SentToGO, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN081"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaMiscellaneousHoldRemovedAtInbondDestinationPort, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN082"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaMiscellaneousHoldPlacedAtInbondDestinationPort, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN083"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaMiscellaneousHoldPlacedAtPortOfDischarge, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN084"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaMiscellaneousHoldRemovedAtPortOfDischarge, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN085"));
			AssertEquals(ShipmentEntryStatusList.Codes.OtherAgencyHoldRemovedAtInbondDestinationPort, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN086"));
			AssertEquals(ShipmentEntryStatusList.Codes.OtherAgencyHoldPlacedAtInbondDestinationPort, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN087"));
			AssertEquals(ShipmentEntryStatusList.Codes.OtherAgencyHoldRemovedAtPortOfDischarge, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN088"));
			AssertEquals(ShipmentEntryStatusList.Codes.OtherAgencyHoldPlacedAtPortOfDischarge, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN089"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaIntensiveHoldPlacedAtPortOfDischarge, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN090"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaInspectiondocumentReviewHoldPlacedAtPortOfDischarge, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN091"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaFumigationHoldPlacedAtPortOfDischarge, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN092"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaIntensiveHoldRemovedAtPortOfDischarge, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN093"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaInspectiondocumentReviewHoldRemovedAtPortOfDischarge, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN094"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaFumigationHoldRemovedAtPortOfDischarge, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN095"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaIntensiveHoldPlacedAtPortOfInbondDestination, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN096"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaInspectiondocumentReviewPlacedAtPortOfInbondDestination, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN097"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaFumigationHoldPlacedAtPortOfInbondDestination, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN098"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaIntensiveHoldRemovedAtPortOfInbondDestination, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN099"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaInspectiondocumentReviewRemovedAtPortOfInbondDestination, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN100"));
			AssertEquals(ShipmentEntryStatusList.Codes.UsdaFumigationHoldRemovedAtPortOfInbondDestination, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN101"));
			AssertEquals(ShipmentEntryStatusList.Codes.PnOnFile, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN102"));
			AssertEquals(ShipmentEntryStatusList.Codes.FdaWarning, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN103"));
			AssertEquals(ShipmentEntryStatusList.Codes.FdaPnRejected, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN104"));
			AssertEquals(ShipmentEntryStatusList.Codes.EntryNotOnFile, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN105"));
			AssertEquals(ShipmentEntryStatusList.Codes.PnNotOnFile, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN106"));
			AssertEquals(ShipmentEntryStatusList.Codes.BillNotOnFill, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN107"));
			AssertEquals(ShipmentEntryStatusList.Codes.Overage, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN108"));
			AssertEquals(ShipmentEntryStatusList.Codes.Shortage, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN109"));
			AssertEquals(ShipmentEntryStatusList.Codes.OverrideShipmentReleasedEnteredAndReleasedOrInbondAuthorizedToMoveToEntered, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN110"));
			AssertEquals(ShipmentEntryStatusList.Codes.OverrideEnteredToReleased, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN111"));
			AssertEquals(ShipmentEntryStatusList.Codes.EntryDeleted, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN112"));
			AssertEquals(ShipmentEntryStatusList.Codes.EntryProcessingHolds, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN113"));
			AssertEquals(ShipmentEntryStatusList.Codes.EntryProcessingHoldRemoved, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN114"));
			AssertEquals(ShipmentEntryStatusList.Codes.MasterInbondAdvisory, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN115"));
			AssertEquals(ShipmentEntryStatusList.Codes.OverdueExport, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN116"));
			AssertEquals(ShipmentEntryStatusList.Codes.EntryOnFile, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "02"));
			AssertEquals(ShipmentEntryStatusList.Codes.QpOnFile, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN117"));
			AssertEquals(ShipmentEntryStatusList.Codes.DuplicateInbondRequestViaQpCarriersInbondAccepted, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN118"));
			AssertEquals(ShipmentEntryStatusList.Codes.DriverAccountInformationNeedsToBeUpdatedWithCountryOfDriversCitizenship, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN119"));
			AssertEquals(ShipmentEntryStatusList.Codes.DriverAccountInformationNeedsToBeUpdatedWithAnApprovedWhtiDocument, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN120"));
			AssertEquals(ShipmentEntryStatusList.Codes.SealAddedToEquipmentByCustoms, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN510"));
			AssertEquals(ShipmentEntryStatusList.Codes.SealRemovedFromEquipmentByCustoms, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN511"));
			AssertEquals(ShipmentEntryStatusList.Codes.BillOfLadingAddedToManifestByCustoms, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN512"));
			AssertEquals(ShipmentEntryStatusList.Codes.BillOfLadingRemovedFromManifestByCustoms, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN513"));
			AssertEquals("200", ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, "SN200"));
			AssertEquals(EntryStatusList.Codes.Error, ShipmentEntryStatusList.GetStatusFromNotificationCode(Factory, EntryStatusList.Codes.Error));
		}
	}
}
