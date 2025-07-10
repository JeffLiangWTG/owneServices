using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class BlockControlGeneratorForTesting : BlockControlGenerator<ZZZB, ZZZY>
	{
		public BlockControlGeneratorForTesting()
			: this(CBPEDIInterchange.ApplicationCodeForTesting)
		{
		}

		public BlockControlGeneratorForTesting(ZString applicationCode)
			: base(applicationCode)
		{
		}

		protected override void EnsureYBlockCanBeDeserialise(string yBlock)
		{
			if (!yBlock.StartsWith("Z¿ºY"))
			{
				throw new InvalidMessageFormatException("message does not end with a 'Z¿ºY' block");
			}
		}

		protected override void EnsureBBlockCanBeDeserialise(string message)
		{
			if (!message.StartsWith("Z¿ºB"))
			{
				throw new InvalidMessageFormatException("message does not start with a 'Z¿ºB' block");
			}
		}

		protected override MessageBlockDeserialiser MessageBlockDeserialiser => new InputMessageBlockDeserialiser();

		protected override ZString ApplicationIdentifier => ApplicationIdentifierCodeList.DummyForTesting1;
	}
}
