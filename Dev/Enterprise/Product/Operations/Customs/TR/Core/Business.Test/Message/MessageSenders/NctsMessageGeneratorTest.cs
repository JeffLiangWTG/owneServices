using CargoWise.EntityFramework;
using Enterprise.Customs.TR.Messaging;
using Moq;

namespace Enterprise.Customs.TR.Business.Testing
{
	sealed class NctsMessageGeneratorTest : TRBaseMessageGeneratorAbstractTest<NCTSMessage>
	{
		protected override string ExpectedMessageType => TRMessageTypes.Codes.TRN;

		protected override string ExpectedApplicationReference => "NCT00050102";

		protected override IMessageSender Sender => provider.Object;

		protected override TRBaseMessageGenerator<NCTSMessage> Generator => generator;

		protected override void SetupData()
		{
			var header = Factory.New<Integration.Customs.TR.ICusInBondHeader>();
			header.BH_JobReference = "NCT00050102";
			header.BH_HeaderType = "D";

			provider = new Mock<INCTSHeaderForTest>();
			provider.Setup(x => x.Parent).Returns((BusinessObject)header);
			provider.Setup(x => x.Messages).Returns(header.Messages);
			provider.Setup(x => x.JobReference).Returns(header.BH_JobReference);
			generator = new NctsMessageGenerator(provider.Object);
		}
		NctsMessageGenerator generator;
		Mock<INCTSHeaderForTest> provider;
	}

	public interface INCTSHeaderForTest : CargoWise.Customs.TR.MessageContracts.Interfaces.NCTS.INCTSHeader, IMessageSender
	{
	}
}
