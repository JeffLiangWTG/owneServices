using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Messaging;
using Moq;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class ETradeQueryForInspectionClerkMessageGeneratorTest : ETradeBaseMessageGeneratorAbstractTest
	{
		protected override ETradeBaseMessageGenerator ETradeGenerator => generator;

		protected override string ExpectedMessageType => TRMessageTypes.Codes.TRI;

		protected override IMessageSender Sender => provider.Object;

		protected override void SetupData()
		{
			provider = new Mock<IETradeQueryForInspectionClerk>();
			provider.Setup(x => x.Parent).Returns((BusinessObject)Header);
			provider.Setup(x => x.Messages).Returns(Header.Messages);
			provider.Setup(x => x.JobReference).Returns("ETR0000442");

			generator = new ETradeQueryForInspectionClerkMessageGenerator(provider.Object);
		}
		ETradeQueryForInspectionClerkMessageGenerator generator;
		Mock<IETradeQueryForInspectionClerk> provider;
	}
}
