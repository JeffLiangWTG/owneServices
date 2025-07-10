using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Services.Calendar;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using StatusList = Enterprise.MasterFiles.Business.CollectionNoteStatusList;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCollectionNote))]
	sealed class OrgCollectionNoteTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesOrgCollectionNote()
		{
			OrgCollectionNote note = Factory.New<OrgCollectionNote>();

			var localList = new List<string>
				{
					nameof(note.PN_AmountOverdueAtCallTime),
					nameof(note.PN_TotalOutstandingValueAtCallTime)
				};

			var tester = new DecimalPlacesAttributeTester(note);
			tester.CheckLocalCurrency(localList, nameof(note.DecimalPlaces));
		}

		public void TestSetDefaultOC()
		{
			AssertEquals(ZGuid.Empty, CollectionNote.PN_OC);
			OrgContact contact = TestHelper.GetNewContact(Organisation, "John Locke", "666");
			CollectionNote.PN_OC = contact.PK;
			Factory.Save();

			OrgCollectionNote secondNote = Factory.New<OrgCollectionNote>();
			secondNote.Header = Organisation;
			secondNote.PN_OC = ZGuid.Empty;
			secondNote.SetDefaultContact();
			AssertEquals("Contact person should be defaulted", contact.PK, secondNote.PN_OC);
		}

		public void TestDecimalPlaces()
		{
			AssertEquals("LocalCurrency Decimal Places", GlbCompany.CurrentCompany.LocalCurrency.Decimals, CollectionNote.DecimalPlaces);
			RefCurrency localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			int saveDecimals = localCurrency.RX_SubUnitRatio;
			try
			{
				localCurrency.RX_SubUnitRatio = 10;
				AssertEquals("One Decimal Place", 1, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
				AssertEquals("One Decimal Place", 1, CollectionNote.DecimalPlaces);
			}
			finally
			{
				localCurrency.RX_SubUnitRatio = saveDecimals;
			}
		}

		public void TestNoAuditLogs()
		{
			var orgCollectionNote = Factory.NewWithValidTestData<OrgCollectionNote>();
			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, orgCollectionNote.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				orgCollectionNote.PN_CallDetailNote = "TEST LOG";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				orgCollectionNote.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		#region Calendar Reminders

		[TestDate(2006, 6, 6)]  // test uses daylight savings
		[ExpectNoExceptions()]
		public void TestCalendarReminder()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Some Special Organisation";
			org.OH_RL_NKClosestPort = "INBOM";
			org.OH_Code = "SOMEORG1";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Igor Kadulenkov";
			contact.OC_Email = "igor.kadulenkov@edi.com.au";
			contact.OC_Fax = "+61 2 9025 1199";
			contact.OC_Phone = "+61 2 9025 1180";

			org.MainAddress.OA_Address1 = "18 Henricks Avenue";
			org.MainAddress.OA_Address2 = "My House";
			org.MainAddress.OA_City = "Newington";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2127";
			org.MainAddress.OA_Email = "mainorg@example.com";
			org.MainAddress.OA_Phone = "+61 2 9911 1199";
			org.MainAddress.OA_Fax = "+61 2 9911 9911";

			var collectionNote = org.CollectionNotes.AddNew();
			collectionNote.PN_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			collectionNote.PN_SystemCreateTimeUtc = new ZDateTime(2006, 3, 5);
			collectionNote.PN_CallBackDate = new ZDateTime(2006, 3, 10, 9, 0, 0);
			collectionNote.PN_CallDisposition = CollectionNoteDispositionList.Codes.CheckIsInMail;
			collectionNote.PN_CallDetailNote = "Really good call";
			collectionNote.PN_OC = contact.PK;

			var collectionNote2 = org.CollectionNotes.AddNew();
			collectionNote2.PN_SystemCreateTimeUtc = new ZDateTime(2006, 3, 5);
			collectionNote2.PN_CallBackDate = new ZDateTime(2006, 3, 10, 9, 0, 0);
			collectionNote2.PN_CallDisposition = CollectionNoteDispositionList.Codes.MissingInvoice;
			collectionNote2.PN_CallDetailNote = "Really good call";
			collectionNote2.PN_OC = contact.PK;
			collectionNote2.PN_Status = "OPE";

			var expected = @"Follow up collection call due for client Some Special Organisation (SOMEORG1)

CONTACT DETAILS:
Name: Igor Kadulenkov
Email: igor.kadulenkov@edi.com.au
Fax: +61 2 9025 1199
Phone: +61 2 9025 1180

18 Henricks Avenue
My House
Newington NSW 2127
Email: mainorg@example.com
Fax: +61 2 9911 9911
Phone: +61 2 9911 1199

CALL DETAILS:
Call Status: Working
Call Disposition: Cheque is in Mail
Amount Overdue at Call Time: 0.00
Total Outstanding Value at Call Time: 0.00

INTERNAL CALL NOTES:
Really good call";

			var expectedHtmlBody = @"<HTML><HEAD><TITLE></TITLE></HEAD><BODY>Follow up collection call due for client Some Special Organisation (SOMEORG1)

CONTACT DETAILS:
Name: Igor Kadulenkov
Email: igor.kadulenkov@edi.com.au
Fax: +61 2 9025 1199
Phone: +61 2 9025 1180

18 Henricks Avenue
My House
Newington NSW 2127
Email: mainorg@example.com
Fax: +61 2 9911 9911
Phone: +61 2 9911 1199

CALL DETAILS:
Call Status: Working
Call Disposition: Cheque is in Mail
Amount Overdue at Call Time: 0.00
Total Outstanding Value at Call Time: 0.00

INTERNAL CALL NOTES:
Really good call
</BODY></HTML>";

			AssertEquals("Calendar reminders created", true, collectionNote.ShouldCreateReminder);
			AssertContains("Calendar reminder created with correct body", expected, collectionNote.NextCallDueReminder.Body);
			Assert("Calendar reminder has html body", collectionNote.NextCallDueReminder.HasHtmlBody);
			AssertContains("Calendar reminder created with correct html body", expectedHtmlBody, collectionNote.NextCallDueReminder.HtmlBody);
			AssertContains("Calendar reminder created with correct ID", collectionNote.PK.ToString() + "FollowUpCall", collectionNote.NextCallDueReminder.ID);
			Assert("should be false as there's no sales rep", !collectionNote2.ShouldCreateReminder);

			collectionNote.PN_CallDisposition = CollectionNoteDispositionList.Codes.PaymentReceived;
			Assert("CallDisposition code should correspond with the description", expected != collectionNote.NextCallDueReminder.Body);

			Factory.Save();

			var loadedCollectionNote2 = new BusinessObjectFactory().Load<OrgCollectionNote>(collectionNote2.PK);
			AssertEquals("Collection note should have valid call back time", true, loadedCollectionNote2.PN_CallBackDate.IsValid);
			loadedCollectionNote2.PN_CallBackDate = ZDateTime.Empty;
			AssertEquals("Calendar reminders created", true, loadedCollectionNote2.ShouldCreateReminder);
			var reminder = loadedCollectionNote2.NextCallDueReminder;
			AssertEquals("Calendar reminder created with correct type", ReminderType.Cancellation, reminder.ReminderType);
			AssertEquals("Calendar reminder created with correct time", new ZDateTime(2006, 3, 10, 9, 0, 0), reminder.LocalDateFrom);

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.ShouldCollectionCallCreateFollowUpAppointments).Returns(false);
			using (ObjectFactory.Substitute(mock.Object))
			{
				AssertEquals("NO Calendar reminders should be created", false, collectionNote.ShouldCreateReminder);
			}
		}

		[TestDate(2019, 3, 25)]
		public void TestCalendarReminderBodyWithLongCallDetailNote()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Some Special Organisation";
			org.OH_RL_NKClosestPort = "INBOM";
			org.OH_Code = "SOMEORG1";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Igor Kadulenkov";
			contact.OC_Email = "igor.kadulenkov@edi.com.au";
			contact.OC_Fax = "+61 2 9025 1199";
			contact.OC_Phone = "+61 2 9025 1180";

			org.MainAddress.OA_Address1 = "18 Henricks Avenue";
			org.MainAddress.OA_Address2 = "My House";
			org.MainAddress.OA_City = "Newington";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2127";
			org.MainAddress.OA_Email = "mainorg@example.com";
			org.MainAddress.OA_Phone = "+61 2 9911 1199";
			org.MainAddress.OA_Fax = "+61 2 9911 9911";

			var collectionNote = org.CollectionNotes.AddNew();
			collectionNote.PN_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
			collectionNote.PN_SystemCreateTimeUtc = new ZDateTime(2019, 3, 25);
			collectionNote.PN_CallBackDate = new ZDateTime(2019, 3, 26);
			collectionNote.PN_CallDisposition = CollectionNoteDispositionList.Codes.CheckIsInMail;

			string note = "This is a short note";
			collectionNote.PN_CallDetailNote = note;

			var expectedBody = @"Follow up collection call due for client Some Special Organisation (SOMEORG1)

CONTACT DETAILS:
18 Henricks Avenue
My House
Newington NSW 2127
Email: mainorg@example.com
Fax: +61 2 9911 9911
Phone: +61 2 9911 1199

CALL DETAILS:
Call Status: Working
Call Disposition: Cheque is in Mail
Amount Overdue at Call Time: 0.00
Total Outstanding Value at Call Time: 0.00

INTERNAL CALL NOTES:
{0}
";

			var expectedHtmlBody = @"<HTML><HEAD><TITLE></TITLE></HEAD><BODY>Follow up collection call due for client Some Special Organisation (SOMEORG1)

CONTACT DETAILS:
18 Henricks Avenue
My House
Newington NSW 2127
Email: mainorg@example.com
Fax: +61 2 9911 9911
Phone: +61 2 9911 1199

CALL DETAILS:
Call Status: Working
Call Disposition: Cheque is in Mail
Amount Overdue at Call Time: 0.00
Total Outstanding Value at Call Time: 0.00

INTERNAL CALL NOTES:
{0}
</BODY></HTML>";
			AssertEquals(string.Format(expectedBody, note), collectionNote.NextCallDueReminder.Body);
			AssertEquals(string.Format(expectedHtmlBody, note), collectionNote.NextCallDueReminder.HtmlBody);

			note = @"This is a really long long long long long note, the length will be 1000.
12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			Assert(note.Length == 1000);
			collectionNote.PN_CallDetailNote = note;
			AssertEquals(string.Format(expectedBody, note), collectionNote.NextCallDueReminder.Body);
			AssertEquals(string.Format(expectedHtmlBody, note), collectionNote.NextCallDueReminder.HtmlBody);

			note = @"This is a really long long long long long note, the length will be greater than 1000.
12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			Assert(note.Length > 1000);
			collectionNote.PN_CallDetailNote = note;
			AssertNotEquals("body will not contain the whole note if length is greater than 1000", string.Format(expectedBody, note), collectionNote.NextCallDueReminder.Body);
			AssertNotEquals("html body will not contain the whole note if length is greater than 1000", string.Format(expectedHtmlBody, note), collectionNote.NextCallDueReminder.HtmlBody);
			AssertEquals("body will contain the first 1000 characters of note if length is greater than 1000", string.Format(expectedBody, note.Substring(0, 1000)), collectionNote.NextCallDueReminder.Body);
			AssertEquals("html body will contain the first 1000 characters of note if length is greater than 1000", string.Format(expectedHtmlBody, note.Substring(0, 1000)), collectionNote.NextCallDueReminder.HtmlBody);
		}

		#endregion

		#region Related Business Objects

		public void TestGet_HeaderFromPN_OB()
		{
			OrgCollectionNote newCollectionNote = Organisation.CollectionNotes.AddNew();
			Factory.Save();

			newCollectionNote.Header = null;
			AssertEquals("Header should be loaded again by using the PN_OB value.", Organisation, newCollectionNote.Header);
		}

		[TestDate(2016, 4, 20, 9, 0, 0)]
		public void TestSet_Header()
		{
			TestHelper.PostTransaction("AR", "REC", "1000014", 100m, 50m, 200m, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.Date, Department.PK.ToGuid());
			TestHelper.PostTransaction("AP", "REC", "1000015", 100, 50, 200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now, Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "INV", "1000016", 100, 50, -200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-1), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "INV", "1000017", 100, 50, -200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddMinutes(5), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "INV", "1000018", 100, 50, -200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), true, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-1), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "CRD", "1000019", 100, 50, 200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-5), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "ADJ", "1000020", 100, 50, 200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-1), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "JNL", "1000021", 100, 50, 200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-1), Department.PK.ToGuid());

			OrgContact contact = TestHelper.GetNewContact(Organisation, "John Locke", "666");
			CollectionNote.PN_OC = contact.PK;
			Factory.Save();

			OrgCollectionNote secondNote = Organisation.CollectionNotes.AddNew();
			Factory.Save();

			AssertEquals(Organisation, secondNote.Header);
			AssertEquals(200m, secondNote.PN_TotalOutstandingValueAtCallTime);
			AssertEquals(400m, secondNote.PN_AmountOverdueAtCallTime);
			AssertEquals("Contact person should be defaulted", contact.PK, secondNote.PN_OC);
			AssertEquals("PN_OB should be reloaded", Organisation.CompanyData.PK, secondNote.PN_OB);

			TestHelper.PostTransaction("AR", "INV", "1000022", 100, 50, -200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-1), Department.PK.ToGuid());
			secondNote.Header = Organisation;
			AssertEquals("Total Outstanding amount should not be changed, because the collection note is already in database", 200m, secondNote.PN_TotalOutstandingValueAtCallTime);
			AssertEquals("Total Outstanding amount should not be changed, because the collection note is already in database", 400m, secondNote.PN_AmountOverdueAtCallTime);
		}

		#endregion

		#region Default Values And Loading

		public void TestSetDefaultValues()
		{
			AssertEquals("PN_SystemCreateTimeUtc", Env.Time.CurrentLocalDateTime.Date, CollectionNote.PN_SystemCreateTimeUtc.Date);
			AssertEquals("PN_SystemCreateUser", ZString.Empty, CollectionNote.PN_SystemCreateUser);
			OrgCollectionNote newCollectionNote;
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.CollectionCallFollowUpDays).Returns(0);
			using (ObjectFactory.Substitute(mock.Object))
			{
				newCollectionNote = Organisation.CollectionNotes.AddNew();
				AssertEquals("PN_CallBackDate", ZDateTime.Empty, newCollectionNote.PN_CallBackDate);
			}

			mock = new Mock<IAccounting>();
			mock.Setup(m => m.CollectionCallFollowUpDays).Returns(10);
			using (ObjectFactory.Substitute(mock.Object))
			{
				newCollectionNote = Organisation.CollectionNotes.AddNew();
				AssertEquals("CallBackDate should be defaulted.", ZDateTime.Today.AddDays(10).Date, newCollectionNote.PN_CallBackDate.Date);
			}
		}

		#endregion

		#region Properties

		public void TestDependentContacts()
		{
			OrgContact contact = Organisation.Contacts.AddNew();
			Assert("DependentContacts should be referenced to Header.Contacts", CollectionNote.Lookups.DependentContacts[0] == contact);

			CollectionNote.Header = null;
			AssertNotNull("DependentContacts shouldn't be null", CollectionNote.Lookups.DependentContacts);
		}

		public void TestDefaultPN_OC()
		{
			Organisation.CollectionNotes.RemoveAll();
			OrgCollectionNote collectionNote1 = Organisation.CollectionNotes.AddNew();
			AssertEquals("Contact should be null", ZGuid.Empty, collectionNote1.PN_OC);

			collectionNote1.PN_OC = ZGuid.NewZGuid();
			OrgCollectionNote collectionNote2 = Organisation.CollectionNotes.AddNew();
			AssertEquals("Contact should be defaulted", collectionNote1.PN_OC, collectionNote2.PN_OC);
		}

		public void TestOutstandingAmount()
		{
			OrgContact contact = TestHelper.GetNewContact(Organisation, "John Locke", "666");
			CollectionNote.PN_OC = contact.PK;
			Factory.Save();

			TestHelper.PostTransaction("AR", "REC", "1000014", 100m, 50m, 200m, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.Date, Department.PK.ToGuid());
			TestHelper.PostTransaction("AP", "REC", "1000015", 100, 50, 200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now, Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "INV", "1000016", 100, 50, -200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-1), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "INV", "1000017", 100, 50, -200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddMinutes(5), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "INV", "1000018", 100, 50, -200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), true, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-1), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "CRD", "1000019", 100, 50, 200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-5), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "ADJ", "1000020", 100, 50, 200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-1), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "JNL", "1000021", 100, 50, 200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-1), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "INB", "1000022", 100, 50, -200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "", Organisation.PK.ToGuid(), ZDateTime.Now.Date, Department.PK.ToGuid());

			GlbCompany currentCompany2 = TestHelper.GetNewGlbCompany("Company For Test");
			AccGLHeader testGLHeader2 = TestHelper.GetNewGLHeader("XXXX.XX.XX", "1111.11.12", "New Gl Header");
			RefCurrency testCurrency2 = TestHelper.USD;
			AccBankAccount testBank2 = TestHelper.GetNewBankAccount("TEZ", "12346", "1233", testGLHeader2.PK, testCurrency2.RX_Code, currentCompany2.PK);
			GlbDepartment department2 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbBranch branch2 = TestHelper.GetNewBranch(currentCompany2.PK);
			OrgDebtorGroup debtorGroup2 = TestHelper.GetNewOrgDebtorGroup();
			OrgHeader header2 = TestHelper.GetNewOrganisationWithCompanyData(false, currentCompany2.PK, branch2, debtorGroup2, "AAA", "BBB", "CCC", ZBool.True, "CODE");
			Factory.Save();

			TestHelper.PostTransaction("AR", "REC", "1000022", 100m, 50m, 200m, branch2.PK.ToGuid(), ZDateTime.Now, testBank2.PK.ToGuid(), false, testCurrency2.RX_Code, "CSH", header2.PK.ToGuid(), ZDateTime.Now.Date, department2.PK.ToGuid());

			OrgCollectionNote newCollectionNote = Organisation.CollectionNotes.AddNew();

			AssertEquals(200m, newCollectionNote.PN_TotalOutstandingValueAtCallTime);
		}

		public void TestOverdueAmount()
		{
			OrgContact contact = TestHelper.GetNewContact(Organisation, "John Locke", "666");
			CollectionNote.PN_OC = contact.PK;
			Factory.Save();

			TestHelper.PostTransaction("AR", "REC", "1000014", 100m, 50m, 200m, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.Date, Department.PK.ToGuid());
			TestHelper.PostTransaction("AP", "REC", "1000015", 100, 50, 200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now, Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "INV", "1000016", 100, 50, -200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-1), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "INV", "1000017", 100, 50, -200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddMinutes(5), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "INV", "1000018", 100, 50, -200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), true, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-1), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "CRD", "1000019", 100, 50, 200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-5), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "ADJ", "1000020", 100, 50, 200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-1), Department.PK.ToGuid());
			TestHelper.PostTransaction("AR", "JNL", "1000021", 100, 50, 200, Branch.PK.ToGuid(), ZDateTime.Now, TestBank.PK.ToGuid(), false, TestCurrency.RX_Code, "CSH", Organisation.PK.ToGuid(), ZDateTime.Now.AddDays(-1), Department.PK.ToGuid());

			GlbCompany currentCompany2 = TestHelper.GetNewGlbCompany("Company For Test");
			AccGLHeader testGLHeader2 = TestHelper.GetNewGLHeader("XXXX.XX.XX", "1111.11.12", "New Gl Header");
			RefCurrency testCurrency2 = TestHelper.USD;
			AccBankAccount testBank2 = TestHelper.GetNewBankAccount("TEZ", "12346", "1233", testGLHeader2.PK, testCurrency2.RX_Code, currentCompany2.PK);
			GlbDepartment department2 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbBranch branch2 = TestHelper.GetNewBranch(currentCompany2.PK);
			OrgDebtorGroup debtorGroup2 = TestHelper.GetNewOrgDebtorGroup();
			OrgHeader header2 = TestHelper.GetNewOrganisationWithCompanyData(false, currentCompany2.PK, branch2, debtorGroup2, "AAA", "BBB", "CCC", ZBool.True, "CODE");
			Factory.Save();

			TestHelper.PostTransaction("AR", "INV", "1000022", 100m, 50m, 200m, branch2.PK.ToGuid(), ZDateTime.Now, testBank2.PK.ToGuid(), false, testCurrency2.RX_Code, "CSH", header2.PK.ToGuid(), ZDateTime.Now.AddDays(-1), department2.PK.ToGuid());

			OrgCollectionNote newCollectionNote = Organisation.CollectionNotes.AddNew();

			AssertEquals(400m, newCollectionNote.PN_AmountOverdueAtCallTime);
		}

		#endregion

		#region ReadOnly Functionality

		public void TestReadOnlyFields()
		{
			CollectionNote.PN_Status = StatusList.Codes.Open;
			AssertFieldsAreReadOnly(false, CollectionNote);
			CollectionNote.PN_Status = StatusList.Codes.Working;
			AssertFieldsAreReadOnly(false, CollectionNote);
			CollectionNote.PN_Status = StatusList.Codes.Closed;
			AssertFieldsAreReadOnly(false, CollectionNote);

			Factory.Save();

			CollectionNote.PN_Status = StatusList.Codes.Open;
			AssertFieldsAreReadOnly(false, CollectionNote);
			CollectionNote.PN_Status = StatusList.Codes.Working;
			AssertFieldsAreReadOnly(false, CollectionNote);
			CollectionNote.PN_Status = StatusList.Codes.Closed;
			AssertFieldsAreReadOnly(true, CollectionNote);
		}

		void AssertFieldsAreReadOnly(bool readOnly, OrgCollectionNote note)
		{
			AssertPropertyInfoIsReadOnly(readOnly, note.PN_SystemCreateUserInfo);
			AssertPropertyInfoIsReadOnly(false, note.PN_StatusInfo);
			AssertPropertyInfoIsReadOnly(readOnly, note.PN_CallDispositionInfo);
			AssertPropertyInfoIsReadOnly(readOnly, note.PN_SystemCreateTimeUtcInfo);
			AssertPropertyInfoIsReadOnly(readOnly, note.PN_CallBackDateInfo);
			AssertPropertyInfoIsReadOnly(readOnly, note.PN_CallDetailNoteInfo);
			AssertPropertyInfoIsReadOnly(readOnly, note.PN_OCInfo);
			AssertPropertyInfoIsReadOnly(true, note.PN_AmountOverdueAtCallTimeInfo);
			AssertPropertyInfoIsReadOnly(true, note.PN_TotalOutstandingValueAtCallTimeInfo);
		}

		void AssertPropertyInfoIsReadOnly(bool expected, ZPropertyInfo info)
		{
			AssertEquals(info.Name + " ReadOnly", expected, info.ReadOnly);
		}

		#endregion

		#region Implementation

		OrgCollectionNote fCollectionNote;
		OrgCollectionNote CollectionNote
		{
			get
			{
				if (fCollectionNote == null)
				{
					fCollectionNote = Organisation.CollectionNotes.AddNew();
				}
				return fCollectionNote;
			}
		}

		OrgHeader fOrganisation;
		OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = TestHelper.GetNewOrganisationWithCompanyData(false, CurrentCompany.PK, Branch, DebtorGroup, "AAA", "BBB", "CCC", ZBool.True, "Code2");
					Factory.Save();
				}
				return fOrganisation;
			}
		}

		MasterFilesTestHelper fTestHelper;
		MasterFilesTestHelper TestHelper
		{
			get
			{
				if (fTestHelper == null)
				{
					fTestHelper = new MasterFilesTestHelper(Factory);
				}
				return fTestHelper;
			}
		}

		GlbBranch Branch;
		OrgDebtorGroup DebtorGroup;
		GlbCompany CurrentCompany;

		AccGLHeader TestGLHeader;
		RefCurrency TestCurrency;
		AccBankAccount TestBank;
		GlbDepartment Department;

		IDisposable userContextChange;

		protected override void SetUp()
		{
			CurrentCompany = TestHelper.GetNewGlbCompany("Company For Test");
			Branch = TestHelper.GetNewBranch(CurrentCompany.PK);
			Factory.Save();
			userContextChange = Env.SetTemporaryUserContext(Env.CurrentUser.PK, Branch.PK.ToGuid(), Env.CurrentDepartment.PK);

			base.SetUp();

			TestGLHeader = TestHelper.GetNewGLHeader("XXXX.XX.XX", "1111.11.11", "New Gl Header");
			TestCurrency = TestHelper.USD;
			TestBank = TestHelper.GetNewBankAccount("TES", "12345", "1234", TestGLHeader.PK, TestCurrency.RX_Code, CurrentCompany.PK);
			Department = Factory.NewWithValidTestData<GlbDepartment>();

			DebtorGroup = TestHelper.GetNewOrgDebtorGroup();
			Factory.Save();
		}

		protected override void TearDown()
		{
			userContextChange.Dispose();
			base.TearDown();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			return org.CollectionNotes.AddNew();
		}

		#endregion
	}
}
