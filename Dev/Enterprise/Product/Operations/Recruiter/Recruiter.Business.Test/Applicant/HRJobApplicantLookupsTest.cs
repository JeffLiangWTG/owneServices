using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRJobApplicantLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCampaigns()
		{
			AssertNotNull(Applicant.Lookups.Campaigns);
		}

		public void TestJobRoles()
		{
			AssertNotNull(Applicant.Lookups.JobRoles);
		}

		public void TestAvailabilities()
		{
			AssertNotNull(Applicant.Lookups.Availabilities);
		}

		public void TestGenders()
		{
			var lookups = Factory.New<HRJobApplicant>().Lookups;
			Assert("Male", lookups.Genders.ContainsCode(Core.Constants.Genders.Man));
			Assert("Female", lookups.Genders.ContainsCode(Core.Constants.Genders.Woman));
			Assert("Non-Binary", lookups.Genders.ContainsCode(Core.Constants.Genders.NonBinary));
			Assert("NotSpecified", lookups.Genders.ContainsCode(Core.Constants.Genders.NotSpecified));
			Assert("Custom", lookups.Genders.ContainsCode(Core.Constants.Genders.Custom));
			Assert("Agender", lookups.Genders.ContainsCode(Core.Constants.Genders.Agender));
		}

		public void TestCountries()
		{
			AssertNotNull(Applicant.Person.Lookups.Countries);
		}

		public void TestWorkPerimtStatuses()
		{
			AssertNotNull(Applicant.Lookups.WorkPermitStatuses);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Applicant = Factory.New<HRJobApplicant>();
		}

		HRJobApplicant Applicant;
	}
}
