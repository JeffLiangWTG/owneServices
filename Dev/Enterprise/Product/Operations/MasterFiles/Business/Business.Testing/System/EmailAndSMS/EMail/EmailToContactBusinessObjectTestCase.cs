using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(EmailToContactBusinessObject), ExcludePrivate = true)]
	public abstract class EmailToContactBusinessObjectTestCase<T> : EmailWithAttachmentTestCase<T> where T : EmailToContactBusinessObject
	{
		public void TestFactory()
		{
			AssertEquals("Factory", Factory, EmailContactObject.Factory);
		}

		public void TestBusinessObjectSendingEmail()
		{
			AssertEquals("BusinessObjectSendingEmail", BusinessObjectSendingEmail, EmailContactObject.BusinessObjectSendingEmail);
		}

		public virtual void TestSaveAsNote()
		{
			AssertEquals("SaveAsNote", true, EmailContactObject.SaveAsNote);
		}

		public void TestPriority()
		{
			AssertEquals("Priority", "MED", EmailContactObject.Priority);
		}

		public void TestShouldAddNoteAndEvent()
		{
			AssertEquals("Should add note and event", true, EmailContactObject.ShouldAddNoteAndEvent);
		}

		#region Bound Properties

		protected virtual void AssertNoEmailSent()
		{
			AssertEquals("No emails should have been sent.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		protected virtual void AssertEmailSent()
		{
			AssertEquals("Email should have been sent.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		protected virtual void AssertEmailPriority(string priority)
		{
			AssertEmailSent();
			AssertNotNull("EmailSaved is not null", Env.OutgoingMailManager.EmailsCreated[0]);

			if (priority == "HI")
			{
				AssertEquals("Email's priority should be High.", EmailDef.PriorityFlag.High, Env.OutgoingMailManager.EmailsCreated[0].Priority);
			}
			else if (priority == "LOW")
			{
				AssertEquals("Email's priority should be Low.", EmailDef.PriorityFlag.Low, Env.OutgoingMailManager.EmailsCreated[0].Priority);
			}
			else
			{
				AssertEquals("Email's priority should be Medium.", EmailDef.PriorityFlag.Medium, Env.OutgoingMailManager.EmailsCreated[0].Priority);
			}

			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestPriorityOnEmail()
		{
			BusinessObjectSendingEmail.FillWithValidTestData();
			PopulateEmailContactObject(EmailContactObject);

			EmailContactObject.Priority = "HI";
			EmailContactObject.SendEmail();
			AssertEmailPriority("HI");

			EmailContactObject.Priority = "MED";
			EmailContactObject.SendEmail();
			AssertEmailPriority("MED");

			EmailContactObject.Priority = "LOW";
			EmailContactObject.SendEmail();
			AssertEmailPriority("LOW");
		}

		#endregion

		#region Send Email

		public void TestSendEmail()
		{
			EmailContactObject.SaveAsNote = true;
			TestSendEmail(EmailContactObject, true);
		}

		public void TestSendEmailWithNoNote()
		{
			EmailContactObject.SaveAsNote = false;
			TestSendEmail(EmailContactObject, false);
		}

		public virtual void TestSendEmailSaveStrategies()
		{
			EmailContactObject.FromEmailAddress = "testsender@cargowise.com";
			EmailContactObject.Subject = "test email";
			EmailContactObject.ToEmailAddress = "testuser@cargowise.com";
			EmailContactObject.ShouldSaveBizOFactoryOnSent = false;
			EmailContactObject.ShouldSaveEmailInNewFactory = false;
			AssertEquals("Precondition", 0, FindMailDbItemsInFactory().Length);

			EmailContactObject.SendEmail();
			BusinessObject[] emails = FindMailDbItemsInFactory();
			AssertEquals(1, emails.Length);
			AssertEquals("test email", emails[0][MailDBItemsSchema.MI_Subject]);
			Assert("Should not be saved on sent", !emails[0].IsInDatabase);

			EmailContactObject.Subject = "test email no. 2";
			EmailContactObject.ShouldSaveBizOFactoryOnSent = true;
			EmailContactObject.SendEmail();
			emails = FindMailDbItemsInFactory();
			AssertEquals(2, emails.Length);
			Array.Sort(emails, (BusinessObject left, BusinessObject right) =>
			{
				return left[MailDBItemsSchema.MI_Subject].ToString().CompareTo(right[MailDBItemsSchema.MI_Subject].ToString());
			});
			Assert("Should now be saved on sent", emails[0].IsInDatabase);
			AssertEquals("test email", emails[0][MailDBItemsSchema.MI_Subject]);
			Assert("Should now be saved on sent", emails[1].IsInDatabase);
			AssertEquals("test email no. 2", emails[1][MailDBItemsSchema.MI_Subject]);

			EmailContactObject.Subject = "test email no. 3";
			EmailContactObject.ShouldSaveEmailInNewFactory = true;
			emails = FindMailDbItemsInFactory();
			AssertEquals("Should not add the new MailItem into this factory", 2, emails.Length);
		}

		BusinessObject[] FindMailDbItemsInFactory()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			foreach (BusinessObject bizO in ((IBusinessObjectFactoryInternals)Factory).AllBusinessObjects)
			{
				if (bizO.TableName == MailDBItemsSchema.Constants.TableName)
				{
					result.Add(bizO);
				}
			}

			return result.ToArray();
		}

		protected virtual void TestSendEmail(T emailContactObject, bool hasNote)
		{
			BusinessObjectSendingEmail.FillWithValidTestData();
			Factory.Save();

			using (TempFile tempFile1 = TempFile.New())
			using (TempFile tempFile2 = TempFile.New())
			{
				using (FileStream stream = File.OpenWrite(tempFile1.Filename))
				{
					stream.Write(new byte[] { 1, 2, 3 }, 0, 3);
				}
				using (FileStream stream = File.OpenWrite(tempFile2.Filename))
				{
					stream.Write(new byte[] { 4, 5, 6 }, 0, 3);
				}

				PopulateEmailContactObject(emailContactObject);
				emailContactObject.AddAttachment(tempFile1.Filename, tempFile2.Filename);

				AssertNoEmailSent();

				emailContactObject.SendEmail();
				AssertSentEmail();
				AssertAttachment(0, Path.GetFileName(tempFile1.Filename), new byte[] { 1, 2, 3 });
				AssertAttachment(1, Path.GetFileName(tempFile2.Filename), new byte[] { 4, 5, 6 });

				StmNote addedNote = null;
				foreach (StmNote note in BusinessObjectSendingEmail.GetNotes().GetAllNotes())
				{
					if (note.ST_Description == "Email Sent")
					{
						AssertNull("Only 1 note should be added.", addedNote);
						addedNote = note;
					}
				}
				if (hasNote)
				{
					AssertNotNull("A note should be added.", addedNote);
					AssertEquals("addedNote.ST_NoteType", StmNoteDescription.Pub, addedNote.ST_NoteType);
					AssertEquals("addedNote.ST_Description", "Email Sent", addedNote.ST_Description);
					AssertContains("addedNote.ST_NoteDataAsText", "Sent To: 1@1.com, 2@2.com, 3@3.com, 4@4.com", addedNote.ST_NoteDataAsText);
					AssertContains("addedNote.ST_NoteDataAsText", "This is the body.", addedNote.ST_NoteDataAsText);
					AssertContains("addedNote.ST_NoteDataAsText", "Attachment1: " + Path.GetFileName(tempFile1.Filename) + "\r\nAttachment2: " + Path.GetFileName(tempFile2.Filename), addedNote.ST_NoteDataAsText);
				}
				else
				{
					AssertNull("No note should be added.", addedNote);
				}

				AssertAddedEvents();
			}
		}

		protected virtual void AssertSentEmail()
		{
			AssertEmailSent();
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("email.FromDisplayName", "From Me", email.FromDisplayName);
			AssertEquals("email.FromAddress", "From@Me.com", email.FromAddress);
			AssertEquals("email.Recipients.Count", 2, email.Recipients.Count);
			AssertEquals("email.CCRecipients.Count", 2, email.CCRecipients.Count);
			AssertEquals("email.Recipients[0]", "1@1.com", email.Recipients[0]);
			AssertEquals("email.Recipients[1]", "2@2.com", email.Recipients[1]);
			AssertEquals("email.CCRecipients[0]", "3@3.com", email.CCRecipients[0]);
			AssertEquals("email.CCRecipients[1]", "4@4.com", email.CCRecipients[1]);
			AssertEquals("email.Subject", "This is the subject.", email.Subject);
			AssertEquals("email.Body", "This is the body.", email.Body);
			AssertEquals("email.Priority", EmailDef.PriorityFlag.Medium, email.Priority);
		}

		protected virtual void AssertAttachment(int index, string fileName, byte[] content)
		{
			AssertEmailSent();
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			string identifier = "email.Attachments[" + index + "]";
			AssertEquals(identifier + ".DisplayName", fileName, email.Attachments[index].DisplayName);
			AssertEquals(identifier + ".Data", content, email.Attachments[index].Data);
		}

		protected virtual void AssertAddedEvents()
		{
			StmALog log = BusinessObjectSendingEmail.GetLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EmailSent.Code))[0];
			AssertEquals("log.SL_Reference", EmailContactObject.AllRecipientsCommaDelimited, log.SL_Reference);
		}

		#endregion

		public virtual void TestSaveStrategyProperties()
		{
			Assert("Default value", EmailContactObject.ShouldSaveEmailInNewFactory);
			Assert("Default value", EmailContactObject.ShouldSaveBizOFactoryOnSent);
		}

		public virtual void TestOverwriteEmailFromAddressAndName()
		{
			EmailContactObject.ToEmailAddress = "1@1.com";
			EmailContactObject.Subject = "This is the subject.";
			EmailContactObject.Body = "This is the body.";

			EmailContactObject.FromEmailAddress = "";
			EmailContactObject.FromDisplayName = "";
			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailContactObject.SendEmail();
			AssertEquals("Expect only one email. " + String.Join(", ", Env.OutgoingMailManager.EmailsCreated.Select(x => x.Subject)), 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotEquals("EmailContactObject.FromDisplayName", "", email.FromDisplayName);
			AssertNotEquals("EmailContactObject.FromAddress", "", email.FromAddress);

			EmailContactObject.FromEmailAddress = "Test@cargowise.com";
			EmailContactObject.FromDisplayName = "Developer";
			Env.OutgoingMailManager.EmailsCreated.Clear();
			EmailContactObject.SendEmail();
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("EmailContactObject.FromDisplayName", "Developer", email.FromDisplayName);
			AssertEquals("EmailContactObject.FromAddress", "Test@cargowise.com", email.FromAddress);
		}

		#region Implementation

		protected override T GetEmailWithAttachment()
		{
			return EmailContactObject;
		}

		protected override string DefaultFromEmailAddress { get { return Env.Registry.SMTPDefaultReturnEmailAddress; } }
		protected override string DefaultFromDisplayName { get { return string.Empty; } }

		protected override BusinessObject GetNewBusinessObject()
		{
			return (T)Activator.CreateInstance(typeof(T), BusinessObjectSendingEmail);
		}

		protected BusinessObject BusinessObjectSendingEmail
		{
			get
			{
				if (businessObjectSendingEmail == null)
				{
					businessObjectSendingEmail = GetNewBusinessObjectSendingEmail();
				}
				return businessObjectSendingEmail;
			}
		}

		protected T EmailContactObject
		{
			get
			{
				if (emailContactObject == null)
				{
					emailContactObject = (T)GetNewBusinessObject();
				}
				return emailContactObject;
			}
		}

		protected virtual BusinessObject GetNewBusinessObjectSendingEmail()
		{
			return Factory.New<DummyEnterpriseBusinessObject>();
		}

		BusinessObject businessObjectSendingEmail;
		T emailContactObject;

		#endregion
	}
}
