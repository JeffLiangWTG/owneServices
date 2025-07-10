using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class FSNMessageTypeProcessor : AIMMessageTypeProcessor<FSNMessage>
	{
		public FSNMessageTypeProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override bool ProcessMessageCore(FSNMessage message, AsycudaManifestHeader manifest, AsycudaBill bill, EDIMessage mostRecentSentMessage)
		{
			if (message.ComponentIdentifier == Constants.AIMMessageSubTypes.FSN)
			{
				var actionCode = message.ActionCode;
				if (!actionCode.IsEmpty)
				{
					bill.ABL_BillStatus = actionCode;

					var arrivalLine = CreateUpdateArrivalLine(manifest, bill, message.Arrival);
					arrivalLine.ATL_CargoStatus = actionCode;
					arrivalLine.ATL_Quantity = message.NumberOfPieces;

					var transferBill = FindActiveTransferBill(bill, arrivalLine.ArrivalHeader);
					if (transferBill != null)
					{
						switch (actionCode)
						{
							case AIMDispositionCodesHelper.InbondMovementAuthorized:
							case AIMDispositionCodesHelper.CBPLocalTransferAuthorized:
								transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferAccepted;
								transferBill.ATB_CustomsStatus = actionCode;
								break;
							case AIMDispositionCodesHelper.InbondTransferNotAuthorized:
							case AIMDispositionCodesHelper._1G:
								transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferError;
								transferBill.ATB_CustomsStatus = actionCode;
								break;
							case AIMDispositionCodesHelper._83:
							case AIMDispositionCodesHelper._95:
								transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.TransferCancelled;
								transferBill.ATB_CustomsStatus = actionCode;
								break;
							case AIMDispositionCodesHelper._11:
							case AIMDispositionCodesHelper._12:
							case AIMDispositionCodesHelper.P3:
								transferBill.ATB_MessageStatus = AIMTransferStatusCodes.Codes.Arrived;
								transferBill.ATB_CustomsStatus = actionCode;
								break;
							default:
								break;
						}
					}
				}

				return true;
			}
			return false;
		}

		protected override EDIMessage FindMostRecentSentMessage(EDIMessageCollectionNonDependent messages, FSNMessage message, AsycudaManifestHeader manifest, AsycudaBill bill)
		{
			message.CalculatedScheduledArrivalDate = CalculateScheduledArrivalDate(message.CalculatedScheduledArrivalDate, manifest.AMA_E_ARV);
			return base.FindMostRecentSentMessage(messages, message, manifest, bill);
		}

		protected override void AddRowsToPropertiesTable(HtmlTableCreator propertiesTable, FSNMessage message)
		{
			propertiesTable.WriteRow("Message Time", message.EM_SystemCreateTimeUtc);
			propertiesTable.WriteRow("Air Waybill Number", message.MAWBNumber);
			propertiesTable.WriteRow("HAWB Number", message.HAWBNumber);
			var packageTrackingIdentifier = message.PackageTrackingIdentifier;
			if (!packageTrackingIdentifier.IsEmpty)
			{
				propertiesTable.WriteRow("Package Tracking Identifier", packageTrackingIdentifier);
			}
			propertiesTable.WriteRow("Flight Number", message.FlightNumber);
			propertiesTable.WriteRow("Scheduled Arrival Date", message.CalculatedScheduledArrivalDate);
			propertiesTable.WriteRow("Part Arrival Reference", message.PartArrivalReference);
			propertiesTable.WriteRow("Airport Of Arrival", message.AirportOfArrival);
			var dispositionCodeList = AIMDispositionCodesHelper.GetCustomsStatusList(message.Factory);
			var actionCode = message.ActionCode;
			var actionCodeAndDescription = actionCode.IsEmpty ? "" : actionCode + " - " + (dispositionCodeList.GetDescriptionFromCode(actionCode));
			propertiesTable.WriteRow("Action Code", actionCodeAndDescription);
			propertiesTable.WriteRow("Remarks", message.Remarks);
			propertiesTable.WriteRow("Number of Pieces", message.NumberOfPieces);
			propertiesTable.WriteRow("Entry Type", message.EntryType);
			propertiesTable.WriteRow("Entry Number", message.EntryNumber);
		}
	}
}
