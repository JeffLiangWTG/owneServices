using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Workflow.MessageDelivery.Testing
{
	sealed class EmailNotifierTest : TestCaseWithFactory
	{
		public void TestNotifyWithEnvrionmentStackTrace()
		{
			var exception = new InvalidOperationException("I am an InvalidOperationException for test.");
			var environmentStackTrace = System.Environment.StackTrace;
			var mode = GetModeSetupForFileOutput();

			new EmailNotifier().Notify(Factory, exception, environmentStackTrace, mode, null, null);

			CombineAssertions(() =>
			{
				AssertEquals("An email should be created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Workflow process delivery failed", email.Subject);
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("mickey.mouse@cargowise.com", email.Recipients[0].Email);

				var emailBody = email.Body;
				AssertContains("I am an InvalidOperationException for test.", emailBody);
				AssertContains("Environment Stack Trace:", emailBody);
				AssertContains("at Enterprise.MasterFiles.Business.Workflow.MessageDelivery.Testing.EmailNotifierTest.TestNotifyWithEnvrionmentStackTrace() in", emailBody);
				AssertContains("File name\t\t\t: " + mode.EK_Filename, emailBody);
				AssertContains("Destination\t\t: " + mode.EK_Destination, emailBody);
				AssertContains("Organization		:  <a href=\"", emailBody);
				AssertContains(mode.Organisation.OH_Code + " - " + mode.Organisation.OH_FullName, emailBody);
			});
		}

		public void TestSendWithNoAttachmentIfStreamCouldNotOpen()
		{
			EDICommunicationsMode mode = GetModeSetupForFileOutput();
			var messageContentStream = new MemoryStream();
			messageContentStream.Dispose();

			AssertNoExceptionThrown("Should be able to handle disposed stream", delegate
			{ new EmailNotifier().Notify(Factory, String.Empty, mode, messageContentStream, null); });

			AssertEquals("Precondition: Email should be created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("No attachment should be created", 0, email.Attachments.Count);
		}

		public void TestSentWithOneAttachment()
		{
			EDICommunicationsMode mode = GetModeSetupForFileOutput();
			var messageContent = System.Text.Encoding.UTF8.GetBytes("Sent Raw Data Content");
			using (var messageContentStream = new MemoryStream(messageContent))
			{
				new EmailNotifier().Notify(Factory, String.Empty, mode, messageContentStream, null);
			}

			AssertEquals("Precondition: Email should be created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(1, email.Attachments.Count);
			var attachment = email.Attachments[0];
			AssertEquals("RawMessageContent.zip", attachment.DisplayName);
		}

		public void TestSendWithBothAttachments()
		{
			EDICommunicationsMode mode = GetModeSetupForFileOutput();
			var messageContent = System.Text.Encoding.UTF8.GetBytes("Raw Message Content");
			var responseContent = System.Text.Encoding.UTF8.GetBytes("Raw Response Content");
			using (var messageContentStream = new MemoryStream(messageContent))
			using (var responseContentStream = new MemoryStream(responseContent))
			{
				new EmailNotifier().Notify(Factory, String.Empty, mode, messageContentStream, responseContentStream);
			}

			AssertEquals("Precondition: Email should be created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(2, email.Attachments.Count);
			var attachment1 = email.Attachments[0];
			AssertEquals("attachment1.DisplayName", "RawMessageContent.zip", attachment1.DisplayName);
			var attachment2 = email.Attachments[1];
			AssertEquals("attachment2.DisplayName", "RawResponseContent.zip", attachment2.DisplayName);
		}

		public void TestSendEmail()
		{
			EDICommunicationsMode mode = GetModeSetupForFileOutput();

			new EmailNotifier().Notify(Factory, String.Empty, mode, null, null);

			AssertEquals("It should create an email for edi delivery fail", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Workflow process delivery failed", email.Subject);
			AssertContains("Wrong email body", "File name\t\t\t: " + mode.EK_Filename, email.Body);
			AssertContains("Wrong email body", "Destination\t\t: " + mode.EK_Destination, email.Body);
			AssertContains("Wrong email body", "Organization		:  <a href=\"", email.Body);
			AssertContains("Wrong email body", mode.Organisation.OH_Code + " - " + mode.Organisation.OH_FullName, email.Body);
			AssertEquals("Wrong recipients", 1, email.Recipients.Count);
			AssertEquals("Wrong recipients", "mickey.mouse@cargowise.com", email.Recipients[0].Email);
		}

		public void TestExceptionContainsStacktraceOnlyWhenRequired()
		{
			var exception = GetInitialException();
			exception = GetNewExceptionWithInner(exception);

			var notifier = new EmailNotifierForTesting();
			AssertContains("GetNewExceptionWithInner", exception.StackTrace);
			AssertContains("GetInitialException", exception.InnerException.StackTrace);

			string message = notifier.BuildHTMLExceptionMessage_Exposed(exception, true);
			AssertNotContains("GetNewExceptionWithInner", message);
			AssertNotContains("GetInitialException", message);
			AssertContains("I am an exception", message);
			AssertContains("I am not an exception", message);

			message = notifier.BuildHTMLExceptionMessage_Exposed(exception, false);
			AssertContains("GetNewExceptionWithInner", message);
			AssertContains("GetInitialException", message);
			AssertContains("I am an exception", message);
			AssertContains("I am not an exception", message);
		}

		Exception GetInitialException()
		{
			var exception = new Exception("I am an exception");
			try
			{ throw exception; }
			catch { /* om nom */ }

			return exception;
		}

		Exception GetNewExceptionWithInner(Exception innerException)
		{
			var newException = new Exception("I am not an exception", innerException);
			try
			{ throw newException; }
			catch { /* om nom */ }

			return newException;
		}

		EDICommunicationsMode GetModeSetupForFileOutput()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "OHD";
			orgHeader.OH_FullName = "FullName";

			TempDirectory tempDir = new TempDirectory();
			EDICommunicationsMode mode = GetFileMode(tempDir, "");
			TempDirectory.DeleteDirectory(new DirectoryInfo(tempDir.DirectoryName), false);
			orgHeader.EDICommunicationsModes.Add(mode);

			GlbGroup group = SetupEmail();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			NotificationDataRegistry.Instance.EDIMessageDeliveryFailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			Factory.Save();
			mode.EK_LastFailed = ZDateTime.Now.AddHours(-3);
			System.Threading.Thread.Sleep(100);
			return mode;
		}

		GlbGroup SetupEmail()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = group.Staff.AddNew();
			staff.GS_EmailAddress = "mickey.mouse@cargowise.com";
			staff.GS_Code = "ZAC";
			Enterprise.Registry.Business.NotificationDataRegistry.Instance.EDIMessageDeliveryFailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			Factory.Save();
			return group;
		}

		EDICommunicationsMode GetFileMode(TempDirectory tempDir, string replaceString)
		{
			EDICommunicationsMode mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.SaveToFile;
			mode.EK_Destination = tempDir.DirectoryName;
			mode.EK_Filename = "SaveAsFileMode" + replaceString + ".txt";
			return mode;
		}

		sealed class EmailNotifierForTesting : EmailNotifier
		{
			internal string BuildHTMLExceptionMessage_Exposed(Exception exception, bool shouldSuppressCallStack)
			{
				return BuildHTMLExceptionMessage(exception, shouldSuppressCallStack);
			}
		}
	}
}
