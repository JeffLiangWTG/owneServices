using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging.Generators;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	[TestedType(typeof(FSQBlockGenerator))]
	class FSQBlockGeneratorTest : AIMBlockGeneratorTest
	{
		protected override AIMBlockGenerator GetGenerator() => new FSQBlockGenerator(GetMessageHeaderMock().Object);

		protected override ZString MessageType => Constants.AIMMessageSubTypes.FSQ;

		protected override Type[] GetExpectedMessageBlockTypes() => new[]
		{
			typeof(AIMCargoControlLocation),
			typeof(AIMAirWaybill_FSQ_FSC),
			typeof(AIMFreightStatusQuery)
		};

		Mock<IFreightStatusQueryMessageHeader> GetMessageHeaderMock()
		{
			var mock = new Mock<IFreightStatusQueryMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(MessageType);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.CargoControlLine).Returns(AIMInterfaceTestHelper.GetCargoControlLocation("AP", "001").Object);
			var airWayBillMock = AIMInterfaceTestHelper.GetAirWaybill("999", "12345675", false);
			airWayBillMock.Setup(m => m.PackageTrackingIdentifier).Returns("PT");
			airWayBillMock.Setup(m => m.PartArrivalReference).Returns("PA11");
			mock.Setup(m => m.AirWaybill).Returns(airWayBillMock.Object);
			mock.Setup(m => m.FreightStatusQuery).Returns(AIMInterfaceTestHelper.GetFreightStatusQuery("01").Object);
			return mock;
		}

		public void TestPopulateCargoControlLocation()
		{
			var iAIMCargoControlLocation = AIMInterfaceTestHelper.GetCargoControlLocation("AP", "001").Object;
			var generator = new FSQBlockGeneratorTestHelper(GetMessageHeaderMock().Object);
			var testBlock = (AIMCargoControlLocation)generator.GetAIMCargoControlLocation(iAIMCargoControlLocation, "");
			AssertEquals("AirportOfArrival", "AP", testBlock.AirportOfArrival);
			AssertEquals("CargoTerminalOperator", "001", testBlock.CargoTerminalOperator);

			var testBlock2 = generator.GetAIMCargoControlLocation(iAIMCargoControlLocation, "HN001");
			AssertNull("No AIMCBPEntryDetail block", testBlock2);
		}

		public void TestPopulateAirWayBill()
		{
			var airWayBillMock = AIMInterfaceTestHelper.GetAirWaybill("999", "12345675", false);
			airWayBillMock.Setup(m => m.HAWBNumber).Returns("HB");
			airWayBillMock.Setup(m => m.PackageTrackingIdentifier).Returns("PT");
			airWayBillMock.Setup(m => m.PartArrivalReference).Returns("PA11");
			var iAIMAirWaybill = airWayBillMock.Object;
			var generator = new FSQBlockGeneratorTestHelper(GetMessageHeaderMock().Object);
			var testBlock = (AIMAirWaybill_FSQ_FSC)generator.GetAIMAirWayBill(iAIMAirWaybill);
			AssertEquals("AirWaybillPrefix", "999", testBlock.AirWaybillPrefix);
			AssertEquals("AWBSerialNumber", "12345675", testBlock.AWBSerialNumber);
			AssertEquals("HAWBNumber", "HB", testBlock.HAWBNumber);
			AssertEquals("PartArrivalReference", "PA11", testBlock.PartArrivalReference);
		}

		class FSQBlockGeneratorTestHelper : FSQBlockGenerator
		{
			public FSQBlockGeneratorTestHelper(IAIMMessageHeader freightStatusQueryMessageHeader) : base(freightStatusQueryMessageHeader)
			{
			}
			public AWBMessageBlock GetAIMCargoControlLocation(IAIMCargoControlLocation cargoControlLine, ZString hawbNumber)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateCargoControlLocation(cargoControlLine, hawbNumber);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMAirWayBill(IAIMAirWaybill airWaybill)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateAirWayBill(airWaybill);
				return messageBlocks.FirstOrDefault();
			}
		}
	}
}
