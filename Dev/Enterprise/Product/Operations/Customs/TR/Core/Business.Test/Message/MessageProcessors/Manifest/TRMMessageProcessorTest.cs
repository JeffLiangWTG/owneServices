using System;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using AsycudaManifestHeader = Enterprise.Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class TRMMessageProcessorTest : ManifestMessageProcessorAbstractTest<TRMMessageProcessor>
	{
		public void TestProcessTRMResponseMessage()
		{
			var factory = Factory;

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2019/00002345";

			var messageText = TRMessageTestHelper.GetFileText("OzbyMuayeneMemuruAdiSorgulaResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.");

			var successMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRM, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			successMessage.EM_ApplicationReference = "ULU-2019/00002345|12345678901";

			Processor.ProcessMessage(successMessage);

			var messageInterpretation = successMessage.EM_MessageInterpretation;

			CombineAssertions(() =>
			{
				AssertEquals("RegistrationStatus should be empty", "", header.RegistrationStatus);
				AssertEquals("AMA_MessageStatus should be empty", "", header.AMA_MessageStatus);
				AssertEquals("Inspection Clerk", "BÜLENT ALİ GÖZÜKÜÇÜK", header.AMA_InspectionClerk);
				Assert("EM_MessageInterpretation should contains 'received successfully.'", messageInterpretation.Contains("Global Manifest Message Type TRM received successfully."));
				Assert("EM_MessageInterpretation should contains 'Inspection Clerk'", messageInterpretation.Contains("<td>Inspection Clerk:</td><td>B&#220;LENT ALİ G&#214;Z&#220;K&#220;&#199;&#220;K</td>"));
			});
		}

		public void TestProcessTRMResponseMessageWithoutInspectionClerk()
		{
			var factory = Factory;

			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2024/00004568";

			var messageText = TRMessageTestHelper.GetFileText("OzbyMuayeneMemuruAdiSorgulaResponseEmpty.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.");

			var successMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRM, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			successMessage.EM_ApplicationReference = "ULU-2019/00002345|12345678901";

			Processor.ProcessMessage(successMessage);

			var messageInterpretation = successMessage.EM_MessageInterpretation;

			CombineAssertions(() =>
			{
				AssertEquals("RegistrationStatus should be empty", "", header.RegistrationStatus);
				AssertEquals("AMA_MessageStatus should be empty", "", header.AMA_MessageStatus);
				AssertEquals("Inspection Clerk", ZString.Empty, header.AMA_InspectionClerk);
				Assert("EM_MessageInterpretation should contains 'received successfully.'", messageInterpretation.Contains("Global Manifest Message Type TRM received successfully."));
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

			var messageText = TRMessageTestHelper.GetFileText("OzbyMuayeneMemuruAdiSorgulaResponse.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.");
			var inspectionClerk = "BÜLENT ALİ GÖZÜKÜÇÜK";
			var noInspectionClerk = string.Empty;
			var inspectionClerkExistsMessage = "<td>Inspection Clerk:</td><td>B&#220;LENT ALİ G&#214;Z&#220;K&#220;&#199;&#220;K</td>";
			var inspectionClerkNotExistsMessage = "There is no Inspection Clerk assigned.";

			var message = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRM, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			message.EM_MessageNum = "99";

			var setupMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.TRM, EDIMessage.Direction.Receive, EDIInterchange.Status.Sent, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			setupMessage.EM_MessageNum = "99";
			setupMessage.EM_SystemCreateUser = staff1.GS_Code;

			var sendemailStaffMember = "ESM";
			SetupRegistryData(sendemailStaffMember, group.PK, false);
			ProcessAndAssertMessageProcessWithExpectedEmail(header, message, staff1, inspectionClerk, inspectionClerkExistsMessage);
			SetupRegistryData(sendemailStaffMember, group.PK, true);
			AssertNullMail(header, message);

			var sendemailNominatedGroup = "ENG";
			SetupRegistryData(sendemailNominatedGroup, group.PK, false);
			ProcessAndAssertMessageProcessWithExpectedEmail(header, message, staff1, inspectionClerk, inspectionClerkExistsMessage);
			SetupRegistryData(sendemailNominatedGroup, group.PK, true);
			AssertNullMail(header, message);

			var sendEmailStaffMemberandNominatedGroup = "ESG";
			SetupRegistryData(sendEmailStaffMemberandNominatedGroup, group.PK, false);
			ProcessAndAssertMessageProcessWithExpectedEmail(header, message, staff1, inspectionClerk, inspectionClerkExistsMessage);
			SetupRegistryData(sendEmailStaffMemberandNominatedGroup, group.PK, true);
			AssertNullMail(header, message);

			var sendEmailStaffMemberOrNominatedGroupForGroup = "EOG";
			SetupRegistryData(sendEmailStaffMemberOrNominatedGroupForGroup, group.PK, false);
			ProcessAndAssertMessageProcessWithExpectedEmail(header, message, staff1, inspectionClerk, inspectionClerkExistsMessage);
			SetupRegistryData(sendEmailStaffMemberOrNominatedGroupForGroup, group.PK, true);
			AssertNullMail(header, message);

			var sendNoEmail = "NOE";
			SetupRegistryData(sendNoEmail, group.PK, false);
			AssertNullMail(header, message);
			SetupRegistryData(sendNoEmail, group.PK, true);
			AssertNullMail(header, message);

			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "ULU-2019/00002346";

			messageText = TRMessageTestHelper.GetFileText("OzbyMuayeneMemuruAdiSorgulaResponseEmpty.xml", "Enterprise.Customs.TR.Messaging.Testing.TestFiles.Manifest.");
			message = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T1O, EDIMessage.Direction.Receive, EDIInterchange.Status.Queued, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			message.EM_MessageNum = "88";

			setupMessage = MessageTestHelper.CreateMessage(factory, TRMessageTypes.Codes.T1O, EDIMessage.Direction.Receive, EDIInterchange.Status.Sent, AsycudaManifestHeaderSchema.Constants.TableName, header.PK, messageText, ZGuid.Empty);
			setupMessage.EM_MessageNum = "88";
			setupMessage.EM_SystemCreateUser = staff1.GS_Code;

			SetupRegistryData(sendemailStaffMember, group.PK, false);
			ProcessAndAssertMessageProcessWithExpectedEmail(header, message, staff1, noInspectionClerk, inspectionClerkNotExistsMessage);

			SetupRegistryData(sendemailNominatedGroup, group.PK, false);
			ProcessAndAssertMessageProcessWithExpectedEmail(header, message, staff1, noInspectionClerk, inspectionClerkNotExistsMessage);

			SetupRegistryData(sendEmailStaffMemberandNominatedGroup, group.PK, false);
			ProcessAndAssertMessageProcessWithExpectedEmail(header, message, staff1, noInspectionClerk, inspectionClerkNotExistsMessage);

			SetupRegistryData(sendEmailStaffMemberOrNominatedGroupForGroup, group.PK, false);
			ProcessAndAssertMessageProcessWithExpectedEmail(header, message, staff1, noInspectionClerk, inspectionClerkNotExistsMessage);

			SetupRegistryData(sendNoEmail, group.PK, false);
			AssertNullMail(header, message);
			SetupRegistryData(sendNoEmail, group.PK, true);
			AssertNullMail(header, message);
		}

		void SetupRegistryData(ZString sendMode, ZGuid sendGroupPK, ZBool sendErrorOnly)
		{
			TRCustomsDataRegistry.Instance.TRMANGroupNotification.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Registry.Business.Customs.ManifestGroupNotification(sendMode, sendGroupPK, sendErrorOnly));
		}

		void ProcessAndAssertMessageProcessWithExpectedEmail(AsycudaManifestHeader header, EDIMessage message, GlbStaff staff1, ZString inspectionClerk, string expectedText)
		{
			Processor.ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Find(x => x.Subject == "Manifest Message Response for " + header.AMA_JobReference);
			var bodyText = email.Body;
			const string imagePath = "Enterprise.Customs.TR.Business.Message.EDIMessage.HtmlTemplates.Success.jpg";
			var imageBase64 = GetImageValue(imagePath);

			CombineAssertions(() =>
			{
				AssertNotNull(email);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("The original sender should be notify", staff1.GS_EmailAddress, email.Recipients[0].Email);
				Assert("Contains received successfully.", bodyText.Contains("Global Manifest Message Type TRM received successfully."));
				Assert("Contains expected text", bodyText.Contains(expectedText));
				AssertEquals("Inspection Clerk", inspectionClerk, header.AMA_InspectionClerk);
				Assert("Contains result image", email.Body.Contains(imageBase64));
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

		static string GetImageValue(string imagePath)
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

		protected override ManifestMessageProcessorBase Processor => new TRMMessageProcessor(logger);
	}
}
