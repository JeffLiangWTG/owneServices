using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	abstract class DocRecruiterTestCase<T> : TestCaseWithFactory
			where T : DocRecruiterBase
	{
		public void TestApplicantNameAndEmailAddress()
		{
			T wrapper = GetDocumentWrapper();
			Applicant.HA_FullName = "MEH MEH";
			Applicant.HA_EmailAddress = "test@cargowise.com";
			AssertEquals("MEH MEH", wrapper.ApplicantName);
			AssertEquals("test@cargowise.com", wrapper.ApplicantEmailAddress);
		}

		[TestDate(2008, 1, 1)]
		public void TestJobApplications()
		{
			const string expected1 =
@"<table>
	<thead>
		<td style='padding-right:20px'><b>Ad Title</b></td>
		<td><b>Applied</b></td>
	</thead>
	<tr><td style='padding-right:20px'>Rabbit Breeder</td><td>01-Jan-08</td></tr>
</table>";

			const string expected2 =
@"<table>
	<thead>
		<td style='padding-right:20px'><b>Ad Title</b></td>
		<td><b>Applied</b></td>
	</thead>
	<tr><td style='padding-right:20px'>Bear Trainer</td><td>01-Jan-08</td></tr><tr><td style='padding-right:20px'>Snake Catcher</td><td>01-Jan-08</td></tr>
</table>";

			campaign.HV_AdTitle = "Rabbit Breeder";
			HRRecruitmentJobCampaign campaign2 = Factory.New<HRRecruitmentJobCampaign>();
			campaign2.HV_AdTitle = "Bear Trainer";
			HRRecruitmentJobCampaign campaign3 = Factory.New<HRRecruitmentJobCampaign>();
			campaign3.HV_AdTitle = "Snake Catcher";

			application.HP_HV = campaign.PK;
			application.HP_CurrentStatus = "MEH";
			HRJobApplication application2 = Applicant.Applications.AddNew();
			application2.HP_HV = campaign2.PK;
			application2.HP_CurrentStatus = "ACC";
			HRJobApplication application3 = Applicant.Applications.AddNew();
			application3.HP_HV = campaign3.PK;
			application3.HP_CurrentStatus = "REJ";

			T wrapper = GetDocumentWrapper();
			AssertEquals(expected1, wrapper.InProgressJobApplications);
			AssertEquals(expected2, wrapper.CompletedJobApplications);
		}

		[TestDate(2008, 1, 1)]
		public void TestJobApplicationsWithNoAddTitle()
		{
			const string expected1 =
				@"<table>
	<thead>
		<td style='padding-right:20px'><b>Ad Title</b></td>
		<td><b>Applied</b></td>
	</thead>
	<tr><td style='padding-right:20px'></td><td>01-Jan-08</td></tr>
</table>";

			application.HP_HV = ZGuid.Empty;
			application.HP_CurrentStatus = "MEH";

			var wrapper = GetDocumentWrapper();
			AssertEquals(expected1, wrapper.InProgressJobApplications);
		}

		public void TestCompanyName()
		{
			T wrapper = GetDocumentWrapper();
			AssertEquals(GlbCompany.CurrentCompany.GC_Name, wrapper.CompanyName);
		}

		protected override void SetUp()
		{
			base.SetUp();

			campaign = Factory.New<HRRecruitmentJobCampaign>();
			role = Factory.New<HRJobRole>();
			campaign.HV_HJ_JobRole = role.PK;
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://mehmehserver/");

			application = Applicant.Applications.AddNew();
			application.HP_HV = campaign.PK;

			Factory.Save();
		}

		protected abstract T GetDocumentWrapper();

		protected HRJobApplicant Applicant
		{
			get
			{
				if (applicant == null)
				{
					applicant = Factory.New<HRJobApplicant>();
					applicant.HA_FullName = "name";
				}

				return applicant;
			}
		}

		HRJobApplicant applicant;
		protected HRRecruitmentJobCampaign campaign;
		protected HRJobApplication application;
		protected HRJobRole role;
	}
}
