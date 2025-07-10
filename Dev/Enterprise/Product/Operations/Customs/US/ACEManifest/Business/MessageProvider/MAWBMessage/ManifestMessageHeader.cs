using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class ManifestMessageHeader : IManifestMessageHeader
	{
		public ManifestMessageHeader(AsycudaManifestHeader header, AsycudaBill bill, ZString messageType, AdditionalMessageInformation additionalMessageInformation)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.bill = Argument.NotNull(bill, nameof(bill));
			this.additionalMessageInformation = Argument.NotNull(additionalMessageInformation, nameof(additionalMessageInformation));
			this.messageType = messageType;
		}

		readonly AsycudaManifestHeader header;
		readonly AsycudaBill bill;
		readonly AdditionalMessageInformation additionalMessageInformation;
		readonly ZString messageType;

		public ZString MessageType => messageType;
		public ZString Reference => header.AMA_MasterBill;

		public IAIMCargoControlLocation CargoControlLine => cargoControlLine ?? (cargoControlLine = new AIMCargoControlLocation(header));
		IAIMCargoControlLocation cargoControlLine;

		public IAIMAirWaybill AirWaybill => airWaybill ?? (airWaybill = new AIMAirWaybillForManifestMsg(header, additionalMessageInformation));
		IAIMAirWaybill airWaybill;

		public IAIMArrival Arrival => arrival ?? (arrival = new AIMArrivalForManifestMsg(additionalMessageInformation));
		IAIMArrival arrival;

		public IAIMAgent Agent => agent ?? (agent = new AIMAgent(additionalMessageInformation));
		IAIMAgent agent;

		public IAIMWaybill Waybill => waybill ?? (waybill = new AIMWaybillForManifestMsg(header, additionalMessageInformation));
		IAIMWaybill waybill;

		public IAIMParty Shipper => CachedValueHelper.GetValue(ref shipper, () => (bill.Shipper != null || !bill.ABL_ShipperName.IsEmpty) ? new AIMShipper(bill) : null);
		CachedValue<IAIMParty> shipper;

		public IAIMParty Consignee => CachedValueHelper.GetValue(ref consignee, () => (bill.Consignee != null || !bill.ABL_ConsigneeName.IsEmpty) ? new AIMConsignee(bill) : null);
		CachedValue<IAIMParty> consignee;

		public IAIMCBPEntryDetail CBPEntryDetail => null;

		public IAIMTransfer Transfer => null;

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
