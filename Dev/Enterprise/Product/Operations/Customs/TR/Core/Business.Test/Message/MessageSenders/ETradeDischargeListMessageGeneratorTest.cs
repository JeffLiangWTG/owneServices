using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Messaging;
using Moq;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class ETradeDischargeListMessageGeneratorTest : ETradeBaseMessageGeneratorAbstractTest
	{
		protected override ETradeBaseMessageGenerator ETradeGenerator => generator;

		protected override string ExpectedMessageType => TRMessageTypes.Codes.TRD;

		protected override string ExpectedApplicationReference => "ETR0000442|12345678901|1";

		protected override IMessageSender Sender => provider.Object;

		protected override void SetupData()
		{
			provider = new Mock<IDischargeList>();

			provider.Setup(x => x.Parent).Returns((BusinessObject)Header);
			provider.Setup(x => x.Messages).Returns(Header.Messages);
			provider.Setup(x => x.JobReference).Returns("ETR0000442");

			generator = new ETradeDischargeListMessageGenerator(provider.Object);
		}
		ETradeDischargeListMessageGenerator generator;
		Mock<IDischargeList> provider;
	}
}
