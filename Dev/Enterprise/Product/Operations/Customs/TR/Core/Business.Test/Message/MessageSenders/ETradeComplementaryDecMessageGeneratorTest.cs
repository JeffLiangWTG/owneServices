using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Messaging;
using Moq;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class ETradeComplementaryDecMessageGeneratorTest : ETradeBaseMessageGeneratorAbstractTest
	{
		protected override ETradeBaseMessageGenerator ETradeGenerator => generator;

		protected override string ExpectedMessageType => TRMessageTypes.Codes.TCD;
		protected override string ExpectedApplicationReference => "ETR0000001|12345678901|1";

		protected override IMessageSender Sender => provider.Object;

		protected override void SetupData()
		{
			provider = new Mock<IETradeComplementaryDec>();

			provider.Setup(m => m.Parent).Returns((BusinessObject)Header);
			provider.Setup(m => m.Messages).Returns(Header.Messages);
			provider.Setup(m => m.JobReference).Returns("ETR0000001");

			generator = new ETradeComplementaryDecMessageGenerator(provider.Object);
		}
		ETradeComplementaryDecMessageGenerator generator;
		Mock<IETradeComplementaryDec> provider;
	}
}
