namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common
{
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendment, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class MADA01 : MessageBlock
	{
	}
}
