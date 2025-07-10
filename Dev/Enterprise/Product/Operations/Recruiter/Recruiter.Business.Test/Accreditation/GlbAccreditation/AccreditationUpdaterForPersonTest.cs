using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(AccreditationUpdaterForPerson))]
	sealed class AccreditationUpdaterForPersonTest : AccreditationUpdaterTest
	{
		public void TestRun_Person()
		{
			ErrorReporter.Clear();
			var accred = PrepareData(out var applicant);

			var updater = GetUpdater() as AccreditationUpdaterForPerson;
			updater.SetPersons(new IGlbPerson[] { applicant.Person });

			updater.Run();

			AssertEquals(0, applicant.Certificates.Count);
			Assert(string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
		}

		public void TestRun_MultiplePersons()
		{
			var accred = PrepareData(out var applicant1, out var applicant2);

			var updater = GetUpdater() as AccreditationUpdaterForPerson;
			updater.SetPersons(new IGlbPerson[] { applicant1.Person, applicant2.Person });

			updater.Run();

			AssertEquals(0, applicant1.Certificates.Count);

			applicant2.Certificates.RefreshFromDb();
			AssertEquals(0, applicant2.Certificates.Count);
		}

		public void TestRun_Person_TargetAccreditation()
		{
			var accred1 = Factory.NewWithValidTestData<GlbAccreditation>();
			accred1.HAC_MustCompleteInDays = 10;
			Factory.Save();

			var accred2 = PrepareData(out var applicant);

			var updater = GetUpdater() as AccreditationUpdaterForPerson;
			updater.SetPersons(new IGlbPerson[] { applicant.Person });
			updater.ParentAccreditation = accred1;
			updater.Run();

			AssertEquals(0, applicant.Person.AccreditationAttemptCollection.Count);

			updater = GetUpdater() as AccreditationUpdaterForPerson;
			updater.SetPersons(new IGlbPerson[] { applicant.Person });
			updater.ParentAccreditation = accred2;
			updater.Run();

			AssertEquals(0, applicant.Person.AccreditationAttemptCollection.Count);

			AssertEquals(0, applicant.Certificates.Count);
		}

		public void TestRun_PersonZSaveExceptionCatching()
		{
			PrepareData(out var applicant);

			var mockUpdater = new Mock<AccreditationUpdaterForPerson>();
			mockUpdater.CallBase = true;
			mockUpdater.Setup(x => x.DeleteAttemptsAndCertificates(null)).Throws(new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), null, Db.Connection), Factory));
			mockUpdater.Object.SetPersons(new IGlbPerson[] { applicant.Person });

			AssertExceptionThrown<ZSaveConcurrencyException>(() => mockUpdater.Object.Run());
			mockUpdater.Verify(mock => mock.DeleteAttemptsAndCertificates(null), Times.Exactly(5));
		}

		[TestDate(2018, 1, 1)]
		public void TestDeleteAttemptsForPerson()
		{
			var accred = PrepareData(out var applicant);

			var updater = new AccreditationUpdaterForPerson();
			updater.SetPersons(new IGlbPerson[] { applicant.Person });
			updater.Run();

			applicant.Person.AccreditationAttemptCollection.Reload();
			AssertEquals(0, applicant.Certificates.Count);

			updater.DeleteExistingCertificates = false;
			updater.DeleteAttemptsAndCertificates();

			applicant.Person.AccreditationAttemptCollection.Reload();
			AssertEquals(0, applicant.Person.AccreditationAttemptCollection.Count);
		}

		[TestDate(2018, 1, 1)]
		public void TestDeleteAttemptsAndCertificatesForPerson()
		{
			var accred = PrepareData(out var applicant);

			var updater = new AccreditationUpdaterForPerson();
			updater.SetPersons(new IGlbPerson[] { applicant.Person });
			updater.Run();

			applicant.Person.AccreditationAttemptCollection.Reload();
			applicant.Certificates.RefreshFromDb();
			AssertEquals(0, applicant.Certificates.Count);

			updater.DeleteExistingCertificates = true;
			updater.DeleteAttemptsAndCertificates();

			applicant.Person.AccreditationAttemptCollection.Reload();
			AssertEquals(0, applicant.Person.AccreditationAttemptCollection.Count);
		}

		[TestDate(2018, 1, 1)]
		public void TestDeleteAttemptsAndCertificatesForPerson_TargetAccred()
		{
			var accred = PrepareData(out var applicant);

			var updater = new AccreditationUpdaterForPerson();
			updater.SetPersons(new IGlbPerson[] { applicant.Person });
			updater.Run();

			applicant.Person.AccreditationAttemptCollection.Reload();

			applicant.Certificates.RefreshFromDb();
			AssertEquals(0, applicant.Certificates.Count);

			var certificate2 = applicant.Certificates.AddNew();
			certificate2.XZ_RefNumber = "A0000002";
			certificate2.XZ_Comment = "bla";
			Factory.Save();

			updater.DeleteExistingCertificates = true;
			updater.DeleteAttemptsAndCertificates(accred);

			applicant.Person.AccreditationAttemptCollection.Reload();
			AssertEquals(0, applicant.Person.AccreditationAttemptCollection.Count);
			AssertEquals(false, certificate2.IsDeleted);
		}

		[TestDate(2018, 1, 1)]
		public void TestRun_PersonIsFirstExamType()
		{
			var accred = PrepareDataForDeleteTest(out var applicant);

			accred.Attempts.Reload(true);
			var attemptsPreRun = accred.Attempts;
			AssertEquals(0, attemptsPreRun.Count);

			var updater = new AccreditationUpdaterForPerson
			{
				CompletionToleranceDays = 1
			};
			updater.SetPersons(new IGlbPerson[] { applicant.Person });

			AssertEquals("Precondition", true, updater.IsFirstExamType);
			updater.Run();

			applicant = new BusinessObjectFactory().Load<HRJobApplicant>(applicant.PK);
			AssertEquals(0, applicant.Person.AccreditationAttemptCollection.Count);

			updater = new AccreditationUpdaterForPerson()
			{
				CompletionToleranceDays = 2
			};
			updater.SetPersons(new IGlbPerson[] { applicant.Person });

			AssertEquals("Precondition", true, updater.IsFirstExamType);
			updater.Run();

			applicant = new BusinessObjectFactory().Load<HRJobApplicant>(applicant.PK);
			applicant.Person.AccreditationAttemptCollection.Reload();
			AssertEquals(0, applicant.Person.AccreditationAttemptCollection.Count);

			applicant.Certificates.RefreshFromDb();
			AssertEquals(0, applicant.Certificates.Count);
		}

		[TestDate(2018, 1, 1)]
		public void TestRun_PersonIsTimePeriodType()
		{
			var accred = PrepareDataForDeleteTest(out var applicant);

			accred.Attempts.Reload(true);
			var attemptsPreRun = accred.Attempts;
			AssertEquals(0, attemptsPreRun.Count);

			var updater = new AccreditationUpdaterForPerson
			{
				IsTimePeriodType = true,
				FromDate = new ZDate(2018, 1, 1),
				ToDate = new ZDate(2018, 1, 1)
			};
			updater.SetPersons(new IGlbPerson[] { applicant.Person });

			updater.Run();

			applicant.Person.AccreditationAttemptCollection.Reload();
			AssertEquals(0, applicant.Person.AccreditationAttemptCollection.Count);

			updater.AdditionalCompletionToleranceDays = 4;
			updater.Run();

			applicant.Person.AccreditationAttemptCollection.Reload();
			AssertEquals(0, applicant.Person.AccreditationAttemptCollection.Count);

			applicant.Certificates.RefreshFromDb();
			AssertEquals(0, applicant.Certificates.Count);
		}

		[TestDate(2018, 1, 1)]
		public void TestRun_PersonShouldDeleteExistingCertificates()
		{
			var accred = PrepareDataForDeleteTest(out var applicant);

			accred.Attempts.Reload(true);
			var attemptsPreRun = accred.Attempts;
			AssertEquals(0, attemptsPreRun.Count);

			var updater = new AccreditationUpdaterForPerson
			{
				CompletionToleranceDays = 3
			};
			updater.SetPersons(new IGlbPerson[] { applicant.Person });

			AssertEquals("Precondition", true, updater.IsFirstExamType);
			AssertEquals("Precondition", true, updater.DeleteExistingCertificates);

			updater.Run();

			applicant.Person.AccreditationAttemptCollection.Reload();
			AssertEquals(0, applicant.Person.AccreditationAttemptCollection.Count);

			applicant.Certificates.RefreshFromDb();
			AssertEquals(0, applicant.Certificates.Count);

			updater.Run();

			applicant.Person.AccreditationAttemptCollection.Reload();
			AssertEquals(0, applicant.Person.AccreditationAttemptCollection.Count);

			applicant.Certificates.RefreshFromDb();
			AssertEquals(0, applicant.Certificates.Count);
		}

		[TestDate(2018, 1, 1)]
		public void TestRun_PersonShouldNotDeleteExistingCertificates()
		{
			var accred = PrepareDataForDeleteTest(out var applicant);

			accred.Attempts.Reload(true);
			var attemptsPreRun = accred.Attempts;
			AssertEquals(0, attemptsPreRun.Count);

			var updater = new AccreditationUpdaterForPerson
			{
				CompletionToleranceDays = 3,
				DeleteExistingCertificates = false
			};
			updater.SetPersons(new IGlbPerson[] { applicant.Person });

			AssertEquals("Precondition", true, updater.IsFirstExamType);
			updater.Run();

			applicant.Person.AccreditationAttemptCollection.Reload();
			AssertEquals(0, applicant.Person.AccreditationAttemptCollection.Count);

			applicant.Certificates.RefreshFromDb();
			AssertEquals(0, applicant.Certificates.Count);

			updater.Run();

			applicant.Person.AccreditationAttemptCollection.Reload();
			AssertEquals(0, applicant.Person.AccreditationAttemptCollection.Count);
		}

		[TestDate(2018, 1, 1)]
		public void TestRun_PersonDeleteExistingCertificatesShouldNotDeleteUnrelatedCertificates()
		{
			var accred = PrepareDataForDeleteTest(out var applicant);

			accred.Attempts.Reload(true);
			var attemptsPreRun = accred.Attempts;
			AssertEquals(0, attemptsPreRun.Count);

			var updater = new AccreditationUpdaterForPerson
			{
				CompletionToleranceDays = 3
			};
			updater.SetPersons(new IGlbPerson[] { applicant.Person });

			AssertEquals("Precondition", true, updater.IsFirstExamType);
			AssertEquals("Precondition", true, updater.DeleteExistingCertificates);

			var cert1 = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			cert1.XZ_RefNumber = "A0000001";
			cert1.XZ_IssueDate = new ZDate(2017, 1, 1);
			cert1.XZ_Type = cusCode;
			cert1.XZ_ParentTableCode = HRJobApplicantSchema.Constants.Prefix;
			cert1.XZ_ParentID = applicant.PK;
			applicant.Certificates.Add(cert1);

			var cert2 = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			cert2.XZ_RefNumber = "B0000002";
			cert2.XZ_IssueDate = new ZDate(2018, 1, 3);
			cert2.XZ_Type = cusCode;
			cert2.XZ_ParentTableCode = HRJobApplicantSchema.Constants.Prefix;
			cert2.XZ_ParentID = applicant.PK;
			applicant.Certificates.Add(cert2);

			var cert3 = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			cert3.XZ_RefNumber = "A0000003";
			cert3.XZ_IssueDate = new ZDate(2018, 1, 3);
			cert3.XZ_Type = "BUS";
			cert3.XZ_ParentTableCode = HRJobApplicantSchema.Constants.Prefix;
			cert3.XZ_ParentID = applicant.PK;
			applicant.Certificates.Add(cert3);

			var cert4 = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			cert4.XZ_RefNumber = "A0000004";
			cert4.XZ_IssueDate = new ZDate(2018, 1, 3);
			cert4.XZ_Type = cusCode;
			cert4.XZ_ParentTableCode = HRJobApplicantSchema.Constants.Prefix;
			cert4.XZ_ParentID = applicant.PK;
			applicant.Certificates.Add(cert4);

			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();

			var cert5 = Factory.NewWithValidTestData<GenRegCertAccredMaintList>();
			cert5.XZ_RefNumber = "A0000005";
			cert5.XZ_IssueDate = new ZDate(2018, 1, 3);
			cert5.XZ_Type = cusCode;
			cert5.XZ_ParentTableCode = HRJobApplicantSchema.Constants.Prefix;
			cert5.XZ_ParentID = applicant2.PK;
			applicant2.Certificates.Add(cert5);

			Factory.Save();

			applicant.Certificates.RefreshFromDb();
			AssertEquals(4, applicant.Certificates.Count);

			updater.Run();

			//Run again to match cert with newly created attempt.

			updater.Run();

			applicant.Certificates.RefreshFromDb();
			AssertEquals("XZ_IssueDate does not match any accreditation attempts", true, applicant.Certificates.Contains(cert1));
			AssertEquals("XZ_RefNumber begins with B, not A", true, applicant.Certificates.Contains(cert2));
			AssertEquals("XZ_Type does not match any accreditation attempts", true, applicant.Certificates.Contains(cert3));
			AssertEquals("Certificate should not be deleted for applicant2", 1, applicant2.Certificates.Count);
			AssertEquals("Certificate should not be deleted for applicant2", true, applicant2.Certificates.Contains(cert5));
		}

		protected override AccreditationUpdater GetUpdater()
		{
			return new AccreditationUpdaterForPerson();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetUpdater();
		}
	}
}
