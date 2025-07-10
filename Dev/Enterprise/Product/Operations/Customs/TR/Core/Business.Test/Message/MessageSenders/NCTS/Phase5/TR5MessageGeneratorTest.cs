using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(TR5MessageGenerator))]
	sealed class TR5MessageGeneratorTest : Phase5NcstMessageGeneratorAbstractTest
	{
		protected override string ExpectedMessageType => TRMessageTypes.Codes.TR5;

		protected override Phase5NcstMessageGenerator CreateGenerator(Mock<INCTSHeaderForTest> provider) => new TR5MessageGenerator(provider.Object);
	}
}
