using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	[TestedType(typeof(PersonMergeEmailSender))]
	public class PersonMergeEmailSenderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestRetainedPerson()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_EmailAddress = "Rand@Al.com";
			personRetained.PER_FullName = "Rand Al'Thor";
			personRetained.PER_PasswordHash = new ZBlob(new byte[] { 2, 3, 4 });
			personRetained.PER_PasswordHashIterations = 9239;
			personRetained.PER_PasswordSalt = new ZBlob(new byte[] { 2, 3, 4 });
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = personRetained.PK;
			contact1.OC_Email = "blah@blah.com";
			contact1.OC_ContactName = "crah com";
			var contactWithNoEmail = org1.Contacts.AddNew();
			contactWithNoEmail.OC_PER = personRetained.PK;
			contactWithNoEmail.OC_ContactName = "com";
			var inactiveContact = org1.Contacts.AddNew();
			inactiveContact.OC_PER = personRetained.PK;
			inactiveContact.OC_Email = "alibah@blah.com";
			inactiveContact.OC_ContactName = "ali bah blah";
			inactiveContact.OC_IsActive = false;

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Lews Therin Telamon";
			personDissolved.PER_EmailAddress = "lanfear@theways.com";
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var dissolvedContact = org1.Contacts.AddNew();
			dissolvedContact.OC_PER = personDissolved.PK;
			dissolvedContact.OC_Email = "crah@blah.com";
			dissolvedContact.OC_ContactName = "crah blah com";
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(personRetained, new ReadOnlyCollection<GlbPerson>(new[] { personDissolved }));
			AssertEquals(personRetained.PK, emailSender.RetainedPerson.PK);
			AssertEquals(personRetained.PER_EmailAddress, emailSender.RetainedPerson.Email);
			AssertEquals(personRetained.PER_FullName, emailSender.RetainedPerson.FullName);
			AssertEquals(personRetained.HasPassword, emailSender.RetainedPerson.HasPassword);
			AssertEquals(personRetained.PER_PasswordHash, emailSender.RetainedPerson.PasswordHash);
			AssertEquals("Should exclude contacts without email and inactive contacts", 1, emailSender.RetainedPerson.ActiveContactsWithEmail.Count);
			AssertEquals("Should exclude contacts without email and inactive contacts", contact1.OC_Email, emailSender.RetainedPerson.ActiveContactsWithEmail.FirstOrDefault().Email);
		}

		public void TestSuccessfulDissolvedPersonCollection()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_EmailAddress = "Rand@Al.com";
			personRetained.PER_FullName = "Rand Al'Thor";
			personRetained.PER_PasswordHash = new ZBlob(new byte[] { 2, 3, 4 });
			personRetained.PER_PasswordHashIterations = 9239;
			personRetained.PER_PasswordSalt = new ZBlob(new byte[] { 2, 3, 4 });

			var personDissolved1 = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved1.PER_FullName = "Lews Therin Telamon";
			personDissolved1.PER_EmailAddress = "lanfear@theways.com";
			personDissolved1.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved1.PER_PasswordHashIterations = 9239;
			personDissolved1.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = personDissolved1.PK;
			contact1.OC_Email = "blah@blah.com";
			contact1.OC_ContactName = "crah com";
			var contactWithNoEmail = org1.Contacts.AddNew();
			contactWithNoEmail.OC_PER = personDissolved1.PK;
			contactWithNoEmail.OC_ContactName = "com";
			var inactiveContact = org1.Contacts.AddNew();
			inactiveContact.OC_PER = personDissolved1.PK;
			inactiveContact.OC_Email = "alibah@blah.com";
			inactiveContact.OC_ContactName = "ali bah blah";
			inactiveContact.OC_IsActive = false;

			var personDissolved2 = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved2.PER_FullName = "Lews Therin Telamon";
			personDissolved2.PER_EmailAddress = "lanfear@theways.com";
			personDissolved2.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved2.PER_PasswordHashIterations = 9239;
			personDissolved2.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var dissolvedContact = org1.Contacts.AddNew();
			dissolvedContact.OC_PER = personDissolved2.PK;
			dissolvedContact.OC_Email = "ragara@blah.com";
			dissolvedContact.OC_ContactName = "Lews Therin Telamon";
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(personRetained, new ReadOnlyCollection<GlbPerson>(new[] { personDissolved1, personDissolved2 }));
			emailSender.MarkPersonAsDissolved(personDissolved1.PK);

			AssertEquals(1, emailSender.SuccessfulDissolvedPersonCollection.Count());
			var successfullyDissolvedPerson = emailSender.SuccessfulDissolvedPersonCollection.FirstOrDefault();
			AssertEquals(personDissolved1.PK, successfullyDissolvedPerson.PK);
			AssertEquals(personDissolved1.PER_EmailAddress, successfullyDissolvedPerson.Email);
			AssertEquals(personDissolved1.PER_FullName, successfullyDissolvedPerson.FullName);
			AssertEquals(personDissolved1.HasPassword, successfullyDissolvedPerson.HasPassword);
			AssertEquals(personDissolved1.PER_PasswordHash, successfullyDissolvedPerson.PasswordHash);
			AssertEquals("Should exclude contacts without email and inactive contacts", 1, successfullyDissolvedPerson.ActiveContactsWithEmail.Count);
			AssertEquals("Should exclude contacts without email and inactive contacts", contact1.OC_Email, successfullyDissolvedPerson.ActiveContactsWithEmail.FirstOrDefault().Email);
		}

		public void TestGetMergedAccountsEmail()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Rand Al'Thor";

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Lews Therin Telamon";
			personDissolved.PER_EmailAddress = "lanfear@theways.com";
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = personDissolved.PK;
			contact1.OC_Email = "blah@blah.com";

			Factory.Save();

			var emailSender = new PersonMergeEmailSender(personRetained, new ReadOnlyCollection<GlbPerson>(new[] { personDissolved }));
			emailSender.MarkPersonAsDissolved(personDissolved.PK);
			personRetained.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personRetained.PER_PasswordHashIterations = 9239;
			personRetained.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var email = emailSender.GetMergedAccountsEmail();
			AssertNotNull(email);
			AssertEquals(1, email.Recipients.Count);
			Assert(email.Recipients.Contains("lanfear@theways.com"));
			Assert("Should only contain personal email", !email.Recipients.Contains("blah@blah.com"));
			AssertEquals(Env.Registry.MailboxDisplayName, email.FromDisplayName);
			AssertEquals(Env.Registry.EnterpriseMailboxEmailAddress, email.FromAddress);
			AssertEquals(Env.Registry.SMTPDefaultReturnEmailAddress, email.ReplyTo);

			var parser = DocumentParser.New(ObjectFactory.GetType<Integration.DocumentWrappers.IDocPersonMergeEmailSender>(), Factory);
			var template = SystemDataRegistry.Instance.PersonMergeWithPasswordNotificationEmailTemplate.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals(parser.Parse(emailSender, template.EmailSubject), email.Subject);
			AssertContains(parser.Parse(emailSender, template.EmailBody), email.Body);
		}

		public void TestGetMergedAccountsEmail_UnsuccessfulDissolve()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Rand Al'Thor";

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Lews Therin Telamon";
			personDissolved.PER_EmailAddress = "lanfear@theways.com";
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = personDissolved.PK;
			contact1.OC_Email = "blah@blah.com";

			Factory.Save();

			var emailSender = new PersonMergeEmailSender(personRetained, new ReadOnlyCollection<GlbPerson>(new[] { personDissolved }));
			AssertEquals("If the dissolved person isn't marked as successfully dissolved, they should not be included in the email", null, emailSender.GetMergedAccountsEmail());
		}

		public void TestGetMergedAccountsEmail_NoContactsShouldReturnNull()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Rand Al'Thor";

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Lews Therin Telamon";
			personDissolved.PER_EmailAddress = "lanfear@theways.com";
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });

			Factory.Save();

			var emailSender = new PersonMergeEmailSender(personRetained, new ReadOnlyCollection<GlbPerson>(new[] { personDissolved }));
			emailSender.MarkPersonAsDissolved(personDissolved.PK);
			var email = emailSender.GetMergedAccountsEmail();
			AssertNull(email);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = personRetained.PK;
			contact1.OC_Email = "blah@blah.com";
			contact1.OC_ContactName = "blah";
			Factory.Save();

			emailSender = new PersonMergeEmailSender(personRetained, new ReadOnlyCollection<GlbPerson>(new[] { personDissolved }));
			emailSender.MarkPersonAsDissolved(personDissolved.PK);
			email = emailSender.GetMergedAccountsEmail();
			AssertEquals("Should not generate email if no contacts attached to the dissolved person", null, email);

			var contact2 = org1.Contacts.AddNew();
			contact2.OC_PER = personDissolved.PK;
			contact2.OC_Email = "hah@blah.com";
			contact2.OC_ContactName = "hah";
			Factory.Save();
			emailSender = new PersonMergeEmailSender(personRetained, new ReadOnlyCollection<GlbPerson>(new[] { personDissolved }));
			emailSender.MarkPersonAsDissolved(personDissolved.PK);
			personRetained.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personRetained.PER_PasswordHashIterations = 9239;
			personRetained.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			Factory.Save();
			email = emailSender.GetMergedAccountsEmail();
			AssertNotNull(email);
		}

		public void TestGetMergedAccountsEmail_WithOnlyInactiveContactShouldReturnNull()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Rand Al'Thor";

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Lews Therin Telamon";
			personDissolved.PER_EmailAddress = "lanfear@theways.com";
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = personRetained.PK;
			contact1.OC_Email = "blah@blah.com";
			contact1.OC_ContactName = "blah";
			var contact2 = org1.Contacts.AddNew();
			contact2.OC_PER = personDissolved.PK;
			contact2.OC_Email = "hah@blah.com";
			contact2.OC_ContactName = "hah";
			contact2.OC_IsActive = false;
			Factory.Save();

			var emailSender = new PersonMergeEmailSender(personRetained, new ReadOnlyCollection<GlbPerson>(new[] { personDissolved }));
			emailSender.MarkPersonAsDissolved(personDissolved.PK);
			var email = emailSender.GetMergedAccountsEmail();
			AssertEquals("Should not generate email if dissolved person's contacts are all inactive", null, email);
		}

		public void TestGetMergedAccountsEmail_ContactsWithNoEmailShouldReturnNull()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Rand Al'Thor";
			personRetained.PER_PasswordHash = new ZBlob(new byte[] { 2, 3, 4, 5 });
			personRetained.PER_PasswordHashIterations = 2918;
			personRetained.PER_PasswordSalt = new ZBlob(new byte[] { 2, 3, 4, 5 });
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = personRetained.PK;

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Lews Therin Telamon";
			personDissolved.PER_EmailAddress = "lanfear@theways.com";
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_EmailAddress = "person@email.com";
			var contact2 = org1.Contacts.AddNew();
			contact2.OC_PER = personDissolved.PK;
			contact2.OC_ContactName = "hah";

			Factory.Save();

			var emailSender = new PersonMergeEmailSender(personRetained, new ReadOnlyCollection<GlbPerson>(new[] { personDissolved }));
			emailSender.MarkPersonAsDissolved(personDissolved.PK);
			var email = emailSender.GetMergedAccountsEmail();
			AssertNull(email);
		}

		public void TestGetMergedAccountsEmail_NoPasswordShouldReturnNull()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Rand Al'Thor";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = personRetained.PK;
			contact1.OC_Email = "contact@email.com";

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Lews Therin Telamon";
			personDissolved.PER_EmailAddress = "lanfear@theways.com";
			var contact2 = org1.Contacts.AddNew();
			contact2.OC_PER = personDissolved.PK;
			contact2.OC_ContactName = "hah";
			contact2.OC_Email = "blag@mag.com";

			Factory.Save();

			var emailSender = new PersonMergeEmailSender(personRetained, new ReadOnlyCollection<GlbPerson>(new[] { personDissolved }));
			emailSender.MarkPersonAsDissolved(personDissolved.PK);
			var email = emailSender.GetMergedAccountsEmail();
			AssertNull(email);
		}

		public void TestGetMergedAccountsEmail_MultiMergeRecipients()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Rand Al'Thor";
			personRetained.PER_EmailAddress = "gaga@oohlala.com";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = personRetained.PK;
			contact1.OC_ContactName = "blah";
			contact1.OC_Email = "blah@blah.com";

			var personDissolved1 = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved1.PER_FullName = "Lews Therin Telamon";
			personDissolved1.PER_EmailAddress = "lanfear@theways.com";
			personDissolved1.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved1.PER_PasswordHashIterations = 9239;
			personDissolved1.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var contact2 = org1.Contacts.AddNew();
			contact2.OC_PER = personDissolved1.PK;
			contact2.OC_ContactName = "rah";
			contact2.OC_Email = "rah@rah.com";

			var personDissolved2 = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved2.PER_FullName = "Bear";
			personDissolved2.PER_EmailAddress = "hey@there.com";
			var contact3 = org1.Contacts.AddNew();
			contact3.OC_PER = personDissolved2.PK;
			contact3.OC_ContactName = "roma";
			contact3.OC_Email = "roma@ma.com";
			var contact4 = org1.Contacts.AddNew();
			contact4.OC_PER = personDissolved2.PK;
			contact4.OC_ContactName = "ya";
			contact4.OC_Email = "ya@ya.com";
			var inactiveContact = org1.Contacts.AddNew();
			inactiveContact.OC_PER = personDissolved2.PK;
			inactiveContact.OC_ContactName = "prone";
			inactiveContact.OC_Email = "water@wolf.com";
			inactiveContact.OC_IsActive = false;

			var personDissolved3 = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved3.PER_FullName = "Big Blue House";
			personDissolved3.PER_EmailAddress = "whey@wheat.com";
			var contact5 = org1.Contacts.AddNew();
			contact5.OC_PER = personDissolved3.PK;
			contact5.OC_ContactName = "chrome";
			contact5.OC_Email = "fire@fox.com";

			Factory.Save();

			var emailSender = new PersonMergeEmailSender(personRetained, new ReadOnlyCollection<GlbPerson>(new[] { personDissolved1, personDissolved2, personDissolved3 }));
			emailSender.MarkPersonAsDissolved(personDissolved1.PK);
			emailSender.MarkPersonAsDissolved(personDissolved2.PK);
			personRetained.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personRetained.PER_PasswordHashIterations = 9239;
			personRetained.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			Factory.Save();
			var email = emailSender.GetMergedAccountsEmail();
			AssertNotNull(email);
			AssertEquals(3, email.Recipients.Count);
			Assert(email.Recipients.Contains("gaga@oohlala.com"));
			Assert("Should only contain personal email", !email.Recipients.Contains("blah@blah.com"));
			Assert(email.Recipients.Contains("lanfear@theways.com"));
			Assert("Should only contain personal email", !email.Recipients.Contains("rah@rah.com"));
			Assert(email.Recipients.Contains("hey@there.com"));
			Assert("Should only contain personal email", !email.Recipients.Contains("roma@ma.com"));
			Assert("Should only contain personal email", !email.Recipients.Contains("ya@ya.com"));
			Assert("Should not contain personDissolved3 since it was not marked as successfully dissolved", !email.Recipients.Contains("whey@wheat.com"));
			Assert("Should not contain personDissolved3's related contact since it was not marked as successfully dissolved", !email.Recipients.Contains("fire@fox.com"));
			Assert("Should not contain inactiveContact since it is inactive", !email.Recipients.Contains("water@wolf.com"));
		}

		public void TestMarkPersonAsDissolved()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Rand Al'Thor";

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Lews Therin Telamon";
			personDissolved.PER_EmailAddress = "lanfear@theways.com";
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = personDissolved.PK;
			contact1.OC_Email = "blah@blah.com";

			Factory.Save();

			var emailSender = new PersonMergeEmailSender(personRetained, new ReadOnlyCollection<GlbPerson>(new[] { personDissolved }));
			AssertEquals("Should not have successfully dissolved any persons", 0, emailSender.SuccessfulDissolvedPersonCollection.Count());
			emailSender.MarkPersonAsDissolved(personDissolved.PK);
			AssertEquals("Should have successfully dissolved personDissolved", 1, emailSender.SuccessfulDissolvedPersonCollection.Count());
		}

		public void TestPasswordMatchesRetainedPassword()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();
			personRetained.PER_FullName = "Rand Al'Thor";

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_FullName = "Lews Therin Telamon";
			personDissolved.PER_EmailAddress = "lanfear@theways.com";
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = personDissolved.PK;
			contact1.OC_Email = "blah@blah.com";

			Factory.Save();

			var emailSender = new PersonMergeEmailSender(personRetained, new ReadOnlyCollection<GlbPerson>(new[] { personDissolved }));
			emailSender.MarkPersonAsDissolved(personDissolved.PK);
			AssertEquals("Password should not match retained password", false, emailSender.PasswordMatchesRetainedPassword(emailSender.SuccessfulDissolvedPersonCollection.FirstOrDefault()));

			personRetained.PER_PasswordHash = personDissolved.PER_PasswordHash;
			personRetained.PER_PasswordHashIterations = personDissolved.PER_PasswordHashIterations;
			personRetained.PER_PasswordSalt = personDissolved.PER_PasswordSalt;
			Factory.Save();
			AssertEquals("Password should match retained password", true, emailSender.PasswordMatchesRetainedPassword(emailSender.SuccessfulDissolvedPersonCollection.FirstOrDefault()));
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var personRetained = Factory.NewWithValidTestData<GlbPerson>();

			var personDissolved = Factory.NewWithValidTestData<GlbPerson>();
			personDissolved.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			personDissolved.PER_PasswordHashIterations = 9239;
			personDissolved.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = personDissolved.PK;
			contact1.OC_Email = "blah@blah.com";

			Factory.Save();

			return new PersonMergeEmailSender(personRetained, new ReadOnlyCollection<GlbPerson>(new[] { personDissolved }));
		}

		#endregion
	}
}
