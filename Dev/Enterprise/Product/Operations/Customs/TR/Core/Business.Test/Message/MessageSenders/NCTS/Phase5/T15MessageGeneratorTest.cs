using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Testing
{
	[TestedType(typeof(T15MessageGenerator))]
	sealed class T15MessageGeneratorTest : Phase5NcstMessageGeneratorAbstractTest
	{
		protected override string ExpectedMessageType => TRMessageTypes.Codes.T15;

		protected override Phase5NcstMessageGenerator CreateGenerator(Mock<INCTSHeaderForTest> provider) => new T15MessageGenerator(provider.Object);
	}
}
