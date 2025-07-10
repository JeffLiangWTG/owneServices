namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IBaseBillOfLading
	{
		IManifestMessageAttachee MessageAttachee { get; }
	}
}
