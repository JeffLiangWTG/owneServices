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
	public class T2OMessageProcessorTest : ManifestMessageProcessorAbstractTest<T2OMessageProcessor>
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
			manifestHeader.AMA_JobReference = "2019/00002356";
			manifestHeader.AMA_GB = GlbBranch.CurrentBranch.PK;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;

			var requestInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.T2O, EDIInterchange.Status.Received, EDIInterchange.Direction.Transmit, messageTrackingID, "Test Interchange message");
			var requestMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T2O, EDIMessage.Direction.Transmit, EDIInterchange.Status.Received, AsycudaManifestHeaderSchema.Constants.TableName, manifestHeader.PK, "T2O Request message", requestInterchange.PK);

			var responeseInterchange = MessageTestHelper.CreateInterchange(factory, TRMessageTypes.Codes.T2O, EDIInterchange.Status.Queued, EDIInterchange.Direction.Receive, messageTrackingID, messageText);
			var responseMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T2O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, manifestHeader.PK, messageText, requestInterchange.PK);

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
			header.AMA_JobReference = "2019/00002345";

			var messageText = TRMessageTestHelper.GetFileText("T2OErrorFormatted.xml");

			var parentNodeList = new List<ZString>() { "Envelope", "Body", "OzetBeyanResponse", "Root", "Error" };
			var soapErrorMessageText = TRMessageHelper.GetNodeValue(messageText, parentNodeList, "Message");

			var soapErrorResponse = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T2O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			soapErrorResponse.EM_ApplicationReference = "ULU-2019/00002345|12345678901";

			Processor.ProcessMessage(soapErrorResponse);

			CombineAssertions(() =>
			{
				AssertEquals("Registration Status", TRMessageStatusCodeList.Codes.CLR, header.RegistrationStatus);
				AssertEquals("Message Status", TRMessageStatusCodeList.Codes.Accepted, header.AMA_MessageStatus);
			});
		}

		[TestDate(2022, 4, 2, 17, 5, 12)]
		public void TestCreateCusPollingTransactionRecord()
		{
			var messageText = TRMessageTestHelper.GetFileText("Common.IslemSorgula3Response.xml");
			var resultMessages = TRMessageHelper.GetChildNodesFromElement(messageText, "Sonuc", "GUID", "Optime");
			var temporaryQueryGUID = ZString.Empty;
			var operationTime = ZDateTime.Empty;
			foreach (var item in resultMessages)
			{
				if (item.Length == 2)
				{
					if (ZDateTime.TryParseISO8601Date(item[1], out var registrationDate))
					{
						if (operationTime.IsEmpty || operationTime <= registrationDate)
						{
							temporaryQueryGUID = item[0];
							operationTime = registrationDate;
						}
					}
				}
			}

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "2019/00002345";

			var message = MessageTestHelper.CreateMessage(Factory, TRMessageTypes.Codes.T2O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			message.EM_ApplicationReference = temporaryQueryGUID;
			Factory.Save();
			Processor.ProcessMessage(message);

			var query = new ZQuery(CusPollingTransactionSchema.CPT_ParentID, message.PK);
			var pollingTransactions = Factory.Load<CusPollingTransaction>(query);
			AssertEquals("There should be one CusPollingTransaction record created", 1, pollingTransactions.Length);

			var pollingTransaction = pollingTransactions[0];
			CombineAssertions("Cus Polling Transaction Record", () =>
			{
				AssertEquals("CPT_ApplicationCode", EDIMessage.ApplicationCodes.TRCustoms, pollingTransaction.CPT_ApplicationCode);
				AssertEquals("CPT_Type", TRMessageTypes.Codes.T2O, pollingTransaction.CPT_Type);
				AssertEquals("CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.OPN, pollingTransaction.CPT_Status);
				AssertEquals("CPT_NumberOfAttempts", 5, pollingTransaction.CPT_NumberOfAttempts.ToZInt());
				AssertEquals("CPT_EarliestTimeOfNextAttemptUtc", message.EM_SystemCreateTimeUtc.AddMinutes(1), pollingTransaction.CPT_EarliestTimeOfNextAttemptUtc);
				AssertEquals("CPT_TransactionID", temporaryQueryGUID, pollingTransaction.CPT_TransactionID);
			});
		}

		public void TestEM_MessageDataShouldHasGUIDAndEmpty()
		{
			var messageText = TRMessageTestHelper.GetFileText("RequestResult.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.Incoming.");
			var guID = TRMessageHelper.GetNodeValue(messageText, "//x:Root/Response/Guid", "http://schemas.microsoft.com/BizTalk/2003/Any");

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "2019/00002345";

			var message = MessageTestHelper.CreateMessage(Factory, TRMessageTypes.Codes.T2O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			message.EM_ApplicationReference = guID;
			Factory.Save();

			Processor.ProcessMessage(message);

			AssertEquals("EM_ApplicationReference should has guID value", guID, message.EM_ApplicationReference);

			message.EM_MessageText = ZString.Empty;
			message.EM_ApplicationReference = ZString.Empty;

			Processor.ProcessMessage(message);
			AssertEquals("EM_ApplicationReference should be empty", ZString.Empty, message.EM_ApplicationReference);
		}

		protected override ManifestMessageProcessorBase Processor => new T2OMessageProcessor(logger);
	}
}

