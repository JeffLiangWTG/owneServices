using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class EmailSenderTest : TestCaseWithFactory
	{
		public void TestSendEmail_CheckIsReadyToSendEmail()
		{
			ISendEmailSource sup = Factory.New<OrgHeader>();
			var selection = new EmailSenderConfigurationForTest(sup);
			AssertEquals("Precondition", false, selection.IsCheckedToSendEmail);

			new EmailSenderForTest().SendEmail_Exposed(selection);
			AssertEquals(true, selection.IsCheckedToSendEmail);
		}

		public void TestGetSendEmailMenuItem()
		{
			var menuItem = EmailSender.GetSendEmailMenuItem();
			AssertEquals("Text", "&Send E-mail", menuItem.Text);
			AssertEquals("Shortcut", Shortcut.CtrlShiftE, menuItem.Shortcut);
			AssertEquals("ShowShortcut", true, menuItem.ShowShortcut);
		}

		public void TestHandleEventHandler()
		{
			var businessObject = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZForm(businessObject))
			{
				var menuItem1 = new MenuItem();
				menuItem1.Click += new EventHandler((o, e) => EmailSender.HandleEventHandler(form, null, null));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				menuItem1.PerformClick();
				AssertEquals("LastMessage for unselected record", "Please select a record before an E-mail can be sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				menuItem1.Dispose();

				var menuItem2 = new MenuItem();
				menuItem2.Click += new EventHandler((o, e) => EmailSender.HandleEventHandler(form, null, (NoResString)"Custom Message"));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				menuItem2.PerformClick();
				AssertEquals("LastMessage for unselected record", "Custom Message", UnitTestUserNotification.Instance.LastMessage.Text);
				menuItem2.Dispose();

				Factory.Save();
				businessObject.HasChanges = true;

				var menuItem3 = new MenuItem();
				menuItem3.Click += new EventHandler((o, e) => EmailSender.HandleEventHandler(form, businessObject, null));
				AssertEquals("Precondition: IsInDatabase", true, businessObject.IsInDatabase);
				AssertEquals("Precondition: HasChanges", true, businessObject.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				menuItem3.PerformClick();
				AssertEquals("LastMessage for unsaved record", "Changes have been made to this record. Please save before an E-mail can be sent.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				AssertEquals("Precondition: IsInDatabase", true, businessObject.IsInDatabase);
				AssertEquals("Precondition: HasChanges", false, businessObject.HasChanges);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				menuItem3.PerformClick();
				var lastFormShown = ZFormModaliser.LastFormShownDialogForTest as EmailSenderForm;
				AssertNotNull("Form shown on click", lastFormShown);
				menuItem3.Dispose();
			}
		}

		public void TestPerformClick_EmailSenderFormMarkOwnerForm()
		{
			var businessObject = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZForm(businessObject))
			{
				var menuItem3 = new MenuItem();
				menuItem3.Click += new EventHandler((o, e) => EmailSender.HandleEventHandler(form, businessObject, null));
				Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				menuItem3.PerformClick();

				AssertEquals("ZForm", (ZFormModaliser.LastFormShownDialogForTest as EmailSenderForm).OwnerName_ForTest);
				menuItem3.Dispose();
			}
		}

		public void TestAddEDoc_FileNotExists()
		{
			ISendEmailSource source = Factory.NewWithValidTestData<OrgHeader>();

			var tempFileName = "JerryTestAttachment.tmp";
			var tempFilePath = Path.Combine(Temp.TempPath, tempFileName);
			byte[] tempFileContents = new byte[1024];
			using (var fileStream = File.OpenWrite(tempFilePath))
			{
				fileStream.Write(tempFileContents, 0, 1024);
			}

			try
			{
				var senderConfig = new EmailSenderConfigurationForTest(source);
				senderConfig.HtmlEmail.FromEmailAddress = "Jerry@form.com";
				senderConfig.HtmlEmail.ToEmailAddress = "Jerry@to.com";
				senderConfig.HtmlEmail.Subject = "Email subject";
				senderConfig.HtmlEmail.Body = "Email body";
				senderConfig.HtmlEmail.AddAttachment(tempFilePath);
				senderConfig.SaveToEDocsDocumentType = "MSC";

				var sender = new EmailSenderForTest(source);
				sender.DeleteAttachmentFileForTest = true;

				AssertNoExceptionThrown(() => sender.SendEmail_Exposed(senderConfig));
				AssertEquals("Should have 2 files", 2, source.DocManagerInfo.Files.Count);
			}
			finally
			{
				File.Delete(tempFilePath);
			}
		}

		public void TestAddEDoc_LongSubject()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			ISendEmailSource source = org;

			string tempFileName = "Attachment.tmp";
			string tempFilePath = Temp.TempPath + tempFileName;
			byte[] tempFileContents = new byte[1024];
			using (FileStream fileStream = File.OpenWrite(tempFilePath))
			{
				fileStream.Write(tempFileContents, 0, 1024);
			}

			try
			{
				var senderConfig = new EmailSenderConfigurationForTest(source);
				senderConfig.HtmlEmail.FromEmailAddress = "jenny.nguyen@cargowise.com";
				senderConfig.HtmlEmail.ToEmailAddress = "test@cargowise.com";
				//build a 256 character string (limit of Subject)
				var a = "0123456789abcdef";
				for (var i = 0; i < 4; ++i)
				{
					a = a + a;
				}
				senderConfig.HtmlEmail.Subject = a;
				senderConfig.HtmlEmail.Body = "Email body";
				senderConfig.HtmlEmail.AddAttachment(tempFilePath);
				senderConfig.SaveToEDocsDocumentType = "MSC";

				new EmailSenderForTest(source).SendEmail_Exposed(senderConfig);
				var parserForExpectedBody = new HtmlEmailDef();
				parserForExpectedBody.Subject = a;
				parserForExpectedBody.LoadHtmlUsingTemplate("Email body");

				AssertEquals("Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("email.Recipients", 1, email.Recipients.Count);
				AssertEquals("test@cargowise.com", email.Recipients[0].Email);
				AssertEquals(a, email.Subject);
				AssertEquals(parserForExpectedBody.Body, email.Body);
				AssertAddedEmailSentEvents(new BusinessObjectFactory().Load<OrgHeader>(org.PK), senderConfig);

				AssertEquals(2, source.DocManagerInfo.Files.Count);
				AssertEquals("Doc type of email", "MSC", source.DocManagerInfo.Files[0].DocType);
				AssertEquals(a.Substring(128), source.DocManagerInfo.Files[0].Description);

				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
			finally
			{
				File.Delete(tempFilePath);
			}
		}

		public void TestAddEDoc()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			ISendEmailSource source = org;

			string tempFileName = "Attachment.tmp";
			string tempFilePath = Temp.TempPath + tempFileName;
			byte[] tempFileContents = new byte[1024];
			using (FileStream fileStream = File.OpenWrite(tempFilePath))
			{
				fileStream.Write(tempFileContents, 0, 1024);
			}

			try
			{
				var senderConfig = new EmailSenderConfigurationForTest(source);
				senderConfig.HtmlEmail.FromEmailAddress = "jenny.nguyen@cargowise.com";
				senderConfig.HtmlEmail.ToEmailAddress = "test@cargowise.com";
				senderConfig.HtmlEmail.Cc = "test+cc@cargowise.com";
				senderConfig.HtmlEmail.Bcc = "test+bcc@cargowise.com";
				senderConfig.HtmlEmail.Subject = "email subject";
				senderConfig.HtmlEmail.Body = "This is the email body with UTF8 encoding characters 测试.";
				senderConfig.HtmlEmail.AddAttachment(tempFilePath);
				senderConfig.SaveToEDocsDocumentType = "MSC";

				new EmailSenderForTest(source).SendEmail_Exposed(senderConfig);
				var parserForExpectedBody = new HtmlEmailDef();
				parserForExpectedBody.Subject = "email subject";
				parserForExpectedBody.LoadHtmlUsingTemplate("This is the email body with UTF8 encoding characters 测试.");

				AssertEquals("Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("email.Recipients", 1, email.Recipients.Count);
				AssertEquals("test@cargowise.com", email.Recipients[0].Email);
				AssertEquals("email subject", email.Subject);
				AssertEquals(parserForExpectedBody.Body, email.Body);
				AssertAddedEmailSentEvents(new BusinessObjectFactory().Load<OrgHeader>(org.PK), senderConfig);

				AssertEquals(2, source.DocManagerInfo.Files.Count);
				AssertEquals("Doc type of email", "MSC", source.DocManagerInfo.Files[0].DocType);
				AssertEquals("email subject", source.DocManagerInfo.Files[0].Description);
				AssertEquals("email subject.html", source.DocManagerInfo.Files[0].FileName);
				AssertContains("<p><strong>Subject</strong>: email subject</p><p><strong>To</strong>: test@cargowise.com</p><p><strong>Cc</strong>: test+cc@cargowise.com</p><p><strong>Bcc</strong>: test+bcc@cargowise.com</p>",
					source.DocManagerInfo.Files[0].ImageData.ToAscii());
				AssertContains("This is the email body with UTF8 encoding characters 测试.", source.DocManagerInfo.Files[0].ImageData.ToUTF8());
				AssertEquals("Doc type of attachment", "MSC", source.DocManagerInfo.Files[1].DocType);
				AssertEquals("", source.DocManagerInfo.Files[1].Description);
				AssertEquals(tempFileName, source.DocManagerInfo.Files[1].FileName);
				AssertEquals(tempFileContents, source.DocManagerInfo.Files[1].ImageData);
				AssertAddedDocumentAllocatedEvents(source, source.DocManagerInfo.Files[0].DocType);
			}
			finally
			{
				File.Delete(tempFilePath);
			}
		}

		public void TestAddEDoc_NonCustomizableDocType()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			ISendEmailSource source = org;

			string tempFileName = "Attachment.tmp";
			string tempFilePath = Temp.TempPath + tempFileName;
			byte[] tempFileContents = new byte[1024];
			using (FileStream fileStream = File.OpenWrite(tempFilePath))
			{
				fileStream.Write(tempFileContents, 0, 1024);
			}

			try
			{
				var senderConfig = new EmailSenderConfigurationForTest(source);
				senderConfig.HtmlEmail.FromEmailAddress = "jenny.nguyen@cargowise.com";
				senderConfig.HtmlEmail.ToEmailAddress = "test@cargowise.com";
				senderConfig.HtmlEmail.Cc = "test+cc@cargowise.com";
				senderConfig.HtmlEmail.Bcc = "test+bcc@cargowise.com";
				senderConfig.HtmlEmail.Subject = "email subject";
				senderConfig.HtmlEmail.Body = "This is the email body.";
				senderConfig.HtmlEmail.AddAttachment(tempFilePath);
				senderConfig.SaveToEDocsDocumentType = "EXD";

				new EmailSenderForTest(source).SendEmail_Exposed(senderConfig);
				var parserForExpectedBody = new HtmlEmailDef();
				parserForExpectedBody.Subject = "email subject";
				parserForExpectedBody.LoadHtmlUsingTemplate("This is the email body.");

				AssertEquals("Email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("email.Recipients", 1, email.Recipients.Count);
				AssertEquals("test@cargowise.com", email.Recipients[0].Email);
				AssertEquals("email subject", email.Subject);
				AssertEquals(parserForExpectedBody.Body, email.Body);
				AssertAddedEmailSentEvents(new BusinessObjectFactory().Load<OrgHeader>(org.PK), senderConfig);

				AssertEquals(2, source.DocManagerInfo.Files.Count);
				AssertEquals("Doc type", "EXD", source.DocManagerInfo.Files[0].DocType);
				AssertEquals("Exporter Documents", source.DocManagerInfo.Files[0].Description);
				AssertEquals("email subject.html", source.DocManagerInfo.Files[0].FileName);

				string imageAsAscii;

				using (var streamReader = new StreamReader(source.DocManagerInfo.Files[0].GetImageDataReader(), Encoding.ASCII))
				{
					imageAsAscii = streamReader.ReadToEnd();
				}

				AssertContains("<p><strong>Subject</strong>: email subject</p><p><strong>To</strong>: test@cargowise.com</p><p><strong>Cc</strong>: test+cc@cargowise.com</p><p><strong>Bcc</strong>: test+bcc@cargowise.com</p>", imageAsAscii);
				AssertContains("This is the email body.", imageAsAscii);
				AssertEquals("Exporter Documents", source.DocManagerInfo.Files[1].Description);
				AssertEquals(tempFileName, source.DocManagerInfo.Files[1].FileName);
				AssertEquals(tempFileContents, source.DocManagerInfo.Files[1].ImageData);
				AssertAddedDocumentAllocatedEvents(source, source.DocManagerInfo.Files[0].DocType);
			}
			finally
			{
				File.Delete(tempFilePath);
			}
		}

		void AssertAddedEmailSentEvents(ISendEmailSource source, EmailSenderConfiguration senderConfig)
		{
			StmALog log = source.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EmailSent.Code))[0];
			AssertEquals(senderConfig.HtmlEmail.AllRecipientsCommaDelimited, log.SL_Reference);
		}

		void AssertAddedDocumentAllocatedEvents(ISendEmailSource source, string documentType)
		{
			StmALog log = source.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DocumentAllocated.Code))[0];
			AssertContains("Correct Reference", documentType, log.SL_Reference);
		}

		[UseSnapshotProtection]
		public void TestAddEventAddingLogsInNewFactory()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			using (SetTemporaryDbConnection(connection))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				var factory = new BusinessObjectFactory(connection);
				var org = factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "TESTORG";
				var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
				factory.Save();

				EmailSender.IsTestEmailSender = true;
				EmailSender.IsTestSendFileToEdoc = false;
				using (var form = new ZForm(org))
				{
					var menuItem = new MenuItem();
					menuItem.Click += new EventHandler((o, e) => EmailSender.HandleEventHandler(form, org, null));

					dummy.Z0_Code = "ERR";

					var sqlText = $"UPDATE dbo.DummyBizo SET Z0_Code = 'CHG' WHERE Z0_PK = '{dummy.PK}'";
					connection.ExecuteNonQuery(sqlText);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					menuItem.PerformClick();
					Assert(UnitTestUserNotification.Instance.LastMessage.Contains("Please review the form carefully before clicking the 'Save' button again."));
				}

				var orgTestSendToEDoc = factory.NewWithValidTestData<OrgHeader>();
				orgTestSendToEDoc.OH_Code = "TESTOH";
				var dummyTestSendToEDoc = factory.NewWithValidTestData<DummyBusinessObject>();
				factory.Save();
				EmailSender.IsTestSendFileToEdoc = true;
				using (var form = new ZForm(org))
				{
					var menuItem = new MenuItem();
					menuItem.Click += new EventHandler((o, e) => EmailSender.HandleEventHandler(form, orgTestSendToEDoc, null));

					dummyTestSendToEDoc.Z0_Code = "TST";

					var sqlText = $"UPDATE dbo.DummyBizo SET Z0_Code = 'CH0' WHERE Z0_PK = '{dummyTestSendToEDoc.PK}'";
					connection.ExecuteNonQuery(sqlText);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					AssertNoExceptionThrown(menuItem.PerformClick);
				}
			}
		}
		IDisposable SetTemporaryDbConnection(DbConnection connection)
		{
			Db.ConnectionOverrideForTest = connection;
			return new DisposableAction(() =>
			{
				Db.ConnectionOverrideForTest = null;
			});
		}
	}
}
