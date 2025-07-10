using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(CandidateStmNote))]
	class CandidateStmNoteTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNoteSource_ApplicationNoRelatedParent()
		{
			var note = Factory.New<CandidateStmNote>();
			note.Master = Factory.New<HRJobApplication>();
			AssertEquals("This Job Application", note.ST_NoteSource.ToString());
		}

		public void TestNoteSource_ApplicantNoRelatedParent()
		{
			var note = Factory.New<CandidateStmNote>();
			note.Master = Factory.New<HRJobApplicant>();
			AssertEquals("This Job Applicant", note.ST_NoteSource.ToString());
		}

		public void TestNoteSource_Application_NoJobOpening()
		{
			var note = Factory.New<TestCandidateStmNote>();
			var application = Factory.New<HRJobApplication>();
			application.HP_ApplicationNumber = "JA0000001";

			note.Master = application;
			AssertEquals("No Valid Campaign JA0000001", note.ST_NoteSource.ToString());
		}

		public void TestNoteSource_Application_WithJobOpening()
		{
			var jobOpening = Factory.New<HRRecruitmentJobCampaign>();
			jobOpening.HV_AdTitle = "Software Engineer";

			var applicant = Factory.New<HRJobApplicant>();
			var application = Factory.New<HRJobApplication>();

			application.HP_HV = jobOpening.PK;
			application.HP_HA = applicant.PK;
			application.HP_ApplicationNumber = "JA0000001";

			var note = Factory.New<TestCandidateStmNote>();
			note.Master = application;
			AssertEquals("Software Engineer JA0000001", note.ST_NoteSource.ToString());
		}

		public void TestNoteSource_Applicant()
		{
			var applicant = Factory.New<HRJobApplicant>();
			applicant.HA_FullName = "Homer Simpson";

			var note = Factory.New<TestCandidateStmNote>();
			note.Master = applicant;
			AssertEquals("Job Applicant Homer Simpson", note.ST_NoteSource.ToString());
		}

		public void TestNoteSource_Application_Multi()
		{
			var jobOpening1 = Factory.New<HRRecruitmentJobCampaign>();
			jobOpening1.HV_AdTitle = "Software Engineer";

			var jobOpening2 = Factory.New<HRRecruitmentJobCampaign>();
			jobOpening2.HV_AdTitle = "Product Specialist";

			var applicant1 = Factory.NewWithValidTestData<HRJobApplicant>();
			var applicant2 = Factory.NewWithValidTestData<HRJobApplicant>();

			var application1 = Factory.NewWithValidTestData<HRJobApplication>();
			application1.HP_HV = jobOpening1.PK;
			application1.HP_HA = applicant1.PK;
			application1.HP_ApplicationNumber = "JA0000001";

			var application2 = Factory.NewWithValidTestData<HRJobApplication>();
			application2.HP_HV = jobOpening1.PK;
			application2.HP_HA = applicant2.PK;
			application2.HP_ApplicationNumber = "JA0000002";

			var application3 = Factory.NewWithValidTestData<HRJobApplication>();
			application3.HP_HV = jobOpening2.PK;
			application3.HP_HA = applicant1.PK;
			application3.HP_ApplicationNumber = "JA0000001";

			var note1 = Factory.New<TestCandidateStmNote>();
			note1.Master = application1;

			var note2 = Factory.New<TestCandidateStmNote>();
			note2.Master = application2;

			var note3 = Factory.New<TestCandidateStmNote>();
			note3.Master = application3;

			AssertEquals("Software Engineer JA0000001", note1.ST_NoteSource.ToString());
			AssertEquals("Software Engineer JA0000002", note2.ST_NoteSource.ToString());
			AssertEquals("Product Specialist JA0000001", note3.ST_NoteSource.ToString());
		}

		public class TestCandidateStmNote : CandidateStmNote
		{
			public TestCandidateStmNote(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override bool IsRelatedInParentView => true;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var note = Factory.NewWithValidTestData<CandidateStmNote>();
			note.Master = Factory.NewWithValidTestData<HRJobApplication>();
			return note;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<CandidateStmNote>();
			result.Master = factory.NewWithValidTestData<HRJobApplication>();
			return result;
		}
	}
}
