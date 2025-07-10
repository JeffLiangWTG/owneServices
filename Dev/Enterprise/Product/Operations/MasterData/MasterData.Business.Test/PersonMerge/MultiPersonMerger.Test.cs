using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	public class MultiPersonMergerTest : TestCaseWithFactory
	{
		public void TestMergeSelected_ThatMergeProgressNotificationEventIsRaised()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Old Smith";

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Smith Jane";

			Factory.Save();

			var dissolvedCollection = new PersonMergeBusinessObjectCollection
			{
				new PersonMergeBusinessObject(dissolvedPerson1),
				new PersonMergeBusinessObject(dissolvedPerson2)
			};

			var retainedCollection = new PersonMergeBusinessObjectCollection
			{
				new PersonMergeBusinessObject(retainedPerson)
			};

			var expectedNotifications = new[] { Tuple.Create(1, 2), Tuple.Create(2, 2) };
			var eventNotifications = new List<Tuple<int, int>>();
			var multiPersonMerger = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection);

			multiPersonMerger.MergeProgressNotification += delegate(object sender, MergeProgressEventArgs e)
			{
				eventNotifications.Add(Tuple.Create(e.ProgressCount, e.ProgressTotal));
			};

			multiPersonMerger.MergeSelected().Wait();

			AssertContainsExactElementsInAnyOrder(expectedNotifications, eventNotifications);
		}

		public void TestMergeSelected()
		{
			var retainedCollection = new PersonMergeBusinessObjectCollection();
			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();

			PersonMergerTest.PersonAssociations.AddNewStaffToPerson(Factory, retainedPerson);
			PersonMergerTest.PersonAssociations.AddNewContactToPerson(Factory, dissolvedPerson1);
			PersonMergerTest.PersonAssociations.AddNewHRJobApplicantToPerson(Factory, dissolvedPerson2);

			Factory.Save();

			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson2));

			CombineAssertions("Pre-condition", () =>
			{
				AssertEquals(0, retainedPerson.ApplicantCollection.Count);
				AssertEquals(0, retainedPerson.ContactCollection.Count);
				AssertEquals(1, retainedPerson.StaffCollection.Count);
				AssertEquals("Staff (1)", retainedCollection[0].ActiveAssociations);
			});

			var multiPersonMerger = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection);

			multiPersonMerger.MergeSelected().Wait();

			CombineAssertions(() =>
			{
				AssertEquals(ParticipantStatus.Completed, dissolvedCollection[0].MergeStatus);
				AssertEquals(ParticipantStatus.Completed, dissolvedCollection[1].MergeStatus);
				AssertEquals("Contact (1), Staff (1), Applicant (1)", retainedCollection[0].ActiveAssociations);
				AssertEquals(1, retainedPerson.ApplicantCollection.Count);
				AssertEquals(1, retainedPerson.StaffCollection.Count);
				AssertEquals(1, retainedPerson.ContactCollection.Count);
			});
		}

		#region Constructor

		public void TestConstructor_ThrowsException_WhenRetainedParamIsNull()
		{
			var collection = new PersonMergeBusinessObjectCollection();

			collection.Add(new PersonMergeBusinessObject(Factory.NewWithValidTestData<GlbPerson>()));

			AssertExceptionThrown<ArgumentNullException>(() => new MultiPersonMerger(null, collection));
		}

		public void TestConstructor_ThrowsException_WhenDissolvedParamIsNull()
		{
			var collection = new PersonMergeBusinessObjectCollection();

			collection.Add(new PersonMergeBusinessObject(Factory.NewWithValidTestData<GlbPerson>()));

			AssertExceptionThrown<ArgumentNullException>(() => new MultiPersonMerger(collection, null));
		}

		#endregion

		#region MergeSelected

		public void TestMergeSelectedSetIsDissolvingProperty()
		{
			var retainedCollection = new PersonMergeBusinessObjectCollection();
			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson));

			AssertNotEquals("Before merge, MergeStatus is not 'Complete'.", ParticipantStatus.Completed, dissolvedCollection[0].MergeStatus);
			AssertEquals("Before merge, dissolved person's IsDissolving is false", false, dissolvedCollection[0].Person.IsDissolving);

			var saverWithException = new PersonMergerTest.PersonMergeTransactionSaverForTest(MergerException.ZSaveDataException, true);
			var dissolvedPersonTuples = new List<Tuple<IPersonMergeTransactionSaver, Exception>>
			{
				{ saverWithException, MergerException.ZSaveDataException }
			};

			var failingMultiPersonMerger_failed = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection, dissolvedPersonTuples);
			failingMultiPersonMerger_failed.MergeSelected().Wait();

			AssertNotEquals("MergeStatus is not 'Complete' means merge is failed.", ParticipantStatus.Completed, dissolvedCollection[0].MergeStatus);
			AssertEquals("After a failed merge, dissolved person's IsDissolving should be false", false, dissolvedCollection[0].Person.IsDissolving);

			var failingMultiPersonMerger_successful = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection);
			failingMultiPersonMerger_successful.MergeSelected().Wait();

			AssertEquals("MergeStatus is 'Complete' means merge is successful.", ParticipantStatus.Completed, dissolvedCollection[0].MergeStatus);
			AssertEquals("After a successful merge, dissolved person's IsDissolving should be true", true, dissolvedCollection[0].Person.IsDissolving);
		}

		public void TestMergeSelected_MergesInOrder()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";
			retainedPerson.PER_WebAccessEnabled = false;

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Old Smith";
			dissolvedPerson1.PER_EmailAddress = "WebaccessTrue@Overrides.com";
			dissolvedPerson1.PER_WebAccessEnabled = true;
			dissolvedPerson1.PER_HomeAddress1 = "Enrichment Street";
			dissolvedPerson1.City = "Alexandria";

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Discardme Smith";
			dissolvedPerson2.PER_HomeAddress1 = "Discardme Street";
			dissolvedPerson2.PER_State = "DiscardAll GroupedFields";

			var dissolvedPerson3 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Enrichment Smith";
			dissolvedPerson3.PER_HomePhone = "0449743938";

			Factory.Save();

			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson2));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson3));

			var retainedCollection = new PersonMergeBusinessObjectCollection();
			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));

			var multiPersonMerger = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection);

			multiPersonMerger.MergeSelected().Wait();

			CombineAssertions("The first non-empty property groups should be used as enrichment.", () =>
			{
				AssertEquals("Bob Smith", retainedPerson.PER_FullName);
				AssertEquals("WebaccessTrue@Overrides.com", retainedPerson.PER_EmailAddress);
				AssertEquals(true, retainedPerson.PER_WebAccessEnabled);
				AssertEquals("Enrichment Street", retainedPerson.PER_HomeAddress1);
				AssertEquals("Alexandria", retainedPerson.City);
				AssertEquals(string.Empty, retainedPerson.PER_State);
				AssertEquals("0449743938", retainedPerson.PER_HomePhone);
			});
		}

		public void TestMergeSelected_SendNotificationEmail()
		{
			SystemDataRegistry.Instance.PersonMergeWithPasswordNotificationEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate() { EmailSubject = "nuts", EmailBody = "bolts" });
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";
			retainedPerson.PER_WebAccessEnabled = false;

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Old Smith";
			dissolvedPerson1.PER_EmailAddress = "WebaccessTrue@Overrides.com";
			dissolvedPerson1.PER_WebAccessEnabled = true;
			dissolvedPerson1.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			dissolvedPerson1.PER_PasswordHashIterations = 9239;
			dissolvedPerson1.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = dissolvedPerson1.PK;
			contact1.OC_Email = "blah@blah.com";

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Discardme Smith";
			dissolvedPerson2.PER_EmailAddress = "maniac@onthefloor.com";
			dissolvedPerson2.PER_PasswordHash = new ZBlob(new byte[] { 2, 3, 4, 5 });
			dissolvedPerson2.PER_PasswordHashIterations = 9240;
			dissolvedPerson2.PER_PasswordSalt = new ZBlob(new byte[] { 2, 3, 4, 5 });

			Factory.Save();

			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson2));

			var retainedCollection = new PersonMergeBusinessObjectCollection();
			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));

			var multiPersonMerger = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection);

			multiPersonMerger.MergeSelected().Wait();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("Bob Smith", retainedPerson.PER_FullName);
				AssertEquals("WebaccessTrue@Overrides.com", retainedPerson.PER_EmailAddress);
				AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), retainedPerson.PER_PasswordHash);
				AssertEquals(9239, retainedPerson.PER_PasswordHashIterations);
				AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), retainedPerson.PER_PasswordSalt);
			});

			AssertEquals("Should send an email to personal email address", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var mergeNotificationEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Should send to the personal email addresses", 2, mergeNotificationEmail.Recipients.Count);
			Assert("Should send to the personal email addresses", mergeNotificationEmail.Recipients.Contains("WebaccessTrue@Overrides.com"));
			Assert("Should not send to the contact email addresses", !mergeNotificationEmail.Recipients.Contains("blah@blah.com"));
			Assert("Should send to the personal email addresses", mergeNotificationEmail.Recipients.Contains("maniac@onthefloor.com"));
			AssertContains("nuts", mergeNotificationEmail.Subject);
			AssertContains("bolts", mergeNotificationEmail.Body);
		}

		#region Error Handling

		public void TestMergeSelected_RollBackMerge_IfErrors()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";
			retainedPerson.PER_WebAccessEnabled = false;

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Old Smith";
			dissolvedPerson1.PER_HomeAddress1 = "Enrichment Street";

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Rollme Bach";
			dissolvedPerson2.PER_HomePhone = "0444444444";

			var retainedCollection = new PersonMergeBusinessObjectCollection();
			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));

			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson2));

			Factory.Save();

			var dissolvedPersonTuples = new List<Tuple<IPersonMergeTransactionSaver, Exception>>()
			{
				{ new PersonMergeTransactionSaver(), null },
				{ new PersonMergerTest.PersonMergeTransactionSaverForTest(MergerException.ZSaveDataException, true), MergerException.ZSaveDataException }
			};

			var multiPersonMerger = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection, dissolvedPersonTuples);

			multiPersonMerger.MergeSelected().Wait();

			var hasException = multiPersonMerger.CaughtExceptionForTest.Any();

			CombineAssertions("Check MergeSelected rolled back only the failed merge.", () =>
			{
				AssertEquals(true, hasException);
				AssertEquals(true, dissolvedCollection[1].HasRowMessageErrors);
				AssertEquals(ParticipantStatus.FailedWithCriticalError, dissolvedCollection[1].MergeStatus);
				Assert(dissolvedCollection[1].RowMessageErrors.Any(x => x.Message.StartsWith("CargoWise.EntityFramework.ZSaveException")));
				AssertEquals("Bob Smith", retainedPerson.PER_FullName);
				AssertEquals("Enrichment Street", retainedPerson.PER_HomeAddress1);
				AssertEquals(string.Empty, retainedPerson.PER_HomePhone);
				AssertEquals("Dissolved person should be deleted.", false, Factory.ExistsInDatabase(AutoGlbPerson.Schema.TableName, new ZQuery(GlbPersonSchema.PK, dissolvedPerson1.PK)));
				AssertEquals("Person in failed merge should not be deleted.", true, Factory.ExistsInDatabase(AutoGlbPerson.Schema.TableName, new ZQuery(GlbPersonSchema.PK, dissolvedPerson2.PK)));
			});
		}

		public void TestMergeSelected_ContinuesProcessingList_AfterRollback()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";

			var dissolvedPerson = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson.PER_FullName = "Rollme Bach";
			dissolvedPerson.PER_HomePhone = "0444444444";
			dissolvedPerson.PER_City = "RollMe Bach City";

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Old Smith";
			dissolvedPerson2.PER_HomePhone = "0449743938";
			dissolvedPerson2.PER_HomeAddress1 = "Enrichment Street";

			Factory.Save();

			var retainedCollection = new PersonMergeBusinessObjectCollection();
			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));

			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson2));

			var dissolvedPersonTuples = new List<Tuple<IPersonMergeTransactionSaver, Exception>>()
			{
				{ new PersonMergerTest.PersonMergeTransactionSaverForTest(MergerException.ZSaveDataException, true), MergerException.ZSaveDataException },
				{ new PersonMergeTransactionSaver(), null }
			};

			var personMultiMerger = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection, dissolvedPersonTuples);

			personMultiMerger.MergeSelected().Wait();

			var hasException = personMultiMerger.CaughtExceptionForTest.Any();

			CombineAssertions("Check MergeSelected rolled the back failed merge and completed the other.", () =>
			{
				AssertEquals(true, hasException);
				AssertEquals(true, dissolvedCollection[0].HasRowMessageErrors);
				AssertEquals(ParticipantStatus.FailedWithCriticalError, dissolvedCollection[0].MergeStatus);
				Assert(dissolvedCollection[0].RowMessageErrors.Any(x => x.Message.StartsWith("CargoWise.EntityFramework.ZSaveException")));
				AssertEquals("Bob Smith", retainedPerson.PER_FullName);
				AssertEquals("0449743938", retainedPerson.PER_HomePhone);
				AssertEquals("Enrichment Street", retainedPerson.PER_HomeAddress1);
				AssertEquals(string.Empty, retainedPerson.PER_City);
				AssertEquals("Person in failed merge should not be deleted.", true, Factory.ExistsInDatabase(AutoGlbPerson.Schema.TableName, new ZQuery(GlbPersonSchema.PK, dissolvedPerson.PK)));
				AssertEquals("Dissolved person should be deleted.", false, Factory.ExistsInDatabase(AutoGlbPerson.Schema.TableName, new ZQuery(GlbPersonSchema.PK, dissolvedPerson2.PK)));
			});
		}

		public void TestMergeSelected_ThrowAggregateException_WhenErrorsInMerge()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "ArgumentNull Smith";

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "InvalidOperation Smith";

			var dissolvedPerson3 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson3.PER_FullName = "ZSave Smith";

			var dissolvedPerson4 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson4.PER_FullName = "NonCriticalZSql Smith";

			var retainedCollection = new PersonMergeBusinessObjectCollection();
			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));

			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson2));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson3));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson4));

			Factory.Save();

			var sqlExceptionBuilder = SqlExceptionBuilder.CreateSqlException(0, "");
			var dissolvedPersonTuples = new List<Tuple<IPersonMergeTransactionSaver, Exception>>()
			{
				{ new PersonMergerTest.PersonMergeTransactionSaverForTest(MergerException.ArgumentNullException, true), MergerException.ArgumentNullException },
				{ new PersonMergerTest.PersonMergeTransactionSaverForTest(MergerException.InvalidOperationException, true), MergerException.InvalidOperationException },
				{ new PersonMergerTest.PersonMergeTransactionSaverForTest(MergerException.ZSaveDataException, true), MergerException.ZSaveDataException },
				{ new PersonMergerTest.PersonMergeTransactionSaverForTest(sqlExceptionBuilder, true), sqlExceptionBuilder }
			};

			var multiPersonMerger = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection, dissolvedPersonTuples);

			multiPersonMerger.MergeSelected().Wait();

			var exceptions = multiPersonMerger.CaughtExceptionForTest;

			AssertEquals(4, exceptions.Count);

			CombineAssertions("All created exceptions are caught", () =>
			{
				AssertType<ArgumentNullException>(exceptions[0]);
				AssertType<InvalidOperationException>(exceptions[1]);
				AssertType<ZSaveException>(exceptions[2]);
				AssertType<SqlException>(exceptions[3]);
				AssertEquals(true, dissolvedCollection[0].HasRowMessageErrors);
				AssertEquals(true, dissolvedCollection[1].HasRowMessageErrors);
				AssertEquals(true, dissolvedCollection[2].HasRowMessageErrors);
				AssertEquals(true, dissolvedCollection[3].HasRowMessageErrors);
				Assert(dissolvedCollection[0].RowMessageErrors.Any(x => x.Message.StartsWith("System.ArgumentNullException")));
				Assert(dissolvedCollection[1].RowMessageErrors.Any(x => x.Message.StartsWith("System.InvalidOperationException")));
				Assert(dissolvedCollection[2].RowMessageErrors.Any(x => x.Message.StartsWith("CargoWise.EntityFramework.ZSaveException")));
				Assert(dissolvedCollection[3].RowMessageErrors.Any(x => x.Message.Contains("SqlClient.SqlException")));
				AssertEquals(ParticipantStatus.FailedWithCriticalError, dissolvedCollection[0].MergeStatus);
				AssertEquals(ParticipantStatus.FailedWithCriticalError, dissolvedCollection[1].MergeStatus);
				AssertEquals(ParticipantStatus.FailedWithCriticalError, dissolvedCollection[2].MergeStatus);
				AssertEquals(ParticipantStatus.FailedWithCriticalError, dissolvedCollection[3].MergeStatus);
			});
		}

		[TestDate(2000, 01, 01)]
		public void TestMergeSelected_RolledBackChanges_ExcludedFromNote()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Keep MyNote";

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "RollBack MyNote";

			var dissolvedPerson3 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson3.PER_FullName = "Append MyNote";

			var retainedCollection = new PersonMergeBusinessObjectCollection();
			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));

			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson2));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson3));

			Factory.Save();

			var dissolvedPersonTuples = new List<Tuple<IPersonMergeTransactionSaver, Exception>>()
			{
				{ new PersonMergeTransactionSaver(), null },
				{ new PersonMergerTest.PersonMergeTransactionSaverForTest(MergerException.ZSaveDataException, true), MergerException.ZSaveDataException },
				{ new PersonMergeTransactionSaver(), null }
			};

			var multiPersonMerger = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection, dissolvedPersonTuples);

			multiPersonMerger.MergeSelected().Wait();

			var hasException = multiPersonMerger.CaughtExceptionForTest.Any();

			AssertEquals("Precondition: ", true, hasException);
			AssertEquals(true, dissolvedCollection[1].HasRowMessageErrors);
			Assert(dissolvedCollection[1].RowMessageErrors.Any(x => x.Message.StartsWith("CargoWise.EntityFramework.ZSaveException")));
			AssertEquals(ParticipantStatus.FailedWithCriticalError, dissolvedCollection[1].MergeStatus);

			var expectedNoteTexts = new string[] {
			"Person Keep MyNote was merged into this Person on 01-Jan-00.\r\n" +
			"The following properties were discarded from Person Keep MyNote during this process:\r\n" +
			"Full Name: Keep MyNote\r\n",
			"Person Append MyNote was merged into this Person on 01-Jan-00.\r\n" +
			"The following properties were discarded from Person Append MyNote during this process:\r\n" +
			"Full Name: Append MyNote\r\n"
					};

			AssertNoteContent(expectedNoteTexts, retainedPerson.PK);
		}

		public void TestMergeSelected_ContainingRelatedChildPerson_RollsbackChildren()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Old Smith";
			dissolvedPerson1.PER_RN_NKNationalityCodeISO = "AU";

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_HomePhone = "0449743938";

			var dissolvedOrgContact = Factory.NewWithValidTestData<OrgContact>();
			dissolvedOrgContact.OC_PER = dissolvedPerson1.PK;
			dissolvedOrgContact.UpdateFromPerson(dissolvedPerson1);

			var dissolvedGlbStaff = Factory.NewWithValidTestData<GlbStaff>();
			dissolvedGlbStaff.SetFromPerson(dissolvedPerson2);

			var retainedCollection = new PersonMergeBusinessObjectCollection();
			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));

			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson2));

			Factory.Save();

			var dissolvedPersonTuples = new List<Tuple<IPersonMergeTransactionSaver, Exception>>()
			{
				{ new PersonMergerTest.PersonMergeTransactionSaverForTest(MergerException.ZSaveDataException, true), MergerException.ZSaveDataException },
				{ new PersonMergeTransactionSaver(), null }
			};

			var multiPersonMerger = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection, dissolvedPersonTuples);

			multiPersonMerger.MergeSelected().Wait();

			var hasException = multiPersonMerger.CaughtExceptionForTest.Any();
			var loadedDissolvedOrgContact = Factory.LoadTop1<OrgContact>(new ZQuery(OrgContactSchema.PK, dissolvedOrgContact.PK));
			var loadedDissolvedGlbStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, dissolvedGlbStaff.PK));

			loadedDissolvedGlbStaff.Reload();
			loadedDissolvedOrgContact.Reload();

			AssertEquals(true, dissolvedCollection[0].HasRowMessageErrors);
			Assert(dissolvedCollection[0].RowMessageErrors.Any(x => x.Message.StartsWith("CargoWise.EntityFramework.ZSaveException")));
			AssertEquals(ParticipantStatus.FailedWithCriticalError, dissolvedCollection[0].MergeStatus);

			CombineAssertions("Precondition: Valid properties for merging are copied, failed merge is rolled back", () =>
			{
				AssertEquals(true, hasException);
				AssertEquals("FullName", "Bob Smith", retainedPerson.PER_FullName);
				AssertEquals("Nationality", string.Empty, retainedPerson.PER_RN_NKNationalityCodeISO);
				AssertEquals("Home phone", "0449743938", retainedPerson.PER_HomePhone);
			});

			CombineAssertions("Child changes should be rolled back", () =>
			{
				AssertEquals("FullName", "Old Smith", loadedDissolvedOrgContact.OC_ContactName);
				AssertEquals("Nationality", "AU", loadedDissolvedOrgContact.OC_RN_NKNationality);
				AssertEquals("Home phone", string.Empty, loadedDissolvedOrgContact.OC_HomePhone);
			});

			CombineAssertions("Child in second merge should not include enrichment from the failed merge.", () =>
			{
				AssertEquals("FullName", "Bob Smith", loadedDissolvedGlbStaff.GS_FullName);
				AssertEquals("Nationality", string.Empty, loadedDissolvedGlbStaff.GS_RN_NKNationalityCode);
				AssertEquals("Home phone", "0449743938", loadedDissolvedGlbStaff.GS_HomePhone);
			});
		}

		#endregion

		#region MergeNote

		public void TestMergeSelected_WithNoDiscardedProperties_ShouldNotCreateNotes()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";
			retainedPerson.PER_HomeAddress1 = "Identical Street";

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Bob Smith";
			dissolvedPerson1.PER_HomeAddress1 = "Identical Street";

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Bob Smith";
			dissolvedPerson2.PER_HomePhone = "0449743938";

			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson2));

			var retainedCollection = new PersonMergeBusinessObjectCollection();
			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));

			Factory.Save();

			var multiPersonMerger = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection);

			multiPersonMerger.MergeSelected().Wait();

			CombineAssertions("Precondition: Valid properties for merging are copied", () =>
			{
				AssertEquals("Bob Smith", retainedPerson.PER_FullName);
				AssertEquals("Identical Street", retainedPerson.PER_HomeAddress1);
				AssertEquals("0449743938", retainedPerson.PER_HomePhone);
			});

			var query = new ZQuery(StmNoteSchema.ST_ParentID, retainedPerson.PK);
			query.AddToFilter(StmNoteSchema.ST_Table, "GlbPerson");
			query.AddToFilter(StmNoteSchema.ST_Description, "Properties Discarded During Merge");

			var loadedNotes = Factory.Load<StmNote>(query);

			AssertEquals("Should not have any merge note", 0, loadedNotes.Length);
		}

		[TestDate(2000, 01, 01)]
		public void TestMergeSelected_PersonsWithDiscardedProperties_ShouldAppendNotes()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";
			retainedPerson.PER_HomeAddress1 = "72 O'Riordan Street";
			retainedPerson.City = "Alexandria";
			retainedPerson.PER_WebAccessEnabled = false;

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Bob Smith";
			dissolvedPerson1.City = "Alexandria";
			dissolvedPerson1.PER_HomePhone = "0449743938";

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "DiscardMe Phone";
			dissolvedPerson2.PER_EmailAddress = "a@b.com";
			dissolvedPerson2.PER_WebAccessEnabled = true;
			dissolvedPerson2.PER_HomePhone = "0449743930";

			var dissolvedPerson3 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson3.PER_FullName = "DiscardMe PhoneWebAcess";
			dissolvedPerson3.PER_HomePhone = "0449743930";
			dissolvedPerson3.PER_WebAccessEnabled = false;

			Factory.Save();

			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson2));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson3));

			var retainedCollection = new PersonMergeBusinessObjectCollection();
			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));

			var multiPersonMerger = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection);

			multiPersonMerger.MergeSelected().Wait();

			CombineAssertions("Precondition: Valid properties for merging are copied", () =>
			{
				AssertEquals("Bob Smith", retainedPerson.PER_FullName);
				AssertEquals("a@b.com", retainedPerson.PER_EmailAddress);
				AssertEquals("72 O'Riordan Street", retainedPerson.PER_HomeAddress1);
				AssertEquals("Alexandria", retainedPerson.City);
				AssertEquals("0449743938", retainedPerson.PER_HomePhone);
				AssertEquals(true, retainedPerson.PER_WebAccessEnabled);
			});

			var expectedNoteText = new string[] {
					"Person DiscardMe Phone was merged into this Person on 01-Jan-00.\r\n" +
				 "The following properties were discarded from Person DiscardMe Phone during this process:\r\n" +
			  "Full Name: DiscardMe Phone\r\nHome Phone: +61 449 743 930\r\n",
					"Person DiscardMe PhoneWebAcess was merged into this Person on 01-Jan-00.\r\n" +
				 "The following properties were discarded from Person DiscardMe PhoneWebAcess during this process:\r\n" +
			  "Full Name: DiscardMe PhoneWebAcess\r\nHome Phone: +61 449 743 930\r\nWeb Access Enabled: No\r\n"
			};

			AssertNoteContent(expectedNoteText, retainedPerson.PK);
		}

		public void TestMergeSelected_ContainingRelatedChildPerson_CreatesAnEDTLogWithChildCounts()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Dissolved";

			var dissolvedPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson2.PER_FullName = "Dissolved2";

			var dissolvedPerson3 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson3.PER_FullName = "Dissolved3";

			var dissolvedOrgContact = Factory.NewWithValidTestData<OrgContact>();
			dissolvedOrgContact.OC_PER = dissolvedPerson1.PK;
			dissolvedOrgContact.UpdateFromPerson(dissolvedPerson1);

			var dissolvedGlbStaff = Factory.NewWithValidTestData<GlbStaff>();
			dissolvedGlbStaff.SetFromPerson(dissolvedPerson1);

			var dissolvedHRJobApplicant = Factory.New<HRJobApplicant>();
			dissolvedHRJobApplicant.SetFromPerson(dissolvedPerson1);

			var dissolvedOrgContact2 = Factory.NewWithValidTestData<OrgContact>();
			dissolvedOrgContact2.OC_PER = dissolvedPerson2.PK;
			dissolvedOrgContact2.UpdateFromPerson(dissolvedPerson2);

			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson2));
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson3));

			var retainedCollection = new PersonMergeBusinessObjectCollection();
			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));

			Factory.Save();

			var multiPersonMerger = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection);

			multiPersonMerger.MergeSelected().Wait();

			var query = new ZQuery(StmALogSchema.SL_Parent, retainedPerson.PK);
			query.AddToFilter(StmALogSchema.SL_Table, "GlbPerson");
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, "Merged");

			var loadedLogs = Factory.Load<StmALog>(query);

			AssertEquals("Each dissolved person should create a log", 3, loadedLogs.Length);

			CombineAssertions(() =>
			{
				loadedLogs.ForEach(log => AssertEquals("EDT", log.Event.SE_Code));
				loadedLogs.ForEach(log => AssertEquals("Edited a record", log.SL_EventDescription));
				AssertEquals($"Merged 'Dissolved' into this person and inherited 1 staff, 1 contact, 1 job applicant.", loadedLogs[0].SL_Reference);
				AssertEquals($"Merged 'Dissolved2' into this person and inherited 0 staff, 1 contact, 0 job applicant.", loadedLogs[1].SL_Reference);
				AssertEquals($"Merged 'Dissolved3' into this person.", loadedLogs[2].SL_Reference);
			});
		}

		#endregion

		public void TestMergeSelected_MergeWithErrors()
		{
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";
			retainedPerson.PER_WebAccessEnabled = false;

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Old Smith";
			dissolvedPerson1.PER_EmailAddress = "WebaccessTrue@Overrides.com";
			dissolvedPerson1.PER_WebAccessEnabled = true;
			dissolvedPerson1.PER_HomeAddress1 = "Enrichment Street";
			dissolvedPerson1.City = "Alexandria";

			Factory.Save();

			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));

			var retainedCollection = new PersonMergeBusinessObjectCollection();
			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));

			var multiPersonMerger = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection, new PersonMergeBusinessObjectFactoryLoaderForTest());

			multiPersonMerger.MergeSelected().Wait();

			retainedPerson.Reload();

			AssertEquals("Bob Smith", retainedPerson.PER_FullName);
			AssertEquals("WebaccessTrue@Overrides.com", retainedPerson.PER_EmailAddress);
			AssertEquals(true, retainedPerson.PER_WebAccessEnabled);
			AssertEquals("Enrichment Street", retainedPerson.PER_HomeAddress1);
			AssertEquals("Alexandria", retainedPerson.City);
			AssertEquals(ParticipantStatus.MergedWithErrors, dissolvedCollection[0].MergeStatus);
		}

		public void TestMergeSelected_ShouldNotSendNotificationEmailIfMergeUnsuccessful()
		{
			SystemDataRegistry.Instance.PersonMergeWithPasswordNotificationEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new NotificationEmailTemplate { EmailSubject = "nuts", EmailBody = "bolts" });
			var retainedPerson = Factory.NewWithValidTestData<GlbPerson>();
			retainedPerson.PER_FullName = "Bob Smith";
			retainedPerson.PER_WebAccessEnabled = false;

			var dissolvedPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			dissolvedPerson1.PER_FullName = "Old Smith";
			dissolvedPerson1.PER_EmailAddress = "WebaccessTrue@Overrides.com";
			dissolvedPerson1.PER_WebAccessEnabled = true;
			dissolvedPerson1.PER_PasswordHash = new ZBlob(new byte[] { 1, 2, 3, 4 });
			dissolvedPerson1.PER_PasswordHashIterations = 9239;
			dissolvedPerson1.PER_PasswordSalt = new ZBlob(new byte[] { 1, 2, 3, 4 });
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org1.Contacts.AddNew();
			contact1.OC_PER = dissolvedPerson1.PK;
			contact1.OC_Email = "blah@blah.com";

			Factory.Save();

			var dissolvedCollection = new PersonMergeBusinessObjectCollection();
			dissolvedCollection.Add(new PersonMergeBusinessObject(dissolvedPerson1));

			var retainedCollection = new PersonMergeBusinessObjectCollection();
			retainedCollection.Add(new PersonMergeBusinessObject(retainedPerson));

			var multiPersonMerger = new MultiPersonMergerForTest(retainedCollection, dissolvedCollection, new PersonMergeBusinessObjectFactoryLoaderForTest());

			multiPersonMerger.MergeSelected().Wait();

			retainedPerson.Reload();
			dissolvedPerson1.Reload();

			CombineAssertions("Preconditions", () =>
			{
				AssertEquals("Bob Smith", retainedPerson.PER_FullName);
				AssertEquals("WebaccessTrue@Overrides.com", retainedPerson.PER_EmailAddress);
				AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), retainedPerson.PER_PasswordHash);
				AssertEquals(9239, retainedPerson.PER_PasswordHashIterations);
				AssertEquals(new ZBlob(new byte[] { 1, 2, 3, 4 }), retainedPerson.PER_PasswordSalt);
				AssertEquals(ParticipantStatus.MergedWithErrors, dissolvedCollection[0].MergeStatus);
				AssertEquals(false, dissolvedCollection[0].IsDeleted);
				AssertEquals("Should still be associated with dissolving person", dissolvedPerson1.PK, contact1.OC_PER);
			});

			AssertEquals("Should not send email if merge was unsuccessful", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#region Implementation

		void AssertNoteContent(string[] expectedNoteTexts, ZGuid retainedPersonPK)
		{
			var query = new ZQuery(StmNoteSchema.ST_ParentID, retainedPersonPK);
			query.AddToFilter(StmNoteSchema.ST_Table, "GlbPerson");
			query.AddToFilter(StmNoteSchema.ST_Description, "Properties Discarded During Merge");

			var loadedNotes = Factory.Load<StmNote>(query);

			AssertContainsExactElementsInAnyOrder("Should have created a note for each merge with discarded properties", expectedNoteTexts, loadedNotes.Select(note => note.ST_NoteDataAsText.ToString()));
			AssertEquals("All notes should be saved in the database", true, loadedNotes.All(note => note.IsInDatabase));
		}

		public class MultiPersonMergerForTest : MultiPersonMerger
		{
			public MultiPersonMergerForTest(PersonMergeBusinessObjectCollection retainedCollection, PersonMergeBusinessObjectCollection dissolvedCollection, List<Tuple<IPersonMergeTransactionSaver, Exception>> dissolvedTuples)
				: base(retainedCollection, dissolvedCollection, new MultiPersonMergerCoreTest.MultiPersonMergerCoreForTest(dissolvedTuples.Select(t => t.Item1).GetEnumerator(), dissolvedTuples.Select(t => t.Item2).GetEnumerator()))
			{
			}

			public MultiPersonMergerForTest(PersonMergeBusinessObjectCollection retainedCollection, PersonMergeBusinessObjectCollection dissolvedCollection)
				: base(retainedCollection, dissolvedCollection)
			{
			}

			public MultiPersonMergerForTest(PersonMergeBusinessObjectCollection retainedCollection, PersonMergeBusinessObjectCollection dissolvedCollection, PersonMergeBusinessObjectFactoryLoader businessObjectFactoryLoaderProvider)
				: base(retainedCollection, dissolvedCollection, new MultiPersonMergerCore(), businessObjectFactoryLoaderProvider)
			{
			}

			protected override TaskScheduler Scheduler => new SynchronousTaskSchedulerForTest();

			public List<Exception> CaughtExceptionForTest => CaughtExceptions;
		}

		class PersonMergeBusinessObjectFactoryLoaderForTest : PersonMergeBusinessObjectFactoryLoader
		{
			public override void Reload(BusinessObjectFactory newFactory, GlbPerson retainedPerson, GlbPerson dissolvedPerson, BusinessObjectFactory originalFactory, PersonMergeMode mergeMode = PersonMergeMode.Single)
			{
				throw new Exception();
			}
		}

		internal static class MergerException
		{
			public static Exception ArgumentNullException = new ArgumentNullException();
			public static Exception InvalidOperationException = new InvalidOperationException();
			public static Exception ZSaveDataException = new ZSaveException(new ZDataException(null, null, null), new BusinessObjectFactory());
		}

		#endregion

		#endregion
	}
}
