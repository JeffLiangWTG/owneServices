namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransfer, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransfer, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class PTTT01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransfer, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransfer, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PermitToTransferResponse, Enterprise.Customs.US.Messaging.Business.CBPEDIInterchange.ApplicationCodes.AMS)]
	public partial class PTTT02 : MessageBlock
	{
	}
}