using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class HRJobApplicationInterviewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckHI_InterviewStatus()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("***", "Interview Status");
			RecruiterDataRegistry.Instance.InterviewStatuses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			HRJobApplicationInterview interview = Factory.New<HRJobApplicationInterview>();
			interview.HI_InterviewStatus = "***";
			AssertNoErrors("Interview Status is valid, should not have errors", interview.HI_InterviewStatusInfo);
			interview.HI_InterviewStatus = ";;;";
			AssertHasErrors("Interview Status is NOT valid, should have errors", interview.HI_InterviewStatusInfo);
		}

		public void TestHI_GS_NKInterviewer()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			HRJobApplicationInterview interview = Factory.New<HRJobApplicationInterview>();

			interview.HI_GS_NKInterviewer = "//";
			AssertHasErrors("Invalid code entered, Interviewer should have errors", interview.HI_GS_NKInterviewerInfo);

			interview.HI_GS_NKInterviewer = staff.GS_Code;
			AssertNoErrors("Valid code entered, Interviewer should NOT have errors", interview.HI_GS_NKInterviewerInfo);

			interview.HI_GS_NKInterviewer = ZString.Empty;
			AssertHasErrors("Interviewer is mandatory field, should have errors", interview.HI_GS_NKInterviewerInfo);
		}
	}
}
