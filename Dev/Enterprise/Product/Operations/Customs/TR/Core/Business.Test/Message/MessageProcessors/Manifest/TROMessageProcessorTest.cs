using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using AsycudaManifestHeader = Enterprise.Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class TROMessageProcessorTest : ManifestMessageProcessorAbstractTest<TROMessageProcessor>
	{
		public void TestProcessInvalidCrendentialMessage()
		{
			var factory = Factory;
			var messageTrackingID = new ZGuid("E7FAB118-139D-4305-B109-91908D2A274F");

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff1 = group.Staff.AddNew();
			staff1.GS_Code = "U1";
			staff1.GS_LoginName = "U1";
			staff1.GS_EmailAddress = "test@ulutestmail.com";
			factory.Save();

			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification("EOG", group.PK, false));

			var messageText = TRMessageTestHelper.GetFileText("CredentialError.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "ULU-2019/00002356";
			manifestHeader.AMA_GB = GlbBranch.CurrentBranch.PK;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;

			var requestInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TRO, EDIInterchange.Status.Received, EDIInterchange.Direction.Transmit, messageTrackingID, "Test Interchange message");
			var requestMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRO, EDIMessage.Direction.Transmit, EDIInterchange.Status.Received, AsycudaManifestHeaderSchema.Constants.TableName, manifestHeader.PK, "TRO Request message", requestInterchange.PK);

			var responeseInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TRO, EDIInterchange.Status.Queued, EDIInterchange.Direction.Receive, messageTrackingID, messageText);
			var responseMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRO, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, manifestHeader.PK, messageText, requestInterchange.PK);
			var userID = "12345678901";

			var externalPassword = Factory.NewWithValidTestData<GlbExternalPassword>();
			externalPassword.GP_UserID = userID;
			externalPassword.GP_PasswordType = PasswordTypesList.Codes.TRK;
			externalPassword.GP_GC = GlbBranch.CurrentBranch.Company.PK;
			externalPassword.GP_GS = staff1.PK;
			externalPassword.GP_StatusReason = "Test";
			externalPassword.GP_CurrentPassword = "SomeBigText";

			Factory.Save();

			Processor.ProcessMessage(responseMessage);

			var newExternalPassword = NewFactory().Load<GlbExternalPassword>(externalPassword.PK);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Message Response (Failure) for " + manifestHeader.AMA_JobReference);
			var bodyText = email.Body;

			CombineAssertions(() =>
			{
				AssertNotNull(newExternalPassword);
				AssertEquals(PasswordStatusList.Codes.PasswordOK, newExternalPassword.GP_PasswordStatus);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The notification for Company " + GlbCompany.CurrentCompany.GC_Name + " should be use.", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains Error Information", bodyText.Contains("Error Message:"));
				AssertEquals("Registration Status", "ERR", manifestHeader.RegistrationStatus);
				AssertEquals("Message Status", "ERR", manifestHeader.AMA_MessageStatus);
			});
		}

		public void TestHasSOAPLevelMessageError()
		{
			var factory = Factory;

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2019/00002345";

			var messageText = TRMessageTestHelper.GetFileText("TROErrorFormatted.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");

			var parentNodeList = new List<ZString>() { "Envelope", "Body", "OzetBeyanResponse", "Root", "Error" };
			var soapErrorMessageText = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "Message");

			var soapErrorResponse = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRO, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			soapErrorResponse.EM_ApplicationReference = "ULU-2019/00002345|12345678901";

			Processor.ProcessMessage(soapErrorResponse);

			CombineAssertions(() =>
			{
				AssertEquals("Registration Status", TRMessageStatusCodeList.Codes.Error, header.RegistrationStatus);
				AssertEquals("Message Status", TRMessageStatusCodeList.Codes.Error, header.AMA_MessageStatus);
			});
		}

		[TestDate(2022, 4, 2, 17, 5, 12)]
		public void TestCreateCusPollingTransactionRecord()
		{
			var messageText = TRMessageTestHelper.GetFileText("RequestResult.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			var queryGUID = TRMessageHelper.GetNodeValue(messageText, "//x:Root/Response/Guid", "http://schemas.microsoft.com/BizTalk/2003/Any");
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2019/00002345";

			var message = MessageTestHelper.CreateMessage(Factory, TRMessageTypes.Codes.TRO, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			message.EM_ApplicationReference = queryGUID;
			Factory.Save();
			Processor.ProcessMessage(message);

			var query = new ZQuery(CusPollingTransactionSchema.CPT_ParentID, message.PK);
			var pollingTransactions = Factory.Load<CusPollingTransaction>(query);
			AssertEquals("There should be one CusPollingTransaction record created", 1, pollingTransactions.Length);

			var pollingTransaction = pollingTransactions[0];
			CombineAssertions(() =>
			{
				AssertEquals(EDIMessage.ApplicationCodes.TRCustoms, pollingTransaction.CPT_ApplicationCode);
				AssertEquals(TRMessageTypes.Codes.TRO, pollingTransaction.CPT_Type);
				AssertEquals(Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN, pollingTransaction.CPT_Status);
				AssertEquals(5, pollingTransaction.CPT_NumberOfAttempts.ToZInt());
				AssertEquals(message.EM_SystemCreateTimeUtc.AddMinutes(1), pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc);
				AssertEquals(queryGUID, pollingTransaction.CPT_TransactionID);
			});
		}

		public void TestEM_MessageDataShouldHasGUIDAndEmpty()
		{
			var messageText = TRMessageTestHelper.GetFileText("RequestResult.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			var guID = TRMessageHelper.GetNodeValue(messageText, "//x:Root/Response/Guid", "http://schemas.microsoft.com/BizTalk/2003/Any");

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2019/00002345";

			var message = MessageTestHelper.CreateMessage(Factory, TRMessageTypes.Codes.TRO, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			message.EM_ApplicationReference = guID;
			Factory.Save();

			Processor.ProcessMessage(message);

			AssertEquals("EM_ApplicationReference should has guID value", guID, message.EM_ApplicationReference);

			message.EM_MessageText = ZString.Empty;
			message.EM_ApplicationReference = ZString.Empty;

			Processor.ProcessMessage(message);
			AssertEquals("EM_ApplicationReference should be empty", ZString.Empty, message.EM_ApplicationReference);
		}

		[TestDate(2022, 09, 05)]
		public void TestCreateT2OMessage()
		{
			var messageText = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"">
			  <s:Body xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
			    <OzetBeyanResponse xmlns=""http://Gumruk.BizTalk.Integration"">
			      <Root xmlns=""http://schemas.microsoft.com/BizTalk/2003/Any"">
			        <Error xmlns="""">
			          <Message>Bu referans ile tescil alınmış yada işlemdedir.</Message>
			        </Error>
			      </Root>
			    </OzetBeyanResponse>
			  </s:Body>
			</s:Envelope>";

			var messageTrackingID = new ZGuid("E7FAB118-139D-4305-B109-91908D2A274F");
			var factory = Factory;
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN0000001";
			manifestHeader.AMA_GB = GlbBranch.CurrentBranch.PK;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
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

			var responeseInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.TRO, EDIInterchange.Status.Received, EDIInterchange.Direction.Receive, messageTrackingID, messageText);
			var responseMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRO, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, manifestHeader.PK, messageText, responeseInterchange.PK);
			responseMessage.EM_SystemCreateUser = staff.GS_Code;

			Processor.ProcessMessage(responseMessage);

			var manifest = factory.Load<AsycudaManifestHeader>(manifestHeader.PK);

			CombineAssertions("Error Message", () =>
			{
				AssertEquals("Messages.Count", 3, manifest.Messages.Count);
				var message1 = ((EDIMessage)manifest.Messages[1]).EM_MessageInterpretation;
				AssertContains("EM_MessageInterpretation should contains 'has been rejected.'", message1, "Manifest Message for job MAN0000001 has been rejected.");
				AssertContains("EM_MessageInterpretation should contains 'SOAP Message'", message1, "SOAP Message");
				AssertContains("EM_MessageInterpretation should contains 'SOAP Message detail'", message1, "<td>Bu referans ile tescil alınmış yada işlemdedir.</td>");
			});

			var expectedFormettedMessageText = @"<soapenv:Envelope xmlns:tem=""http://tempuri.org/"" xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
    <tem:IslemSorgula3>
      <tem:KullaniciAdi>20201224104</tem:KullaniciAdi>
      <tem:KullaniciSifre>25d55ad283aa400af464c76d713c07ad</tem:KullaniciSifre>
      <tem:RefID>ULU-MAN0000001|20201224104</tem:RefID>
      <tem:BasIslemGunu>2022-09-05</tem:BasIslemGunu>
    </tem:IslemSorgula3>
  </soapenv:Body>
</soapenv:Envelope>";

			CombineAssertions("Sent Message", () =>
			{
				var message2 = (EDIMessage)manifest.Messages[2];
				AssertEquals("EM_IsActive", true, message2.EM_IsActive);
				AssertEquals("EM_ApplicationCode", "TRC", message2.EM_ApplicationCode);
				AssertEquals("EM_MessageType", "T2O", message2.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", "TRX", message2.EM_ReceiveTransmit);
				AssertEquals("EM_Status", "QUE", message2.EM_Status);
				AssertEquals("EM_GB", ((ManifestBase.AsycudaManifestHeader)manifest).Branch.PK, message2.EM_GB);
				AssertEquals("EM_GE", message2.Department.PK, message2.EM_GE);
				AssertEquals("EM_FormattedMessageText", expectedFormettedMessageText, message2.EM_FormattedMessageText);
			});
		}

		public void TestProcessTROResponseMessage()
		{
			var factory = Factory;

			var messageText = TRMessageTestHelper.GetFileText("RequestResult.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			var queryGUID = TRMessageHelper.GetNodeValue(messageText, "//x:Root/Response/Guid", "http://schemas.microsoft.com/BizTalk/2003/Any");
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2019/00002345";

			var message = MessageTestHelper.CreateMessage(Factory, TRMessageTypes.Codes.TRO, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			message.EM_ApplicationReference = queryGUID;
			Factory.Save();
			Processor.ProcessMessage(message);

			Assert("EM_MessageInterpretation should contains 'Query GUID'", message.EM_MessageInterpretation.Contains("<td>Query GUID:</td><td>4142285b-6b4f-4eb8-9bfa-6baa3b884b06</td>"));
		}

		protected override ManifestMessageProcessorBase Processor => new TROMessageProcessor(logger);
	}
}

