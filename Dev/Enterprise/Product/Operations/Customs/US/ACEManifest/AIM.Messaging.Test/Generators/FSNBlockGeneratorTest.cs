using System;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging.Generators;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	[TestedType(typeof(FSNBlockGenerator))]
	class FSNBlockGeneratorTest : AIMBlockGeneratorTest
	{
		protected override AIMBlockGenerator GetGenerator() => new FSNBlockGenerator(GetMessageHeaderMock().Object);

		protected override ZString MessageType => Constants.AIMMessageSubTypes.FSN;

		protected override Type[] GetExpectedMessageBlockTypes() => new[]
		{
			typeof(AIMCargoControlLocation),
			typeof(AIMAirWaybill),
			typeof(AIMArrival),
			typeof(AIMAirlineStatusNotification)
		};

		Mock<IArrivalMessageHeader> GetMessageHeaderMock()
		{
			var mock = new Mock<IArrivalMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(MessageType);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.CargoControlLine).Returns(AIMInterfaceTestHelper.GetCargoControlLocation("JFK", "XYZ").Object);
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345675", false).Object);
			mock.Setup(m => m.Arrival).Returns(AIMInterfaceTestHelper.GetArrival("XYZ1234A", new ZDate(2019, 10, 25), "", false, 0, "", 0).Object);
			mock.Setup(m => m.AirlineStatusNotification).Returns(AIMInterfaceTestHelper.GetAirlineStatusNotification("SC", "Delay").Object);
			return mock;
		}
	}
}
