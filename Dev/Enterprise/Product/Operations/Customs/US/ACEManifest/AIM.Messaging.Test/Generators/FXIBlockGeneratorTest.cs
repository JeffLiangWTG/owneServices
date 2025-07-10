using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging.Generators;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	[TestedType(typeof(FXIBlockGenerator))]
	class FXIBlockGeneratorTest : AIMBlockGeneratorTest
	{
		protected override AIMBlockGenerator GetGenerator() => new FXIBlockGenerator(GetMessageHeaderMock().Object);
		protected override ZString MessageType => Constants.AIMMessageSubTypes.FXI;

		protected override Type[] GetExpectedMessageBlockTypes() => new[]
		{
			typeof(AIMCargoControlLocation),
			typeof(AIMAirWaybill),
			typeof(AIMWaybill),
			typeof(AIMArrival),
			typeof(AIMAgent),
			typeof(AIMCBPEntryDetail),
			typeof(AIMShipper),
			typeof(AIMConsignee)
		};

		Mock<IManifestMessageHeader> GetMessageHeaderMock()
		{
			var mock = new Mock<IManifestMessageHeader>();
			mock.Setup(m => m.MessageType).Returns(MessageType);
			mock.Setup(m => m.Reference).Returns("HAWB001");
			mock.Setup(m => m.CargoControlLine).Returns(AIMInterfaceTestHelper.GetCargoControlLocation("JFK", "XYZ").Object);
			mock.Setup(m => m.CBPEntryDetail).Returns(AIMInterfaceTestHelper.GetCBPEntryDetail("86", "12345678901").Object);
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

		public void TestPopulateMessage()
		{
			var messageBlocks = GetGenerator().Generate();
			var cargoControlLocation = messageBlocks.FirstOrDefault(x => x is AIMCargoControlLocation);
			var airWayBill = messageBlocks.FirstOrDefault(x => x is AIMAirWaybill);
			var wayBill = messageBlocks.FirstOrDefault(x => x is AIMWaybill);
			var arrival = messageBlocks.FirstOrDefault(x => x is AIMArrival);
			var agent = messageBlocks.FirstOrDefault(x => x is AIMAgent);
			var cbpEntryDetail = messageBlocks.FirstOrDefault(x => x is AIMCBPEntryDetail);
			var shipper = messageBlocks.FirstOrDefault(x => x is AIMShipper);
			var consignee = messageBlocks.FirstOrDefault(x => x is AIMConsignee);
			CombineAssertions(() =>
			{
				AssertEquals(@"--------AIMCargoControlLocation---------
AirportOfArrival (3-3A)       :JFK
CargoTerminalOperator (2-3AN) :XYZ

", cargoControlLocation.Serialise());
				AssertEquals(@"-------------AIMAirWaybill--------------
AirWaybillPrefix (3-3AN) :999
AWBSerialNumber (8-8N)   :12345675

", airWayBill.Serialise());
				AssertEquals(@"---------------AIMWaybill---------------
AirportOfOrigin (3-3A)   :FRA
NumberOfPieces (1-5N)    :1
WeightCode (1-1A)        :K
Weight (1-7N)            :10
CargoDescription (1-35T) :TOYS

", wayBill.Serialise());
				AssertEquals(@"---------------AIMArrival---------------
FlightNumber (5-8AN)         :XYZ1234A
ScheduledArrivalDate (5-5AN) :25OCT

", arrival.Serialise());
				AssertEquals(@"----------------AIMAgent----------------
AirAMSParticipantCode (2-7AN) :PP

", agent.Serialise());
				AssertEquals(@"-----------AIMCBPEntryDetail------------
EntryType (2-2N)      :86
EntryNumber (11-11AN) :12345678901

", cbpEntryDetail.Serialise());
				AssertEquals(@"---------------AIMShipper---------------
Name (1-35T)               :TOTLER TOYS
StreetAddress (1-35T)      :12 VIRGINIA COURT
CityCountyTownship (1-17T) :FRANKFURT
CountryCode (2-2A)         :DE

", shipper.Serialise());
				AssertEquals(@"--------------AIMConsignee--------------
Name (1-35T)               :TOYS R WE
StreetAddress (1-35T)      :8812 FUN STREET
CityCountyTownship (1-17T) :NEW YORK
StateOrProvince (1-9AN)    :NY
CountryCode (2-2A)         :US
PostalCode (1-9AN)         :12345
TelephoneNumber (1-14T)    :123-456-7890

", consignee.Serialise());
			});
		}
	}
}
