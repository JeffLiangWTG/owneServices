using System;
using System.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;

namespace Enterprise.ReportTesting.MasterFiles
{
	[TemplateName("HRM Referred Staff Report")]
	public class HRMReferredStaffReportTemplateTest : TemplateTestCase
	{
		public void TestHRMReferredStaffReport()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var person1 = CreatePerson("Person Test 01");
			var person2 = CreatePerson("Person Test 02");
			var person3 = CreatePerson("Person Test 03");

			var person4 = CreatePerson("Person Test 04");
			var person5 = CreatePerson("Person Test 05");
			var person6 = CreatePerson("Person Test 06");

			var person7 = CreatePerson("Person Test 07");
			var person8 = CreatePerson("Person Test 08");

			_ = CreateStaff(person1, "teststaff01", "TS1", DateTime.Today.AddDays(-1), new DateTime(2029, 12, 12), "Test Staff Full Name 01", "AU");
			_ = CreateStaff(person2, "teststaff02", "TS2", DateTime.Today.AddDays(-2), null, "Test Staff Full Name 02", "US");
			_ = CreateStaff(person3, "teststaff03", "TS3", DateTime.Today.AddDays(-3), null, "Test Staff Full Name 03", "UK");

			_ = CreateStaff(person4, "teststaff04", "TS4", new DateTime(2001, 11, 11), new DateTime(2025, 01, 12), "Test Staff Full Name 04", "JP");
			_ = CreateStaff(person5, "teststaff05", "TS5", new DateTime(2002, 02, 12), null, "Test Staff Full Name 05", "CN");
			_ = CreateStaff(person6, "teststaff06", "TS6", new DateTime(2003, 10, 05), null, "Test Staff Full Name 06", "NZ");

			_ = CreateStaff(person7, "teststaff07", "TS7", DateTime.Today.AddDays(-7), null, "Test Staff Full Name 07", "FR");
			_ = CreateStaff(person8, "teststaff08", "TS8", new DateTime(2005, 07, 04), null, "Test Staff Full Name 08", "BE");

			var applicant1 = CreateApplicant(person1, "email11@gamil.com");
			var applicant2 = CreateApplicant(person2, "email22@gamil.com");
			var applicant3 = CreateApplicant(person3, "email33@gamil.com");

			var applicant7 = CreateApplicant(person7, "email77@gamil.com");

			_ = CreateApplication(applicant1, "111", "STF", person4);
			_ = CreateApplication(applicant2, "222", "STF", person5);
			_ = CreateApplication(applicant3, "333", "STF", person6);

			_ = CreateApplication(applicant7, "777", "AGT", person8);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0];

				AssertEquals(DateTime.Today.AddDays(-2), DateTime.FromOADate((double)sheetContent[3, 2]));
				AssertEquals(string.Empty, sheetContent[3, 3]);
				AssertEquals("TS2", sheetContent[3, 4].ToString());
				AssertEquals("Test Staff Full Name 02", sheetContent[3, 5].ToString());
				AssertEquals("TS5", sheetContent[3, 6].ToString());
				AssertEquals("Test Staff Full Name 05", sheetContent[3, 7].ToString());
				AssertEquals("China", sheetContent[3, 8].ToString());

				AssertEquals(DateTime.Today.AddDays(-3), DateTime.FromOADate((double)sheetContent[4, 2]));
				AssertEquals(string.Empty, sheetContent[4, 3]);
				AssertEquals("TS3", sheetContent[4, 4].ToString());
				AssertEquals("Test Staff Full Name 03", sheetContent[4, 5].ToString());
				AssertEquals("TS6", sheetContent[4, 6].ToString());
				AssertEquals("Test Staff Full Name 06", sheetContent[4, 7].ToString());
				AssertEquals("New Zealand", sheetContent[4, 8].ToString());

				AssertEquals(DateTime.Today.AddDays(-1), DateTime.FromOADate((double)sheetContent[5, 2]));
				AssertEquals(new DateTime(2029, 12, 12), DateTime.FromOADate((double)sheetContent[5, 3]));
				AssertEquals("TS1", sheetContent[5, 4].ToString());
				AssertEquals("Test Staff Full Name 01", sheetContent[5, 5].ToString());
				AssertEquals("TS4", sheetContent[5, 6].ToString());
				AssertEquals("Test Staff Full Name 04", sheetContent[5, 7].ToString());
				AssertEquals("Japan", sheetContent[5, 8].ToString());
			}
		}

		public void TestHRMReferredStaffReport_DoNotShowNonSTFTypeApplications()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var person1 = CreatePerson("Person Test 01");
			var person2 = CreatePerson("Person Test 02");
			var person3 = CreatePerson("Person Test 03");

			var person4 = CreatePerson("Person Test 04");
			var person5 = CreatePerson("Person Test 05");
			var person6 = CreatePerson("Person Test 06");

			_ = CreateStaff(person1, "teststaff01", "TS1", DateTime.Today.AddDays(-1), new DateTime(2029, 12, 12), "Test Staff Full Name 01", "AU");
			_ = CreateStaff(person2, "teststaff02", "TS2", DateTime.Today.AddDays(-2), null, "Test Staff Full Name 02", "US");
			_ = CreateStaff(person3, "teststaff03", "TS3", DateTime.Today.AddDays(-3), null, "Test Staff Full Name 03", "UK");

			_ = CreateStaff(person4, "teststaff04", "TS4", new DateTime(2001, 11, 11), new DateTime(2025, 01, 12), "Test Staff Full Name 04", "JP");
			_ = CreateStaff(person5, "teststaff05", "TS5", new DateTime(2002, 02, 12), null, "Test Staff Full Name 05", "CN");
			_ = CreateStaff(person6, "teststaff06", "TS6", new DateTime(2003, 10, 05), null, "Test Staff Full Name 06", "NZ");

			var applicant1 = CreateApplicant(person1, "email11@gamil.com");
			var applicant2 = CreateApplicant(person2, "email22@gamil.com");
			var applicant3 = CreateApplicant(person3, "email33@gamil.com");

			_ = CreateApplication(applicant1, "111", "WEB", person4);
			_ = CreateApplication(applicant2, "222", "AGT", person5);
			_ = CreateApplication(applicant3, "333", "MAN", person6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0].ToString();

				AssertNotContains("TS1", sheetContent);
				AssertNotContains("TS2", sheetContent);
				AssertNotContains("TS3", sheetContent);
			}
		}

		public void TestHRMReferredStaffReport_DoNotShowStaffsEmployedMoreThanOneMonth()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var person1 = CreatePerson("Person Test 01");
			var person2 = CreatePerson("Person Test 02");

			var person3 = CreatePerson("Person Test 03");
			var person4 = CreatePerson("Person Test 04");

			_ = CreateStaff(person1, "teststaff01", "TS1", DateTime.Today.AddMonths(-1), null, "Test Staff Full Name 01", "AU");
			_ = CreateStaff(person2, "teststaff02", "TS2", DateTime.Today.AddMonths(-1).AddDays(1), null, "Test Staff Full Name 02", "UK");

			_ = CreateStaff(person3, "teststaff03", "TS3", new DateTime(2012, 03, 05), null, "Test Staff Full Name 03", "US");
			_ = CreateStaff(person4, "teststaff04", "TS4", new DateTime(2001, 11, 11), null, "Test Staff Full Name 04", "JP");

			var applicant1 = CreateApplicant(person1, "email11@gamil.com");
			var applicant2 = CreateApplicant(person2, "email22@gamil.com");

			_ = CreateApplication(applicant1, "111", "STF", person3);
			_ = CreateApplication(applicant2, "222", "STF", person4);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0].ToString();

				AssertNotContains("TS1", sheetContent);
				AssertContains("TS2", sheetContent);
			}
		}

		public void TestHRMReferredStaffReport_DoNotShowStaffsNotProbationedOrProbationedMoreThanOneMonth()
		{
			PrepareReportForRender();
			FillReportWithDefaultValues();

			var person1 = CreatePerson("Person Test 01");
			var person2 = CreatePerson("Person Test 02");
			var person3 = CreatePerson("Person Test 03");

			var person4 = CreatePerson("Person Test 04");
			var person5 = CreatePerson("Person Test 05");
			var person6 = CreatePerson("Person Test 06");

			_ = CreateStaff(person1, "teststaff01", "TS1", DateTime.Today.AddYears(-1), null, "Test Staff Full Name 01", "AU");
			_ = CreateStaff(person2, "teststaff02", "TS2", DateTime.Today.AddYears(-2), DateTime.Today.AddMonths(-1), "Test Staff Full Name 02", "US");
			_ = CreateStaff(person3, "teststaff03", "TS3", DateTime.Today.AddYears(-3), DateTime.Today.AddMonths(-1).AddDays(1), "Test Staff Full Name 03", "UK");

			_ = CreateStaff(person4, "teststaff04", "TS4", new DateTime(2001, 11, 11), new DateTime(2025, 01, 12), "Test Staff Full Name 04", "JP");
			_ = CreateStaff(person5, "teststaff05", "TS5", new DateTime(2002, 02, 12), null, "Test Staff Full Name 05", "CN");
			_ = CreateStaff(person6, "teststaff06", "TS6", new DateTime(2003, 10, 05), null, "Test Staff Full Name 06", "NZ");

			var applicant1 = CreateApplicant(person1, "email11@gamil.com");
			var applicant2 = CreateApplicant(person2, "email22@gamil.com");
			var applicant3 = CreateApplicant(person3, "email33@gamil.com");

			_ = CreateApplication(applicant1, "111", "STF", person4);
			_ = CreateApplication(applicant2, "222", "STF", person5);
			_ = CreateApplication(applicant3, "333", "STF", person6);

			using (var stream = new MemoryStream())
			using (var excelInterface = new ExcelInterface())
			{
				Report.Save(stream);
				stream.Position = 0;
				excelInterface.LoadExcelFile(stream);

				var sheetContent = excelInterface.WorkSheets[0].ToString();

				AssertNotContains("TS1", sheetContent);
				AssertNotContains("TS2", sheetContent);
				AssertContains("TS3", sheetContent);
			}
		}

		HRJobApplication CreateApplication(HRJobApplicant applicant, string applicationNumber, string sourceType, GlbPerson referringPerson)
		{
			var application = Factory.NewWithValidTestData<HRJobApplication>();
			application.HP_HA = applicant.PK;
			application.HP_ApplicationNumber = applicationNumber;
			application.HP_SourceType = sourceType;
			application.HP_PER_ReferringPerson = referringPerson.PK;

			Factory.Save();

			return application;
		}

		HRJobApplicant CreateApplicant(GlbPerson person, string emailAddress)
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_PER = person.PK;
			applicant.HA_EmailAddress = emailAddress;

			Factory.Save();

			return applicant;
		}

		GlbStaff CreateStaff(GlbPerson person, string loginName, string code, DateTime? employmentDate, DateTime? probationEndDate, string fullName, string countryCode)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = loginName;
			staff.GS_PER = person.PK;
			staff.GS_Code = code;

			if (employmentDate != null)
			{
				staff.GS_EmploymentDate = new ZDateTime(employmentDate);
			}
			if (probationEndDate != null)
			{
				staff.GS_ProbationEndDate = new ZDate(probationEndDate);
			}

			staff.GS_FullName = fullName;
			staff.GS_RN_NKCountryCode = countryCode;

			Factory.Save();

			return staff;
		}

		GlbPerson CreatePerson(string fullName)
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = fullName;

			Factory.Save();

			return person;
		}
	}
}
