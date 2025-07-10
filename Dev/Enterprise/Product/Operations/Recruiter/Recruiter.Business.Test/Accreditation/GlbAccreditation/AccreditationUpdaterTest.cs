using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Recruiter.Business.Testing
{
	abstract class AccreditationUpdaterTest : NonPersistentBusinessObjectTestCase
	{
		protected abstract AccreditationUpdater GetUpdater();

		#region Validation

		public void TestValidateCompletionTolerance()
		{
			var updater = GetUpdater();
			updater.ValidateCompletionToleranceDays();
			AssertHasError(updater.CompletionToleranceDaysInfo, "value cannot be zero.");

			updater.IsTimePeriodType = true;
			updater.ValidateCompletionToleranceDays();
			AssertNoErrors(updater.CompletionToleranceDaysInfo);

			updater.IsFirstExamType = true;
			AssertEquals(true, updater.IsFirstExamType);
			updater.ValidateCompletionToleranceDays();
			AssertHasError(updater.CompletionToleranceDaysInfo, "value cannot be zero.");

			updater.CompletionToleranceDays = 24;
			AssertNoErrors(updater.CompletionToleranceDaysInfo);

			updater.CompletionToleranceDays = -1;
			AssertHasError(updater.CompletionToleranceDaysInfo, "value cannot be negative.");
		}

		public void TestValidateFromDate()
		{
			var updater = GetUpdater();
			updater.ValidateFromDate();
			AssertNoErrors(updater.FromDateInfo);

			updater.IsTimePeriodType = true;
			updater.ValidateFromDate();
			AssertHasError(updater.FromDateInfo, "Please enter a value.");

			updater.FromDate = ZDate.Invalid;
			AssertHasError(updater.FromDateInfo, "Enter a valid selection.");

			updater.FromDate = ZDate.Today;
			AssertNoErrors(updater.FromDateInfo);

			updater.FromDate = ZDate.Today.AddDays(1);
			AssertHasError(updater.FromDateInfo, "Date cannot be in the future.");

			updater.ToDate = ZDate.Today.AddDays(-1);
			updater.FromDate = ZDate.Today;
			AssertHasError(updater.FromDateInfo, "To Date cannot be earlier than From Date.");
		}

		public void TestValidateToDate()
		{
			var updater = GetUpdater();
			updater.ValidateToDate();
			AssertNoErrors(updater.ToDateInfo);

			updater.IsTimePeriodType = true;
			updater.ValidateToDate();
			AssertHasError(updater.ToDateInfo, "Please enter a value.");

			updater.ToDate = ZDate.Invalid;
			AssertHasError(updater.ToDateInfo, "Enter a valid selection.");

			updater.ToDate = ZDate.Today;
			AssertNoErrors(updater.ToDateInfo);

			updater.ToDate = ZDate.Today.AddDays(1);
			AssertHasError(updater.ToDateInfo, "Date cannot be in the future.");

			updater.FromDate = ZDate.Today;
			updater.ToDate = ZDate.Today.AddDays(-1);
			AssertHasError(updater.ToDateInfo, "To Date cannot be earlier than From Date.");
		}

		public void TestValidateAdditionalCompletionTolerance()
		{
			var updater = GetUpdater();
			updater.ValidateAdditionalCompletionToleranceDays();
			AssertNoErrors(updater.AdditionalCompletionToleranceDaysInfo);

			updater.IsTimePeriodType = true;
			updater.ValidateAdditionalCompletionToleranceDays();
			AssertNoErrors(updater.AdditionalCompletionToleranceDaysInfo);
			AssertEquals(updater.AdditionalCompletionToleranceDays, 0);

			updater.IsFirstExamType = true;
			updater.ValidateAdditionalCompletionToleranceDays();
			AssertNoErrors(updater.AdditionalCompletionToleranceDaysInfo);

			updater.IsTimePeriodType = true;
			updater.AdditionalCompletionToleranceDays = 24;
			AssertNoErrors(updater.AdditionalCompletionToleranceDaysInfo);

			updater.AdditionalCompletionToleranceDays = -1;
			AssertHasError(updater.AdditionalCompletionToleranceDaysInfo, "value cannot be negative.");
		}

		#endregion

		public void TestDefaultValues()
		{
			var updater = GetUpdater();

			AssertEquals(true, updater.IsFirstExamType);
			AssertEquals(false, updater.IsTimePeriodType);
			AssertEquals(0, updater.AdditionalCompletionToleranceDays);
			AssertEquals(true, updater.DeleteExistingCertificates);
		}

		protected GlbAccreditation PrepareDataForDeleteTest(out HRJobApplicant applicant)
		{
			cusCode = "CUS";

			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			accred.HAC_CertificateCode = cusCode;
			accred.HAC_MustCompleteInDays = 1;

			var group1 = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
			group1.HJG_ParentID = accred.PK;
			group1.HJG_ParentTableCode = accred.TablePrefix;
			group1.HJG_Threshold = 1;
			var group2 = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
			group2.HJG_ParentID = accred.PK;
			group2.HJG_ParentTableCode = accred.TablePrefix;
			group2.HJG_Threshold = 1;

			applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			Factory.Save();

			accred.Attempts.Reload(true);
			AssertEquals(0, accred.Attempts.Count);
			AssertNull(accred.GetCurrentAttemptForPerson(applicant.Person, ZDateTime.UtcToday.Date));

			Factory.Save();
			return accred;
		}

		protected ZString cusCode;

		protected GlbAccreditation PrepareData(out HRJobApplicant applicant1)
		{
			HRJobApplicant applicant2 = null;
			return PrepareData(out applicant1, out applicant2);
		}

		protected GlbAccreditation PrepareData(out HRJobApplicant applicant1, out HRJobApplicant applicant2)
		{
			var accred = Factory.NewWithValidTestData<GlbAccreditation>();
			accred.HAC_CertificateCode = "CUS";
			accred.HAC_MustCompleteInDays = 10;

			var group1 = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
			group1.HJG_ParentID = accred.PK;
			group1.HJG_ParentTableCode = accred.TablePrefix;
			group1.HJG_Threshold = 2;
			var group2 = Factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
			group2.HJG_ParentID = accred.PK;
			group2.HJG_ParentTableCode = accred.TablePrefix;
			group2.HJG_Threshold = 1;

			AddApplicant(out applicant1, accred);
			AddApplicant(out applicant2, accred);

			return accred;
		}

		void AddApplicant(out HRJobApplicant applicant, GlbAccreditation accred)
		{
			applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			Factory.Save();

			accred.Attempts.Reload(true);
			AssertEquals(0, accred.Attempts.Count);

			applicant.Certificates.RefreshFromDb();
			accred.Attempts.RemoveAndDeleteAll();
			applicant.Certificates.DeleteAll();
			Factory.Save();

			applicant.Certificates.RefreshFromDb();
			AssertNull(accred.GetCurrentAttemptForPerson(applicant.Person, ZDateTime.UtcNow.Date));
			AssertEquals(0, applicant.Certificates.Count);
		}
	}
}
