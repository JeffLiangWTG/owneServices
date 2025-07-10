using System;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging.Generators;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	[TestedType(typeof(FDMBlockGenerator))]
	class FDMBlockGeneratorTest : AIMBlockGeneratorTest
	{
		protected override AIMBlockGenerator GetGenerator() => new FDMBlockGenerator(GetMessageHeaderMock().Object);

		protected override ZString MessageType => Constants.AIMMessageSubTypes.FDM;

		protected override Type[] GetExpectedMessageBlockTypes() => new[] { typeof(AIMDeparture) };

		Mock<IDepartureMessageHeader> GetMessageHeaderMock()
		{
			var mock = new Mock<IDepartureMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(MessageType);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.Departure).Returns(AIMInterfaceTestHelper.GetDeparture("QT001", new ZDate(2022, 02, 22), new ZDate(2022, 02, 21), "090603", "SED", "QT002").Object);
			return mock;
		}
	}
}
