using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business.Applicant;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRJobApplicantContactPhoneDialBuilderTest : TestCaseWithFactory
	{
		const string mobilePhone = "04 1111 9999";
		const string homePhone = "02 1111 9999";
		const string workPhone = "02 1111 8888";
		public void TestDefaultPhoneNumbers()
		{
			HRJobApplicant applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			var result = HRJobApplicantContactPhoneDialBuilder.GetDefaultDialInfo(applicant);
			AssertEquals(null, result);

			applicant.HA_MobilePhone = mobilePhone;
			applicant.HA_HomePhone = homePhone;
			applicant.HA_WorkPhone = workPhone;
			VerifyExpectedDefaultResult(applicant, "Mobile", "04 1111 9999");

			applicant.HA_MobilePhone = mobilePhone;
			applicant.HA_WorkPhone = workPhone;
			VerifyExpectedDefaultResult(applicant, "Mobile", "04 1111 9999");

			applicant.HA_HomePhone = homePhone;
			applicant.HA_WorkPhone = workPhone;
			VerifyExpectedDefaultResult(applicant, "Home", "02 1111 9999");

			applicant.HA_WorkPhone = workPhone;
			VerifyExpectedDefaultResult(applicant, "Work", "02 1111 8888");
		}

		public void TestAlternativePhoneNumbers()
		{
			HRJobApplicant applicant = Factory.NewWithValidTestData<HRJobApplicant>();

			VerifyExpectedAlternativeResult(applicant, System.Array.Empty<string>());

			applicant.HA_MobilePhone = mobilePhone;
			applicant.HA_HomePhone = homePhone;
			applicant.HA_WorkPhone = workPhone;
			VerifyExpectedAlternativeResult(applicant, new[] { "Mobile (04 1111 9999)", "Home (02 1111 9999)", "Work (02 1111 8888)" });

			applicant.HA_WorkPhone = workPhone;
			applicant.HA_MobilePhone = mobilePhone;
			VerifyExpectedAlternativeResult(applicant, new[] { "Mobile (04 1111 9999)", "Work (02 1111 8888)" });

			applicant.HA_HomePhone = homePhone;
			applicant.HA_WorkPhone = workPhone;
			VerifyExpectedAlternativeResult(applicant, new[] { "Home (02 1111 9999)", "Work (02 1111 8888)" });

			applicant.HA_WorkPhone = workPhone;
			VerifyExpectedAlternativeResult(applicant, new[] { "Work (02 1111 8888)" });
		}

		#region Implementation
		void VerifyExpectedDefaultResult(HRJobApplicant applicant, string description, string callNumber)
		{
			var defaultNumber = HRJobApplicantContactPhoneDialBuilder.GetDefaultDialInfo(applicant);
			AssertEquals(callNumber, defaultNumber.Number);
			AssertEquals(description, defaultNumber.Description);
			CleanApplicantPhoneNumber(applicant);
		}

		void VerifyExpectedAlternativeResult(HRJobApplicant applicant, string[] result)
		{
			var resultWorkFrist = HRJobApplicantContactPhoneDialBuilder.GetAlternativePhoneDialInfos(applicant);
			AssertArrayEqualsByElements(result,
				PhoneNumberToArray(resultWorkFrist));
			CleanApplicantPhoneNumber(applicant);
		}

		static string[] PhoneNumberToArray(IEnumerable<PhoneDialInfo> resultWorkFrist)
		{
			return resultWorkFrist.Select(info => string.Format("{0} ({1})", info.Description, info.Number)).ToArray();
		}

		void CleanApplicantPhoneNumber(HRJobApplicant applicant)
		{
			applicant.HA_MobilePhone = string.Empty;
			applicant.HA_HomePhone = string.Empty;
			applicant.HA_WorkPhone = string.Empty;
		}

		#endregion
	}
}
