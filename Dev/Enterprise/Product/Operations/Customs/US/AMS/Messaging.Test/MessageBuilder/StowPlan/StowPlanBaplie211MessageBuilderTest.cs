using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Business.StowPlan.Testing
{
	class StowPlanBaplie211MessageBuilderTest : TestCaseWithFactory
	{
		[TestDate(2012, 10, 25, 23, 10, 0)]
		public void TestPopulateMessage()
		{
			var stockMock = new Mock<IStowPlanContainerStockData>();
			stockMock.Setup(m => m.ContainerOperator).Returns("ABAAASSYD");
			var containerMock = new Mock<IStowPlanContainerData>();
			containerMock.Setup(m => m.EquipmentNumber).Returns("APLU1234564");
			containerMock.Setup(m => m.StowPosition).Returns("1112233");
			containerMock.Setup(m => m.GrossWeightInKG).Returns(23m);
			containerMock.Setup(m => m.HazardCodes).Returns(new ZString[] { "0004A" });
			containerMock.Setup(m => m.Stock).Returns(stockMock.Object);
			containerMock.Setup(m => m.ISOType).Returns("40U2");
			var shipmentMock = new Mock<IStowPlanShipmentData>();
			shipmentMock.Setup(m => m.JS_HouseBill).Returns("H1234");
			shipmentMock.Setup(m => m.PortOfLading).Returns("DEHAM");
			shipmentMock.Setup(m => m.PortOfDischarge).Returns("USBAL");
			shipmentMock.Setup(m => m.Containers).Returns(new[] { containerMock.Object });
			var vesselMock = new Mock<IStowPlanVesselData>();
			vesselMock.Setup(m => m.VesselName).Returns("APL BAHRAIN");
			vesselMock.Setup(m => m.IMONumber).Returns("9395927");
			vesselMock.Setup(m => m.VesselOperator).Returns("ABAH");
			var voyageMock = new Mock<IStowPlanSailingData>();
			voyageMock.Setup(m => m.Vessel).Returns(vesselMock.Object);
			voyageMock.Setup(m => m.VoyageNumber).Returns("1299");
			voyageMock.Setup(m => m.Departure).Returns("DEHAM");
			voyageMock.Setup(m => m.DepartureTime).Returns(new ZDate(2012, 04, 20));
			voyageMock.Setup(m => m.IsDepartureTimeEstimated).Returns(true);
			voyageMock.Setup(m => m.Arrival).Returns("USBAL");
			voyageMock.Setup(m => m.ArrivalTime).Returns(new ZDate(2012, 04, 30));
			voyageMock.Setup(m => m.IsArrivalTimeEstimated).Returns(true);
			var msgColl = new EDIMessageCollection(Factory.New<DummyBusinessObject>());
			voyageMock.Setup(m => m.Messages).Returns(msgColl);
			voyageMock.Setup(m => m.Shipments).Returns(new[] { shipmentMock.Object });
			var builder = new StowPlanBaplie211MessageBuilder(voyageMock.Object);
			var message = builder.PopulateMessages().GetBuilderResults().ElementAt(0).Message;
			AssertMultilineASCIIEquals("Message contents", @"UNH+<<MSGNO PLACEHOLDER>>+BAPLIE:D:95B:UN:SMDG20
BGM++<<MSGNO PLACEHOLDER>>+9
DTM+137:121025:101
TDT+20+1299+++ABAH:172:166+++9395927:146:11:APL BAHRAIN
LOC+5+DEHAM:139:6
LOC+61+USBAL:139:6
DTM+133:120420:101
DTM+132:120430:101
LOC+147+1112233::5
MEA+WT++KG:23
LOC+9+DEHAM
LOC+11+USBAL
RFF+BM:H1234
EQD+CN+APLU1234564+40U2+++5
NAD+CA+ABAAASSYD:172:166
DGS+IMD+0004A
UNT+17+<<MSGNO PLACEHOLDER>>", message.EM_FormattedMessageText);

			stockMock.VerifyAll();
			containerMock.VerifyAll();
			shipmentMock.VerifyAll();
			vesselMock.VerifyAll();
			voyageMock.VerifyAll();
		}
	}
}
