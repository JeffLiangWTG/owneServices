#if DEBUG
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	class MessageProcessorFactoryForTesting : MessageProcessorFactory
	{
		public MessageProcessorFactoryForTesting(LoggingInformation logger, ZString applicationCode)
			: base(logger, applicationCode, applicationCode + " Message Processor")
		{
		}
	}

	sealed class HookedMessageProcessorFactoryForTesting : MessageProcessorFactoryForTesting, IMessageProcessorProvider
	{
		public HookedMessageProcessorFactoryForTesting(int maximumTopLevelMessageBlocksPerProcessor)
			: base(new LoggingInformation(), CBPEDIInterchange.ApplicationCodeForTesting)
		{
			provider = this;
			hookedProcessor = new HookedProcessor(maximumTopLevelMessageBlocksPerProcessor);
		}

		IProcessor IMessageProcessorProvider.GetProcessor(string applicationCodes, string applicationIdentifier, MessageBlock topLevelBlock)
		{
			return topLevelBlock is ZZZC2 ? hookedProcessor : null;
		}
		internal HookedProcessor hookedProcessor;

		[ApplicationIdentifier(ApplicationIdentifierCodeList.DummyForTesting2, CBPEDIInterchange.ApplicationCodeForTesting)]
		[TopLevel(typeof(ZZZC2))]
		sealed internal class HookedProcessor : ProcessorTestClass<ZZZA, ZZZB2, ZZZY2>
		{
			public HookedProcessor()
				: this(1)
			{
			}

			public HookedProcessor(int maximumTopLevelMessageBlocksPerProcessor)
			{
				this.maximumTopLevelMessageBlocksPerProcessor = maximumTopLevelMessageBlocksPerProcessor;
			}

			public override int MaximumTopLevelMessageBlocksPerProcessor => maximumTopLevelMessageBlocksPerProcessor;

			public override void Process()
			{
				base.Process();
				ProcessCount++;
			}

			public int maximumTopLevelMessageBlocksPerProcessor;
			public int ProcessCount;
		}
	}
}
#endif
