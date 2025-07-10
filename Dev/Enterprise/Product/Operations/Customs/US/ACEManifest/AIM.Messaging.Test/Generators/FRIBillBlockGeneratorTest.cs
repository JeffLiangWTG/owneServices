using System;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging.Generators;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	[TestedType(typeof(FRIBillBlockGenerator))]
	class FRIBillBlockGeneratorTest : AIMBlockGeneratorTest
	{
		protected override AIMBlockGenerator GetGenerator() => new FRIBillBlockGenerator(GetMessageHeaderMock().Object);
		protected override ZString MessageType => Constants.AIMMessageSubTypes.FRI;

		protected override Type[] GetExpectedMessageBlockTypes() => new[]
		{
			typeof(AIMAirWaybill),
			typeof(AIMWaybill),
			typeof(AIMShipper),
			typeof(AIMConsignee),
			typeof(AIMCBPShipmentDescription),
			typeof(AIMFDAFreightIndicator),
		};

		Mock<IBillMessageHeader> GetMessageHeaderMock()
		{
			var mock = new Mock<IBillMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(MessageType);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.CBPEntryDetail).Returns((IAIMCBPEntryDetail)null);
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345675", false).Object);
			mock.Setup(m => m.Waybill).Returns(AIMInterfaceTestHelper.GetWaybillDetails("FRA", 1, "K", 10, "TOYS", "", null).Object);
			mock.Setup(m => m.Shipper).Returns(AIMInterfaceTestHelper.GetParty("TOTLER TOYS", "FRANKFURT", "DE", "12 VIRGINIA COURT", "", "", "").Object);
			mock.Setup(m => m.Consignee).Returns(AIMInterfaceTestHelper.GetParty("TOYS R WE", "NEW YORK", "US", "8812 FUN STREET", "NY", "12345", "123-456-7890").Object);
			mock.Setup(m => m.Transfer).Returns((IAIMTransfer)null);
			mock.Setup(m => m.CPBShipmentDescription).Returns(AIMInterfaceTestHelper.GetCBPShipmentDescription(123000m, "USD", "", "").Object);
			mock.Setup(m => m.FDAFreightIndicator).Returns(AIMInterfaceTestHelper.GetFDAFreightIndicator(true).Object);
			return mock;
		}
	}
}
