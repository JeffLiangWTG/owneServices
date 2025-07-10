namespace Enterprise.Customs.US.AIM.Messaging
{
	public interface ICommonMessageHeader : IAIMMessageHeader
	{
		IAIMAirWaybill AirWaybill { get; }
		IAIMWaybill Waybill { get; }
		IAIMCBPEntryDetail CBPEntryDetail { get; }
		IAIMParty Shipper { get; }
		IAIMParty Consignee { get; }
		IAIMTransfer Transfer { get; }
		IAIMFDAFreightIndicator FDAFreightIndicator { get; }
		IAIMReasonForAmendment ReasonForAmendment { get; }
	}
}
