using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	class DKOMessageProcessorTest : DeclarationMessageProcessorBaseTest<DKOMessageProcessor>
	{
		public void TestGetCorrectBranchPK()
		{
			var messageObjects = CreateMessageWithOrigins(GetDefaultMessageText(), "DKO001");

			var incomingMessage = messageObjects.incomingMessage;
			Processor.PreProcessMessage(incomingMessage);

			AssertEquals("GetCorrectBranchPK should have returned the correct BranchPK", messageObjects.messageAttachee.JE_GB, incomingMessage.EM_GB);
		}

		public void TestProcessMessage_EmptyMessage()
		{
			var emptyMessage = TRMessageTestHelper.CreateMessage<TRImportExportMessage>(Factory, TRMessageTypes.Codes.DKO, EDIInterchange.Direction.Receive, EDIInterchange.Status.Sent, string.Empty, "DKORCV001", string.Empty);

			AssertNoExceptionThrown("Processing empty DKO message should not throw exception.",
				() =>
				{
					Processor.PreProcessMessage(emptyMessage);
					Processor.ProcessMessage(emptyMessage);
				}
			);
			AssertNull("Should not create CusPollingTransaction.", Factory.LoadTop1<CusPollingTransaction>(new ZQuery().AddToFilter(CusPollingTransactionSchema.CPT_ParentID, emptyMessage.PK)));
		}

		public void TestProcessMessage()
		{
			var messageObjects = CreateMessageWithOrigins(GetDefaultMessageText(), "DKO001");
			var incomingMessage = messageObjects.incomingMessage;
			incomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);
			var createdPollingTransaction = Factory.LoadTop1<CusPollingTransaction>(new ZQuery().AddToFilter(CusPollingTransactionSchema.CPT_ParentID, incomingMessage.PK));

			var actualMessage = incomingMessage.EM_MessageInterpretation;

			AssertNotNull("Should have created a CusPollingTransaction linked to the incoming message.", createdPollingTransaction);
			CombineAssertions("Created PollingTransaction created.", () =>
			{
				AssertEquals("CPT_ApplicationCode", EDIMessage.ApplicationCodes.TRCustoms, createdPollingTransaction.CPT_ApplicationCode);
				AssertEquals("CPT_Type", DefaultMessageType, createdPollingTransaction.CPT_Type);
				AssertEquals("CPT_TransactionID", "4142285B-6B4F-4EB8-9BFA-6BAA3B884B06", createdPollingTransaction.CPT_TransactionID);
				AssertNotNullOrEmpty(actualMessage);
				Assert("EM_MessageInterpretation should contains 'Query GUID'", actualMessage.Contains("<td>Query GUID:</td><td>4142285B-6B4F-4EB8-9BFA-6BAA3B884B06</td>"));
			});

			AssertStatusesMessage(incomingMessage, TRMessageStatusCodeList.Codes.Accepted, EntryStatusTypeList.Codes.ACM, EDIMessage.Status.ProcessedOK);
		}

		public void TestCreateEmailBodyWithImage()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";

			var messageObjects = CreateMessageWithOrigins(GetDefaultMessageText(), "DK1001");
			var originalMessage = messageObjects.originalMessage;
			originalMessage.EM_SystemCreateUser = user.GS_Code;

			var incomingMessage = messageObjects.incomingMessage;

			Processor.PreProcessMessage(incomingMessage);
			Processor.ProcessMessage(incomingMessage);

			var cusEntryHeader = incomingMessage.EM_LinkedObject as CusEntryHeader;
			var declarationReference = cusEntryHeader.Declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(declarationReference));

			var imagePath = $"Enterprise.Customs.TR.Business.Message.EDIMessage.HtmlTemplates.Success.jpg";
			string imageBase64 = default;
			imageBase64 = getImageValue(imagePath);

			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("Subject", "Customs Declaration Response for 3-B00001000", email.Subject);
				Assert("Contains imageBase64", email.Body.Contains(imageBase64));
				Assert("Email body should contains 'Message Information'", email.Body.Contains("Customs Declaration Control Message for Job 3-B00001000 has been Accepted. For details please follow the Link to the Customs Declaration"));
			});
		}

		static string getImageValue(string imagePath)
		{
			string imageBase64;
			using (var imageStream = typeof(TRManifestMessage).Assembly.GetManifestResourceStream(imagePath))
			{
				var imageBytes = new byte[imageStream.Length];
				_ = imageStream.Read(imageBytes, 0, imageBytes.Length);
				imageBase64 = Convert.ToBase64String(imageBytes);
			}

			return imageBase64;
		}

		protected override DKOMessageProcessor Processor => new DKOMessageProcessor(logger);

		protected override ZString DefaultMessageType => TRMessageTypes.Codes.DKO;

		ZString GetDefaultMessageText() => TRMessageTestHelper.GetFileText("DKOSuccess.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Declaration.Incoming.");

		protected override void SetUp()
		{
			base.SetUp();
		}
	}
}
