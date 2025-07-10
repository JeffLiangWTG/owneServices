using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class NCTSGetMessagesListByGuidResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessMessage()
		{
			var queryGUID = "5082D1A82A01C3A6E0536803A8C0E158";
			var trackingID = new ZGuid("A140144A-4825-4424-8884-821F73BEF504");
			var trnResponseMessageText = TRMessageTestHelper.GetFileText("SubmitDeclarationResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var trnResponseMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.TRN, EDIMessage.Direction.Receive, EDIMessage.Status.ProcessedOK, trnResponseMessageText, "1", queryGUID);

			var t1nRequestMessageText = TRMessageTestHelper.GetFileText("GetMessagesListByGuid.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var t1nRequestMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.T1N, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, t1nRequestMessageText, "2", queryGUID);
			var t1nRequestInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.T1N, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, t1nRequestMessageText, trackingID);
			t1nRequestMessage.EM_EI = t1nRequestInterchange.PK;

			var t1nResponseMessageText = TRMessageTestHelper.GetFileText("GetMessagesListByGuidResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var t1nResponseMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.T1N, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, t1nResponseMessageText, "3", "");
			var t1nResponseInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.T1N, EDIInterchange.Direction.Receive, EDIInterchange.Status.Received, t1nResponseMessageText, trackingID);
			t1nResponseMessage.EM_EI = t1nResponseInterchange.PK;

			var nctsHeader = Factory.New<Integration.Customs.TR.ICusInBondHeader>();
			t1nRequestMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			t1nRequestMessage.EM_LinkUniqueID = nctsHeader.PK;
			t1nResponseMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			t1nResponseMessage.EM_LinkUniqueID = nctsHeader.PK;

			var trnPollingTransaction = Factory.New<CusPollingTransaction>();
			trnPollingTransaction.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			trnPollingTransaction.CPT_Type = TRMessageTypes.Codes.TRN;
			trnPollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			trnPollingTransaction.CPT_NumberOfAttempts = 5;
			trnPollingTransaction.CPT_TransactionID = queryGUID;
			trnPollingTransaction.CPT_ParentID = trnResponseMessage.PK;
			Factory.Save();

			Processor.ProcessMessage(t1nResponseMessage);

			var query = new ZQuery(CusPollingTransactionSchema.CPT_ParentID, t1nResponseMessage.PK);
			query.OrderBy = CusPollingTransactionSchema.CPT_TransactionID.Name;
			var pollingTransactions = Factory.Load<CusPollingTransaction>(query);
			var listValues = TRMessageHelper.GetNodeValues(t1nResponseMessageText, new List<ZString>() { "Envelope", "Body", "getMessagesListByGuidResponse", "return" }, "list").OrderBy(x => x).ToArray();
			CombineAssertions("Create one record for each list element.", () =>
			{
				AssertEquals(Core.Constants.Customs.CusPollingTransactionStatus.Codes.CLS, trnPollingTransaction.CPT_Status);

				AssertEquals(listValues.Length, pollingTransactions.Length);

				for (int i = 0; i < pollingTransactions.Length; i++)
				{
					var pollingTransaction = pollingTransactions[i];
					var index = listValues[i];

					AssertEquals("CPT_ApplicationCode", EDIMessage.ApplicationCodes.TRCustoms, pollingTransaction.CPT_ApplicationCode);
					AssertEquals("CPT_Type", TRMessageTypes.Codes.T1N, pollingTransaction.CPT_Type);
					AssertEquals("CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN, pollingTransaction.CPT_Status);
					AssertEquals("CPT_NumberOfAttempts", 5, pollingTransaction.CPT_NumberOfAttempts.ToZInt());
					AssertEquals("CPT_EarliestTimeOfNextAttemptUtc", t1nResponseMessage.EM_SystemCreateTimeUtc.AddMinutes(TRMessageConstants.TransactionPollingDelay), pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc);
					AssertEquals("CPT_TransactionID", index, pollingTransaction.CPT_TransactionID);
				}
			});
		}

		public void TestProcessERRMessage()
		{
			var queryGUID = "5082D1A82A01C3A6E0536803A8C0E158";
			var trackingID = new ZGuid("A140144A-4825-4424-8884-821F73BEF504");
			var trnResponseMessageText = TRMessageTestHelper.GetFileText("SubmitDeclarationResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var trnResponseMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.TRN, EDIMessage.Direction.Receive, EDIMessage.Status.ProcessedOK, trnResponseMessageText, "1", queryGUID);

			var t1nRequestMessageText = TRMessageTestHelper.GetFileText("GetMessagesListByGuid.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var t1nRequestMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.T1N, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, t1nRequestMessageText, "2", queryGUID);
			var t1nRequestInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.T1N, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, t1nRequestMessageText, trackingID);
			t1nRequestMessage.EM_EI = t1nRequestInterchange.PK;

			var t1nResponseMessageText = TRMessageTestHelper.GetFileText("GetMessagesListByGuidERRResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var t1nResponseMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.T1N, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, t1nResponseMessageText, "3", "");
			var t1nResponseInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.T1N, EDIInterchange.Direction.Receive, EDIInterchange.Status.Received, t1nResponseMessageText, trackingID);
			t1nResponseMessage.EM_EI = t1nResponseInterchange.PK;

			var nctsHeader = Factory.New<Integration.Customs.TR.ICusInBondHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = "D";

			t1nRequestMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			t1nRequestMessage.EM_LinkUniqueID = nctsHeader.PK;
			t1nResponseMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			t1nResponseMessage.EM_LinkUniqueID = nctsHeader.PK;

			var trnPollingTransaction = Factory.New<CusPollingTransaction>();
			trnPollingTransaction.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			trnPollingTransaction.CPT_Type = TRMessageTypes.Codes.TRN;
			trnPollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			trnPollingTransaction.CPT_NumberOfAttempts = 5;
			trnPollingTransaction.CPT_TransactionID = queryGUID;
			trnPollingTransaction.CPT_ParentID = trnResponseMessage.PK;
			Factory.Save();

			Processor.ProcessMessage(t1nResponseMessage);

			CombineAssertions("Error Status", () =>
			{
				AssertEquals("BH_MessageStatus", "ERR", nctsHeader.BH_MessageStatus);
				AssertEquals("BM_CustomsStatus", "DRJ", nctsHeader.BM_CustomsStatus);
			});
		}

		public void TestMessageInterpretation()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "YK1";
			staff1.GS_LoginName = "Yusuf1";
			staff1.GS_EmailAddress = "yusuf.kamhi@wisetechglobal.com";
			var user1 = TRGlbStaffWrapper.Get(staff1).TRBPassword;
			user1.GP_UserID = "20201224104";
			user1.CurrentDecryptedPassword = "12345678";
			var staff2 = group.Staff.AddNew();
			staff2.GS_Code = "YK2";
			staff2.GS_LoginName = "Yusuf2";
			staff2.GS_EmailAddress = "yusuf.kamhi@wisetechglobal.com";
			var user2 = TRGlbStaffWrapper.Get(staff2).TRBPassword;
			user2.GP_UserID = "20201224104";
			user2.CurrentDecryptedPassword = "12345678";

			var errorCode = "000";
			var errorDesc = "Success";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("TR", "Turkish");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("NCTER", "NCTS Errors", Core.Constants.CountryCodes.Turkey);
			var addedCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, "NCTER", errorCode, errorDesc, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListLanguage(addedCode, "TR", "Başarılı");
			Factory.Save();

			var queryGUID = "5082D1A82A01C3A6E0536803A8C0E158";
			var trackingID = new ZGuid("A140144A-4825-4424-8884-821F73BEF504");
			var nctsHeader = Factory.New<Integration.Customs.TR.ICusInBondHeader>();

			var trnRequestMessageText = "~Test";
			var trnRequestMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.TRN, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, trnRequestMessageText, "1", queryGUID);
			var trnRequestInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.TRN, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, trnRequestMessageText, trackingID);
			trnRequestMessage.EM_EI = trnRequestInterchange.PK;
			trnRequestMessage.EM_SystemCreateUser = "YK1";

			var trnResponseMessageText = TRMessageTestHelper.GetFileText("SubmitDeclarationResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var trnResponseMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.TRN, EDIMessage.Direction.Receive, EDIMessage.Status.ProcessedOK, trnResponseMessageText, "2", queryGUID);

			var t1nRequestMessageText = TRMessageTestHelper.GetFileText("GetMessagesListByGuid.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var t1nRequestMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.T1N, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, t1nRequestMessageText, "3", queryGUID);
			var t1nRequestInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.T1N, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, t1nRequestMessageText, trackingID);
			t1nRequestMessage.EM_EI = t1nRequestInterchange.PK;

			var t1nResponseMessageText = TRMessageTestHelper.GetFileText("GetMessagesListByGuidResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var t1nResponseMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.T1N, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, t1nResponseMessageText, "4", "");
			var t1nResponseInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.T1N, EDIInterchange.Direction.Receive, EDIInterchange.Status.Received, t1nResponseMessageText, trackingID);
			t1nResponseMessage.EM_EI = t1nResponseInterchange.PK;
			t1nResponseMessage.EM_MessageOwner = "YK2";

			trnRequestMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			trnRequestMessage.EM_LinkUniqueID = nctsHeader.PK;
			t1nRequestMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			t1nRequestMessage.EM_LinkUniqueID = nctsHeader.PK;
			t1nResponseMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			t1nResponseMessage.EM_LinkUniqueID = nctsHeader.PK;

			var trnPollingTransaction = Factory.New<CusPollingTransaction>();
			trnPollingTransaction.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			trnPollingTransaction.CPT_Type = TRMessageTypes.Codes.TRN;
			trnPollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			trnPollingTransaction.CPT_NumberOfAttempts = 5;
			trnPollingTransaction.CPT_TransactionID = queryGUID;
			trnPollingTransaction.CPT_ParentID = trnResponseMessage.PK;
			Factory.Save();
			
			Processor.ProcessMessage(t1nResponseMessage);
			var messageInterpretation = t1nResponseMessage.EM_MessageInterpretation;

			CombineAssertions("EM_MessageInterpretation", () =>
			{
				Assert("EM_MessageInterpretation should contains 'Caption'", messageInterpretation.Contains("NCTS Get Messages List Message for job NCT00000001 By Guid Response Message Processor"));
				Assert("EM_MessageInterpretation should contains 'Labels | Message Code'", messageInterpretation.Contains(">Message Code<"));
				Assert("EM_MessageInterpretation should contains 'Labels | Description'", messageInterpretation.Contains(">Description<"));
				Assert("EM_MessageInterpretation should contains 'Labels | Index No'", messageInterpretation.Contains(">Index No<"));
				Assert("EM_MessageInterpretation should contains 'Error Code", messageInterpretation.Contains("<td>000</td>"));
				Assert("EM_MessageInterpretation should contains 'Success'", messageInterpretation.Contains("<td>Success</td>"));
				Assert("EM_MessageInterpretation should contains 'Indexses'", messageInterpretation.Contains("<td>39906267<br>39906263<br>39916956<br>39903172<br>39906268<br>39916957</td>"));
			});
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("NCTS Get Messages List By Guid Response Message Processor", Processor.MessageFriendlyName);
		}

		public void TestSendEmail()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "YK1";
			staff1.GS_LoginName = "Yusuf1";
			staff1.GS_EmailAddress = "yusuf.kamhi@wisetechglobal.com";
			var user1 = TRGlbStaffWrapper.Get(staff1).TRBPassword;
			user1.GP_UserID = "20201224104";
			user1.CurrentDecryptedPassword = "12345678";
			var staff2 = group.Staff.AddNew();
			staff2.GS_Code = "YK2";
			staff2.GS_LoginName = "Yusuf2";
			staff2.GS_EmailAddress = "yusuf.kamhi@wisetechglobal.com";
			var user2 = TRGlbStaffWrapper.Get(staff2).TRBPassword;
			user2.GP_UserID = "20201224104";
			user2.CurrentDecryptedPassword = "12345678";

			var errorCode = "000";
			var errorDesc = "Success";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("TR", "Turkish");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("NCTER", "NCTS Errors", Core.Constants.CountryCodes.Turkey);
			var addedCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, "NCTER", errorCode, errorDesc, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeListLanguage(addedCode, "TR", "Başarılı");
			Factory.Save();

			var queryGUID = "5082D1A82A01C3A6E0536803A8C0E158";
			var trackingID = new ZGuid("A140144A-4825-4424-8884-821F73BEF504");
			var nctsHeader = Factory.New<Integration.Customs.TR.ICusInBondHeader>();

			var trnRequestMessageText = "~Test";
			var trnRequestMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.TRN, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, trnRequestMessageText, "1", queryGUID);
			var trnRequestInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.TRN, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, trnRequestMessageText, trackingID);
			trnRequestMessage.EM_EI = trnRequestInterchange.PK;
			trnRequestMessage.EM_SystemCreateUser = "YK1";

			var trnResponseMessageText = TRMessageTestHelper.GetFileText("SubmitDeclarationResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var trnResponseMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.TRN, EDIMessage.Direction.Receive, EDIMessage.Status.ProcessedOK, trnResponseMessageText, "2", queryGUID);

			var t1nRequestMessageText = TRMessageTestHelper.GetFileText("GetMessagesListByGuid.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var t1nRequestMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.T1N, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, t1nRequestMessageText, "3", queryGUID);
			var t1nRequestInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.T1N, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, t1nRequestMessageText, trackingID);
			t1nRequestMessage.EM_EI = t1nRequestInterchange.PK;

			var t1nResponseMessageText = TRMessageTestHelper.GetFileText("GetMessagesListByGuidResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var t1nResponseMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.T1N, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, t1nResponseMessageText, "4", "");
			var t1nResponseInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.T1N, EDIInterchange.Direction.Receive, EDIInterchange.Status.Received, t1nResponseMessageText, trackingID);
			t1nResponseMessage.EM_EI = t1nResponseInterchange.PK;
			t1nResponseMessage.EM_MessageOwner = "YK2";

			trnRequestMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			trnRequestMessage.EM_LinkUniqueID = nctsHeader.PK;
			t1nRequestMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			t1nRequestMessage.EM_LinkUniqueID = nctsHeader.PK;
			t1nResponseMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			t1nResponseMessage.EM_LinkUniqueID = nctsHeader.PK;

			var trnPollingTransaction = Factory.New<CusPollingTransaction>();
			trnPollingTransaction.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			trnPollingTransaction.CPT_Type = TRMessageTypes.Codes.TRN;
			trnPollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			trnPollingTransaction.CPT_NumberOfAttempts = 5;
			trnPollingTransaction.CPT_TransactionID = queryGUID;
			trnPollingTransaction.CPT_ParentID = trnResponseMessage.PK;
			Factory.Save();

			Processor.ProcessMessage(t1nResponseMessage);
			var messageInterpretation = t1nResponseMessage.EM_MessageInterpretation;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "NCTS Get Messages List By Guid Response Response for " + nctsHeader.BH_JobReference);
			var bodyText = email.Body;

			CombineAssertions("E-mail Content", () =>
			{
				Assert("e-Mail should contains 'Caption'", bodyText.Contains("NCTS Get Messages List By Guid Response error"));
				Assert("e-Mail should contains 'Labels | Message Code'", bodyText.Contains(">Message Code<"));
				Assert("e-Mail should contains 'Labels | Description'", bodyText.Contains(">Description<"));
				Assert("e-Mail should contains 'Labels | Index No'", bodyText.Contains(">Index No<"));
				Assert("e-Mail should contains 'Error Code", bodyText.Contains("<td>000</td>"));
				Assert("e-Mail should contains 'Success'", bodyText.Contains("<td>Success</td>"));
				Assert("e-Mail should contains 'Indexses'", bodyText.Contains("<td>39906267<br>39906263<br>39916956<br>39903172<br>39906268<br>39916957</td>"));
			});
			
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		NCTSGetMessagesListByGuidResponseMessageProcessor Processor => fProcessor ?? (fProcessor = new NCTSGetMessagesListByGuidResponseMessageProcessor(new LoggingInformation()));
		NCTSGetMessagesListByGuidResponseMessageProcessor fProcessor;
	}
}
