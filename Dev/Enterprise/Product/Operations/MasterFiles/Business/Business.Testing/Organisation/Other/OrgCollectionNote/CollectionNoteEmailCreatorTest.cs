using System.IO;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CollectionNoteEmailCreator))]
	sealed class CollectionNoteEmailCreatorTest : EmailToContactBusinessObjectTestCase<CollectionNoteEmailCreator>
	{
		protected override void SetUp()
		{
			base.SetUp();
			ResetCurrentUserSetup(); // If current user has an email address, calendar reminders will be also be sent
		}

		protected override void TestSendEmail(CollectionNoteEmailCreator emailContactObject, bool hasNote)
		{
			BusinessObjectSendingEmail.FillWithValidTestData();
			Factory.Save();
			OrgCollectionNote note = (OrgCollectionNote)BusinessObjectSendingEmail;

			AssertEquals("precondition: no files", 0, ((IDocManagerSupport)note.Header).DocManagerInfo.AllEDocs.Count);

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
			}
			if (hasNote)
			{
				AssertContains("Body of the Email", EmailContactObject.Body, note.PN_CallDetailNote);
			}
			else
			{
				AssertNotContains("Body of the Email", EmailContactObject.Body, note.PN_CallDetailNote);
			}
			AssertEquals("Should be added to the Header.Logs after sending", "1@1.com, 2@2.com, 3@3.com, 4@4.com", note.Header.Logs.MostRecentLogByEventTime(Events.EmailSent).SL_Reference);
			AssertEquals("The OrgHeader should contain eDoc with email text", "Collection Call Email.txt", ((IDocManagerSupport)note.Header).DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("The OrgHeader should contain 3 eDocs after sending the email (one email text and 2 attachments)", 3, ((IDocManagerSupport)note.Header).DocManagerInfo.AllEDocs.Count);
		}

		protected override BusinessObject GetNewBusinessObjectSendingEmail()
		{
			return Header.CollectionNotes.AddNew();
		}

		OrgHeader Header
		{
			get
			{
				if (fHeader == null)
				{
					fHeader = Factory.NewWithValidTestData<OrgHeader>();
				}

				return fHeader;
			}
		}
		OrgHeader fHeader;
	}
}
