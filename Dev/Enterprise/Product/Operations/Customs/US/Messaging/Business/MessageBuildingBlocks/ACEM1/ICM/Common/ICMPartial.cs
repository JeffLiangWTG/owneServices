namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse, Constants.ACE)]
	public partial class ICMH01 : MessageBlock
	{
	}

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrival, Constants.ACE)]
	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.PaperlessInBondOrVesselArrivalResponse, Constants.ACE)]
	public partial class ICMH02 : MessageBlock
	{
	}
}