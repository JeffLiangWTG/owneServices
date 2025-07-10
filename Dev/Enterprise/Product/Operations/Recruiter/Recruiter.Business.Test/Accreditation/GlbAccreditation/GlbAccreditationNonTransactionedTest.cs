using System.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[UseSnapshotProtection(true)]
	sealed class GlbAccreditationNonTransactionedTest : TestCase
	{
		class GlbPersonForTest : GlbPerson
		{
			public GlbPersonForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}

		public void TestCompleteDateIsSet()
		{
			GlbAccreditation accred = null;
			GlbAccreditationJobSkillGroup group = null;
			LearningCentreCampaign exam = null;
			GlbAccreditationJobSkillPivot jobSkillPivot = null;
			IGlbAccreditationAttempt attempt = null;
			GlbPersonForTest person = null;
			HRJobApplicant applicant = null;
			GenRegCertAccredMaintList cert = null;

			try
			{
				var factory = new BusinessObjectFactory();
				accred = factory.New<GlbAccreditation>();
				accred.HAC_Code = ZGuid.NewZGuid().ToString().Substring(0, 3);
				accred.HAC_CertificateCode = "CUS";
				accred.HAC_MustCompleteInDays = 10;

				group = factory.NewWithValidTestData<GlbAccreditationJobSkillGroup>();
				group.HJG_ParentID = accred.PK;
				group.HJG_ParentTableCode = accred.TablePrefix;
				group.HJG_Threshold = 1;

				exam = factory.NewWithValidTestData<LearningCentreCampaign>();

				jobSkillPivot = factory.New<GlbAccreditationJobSkillPivot>();
				jobSkillPivot.HAJ_HJG = group.PK;
				group.SkillPivots.Add(jobSkillPivot);

				person = factory.New<GlbPersonForTest>();
				applicant = factory.New<HRJobApplicant>();
				applicant.HA_EmailAddress = "addr@email.com";
				applicant.HA_PER = person.PK;
				applicant.HA_FullName = "name";

				accred.StartAttempt(person);
				attempt = person.GetLastAttempt(accred.PK);
				AssertNotNull(attempt);

				accred.CompleteAttemptIfRequired(person);
				AssertEquals(0, applicant.Certificates.Count);
				AssertEquals(false, attempt.IsCompleted);
			}
			finally
			{
				if (cert != null)
				{
					cert.Delete();
				}

				if (applicant != null)
				{
					applicant.Delete();
				}

				if (jobSkillPivot != null)
				{
					jobSkillPivot.Delete();
				}

				if (exam != null)
				{
					exam.Delete();
				}

				if (accred != null)
				{
					accred.Delete();
				}
			}
		}
	}
}
