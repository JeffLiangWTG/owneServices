using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Messaging;
using Moq;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class ETradeQueryForInspectionLineMessageGeneratorTest : ETradeBaseMessageGeneratorAbstractTest
	{
		protected override ETradeBaseMessageGenerator ETradeGenerator => generator;

		protected override string ExpectedMessageType => TRMessageTypes.Codes.TRL;

		protected override IMessageSender Sender => provider.Object;

		protected override void SetupData()
		{
			provider = new Mock<IETradeQueryForInspectionLine>();
			provider.Setup(x => x.Parent).Returns((BusinessObject)Header);
			provider.Setup(x => x.Messages).Returns(Header.Messages);
			provider.Setup(x => x.JobReference).Returns("ETR0000442");

			generator = new ETradeQueryForInspectionLineMessageGenerator(provider.Object);
		}
		ETradeQueryForInspectionLineMessageGenerator generator;
		Mock<IETradeQueryForInspectionLine> provider;
	}
}
