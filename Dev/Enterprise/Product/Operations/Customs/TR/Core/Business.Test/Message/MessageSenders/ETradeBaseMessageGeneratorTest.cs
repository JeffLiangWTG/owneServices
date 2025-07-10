using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Moq;

namespace Enterprise.Customs.TR.Business.Testing
{
	abstract class ETradeBaseMessageGeneratorAbstractTest : TRBaseMessageGeneratorAbstractTest<ETradeEDIMessage>
	{
		protected sealed override TRBaseMessageGenerator<ETradeEDIMessage> Generator => ETradeGenerator;
		protected abstract ETradeBaseMessageGenerator ETradeGenerator { get; }

		protected override string ExpectedApplicationReference => "ETR0000442|TRM";

		protected Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
					Header.AMA_JobReference = "ETR0000442";
				}
				return header;
			}
		}
		Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader header;
	}

	sealed class ETradeBaseMessageGeneratorTest : ETradeBaseMessageGeneratorAbstractTest
	{
		protected override IMessageSender Sender => sender.Object;
		protected override ETradeBaseMessageGenerator ETradeGenerator => generator;

		protected override string ExpectedMessageType => "EMT";

		protected override void SetupData()
		{
			sender = new Mock<IMessageSender>();
			sender.Setup(x => x.Parent).Returns((BusinessObject)Header);
			sender.Setup(x => x.Messages).Returns(Header.Messages);
			sender.Setup(x => x.JobReference).Returns(Header.AMA_JobReference);

			generator = new DummyETradeGenerator(sender.Object, ExpectedMessageType);
		}
		DummyETradeGenerator generator;
		Mock<IMessageSender> sender;

		class DummyETradeGenerator : ETradeBaseMessageGenerator
		{
			public DummyETradeGenerator(IMessageSender sender, ZString messageType) : base(sender)
			{
				MessageType = messageType;
			}

			public override ZString MessageType { get; }

			protected override ZString MessageText => "ETrade Base Generator Message Test";
		}
	}
}
