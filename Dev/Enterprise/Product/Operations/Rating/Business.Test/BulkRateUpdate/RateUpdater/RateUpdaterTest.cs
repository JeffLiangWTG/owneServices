using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ContactNotifyModes = Enterprise.Core.Constants.ContactNotifyModes;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateUpdater))]
	internal sealed class RateUpdaterTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2009, 1, 1)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestLastRunDate()
		{
			Env.Registry.Rating.GRINotificationLastRunDate = new ZDateTime(2005, 1, 1, 11, 30, 0).ToDateTime();

			var updater = new RateUpdater();
			AssertEquals(new ZDateTime(2005, 1, 1, 11, 30, 0).ToLongTimeString(), updater.LastRunDate.ToLongTimeString());

			updater.LastRunDate = new ZDateTime(2000, 1, 1);
			AssertHasWarning(updater.LastRunDateInfo, string.Format("The date '{0}' is more than 1 year old.", updater.LastRunDate.ToString("dd-MMM-yyyy")));

			updater.LastRunDate = ZDateTime.Empty;
			AssertNoErrors(updater.LastRunDateInfo);

			updater.LastRunDate = ZDateTime.Invalid;
			AssertHasError(updater.LastRunDateInfo, "Enter a valid Last Time Updated.");

			updater.LastRunDate = ZDateTime.Now.AddMonths(-1);
			AssertNoErrors(updater.LastRunDateInfo);

			updater.UpdateRegistryLastRunDate();
			AssertEquals(ZDateTime.Today, Env.Registry.Rating.GRINotificationLastRunDate.Date);
		}

		public void TestRates()
		{
			var rate1 = Helper.NewClientRate(Helper.NewOrgHeader(1));
			rate1.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");

			var rate2 = Helper.NewClientRate(Helper.NewOrgHeader(1));
			rate2.AddRateEntry("ORG", "AIR", "AUSYD", "");

			var companyTariff1 = Helper.NewCompanyTariff();
			companyTariff1.AddRateEntry("ORG", "AIR", "AUSYD", "");

			Factory.Save();

			Env.Registry.Rating.GRINotificationLastRunDate = DateTime.MinValue;
			var updater = new RateUpdater();
			AssertEquals(0, updater.Rates.Count);

			updater.LastRunDate = ZDateTime.UtcNow.AddHours(-1).ToDateTime();
			AssertEquals(2, updater.Rates.Count);
		}

		public void TestSendNotifications()
		{
			var orgHeader = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(orgHeader);
			clientRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0].Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)100m;

			var contact = CreateOrgContact(orgHeader, "appointed@test.com", "appointed");
			var document = CreateOrgDocument(contact, ContactType.All.Code);

			contact = CreateOrgContact(orgHeader, "someone@test.com", "someone");
			document = CreateOrgDocument(contact, ContactType.Administration.Code);

			contact = CreateOrgContact(orgHeader, "appointed1@test.com", "appointed1");
			document = CreateOrgDocument(contact, ContactType.Sales.Code);
			document.OD_DeliverBy = ContactNotifyModes.Print;

			contact = CreateOrgContact(orgHeader, "faxer@test.com", "faxer");
			contact.OC_Fax = "555 1234 5678";
			document = CreateOrgDocument(contact, ContactType.Sales.Code);
			document.OD_DeliverBy = ContactNotifyModes.Fax;

			contact = CreateOrgContact(orgHeader, "", "no email address");
			contact.OC_Email = "";
			document = CreateOrgDocument(contact, ContactType.All.Code);

			contact = CreateOrgContact(orgHeader, "", "no fax number");
			contact.OC_Fax = "";
			document = CreateOrgDocument(contact, ContactType.All.Code);
			document.OD_DeliverBy = ContactNotifyModes.Fax;

			var staff = CreateStaffAssigment(orgHeader, "sales@test.com", "sales", StaffAssignmentRoles.Codes.SalesRep);
			staff = CreateStaffAssigment(orgHeader, "accountmanager@test.com", "accountmanager", StaffAssignmentRoles.Codes.AccountManager);
			staff = CreateStaffAssigment(orgHeader, "somemanager@test.com", "somemanager", StaffAssignmentRoles.Codes.Controller);

			var anotherHeader = Helper.NewOrgHeader();
			var anotherRate = Helper.NewClientRate(anotherHeader);
			anotherRate.AddRateEntry("AIR", "LSE", "AUSYD", "NZAKL").RateLines[0].Calculator[Calculator.Items.Operator.MIN] = (ZDecimal)100m;

			contact = CreateOrgContact(anotherHeader, "appointed@anothertest.com", "appointed");
			document = CreateOrgDocument(contact, ContactType.All.Code);
			staff = CreateStaffAssigment(anotherHeader, "sales@anothertest.com", "sales", StaffAssignmentRoles.Codes.SalesRep);

			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "Printer";

			Factory.Save();

			var updater = new RateUpdater();
			AssertEquals("precondition:", 0, updater.Rates.Count);

			updater.PrinterPK = printer.PK;
			updater.Rates.AddNew().ClientPK = clientRate.TH_OH;
			updater.Rates.AddNew().ClientPK = anotherRate.TH_OH;

			foreach (UpdateRate rate in updater.Rates)
			{
				rate.IncludeInUpdate = true;
			}

			var updaterResult = updater.SendNotifications();

			var query = new ZQuery(StmPrintJobSchema.SP_ParentTableName, "RatingHeader");
			query.AddToFilter(StmPrintJobSchema.SP_ParentGuid, new ZGuid[] { clientRate.PK, anotherRate.PK });

			var deliveredDocuments = Factory.Load<StmPrintJob>(query);

			CombineAssertions(delegate
			{
				AssertEquals("Updater executed ok", true, updaterResult);

				AssertContainsExactElementsInAnyOrder("Notification documents sent",
					new string[]
					{
						"Printer",
						"555 1234 5678",

						"appointed@test.com",
						"sales@test.com",
						"accountmanager@test.com",

						"sales@anothertest.com",
						"appointed@anothertest.com"
					},
					Array.ConvertAll(deliveredDocuments, (d) => d.SP_Destination.ToString()));
			});
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestGetPrintTask_RateIsNull_ThrowArgumentNullException()
		{
			var updater = new RateUpdater();

			DocumentPack documentPack;
			DeliveryInstructions delvieryInstructions;

			updater.GetPrintTask(null, out documentPack, out delvieryInstructions);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestGetPrintTask_RateWithoutClientRate_ThrowArgumentException()
		{
			var updater = new RateUpdater();

			DocumentPack documentPack;
			DeliveryInstructions delvieryInstructions;
			var rate = new UpdateRate(Factory);

			updater.GetPrintTask(rate, out documentPack, out delvieryInstructions);
		}

		#region Implementation

		OrgContact CreateOrgContact(OrgHeader organisation, ZString email, ZString name)
		{
			var contact = organisation.Contacts.AddNew();
			contact.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
			contact.OC_AttachmentType = OrgConstants.AttachmentType.PDF;
			contact.OC_Email = email;
			contact.OC_ContactName = name;

			return contact;
		}

		OrgDocument CreateOrgDocument(OrgContact contact, ZString documentGroup)
		{
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = documentGroup;
			document.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;

			return document;
		}

		GlbStaff CreateStaffAssigment(OrgHeader organisation, ZString email, ZString name, ZString role)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = email;
			staff.GS_FullName = name;
			organisation.StaffAssignments.SetStaffAssignment(role, staff.GS_Code, OrgStaffAssignmentsLookups.AllServices);

			return staff;
		}

		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		#endregion
	}
}
