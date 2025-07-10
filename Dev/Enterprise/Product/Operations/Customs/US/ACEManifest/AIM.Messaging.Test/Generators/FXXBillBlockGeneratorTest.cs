using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging.Generators;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	[TestedType(typeof(FXXBillBlockGenerator))]
	class FXXBillBlockGeneratorTest : AIMBlockGeneratorTest
	{
		protected override AIMBlockGenerator GetGenerator() => new FXXBillBlockGenerator(GetMessageHeaderMock().Object);

		protected override ZString MessageType => Constants.AIMMessageSubTypes.FXX;

		protected override Type[] GetExpectedMessageBlockTypes() => new[]
		{
			typeof(AIMAirWaybill),
			typeof(AIMCBPEntryDetailForCancellation),
			typeof(AIMReasonForAmendment)
		};

		Mock<IBillMessageHeader> GetMessageHeaderMock()
		{
			var mock = new Mock<IBillMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(MessageType);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.CBPEntryDetail).Returns(AIMInterfaceTestHelper.GetCBPEntryDetail("86", "12345678901").Object);
			mock.Setup(m => m.AirWaybill).Returns(AIMInterfaceTestHelper.GetAirWaybill("999", "12345675", false).Object);
			mock.Setup(m => m.Waybill).Returns(AIMInterfaceTestHelper.GetWaybillDetails("FRA", 1, "K", 10, "TOYS", "", null).Object);
			mock.Setup(m => m.Shipper).Returns(AIMInterfaceTestHelper.GetParty("TOTLER TOYS", "FRANKFURT", "DE", "12 VIRGINIA COURT", "", "", "").Object);
			mock.Setup(m => m.Consignee).Returns(AIMInterfaceTestHelper.GetParty("TOYS R WE", "NEW YORK", "US", "8812 FUN STREET", "NY", "12345", "123-456-7890").Object);
			mock.Setup(m => m.Transfer).Returns((IAIMTransfer)null);
			mock.Setup(m => m.CPBShipmentDescription).Returns(AIMInterfaceTestHelper.GetCBPShipmentDescription(123000m, "USD", "", "").Object);
			mock.Setup(m => m.FDAFreightIndicator).Returns(AIMInterfaceTestHelper.GetFDAFreightIndicator(true).Object);
			mock.Setup(m => m.ReasonForAmendment).Returns(AIMInterfaceTestHelper.GetReasonForAmendment("KK", "Change info").Object);
			return mock;
		}

		public void TestPopulateMessage()
		{
			var messageBlocks = GetGenerator().Generate();
			var airWayBill = messageBlocks.FirstOrDefault(x => x is AIMAirWaybill);
			var cbpEntryDetailForCancellation = messageBlocks.FirstOrDefault(x => x is AIMCBPEntryDetailForCancellation);
			var reasonForAmendment = messageBlocks.FirstOrDefault(x => x is AIMReasonForAmendment);
			CombineAssertions(() =>
			{
				AssertEquals(@"-------------AIMAirWaybill--------------
AirWaybillPrefix (3-3AN) :999
AWBSerialNumber (8-8N)   :12345675

", airWayBill.Serialise());
				AssertEquals(@"----AIMCBPEntryDetailForCancellation----

", cbpEntryDetailForCancellation.Serialise());
				AssertEquals(@"---------AIMReasonForAmendment----------
AmendmentCode (2-2N)          :??
AmendmentExplanation (1-20AN) :CHANGEINFO

", reasonForAmendment.Serialise());
			});
		}
	}
}
