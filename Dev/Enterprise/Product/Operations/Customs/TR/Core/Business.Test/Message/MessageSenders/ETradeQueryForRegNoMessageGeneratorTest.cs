using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Messaging;
using Moq;

namespace Enterprise.Customs.TR.Business.Testing
{
	class ETradeQueryForRegNoMessageGeneratorTest : ETradeBaseMessageGeneratorAbstractTest
	{
		protected override ETradeBaseMessageGenerator ETradeGenerator => generator;

		protected override string ExpectedMessageType => TRMessageTypes.Codes.TRQ;

		protected override IMessageSender Sender => provider.Object;

		protected override void SetupData()
		{
			provider = new Mock<IETradeQueryForRegNo>();
			provider.Setup(x => x.Parent).Returns((BusinessObject)Header);
			provider.Setup(x => x.Messages).Returns(Header.Messages);
			provider.Setup(x => x.JobReference).Returns(Header.AMA_JobReference);

			generator = new ETradeQueryForRegNoMessageGenerator(provider.Object);
		}
		ETradeQueryForRegNoMessageGenerator generator;
		Mock<IETradeQueryForRegNo> provider;
	}
}
