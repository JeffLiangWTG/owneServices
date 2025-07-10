using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class BillMessageHeader : IBillMessageHeader
	{
		public BillMessageHeader(AsycudaBill bill, ZString messageType)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.messageType = messageType;
		}

		protected readonly AsycudaBill bill;
		readonly ZString messageType;

		public ZString MessageType => messageType;
		public ZString Reference => bill.ABL_BillNumber;

		public IAIMAirWaybill AirWaybill => airWaybill ?? (airWaybill = new AIMAirWaybill(bill));
		IAIMAirWaybill airWaybill;

		public IAIMWaybill Waybill => waybill ?? (waybill = new AIMWaybill(bill));
		IAIMWaybill waybill;

		public IAIMParty Shipper => CachedValueHelper.GetValue(ref shipper, () => (bill.Shipper != null || !bill.ABL_ShipperName.IsEmpty) ? new AIMShipper(bill) : null);
		CachedValue<IAIMParty> shipper;

		public IAIMParty Consignee => CachedValueHelper.GetValue(ref consignee, () => (bill.Consignee != null || !bill.ABL_ConsigneeName.IsEmpty) ? new AIMConsignee(bill) : null);
		CachedValue<IAIMParty> consignee;

		public IAIMCBPEntryDetail CBPEntryDetail => CachedValueHelper.GetValue(ref cbpEntryDetail, () => !bill.CustomsEntryNumberType.IsEmpty ? new AIMCBPEntryDetail(bill) : null);
		CachedValue<IAIMCBPEntryDetail> cbpEntryDetail;

		public IAIMTransfer Transfer => null;

		public IAIMCBPShipmentDescription CPBShipmentDescription => cpbShipmentDescription ?? (cpbShipmentDescription = AIMCBPShipmentDescription.CreateIfHasGoodsValue(bill));
		IAIMCBPShipmentDescription cpbShipmentDescription;

		public IAIMFDAFreightIndicator FDAFreightIndicator => fdaFreightIndicator ?? (fdaFreightIndicator = new AIMFDAFreightIndicator(bill));
		IAIMFDAFreightIndicator fdaFreightIndicator;

		public IAIMReasonForAmendment ReasonForAmendment => reasonForAmendment ?? (reasonForAmendment = new AIMReasonForAmendment());
		IAIMReasonForAmendment reasonForAmendment;

		public void SetAmendmentReason(string amendmentCode, string amendmentExplanation)
		{
			((AIMReasonForAmendment)ReasonForAmendment).SetAmendmentReason(amendmentCode, amendmentExplanation);
		}
	}
}
