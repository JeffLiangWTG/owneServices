namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common
{
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.GeneralOrderStatusResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.BaplieResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBondResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class TARW01 : MessageBlock { }

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.GeneralOrderStatusResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.SubsequentInBondResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestCreateResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Constants.ACE)]
	public partial class TARW02 : MessageBlock
	{
	}
}
