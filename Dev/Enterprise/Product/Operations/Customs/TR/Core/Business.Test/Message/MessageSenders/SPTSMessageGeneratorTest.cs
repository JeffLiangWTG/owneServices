using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Messaging;
using Moq;

namespace Enterprise.Customs.TR.Business.Testing
{
	class SPTSMessageGeneratorTest : TRBaseMessageGeneratorAbstractTest<SPTSMessage>
	{
		protected override string ExpectedMessageType => TRMessageTypes.Codes.TSP;

		protected override string ExpectedApplicationReference => "ULU-SPTS0001|12345678901";

		protected override IMessageSender Sender => provider.Object;

		protected override TRBaseMessageGenerator<SPTSMessage> Generator => generator;

		protected override void SetupData()
		{
			var header = Factory.New<Integration.Customs.TR.ICusInBondSPTSHeader>();
			header.BH_JobReference = "ULU-SPTS0001";

			provider = new Mock<ISPTS>();
			provider.Setup(x => x.Parent).Returns((BusinessObject)header);
			provider.Setup(x => x.Messages).Returns(header.Messages);
			provider.Setup(x => x.JobReference).Returns(header.BH_JobReference);
			generator = new SPTSMessageGenerator(provider.Object);
		}
		SPTSMessageGenerator generator;
		Mock<ISPTS> provider;
	}
}
