using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.TR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.TR.Business.Testing
{
	class TRMessageSendingHelperTest : TestCaseWithFactory
	{
		public void TestSendManifestAutoReceiveResponseMessage()
		{
			var manifestHeader = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN0000001";
			var guid = "b5d51907-d1bf-4852-91c6-5c33e535a167";

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "YE";
			staff.GS_LoginName = "Yigit";
			staff.GS_EmailAddress = "yigit.ersan@wisetechglobal.com";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var originalMessage = Factory.New<TRManifestMessage>();
			originalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			originalMessage.EM_MessageType = TRMessageTypes.Codes.TRO;
			originalMessage.EM_MessageSubType = "XXX";
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_Status = EDIMessageStatusList.Codes.Received;
			originalMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			originalMessage.EM_IsTestMessage = true;
			originalMessage.EM_MessageText = "Test edimessage";
			originalMessage.EM_SystemCreateUser = "YE";
			originalMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			originalMessage.EM_LinkUniqueID = manifestHeader.PK;

			Factory.Save();

			TRMessageSendingHelper.SendManifestAutoReceiveResponseMessage(manifestHeader, guid, originalMessage);

			var factory = new BusinessObjectFactory();
			var manifest = factory.Load<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>(manifestHeader.PK);

			CombineAssertions("Message should have been saved into database", () =>
			{
				AssertEquals("Messages.Count", 2, manifest.Messages.Count);
				var message = (EDIMessage)manifest.Messages[1];
				AssertEquals("EM_IsActive", true, message.EM_IsActive);
				AssertEquals("EM_ApplicationCode", "TRC", message.EM_ApplicationCode);
				AssertEquals("EM_MessageOwner", "YE", message.EM_MessageOwner);
				AssertEquals("EM_MessageType", "T1O", message.EM_MessageType);
				AssertEquals("EM_MessageSubType", "XXX", message.EM_MessageSubType);
				AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
				AssertEquals("EM_ApplicationReference", "b5d51907-d1bf-4852-91c6-5c33e535a167", message.EM_ApplicationReference);
				AssertEquals("EM_Status", "QUE", message.EM_Status);
				AssertEquals("EM_GB", ((ManifestBase.AsycudaManifestHeader)manifest).Branch.PK, message.EM_GB);
				AssertEquals("EM_GE", message.Department.PK, message.EM_GE);
				AssertEquals("EM_FormattedMessageText", TRMessageTestHelper.GetFileText("Manifest.IslemSonucGetir2.xml"), message.EM_FormattedMessageText);
				Assert("EM_MessageInterpretation should contains 'successfully'", message.EM_MessageInterpretation.Contains("Global Manifest Message Type T1O sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Query GUID'", message.EM_MessageInterpretation.Contains("<td>Query GUID:</td><td>b5d51907-d1bf-4852-91c6-5c33e535a167</td>"));
			});
		}

		public void TestSendETradeAutoReceiveResponseMessage()
		{
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_JobReference = "ETG0000001";
			var guid = "b5d51907-d1bf-4852-91c6-5c33e535a167";

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "YE";
			staff.GS_LoginName = "Yigit";
			staff.GS_EmailAddress = "yigit.ersan@wisetechglobal.com";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var originalMessage = Factory.New<TRManifestMessage>();
			originalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			originalMessage.EM_MessageType = TRMessageTypes.Codes.TRS;
			originalMessage.EM_MessageSubType = "XXX";
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_Status = EDIMessageStatusList.Codes.Received;
			originalMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			originalMessage.EM_IsTestMessage = true;
			originalMessage.EM_MessageText = "Test edimessage";
			originalMessage.EM_SystemCreateUser = "YE";
			originalMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			originalMessage.EM_LinkUniqueID = header.PK;

			Factory.Save();

			TRMessageSendingHelper.SendETradeAutoReceiveResponseMessage(header, guid, originalMessage);

			var factory = new BusinessObjectFactory();
			var eTrade = factory.Load<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>(header.PK);
			CombineAssertions("Message should have been saved into database", () =>
			{
				AssertEquals("Messages.Count", 2, eTrade.Messages.Count);
				var message = (EDIMessage)eTrade.Messages[1];
				AssertEquals("EM_IsActive", true, message.EM_IsActive);
				AssertEquals("EM_ApplicationCode", "TRC", message.EM_ApplicationCode);
				AssertEquals("EM_MessageOwner", "YE", message.EM_MessageOwner);
				AssertEquals("EM_MessageType", "T1S", message.EM_MessageType);
				AssertEquals("EM_MessageSubType", "XXX", message.EM_MessageSubType);
				AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
				AssertEquals("EM_ApplicationReference", "b5d51907-d1bf-4852-91c6-5c33e535a167", message.EM_ApplicationReference);
				AssertEquals("EM_Status", "QUE", message.EM_Status);
				AssertEquals("EM_GB", ((ManifestBase.AsycudaManifestHeader)eTrade).Branch.PK, message.EM_GB);
				AssertEquals("EM_GE", message.Department.PK, message.EM_GE);
				AssertEquals("EM_MessageInterpretation", TRMessageTestHelper.GetFileText("ETrade.ExportRegistrationNo.ServisCevabiSorgulamaT1S.htm"), message.EM_MessageInterpretation);
			});
		}

		public void TestSendETradeAutoReceiveResponseMessageTRD()
		{
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_JobReference = "ETR0000001";
			var guid = "4142285b-6b4f-4eb8-9bfa-6baa3b884b06";

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "YE";
			staff.GS_LoginName = "Yigit";
			staff.GS_EmailAddress = "yigit.ersan@wisetechglobal.com";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var originalMessage = Factory.New<TRManifestMessage>();
			originalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			originalMessage.EM_MessageType = TRMessageTypes.Codes.TRD;
			originalMessage.EM_MessageSubType = "XXX";
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_Status = EDIMessageStatusList.Codes.Received;
			originalMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			originalMessage.EM_IsTestMessage = true;
			originalMessage.EM_MessageText = "Test edimessage";
			originalMessage.EM_SystemCreateUser = "YE";
			originalMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			originalMessage.EM_LinkUniqueID = header.PK;

			Factory.Save();

			TRMessageSendingHelper.SendETradeAutoReceiveResponseMessage(header, guid, originalMessage);

			var factory = new BusinessObjectFactory();
			var manifest = factory.Load<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>(header.PK);
			CombineAssertions("Message should have been saved into database", () =>
			{
				AssertEquals("Messages.Count", 2, manifest.Messages.Count);
				var message = (EDIMessage)manifest.Messages[1];
				AssertEquals("EM_IsActive", true, message.EM_IsActive);
				AssertEquals("EM_ApplicationCode", "TRC", message.EM_ApplicationCode);
				AssertEquals("EM_MessageOwner", "YE", message.EM_MessageOwner);
				AssertEquals("EM_MessageType", "T1D", message.EM_MessageType);
				AssertEquals("EM_MessageSubType", "XXX", message.EM_MessageSubType);
				AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
				AssertEquals("EM_ApplicationReference", "4142285b-6b4f-4eb8-9bfa-6baa3b884b06", message.EM_ApplicationReference);
				AssertEquals("EM_Status", "QUE", message.EM_Status);
				AssertEquals("EM_GB", ((ManifestBase.AsycudaManifestHeader)manifest).Branch.PK, message.EM_GB);
				AssertEquals("EM_GE", message.Department.PK, message.EM_GE);
				AssertEquals("EM_MessageInterpretation", TRMessageTestHelper.GetFileText("ETrade.ImportDischargeList.ServisCevabiSorgulamaT1D.htm"), message.EM_MessageInterpretation);
			});
		}

		public void TestSendETradeAutoReceiveResponseMessageTCD()
		{
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_JobReference = "ETR0000001";
			var guid = "4142285b-6b4f-4eb8-9bfa-6baa3b884b06";

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "YE";
			staff.GS_LoginName = "Yigit";
			staff.GS_EmailAddress = "yigit.ersan@wisetechglobal.com";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var originalMessage = Factory.New<TRManifestMessage>();
			originalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			originalMessage.EM_MessageType = TRMessageTypes.Codes.TCD;
			originalMessage.EM_MessageSubType = "XXX";
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_Status = EDIMessageStatusList.Codes.Received;
			originalMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			originalMessage.EM_IsTestMessage = true;
			originalMessage.EM_MessageText = "Test edimessage";
			originalMessage.EM_SystemCreateUser = "YE";
			originalMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			originalMessage.EM_LinkUniqueID = header.PK;

			Factory.Save();

			TRMessageSendingHelper.SendETradeAutoReceiveResponseMessage(header, guid, originalMessage);

			var factory = new BusinessObjectFactory();
			var manifest = factory.Load<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>(header.PK);
			CombineAssertions("Message should have been saved into database", () =>
			{
				AssertEquals("Messages.Count", 2, manifest.Messages.Count);
				var message = (EDIMessage)manifest.Messages[1];
				AssertEquals("EM_IsActive", true, message.EM_IsActive);
				AssertEquals("EM_ApplicationCode", "TRC", message.EM_ApplicationCode);
				AssertEquals("EM_MessageOwner", "YE", message.EM_MessageOwner);
				AssertEquals("EM_MessageType", "T2D", message.EM_MessageType);
				AssertEquals("EM_MessageSubType", "XXX", message.EM_MessageSubType);
				AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
				AssertEquals("EM_ApplicationReference", "4142285b-6b4f-4eb8-9bfa-6baa3b884b06", message.EM_ApplicationReference);
				AssertEquals("EM_Status", "QUE", message.EM_Status);
				AssertEquals("EM_GB", ((ManifestBase.AsycudaManifestHeader)manifest).Branch.PK, message.EM_GB);
				AssertEquals("EM_GE", message.Department.PK, message.EM_GE);
				AssertEquals("EM_MessageInterpretation", TRMessageTestHelper.GetFileText("ETrade.ImportSendforComplementaryDeclaration.ServisCevabiSorgulamaT2D.htm"), message.EM_MessageInterpretation);
			});
		}

		public void TestSendETradeAutoReceiveResponseMessageTRE()
		{
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			header.AMA_JobReference = "ETR0000001";
			var guid = "4142285b-6b4f-4eb8-9bfa-6baa3b884b06";

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "YE";
			staff.GS_LoginName = "Yigit";
			staff.GS_EmailAddress = "yigit.ersan@wisetechglobal.com";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var originalMessage = Factory.New<TRManifestMessage>();
			originalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			originalMessage.EM_MessageType = TRMessageTypes.Codes.TRE;
			originalMessage.EM_MessageSubType = "XXX";
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_Status = EDIMessageStatusList.Codes.Received;
			originalMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			originalMessage.EM_IsTestMessage = true;
			originalMessage.EM_MessageText = "Test edimessage";
			originalMessage.EM_SystemCreateUser = "YE";
			originalMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			originalMessage.EM_LinkUniqueID = header.PK;

			Factory.Save();

			TRMessageSendingHelper.SendETradeAutoReceiveResponseMessage(header, guid, originalMessage);

			var factory = new BusinessObjectFactory();
			var manifest = factory.Load<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>(header.PK);

			CombineAssertions("Message should have been saved into database", () =>
			{
				AssertEquals("Messages.Count", 2, manifest.Messages.Count);
				var message = (EDIMessage)manifest.Messages[1];
				AssertEquals("EM_IsActive", true, message.EM_IsActive);
				AssertEquals("EM_ApplicationCode", "TRC", message.EM_ApplicationCode);
				AssertEquals("EM_MessageOwner", "YE", message.EM_MessageOwner);
				AssertEquals("EM_MessageType", "T1E", message.EM_MessageType);
				AssertEquals("EM_MessageSubType", "XXX", message.EM_MessageSubType);
				AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
				AssertEquals("EM_ApplicationReference", "4142285b-6b4f-4eb8-9bfa-6baa3b884b06", message.EM_ApplicationReference);
				AssertEquals("EM_Status", "QUE", message.EM_Status);
				AssertEquals("EM_GB", ((ManifestBase.AsycudaManifestHeader)manifest).Branch.PK, message.EM_GB);
				AssertEquals("EM_GE", message.Department.PK, message.EM_GE);
				Assert("EM_MessageInterpretation should contains 'successfully'", message.EM_MessageInterpretation.Contains("E-Trade Message Type T1E sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Action and Query GUID'", message.EM_MessageInterpretation.Contains("<td>Action:</td><td>Query for Temporary Registration No</td></tr><tr><td>Query GUID:</td><td>4142285b-6b4f-4eb8-9bfa-6baa3b884b06</td>"));
			});
		}

		public void TestSendSPTSAutoReceiveResponseMessage()
		{
			var sptsHeader = Factory.New<Integration.Customs.TR.ICusInBondSPTSHeader>();
			sptsHeader.BH_JobReference = "SPTS0000001";
			var guid = "b5d51907-d1bf-4852-91c6-5c33e535a167";

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "YE";
			staff.GS_LoginName = "Yigit";
			staff.GS_EmailAddress = "yigit.ersan@wisetechglobal.com";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var originalMessage = Factory.New<SPTSMessage>();
			originalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			originalMessage.EM_MessageType = TRMessageTypes.Codes.TSP;
			originalMessage.EM_MessageSubType = "XXX";
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_Status = EDIMessageStatusList.Codes.Received;
			originalMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			originalMessage.EM_IsTestMessage = true;
			originalMessage.EM_MessageText = "Test edimessage";
			originalMessage.EM_SystemCreateUser = "YE";
			originalMessage.EM_LinkTable = SPTSHeader.Schema.TableName;
			originalMessage.EM_LinkUniqueID = sptsHeader.PK;

			Factory.Save();

			TRMessageSendingHelper.SendSPTSAutoReceiveResponseMessage(sptsHeader, guid, originalMessage);

			var factory = new BusinessObjectFactory();
			var spts = factory.Load<Integration.Customs.TR.ICusInBondSPTSHeader>(sptsHeader.PK);
			var expectedInterpretation = ZString.Empty;
			expectedInterpretation = TRMessageTestHelper.GetFileText("SPTS.IslemSonucGetir2.htm");

			CombineAssertions("Message should have been saved into database", () =>
			{
				AssertEquals("Messages.Count", 2, spts.Messages.Count);
				var message = (EDIMessage)spts.Messages[1];
				AssertEquals("EM_IsActive", true, message.EM_IsActive);
				AssertEquals("EM_ApplicationCode", "TRC", message.EM_ApplicationCode);
				AssertEquals("EM_MessageOwner", "YE", message.EM_MessageOwner);
				AssertEquals("EM_MessageType", "T1P", message.EM_MessageType);
				AssertEquals("EM_MessageSubType", "XXX", message.EM_MessageSubType);
				AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
				AssertEquals("EM_ApplicationReference", "b5d51907-d1bf-4852-91c6-5c33e535a167", message.EM_ApplicationReference);
				AssertEquals("EM_Status", "QUE", message.EM_Status);
				AssertEquals("EM_GB", ((SPTSHeader)spts).Branch.PK, message.EM_GB);
				AssertEquals("EM_GE", message.Department.PK, message.EM_GE);
				AssertEquals("Message Text", TRMessageTestHelper.GetFileText("SPTS.IslemSonucGetir2.xml"), message.EM_FormattedMessageText);
				AssertEquals("Message Interpretation", expectedInterpretation, message.EM_MessageInterpretation);
			});
		}

		public void TestProcessCusPollingTransaction()
		{
			var message = Factory.NewWithValidTestData<TRManifestMessage>();
			var messageText = TRMessageTestHelper.GetFileText("T1OSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			var queryMessageType = TRMessageTypes.Codes.TRO;
			var actual = TRMessageSendingHelper.ProcessCusPollingTransaction(message, messageText, queryMessageType);
			AssertEquals("No original message", ZString.Empty, actual);

			var queryGuidString = ZGuid.NewZGuid().ToString();
			var sessionId = ZGuid.NewZGuid();
			var originalMessage = Factory.NewWithValidTestData<TRManifestMessage>();
			originalMessage.EM_ApplicationReference = queryGuidString;
			var originalInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			originalInterchange.EI_SessionGUID = sessionId;
			originalInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			originalMessage.EM_EI = originalInterchange.PK;

			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_SessionGUID = sessionId;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			message.EM_EI = interchange.PK;

			var pollingTransaction = message.Factory.New<CusPollingTransaction>();
			pollingTransaction.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction.CPT_Type = queryMessageType;
			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			pollingTransaction.CPT_NumberOfAttempts = 5;
			pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.Now.AddDays(-3);
			pollingTransaction.CPT_TransactionID = queryGuidString;
			pollingTransaction.CPT_ParentID = message.PK;
			Factory.Save();

			actual = TRMessageSendingHelper.ProcessCusPollingTransaction(message, messageText, queryMessageType);
			AssertEquals("messageText is not empty", Core.Constants.Customs.CusPollingTransactionStatus.Codes.CLS, actual);

			messageText = ZString.Empty;
			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			Factory.Save();

			actual = TRMessageSendingHelper.ProcessCusPollingTransaction(message, messageText, queryMessageType);
			AssertEquals("messageText is empty and CPT_NumberOfAttempts is greater than 1", Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN, actual);

			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			pollingTransaction.CPT_NumberOfAttempts = 1;
			Factory.Save();

			actual = TRMessageSendingHelper.ProcessCusPollingTransaction(message, messageText, queryMessageType);
			AssertEquals("messageText is empty and CPT_NumberOfAttempts is 1", Core.Constants.Customs.CusPollingTransactionStatus.Codes.ERR, actual);
		}

		public void TestCreateCusPollingTransaction()
		{
			var message = Factory.NewWithValidTestData<TRManifestMessage>();
			message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-3);
			var queryMessageType = TRMessageTypes.Codes.TRO;
			var queryGuidString = ZGuid.NewZGuid().ToString();
			var pollingTransaction = TRMessageSendingHelper.CreateCusPollingTransaction(message, queryMessageType, queryGuidString);
			CombineAssertions("Created CusPollingTransaction", () =>
			{
				AssertEquals("CPT_ApplicationCode", EDIMessage.ApplicationCodes.TRCustoms, pollingTransaction.CPT_ApplicationCode);
				AssertEquals("CPT_Type", queryMessageType, pollingTransaction.CPT_Type);
				AssertEquals("CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN, pollingTransaction.CPT_Status);
				AssertEquals("CPT_NumberOfAttempts", (ZByte)5, pollingTransaction.CPT_NumberOfAttempts);
				AssertEquals("CPT_EarliestTimeOfNextAttemptUtc", message.EM_SystemCreateTimeUtc.AddMinutes(1), pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc);
				AssertEquals("CPT_TransactionID", queryGuidString, pollingTransaction.CPT_TransactionID);
				AssertEquals("CPT_ParentID", message.PK, pollingTransaction.CPT_ParentID);
			});
		}

		public void TestSendNctsAutoReceiveResponseMessage_TRN()
		{
			ZString actionResult = "Not called";
			var pk = SetupAndSentNcts(TRMessageTypes.Codes.TRN, (x) => actionResult = x.Notifications.NotificationsAsString());

			var header = Factory.Load<Integration.Customs.TR.ICusInBondHeader>(pk);
			AssertEquals("Expect 2 messages", 2, header.Messages.Count);
			AssertContains("Trans Id", "TransId-TRN", header.Messages.OfType<EDIMessage>().ToArray()[1].EM_MessageText);
			AssertEquals("actionResult called", "Message sent successfully.\r\n", actionResult);
		}

		public void TestSendNctsAutoReceiveResponseMessage_T1N()
		{
			ZString actionResult = "Not called";
			var pk = SetupAndSentNcts(TRMessageTypes.Codes.T1N, (x) => actionResult = x.Notifications.NotificationsAsString());

			var header = Factory.Load<Integration.Customs.TR.ICusInBondHeader>(pk);
			AssertEquals("Expect 2 messages", 2, header.Messages.Count);
			AssertContains("Trans Id", "TransId-T1N", header.Messages.OfType<EDIMessage>().ToArray()[1].EM_MessageText);
			AssertEquals("actionResult called", "Message sent successfully.\r\n", actionResult);
		}

		public void TestSendNctsAutoReceiveResponseMessage_Wrong()
		{
			ZString actionResult = "Not called";
			var pk = SetupAndSentNcts(TRMessageTypes.Codes.TRE, (x) => actionResult = x.Notifications.NotificationsAsString());

			var header = Factory.Load<Integration.Customs.TR.ICusInBondHeader>(pk);
			AssertEquals("Expect 1 messages", 1, header.Messages.Count);
			AssertEquals("actionResult called", "Not called", actionResult);
		}

		[TestDate(2022, 09, 05)]
		public void TestSendManifestAutoReceiveResponseMessageT2O()
		{
			var manifestHeader = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN0000001";
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			staff.GS_LoginName = "Kevin";
			staff.GS_EmailAddress = "kevin.zhang@wisetechglobal.com";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var originalMessage = Factory.New<TRManifestMessage>();
			originalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			originalMessage.EM_MessageType = TRMessageTypes.Codes.T1O;
			originalMessage.EM_MessageSubType = "XXX";
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_Status = EDIMessageStatusList.Codes.Received;
			originalMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			originalMessage.EM_IsTestMessage = true;
			originalMessage.EM_MessageText = "test message";
			originalMessage.EM_SystemCreateUser = "KNZ";
			originalMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			originalMessage.EM_LinkUniqueID = manifestHeader.PK;
			originalMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);

			var message1 = Factory.NewWithValidTestData<TRManifestMessage>();
			message1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(2);
			message1.EM_MessageType = TRMessageTypes.Codes.T1O;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_LinkUniqueID = manifestHeader.PK;
			message1.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;

			var message2 = Factory.NewWithValidTestData<TRManifestMessage>();
			message2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(3);
			message2.EM_MessageType = TRMessageTypes.Codes.T1O;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_LinkUniqueID = manifestHeader.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;

			Factory.Save();

			TRMessageSendingHelper.SendManifestAutoReceiveResponseMessageForT2O(manifestHeader, originalMessage);

			var factory = new BusinessObjectFactory();
			var manifest = factory.Load<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>(manifestHeader.PK);
			manifestHeader.AMA_JobReference = "MAN0000001";
			var expectedFormatedMessageText = TRMessageTestHelper.GetFileText("Manifest.IslemSorgula3.xml");

			CombineAssertions(() =>
			{
				AssertEquals("Messages.Count", 4, manifest.Messages.Count);
				var message = (EDIMessage)manifest.Messages[0];
				AssertEquals("EM_IsActive", true, message.EM_IsActive);
				AssertEquals("EM_ApplicationCode", "TRC", message.EM_ApplicationCode);
				AssertEquals("EM_MessageOwner", "KNZ", message.EM_MessageOwner);
				AssertEquals("EM_MessageType", "T2O", message.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
				AssertEquals("EM_ApplicationReference", "ULU-MAN0000001|20201224104", message.EM_ApplicationReference);
				AssertEquals("EM_Status", "QUE", message.EM_Status);
				AssertEquals("EM_GB", ((ManifestBase.AsycudaManifestHeader)manifest).Branch.PK, message.EM_GB);
				AssertEquals("EM_GE", message.Department.PK, message.EM_GE);
				AssertEquals("EM_FormattedMessageText", expectedFormatedMessageText, message.EM_FormattedMessageText);
				Assert("EM_MessageInterpretation should contains 'successfully'", message.EM_MessageInterpretation.Contains("Global Manifest Message Type T2O sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Job Number'", message.EM_MessageInterpretation.Contains("<td>Job Number:</td><td>MAN0000001|20201224104</td>"));
				Assert("EM_MessageInterpretation should contains 'Query Date'", message.EM_MessageInterpretation.Contains("Query Date:</td><td>2022-09-08</td>"));
			});
		}

		[TestDate(2022, 09, 05)]
		public void TestSendManifestAutoReceiveResponseMessageT3O()
		{
			var manifestHeader = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN0000001";
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			staff.GS_LoginName = "Kevin";
			staff.GS_EmailAddress = "kevin.zhang@wisetechglobal.com";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var originalTROMessage = Factory.New<TRManifestMessage>();
			originalTROMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			originalTROMessage.EM_MessageType = TRMessageTypes.Codes.TRO;
			originalTROMessage.EM_MessageSubType = "XXX";
			originalTROMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalTROMessage.EM_Status = EDIMessageStatusList.Codes.Received;
			originalTROMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			originalTROMessage.EM_IsTestMessage = true;
			originalTROMessage.EM_MessageText = "TRO test message";
			originalTROMessage.EM_SystemCreateUser = "KNZ";
			originalTROMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			originalTROMessage.EM_LinkUniqueID = manifestHeader.PK;

			var originalT2OMessage = Factory.New<TRManifestMessage>();
			originalT2OMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			originalT2OMessage.EM_MessageType = TRMessageTypes.Codes.T2O;
			originalT2OMessage.EM_MessageSubType = "XXX";
			originalT2OMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalT2OMessage.EM_Status = EDIMessageStatusList.Codes.Received;
			originalT2OMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			originalT2OMessage.EM_IsTestMessage = true;
			originalT2OMessage.EM_MessageText = "T2O test message";
			originalT2OMessage.EM_SystemCreateUser = "YK";
			originalT2OMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			originalT2OMessage.EM_LinkUniqueID = manifestHeader.PK;

			Factory.Save();

			TRMessageSendingHelper.SendManifestAutoReceiveResponseMessageForT3O(manifestHeader, "344d5a52-938e-4fc4-b38b-c58d2f03319d", originalTROMessage);
			var factory = new BusinessObjectFactory();
			var manifest = factory.Load<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>(manifestHeader.PK);
			manifestHeader.AMA_JobReference = "MAN0000001";
			var expectedFormattedMessageText = TRMessageTestHelper.GetFileText("Manifest.IslemSonucGetir4.xml");

			CombineAssertions(() =>
			{
				AssertEquals("Messages.Count", 3, manifest.Messages.Count);
				var message = (EDIMessage)manifest.Messages[2];
				AssertEquals("EM_IsActive", true, message.EM_IsActive);
				AssertEquals("EM_ApplicationCode", "TRC", message.EM_ApplicationCode);
				AssertEquals("EM_MessageOwner", "KNZ", message.EM_MessageOwner);
				AssertEquals("EM_MessageType", "T3O", message.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
				AssertEquals("EM_ApplicationReference", "344d5a52-938e-4fc4-b38b-c58d2f03319d", message.EM_ApplicationReference);
				AssertEquals("EM_Status", "QUE", message.EM_Status);
				AssertEquals("EM_GB", ((ManifestBase.AsycudaManifestHeader)manifest).Branch.PK, message.EM_GB);
				AssertEquals("EM_GE", message.Department.PK, message.EM_GE);
				AssertEquals("EM_FormattedMessageText", expectedFormattedMessageText, message.EM_FormattedMessageText);
				Assert("EM_MessageInterpretation should contains 'successfully'", message.EM_MessageInterpretation.Contains("Global Manifest Message Type T3O sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Query GUID'", message.EM_MessageInterpretation.Contains("<td>Query GUID:</td><td>344d5a52-938e-4fc4-b38b-c58d2f03319d</td>"));
			});
		}

		[TestDate(2022, 09, 05)]
		public void TestSendManifestAutoReceiveResponseMessageTRM()
		{
			var manifestHeader = Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN0000001";
			manifestHeader.AMA_CustomsOffice = "066666";
			manifestHeader.RegistrationNumber = "22067777IM000002";
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			staff.GS_LoginName = "Kevin";
			staff.GS_EmailAddress = "kevin.zhang@wisetechglobal.com";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var originalMessage = Factory.New<TRManifestMessage>();
			originalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			originalMessage.EM_MessageType = TRMessageTypes.Codes.T1O;
			originalMessage.EM_MessageSubType = "XXX";
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_Status = EDIMessageStatusList.Codes.Received;
			originalMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			originalMessage.EM_IsTestMessage = true;
			originalMessage.EM_MessageText = "test message";
			originalMessage.EM_SystemCreateUser = "KNZ";
			originalMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			originalMessage.EM_LinkUniqueID = manifestHeader.PK;
			originalMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);

			var message1 = Factory.NewWithValidTestData<TRManifestMessage>();
			message1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(2);
			message1.EM_MessageType = TRMessageTypes.Codes.T1O;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_LinkUniqueID = manifestHeader.PK;
			message1.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;

			var message2 = Factory.NewWithValidTestData<TRManifestMessage>();
			message2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(3);
			message2.EM_MessageType = TRMessageTypes.Codes.T1O;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_LinkUniqueID = manifestHeader.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;

			Factory.Save();

			TRMessageSendingHelper.SendManifestAutoReceiveResponseMessageForTRM(manifestHeader, originalMessage);

			var factory = new BusinessObjectFactory();
			var manifest = factory.Load<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>(manifestHeader.PK);
			manifestHeader.AMA_JobReference = "MAN0000001";
			var expectedFormatedMessageText = TRMessageTestHelper.GetFileText("Manifest.OzbyMuayeneMemuruAdiSorgula.xml");

			CombineAssertions(() =>
			{
				AssertEquals("Messages.Count", 4, manifest.Messages.Count);
				var message = (EDIMessage)manifest.Messages[0];
				AssertEquals("EM_IsActive", true, message.EM_IsActive);
				AssertEquals("EM_ApplicationCode", "TRC", message.EM_ApplicationCode);
				AssertEquals("EM_MessageOwner", "KNZ", message.EM_MessageOwner);
				AssertEquals("EM_MessageType", "TRM", message.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
				AssertEquals("EM_ApplicationReference", "ULU-MAN0000001|20201224104", message.EM_ApplicationReference);
				AssertEquals("EM_Status", "QUE", message.EM_Status);
				AssertEquals("EM_GB", ((ManifestBase.AsycudaManifestHeader)manifest).Branch.PK, message.EM_GB);
				AssertEquals("EM_GE", message.Department.PK, message.EM_GE);
				AssertEquals("EM_FormattedMessageText", expectedFormatedMessageText, message.EM_FormattedMessageText);
				Assert("EM_MessageInterpretation should contains 'successfully'", message.EM_MessageInterpretation.Contains("Global Manifest Message Type TRM sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Job Number'", message.EM_MessageInterpretation.Contains("<td>Customs Office:</td><td>066666</td>"));
				Assert("EM_MessageInterpretation should contains 'Query Date'", message.EM_MessageInterpretation.Contains("<td>Registration Number:</td><td>22067777IM000002</td>"));
			});
		}

		public void TestSendImportExportAutoReceiveResponseMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "IE0000001";
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var guid = "b5d51907-d1bf-4852-91c6-5c33e535a167";

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			staff.GS_LoginName = "Kevin";
			staff.GS_EmailAddress = "kevin@wisetechglobal.com";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var originalMessage = Factory.New<TRImportExportMessage>();
			originalMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			originalMessage.EM_MessageType = TRMessageTypes.Codes.DKO;
			originalMessage.EM_MessageSubType = "XXX";
			originalMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			originalMessage.EM_Status = EDIMessageStatusList.Codes.Received;
			originalMessage.EM_GB = GlbBranch.CurrentBranch.PK;
			originalMessage.EM_IsTestMessage = true;
			originalMessage.EM_MessageText = "Test edimessage";
			originalMessage.EM_SystemCreateUser = "KNZ";
			originalMessage.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			originalMessage.EM_LinkUniqueID = cusEntryHeader.PK;

			Factory.Save();

			TRMessageSendingHelper.SendImportExportAutoReceiveResponseMessage(cusEntryHeader, guid, originalMessage, ZString.Empty);

			var factory = new BusinessObjectFactory();
			var customsEntryHeader = factory.Load<CusEntryHeader>(cusEntryHeader.PK);

			CombineAssertions("Message should have been saved into database", () =>
			{
				AssertEquals("Messages.Count", 2, customsEntryHeader.Messages.Count);
				var message = customsEntryHeader.Messages[1];
				AssertEquals("EM_IsActive", true, message.EM_IsActive);
				AssertEquals("EM_ApplicationCode", "TRC", message.EM_ApplicationCode);
				AssertEquals("EM_MessageOwner", "KNZ", message.EM_MessageOwner);
				AssertEquals("EM_MessageType", "DK1", message.EM_MessageType);
				AssertEquals("EM_MessageSubType", "XXX", message.EM_MessageSubType);
				AssertEquals("EM_ReceiveTransmit", "TRX", message.EM_ReceiveTransmit);
				AssertEquals("EM_ApplicationReference", "b5d51907-d1bf-4852-91c6-5c33e535a167", message.EM_ApplicationReference);
				AssertEquals("EM_Status", "QUE", message.EM_Status);
				AssertEquals("EM_GE", message.Department.PK, message.EM_GE);
				Assert("EM_MessageInterpretation should contains 'successfully'", message.EM_MessageInterpretation.Contains("Declaration Message Type DK1 sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Query GUID'", message.EM_MessageInterpretation.Contains("<td>Query GUID:</td><td>b5d51907-d1bf-4852-91c6-5c33e535a167</td>"));
			});
		}

		public void TestCreateCusPollingTransaction_ShouldRespectEarliestTimeParameter()
		{
			var message = Factory.NewWithValidTestData<NCTSMessage>();
			message.EM_MessageType = TRMessageTypes.Codes.T1N;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			var queryGuidString = ZGuid.NewZGuid().ToString();

			CombineAssertions("Transaction Polling Delay", () =>
			{
				var pollingTransaction = TRMessageSendingHelper.CreateCusPollingTransaction(message, TRMessageTypes.Codes.T1N, queryGuidString, TRMessageConstants.TransactionPollingDelay);
				AssertEquals("CPT_EarliestTimeOfNextAttemptUtc", message.EM_SystemCreateTimeUtc.AddMinutes(TRMessageConstants.TransactionPollingDelay), pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc);
				pollingTransaction = TRMessageSendingHelper.CreateCusPollingTransaction(message, TRMessageTypes.Codes.T1N, queryGuidString);
				AssertEquals("CPT_EarliestTimeOfNextAttemptUtc", message.EM_SystemCreateTimeUtc.AddMinutes(1), pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc);
			});
		}

		ZGuid SetupAndSentNcts(string messageType, System.Action<ActionResult> action)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "YE";
			staff.GS_LoginName = "Yigit";
			staff.GS_EmailAddress = "yigit.ersan@wisetechglobal.com";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var nctsHeader = Factory.New<Integration.Customs.TR.ICusInBondHeader>();

			var requestMessage = Factory.New<DummyEDIMessage_TRMessageSendingHelperTest>();
			requestMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			requestMessage.EM_LinkUniqueID = nctsHeader.PK;
			requestMessage.EM_SystemCreateUser = "YE";
			requestMessage.GetMessageReferenceNumberToReturn = "1";

			Factory.Save();

			TRMessageSendingHelper.SendNCTSAutoReceiveResponseMessage(messageType, nctsHeader, $"TransId-{messageType}", requestMessage, action);

			return nctsHeader.PK;
		}
	}

	public class DummyEDIMessage_TRMessageSendingHelperTest : EDIMessage
	{
		public DummyEDIMessage_TRMessageSendingHelperTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public string GetMessageReferenceNumberToReturn { get; set; }

		protected override string GetMessageReferenceNumber()
		{
			return GetMessageReferenceNumberToReturn;
		}
	}
}
