using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using static Enterprise.Integration.Customs.US.InBond;
using Constants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ACEInBondWPMessageBlockBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateWP10()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.TypeAMSHBRE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var dummy = Factory.New<DummyIInBondWP>();
				dummy.InBondNumber = "111111111";
				dummy.MasterBillIssuerCode = "2222";
				dummy.MasterBillNumber = "333333333333";
				dummy.ContainerNumber = "4444";
				dummy.InBondImportTransportMode = TransportModeCodes.Codes.VesselContainer;
				var builder = new ACEInBondWPMessageBlockBuilder(dummy);
				var blocks = new List<MessageBlock>(builder.Build(InBondWPActionCodeList.Codes.ArriveEntireInBondAtDestination));
				AssertEquals(2, blocks.Count);
				AssertEquals("101111111111   2222333333333333                BLAH            4444             ", blocks[0].Serialise());

				var iBill = Factory.New<ICusInBondBill>();
				iBill.B0_IssuerCode = "iss1";
				iBill.B0_MasterBillNumber = "mbill1";
				iBill.B0_HouseBillNumber = "hbill1";
				iBill.B0_HouseBillIssuerCode = "iss2";
				var iContainer = Factory.New<ICusInBondContainer>();
				iContainer.BC_ContainerNum = "container1";
				builder = new ACEInBondWPMessageBlockBuilder(dummy, iBill, iContainer);
				blocks = new List<MessageBlock>(builder.Build(InBondWPActionCodeList.Codes.ArriveBillOfLadingAtDestination));
				AssertEquals(2, blocks.Count);
				AssertEquals("102111111111   ISS1MBILL1      ISS2HBILL1      BLAH                             ", blocks[0].Serialise());

				blocks = new List<MessageBlock>(builder.Build(InBondWPActionCodeList.Codes.ArriveContainerAtDestination));
				AssertEquals(2, blocks.Count);
				AssertEquals("103111111111   ISS1MBILL1      ISS2HBILL1      BLAH            CONTAINER1       ", blocks[0].Serialise());
			}
		}

		public void TestGenerateWP20()
		{
			var dummy = Factory.New<DummyIInBondWP>();
			dummy.ArrivalDateTime = new ZDateTime(2006, 12, 10, 13, 1, 1);
			dummy.ScheduleDPortOfArrival = "2222";
			dummy.InBondCarrierCode = "3333";
			dummy.BondedCarrierID = "4444";
			dummy.CityName = "City Name Which Is Very Very long";
			dummy.StateCode = "IL";
			dummy.InBondExportTransportMode = TransportModeCodes.Codes.AirContainer;
			dummy.ExportConveyance = "Export conveyance";

			var builder = new ACEInBondWPMessageBlockBuilder(dummy);
			var blocks = new List<MessageBlock>(builder.Build(InBondWPActionCodeList.Codes.ArriveEntireInBondAtDestination));
			AssertEquals(2, blocks.Count);
			AssertEquals("20061210130101222233334444        CITY NAME WHICH IS IL41EXPORT CONVEYANCE      ", blocks[1].Serialise());
		}

		public void TestGenerateWP20ForExportation()
		{
			var dummy = Factory.New<DummyIInBondWP>();
			dummy.ExportDateTime = new ZDateTime(2006, 12, 10, 13, 1, 1);
			dummy.PortOfExport = "2222";
			dummy.InBondCarrierCode = "3333";
			dummy.BondedCarrierID = "4444";
			dummy.CityName = "City Name Which Is Very Very long";
			dummy.StateCode = "IL";
			dummy.InBondExportTransportMode = TransportModeCodes.Codes.AirContainer;
			dummy.ExportConveyance = "Export conveyance";

			var builder = new ACEInBondWPMessageBlockBuilder(dummy);
			var blocks = new List<MessageBlock>(builder.Build(InBondWPActionCodeList.Codes.ExportEntireInBondFromDestinationPort));
			AssertEquals(2, blocks.Count);
			AssertEquals("20061210130101222233334444        CITY NAME WHICH IS IL41EXPORT CONVEYANCE      ", blocks[1].Serialise());
		}

		public void TestTestGenerateWP20ForDiversionMessage()
		{
			var dummy = Factory.New<DummyIInBondWP>();
			dummy.ExportDateTime = new ZDateTime(2006, 12, 10, 13, 1, 1);
			dummy.PortOfExport = "2222";
			dummy.InBondCarrierCode = "3333";
			dummy.BondedCarrierID = "4444";
			dummy.CityName = "City Name Which Is Very Very long";
			dummy.StateCode = "IL";
			dummy.InBondExportTransportMode = TransportModeCodes.Codes.AirContainer;
			dummy.ExportConveyance = "Export conveyance";

			var dummy2 = Factory.New<DummyIInBondMessageSendingWP>();
			dummy2.DiversionDateTime = new ZDateTime(2006, 12, 10, 13, 1, 1);
			dummy2.DiversionPortCode = "2222";
			dummy2.DiversionInBondCarrierCode = "3333";
			dummy2.DiversionBondedCarrierID = "4444";

			var builder = new ACEInBondWPMessageBlockBuilder(dummy, dummy2);
			var blocks = new List<MessageBlock>(builder.Build(InBondWPActionCodeList.Codes.DiversionRequest));
			AssertEquals(2, blocks.Count);
			AssertEquals("20061210130101222233334444                                                      ", blocks[1].Serialise());
		}
	}
}
