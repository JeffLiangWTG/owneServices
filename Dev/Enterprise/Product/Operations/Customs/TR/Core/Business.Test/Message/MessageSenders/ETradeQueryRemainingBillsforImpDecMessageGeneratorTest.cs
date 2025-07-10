using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Messaging;
using Moq;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class ETradeQueryRemainingBillsforImpDecMessageGeneratorTest : ETradeBaseMessageGeneratorAbstractTest
	{
		protected override ETradeBaseMessageGenerator ETradeGenerator => generator;

		protected override string ExpectedMessageType => TRMessageTypes.Codes.TRB;

		protected override string ExpectedApplicationReference => "ETR0000442";

		protected override IMessageSender Sender => provider.Object;

		protected override void SetupData()
		{
			provider = new Mock<IETradeQueryRemainingBillsforImpDec>();
			provider.Setup(x => x.Parent).Returns((BusinessObject)Header);
			provider.Setup(x => x.Messages).Returns(Header.Messages);
			provider.Setup(x => x.JobReference).Returns("ETR0000442");

			generator = new ETradeQueryRemainingBillsforImpDecMessageGenerator(provider.Object);
		}
		ETradeQueryRemainingBillsforImpDecMessageGenerator generator;
		Mock<IETradeQueryRemainingBillsforImpDec> provider;
	}
}
