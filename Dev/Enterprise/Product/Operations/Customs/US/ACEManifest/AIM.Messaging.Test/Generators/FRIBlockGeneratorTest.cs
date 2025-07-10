using System;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging.Generators;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	[TestedType(typeof(FRIBlockGenerator))]
	class FRIBlockGeneratorTest : AIMBlockGeneratorTest
	{
		protected override AIMBlockGenerator GetGenerator() => new FRIBlockGenerator(GetMessageHeaderMock().Object);
		protected override ZString MessageType => Constants.AIMMessageSubTypes.FRI;

		protected override Type[] GetExpectedMessageBlockTypes() => new[]
		{
			typeof(AIMCargoControlLocation),
			typeof(AIMAirWaybill),
			typeof(AIMWaybill),
			typeof(AIMArrival),
			typeof(AIMAgent),
			typeof(AIMShipper),
			typeof(AIMConsignee)
		};

		Mock<IManifestMessageHeader> GetMessageHeaderMock()
		{
			var mock = new Mock<IManifestMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(MessageType);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.CargoControlLine).Returns(AIMInterfaceTestHelper.GetCargoControlLocation("JFK", "XYZ").Object);
			mock.Setup(m => m.CBPEntryDetail).Returns((IAIMCBPEntryDetail)null);
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345675", false).Object);
			mock.Setup(m => m.Waybill).Returns(AIMInterfaceTestHelper.GetWaybillDetails("FRA", 1, "K", 10, "TOYS", "", null).Object);
			mock.Setup(m => m.Arrival).Returns(AIMInterfaceTestHelper.GetArrival("XYZ1234A", new ZDate(2019, 10, 25), "", false, 0, "", 0).Object);
			mock.Setup(m => m.Agent).Returns(AIMInterfaceTestHelper.GetAgent("PP").Object);
			mock.Setup(m => m.Shipper).Returns(AIMInterfaceTestHelper.GetParty("TOTLER TOYS", "FRANKFURT", "DE", "12 VIRGINIA COURT", "", "", "").Object);
			mock.Setup(m => m.Consignee).Returns(AIMInterfaceTestHelper.GetParty("TOYS R WE", "NEW YORK", "US", "8812 FUN STREET", "NY", "12345", "123-456-7890").Object);
			mock.Setup(m => m.Transfer).Returns((IAIMTransfer)null);
			mock.Setup(m => m.FDAFreightIndicator).Returns((IAIMFDAFreightIndicator)null);
			mock.Setup(m => m.ReasonForAmendment).Returns((IAIMReasonForAmendment)null);
			return mock;
		}
	}
}
