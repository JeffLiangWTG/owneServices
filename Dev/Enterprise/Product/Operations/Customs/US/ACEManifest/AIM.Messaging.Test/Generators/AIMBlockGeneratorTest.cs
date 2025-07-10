using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AIM.Messaging.Generators;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AWB.AIM;
using Enterprise.Messaging.Business.AWB;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	[TestsSubclassesOf(typeof(AIMBlockGenerator))]
	abstract class AIMBlockGeneratorTest : TestCaseWithFactory
	{
		public void TestGenerate()
		{
			var generator = GetGenerator();
			var messageBlocks = generator.Generate();
			var expectedMessageBlockTypes = GetExpectedMessageBlockTypes();
			var total = expectedMessageBlockTypes.Length + 1;
			AssertEquals("messageBlocks.Length", total, messageBlocks.Length);
			// Standard Message Identifier
			var standardMessageIdentifierMessageBlock = (AIMStandardMessageIdentifier)messageBlocks[0];
			AssertEquals("", MessageType, standardMessageIdentifierMessageBlock.ComponentIdentifier);
			for (var messageBlockIndex = 1; messageBlockIndex < total; messageBlockIndex++)
			{
				var messageBlock = messageBlocks[messageBlockIndex];
				var expectedMessageBlockType = expectedMessageBlockTypes[messageBlockIndex - 1];
				var message = "MessageBlock at " + messageBlockIndex;
				AssertType("MessageBlock at " + messageBlockIndex, expectedMessageBlockTypes[messageBlockIndex - 1], messageBlock);
			}
		}

		protected abstract Type[] GetExpectedMessageBlockTypes();

		protected abstract AIMBlockGenerator GetGenerator();

		protected abstract ZString MessageType { get; }
	}

	public class BaseAIMBlockGeneratorTest : TestCaseWithFactory
	{
		public void TestAIMStandardMessageIdentifier()
		{
			var manifestMock = new Mock<IAIMMessageHeader>();
			manifestMock.Setup(m => m.MessageType).Returns("FSI");
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMStandardMessageIdentifier)generator.GetAIMStandardMessageIdentifier(manifestMock.Object);
			AssertEquals("ComponentIdentifier", "FSI", testBlock.ComponentIdentifier);

			manifestMock.VerifyAll();
		}

		public void TestAIMCargoControlLocation()
		{
			var iAIMCargoControlLocation = AIMInterfaceTestHelper.GetCargoControlLocation("JFK", "XYZ").Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMCargoControlLocation)generator.GetAIMCargoControlLocation(iAIMCargoControlLocation);
			AssertEquals("AirportOfArrival", "JFK", testBlock.AirportOfArrival);
			AssertEquals("CargoTerminalOperator", "XYZ", testBlock.CargoTerminalOperator);
		}

		public void TestAIMAirWaybill()
		{
			var mock = AIMInterfaceTestHelper.GetAirWaybill("999", "12345675", false);
			mock.Setup(m => m.PackageTrackingIdentifier).Returns("PT");
			mock.Setup(m => m.HAWBNumber).Returns("HB");
			var iAIMAirWaybill = mock.Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMAirWaybill)generator.GetAIMAirWaybill(iAIMAirWaybill);
			AssertEquals("AirWaybillPrefix", "999", testBlock.AirWaybillPrefix);
			AssertEquals("AWBSerialNumber", "12345675", testBlock.AWBSerialNumber);
			AssertEquals("HAWBNumber", "HB", testBlock.HAWBNumber);
			AssertEquals("PackageTrackingIdentifier", "PT", testBlock.PackageTrackingIdentifier);

			mock.VerifyAll();
		}

		public void TestAIMAirWaybill_ConsolidateIdentifier()
		{
			var mock = AIMInterfaceTestHelper.GetAirWaybill("999", "12345675", true);
			mock.Setup(m => m.PackageTrackingIdentifier).Returns("PT");
			var iAIMAirWaybill = mock.Object;
			iAIMAirWaybill = mock.Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMAirWaybill)generator.GetAIMAirWaybill(iAIMAirWaybill);
			AssertEquals("ConsolidationIdentifier", "M", testBlock.HAWBNumber);

			mock.VerifyAll();
		}

		public void TestAIMWaybill()
		{
			var iAIMWaybill = AIMInterfaceTestHelper.GetWaybillDetails("FRA", 1m, "K", 10m, "TOYS", "PS", new ZDate(2022, 02, 22)).Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMWaybill)generator.GetAIMWaybill(iAIMWaybill);
			AssertEquals("AirportOfOrigin", "FRA", testBlock.AirportOfOrigin);
			AssertEquals("CargoDescription", "TOYS", testBlock.CargoDescription);
			AssertEquals("DateOfArrivalAtThePermitToProceedDestinationAirport", new ZDate(2022, 02, 22), testBlock.DateOfArrivalAtThePermitToProceedDestinationAirport);
			AssertEquals("NumberOfPieces", 1m, testBlock.NumberOfPieces);
			AssertEquals("PermitToProceedDestinationAirport", "PS", testBlock.PermitToProceedDestinationAirport);
			AssertEquals("Weight", 10m, testBlock.Weight);
			AssertEquals("WeightCode", "K", testBlock.WeightCode);
		}

		public void TestAIMWayBill_CargoDescript_SubStringOnLengthViolation()
		{
			var iAIMWaybill = AIMInterfaceTestHelper.GetWaybillDetails("FRA", 1m, "K", 10m, "1234567890123456789012345678901234567890", "PS", new ZDate(2022, 02, 22)).Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = generator.GetAIMWaybill(iAIMWaybill).Serialise();
			AssertEquals("Cargo Description is upto 35ch", true, testBlock.Contains("12345678901234567890123456789012345"));
			AssertEquals("Cargo Description is trimmed after 35ch", false, testBlock.Contains("123456789012345678901234567890123456"));
		}

		public void TestAIMArrival()
		{
			var iAIMArrival = AIMInterfaceTestHelper.GetArrival("XYZ1234A", new ZDate(2019, 10, 25), "AM", false, 2m, "K", 10m).Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMArrival)generator.GetAIMArrival(iAIMArrival);
			AssertEquals("BoardedPieceCount", 2m, testBlock.BoardedPieceCount);
			AssertEquals("BoardedQuantityIdentifier", "", testBlock.BoardedQuantityIdentifier);
			AssertEquals("FlightNumber", "XYZ1234A", testBlock.FlightNumber);
			AssertEquals("PartArrivalReference", "AM", testBlock.PartArrivalReference);
			AssertEquals("ScheduledArrivalDate", new ZDate(2019, 10, 25), testBlock.ScheduledArrivalDate);
			AssertEquals("Weight", 10m, testBlock.Weight);
			AssertEquals("WeightCode", "K", testBlock.WeightCode);

			iAIMArrival = AIMInterfaceTestHelper.GetArrival("XYZ1234A", new ZDate(2019, 10, 25), "AM", true, 2m, "K", 10m).Object;
			testBlock = (AIMArrival)generator.GetAIMArrival(iAIMArrival);
			AssertEquals("BoardedQuantityIdentifier", "B", testBlock.BoardedQuantityIdentifier);
		}

		public void TestAIMAgent()
		{
			var iAIMAgent = AIMInterfaceTestHelper.GetAgent("PP").Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMAgent)generator.GetAIMAgent(iAIMAgent);
			AssertEquals("AirAMSParticipantCode", "PP", testBlock.AirAMSParticipantCode);

			iAIMAgent = AIMInterfaceTestHelper.GetAgent("").Object;
			var testBlock2 = generator.GetAIMAgent(iAIMAgent);
			AssertNull("No AIMAgent block", testBlock2);

			var testBlock3 = generator.GetAIMAgent(null);
			AssertNull("No AIMAgent block", testBlock3);
		}

		public void TestAIMShipper()
		{
			var iAIMParty = AIMInterfaceTestHelper.GetParty("TOTLER TOYS", "FRANKFURT", "DE", "12 VIRGINIA COURT", "NN", "223", "12345678").Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMShipper)generator.GetAIMShipper(iAIMParty);
			AssertEquals("CityCountyTownship", "FRANKFURT", testBlock.CityCountyTownship);
			AssertEquals("CountryCode", "DE", testBlock.CountryCode);
			AssertEquals("Name", "TOTLER TOYS", testBlock.Name);
			AssertEquals("PostalCode", "223", testBlock.PostalCode);
			AssertEquals("StreetAddress", "12 VIRGINIA COURT", testBlock.StreetAddress);
			AssertEquals("TelephoneNumber", "12345678", testBlock.TelephoneNumber);

			var testBlock2 = generator.GetAIMShipper(null);
			AssertNull("No AIMShipper block", testBlock2);
		}

		public void TestAIMConsignee()
		{
			var iAIMParty = AIMInterfaceTestHelper.GetParty("TOTLER TOYS", "FRANKFURT", "DE", "12 VIRGINIA COURT", "NN", "223", "12345678").Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMConsignee)generator.GetAIMConsignee(iAIMParty);
			AssertEquals("CityCountyTownship", "FRANKFURT", testBlock.CityCountyTownship);
			AssertEquals("CountryCode", "DE", testBlock.CountryCode);
			AssertEquals("Name", "TOTLER TOYS", testBlock.Name);
			AssertEquals("PostalCode", "223", testBlock.PostalCode);
			AssertEquals("StreetAddress", "12 VIRGINIA COURT", testBlock.StreetAddress);
			AssertEquals("TelephoneNumber", "12345678", testBlock.TelephoneNumber);

			var testBlock2 = generator.GetAIMConsignee(null);
			AssertNull("No AIMConsignee block", testBlock2);
		}

		public void TestAIMTransfer()
		{
			var iAIMTransfer = AIMInterfaceTestHelper.GetTransfer("SYDAP", "SSS", "DEDSD", "1234").Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMTransfer)generator.GetAIMTransfer(iAIMTransfer);
			AssertEquals("BondedCarrierIDOrOnwardCarrier", "DEDSD", testBlock.BondedCarrierIDOrOnwardCarrier);
			AssertEquals("BondedPremisesIdentifierOrInbondControlNumber", "1234", testBlock.BondedPremisesIdentifierOrInbondControlNumber);
			AssertEquals("DestinationAirport", "SYDAP", testBlock.DestinationAirport);
			AssertEquals("DomesticInternationalIdentifier", "SSS", testBlock.DomesticInternationalIdentifier);
		}

		public void TestAIMCBPShipmentDescription()
		{
			var iAIMCBPShipmentDescription = AIMInterfaceTestHelper.GetCBPShipmentDescription(3m, "USD", "DE", "1234").Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMCBPShipmentDescription)generator.GetAIMCBPShipmentDescription(iAIMCBPShipmentDescription);
			AssertEquals("DeclaredValue", 3m, testBlock.DeclaredValue);
			AssertEquals("HarmonizedCommodityCode", "1234", testBlock.HarmonizedCommodityCode);
			AssertEquals("ISOCurrencyCode", "USD", testBlock.ISOCurrencyCode);
			AssertEquals("OriginOfGoods", "DE", testBlock.OriginOfGoods);

			var testBlock2 = generator.GetAIMCBPShipmentDescription(null);
			AssertNull("No AIMCBPShipmentDescription block", testBlock2);
		}

		public void TestAIMFDAFreightIndicator()
		{
			var iAIMFDAFreightIndicator = AIMInterfaceTestHelper.GetFDAFreightIndicator(true).Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMFDAFreightIndicator)generator.GetAIMFDAFreightIndicator(iAIMFDAFreightIndicator);
			AssertNotNull("Have AIMFDAFreightIndicator block", testBlock);

			iAIMFDAFreightIndicator = AIMInterfaceTestHelper.GetFDAFreightIndicator(false).Object;
			var testBlock2 = generator.GetAIMFDAFreightIndicator(iAIMFDAFreightIndicator);
			AssertNull("No AIMFDAFreightIndicator block", testBlock2);

			var testBlock3 = generator.GetAIMFDAFreightIndicator(null);
			AssertNull("No AIMFDAFreightIndicator block", testBlock3);
		}

		public void TestAIMCBPEntryDetail()
		{
			var iAIMCBPEntryDetail = AIMInterfaceTestHelper.GetCBPEntryDetail("AA", "12345").Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMCBPEntryDetail)generator.GetAIMCBPEntryDetail(iAIMCBPEntryDetail);
			AssertEquals("EntryType", "AA", testBlock.EntryType);
			AssertEquals("EntryNumber", "12345", testBlock.EntryNumber);

			var testBlock2 = generator.GetAIMCBPEntryDetail(null);
			AssertNull("No AIMCBPEntryDetail block", testBlock2);
		}

		public void TestAIMCBPEntryDetailsForCancellation()
		{
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMCBPEntryDetailForCancellation)generator.GetAIMCBPEntryDetailsForCancellation();
			var fields = testBlock.GetFieldInfos().ToArray();
			AssertEquals(4, fields.Length);
			var info = fields[0] as AWBMessageBlock.SpecialFieldInfo;
			AssertEquals("CED", info.Value);
			info = fields[1] as AWBMessageBlock.SpecialFieldInfo;
			AssertEquals(SpecialChars.Slant, info.Value);
			info = fields[2] as AWBMessageBlock.SpecialFieldInfo;
			AssertEquals("000", info.Value);
			info = fields[3] as AWBMessageBlock.SpecialFieldInfo;
			AssertEquals(SpecialChars.CRLF, info.Value);
		}

		public void TestAIMReasonForAmendment()
		{
			var iAIMReasonForAmendment = AIMInterfaceTestHelper.GetReasonForAmendment("KK", "Change info").Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMReasonForAmendment)generator.GetAIMReasonForAmendment(iAIMReasonForAmendment);
			AssertEquals("AmendmentCode", "KK", testBlock.AmendmentCode);
			AssertEquals("AmendmentExplanation", "Change info", testBlock.AmendmentExplanation);
		}

		public void TestAIMAirlineStatusNotification()
		{
			var iAIMAirlineStatusNotification = AIMInterfaceTestHelper.GetAirlineStatusNotification("SC", "Delay").Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMAirlineStatusNotification)generator.GetAIMAirlineStatusNotification(iAIMAirlineStatusNotification);
			AssertEquals("AmendmentCode", "SC", testBlock.StatusCode);
			AssertEquals("AmendmentExplanation", "Delay", testBlock.ActionExplanation);
		}

		public void TestAIMDeparture()
		{
			var iAIMDeparture = AIMInterfaceTestHelper.GetDeparture("QT001", new ZDate(2022, 02, 22), new ZDate(2022, 02, 21), "090603", "SED", "QT002").Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMDeparture)generator.GetAIMDeparture(iAIMDeparture);
			AssertEquals("FlightNumber", "QT001", testBlock.FlightNumber);
			AssertEquals("DateOfScheduledArrival", new ZDate(2022, 02, 22), testBlock.DateOfScheduledArrival);
			AssertEquals("LiftoffDate", new ZDate(2022, 02, 21), testBlock.LiftoffDate);
			AssertEquals("LiftoffTime", "090603", testBlock.LiftoffTime);
			AssertEquals("ActualImportingCarrier", "SED", testBlock.ActualImportingCarrier);
			AssertEquals("ActualFlightNumber", "QT002", testBlock.ActualFlightNumber);
		}

		public void TestAIMFreightStatusQuery()
		{
			var iFreightStatusQuery = AIMInterfaceTestHelper.GetFreightStatusQuery("01").Object;
			var generator = new AIMBlockGeneratorTestHelper();
			var testBlock = (AIMFreightStatusQuery)generator.GetAIMFreightStatusQuery(iFreightStatusQuery);
			AssertEquals("StatusRequestCode", "01", testBlock.StatusRequestCode);
		}

		class AIMBlockGeneratorTestHelper : AIMBlockGenerator
		{
			public AWBMessageBlock GetAIMStandardMessageIdentifier(IAIMMessageHeader header)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateStandardMessageIdentifier(header);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMCargoControlLocation(IAIMCargoControlLocation cargoControlLine)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateCargoControlLocation(cargoControlLine);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMAirWaybill(IAIMAirWaybill airWaybill)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateAirWayBill(airWaybill);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMWaybill(IAIMWaybill waybill)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateWayBillDetails(waybill);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMArrival(IAIMArrival arrival)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateArrivalDetails(arrival);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMAgent(IAIMAgent agent)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateAgent(agent);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMShipper(IAIMParty shipper)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateShipper(shipper);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMConsignee(IAIMParty consignee)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateConsignee(consignee);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMTransfer(IAIMTransfer transfer)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateTransferDetails(transfer);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMCBPShipmentDescription(IAIMCBPShipmentDescription cpbShipmentDescription)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateCBPShipmentDescription(cpbShipmentDescription);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMFDAFreightIndicator(IAIMFDAFreightIndicator fdaFreightIndicator)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateFDAFreightIndicator(fdaFreightIndicator);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMCBPEntryDetail(IAIMCBPEntryDetail cbpEntryDetails)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateCBPEntryDetails(cbpEntryDetails);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMCBPEntryDetailsForCancellation()
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateCBPEntryDetailsForCancellation();
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMReasonForAmendment(IAIMReasonForAmendment reasonForAmendment)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateReasonForAmendment(reasonForAmendment);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMAirlineStatusNotification(IAIMAirlineStatusNotification airlineStatusNotification)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateAirlineStatusNotification(airlineStatusNotification);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMDeparture(IAIMDeparture departureDetails)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateDepartureDetails(departureDetails);
				return messageBlocks.FirstOrDefault();
			}

			public AWBMessageBlock GetAIMFreightStatusQuery(IFreightStatusQuery freightStatusQuery)
			{
				messageBlocks = new List<AWBMessageBlock>();
				PopulateFreightStatusQuery(freightStatusQuery);
				return messageBlocks.FirstOrDefault();
			}
		}
	}
}
