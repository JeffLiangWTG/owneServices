using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Recruiter.ServiceTasks.Testing
{
	[TestedType(typeof(GlbAccreditationCompletionServiceTask))]
	class GlbAccreditationCompletionServiceTaskTest : ServiceTaskTestCase<GlbAccreditationCompletionServiceTask>
	{
		public void TestRunTask()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "staff@cw1.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff);
			Factory.Save();

			var accreditation = PrepareData(out var jobApplicant);
			accreditation.HAC_Code = "CCO";
			jobApplicant.Person.PER_FullName = "Jim Green";
			Factory.Save();

			var updater = new AccreditationUpdaterForPerson();
			updater.SetPersons(new IGlbPerson[] { jobApplicant.Person });
			updater.Run();
			Factory.Save();

			var person = new BusinessObjectFactory().Load<GlbPerson>(jobApplicant.Person.PK);
			person.Factory.Save();

			var process = new GlbAccreditationCompletionServiceTask();
			var logger = new TestServiceLogger();
			process.ServiceLogger = logger;
			process.RunTask();

			person = new BusinessObjectFactory().Load<GlbPerson>(jobApplicant.Person.PK);

			AssertEquals(@"Information|Begin processing
Information|0 Exam Attempt(s) processed
Information|End processing
", logger.ToString());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						ExamAttemptSchema.Constants.TableName,
						null,
						ExamAttemptSchema.Constants.EXA_Status + "=" + ExamAttempt.StatusCodes.Queued),
				};
			}
		}

		#region PrepareData

		GlbAccreditation PrepareData(out HRJobApplicant applicant1)
		{
			HRJobApplicant applicant2 = null;
			return PrepareData(out applicant1, out applicant2);
		}

		GlbAccreditation PrepareData(out HRJobApplicant applicant1, out HRJobApplicant applicant2)
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
			AssertNull(GetCurrentAttemptForPerson(accred, applicant.Person, ZDateTime.UtcNow.Date));

			applicant.Certificates.RefreshFromDb();
			accred.Attempts.RemoveAndDeleteAll();
			applicant.Certificates.DeleteAll();
			Factory.Save();

			applicant.Certificates.RefreshFromDb();
			AssertNull(GetCurrentAttemptForPerson(accred, applicant.Person, ZDateTime.UtcNow.Date));
			AssertEquals(0, applicant.Certificates.Count);
		}

		static GlbAccreditationAttempt GetCurrentAttemptForPerson(GlbAccreditation accreditation, GlbPerson person, ZDate currentDate)
		{
			return person.GetCurrentAttempt(accreditation.PK, currentDate) as GlbAccreditationAttempt;
		}

		#endregion PrepareData
	}
}
