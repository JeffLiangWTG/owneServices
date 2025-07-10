using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class FreightCodePairListsTest : TestCase
	{
		public void TestCarrierBookingStatusList()
		{
			AssertEquals("NSN, BSN, BAC, BIR, BCF, BRJ, BWS, BWR, BWA, BPP, SSN, SAC, SIR, SCF, SRJ, SPP", FreightCodePairLists.CarrierBookingStatusList().CodesAsString);
		}

		public void TestCartageJobBookingStatusList()
		{
			AssertEquals("Expected list count to be 8", 8, FreightCodePairLists.CartageJobBookingStatusList().Count);
		}

		public void TestCartageJobFWDBookingActionList()
		{
			AssertEquals("Expected list count to be 4", 4, FreightCodePairLists.CartageJobFWDBookingActionList().Count);
		}

		public void TestLinkableTransportModeList()
		{
			AssertEquals("AIR, SEA, ROA, RAI", FreightCodePairLists.LinkableTransportModeList().CodesAsString);
		}

		public void TestRoutingTransportModeList()
		{
			AssertEquals("AIR, SEA, ROA, RAI, STO, IWT", FreightCodePairLists.RoutingTransportModeList().CodesAsString);
		}

		public void TestConsolModeList()
		{
			CodeDescriptionPairList testList;

			testList = FreightCodePairLists.ConsolModeList("Junk", "Junk");
			AssertEquals("Agent=Junk, Transport=Junk", 0, testList.Count);

			testList = FreightCodePairLists.ConsolModeList("", Constants.TransportModes.Air);
			AssertEquals("blank Agent, Transport=Air. Count", 5, testList.Count);
			AssertEquals("blank Agent, Transport=Air", Constants.ContainerModes.Loose, ((CodeDescriptionPair)testList[0]).Code);

			testList = FreightCodePairLists.ConsolModeList(Constants.AgentType.Agent, Constants.TransportModes.Sea);
			AssertEquals("Agent=Agent, Transport=Sea. Count", 10, testList.Count);
			AssertEquals("Agent=Agent, Transport=Sea", Constants.ContainerModes.Groupage, ((CodeDescriptionPair)testList[0]).Code);

			testList = FreightCodePairLists.ConsolModeList(Constants.AgentType.Other, Constants.TransportModes.Sea);
			AssertEquals("Agent=Other, Transport=Sea. Count", 1, testList.Count);
			AssertEquals("Agent=Other, Transport=Sea", Constants.ContainerModes.Other, ((CodeDescriptionPair)testList[0]).Code);

			testList = FreightCodePairLists.ConsolModeList(Constants.AgentType.Direct, Constants.TransportModes.Sea);
			AssertEquals("Agent=Direct, Transport=Sea. Count",9, testList.Count);
			AssertEquals("Agent=Direct, Transport=Sea", Constants.ContainerModes.FCL, ((CodeDescriptionPair)testList[0]).Code);

			testList = FreightCodePairLists.ConsolModeList("", Constants.TransportModes.Courier);
			AssertEquals("blank Agent, Transport=Courier. Count", 3, testList.Count);
			AssertEquals("blank Agent, Transport=Courier", Constants.ContainerModes.OnBoardCourier, ((CodeDescriptionPair)testList[0]).Code);

			testList = FreightCodePairLists.ConsolModeList("", Constants.TransportModes.Other);
			AssertEquals("blank Agent, Transport=Other. Count", 1, testList.Count);
			AssertEquals("blank Agent, Transport=Other", Constants.ContainerModes.Other, ((CodeDescriptionPair)testList[0]).Code);

			testList = FreightCodePairLists.ConsolModeList("", "");
			AssertEquals("blank Agent, blank Transport. Count", 14, testList.Count);
			AssertEquals("blank Agent, blank Transport", Constants.ContainerModes.LCL, ((CodeDescriptionPair)testList[0]).Code);

			testList = FreightCodePairLists.ConsolModeList("", Constants.TransportModes.Road);
			AssertEquals("blank Agent, Transport=Road. Count", 8, testList.Count);
			AssertEquals("blank Agent, Transport=Road", true, testList.ContainsCode(Constants.ContainerModes.BuyersConsol));

			testList = FreightCodePairLists.ConsolModeList("", Constants.TransportModes.Rail);
			AssertEquals("blank Agent, Transport=Rail. Count", 10, testList.Count);
			AssertEquals("blank Agent, Transport=Rail", true, testList.ContainsCode(Constants.ContainerModes.BuyersConsol));
		}

		public void TestJS_TransportModeList()
		{
			Assert("Expected list count > 0", FreightCodePairLists.JS_TransportModeList().Count > 0);
		}

		public void TestJS_ShipmentTypeList()
		{
			Assert("Expected list count > 0", FreightCodePairLists.JS_ShipmentTypeList().Count > 0);
		}

		public void TestPackageGroupingList()
		{
			AssertEquals("DNG, SHP, PKL", FreightCodePairLists.PackageGroupingList().CodesAsString);
		}

		public void TestJS_PackingModeList()
		{
			Assert("Expected list count = 0", FreightCodePairLists.JS_PackingModeList("Junk").Count == 0);
			Assert("Expected list count > 0", FreightCodePairLists.JS_PackingModeList(Constants.TransportModes.Air).Count > 0);

			CodeDescriptionPairList testList;

			testList = FreightCodePairLists.JS_PackingModeList("Junk");
			AssertEquals("Transport=Junk", 0, testList.Count);

			testList = FreightCodePairLists.JS_PackingModeList(Constants.TransportModes.Air);
			Assert("Transport=Air. Count", testList.Count > 0);
			AssertEquals("Transport=Air", Constants.ContainerModes.Loose, ((CodeDescriptionPair)testList[0]).Code);

			testList = FreightCodePairLists.JS_PackingModeList(Constants.TransportModes.Sea);
			Assert("Transport=Sea. Count", testList.Count > 0);
			AssertEquals("Transport=Sea", Constants.ContainerModes.FCL, ((CodeDescriptionPair)testList[0]).Code);

			testList = FreightCodePairLists.JS_PackingModeList(Constants.TransportModes.SeaAir);
			Assert("Transport=Sea. Count", testList.Count > 0);
			AssertEquals("Transport=Sea", Constants.ContainerModes.LCL, ((CodeDescriptionPair)testList[0]).Code);

			testList = FreightCodePairLists.JS_PackingModeList(Constants.TransportModes.AirSea);
			Assert("Transport=Sea. Count", testList.Count > 0);
			AssertEquals("Transport=Sea", Constants.ContainerModes.Loose, ((CodeDescriptionPair)testList[0]).Code);

			testList = FreightCodePairLists.JS_PackingModeList(Constants.TransportModes.Road);
			Assert("Transport=Sea. Count", testList.Count > 0);
			AssertEquals("Transport=Sea", Constants.ContainerModes.FCL, ((CodeDescriptionPair)testList[0]).Code);

			testList = FreightCodePairLists.JS_PackingModeList(Constants.TransportModes.Rail);
			Assert("Transport=Sea. Count", testList.Count > 0);
			AssertEquals("Transport=Sea", Constants.ContainerModes.FCL, ((CodeDescriptionPair)testList[0]).Code);

			testList = FreightCodePairLists.JS_PackingModeList(Constants.TransportModes.Courier);
			Assert("Transport=Sea. Count", testList.Count > 0);
			AssertEquals("Transport=Sea", Constants.ContainerModes.OnBoardCourier, ((CodeDescriptionPair)testList[0]).Code);

			testList = FreightCodePairLists.JS_PackingModeList("");
			Assert("blank Transport. Count", testList.Count > 0);
			AssertEquals("blank Transport", Constants.ContainerModes.LCL, ((CodeDescriptionPair)testList[0]).Code);
		}

		public void TestAdditionalTransportModeList()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.New<CommonConsol>();
			var transport = consol.Transports.AddNew();

			var tuples = new Tuple<string, string, ZString[]>[]
			{
				Tuple.Create(Constants.TransportModes.Air, Constants.TransportPlanningType.Flight1, Array.Empty<ZString>()),
				Tuple.Create(Constants.TransportModes.Sea, Constants.TransportPlanningType.MainVessel, Array.Empty<ZString>()),
				Tuple.Create(Constants.TransportModes.Sea, Constants.TransportPlanningType.OnForwarding, Array.Empty<ZString>()),
				Tuple.Create(Constants.TransportModes.Sea, Constants.TransportPlanningType.PreCarriage, Array.Empty<ZString>()),
				Tuple.Create(Constants.TransportModes.Sea, Constants.TransportPlanningType.Other, Array.Empty<ZString>()),
				Tuple.Create(Constants.TransportModes.Storage, string.Empty, Array.Empty<ZString>()),
				Tuple.Create(Constants.TransportModes.Road, Constants.TransportPlanningType.MainVessel, Array.Empty<ZString>()),
				Tuple.Create(Constants.TransportModes.Rail, Constants.TransportPlanningType.MainVessel, Array.Empty<ZString>()),
				Tuple.Create(Constants.TransportModes.Rail, Constants.TransportPlanningType.Other, Array.Empty<ZString>()),
				Tuple.Create(Constants.TransportModes.Rail, Constants.TransportPlanningType.OnForwarding, new ZString[] { Constants.TransportModes.Road }),
				Tuple.Create(Constants.TransportModes.Rail, Constants.TransportPlanningType.PreCarriage, new ZString[] { Constants.TransportModes.Road }),
				Tuple.Create(Constants.TransportModes.InlandWaterwayTransport, Constants.TransportPlanningType.OnForwarding, new ZString[] { Constants.TransportModes.Rail, Constants.TransportModes.Road }),
				Tuple.Create(Constants.TransportModes.InlandWaterwayTransport, Constants.TransportPlanningType.PreCarriage, new ZString[] { Constants.TransportModes.Rail, Constants.TransportModes.Road }),
			};

			foreach (var tuple in tuples)
			{
				transport.JW_TransportMode = tuple.Item1;
				transport.JW_TransportType = tuple.Item2;

				var additionalModes = transport.AdditionalTransportModeList().GetAllCodesZString();
				AssertContainsExactElementsInAnyOrder(tuple.Item3, additionalModes);
			}
		}

		public void TestBillOfLadingBillTypeList()
		{
			var billOfLadingBillTypeList = FreightCodePairLists.BillOfLadingBillTypeList();

			AssertEquals("STR, TOR, BLE", billOfLadingBillTypeList.CodesAsString);
			AssertEquals("Straight", billOfLadingBillTypeList[0].Description);
			AssertEquals("To Order", billOfLadingBillTypeList[1].Description);
			AssertEquals("Blank Endorse", billOfLadingBillTypeList[2].Description);
		}

		public void TestBillOfLadingBillTermsList()
		{
			var billOfLadingBillTermsList = FreightCodePairLists.BillOfLadingBillTermsList();

			AssertEquals("TRA, NTR", billOfLadingBillTermsList.CodesAsString);
			AssertEquals("Transferable", billOfLadingBillTermsList[0].Description);
			AssertEquals("Non-Transferable", billOfLadingBillTermsList[1].Description);
		}

		public void TestBillOfLadingBillStatusList()
		{
			var billOfLadingBillStatusList = FreightCodePairLists.BillOfLadingBillStatusList();

			AssertEquals("OBR, OBT, OBA, STP, SUR", billOfLadingBillStatusList.CodesAsString);

			AssertEquals("Original Bill Received", billOfLadingBillStatusList[0].Description);
			AssertEquals("Original Bill Transferred", billOfLadingBillStatusList[1].Description);
			AssertEquals("Original Bill Amendment in Progress", billOfLadingBillStatusList[2].Description);
			AssertEquals("Switched To Paper", billOfLadingBillStatusList[3].Description);
			AssertEquals("Surrendered", billOfLadingBillStatusList[4].Description);
		}

		public void TestHouseBillOfLadingBillStatusList()
		{
			var houseBillOfLadingBillStatusList = FreightCodePairLists.HouseBillOfLadingBillStatusList();

			AssertEquals("OBS, OBP, OBF, OBA, OBT, STP, SUR", houseBillOfLadingBillStatusList.CodesAsString);

			AssertEquals("Sent For Publication", houseBillOfLadingBillStatusList[0].Description);
			AssertEquals("Original Bill Published", houseBillOfLadingBillStatusList[1].Description);
			AssertEquals("Publishing Rejected", houseBillOfLadingBillStatusList[2].Description);
			AssertEquals("Original Bill Amendment in Progress", houseBillOfLadingBillStatusList[3].Description);
			AssertEquals("Original Bill Transferred", houseBillOfLadingBillStatusList[4].Description);
			AssertEquals("Switched To Paper", houseBillOfLadingBillStatusList[5].Description);
			AssertEquals("Surrendered", houseBillOfLadingBillStatusList[6].Description);
		}

		public void TestPackLineHouseBillPaymentTypeList()
		{
			var houseBillPaymentTypeList = FreightCodePairLists.PackLineHouseBillPaymentTypeList();

			AssertEquals("A, B, C, D, H, Y, Z", houseBillPaymentTypeList.CodesAsString);

			AssertEquals("Payment in cash", houseBillPaymentTypeList[0].Description);
			AssertEquals("Payment by credit card", houseBillPaymentTypeList[1].Description);
			AssertEquals("Payment by cheque", houseBillPaymentTypeList[2].Description);
			AssertEquals("Other", houseBillPaymentTypeList[3].Description);
			AssertEquals("Electronic funds transfer", houseBillPaymentTypeList[4].Description);
			AssertEquals("Account holder with carrier", houseBillPaymentTypeList[5].Description);
			AssertEquals("Not pre-paid", houseBillPaymentTypeList[6].Description);
		}
	}
}
