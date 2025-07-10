using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	internal static class IPGADispositionProviderExtensionMethods
	{
		public static IPGADispositionProvider GetFirstPGADispositionBlock(this MQEDIMessage message)
		{
			return message.MessageBlock.MessageBlocks.OfType<IPGADispositionProvider>().FirstOrDefault(x => x.DispositionDateTime.IsValid);
		}

		public static bool HasEarlierDispositionDateTime(this IPGADispositionProvider dispositionBlock, IEnumerable<MQEDIMessage> existingMessages)
		{
			var existingLatestDispositionTime = (from MQEDIMessage message in existingMessages
												 let existingReleaseBlock = message.GetFirstPGADispositionBlock()
												 where existingReleaseBlock != null
												 orderby existingReleaseBlock.DispositionDateTime descending
												 select existingReleaseBlock.DispositionDateTime).FirstOrDefault();

			return existingLatestDispositionTime.IsValid && dispositionBlock.DispositionDateTime < existingLatestDispositionTime;
		}
	}
}
