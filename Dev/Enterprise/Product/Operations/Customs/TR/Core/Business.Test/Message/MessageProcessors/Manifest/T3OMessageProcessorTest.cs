using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader;

namespace Enterprise.Customs.TR.Business.Testing
{
	class T3OMessageProcessorTest : ManifestMessageProcessorAbstractTest<T3OMessageProcessor>
	{
		protected override ManifestMessageProcessorBase Processor => new T3OMessageProcessor(logger);

		public void TestProcessT3OResponseMessage()
		{
			var factory = Factory;

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2019/00002345";

			var messageText = TRMessageTestHelper.GetFileText("T3OSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			ZString[] namespaceList = { "http://schemas.xmlsoap.org/soap/envelope/", "http://tempuri.org/", "urn:schemas-microsoft-com:xml-diffgram-v1" };
			var processedDxTMessage = TRMessageHelper.GetNodeValue(messageText, "/A:Envelope/A:Body/B:IslemSonucGetir4Response/B:IslemSonucGetir4Result/C:diffgram/NewDataSet/Sonuc/GidenXML", namespaceList);
			var tescilTarihi = TRMessageHelper.GetNodeValue(processedDxTMessage, "/x:SonucBilgisi/x:TescilTarihi");
			var tescilNo = TRMessageHelper.GetNodeValue(processedDxTMessage, "/x:SonucBilgisi/x:TescilNo");

			var successMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			successMessage.EM_ApplicationReference = "ULU-2019/00002345|12345678901";

			Processor.ProcessMessage(successMessage);

			CombineAssertions(() =>
			{
				AssertEquals("RegistrationNumber", tescilNo, header.RegistrationNumber);
				AssertEquals("RegistrationDate", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.RegistrationDate.ToString("yyyyMMddhhmm"));
				AssertEquals("Registration Status", TRMessageStatusCodeList.Codes.CLR, header.RegistrationStatus);
				AssertEquals("Message Status", TRMessageStatusCodeList.Codes.Accepted, header.AMA_MessageStatus);
				Assert("EM_MessageInterpretation should contains 'has been cleared.'", successMessage.EM_MessageInterpretation.Contains("Manifest Message for job ULU-2019/00002345 has been cleared."));
				Assert("EM_MessageInterpretation should contains 'Registration Number'", successMessage.EM_MessageInterpretation.Contains("<td>Registration Number:</td><td>22066666VB0000000032</td>"));
			});

			messageText = TRMessageTestHelper.GetFileText("T3OError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			var failedMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			successMessage.EM_ApplicationReference = "ULU-2019/00002345|12345678901";

			Processor.ProcessMessage(failedMessage);

			CombineAssertions("Error Message 1", () =>
			{
				AssertEquals("Registration Status", TRMessageStatusCodeList.Codes.Error, header.RegistrationStatus);
				AssertEquals("Message Status", TRMessageStatusCodeList.Codes.Error, header.AMA_MessageStatus);
				Assert("EM_MessageInterpretation should contains 'has been rejected.'", failedMessage.EM_MessageInterpretation.Contains("Manifest Message for job ULU-2019/00002345 has been rejected."));
				Assert("EM_MessageInterpretation should contains 'Error Message'", failedMessage.EM_MessageInterpretation.Contains("Error Message"));
				Assert("EM_MessageInterpretation should contains 'Error Message detail'", failedMessage.EM_MessageInterpretation.Contains("<td>Konteyner oldugunda acentanin vergi numarasi girilmeli (TS=3432534534)</td>"));
			});

			messageText = TRMessageTestHelper.GetFileText("Manifest.Incoming.T3OError2.xml");
			failedMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			failedMessage.EM_ApplicationReference = "ULU-2019/00002345|12345678901";

			Processor.ProcessMessage(failedMessage);

			CombineAssertions("Error Message 2", () =>
			{
				AssertEquals("Registration Status", TRMessageStatusCodeList.Codes.Error, header.RegistrationStatus);
				AssertEquals("Message Status", TRMessageStatusCodeList.Codes.Error, header.AMA_MessageStatus);
				Assert("EM_MessageInterpretation should contains 'has been rejected.'", failedMessage.EM_MessageInterpretation.Contains("Manifest Message for job ULU-2019/00002345 has been rejected."));
				Assert("EM_MessageInterpretation should contains 'Error Message'", failedMessage.EM_MessageInterpretation.Contains("Error Message"));
				Assert("EM_MessageInterpretation should contains 'Error Message detail'", failedMessage.EM_MessageInterpretation.Contains("<td>Mesajınız işleme alınamamıştır, l&#252;tfen yeniden g&#246;nderiniz.</td>"));
			});
		}

		[TestDate(2022, 4, 2, 17, 5, 12)]
		public void TestUpdateCusPollingTransactionRecordForValidT3OResponseMessage()
		{
			var factory = Factory;
			var queryGUID = "4142285b-6b4f-4eb8-9bfa-6baa3b884b06";
			var trackingID = new ZGuid("E9F7165A-CF90-4B5B-856C-9E23D36344FE");
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2019/00002345";

			var t2oResponseMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T2O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, "T2O Response message", ZGuid.Empty);
			t2oResponseMessage.EM_ApplicationReference = queryGUID;

			var t3ORequestInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.T3O, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, trackingID, "T3O Request message");
			var t3ORequestMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Transmit, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, "T2O Response message", ZGuid.Empty);
			t3ORequestMessage.EM_ApplicationReference = queryGUID;
			t3ORequestMessage.EM_EI = t3ORequestInterchange.PK;

			var t3OResponseMessageText = TRMessageTestHelper.GetFileText("T3OSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			var t3OResponseInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.T3O, EDIInterchange.Direction.Receive, EDIInterchange.Status.Received, trackingID, t3OResponseMessageText);
			var t3OResponseMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, t3OResponseMessageText, ZGuid.Empty);
			t3OResponseMessage.EM_EI = t3OResponseInterchange.PK;
			t3OResponseMessage.EM_LinkUniqueID = header.PK;
			t3OResponseMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;

			var pollingTransaction = Factory.New<CusPollingTransaction>();
			pollingTransaction.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction.CPT_Type = TRMessageTypes.Codes.T2O;
			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			pollingTransaction.CPT_NumberOfAttempts = 5;
			pollingTransaction.CPT_TransactionID = queryGUID;
			pollingTransaction.CPT_ParentID = t2oResponseMessage.PK;
			Factory.Save();

			Processor.ProcessMessage(t3OResponseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.CLS, pollingTransaction.CPT_Status);
				AssertEquals("CPT_StatusTimeUtc", t3OResponseMessage.EM_SystemCreateTimeUtc, pollingTransaction.CPT_StatusTimeUtc);
			});
		}

		[TestDate(2022, 4, 2, 17, 5, 12)]
		public void TestUpdateCusPollingTransactionRecordForInvalidT3OResponseMessage()
		{
			var factory = Factory;

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZZ";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "Z1";
			staff1.GS_LoginName = "Z1";
			staff1.GS_EmailAddress = "test@test.mail.com";

			var queryGUID = "4142285b-6b4f-4eb8-9bfa-6baa3b884b06";
			var trackingID = new ZGuid("E9F7165A-CF90-4B5B-856C-9E23D36344FE");
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2019/00002345";

			var t2oRequestMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T2O, EDIMessage.Direction.Transmit, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, "T2O Request message", ZGuid.Empty);
			t2oRequestMessage.EM_SystemCreateUser = staff1.GS_Code;
			t2oRequestMessage.EM_LinkUniqueID = header.PK;
			t2oRequestMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			var t2oResponseMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T2O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, "T2O Response message", ZGuid.Empty);

			var t3ORequestInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.T3O, EDIInterchange.Direction.Transmit, EDIInterchange.Status.Sent, trackingID, "T3O Request message");
			var t3ORequestMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Transmit, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, "T3O Request message", ZGuid.Empty);
			t3ORequestMessage.EM_ApplicationReference = queryGUID;
			t3ORequestMessage.EM_EI = t3ORequestInterchange.PK;

			var t3OResponseMessageText = TRMessageTestHelper.GetFileText("T3ONoGidenXML.xml");
			var t3OResponseInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.T3O, EDIInterchange.Direction.Receive, EDIInterchange.Status.Received, trackingID, t3OResponseMessageText);
			var t3OResponseMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, t3OResponseMessageText, ZGuid.Empty);
			t3OResponseMessage.EM_EI = t3OResponseInterchange.PK;
			t3OResponseMessage.EM_LinkUniqueID = header.PK;
			t3OResponseMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;

			var pollingTransaction = Factory.New<CusPollingTransaction>();
			pollingTransaction.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction.CPT_Type = TRMessageTypes.Codes.T2O;
			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc = ZDateTime.UtcNow.AddMinutes(1);
			pollingTransaction.CPT_NumberOfAttempts = 5;
			pollingTransaction.CPT_TransactionID = queryGUID;
			pollingTransaction.CPT_ParentID = t2oResponseMessage.PK;
			Factory.Save();

			Processor.ProcessMessage(t3OResponseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN, pollingTransaction.CPT_Status);
				AssertEquals("CPT_StatusTimeUtc", t3OResponseMessage.EM_SystemCreateTimeUtc, pollingTransaction.CPT_StatusTimeUtc);
				AssertEquals("CPT_EarliestTimeOfNextAttemptUtc", t2oResponseMessage.EM_SystemCreateTimeUtc.AddMinutes(3), pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc);
				AssertEquals("CPT_NumberOfAttempts", Convert.ToByte(4), pollingTransaction.CPT_NumberOfAttempts);
			});

			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			Processor.ProcessMessage(t3OResponseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN, pollingTransaction.CPT_Status);
				AssertEquals("CPT_StatusTimeUtc", t3OResponseMessage.EM_SystemCreateTimeUtc, pollingTransaction.CPT_StatusTimeUtc);
				AssertEquals("CPT_EarliestTimeOfNextAttemptUtc", t2oResponseMessage.EM_SystemCreateTimeUtc.AddMinutes(5), pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc);
				AssertEquals("CPT_NumberOfAttempts", Convert.ToByte(3), pollingTransaction.CPT_NumberOfAttempts);
			});

			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			Processor.ProcessMessage(t3OResponseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN, pollingTransaction.CPT_Status);
				AssertEquals("CPT_StatusTimeUtc", t3OResponseMessage.EM_SystemCreateTimeUtc, pollingTransaction.CPT_StatusTimeUtc);
				AssertEquals("CPT_EarliestTimeOfNextAttemptUtc", t2oResponseMessage.EM_SystemCreateTimeUtc.AddMinutes(10), pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc);
				AssertEquals("CPT_NumberOfAttempts", Convert.ToByte(2), pollingTransaction.CPT_NumberOfAttempts);
			});

			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			Processor.ProcessMessage(t3OResponseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN, pollingTransaction.CPT_Status);
				AssertEquals("CPT_StatusTimeUtc", t3OResponseMessage.EM_SystemCreateTimeUtc, pollingTransaction.CPT_StatusTimeUtc);
				AssertEquals("CPT_EarliestTimeOfNextAttemptUtc", t2oResponseMessage.EM_SystemCreateTimeUtc.AddMinutes(30), pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc);
				AssertEquals("CPT_NumberOfAttempts", Convert.ToByte(1), pollingTransaction.CPT_NumberOfAttempts);
			});

			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("ESM", group.PK, false));
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			Processor.ProcessMessage(t3OResponseMessage);

			CombineAssertions(() =>
			{
				AssertEquals("CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.ERR, pollingTransaction.CPT_Status);
				AssertEquals("CPT_StatusTimeUtc", t3OResponseMessage.EM_SystemCreateTimeUtc, pollingTransaction.CPT_StatusTimeUtc);
				AssertEquals("Registration Status", TRMessageStatusCodeList.Codes.Error, header.RegistrationStatus);
				AssertEquals("Message Status", TRMessageStatusCodeList.Codes.Error, (header as IMessageAttachee).MessageStatus);
				AssertNotNull("Should have sent an email", Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Message Response (Failure) for " + header.AMA_JobReference));
			});
		}

		public void TestCusStatementWhenSuccessMessage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2019/00002345";
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			var messageText = TRMessageTestHelper.GetFileText("T3OSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			var message = MessageTestHelper.CreateMessage(Factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);

			Factory.Save();

			Processor.ProcessMessage(message);

			var dueDate = new ZDate(header.RegistrationDate.Year, header.RegistrationDate.Month, 20).AddMonths(1);
			var query = new ZQuery();
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, CusStatementHeaderTypes.Codes.GlobalManifest);
			query.AddToFilter(CusStatementHeaderSchema.B2_DueDate, dueDate);

			var cusStatementHeader = Factory.Load<CusStatementHeader>(query).FirstOrDefault();

			CombineAssertions("Called FindOrCreateCusStatement", () =>
			{
				AssertEquals("B2_IsMonthlyStatement", ZBool.True, cusStatementHeader.B2_IsMonthlyStatement);
				AssertEquals("B2_Status", "PRE", cusStatementHeader.B2_Status);
				AssertEquals("B2_StatementType", "M", cusStatementHeader.B2_StatementType);
				AssertEquals("B2_PaymentType", "1", cusStatementHeader.B2_PaymentType);
				AssertEquals("B2_DueDate", new ZDateTime(2022, 12, 20), cusStatementHeader.B2_DueDate);
				AssertEquals("B2_PaymentParty", "BRK", cusStatementHeader.B2_PaymentParty);
				AssertEquals("B2_PeriodStartDate", new ZDate(2022, 11, 01), cusStatementHeader.B2_PeriodStartDate);
				AssertEquals("B2_PeriodEndDate", new ZDate(2022, 11, 30), cusStatementHeader.B2_PeriodEndDate);
			});
		}

		[TestDate(2022, 09, 05)]
		public void TestCreateTRMMessage()
		{
			var messageText = TRMessageTestHelper.GetFileText("Manifest.Incoming.T3OSuccess.xml");
			var queryGUID = "4142285b-6b4f-4eb8-9bfa-6baa3b884b06";
			var messageTrackingID = new ZGuid("E7FAB118-139D-4305-B109-91908D2A274F");
			var factory = Factory;
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN0000001";
			manifestHeader.AMA_GB = GlbBranch.CurrentBranch.PK;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
			manifestHeader.AMA_CustomsOffice = "066666";
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			staff.GS_LoginName = "Kevin";
			staff.GS_EmailAddress = "kevin.zhang@wisetechglobal.com";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";
			Factory.Save();

			var requestInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TRO, EDIMessage.Direction.Transmit, EDIInterchange.Direction.Transmit, messageTrackingID, "Test request Interchange message");
			var requestMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRO, EDIMessage.Direction.Transmit, EDIInterchange.Status.Received, AsycudaManifestHeaderSchema.Constants.TableName, manifestHeader.PK, "TRO Request message", requestInterchange.PK);
			requestMessage.EM_SystemCreateUser = staff.GS_Code;
			requestMessage.EM_ApplicationReference = queryGUID;

			var requestInterchangeT2O = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.T2O, EDIMessage.Direction.Transmit, EDIInterchange.Direction.Transmit, messageTrackingID, "Test request Interchange message");
			var requestMessageT2O = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T2O, EDIMessage.Direction.Transmit, EDIInterchange.Status.Received, AsycudaManifestHeaderSchema.Constants.TableName, manifestHeader.PK, "TRO Request message", requestInterchangeT2O.PK);
			requestMessageT2O.EM_SystemCreateUser = staff.GS_Code;
			requestMessageT2O.EM_ApplicationReference = queryGUID;

			var responeseInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.T3O, EDIInterchange.Status.Received, EDIInterchange.Direction.Receive, messageTrackingID, messageText);
			var responseMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, manifestHeader.PK, messageText, responeseInterchange.PK);
			responseMessage.EM_SystemCreateUser = staff.GS_Code;
			responseMessage.EM_ApplicationReference = queryGUID;

			var pollingTransaction = Factory.New<CusPollingTransaction>();
			pollingTransaction.CPT_ApplicationCode = EDIMessage.ApplicationCodes.TRCustoms;
			pollingTransaction.CPT_Type = TRMessageTypes.Codes.T2O;
			pollingTransaction.CPT_Status = Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND;
			pollingTransaction.CPT_NumberOfAttempts = 5;
			pollingTransaction.CPT_TransactionID = queryGUID;
			pollingTransaction.CPT_ParentID = responseMessage.PK;

			Processor.ProcessMessage(responseMessage);

			var manifest = factory.Load<AsycudaManifestHeader>(manifestHeader.PK);

			var expectedMessageText = @"<soapenv:Envelope xmlns:tem=""http://tempuri.org/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <tem:ozbyMuayeneMemuruAdiSorgula>
      <tem:KullaniciAdi>20201224104</tem:KullaniciAdi>
      <tem:KullaniciSifre>25d55ad283aa400af464c76d713c07ad</tem:KullaniciSifre>
      <tem:Gumruk>066666</tem:Gumruk>
      <tem:TescilNo>22066666VB0000000032</tem:TescilNo>
    </tem:ozbyMuayeneMemuruAdiSorgula>
  </soapenv:Body>
</soapenv:Envelope>";

			var message2 = (EDIMessage)manifest.Messages[3];
			CombineAssertions("Sent Message", () =>
			{
				AssertEquals("EM_IsActive", true, message2.EM_IsActive);
				AssertEquals("EM_ApplicationCode", "TRC", message2.EM_ApplicationCode);
				AssertEquals("EM_MessageType", "TRM", message2.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", "TRX", message2.EM_ReceiveTransmit);
				AssertEquals("EM_Status", "QUE", message2.EM_Status);
				AssertEquals("EM_GB", ((ManifestBase.AsycudaManifestHeader)manifest).Branch.PK, message2.EM_GB);
				AssertEquals("EM_GE", message2.Department.PK, message2.EM_GE);
				AssertEquals("EM_FormattedMessageText", expectedMessageText, message2.EM_FormattedMessageText);
			});

			var messageInterpretation = message2.EM_MessageInterpretation;
			CombineAssertions("Message Interpretation", () =>
			{
				Assert("EM_MessageInterpretation should contains 'successfully'", messageInterpretation.Contains("Global Manifest Message Type TRM sent successfully."));
				Assert("EM_MessageInterpretation should contains 'Customs Office'", messageInterpretation.Contains("<td>Customs Office:</td><td>066666</td>"));
				Assert("EM_MessageInterpretation should contains 'Registration Number'", messageInterpretation.Contains("<td>Registration Number:</td><td>22066666VB0000000032</td>"));
			});
		}

		public void TestStampTaxCalculation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			var startDate = ZDateTime.Now.AddDays(-1);
			var endDate = ZDateTime.Now.AddDays(1);
			helper.CreateTaxOrFee("GMS", 14.60000000m, "TR", startDate, endDate, "TR Global Manifest Stamp Duty");
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2019/00002345";
			header.AMA_ManifestType = "DENIHR";
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			var messageText = TRMessageTestHelper.GetFileText("T3OSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			var message = MessageTestHelper.CreateMessage(Factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			message.EM_LinkUniqueID = header.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			Factory.Save();

			Processor.ProcessMessage(message);

			var gmsDuty = header.GetType().GetProperty("GlobalManifestStampDutyValue").GetValue(header);
			var totalDuty = header.GetType().GetProperty("TotalStampDutyValue").GetValue(header);
			CombineAssertions("CalculateStampDuties for SEA", () =>
			{
				AssertEquals("CalculateStampDuties should be invoked on AsycudaManifestHeader", 14.60000000m, gmsDuty);
				AssertEquals("CalculateStampDuties should be invoked on AsycudaManifestHeader", 14.60000000m, totalDuty);
			});

			var header2 = Factory.New<AsycudaManifestHeader>();
			header2.AMA_ManifestType = "HAVIHR";
			header2.AMA_JobReference = "ULU-2019/00002346";
			header2.AMA_TransportMode = Core.Constants.TransportModes.Air;

			messageText = TRMessageTestHelper.GetFileText("T3OSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			message = MessageTestHelper.CreateMessage(Factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header2.PK, messageText, ZGuid.Empty);
			Factory.Save();

			Processor.ProcessMessage(message);

			var gmsDuty2 = header2.GetType().GetProperty("GlobalManifestStampDutyValue").GetValue(header2);
			var totalDuty2 = header2.GetType().GetProperty("TotalStampDutyValue").GetValue(header2);
			CombineAssertions("CalculateStampDuties for AIR", () =>
			{
				AssertEquals("CalculateStampDuties should be invoked on AsycudaManifestHeader", 14.60000000m, gmsDuty2);
				AssertEquals("CalculateStampDuties should be invoked on AsycudaManifestHeader", 14.60000000m, totalDuty2);
			});
		}

		public void TestGeneratedMail()
		{
			var factory = Factory;

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ZZZ";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "Z1";
			staff1.GS_LoginName = "Z1";
			staff1.GS_EmailAddress = "test@test.mail.com";
			factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2019/00002345";

			var messageText = TRMessageTestHelper.GetFileText("T3OSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			ZString[] namespaceList = { "http://schemas.xmlsoap.org/soap/envelope/", "http://tempuri.org/", "urn:schemas-microsoft-com:xml-diffgram-v1" };
			var processedDxTMessage = TRMessageHelper.GetNodeValue(messageText, "/A:Envelope/A:Body/B:IslemSonucGetir4Response/B:IslemSonucGetir4Result/C:diffgram/NewDataSet/Sonuc/GidenXML", namespaceList);
			var tescilTarihi = TRMessageHelper.GetNodeValue(processedDxTMessage, "/x:SonucBilgisi/x:TescilTarihi");
			var tescilNo = TRMessageHelper.GetNodeValue(processedDxTMessage, "/x:SonucBilgisi/x:TescilNo");

			var message = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			message.EM_MessageNum = "99";

			var setupMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Sent, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			setupMessage.EM_MessageNum = "99";
			setupMessage.EM_SystemCreateUser = staff1.GS_Code;

			var sendemailStaffMember = "ESM";
			SetupRegistryData(sendemailStaffMember, group.PK, false);
			AssertSuccessMail(header, message, staff1, tescilNo, tescilTarihi);
			SetupRegistryData(sendemailStaffMember, group.PK, true);
			AssertNullMail(header, message);

			var sendemailNominatedGroup = "ENG";
			SetupRegistryData(sendemailNominatedGroup, group.PK, false);
			AssertSuccessMail(header, message, staff1, tescilNo, tescilTarihi);
			SetupRegistryData(sendemailNominatedGroup, group.PK, true);
			AssertNullMail(header, message);

			var sendEmailStaffMemberandNominatedGroup = "ESG";
			SetupRegistryData(sendEmailStaffMemberandNominatedGroup, group.PK, false);
			AssertSuccessMail(header, message, staff1, tescilNo, tescilTarihi);
			SetupRegistryData(sendEmailStaffMemberandNominatedGroup, group.PK, true);
			AssertNullMail(header, message);

			var sendEmailStaffMemberOrNominatedGroupForGroup = "EOG";
			SetupRegistryData(sendEmailStaffMemberOrNominatedGroupForGroup, group.PK, false);
			AssertSuccessMail(header, message, staff1, tescilNo, tescilTarihi);
			SetupRegistryData(sendEmailStaffMemberOrNominatedGroupForGroup, group.PK, true);
			AssertNullMail(header, message);

			var sendNoEmail = "NOE";
			SetupRegistryData(sendNoEmail, group.PK, false);
			AssertNullMail(header, message);
			SetupRegistryData(sendNoEmail, group.PK, true);
			AssertNullMail(header, message);

			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2019/00002346";

			messageText = TRMessageTestHelper.GetFileText("T3OError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			message = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			message.EM_MessageNum = "88";

			setupMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Sent, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			setupMessage.EM_MessageNum = "88";
			setupMessage.EM_SystemCreateUser = staff1.GS_Code;

			SetupRegistryData(sendemailStaffMember, group.PK, false);
			AssertFailMail(header, message, staff1);
			SetupRegistryData(sendemailStaffMember, group.PK, true);
			AssertFailMail(header, message, staff1);

			SetupRegistryData(sendemailNominatedGroup, group.PK, false);
			AssertFailMail(header, message, staff1);
			SetupRegistryData(sendemailNominatedGroup, group.PK, true);
			AssertFailMail(header, message, staff1);

			SetupRegistryData(sendEmailStaffMemberandNominatedGroup, group.PK, false);
			AssertFailMail(header, message, staff1);
			SetupRegistryData(sendEmailStaffMemberandNominatedGroup, group.PK, true);
			AssertFailMail(header, message, staff1);

			SetupRegistryData(sendEmailStaffMemberOrNominatedGroupForGroup, group.PK, false);
			AssertFailMail(header, message, staff1);
			SetupRegistryData(sendEmailStaffMemberOrNominatedGroupForGroup, group.PK, true);
			AssertFailMail(header, message, staff1);

			SetupRegistryData(sendNoEmail, group.PK, false);
			AssertNullMail(header, message);
			SetupRegistryData(sendNoEmail, group.PK, true);
			AssertNullMail(header, message);
		}

		public void TestRegistrationUser()
		{
			SetUserInfo();

			var factory = Factory;

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2019/00002345";

			var messageText = TRMessageTestHelper.GetFileText("T3OSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			var successMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			successMessage.EM_ApplicationReference = "ULU-2019/00002345|12345678901";

			var outgoingTROMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRO, EDIMessage.Direction.Transmit, EDIInterchange.Status.Sent, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			outgoingTROMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 24, 13, 30, 38);
			outgoingTROMessage.EM_MessageOwner = "BP";

			var outgoingTROMessage1 = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRO, EDIMessage.Direction.Transmit, EDIInterchange.Status.Sent, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			outgoingTROMessage1.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 24, 14, 02, 51);
			outgoingTROMessage1.EM_MessageOwner = "WZG";

			var outgoingTROMessage2 = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRO, EDIMessage.Direction.Transmit, EDIInterchange.Status.Sent, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			outgoingTROMessage2.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 24, 15, 43, 34);
			outgoingTROMessage2.EM_MessageOwner = "KNZ";

			var outgoingT1OMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T1O, EDIMessage.Direction.Transmit, EDIInterchange.Status.Sent, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			outgoingT1OMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 20, 07, 17, 04);
			outgoingT1OMessage.EM_MessageOwner = "BP";

			var incomingT1OMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T1O, EDIMessage.Direction.Receive, EDIInterchange.Status.Received, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			incomingT1OMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 20, 07, 24, 36);

			var outgoingT3OMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Transmit, EDIInterchange.Status.Sent, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			outgoingT3OMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 20, 07, 45, 21);
			outgoingT3OMessage.EM_MessageOwner = "AAA";

			var incomingT3OMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T3O, EDIMessage.Direction.Receive, EDIInterchange.Status.Received, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			incomingT3OMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 04, 20, 07, 51, 56);

			Processor.ProcessMessage(successMessage);

			CombineAssertions(() =>
			{
				AssertEquals("KNZ", header.AMA_GS_NKCustomsAgent);
				Assert("EM_MessageInterpretation should contains 'has been cleared.'", successMessage.EM_MessageInterpretation.Contains("Manifest Message for job ULU-2019/00002345 has been cleared."));
				Assert("EM_MessageInterpretation should contains 'Registration Number'", successMessage.EM_MessageInterpretation.Contains("<td>Registration Number:</td><td>22066666VB0000000032</td>"));
				AssertEquals("Registration Status", TRMessageStatusCodeList.Codes.CLR, header.RegistrationStatus);
				AssertEquals("Message Status", TRMessageStatusCodeList.Codes.Accepted, header.AMA_MessageStatus);
			});
		}

		void SetupRegistryData(ZString sendMode, ZGuid sendGroupPK, ZBool sendErrorOnly)
		{
			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification(sendMode, sendGroupPK, sendErrorOnly));
		}

		void AssertSuccessMail(AsycudaManifestHeader header, EDIMessage message, GlbStaff staff1, ZString tescilNo, ZString tescilTarihi)
		{
			Processor.ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Message Response for " + header.AMA_JobReference);
			var bodyText = email.Body;

			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Registration Number", bodyText.Contains("Registration Number"));
				Assert("Contains Issue Date", bodyText.Contains("Issue Date"));
				Assert("Contains cleared", bodyText.Contains("has been cleared."));
				AssertEquals("RegistrationNumber", tescilNo, header.RegistrationNumber);
				AssertEquals("RegistrationDate", new ZDateTime(tescilTarihi).ToString("yyyyMMddhhmm"), header.RegistrationDate.ToString("yyyyMMddhhmm"));
				AssertEquals("RegistrationStatus", TRMessageStatusCodeList.Codes.CLR, header.RegistrationStatus);
				AssertEquals("AMA_MessageStatus", "ACP", header.AMA_MessageStatus);
			});

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		void AssertFailMail(AsycudaManifestHeader header, EDIMessage message, GlbStaff staff1)
		{
			Processor.ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Message Response (Failure) for " + header.AMA_JobReference);
			var bodyText = email.Body;

			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Information", bodyText.Contains("A response message has been received from Customs."));
				Assert("Contains Error Information", bodyText.Contains("Shown below is a summary of relevant information received in the message."));
				Assert("Contains Error Information", bodyText.Contains("Konteyner oldugunda acentanin vergi numarasi girilmeli (TS=3432534534)"));
				Assert("Contains rejected", bodyText.Contains("has been rejected."));
			});
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		void AssertNullMail(AsycudaManifestHeader header, EDIMessage message)
		{
			Processor.ProcessMessage(message);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Status Message Response for " + header.AMA_JobReference);
			AssertNull(email);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		void SetUserInfo()
		{
			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			staff.GS_LoginName = "KNZ";
			staff.GS_FullName = "KNZ Testing Signed User";

			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "BP";
			staff1.GS_LoginName = "BP";
			staff1.GS_FullName = "BP Testing Signed User";

			var staff2 = group.Staff.AddNew();
			staff2.GS_Code = "WZG";
			staff2.GS_LoginName = "WZG";
			staff2.GS_FullName = "WZG Testing Signed User";

			var staff3 = group.Staff.AddNew();
			staff3.GS_Code = "AAA";
			staff3.GS_LoginName = "AAA";
			staff3.GS_FullName = "AAA Testing Signed User";

			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";

			var user1 = TRGlbStaffWrapper.Get(staff3).TRBPassword;
			user1.GP_UserID = "20201224199";
			user1.CurrentDecryptedPassword = "12345699";
		}
	}
}

