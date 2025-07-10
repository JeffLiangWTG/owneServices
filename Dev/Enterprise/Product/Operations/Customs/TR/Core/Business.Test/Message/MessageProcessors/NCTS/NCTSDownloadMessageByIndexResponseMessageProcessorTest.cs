using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	class NCTSDownloadMessageByIndexResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessMessage()
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

			var errorCode = "000";
			var errorDesc = "Success";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("NCTER", "NCTS Errors", Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, "NCTER", errorCode, errorDesc, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			AssertProcessMessage("CC060A.xml", "CTR");
			AssertProcessMessage("CC028A.xml", "MRN");
			AssertProcessMessage("CC029B.xml", "REL");
			AssertProcessMessage("CC015B_RES.xml", "");
			AssertProcessMessage("CC015B_RES_validation_failed.xml", "DRJ");
			AssertProcessMessage("CTRINFDEP.xml", "");
			AssertProcessMessage("GUAINF.xml", "");
			AssertProcessMessage("TRC_RCV_T2N.xml", "DRJ");
		}

		void AssertProcessMessage(ZString testFileName, ZString expectedBM_CustomsStatus)
		{
			var nctsHeader = Factory.New<Integration.Customs.TR.ICusInBondHeader>();
			nctsHeader.BH_HeaderType = "D";

			var queryGUID = ZGuid.NewZGuid().ToString();
			var trackingID = ZGuid.NewZGuid();

			var trnRequestMessageText = "~Test";
			var trnRequestMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.TRN, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, trnRequestMessageText, "1", queryGUID);
			var trnRequestInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.TRN, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, trnRequestMessageText, trackingID);
			trnRequestMessage.EM_EI = trnRequestInterchange.PK;

			var t1nResponseMessageText = TRMessageTestHelper.GetFileText("GetMessagesListByGuidResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var t1nResponseMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.T1N, EDIMessage.Direction.Receive, EDIMessage.Status.ProcessedOK, t1nResponseMessageText, "1", queryGUID);

			var t2nRequestMessageText = TRMessageTestHelper.GetFileText("DownloadMessageByIndex.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var t2nRequestMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.T2N, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, t2nRequestMessageText, "2", queryGUID);
			var t2nRequestInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.T2N, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, t2nRequestMessageText, trackingID);
			t2nRequestMessage.EM_EI = t2nRequestInterchange.PK;

			var t2nResponseMessageText = TRMessageTestHelper.GetFileText(testFileName, "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.DownloadMessageByIndexResponse.");
			var t2nResponseMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.T2N, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, t2nResponseMessageText, "3", "");
			var t2nResponseInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.T2N, EDIInterchange.Direction.Receive, EDIInterchange.Status.Received, t2nResponseMessageText, trackingID);
			t2nResponseMessage.EM_EI = t2nResponseInterchange.PK;

			trnRequestMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			trnRequestMessage.EM_LinkUniqueID = nctsHeader.PK;
			trnRequestMessage.EM_SystemCreateUser = "YK";
			t2nRequestMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			t2nRequestMessage.EM_LinkUniqueID = nctsHeader.PK;
			t2nResponseMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			t2nResponseMessage.EM_LinkUniqueID = nctsHeader.PK;

			var trnPollingTransaction = Factory.New<CusPollingTransaction>();
			trnPollingTransaction.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			trnPollingTransaction.CPT_Type = TRMessageTypes.Codes.T1N;
			trnPollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			trnPollingTransaction.CPT_NumberOfAttempts = 5;
			trnPollingTransaction.CPT_TransactionID = queryGUID;
			trnPollingTransaction.CPT_ParentID = t1nResponseMessage.PK;
			Factory.Save();

			Processor.ProcessMessage(t2nResponseMessage);
			var messageInterpretation = t2nResponseMessage.EM_MessageInterpretation;

			CombineAssertions("EM_MessageInterpretation " + testFileName, () =>
			{
				Assert("EM_MessageInterpretation should contains 'Caption'", messageInterpretation.Contains("NCTS Download Message for job"));
				Assert("EM_MessageInterpretation should contains 'Label | Message Code'", messageInterpretation.Contains(">Message Code<"));
				Assert("EM_MessageInterpretation should contains 'Label | Description'", messageInterpretation.Contains(">Description<"));
				Assert("EM_MessageInterpretation should contains 'Label | Error Code'", messageInterpretation.Contains(">Response Message<"));
				Assert("EM_MessageInterpretation should contains 'Error Code'", messageInterpretation.Contains("<td>000</td>"));
				Assert("EM_MessageInterpretation should contains 'Success'", messageInterpretation.Contains("<td>Success</td>"));

				switch (testFileName)
				{
					case "CC060A.xml":
						AssertEquals("BM_CustomsStatus", expectedBM_CustomsStatus, nctsHeader.BM_CustomsStatus);
						Assert("EM_MessageInterpretation should contains 'Control Decision Made'", messageInterpretation.Contains("<td>Control Decision Made</td>"));
						AssertEquals("MRN Number", "19TR59010000163675", nctsHeader.ArrivalMrnFromUser);
						AssertEquals("MRN Date", new ZDateTime(2019, 11, 4), nctsHeader.MrnIssueDateFromUser);
						break;
					case "CC028A.xml":
						AssertEquals("BM_CustomsStatus", expectedBM_CustomsStatus, nctsHeader.BM_CustomsStatus);
						Assert("EM_MessageInterpretation should contains 'Declaration Registered'", messageInterpretation.Contains("<td>Declaration Registered</td"));
						AssertEquals("LRN Number", "19LR5901002365322", nctsHeader.LrnRegistrationNumber);
						AssertEquals("MRN Number", "19TR59010000163675", nctsHeader.ArrivalMrnFromUser);
						AssertEquals("MRN Date", new ZDateTime(2019, 11, 4), nctsHeader.MrnIssueDateFromUser);
						break;
					case "CC029B.xml":
						AssertEquals("BM_CustomsStatus", expectedBM_CustomsStatus, nctsHeader.BM_CustomsStatus);
						Assert("EM_MessageInterpretation should contains 'Movement Released'", messageInterpretation.Contains("<td>Movement Released</td>"));
						AssertEquals("MRN Number", "19TR59010000163675", nctsHeader.ArrivalMrnFromUser);
						AssertEquals("BM_CustomsStatus", "REL", nctsHeader.BM_CustomsStatus);
						AssertEquals("LRN Number", "19LR5901002365322", nctsHeader.LrnRegistrationNumber);
						AssertEquals("MRN Number", "19TR59010000163675", nctsHeader.ArrivalMrnFromUser);
						AssertEquals("MRN Date", new ZDateTime(2019, 11, 4), nctsHeader.MrnIssueDateFromUser);
						break;
					case "CC015B_RES.xml":
						Assert("EM_MessageInterpretation should contains 'Registration No'", messageInterpretation.Contains("<td>Registration No: 19LR5901002365322</td>"));
						AssertEquals("Registration Number", "19LR5901002365322", nctsHeader.LrnRegistrationNumber);
						AssertEquals("Registration Date", new ZDateTime(2019, 11, 4), nctsHeader.LrnRegistrationDate);
						AssertEquals("BM_CustomsStatus", "LRN", nctsHeader.BM_CustomsStatus);
						break;
					case "CC015B_RES_validation_failed.xml":
						AssertEquals("BM_CustomsStatus", expectedBM_CustomsStatus, nctsHeader.BM_CustomsStatus);
						Assert("EM_MessageInterpretation should contains 'TEC Error 1.1'", messageInterpretation.Contains("NOT OKORA-12899: value too large for column &quot;NCTS_TRA_DEP"));
						Assert("EM_MessageInterpretation should contains 'TEC Error 1.2'", messageInterpretation.Contains("CC015B&quot;.&quot;STRANDNUMPC122&quot; (actual: 47, maximum: 35)"));
						Assert("EM_MessageInterpretation should contains 'VAL_MAIN Error 1'", messageInterpretation.Contains("(G59000002) Ambar kodu hatalı ya da Eşyalar ambarda değil"));
						Assert("EM_MessageInterpretation should contains 'VAL_MAIN Error 2.1'", messageInterpretation.Contains("A&#231;malarda eşya ambar i&#231;inde se&#231;ilmiş"));
						Assert("EM_MessageInterpretation should contains 'VAL_MAIN Error 2.2'", messageInterpretation.Contains("Taşıma senedi hen&#252;z ambara alınmamış ya da ambar &#231;ıkış işlemi yapılmıştır. (MEDUK2795475)"));
						Assert("EM_MessageInterpretation should contains 'VAL_GUA Error 1'", messageInterpretation.Contains("C085. Guarantee reference can not be empty if Guarantee type is 0, 1, 2, 4 or 9"));
						Assert("EM_MessageInterpretation should contains 'VAL_GUA Error 2'", messageInterpretation.Contains("Guarantee amount should be entered for Guarantee type 1,2,3,4,5,9,G"));
						break;
					case "CTRINFDEP.xml":
						AssertEquals("BM_CustomsStatus", expectedBM_CustomsStatus, nctsHeader.BM_CustomsStatus);
						Assert("EM_MessageInterpretation should contains 'Inspection Clerk'", messageInterpretation.Contains("<td>Inspection Clerk: G&#195;\u0096KHAN ERDO&#196;\u009eAN<br>Inspection Line: SARI</td>"));
						break;
					case "GUAINF.xml":
						AssertEquals("BM_CustomsStatus", expectedBM_CustomsStatus, nctsHeader.BM_CustomsStatus);
						Assert("EM_MessageInterpretation should contains 'Guarantee Amount'", messageInterpretation.Contains("<td>Guarantee Amount: 1000 TRY</td>"));
						break;
					case "TRC_RCV_T2N.xml":
						AssertEquals("BM_CustomsStatus", expectedBM_CustomsStatus, nctsHeader.BM_CustomsStatus);
						Assert("EM_MessageInterpretation should contains 'Proc Error'", messageInterpretation.Contains("validatecc015b_tr validation failed"));
						Assert("EM_MessageInterpretation should contains 'VAL_MAIN Error 1'", messageInterpretation.Contains("&#199;ıkış (Sınır) G&#252;mr&#252;k İdaresi sınır g&#252;mr&#252;ğ&#252; ya da RORO g&#252;mr&#252;ğ&#252; se&#231;ilebilir.(TR066666)"));
						Assert("EM_MessageInterpretation should contains 'VAL_MAIN Error 2'", messageInterpretation.Contains("NTR_C186_08----Baslik b&#246;l&#252;m&#252;ndeki g&#252;venlik kullanilmadiysa (G&#252;mr&#252;k alt birimi) kullanilamaz, kullanildiysa veri grubu varsayilan olarak 0"));
						break;
				}
			});
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals("NCTS Download Message By Index Response Message Processor", Processor.MessageFriendlyName);
		}

		public void TestSendEmail()
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

			var errorCode = "000";
			var errorDesc = "Success";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeType("NCTER", "NCTS Errors", Core.Constants.CountryCodes.Turkey);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, "NCTER", errorCode, errorDesc, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			AssertProcessMail("CC060A.xml");
			AssertProcessMail("CC028A.xml");
			AssertProcessMail("CC029B.xml");
			AssertProcessMail("CC015B_RES.xml");
			AssertProcessMail("CC015B_RES_validation_failed.xml");
			AssertProcessMail("CTRINFDEP.xml");
			AssertProcessMail("GUAINF.xml");
			AssertProcessMail("TRC_RCV_T2N.xml");
		}

		void AssertProcessMail(ZString testFileName)
		{
			var nctsHeader = Factory.New<Integration.Customs.TR.ICusInBondHeader>();
			nctsHeader.BH_HeaderType = "D";

			var queryGUID = ZGuid.NewZGuid().ToString();
			var trackingID = ZGuid.NewZGuid();

			var trnRequestMessageText = "~Test";
			var trnRequestMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.TRN, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, trnRequestMessageText, "1", queryGUID);
			var trnRequestInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.TRN, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, trnRequestMessageText, trackingID);
			trnRequestMessage.EM_EI = trnRequestInterchange.PK;

			var t1nResponseMessageText = TRMessageTestHelper.GetFileText("GetMessagesListByGuidResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var t1nResponseMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.T1N, EDIMessage.Direction.Receive, EDIMessage.Status.ProcessedOK, t1nResponseMessageText, "1", queryGUID);

			var t2nRequestMessageText = TRMessageTestHelper.GetFileText("DownloadMessageByIndex.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.");
			var t2nRequestMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.T2N, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent, t2nRequestMessageText, "2", queryGUID);
			var t2nRequestInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.T2N, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, t2nRequestMessageText, trackingID);
			t2nRequestMessage.EM_EI = t2nRequestInterchange.PK;

			var t2nResponseMessageText = TRMessageTestHelper.GetFileText(testFileName, "Enterprise.Customs.TR.Messaging.Testing.TestFiles.NCTS.DownloadMessageByIndexResponse.");
			var t2nResponseMessage = TRMessageTestHelper.CreateMessage<NCTSMessage>(Factory, TRMessageTypes.Codes.T2N, EDIMessage.Direction.Receive, EDIMessage.Status.Queued, t2nResponseMessageText, "3", "");
			var t2nResponseInterchange = TRMessageTestHelper.CreateInterchange(Factory, TRMessageTypes.Codes.T2N, EDIInterchange.Direction.Receive, EDIInterchange.Status.Received, t2nResponseMessageText, trackingID);
			t2nResponseMessage.EM_EI = t2nResponseInterchange.PK;

			trnRequestMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			trnRequestMessage.EM_LinkUniqueID = nctsHeader.PK;
			trnRequestMessage.EM_SystemCreateUser = "YK";
			t2nRequestMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			t2nRequestMessage.EM_LinkUniqueID = nctsHeader.PK;
			t2nResponseMessage.EM_LinkTable = CusInBondHeaderSchema.Constants.TableName;
			t2nResponseMessage.EM_LinkUniqueID = nctsHeader.PK;

			var trnPollingTransaction = Factory.New<CusPollingTransaction>();
			trnPollingTransaction.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			trnPollingTransaction.CPT_Type = TRMessageTypes.Codes.T1N;
			trnPollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			trnPollingTransaction.CPT_NumberOfAttempts = 5;
			trnPollingTransaction.CPT_TransactionID = queryGUID;
			trnPollingTransaction.CPT_ParentID = t1nResponseMessage.PK;
			Factory.Save();

			Processor.ProcessMessage(t2nResponseMessage);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "NCTS Download Message By Index Response Response for " + nctsHeader.BH_JobReference);
			var bodyText = email.Body;

			CombineAssertions("E-mail Content " + testFileName, () =>
			{
				switch (testFileName)
				{
					case "CC060A.xml":
						Assert("e-Mail Contains", bodyText.Contains("<td>Control Decision Made</td>"));
						break;
					case "CC028A.xml":
						Assert("e-Mail Contains", bodyText.Contains("<td>Declaration Registered</td"));
						break;
					case "CC029B.xml":
						Assert("e-Mail Contains", bodyText.Contains("<td>Movement Released</td>"));
						break;
					case "CC015B_RES.xml":
						Assert("e-Mail Contains", bodyText.Contains("<td>Registration No: 19LR5901002365322</td>"));
						break;
					case "CC015B_RES_validation_failed.xml":
						Assert("e-Mail Contains", bodyText.Contains("NOT OKORA-12899: value too large for column &quot;NCTS_TRA_DEP"));
						break;
					case "CTRINFDEP.xml":
						Assert("e-Mail Contains", bodyText.Contains("<td>Inspection Clerk: G&#195;\u0096KHAN ERDO&#196;\u009eAN<br>Inspection Line: SARI</td>"));
						break;
					case "GUAINF.xml":
						Assert("e-Mail Contains", bodyText.Contains("<td>Guarantee Amount: 1000 TRY</td>"));
						break;
					case "TRC_RCV_T2N.xml":
						Assert("e-Mail Contains", bodyText.Contains("validatecc015b_tr validation failed"));
						break;
				}
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		NCTSDownloadMessageByIndexResponseMessageProcessor Processor => fProcessor ?? (fProcessor = new NCTSDownloadMessageByIndexResponseMessageProcessor(new LoggingInformation()));
		NCTSDownloadMessageByIndexResponseMessageProcessor fProcessor;
	}
}
