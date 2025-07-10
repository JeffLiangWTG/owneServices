using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class AIMMessageHeaderForTransfer : IManifestMessageHeader
	{
		public AIMMessageHeaderForTransfer(AsycudaTransferBill transferBill, ZString messageType, bool isCancel)
		{
			this.transferBill = Argument.NotNull(transferBill, nameof(transferBill));
			this.messageType = messageType;
			this.isCancel = isCancel;

			bill = transferBill.Bill;
			arrivalHeader = transferBill.TransferHeader.ArrivalHeader;
			manifestHeader = arrivalHeader.Header;
		}

		readonly AsycudaTransferBill transferBill;
		readonly ZString messageType;
		readonly bool isCancel;
		readonly AsycudaBill bill;
		readonly AsycudaArrivalHeader arrivalHeader;
		readonly AsycudaManifestHeader manifestHeader;

		public ZString MessageType => messageType;

		public ZString Reference => bill.ABL_BillNumber;

		public IAIMCargoControlLocation CargoControlLine => cargoControlLine ?? (cargoControlLine = new AIMCargoControlLocation(manifestHeader));
		IAIMCargoControlLocation cargoControlLine;

		public IAIMAirWaybill AirWaybill => airWaybill ?? (airWaybill = GetNewAirWaybill());
		IAIMAirWaybill airWaybill;

		public IAIMWaybill Waybill => waybill ?? (waybill = GetNewWaybill());
		IAIMWaybill waybill;

		public IAIMArrival Arrival => arrival ?? (arrival = new AIMArrival(arrivalHeader));
		IAIMArrival arrival;

		public IAIMCBPEntryDetail CBPEntryDetail => CachedValueHelper.GetValue(ref cbpEntryDetail, () => !bill.CustomsEntryNumberType.IsEmpty ? new AIMCBPEntryDetail(bill) : null);
		CachedValue<IAIMCBPEntryDetail> cbpEntryDetail;

		public IAIMAgent Agent => null;

		public IAIMParty Shipper => null;

		public IAIMParty Consignee => null;

		public IAIMTransfer Transfer => transfer ?? (transfer = new AIMTransfer(isCancel ? null : transferBill));
		IAIMTransfer transfer;

		public IAIMFDAFreightIndicator FDAFreightIndicator => null;

		public IAIMReasonForAmendment ReasonForAmendment => reasonForAmendment ?? (reasonForAmendment = new AIMReasonForAmendment("19", ""));
		IAIMReasonForAmendment reasonForAmendment;

		IAIMAirWaybill GetNewAirWaybill()
		{
			if (bill.IsChildMasterBill)
			{
				return new AIMAirWaybillForManifestMsg(manifestHeader, new AdditionalMessageInformation(manifestHeader) { AM_IsConsolidation = true });
			}
			else
			{
				return new AIMAirWaybill(bill);
			}
		}

		IAIMWaybill GetNewWaybill()
		{
			if (bill.IsChildMasterBill)
			{
				return new AIMWaybillForManifestMsg(manifestHeader, new AdditionalMessageInformation(manifestHeader) { AM_IsConsolidation = true });
			}
			else
			{
				return new AIMWaybill(bill);
			}
		}
	}
}
