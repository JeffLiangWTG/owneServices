using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class NCTSSubmitDeclarationResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessTRNResponseMessage()
		{
			var messageText = TRMessageTestHelper.GetFileText("SubmitDeclarationResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var queryGUID = TRMessageHelper.GetNodeValue(messageText, "//x:submitdeclarationResponse/return/corrGuid", "http://ws/");
			var nctsHeader = Factory.New<Integration.Customs.TR.ICusInBondHeader>();
			var message = CreateNCTSMessage(nctsHeader.PK, EDIMessage.Direction.Receive, messageText, "10");
			Factory.Save();

			processor.ProcessMessage(message);

			var query = new ZQuery(CusPollingTransactionSchema.CPT_ParentID, message.PK);
			var pollingTransactions = Factory.Load<CusPollingTransaction>(query);
			AssertEquals("There should be one CusPollingTransaction record created", 1, pollingTransactions.Length);

			var pollingTransaction = pollingTransactions[0];
			CombineAssertions(() =>
			{
				AssertEquals(EDIMessage.ApplicationCodes.TRCustoms, pollingTransaction.CPT_ApplicationCode);
				AssertEquals(TRMessageTypes.Codes.TRN, pollingTransaction.CPT_Type);
				AssertEquals(Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN, pollingTransaction.CPT_Status);
				AssertEquals(5, pollingTransaction.CPT_NumberOfAttempts.ToZInt());
				AssertEquals(message.EM_SystemCreateTimeUtc.AddMinutes(1), pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc);
				AssertEquals(queryGUID, message.EM_ApplicationReference);
				AssertEquals(queryGUID, pollingTransaction.CPT_TransactionID);
			});
		}

		public void TestProcessTRNResponseERRMessage()
		{
			var messageText = TRMessageTestHelper.GetFileText("SubmitDeclarationERRResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var queryGUID = TRMessageHelper.GetNodeValue(messageText, "//x:submitdeclarationResponse/return/corrGuid", "http://ws/");
			var nctsHeader = Factory.New<Integration.Customs.TR.ICusInBondHeader>();
			nctsHeader.BH_HeaderType = "D";
			var message = CreateNCTSMessage(nctsHeader.PK, EDIMessage.Direction.Receive, messageText, "10");
			Factory.Save();

			processor.ProcessMessage(message);

			CombineAssertions("Error Status", () =>
			{
				AssertEquals("Query GUID", ZString.Empty, queryGUID);
				AssertEquals("BM_CustomsStatus", "DRJ", nctsHeader.BM_CustomsStatus);
			});
		}

		public void TestMessageInterpretation()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "YK";
			staff.GS_LoginName = "Yusuf";
			staff.GS_EmailAddress = "yusuf.kamhi@wisetechglobal.com";
			staff.GS_WorkingLanguage = Enterprise.Core.Constants.Languages.English;
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var messageText = TRMessageTestHelper.GetFileText("SubmitDeclarationResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var queryGUID = TRMessageHelper.GetNodeValue(messageText, "//x:submitdeclarationResponse/return/corrGuid", "http://ws/");
			var nctsHeader = Factory.New<Integration.Customs.TR.ICusInBondHeader>();
			var messageRequest = CreateNCTSMessage(nctsHeader.PK, EDIMessage.Direction.Transmit, messageText, "10");
			var messageResponse = CreateNCTSMessage(nctsHeader.PK, EDIMessage.Direction.Receive, messageText, "20");
			Factory.Save();

			processor.ProcessMessage(messageResponse);
			var messageInterpretation = messageResponse.EM_MessageInterpretation;

			CombineAssertions("EM_MessageInterpretation", () =>
			{
				Assert("EM_MessageInterpretation should contains 'Caption'", messageInterpretation.Contains("NCTS Query GUID for job NCT00000001 sent"));
				Assert("EM_MessageInterpretation should contains 'Query GUID'", messageInterpretation.Contains("<td>Query GUID:</td><td>5082D1A82A01C3A6E0536803A8C0E158</td>"));
			});
		}

		NCTSMessage CreateNCTSMessage(ZGuid linkID, ZString direction, ZString messageText, ZString messageNum)
		{
			var message = Factory.New<NCTSMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = direction;
			message.EM_MessageType = TRMessageTypes.Codes.TRN;
			message.EM_MessageText = messageText;
			message.EM_MessageNum = messageNum;
			message.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = linkID;
			message.EM_SystemCreateUser = "YK";
			return message;
		}

		NCTSSubmitDeclarationResponseMessageProcessor processor;

		protected override void SetUp()
		{
			base.SetUp();

			processor = new NCTSSubmitDeclarationResponseMessageProcessor(new LoggingInformation());
		}
	}
}
