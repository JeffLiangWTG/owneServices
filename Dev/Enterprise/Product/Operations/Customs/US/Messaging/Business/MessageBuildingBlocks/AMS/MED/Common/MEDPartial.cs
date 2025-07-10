namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common
{
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEdit, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class MEDK01 : MessageBlock
	{
	}
}
