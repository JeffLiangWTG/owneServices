using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.UY.Manifest.Business
{
	public class UYBranchMessageProcessor : BranchCustomsMessageProcessor
	{
		public UYBranchMessageProcessor() : base(new ZString[] { EDIMessage.ApplicationCodes.UYCustoms }, null)
		{
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message) => new UYManifestMessageProcessor<UYMessage>(Logger);
	}
}
