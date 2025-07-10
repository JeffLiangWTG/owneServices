using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class TariffHeaderResponseProcessorTest : ABIProcessorTest<ACSTariffHeaderResponseProcessor, APLA, APLB, APLY>
	{
		protected override void EndToEndCore()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "08045060F0";
			tariff.UE_DateFrom = new ZDateTime(2008, 1, 1);
			tariff.UE_DateTo = new ZDateTime(2008, 12, 31);
			Factory.Save();

			var processor = new ACSSolicitedTariffProcessor();
			AddMessageBlocksToProcessor(processor, "WR",
"W108045060F0 0101081231081KG       1FRESH MANGOES,ENT 6/1-8/31    000006600000  ",
"W208045060F0000000000000000000000000000033100000000000000000000000000000        ",
"W308045060F0                                        A E J P AUBHCACLILJOMAMXSG  ",
"W408045060F0                       106010831                                    ",
"W508045060F0SG00000240000000000000000000000000000010811000001102300000000000000 ",
"WL08045060F0EP3EP2FS3                                                           ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			processor.Message = responseMessage;

			processor.Process();
			Factory.Save();
			tariff.Reload();
			AssertEquals("1 date restriction", 1, tariff.TariffDateRestrictions.Count);
			AssertEquals("Date restriction code", "1", tariff.TariffDateRestrictions[0].UF_EntryDateRestrictionCode);
			AssertEquals("Date restriction dateFrom", (short)601, tariff.TariffDateRestrictions[0].UF_EntryDateRestrictionFrom);
			AssertEquals("Date restriction dateTo", (short)831, tariff.TariffDateRestrictions[0].UF_EntryDateRestrictionTo);
			AssertEquals("No New PGA codes are received in block W3", "", tariff.UE_OGACodes);
			AssertEquals("EP3EP2FS3", tariff.UE_PGACodes);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 0; }
		}

		public void TestW0()
		{
			//Test Invalid Truncates Date
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "6203219015";
			tariff.UE_DateFrom = ZDate.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTimeValue;
			Factory.Save();

			var processor = new ACSTariffHeaderResponseProcessor();
			AddMessageBlocksToProcessor(processor, "WR",
"W06203219015051607          NOT ON FILE OR EXPIRED                              ",
"W06203219020051607          NOT ON FILE OR EXPIRED                              ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			processor.Message = responseMessage;

			processor.Process();
			Factory.Save();
			tariff.Reload();

			AssertEquals(new ZDateTime(2007, 5, 16), tariff.UE_DateTo);

			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "7113192900";
			tariff1.UE_DateFrom = ZDate.BrettsBirthday;
			tariff1.UE_DateTo = ZDateTime.MaxSmallDateTimeValue;
			Factory.Save();

			AddMessageBlocksToProcessor(processor, "WR",
"W07113192900110408          NOT ON FILE OR EXPIRED                              ",
"W07113195000110408          NOT ON FILE OR EXPIRED                              ");

			processor.Process();
			Factory.Save();
			tariff1.Reload();
			AssertEquals(new ZDateTime(2008, 11, 4), tariff1.UE_DateTo);

			// from TariffUpdate document: "If there is no as of date, the current date is assumed."
			tariff1.UE_DateTo = ZDateTime.Today;

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "7113195000";
			tariff2.UE_DateFrom = ZDate.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;
			Factory.Save();

			var responseMessage2 = Factory.New<MQEDIMessage>();
			responseMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var processor2 = new ACSTariffHeaderResponseProcessor();
			AddMessageBlocksToProcessor(processor2, "WR",
"W07113192900                NOT ON FILE OR EXPIRED                              ",
"W07113195000                NOT ON FILE OR EXPIRED                              ");

			processor2.Message = responseMessage2;
			processor2.Process();
			Factory.Save();

			tariff1.Reload();
			tariff2.Reload();
			AssertEquals(ZDateTime.Today, tariff1.UE_DateTo);
			AssertEquals(ZDateTime.Today, tariff2.UE_DateTo);

			// Test Different Error Message
			tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "8708997060";
			tariff2.UE_DateFrom = ZDate.BrettsBirthday;
			tariff2.UE_DateTo = ZDateTime.Today;
			Factory.Save();

			responseMessage2 = Factory.New<MQEDIMessage>();
			responseMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;

			processor2 = new ACSTariffHeaderResponseProcessor();
			AddMessageBlocksToProcessor(processor2, "WR",
"W087089970601116098708997060NONE IN FILE OR EXPIRED                             ");

			processor2.Message = responseMessage2;
			processor2.Process();
			Factory.Save();

			tariff2.Reload();
			AssertEquals(new ZDateTime(2009, 11, 16), tariff2.UE_DateTo);
		}

		public void TestQueryOnPartOfTariffShouldNotUpdateAllTariffsStartingWith_CS00086178()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "7010905005";
			tariff.UE_DateFrom = new ZDateTime(2008, 9, 11);
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTimeValue;
			Factory.Save();

			var processor = new ACSTariffHeaderResponseProcessor();
			AddMessageBlocksToProcessor(processor, "WR", "W0701090    072209          NOT ON FILE OR EXPIRED                              ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			processor.Message = responseMessage;

			processor.Process();
			tariff.Reload();

			AssertEquals(ZDateTime.MaxSmallDateTimeValue, tariff.UE_DateTo);
		}

		public void TestLinkToDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();

			var message = Factory.New<MQEDIMessage>();
			message.EM_LinkedObject = declaration;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_MessageText = "B018888XJ5WI                                               ~002874              W 98176101  022808                                                              Y  8888XJ5WI00001";
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "~002874";

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_MessageText = "B018888XJ5WR                                               ~002874              W091021110  051707          NOT ON FILE OR EXPIRED                              Y  8888XJ5WR00017";
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			response.EM_Status = "QUE";
			response.EM_MessageNum = "~002874";
			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();

			response.Reload();
			AssertEquals("PreCondition", "RCV", response.EM_Status);
			AssertEquals("response is attached to Declaration", declaration, response.EM_LinkedObject);
		}
	}
}
