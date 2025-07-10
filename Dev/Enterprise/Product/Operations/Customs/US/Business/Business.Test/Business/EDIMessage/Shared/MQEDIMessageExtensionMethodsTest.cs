using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class MQEDIMessageExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestIsSTUMessageAndAccepted()
		{
			var transmittedMessage = Factory.New<MQEDIMessage>();
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction;
			transmittedMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			transmittedMessage.EM_MessageText = "B018888XJ5HP                                               30449                H8888XJ5 700078811                                                              Y  8888XJ5HP00001";
			transmittedMessage.EM_MessageNum = "30449";
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_MessageText = "B018888XJ5HT                                               30449                H18888XJ5 7000788157FSUMM REMVD FR STMT, DOCS NOW REQD        1      B00151238  Y  8888XJ5HT00001";
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransactionResponse;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			responseMessage.EM_MessageNum = "30449";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertNotNull(transmittedMessage.RelatedMessage);

			Assert(transmittedMessage.IsSTUMessageAndAccepted());
		}

		public void TestIsSTUMessageAndAcceptedForSuccess()
		{
			var transmittedMessage = Factory.New<MQEDIMessage>();
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction;
			transmittedMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			transmittedMessage.EM_MessageNum = "~000771";
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmittedMessage.EM_MessageText =
"B013002221HP                                  3001221  1   JJBSFOSFO_413325     " +
"H3002221 362471316      2901                                                    " +
"Y  3002221HP00001                                                               ";

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransactionResponse;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			responseMessage.EM_MessageNum = "~000771";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			responseMessage.EM_MessageText =
"B013002221HT                                  3001221  1   JJBSFOSFO_413325     " +
"H13002221 362471312GBDATA REPLACED AS REQUESTED               6121615X0002499029" +
"Y  3002221HT00001                                                               ";

			AssertNotNull(transmittedMessage.RelatedMessage);
			Assert(transmittedMessage.IsSTUMessageAndAccepted());
		}

		public void TestIsSTUMessageAndAcceptedForFailure()
		{
			var transmittedMessage = Factory.New<MQEDIMessage>();
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction;
			transmittedMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			transmittedMessage.EM_MessageNum = "~000771";
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmittedMessage.EM_MessageText =
"B018888XJ5HP                                               ~000771              " +
"H8888XJ5 100011277092410                                                        " +
"Y  8888XJ5HP00001                                                               ";

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransactionResponse;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			responseMessage.EM_MessageNum = "~000771";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			responseMessage.EM_MessageText =
"B012704836HT                                  5501836  1   ~000771              " +
"H18888XJ5 10001127HP8PERIODIC MONTH INVALID                   7092410S00002189  " +
"H28888XJ5 100011272710260G4500008701951                                         " +
"Y  2704836HT00002                                                               ";

			AssertNotNull(transmittedMessage.RelatedMessage);
			Assert(!transmittedMessage.IsSTUMessageAndAccepted());
		}

		public void TestIsSTUMessageAndAcceptedFor96UFailure()
		{
			var transmittedMessage = Factory.New<MQEDIMessage>();
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction;
			transmittedMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			transmittedMessage.EM_MessageNum = "~000771";
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmittedMessage.EM_MessageText =
"B01270498GHP                                  520198G  1   WHBIYHIYH_6984       " +
"H270498G 000252777030114  02                                                    " +
"Y  270498GHP00001                                                               ";

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransactionResponse;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			responseMessage.EM_MessageNum = "~000771";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			responseMessage.EM_MessageText =
"B01270498GHT                                  520198G  1   WHBIYHIYH_6984       " +
"H1270498G 0002527796UDATE IS WEEKEND DAY                      7030114B00002545  " +
"H2270498G 000252772714014BVD00007264707                                         " +
"Y  270498GHT00002                                                               ";

			AssertNotNull(transmittedMessage.RelatedMessage);
			Assert(!transmittedMessage.IsSTUMessageAndAccepted());
		}

		public void TestIsSTUMessageAndAcceptedFor96TFailure()
		{
			var transmittedMessage = Factory.New<MQEDIMessage>();
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction;
			transmittedMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			transmittedMessage.EM_MessageNum = "~000771";
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmittedMessage.EM_MessageText =
"B01270498GHP                                  520198G  1   WHBIYHIYH_6984       " +
"H270498G 000252777030114  02                                                    " +
"Y  270498GHP00001                                                               ";

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransactionResponse;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			responseMessage.EM_MessageNum = "~000771";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			responseMessage.EM_MessageText =
"B01270498GHT                                  520198G  1   WHBIYHIYH_6984       " +
"H1270498G 0002527796TDATE IS HOLIDAY                          7030114B00002545  " +
"H2270498G 000252772714014BVD00007264707                                         " +
"Y  270498GHT00002                                                               ";

			AssertNotNull(transmittedMessage.RelatedMessage);
			Assert(!transmittedMessage.IsSTUMessageAndAccepted());
		}

		public void TestBranchCodeRequiredRejectionIsRecognised()
		{
			var transmittedMessage = Factory.New<MQEDIMessage>();
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction;
			transmittedMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			transmittedMessage.EM_MessageNum = "~000771";
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmittedMessage.EM_MessageText =
"B018888XJ5HP                                               ~000771              " +
"H8888XJ5 011492532080112                                                        " +
"Y  8888XJ5HP00001                                                               ";

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.StatementDeleteTransactionResponse;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			responseMessage.EM_MessageNum = "~000771";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			responseMessage.EM_MessageText =
"B012704836HT                                  5501836  1   ~000771              " +
"H18888XJ5 01149253C04BRANCH CODE REQUIRED                     2080112000114925  " +
"H28888XJ5 01149253371220702600000030408                                         " +
"Y  2704836HT00002                                                               ";

			AssertNotNull(transmittedMessage.RelatedMessage);
			Assert(!transmittedMessage.IsSTUMessageAndAccepted());
		}

		public void TestIsSTUMessageAndAcceptedForACEStatement()
		{
			var transmittedMessage = Factory.New<MQEDIMessage>();
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.StatementUpdate;
			transmittedMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			transmittedMessage.EM_MessageText = "B018888XJ5SU                                               30449                H8888XJ5 700078811                                                              Y  8888XJ5SU00001";
			transmittedMessage.EM_MessageNum = "30449";
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_MessageText = "B018888XJ5SQ                                               30449                H2 XJ5                                                                          Y  8888XJ5SQ00001";
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.StatementUpdateResponse;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			responseMessage.EM_MessageNum = "30449";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertNotNull(transmittedMessage.RelatedMessage);

			Assert(transmittedMessage.IsSTUMessageAndAccepted());
		}

		public void TestIsSTUMessageAndAcceptedForACEFailure()
		{
			var transmittedMessage = Factory.New<MQEDIMessage>();
			transmittedMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			transmittedMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.StatementUpdate;
			transmittedMessage.EM_MessageText = "B018888XJ5HP                                               30449                H8888XJ5 700078811                                                              Y  8888XJ5HP00001";
			transmittedMessage.EM_MessageNum = "30449";
			transmittedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			responseMessage.EM_MessageText = "B018888XJ5HT                                               30449                H2FXJ5                                                                          Y  8888XJ5HT00001";
			responseMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.StatementUpdateResponse;
			responseMessage.EM_MessageSubType = EM_MessageSubTypeList.Codes.StatementUpdateMessage;
			responseMessage.EM_MessageNum = "30449";
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertNotNull(transmittedMessage.RelatedMessage);

			Assert(!transmittedMessage.IsSTUMessageAndAccepted());
		}
	}
}
