using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestsSubclassesOf(typeof(Phase5NcstMessageGenerator))]
	abstract class Phase5NcstMessageGeneratorAbstractTest : TRBaseMessageGeneratorAbstractTest<NCTSMessage>
	{
		protected override TRBaseMessageGenerator<NCTSMessage> Generator => generator;

		protected override ZGuid ExpectedEMGP => currentUser.PK;

		protected override IMessageSender Sender => provider.Object;

		protected override string ExpectedApplicationReference => "NCT00050101";

		protected override void SetupData()
		{
			var header = Factory.New<Integration.Customs.TR.ICusInBondHeader>();
			header.BH_JobReference = "NCT00050101";
			header.BH_HeaderType = "D";

			provider = new Mock<INCTSHeaderForTest>();
			provider.Setup(x => x.Parent).Returns((BusinessObject)header);
			provider.Setup(x => x.Messages).Returns(header.Messages);
			provider.Setup(x => x.JobReference).Returns(header.BH_JobReference);
			generator = CreateGenerator(provider);
		}

		protected abstract Phase5NcstMessageGenerator CreateGenerator(Mock<INCTSHeaderForTest> provider);

		protected Phase5NcstMessageGenerator generator;
		protected Mock<INCTSHeaderForTest> provider;
	}
}
