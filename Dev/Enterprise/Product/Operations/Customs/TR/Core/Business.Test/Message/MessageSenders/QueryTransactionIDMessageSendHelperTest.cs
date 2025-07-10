using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	class QueryTransactionIDMessageSendHelperTest : TestCaseWithFactory
	{
		public void TestGetPollingTransactionFilter()
		{
			var query = QueryTransactionIDMessageSendHelper.GetPollingTransactionFilter(Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN,
																						TRMessageTypes.Codes.TRO, TRMessageTypes.Codes.TRE, TRMessageTypes.Codes.T2O,
																						TRMessageTypes.Codes.DKO, TRMessageTypes.Codes.DT2, TRMessageTypes.Codes.DTE);
			var pollingTransaction = Factory.New<CusPollingTransaction>();
			pollingTransaction.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction.CPT_Type = TRMessageTypes.Codes.TRO;
			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
			pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow.AddDays(-1);
			AssertEquals("TRO OPN should match", true, pollingTransaction.MatchesFilter(query));

			pollingTransaction.CPT_Type = TRMessageTypes.Codes.T2O;
			AssertEquals("T2O OPN should match", true, pollingTransaction.MatchesFilter(query));

			pollingTransaction.CPT_Type = TRMessageTypes.Codes.TRE;
			AssertEquals("TRE OPN should match", true, pollingTransaction.MatchesFilter(query));

			pollingTransaction.CPT_Type = TRMessageTypes.Codes.CPL;
			AssertEquals("CPL should not match", false, pollingTransaction.MatchesFilter(query));

			pollingTransaction.CPT_Type = TRMessageTypes.Codes.TRE;
			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			AssertEquals("PND should not match", false, pollingTransaction.MatchesFilter(query));

			pollingTransaction.CPT_Type = TRMessageTypes.Codes.DKO;
			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
			AssertEquals("DKO OPN should match", true, pollingTransaction.MatchesFilter(query));

			pollingTransaction.CPT_Type = TRMessageTypes.Codes.DT2;
			AssertEquals("DT2 OPN should match", true, pollingTransaction.MatchesFilter(query));

			pollingTransaction.CPT_Type = TRMessageTypes.Codes.DTE;
			AssertEquals("DTE OPN should match", true, pollingTransaction.MatchesFilter(query));
		}

		public void TestSendQueryMessage_None()
		{
			SendQueryMessage();
			AssertEquals("No trans", "No polling transaction record needs to be processed", GetLogsAsString);
		}

		public void TestSendQueryMessage_Manifest()
		{
			var manifest = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			manifest.AMA_RN_NKCountry = "TR";
			manifest.AMA_ManifestType = "DENITH";

			AssertPollingDetailsAfterSend(TRMessageTypes.Codes.TRO, AsycudaManifestHeaderSchema.Constants.TableName, manifest.PK);
		}

		public void TestSendQueryMessage_Manifest_T2O()
		{
			var manifest = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			manifest.AMA_RN_NKCountry = "TR";
			manifest.AMA_ManifestType = "DENITH";

			var sessionGuid = ZGuid.NewZGuid();
			var requestMessage = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			requestMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			requestMessage.EM_LinkUniqueID = manifest.PK;
			requestMessage.GetMessageReferenceNumberToReturn = "1";
			requestMessage.EM_MessageType = TRMessageTypes.Codes.TRO;
			var reqInterchange = Factory.NewMoq<EDIInterchange>();
			reqInterchange.Object.EI_InterchangeType = TRMessageTypes.Codes.TRO;
			reqInterchange.Object.EI_InterchangeNum = "001";
			reqInterchange.Object.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			reqInterchange.Object.ContainedMessages.Add(requestMessage);
			reqInterchange.Object.EI_SessionGUID = sessionGuid;

			var responseMessage = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			responseMessage.GetMessageReferenceNumberToReturn = "2";
			responseMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			responseMessage.EM_LinkUniqueID = manifest.PK;
			responseMessage.EM_MessageType = TRMessageTypes.Codes.TRO;
			var resInterchange = Factory.NewMoq<EDIInterchange>();
			resInterchange.Object.EI_InterchangeNum = "002";
			resInterchange.Object.ContainedMessages.Add(responseMessage);
			resInterchange.Object.EI_SessionGUID = sessionGuid;

			var requestMessage2 = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			requestMessage2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			requestMessage2.EM_LinkUniqueID = manifest.PK;
			requestMessage2.GetMessageReferenceNumberToReturn = "3";
			requestMessage2.EM_MessageType = TRMessageTypes.Codes.T2O;
			var reqInterchange2 = Factory.NewMoq<EDIInterchange>();
			reqInterchange2.Object.EI_InterchangeNum = "003";
			reqInterchange2.Object.EI_InterchangeType = TRMessageTypes.Codes.TRO;
			reqInterchange2.Object.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			reqInterchange2.Object.ContainedMessages.Add(requestMessage2);
			reqInterchange2.Object.EI_SessionGUID = sessionGuid;

			var responseMessage2 = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			responseMessage2.GetMessageReferenceNumberToReturn = "4";
			responseMessage2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			responseMessage2.EM_LinkUniqueID = manifest.PK;
			responseMessage2.EM_MessageType = TRMessageTypes.Codes.T2O;
			var resInterchange2 = Factory.NewMoq<EDIInterchange>();
			resInterchange2.Object.EI_InterchangeNum = "004";
			resInterchange2.Object.ContainedMessages.Add(responseMessage2);
			resInterchange2.Object.EI_SessionGUID = sessionGuid;

			var pollingTransaction2 = Factory.New<CusPollingTransaction>();
			pollingTransaction2.CPT_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction2.CPT_Type = TRMessageTypes.Codes.T2O;
			pollingTransaction2.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
			pollingTransaction2.CPT_NumberOfAttempts = 5;
			pollingTransaction2.CPT_EarliestTimeOfNextAttemptUtc = new ZDateTime(2022, 1, 1);
			pollingTransaction2.CPT_TransactionID = sessionGuid.ToString();
			pollingTransaction2.CPT_ParentID = responseMessage2.PK;
			Factory.Save();

			SendQueryMessage();

			var newFactory = new BusinessObjectFactory();
			var pollTrans = newFactory.LoadTop1<CusPollingTransaction>(new ZQuery(CusPollingTransactionSchema.CPT_Type, TRMessageTypes.Codes.T2O));
			AssertNotNull("Polling trans not found", pollTrans);
			AssertEquals("Status Updated", Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND, pollTrans.CPT_Status);
			AssertEquals("Reason Updated", "Pending", pollTrans.CPT_StatusReason);
			AssertEquals("Success", $"Polling transaction processed, TransactionID: {sessionGuid.ToString()}. Message sent successfully.\r\n", GetLogsAsString);
		}

		public void TestSendQueryMessage_ETrade()
		{
			var eTradeHeader = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			AssertPollingDetailsAfterSend(TRMessageTypes.Codes.TRE, AsycudaManifestHeaderSchema.Constants.TableName, eTradeHeader.PK);
		}

		public void TestSendQueryMessage_Ncts_TRN()
		{
			var nctsHeader = Factory.New<Integration.Customs.TR.ICusInBondHeader>();
			AssertPollingDetailsAfterSend(TRMessageTypes.Codes.TRN, CusInBondHeaderSchema.Constants.TableName, nctsHeader.PK);
		}

		public void TestSendQueryMessage_Ncts_T1N()
		{
			var nctsHeader = Factory.New<Integration.Customs.TR.ICusInBondHeader>();

			var requestMessageTRN = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			requestMessageTRN.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			requestMessageTRN.EM_LinkUniqueID = nctsHeader.PK;
			requestMessageTRN.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			requestMessageTRN.GetMessageReferenceNumberToReturn = "3";
			requestMessageTRN.EM_MessageType = "TRN";

			AssertPollingDetailsAfterSend(TRMessageTypes.Codes.T1N, CusInBondHeaderSchema.Constants.TableName, nctsHeader.PK);
		}

		public void TestSendQueryMessageImportExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertPollingDetailsAfterSend(TRMessageTypes.Codes.DKO, CusEntryHeaderSchema.Constants.TableName, cusEntryHeader.PK);
			AssertPollingDetailsAfterSend(TRMessageTypes.Codes.DT2, CusEntryHeaderSchema.Constants.TableName, cusEntryHeader.PK);
			AssertPollingDetailsAfterSend(TRMessageTypes.Codes.DTE, CusEntryHeaderSchema.Constants.TableName, cusEntryHeader.PK);
		}

		public void TestSendMessageTypesValues()
		{
			var expected = new[] { "TRO", "TRE", "TRN", "T1N", "T2O", "DKO", "DT2", "DTE" };
			logger.ClearLogs();
			var sender = new QueryTransactionIDMessageSendHelper(logger);
			var result = sender.SendMessageTypes;

			CombineAssertions("Send Message Types", () =>
			{
				AssertNotNull(result);
				AssertContainsExactElementsInAnyOrder(expected, result);
			});
		}

		void AssertPollingDetailsAfterSend(string code, string parentTable, ZGuid parentPK)
		{
			if (code == TRMessageTypes.Codes.DKO || code == TRMessageTypes.Codes.DTE)
			{
				CreatePollingTransactionWithOriginalMessage(code, parentTable, parentPK);
			}
			else
			{
				CreatePollingTransaction(code, parentTable, parentPK);
			}

			SendQueryMessage();

			var newFactory = new BusinessObjectFactory();
			var pollTrans = newFactory.LoadTop1<CusPollingTransaction>(new ZQuery(CusPollingTransactionSchema.CPT_Type, code));
			AssertNotNull("Polling trans not found", pollTrans);
			AssertEquals("Status Updated", Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND, pollTrans.CPT_Status);
			AssertEquals("Reason Updated", "Pending", pollTrans.CPT_StatusReason);
			AssertEquals("Success", $"Polling transaction processed, TransactionID: TransId-{code}. Message sent successfully.\r\n", GetLogsAsString);
		}

		string GetLogsAsString => string.Join("\n ", logger.Logs.Select(x => x.Message));

		void CreatePollingTransaction(string messageType, string parentTable, ZGuid parentPK)
		{
			var sessionGuid = ZGuid.NewZGuid();
			var requestMessage = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			requestMessage.EM_LinkTable = parentTable;
			requestMessage.EM_LinkUniqueID = parentPK;
			requestMessage.GetMessageReferenceNumberToReturn = "1";
			requestMessage.EM_MessageType = messageType;
			var reqInterchange = Factory.NewMoq<EDIInterchange>();
			reqInterchange.Object.EI_InterchangeNum = messageType + "001";
			reqInterchange.Object.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			reqInterchange.Object.ContainedMessages.Add(requestMessage);
			reqInterchange.Object.EI_SessionGUID = sessionGuid;
			reqInterchange.Object.EI_InterchangeType = messageType;

			var responseMessage = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			responseMessage.GetMessageReferenceNumberToReturn = "2";
			responseMessage.EM_LinkTable = parentTable;
			responseMessage.EM_LinkUniqueID = parentPK;
			responseMessage.EM_MessageType = messageType;
			var resInterchange = Factory.NewMoq<EDIInterchange>();
			resInterchange.Object.EI_InterchangeNum = messageType + "002";
			resInterchange.Object.ContainedMessages.Add(responseMessage);
			resInterchange.Object.EI_SessionGUID = sessionGuid;
			resInterchange.Object.EI_InterchangeType = messageType;
			CreateCusPollingTransaction(messageType, responseMessage);
			Factory.Save();
		}

		void CreatePollingTransactionWithOriginalMessage(string messageType, string parentTable, ZGuid parentPK)
		{
			var sessionGuid = ZGuid.NewZGuid();
			var requestMessage = Factory.New<TRImportExportMessage>();
			requestMessage.EM_LinkTable = parentTable;
			requestMessage.EM_LinkUniqueID = parentPK;
			requestMessage.EM_MessageNum = "1";
			requestMessage.EM_MessageType = messageType;
			var reqInterchange = Factory.NewMoq<EDIInterchange>();
			reqInterchange.Object.EI_InterchangeNum = messageType + "001";
			reqInterchange.Object.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			reqInterchange.Object.ContainedMessages.Add(requestMessage);
			reqInterchange.Object.EI_SessionGUID = sessionGuid;
			reqInterchange.Object.EI_InterchangeType = messageType;

			var responseMessage = Factory.New<TRImportExportMessage>();
			responseMessage.EM_MessageNum = "2";
			responseMessage.EM_LinkTable = parentTable;
			responseMessage.EM_LinkUniqueID = parentPK;
			responseMessage.EM_MessageType = messageType;
			var resInterchange = Factory.NewMoq<EDIInterchange>();
			resInterchange.Object.EI_InterchangeNum = messageType + "002";
			resInterchange.Object.ContainedMessages.Add(responseMessage);
			resInterchange.Object.EI_SessionGUID = sessionGuid;
			resInterchange.Object.EI_InterchangeType = messageType;

			CreateCusPollingTransaction(messageType, responseMessage);

			Factory.Save();
		}

		void CreateCusPollingTransaction(string messageType, EDIMessage responseMessage)
		{
			var pollingTransaction = Factory.New<CusPollingTransaction>();
			pollingTransaction.CPT_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction.CPT_Type = messageType;
			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN;
			pollingTransaction.CPT_NumberOfAttempts = 5;
			pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = new ZDateTime(2022, 1, 1);
			pollingTransaction.CPT_TransactionID = "TransId-" + messageType;
			pollingTransaction.CPT_ParentID = responseMessage.PK;
		}

		void SendQueryMessage()
		{
			logger.ClearLogs();
			var sender = new QueryTransactionIDMessageSendHelper(logger);

			sender.SendQueryMessage(new System.Threading.CancellationToken());
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new BatchProcessor.LoggingInformation();
		}

		BatchProcessor.LoggingInformation logger;
	}

	public class DummyEDIMessage_QueryTransactionIDMessageSendHelperTest : EDIMessage
	{
		public DummyEDIMessage_QueryTransactionIDMessageSendHelperTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public string GetMessageReferenceNumberToReturn { get; set; }

		protected override string GetMessageReferenceNumber()
		{
			return GetMessageReferenceNumberToReturn;
		}
	}
}
